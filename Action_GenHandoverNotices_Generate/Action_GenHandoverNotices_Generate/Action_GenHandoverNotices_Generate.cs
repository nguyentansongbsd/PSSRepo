using Microsoft.Crm.Sdk.Messages;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Query;
using System;
using System.Collections.Generic;
using System.Text;

namespace Action_GenHandoverNotices_Generate
{
    public class Action_GenHandoverNotices_Generate : IPlugin
    {
        public static IOrganizationService service = null;
        static IOrganizationServiceFactory factory = null;
        ITracingService traceService = null;
        static StringBuilder strMess = new StringBuilder();
        static StringBuilder strMess2 = new StringBuilder();
        void IPlugin.Execute(IServiceProvider serviceProvider)
        {
            IPluginExecutionContext context = (IPluginExecutionContext)serviceProvider.GetService(typeof(IPluginExecutionContext));
            traceService = (ITracingService)serviceProvider.GetService(typeof(ITracingService));
            factory = (IOrganizationServiceFactory)serviceProvider.GetService(typeof(IOrganizationServiceFactory));
            service = factory.CreateOrganizationService(context.UserId);
            string input01 = "";
            if (!string.IsNullOrEmpty((string)context.InputParameters["input01"]))
            {
                input01 = context.InputParameters["input01"].ToString();
            }
            string input02 = "";
            if (!string.IsNullOrEmpty((string)context.InputParameters["input02"]))
            {
                input02 = context.InputParameters["input02"].ToString();
            }
            string input03 = "";
            if (!string.IsNullOrEmpty((string)context.InputParameters["input03"]))
            {
                input03 = context.InputParameters["input03"].ToString();
            }
            string input04 = "";
            if (!string.IsNullOrEmpty((string)context.InputParameters["input04"]))
            {
                input04 = context.InputParameters["input04"].ToString();
            }
            if (input01 == "Bước 01" && input02 != "")
            {
                traceService.Trace("Bước 01");
                Entity enUp = new Entity("bsd_generatehandovernotices");
                enUp.Id = Guid.Parse(input02);
                enUp["bsd_powerautomate"] = true;
                service.Update(enUp);
                context.OutputParameters["output01"] = context.UserId.ToString();
                string url = "";
                EntityCollection configGolive = RetrieveMultiRecord(service, "bsd_configgolive",
                    new ColumnSet(new string[] { "bsd_url" }), "bsd_name", "GenHandoverNotices Generate");
                foreach (Entity item in configGolive.Entities)
                {
                    if (item.Contains("bsd_url")) url = (string)item["bsd_url"];
                }
                if (url == "") throw new InvalidPluginExecutionException("Link to run PA not found. Please check again.");
                context.OutputParameters["output02"] = url;

                Entity enTarget = service.Retrieve(enUp.LogicalName, enUp.Id, new ColumnSet(true));

                //LAY DANH SACH CAC UEHD DETAIL HOP LE
                QueryExpression query = new QueryExpression("bsd_updateestimatehandoverdatedetail");
                query.ColumnSet = new ColumnSet(new string[3] { "bsd_updateestimatehandoverdatedetailid", "bsd_optionentry", "bsd_installment" });
                query.Criteria = new FilterExpression(LogicalOperator.And);
                query.Criteria.AddCondition(new ConditionExpression("statuscode", ConditionOperator.Equal, "100000000"));// == APPROVED
                query.Criteria.AddCondition(new ConditionExpression("statecode", ConditionOperator.Equal, 0));// == Active
                query.Criteria.AddCondition(new ConditionExpression("bsd_optionentry", ConditionOperator.NotNull));// == bsd_optionentry
                query.Criteria.AddCondition(new ConditionExpression("bsd_installment", ConditionOperator.NotNull));// == bsd_installment
                query.LinkEntities.Add(new LinkEntity("bsd_updateestimatehandoverdatedetail", "bsd_updateestimatehandoverdate", "bsd_updateestimatehandoverdate", "bsd_updateestimatehandoverdateid", JoinOperator.Inner));
                query.LinkEntities[0].LinkCriteria = new FilterExpression(LogicalOperator.And);
                var bsd_types_1 = 100000002;
                var bsd_types_2 = 100000001;
                query.LinkEntities[0].LinkCriteria.AddCondition(new ConditionExpression("bsd_types", ConditionOperator.In, bsd_types_1, bsd_types_2)); // Update Only for Installment or Update All
                query.LinkEntities[0].LinkCriteria.AddCondition(new ConditionExpression("bsd_usegeneratehandovernotice", ConditionOperator.Equal, "1"));//Use Generate Handover Notice = Yes
                query.LinkEntities[0].LinkCriteria.AddCondition(new ConditionExpression("bsd_generated", ConditionOperator.Equal, "0"));// == NO
                if (enTarget.Contains("bsd_project"))
                {
                    query.LinkEntities[0].LinkCriteria.AddCondition(new ConditionExpression("bsd_project", ConditionOperator.Equal, ((EntityReference)enTarget["bsd_project"]).Id));
                }
                if (enTarget.Contains("bsd_estimatehandoverdate"))
                {
                    query.LinkEntities[0].LinkCriteria.AddCondition(new ConditionExpression("bsd_updateestimatehandoverdateid", ConditionOperator.Equal, ((EntityReference)enTarget["bsd_estimatehandoverdate"]).Id));
                }
                query.LinkEntities.Add(new LinkEntity("bsd_updateestimatehandoverdatedetail", "salesorder", "bsd_optionentry", "salesorderid", JoinOperator.Inner));
                query.LinkEntities[1].LinkCriteria = new FilterExpression(LogicalOperator.And);
                query.LinkEntities[1].LinkCriteria.AddCondition(new ConditionExpression("bsd_signedcontractdate", ConditionOperator.NotNull));// Not NULL
                query.LinkEntities.Add(new LinkEntity("bsd_updateestimatehandoverdatedetail", "product", "bsd_units", "productid", JoinOperator.Inner));
                query.LinkEntities[2].LinkCriteria = new FilterExpression(LogicalOperator.And);
                query.LinkEntities[2].LinkCriteria.AddCondition(new ConditionExpression("statuscode", ConditionOperator.Equal, "100000002"));// == Sold
                EntityCollection list = service.RetrieveMultiple(query);
                List<string> listUnit = new List<string>();
                foreach (Entity detail in list.Entities)
                {
                    if (!CheckExistHandoverNotices(service, (EntityReference)detail["bsd_optionentry"], (EntityReference)detail["bsd_installment"]))
                        listUnit.Add(detail.Id.ToString());
                }
                if (listUnit.Count == 0)
                    throw new InvalidPluginExecutionException("The list is empty. Please check again.");
                context.OutputParameters["output03"] = string.Join(";", listUnit);
            }
            else if (input01 == "Bước 02" && input02 != "" && input03 != "" && input04 != "")
            {
                traceService.Trace("Bước 02");
                service = factory.CreateOrganizationService(Guid.Parse(input04));
                Entity enTarget = service.Retrieve("bsd_generatehandovernotices", Guid.Parse(input02), new ColumnSet(true));
                Entity detail = service.Retrieve("bsd_updateestimatehandoverdatedetail", Guid.Parse(input03), new ColumnSet(true));
                DateTime today = RetrieveLocalTimeFromUTCTime(DateTime.Now, service);
                Entity UpEHDEn = service.Retrieve(((EntityReference)detail["bsd_updateestimatehandoverdate"]).LogicalName,
                        ((EntityReference)detail["bsd_updateestimatehandoverdate"]).Id,
                            new ColumnSet(true));
                DateTime UpEHD_SimuDate = today;
                if (UpEHDEn.Contains("bsd_simulationdate"))
                    UpEHD_SimuDate = RetrieveLocalTimeFromUTCTime((DateTime)UpEHDEn["bsd_simulationdate"], service);
                bool bsd_isincludelastinstallment = UpEHDEn.Contains("bsd_isincludelastinstallment") ? (bool)UpEHDEn["bsd_isincludelastinstallment"] : false;
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
                Entity hn = new Entity("bsd_handovernotice");
                hn["bsd_name"] = "Handover Notices of " + ((EntityReference)detail["bsd_optionentry"]).Name;
                Entity OE = service.Retrieve(((EntityReference)detail["bsd_optionentry"]).LogicalName, ((EntityReference)detail["bsd_optionentry"]).Id,
                    new ColumnSet(new string[] { "name", "bsd_paymentscheme", "bsd_totalpercent", "customerid", "bsd_estimatehandoverdatecontract", "bsd_freightamount", "bsd_managementfee", "bsd_depositamount", "bsd_project", "bsd_numberofmonthspaidmf", "bsd_signedcontractdate", "bsd_signeddadate" }));
                if (OE.Contains("customerid"))
                {
                    hn["bsd_customer"] = OE["customerid"];

                    EntityCollection advance = CalculateAdvancePayment(service, (EntityReference)OE["customerid"], (EntityReference)OE["bsd_project"], OE.ToEntityReference());
                    foreach (Entity e in advance.Entities)
                    {
                        advancePaymentAmount = (e.Contains("remaining") && ((AliasedValue)e["remaining"]).Value != null) ? ((Money)((AliasedValue)e["remaining"]).Value).Value : decimal.Zero;
                    }
                }
                traceService.Trace("333333");
                if (OE.Contains("bsd_estimatehandoverdatecontract"))
                    hn["bsd_handoverdatebaseoncontract"] = OE["bsd_estimatehandoverdatecontract"];

                if (OE.Contains("bsd_project"))
                    hn["bsd_project"] = OE["bsd_project"];
                hn["bsd_totalamountpaid"] = OE.Contains("bsd_totalpercent") ? OE["bsd_totalpercent"] : decimal.Zero;
                hn["bsd_optionentry"] = OE.ToEntityReference();
                hn["bsd_generatehandovernotices"] = enTarget.ToEntityReference();
                if (OE.Contains("bsd_freightamount")) maintenanceF = ((Money)OE["bsd_freightamount"]).Value;
                if (OE.Contains("bsd_managementfee")) managementF = ((Money)OE["bsd_managementfee"]).Value;
                traceService.Trace("444444");
                //Han_15082018 : Get Remaining Amount of Mana Fee + Main Fee
                EntityCollection InstallFee = CalSum_FeeRemaining(service, OE.ToEntityReference());

                foreach (Entity e in InstallFee.Entities)
                {
                    if (e.Contains("MainFeeReAmt") && ((AliasedValue)e["MainFeeReAmt"]).Value != null)
                        maintenanceF = ((Money)((AliasedValue)e.Attributes["MainFeeReAmt"]).Value).Value;

                    if (e.Contains("MainFeeReAmt") && ((AliasedValue)e["ManaFeeReAmt"]).Value != null)
                        managementF = ((Money)((AliasedValue)e.Attributes["ManaFeeReAmt"]).Value).Value;
                }
                traceService.Trace("555555");
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
                traceService.Trace("6666666666");
                // Total System Receipt
                EntityCollection SysReceiptEn = CalSum_SystemReceipt(service, OE.ToEntityReference());

                foreach (Entity e in SysReceiptEn.Entities)
                {
                    if (e.Contains("TotalSysRe") && ((AliasedValue)e["TotalSysRe"]).Value != null)
                        TotalSysRe = ((Money)((AliasedValue)e.Attributes["TotalSysRe"]).Value).Value;
                }
                traceService.Trace("77777777777");
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
                traceService.Trace("88888888888");
                EntityCollection calculateOther = CalculateOther(service, OE.ToEntityReference(), true);
                foreach (Entity e in calculateOther.Entities)
                {
                    orther = (e.Contains("sumMis") && ((AliasedValue)e["sumMis"]).Value != null) ? ((Money)((AliasedValue)e.Attributes["sumMis"]).Value).Value : decimal.Zero;
                }
                //estimateInterest = Interest(service, OE, today.Date);
                estimateInterest = Interest(service, OE, UpEHD_SimuDate.Date);
                traceService.Trace("estimateInterest :" + estimateInterest);
                hn["bsd_installment"] = detail["bsd_installment"];
                Entity ins = service.Retrieve(((EntityReference)detail["bsd_installment"]).LogicalName, ((EntityReference)detail["bsd_installment"]).Id,
                    new ColumnSet(new string[2] { "bsd_amountofthisphase", "bsd_balance" }));
                installmentAmount = ins.Contains("bsd_balance") ? ((Money)ins["bsd_balance"]).Value : decimal.Zero;
                hn["bsd_installmentamount"] = new Money(installmentAmount);
                hn["bsd_advancepaymentamount"] = new Money(advancePaymentAmount);
                hn["bsd_outstandingincludeinterest"] = new Money(outstandingUnPaid);
                hn["bsd_actualinterest"] = new Money(actualInterest);

                hn["bsd_estimateintesrest"] = new Money(estimateInterest); // HN = estimateInterest = 0
                traceService.Trace("estimateInterest :" + estimateInterest);
                hn["bsd_totalinterestamount"] = new Money(actualInterest + estimateInterest);
                hn["bsd_subject"] = "Handover Notices";
                hn["bsd_issuedate"] = today.Date;
                if (detail.Contains("bsd_units"))
                    hn["bsd_units"] = detail["bsd_units"];
                if (detail.Contains("bsd_paymentduedate"))
                    hn["bsd_paymentduedate"] = detail["bsd_paymentduedate"];
                if (detail.Contains("bsd_estimatehandoverdatenew"))
                    hn["bsd_handoverdateactual"] = detail["bsd_estimatehandoverdatenew"];
                hn["bsd_other"] = new Money(orther);
                decimal total = managementF + maintenanceF + outstandingUnPaid + orther + actualInterest + estimateInterest + installmentAmount - advancePaymentAmount;
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
                if (enTarget.Contains("bsd_date"))
                    hn["bsd_billdate"] = RetrieveLocalTimeFromUTCTime((DateTime)enTarget["bsd_date"], service);
                service.Create(hn);
                //UPDATE UEHD
                Entity uehd = new Entity("bsd_updateestimatehandoverdate");
                uehd.Id = ((EntityReference)detail["bsd_updateestimatehandoverdate"]).Id;
                uehd["bsd_generated"] = true;
                service.Update(uehd);
            }
            else if (input01 == "Bước 03" && input02 != "" && input04 != "")
            {
                traceService.Trace("Bước 03");
                service = factory.CreateOrganizationService(Guid.Parse(input04));
                Entity enUp = new Entity("bsd_generatehandovernotices");
                enUp.Id = Guid.Parse(input02);
                enUp["bsd_powerautomate"] = false;
                enUp["statuscode"] = new OptionSetValue(100000000);
                enUp["bsd_generateddate"] = DateTime.Now;
                enUp["bsd_generator"] = new EntityReference("systemuser", Guid.Parse(input04));
                service.Update(enUp);
            }
        }
        EntityCollection RetrieveMultiRecord(IOrganizationService crmservices, string entity, ColumnSet column, string condition, object value)
        {
            QueryExpression q = new QueryExpression(entity);
            q.ColumnSet = column;
            q.Criteria = new FilterExpression();
            q.Criteria.AddCondition(new ConditionExpression(condition, ConditionOperator.Equal, value));
            q.Criteria.AddCondition(new ConditionExpression("statecode", ConditionOperator.Equal, 0));
            EntityCollection entc = service.RetrieveMultiple(q);

            return entc;
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
            fetchXml = string.Format(fetchXml, oe.Id, ins.Id);
            EntityCollection entc = crmservices.RetrieveMultiple(new FetchExpression(fetchXml));
            return (entc.Entities.Count > 0 ? true : false);
        }
        private decimal Interest(IOrganizationService crmservices, Entity oe, DateTime dateCalculate)
        {
            decimal interest = 0;
            //GET INSTALLMENT
            QueryExpression q1 = new QueryExpression("bsd_paymentschemedetail");
            q1.ColumnSet = new ColumnSet(true);
            q1.Criteria = new FilterExpression(LogicalOperator.And);
            q1.Criteria.AddCondition(new ConditionExpression("bsd_optionentry", ConditionOperator.Equal, oe.Id));
            q1.Criteria.AddCondition(new ConditionExpression("statuscode", ConditionOperator.Equal, "100000000"));//NOT PAID
            q1.Criteria.AddCondition(new ConditionExpression("bsd_duedate", ConditionOperator.NotNull));
            //q1.Criteria.AddCondition(new ConditionExpression("bsd_duedate", ConditionOperator.OnOrBefore, dateCalculate));//Duedate <= simulation date
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
                //int Graceday = ((int)interestMaster["bsd_gracedays"]);
                int interestStartDateType = ((OptionSetValue)interestMaster["bsd_intereststartdatetype"]).Value;//100000000: Due date;         100000001: Grace period  
                //decimal interestMasterPercent = (decimal)interestMaster["bsd_termsinterestpercentage"];
                //decimal interestProjectDaily = 0;
                //interestProjectDaily = DailyInterest(service, oe.ToEntityReference());
                //decimal interestPercent = interestMasterPercent + interestProjectDaily;
                //var bsd_signedcontractdate = new DateTime();
                //var bsd_signeddadate = new DateTime();
                //CREATE INTEREST SIMULATION DETAIL
                foreach (Entity ins in listInstallment.Entities)
                {
                    int lateDays = 0;
                    decimal balance = 0;
                    decimal interestPercent = ins.Contains("bsd_interestchargeper") ? (decimal)ins["bsd_interestchargeper"] : 0;
                    int Graceday = ins.Contains("bsd_gracedays") ? ((int)ins["bsd_gracedays"]) : 0;
                    int bsd_ordernumber = ins.Contains("bsd_ordernumber") ? (int)ins["bsd_ordernumber"] : 0;
                    //TINH LAI
                    DateTime duedate = RetrieveLocalTimeFromUTCTime((DateTime)ins["bsd_duedate"], service);
                    //DateTime InterestStarDate = duedate.AddDays(Graceday);

                    TimeSpan difference = dateCalculate - duedate;
                    int bsd_latedays = difference.Days - Graceday;
                    bsd_latedays = bsd_latedays < 0 ? 0 : bsd_latedays;
                    int orderNumberSightContract = getViTriDotSightContract(oe.Id);
                    int numberOfDays2 = 0;
                    if (orderNumberSightContract != -1)
                    {
                        if (orderNumberSightContract <= bsd_ordernumber && oe.Contains("bsd_signedcontractdate"))
                        {
                            numberOfDays2 = -100599;
                        }
                        else if (orderNumberSightContract > bsd_ordernumber)
                        {
                            if (oe.Contains("bsd_signeddadate"))
                            {
                                DateTime bsd_signeddadate = RetrieveLocalTimeFromUTCTime((DateTime)oe["bsd_signeddadate"], service);
                                TimeSpan difference2 = dateCalculate - bsd_signeddadate;
                                numberOfDays2 = difference2.Days;
                                numberOfDays2 = numberOfDays2 < 0 ? 0 : numberOfDays2;
                                traceService.Trace("bsd_signeddadate " + bsd_signeddadate);
                            }
                        }
                        else numberOfDays2 = -100599;
                    }
                    if (numberOfDays2 != -100599 && numberOfDays2 < bsd_latedays) bsd_latedays = numberOfDays2;
                    traceService.Trace("bsd_latedays " + bsd_latedays);
                    lateDays = bsd_latedays;
                    balance = ins.Contains("bsd_balance") ? ((Money)ins["bsd_balance"]).Value : decimal.Zero;
                    decimal Newinterest = CalculateNewInterest(balance, lateDays, interestPercent);
                    interest = interest + Newinterest;
                    traceService.Trace("interest: " + interest.ToString());
                }
            }
            return interest;
        }
        private int getViTriDotSightContract(Guid idOE)
        {
            int location = -1;
            var fetchXml_instalment = $@"<?xml version=""1.0"" encoding=""utf-16""?>
                                        <fetch>
                                          <entity name=""bsd_paymentschemedetail"">
                                            <attribute name=""bsd_ordernumber"" />
                                            <filter>
                                              <condition attribute=""bsd_optionentry"" operator=""eq"" value=""{idOE}"" />
                                              <condition attribute=""bsd_signcontractinstallment"" operator=""eq"" value=""1"" />
                                              <condition attribute=""statecode"" operator=""eq"" value=""0"" />
                                            </filter>
                                          </entity>
                                        </fetch>";
            EntityCollection rs = service.RetrieveMultiple(new FetchExpression(fetchXml_instalment));
            foreach (Entity entity in rs.Entities)
            {
                location = entity.Contains("bsd_ordernumber") ? (int)entity["bsd_ordernumber"] : 0;
            }
            return location;
        }
        private decimal CalculateNewInterest(decimal balance, int lateDays, decimal interestPercent)
        {
            decimal interest = balance * lateDays * (interestPercent / 100);
            return interest;
        }
        private EntityCollection CalculateOutstanding(IOrganizationService crmservices, EntityReference oe, Boolean isincludelastinstallment)
        {
            string fetchXml = "";
            if (isincludelastinstallment == false)
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
        private EntityCollection CalculateOther(IOrganizationService crmservices, EntityReference oe, Boolean isincludelastinstallment)
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
