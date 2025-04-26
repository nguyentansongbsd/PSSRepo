using System;
using Microsoft.Xrm.Sdk;
using Microsoft.Crm.Sdk.Messages;
using System.Linq;
using System.Text;
using Microsoft.Xrm.Sdk.Query;
namespace Action_InterestSimulation_CalculateSimulation
{
    public class Action_InterestSimulation_Calculate : IPlugin
    {
        public static IOrganizationService service = null;
        static IOrganizationServiceFactory factory = null;
        static Entity target = null;
        static StringBuilder strMess = new StringBuilder();
        static IServiceProvider serviceProvider1;
        static int intCount = 0;
        private static Entity enInstallment;
        public static Installment objIns = new Installment();

        void IPlugin.Execute(IServiceProvider serviceProvider)
        {
            serviceProvider1 = serviceProvider;
            IPluginExecutionContext context = (IPluginExecutionContext)serviceProvider.GetService(typeof(IPluginExecutionContext));
            factory = (IOrganizationServiceFactory)serviceProvider.GetService(typeof(IOrganizationServiceFactory));
            service = factory.CreateOrganizationService(context.UserId);

            try
            {
                #region --- Bodye Code ---

                string interestsimulationid = context.InputParameters["interestsimulationid"].ToString();
                strMess.AppendLine("interestsimulationid: " + interestsimulationid);
                string callfrom = context.InputParameters["callfrom"].ToString();
                strMess.AppendLine("callfrom: " + callfrom);
                string optinentryid = context.InputParameters["optinentryid"].ToString();
                strMess.AppendLine("optinentryid: " + optinentryid);
                string aginginterestsimulationoptionid = context.InputParameters.Contains("aginginterestsimulationoptionid") ? context.InputParameters["aginginterestsimulationoptionid"].ToString() : "";
                strMess.AppendLine("aginginterestsimulationoptionid: " + aginginterestsimulationoptionid);
                string strsimulationdate = context.InputParameters["simulationdate"].ToString();
                strMess.AppendLine("strsimulationdate: " + strsimulationdate);
                string strdateofinterestcalculation = context.InputParameters["dateofinterestcalculation"].ToString();
                Main(interestsimulationid, callfrom, optinentryid, aginginterestsimulationoptionid, strsimulationdate, strdateofinterestcalculation);
            }
            catch (Exception ex)
            {
                strMess.AppendLine(ex.ToString());
                throw new InvalidPluginExecutionException(strMess.ToString());
            }
        }
        public static void Main(string interestsimulationid, string callfrom, string optinentryid, string aginginterestsimulationoptionid, string strsimulationdate, string strdateofinterestcalculation)
        {
            EntityReference enRef = interestsimulationid != "" ? new EntityReference("bsd_interestsimulation", new Guid(interestsimulationid)) : null;
            if (strdateofinterestcalculation == null || strdateofinterestcalculation == "")
            {
                strdateofinterestcalculation = strsimulationdate;
            }
            DateTime simulationDate = new DateTime();
            DateTime dateofinterestcalculation = new DateTime();
            QueryExpression q1 = new QueryExpression("bsd_paymentschemedetail");
            EntityCollection listInstallment = null;
            strMess.AppendLine("1");
            if (strsimulationdate != "")
            {
                strMess.AppendLine("strsimulationdate: " + strsimulationdate);
                string[] arrstring = strsimulationdate.Split('-');
                simulationDate = new DateTime(Convert.ToInt16(arrstring[0]), Convert.ToInt16(arrstring[1]) + 1, Convert.ToInt16(arrstring[2]));
                simulationDate = RetrieveLocalTimeFromUTCTime(simulationDate);
            }
            if (strdateofinterestcalculation != "")
            {
                strMess.AppendLine("strdateofinterestcalculation: " + strdateofinterestcalculation);
                string[] arrstring = strdateofinterestcalculation.Split('-');
                dateofinterestcalculation = new DateTime(Convert.ToInt16(arrstring[0]), Convert.ToInt16(arrstring[1]) + 1, Convert.ToInt16(arrstring[2]));
                dateofinterestcalculation = RetrieveLocalTimeFromUTCTime(dateofinterestcalculation);
            }
            Entity enInterestSimulation = null;
            Entity simulationoptions = null;
            if (aginginterestsimulationoptionid != "" && callfrom != "InterestSimulation")
            {
                simulationoptions = service.Retrieve("bsd_aginginterestsimulationoption", new Guid(aginginterestsimulationoptionid), new ColumnSet(true));
            }


            switch (callfrom)
            {
                case "":

                    if (enRef.LogicalName == "bsd_interestsimulation")
                    {

                        target = service.Retrieve(enRef.LogicalName, enRef.Id, new ColumnSet(new string[] { "bsd_project", "bsd_block", "bsd_floor", "bsd_simulationdate", "bsd_dateofinterestcalculation", "bsd_bankinterestrate", "bsd_name", "bsd_type", "bsd_simulationdate" }));
                        if (!target.Contains("bsd_project"))
                            throw new InvalidPluginExecutionException("Please input Project.");
                        if (!target.Contains("bsd_simulationdate"))
                            throw new InvalidPluginExecutionException("Please input Simulation date.");
                        if (!target.Contains("bsd_type"))
                            throw new InvalidPluginExecutionException("Please select the type of report before calculating the report.");


                        //if (!target.Contains("bsd_dateofinterestcalculation"))
                        //    throw new InvalidPluginExecutionException("Please input Date of interest calculation.");
                        //if (!target.Contains("bsd_bankinterestrate"))
                        //    throw new InvalidPluginExecutionException("Please input Bank interest rate.");

                        //DELETE RECORDS OLD
                        QueryExpression qued = new QueryExpression("bsd_interestsimulationdetail");
                        qued.ColumnSet = new ColumnSet(new string[1] { "bsd_interestsimulationdetailid" });
                        qued.Criteria = new FilterExpression(LogicalOperator.And);
                        qued.Criteria.AddCondition(new ConditionExpression("bsd_interestsimulation", ConditionOperator.Equal, enRef.Id));

                        foreach (Entity isd in service.RetrieveMultiple(qued).Entities)
                            service.Delete("bsd_interestsimulationdetail", (Guid)isd["bsd_interestsimulationdetailid"]);
                        //GET UNITS SELECTED
                        QueryExpression queryExpression1 = new QueryExpression("bsd_bsd_interestsimulation_product");
                        queryExpression1.ColumnSet = new ColumnSet(new string[] { "productid" });
                        queryExpression1.Criteria = new FilterExpression(LogicalOperator.And);
                        queryExpression1.Criteria.AddCondition(new ConditionExpression("bsd_interestsimulationid", ConditionOperator.Equal, enRef.Id));
                        EntityCollection listUnits = service.RetrieveMultiple(queryExpression1);
                        if (listUnits.Entities.Count == 0)
                            throw new InvalidPluginExecutionException("Unit List can not empty when doing this action. Please Generate Unit!");
                        //Kien tra AgingInterestSimulationOption có dũ liệu chưa nếu chua thì tao với listunit tuong ung
                        createSimulationOptionByListUnit(enRef, listUnits);

                        #region -- Code Tin
                        simulationDate = RetrieveLocalTimeFromUTCTime((DateTime)target["bsd_simulationdate"]);
                        foreach (Entity unit in listUnits.Entities)
                        {
                            Entity UnitsEn = service.Retrieve("product", (Guid)unit["productid"], new ColumnSet(new string[1] { "name" }));



                            //DateTime simulationDate = target.Contains("bsd_simulationdate") ? (DateTime) target["bsd_simulationdate"] : null;
                            int SimuType = ((OptionSetValue)target["bsd_type"]).Value;
                            enInterestSimulation = service.Retrieve(target.LogicalName, target.Id, new ColumnSet(true));
                            EntityCollection listOE = GetOptionList(UnitsEn.Id.ToString());

                            if (listOE.Entities.Count > 0)
                            {
                                Entity oe = listOE.Entities[0];

                                q1.ColumnSet = new ColumnSet(true);
                                q1.Criteria = new FilterExpression(LogicalOperator.And);
                                q1.Criteria.AddCondition(new ConditionExpression("bsd_optionentry", ConditionOperator.Equal, oe.Id));
                                q1.Criteria.AddCondition(new ConditionExpression("statecode", ConditionOperator.Equal, 0));
                                //q1.Criteria.AddCondition(new ConditionExpression("bsd_duedate", ConditionOperator.OnOrBefore, simulationDate));//Duedate <= simulation date
                                listInstallment = service.RetrieveMultiple(q1);
                                simulationoptions = getSimulationOption(enRef.Id.ToString(), oe.ToEntityReference().Id.ToString());
                                strMess.AppendLine("simulationoptions: " + simulationoptions.Id.ToString());
                                if (listInstallment.Entities.Count > 0)
                                {
                                    DateTime dateCalculate = target.Contains("bsd_dateofinterestcalculation") ? RetrieveLocalTimeFromUTCTime((DateTime)target["bsd_dateofinterestcalculation"]) : simulationDate;
                                    decimal interestProjectDaily = 0;
                                    if (target.Contains("bsd_bankinterestrate"))
                                    {
                                        Entity dailyinterest = service.Retrieve(((EntityReference)target["bsd_bankinterestrate"]).LogicalName, ((EntityReference)target["bsd_bankinterestrate"]).Id,
                                        new ColumnSet(new string[]
                                        {
                                    "bsd_interestrate"
                                        }));
                                        interestProjectDaily = dailyinterest.Contains("bsd_interestrate") ? (decimal)dailyinterest["bsd_interestrate"] : decimal.Zero;
                                    }

                                    #region CREATE INTEREST SIMULATION DETAIL
                                    foreach (Entity enIntallment in listInstallment.Entities)
                                    {
                                        //Cập nhật thêm trường thông tin Aging/ Interest Simulation Option khi tạo Aging/ Interest Simulation Detail"

                                        if (simulationoptions != null)
                                        {
                                            createAgingInterestSimulationDetail(oe, UnitsEn, enInterestSimulation, enIntallment, simulationoptions, simulationDate, dateCalculate, interestProjectDaily, SimuType);
                                        }
                                    }
                                    #endregion
                                }
                                strMess.AppendLine("aaaaaaaaaaaaaaaaaaaaaaaa");
                                strMess.AppendLine("updateAdvantPayment:" + oe.LogicalName);
                                strMess.AppendLine("updateAdvantPayment:" + oe["name"].ToString());
                                strMess.AppendLine("simulationoptions: " + simulationoptions.Id.ToString());
                                updateNewInterestAmount(oe, simulationoptions, SimuType);
                                updateAdvantPayment(oe, simulationoptions.Id.ToString());

                                strMess.AppendLine("done");

                            }

                        }
                        #endregion
                    }

                    break;
                case "DelInterestSimulation":
                    #region DelInterestSimulation
                    strMess.AppendLine("DELETE RECORDS OLD");
                    strMess.AppendLine("enRef.Id: " + enRef.Id.ToString());
                    QueryExpression que1 = new QueryExpression("bsd_interestsimulationdetail");
                    que1.ColumnSet = new ColumnSet(new string[1] { "bsd_interestsimulationdetailid" });
                    que1.Criteria = new FilterExpression(LogicalOperator.And);
                    que1.Criteria.AddCondition(new ConditionExpression("bsd_interestsimulation", ConditionOperator.Equal, enRef.Id));
                    que1.TopCount = 500;

                    foreach (Entity isd in service.RetrieveMultiple(que1).Entities)
                    {
                        service.Delete("bsd_interestsimulationdetail", (Guid)isd["bsd_interestsimulationdetailid"]);
                    }
                    /*var userRequest = new WhoAmIRequest();
                    var userResponse = (WhoAmIResponse)service.Execute(userRequest);
                    Guid currentUserId = userResponse.UserId;

                    var deleteCondition = new ConditionExpression(
                    "bsd_interestsimulation", ConditionOperator.Equal, enRef.Id);

                    // Create a fiter expression for the bulk delete request.
                    var deleteFilter = new FilterExpression(LogicalOperator.And);
                    deleteFilter.Conditions.Add(deleteCondition);
                    var bulkDeleteQuery = new QueryExpression
                    {
                        EntityName = "bsd_interestsimulationdetail",
                        Distinct = false,
                        Criteria = deleteFilter
                    };
                    // Create the bulk delete request.
                    var bulkDeleteRequest = new BulkDeleteRequest
                    {
                        JobName = "Sample Bulk Delete",
                        QuerySet = new[] { bulkDeleteQuery },
                        StartDateTime = DateTime.Now,
                        ToRecipients = new[] { currentUserId },
                        CCRecipients = new Guid[] { },
                        SendEmailNotification = false,
                        RecurrencePattern = String.Empty

                    };

                    var bulkDeleteResponse = (BulkDeleteResponse)service.Execute(bulkDeleteRequest);*/
                    #endregion
                    break;
                case "OptionEntrySOA":
                    #region OptionEntrySOA
                    strMess.AppendLine("2");
                    Entity enOptionEntry = service.Retrieve("salesorder", new Guid(optinentryid), new ColumnSet(true));
                    int bsd_type = 100000002;
                    if (callfrom == "InterestSimulation")
                    {
                        bsd_type = 100000001;
                    }
                    strMess.AppendLine("3");

                    if (callfrom == "InterestSimulation")
                    {

                        enInterestSimulation = service.Retrieve(enRef.LogicalName, enRef.Id, new ColumnSet(true));
                        strMess.AppendLine("enInterestSimulation ID:" + enInterestSimulation.Id.ToString());
                        bsd_type = ((OptionSetValue)enInterestSimulation["bsd_type"]).Value;
                    }
                    strMess.AppendLine("4");

                    //DELETE RECORDS OLD
                    QueryExpression que11 = new QueryExpression("bsd_interestsimulationdetail");
                    que11.ColumnSet = new ColumnSet(new string[1] { "bsd_interestsimulationdetailid" });
                    que11.Criteria = new FilterExpression(LogicalOperator.And);
                    que11.Criteria.AddCondition(new ConditionExpression("bsd_optionentry", ConditionOperator.Equal, enOptionEntry.Id));
                    que11.Criteria.AddCondition(new ConditionExpression("bsd_type", ConditionOperator.Equal, bsd_type));
                    foreach (Entity isd in service.RetrieveMultiple(que11).Entities)
                        service.Delete("bsd_interestsimulationdetail", (Guid)isd["bsd_interestsimulationdetailid"]);
                    strMess.AppendLine("5");
                    if (callfrom == "OptionEntrySOA" || callfrom == "InterestSimulation")
                    {
                        q1.ColumnSet = new ColumnSet(true);
                        //q1.Criteria = new FilterExpression(LogicalOperator.And);
                        //q1.Criteria.AddCondition(new ConditionExpression("bsd_optionentry", ConditionOperator.Equal, enOptionEntry.Id));
                        switch (bsd_type)
                        {
                            case 100000000://Aging Report
                                           // Thêm điều kiện Main
                                FilterExpression filter_Main = new FilterExpression(LogicalOperator.Or);

                                FilterExpression filter_notpaid = new FilterExpression(LogicalOperator.And);
                                filter_notpaid.AddCondition(new ConditionExpression("bsd_duedate", ConditionOperator.NotNull));
                                filter_notpaid.AddCondition(new ConditionExpression("bsd_duedate", ConditionOperator.OnOrBefore, dateofinterestcalculation));
                                filter_notpaid.AddCondition(new ConditionExpression("statecode", ConditionOperator.Equal, 0));
                                filter_notpaid.AddCondition(new ConditionExpression("statuscode", ConditionOperator.Equal, 100000000));
                                filter_notpaid.AddCondition(new ConditionExpression("bsd_optionentry", ConditionOperator.Equal, enOptionEntry.Id));
                                filter_Main.AddFilter(filter_notpaid);

                                FilterExpression filter_paid = new FilterExpression(LogicalOperator.And);
                                filter_paid.AddCondition(new ConditionExpression("bsd_duedate", ConditionOperator.NotNull));
                                filter_paid.AddCondition(new ConditionExpression("bsd_duedate", ConditionOperator.OnOrBefore, dateofinterestcalculation));
                                filter_paid.AddCondition(new ConditionExpression("statecode", ConditionOperator.Equal, 0));
                                filter_paid.AddCondition(new ConditionExpression("statuscode", ConditionOperator.Equal, 100000001));
                                filter_paid.AddCondition(new ConditionExpression("bsd_optionentry", ConditionOperator.Equal, enOptionEntry.Id));

                                FilterExpression filter_interestcharge_notPaid = new FilterExpression(LogicalOperator.And);
                                filter_interestcharge_notPaid.AddCondition(new ConditionExpression("bsd_interestchargestatus", ConditionOperator.Equal, 100000000));
                                filter_interestcharge_notPaid.AddCondition(new ConditionExpression("bsd_interestchargeamount", ConditionOperator.GreaterThan, 0));

                                filter_paid.AddFilter(filter_interestcharge_notPaid);

                                filter_Main.AddFilter(filter_notpaid);
                                filter_Main.AddFilter(filter_paid);
                                q1.Criteria = filter_Main;

                                strMess.AppendLine("ID OE:" + enOptionEntry.Id);
                                strMess.AppendLine(string.Format("#Add filter_Main: "));
                                break;
                            case 100000001://Interest Simulation
                                q1.Criteria = new FilterExpression(LogicalOperator.And);
                                q1.Criteria.AddCondition(new ConditionExpression("bsd_optionentry", ConditionOperator.Equal, enOptionEntry.Id));
                                q1.Criteria.AddCondition(new ConditionExpression("statecode", ConditionOperator.Equal, 0));
                                break;
                            default:
                                q1.Criteria = new FilterExpression(LogicalOperator.And);
                                q1.Criteria.AddCondition(new ConditionExpression("bsd_optionentry", ConditionOperator.Equal, enOptionEntry.Id));
                                q1.Criteria.AddCondition(new ConditionExpression("statecode", ConditionOperator.Equal, 0));
                                break;
                        }

                        strMess.AppendLine(string.Format("#Case: {0}", bsd_type));
                        listInstallment = service.RetrieveMultiple(q1);
                        strMess.AppendLine(string.Format("- List result: {0}", listInstallment.Entities.Count));
                        strMess.AppendLine(listInstallment.Entities.Count.ToString());
                    }
                    else
                    {
                        listInstallment = getInstallmentInterest(dateofinterestcalculation, enOptionEntry.Id.ToString());
                    }


                    strMess.AppendLine("6");
                    Entity enUint11 = service.Retrieve(((EntityReference)enOptionEntry["bsd_unitnumber"]).LogicalName, ((EntityReference)enOptionEntry["bsd_unitnumber"]).Id, new ColumnSet(true));

                    if (listInstallment.Entities.Count > 0)
                    {
                        decimal interestProjectDaily = 0;
                        #region CREATE INTEREST SIMULATION DETAIL
                        foreach (Entity ins in listInstallment.Entities)
                        {
                            //Cập nhật thêm trường thông tin Aging/ Interest Simulation Option khi tạo Aging/ Interest Simulation Detail"

                            createAgingInterestSimulationDetail(enOptionEntry, enUint11, enInterestSimulation, ins, simulationoptions, simulationDate, dateofinterestcalculation, interestProjectDaily, bsd_type);

                        }
                        #endregion
                    }
                    strMess.AppendLine("updateAdvantPayment");
                    updateNewInterestAmount(enOptionEntry, null, bsd_type);
                    updateAdvantPayment(enOptionEntry, "");

                    strMess.AppendLine("done");
                    //throw new InvalidPluginExecutionException(strMess.ToString());
                    #endregion
                    break;
            }
            #endregion


        }
        private static EntityCollection getInstallmentInterest(DateTime dateofinterestcalculation, string optionEntryid)
        {
            string xml = @"<fetch version='1.0' output-format='xml-platform' mapping='logical' distinct='false'>
              <entity name='bsd_paymentschemedetail'>
                <attribute name='bsd_paymentschemedetailid' />
                <attribute name='bsd_name' />
                <attribute name='createdon' />
                <attribute name='bsd_optionentry' />
                <attribute name='bsd_duedate' />
                <attribute name='bsd_balance' />
                <attribute name='bsd_amountofthisphase' />
                <attribute name='bsd_amountwaspaid' />
                <attribute name='bsd_interestchargeamount' />
                <attribute name='bsd_interestwaspaid' />
                <attribute name='bsd_waiverinterest' />
                <attribute name='statuscode' />
                <order attribute='bsd_name' descending='false' />
                <filter type='and'>
                  <filter type='or'>
                    <condition attribute='statuscode' operator='eq' value='100000000' />
                    <filter type='and'>
                      <condition attribute='statuscode' operator='eq' value='100000001' />
                      <condition attribute='bsd_interestchargeremaining' operator='gt' value='0' />
                    </filter>
                  </filter>
                  <condition attribute='bsd_duedate' operator='on-or-before' value='" + dateofinterestcalculation.ToString("yyyy-MM-dd") + @"' />
                  <condition attribute='bsd_optionentry' operator='eq' uitype='salesorder' value='" + optionEntryid + @"' />
                  <condition attribute='statecode' operator='eq' uitype='statecode' value='0' />
                </filter>
               </entity> 
            </fetch>";
            strMess.AppendLine(xml);
            EntityCollection encol = service.RetrieveMultiple(new FetchExpression(xml));
            return encol;
        }
        private static void createAgingInterestSimulationDetail(Entity oe, Entity UnitsEn, Entity enInterestSimulation, Entity ins, Entity simulationoptions, DateTime simulationDate, DateTime dateCalculate, decimal interestProjectDaily, int SimuType)
        {
            try
            {
                intCount += 1;
                strMess.AppendLine("++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++ dòng thứ " + intCount);
                if (ins.Contains("bsd_appendixcontract") && ins.Contains("bsd_optionentry") || (!ins.Contains("bsd_appendixcontract") && ins.Contains("bsd_optionentry")))
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
                    var intereststartdatetype = 0;
                    decimal interestMasterPercent = 0;
                    DateTime InterestStarDate = new DateTime();
                    int lateDays = 0;
                    decimal balance = 0;
                    decimal interest_New = 0;
                    decimal interest = 0;
                    decimal decInterestCharge = 0;
                    int bsd_numberofdaysdue;
                    // Số tiền trễ chưa thanh toán
                    decimal interest_NotPaid = 0;
                    interest_NotPaid = CalculateInterestNotPaid(ins);
                    var decInterestAmountIns = CalculateInterestAmount(ins);
                    objIns = new Installment();
                    //Gọi tính trễ

                    Calculate_Interest(ins.Id.ToString(), interest_NotPaid.ToString(), dateCalculate.ToString("MM/dd/yyyy"), ref lateDays, ref interestMasterPercent);
                    intereststartdatetype = objIns.Intereststartdatetype;
                    decInterestCharge = objIns.InterestCharge;
                    InterestStarDate = objIns.InterestStarDate;
                    Entity interestMaster = service.Retrieve(((EntityReference)payScheme["bsd_interestratemaster"]).LogicalName, ((EntityReference)payScheme["bsd_interestratemaster"]).Id,
                        new ColumnSet(new string[]
                        {
                                "bsd_intereststartdatetype",
                                "bsd_gracedays",
                                "bsd_termsinterestpercentage"
                        }));
                    if (!(interestMaster.Contains("bsd_intereststartdatetype") && interestMaster.Contains("bsd_gracedays") && interestMaster.Contains("bsd_termsinterestpercentage")))
                        throw new InvalidPluginExecutionException("Interest charge master not enough infomation required. Please check again!");
                    strMess.AppendLine("createAgingInterestSimulationDetail " + ins.Id.ToString());
                    decimal interestPercent = interestMasterPercent + interestProjectDaily;

                    string UnitsID = UnitsEn.Id.ToString();
                    string UnitsName = UnitsEn.Contains("name") ? UnitsEn["name"].ToString() : "";
                    Entity ISDetail = new Entity("bsd_interestsimulationdetail");
                    if (enInterestSimulation != null)
                    {
                        ISDetail["bsd_name"] = "" + (enInterestSimulation.Contains("bsd_name") ? enInterestSimulation["bsd_name"] : "") + "-" + UnitsName + "-" + (ins.Contains("bsd_name") ? ins["bsd_name"] : "");
                        ISDetail["bsd_interestsimulation"] = enInterestSimulation.ToEntityReference();
                    }
                    else
                    {
                        ISDetail["bsd_name"] = UnitsName + "-" + (ins.Contains("bsd_name") ? ins["bsd_name"] : "");
                        ISDetail["bsd_interestsimulation"] = null;
                    }
                    ISDetail["bsd_optionentry"] = oe.ToEntityReference();
                    ISDetail["bsd_simulationdate"] = simulationDate;
                    ISDetail["bsd_type"] = new OptionSetValue(SimuType);
                    ISDetail["bsd_paymentscheme"] = payScheme.ToEntityReference();
                    ISDetail["bsd_units"] = UnitsEn.ToEntityReference();
                    strMess.AppendLine(ISDetail["bsd_name"].ToString() + "-----" + ins["bsd_name"].ToString());
                    strMess.AppendLine("interest_NotPaid: " + interest_NotPaid.ToString());

                    ISDetail["bsd_aginginterestsimulationoption"] = simulationoptions != null ? simulationoptions.ToEntityReference() : null;
                    ISDetail["bsd_installment"] = ins.ToEntityReference();
                    ISDetail["bsd_installmentamount"] = ins.Contains("bsd_amountofthisphase") ? ins["bsd_amountofthisphase"] : new Money(0);
                    ISDetail["bsd_paidamount"] = ins.Contains("bsd_amountwaspaid") ? ins["bsd_amountwaspaid"] : new Money(0);
                    ISDetail["bsd_outstandingamount"] = ins.Contains("bsd_balance") ? ins["bsd_balance"] : new Money(0);


                    ISDetail["bsd_numberofdaysdue"] = 0;

                    strMess.AppendLine("1");
                    strMess.AppendLine("2");

                    strMess.AppendLine("Info .. ");

                    // Create Detail
                    interest = interest_New + interest_NotPaid;
                    #region -- type = Interest Simulation = 100000001
                    DateTime duedate = new DateTime();
                    decimal bsd_interestchargeamount = 0;
                    strMess.AppendLine("statuscode: " + ((OptionSetValue)ins["statuscode"]).Value.ToString());
                    bool bolCheckPaid = false;
                    bolCheckPaid = (((OptionSetValue)ins["statuscode"]).Value == 100000001 && ins.Contains("bsd_interestchargestatus") && ((OptionSetValue)ins["bsd_interestchargestatus"]).Value == 100000000);
                    if (interest_NotPaid > 0 || ((OptionSetValue)ins["statuscode"]).Value == 100000000 || bolCheckPaid)
                    {
                        strMess.AppendLine("11111111111111111111");
                        if (ins.Contains("bsd_duedate"))
                        {
                            duedate = RetrieveLocalTimeFromUTCTime((DateTime)ins["bsd_duedate"]);
                            ISDetail["bsd_duedate"] = duedate;
                            DateTime InterestStarDate1 = InterestStarDate;
                            ISDetail["bsd_intereststartdate"] = InterestStarDate1;
                            strMess.AppendLine("duedate: " + duedate.ToString());
                            bsd_numberofdaysdue = (int)simulationDate.Date.Subtract(duedate.Date).TotalDays;
                            strMess.AppendLine("simulationDate: " + simulationDate.ToString());
                            strMess.AppendLine("InterestStarDate: " + InterestStarDate1.ToString());
                            strMess.AppendLine("dateCalculate: " + dateCalculate.ToString());
                            if (InterestStarDate1 < dateCalculate)
                            {
                                balance = ins.Contains("bsd_balance") ? ((Money)ins["bsd_balance"]).Value : decimal.Zero;
                                decimal returnValue = CalculateNewInterest(balance, lateDays, interestPercent);
                                interest_New = returnValue < 0 ? 0 : returnValue;
                            }
                            else
                            {
                                lateDays = 0;
                                balance = 0;
                                interest_New = 0;
                            }

                            if (bsd_numberofdaysdue > 0)
                            {
                                ISDetail["bsd_numberofdaysdue"] = bsd_numberofdaysdue;
                            }

                            ISDetail["bsd_outstandingday"] = lateDays;
                            ISDetail["bsd_paymentscheme"] = payScheme.ToEntityReference();
                            ISDetail["bsd_interestpercent"] = interestPercent;
                            ISDetail["bsd_groupaging"] = new OptionSetValue(CheckGroupAging(lateDays));
                            ISDetail["bsd_interestamountinstallment"] = new Money(decInterestAmountIns);
                            decimal decnewinterestamount = bolCheckPaid ? 0 : interest_New > decInterestCharge ? decInterestCharge : interest_New;
                            strMess.AppendLine("decnewinterestamount " + decnewinterestamount);
                            strMess.AppendLine("bolCheckPaid " + bolCheckPaid);
                            strMess.AppendLine("interest_New " + interest_New);
                            strMess.AppendLine("decInterestCharge " + decInterestCharge);
                            ISDetail["bsd_newinterestamount"] = new Money(decnewinterestamount);
                            ISDetail["bsd_interestchargeamount"] = new Money(decnewinterestamount + decInterestAmountIns);
                            bsd_interestchargeamount = interest + interest_New;
                            ISDetail["bsd_advancepayment"] = new Money(0);
                            strMess.AppendLine("bsd_numberofdaysdue: " + ISDetail["bsd_numberofdaysdue"].ToString());
                            strMess.AppendLine("bsd_outstandingday: " + ISDetail["bsd_outstandingday"].ToString());
                            strMess.AppendLine("2.1");
                        }
                        else
                        {

                            ISDetail["bsd_outstandingday"] = 0;
                            ISDetail["bsd_paymentscheme"] = payScheme.ToEntityReference();
                            ISDetail["bsd_interestpercent"] = (decimal)0;
                            ISDetail["bsd_interestamountinstallment"] = new Money(0);
                            ISDetail["bsd_newinterestamount"] = new Money(0);
                            ISDetail["bsd_interestchargeamount"] = new Money(0);
                            ISDetail["bsd_advancepayment"] = new Money(0);

                            strMess.AppendLine("bsd_numberofdaysdue: " + ISDetail["bsd_numberofdaysdue"].ToString());
                            strMess.AppendLine("bsd_outstandingday: " + ISDetail["bsd_outstandingday"].ToString());
                            strMess.AppendLine("2.2");
                        }

                    }
                    #endregion
                    else
                    {
                        if (ins.Contains("bsd_duedate"))
                        {
                            strMess.AppendLine("222222222222222");
                            duedate = RetrieveLocalTimeFromUTCTime((DateTime)ins["bsd_duedate"]);
                            ISDetail["bsd_duedate"] = duedate;
                            strMess.AppendLine("duedate: " + duedate.ToString());
                            ISDetail["bsd_outstandingday"] = 0;
                            ISDetail["bsd_paymentscheme"] = payScheme.ToEntityReference();
                            ISDetail["bsd_interestpercent"] = (decimal)0;
                            ISDetail["bsd_interestamountinstallment"] = new Money(0);
                            ISDetail["bsd_newinterestamount"] = new Money(0);
                            ISDetail["bsd_interestchargeamount"] = new Money(0);
                            ISDetail["bsd_advancepayment"] = new Money(0);

                            strMess.AppendLine("bsd_numberofdaysdue: " + ISDetail["bsd_numberofdaysdue"].ToString());
                            strMess.AppendLine("bsd_outstandingday: " + ISDetail["bsd_outstandingday"].ToString());
                            strMess.AppendLine("2.3");
                        }
                    }
                    strMess.AppendLine("++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++ 5");
                    service.Create(ISDetail);
                }
                //throw new InvalidPluginExecutionException(strMess.ToString());
                strMess.AppendLine("---------------------------------------");
            }
            catch (InvalidPluginExecutionException ex)
            {
                strMess.AppendLine(ex.ToString());
                throw new InvalidPluginExecutionException(strMess.ToString());
            }
        }
        private static decimal getInterestCap(Entity enOptionEntry)
        {
            decimal lim = -100599;
            decimal totalamount = enOptionEntry.Contains("totalamount") ? ((Money)enOptionEntry["totalamount"]).Value : 0;
            strMess.AppendLine("totalamount: " + totalamount.ToString());
            EntityReference enrefPaymentScheme = enOptionEntry.Contains("bsd_paymentscheme") ? (EntityReference)enOptionEntry["bsd_paymentscheme"] : null;
            if (enrefPaymentScheme != null)
            {
                Entity enPaymentScheme = service.Retrieve(enrefPaymentScheme.LogicalName, enrefPaymentScheme.Id, new ColumnSet(true));
                EntityReference enrefInterestRateMaster = enPaymentScheme.Contains("bsd_interestratemaster") ? (EntityReference)enPaymentScheme["bsd_interestratemaster"] : null;
                if (enrefInterestRateMaster != null)
                {
                    Entity enInterestRateMaster = service.Retrieve(enrefInterestRateMaster.LogicalName, enrefInterestRateMaster.Id, new ColumnSet(true));
                    decimal bsd_toleranceinterestamount = enInterestRateMaster.Contains("bsd_toleranceinterestamount") ? ((Money)enInterestRateMaster["bsd_toleranceinterestamount"]).Value : 0;
                    decimal bsd_toleranceinterestpercentage = enInterestRateMaster.Contains("bsd_toleranceinterestpercentage") ? (decimal)enInterestRateMaster["bsd_toleranceinterestpercentage"] : 0;
                    decimal amountcalbypercent = totalamount * bsd_toleranceinterestpercentage / 100;
                    strMess.AppendLine("bsd_toleranceinterestamount: " + bsd_toleranceinterestamount.ToString());
                    strMess.AppendLine("bsd_toleranceinterestpercentage: " + bsd_toleranceinterestpercentage.ToString());
                    strMess.AppendLine("amountcalbypercent: " + bsd_toleranceinterestamount.ToString());
                    if (bsd_toleranceinterestamount > 0 && amountcalbypercent > 0 && enInterestRateMaster.Contains("bsd_toleranceinterestamount") && enInterestRateMaster.Contains("bsd_toleranceinterestpercentage"))
                    {
                        lim = Math.Min(bsd_toleranceinterestamount, amountcalbypercent);
                    }
                    else
                    {
                        if (bsd_toleranceinterestamount > 0 && enInterestRateMaster.Contains("bsd_toleranceinterestamount"))
                        {
                            lim = bsd_toleranceinterestamount;
                        }
                        if (amountcalbypercent > 0 && enInterestRateMaster.Contains("bsd_toleranceinterestpercentage"))
                        {
                            lim = amountcalbypercent;
                        }
                    }

                }
            }
            strMess.AppendLine("Cap: " + lim.ToString());
            return lim;
        }
        private static void updateNewInterestAmount(Entity enOptionEntry, Entity enInterestSimulateOption, int reportype)
        {
            Entity optionEntry = service.Retrieve(enOptionEntry.LogicalName, enOptionEntry.Id, new ColumnSet(true));
            //Lấy list Interest Simulation Detail có New Interest Amount > 0 theo thứ tự
            string condition = "<condition attribute='bsd_interestsimulation' operator='null' />";
            if (enInterestSimulateOption != null)
            {
                condition = "<condition attribute='bsd_aginginterestsimulationoption' operator='eq' uitype='bsd_aginginterestsimulationoption' value='" + enInterestSimulateOption.Id.ToString() + "' />";
            }
            string xml = @"<fetch version='1.0' output-format='xml-platform' mapping='logical' distinct='false'>
	            <entity name='bsd_interestsimulationdetail'>
		            <attribute name='bsd_interestsimulationdetailid' />
		            <attribute name='bsd_name' />
		            <attribute name='createdon' />
		            <attribute name='bsd_interestamountinstallment' />
		            <attribute name='bsd_interestchargeamount' />
		            <attribute name='bsd_newinterestamount' />
		            <attribute name='bsd_interestsimulation' />
		            <attribute name='bsd_inrerestamountinstallment' />
		            <filter type='and'>
			            <condition attribute='bsd_optionentry' operator='eq' uitype='salesorder' value='" + enOptionEntry.Id.ToString() + @"' />
			            " + condition + @"
		            </filter>
		            <link-entity name='bsd_paymentschemedetail' from='bsd_paymentschemedetailid' to='bsd_installment' visible='false' link-type='outer' alias='interestsimulationdetail_paymentschemedetail'>
			            <attribute name='bsd_ordernumber' />
			            <order attribute='bsd_ordernumber' descending='false' />
		            </link-entity>
	            </entity>
            </fetch>";
            EntityCollection encolInterestSimulationDetail = service.RetrieveMultiple(new FetchExpression(xml));
            decimal cap = getInterestCap(optionEntry);
            strMess.AppendLine("Cap: " + cap.ToString());
            if (cap != -100599)
            {
                //Tính toán New Interest Amount sao cho tổng nhỏ hơn hoặc bằng cap
                //Ưu tiên giảm các đợt cuối trở về trước
                decimal sumInterestAmount = encolInterestSimulationDetail.Entities.AsEnumerable().Sum(x => x.Contains("bsd_interestamountinstallment") ? ((Money)x["bsd_interestamountinstallment"]).Value : 0);
                strMess.AppendLine("sumInterestAmount: " + sumInterestAmount);
                decimal[] arrNewInterestAmount = { };
                for (int i = 0; i < encolInterestSimulationDetail.Entities.Count; i++)
                {
                    Entity enInterestSimulationDetail = encolInterestSimulationDetail.Entities[i];
                    strMess.AppendLine("sumInterestAmount < cap: " + (sumInterestAmount < cap).ToString());
                    decimal bsd_newinterestamount = enInterestSimulationDetail.Contains("bsd_newinterestamount") ? ((Money)enInterestSimulationDetail["bsd_newinterestamount"]).Value : 0;
                    decimal bsd_interestamountinstallment = enInterestSimulationDetail.Contains("bsd_interestamountinstallment") ? ((Money)enInterestSimulationDetail["bsd_interestamountinstallment"]).Value : 0;
                    if (sumInterestAmount < cap)
                    {
                        strMess.AppendLine("bsd_newinterestamount: " + bsd_newinterestamount.ToString());
                        decimal total = sumInterestAmount + bsd_newinterestamount;
                        strMess.AppendLine("total: " + total.ToString());
                        if (total > cap && bsd_newinterestamount != 0)
                        {
                            decimal denta = cap - sumInterestAmount;
                            strMess.AppendLine("denta:" + denta.ToString());
                            //Set New Interest Amount = denta
                            Entity en = new Entity(enInterestSimulationDetail.LogicalName, enInterestSimulationDetail.Id);
                            en["bsd_newinterestamount"] = new Money(denta);
                            en["bsd_interestchargeamount"] = new Money(denta + bsd_interestamountinstallment);
                            service.Update(en);
                            sumInterestAmount += denta;
                        }
                        else
                        {
                            sumInterestAmount += bsd_newinterestamount;
                        }
                    }
                    else
                    {
                        //Set New Interest Amount = 0
                        Entity en = new Entity(enInterestSimulationDetail.LogicalName, enInterestSimulationDetail.Id);
                        en["bsd_newinterestamount"] = new Money(0);
                        en["bsd_interestchargeamount"] = new Money(bsd_interestamountinstallment);
                        service.Update(en);
                    }

                }
            }
            //Với Type = Aging report(100000000) --> Chỉ lấy Installment có Interest
            if (reportype == 100000000)
            {
                encolInterestSimulationDetail = service.RetrieveMultiple(new FetchExpression(xml));
                for (int i = 0; i < encolInterestSimulationDetail.Entities.Count; i++)
                {
                    Entity enInterestSimulationDetail = encolInterestSimulationDetail.Entities[i];
                    decimal bsd_interestamountinstallment = enInterestSimulationDetail.Contains("bsd_interestamountinstallment") ? ((Money)enInterestSimulationDetail["bsd_interestamountinstallment"]).Value : 0;
                    decimal bsd_newinterestamount = enInterestSimulationDetail.Contains("bsd_newinterestamount") ? ((Money)enInterestSimulationDetail["bsd_newinterestamount"]).Value : 0;
                    if (bsd_newinterestamount == 0 && bsd_interestamountinstallment == 0)
                    {
                        service.Delete(enInterestSimulationDetail.LogicalName, enInterestSimulationDetail.Id);
                    }
                }
            }
        }
        private static void createSimulationOptionByListUnit(EntityReference enRef, EntityCollection listUnit)
        {

            string fetchXml = @"<fetch version='1.0' output-format='xml-platform' mapping='logical' distinct='false'>
                  <entity name='bsd_aginginterestsimulationoption'>
                    <attribute name='bsd_name' />
                    <filter type='and'>
                      <condition attribute='bsd_aginginterestsimulation' operator='eq' value='" + enRef.Id.ToString() + @"' />
                    </filter>

                  </entity>
                </fetch>";
            EntityCollection interestSimulations = service.RetrieveMultiple(new FetchExpression(fetchXml));

            if (interestSimulations.Entities.Count == 0)
            {
                EntityCollection listOptionEntry = new EntityCollection();
                foreach (Entity unit in listUnit.Entities)
                {

                    EntityCollection optionEntrys = GetOptionList(unit.Attributes["productid"].ToString());
                    foreach (Entity optionEntry in optionEntrys.Entities)
                    {
                        listOptionEntry.Entities.Add(optionEntry);
                    }
                }

                foreach (Entity optionEntry in listOptionEntry.Entities)
                {
                    //throw new Exception(enRef.Id.ToString() + "---"+ optionEntry.Id.ToString());
                    Entity agInSimOption = new Entity("bsd_aginginterestsimulationoption");
                    agInSimOption["bsd_name"] = optionEntry.Attributes["name"].ToString();
                    EntityReference aginginterestsimulation = new EntityReference(enRef.LogicalName, enRef.Id);
                    EntityReference optionentry = new EntityReference(optionEntry.LogicalName, optionEntry.Id);
                    agInSimOption["bsd_aginginterestsimulation"] = aginginterestsimulation;
                    agInSimOption["bsd_optionentry"] = optionentry;
                    Guid createdId = service.Create(agInSimOption);
                }
            }
        }
        private static Entity getSimulationOption(string simulationid, string bsd_optionentry)
        {
            strMess.AppendLine("simulationid: " + simulationid);
            strMess.AppendLine("bsd_optionentryid: " + bsd_optionentry);
            QueryExpression que = new QueryExpression("bsd_aginginterestsimulationoption");
            que.ColumnSet = new ColumnSet(true);
            que.Criteria = new FilterExpression(LogicalOperator.And);
            que.Criteria.AddCondition(new ConditionExpression("bsd_aginginterestsimulation", ConditionOperator.Equal, simulationid));
            que.Criteria.AddCondition(new ConditionExpression("bsd_optionentry", ConditionOperator.Equal, bsd_optionentry));
            EntityCollection simulationoptions = service.RetrieveMultiple(que);
            strMess.AppendLine(simulationoptions.Entities.Count.ToString());
            if (simulationoptions.Entities.Count > 0)
            {
                return simulationoptions.Entities[0];
            }
            else
            {
                return null;
            }

        }
        private static decimal CalculateNewInterest(decimal balance, int lateDays, decimal interestPercent)
        {
            decimal interest = balance * lateDays * interestPercent / 100;
            return interest;
        }
        private static decimal CalculateInterestNotPaid(Entity ins)
        {
            decimal interestamount = ins.Contains("bsd_amountofthisphase") ? ((Money)ins["bsd_amountofthisphase"]).Value : decimal.Zero;
            decimal interestamountpaid = ins.Contains("bsd_amountwaspaid") ? ((Money)ins["bsd_amountwaspaid"]).Value : decimal.Zero;
            decimal waiverinterest = ins.Contains("bsd_waiverinstallment") ? ((Money)ins["bsd_waiverinstallment"]).Value : decimal.Zero;
            return interestamount - interestamountpaid - waiverinterest;
        }
        private static decimal CalculateInterestAmount(Entity ins)
        {
            decimal interestamount = ins.Contains("bsd_interestchargeamount") ? ((Money)ins["bsd_interestchargeamount"]).Value : decimal.Zero;
            decimal interestamountpaid = ins.Contains("bsd_interestwaspaid") ? ((Money)ins["bsd_interestwaspaid"]).Value : decimal.Zero;
            decimal waiverinterest = ins.Contains("bsd_waiverinterest") ? ((Money)ins["bsd_waiverinterest"]).Value : decimal.Zero;
            return interestamount - interestamountpaid - waiverinterest;
        }
        private static int CheckGroupAging(decimal i)
        {
            if (i <= 15)
                return 100000000;
            else if (i > 15 && i <= 30)
                return 100000001;
            else if (i > 30 && i <= 60)
                return 100000002;
            else if (i > 60 && i <= 90)
                return 100000003;
            else // (i > 90)
                return 100000004;
        }
        private static EntityCollection GetOptionList(string UnitsID)
        {
            StringBuilder xml = new StringBuilder();
            xml.AppendLine("<fetch version='1.0' output-format='xml-platform' mapping='logical'>");
            xml.AppendLine("<entity name='salesorder'>");
            xml.AppendLine("<attribute name='name' />");
            xml.AppendLine("<attribute name='bsd_paymentscheme' />");
            xml.AppendLine("<attribute name='bsd_project' />");
            xml.AppendLine("<attribute name='statuscode' />");
            xml.AppendLine("<filter type='and'>");
            xml.AppendLine(string.Format("<condition attribute='bsd_unitnumber' operator='eq' value='{0}'/>", UnitsID));
            xml.AppendLine("<condition attribute='statuscode' operator='neq' value='100000006'/>");
            xml.AppendLine("</filter>");
            xml.AppendLine("</entity>");
            xml.AppendLine("</fetch>");

            return service.RetrieveMultiple(new FetchExpression(xml.ToString()));
        }
        private static EntityCollection CalSum_AdvancePayment(string OptionID)
        {
            StringBuilder xml = new StringBuilder();
            xml.AppendLine("<fetch version='1.0' output-format='xml-platform' mapping='logical' aggregate='true'>");
            xml.AppendLine("<entity name='bsd_advancepayment'>");
            xml.AppendLine("<attribute name='bsd_remainingamount' alias='SumAdv' aggregate='sum'/>");
            xml.AppendLine("<filter type='and'>");
            xml.AppendLine(string.Format("<condition attribute='bsd_optionentry' operator='eq' value='{0}'/>", OptionID));
            xml.AppendLine("<condition attribute='statuscode' operator='eq' value='100000000'/>");
            xml.AppendLine("</filter>");
            xml.AppendLine("</entity>");
            xml.AppendLine("</fetch>");
            strMess.AppendLine(xml.ToString());
            return service.RetrieveMultiple(new FetchExpression(xml.ToString()));
        }
        private static void updateAdvantPayment(Entity oe, string aginginterestsimulationoption)
        {
            //Cal Sum Advance Payment
            decimal AdvPayAmt = 0;
            EntityCollection AdvSum = CalSum_AdvancePayment(oe.Id.ToString());
            strMess.AppendLine("1");
            if (AdvSum.Entities.Count > 0)
            {
                Entity AdvSumEn = AdvSum.Entities[0];
                //throw new InvalidPluginExecutionException(oe.Id.ToString());

                if (((AliasedValue)AdvSumEn.Attributes["SumAdv"]).Value != null)
                    AdvPayAmt = ((Money)((AliasedValue)AdvSumEn.Attributes["SumAdv"]).Value).Value;
            }
            strMess.AppendLine("2");
            string condition = "<condition attribute='bsd_aginginterestsimulationoption' operator='null' />";
            if (aginginterestsimulationoption != "")
            {
                condition = "<condition attribute='bsd_aginginterestsimulationoption' operator='eq' value='" + aginginterestsimulationoption + "'/>";
            }
            strMess.AppendLine("AdvantPayment: " + AdvPayAmt.ToString());
            //Hồ fix 03-06-2019
            string xml = @"<fetch version='1.0' output-format='xml-platform' mapping='logical' distinct='false'>
	            <entity name='bsd_interestsimulationdetail'>
		            <attribute name='bsd_name' />
		            <attribute name='createdon' />
		            <attribute name='bsd_units' />
		            <attribute name='bsd_optionentry' />
		            <attribute name='bsd_installment' />
		            <attribute name='statuscode' />
		            <attribute name='bsd_outstandingday' />
		            <attribute name='bsd_interestsimulation' />
		            <attribute name='bsd_interestchargeamount' />
		            <attribute name='bsd_interestpercent' />
		            <attribute name='bsd_aginginterestsimulationoption' />
		            <attribute name='bsd_interestsimulationdetailid' />
		            <attribute name='bsd_duedate' />
		            <attribute name='bsd_simulationdate' />
		            <attribute name='bsd_paidamount' />
		            <attribute name='bsd_installmentamount' />
		            <attribute name='bsd_outstandingamount' />
                    
		            <filter type='and'>
			            <condition attribute='statecode' operator='eq' value='0' />
			            <condition attribute='bsd_optionentry' operator='eq' uitype='salesorder' value='" + oe.Id.ToString() + @"' />
			            " + condition + @"
                       
		            </filter>
		            <link-entity name='bsd_paymentschemedetail' from='bsd_paymentschemedetailid' to='bsd_installment' visible='false' link-type='outer' alias='installment'>
			            <attribute name='bsd_ordernumber' />
			            <order attribute='bsd_ordernumber' descending='false' />
		            </link-entity>
	            </entity>
            </fetch>";
            //Hồ fix 03-06-2019
            strMess.AppendLine(xml);

            EntityCollection encoInterestSimulationDetail = service.RetrieveMultiple(new FetchExpression(xml));
            strMess.AppendLine(encoInterestSimulationDetail.Entities.Count.ToString());
            if (encoInterestSimulationDetail.Entities.Count > 0)
            {
                Entity enInterestSimulationDetail = new Entity("bsd_interestsimulationdetail", encoInterestSimulationDetail.Entities[0].Id);
                enInterestSimulationDetail["bsd_advancepayment"] = new Money(AdvPayAmt);
                service.Update(enInterestSimulationDetail);
                strMess.AppendLine("Update done");
            }
        }
        public static DateTime RetrieveLocalTimeFromUTCTime(DateTime utcTime)
        {
            int? timeZoneCode = RetrieveCurrentUsersSettings(service);
            if (!timeZoneCode.HasValue)
                throw new InvalidPluginExecutionException("Can't find time zone code");
            var request = new LocalTimeFromUtcTimeRequest
            {
                TimeZoneCode = timeZoneCode.Value,
                UtcTime = utcTime.ToUniversalTime()
            };

            LocalTimeFromUtcTimeResponse response = (LocalTimeFromUtcTimeResponse)service.Execute(request);
            return response.LocalTime;
            //var utcTime = utcTime.ToString("MM/dd/yyyy HH:mm:ss");
            //var localDateOnly = response.LocalTime.ToString("dd-MM-yyyy");
        }
        private static int? RetrieveCurrentUsersSettings(IOrganizationService service)
        {
            var currentUserSettings = service.RetrieveMultiple(
            new QueryExpression("usersettings")
            {
                ColumnSet = new ColumnSet("localeid", "timezonecode"),
                Criteria = new FilterExpression
                {
                    Conditions = { new ConditionExpression("systemuserid", ConditionOperator.EqualUserId) }
                }
            }).Entities[0].ToEntity<Entity>();

            return (int?)currentUserSettings.Attributes["timezonecode"];
        }
        #region Code tính Interest
        private static void getInterestStartDate()
        {
            try
            {
                EntityReference enrefOptionEntry = enInstallment.Contains("bsd_optionentry") ? (EntityReference)enInstallment["bsd_optionentry"] : null;
                Entity enOptionEntry = enInstallment.Contains("bsd_optionentry") ? service.Retrieve(enrefOptionEntry.LogicalName, enrefOptionEntry.Id, new ColumnSet(true)) : null;

                EntityReference enrefPaymentScheme = enOptionEntry.Contains("bsd_paymentscheme") ? (EntityReference)enOptionEntry["bsd_paymentscheme"] : null;
                Entity enPaymentScheme = service.Retrieve(enrefPaymentScheme.LogicalName, enrefPaymentScheme.Id, new ColumnSet(true));
                EntityReference enrefInterestrateMaster = enPaymentScheme.Contains("bsd_interestratemaster") ? (EntityReference)enPaymentScheme["bsd_interestratemaster"] : null;
                if (enrefInterestrateMaster != null)
                {
                    Entity enInterestrateMaster = service.Retrieve(enrefInterestrateMaster.LogicalName, enrefInterestrateMaster.Id, new ColumnSet(true));
                    objIns.Gracedays = enInterestrateMaster.Contains("bsd_gracedays") ? ((int)enInterestrateMaster["bsd_gracedays"]) : 0;
                    objIns.orderNumber = enInstallment.Contains("bsd_ordernumber") ? ((int)enInstallment["bsd_ordernumber"]) : 0;
                    objIns.idOE = enOptionEntry.Id;

                    objIns.Intereststartdatetype = ((OptionSetValue)enInterestrateMaster["bsd_intereststartdatetype"]).Value;//100000000: Due date;100000001: Grace period
                                                                                                                             //throw new InvalidPluginExecutionException(((DateTime)enInstallment["bsd_duedate"]).ToString());
                    objIns.Duedate = enInstallment.Contains("bsd_duedate") ? RetrieveLocalTimeFromUTCTime((DateTime)enInstallment["bsd_duedate"]) : DateTime.Now;

                    objIns.MaxPercent = enInterestrateMaster.Contains("bsd_toleranceinterestpercentage") ? (decimal)enInterestrateMaster["bsd_toleranceinterestpercentage"] : 100;
                    objIns.MaxAmount = enInterestrateMaster.Contains("bsd_toleranceinterestamount") ? ((Money)enInterestrateMaster["bsd_toleranceinterestamount"]).Value : 0;
                    objIns.InterestPercent = enInstallment.Contains("bsd_interestchargeper") ? (decimal)enInstallment["bsd_interestchargeper"] : 0;
                    objIns.InterestStarDate = objIns.Duedate.AddDays(objIns.Gracedays + 1);
                    strMess.AppendLine("InterestStarDate: " + objIns.InterestStarDate.ToString());
                }
            }
            catch (InvalidPluginExecutionException ex)
            {
                throw new InvalidPluginExecutionException(strMess.ToString());
            }


        }
        public static int getLateDays(DateTime dateCalculate)
        {
            try
            {
                int lateDays = (int)dateCalculate.Date.Subtract(objIns.Duedate.Date).TotalDays;
                int orderNumberSightContract = getViTriDotSightContract(objIns.idOE);
                Entity oe = service.Retrieve("salesorder", objIns.idOE, new ColumnSet(true));
                int bsd_ordernumber = objIns.orderNumber;
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
                            DateTime bsd_signeddadate = RetrieveLocalTimeFromUTCTime((DateTime)oe["bsd_signeddadate"]);
                            TimeSpan difference2 = dateCalculate - bsd_signeddadate;
                            numberOfDays2 = difference2.Days;
                            numberOfDays2 = numberOfDays2 < 0 ? 0 : numberOfDays2;
                        }
                    }
                    else numberOfDays2 = -100599;
                }
                if (objIns.Intereststartdatetype == 100000001)// Grace Period
                {
                    lateDays = lateDays - objIns.Gracedays;
                    lateDays = lateDays < 0 ? 0 : lateDays;
                    if (numberOfDays2 != -100599 && numberOfDays2 < lateDays) lateDays = numberOfDays2;
                }
                else//DueDate
                {
                    lateDays = lateDays < 0 ? 0 : lateDays;
                    if (numberOfDays2 != -100599 && numberOfDays2 < lateDays) lateDays = numberOfDays2;
                    if (objIns.InterestStarDate > dateCalculate)
                    {
                        lateDays = 0;
                    }
                }

                objIns.LateDays = lateDays;
                if (lateDays < 0)
                    lateDays = 0;
                return lateDays;
            }
            catch (InvalidPluginExecutionException ex)
            {
                strMess.AppendLine(ex.ToString());
                throw new InvalidPluginExecutionException(strMess.ToString());
            }

        }
        private static int getViTriDotSightContract(Guid idOE)
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
        /// <summary>
        /// Điều chỉnh logic code 18.09.2019
        /// </summary>
        /// <param name="dateCalculate"></param>
        /// <param name="amountPay"></param>
        /// <returns></returns>
        public static decimal calc_InterestCharge(DateTime dateCalculate, decimal amountPay, ref decimal interestMasterPercent)
        {
            try
            {
                var result = 0m;
                strMess.AppendLine("[calc_InterestCharge2] Tham số đầu vào:");
                strMess.AppendLine(string.Format("- amountPay: {0}", format_Money(amountPay)));
                strMess.AppendLine("11111111");
                EntityReference OE_ref = enInstallment.Contains("bsd_optionentry") ? (EntityReference)enInstallment["bsd_optionentry"] : null;
                if (OE_ref == null)
                {
                    strMess.AppendLine(string.Format("Không có dữ liệu record [bsd_optionentry]"));
                    throw new Exception(strMess.ToString());
                }
                #region --- Body Code ---
                if (OE_ref != null)
                {
                    strMess.AppendLine(string.Format("ID record [bsd_optionentry]: {0}", OE_ref.Id));
                    decimal interestcharge_amount = 0;
                    Entity OE = service.Retrieve(OE_ref.LogicalName, OE_ref.Id, new ColumnSet(true));
                    Entity Project = service.Retrieve("bsd_project", ((EntityReference)OE["bsd_project"]).Id, new ColumnSet(new string[] { "bsd_name", "bsd_dailyinterestchargebank" }));
                    bool bsd_dailyinterestchargebank = Project.Contains("bsd_dailyinterestchargebank") ? (bool)Project["bsd_dailyinterestchargebank"] : false;
                    decimal d_dailyinterest = 0;

                    strMess.AppendLine(string.Format("- bsd_dailyinterestchargebank: {0}", bsd_dailyinterestchargebank));
                    if (bsd_dailyinterestchargebank)
                    {
                        #region --- Lấy thông tin setup bsd_dailyinterestrate, dòng đầu tiên ---
                        EntityCollection entc = get_ec_bsd_dailyinterestrate(Project.Id);
                        Entity ent = entc.Entities[0];
                        ent.Id = entc.Entities[0].Id;
                        if (!ent.Contains("bsd_interestrate"))
                            throw new InvalidPluginExecutionException("Can not find Daily Interestrate in Project " + (string)Project["bsd_name"] + " in master data. Please check again!");
                        d_dailyinterest = (decimal)ent["bsd_interestrate"];
                        #endregion
                    }
                    strMess.AppendLine("InterestPercent" + objIns.InterestPercent);
                    strMess.AppendLine("d_dailyinterest" + d_dailyinterest);

                    objIns.InterestPercent = (objIns.InterestPercent + d_dailyinterest);
                    interestMasterPercent = objIns.InterestPercent;
                    decimal interestcharge_percent = objIns.InterestPercent / 100 * objIns.LateDays;
                    interestcharge_amount = Convert.ToDecimal(amountPay) * interestcharge_percent;
                    strMess.AppendLine("LateDays" + objIns.LateDays);
                    strMess.AppendLine("amountPay" + amountPay);
                    #region --- Trace đợt đang xét ---
                    strMess.AppendLine(string.Format("----------------------------------------------------"));
                    strMess.AppendLine(string.Format("#. [Đợt đang xét] "));
                    strMess.AppendLine(string.Format("- InterestPercent: {0}", format_Money(objIns.InterestPercent)));
                    strMess.AppendLine(string.Format("- interestcharge_percent: {0}", format_Money(interestcharge_percent)));
                    strMess.AppendLine(string.Format("- interestcharge_amount 'tiền lãi': {0}", format_Money(interestcharge_amount)));
                    strMess.AppendLine(string.Format("----------------------------------------------------"));
                    #endregion

                    decimal sum_bsd_waiverinterest = sumWaiverInterest(OE);
                    decimal sum_Inr_AM = SumInterestAM_OE_New(OE.Id, dateCalculate, amountPay) - sum_bsd_waiverinterest;
                    decimal sum_temp = sum_Inr_AM + interestcharge_amount;

                    #region --- Trace kết quả tổng đợt: tạm tính ---
                    strMess.AppendLine(string.Format("----------------------------------------------------"));
                    strMess.AppendLine(string.Format("##. Kết quả tổng đợt TẠM TÍNH: "));
                    strMess.AppendLine(string.Format("- Số tiền trả trước [waiverinterest]: {0}", format_Money(sum_bsd_waiverinterest)));
                    strMess.AppendLine(string.Format("- Tổng lãi phát sinh, không tính lãi đợt đang xét [sum_Inr_AM]: {0}", format_Money(sum_Inr_AM)));
                    strMess.AppendLine(string.Format("- Tổng lãi phát sinh [sum_temp] (bao gồm đợt đang xét): {0}", format_Money(sum_temp)));
                    strMess.AppendLine(string.Format("----------------------------------------------------"));
                    #endregion

                    #region --- @. Throw theo điều kiện: Hân note ---
                    // 170224 - @Han confirm - su dung field net selling price de tinh interest charge  - k dung total amount
                    decimal d_enOptionEntry_bsd_totalamountlessfreight = OE.Contains("bsd_totalamountlessfreight") ? ((Money)OE["bsd_totalamountlessfreight"]).Value : 0;
                    if (d_enOptionEntry_bsd_totalamountlessfreight == 0) throw new InvalidPluginExecutionException("'Net Selling Price' of " + (string)OE["name"] + " must be larger than 0");

                    // Han_28072018 - Update field tính Interest Charge = field Total Amount
                    decimal enOptionEntry_TotalAmount = OE.Contains("totalamount") ? ((Money)OE["totalamount"]).Value : 0;
                    if (enOptionEntry_TotalAmount == 0) throw new InvalidPluginExecutionException("'Total Amount' of " + (string)OE["name"] + " must be larger than 0");
                    #endregion

                    #region --- #. Tính số tiền CAP ---
                    decimal range_enOptionEntryAM = 0;
                    decimal cap = 0;
                    /* ------------------------------------------------------------------------
                     * Lưu ý: 
                     * - Tính total range interertcharge dựa vào tỷ lệ % và số tiền
                     * - Nếu range nào chạm mức trước thì tính range đó
                     *      + MaxPercent ->  Cài đặt %, tính ra số tiền theo %
                     *      + Maxamount  ->  Cài đặt trước số tiền
                     --------------------------------------------------------------------------*/
                    strMess.AppendLine("#. Tính số tiền CAP:");
                    strMess.AppendLine(string.Format("- MaxPercent: {0}", format_Money(objIns.MaxPercent)));
                    strMess.AppendLine(string.Format("- Maxamount: {0}", format_Money(objIns.MaxAmount)));

                    if (objIns.MaxPercent > 0)
                    {
                        //range_enOptionEntryAM = d_enOptionEntry_bsd_totalamountlessfreight * MaxPercent / 100;
                        range_enOptionEntryAM = enOptionEntry_TotalAmount * objIns.MaxPercent / 100;
                        if (objIns.MaxAmount > 0)
                        {
                            if (range_enOptionEntryAM > objIns.MaxAmount) cap = objIns.MaxAmount;
                            else cap = range_enOptionEntryAM;
                        }
                        else cap = range_enOptionEntryAM;
                    }
                    else
                    {
                        cap = objIns.MaxAmount > 0 ? objIns.MaxAmount : 0;
                    }

                    strMess.AppendLine(string.Format("- Calc Amout of MaxPercent: {0}", format_Money(range_enOptionEntryAM)));
                    strMess.AppendLine(string.Format("@ CAP: {0}", format_Money(cap)));
                    #endregion

                    #region --- @@ Nghiệp vụ tính tiền lãi ---
                    /* ------------------------------------------------------------------------------------------
                     * Khái niệm: 
                     * - Tổng tiền trễ tất cả các đợt. bao gồm đợt hiện tại             : sum_temp
                     * - Cap : số tiền chạm móc đầu tiên ()                             : cap    
                     * - Tổng tiền trễ không tính đợt hiện tại - sum(waiverinterest)    : sum_Inr_AM
                     * - Tiền trễ đợt hiện tại                                          : interestcharge_amount
                     --------------------------------------------------------------------------------------------*/
                    strMess.AppendLine("222222222");
                    if (cap <= 0)
                    {
                        var rs = check_Data_Setup();
                        strMess.AppendLine(string.Format("Case: cap <= 0"));
                        if (rs)
                        {
                            strMess.AppendLine("Không giới hạn tiền lãi");
                            result = interestcharge_amount;
                        }
                        else
                        {
                            strMess.AppendLine("Cap thiết lặp chạm móc là 0. return 0");
                            result = 0m;
                        }
                    }
                    else if (sum_temp > cap)
                    {
                        strMess.AppendLine(string.Format("sum_temp " + sum_temp));
                        strMess.AppendLine(string.Format("cap " + cap));
                        strMess.AppendLine(string.Format("Case: sum_temp > cap"));
                        if (cap > sum_Inr_AM)
                        {
                            strMess.AppendLine(string.Format("@ cap > sum_Inr_AM"));
                            result = cap - sum_Inr_AM;
                        }
                        else
                        {
                            strMess.AppendLine(string.Format("@ cap <= sum_Inr_AM"));
                            result = 0m;
                        }

                    }
                    else if (sum_temp == cap)
                    {
                        strMess.AppendLine(string.Format("Case: sum_temp == cap"));
                        if (sum_Inr_AM < cap)
                            result = cap - sum_Inr_AM;
                        else
                            result = 0m;
                    }
                    else if (sum_temp < cap)
                    {
                        strMess.AppendLine(string.Format("Case: sum_temp < cap"));
                        result = interestcharge_amount;
                    }
                    else
                    {
                        strMess.AppendLine(string.Format("Case: Chưa xác định, lấy nguyên lãi của đợt này"));
                        result = interestcharge_amount;
                    }
                    #endregion

                }
                #endregion

                strMess.AppendLine(string.Format("result : {0}", format_Money(result)));
                //throw new InvalidPluginExecutionException(strMess.ToString());
                return result;
                //Test trace ##############################

            }
            catch (Exception ex)
            {
                strMess.AppendLine(ex.ToString());
                throw new InvalidPluginExecutionException(strMess.ToString());
            }
        }
        public static decimal sumWaiverInterest(Entity enOptionEntry)
        {
            // Define Condition Values
            var QEbsd_paymentschemedetail_bsd_optionentry = enOptionEntry.Id;
            var QEbsd_paymentschemedetail_statecode = 0;

            // Instantiate QueryExpression QEbsd_paymentschemedetail
            var QEbsd_paymentschemedetail = new QueryExpression("bsd_paymentschemedetail");

            // Add columns to QEbsd_paymentschemedetail.ColumnSet
            QEbsd_paymentschemedetail.ColumnSet.AddColumns("bsd_name");
            QEbsd_paymentschemedetail.ColumnSet.AddColumns("bsd_waiverinterest");

            // Define filter QEbsd_paymentschemedetail.Criteria
            QEbsd_paymentschemedetail.Criteria.AddCondition("bsd_optionentry", ConditionOperator.Equal, QEbsd_paymentschemedetail_bsd_optionentry);
            QEbsd_paymentschemedetail.Criteria.AddCondition("statecode", ConditionOperator.Equal, QEbsd_paymentschemedetail_statecode);
            EntityCollection encolInstallment = service.RetrieveMultiple(QEbsd_paymentschemedetail);
            strMess.AppendLine("Count encolInstallment: " + encolInstallment.Entities.Count.ToString());
            //decimal sum = encolInstallment.Entities.Sum(x => ((Money)x.Attributes["bsd_waiverinterest"]).Value);
            decimal sum = 0;
            foreach (Entity en in encolInstallment.Entities)
            {
                decimal bsd_waiverinterest = en.Contains("bsd_waiverinterest") ? ((Money)en["bsd_waiverinterest"]).Value : 0;
                strMess.AppendLine("bsd_waiverinterest: " + bsd_waiverinterest.ToString());
                sum += bsd_waiverinterest;
            }
            strMess.AppendLine("Sum bsd_waiverinterest: " + sum.ToString());
            return sum;
        }
        public static decimal getInterestSimulation(Entity enIns, DateTime dateCalculate, decimal amountpay)
        {
            strMess.AppendLine("aaaaaaaaaaaaaaaaaaaaaamountPay:" + amountpay);
            enInstallment = service.Retrieve(enIns.LogicalName, enIns.Id, new ColumnSet(true));
            strMess.AppendLine("2");
            getInterestStartDate();
            objIns.LateDays = getLateDays(dateCalculate);
            //Lãi ước tính = số ngày tre * lai suat * so tien trể = balance
            decimal interestcharge_percent = objIns.InterestPercent / 100 * objIns.LateDays;
            decimal interestSimulation = amountpay * interestcharge_percent;
            return interestSimulation;
        }
        public static decimal SumInterestAM_OE(Guid OEID, DateTime dateCalculate, decimal amountpay)
        {
            //Tổng lãi đã phát sinh den bsd_ordernumber hiện tại
            int bsd_ordernumber = (int)enInstallment["bsd_ordernumber"];
            decimal sumAmount = 0;
            decimal sumSimulation = 0;
            string fetchXml =
              @"<fetch version='1.0' output-format='xml-platform' mapping='logical' distinct='false'>
                  <entity name='bsd_paymentschemedetail' >
                    <all-attributes/>
                    <filter type='and' >
                      <condition attribute='bsd_optionentry' operator='eq' value='{0}' />
                        <condition attribute='bsd_interestchargeamount' operator='not-null' />
                        <condition attribute='bsd_ordernumber' operator='le' value='{1}' />
                        <condition attribute='statecode' operator='eq' value='0' />
                    </filter>
                    <order attribute='bsd_ordernumber' descending='false' />
                  </entity>
                </fetch>";

            fetchXml = string.Format(fetchXml, OEID, bsd_ordernumber.ToString());
            strMess.AppendLine(fetchXml);
            EntityCollection entc = service.RetrieveMultiple(new FetchExpression(fetchXml));
            //foreach (Entity en_r in entc.Entities)
            for (int i = 0; i < entc.Entities.Count; i++)
            {
                Entity en_r = entc.Entities[i];
                string name = (string)en_r["bsd_name"];
                //Tính tổng lãi đã phát sinh

                //decimal bsd_interestchargeremaining = en_r.Contains("bsd_interestchargeremaining") ? ((Money)en_r["bsd_interestchargeremaining"]).Value : 0;
                decimal bsd_interestchargeremaining = en_r.Contains("bsd_interestchargeamount") ? ((Money)en_r["bsd_interestchargeamount"]).Value : 0;
                sumAmount += bsd_interestchargeremaining;

                //Trace Test ...................
                //throw new Exception(string.Format("bsd_interestchargeamount: {0}" + bsd_interestchargeremaining));
                //Tính tổng lãi ước tính
                if (i != entc.Entities.Count - 1)
                {
                    decimal bsd_balance = en_r.Contains("bsd_balance") ? ((Money)en_r["bsd_balance"]).Value : 0;
                    if (bsd_balance > 0)
                    {
                        decimal interestSimulation = getInterestSimulation(en_r, dateCalculate, bsd_balance);
                        sumSimulation += interestSimulation;
                    }

                }

            }
            strMess.AppendLine("Tong Lãi phát sinh: " + sumAmount.ToString());
            strMess.AppendLine("Tong Lãi ước tinh đợt trước: " + sumSimulation.ToString());
            return sumSimulation + sumAmount;


            //EntityCollection result = service.RetrieveMultiple(new FetchExpression(xml.ToString()));
            //foreach (var c in result.Entities)
            //{
            //    AliasedValue aValue = c.Contains("sumInterestAmount") ? (AliasedValue)c["sumInterestAmount"] : null;
            //    if (aValue != null && aValue.Value != null)
            //        sumAmount = ((Money)aValue.Value).Value;
            //    else
            //        sumAmount = 0;

            //    break;
            //    // sumAmount = ((decimal)((AliasedValue)c["sumAmount"]).Value);
            //}
            //return sumAmount;
            //return sumAmount;

        }
        /// <summary>
        /// Return tổng lãi phát sinh, không tính đợt hiện tại
        /// </summary>
        /// <param name="OEID"></param>
        /// <param name="dateCalculate"></param>
        /// <param name="amountpay"></param>
        /// <returns></returns>
        public static decimal SumInterestAM_OE_New(Guid OEID, DateTime dateCalculate, decimal amountpay)
        {
            decimal result = 0m;
            try
            {
                if (enInstallment == null || enInstallment.Contains("bsd_ordernumber"))
                {
                    strMess.AppendLine("Reocord [Installment] is null or Field [bsd_ordernumber] not data, Please check code Func [SumInterestAM_OE_New]");
                }
                int bsd_ordernumber = (int)enInstallment["bsd_ordernumber"];

                #region --- Fetch XML: bsd_paymentschemedetail ---
                string fetchXml = @"<fetch version='1.0' output-format='xml-platform' mapping='logical' distinct='false'>
                                      <entity name='bsd_paymentschemedetail' >
                                        <all-attributes/>
                                        <filter type='and' >
                                          <condition attribute='bsd_optionentry' operator='eq' value='{0}' />
                                            <condition attribute='bsd_interestchargeamount' operator='not-null' />
                                            <condition attribute='bsd_ordernumber' operator='le' value='{1}' />
                                            <condition attribute='statecode' operator='eq' value='0' />
                                        </filter>
                                        <order attribute='bsd_ordernumber' descending='false' />
                                      </entity>
                                    </fetch>";
                fetchXml = string.Format(fetchXml, OEID, bsd_ordernumber.ToString());
                #endregion

                EntityCollection entc = service.RetrieveMultiple(new FetchExpression(fetchXml));
                if (entc.Entities.Any())
                {
                    decimal sumAmount = 0;
                    decimal sumSimulation = 0;
                    int count = 0;
                    foreach (var item in entc.Entities)
                    {
                        count += 1;
                        strMess.AppendLine(string.Format("- Dòng thứ i = {0}", count));
                        if (item.Contains("bsd_interestchargeamount"))
                        {
                            sumAmount += ((Money)item["bsd_interestchargeamount"]).Value;
                            strMess.AppendLine(string.Format("+ lãi phát sinh: {0}", ((Money)item["bsd_interestchargeamount"]).Value));

                            if (count != entc.Entities.Count)
                            {
                                if (item.Contains("bsd_balance") && ((Money)item["bsd_balance"]).Value > 0)
                                {
                                    decimal bsd_balance = ((Money)item["bsd_balance"]).Value;
                                    sumSimulation += getInterestSimulation(item, dateCalculate, bsd_balance);
                                    strMess.AppendLine(string.Format("+ lãi ước tính đợt trước: {0}", getInterestSimulation(item, dateCalculate, bsd_balance)));
                                }
                            }
                        }
                    }
                    result = sumSimulation + sumAmount;

                    #region --- Trace debug ---
                    strMess.AppendLine(string.Format("Tổng lãi phát sinh: {0}", sumAmount));
                    strMess.AppendLine(string.Format("Tổng lãi ước tính đợt trước: {0}", sumSimulation));
                    strMess.AppendLine(string.Format("result: {0}", result));
                    #endregion
                }
                else
                {
                    strMess.AppendLine("Fetch bsd_paymentschemedetail is not data!");
                }
            }
            catch (Exception ex)
            {
                result = 0;
                throw new Exception(string.Format(strMess.ToString()));
            }
            return result;
        }
        private static bool check_Data_Setup()
        {
            try
            {
                EntityReference OE_ref = enInstallment.Contains("bsd_optionentry") ? (EntityReference)enInstallment["bsd_optionentry"] : null;
                Entity OE = enInstallment.Contains("bsd_optionentry") ? service.Retrieve(OE_ref.LogicalName, OE_ref.Id, new ColumnSet(true)) : null;
                if (OE == null) return false;
                EntityReference paymentscheme_ref = OE.Contains("bsd_paymentscheme") ? (EntityReference)OE["bsd_paymentscheme"] : null;
                Entity PaymentScheme = service.Retrieve(paymentscheme_ref.LogicalName, paymentscheme_ref.Id, new ColumnSet(true));
                EntityReference interestratemaster_ref = PaymentScheme.Contains("bsd_interestratemaster") ? (EntityReference)PaymentScheme["bsd_interestratemaster"] : null;
                if (interestratemaster_ref != null)
                {
                    Entity InterestrateMaster = service.Retrieve(interestratemaster_ref.LogicalName, interestratemaster_ref.Id, new ColumnSet(true));
                    if (!InterestrateMaster.Contains("bsd_termsinterestpercentage") && !InterestrateMaster.Contains("bsd_toleranceinterestamount"))
                    {
                        strMess.AppendLine(string.Format("Chưa setup {tỷ lệ / số tiền} tính lãi cho CAP"));
                        return true;
                    }
                }
                return false;
            }
            catch (Exception)
            {
                strMess.AppendLine(string.Format("sys exception ..."));
                return false;
            }

        }
        private static string format_Money(decimal money)
        {
            string result = string.Format("{0:#,##0.00}", money);
            return result;
        }
        public static EntityCollection get_ec_bsd_dailyinterestrate(Guid projID)
        {
            string fetchXml =
              @"<fetch version='1.0' output-format='xml-platform' mapping='logical' distinct='false'>
                  <entity name='bsd_dailyinterestrate' >
                    <attribute name='bsd_date' />
                    <attribute name='bsd_project' />
                    <attribute name='bsd_interestrate' />
                    <attribute name='createdon' />
                    <filter type='and' >
                      <condition attribute='bsd_project' operator='eq' value='{0}' />
                        <condition attribute='statuscode' operator='eq' value='1' />
                    </filter>
                    <order attribute='createdon' descending='true' />
                  </entity>
                </fetch>";

            fetchXml = string.Format(fetchXml, projID);
            EntityCollection entc = service.RetrieveMultiple(new FetchExpression(fetchXml));
            return entc;
        }

        private static void Calculate_Interest(string installmentid, string stramountpay, string receiptdateimport, ref int lateDays, ref decimal interestMasterPercent)
        {
            decimal amountpay = Convert.ToDecimal(stramountpay);

            DateTime receiptdate = Convert.ToDateTime(receiptdateimport);

            receiptdate = RetrieveLocalTimeFromUTCTime(receiptdate);
            enInstallment = service.Retrieve("bsd_paymentschemedetail", new Guid(installmentid), new ColumnSet(true));
            strMess.AppendLine("2");
            getInterestStartDate();
            objIns.LateDays = getLateDays(receiptdate);
            lateDays = objIns.LateDays;
            objIns.InterestCharge = calc_InterestCharge(receiptdate, amountpay, ref interestMasterPercent);
        }
        #endregion
    }
    public class Installment
    {
        public DateTime InterestStarDate { get; set; }
        public int Intereststartdatetype { get; set; }
        public int Gracedays { get; set; }
        public int LateDays { get; set; }
        public int orderNumber { get; set; }
        public Guid idOE { get; set; }
        public decimal MaxPercent { get; set; }
        public decimal MaxAmount { get; set; }
        public decimal InterestPercent { get; set; }
        public decimal InterestCharge { get; set; }
        public DateTime Duedate { get; set; }
    }
}