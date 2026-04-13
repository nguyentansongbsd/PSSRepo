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

namespace Plugin_UtopSyncSubSale
{
    public class Plugin_UtopSyncSubSale : IPlugin
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
            Entity subsale = service.Retrieve(this.target.LogicalName, this.target.Id, new ColumnSet("bsd_optionentry", "statuscode"));
            EntityReference oe = subsale.Contains("bsd_optionentry") ? (EntityReference)subsale["bsd_optionentry"] : null;
            Guid optionEntryId = oe.Id;
            int status = this.target.GetAttributeValue<OptionSetValue>("statuscode").Value;
            
            if (status == 100000001)
            {
               
                tracingService.Trace("VÀo Plugin_UtopSyncSubSale case sub_sale");
                Entity ensub = service.Retrieve(this.target.LogicalName, this.target.Id, new Microsoft.Xrm.Sdk.Query.ColumnSet("bsd_currentcustomer", "bsd_newcustomer"));
                Entity enCustomer_old = service.Retrieve(((EntityReference)ensub["bsd_currentcustomer"]).LogicalName, ((EntityReference)ensub["bsd_currentcustomer"]).Id, new Microsoft.Xrm.Sdk.Query.ColumnSet("bsd_isconsent"));
                Entity enCustomer_new = service.Retrieve(((EntityReference)ensub["bsd_newcustomer"]).LogicalName, ((EntityReference)ensub["bsd_newcustomer"]).Id, new Microsoft.Xrm.Sdk.Query.ColumnSet("bsd_isconsent"));
                Guid idcusnew = ((EntityReference)ensub["bsd_newcustomer"]).Id;
                if ((!enCustomer_old.Contains("bsd_isconsent") || (enCustomer_old.Contains("bsd_isconsent") && (bool)enCustomer_old["bsd_isconsent"] == false)) && (!enCustomer_new.Contains("bsd_isconsent") || (enCustomer_new.Contains("bsd_isconsent") && (bool)enCustomer_new["bsd_isconsent"] == false)))
                    return;
                bool isconsent = false;
                if (enCustomer_new.Contains("bsd_isconsent"))
                {
                    isconsent = (bool)enCustomer_new["bsd_isconsent"];
                }

                string isconsentStr = isconsent.ToString();
                tracingService.Trace("isconsent" + isconsentStr);
                tracingService.Trace("oeid" + optionEntryId);
                tracingService.Trace("customerid_" + idcusnew);
                // call api azure function to sync project data to utop system
                string url = $@"https://functionapp-cldvncapitaone-prod-fdezg4fwgphzcuef.southeastasia-01.azurewebsites.net/api/upsertcontract?id={enCustomer_new.Id}&entity={enCustomer_new.LogicalName}&oeid={optionEntryId}&isconsent={isconsentStr}";
                tracingService.Trace("api_" + url);
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
}
