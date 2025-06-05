using System;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Query;
using System.Text;
using Microsoft.Crm.Sdk.Messages;
using System.Collections;

namespace Action_CustomerNotices_GenerateCustomerNotices
{
    public class Action_CustomerNotices_Generate : IPlugin
    {

        IOrganizationService service = null;
        IOrganizationServiceFactory factory = null;
        ITracingService traceService = null;

        private string owner = null;
        void IPlugin.Execute(IServiceProvider serviceProvider)
        {
            IPluginExecutionContext context = (IPluginExecutionContext)serviceProvider.GetService(typeof(IPluginExecutionContext));
            factory = (IOrganizationServiceFactory)serviceProvider.GetService(typeof(IOrganizationServiceFactory));
            service = factory.CreateOrganizationService(context.UserId);
            traceService = (ITracingService)serviceProvider.GetService(typeof(ITracingService));
            traceService.Trace(string.Format("Context Depth {0}", context.Depth));

            string date = "";
            if (context.InputParameters["Date"] != null)
            {
                date = context.InputParameters["Date"].ToString();
            }
            if (context.InputParameters["Owner"] != null)
            {
                owner = context.InputParameters["Owner"].ToString().Replace("{", "").Replace("}", "");
            }
            string OEId = context.InputParameters["OptionEntryId"] != null ? context.InputParameters["OptionEntryId"].ToString() : null;
            if (string.IsNullOrWhiteSpace(OEId)) return;
            Entity OE = this.service.Retrieve("salesorder", Guid.Parse(OEId), new ColumnSet(new string[] { "salesorderid", "bsd_paymentscheme",
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

                            if (!string.IsNullOrWhiteSpace(owner))
                            {
                                traceService.Trace("owner: " + owner);
                                customerNotices["ownerid"] = new EntityReference("systemuser", Guid.Parse(owner));
                                customerNotices["createdby"] = new EntityReference("systemuser", Guid.Parse(owner));
                            }

                            EntityCollection list_orderproduct = RetrieveMultiRecord(service, "salesorderdetail", new ColumnSet(new string[] { "productid" }), "salesorderid", OE.Id);
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

                            Guid id = service.Create(customerNotices);
                            Entity ins = new Entity(PSD.LogicalName);
                            ins.Id = PSD.Id;
                            ins["bsd_paymentnotices"] = true;
                            service.Update(ins);

                            generateWarningNoticesByPaymentNotices(id, date);
                        }
                    }
                    #endregion
                }
                #endregion
            }
        }
        private decimal CompareDate(DateTime date1, DateTime date2)
        {
            string currentTimerZone = TimeZoneInfo.Local.Id;
            DateTime d1 = TimeZoneInfo.ConvertTimeBySystemTimeZoneId(date1, currentTimerZone);
            DateTime d2 = TimeZoneInfo.ConvertTimeBySystemTimeZoneId(date2, currentTimerZone);
            return (decimal)d1.Date.Subtract(d2.Date).TotalDays;
        }
        EntityCollection RetrieveMultiRecord(IOrganizationService crmservices, string entity, ColumnSet column, string condition, object value)
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
        private EntityCollection findCustomerNotices(IOrganizationService crmservices)
        {
            string fetchXml =
              @"<fetch version='1.0' output-format='xml-platform' count='1' mapping='logical' distinct='false'>
              <entity name='bsd_customernotices'>
                <attribute name='bsd_customernoticesid' />
                <attribute name='bsd_name' />
                <attribute name='createdon' />
                <order attribute='bsd_name' descending='false' />
                <filter type='and'>
                  <condition attribute='createdon' operator='today' />
                </filter>
              </entity>
            </fetch>";
            fetchXml = string.Format(fetchXml);
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
        private EntityCollection findOptionEntry(IOrganizationService crmservices, string project, string block, string floor, string units)
        {
            //#region -- Fetch No Condition - Code Tín
            //string fetchXml =
            //  @"<fetch version='1.0' output-format='xml-platform' mapping='logical' distinct='false'>
            //  <entity name='salesorder'>
            //    <attribute name='salesorderid' />
            //    <attribute name='bsd_paymentscheme' />
            //    <attribute name='bsd_project' />
            //    <attribute name='name' />
            //    <attribute name='customerid' />
            //    <order attribute='bsd_paymentscheme' descending='false' />
            //    <filter type='and'>
            //     <condition attribute='statuscode' operator='in'>
            //        <value>100000000</value>
            //        <value>100000001</value>
            //        <value>100000002</value>
            //        <value>100000003</value>
            //        <value>100000005</value>
            //      </condition>
            //      <condition attribute='bsd_paymentscheme' operator='not-null' />
            //      <condition attribute='customerid' operator='not-null' />
            //      <condition attribute='totalamount' operator='gt' value='0' />
            //    </filter>
            //  </entity>
            //</fetch>";
            //#endregion

            //#region -- Fetch Condition 
            StringBuilder sale = new StringBuilder();
            sale.AppendLine("<fetch version='1.0' output-format='xml-platform' mapping='logical' distinct='false'>");
            sale.AppendLine("<entity name='salesorder'>");
            sale.AppendLine("<attribute name='salesorderid'/>");
            sale.AppendLine("<attribute name='bsd_paymentscheme'/>");
            sale.AppendLine("<attribute name='bsd_project' />");
            sale.AppendLine("<attribute name='name'/>");
            sale.AppendLine("<attribute name='customerid'/>");
            sale.AppendLine("<order attribute='bsd_paymentscheme' descending='false'/>");
            sale.AppendLine("<filter type='and'>");
            sale.AppendLine("<condition attribute='statuscode' operator='neq' value='100000006'/>");
            sale.AppendLine("<condition attribute='bsd_paymentscheme' operator='not-null'/>");
            sale.AppendLine("<condition attribute='customerid' operator='not-null'/>");
            sale.AppendLine("<condition attribute='totalamount' operator='gt' value='0'/>");
            sale.AppendLine("</filter>");
            if (project != "")
            {
                sale.AppendLine("<link-entity name='product' from='productid' to='bsd_unitnumber'>");
                sale.AppendLine("<filter type='and'>");
                sale.AppendLine(string.Format("<condition attribute='bsd_projectcode' operator='eq' value='{0}' />", project));
                if (block != "")
                {
                    sale.AppendLine(string.Format("<condition attribute='bsd_blocknumber' operator='eq' value='{0}' />", block));
                }
                if (floor != "")
                {
                    sale.AppendLine(string.Format("<condition attribute='bsd_floor' operator='eq' value='{0}' />", floor));
                }
                if (units != "")
                {
                    sale.AppendLine(string.Format("<condition attribute='productid' operator='eq' value='{0}' />", units));
                }
                sale.AppendLine("</filter>");
                sale.AppendLine("</link-entity>");
            }
            sale.AppendLine("</entity>");
            sale.AppendLine("</fetch>");
            //#endregion
            //#region -- Fetch Condition - Used
            //string fetchXml =
            //@"<fetch version='1.0' output-format='xml-platform' mapping='logical' distinct='false'>
            //  <entity name='salesorder'>
            //    <attribute name='salesorderid' />
            //    <attribute name='bsd_paymentscheme' />
            //    <attribute name='bsd_project' />
            //    <attribute name='name' />
            //    <attribute name='customerid' />
            //    <order attribute='bsd_paymentscheme' descending='false' />
            //    <filter type='and'>
            //      <condition attribute='statuscode' operator='neq' value='100000006' />
            //      <condition attribute='bsd_paymentscheme' operator='not-null' />
            //      <condition attribute='customerid' operator='not-null' />
            //      <condition attribute='totalamount' operator='gt' value='0' />
            //    </filter>
            //  </entity>
            //</fetch>";

            //#endregion
            //fetchXml = string.Format(fetchXml);
            //EntityCollection entc = crmservices.RetrieveMultiple(new FetchExpression(fetchXml));
            //return entc;
            // #endregion
            // fetchXml = string.Format(fetchXml, project, block, floor, units);
            EntityCollection entc = crmservices.RetrieveMultiple(new FetchExpression(sale.ToString()));
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

        private void generateWarningNoticesByPaymentNotices(Guid paymentNoticesId, string date)
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

                                            if (!string.IsNullOrWhiteSpace(owner))
                                            {
                                                traceService.Trace("owner: " + owner);
                                                warningNotices["ownerid"] = new EntityReference("systemuser", Guid.Parse(owner));
                                            }

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

                                    if (!string.IsNullOrWhiteSpace(owner))
                                    {
                                        traceService.Trace("owner: " + owner);
                                        warningNotices["ownerid"] = new EntityReference("systemuser", Guid.Parse(owner));
                                    }

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
