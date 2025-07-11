using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Query;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Plugin_Installment_Delete
{
    public class Plugin_Installment_Delete : IPlugin
    {
        private IPluginExecutionContext context = null;
        private IOrganizationServiceFactory factory = null;
        private IOrganizationService service = null;
        private ITracingService tracingService = null;
        public void Execute(IServiceProvider serviceProvider)
        {
            this.context = (IPluginExecutionContext)serviceProvider.GetService(typeof(IPluginExecutionContext));
            this.factory = (IOrganizationServiceFactory)serviceProvider.GetService(typeof(IOrganizationServiceFactory));
            this.service = factory.CreateOrganizationService(this.context.UserId);
            this.tracingService = (ITracingService)serviceProvider.GetService(typeof(ITracingService));

            Init();
        }
        private void Init()
        {
            try
            {
                if (this.context.Depth > 3) return;
                if (this.context.MessageName != "Delete") return;

                Entity enInstallment = (Entity)this.context.PreEntityImages["preImage"];
                if (!enInstallment.Contains("bsd_paymentscheme") || !enInstallment.Contains("bsd_ordernumber")) return;

                int currentOrderNumber = (int)enInstallment["bsd_ordernumber"];
                tracingService.Trace("Current ordernum: " + currentOrderNumber);
                EntityCollection listPMDs = getPaymentSchemeDetails(((EntityReference)enInstallment["bsd_paymentscheme"]).Id);
                int lastOrderNumber = (int)listPMDs.Entities.LastOrDefault()["bsd_ordernumber"];
                tracingService.Trace("Last ordernum: " + lastOrderNumber);
                if (currentOrderNumber < lastOrderNumber)
                {
                    tracingService.Trace("Vao update order number");
                    var listPMD_Update = listPMDs.Entities.Where(x => (int)x["bsd_ordernumber"] > currentOrderNumber);
                    foreach (var item in listPMD_Update)
                    {
                        int newOrderNumber = ((int)item["bsd_ordernumber"]) - 1;
                        var newName = item["bsd_name"].ToString().Replace(item["bsd_ordernumber"].ToString(), newOrderNumber.ToString());
                        tracingService.Trace("orderNumber: " + (int)item["bsd_ordernumber"]);
                        tracingService.Trace("newOrderNumber: " + newOrderNumber);
                        tracingService.Trace("newName: " + newName);

                        Entity enPMD = new Entity(item.LogicalName,item.Id);
                        enPMD["bsd_ordernumber"] = newOrderNumber;
                        enPMD["bsd_name"] = newName;
                        this.service.Update(enPMD);
                        tracingService.Trace("update thanh cong");
                    }
                }
            }
            catch (InvalidPluginExecutionException ex)
            {
                throw ex;
            }
        }
        private EntityCollection getPaymentSchemeDetails(Guid paymentSchemeId)
        {
            try
            {
                var fetchXml = $@"<?xml version=""1.0"" encoding=""utf-16""?>
                <fetch>
                  <entity name=""bsd_paymentschemedetail"">
                    <attribute name=""bsd_ordernumber"" />
                    <attribute name=""bsd_name"" />
                    <filter>
                      <condition attribute=""statecode"" operator=""eq"" value=""0"" />
                      <condition attribute=""bsd_paymentscheme"" operator=""eq"" value=""{paymentSchemeId}"" />
                    </filter>
                    <order attribute=""bsd_ordernumber"" />
                  </entity>
                </fetch>";
                var result = this.service.RetrieveMultiple(new FetchExpression(fetchXml));
                return result;
            }
            catch (InvalidPluginExecutionException ex)
            {
                throw ex;
            }
        }
    }
}
