using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Query;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Action_ConfirmInvoice_Approve
{
    public class Action_ConfirmInvoice_Approve : IPlugin
    {
        IPluginExecutionContext context = null;
        IOrganizationServiceFactory serviceFactory = null;
        IOrganizationService service = null;
        ITracingService tracingService = null;

        EntityReference Target = null;
        public void Execute(IServiceProvider serviceProvider)
        {
            this.context = (IPluginExecutionContext)serviceProvider.GetService(typeof(IPluginExecutionContext));
            this.serviceFactory = (IOrganizationServiceFactory)serviceProvider.GetService(typeof(IOrganizationServiceFactory));
            this.service = serviceFactory.CreateOrganizationService(context.UserId);
            this.tracingService = (ITracingService)serviceProvider.GetService(typeof(ITracingService));
            Init();
        }
        private void Init()
        {
            try
            {
                this.Target = (EntityReference)context.InputParameters["Target"];
                Entity enConfirmInvoice = service.Retrieve(Target.LogicalName, Target.Id, new ColumnSet("bsd_invoicedate", "statuscode"));
                if (((OptionSetValue)enConfirmInvoice["statuscode"]).Value != 100000001) return;
                UpdateInvoices(enConfirmInvoice);
                UpdateConfirmInvoice(enConfirmInvoice);
            }
            catch (Exception ex)
            {
                throw new InvalidPluginExecutionException(ex.Message);
            }
        }
        private void UpdateInvoices(Entity enConfirmInvoice)
        {
            tracingService.Trace("Start Update Invoices");
            var fetchXml = $@"<?xml version=""1.0"" encoding=""utf-16""?>
            <fetch>
              <entity name=""bsd_invoice"">
                <attribute name=""bsd_name"" />
                <filter>
                  <condition attribute=""statecode"" operator=""eq"" value=""0"" />
                  <condition attribute=""statuscode"" operator=""eq"" value=""100000000"" />
                </filter>
                <link-entity name=""bsd_confirminvoice_bsd_invoice_bsd_invoice"" from=""bsd_invoiceid"" to=""bsd_invoiceid"" alias=""confirminvoice"" intersect=""true"">
                  <filter>
                    <condition attribute=""bsd_confirminvoiceid"" operator=""eq"" value=""{enConfirmInvoice.Id}"" />
                  </filter>
                </link-entity>
              </entity>
            </fetch>";
            var result = service.RetrieveMultiple(new FetchExpression(fetchXml));
            if (result.Entities.Count > 0)
            {
                foreach (var invoice in result.Entities)
                {
                    if (enConfirmInvoice.Contains("bsd_invoicedate"))
                    {
                        Entity enInvoice = new Entity(invoice.LogicalName, invoice.Id);
                        enInvoice["bsd_issueddate"] = (DateTime)enConfirmInvoice["bsd_invoicedate"];
                        service.Update(enInvoice);
                        tracingService.Trace(((DateTime)enConfirmInvoice["bsd_invoicedate"]).ToString());
                    }
                }
            }
            tracingService.Trace("End Update Invoices");
        }
        private void UpdateConfirmInvoice(Entity enConfirmInvoice)
        {
            tracingService.Trace("Start Update Confirm Invoices");
            Entity enUpdate = new Entity(enConfirmInvoice.LogicalName, enConfirmInvoice.Id);
            enUpdate["statuscode"] = new OptionSetValue(100000002);
            enUpdate["bsd_approvedby"] = new EntityReference("systemuser",this.context.UserId);
            enUpdate["bsd_approvaldate"] = DateTime.Now;
            service.Update(enUpdate);
            tracingService.Trace("End Update Confirm Invoices");
        }
    }
}
