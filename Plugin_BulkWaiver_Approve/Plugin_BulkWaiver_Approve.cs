using System;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Query;
using Microsoft.Crm.Sdk.Messages;
using System.Text;
using System.Web.Script.Serialization;

namespace Plugin_BulkWaiver_Approve
{
    public class Plugin_BulkWaiver_Approve : IPlugin
    {
        IOrganizationService service = null;
        IOrganizationServiceFactory factory = null;
        IPluginExecutionContext context = null;
        StringBuilder strMess = new StringBuilder();

        void IPlugin.Execute(IServiceProvider serviceProvider)
        {
            context = (IPluginExecutionContext)serviceProvider.GetService(typeof(IPluginExecutionContext));
            factory = (IOrganizationServiceFactory)serviceProvider.GetService(typeof(IOrganizationServiceFactory));
            service = factory.CreateOrganizationService(context.UserId);
            strMess.AppendLine("Vào plugin Plugin_BulkWaiver_Approve");
            strMess.AppendLine("context.Depth " + context.Depth);
            if (context.Depth > 1)
                return;
            if (context.InputParameters.Contains("Target") && (context.InputParameters["Target"] is Entity))
            {
                Entity tar = (Entity)context.InputParameters["Target"];
                var serializer = new JavaScriptSerializer();
                if (tar.LogicalName == "bsd_bulkwaiver")
                {
                    try
                    {
                        strMess.AppendLine("tar.LogicalName " + tar.LogicalName);
                        //var strResult3 = serializer.Serialize(tar);
                        strMess.AppendLine(context.MessageName);
                        var EnBulkWaiver = service.Retrieve(tar.LogicalName, tar.Id, new ColumnSet(true));
                        //var strResult3 = serializer.Serialize(tar);
                        //strMess.AppendLine(strResult3);
                        if (tar.Contains("statuscode"))
                            strMess.AppendLine("có statuscode");
                        else
                            strMess.AppendLine("Ko statuscode");
                        #region --- APPROVE ---
                        if (context.MessageName == "Update" && tar.Contains("statuscode") && ((OptionSetValue)tar["statuscode"]).Value == 100000000)//APPROVE
                        {
                            EntityCollection list = find(tar.ToEntityReference());
                            strMess.AppendLine("list count " + list.Entities.Count);
                            if (list.Entities.Count > 0)
                            {
                                foreach (Entity e in list.Entities)
                                {
                                    strMess.AppendLine("vào foreach ");
                                    //KIEM TRA DU LIEU
                                    if (!e.Contains("bsd_installment"))
                                        throw new InvalidPluginExecutionException("The condition to approve this form is not to let field Installment in Waiver Approve Detail: " + e["bsd_name"].ToString() + " empty.");
                                    if (!e.Contains("bsd_waiveramount"))
                                        throw new InvalidPluginExecutionException("The condition to approve this form is not to let field Waiver amount in Waiver Approve Detail: " + e["bsd_name"].ToString() + " empty.");
                                    //if (!e.Contains("bsd_installmentamount"))
                                    //    throw new InvalidPluginExecutionException("The condition to approve this form is not to let field Installment amount in Waiver Approve Detail: " + e["bsd_name"].ToString() + " empty.");

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
                                                strMess.AppendLine(Wdetail_waiver.ToString() + "______" + balance.ToString() + "______" + amountOTP.ToString() + "______" + paid.ToString() + "______" + waiverinstallment.ToString());
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
                                                //decimal balace_includeInterest = (amountOTP - paid - deposit - ins_waiveramount);
                                                //ins["bsd_balanceincludeinterest"] = new Money(balace_includeInterest);
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
                                                strMess.AppendLine("balancemanagement: " + balancemanagement.ToString());
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
                                                    else
                                                    {

                                                    }
                                                    ins["bsd_managementfeesstatus"] = false;

                                                }
                                                ins["bsd_managementfeewaiver"] = new Money(Wdetail_waiver + managementfeewaiver);
                                                strMess.AppendLine("ins.Id: " + ins.Id);
                                                strMess.AppendLine("bsd_managementfeesstatus: " + ins["bsd_managementfeesstatus"]);
                                                service.Update(ins);
                                                #endregion
                                                break;
                                            case 100000003://type=Maintenance Fee
                                                #region Maintenance Fee
                                                ins.Id = installment.Id;
                                                decimal balancemaintence = maintenanceamount - maintenancefeepaid - maintenancefeewaiver - Wdetail_waiver;

                                                strMess.AppendLine("balancemaintence: " + balancemaintence.ToString());
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
                                                    else
                                                    {

                                                    }

                                                    ins["bsd_maintenancefeesstatus"] = false;
                                                }
                                                ins["bsd_maintenancefeewaiver"] = new Money(Wdetail_waiver + maintenancefeewaiver);
                                                strMess.AppendLine("ins.Id: " + ins.Id);
                                                strMess.AppendLine("bsd_maintenancefeesstatus: " + ins["bsd_maintenancefeesstatus"]);
                                                service.Update(ins);
                                                #endregion
                                                break;
                                        }
                                    }
                                    Entity bulkwaiverdetail = new Entity(e.LogicalName);
                                    bulkwaiverdetail.Id = e.Id;
                                    bulkwaiverdetail["statuscode"] = new OptionSetValue(100000000);//waiver detail: statuscode -> approve 100000000
                                    service.Update(bulkwaiverdetail);

                                    //Update Installment(Waiver Amount) = Waiver (Installment) + Waiver (Interest) + Maintenance Fee (Waiver) + Management Fee (Waiver)
                                    Entity enInstallment = service.Retrieve(installment.LogicalName, installment.Id, new ColumnSet(true));
                                    decimal bsd_waiverinstallment = enInstallment.Contains("bsd_waiverinstallment") ? ((Money)enInstallment["bsd_waiverinstallment"]).Value : 0;
                                    decimal bsd_waiverinterest = enInstallment.Contains("bsd_waiverinterest") ? ((Money)enInstallment["bsd_waiverinterest"]).Value : 0;
                                    decimal bsd_maintenancefeewaiver = enInstallment.Contains("bsd_maintenancefeewaiver") ? ((Money)enInstallment["bsd_maintenancefeewaiver"]).Value : 0;
                                    decimal bsd_managementfeewaiver = enInstallment.Contains("bsd_managementfeewaiver") ? ((Money)enInstallment["bsd_managementfeewaiver"]).Value : 0;
                                    decimal bsd_waiveramount = bsd_waiverinstallment + bsd_waiverinterest + bsd_maintenancefeewaiver + bsd_managementfeewaiver;
                                    Entity enInstall = new Entity(enInstallment.LogicalName, enInstallment.Id);
                                    enInstall["bsd_waiveramount"] = new Money(bsd_waiveramount);
                                    service.Update(enInstall);

                                    strMess.AppendLine("cập nhật xong ins");
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
                                        strMess.AppendLine("22");
                                        if (phaseNum == 1)
                                        {
                                            strMess.AppendLine("23");
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
                                            strMess.AppendLine("24");
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
                                        //oe_tmp["bsd_totalamountpaid"] = new Money(d_oe_bsd_totalamountpaid);
                                        //oe_tmp["bsd_totalpercent"] = (d_oe_bsd_totalamountpaid / d_oe_amountCalcPercent) * 100;
                                        service.Update(oe_tmp);
                                    }
                                }
                            }
                            else throw new InvalidPluginExecutionException("The list of waiver to be processed is currently empty. Please check again.");
                            updatePerson(tar);
                        }
                        #endregion
                        //throw new InvalidPluginExecutionException("test system");
                        #region --- Rejected ---
                        if (context.MessageName == "Update" && tar.Contains("statuscode") && ((OptionSetValue)tar["statuscode"]).Value == 100000001)//Rejected
                        {
                            EntityCollection list = find(tar.ToEntityReference());
                            if (list.Entities.Count > 0)
                            {
                                foreach (Entity e in list.Entities)
                                {
                                    Entity bulkwaiverdetail = new Entity(e.LogicalName);
                                    bulkwaiverdetail.Id = e.Id;
                                    bulkwaiverdetail["statuscode"] = new OptionSetValue(100000001);//waiver detail: statuscode -> Rejected 100000001
                                    service.Update(bulkwaiverdetail);
                                }
                            }
                            updatePerson(tar);
                        }
                        #endregion
                    }
                    catch (Exception ex)
                    {
                        //strMess.AppendLine(ex.ToString());
                        throw new InvalidPluginExecutionException(ex.Message);
                    }

                }
                //throw new InvalidPluginExecutionException(strMess.ToString());
            }
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
        private EntityCollection find(EntityReference bulkwaiver)
        {
            string fetXml = @"<fetch version='1.0' output-format='xml-platform' mapping='logical' distinct='false'>
                      <entity name='bsd_bulkwaiverdetail'>
                        <attribute name='bsd_bulkwaiverdetailid' />
                        <attribute name='bsd_name' />
                        <attribute name='bsd_waivertype' />
                        <attribute name='bsd_waiveramount' />
                        <attribute name='bsd_optionentry' />
                        <attribute name='bsd_installmentamount' />
                        <attribute name='bsd_installment' />
                        <attribute name='bsd_bulkwaiver' />
                        <order attribute='bsd_name' descending='false' />
                        <filter type='and'>
                          <condition attribute='bsd_bulkwaiver' operator='eq'  uitype='bsd_bulkwaiver' value='{0}' />
                          <condition attribute='statecode' operator='eq' value='0' />
                          <condition attribute='statuscode' operator='eq' value='1' />
                        </filter>
                      </entity>
                    </fetch>";
            fetXml = string.Format(fetXml, bulkwaiver.Id.ToString());
            EntityCollection entc = service.RetrieveMultiple(new FetchExpression(fetXml));
            return entc;
        }
        private void updatePerson(Entity tar)
        {
            Entity enWaiverApprove = new Entity(tar.LogicalName, tar.Id);
            enWaiverApprove["bsd_approvedrejectedperson"] = new EntityReference("systemuser", context.UserId);
            enWaiverApprove["bsd_approvedrejecteddate"] = RetrieveLocalTimeFromUTCTime(DateTime.Now, service);
            service.Update(enWaiverApprove);
        }
        private DateTime RetrieveLocalTimeFromUTCTime(DateTime utcTime, IOrganizationService service)
        {
            int? timeZoneCode = RetrieveCurrentUsersSettings(service);
            if (!timeZoneCode.HasValue)
                throw new InvalidPluginExecutionException("Can't find time zone code");
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
                    Conditions = { new ConditionExpression("systemuserid", ConditionOperator.EqualUserId) }
                }
            }).Entities[0].ToEntity<Entity>();

            return (int?)currentUserSettings.Attributes["timezonecode"];
        }
    }
}