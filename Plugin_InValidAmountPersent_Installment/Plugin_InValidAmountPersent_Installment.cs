using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Query;
using Microsoft.Crm.Sdk.Messages;

namespace Plugin_InValidAmountPersent_Installment
{
    public class Plugin_InValidAmountPersent_Installment : IPlugin
    {
        // Thạnh Đỗ  23.10.2018
        IOrganizationService service = null;
        IOrganizationServiceFactory factory = null;
        ITracingService traceService = null;
        IPluginExecutionContext context = null;
        void IPlugin.Execute(IServiceProvider serviceProvider)
        {
            context = (IPluginExecutionContext)serviceProvider.GetService(typeof(IPluginExecutionContext));
            factory = (IOrganizationServiceFactory)serviceProvider.GetService(typeof(IOrganizationServiceFactory));
            service = factory.CreateOrganizationService(context.UserId);
            traceService = (ITracingService)serviceProvider.GetService(typeof(ITracingService));
            Entity target = (Entity)context.InputParameters["Target"];
            #region THẠNH ĐỖ 23.10.2018
            switch (context.MessageName)
            {
                case "Create":
                case "Update":
                    Entity enInstallment = service.Retrieve(target.LogicalName, target.Id, new ColumnSet(true));
                    decimal bsd_amountpercent = enInstallment.Contains("bsd_amountpercent") ? (decimal)enInstallment["bsd_amountpercent"] : 0;
                    int bsd_typepaymentCur = enInstallment.Contains("bsd_typepayment") ? ((OptionSetValue)enInstallment["bsd_typepayment"]).Value : 0;
                    int bsd_numberCur = enInstallment.Contains("bsd_number") ? (int)enInstallment["bsd_number"] : 0;
                    bool bsd_signcontractinstallment = enInstallment.Contains("bsd_signcontractinstallment") ? (bool)enInstallment["bsd_signcontractinstallment"] : false;
                    bool bsd_lastinstallment = enInstallment.Contains("bsd_lastinstallment") ? (bool)enInstallment["bsd_lastinstallment"] : false;

                    EntityReference enrfPaymentScheme = enInstallment.Contains("bsd_paymentscheme") ? ((EntityReference)enInstallment["bsd_paymentscheme"]) : null;
                    EntityCollection enclInstallment = getAllInstallment(enInstallment, enrfPaymentScheme);
                    EntityCollection enclInstallmentSign = getAllInstallmentSign(enInstallment, enrfPaymentScheme);
                    //throw new InvalidPluginExecutionException("enclInstallmentSign:" + enclInstallmentSign.Entities.Count);
                    decimal sum = 0;
                    if (!enInstallment.Contains("bsd_reservation")&& !enInstallment.Contains("bsd_quotation") && !enInstallment.Contains("bsd_optionentry") && !enInstallment.Contains("bsd_conversioncontractapproval") && !enInstallment.Contains("bsd_appendixcontract") && !enInstallment.Contains("bsd_changeinformation"))
                    {
                        if (enclInstallment.Entities.Count > 0)
                        {
                            foreach (Entity en in enclInstallment.Entities)
                            {
                                decimal amountPercent = en.Contains("bsd_amountpercent") ? (decimal)en["bsd_amountpercent"] : 0;
                                int bsd_typepayment = en.Contains("bsd_typepayment") ? ((OptionSetValue)en["bsd_typepayment"]).Value : 0;
                                int bsd_number = en.Contains("bsd_number") ? (int)en["bsd_number"] : 0;
                                if (bsd_typepayment != 0 && bsd_number > 0)
                                {
                                    if (bsd_typepayment == 2 && bsd_number > 0)
                                    {
                                        int numberTemp = bsd_number * (int)amountPercent;
                                        sum += numberTemp;
                                    }
                                }
                                else
                                {
                                    sum += amountPercent;
                                }
                            }
                            if (bsd_typepaymentCur != 0 && bsd_numberCur > 0)
                            {
                                sum += (bsd_amountpercent * bsd_numberCur);
                            }
                            else
                            {
                                sum += bsd_amountpercent;
                            }
                            if (sum > 100)
                            {
                                throw new InvalidPluginExecutionException("Tổng các đợt thanh toán phải bằng 100%!");
                            }
                            if (bsd_lastinstallment == true && sum < 100)
                            {
                                throw new InvalidPluginExecutionException("Tổng các đợt thanh toán phải bằng 100%!");
                            }
                        }
                        if (enclInstallmentSign.Entities.Count == 1 && bsd_signcontractinstallment == true)
                        {
                            foreach (Entity item in enclInstallmentSign.Entities)
                            {
                                int bsd_ordernumber = item.Contains("bsd_ordernumber") ? (int)item["bsd_ordernumber"] : 0;
                                throw new InvalidPluginExecutionException("Đã có đợt thanh toán đủ điều kiện ký hợp đồng: Đợt số " + bsd_ordernumber.ToString() + "");
                            }
                        }
                    }
                   

                    break;
            }
            #endregion

        }

