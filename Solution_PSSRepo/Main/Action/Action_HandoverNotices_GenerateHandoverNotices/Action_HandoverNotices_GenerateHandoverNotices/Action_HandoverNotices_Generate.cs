using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Remoting.Services;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Crm.Sdk.Messages;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Query;

namespace Action_HandoverNotices_GenerateHandoverNotices
{
    public class Action_HandoverNotices_Generate : IPlugin
    {
        IOrganizationService service = null;
        IOrganizationServiceFactory factory = null;
        ITracingService traceService = null;
        /// <summary>
        /// caseSign=0 : Không tính lãi <br></br>
        /// caseSign=1 : Chỉ tính lãi cho các đợt trước đợt tích Sign Contract Installment <br></br>
        /// caseSign=2 : Chỉ tính lãi cho các đợt sau đợt tích Sign Contract Installment <br></br>
        /// caseSign=3 : Tính lãi cho các đợt theo mức lãi suất bình thường <br></br>
        /// </summary>
        int caseSign = 0;
        void IPlugin.Execute(IServiceProvider serviceProvider)
        {
            IPluginExecutionContext context = (IPluginExecutionContext)serviceProvider.GetService(typeof(IPluginExecutionContext));
            int count = 0;
            factory = (IOrganizationServiceFactory)serviceProvider.GetService(typeof(IOrganizationServiceFactory));
            service = factory.CreateOrganizationService(context.UserId);
            traceService = (ITracingService)serviceProvider.GetService(typeof(ITracingService));
            string pro = "";
            if (context.InputParameters["project"] != null)
            {
                pro = context.InputParameters["project"].ToString();
            }
            else
            {
                pro = null;
            }
            string estimatehandover = "";
            if (context.InputParameters["enstimatehandover"] != null)
            {
                estimatehandover = context.InputParameters["enstimatehandover"].ToString();
            }
            else
            {
                estimatehandover = null;
            }
            //LAY DANH SACH CAC UEHD DETAIL HOP LE
            QueryExpression query = new QueryExpression("bsd_updateestimatehandoverdatedetail");
            query.ColumnSet = new ColumnSet(new string[7] { "bsd_updateestimatehandoverdatedetailid","bsd_updateestimatehandoverdate", "bsd_units", "bsd_optionentry",
                "bsd_installment", "bsd_paymentduedate","bsd_estimatehandoverdatenew" });
            query.Criteria = new FilterExpression(LogicalOperator.And);
            query.Criteria.AddCondition(new ConditionExpression("statuscode", ConditionOperator.Equal, "100000000"));// == APPROVED
            query.Criteria.AddCondition(new ConditionExpression("statecode", ConditionOperator.Equal, 0));// == Active
            query.LinkEntities.Add(new LinkEntity("bsd_updateestimatehandoverdatedetail", "bsd_updateestimatehandoverdate",
                                                        "bsd_updateestimatehandoverdate", "bsd_updateestimatehandoverdateid", JoinOperator.Inner));
            query.LinkEntities[0].LinkCriteria = new FilterExpression(LogicalOperator.And);
            var bsd_types_1 = 100000002;
            var bsd_types_2 = 100000001;
            query.LinkEntities[0].LinkCriteria.AddCondition(new ConditionExpression("bsd_types", ConditionOperator.In, bsd_types_1, bsd_types_2)); // Update Only for Installment or Update All
            query.LinkEntities[0].LinkCriteria.AddCondition(new ConditionExpression("bsd_usegeneratehandovernotice", ConditionOperator.Equal, "1"));//Use Generate Handover Notice = Yes
            query.LinkEntities[0].LinkCriteria.AddCondition(new ConditionExpression("bsd_generated", ConditionOperator.Equal, "0"));// == NO
            if (pro != "-1")
            {
                query.LinkEntities[0].LinkCriteria.AddCondition(new ConditionExpression("bsd_project", ConditionOperator.Equal, pro));
            }
            if (estimatehandover != "-1")
            {
                query.LinkEntities[0].LinkCriteria.AddCondition(new ConditionExpression("bsd_updateestimatehandoverdateid", ConditionOperator.Equal, estimatehandover));
            }
            //query.LinkEntities[0].LinkCriteria.AddCondition(new ConditionExpression("createdon", ConditionOperator.Today)); 

            //query.LinkEntities[0].LinkCriteria = new FilterExpression(LogicalOperator.Or);
            //query.LinkEntities[0].LinkCriteria.AddCondition(new ConditionExpression("bsd_generated", ConditionOperator.Equal, "0"));// == NO
            //query.LinkEntities[0].LinkCriteria.AddCondition(new ConditionExpression("bsd_generated", ConditionOperator.Null));// NULL

            query.LinkEntities.Add(new LinkEntity("bsd_updateestimatehandoverdatedetail", "salesorder",
                                                        "bsd_optionentry", "salesorderid", JoinOperator.Inner));
            query.LinkEntities[1].LinkCriteria = new FilterExpression(LogicalOperator.And);
            query.LinkEntities[1].LinkCriteria.AddCondition(new ConditionExpression("bsd_signedcontractdate", ConditionOperator.NotNull));// Not NULL

            query.LinkEntities.Add(new LinkEntity("bsd_updateestimatehandoverdatedetail", "product",
                                                        "bsd_units", "productid", JoinOperator.Inner));
            query.LinkEntities[2].LinkCriteria = new FilterExpression(LogicalOperator.And);
            query.LinkEntities[2].LinkCriteria.AddCondition(new ConditionExpression("statuscode", ConditionOperator.Equal, "100000002"));// == Sold

            //Hân - 02.03.2018
            //query.LinkEntities.Add(new LinkEntity("bsd_updateestimatehandoverdatedetail", "bsd_project",
            //                                            "bsd_project", "bsd_projectid", JoinOperator.Inner));
            //query.LinkEntities[3].LinkCriteria = new FilterExpression(LogicalOperator.And);
            //query.LinkEntities[3].LinkCriteria.AddCondition(new ConditionExpression("bsd_projectcode", ConditionOperator.Equal, "SSA"));

            EntityCollection list = service.RetrieveMultiple(query);
            //throw new InvalidPluginExecutionException(list.Entities.Count.ToString());

            foreach (Entity detail in list.Entities)
            {
                DateTime today = RetrieveLocalTimeFromUTCTime(DateTime.Now, service);
                //KIEM TRA DA CO HANDOVER NOTICES CHUA?
                if (detail.Contains("bsd_optionentry") && CheckExistHandoverNotices(service, detail.Contains("bsd_optionentry")?(EntityReference)detail["bsd_optionentry"]:null, detail.Contains("bsd_installment")?(EntityReference)detail["bsd_installment"]:null) == false)
                {
                    //Han-28.05.2018: Add Simulation Date 
                    Entity UpEHDEn = service.Retrieve(((EntityReference)detail["bsd_updateestimatehandoverdate"]).LogicalName,
                        ((EntityReference)detail["bsd_updateestimatehandoverdate"]).Id,
                            new ColumnSet(true));
                    DateTime UpEHD_SimuDate = today;
                    if (UpEHDEn.Contains("bsd_simulationdate"))
                        UpEHD_SimuDate = RetrieveLocalTimeFromUTCTime((DateTime)UpEHDEn["bsd_simulationdate"], service);
                    Boolean bsd_isincludelastinstallment = UpEHDEn.Contains("bsd_isincludelastinstallment") ? (Boolean)UpEHDEn["bsd_isincludelastinstallment"] : false;
                    //Boolean bsd_isincludelastinstallment = true;
                    //CHUA CO --> TAO HANDOVER NOTICE
                    decimal maintenanceF = decimal.Zero;
                    decimal managementF = decimal.Zero;
                    decimal outstandingUnPaid = decimal.Zero;
                    decimal actualInterest = decimal.Zero;
                    decimal estimateInterest = decimal.Zero;
                    decimal advancePaymentAmount = decimal.Zero;
                    decimal installmentAmount = decimal.Zero;
                    decimal orther = decimal.Zero;

                    decimal TotalSysRe = decimal.Zero;
                    //EntityReference enrefUpdatEestimateHandoverdate = detail.Contains("bsd_updateestimatehandoverdate") ? (EntityReference)detail["bsd_updateestimatehandoverdate"] : null;
                    Entity hn = new Entity("bsd_handovernotice");
                    if (detail.Contains("bsd_optionentry"))
                    {
                        hn["bsd_name"] = "Handover Notices of " + ((EntityReference)detail["bsd_optionentry"]).Name;
                        Entity OE = service.Retrieve(((EntityReference)detail["bsd_optionentry"]).LogicalName, ((EntityReference)detail["bsd_optionentry"]).Id,
                            new ColumnSet(new string[] { "name", "bsd_paymentscheme", "bsd_totalpercent", "customerid", "bsd_estimatehandoverdatecontract", "bsd_freightamount", "bsd_managementfee", "bsd_depositamount", "bsd_project", "bsd_numberofmonthspaidmf" }));
                        if (OE.Contains("customerid"))
                        {
                            hn["bsd_customer"] = OE["customerid"];

                            EntityCollection advance = CalculateAdvancePayment(service, (EntityReference)OE["customerid"], (EntityReference)OE["bsd_project"],OE.ToEntityReference());
                            foreach (Entity e in advance.Entities)
                            {
                                advancePaymentAmount = (e.Contains("remaining") && ((AliasedValue)e["remaining"]).Value != null) ? ((Money)((AliasedValue)e["remaining"]).Value).Value : decimal.Zero;
                            }
                        }

                        if (OE.Contains("bsd_estimatehandoverdatecontract"))
                            hn["bsd_handoverdatebaseoncontract"] = OE["bsd_estimatehandoverdatecontract"];

                        if (OE.Contains("bsd_project"))
                            hn["bsd_project"] = OE["bsd_project"];
                        hn["bsd_totalamountpaid"] = OE.Contains("bsd_totalpercent") ? OE["bsd_totalpercent"] : decimal.Zero;
                        hn["bsd_optionentry"] = OE.ToEntityReference();
                        if (OE.Contains("bsd_freightamount")) maintenanceF = ((Money)OE["bsd_freightamount"]).Value;
                        if (OE.Contains("bsd_managementfee")) managementF = ((Money)OE["bsd_managementfee"]).Value;

                        //Han_15082018 : Get Remaining Amount of Mana Fee + Main Fee
                        EntityCollection InstallFee = CalSum_FeeRemaining(service, OE.ToEntityReference());

                        foreach (Entity e in InstallFee.Entities)
                        {
                            if (e.Contains("MainFeeReAmt") && ((AliasedValue)e["MainFeeReAmt"]).Value != null)
                                maintenanceF = ((Money)((AliasedValue)e.Attributes["MainFeeReAmt"]).Value).Value;

                            if (e.Contains("MainFeeReAmt") && ((AliasedValue)e["ManaFeeReAmt"]).Value != null)
                                managementF = ((Money)((AliasedValue)e.Attributes["ManaFeeReAmt"]).Value).Value;
                        }                       
                        hn["bsd_maintenancefee"] = new Money(maintenanceF);
                        hn["bsd_managementfee"] = new Money(managementF);
                        hn["bsd_depositamount"] = OE.Contains("bsd_depositamount") ? OE["bsd_depositamount"] : new Money(decimal.Zero);
                        hn["bsd_numberofmonthspaidmf"] = OE.Contains("bsd_numberofmonthspaidmf") ? OE["bsd_numberofmonthspaidmf"] : 0;
                        hn["bsd_updateestimatehandoverdatedetail"] = detail.ToEntityReference();
                        EntityCollection calculateOutstanding = CalculateOutstanding(service, OE.ToEntityReference(), bsd_isincludelastinstallment);
                        foreach (Entity e in calculateOutstanding.Entities)
                        {
                            if (e.Contains("balance") && ((AliasedValue)e["balance"]).Value != null)
                                outstandingUnPaid = ((Money)((AliasedValue)e.Attributes["balance"]).Value).Value;
                        }

                        // Total System Receipt
                        EntityCollection SysReceiptEn = CalSum_SystemReceipt(service, OE.ToEntityReference());

                        foreach (Entity e in SysReceiptEn.Entities)
                        {
                            if (e.Contains("TotalSysRe") && ((AliasedValue)e["TotalSysRe"]).Value != null)
                                TotalSysRe = ((Money)((AliasedValue)e.Attributes["TotalSysRe"]).Value).Value;
                        }

                        outstandingUnPaid = outstandingUnPaid + TotalSysRe;

                        //Waiver
                        decimal waiverSum = 0;
                        EntityCollection calculateActualInterest = CalculateActualInterest(service, OE.ToEntityReference());
                        foreach (Entity e in calculateActualInterest.Entities)
                        {
                            decimal interestSum = (e.Contains("amount") && ((AliasedValue)e["amount"]).Value != null) ? ((Money)((AliasedValue)e["amount"]).Value).Value : decimal.Zero;
                            decimal interestPaidSum = (e.Contains("paid") && ((AliasedValue)e["paid"]).Value != null) ? ((Money)((AliasedValue)e["paid"]).Value).Value : decimal.Zero;
                            waiverSum = (e.Contains("waiver") && ((AliasedValue)e["waiver"]).Value != null) ? ((Money)((AliasedValue)e["waiver"]).Value).Value : decimal.Zero;
                            actualInterest = interestSum - interestPaidSum - waiverSum;
                            hn["bsd_totalwaiveramount"] = new Money(waiverSum);
                        }
                        EntityCollection calculateOther = CalculateOther(service, OE.ToEntityReference(), true);
                        foreach (Entity e in calculateOther.Entities)
                        {
                            orther = (e.Contains("sumMis") && ((AliasedValue)e["sumMis"]).Value != null) ? ((Money)((AliasedValue)e.Attributes["sumMis"]).Value).Value : decimal.Zero;
                        }
                        //estimateInterest = Interest(service, OE, today.Date);
                        estimateInterest = Interest(service, OE, UpEHD_SimuDate.Date);
                        
                        EntityCollection oe_pay = fect_paymentcheme(service, OE);
                        Entity oe_paymentcheme = oe_pay.Entities[0];                       
                        decimal totalamount = oe_paymentcheme.Contains("totalamount") ? ((Money)oe_paymentcheme["totalamount"]).Value : 0;
                        EntityReference PaymentScheme = oe_paymentcheme.Contains("bsd_paymentscheme") ? (EntityReference)oe_paymentcheme["bsd_paymentscheme"] : null;
                        Entity enPaymentScheme = service.Retrieve(PaymentScheme.LogicalName, PaymentScheme.Id, new ColumnSet(true));
                        EntityReference enrefInterestRateMaster = enPaymentScheme.Contains("bsd_interestratemaster") ? (EntityReference)enPaymentScheme["bsd_interestratemaster"] : null;
                        decimal lim = 0;
                        if (enrefInterestRateMaster != null)
                        {
                           
                            Entity enInterestRateMaster = service.Retrieve(enrefInterestRateMaster.LogicalName, enrefInterestRateMaster.Id, new ColumnSet(true));
                            decimal bsd_toleranceinterestamount = enInterestRateMaster.Contains("bsd_toleranceinterestamount") ? ((Money)enInterestRateMaster["bsd_toleranceinterestamount"]).Value : 0;
                            decimal bsd_toleranceinterestpercentage = enInterestRateMaster.Contains("bsd_toleranceinterestpercentage") ? (decimal)enInterestRateMaster["bsd_toleranceinterestpercentage"] : 0;
                            decimal amountcalbypercent = totalamount * bsd_toleranceinterestpercentage / 100;
                            traceService.Trace("bsd_toleranceinterestamount: " + bsd_toleranceinterestamount.ToString());
                            traceService.Trace("bsd_toleranceinterestpercentage: " + bsd_toleranceinterestpercentage.ToString());
                            traceService.Trace("amountcalbypercent: " + bsd_toleranceinterestamount.ToString());
                            if (bsd_toleranceinterestamount > 0 && amountcalbypercent > 0)
                            {
                                lim = Math.Min(bsd_toleranceinterestamount, amountcalbypercent);
                            }
                            else
                            {
                                if (bsd_toleranceinterestamount > 0)
                                {
                                    lim = bsd_toleranceinterestamount;
                                }
                                if (amountcalbypercent > 0)
                                {
                                    lim = amountcalbypercent;
                                }
                            }
                        }
                        estimateInterest = estimateInterest < lim ? estimateInterest : lim;
                        //estimateInterest = Interest(service, OE, today.Date);
                    }
                    else
                        hn["bsd_name"] = "Handover Notices";

                    if (detail.Contains("bsd_installment"))
                    {
                        hn["bsd_installment"] = detail["bsd_installment"];
                        Entity ins = service.Retrieve(((EntityReference)detail["bsd_installment"]).LogicalName, ((EntityReference)detail["bsd_installment"]).Id,
                            new ColumnSet(new string[2] { "bsd_amountofthisphase", "bsd_balance" }));
                        //installmentAmount = ins.Contains("bsd_amountofthisphase") ? ((Money)ins["bsd_amountofthisphase"]).Value : decimal.Zero;
                        installmentAmount = ins.Contains("bsd_balance") ? ((Money)ins["bsd_balance"]).Value : decimal.Zero;
                        hn["bsd_installmentamount"] = new Money(installmentAmount);
                    }

                    hn["bsd_advancepaymentamount"] = new Money(advancePaymentAmount);
                    hn["bsd_outstandingincludeinterest"] = new Money(outstandingUnPaid);
                    hn["bsd_actualinterest"] = new Money(actualInterest);

                    hn["bsd_estimateintesrest"] = new Money(estimateInterest); // HN = estimateInterest = 0
                    
                    hn["bsd_totalinterestamount"] = new Money(actualInterest + estimateInterest);
                    hn["bsd_subject"] = "Handover Notices";
                    hn["bsd_issuedate"] = today.Date;
                    if (detail.Contains("bsd_units"))
                        hn["bsd_units"] = detail["bsd_units"];
                    if (detail.Contains("bsd_paymentduedate"))
                        hn["bsd_paymentduedate"] = detail["bsd_paymentduedate"];
                    if (detail.Contains("bsd_estimatehandoverdatenew"))
                        hn["bsd_handoverdateactual"] = detail["bsd_estimatehandoverdatenew"];
                    //hn["bsd_other"] = new Money(orther - TotalSysRe);
                    hn["bsd_other"] = new Money(orther);
                    decimal total = managementF + maintenanceF + outstandingUnPaid + orther + actualInterest + estimateInterest + installmentAmount - advancePaymentAmount;
                    //throw new InvalidPluginExecutionException(total.ToString());
                    hn["bsd_totalamount"] = new Money(total);
                    hn["bsd_generatedbysystem"] = true;
                    if (UpEHDEn.Contains("bsd_isincludelastinstallment"))
                    {
                        if (bsd_isincludelastinstallment == true)
                        {
                            hn["bsd_isincludelastinstallment"] = true;
                        }
                        else
                        {
                            hn["bsd_isincludelastinstallment"] = false;
                        }
                    }
                    else
                    {
                        hn["bsd_isincludelastinstallment"] = false;
                    }
                    service.Create(hn);

                    //UPDATE UEHD
                    Entity uehd = new Entity("bsd_updateestimatehandoverdate");
                    uehd.Id = ((EntityReference)detail["bsd_updateestimatehandoverdate"]).Id;
                    uehd["bsd_generated"] = true;
                    service.Update(uehd);

                    count++;
                }
            }
            context.OutputParameters["ReturnId"] = count.ToString();

        }
        private decimal Interest(IOrganizationService crmservices, Entity oe, DateTime dateCalculate)
        {
            decimal interest = 0;
            //GET INSTALLMENT
            QueryExpression q1 = new QueryExpression("bsd_paymentschemedetail");
            q1.ColumnSet = new ColumnSet(new string[] { "bsd_paymentschemedetailid", "bsd_duedate", "bsd_balance", });
            q1.Criteria = new FilterExpression(LogicalOperator.And);
            q1.Criteria.AddCondition(new ConditionExpression("bsd_optionentry", ConditionOperator.Equal, oe.Id));
            q1.Criteria.AddCondition(new ConditionExpression("statuscode", ConditionOperator.Equal, "100000000"));//NOT PAID
            q1.Criteria.AddCondition(new ConditionExpression("bsd_duedate", ConditionOperator.OnOrBefore, dateCalculate));//Duedate <= simulation date
            EntityCollection listInstallment = service.RetrieveMultiple(q1);
            if (listInstallment.Entities.Count > 0)
            {
                // GET INTEREST CHARGE MASTER
                if (!oe.Contains("bsd_paymentscheme"))
                    throw new InvalidPluginExecutionException("Please input Payment Scheme in Option entry: " + (oe.Contains("name") ? oe["name"] : ""));
                Entity payScheme = service.Retrieve(((EntityReference)oe["bsd_paymentscheme"]).LogicalName, ((EntityReference)oe["bsd_paymentscheme"]).Id,
                    new ColumnSet(new string[2] {
                                "bsd_name",
                                "bsd_interestratemaster"
                    }));
                if (!payScheme.Contains("bsd_interestratemaster"))
                    throw new InvalidPluginExecutionException("Please input Interest Charge Master in Payment Scheme: " + (payScheme.Contains("bsd_name") ? payScheme["bsd_name"] : ""));

                Entity interestMaster = service.Retrieve(((EntityReference)payScheme["bsd_interestratemaster"]).LogicalName, ((EntityReference)payScheme["bsd_interestratemaster"]).Id,
                    new ColumnSet(new string[]
                    {
                                "bsd_intereststartdatetype",
                                "bsd_gracedays",
                                "bsd_termsinterestpercentage"
                    }));
                if (!(interestMaster.Contains("bsd_intereststartdatetype") && interestMaster.Contains("bsd_gracedays") && interestMaster.Contains("bsd_termsinterestpercentage")))
                    throw new InvalidPluginExecutionException("Interest charge master not enough infomation required. Please check again!");

                //Khai báo biến
                int Graceday = ((int)interestMaster["bsd_gracedays"]);
                int interestStartDateType = ((OptionSetValue)interestMaster["bsd_intereststartdatetype"]).Value;//100000000: Due date;         100000001: Grace period  
                decimal interestMasterPercent = (decimal)interestMaster["bsd_termsinterestpercentage"];
                decimal interestProjectDaily = 0;

                interestProjectDaily = DailyInterest(service, oe.ToEntityReference());

                decimal interestPercent = interestMasterPercent + interestProjectDaily;


                var bsd_signedcontractdate = new DateTime();

                var bsd_signeddadate = new DateTime();
                //CREATE INTEREST SIMULATION DETAIL
                foreach (Entity ins in listInstallment.Entities)
                {
                    int lateDays = 0;
                    decimal balance = 0;
                    interestPercent =(decimal) ins["bsd_interestchargeper"];
                    //TINH LAI

                    DateTime duedate = RetrieveLocalTimeFromUTCTime((DateTime)ins["bsd_duedate"], service);

                    DateTime InterestStarDate = duedate.AddDays(Graceday);

                    if (InterestStarDate.Date < dateCalculate.Date)
                    {

                        lateDays = (int)dateCalculate.Date.Subtract(InterestStarDate.Date).TotalDays;
                        if (interestStartDateType == 100000000)// Interest Start Date type: 100000000--> DueDate
                        {
                            lateDays = lateDays + Graceday;
                        }

                        if (oe.Contains("bsd_signedcontractdate"))
                        {
                            bsd_signedcontractdate = (DateTime)oe["bsd_signedcontractdate"];
                            caseSign = 2;
                            if (oe.Contains("bsd_signeddadate"))
                            {
                                bsd_signeddadate = (DateTime)oe["bsd_signeddadate"];
                                caseSign = 3;
                            }
                        }
                        else
                        {
                            if (oe.Contains("bsd_signeddadate"))
                            {
                                bsd_signeddadate = (DateTime)oe["bsd_signeddadate"];
                                caseSign = 1;
                            }
                        }
                        var latedays2 = lateDays;
                        traceService.Trace("lateDays " + lateDays);

                        var resCheckCaseSign = checkCaseSignAndCalLateDays(ins,oe,bsd_signedcontractdate, bsd_signeddadate, dateCalculate, ref latedays2);
                        lateDays = latedays2 <= lateDays ? latedays2 : lateDays;
                        traceService.Trace("lateDays_ " + lateDays);
                        balance = ins.Contains("bsd_balance") ? ((Money)ins["bsd_balance"]).Value : decimal.Zero;
                        if(resCheckCaseSign)
                        {
                            decimal Newinterest = CalculateNewInterest(balance, lateDays, interestPercent);
                            interest = interest + Newinterest;
                        }    
                        traceService.Trace("interest: " + interest.ToString());
                    }
                }
            }

            return interest;
        }
        public bool checkCaseSignAndCalLateDays(Entity enInstallment,Entity enOptionEntry, DateTime bsd_signedcontractdate, DateTime bsd_signeddadate, DateTime receiptdate, ref int lateDays)
        {
            bool result = false;
            bool isContainDueDate = false;
            if (enInstallment.Contains("bsd_duedate"))
                isContainDueDate = true;

            var bsd_duedateFlag = new DateTime();
            var bsd_duedate = new DateTime();
            if (isContainDueDate == true)
            {
                bsd_duedate = (DateTime)enInstallment["bsd_duedate"];
            }
            #region lấy ra đợt tích Sign Contract Installment
            var query_bsd_signcontractinstallment = true;

            var query = new QueryExpression("bsd_paymentschemedetail");
            query.ColumnSet.AllColumns = true;
            query.Criteria.AddCondition("bsd_signcontractinstallment", ConditionOperator.Equal, query_bsd_signcontractinstallment);
            query.Criteria.AddCondition("bsd_optionentry", ConditionOperator.Equal, enOptionEntry.Id.ToString());
            var rs = service.RetrieveMultiple(query);
            traceService.Trace($"enOptionEntry {enOptionEntry.Id}");
            traceService.Trace($"enInstallment {enInstallment.Id}");
            #endregion
            switch (caseSign)
            {
                case 0:
                    result = false;
                    break;
                case 1:
                    if (rs.Entities.Count > 0)
                    {
                        if (rs.Entities[0].Id == enInstallment.Id)
                        {
                            result = false;
                            break;
                        }
                        if (isContainDueDate == false)
                            result = false;
                        else
                        {
                            bsd_duedateFlag = (DateTime)rs.Entities[0]["bsd_duedate"];
                            if (bsd_duedate < bsd_duedateFlag)
                            {
                                result = true;
                                //tính số ngày trễ hạn 
                                lateDays = (int)(receiptdate - bsd_signeddadate).TotalDays;
                            }
                            else result = false;
                        }
                    }
                    break;
                case 2:

                    if (rs.Entities.Count > 0)
                    {
                        if (isContainDueDate == false)
                            result = false;
                        else
                        {
                            bsd_duedateFlag = (DateTime)rs.Entities[0]["bsd_duedate"];
                            if (bsd_duedate >= bsd_duedateFlag)
                            {
                                result = true;
                                lateDays = (int)(receiptdate - bsd_signedcontractdate).TotalDays;
                            }
                            else result = false;
                        }
                    }
                    break;
                case 3:
                    result = true;
                    bsd_duedateFlag = (DateTime)rs.Entities[0]["bsd_duedate"];
                    if (bsd_duedate < bsd_duedateFlag)
                    {
                        //tính số ngày trễ hạn 
                        lateDays = (int)(receiptdate - bsd_signeddadate).TotalDays;
                    }
                    else
                    {
                        //tính số ngày trễ hạn 
                        lateDays = (int)(receiptdate - bsd_signedcontractdate).TotalDays;
                    }
                    break;
                default:
                    result = false;
                    break;
            }
            traceService.Trace($"start caseSign {caseSign}");
            traceService.Trace($"rs.Entities.Count {rs.Entities.Count}");

            traceService.Trace("resultCaseSign:" + result);
            return result;
        }
        private decimal CalculateNewInterest(decimal balance, int lateDays, decimal interestPercent)
        {
            //decimal interest = balance * lateDays * (interestPercent / 30 / 100);
            decimal interest = balance * lateDays * (interestPercent / 100);

            return interest;
        }
        private decimal DailyInterest(IOrganizationService crmservices, EntityReference oe)
        {
            string fetchXml =
                  @"<fetch version='1.0' output-format='xml-platform' count='1' mapping='logical' distinct='true' >
                    <entity name='bsd_dailyinterestrate'>
                    <attribute name='bsd_dailyinterestrateid' />
                    <attribute name='bsd_interestrate' />
                    <attribute name='bsd_date' />
                    <order attribute='bsd_date' descending='true' />
                    <link-entity name='bsd_project' from='bsd_projectid' to='bsd_project' alias='ae'>
                      <link-entity name='salesorder' from='bsd_project' to='bsd_projectid' alias='af'>
                        <filter type='and'>
                          <condition attribute='salesorderid' operator='eq' uitype='salesorder' value='{0}' />
                        </filter>
                      </link-entity>
                    </link-entity>
                  </entity>
                </fetch>    ";
            fetchXml = string.Format(fetchXml, oe.Id);
            EntityCollection entc = crmservices.RetrieveMultiple(new FetchExpression(fetchXml));
            if (entc.Entities.Count > 0 && (entc.Entities[0].Contains("bsd_interestrate")))
            {
                return (decimal)entc.Entities[0]["bsd_interestrate"];
            }
            return 0;
        }

