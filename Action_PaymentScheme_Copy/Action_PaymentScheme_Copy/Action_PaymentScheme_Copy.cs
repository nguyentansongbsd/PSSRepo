using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Query;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Action_PaymentScheme_Copy
{
    public class Action_PaymentScheme_Copy : IPlugin
    {
        IPluginExecutionContext context = null;
        IOrganizationServiceFactory serviceFactory = null;
        IOrganizationService service = null;
        ITracingService tracingService = null;
        EntityReference target = null;
        Entity enPaymentScheme = null;
        public void Execute(IServiceProvider serviceProvider)
        {
            context = (IPluginExecutionContext)serviceProvider.GetService(typeof(IPluginExecutionContext));
            serviceFactory = (IOrganizationServiceFactory)serviceProvider.GetService(typeof(IOrganizationServiceFactory));
            service = serviceFactory.CreateOrganizationService(context.UserId);
            tracingService = (ITracingService)serviceProvider.GetService(typeof(ITracingService));
            target = (EntityReference)context.InputParameters["Target"];

            try
            {
                Init().Wait();
            }
            catch (AggregateException ex)
            {
                var innerEx = ex.InnerExceptions.FirstOrDefault();
                if (innerEx != null)
                {
                    throw innerEx;
                }
            }
        }
        private async Task Init()
        {
            try
            {
                string projectId = (string)context.InputParameters["projectId"];
                enPaymentScheme = this.service.Retrieve(this.target.LogicalName, this.target.Id, new ColumnSet(true));
                await createPaymentSchemeCopy(enPaymentScheme, projectId);
            }
            catch (InvalidPluginExecutionException ex)
            {
                throw ex;
            }
        }
        private async Task createPaymentSchemeCopy(Entity _enPaymentScheme, string projectId)
        {
            try
            {
                _enPaymentScheme.Attributes.Remove("bsd_paymentschemeid");
                _enPaymentScheme.Attributes.Remove("ownerid");

                Guid id = Guid.NewGuid();
                _enPaymentScheme.Id = id;
                _enPaymentScheme["bsd_name"] = (string)this.enPaymentScheme["bsd_name"] + " Copy";
                _enPaymentScheme["bsd_project"] = new EntityReference("bsd_project", Guid.Parse(projectId));
                _enPaymentScheme["statuscode"] = new OptionSetValue(1);

                this.service.Create(_enPaymentScheme);

                await copyInstallments_CaoTang(id, projectId);
                this.context.OutputParameters["paymentSchemeId"] = id;
            }
            catch (InvalidPluginExecutionException ex)
            {
                throw ex;
            }
        }
        private async Task copyInstallments_CaoTang(Guid paymentSchemeIdNew, string projectId)
        {
            try
            {
                tracingService.Trace("Start copy installment");
                var fetchXml = $@"<?xml version=""1.0"" encoding=""utf-16""?>
                <fetch version=""1.0"" output-format=""xml-platform"" mapping=""logical"" distinct=""false"">
                  <entity name=""bsd_paymentschemedetail"">
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
                var result = this.service.RetrieveMultiple(new FetchExpression(fetchXml));
                if (result == null || result.Entities.Count == 0) return;
                Guid paymentSchemeDetailId = Guid.Empty;
                foreach (var item in result.Entities)
                {
                    Entity enPaymentSchemeDetailNew = item;
                    enPaymentSchemeDetailNew.Attributes.Remove("bsd_paymentschemedetailid");
                    enPaymentSchemeDetailNew.Attributes.Remove("ownerid");

                    enPaymentSchemeDetailNew.Id = Guid.NewGuid();
                    enPaymentSchemeDetailNew["bsd_paymentscheme"] = new EntityReference("bsd_paymentscheme", paymentSchemeIdNew);
                    enPaymentSchemeDetailNew["bsd_project"] = new EntityReference("bsd_project", Guid.Parse(projectId));

                    paymentSchemeDetailId = this.service.Create(enPaymentSchemeDetailNew);
                }
                tracingService.Trace("End copy installment");
            }
            catch (InvalidPluginExecutionException ex)
            {
                throw ex;
            }
        }
    }
}
