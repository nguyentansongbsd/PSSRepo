using Microsoft.Crm.Sdk.Messages;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Messages;
using Microsoft.Xrm.Sdk.Query;
using System;
using System.Linq;
using System.Runtime.ConstrainedExecution;
using System.Text;

namespace Action_BulkWaiver_Approve
{
    public class Action_BulkWaiver_Approve : IPlugin
    {
        public static IOrganizationService service = null;
        static IOrganizationServiceFactory factory = null;
        public static ITracingService traceService = null;
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
                Entity enBulkWaiver = service.Retrieve("bsd_bulkwaiver", Guid.Parse(input02), new ColumnSet(true));
                int statuscode = ((OptionSetValue)enBulkWaiver["statuscode"]).Value;
                if (statuscode != 1) throw new InvalidPluginExecutionException("The status of the Bulk Waiver is invalid. Please check again.");
                bool bsd_powerautomate = enBulkWaiver.Contains("bsd_powerautomate") ? (bool)enBulkWaiver["bsd_powerautomate"] : false;
                if (bsd_powerautomate) throw new InvalidPluginExecutionException("The Record Bulk Waiver is running Power Automate. Please check again.");
                Entity enTarget = new Entity(enBulkWaiver.LogicalName, enBulkWaiver.Id);
                enTarget["bsd_powerautomate"] = true;
                service.Update(enTarget);
                EntityCollection list = find(input02, 1);
                if (list.Entities.Count == 0) throw new InvalidPluginExecutionException("The list of waiver to be processed is currently empty. Please check again.");
                context.OutputParameters["output01"] = context.UserId.ToString();
                string url = "";
                EntityCollection configGolive = RetrieveMultiRecord(service, "bsd_configgolive",
                    new ColumnSet(new string[] { "bsd_url" }), "bsd_name", "Bulk Waiver Approve");
                foreach (Entity item in configGolive.Entities)
                {
                    if (item.Contains("bsd_url")) url = (string)item["bsd_url"];
                }
                if (url == "") throw new InvalidPluginExecutionException("Link to run PA not found. Please check again.");
                context.OutputParameters["output02"] = url;
            }
            else if (input01 == "Bước 02" && input02 != "" && input03 != "" && input04 != "")
            {
                traceService.Trace("Bước 02");
                service = factory.CreateOrganizationService(Guid.Parse(input04));
                Entity enBulkWaiver = service.Retrieve("bsd_bulkwaiver", Guid.Parse(input02), new ColumnSet(true));
                Entity e = service.Retrieve("bsd_bulkwaiverdetail", Guid.Parse(input03), new ColumnSet(true));
                //KIEM TRA DU LIEU
                if (!e.Contains("bsd_installment"))
                    throw new InvalidPluginExecutionException("The condition to approve this form is not to let field Installment in Waiver Approve Detail: " + e["bsd_name"].ToString() + " empty.");
                if (!e.Contains("bsd_waiveramount"))
                    throw new InvalidPluginExecutionException("The condition to approve this form is not to let field Waiver amount in Waiver Approve Detail: " + e["bsd_name"].ToString() + " empty.");
                //SO SANH
                decimal Wdetail_waiver = ((Money)e["bsd_waiveramount"]).Value;
                //Get Info Installment
                Entity installment = service.Retrieve(((EntityReference)e["bsd_installment"]).LogicalName, ((EntityReference)e["bsd_installment"]).Id,
                    new ColumnSet(new string[]
                    {
                                        "bsd_waiveramount",
                                        "bsd_waiverinstallment",
                                        "bsd_waiverinterest",
                                        "bsd_amountofthisphase",
                                        "statuscode",
                                        "bsd_ordernumber",
                                        "bsd_optionentry",
                                        "bsd_name",
                                        "bsd_depositamount",
                                        "bsd_amountwaspaid",
                                        "bsd_interestchargeamount",
                                        "bsd_interestwaspaid",

                                        "bsd_managementamount",
                                        "bsd_managementfeepaid",
                                        "bsd_managementfeewaiver",

                                        "bsd_maintenanceamount",
                                        "bsd_maintenancefeepaid",
                                        "bsd_maintenancefeewaiver"
                    }));
                decimal amountOTP = installment.Contains("bsd_amountofthisphase") ? ((Money)installment["bsd_amountofthisphase"]).Value : 0;
                decimal paid = installment.Contains("bsd_amountwaspaid") ? ((Money)installment["bsd_amountwaspaid"]).Value : 0;
                decimal deposit = installment.Contains("bsd_depositamount") ? ((Money)installment["bsd_depositamount"]).Value : 0;
                decimal ins_waiveramount = installment.Contains("bsd_waiveramount") ? ((Money)installment["bsd_waiveramount"]).Value : 0;
                decimal waiverinterest = installment.Contains("bsd_waiverinterest") ? ((Money)installment["bsd_waiverinterest"]).Value : 0;
                decimal waiverinstallment = installment.Contains("bsd_waiverinstallment") ? ((Money)installment["bsd_waiverinstallment"]).Value : 0;

                decimal managementamount = installment.Contains("bsd_managementamount") ? ((Money)installment["bsd_managementamount"]).Value : 0;
                decimal managementfeepaid = installment.Contains("bsd_managementfeepaid") ? ((Money)installment["bsd_managementfeepaid"]).Value : 0;
                decimal managementfeewaiver = installment.Contains("bsd_managementfeewaiver") ? ((Money)installment["bsd_managementfeewaiver"]).Value : 0;

                decimal maintenanceamount = installment.Contains("bsd_maintenanceamount") ? ((Money)installment["bsd_maintenanceamount"]).Value : 0;
                decimal maintenancefeepaid = installment.Contains("bsd_maintenancefeepaid") ? ((Money)installment["bsd_maintenancefeepaid"]).Value : 0;
                decimal maintenancefeewaiver = installment.Contains("bsd_maintenancefeewaiver") ? ((Money)installment["bsd_maintenancefeewaiver"]).Value : 0;

                if (e.Contains("bsd_waivertype"))
                {
                    Entity ins = new Entity(installment.LogicalName);
                    decimal interestamount, interestpaid, balace_Interest, balance;
                    switch (((OptionSetValue)e["bsd_waivertype"]).Value)
                    {
                        case 100000000://type=installment
                            #region installment
                            balance = (amountOTP - paid - deposit - waiverinstallment);
                            if (Wdetail_waiver > balance)
                                throw new InvalidPluginExecutionException("Cannot set Waiver Amount bigger than the balance in Installment: " + installment["bsd_name"]);
                            waiverinstallment = waiverinstallment + Wdetail_waiver;
                            ins_waiveramount = waiverinstallment + waiverinterest;
                            //CAP NHAT
                            ins.Id = installment.Id;
                            ins["bsd_waiveramount"] = new Money(ins_waiveramount);
                            ins["bsd_waiverinstallment"] = new Money(waiverinstallment);
                            balance = (amountOTP - paid - deposit - waiverinstallment);
                            ins["bsd_balance"] = new Money(balance);
                            if (balance == 0)
                            {
                                ins["statuscode"] = new OptionSetValue(100000001);//--> PAID
                                if (installment.Contains("bsd_ordernumber") && (int)installment["bsd_ordernumber"] == 1 && installment.Contains("bsd_optionentry"))
                                {
                                    //cap nhat option entry va units :
                                    Entity oe = new Entity(((EntityReference)installment["bsd_optionentry"]).LogicalName);
                                    oe.Id = ((EntityReference)installment["bsd_optionentry"]).Id;
                                    oe["statuscode"] = new OptionSetValue(100000001);//first installment
                                    service.Update(oe);

                                    Entity option = service.Retrieve(((EntityReference)installment["bsd_optionentry"]).LogicalName, ((EntityReference)installment["bsd_optionentry"]).Id, new ColumnSet(new string[]
                                    { "bsd_unitnumber" }));
                                    Entity units = new Entity(((EntityReference)option["bsd_unitnumber"]).LogicalName);
                                    units.Id = ((EntityReference)option["bsd_unitnumber"]).Id;
                                    units["statuscode"] = new OptionSetValue(100000001);//first installment
                                    service.Update(units);
                                }
                                interestamount = installment.Contains("bsd_interestchargeamount") ? ((Money)installment["bsd_interestchargeamount"]).Value : 0;
                                interestpaid = installment.Contains("bsd_interestwaspaid") ? ((Money)installment["bsd_interestwaspaid"]).Value : 0;
                                balace_Interest = interestamount - interestpaid - waiverinterest;
                                if (waiverinterest > 0 && balace_Interest == 0)
                                    ins["bsd_interestchargestatus"] = new OptionSetValue(100000001);//--> PAID
                            }
                            service.Update(ins);
                            #endregion
                            break;
                        case 100000001://type=interest
                            #region interest
                            interestamount = installment.Contains("bsd_interestchargeamount") ? ((Money)installment["bsd_interestchargeamount"]).Value : 0;
                            interestpaid = installment.Contains("bsd_interestwaspaid") ? ((Money)installment["bsd_interestwaspaid"]).Value : 0;
                            balace_Interest = interestamount - interestpaid - waiverinterest;
                            if (Wdetail_waiver > balace_Interest)
                                throw new InvalidPluginExecutionException("Cannot set Waiver Amount bigger than the balance interest in Installment: " + installment["bsd_name"] + ".");
                            waiverinterest = waiverinterest + Wdetail_waiver;
                            ins_waiveramount = waiverinstallment + waiverinterest;
                            //CAP NHAT
                            ins.Id = installment.Id;
                            ins["bsd_waiveramount"] = new Money(ins_waiveramount);
                            ins["bsd_waiverinterest"] = new Money(waiverinterest);
                            balance = (amountOTP - paid - deposit - waiverinstallment);
                            ins["bsd_balance"] = new Money(balance);
                            balace_Interest = interestamount - interestpaid - waiverinterest;
                            if (waiverinterest > 0 && balace_Interest == 0 && ((OptionSetValue)installment["statuscode"]).Value == 100000001)// ==PAID
                                ins["bsd_interestchargestatus"] = new OptionSetValue(100000001);//--> PAID
                            service.Update(ins);
                            #endregion
                            break;
                        case 100000002://type=Management Fee
                            #region Management Fee
                            ins.Id = installment.Id;
                            //Cập nhật thêm số tiền Management Fee(Waiver) trong Installment tương ứng
                            decimal balancemanagement = managementamount - managementfeepaid - managementfeewaiver - Wdetail_waiver;
                            if (balancemanagement == 0)
                            {
                                ins["bsd_managementfeesstatus"] = true;
                            }
                            else
                            {
                                if (balancemanagement < 0)
                                {
                                    throw new InvalidPluginExecutionException("Cannot set Waiver Amount bigger than the balance in Management fees: " + installment["bsd_name"]);
                                }
                                ins["bsd_managementfeesstatus"] = false;
                            }
                            ins["bsd_managementfeewaiver"] = new Money(Wdetail_waiver + managementfeewaiver);
                            service.Update(ins);
                            #endregion
                            break;
                        case 100000003://type=Maintenance Fee
                            #region Maintenance Fee
                            ins.Id = installment.Id;
                            decimal balancemaintence = maintenanceamount - maintenancefeepaid - maintenancefeewaiver - Wdetail_waiver;
                            if (balancemaintence == 0)
                            {
                                ins["bsd_maintenancefeesstatus"] = true;
                            }
                            else
                            {
                                if (balancemaintence < 0)
                                {
                                    throw new InvalidPluginExecutionException("Cannot set Waiver Amount bigger than the balance in Maintenance fees: " + installment["bsd_name"]);
                                }
                                ins["bsd_maintenancefeesstatus"] = false;
                            }
                            ins["bsd_maintenancefeewaiver"] = new Money(Wdetail_waiver + maintenancefeewaiver);
                            service.Update(ins);
                            #endregion
                            break;
                    }
                }
                Entity bulkwaiverdetail = new Entity(e.LogicalName);
                bulkwaiverdetail.Id = e.Id;
                bulkwaiverdetail["statuscode"] = new OptionSetValue(100000000);//waiver detail: statuscode -> approve 100000000
                service.Update(bulkwaiverdetail);
                Entity enInstallment = service.Retrieve(installment.LogicalName, installment.Id, new ColumnSet(true));
                decimal bsd_waiverinstallment = enInstallment.Contains("bsd_waiverinstallment") ? ((Money)enInstallment["bsd_waiverinstallment"]).Value : 0;
                decimal bsd_waiverinterest = enInstallment.Contains("bsd_waiverinterest") ? ((Money)enInstallment["bsd_waiverinterest"]).Value : 0;
                decimal bsd_maintenancefeewaiver = enInstallment.Contains("bsd_maintenancefeewaiver") ? ((Money)enInstallment["bsd_maintenancefeewaiver"]).Value : 0;
                decimal bsd_managementfeewaiver = enInstallment.Contains("bsd_managementfeewaiver") ? ((Money)enInstallment["bsd_managementfeewaiver"]).Value : 0;
                decimal bsd_waiveramount = bsd_waiverinstallment + bsd_waiverinterest + bsd_maintenancefeewaiver + bsd_managementfeewaiver;
                Entity enInstall = new Entity(enInstallment.LogicalName, enInstallment.Id);
                enInstall["bsd_waiveramount"] = new Money(bsd_waiveramount);
                service.Update(enInstall);

                int psd_statuscodeInterest = enInstallment.Contains("bsd_interestchargestatus") ? ((OptionSetValue)enInstallment["bsd_interestchargestatus"]).Value : 100000000;//check interest đã thanh toán chưa
                bool psd_statuscodeFeeMain = enInstallment.Contains("bsd_maintenancefeesstatus") ? ((bool)enInstallment["bsd_maintenancefeesstatus"]) : false;//check fee đã thanh toán chưa
                bool psd_statuscodeFeeMana = enInstallment.Contains("bsd_managementfeesstatus") ? ((bool)enInstallment["bsd_managementfeesstatus"]) : false;//check fee đã thanh toán chưa
                int psd_statuscode = enInstallment.Contains("statuscode") ? ((OptionSetValue)enInstallment["statuscode"]).Value : 100000000;
                int phaseNum = enInstallment.Contains("bsd_ordernumber") ? (int)enInstallment["bsd_ordernumber"] : 1;
                Entity optionentryEn = e.Contains("bsd_optionentry") ? service.Retrieve("salesorder", ((EntityReference)e["bsd_optionentry"]).Id, new ColumnSet(true)) : null;
                if (optionentryEn != null)
                {
                    var enmis = get_All_MIS_NotPaid(optionentryEn.Id.ToString());//dùng để kiểm tra xem có misc nào chưa thanh toán hay không
                    EntityCollection psdFirst = GetPSD(optionentryEn.Id.ToString());
                    Entity detailFirst = psdFirst.Entities[0];
                    int t = psdFirst.Entities.Count;
                    Entity detailLast = psdFirst.Entities[t - 1]; // entity cuoi cung ( phase cuoi cung )
                    string detailLastID = detailLast.Id.ToString();
                    int sttOE = 100000001; // statuscode of OE= 1st installment
                    int sttUnit = 100000001; // statuscode of unit= 1st installment
                    Entity Unit = service.Retrieve("product", ((EntityReference)optionentryEn["bsd_unitnumber"]).Id, new ColumnSet(true));
                    if (phaseNum == 1)
                    {
                        if (optionentryEn.Contains("bsd_signedcontractdate"))
                        {
                            sttOE = 100000002; // sign contract OE
                            sttUnit = 100000002; // unit = sold
                        }
                        else
                        { // khi 1st da Paid roi moi duoc chuyen sang 1st installment else van la option
                            if (detailFirst.Contains("statuscode"))
                            {
                                if (((OptionSetValue)detailFirst["statuscode"]).Value == 100000000) // 1st installment not paid
                                {
                                    sttOE = 100000000; // option
                                    sttUnit = 100000003; // deposit
                                }
                                else
                                {
                                    sttOE = 100000001;//1st
                                    sttUnit = 100000001; // 1st
                                }
                            }
                        }
                    }
                    else
                    {
                        if (!optionentryEn.Contains("bsd_signedcontractdate"))
                        {
                            sttOE = 100000001; // if OE not signcontract - status code still is 1st Installment
                            sttUnit = 100000001; // 1st
                        }
                        else
                        {
                            sttUnit = 100000002;
                            sttOE = 100000003; //Being Payment (khi da sign contract)

                            if ((detailLastID == enInstallment.Id.ToString()) && psd_statuscode == 100000001 && psd_statuscodeInterest == 100000001 && psd_statuscodeFeeMain &&
                                    psd_statuscodeFeeMana && enmis != null && enmis.Entities.Count == 0)
                                sttOE = 100000004; //Complete Payment
                        }
                    }
                    Entity oe_tmp = new Entity(optionentryEn.LogicalName);
                    oe_tmp.Id = optionentryEn.Id;
                    oe_tmp["bsd_unitstatus"] = new OptionSetValue(sttUnit);
                    oe_tmp["statuscode"] = new OptionSetValue(sttOE);
                    service.Update(oe_tmp);
                }

            }
            else if (input01 == "Bước 03" && input02 != "" && input04 != "")
            {
                traceService.Trace("Bước 03");
                service = factory.CreateOrganizationService(Guid.Parse(input04));
                EntityCollection list = find(input02, 100000000);
                Entity enBulkWaiver = new Entity("bsd_bulkwaiver");
                enBulkWaiver.Id = Guid.Parse(input02);
                enBulkWaiver["bsd_powerautomate"] = false;
                if (list.Entities.Count > 0)
                {
                    enBulkWaiver["statuscode"] = new OptionSetValue(100000000);
                    enBulkWaiver["bsd_approvedrejectedperson"] = new EntityReference("systemuser", Guid.Parse(input04));
                    enBulkWaiver["bsd_approvedrejecteddate"] = RetrieveLocalTimeFromUTCTime(DateTime.Now);
                }
                else enBulkWaiver["bsd_error"] = "The detail list is invalid. Please check again.";
                service.Update(enBulkWaiver);
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
        private EntityCollection find(string id, int sts)
        {
            string fetXml = @"<fetch  top='1'>
                      <entity name='bsd_bulkwaiverdetail'>
                        <attribute name='bsd_bulkwaiverdetailid' />
                        <filter type='and'>
                          <condition attribute='bsd_bulkwaiver' operator='eq' value='{0}' />
                          <condition attribute='statecode' operator='eq' value='0' />
                          <condition attribute='statuscode' operator='eq' value='{1}' />
                        </filter>
                      </entity>
                    </fetch>";
            fetXml = string.Format(fetXml, id, sts);
            EntityCollection entc = service.RetrieveMultiple(new FetchExpression(fetXml));
            return entc;
        }
        public EntityCollection get_All_MIS_NotPaid(string oeID)
        {
            string fetchXml =
              @"<fetch version='1.0' output-format='xml-platform' mapping='logical' distinct='true' >
                <entity name='bsd_miscellaneous' >
                <attribute name='bsd_balance' />
                <attribute name='statuscode' />
                <attribute name='bsd_miscellaneousnumber' />
                <attribute name='bsd_units' />
                <attribute name='bsd_optionentry' />
                <attribute name='bsd_miscellaneousid' />
                <attribute name='bsd_amount' />
                <attribute name='bsd_paidamount' />
                <attribute name='bsd_installment' />
                <attribute name='bsd_name' />
                <attribute name='bsd_project' />
                <attribute name='bsd_installmentnumber' />
                <filter type='and' >
                    <condition attribute='bsd_optionentry' operator='eq' value='{0}' />
                    <condition attribute='statecode' operator='eq' value='0' />
                    <condition attribute='statuscode' operator='eq' value='1' />
                </filter>                           
                </entity>
            </fetch>";
            fetchXml = string.Format(fetchXml, oeID);
            EntityCollection entc = service.RetrieveMultiple(new FetchExpression(fetchXml));
            return entc;
        }
        private EntityCollection GetPSD(string OptionEntryID)
        {
            QueryExpression query = new QueryExpression("bsd_paymentschemedetail");
            query.ColumnSet = new ColumnSet(true);
            query.Distinct = true;
            query.Criteria = new FilterExpression();
            query.Criteria.AddCondition("bsd_optionentry", ConditionOperator.Equal, OptionEntryID);
            query.AddOrder("bsd_ordernumber", OrderType.Ascending);
            //query.TopCount = 1;
            EntityCollection psdFirst = service.RetrieveMultiple(query);
            return psdFirst;
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
    }
}