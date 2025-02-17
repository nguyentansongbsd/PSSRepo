using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Query;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Plugin_PaymentScheme_CheckInstallment
{
    public class Plugin_PaymentScheme_CheckInstallment : IPlugin
    {
        IPluginExecutionContext context = null;
        IOrganizationServiceFactory factory = null;
        IOrganizationService service = null;
        ITracingService tracingService = null;

        Entity target = null;
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
                this.target = (Entity)this.context.InputParameters["Target"];
                if (this.context.MessageName != "Update") return;
                if (((OptionSetValue)this.target["statuscode"]).Value != 100000000) return; // 100000000 = Confirm

                EntityCollection installments = getInstallments();
                if (installments == null) throw new InvalidPluginExecutionException("The payment scheme does not yet include a installment. Please check information again.");
                if (installments.Entities.Any(x => x.Contains("bsd_duedatecalculatingmethod") && ((OptionSetValue)x["bsd_duedatecalculatingmethod"]).Value == 100000002) == false) throw new InvalidPluginExecutionException("The payment scheme does not yet inclede a handover installment. Please check information again.");
                if (installments.Entities.Any(x => x.Contains("bsd_lastinstallment") && (bool)x["bsd_lastinstallment"] == true) == false) throw new InvalidPluginExecutionException("The payment scheme does not yet inclede a last installment. Please check information again.");
                if (installments.Entities.Sum(x => (decimal)x["bsd_amountpercent"]) < 100) throw new InvalidPluginExecutionException("The payment schedule has not yet reached 100%.");
            }
            catch (InvalidPluginExecutionException ex)
            {
                throw ex;
            }
        }
        private EntityCollection getInstallments()
        {
            try
            {
                var fetchXml = $@"<?xml version=""1.0"" encoding=""utf-16""?>
                <fetch version=""1.0"" output-format=""xml-platform"" mapping=""logical"" distinct=""false"">
                  <entity name=""bsd_paymentschemedetail"">
                    <attribute name=""bsd_name"" />
                    <attribute name=""bsd_ordernumber"" />
                    <attribute name=""bsd_lastinstallment"" />
                    <attribute name=""bsd_paymentschemedetailid"" />
                    <attribute name=""bsd_amountpercent"" />
                    <attribute name=""bsd_duedatecalculatingmethod"" />
                    <order attribute=""bsd_ordernumber"" descending=""false"" />
                    <filter type=""and"">
                      <condition attribute=""bsd_optionentry"" operator=""null"" />
                      <condition attribute=""bsd_reservation"" operator=""null"" />
                      <condition attribute=""bsd_quotation"" operator=""null"" />
                      <condition attribute=""bsd_conversioncontractapproval"" operator=""null"" />
                      <condition attribute=""bsd_appendixcontract"" operator=""null"" />
                      <condition attribute=""bsd_paymentscheme"" operator=""eq"" value=""{this.target.Id}"" />
                    </filter>
                  </entity>
                </fetch>";
                EntityCollection result = this.service.RetrieveMultiple(new FetchExpression(fetchXml));
                if (result == null || result.Entities.Count <= 0) return null;
                return result;
            }
            catch (InvalidPluginExecutionException ex)
            {
                throw ex;
            }
        }
    }
}
