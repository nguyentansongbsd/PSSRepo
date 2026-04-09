using Microsoft.Xrm.Sdk;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace Plugin_UtopSyncInstallment
{
    public class Plugin_UtopSyncInstallment : IPlugin
    {
        IOrganizationService service = null;
        IOrganizationServiceFactory serviceFactory = null;
        IPluginExecutionContext context = null;
        ITracingService tracingService = null;

        Entity target = null;
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
            if (this.context.Depth > 3)
                return;
            this.target = this.context.InputParameters["Target"] as Entity;
            Entity enInstallment = service.Retrieve(this.target.LogicalName, this.target.Id, new Microsoft.Xrm.Sdk.Query.ColumnSet("bsd_optionentry"));
            Entity enOE = service.Retrieve(((EntityReference)enInstallment["bsd_optionentry"]).LogicalName, ((EntityReference)enInstallment["bsd_optionentry"]).Id, new Microsoft.Xrm.Sdk.Query.ColumnSet("customerid"));
            Entity enContact = service.Retrieve(((EntityReference)enOE["customerid"]).LogicalName, ((EntityReference)enOE["customerid"]).Id, new Microsoft.Xrm.Sdk.Query.ColumnSet("bsd_isconsent"));
            if (!enContact.Contains("bsd_isconsent") || (enContact.Contains("bsd_isconsent") && (bool)enContact["bsd_isconsent"] == false))
                return;

            // call api azure function to sync project data to utop system
            string url = $@"https://functionapp-cldvncapitaone-prod-fdezg4fwgphzcuef.southeastasia-01.azurewebsites.net/api/upsertcontract?id={enContact.Id}&entity={enContact.LogicalName}";
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

    }
}