        private bool CheckExistHandoverNotices(IOrganizationService crmservices, EntityReference oe, EntityReference ins)
        {
            string fetchXml =
                  @"<fetch version='1.0' output-format='xml-platform' count='1' mapping='logical' distinct='false'>
                      <entity name='bsd_handovernotice'>
                        <attribute name='bsd_handovernoticeid' />
                        <filter type='and'>
                          <condition attribute='bsd_optionentry' operator='eq'  uitype='salesorder' value='{0}' />
                         <condition attribute='bsd_installment' operator='eq'  value='{1}' />
                          <condition attribute='statuscode' operator='eq' value='1'/>
                          <condition attribute='statecode' operator='eq' value='0'/>
                        </filter>
                      </entity>
                    </fetch>";
            fetchXml = string.Format(fetchXml, oe.Id,ins.Id);
            EntityCollection entc = crmservices.RetrieveMultiple(new FetchExpression(fetchXml));
            return (entc.Entities.Count > 0 ? true : false);
        }
        private EntityCollection CalculateOutstanding(IOrganizationService crmservices, EntityReference oe,Boolean isincludelastinstallment)
        {
            string fetchXml = "";
            if(isincludelastinstallment==false)
            { 
             fetchXml =
                  @"<fetch aggregate='true' >
                  <entity name='bsd_paymentschemedetail' >
                    <attribute name='bsd_balance' alias='balance' aggregate='sum' />
                    <filter>
                      <condition attribute='bsd_optionentry' operator='eq' value='{0}' />
                      <condition attribute='bsd_duedatecalculatingmethod' operator='ne' value='100000002' />
                      <condition attribute='bsd_lastinstallment' operator='ne' value='1' />
                    </filter>
                  </entity>
                </fetch>";
                fetchXml = string.Format(fetchXml, oe.Id);
            }
            else
            {
                 fetchXml =
                        @"<fetch aggregate='true' >
                  <entity name='bsd_paymentschemedetail' >
                    <attribute name='bsd_balance' alias='balance' aggregate='sum' />
                    <filter>
                      <condition attribute='bsd_optionentry' operator='eq' value='{0}' />
                      <condition attribute='bsd_duedatecalculatingmethod' operator='ne' value='100000002' />
                    </filter>
                  </entity>
                </fetch>";
                fetchXml = string.Format(fetchXml, oe.Id);
            }
            EntityCollection entc = crmservices.RetrieveMultiple(new FetchExpression(fetchXml));
            return entc;
        }