        private EntityCollection getAllInstallment(Entity enInstallment, EntityReference enrfPaymentScheme)
        {
            var QEbsd_paymentschemedetail = new QueryExpression("bsd_paymentschemedetail");
            QEbsd_paymentschemedetail.ColumnSet.AllColumns = true;
            QEbsd_paymentschemedetail.AddOrder("bsd_ordernumber", OrderType.Descending);
            QEbsd_paymentschemedetail.Criteria.AddCondition("bsd_optionentry", ConditionOperator.Null);
            QEbsd_paymentschemedetail.Criteria.AddCondition("bsd_reservation", ConditionOperator.Null);
            QEbsd_paymentschemedetail.Criteria.AddCondition("bsd_quotation", ConditionOperator.Null);
            QEbsd_paymentschemedetail.Criteria.AddCondition("bsd_conversioncontractapproval", ConditionOperator.Null);
            QEbsd_paymentschemedetail.Criteria.AddCondition("bsd_appendixcontract", ConditionOperator.Null);
            QEbsd_paymentschemedetail.Criteria.AddCondition("bsd_changeinformation", ConditionOperator.Null);
            QEbsd_paymentschemedetail.Criteria.AddCondition("bsd_paymentscheme", ConditionOperator.Equal, enrfPaymentScheme.Id);
            QEbsd_paymentschemedetail.Criteria.AddCondition("bsd_paymentschemedetailid", ConditionOperator.NotEqual, enInstallment.Id);
            EntityCollection encl = service.RetrieveMultiple(QEbsd_paymentschemedetail);
            return encl;
        }
        private EntityCollection getAllInstallmentSign(Entity enInstallment, EntityReference enrfPaymentScheme)
        {
            var QEbsd_paymentschemedetail_bsd_signcontractinstallment = true;
            var QEbsd_paymentschemedetail = new QueryExpression("bsd_paymentschemedetail");
            QEbsd_paymentschemedetail.ColumnSet.AllColumns = true;
            QEbsd_paymentschemedetail.AddOrder("bsd_ordernumber", OrderType.Descending);
            QEbsd_paymentschemedetail.Criteria.AddCondition("bsd_optionentry", ConditionOperator.Null);
            QEbsd_paymentschemedetail.Criteria.AddCondition("bsd_reservation", ConditionOperator.Null);
            QEbsd_paymentschemedetail.Criteria.AddCondition("bsd_quotation", ConditionOperator.Null);
            QEbsd_paymentschemedetail.Criteria.AddCondition("bsd_conversioncontractapproval", ConditionOperator.Null);
            QEbsd_paymentschemedetail.Criteria.AddCondition("bsd_appendixcontract", ConditionOperator.Null);
            QEbsd_paymentschemedetail.Criteria.AddCondition("bsd_changeinformation", ConditionOperator.Null);
            QEbsd_paymentschemedetail.Criteria.AddCondition("bsd_paymentscheme", ConditionOperator.Equal, enrfPaymentScheme.Id);
            QEbsd_paymentschemedetail.Criteria.AddCondition("bsd_paymentschemedetailid", ConditionOperator.NotEqual, enInstallment.Id);
            QEbsd_paymentschemedetail.Criteria.AddCondition("bsd_signcontractinstallment", ConditionOperator.Equal, QEbsd_paymentschemedetail_bsd_signcontractinstallment);
            EntityCollection encl = service.RetrieveMultiple(QEbsd_paymentschemedetail);
            return encl;
        }

    }
}
