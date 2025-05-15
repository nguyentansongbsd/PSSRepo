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
            string ValueInstallment = "";
            string nameF = "";
            /*
             * * Đới với Payment Type =IntallMent, hoặc New Intall ment
             * Nếu bsd_differentamount < 0, hoặc bsd_differentamount =0 -->bsd_ordernumbernd Installment / Thanh toán lần thứ oder Number = bsd_amountpay
             * Nếu bsd_differentamount > 0 thì bsd_ordernumbernd Installment / Thanh toán lần thứ oder Number = bsd_balance
             */
            var bsd_differentamount = payment.Contains("bsd_differentamount") ? ((Money)payment["bsd_differentamount"]).Value : 0;
            switch (((OptionSetValue)payment["bsd_paymenttype"]).Value)
            {
                case 100000002:
                    if (bsd_differentamount <= 0)
                    {
                        ValueInstallment = ((Money)payment["bsd_amountpay"]).Value.ToString("N0");
                    }
                    else
                    {
                        ValueInstallment = ((Money)payment["bsd_balance"]).Value.ToString("N0");

                    }
                    var bsd_paymentschemedetailRef = (EntityReference)payment["bsd_paymentschemedetail"];
                    var bsd_paymentschemedetail = service.Retrieve("bsd_paymentschemedetail", bsd_paymentschemedetailRef.Id, new ColumnSet(true));
                    var orderNumber = (int)bsd_paymentschemedetail["bsd_ordernumber"];
                    var ordernumberName = "";
                    if ((orderNumber) == 2)
                    {
                        ordernumberName = "2nd";
                    }
                    else if ((orderNumber) == 3)
                    {
                        ordernumberName = "3rd";
                    }
                    else if ((orderNumber) == 1)
                    {
                        ordernumberName = "1st";
                    }
                    else
                    {
                        ordernumberName = $"{orderNumber}th";
                    }
                    nameF= $"{ordernumberName} Installment / Thanh toán đợt {orderNumber}";
                    break;
                case 100000001:
                    ValueInstallment = ((Money)payment["bsd_amountpay"]).Value.ToString("N0");
                    nameF = $"Deposit fee/ Đặt cọc";

                    break;
                case 100000000:
                    ValueInstallment = ((Money)payment["bsd_amountpay"]).Value.ToString("N0");
                    break;
                default:
                    ValueInstallment = "";
                    break;
            }

            string resultName = "";
            string resultValue = "";
            string giacanho = "";
            #region lấy giá bán căn hộ 
            if (((OptionSetValue)payment["bsd_paymenttype"]).Value == 100000001)
            {
                var enQuoteRef = (EntityReference)payment["bsd_reservation"];
                var enQuote = service.Retrieve(enQuoteRef.LogicalName, enQuoteRef.Id, new ColumnSet(true));
                giacanho = ((Money)enQuote["totalamount"]).Value.ToString("N0");
            }
            else
            {
                var enOPRef = (EntityReference)payment["bsd_optionentry"];
                var enOP = service.Retrieve(enOPRef.LogicalName, enOPRef.Id, new ColumnSet(true));
                giacanho = ((Money)enOP["totalamount"]).Value.ToString("N0");
            }
            #endregion
            tracingService.Trace("1");
            var query = new QueryExpression("bsd_transactionpayment");
            query.ColumnSet.AllColumns = true;
            query.Criteria.AddCondition("bsd_payment", ConditionOperator.Equal, context.InputParameters["id"].ToString());
            var rs = service.RetrieveMultiple(query);
            var query2 = new QueryExpression("bsd_advancepayment");
            query2.ColumnSet.AllColumns = true;

            query2.Criteria.AddCondition("bsd_payment", ConditionOperator.Equal, context.InputParameters["id"].ToString());
            var rs2 = service.RetrieveMultiple(query2);
            tracingService.Trace("2 " + rs.Entities.Count.ToString());

            foreach (var item in rs.Entities)
            {

                switch (((OptionSetValue)item["bsd_transactiontype"]).Value)
                {
                    case 100000000:
                        tracingService.Trace("33");
                        var enInsMapRef = (EntityReference)item["bsd_installment"];
                        var enInsMap = service.Retrieve(enInsMapRef.LogicalName, enInsMapRef.Id, new ColumnSet(true));
                        var orderNumberMap = "";
                        if (((int)enInsMap["bsd_ordernumber"]) == 2)
                        {
                            orderNumberMap = "2nd";
                        }
                        else if ((((int)enInsMap["bsd_ordernumber"])) == 3)
                        {
                            orderNumberMap = "3rd";
                        }
                        else if ((((int)enInsMap["bsd_ordernumber"])) == 1)
                        {
                            orderNumberMap = "1st";
                        }
                        else
                        {
                            orderNumberMap = $"{(int)enInsMap["bsd_ordernumber"]}th";
                        }
                        resultName += $"{orderNumberMap} Installment / Thanh toán đợt {orderNumberMap}\n";
                        resultValue += $"{((Money)item["bsd_amount"]).Value.ToString("N0")}\n";
                        break;
                    case 100000001:
                        tracingService.Trace("3");
                        var enInsTransactionRef = (EntityReference)item["bsd_installment"];
                        var enInsTransaction = service.Retrieve(enInsTransactionRef.LogicalName, enInsTransactionRef.Id, new ColumnSet(true));
                        var orderNumberMapTransaction = "";
                        if (((int)enInsTransaction["bsd_ordernumber"]) == 2)
                        {
                            orderNumberMapTransaction = "2nd";
                        }
                        else if ((((int)enInsTransaction["bsd_ordernumber"])) == 3)
                        {
                            orderNumberMapTransaction = "3rd";
                        }
                        else if ((((int)enInsTransaction["bsd_ordernumber"])) == 1)
                        {
                            orderNumberMapTransaction = "1st";
                        }
                        else
                        {
                            orderNumberMapTransaction = $"{(int)enInsTransaction["bsd_ordernumber"]}th";
                        }
                        resultName += $"{orderNumberMapTransaction} Installment Interest Charge/ Trả lãi suất cho đợt thứ {(int)enInsTransaction["bsd_ordernumber"]}\n";
                        resultValue += $"{((Money)item["bsd_amount"]).Value.ToString("N0")}\n";
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
                            resultValue += $"{((Money)item["bsd_amount"]).Value.ToString("N0")}\n";
                        }
                        break;
                    case 100000003:
                        tracingService.Trace("5");
                        if (item.Contains("bsd_miscellaneous"))
                        {
                            var bsd_miscellaneousRef = (EntityReference)item["bsd_miscellaneous"];
                            var bsd_miscellaneous = service.Retrieve(bsd_miscellaneousRef.LogicalName, bsd_miscellaneousRef.Id, new ColumnSet(true));
                            resultName += $"{bsd_miscellaneous["bsd_description"]}\n";
                        }
                        else
                            resultName += $"Other/ Khoản khác\n";
                        resultValue += $"{((Money)item["bsd_amount"]).Value.ToString("N0")}\n";
                        break;
                    default:
                        break;
                }
            }
            foreach (var item in rs2.Entities)
            {
                tracingService.Trace("6");

                resultName += "Advance Payment/Thanh toán trước hạn\n";
                resultValue += $"{((Money)item["bsd_amount"]).Value.ToString("N0")}\n";

            }
            context.OutputParameters["giacanho"] = giacanho;
            context.OutputParameters["nameF"] = nameF;
            context.OutputParameters["resultName"] = resultName;
            context.OutputParameters["resultValue"] = resultValue;
            context.OutputParameters["valueInstallment"] = ValueInstallment;
        }
    }
}