        private EntityCollection CalSum_SystemReceipt(IOrganizationService crmservices, EntityReference oe)
        {
            string fetchXml =
                  @"<fetch aggregate='true' >
                  <entity name='bsd_systemreceipt' >
                    <attribute name='bsd_amountpay' alias='TotalSysRe' aggregate='sum' />
                    <filter type='and'>
                      <condition attribute='bsd_optionentry' operator='eq' value='{0}' />
                      <condition attribute='statuscode' operator='eq' value='100000000' />
                    </filter>
                  </entity>
                </fetch>";
            fetchXml = string.Format(fetchXml, oe.Id);
            EntityCollection entc = crmservices.RetrieveMultiple(new FetchExpression(fetchXml));
            return entc;
        }

        private EntityCollection CalculateOther(IOrganizationService crmservices, EntityReference oe,Boolean isincludelastinstallment)
        {
            string fetchXml = "";
            if (isincludelastinstallment == false)
            {
                 fetchXml =
                      @"<fetch  aggregate='true'>
                  <entity name='bsd_miscellaneous'>
                    <attribute name='bsd_balance' alias='sumMis' aggregate='sum'  />
                    <link-entity name='bsd_paymentschemedetail' from='bsd_paymentschemedetailid' to='bsd_installment' alias='aa'>
                      <filter type='and'>
                        <condition attribute='bsd_optionentry' operator='eq'  uitype='salesorder' value='{0}' />
                        <condition attribute='bsd_lastinstallment' operator='ne' value='1' />
                         </filter>
                    </link-entity>
                        <filter type='and'>
                        <condition attribute='statuscode' operator='eq' value='1' />
                      </filter>
                  </entity>
                </fetch>";
                fetchXml = string.Format(fetchXml, oe.Id);
            }
            else
            {
                 fetchXml =
                     @"<fetch  aggregate='true'>
                  <entity name='bsd_miscellaneous'>
                    <attribute name='bsd_balance' alias='sumMis' aggregate='sum'  />
                    <link-entity name='bsd_paymentschemedetail' from='bsd_paymentschemedetailid' to='bsd_installment' alias='aa'>
                      <filter type='and'>
                        <condition attribute='bsd_optionentry' operator='eq'  uitype='salesorder' value='{0}' />
                         </filter>
                    </link-entity>
                        <filter type='and'>
                        <condition attribute='statuscode' operator='eq' value='1' />
                      </filter>
                  </entity>
                </fetch>";
                fetchXml = string.Format(fetchXml, oe.Id);
            }
            EntityCollection entc = crmservices.RetrieveMultiple(new FetchExpression(fetchXml));
            return entc;
        }
        private EntityCollection CalculateActualInterest(IOrganizationService crmservices, EntityReference oe)
        {
            string fetchXml =
                  @"<fetch  aggregate='true' >
                  <entity name='bsd_paymentschemedetail' >
                    <attribute name='bsd_interestchargeamount' alias='amount' aggregate='sum' />
                    <attribute name='bsd_interestwaspaid' alias='paid' aggregate='sum' />
                    <attribute name='bsd_waiverinterest' alias='waiver' aggregate='sum' />
                    <filter>
                      <condition attribute='bsd_optionentry' operator='eq' value='{0}' />
                    </filter>
                  </entity>
                </fetch>";
            fetchXml = string.Format(fetchXml, oe.Id);
            EntityCollection entc = crmservices.RetrieveMultiple(new FetchExpression(fetchXml));
            return entc;
        }
        private EntityCollection CalculateAdvancePayment(IOrganizationService crmservices, EntityReference cus, EntityReference pro, EntityReference oe)
        {
            //string fetchXml =
            //      @"<fetch version='1.0' output-format='xml-platform' mapping='logical' distinct='false'  aggregate='true' >
            //      <entity name='bsd_advancepayment'>
            //        <attribute name='bsd_remainingamount' alias='remaining' aggregate='sum'  />
            //        <filter type='and'>
            //          <condition attribute='bsd_customer' operator='eq' value='{0}' />
            //          <condition attribute='statuscode' operator='eq' value='100000000' />
            //          <condition attribute='bsd_project' operator='eq' value='{1} '/>
            //          <condition attribute='bsd_optionentry' operator='eq' value='{2} '/>
            //        </filter>
            //      </entity>
            //    </fetch>";

            string fetchXml =
                  @"<fetch version='1.0' output-format='xml-platform' mapping='logical' distinct='false'  aggregate='true' >
                  <entity name='bsd_advancepayment'>
                    <attribute name='bsd_remainingamount' alias='remaining' aggregate='sum'  />
                    <filter type='and'>
                      <condition attribute='statuscode' operator='eq' value='100000000' />
                      <condition attribute='bsd_optionentry' operator='eq' value='{2} '/>
                    </filter>
                  </entity>
                </fetch>";

            fetchXml = string.Format(fetchXml, cus.Id, pro.Id, oe.Id);
            EntityCollection entc = crmservices.RetrieveMultiple(new FetchExpression(fetchXml));
            return entc;
        }

