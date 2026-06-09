using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Query;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Runtime.CompilerServices;
using System.Runtime.ConstrainedExecution;
using System.Text;
using System.Threading.Tasks;

namespace Plugin_UtopSyncContract
{
    public class Plugin_UtopSyncContract : IPlugin
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
            if (this.context.Depth > 2)
                return;
            this.target = this.context.InputParameters["Target"] as Entity;
            Entity enContract = service.Retrieve(this.target.LogicalName, this.target.Id, new Microsoft.Xrm.Sdk.Query.ColumnSet("customerid"));
            Entity enCustomer = service.Retrieve(((EntityReference)enContract["customerid"]).LogicalName, ((EntityReference)enContract["customerid"]).Id, new Microsoft.Xrm.Sdk.Query.ColumnSet("bsd_isconsent"));
            if (!enCustomer.Contains("bsd_isconsent") || (enCustomer.Contains("bsd_isconsent") && (bool)enCustomer["bsd_isconsent"] == false))
                return;
            GetEnvironment();
            Guid optionEntryId = enContract.Id;
            tracingService.Trace("idoe" + optionEntryId);
            // call api azure function to sync project data to utop system
            string url = $@"https://functionapp-cldvncapitaone-prod-fdezg4fwgphzcuef.southeastasia-01.azurewebsites.net/api/{environment}/upsertcontract?id={enCustomer.Id}&entity={enCustomer.LogicalName}&oeid={optionEntryId}";
            tracingService.Trace("Call api"+ url);
            HttpClient httpClient = new HttpClient();

            var respose = await httpClient.GetAsync(url);
            if (respose.IsSuccessStatusCode)
            {
                tracingService.Trace("Sync data to utop system successfully.");
            }
            else
            {
                tracingService.Trace("Sync data to utop system failed.");
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
            }
            else
                tracingService.Trace("EnvironmentIntergrationUtop config is not found, please check.");
        }

    }
}
