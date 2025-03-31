using Microsoft.Crm.Sdk.Messages;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Messages;
using Microsoft.Xrm.Sdk.Query;
using System;
using System.Linq;
using System.Runtime.ConstrainedExecution;
using System.Text;

namespace Action_AgingSimulation_Calculation
{
    public class Action_AgingSimulation_Calculation : IPlugin
    {
        public static IOrganizationService service = null;
        static IOrganizationServiceFactory factory = null;
        public static ITracingService traceService = null;
        static StringBuilder strMess = new StringBuilder();
        static StringBuilder strMess2 = new StringBuilder();
        //public static Installment objIns = new Installment();
        //public static Entity enInstallment;
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
                Entity enTarget = new Entity("bsd_interestsimulation");
                enTarget.Id = Guid.Parse(input02);
                enTarget["bsd_powerautomate"] = true;
                service.Update(enTarget);
                context.OutputParameters["output01"] = context.UserId.ToString();
                string url = "";
                EntityCollection configGolive = RetrieveMultiRecord(service, "bsd_configgolive",
                    new ColumnSet(new string[] { "bsd_url" }), "bsd_name", "Aging Simulation Calculation");
                foreach (Entity item in configGolive.Entities)
                {
                    if (item.Contains("bsd_url")) url = (string)item["bsd_url"];
                }
                if (url == "") throw new InvalidPluginExecutionException("Link to run PA not found. Please check again.");
                context.OutputParameters["output02"] = url;
            }
            else if (input01 == "Bước 03" && input02 != "" && input03 != "" && input04 != "")
            {
                traceService.Trace("Bước 03");
                service = factory.CreateOrganizationService(Guid.Parse(input04));
                EntityCollection optionEntrys = getOptionEntrys(service, input03);
                foreach (Entity optionEntry in optionEntrys.Entities)
                {
                    Entity agInSimOption = new Entity("bsd_aginginterestsimulationoption");
                    agInSimOption["bsd_name"] = optionEntry.Attributes["name"].ToString();
                    agInSimOption["bsd_aginginterestsimulation"] = new EntityReference("bsd_interestsimulation", Guid.Parse(input02));
                    agInSimOption["bsd_optionentry"] = optionEntry.ToEntityReference();
                    service.Create(agInSimOption);
                }
            }
            else if (input01 == "Bước 05" && input02 != "" && input03 != "" && input04 != "")
            {
                traceService.Trace("Bước 05");
                service = factory.CreateOrganizationService(Guid.Parse(input04));
                var fetchXml = $@"
                            <fetch>
                              <entity name='bsd_aginginterestsimulationoption'>
                                <all-attributes />
                                <filter type='and'>
                                  <condition attribute='bsd_aginginterestsimulationoptionid' operator='eq' value='{input03}'/>
                                </filter>
                              </entity>
                            </fetch>";
                EntityCollection lstInterestSimulationOption = service.RetrieveMultiple(new FetchExpression(fetchXml));
                foreach (var InterestOption in lstInterestSimulationOption.Entities)
                {
                    CreateAgingDetail(InterestOption);
                }
            }
            else if (input01 == "Bước 06" && input02 != "" && input04 != "")
            {
                traceService.Trace("Bước 06");
                service = factory.CreateOrganizationService(Guid.Parse(input04));
                Entity enConfirmPayment = new Entity("bsd_interestsimulation");
                enConfirmPayment.Id = Guid.Parse(input02);
                enConfirmPayment["bsd_powerautomate"] = false;
                enConfirmPayment["bsd_errorincalculation"] = "";
                service.Update(enConfirmPayment);
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
        private void CreateAgingDetail(Entity InterestOption)
        {
            #region Code tạo detail
            var optinentryid = ((EntityReference)InterestOption["bsd_optionentry"]).Id.ToString();
            var aginginterestsimulationoptionid = InterestOption.Id.ToString();

            var simulationoptions = service.Retrieve("bsd_aginginterestsimulationoption", new Guid(aginginterestsimulationoptionid), new ColumnSet(true));

            Entity enOptionEntry1 = service.Retrieve("salesorder", new Guid(optinentryid), new ColumnSet(true));
            int bsd_type1 = 100000002;
            bsd_type1 = 100000001;
            Entity enInterestSimulation = service.Retrieve("bsd_interestsimulation", ((EntityReference)simulationoptions["bsd_aginginterestsimulation"]).Id, new ColumnSet(true));
            DateTime dateofinterestcalculation = new DateTime();
            DateTime simulationDate = RetrieveLocalTimeFromUTCTime((DateTime)enInterestSimulation["bsd_simulationdate"]);
            if (enInterestSimulation.Contains("bsd_dateofinterestcalculation"))
            {
                dateofinterestcalculation = RetrieveLocalTimeFromUTCTime((DateTime)enInterestSimulation["bsd_dateofinterestcalculation"]);
            }
            else
                dateofinterestcalculation = simulationDate;
            bsd_type1 = ((OptionSetValue)enInterestSimulation["bsd_type"]).Value;
            //DELETE RECORDS OLD
            QueryExpression q1 = new QueryExpression("bsd_paymentschemedetail");
            q1.ColumnSet = new ColumnSet(true);
            switch (bsd_type1)
            {
                case 100000000://Aging Report
                               // Thêm điều kiện Main
                    FilterExpression filter_Main = new FilterExpression(LogicalOperator.Or);

                    FilterExpression filter_notpaid = new FilterExpression(LogicalOperator.And);
                    filter_notpaid.AddCondition(new ConditionExpression("bsd_duedate", ConditionOperator.NotNull));
                    filter_notpaid.AddCondition(new ConditionExpression("bsd_duedate", ConditionOperator.OnOrBefore, dateofinterestcalculation));
                    filter_notpaid.AddCondition(new ConditionExpression("statecode", ConditionOperator.Equal, 0));
                    filter_notpaid.AddCondition(new ConditionExpression("statuscode", ConditionOperator.Equal, 100000000));
                    filter_notpaid.AddCondition(new ConditionExpression("bsd_optionentry", ConditionOperator.Equal, enOptionEntry1.Id));
                    filter_Main.AddFilter(filter_notpaid);

                    FilterExpression filter_paid = new FilterExpression(LogicalOperator.And);
                    filter_paid.AddCondition(new ConditionExpression("bsd_duedate", ConditionOperator.NotNull));
                    filter_paid.AddCondition(new ConditionExpression("bsd_duedate", ConditionOperator.OnOrBefore, dateofinterestcalculation));
                    filter_paid.AddCondition(new ConditionExpression("statecode", ConditionOperator.Equal, 0));
                    filter_paid.AddCondition(new ConditionExpression("statuscode", ConditionOperator.Equal, 100000001));
                    filter_paid.AddCondition(new ConditionExpression("bsd_optionentry", ConditionOperator.Equal, enOptionEntry1.Id));

                    FilterExpression filter_interestcharge_notPaid = new FilterExpression(LogicalOperator.And);
                    filter_interestcharge_notPaid.AddCondition(new ConditionExpression("bsd_interestchargestatus", ConditionOperator.Equal, 100000000));
                    filter_interestcharge_notPaid.AddCondition(new ConditionExpression("bsd_interestchargeamount", ConditionOperator.GreaterThan, 0));

                    filter_paid.AddFilter(filter_interestcharge_notPaid);

                    filter_Main.AddFilter(filter_notpaid);
                    filter_Main.AddFilter(filter_paid);
                    q1.Criteria = filter_Main;

                    break;
                case 100000001://Interest Simulation
                    q1.Criteria = new FilterExpression(LogicalOperator.And);
                    q1.Criteria.AddCondition(new ConditionExpression("bsd_optionentry", ConditionOperator.Equal, enOptionEntry1.Id));
                    break;
                default:
                    q1.Criteria = new FilterExpression(LogicalOperator.And);
                    q1.Criteria.AddCondition(new ConditionExpression("bsd_optionentry", ConditionOperator.Equal, enOptionEntry1.Id));
                    break;
            }
            var listInstallment = service.RetrieveMultiple(q1);
            Entity enUint112 = service.Retrieve(((EntityReference)enOptionEntry1["bsd_unitnumber"]).LogicalName, ((EntityReference)enOptionEntry1["bsd_unitnumber"]).Id, new ColumnSet(true));

            if (listInstallment.Entities.Count > 0)
            {
                decimal interestProjectDaily = 0;
                #region CREATE INTEREST SIMULATION DETAIL
                foreach (Entity ins in listInstallment.Entities)
                {
                    //Cập nhật thêm trường thông tin Aging/ Interest Simulation Option khi tạo Aging/ Interest Simulation Detail"
                    createAgingInterestSimulationDetail(enOptionEntry1, enUint112, enInterestSimulation, ins, simulationoptions, simulationDate, dateofinterestcalculation, interestProjectDaily, bsd_type1);
                }
                #endregion
            }
            updateNewInterestAmount(enOptionEntry1, null, bsd_type1);
            updateAdvantPayment(enOptionEntry1, aginginterestsimulationoptionid);
            #endregion
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
        private static void createAgingInterestSimulationDetail(Entity oe, Entity UnitsEn, Entity enInterestSimulation, Entity ins, Entity simulationoptions, DateTime simulationDate, DateTime dateCalculate, decimal interestProjectDaily, int SimuType)
        {
            try
            {
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
                    Installment objIns = new Installment();
                    //Gọi tính trễ

                    Calculate_Interest(ins.Id.ToString(), interest_NotPaid.ToString(), dateCalculate.ToString("MM/dd/yyyy"), objIns, ref lateDays);
                    intereststartdatetype = objIns.Intereststartdatetype;
                    interestMasterPercent = objIns.InterestPercent;
                    InterestStarDate = objIns.InterestStarDate;
                    traceService.Trace("ra Calculate_Interest: " + lateDays);
                    decInterestCharge = objIns.InterestCharge;
                    Entity interestMaster = service.Retrieve(((EntityReference)payScheme["bsd_interestratemaster"]).LogicalName, ((EntityReference)payScheme["bsd_interestratemaster"]).Id,
                        new ColumnSet(new string[]
                        {
                                "bsd_intereststartdatetype",
                                "bsd_gracedays",
                                "bsd_termsinterestpercentage"
                        }));
                    if (!(interestMaster.Contains("bsd_intereststartdatetype") && interestMaster.Contains("bsd_gracedays") && interestMaster.Contains("bsd_termsinterestpercentage")))
                        throw new InvalidPluginExecutionException("Interest charge master not enough infomation required. Please check again!");
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
                    ISDetail["bsd_aginginterestsimulationoption"] = simulationoptions != null ? simulationoptions.ToEntityReference() : null;
                    ISDetail["bsd_installment"] = ins.ToEntityReference();
                    ISDetail["bsd_installmentamount"] = ins.Contains("bsd_amountofthisphase") ? ins["bsd_amountofthisphase"] : new Money(0);
                    ISDetail["bsd_paidamount"] = ins.Contains("bsd_amountwaspaid") ? ins["bsd_amountwaspaid"] : new Money(0);
                    ISDetail["bsd_outstandingamount"] = ins.Contains("bsd_balance") ? ins["bsd_balance"] : new Money(0);
                    ISDetail["bsd_numberofdaysdue"] = 0;
                    // Create Detail
                    interest = interest_New + interest_NotPaid;
                    #region -- type = Interest Simulation = 100000001
                    DateTime duedate = new DateTime();
                    decimal bsd_interestchargeamount = 0;
                    bool bolCheckPaid = false;
                    bolCheckPaid = (((OptionSetValue)ins["statuscode"]).Value == 100000001 && ins.Contains("bsd_interestchargestatus") && ((OptionSetValue)ins["bsd_interestchargestatus"]).Value == 100000000);
                    if (interest_NotPaid > 0 || ((OptionSetValue)ins["statuscode"]).Value == 100000000 || bolCheckPaid)
                    {
                        if (ins.Contains("bsd_duedate"))
                        {
                            duedate = RetrieveLocalTimeFromUTCTime((DateTime)ins["bsd_duedate"]);
                            ISDetail["bsd_duedate"] = duedate;
                            DateTime InterestStarDate1 = InterestStarDate;
                            ISDetail["bsd_intereststartdate"] = InterestStarDate1;
                            bsd_numberofdaysdue = (int)simulationDate.Date.Subtract(duedate.Date).TotalDays;
                            if (InterestStarDate1 < dateCalculate)
                            {
                                balance = ins.Contains("bsd_balance") ? ((Money)ins["bsd_balance"]).Value : decimal.Zero;
                                decimal returnValue = CalculateNewInterest(balance, lateDays, interestPercent);
                                traceService.Trace("ra CalculateNewInterest: " + lateDays);
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
                            traceService.Trace("gán lateDays: " + lateDays);
                            ISDetail["bsd_outstandingday"] = lateDays;
                            ISDetail["bsd_paymentscheme"] = payScheme.ToEntityReference();
                            ISDetail["bsd_interestpercent"] = interestPercent;
                            ISDetail["bsd_groupaging"] = new OptionSetValue(CheckGroupAging(lateDays));
                            ISDetail["bsd_interestamountinstallment"] = new Money(decInterestAmountIns);
                            decimal decnewinterestamount = bolCheckPaid ? 0 : interest_New > decInterestCharge ? decInterestCharge : interest_New;
                            ISDetail["bsd_newinterestamount"] = new Money(decnewinterestamount);
                            ISDetail["bsd_interestchargeamount"] = new Money(decnewinterestamount + decInterestAmountIns);
                            bsd_interestchargeamount = interest + interest_New;
                            ISDetail["bsd_advancepayment"] = new Money(0);
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
                        }

                    }
                    #endregion
                    else
                    {
                        if (ins.Contains("bsd_duedate"))
                        {
                            duedate = RetrieveLocalTimeFromUTCTime((DateTime)ins["bsd_duedate"]);
                            ISDetail["bsd_duedate"] = duedate;
                            ISDetail["bsd_outstandingday"] = 0;
                            ISDetail["bsd_paymentscheme"] = payScheme.ToEntityReference();
                            ISDetail["bsd_interestpercent"] = (decimal)0;
                            ISDetail["bsd_interestamountinstallment"] = new Money(0);
                            ISDetail["bsd_newinterestamount"] = new Money(0);
                            ISDetail["bsd_interestchargeamount"] = new Money(0);
                            ISDetail["bsd_advancepayment"] = new Money(0);
                        }
                    }
                    service.Create(ISDetail);
                }
            }
            catch (InvalidPluginExecutionException ex)
            {
                throw new InvalidPluginExecutionException(ex.ToString());
            }

        }
        private EntityCollection getOptionEntrys(IOrganizationService crmservices, string idUnit)
        {
            StringBuilder xml = new StringBuilder();
            xml.AppendLine("<fetch version='1.0' output-format='xml-platform' mapping='logical'>");
            xml.AppendLine("<entity name='salesorder'>");
            xml.AppendLine("<attribute name='name' />");
            xml.AppendLine("<attribute name='totalamount' />");
            xml.AppendLine("<attribute name='statuscode' />");
            xml.AppendLine("<attribute name='customerid' />");
            xml.AppendLine("<attribute name='createdon' />");
            xml.AppendLine("<attribute name='bsd_unitnumber' />");
            xml.AppendLine("<attribute name='bsd_project' />");
            xml.AppendLine("<attribute name='bsd_optionno' />");
            xml.AppendLine("<attribute name='bsd_contractnumber' />");
            xml.AppendLine("<attribute name='bsd_optioncodesams' />");
            xml.AppendLine("<attribute name='salesorderid' />"); ;
            xml.AppendLine("<filter type='and'>");
            xml.AppendLine(string.Format("<condition attribute='bsd_unitnumber' operator='eq' value='{0}'/>", idUnit));
            xml.AppendLine("<condition attribute='statuscode' operator='ne' value='100000006'/>");
            xml.AppendLine("<condition attribute='statuscode' operator='ne' value='100000007'/>");
            xml.AppendLine("</filter>");
            xml.AppendLine("</entity>");
            xml.AppendLine("</fetch>");
            traceService.Trace(xml.ToString());
            EntityCollection entc = service.RetrieveMultiple(new FetchExpression(xml.ToString()));
            return entc;
        }
        private static decimal getInterestCap(Entity enOptionEntry)
        {
            decimal lim = 0;
            decimal totalamount = enOptionEntry.Contains("totalamount") ? ((Money)enOptionEntry["totalamount"]).Value : 0;
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
            }
            return lim;
        }
        private static void updateNewInterestAmount(Entity enOptionEntry, Entity enInterestSimulateOption, int reportype)
        {
            Entity optionEntry = service.Retrieve(enOptionEntry.LogicalName, enOptionEntry.Id, new ColumnSet(true));
            decimal cap = getInterestCap(optionEntry);
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
            //Tính toán New Interest Amount sao cho tổng nhỏ hơn hoặc bằng cap
            //Ưu tiên giảm các đợt cuối trở về trước
            decimal sumInterestAmount = encolInterestSimulationDetail.Entities.AsEnumerable().Sum(x => x.Contains("bsd_interestamountinstallment") ? ((Money)x["bsd_interestamountinstallment"]).Value : 0);
            decimal[] arrNewInterestAmount = { };
            for (int i = 0; i < encolInterestSimulationDetail.Entities.Count; i++)
            {
                Entity enInterestSimulationDetail = encolInterestSimulationDetail.Entities[i];
                decimal bsd_newinterestamount = enInterestSimulationDetail.Contains("bsd_newinterestamount") ? ((Money)enInterestSimulationDetail["bsd_newinterestamount"]).Value : 0;
                decimal bsd_interestamountinstallment = enInterestSimulationDetail.Contains("bsd_interestamountinstallment") ? ((Money)enInterestSimulationDetail["bsd_interestamountinstallment"]).Value : 0;
                if (sumInterestAmount < cap)
                {
                    decimal total = sumInterestAmount + bsd_newinterestamount;
                    if (total > cap && bsd_newinterestamount != 0)
                    {
                        decimal denta = cap - sumInterestAmount;
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

        private static decimal CalculateNewInterest(decimal balance, int lateDays, decimal interestPercent)
        {
            decimal interest = balance * lateDays * interestPercent / 100;
            return interest;
        }
        private static decimal CalculateInterestNotPaid(Entity ins)
        {
            decimal interestamount = ins.Contains("bsd_amountofthisphase") ? ((Money)ins["bsd_amountofthisphase"]).Value : decimal.Zero;
            decimal interestamountpaid = ins.Contains("bsd_amountwaspaid") ? ((Money)ins["bsd_amountwaspaid"]).Value : decimal.Zero;
            decimal waiverinterest = ins.Contains("waiverinstallment") ? ((Money)ins["waiverinstallment"]).Value : decimal.Zero;
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
            return service.RetrieveMultiple(new FetchExpression(xml.ToString()));
        }
        private static void updateAdvantPayment(Entity oe, string aginginterestsimulationoption)
        {
            //Cal Sum Advance Payment
            decimal AdvPayAmt = 0;
            EntityCollection AdvSum = CalSum_AdvancePayment(oe.Id.ToString());
            if (AdvSum.Entities.Count > 0)
            {
                Entity AdvSumEn = AdvSum.Entities[0];
                if (((AliasedValue)AdvSumEn.Attributes["SumAdv"]).Value != null)
                    AdvPayAmt = ((Money)((AliasedValue)AdvSumEn.Attributes["SumAdv"]).Value).Value;
            }
            string condition = "<condition attribute='bsd_aginginterestsimulationoption' operator='null' />";
            if (aginginterestsimulationoption != "")
            {
                condition = "<condition attribute='bsd_aginginterestsimulationoption' operator='eq' value='" + aginginterestsimulationoption + "'/>";
            }
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
            EntityCollection encoInterestSimulationDetail = service.RetrieveMultiple(new FetchExpression(xml));
            if (encoInterestSimulationDetail.Entities.Count > 0)
            {
                Entity enInterestSimulationDetail = new Entity("bsd_interestsimulationdetail", encoInterestSimulationDetail.Entities[0].Id);
                enInterestSimulationDetail["bsd_advancepayment"] = new Money(AdvPayAmt);
                service.Update(enInterestSimulationDetail);
            }
        }
        private static void getInterestStartDate(Entity enInstallment, Installment objIns)
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
                }
            }
            catch (InvalidPluginExecutionException ex)
            {
                throw new InvalidPluginExecutionException(ex.ToString());
            }


        }
        public static int getLateDays(DateTime dateCalculate, Installment objIns)
        {
            try
            {
                traceService.Trace("dateCalculate: " + dateCalculate);
                traceService.Trace("Duedate: " + objIns.Duedate);
                int lateDays = (int)dateCalculate.Date.Subtract(objIns.Duedate.Date).TotalDays;

                int orderNumberSightContract = getViTriDotSightContract(objIns.idOE);
                Entity oe = service.Retrieve("salesorder", objIns.idOE, new ColumnSet(true));
                int bsd_ordernumber = objIns.orderNumber;
                traceService.Trace("bsd_ordernumber: " + bsd_ordernumber);
                traceService.Trace("orderNumberSightContract: " + orderNumberSightContract);
                int numberOfDays2 = 0;
                if (orderNumberSightContract != -1)
                {
                    if (orderNumberSightContract <= bsd_ordernumber && oe.Contains("bsd_signedcontractdate"))
                    {
                        numberOfDays2 = -100599;
                    }
                    else if (orderNumberSightContract > bsd_ordernumber && oe.Contains("bsd_signeddadate"))
                    {
                        DateTime bsd_signeddadate = RetrieveLocalTimeFromUTCTime((DateTime)oe["bsd_signeddadate"]);
                        TimeSpan difference2 = dateCalculate - bsd_signeddadate;
                        numberOfDays2 = difference2.Days;
                        numberOfDays2 = numberOfDays2 < 0 ? 0 : numberOfDays2;
                        traceService.Trace("bsd_signeddadate: " + numberOfDays2);
                    }
                }
                traceService.Trace("lateDays: " + lateDays);
                traceService.Trace("Intereststartdatetype: " + objIns.Intereststartdatetype);
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
                traceService.Trace("lateDays: " + lateDays);
                objIns.LateDays = lateDays;
                if (lateDays < 0)
                    lateDays = 0;
                return lateDays;
            }
            catch (InvalidPluginExecutionException ex)
            {
                throw new InvalidPluginExecutionException(ex.ToString());
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
        public static decimal calc_InterestCharge(DateTime dateCalculate, decimal amountPay, Entity enInstallment, Installment objIns)
        {
            try
            {
                var result = 0m;
                EntityReference OE_ref = enInstallment.Contains("bsd_optionentry") ? (EntityReference)enInstallment["bsd_optionentry"] : null;
                if (OE_ref == null)
                {
                    throw new Exception(string.Format("Không có dữ liệu record [bsd_optionentry]"));
                }
                #region --- Body Code ---
                if (OE_ref != null)
                {
                    decimal interestcharge_amount = 0;
                    Entity OE = service.Retrieve(OE_ref.LogicalName, OE_ref.Id, new ColumnSet(true));
                    if (OE.Contains("bsd_signeddadate") || OE.Contains("bsd_signedcontractdate"))
                    {
                        Entity Project = service.Retrieve("bsd_project", ((EntityReference)OE["bsd_project"]).Id, new ColumnSet(new string[] { "bsd_name", "bsd_dailyinterestchargebank" }));
                        bool bsd_dailyinterestchargebank = Project.Contains("bsd_dailyinterestchargebank") ? (bool)Project["bsd_dailyinterestchargebank"] : false;
                        decimal d_dailyinterest = 0;
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
                        objIns.InterestPercent = (objIns.InterestPercent + d_dailyinterest);
                        decimal interestcharge_percent = objIns.InterestPercent / 100 * objIns.LateDays;
                        interestcharge_amount = Convert.ToDecimal(amountPay) * interestcharge_percent;
                        decimal sum_bsd_waiverinterest = sumWaiverInterest(OE);
                        decimal sum_Inr_AM = SumInterestAM_OE_New(OE.Id, dateCalculate, amountPay, enInstallment, objIns) - sum_bsd_waiverinterest;
                        decimal sum_temp = sum_Inr_AM + interestcharge_amount;
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
                        #endregion

                        #region --- @@ Nghiệp vụ tính tiền lãi ---
                        /* ------------------------------------------------------------------------------------------
                         * Khái niệm: 
                         * - Tổng tiền trễ tất cả các đợt. bao gồm đợt hiện tại             : sum_temp
                         * - Cap : số tiền chạm móc đầu tiên ()                             : cap    
                         * - Tổng tiền trễ không tính đợt hiện tại - sum(waiverinterest)    : sum_Inr_AM
                         * - Tiền trễ đợt hiện tại                                          : interestcharge_amount
                         --------------------------------------------------------------------------------------------*/
                        if (cap <= 0)
                        {
                            var rs = check_Data_Setup(enInstallment);
                            if (rs)
                            {
                                result = interestcharge_amount;
                            }
                            else
                            {
                                result = 0m;
                            }
                        }
                        else if (sum_temp > cap)
                        {
                            if (cap > sum_Inr_AM)
                            {
                                result = cap - sum_Inr_AM;
                            }
                            else
                            {
                                result = 0m;
                            }

                        }
                        else if (sum_temp == cap)
                        {
                            if (sum_Inr_AM < cap)
                                result = cap - sum_Inr_AM;
                            else
                                result = 0m;
                        }
                        else if (sum_temp < cap)
                        {
                            result = interestcharge_amount;
                        }
                        else
                        {
                            result = interestcharge_amount;
                        }
                        #endregion
                    }
                    else result = 0;
                }
                #endregion
                return result;

            }
            catch (Exception ex)
            {
                throw new InvalidPluginExecutionException(ex.ToString());
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
            decimal sum = 0;
            foreach (Entity en in encolInstallment.Entities)
            {
                decimal bsd_waiverinterest = en.Contains("bsd_waiverinterest") ? ((Money)en["bsd_waiverinterest"]).Value : 0;
                sum += bsd_waiverinterest;
            }
            return sum;
        }
        public static decimal getInterestSimulation(Entity enIns, DateTime dateCalculate, decimal amountpay, Installment objIns)
        {
            Entity enInstallment = service.Retrieve(enIns.LogicalName, enIns.Id, new ColumnSet(true));
            getInterestStartDate(enInstallment, objIns);
            objIns.LateDays = getLateDays(dateCalculate, objIns);
            //Lãi ước tính = số ngày tre * lai suat * so tien trể = balance
            decimal interestcharge_percent = objIns.InterestPercent / 100 * objIns.LateDays;
            decimal interestSimulation = amountpay * interestcharge_percent;
            return interestSimulation;
        }
        /// Return tổng lãi phát sinh, không tính đợt hiện tại
        public static decimal SumInterestAM_OE_New(Guid OEID, DateTime dateCalculate, decimal amountpay, Entity enInstallment, Installment objIns)
        {
            decimal result = 0m;
            try
            {
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
                        if (item.Contains("bsd_interestchargeamount"))
                        {
                            sumAmount += ((Money)item["bsd_interestchargeamount"]).Value;

                            if (count != entc.Entities.Count)
                            {
                                if (item.Contains("bsd_balance") && ((Money)item["bsd_balance"]).Value > 0)
                                {
                                    decimal bsd_balance = ((Money)item["bsd_balance"]).Value;
                                    sumSimulation += getInterestSimulation(item, dateCalculate, bsd_balance, objIns);
                                }
                            }
                        }
                    }
                    result = sumSimulation + sumAmount;
                }
                return result;
            }
            catch (Exception ex)
            {
                result = 0;
                return result;
                throw new Exception(string.Format(ex.ToString()));
            }

        }
        private static bool check_Data_Setup(Entity enInstallment)
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
                    return true;
                }
            }
            return false;
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
        private static void Calculate_Interest(string installmentid, string stramountpay, string receiptdateimport, Installment objIns, ref int lateDays)
        {
            decimal amountpay = Convert.ToDecimal(stramountpay);

            DateTime receiptdate = Convert.ToDateTime(receiptdateimport);

            receiptdate = RetrieveLocalTimeFromUTCTime(receiptdate);
            Entity enInstallment = service.Retrieve("bsd_paymentschemedetail", new Guid(installmentid), new ColumnSet(true));
            getInterestStartDate(enInstallment, objIns);
            objIns.LateDays = getLateDays(receiptdate, objIns);
            lateDays = objIns.LateDays;
            traceService.Trace("Calculate_Interest lateDays: " + objIns.LateDays);
            objIns.InterestCharge = calc_InterestCharge(receiptdate, amountpay, enInstallment, objIns);
        }
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