        private DateTime RetrieveLocalTimeFromUTCTime(DateTime utcTime, IOrganizationService service)
        {
            var currentUserSettings = service.RetrieveMultiple(
           new QueryExpression("usersettings")
           {
               ColumnSet = new ColumnSet("localeid", "timezonecode"),
               Criteria = new FilterExpression
               {
                   Conditions =
           {
            new ConditionExpression("systemuserid", ConditionOperator.EqualUserId)
           }
               }
           }).Entities[0].ToEntity<Entity>();

            int? timeZoneCode = (int?)currentUserSettings.Attributes["timezonecode"];
            if (!timeZoneCode.HasValue)
                throw new Exception("Can't find time zone code");

            var request = new LocalTimeFromUtcTimeRequest
            {
                TimeZoneCode = timeZoneCode.Value,
                UtcTime = utcTime.ToUniversalTime()
            };

            var response = (LocalTimeFromUtcTimeResponse)service.Execute(request);

            return response.LocalTime;
            //var utcTime = utcTime.ToString("MM/dd/yyyy HH:mm:ss");
            //var localDateOnly = response.LocalTime.ToString("dd-MM-yyyy");
        }
        private EntityCollection fect_paymentcheme(IOrganizationService crmservices, Entity oe)
        {
            string fetchXml =
                  @"<fetch version='1.0' output-format='xml-platform' mapping='logical' distinct='false'>
  <entity name='salesorder'>
    <attribute name='name' />
    <attribute name='customerid' />
    <attribute name='statuscode' />
    <attribute name='totalamount' />
    <attribute name='bsd_unitnumber' />
    <attribute name='bsd_project' />
    <attribute name='bsd_optionno' />
    <attribute name='createdon' />
    <attribute name='bsd_paymentscheme' />
    <attribute name='bsd_contractnumber' />
    <attribute name='totalamount' />
    <attribute name='bsd_phaseslaunch' />
    <attribute name='salesorderid' />
    <order attribute='createdon' descending='true' />
    <filter type='and'>
      <condition attribute='salesorderid' operator='eq' value='{0}' />
    </filter>
  </entity>
</fetch>";
            fetchXml = string.Format(fetchXml, oe.Id);
            EntityCollection entc = crmservices.RetrieveMultiple(new FetchExpression(fetchXml));
            return entc;
   }
        private EntityCollection fect_ins(IOrganizationService crmservices, EntityReference oe)
        {
            string fetchXml =
                  @"<fetch version='1.0' output-format='xml-platform' mapping='logical' distinct='false'>
  <entity name='bsd_paymentschemedetail'>
    <attribute name='bsd_paymentschemedetailid' />
    <attribute name='bsd_name' />
    <attribute name='createdon' />
    <order attribute='bsd_name' descending='false' />
    <filter type='and'>
      <condition attribute='bsd_optionentry' operator='eq' value='{0}' />
      <condition attribute='bsd_managementfee' operator='eq' value='1' />
    </filter>
  </entity>
</fetch>";
            fetchXml = string.Format(fetchXml, oe.Id);
            EntityCollection entc = crmservices.RetrieveMultiple(new FetchExpression(fetchXml));
            return entc;
        }
        private EntityCollection CalSum_FeeRemaining(IOrganizationService crmservices, EntityReference oe)
        {
            string fetchXml =
                  @"<fetch aggregate='true' >
                  <entity name='bsd_paymentschemedetail' >
                    <attribute name='bsd_maintenancefeeremaining' alias='MainFeeReAmt' aggregate='sum' />
                    <attribute name='bsd_managementfeeremaining' alias='ManaFeeReAmt' aggregate='sum' />
                    <filter type='and'>
                      <condition attribute='bsd_optionentry' operator='eq' value='{0}' />
                    </filter>
                  </entity>
                </fetch>";
            fetchXml = string.Format(fetchXml, oe.Id);
            EntityCollection entc = crmservices.RetrieveMultiple(new FetchExpression(fetchXml));
            return entc;
        }
    }
}

