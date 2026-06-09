using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Query;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace PLugin_UtopSyncProject
{
    public class PLugin_UtopSyncProject : IPlugin
    {
        IOrganizationService service = null;
        IOrganizationServiceFactory serviceFactory = null;
        IPluginExecutionContext context = null;
        ITracingService tracingService = null;

        Entity target = null;
        string environment = string.Empty;
        public void Execute(IServiceProvider serviceProvider)
        {
            this.context = (IPluginExecutionContext)serviceProvider.GetService(typeof(IPluginExecutionContext));
            this.serviceFactory = (IOrganizationServiceFactory)serviceProvider.GetService(typeof(IOrganizationServiceFactory));
            this.service = serviceFactory.CreateOrganizationService(context.UserId);
            this.tracingService = (ITracingService)serviceProvider.GetService(typeof(ITracingService));
            Init();
        }
        private async Task Init()
        {
            if (this.context.MessageName == "Delete")
                return;
            if(this.context.Depth > 3)
                return;
            this.target = this.context.InputParameters["Target"] as Entity;

            GetEnvironment();
            // call api azure function to sync project data to utop system
            string url = $@"https://functionapp-cldvncapitaone-prod-fdezg4fwgphzcuef.southeastasia-01.azurewebsites.net/api/{environment}/upsertProject/";
            HttpClient httpClient = new HttpClient();

            var respose = await httpClient.PostAsync(url + this.target.Id, null);
            if(respose.IsSuccessStatusCode)
            {
                tracingService.Trace("Sync project data to utop system successfully.");
            }
            else
            {
                tracingService.Trace("Sync project data to utop system failed.");
            }
        }
        private void GetEnvironment()
        {
            var fetchXml = $@"<?xml version=""1.0"" encoding=""utf-16""?>
            <fetch>
              <entity name=""bsd_configgolive"">
                <attribute name=""bsd_url"" />
                <filter>
                  <condition attribute=""bsd_name"" operator=""eq"" value=""EnvironmentIntergrationUtop"" />
                </filter>
              </entity>
            </fetch>";
            var result = service.RetrieveMultiple(new FetchExpression(fetchXml));
            if (result.Entities.Count > 0)
            {
                this.environment = result.Entities[0].GetAttributeValue<string>("bsd_url").Replace("https://", "");
            }else
                tracingService.Trace("EnvironmentIntergrationUtop config is not found, please check.");
        }
    }
}
