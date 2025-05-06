using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Query;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Action_GetParamTransactionPayment
{
    public class Action_GetParamTransactionPayment : IPlugin
    {
        IPluginExecutionContext context = null;
        IOrganizationServiceFactory serviceFactory = null;
        IOrganizationService service = null;
        ITracingService tracingService = null;
        public void Execute(IServiceProvider serviceProvider)
        {
            // Lấy context
            context = (IPluginExecutionContext)serviceProvider.GetService(typeof(IPluginExecutionContext));
            serviceFactory = (IOrganizationServiceFactory)serviceProvider.GetService(typeof(IOrganizationServiceFactory));
            service = serviceFactory.CreateOrganizationService(context.UserId);
            tracingService = (ITracingService)serviceProvider.GetService(typeof(ITracingService));
            Entity payment = service.Retrieve("bsd_payment", new Guid(context.InputParameters["id"].ToString()), new Microsoft.Xrm.Sdk.Query.ColumnSet(true));
            var bsd_paymentschemedetailRef = (EntityReference)payment["bsd_paymentschemedetail"];
            var bsd_paymentschemedetail = service.Retrieve("bsd_paymentschemedetail", bsd_paymentschemedetailRef.Id, new ColumnSet(true));
            var ordernumberName = "";
            var orderNumber = (int)bsd_paymentschemedetail["bsd_ordernumber"];
            if ((orderNumber) == 2)
            {
                ordernumberName = "2nd";
            }
            else
            {
                ordernumberName = $"{orderNumber}th";
            }
            string resultName = "";
            string resultValue = "";
            tracingService.Trace("1");
            var query = new QueryExpression("bsd_transactionpayment");
            query.ColumnSet.AllColumns=true;
            query.Criteria.AddCondition("bsd_payment", ConditionOperator.Equal, context.InputParameters["id"].ToString());
            var rs = service.RetrieveMultiple(query);
            var query2 = new QueryExpression("bsd_advancepayment");
            query2.ColumnSet.AllColumns = true;

            query2.Criteria.AddCondition("bsd_payment", ConditionOperator.Equal, context.InputParameters["id"].ToString());
            var rs2 = service.RetrieveMultiple(query2);
            tracingService.Trace("2 "+ rs.Entities.Count.ToString());

            foreach (var item in rs.Entities)
            {

                switch (((OptionSetValue)item["bsd_transactiontype"]).Value)
                {
                    case 100000000:
                        break;
                    case 100000001:
                        tracingService.Trace("3");

                        resultName += $"{ordernumberName} Installment Interest Charge/ Trả lãi suất cho đợt thứ {orderNumber}\n";
                        resultValue += ((Money)item["bsd_amount"]).Value.ToString("N0") + "\n";
                        break;
                    case 100000002:
                        if (item.Contains("bsd_feetype"))
                        {
                            tracingService.Trace("4 " + ((OptionSetValue)item["bsd_feetype"]).Value.ToString());

                            if (((OptionSetValue)item["bsd_feetype"]).Value == 100000000)
                            {
                                resultName += $"Maintenance Fee/ Phí bảo trì\n";
                            }
                            else
                            {
                                resultName += $"Management Fee/ Phí quản lý\n";

                            }
                            tracingService.Trace("4.4");
                            resultValue += ((Money)item["bsd_amount"]).Value.ToString("N0") + "\n";
                        }
                        break;
                    case 100000003:
                        tracingService.Trace("5");

                        resultName += $"Other/ Khoản khác";
                        resultValue += ((Money)item["bsd_amount"]).Value.ToString("N0") + "\n";
                        break;
                    default:
                        break;
                }
            }
            foreach (var item in rs2.Entities)
            {
                tracingService.Trace("6");

                resultName += "Advance Payment/Thanh toán trước hạn";
                resultValue += ((Money)item["bsd_amount"]).Value.ToString("N0") + "\n";

            }
            context.OutputParameters["resultName"] = resultName;
            context.OutputParameters["resultValue"] = resultValue;
        }
    }
}
