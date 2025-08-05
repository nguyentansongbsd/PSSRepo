using Microsoft.Crm.Sdk.Messages;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Query;
using System;
using System.Collections.Generic;
using System.Text;

namespace Action_CustomerNotices_GenerateCustomerNotices
{
    public class Action_CustomerNotices_Generate : IPlugin
    {
        public static IOrganizationService service = null;
        static IOrganizationServiceFactory factory = null;
        ITracingService traceService = null;
        void IPlugin.Execute(IServiceProvider serviceProvider)
        {
            IPluginExecutionContext context = (IPluginExecutionContext)serviceProvider.GetService(typeof(IPluginExecutionContext));
            traceService = (ITracingService)serviceProvider.GetService(typeof(ITracingService));
            factory = (IOrganizationServiceFactory)serviceProvider.GetService(typeof(IOrganizationServiceFactory));
            service = factory.CreateOrganizationService(context.UserId);
            string input01 = "";
            if (!string.IsNullOrEmpty((string)context.InputParameters["Units"]))
            {
                input01 = context.InputParameters["Units"].ToString();
            }
            string input02 = "";
            if (!string.IsNullOrEmpty((string)context.InputParameters["Project"]))
            {
                input02 = context.InputParameters["Project"].ToString();
            }
            string input03 = "";
            if (!string.IsNullOrEmpty((string)context.InputParameters["OptionEntryId"]))
            {
                input03 = context.InputParameters["OptionEntryId"].ToString();
            }
            string input04 = "";
            if (!string.IsNullOrEmpty((string)context.InputParameters["Owner"]))
            {
                input04 = context.InputParameters["Owner"].ToString();
            }
            traceService.Trace("vào xử lý " + input01);
            if (input01 == "Bước 01" && input02 != "")
            {
                traceService.Trace("Bước 01");
                Entity enUp = new Entity("bsd_genpaymentnotices");
                enUp.Id = Guid.Parse(input02);
                enUp["bsd_powerautomate"] = true;
                service.Update(enUp);
                string url = "";
                EntityCollection configGolive = RetrieveMultiRecord(service, "bsd_configgolive",
                    new ColumnSet(new string[] { "bsd_url" }), "bsd_name", "GenPaymentNotices");
                foreach (Entity item in configGolive.Entities)
                {
                    if (item.Contains("bsd_url")) url = (string)item["bsd_url"];
                }
                if (url == "") throw new InvalidPluginExecutionException("Link to run PA not found. Please check again.");
                context.OutputParameters["Count"] = url;

                Entity enTarget = service.Retrieve(enUp.LogicalName, enUp.Id, new ColumnSet(true));
                //LAY DANH SACH CAC OE HOP LE
                var fetchXml = $@"<?xml version=""1.0"" encoding=""utf-16""?>
                    <fetch>
                      <entity name=""salesorder"">
                        <attribute name=""salesorderid"" />
                        <filter>
                          <condition attribute=""statuscode"" operator=""ne"" value=""{100000006}"" />
                          <condition attribute=""totalamount"" operator=""gt"" value=""{0}"" />
                          <condition attribute=""bsd_paymentscheme"" operator=""not-null"" />
                          <condition attribute=""customerid"" operator=""not-null"" />
                          <condition attribute=""bsd_tobeterminated"" operator=""ne"" value=""1"" />
                        </filter>
                        <link-entity name=""product"" from=""productid"" to=""bsd_unitnumber"">
                          <filter>
                            <condition attribute=""bsd_projectcode"" operator=""eq"" value=""{((EntityReference)enTarget["bsd_project"]).Id}"" />
                          </filter>
                        </link-entity>
                      </entity>
                    </fetch>";
                if (enTarget.Contains("bsd_units"))
                {
                    fetchXml = $@"<?xml version=""1.0"" encoding=""utf-16""?>
                    <fetch>
                      <entity name=""salesorder"">
                        <attribute name=""salesorderid"" />
                        <filter>
                          <condition attribute=""statuscode"" operator=""ne"" value=""{100000006}"" />
                          <condition attribute=""totalamount"" operator=""gt"" value=""{0}"" />
                          <condition attribute=""bsd_paymentscheme"" operator=""not-null"" />
                          <condition attribute=""customerid"" operator=""not-null"" />
                        </filter>
                        <link-entity name=""product"" from=""productid"" to=""bsd_unitnumber"">
                          <filter>
                            <condition attribute=""bsd_projectcode"" operator=""eq"" value=""{((EntityReference)enTarget["bsd_project"]).Id}"" />
                            <condition attribute=""productid"" operator=""eq"" value=""{((EntityReference)enTarget["bsd_units"]).Id}"" />
                          </filter>
                        </link-entity>
                      </entity>
                    </fetch>";
                }
                else if (enTarget.Contains("bsd_floor"))
                {
                    fetchXml = $@"<?xml version=""1.0"" encoding=""utf-16""?>
                    <fetch>
                      <entity name=""salesorder"">
                        <attribute name=""salesorderid"" />
                        <filter>
                          <condition attribute=""statuscode"" operator=""ne"" value=""{100000006}"" />
                          <condition attribute=""totalamount"" operator=""gt"" value=""{0}"" />
                          <condition attribute=""bsd_paymentscheme"" operator=""not-null"" />
                          <condition attribute=""customerid"" operator=""not-null"" />
                        </filter>
                        <link-entity name=""product"" from=""productid"" to=""bsd_unitnumber"">
                          <filter>
                            <condition attribute=""bsd_projectcode"" operator=""eq"" value=""{((EntityReference)enTarget["bsd_project"]).Id}"" />
                            <condition attribute=""bsd_floor"" operator=""eq"" value=""{((EntityReference)enTarget["bsd_floor"]).Id}"" />
                          </filter>
                        </link-entity>
                      </entity>
                    </fetch>";
                }
                else if (enTarget.Contains("bsd_block"))
                {
                    fetchXml = $@"<?xml version=""1.0"" encoding=""utf-16""?>
                    <fetch>
                      <entity name=""salesorder"">
                        <attribute name=""salesorderid"" />
                        <filter>
                          <condition attribute=""statuscode"" operator=""ne"" value=""{100000006}"" />
                          <condition attribute=""totalamount"" operator=""gt"" value=""{0}"" />
                          <condition attribute=""bsd_paymentscheme"" operator=""not-null"" />
                          <condition attribute=""customerid"" operator=""not-null"" />
                        </filter>
                        <link-entity name=""product"" from=""productid"" to=""bsd_unitnumber"">
                          <filter>
                            <condition attribute=""bsd_projectcode"" operator=""eq"" value=""{((EntityReference)enTarget["bsd_project"]).Id}"" />
                            <condition attribute=""bsd_blocknumber"" operator=""eq"" value=""{((EntityReference)enTarget["bsd_block"]).Id}"" />
                          </filter>
                        </link-entity>
                      </entity>
                    </fetch>";
                }
                EntityCollection list = service.RetrieveMultiple(new FetchExpression(fetchXml));
                traceService.Trace("count " + list.Entities.Count);
                traceService.Trace("url " + url);
                List<string> listOE = new List<string>();
                foreach (Entity detail in list.Entities)
                {
                    listOE.Add(detail.Id.ToString());
                }
                if (listOE.Count == 0)
                    throw new InvalidPluginExecutionException("The list is empty. Please check again.");
                context.OutputParameters["ReturnId"] = string.Join(";", listOE);
            }
            else if (input01 == "Bước 02" && input02 != "" && input03 != "" && input04 != "")
            {
                traceService.Trace("Bước 02");
                service = factory.CreateOrganizationService(Guid.Parse(input04));
                Entity enTarget = service.Retrieve("bsd_genpaymentnotices", Guid.Parse(input02), new ColumnSet(true));
                string date = "";
                if (enTarget.Contains("bsd_date"))
                {
                    date = RetrieveLocalTimeFromUTCTime((DateTime)enTarget["bsd_date"], service).ToString();
                }
                Entity OE = service.Retrieve("salesorder", Guid.Parse(input03), new ColumnSet(new string[] { "salesorderid", "bsd_paymentscheme",
                "bsd_project","name","customerid"}));

                EntityCollection l_PS = findPaymentScheme(service, (EntityReference)OE["bsd_paymentscheme"]);
                int PN_Date;
                if (l_PS.Entities.Count > 0)//bsd_paymentnoticesdate đã khác null
                {
                    PN_Date = (int)l_PS[0]["bsd_paymentnoticesdate"];
                    traceService.Trace("PN_Date: " + PN_Date);
                    EntityCollection l_PSD = findPaymentSchemeDetail(service, OE);
                    #region Customernotices
                    foreach (Entity PSD in l_PSD.Entities)
                    {
                        Entity PSDetail = service.Retrieve(PSD.LogicalName, PSD.Id, new ColumnSet(true));
                        #region check bsd_miscellaneous nếu là đợt cuối nếu notpaid thì check bsd_miscellaneous 
                        traceService.Trace("PSD[\"bsd_lastinstallment\"] " + ((bool)PSD["bsd_lastinstallment"]).ToString());
                        if (PSD.Contains("bsd_lastinstallment") && ((bool)PSD["bsd_lastinstallment"]) == true && ((OptionSetValue)PSD["statuscode"]).Value == 100000001)
                        {
                            var query_bsd_installment = PSD.Id.ToString();
                            var query = new QueryExpression("bsd_miscellaneous");
                            query.ColumnSet.AllColumns = true;
                            query.Criteria.AddCondition("bsd_installment", ConditionOperator.Equal, query_bsd_installment);
                            query.Criteria.AddCondition("statuscode", ConditionOperator.Equal, 1);
                            var resmiscellaneous = service.RetrieveMultiple(query);
                            if (resmiscellaneous.Entities == null || resmiscellaneous.Entities.Count == 0)
                                continue;
                        }
                        #endregion
                        traceService.Trace("step1");
                        decimal bsd_percentforcustomer = PSDetail.Contains("bsd_percentforcustomer") ? (decimal)PSDetail["bsd_percentforcustomer"] : 0;
                        decimal bsd_percentforbank = PSDetail.Contains("bsd_percentforbank") ? (decimal)PSDetail["bsd_percentforbank"] : 0;
                        decimal bsd_amountofthisphase = PSDetail.Contains("bsd_amountofthisphase") ? ((Money)PSDetail["bsd_amountofthisphase"]).Value : 0;
                        decimal bsd_amountforcustomer = bsd_amountofthisphase * bsd_percentforcustomer / 100;
                        decimal bsd_amountforbank = bsd_amountofthisphase * bsd_percentforbank / 100;
                        if (PSD.Contains("bsd_duedate") == false) continue;
                        int nday = (int)(((DateTime)PSD["bsd_duedate"]).AddHours(7).Date.Subtract(DateTime.Now.AddHours(7).Date).TotalDays);
                        traceService.Trace("nday");
                        #region Custom
                        if (nday >= 0 && nday <= PN_Date)
                        {
                            EntityCollection l_CustomerNotices_byIns = findCustomerNoticesByIntallment(service, PSD.ToEntityReference());
                            if (l_CustomerNotices_byIns.Entities.Count == 0)
                            {
                                Entity customerNotices = new Entity("bsd_customernotices");
                                if (OE.Contains("name"))
                                    customerNotices["bsd_name"] = "Payment Notices of " + OE["name"];
                                else
                                    customerNotices["bsd_name"] = "Payment Notices";
                                customerNotices["bsd_subject"] = "Payment Notices";

                                if (OE.Contains("bsd_project"))
                                    customerNotices["bsd_project"] = OE["bsd_project"];
                                customerNotices["bsd_customer"] = OE["customerid"];
                                customerNotices["bsd_date"] = !string.IsNullOrWhiteSpace(date) ? Convert.ToDateTime(date) : RetrieveLocalTimeFromUTCTime(DateTime.Now, service); //RetrieveLocalTimeFromUTCTime(DateTime.Now, service);
                                customerNotices["bsd_optionentry"] = OE.ToEntityReference();
                                customerNotices["bsd_paymentschemedetail"] = PSD.ToEntityReference();
                                EntityCollection list_orderproduct = RetrieveMultiRecord2(service, "salesorderdetail", new ColumnSet(new string[] { "productid" }), "salesorderid", OE.Id);
                                if (list_orderproduct.Entities.Count > 0)
                                {
                                    Entity orderPro = list_orderproduct.Entities[0];
                                    customerNotices["bsd_units"] = orderPro["productid"];
                                }
                                #region xử lý OdernumberE
                                if (((int)PSDetail["bsd_ordernumber"]) == 1)
                                {
                                    customerNotices["bsd_odernumber_e"] = "1st";
                                }
                                else if (((int)PSDetail["bsd_ordernumber"]) == 2)
                                {
                                    customerNotices["bsd_odernumber_e"] = "2nd";
                                }
                                else if (((int)PSDetail["bsd_ordernumber"]) == 3)
                                {
                                    customerNotices["bsd_odernumber_e"] = "3rd";

                                }
                                else
                                {
                                    customerNotices["bsd_odernumber_e"] = $"{((int)PSDetail["bsd_ordernumber"])}th";
                                }
                                #endregion
                                customerNotices["bsd_genpaymentnotices"] = enTarget.ToEntityReference();
                                Guid id = service.Create(customerNotices);
                                Entity ins = new Entity(PSD.LogicalName);
                                ins.Id = PSD.Id;
                                ins["bsd_paymentnotices"] = true;
                                service.Update(ins);

                                generateWarningNoticesByPaymentNotices(id, date, enTarget.ToEntityReference());
                            }
                        }
                        #endregion
                    }
                    #endregion
                }
            }
            else if (input01 == "Bước 03" && input02 != "" && input04 != "")
            {
                traceService.Trace("Bước 03");
                service = factory.CreateOrganizationService(Guid.Parse(input04));
                Entity enUp = new Entity("bsd_genpaymentnotices");
                enUp.Id = Guid.Parse(input02);
                enUp["bsd_powerautomate"] = false;
                string fetchXml =
                  @"<fetch top='1'>
                  <entity name='bsd_customernotices'>
                    <attribute name='bsd_customernoticesid' />
                    <filter type='and'>
                      <condition attribute='bsd_genpaymentnotices' operator='eq' value='{0}' />
                    </filter>
                  </entity>
                </fetch>";
                fetchXml = string.Format(fetchXml, enUp.Id);
                EntityCollection entc = service.RetrieveMultiple(new FetchExpression(fetchXml));
                if (entc.Entities.Count == 0)
                {
                    enUp["bsd_notices"] = "No record was created. Please check again.";
                }
                else
                {
                    enUp["statuscode"] = new OptionSetValue(100000000);
                    enUp["bsd_notices"] = "";
                }
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
        EntityCollection RetrieveMultiRecord2(IOrganizationService crmservices, string entity, ColumnSet column, string condition, object value)
        {
            QueryExpression q = new QueryExpression(entity);
            q.ColumnSet = column;
            q.Criteria = new FilterExpression();
            q.Criteria.AddCondition(new ConditionExpression(condition, ConditionOperator.Equal, value));
            EntityCollection entc = service.RetrieveMultiple(q);

            return entc;
        }
        private EntityCollection findPaymentSchemeDetail(IOrganizationService crmservices, Entity oe)
        {
            string fetchXml =
              @"<fetch version='1.0' output-format='xml-platform' mapping='logical' distinct='false'>
                  <entity name='bsd_paymentschemedetail'>
                    <attribute name='bsd_paymentschemedetailid' />
                    <attribute name='bsd_duedate' />
                    <attribute name='bsd_lastinstallment' />
                    <attribute name='statuscode' />
                    <order attribute='bsd_duedate' descending='true' />
                    <filter type='or'>
                        <filter type='and'>
                            <condition attribute='bsd_optionentry' operator='eq'  uitype='salesorder' value='{0}' />
                            <condition attribute='statuscode' operator='eq' value='100000000' />
                            <condition attribute='bsd_duedate' operator='not-null' />
                        </filter>
                        <filter type='and'>
                            <condition attribute='bsd_optionentry' operator='eq'  uitype='salesorder' value='{0}' />
                            <condition attribute='bsd_lastinstallment' operator='eq' value='1' />

                        </filter>
                    </filter>
                  </entity>
                </fetch>";
            fetchXml = string.Format(fetchXml, oe.Id);
            EntityCollection entc = crmservices.RetrieveMultiple(new FetchExpression(fetchXml));
            return entc;
        }
        private EntityCollection findCustomerNoticesByIntallment(IOrganizationService crmservices, EntityReference ins)
        {
            string fetchXml =
              @"<fetch version='1.0' output-format='xml-platform' count='1' mapping='logical' distinct='false'>
                  <entity name='bsd_customernotices'>
                    <attribute name='bsd_customernoticesid' />
                    <filter type='and'>
                      <condition attribute='bsd_paymentschemedetail' operator='eq' uitype='bsd_paymentschemedetail' value='{0}' />
                    </filter>   
                  </entity>
                </fetch>";
            fetchXml = string.Format(fetchXml, ins.Id);
            EntityCollection entc = crmservices.RetrieveMultiple(new FetchExpression(fetchXml));
            return entc;
        }
        private EntityCollection findPaymentScheme(IOrganizationService crmservices, EntityReference ps)
        {
            string fetchXml =
              @"<fetch version='1.0' output-format='xml-platform' count='1' mapping='logical' distinct='false'>
              <entity name='bsd_paymentscheme'>
                <attribute name='bsd_paymentschemeid' />
                <attribute name='bsd_paymentnoticesdate' />
                <order attribute='bsd_paymentnoticesdate' descending='false' />
                <filter type='and'>
                  <condition attribute='bsd_paymentschemeid' operator='eq' uitype='bsd_paymentscheme' value='{0}' />
                  <condition attribute='bsd_paymentnoticesdate' operator='not-null' />
                </filter>
              </entity>
            </fetch>";
            fetchXml = string.Format(fetchXml, ps.Id);
            EntityCollection entc = crmservices.RetrieveMultiple(new FetchExpression(fetchXml));
            return entc;
        }
        private DateTime RetrieveLocalTimeFromUTCTime(DateTime utcTime, IOrganizationService service)
        {

            int? timeZoneCode = RetrieveCurrentUsersSettings(service);


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
        private int? RetrieveCurrentUsersSettings(IOrganizationService service)
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

            return (int?)currentUserSettings.Attributes["timezonecode"];
        }
        private void generateWarningNoticesByPaymentNotices(Guid paymentNoticesId, string date, EntityReference erfEntity)
        {
            traceService.Trace("vào gen WN");
            // Lấy lên tất cả payment notices
            var QEbsd_customernotices = new QueryExpression("bsd_customernotices");
            QEbsd_customernotices.ColumnSet.AllColumns = true;
            QEbsd_customernotices.Criteria.AddCondition("bsd_customernoticesid", ConditionOperator.Equal, paymentNoticesId);
            EntityCollection enPaymentNotices = service.RetrieveMultiple(QEbsd_customernotices);
            // Lặp qua payment notices
            foreach (Entity en in enPaymentNotices.Entities)
            {
                // lấy ra optionentry
                EntityReference enrfOptionEntry = en.Contains("bsd_optionentry") ? (EntityReference)en["bsd_optionentry"] : null;
                Entity enOptionEntry = service.Retrieve(enrfOptionEntry.LogicalName, enrfOptionEntry.Id, new ColumnSet(true));
                EntityReference enrfPaymentScheme = enOptionEntry.Contains("bsd_paymentscheme") ? (EntityReference)enOptionEntry["bsd_paymentscheme"] : null;
                Entity enPaymentScheme = service.Retrieve(enrfPaymentScheme.LogicalName, enrfPaymentScheme.Id, new ColumnSet(true));
                int PN_Date = -1;
                Entity PS = enPaymentScheme;
                if (PS.Contains("bsd_warningnotices1date") || PS.Contains("bsd_warningnotices2date") || PS.Contains("bsd_warningnotices3date") || PS.Contains("bsd_warningnotices4date"))
                {
                    // Lấy tất cả Installment thuộc optionentry
                    EntityCollection enclInstallment = getAllInstallmentByOptionEntry(enOptionEntry);
                    foreach (Entity PSD in enclInstallment.Entities)
                    {
                        int nday = (int)(DateTime.Now.AddHours(7).Date.Subtract(((DateTime)PSD["bsd_duedate"]).AddHours(7).Date).TotalDays);
                        if (nday > 0)
                        {
                            EntityCollection L_warning = findWarningNotices(PSD);
                            #region Da Generate WN
                            if (L_warning.Entities.Count > 0)
                            {
                                Entity warning = L_warning[0];
                                int numberofWarning = warning.Contains("bsd_numberofwarning") ? (int)warning["bsd_numberofwarning"] : -1;
                                if (numberofWarning > 0 && numberofWarning < 4)
                                {
                                    //numberofWarning == 1 ? "bsd_warningnotices2date" : (numberofWarning == 2 ? "bsd_warningnotices3date" : (numberofWarning == 3 ? "bsd_warningnotices4date" : null));
                                    string warningdate = "bsd_warningnotices" + (numberofWarning + 1) + "date";
                                    if (PS.Contains(warningdate) && nday >= (int)PS[warningdate])
                                    {
                                        EntityCollection l_war = findWarningNoticesByNumberOfWarning(PSD, enrfOptionEntry);//(numberofWarning + 1)
                                        if (l_war.Entities.Count == 0)
                                        {
                                            Entity warningNotices = new Entity("bsd_warningnotices");
                                            if (enOptionEntry.Contains("name"))
                                                warningNotices["bsd_name"] = "Warning Notices of " + enOptionEntry["name"];
                                            else warningNotices["bsd_name"] = "Warning Notices";
                                            warningNotices["bsd_subject"] = "Warning Notices";
                                            //warningNotices["bsd_paymentnotice"] = en.ToEntityReference();
                                            warningNotices["bsd_optionentry"] = new EntityReference(enOptionEntry.LogicalName, enOptionEntry.Id);
                                            if (enOptionEntry.Contains("customerid"))
                                                warningNotices["bsd_customer"] = enOptionEntry["customerid"];
                                            if (enOptionEntry.Contains("bsd_project"))
                                                warningNotices["bsd_project"] = enOptionEntry["bsd_project"];
                                            if (enOptionEntry.Contains("bsd_unitnumber"))
                                                warningNotices["bsd_units"] = enOptionEntry["bsd_unitnumber"];
                                            warningNotices["bsd_numberofwarning"] = numberofWarning + 1;
                                            warningNotices["bsd_type"] = new OptionSetValue(100000000);
                                            if (PSD.Contains("bsd_balance"))
                                            {
                                                decimal amount = PSD.Contains("bsd_balance") ? ((Money)PSD["bsd_balance"]).Value : 0;
                                                warningNotices["bsd_amount"] = new Money(amount);
                                            }
                                            warningNotices["bsd_date"] = !string.IsNullOrWhiteSpace(date) ? Convert.ToDateTime(date) : RetrieveLocalTimeFromUTCTime(DateTime.Now, service);
                                            warningNotices["bsd_paymentschemedeitail"] = PSD.ToEntityReference();
                                            if (PSD.Contains("bsd_duedate"))
                                            {
                                                warningNotices["bsd_duedate"] = ((DateTime)PSD["bsd_duedate"]);
                                                int graceday = findGraceDays(PS.ToEntityReference());
                                                if (graceday != -1)
                                                    warningNotices["bsd_estimateduedate"] = ((DateTime)PSD["bsd_duedate"]).AddDays(graceday);
                                            }
                                            #region bsd_deadlinewn1-bsd_deadlinewn2
                                            if (PSD.Contains("bsd_duedate"))
                                            {

                                                if (PSD.Contains("bsd_gracedays"))
                                                {
                                                    traceService.Trace("1@@@");
                                                    warningNotices["bsd_deadlinewn1"] = ((DateTime)PSD["bsd_duedate"]).AddHours(7).AddDays((int)PSD["bsd_gracedays"]);
                                                }
                                                else
                                                {
                                                    var bsd_paymentscheme = service.Retrieve("bsd_paymentscheme", ((EntityReference)PSD["bsd_paymentscheme"]).Id, new ColumnSet("bsd_interestratemaster"));
                                                    var bsd_interestratemaster = service.Retrieve("bsd_interestratemaster", ((EntityReference)bsd_paymentscheme["bsd_interestratemaster"]).Id, new ColumnSet("bsd_gracedays"));
                                                    warningNotices["bsd_deadlinewn1"] = ((DateTime)PSD["bsd_duedate"]).AddHours(7).AddDays((int)bsd_interestratemaster["bsd_gracedays"]);
                                                }
                                                warningNotices["bsd_deadlinewn2"] = ((DateTime)PSD["bsd_duedate"]).AddDays(60).AddHours(7);
                                            }

                                            #endregion
                                            #region xử lý OdernumberE
                                            if (((int)PSD["bsd_ordernumber"]) == 1)
                                            {
                                                warningNotices["bsd_odernumber_e"] = "1st";
                                            }
                                            else if (((int)PSD["bsd_ordernumber"]) == 2)
                                            {
                                                warningNotices["bsd_odernumber_e"] = "2nd";
                                            }
                                            else if (((int)PSD["bsd_ordernumber"]) == 3)
                                            {
                                                warningNotices["bsd_odernumber_e"] = "3rd";

                                            }
                                            else
                                            {
                                                warningNotices["bsd_odernumber_e"] = $"{((int)PSD["bsd_ordernumber"])}th";
                                            }
                                            #endregion
                                            warningNotices["bsd_genpaymentnotices"] = erfEntity;
                                            service.Create(warningNotices);
                                            Entity ins = new Entity(PSD.LogicalName);
                                            ins.Id = PSD.Id;
                                            string field = "bsd_warningnotices" + (numberofWarning + 1);
                                            ins[field] = true;

                                            traceService.Trace("step1");
                                            string field2 = "bsd_warningdate" + (numberofWarning + 1);
                                            ins[field2] = ((DateTime)warningNotices["bsd_date"]).AddHours(7);
                                            traceService.Trace("step2");

                                            string field3 = "bsd_w_noticesnumber" + (numberofWarning + 1);
                                            ins[field3] = warningNotices["bsd_noticesnumber"];
                                            traceService.Trace("step3");
                                            service.Update(ins);
                                        }
                                    }
                                }
                            }
                            #endregion
                            #region Chua Generate WN
                            else
                            {
                                PN_Date = PS.Contains("bsd_warningnotices1date") ? (int)PS["bsd_warningnotices1date"] : -1;
                                if (PN_Date >= 0 && nday >= PN_Date)
                                {
                                    Entity warningNotices = new Entity("bsd_warningnotices");
                                    if (enOptionEntry.Contains("name"))
                                        warningNotices["bsd_name"] = "Warning Notices of " + enOptionEntry["name"];
                                    else warningNotices["bsd_name"] = "Warning Notices";
                                    warningNotices["bsd_subject"] = "Warning Notices";
                                    warningNotices["bsd_optionentry"] = enOptionEntry.ToEntityReference();
                                    //warningNotices["bsd_paymentnotice"] = en.ToEntityReference();
                                    if (enOptionEntry.Contains("customerid"))
                                        warningNotices["bsd_customer"] = enOptionEntry["customerid"];
                                    if (enOptionEntry.Contains("bsd_project"))
                                        warningNotices["bsd_project"] = enOptionEntry["bsd_project"];
                                    if (enOptionEntry.Contains("bsd_unitnumber"))
                                        warningNotices["bsd_units"] = enOptionEntry["bsd_unitnumber"];
                                    warningNotices["bsd_numberofwarning"] = 1;
                                    warningNotices["bsd_type"] = new OptionSetValue(100000000);
                                    if (PSD.Contains("bsd_balance"))
                                    {
                                        decimal amount = PSD.Contains("bsd_balance") ? ((Money)PSD["bsd_balance"]).Value : 0;
                                        warningNotices["bsd_amount"] = new Money(amount);
                                    }
                                    warningNotices["bsd_date"] = !string.IsNullOrWhiteSpace(date) ? Convert.ToDateTime(date) : RetrieveLocalTimeFromUTCTime(DateTime.Now, service);
                                    warningNotices["bsd_paymentschemedeitail"] = PSD.ToEntityReference();
                                    if (PSD.Contains("bsd_duedate"))
                                    {
                                        warningNotices["bsd_duedate"] = ((DateTime)PSD["bsd_duedate"]);
                                        int graceday = findGraceDays(PS.ToEntityReference());
                                        if (graceday != -1)
                                            warningNotices["bsd_estimateduedate"] = ((DateTime)PSD["bsd_duedate"]).AddDays(graceday);
                                    }
                                    #region bsd_deadlinewn1-bsd_deadlinewn2
                                    if (PSD.Contains("bsd_duedate"))
                                    {

                                        if (PSD.Contains("bsd_gracedays"))
                                        {
                                            traceService.Trace("1@@@");
                                            warningNotices["bsd_deadlinewn1"] = ((DateTime)PSD["bsd_duedate"]).AddHours(7).AddDays((int)PSD["bsd_gracedays"]);
                                        }
                                        else
                                        {
                                            var bsd_paymentscheme = service.Retrieve("bsd_paymentscheme", ((EntityReference)PSD["bsd_paymentscheme"]).Id, new ColumnSet("bsd_interestratemaster"));
                                            var bsd_interestratemaster = service.Retrieve("bsd_interestratemaster", ((EntityReference)bsd_paymentscheme["bsd_interestratemaster"]).Id, new ColumnSet("bsd_gracedays"));
                                            warningNotices["bsd_deadlinewn1"] = ((DateTime)PSD["bsd_duedate"]).AddHours(7).AddDays((int)bsd_interestratemaster["bsd_gracedays"]);
                                        }
                                        warningNotices["bsd_deadlinewn2"] = ((DateTime)PSD["bsd_duedate"]).AddDays(60);
                                    }

                                    #endregion
                                    #region xử lý OdernumberE
                                    if (((int)PSD["bsd_ordernumber"]) == 1)
                                    {
                                        warningNotices["bsd_odernumber_e"] = "1st";
                                    }
                                    else if (((int)PSD["bsd_ordernumber"]) == 2)
                                    {
                                        warningNotices["bsd_odernumber_e"] = "2nd";
                                    }
                                    else if (((int)PSD["bsd_ordernumber"]) == 3)
                                    {
                                        warningNotices["bsd_odernumber_e"] = "3rd";

                                    }
                                    else
                                    {
                                        warningNotices["bsd_odernumber_e"] = $"{((int)PSD["bsd_ordernumber"])}th";
                                    }
                                    #endregion
                                    warningNotices["bsd_genpaymentnotices"] = erfEntity;
                                    service.Create(warningNotices);

                                    Entity ins = new Entity(PSD.LogicalName);
                                    ins.Id = PSD.Id;
                                    string field = "bsd_warningnotices1";
                                    ins[field] = true;
                                    service.Update(ins);
                                }
                            }
                            #endregion
                        }

                    }
                }
            }
        }
        private EntityCollection getAllInstallmentByOptionEntry(Entity enOptionEntry)
        {
            var QEbsd_paymentschemedetail_statuscode = 100000000;
            var QEbsd_paymentschemedetail = new QueryExpression("bsd_paymentschemedetail");
            QEbsd_paymentschemedetail.ColumnSet.AllColumns = true;
            QEbsd_paymentschemedetail.AddOrder("bsd_duedate", OrderType.Ascending);
            QEbsd_paymentschemedetail.Criteria.AddCondition("bsd_optionentry", ConditionOperator.Equal, enOptionEntry.Id);
            QEbsd_paymentschemedetail.Criteria.AddCondition("statuscode", ConditionOperator.Equal, QEbsd_paymentschemedetail_statuscode);
            QEbsd_paymentschemedetail.Criteria.AddCondition("bsd_duedate", ConditionOperator.NotNull);
            EntityCollection encl = service.RetrieveMultiple(QEbsd_paymentschemedetail);
            return encl;

        }
        private EntityCollection findWarningNotices(Entity enInstallment)
        {
            var QEbsd_warningnotices = new QueryExpression("bsd_warningnotices");
            QEbsd_warningnotices.ColumnSet.AllColumns = true;
            QEbsd_warningnotices.AddOrder("bsd_numberofwarning", OrderType.Descending);
            QEbsd_warningnotices.Criteria.AddCondition("bsd_paymentschemedeitail", ConditionOperator.Equal, enInstallment.Id);
            EntityCollection encl = service.RetrieveMultiple(QEbsd_warningnotices);
            return encl;
        }
        private EntityCollection findWarningNoticesByNumberOfWarning(Entity paymentDet, EntityReference enfOptionEntry)
        {
            var QEbsd_warningnotices = new QueryExpression("bsd_warningnotices");
            QEbsd_warningnotices.ColumnSet.AllColumns = true;
            QEbsd_warningnotices.AddOrder("bsd_numberofwarning", OrderType.Descending);
            QEbsd_warningnotices.Criteria.AddCondition("bsd_paymentschemedeitail", ConditionOperator.Equal, paymentDet.Id);
            QEbsd_warningnotices.Criteria.AddCondition("bsd_optionentry", ConditionOperator.Equal, enfOptionEntry.Id);
            EntityCollection encl = service.RetrieveMultiple(QEbsd_warningnotices);
            return encl;
        }
        private int findGraceDays(EntityReference ps)
        {
            var QEbsd_interestratemaster = new QueryExpression("bsd_interestratemaster");
            QEbsd_interestratemaster.Distinct = true;
            QEbsd_interestratemaster.ColumnSet.AllColumns = true;
            var QEbsd_interestratemaster_bsd_paymentscheme = QEbsd_interestratemaster.AddLink("bsd_paymentscheme", "bsd_interestratemasterid", "bsd_interestratemaster");
            QEbsd_interestratemaster_bsd_paymentscheme.EntityAlias = "ab";
            QEbsd_interestratemaster_bsd_paymentscheme.LinkCriteria.AddCondition("bsd_paymentschemeid", ConditionOperator.Equal, ps.Id);
            EntityCollection encl = service.RetrieveMultiple(QEbsd_interestratemaster);
            if (encl.Entities.Count > 0)
            {
                if (((OptionSetValue)(encl.Entities[0]["bsd_intereststartdatetype"])).Value == 100000001)//type == graceday
                    return (int)encl.Entities[0]["bsd_gracedays"];
                else return 0;
            }
            else return -1;
        }
    }
}
