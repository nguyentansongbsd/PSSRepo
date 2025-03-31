
// 160827 VoidPayment
//@@ TriCM
//$$ if CREATE (xay ra khi khach hang thanh toan cho OE nay ma user nhap sai OE hoac thanh toan nhầm)
//$$ create new record in voidPayment - ( se co payment infor) gan voi payment nao thi change status code cua payment do thanh pending revert=100,000,001
//Void payment statuscode	active:1
//Approved: 100,000,000
//reject	:100,000,001

// Payment  Revert: 100,000,002
//          Pending revert: 100,000,001
//          paid: 100,000,000

//$$ IF UPDATE statuscode = revert if approve void payment
//+ Cập nhật statuscode - Payment = Reverted
//+ Từ payment > lấy được installment đang thanh toán, có các trường hợp sau:
//----> Amount Pay = Amount Phase và không có Advance Payment > Thì cập nhật installment thành Not Paid
//----> Amount Pay = Amount Phase và có Advance Payment > Thì cập nhật installment thành Not Paid và Advance Payment thành Reverted
//----> Amount Pay < Amount Phase > Thì cập nhật installment thành Not Paid và trừ lại số tiền Amount Pay
//+ Kiểm tra có Outstanding của Payment thì cập nhật thành Reverted

// update total amount paid, total percent paid of OE

// 161103
// ## bo outstanding
// ## kiem tra neu payment co transaction payment thi revert udpate installment khi update approve cho void PM
// ## check co transaction PM hay k - change statuscode cua transaction PM thanh revert
//%% update statuscode of OE & Installment
//161124 - co them 2 field waiver


// 170303 - them moi
// revert cho apply document thi update cac du lieu lien quan & chuyen trang thai apply document sag revert - hoàn lai so tien cua cac advance payment va instllment, deposit, OE da tinh toan trong apply document
// chia 2 loai - neu void pm chua payment infor thi revert payment , else neu chua apply doc thi revert app doc

/// 170506 NEW SUBGRID
// su dung field moi contain sub grid check of transaction payment
// installment transaction pm - 1 field
// interest charge transaction pm - 1 field

/// 170516
/// @Han
// when create pm copy - update  // 170516 - update field bsd_createbyrevert = yes

/// 170519 - Nhung setting cho 1 field auto tinh toan lai total percent tren OE

/// 170608
// them bsd_paiddate - chua du lieu - khi payment cho INS - chuyen trang thai Paid thi update payment date vao field nay
// neu revert payment va INS not paid thi xoa trang field nay

/// 170814
// them fan void cho Miscellaneous -  type of payment= other & apply doc. them subgrid
// lam tuong tu nhu fees

using Microsoft.Crm.Sdk.Messages;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Query;
using System;


namespace Plugin_VoidPayment_updatePendingPM
{
    public class Plugin_VoidPayment_updatePendingPM : IPlugin
    {
        IOrganizationService service = null;
        IOrganizationServiceFactory factory = null;
        ITracingService traceService = null;

        void IPlugin.Execute(IServiceProvider serviceProvider)
        {
            IPluginExecutionContext context = (IPluginExecutionContext)serviceProvider.GetService(typeof(IPluginExecutionContext));

            Entity target = (Entity)context.InputParameters["Target"];
            if (target.LogicalName == "bsd_voidpayment")
            {
                factory = (IOrganizationServiceFactory)serviceProvider.GetService(typeof(IOrganizationServiceFactory));
                service = factory.CreateOrganizationService(context.UserId);
                traceService = (ITracingService)serviceProvider.GetService(typeof(ITracingService));
                DateTime d_now = RetrieveLocalTimeFromUTCTime(DateTime.Now, service);
                Entity en_voidPM = service.Retrieve(target.LogicalName, target.Id, new ColumnSet(new string[] {
                    "bsd_name", "bsd_payment", "statuscode", "bsd_amount",
                    "bsd_applydocument",
                    "bsd_newpayment",
                    "bsd_totalapplyamount"
                }));

                #region ---------------- UPDATE -------------------
                //update with status = approve - check installment and current payment - revert
                if ((context.MessageName == "Update") && (target.Contains("statuscode")))
                {
                    traceService.Trace("context.MessageName" + context.MessageName);
                    #region  --------------------------------------- Approve ----------------------------------------------
                    if (((OptionSetValue)target["statuscode"]).Value == 100000000)   // = approve
                    {
                        traceService.Trace("1");
                        #region ------------------------- update void PM for PM ---------------------------
                        if (en_voidPM.Contains("bsd_payment"))
                        {
                            traceService.Trace("2");
                            #region ------------------ retrieve payment -----------------
                            Entity en_PM = service.Retrieve(((EntityReference)en_voidPM["bsd_payment"]).LogicalName, ((EntityReference)en_voidPM["bsd_payment"]).Id,
                            new ColumnSet(new string[]{
                                    "bsd_name",
                                    "bsd_paymentid",
                                    "bsd_amountpay",
                                    "bsd_paymenttype",
                                    "bsd_paymentactualtime",
                                    "bsd_transactiontype",
                                    "bsd_paymentmode",
                                    "bsd_totalamountpayablephase",
                                    "bsd_totalamountpaidphase",
                                    "bsd_optionentry",
                                    "bsd_reservation",
                                    "bsd_paymentschemedetail",
                                    "bsd_depositamount",
                                    "bsd_managementfee",
                                    "bsd_maintenancefee",
                                    "bsd_interestcharge",
                                    "bsd_balance",
                                    "bsd_differentamount",
                                    "statuscode",
                                    "bsd_savegridinstallment",
                                    "bsd_latepayment",
                                    "bsd_latedays",
                                    "bsd_waiveramount",
                                    "bsd_waiverinstallment",
                                    "bsd_waiverinterest",
                                    "bsd_project",
                                    "bsd_units",
                                    "bsd_purchaser",
                                    "bsd_exchangeratetext",
                                    "bsd_exchangemoneytext",
                                    "bsd_paymentactualtime",
                                    "bsd_arraypsdid",
                                    "bsd_arrayamountpay",
                                    "bsd_arrayinstallmentinterest",
                                    "bsd_arrayinterestamount",
                                    "bsd_arraymicellaneousid",
                                    "bsd_arraymicellaneousamount",
                                    "bsd_miscellaneous",
                                    "bsd_arrayfees",
                                    "bsd_arrayfeesamount"
                                }));
                            int pm_status = en_PM.Contains("statuscode") ? ((OptionSetValue)en_PM["statuscode"]).Value : 0;
                            if (pm_status == 100000002)
                                throw new InvalidPluginExecutionException("Payment " + (string)en_PM["bsd_name"] + " has been revert!");
                            if (!en_PM.Contains("bsd_paymentactualtime"))
                                throw new InvalidPluginExecutionException("Please choose 'Receipt Date'!");
                            if (!en_PM.Contains("bsd_transactiontype"))
                                throw new InvalidPluginExecutionException("Please choose 'Transaction Type'!");
                            if (!en_PM.Contains("bsd_paymenttype"))
                                throw new InvalidPluginExecutionException("Please choose 'Payment Type'!");
                            if (!en_PM.Contains("bsd_paymentmode"))
                                throw new InvalidPluginExecutionException("Please choose 'Payment Mode'!");
                            if (!en_PM.Contains("bsd_purchaser"))
                                throw new InvalidPluginExecutionException("Please choose 'Purchaser'!");
                            if (!en_PM.Contains("bsd_units"))
                                throw new InvalidPluginExecutionException("Please choose 'Units'!");

                            decimal d_pmAMpay = en_PM.Contains("bsd_amountpay") ? ((Money)en_PM["bsd_amountpay"]).Value : 0;
                            decimal d_pmAMPhase = en_PM.Contains("bsd_totalamountpayablephase") ? ((Money)en_PM["bsd_totalamountpayablephase"]).Value : 0;
                            decimal d_pmAMpaid = en_PM.Contains("bsd_totalamountpaidphase") ? ((Money)en_PM["bsd_totalamountpaidphase"]).Value : 0;
                            decimal d_pmMaintenance = en_PM.Contains("bsd_maintenancefee") ? ((Money)en_PM["bsd_maintenancefee"]).Value : 0;
                            decimal d_pmManagement = en_PM.Contains("bsd_managementfee") ? ((Money)en_PM["bsd_managementfee"]).Value : 0;
                            decimal d_pmDeposit = en_PM.Contains("bsd_depositamount") ? ((Money)en_PM["bsd_depositamount"]).Value : 0;
                            decimal d_pmDifference = en_PM.Contains("bsd_differentamount") ? ((Money)en_PM["bsd_differentamount"]).Value : 0;
                            decimal d_pmBalance = en_PM.Contains("bsd_balance") ? ((Money)en_PM["bsd_balance"]).Value : 0;
                            decimal d_pmInterestAm = en_PM.Contains("bsd_interestcharge") ? ((Money)en_PM["bsd_interestcharge"]).Value : 0;
                            bool f_latePM = en_PM.Contains("bsd_latepayment") ? (bool)en_PM["bsd_latepayment"] : false;
                            int i_PmLateday = en_PM.Contains("bsd_latedays") ? (int)en_PM["bsd_latedays"] : 0;

                            string s_PMgrid = en_PM.Contains("bsd_savegridinstallment") ? (string)en_PM["bsd_savegridinstallment"] : "";
                            // new subgrid 170506
                            string s_bsd_arraypsdid = en_PM.Contains("bsd_arraypsdid") ? (string)en_PM["bsd_arraypsdid"] : "";
                            string s_bsd_arrayamountpay = en_PM.Contains("bsd_arrayamountpay") ? (string)en_PM["bsd_arrayamountpay"] : "";

                            string s_bsd_arrayinstallmentinterest = en_PM.Contains("bsd_arrayinstallmentinterest") ? (string)en_PM["bsd_arrayinstallmentinterest"] : "";
                            string s_bsd_arrayinterestamount = en_PM.Contains("bsd_arrayinterestamount") ? (string)en_PM["bsd_arrayinterestamount"] : "";
                            string s_bsd_arrayfees = en_PM.Contains("bsd_arrayfees") ? (string)en_PM["bsd_arrayfees"] : "";
                            string s_bsd_arrayfeesamount = en_PM.Contains("bsd_arrayfeesamount") ? (string)en_PM["bsd_arrayfeesamount"] : "";
                            string s_bsd_arraymicellaneousid = en_PM.Contains("bsd_arraymicellaneousid") ? (string)en_PM["bsd_arraymicellaneousid"] : "";
                            string s_bsd_arraymicellaneousamount = en_PM.Contains("bsd_arraymicellaneousamount") ? (string)en_PM["bsd_arraymicellaneousamount"] : "";

                            int i_pmType = en_PM.Contains("bsd_paymenttype") ? ((OptionSetValue)en_PM["bsd_paymenttype"]).Value : 0;
                            // Thạnh đỗ
                            EntityReference enrfPaymentSchemeDetail = en_PM.Contains("bsd_paymentschemedetail") ? (EntityReference)en_PM["bsd_paymentschemedetail"] : null;
                            #endregion  // end retrieve payment
                            traceService.Trace("3");
                            #region ------------------ pm type = DEPOSIT PM --------------------------------------

                            if (i_pmType == 100000001)
                            {
                                if (!en_PM.Contains("bsd_reservation"))
                                    throw new InvalidPluginExecutionException("Payment " + (string)en_PM["bsd_name"] + " does not contain Reservation information. Please check again!");
                                else
                                {
                                    Entity en_Resv = new Entity(((EntityReference)en_PM["bsd_reservation"]).LogicalName);
                                    en_Resv = service.Retrieve(((EntityReference)en_PM["bsd_reservation"]).LogicalName, ((EntityReference)en_PM["bsd_reservation"]).Id, new ColumnSet(new string[] {
                                        "statuscode",
                                        "name",
                                        "bsd_salesdepartmentreceiveddeposit",
                                        "bsd_paymentscheme",
                                        "bsd_unitno",
                                        "bsd_totalamountpaid"
                                        }));
                                    Entity en_ResvTmp = new Entity(en_Resv.LogicalName);
                                    en_ResvTmp.Id = en_Resv.Id;
                                    if (en_Resv.Contains("statuscode"))
                                    {
                                        int i_ResvStatus = ((OptionSetValue)en_Resv["statuscode"]).Value;
                                        switch (i_ResvStatus)
                                        {
                                            case 4:
                                                throw new InvalidPluginExecutionException("Reservation " + (string)en_Resv["name"] + " has already attached with an Option Entry! Can not revert this Payment!");
                                            //break;
                                            case 6:
                                                throw new InvalidPluginExecutionException("Reservation " + (string)en_Resv["name"] + " has been Canceled. Can not reverted this payment!");
                                            //break;
                                            case 100000001:
                                                throw new InvalidPluginExecutionException("Reservation " + (string)en_Resv["name"] + " has been Terminated. Can not reverted this payment!");
                                            //break;
                                            case 100000003:
                                                throw new InvalidPluginExecutionException("Reservation " + (string)en_Resv["name"] + " has been Reject. Can not reverted this payment!");
                                            //break;
                                            case 100000002:
                                                throw new InvalidPluginExecutionException("Reservation " + (string)en_Resv["name"] + " has been Pending Cancel Deposit. Can not reverted this payment!");
                                        }
                                    }

                                    #region ----------- open state of reservation ----------
                                    SetStateRequest setStateRequest = new SetStateRequest()
                                    {
                                        EntityMoniker = new EntityReference
                                        {
                                            Id = en_Resv.Id,                 //id form
                                            LogicalName = en_Resv.LogicalName        //localname
                                        },
                                        State = new OptionSetValue(0),          //state value = draft
                                        Status = new OptionSetValue(100000000)      //status value = reservation
                                    };
                                    service.Execute(setStateRequest);

                                    #endregion

                                    Entity en_Unit = new Entity(((EntityReference)en_Resv["bsd_unitno"]).LogicalName);
                                    en_Unit.Id = ((EntityReference)en_Resv["bsd_unitno"]).Id;
                                    if (en_Resv.Contains("bsd_salesdepartmentreceiveddeposit"))
                                    {
                                        if ((bool)en_Resv["bsd_salesdepartmentreceiveddeposit"] == true)
                                        {
                                            en_ResvTmp["statuscode"] = new OptionSetValue(100000006); // collect
                                            en_Unit["statuscode"] = new OptionSetValue(100000005); // collect
                                        }
                                        else
                                        {
                                            en_ResvTmp["statuscode"] = new OptionSetValue(100000000); //reser
                                            en_Unit["statuscode"] = new OptionSetValue(100000006); // reserve
                                        }
                                    }
                                    else
                                    {
                                        en_ResvTmp["statuscode"] = new OptionSetValue(100000000);
                                        en_Unit["statuscode"] = new OptionSetValue(100000006); // reserve
                                    }
                                    en_ResvTmp["bsd_deposittime"] = null;
                                    en_ResvTmp["bsd_isdeposited"] = false;

                                    en_ResvTmp["bsd_totalamountpaid"] = new Money(0);

                                    service.Update(en_ResvTmp);
                                    service.Update(en_Unit);
                                    // tra du deposit - tru lai tien cho 1st installment
                                    #region  1st installment
                                    EntityCollection en_1stinstallment = get_1stinstallment(service, ((EntityReference)en_PM["bsd_reservation"]).Id);

                                    if (en_1stinstallment.Entities.Count > 0)
                                    {
                                        Entity en_1stTmp = new Entity(en_1stinstallment.Entities[0].LogicalName);
                                        en_1stTmp.Id = en_1stinstallment.Entities[0].Id;
                                        en_1stTmp["bsd_amountwaspaid"] = new Money(0);
                                        decimal bsd_balance = en_1stinstallment.Entities[0].Contains("bsd_balance") ? ((Money)en_1stinstallment.Entities[0]["bsd_balance"]).Value : 0;
                                        decimal bsd_amountofthisphase = en_1stinstallment.Entities[0].Contains("bsd_amountofthisphase") ? ((Money)en_1stinstallment.Entities[0]["bsd_amountofthisphase"]).Value : 0;
                                        bsd_balance += d_pmAMpay;
                                        en_1stTmp["bsd_balance"] = new Money(bsd_amountofthisphase);
                                        en_1stTmp["bsd_depositamount"] = new Money(0);
                                        en_1stTmp["statuscode"] = new OptionSetValue(100000000); // not paid
                                        en_1stTmp["bsd_paiddate"] = null;
                                        service.Update(en_1stTmp);
                                    }

                                    #endregion // end deposit

                                    #region  ------------------------------- advance payment ------------------------------------
                                    //update_AdvPM(((EntityReference)en_voidPM["bsd_payment"]).Id);
                                    EntityCollection ec_advPM = get_AdvPM(service, en_PM.Id);

                                    if (ec_advPM.Entities.Count > 0)
                                    {
                                        Entity en_Adv_up = new Entity(ec_advPM.Entities[0].LogicalName);
                                        en_Adv_up.Id = ec_advPM.Entities[0].Id;
                                        en_Adv_up["statuscode"] = new OptionSetValue(100000002); // revert

                                        service.Update(en_Adv_up);
                                    }
                                    #endregion

                                    #region ------------------ update  payment ----------------------------

                                    Entity en_pmUp = new Entity(en_PM.LogicalName);
                                    en_pmUp.Id = en_PM.Id;
                                    en_pmUp["statuscode"] = new OptionSetValue(100000002);
                                    service.Update(en_pmUp);
                                    #endregion


                                }
                            }
                            #endregion

                            #region ------------------ pm type != deposit  (other , fees , installment) -------------------------
                            else
                            {
                                traceService.Trace("4");
                                #region ------------ retrieve OE -----------
                                if ((i_pmType == 100000002 || i_pmType == 100000003 || i_pmType == 100000004) && (!en_PM.Contains("bsd_optionentry")))
                                    throw new InvalidPluginExecutionException("Payment " + (string)en_PM["bsd_name"] + " does not contain Option Entry information. Please check again!");

                                Entity en_OE = new Entity(((EntityReference)en_PM["bsd_optionentry"]).LogicalName);
                                en_OE = service.Retrieve(((EntityReference)en_PM["bsd_optionentry"]).LogicalName, ((EntityReference)en_PM["bsd_optionentry"]).Id, new ColumnSet(new string[] {
                                    "ordernumber",
                                    "bsd_project",
                                    "statuscode",
                                    "customerid",
                                    "name",
                                    "bsd_managementfee",
                                    "bsd_freightamount",
                                    "bsd_paymentscheme",
                                    "bsd_signedcontractdate",
                                    "bsd_totalpercent",
                                    "bsd_totallatedays",
                                    "bsd_totalamountpaid",
                                    "totalamount",
                                    "bsd_unitnumber",
                                    "bsd_havemaintenancefee",
                                    "bsd_havemanagementfee",
                                    "bsd_maintenancefeesstatus",
                                    "bsd_managementfeesstatus"
                                    }));

                                int i_bsd_totallatedays = en_OE.Contains("bsd_totallatedays") ? (int)en_OE["bsd_totallatedays"] : 0;
                                decimal d_oe_totalamount = en_OE.Contains("totalamount") ? ((Money)en_OE["totalamount"]).Value : 0;
                                decimal d_oe_bsd_totalamountpaid = en_OE.Contains("bsd_totalamountpaid") ? ((Money)en_OE["bsd_totalamountpaid"]).Value : 0;
                                int i_oe_statuscode = en_OE.Contains("statuscode") ? ((OptionSetValue)en_OE["statuscode"]).Value : 100000000;
                                int i_Unitstatus = 100000003;


                                decimal d_oe_bsd_totalpercent = en_OE.Contains("bsd_totalpercent") ? (decimal)en_OE["bsd_totalpercent"] : 0;

                                decimal d_oe_bsd_freightamount = en_OE.Contains("bsd_freightamount") ? ((Money)en_OE["bsd_freightamount"]).Value : 0;
                                decimal d_oe_amountCalcPercent = d_oe_totalamount - d_oe_bsd_freightamount;

                                bool f_OE_Signcontract = en_OE.Contains("bsd_signedcontractdate") ? true : false;

                                decimal d_TransactionAM = 0;

                                Entity en_OE_Up = new Entity(en_OE.LogicalName);
                                en_OE_Up.Id = en_OE.Id;
                                #endregion
                                traceService.Trace("5");
                                #region  ------------------- retrieve Installment ----------------
                                if (en_PM.Contains("bsd_paymentschemedetail"))
                                {
                                    Entity en_Ins = service.Retrieve(((EntityReference)en_PM["bsd_paymentschemedetail"]).LogicalName,
                                           ((EntityReference)en_PM["bsd_paymentschemedetail"]).Id, new ColumnSet(true));
                                    traceService.Trace("5.1");
                                    decimal d_PmsDtlDeposit = en_Ins.Contains("bsd_depositamount") ? ((Money)en_Ins["bsd_depositamount"]).Value : 0;
                                    decimal d_PmsDtlAmPaid = en_Ins.Contains("bsd_amountwaspaid") ? ((Money)en_Ins["bsd_amountwaspaid"]).Value : 0;
                                    decimal d_PmsDtlAmPhase = en_Ins.Contains("bsd_amountofthisphase") ? ((Money)en_Ins["bsd_amountofthisphase"]).Value : 0;
                                    decimal d_PmsDtlBalance = en_Ins.Contains("bsd_balance") ? ((Money)en_Ins["bsd_balance"]).Value : 0;
                                    traceService.Trace("5.2");
                                    decimal d_PmsDtlWaiverAM = en_Ins.Contains("bsd_waiveramount") ? ((Money)en_Ins["bsd_waiveramount"]).Value : 0;

                                    int i_PmsDtlStatuscode = en_Ins.Contains("statuscode") ? ((OptionSetValue)en_Ins["statuscode"]).Value : 100000000;
                                    int i_Pms_bsd_ordernumber = en_Ins.Contains("bsd_ordernumber") ? (int)en_Ins["bsd_ordernumber"] : 1;
                                    traceService.Trace("5.3");
                                    decimal d_PmsDtlInterestcharge = en_Ins.Contains("bsd_interestchargeamount") ? ((Money)en_Ins["bsd_interestchargeamount"]).Value : 0;
                                    decimal d_PmsDtlInterestchargePaid = en_Ins.Contains("bsd_interestwaspaid") ? ((Money)en_Ins["bsd_interestwaspaid"]).Value : 0;
                                    int i_PmsDtlbsd_interestchargestatus = en_Ins.Contains("bsd_interestchargestatus") ? ((OptionSetValue)en_Ins["bsd_interestchargestatus"]).Value : 0;
                                    int i_PmsDtlActualgraceday = en_Ins.Contains("bsd_actualgracedays") ? (int)en_Ins["bsd_actualgracedays"] : 0;
                                    int i_bsd_previousdelays = en_Ins.Contains("bsd_previousdelays") ? (int)en_Ins["bsd_previousdelays"] : 0;
                                    traceService.Trace("5.4");
                                }




                                #endregion
                                traceService.Trace("6");
                                #region -------------------------- typePM = Fees  100000004 -------------------
                                if (i_pmType == 100000004) // Fees
                                {
                                    traceService.Trace("6.1");
                                    string bsd_arrayfees = en_PM.Contains("bsd_arrayfees") ? (string)en_PM["bsd_arrayfees"] : "";
                                    string bsd_arrayfeesamount = en_PM.Contains("bsd_arrayfeesamount") ? (string)en_PM["bsd_arrayfeesamount"] : "";
                                    string[] arrId = bsd_arrayfees.Split(',');
                                    string[] arrAmount = bsd_arrayfeesamount.Split(',');
                                    traceService.Trace("6.2");
                                    bool newtype = true;
                                    for (int i = 0; i < arrId.Length; i++)
                                    {
                                        traceService.Trace("6.3");
                                        decimal voidAmount = decimal.Parse(arrAmount[i].ToString());
                                        string[] arr = arrId[i].Split('_');
                                        if (arr.Length == 2)
                                        {
                                            string installmentId = arr[0];
                                            string type = arr[1];
                                            Entity enInstallment = null;
                                            traceService.Trace("6.4");
                                            switch (type)
                                            {
                                                case "main":
                                                    enInstallment = service.Retrieve("bsd_paymentschemedetail", new Guid(installmentId), new ColumnSet(new string[] {
                                                    "bsd_maintenancefeesstatus", "bsd_maintenancefeepaid" }));
                                                    bool bsd_maintenancefeesstatus = enInstallment.Contains("bsd_maintenancefeesstatus") ? (bool)enInstallment["bsd_maintenancefeesstatus"] : false;
                                                    decimal bsd_maintenancefeepaid = enInstallment.Contains("bsd_maintenancefeepaid") ? ((Money)enInstallment["bsd_maintenancefeepaid"]).Value : 0;
                                                    bsd_maintenancefeesstatus = false;
                                                    bsd_maintenancefeepaid -= voidAmount;
                                                    d_oe_bsd_totalamountpaid -= voidAmount;
                                                    enInstallment["bsd_maintenancefeesstatus"] = bsd_maintenancefeesstatus;
                                                    enInstallment["bsd_maintenancefeepaid"] = new Money(bsd_maintenancefeepaid);

                                                    break;
                                                case "mana":
                                                    enInstallment = service.Retrieve("bsd_paymentschemedetail", new Guid(installmentId), new ColumnSet(new string[] {
                                                    "bsd_managementfeesstatus", "bsd_managementfeepaid" }));
                                                    bool bsd_managementfeesstatus = enInstallment.Contains("bsd_managementfeesstatus") ? (bool)enInstallment["bsd_managementfeesstatus"] : false;
                                                    decimal bsd_managementfeepaid = enInstallment.Contains("bsd_managementfeepaid") ? ((Money)enInstallment["bsd_managementfeepaid"]).Value : 0;
                                                    bsd_managementfeesstatus = false;
                                                    bsd_managementfeepaid -= voidAmount;
                                                    d_oe_bsd_totalamountpaid -= voidAmount;
                                                    enInstallment["bsd_managementfeesstatus"] = bsd_managementfeesstatus;
                                                    enInstallment["bsd_managementfeepaid"] = new Money(bsd_managementfeepaid);

                                                    break;
                                            }
                                            service.Update(enInstallment); 
                                        }
                                        else
                                        {
                                            newtype = false;
                                            break;
                                        }
                                    }
                                    if (newtype == false)
                                    {
                                        if (en_PM.Contains("bsd_paymentschemedetail"))
                                        {
                                            Entity en_Ins = service.Retrieve(((EntityReference)en_PM["bsd_paymentschemedetail"]).LogicalName,
                                                   ((EntityReference)en_PM["bsd_paymentschemedetail"]).Id, new ColumnSet(true));
                                            bool f_main = (en_Ins.Contains("bsd_maintenancefeesstatus")) ? (bool)en_Ins["bsd_maintenancefeesstatus"] : false;
                                            bool f_mana = (en_Ins.Contains("bsd_managementfeesstatus")) ? (bool)en_Ins["bsd_managementfeesstatus"] : false;

                                            #region  check amount pay & balance --------
                                            decimal d_bsd_maintenanceamount = en_Ins.Contains("bsd_maintenanceamount") ? ((Money)en_Ins["bsd_maintenanceamount"]).Value : 0;
                                            decimal d_bsd_managementamount = en_Ins.Contains("bsd_managementamount") ? ((Money)en_Ins["bsd_managementamount"]).Value : 0;

                                            decimal d_bsd_maintenancefeepaid = en_Ins.Contains("bsd_maintenancefeepaid") ? ((Money)en_Ins["bsd_maintenancefeepaid"]).Value : 0;
                                            decimal d_bsd_managementfeepaid = en_Ins.Contains("bsd_managementfeepaid") ? ((Money)en_Ins["bsd_managementfeepaid"]).Value : 0;

                                            //decimal d_mainBL = d_bsd_maintenanceamount - d_bsd_maintenancefeepaid;
                                            //decimal d_manaBL = d_bsd_managementamount - d_bsd_managementfeepaid;
                                            #endregion

                                            EntityCollection ec_transactionPM = get_TransactionPM(service, en_PM.Id);

                                            if (ec_transactionPM.Entities.Count > 0)
                                            {
                                                foreach (Entity en_trans in ec_transactionPM.Entities)
                                                {
                                                    // kiem tra transaction type
                                                    //Installments    100000000
                                                    //Interest        100000001
                                                    //Fees            100000002
                                                    //Other           100000003
                                                    int i_trans_bsd_transactiontype = en_trans.Contains("bsd_transactiontype") ? ((OptionSetValue)en_trans["bsd_transactiontype"]).Value : 0;
                                                    decimal d_trans_bsd_amount = en_trans.Contains("bsd_amount") ? ((Money)en_trans["bsd_amount"]).Value : 0;
                                                    int i_bsd_feetype = en_trans.Contains("bsd_feetype") ? ((OptionSetValue)en_trans["bsd_feetype"]).Value : 0;

                                                    #region  -------- transaction type = Fees -----------------
                                                    if (i_trans_bsd_transactiontype == 100000002)
                                                    {
                                                        if (i_bsd_feetype == 100000000) // main
                                                        {
                                                            f_main = false;
                                                            d_bsd_maintenancefeepaid -= d_trans_bsd_amount;
                                                            d_oe_bsd_totalamountpaid -= d_trans_bsd_amount;
                                                        }
                                                        else if (i_bsd_feetype == 100000001)// mana
                                                        {
                                                            f_mana = false;
                                                            d_bsd_managementfeepaid -= d_trans_bsd_amount;
                                                        }

                                                    }
                                                    #endregion ------------
                                                }// end foreach
                                                Entity tmp_Ins = new Entity(en_Ins.LogicalName); // entity Installment(PMSDtl)
                                                tmp_Ins.Id = en_Ins.Id;
                                                tmp_Ins["bsd_maintenancefeesstatus"] = f_main;
                                                tmp_Ins["bsd_managementfeesstatus"] = f_mana;
                                                tmp_Ins["bsd_maintenancefeepaid"] = new Money(d_bsd_maintenancefeepaid);
                                                tmp_Ins["bsd_managementfeepaid"] = new Money(d_bsd_managementfeepaid);

                                                service.Update(tmp_Ins);

                                            } // end if ec_trans.entity.count > 0
                                            else
                                                throw new InvalidPluginExecutionException("Cannot find any Transaction payment (Fees) for payment " + (string)en_PM["bsd_name"] + "!");
                                        }

                                    }
                                    //#region ---------------- Transaction PM ---------------------
                                    if (s_bsd_arraypsdid != "" || s_bsd_arrayinstallmentinterest != "" || s_bsd_arrayfees != "" || s_bsd_arraymicellaneousid != "") // update lai cac transaction pm cua pm
                                    {
                                        d_TransactionAM = update_transactionPM(service, ((EntityReference)en_voidPM["bsd_payment"]).Id, en_OE);
                                    }
                                    //#endregion
                                    traceService.Trace("9.5");
                                    //update_AdvPM(((EntityReference)en_voidPM["bsd_payment"]).Id);
                                    EntityCollection ec_advPM = get_AdvPM(service, en_PM.Id);
                                    if (ec_advPM.Entities.Count > 0)
                                    {
                                        Entity en_Adv_up = new Entity(ec_advPM.Entities[0].LogicalName);
                                        en_Adv_up.Id = ec_advPM.Entities[0].Id;
                                        en_Adv_up["statuscode"] = new OptionSetValue(100000002); // revert
                                        service.Update(en_Adv_up);
                                    }

                                }
                                #endregion --- end fees  ---------
                                traceService.Trace("7");
                                #region -------------------- typePM = Interestcharge  100000003    -----------------
                                if (i_pmType == 100000003 ) // Interestcharge
                                {
                                    // Transaction Payment
                                    EntityCollection enclTransactionPayment = getTracsactionPaymentByPayment(en_PM);
                                    #region -------- difference AM > 0 -----------
                                    if (d_pmDifference > 0 && (enclTransactionPayment.Entities.Count > 0))
                                    {
                                        if (s_bsd_arraypsdid != "" || s_bsd_arrayinstallmentinterest != "" || s_bsd_arrayfees != "" || s_bsd_arraymicellaneousid != "")// update lai cac transaction pm cua pm
                                        {
                                            d_TransactionAM = update_transactionPM(service, ((EntityReference)en_voidPM["bsd_payment"]).Id, en_OE);
                                        }
                                        // check advance PM
                                        // update_AdvPM(((EntityReference)en_voidPM["bsd_payment"]).Id);
                                        EntityCollection ec_advPM = get_AdvPM(service, en_PM.Id);
                                        if (ec_advPM.Entities.Count > 0)
                                        {
                                            Entity en_Adv_up = new Entity(ec_advPM.Entities[0].LogicalName);
                                            en_Adv_up.Id = ec_advPM.Entities[0].Id;
                                            en_Adv_up["statuscode"] = new OptionSetValue(100000002); // revert
                                            service.Update(en_Adv_up);
                                        }

                                    }
                                    #endregion  // end difference > 0
                                    // update Interestcharge amount paid & status in installment
                                    if (en_PM.Contains("bsd_paymentschemedetail") && (enclTransactionPayment.Entities.Count == 0))
                                    {
                                        Entity en_Ins = service.Retrieve(((EntityReference)en_PM["bsd_paymentschemedetail"]).LogicalName,
                                               ((EntityReference)en_PM["bsd_paymentschemedetail"]).Id, new ColumnSet(true));
                                        traceService.Trace("5.1");
                                        decimal d_PmsDtlDeposit = en_Ins.Contains("bsd_depositamount") ? ((Money)en_Ins["bsd_depositamount"]).Value : 0;
                                        decimal d_PmsDtlAmPaid = en_Ins.Contains("bsd_amountwaspaid") ? ((Money)en_Ins["bsd_amountwaspaid"]).Value : 0;
                                        decimal d_PmsDtlAmPhase = en_Ins.Contains("bsd_amountofthisphase") ? ((Money)en_Ins["bsd_amountofthisphase"]).Value : 0;
                                        decimal d_PmsDtlBalance = en_Ins.Contains("bsd_balance") ? ((Money)en_Ins["bsd_balance"]).Value : 0;
                                        traceService.Trace("5.2");
                                        decimal d_PmsDtlWaiverAM = en_Ins.Contains("bsd_waiveramount") ? ((Money)en_Ins["bsd_waiveramount"]).Value : 0;

                                        int i_PmsDtlStatuscode = en_Ins.Contains("statuscode") ? ((OptionSetValue)en_Ins["statuscode"]).Value : 100000000;
                                        int i_Pms_bsd_ordernumber = en_Ins.Contains("bsd_ordernumber") ? (int)en_Ins["bsd_ordernumber"] : 1;
                                        traceService.Trace("5.3");
                                        decimal d_PmsDtlInterestcharge = en_Ins.Contains("bsd_interestchargeamount") ? ((Money)en_Ins["bsd_interestchargeamount"]).Value : 0;
                                        decimal d_PmsDtlInterestchargePaid = en_Ins.Contains("bsd_interestwaspaid") ? ((Money)en_Ins["bsd_interestwaspaid"]).Value : 0;
                                        int i_PmsDtlbsd_interestchargestatus = en_Ins.Contains("bsd_interestchargestatus") ? ((OptionSetValue)en_Ins["bsd_interestchargestatus"]).Value : 0;
                                        int i_PmsDtlActualgraceday = en_Ins.Contains("bsd_actualgracedays") ? (int)en_Ins["bsd_actualgracedays"] : 0;
                                        int i_bsd_previousdelays = en_Ins.Contains("bsd_previousdelays") ? (int)en_Ins["bsd_previousdelays"] : 0;
                                        traceService.Trace("5.4");
                                        d_PmsDtlInterestchargePaid -= d_pmAMpay < d_pmBalance ? d_pmAMpay : d_pmBalance;
                                        i_PmsDtlbsd_interestchargestatus = 100000000; // not paid
                                        Entity tmp_Ins = new Entity(en_Ins.LogicalName); // entity Installment(PMSDtl)
                                        tmp_Ins.Id = en_Ins.Id;
                                        tmp_Ins["bsd_interestwaspaid"] = new Money(d_PmsDtlInterestchargePaid);
                                        tmp_Ins["bsd_interestchargestatus"] = new OptionSetValue(i_PmsDtlbsd_interestchargestatus);
                                        service.Update(tmp_Ins);
                                    }

                                }
                                #endregion
                                traceService.Trace("8");
                                #region  --------------- pm type =  Installment    - - -   100000002   ------------------
                                if (i_pmType == 100000002)
                                {

                                    Entity en_Ins = service.Retrieve(((EntityReference)en_PM["bsd_paymentschemedetail"]).LogicalName,
                                            ((EntityReference)en_PM["bsd_paymentschemedetail"]).Id, new ColumnSet(true));
                                    traceService.Trace("5.1");
                                    decimal d_PmsDtlDeposit = en_Ins.Contains("bsd_depositamount") ? ((Money)en_Ins["bsd_depositamount"]).Value : 0;
                                    decimal d_PmsDtlAmPaid = en_Ins.Contains("bsd_amountwaspaid") ? ((Money)en_Ins["bsd_amountwaspaid"]).Value : 0;
                                    decimal d_PmsDtlAmPhase = en_Ins.Contains("bsd_amountofthisphase") ? ((Money)en_Ins["bsd_amountofthisphase"]).Value : 0;
                                    decimal d_PmsDtlBalance = en_Ins.Contains("bsd_balance") ? ((Money)en_Ins["bsd_balance"]).Value : 0;
                                    traceService.Trace("5.2");
                                    decimal d_PmsDtlWaiverAM = en_Ins.Contains("bsd_waiveramount") ? ((Money)en_Ins["bsd_waiveramount"]).Value : 0;

                                    int i_PmsDtlStatuscode = en_Ins.Contains("statuscode") ? ((OptionSetValue)en_Ins["statuscode"]).Value : 100000000;
                                    int i_Pms_bsd_ordernumber = en_Ins.Contains("bsd_ordernumber") ? (int)en_Ins["bsd_ordernumber"] : 1;
                                    traceService.Trace("5.3");
                                    decimal d_PmsDtlInterestcharge = en_Ins.Contains("bsd_interestchargeamount") ? ((Money)en_Ins["bsd_interestchargeamount"]).Value : 0;
                                    decimal d_PmsDtlInterestchargePaid = en_Ins.Contains("bsd_interestwaspaid") ? ((Money)en_Ins["bsd_interestwaspaid"]).Value : 0;
                                    int i_PmsDtlbsd_interestchargestatus = en_Ins.Contains("bsd_interestchargestatus") ? ((OptionSetValue)en_Ins["bsd_interestchargestatus"]).Value : 0;
                                    int i_PmsDtlActualgraceday = en_Ins.Contains("bsd_actualgracedays") ? (int)en_Ins["bsd_actualgracedays"] : 0;
                                    int i_bsd_previousdelays = en_Ins.Contains("bsd_previousdelays") ? (int)en_Ins["bsd_previousdelays"] : 0;
                                    traceService.Trace("5.4");
                                    if (d_PmsDtlInterestcharge > 0 && (i_PmsDtlbsd_interestchargestatus == 100000001 || d_PmsDtlInterestchargePaid > 0 ))//Paid
                                    {
                                        EntityCollection enclPayment = getTransactionPaymentByInterest(en_Ins, en_PM);
                                        if (enclPayment.Entities.Count > 0)
                                        {
                                        }
                                        else
                                        {
                                            throw new InvalidPluginExecutionException("You can't approve this void payment! You must void payment for interest of this installment!");
                                        }
                                    }
                                    d_PmsDtlAmPaid -= d_pmAMpay > d_pmBalance ? d_pmBalance : d_pmAMpay;
                                    d_PmsDtlBalance = d_PmsDtlAmPhase - d_PmsDtlAmPaid - d_PmsDtlDeposit;
                                    if (i_Pms_bsd_ordernumber == 1 && d_PmsDtlAmPaid == 0)
                                    {
                                        d_oe_bsd_totalamountpaid = d_PmsDtlDeposit; // luon luon cap nhat total amount of OE =
                                    }
                                    else
                                        d_oe_bsd_totalamountpaid -= d_pmAMpay > d_pmBalance ? d_pmBalance : d_pmAMpay;

                                    #region --------------- interestcharge > 0 --------------
                                    if (d_pmInterestAm > 0)
                                    {
                                        #region ---------------- LatePM = true ----------------
                                        if (f_latePM == true)
                                        {
                                            i_PmsDtlbsd_interestchargestatus = 100000000;
                                            d_PmsDtlInterestchargePaid -= d_pmInterestAm >= d_pmDifference ? d_pmDifference : d_pmInterestAm;

                                            d_PmsDtlInterestcharge -= d_pmInterestAm;
                                        }
                                        #endregion

                                        #region  --------- late pm = false ------------------
                                        else
                                        {
                                            d_PmsDtlInterestcharge -= d_pmInterestAm;
                                        }
                                        #endregion

                                        #region -------- actual late day -----------
                                        EntityCollection ec_paymentPrevious = get_previous_PM(service, en_Ins.Id, en_PM.Id, en_OE.Id);

                                        if (ec_paymentPrevious.Entities.Count <= 0)
                                        {
                                            if (i_bsd_previousdelays > 0) i_PmsDtlActualgraceday = i_bsd_previousdelays;
                                            else
                                                i_PmsDtlActualgraceday = 0;
                                        }
                                        else
                                        {
                                            i_PmsDtlActualgraceday = ec_paymentPrevious.Entities[0].Contains("bsd_latedays") ? (int)ec_paymentPrevious.Entities[0]["bsd_latedays"] : 0;
                                        }
                                        #endregion
                                        Entity tmp_Ins = new Entity(en_Ins.LogicalName); // entity Installment(PMSDtl)
                                        tmp_Ins.Id = en_Ins.Id;
                                        tmp_Ins["bsd_actualgracedays"] = i_PmsDtlActualgraceday;
                                        tmp_Ins["bsd_interestchargestatus"] = new OptionSetValue(i_PmsDtlbsd_interestchargestatus); // not paid
                                        tmp_Ins["bsd_interestchargeamount"] = new Money(d_PmsDtlInterestcharge > 0 ? d_PmsDtlInterestcharge : 0);
                                        tmp_Ins["bsd_interestwaspaid"] = new Money(d_PmsDtlInterestchargePaid > 0 ? d_PmsDtlInterestchargePaid : 0);

                                        service.Update(tmp_Ins);
                                    }
                                    #endregion

                                    #region  ---------------------- difference amount > 0  ------------------------------
                                    if (d_pmDifference > 0)
                                    {
                                        #region ---------------- Transaction PM ---------------------
                                        if (s_bsd_arraypsdid != "" || s_bsd_arrayinstallmentinterest != "" || s_bsd_arrayfees != "" || s_bsd_arraymicellaneousid != "")// update lai cac transaction pm cua pm
                                        {
                                            d_TransactionAM = update_transactionPM(service, ((EntityReference)en_voidPM["bsd_payment"]).Id, en_OE);
                                        }

                                        #endregion

                                        #region  ------------ advance payment -------------------------
                                        //update_AdvPM(((EntityReference)en_voidPM["bsd_payment"]).Id);

                                        // Update Statuscode Advance Payment khi Approve Void Payment
                                        EntityCollection ec_advPM = get_AdvPM(service, en_PM.Id);
                                        if (ec_advPM.Entities.Count > 0)
                                        {
                                            Entity en_Adv_up = new Entity(ec_advPM.Entities[0].LogicalName);
                                            en_Adv_up.Id = ec_advPM.Entities[0].Id;
                                            en_Adv_up["statuscode"] = new OptionSetValue(100000002); // revert
                                            service.Update(en_Adv_up);
                                        }
                                        #endregion
                                    }

                                    #endregion

                                    #region ------------------- OE & Unit status  ---------------
                                    // xet statuscode cua OE
                                    int i_Ustatus = 100000003; // deposit
                                    int i_oeStatus = 100000000;// option
                                    int sttOE = 100000002; // sign contract OE
                                    int sttUnit = 100000002; // unit = sold
                                    bool f_check_1st = false;
                                    f_check_1st = check_Ins_Paid(service, en_OE.Id, 1);
                                    EntityCollection psdFirst = GetPSD(en_OE.Id.ToString());
                                    int t = psdFirst.Entities.Count;
                                    Entity detailLast = psdFirst.Entities[t - 1]; // entity cuoi cung ( phase cuoi cung )
                                    string detailLastID = detailLast.Id.ToString();

                                    #region check & get status of Unit & OE
                                    if (i_Pms_bsd_ordernumber == 1)
                                    {
                                        if (en_OE.Contains("bsd_signedcontractdate"))
                                        {
                                            throw new InvalidPluginExecutionException("Contract has been signed. Cannot revert Void payment " + (string)en_voidPM["bsd_name"] + "!");
                                        }
                                        else
                                        {
                                            if (i_PmsDtlStatuscode == 100000000) // not paid
                                            {
                                                f_check_1st = false;
                                                sttOE = 100000000; // option
                                                sttUnit = 100000003; // deposit
                                            }
                                            else // paid
                                            {
                                                f_check_1st = true;
                                                sttOE = 100000001;//1st
                                                sttUnit = 100000001; // 1st
                                            }
                                        }
                                    }
                                    else
                                    {
                                        f_check_1st = true;
                                        if (!en_OE.Contains("bsd_signedcontractdate"))
                                        {
                                            sttOE = 100000001; // if OE not signcontract - status code still is 1st Installment
                                            sttUnit = 100000001; // 1st
                                        }
                                        else
                                        {
                                            sttUnit = 100000002; // sold
                                            sttOE = 100000003; //Being Payment (khi da sign contract)
                                            if (detailLastID == en_Ins.Id.ToString() && i_PmsDtlStatuscode == 100000001)
                                                sttOE = 100000004; //Complete Payment
                                        }
                                    }

                                    if (f_check_1st == false)
                                    {
                                        sttOE = i_oeStatus;
                                        sttUnit = i_Ustatus;
                                    }
                                    #endregion

                                    //update installment
                                    Entity en_tmp1 = new Entity(en_Ins.LogicalName);
                                    en_tmp1.Id = en_Ins.Id;
                                    en_tmp1["statuscode"] = new OptionSetValue(100000000); // not paid
                                    en_tmp1["bsd_paiddate"] = null;

                                    en_tmp1["bsd_amountwaspaid"] = new Money(d_PmsDtlAmPaid > 0 ? d_PmsDtlAmPaid : 0);
                                    en_tmp1["bsd_balance"] = new Money(d_PmsDtlBalance);
                                    //tmp_Ins["bsd_actualgracedays"] = i_PmsDtlActualgraceday;
                                    //tmp_Ins["bsd_interestchargestatus"] = new OptionSetValue(i_PmsDtlbsd_interestchargestatus); // not paid
                                    //tmp_Ins["bsd_interestchargeamount"] = new Money(d_PmsDtlInterestcharge > 0 ? d_PmsDtlInterestcharge : 0);

                                    //tmp_Ins["bsd_interestwaspaid"] = new Money(d_PmsDtlInterestchargePaid > 0 ? d_PmsDtlInterestchargePaid:0);
                                    service.Update(en_tmp1);

                                    // update OE
                                    // update lai total amount paid, total lateday
                                    // tinh lai & update total percent paid

                                    d_oe_bsd_totalamountpaid -= d_TransactionAM;

                                    en_OE_Up["bsd_totalamountpaid"] = new Money(d_oe_bsd_totalamountpaid > 0 ? d_oe_bsd_totalamountpaid : 0);
                                    en_OE_Up["statuscode"] = new OptionSetValue(sttOE);
                                    decimal d_bsd_totalpercent = (d_oe_bsd_totalamountpaid / d_oe_amountCalcPercent * 100);
                                    //   en_OE_Up["bsd_totalpercent"] = d_bsd_totalpercent;

                                    service.Update(en_OE_Up);

                                    Entity en_Unit = service.Retrieve(((EntityReference)en_OE["bsd_unitnumber"]).LogicalName, ((EntityReference)en_OE["bsd_unitnumber"]).Id,
                                        new ColumnSet(new string[] { "statuscode", "name" }));
                                    Entity en_Utmp = new Entity(en_Unit.LogicalName);
                                    en_Utmp.Id = en_Unit.Id;
                                    en_Utmp["statuscode"] = new OptionSetValue(sttUnit);
                                    service.Update(en_Utmp);

                                    #endregion

                                }
                                #endregion // end pm type = installment
                                traceService.Trace("9");
                                #region  ------------------ pm type = Other - MISS---------------------------
                                if (i_pmType == 100000005)
                                {
                                    // retrieve MISS
                                    traceService.Trace("9.1");
                                    //if (!en_PM.Contains("bsd_miscellaneous")) throw new InvalidPluginExecutionException("Can not find Misscellaneous information. Please check again!");
                                    if (en_PM.Contains("bsd_miscellaneous") == true)
                                    {
                                        traceService.Trace("9.1.2");
                                        //Nghiệp vụ cũ
                                        Entity en_Mis = service.Retrieve(((EntityReference)en_PM["bsd_miscellaneous"]).LogicalName, ((EntityReference)en_PM["bsd_miscellaneous"]).Id,
                                        new ColumnSet(new string[] { "bsd_name", "bsd_balance", "bsd_miscellaneousnumber", "statuscode","bsd_units","bsd_optionentry","bsd_amount","bsd_paidamount",
                                            "bsd_installment","bsd_installmentnumber"}));
                                        int f_MIS_status = en_Mis.Contains("statuscode") ? ((OptionSetValue)en_Mis["statuscode"]).Value : 1;

                                        decimal d_bsd_amount = en_Mis.Contains("bsd_amount") ? ((Money)en_Mis["bsd_amount"]).Value : 0;
                                        decimal d_bsd_paidamount = en_Mis.Contains("bsd_paidamount") ? ((Money)en_Mis["bsd_paidamount"]).Value : 0;
                                        decimal d_bsd_balance = en_Mis.Contains("bsd_balance") ? ((Money)en_Mis["bsd_balance"]).Value : 0;
                                        Entity en_MIS_up = new Entity(en_Mis.LogicalName);
                                        en_MIS_up.Id = en_Mis.Id;
                                        traceService.Trace("9.2");
                                        if (d_pmAMpay <= d_pmBalance)
                                        {
                                            d_bsd_paidamount -= d_pmAMpay;
                                            d_bsd_balance += d_pmAMpay;
                                            f_MIS_status = 1;
                                        }
                                        else // difference am > 0
                                        {
                                            d_bsd_paidamount -= d_pmBalance;
                                            d_bsd_balance += d_pmBalance;
                                            f_MIS_status = 1;
                                        }
                                        traceService.Trace("9.3");
                                        en_MIS_up["bsd_paidamount"] = new Money(d_bsd_paidamount);
                                        en_MIS_up["bsd_balance"] = new Money(d_bsd_balance);
                                        en_MIS_up["statuscode"] = new OptionSetValue(f_MIS_status);
                                        service.Update(en_MIS_up);
                                    }
                                    else
                                    {
                                        traceService.Trace("9.1.3");
                                        //Nghiệp vụ mới: Thạnh Đỗ
                                        //1 Lấy cái Miss trong Array Miscellaneous ID
                                        string missID = en_PM.Contains("bsd_arraymicellaneousid") ? (string)en_PM["bsd_arraymicellaneousid"] : "";
                                        string missAM = en_PM.Contains("bsd_arraymicellaneousamount") ? (string)en_PM["bsd_arraymicellaneousamount"] : "";
                                        traceService.Trace("missID: " + missID.ToString());
                                        traceService.Trace("missAM: " + missAM.ToString());
                                        if (missID != "")
                                        {
                                            string[] arrId = missID.Split(',');
                                            string[] arrMissAmount = missAM.Split(',');
                                            traceService.Trace("arrId: " + arrId.ToString());
                                            traceService.Trace("arrMissAmount: " + arrMissAmount.ToString());
                                            for (int i = 0; i < arrId.Length; i++)
                                            {
                                                string[] arr = arrId[i].Split('_');
                                                string micellaneousid = arr[0];
                                                decimal miss = decimal.Parse(arrMissAmount[i].ToString());

                                                Entity enMiss = getMiscellaneous(service, micellaneousid);
                                                traceService.Trace("arrId1");
                                                decimal bsd_paidamount = enMiss.Contains("bsd_paidamount") ? ((Money)enMiss["bsd_paidamount"]).Value : 0;
                                                decimal bsd_totalamount = enMiss.Contains("bsd_totalamount") ? ((Money)enMiss["bsd_totalamount"]).Value : 0;
                                                traceService.Trace("arrId2");
                                                bsd_paidamount -= miss;
                                                decimal bsd_balance = bsd_totalamount - bsd_paidamount;
                                                traceService.Trace("arrId3");
                                                traceService.Trace("arrId4");
                                                Entity en_MIS_up = new Entity(enMiss.LogicalName, enMiss.Id);
                                                //2 Cập lại sô tiền paid và balance của từng miss
                                                en_MIS_up["bsd_paidamount"] = new Money(bsd_paidamount);
                                                en_MIS_up["bsd_balance"] = new Money(bsd_balance);
                                                //3 Cập nhật lại trạng thái not paid
                                                en_MIS_up["statuscode"] = new OptionSetValue(1);
                                                traceService.Trace("arrId5");
                                                //4 Lưu lại thông tin của miss
                                                service.Update(en_MIS_up);
                                                traceService.Trace("arrId6");
                                            }
                                        }
                                    }

                                    traceService.Trace("9.4");
                                    #region ---------------- Transaction PM ---------------------
                                    if (s_bsd_arraypsdid != "" || s_bsd_arrayinstallmentinterest != "" || s_bsd_arrayfees != "" || s_bsd_arraymicellaneousid != "") // update lai cac transaction pm cua pm
                                    {
                                        d_TransactionAM = update_transactionPM(service, ((EntityReference)en_voidPM["bsd_payment"]).Id, en_OE);
                                    }
                                    #endregion
                                    traceService.Trace("9.5");
                                    //update_AdvPM(((EntityReference)en_voidPM["bsd_payment"]).Id);
                                    EntityCollection ec_advPM = get_AdvPM(service, en_PM.Id);
                                    if (ec_advPM.Entities.Count > 0)
                                    {
                                        Entity en_Adv_up = new Entity(ec_advPM.Entities[0].LogicalName);
                                        en_Adv_up.Id = ec_advPM.Entities[0].Id;
                                        en_Adv_up["statuscode"] = new OptionSetValue(100000002); // revert
                                        service.Update(en_Adv_up);
                                    }
                                    traceService.Trace("9.6");
                                }
                                #endregion ------------- end pm type = Other ---------------
                                traceService.Trace("10");
                                //update payment = revert
                                Entity en_tmp = new Entity(en_PM.LogicalName);
                                traceService.Trace("11");
                                en_tmp.Id = en_PM.Id;
                                en_tmp["statuscode"] = new OptionSetValue(100000002); // revert
                                service.Update(en_tmp);
                                traceService.Trace("12");

                            } // end payment type != deposit
                            #endregion
                            traceService.Trace("13");
                            #region ------------------ create a new payment copy from paymenet reverted --------------------------
                            Entity en_NewPM = new Entity(en_PM.LogicalName);
                            en_NewPM.Id = Guid.NewGuid();
                            traceService.Trace("14");

                            en_NewPM["bsd_name"] = (string)en_PM["bsd_name"] + "_Copy"; // active
                            en_NewPM["statuscode"] = new OptionSetValue(1); // active

                            en_NewPM["bsd_amountpay"] = new Money(d_pmAMpay);
                            en_NewPM["bsd_totalamountpayablephase"] = new Money(d_pmAMPhase); // active
                            en_NewPM["bsd_totalamountpaidphase"] = new Money(d_pmAMpaid); // active
                            en_NewPM["bsd_maintenancefee"] = new Money(d_pmMaintenance); // active
                            en_NewPM["bsd_managementfee"] = new Money(d_pmManagement); // active
                            en_NewPM["bsd_depositamount"] = new Money(d_pmDeposit); // active
                            en_NewPM["bsd_differentamount"] = new Money(d_pmDifference); // active
                            en_NewPM["bsd_balance"] = new Money(d_pmBalance); // active
                            en_NewPM["bsd_interestcharge"] = new Money(d_pmInterestAm); // active
                            en_NewPM["bsd_latedays"] = i_PmLateday; // active
                            traceService.Trace("15");

                            en_NewPM["bsd_latepayment"] = (bool)f_latePM;
                            en_NewPM["bsd_paymenttype"] = new OptionSetValue(i_pmType); // active
                            en_NewPM["bsd_units"] = (EntityReference)en_PM["bsd_units"];
                            en_NewPM["bsd_purchaser"] = (EntityReference)en_PM["bsd_purchaser"];
                            en_NewPM["bsd_paymentmode"] = (EntityReference)en_PM["bsd_paymentmode"];
                            en_NewPM["bsd_project"] = (EntityReference)en_PM["bsd_project"];
                            en_NewPM["bsd_transactiontype"] = new OptionSetValue(((OptionSetValue)en_PM["bsd_transactiontype"]).Value);
                            traceService.Trace("16");

                            en_NewPM["bsd_arraypsdid"] = en_PM.Contains("bsd_arraypsdid") ? (string)en_PM["bsd_arraypsdid"] : "";
                            en_NewPM["bsd_arrayamountpay"] = en_PM.Contains("bsd_arrayamountpay") ? (string)en_PM["bsd_arrayamountpay"] : "";
                            en_NewPM["bsd_arrayinstallmentinterest"] = en_PM.Contains("bsd_arrayinstallmentinterest") ? (string)en_PM["bsd_arrayinstallmentinterest"] : "";
                            en_NewPM["bsd_arrayinterestamount"] = en_PM.Contains("bsd_arrayinterestamount") ? (string)en_PM["bsd_arrayinterestamount"] : "";
                            traceService.Trace("17");

                            en_NewPM["bsd_arrayfees"] = en_PM.Contains("bsd_arrayfees") ? (string)en_PM["bsd_arrayfees"] : "";
                            en_NewPM["bsd_arrayfeesamount"] = en_PM.Contains("bsd_arrayfeesamount") ? (string)en_PM["bsd_arrayfeesamount"] : "";
                            traceService.Trace("18");

                            decimal d_bsd_waiverinstallment = en_PM.Contains("bsd_waiverinstallment") ? ((Money)en_PM["bsd_waiverinstallment"]).Value : 0;
                            decimal d_bsd_waiverinterest = en_PM.Contains("bsd_waiverinterest") ? ((Money)en_PM["bsd_waiverinterest"]).Value : 0;
                            decimal d_bsd_waiveramount = en_PM.Contains("bsd_waiveramount") ? ((Money)en_PM["bsd_waiveramount"]).Value : 0;
                            en_NewPM["bsd_waiverinstallment"] = new Money(d_bsd_waiverinstallment);
                            en_NewPM["bsd_waiverinterest"] = new Money(d_bsd_waiverinterest);
                            en_NewPM["bsd_waiveramount"] = new Money(d_bsd_waiveramount);
                            traceService.Trace("19");

                            // 170516 - update field bsd_createbyrevert = yes
                            en_NewPM["bsd_createbyrevert"] = true;
                            traceService.Trace("19.1");
                            if (en_PM.Contains("bsd_optionentry"))
                            {
                                traceService.Trace("19.2");
                                en_NewPM["bsd_optionentry"] = (EntityReference)en_PM["bsd_optionentry"];
                                traceService.Trace("19.2.1");
                                // en_NewPM["bsd_reservation"] = (EntityReference)en_PM["bsd_reservation"];
                                //holu
                                en_NewPM["bsd_paymentschemedetail"] = en_PM.Contains("bsd_paymentschemedetail") ? (EntityReference)en_PM["bsd_paymentschemedetail"] : null;
                                traceService.Trace("19.3");
                            }
                            else
                                en_NewPM["bsd_reservation"] = (EntityReference)en_PM["bsd_reservation"];

                            traceService.Trace("20");
                            en_NewPM["bsd_paymentactualtime"] = en_PM["bsd_paymentactualtime"];
                            en_NewPM["bsd_exchangeratetext"] = en_PM.Contains("bsd_exchangeratetext") ? en_PM["bsd_exchangeratetext"] : null;
                            en_NewPM["bsd_exchangemoneytext"] = en_PM.Contains("bsd_exchangemoneytext") ? en_PM["bsd_exchangemoneytext"] : null;
                            traceService.Trace("21");
                            // MISS 170814
                            if (en_PM.Contains("bsd_miscellaneous"))
                                en_NewPM["bsd_miscellaneous"] = (EntityReference)en_PM["bsd_miscellaneous"];
                            en_NewPM["bsd_arraymicellaneousid"] = en_PM.Contains("bsd_arraymicellaneousid") ? en_PM["bsd_arraymicellaneousid"] : "";
                            en_NewPM["bsd_arraymicellaneousamount"] = en_PM.Contains("bsd_arraymicellaneousamount") ? en_PM["bsd_arraymicellaneousamount"] : "";
                            traceService.Trace("22");
                            service.Create(en_NewPM);

                            // update new payment to void payment entity
                            Entity en_voidTmp = new Entity(en_voidPM.LogicalName);
                            en_voidTmp.Id = en_voidPM.Id;
                            en_voidTmp["bsd_newpayment"] = en_NewPM.ToEntityReference();
                            en_voidTmp["bsd_approvaldate"] = d_now;
                            en_voidTmp["bsd_approver"] = new EntityReference("systemuser", context.UserId);
                            service.Update(en_voidTmp);
                            traceService.Trace("23");
                            #endregion


                        } // end if void pm contain pm

                        #endregion // end update void pm for pm --------------------------------------------------

                        #region  ------------------- update void pm for applydocument -------------------------------------
                        if (en_voidPM.Contains("bsd_applydocument"))
                        {
                            #region  ------------------------------- retrieve apply document -------------------------------------
                            Entity en_app = service.Retrieve(((EntityReference)en_voidPM["bsd_applydocument"]).LogicalName, ((EntityReference)en_voidPM["bsd_applydocument"]).Id, new ColumnSet(new string[] {
                            "bsd_name",
                            "bsd_project",
                            "bsd_customer",
                            "bsd_units",
                            "bsd_optionentry",
                            "bsd_reservation",
                            "statuscode",
                            "bsd_paymentscheme",
                            "bsd_depositamount",
                            "bsd_transactiontype",
                            "bsd_arrayadvancepayment",
                            "bsd_arrayamountadvance",
                            "bsd_amountadvancepayment",
                            "bsd_arraypsdid",
                            "bsd_totalapplyamount",
                            "bsd_arrayamountpay",
                             "bsd_arrayinterestamount",
                            "bsd_arrayinstallmentinterest",
                            "bsd_applydocumentid",
                            "bsd_arrayfeesid",
                            "bsd_arrayfeesamount",
                            "bsd_arraymicellaneousid",
                            "bsd_arraymicellaneousamount"

                            }));

                            if (((OptionSetValue)en_app["statuscode"]).Value == 100000004)
                                throw new InvalidPluginExecutionException("Apply document '" + (string)en_app["bsd_name"] + "' has been revert!");

                            if (!en_app.Contains("bsd_transactiontype"))
                                throw new InvalidPluginExecutionException("Please choose 'Type of payment'!");

                            // bsd_transactiontype : 1: deposit // 2: installment
                            int i_bsd_transactiontype = ((OptionSetValue)en_app["bsd_transactiontype"]).Value;

                            #endregion

                            #region ------------------------------------------- check array input and amount input --------------------------------------------
                            // apply document - input advance payment
                            decimal d_bsd_amountadvancepayment = en_app.Contains("bsd_amountadvancepayment") ? ((Money)en_app["bsd_amountadvancepayment"]).Value : 0;
                            if (d_bsd_amountadvancepayment == 0) throw new InvalidPluginExecutionException("Cannot find 'Amount Advance Payment' of " + (string)en_app["bsd_name"] + "!");
                            if (!en_app.Contains("bsd_arrayadvancepayment")) throw new InvalidPluginExecutionException("Cannot find any advance payment data of " + (string)en_app["bsd_name"] + "!");
                            if (!en_app.Contains("bsd_arrayamountadvance")) throw new InvalidPluginExecutionException("Cannot find any amount of advance payment list of " + (string)en_app["bsd_name"] + "!");

                            string s_bsd_arrayadvancepayment = (string)en_app["bsd_arrayadvancepayment"];
                            string s_bsd_arrayamountadvance = (string)en_app["bsd_arrayamountadvance"];
                            string[] s_eachAdv = s_bsd_arrayadvancepayment.Split(',');
                            string[] s_amAdv = s_bsd_arrayamountadvance.Split(',');
                            int i_adv = s_eachAdv.Length;

                            // installment & deposit list
                            decimal d_bsd_totalapplyamount = en_app.Contains("bsd_totalapplyamount") ? ((Money)en_app["bsd_totalapplyamount"]).Value : 0;
                            if (d_bsd_totalapplyamount == 0) throw new InvalidPluginExecutionException("Cannot find total apply amount of " + (string)en_app["bsd_name"] + "!");

                            // Check Array ID
                            if (!en_app.Contains("bsd_arraypsdid") && !en_app.Contains("bsd_arrayinstallmentinterest") && !en_app.Contains("bsd_arrayfeesid") && !en_app.Contains("bsd_arraymicellaneousid"))
                                throw new InvalidPluginExecutionException("Please choose at least one row of " + ((i_bsd_transactiontype == 1) ? " Deposit list!" : " Installment or Interest or Fees or Misc List!"));

                            //Check Array Amount
                            if (!en_app.Contains("bsd_arrayamountpay") && !en_app.Contains("bsd_arrayinterestamount") && !en_app.Contains("bsd_arrayfeesamount") && !en_app.Contains("bsd_arraymicellaneousamount"))
                                throw new InvalidPluginExecutionException("Cannot find any amount of " + ((i_bsd_transactiontype == 1) ? " Deposit list!" : " Installment or Interest or Fees or Misc List!"));

                            if (d_bsd_totalapplyamount > d_bsd_amountadvancepayment) throw new InvalidPluginExecutionException("The amout you pay exceeds the amount paid. Please choose another Advance Payment or other Payment Phase!");

                            string s_bsd_arraypsdid = en_app.Contains("bsd_arraypsdid") ? (string)en_app["bsd_arraypsdid"] : "";
                            string s_bsd_arrayamountpay = en_app.Contains("bsd_arrayamountpay") ? (string)en_app["bsd_arrayamountpay"] : "";

                            string s_bsd_arrayinstallmentinterest = en_app.Contains("bsd_arrayinstallmentinterest") ? (string)en_app["bsd_arrayinstallmentinterest"] : "";
                            string s_bsd_arrayinterestamount = en_app.Contains("bsd_arrayinterestamount") ? (string)en_app["bsd_arrayinterestamount"] : "";

                            string s_fees = en_app.Contains("bsd_arrayfeesid") ? (string)en_app["bsd_arrayfeesid"] : "";
                            string s_feesAM = en_app.Contains("bsd_arrayfeesamount") ? (string)en_app["bsd_arrayfeesamount"] : "";
                            string s_bsd_arraymicellaneousid = en_app.Contains("bsd_arraymicellaneousid") ? (string)en_app["bsd_arraymicellaneousid"] : "";
                            string s_bsd_arraymicellaneousamount = en_app.Contains("bsd_arraymicellaneousamount") ? (string)en_app["bsd_arraymicellaneousamount"] : "";

                            decimal d_tienconlai = d_bsd_amountadvancepayment - d_bsd_totalapplyamount;
                            #endregion

                            #region --------------------- transaction type = 1 - deposit ------------------
                            if (i_bsd_transactiontype == 1)
                            {
                                string[] s_psd = s_bsd_arraypsdid.Split(',');
                                string[] s_Amp = s_bsd_arrayamountpay.Split(',');
                                int i_psd = s_psd.Length;

                                // update deposit ( Quote)
                                for (int m = 0; m < i_psd; m++)
                                {
                                    Up_payment_deposit(Guid.Parse(s_psd[m]), decimal.Parse(s_Amp[m]), d_now, en_app);
                                }

                            } // end of transaction type = deposit
                            #endregion

                            #region ---------------- transaction type = 2 - installment -----------------
                            if (i_bsd_transactiontype == 2) // installment
                            {
                                int i_Ustatus = 100000003; // deposit
                                int i_oeStatus = 100000000;// option

                                #region ------------ retrieve OE -----------

                                Entity en_OE = new Entity(((EntityReference)en_app["bsd_optionentry"]).LogicalName);
                                en_OE = service.Retrieve(((EntityReference)en_app["bsd_optionentry"]).LogicalName, ((EntityReference)en_app["bsd_optionentry"]).Id, new ColumnSet(new string[] {
                                    "ordernumber",
                                    "bsd_project",
                                    "statuscode",
                                    "customerid",
                                    "name",
                                    "bsd_managementfee",
                                    "bsd_freightamount",
                                    "bsd_paymentscheme",
                                    "bsd_signedcontractdate",
                                    "bsd_totalpercent",
                                    "bsd_totallatedays",
                                    "bsd_totalamountpaid",
                                    "totalamount",
                                    "bsd_unitnumber",
                                    "bsd_havemaintenancefee",
                                    "bsd_havemanagementfee",
                                    "bsd_maintenancefeesstatus",
                                    "bsd_managementfeesstatus",
                                    "salesorderid"
                                    }));

                                int i_bsd_totallatedays = en_OE.Contains("bsd_totallatedays") ? (int)en_OE["bsd_totallatedays"] : 0;
                                decimal d_oe_totalamount = en_OE.Contains("totalamount") ? ((Money)en_OE["totalamount"]).Value : 0;
                                decimal d_oe_bsd_totalamountpaid = en_OE.Contains("bsd_totalamountpaid") ? ((Money)en_OE["bsd_totalamountpaid"]).Value : 0;
                                int i_oe_statuscode = en_OE.Contains("statuscode") ? ((OptionSetValue)en_OE["statuscode"]).Value : 100000000;
                                int i_Unitstatus = 100000003;

                                decimal d_oe_bsd_totalpercent = en_OE.Contains("bsd_totalpercent") ? (decimal)en_OE["bsd_totalpercent"] : 0;

                                decimal d_oe_bsd_freightamount = en_OE.Contains("bsd_freightamount") ? ((Money)en_OE["bsd_freightamount"]).Value : 0;
                                decimal d_oe_amountCalcPercent = d_oe_totalamount - d_oe_bsd_freightamount;

                                bool f_OE_Signcontract = en_OE.Contains("bsd_signedcontractdate") ? true : false;
                                #endregion

                                EntityCollection psdFirst = GetPSD(en_OE.Id.ToString());
                                //Entity detailFirst = psdFirst.Entities[0];

                                #region  ------------ installment ----------------
                                if (s_bsd_arraypsdid != "")
                                {
                                    string[] s_psd = s_bsd_arraypsdid.Split(',');
                                    string[] s_Amp = s_bsd_arrayamountpay.Split(',');
                                    int i_psd = s_psd.Length;

                                    EntityCollection ec_ins = get_ecIns(service, s_psd);
                                    for (int i = 0; i < ec_ins.Entities.Count; i++)
                                    {
                                        bool f_check_1st = false;
                                        f_check_1st = check_Ins_Paid(service, en_OE.Id, 1);
                                        int t = psdFirst.Entities.Count;
                                        Entity detailLast = psdFirst.Entities[t - 1]; // entity cuoi cung ( phase cuoi cung )
                                        string detailLastID = detailLast.Id.ToString();

                                        int i_outstanding_AppDtl = 0;
                                        decimal d_ampay = 0;
                                        int sttOE = 100000002; // sign contract OE
                                        int sttUnit = 100000002; // unit = sold

                                        #region  ------------ retrieve installment-----------------------
                                        Entity en_pms = ec_ins.Entities[i];
                                        en_pms.Id = ec_ins.Entities[i].Id;

                                        if (!en_pms.Contains("statuscode")) throw new InvalidPluginExecutionException("Please check status code of '" + (string)en_pms["bsd_name"] + "!");
                                        int psd_statuscode = ((OptionSetValue)en_pms["statuscode"]).Value;

                                        if (!en_pms.Contains("bsd_amountofthisphase")) throw new InvalidPluginExecutionException("Installment " + (string)en_pms["bsd_name"] + " did not contain 'Amount of this phase'!");
                                        decimal psd_amountPhase = ((Money)en_pms["bsd_amountofthisphase"]).Value;
                                        decimal psd_amountPaid = en_pms.Contains("bsd_amountwaspaid") ? ((Money)en_pms["bsd_amountwaspaid"]).Value : 0;
                                        decimal psd_deposit = en_pms.Contains("bsd_depositamount") ? ((Money)en_pms["bsd_depositamount"]).Value : 0;
                                        decimal psd_waiver = en_pms.Contains("bsd_waiveramount") ? ((Money)en_pms["bsd_waiveramount"]).Value : 0;
                                        decimal psd_bsd_balance = en_pms.Contains("bsd_balance") ? ((Money)en_pms["bsd_balance"]).Value : 0;

                                        bool psd_bsd_maintenancefeesstatus = en_pms.Contains("bsd_maintenancefeesstatus") ? (bool)en_pms["bsd_maintenancefeesstatus"] : false;
                                        bool psd_bsd_managementfeesstatus = en_pms.Contains("bsd_managementfeesstatus") ? (bool)en_pms["bsd_managementfeesstatus"] : false;
                                        int i_phaseNum = (int)en_pms["bsd_ordernumber"];
                                        int i_bsd_actualgracedays = en_pms.Contains("bsd_actualgracedays") ? (int)en_pms["bsd_actualgracedays"] : 0;
                                        decimal psd_d_bsd_interestchargeamount = en_pms.Contains("bsd_interestchargeamount") ? ((Money)en_pms["bsd_interestchargeamount"]).Value : 0;
                                        #endregion  ----------

                                        #region ----------------- get oustanding day from appdtl of installment ( base on installment id ) --------------------
                                        EntityCollection ec_appDetail = get_appDtl_Ins(service, en_app.Id, en_pms.Id);
                                        decimal d_interAM = 0;
                                        if (ec_appDetail.Entities.Count > 0)
                                        {
                                            i_outstanding_AppDtl = ec_appDetail.Entities[0].Contains("bsd_actualgracedays") ? (int)ec_appDetail.Entities[0]["bsd_actualgracedays"] : 0;
                                            d_interAM = ec_appDetail.Entities[0].Contains("bsd_interestchargeamount") ? ((Money)ec_appDetail.Entities[0]["bsd_interestchargeamount"]).Value : 0;
                                            d_ampay = ((Money)ec_appDetail.Entities[0]["bsd_amountapply"]).Value;
                                        }
                                        #endregion

                                        #region check dieu kien

                                        bool f_checkIDagain = false;

                                        for (int m = s_psd.Length; m > 0; m--)
                                        {
                                            if (en_pms.Id.ToString() == s_psd[m - 1])
                                            {
                                                d_ampay = decimal.Parse(s_Amp[m - 1]);
                                                f_checkIDagain = true;
                                                break;
                                            }
                                        }

                                        if (f_checkIDagain == false) throw new InvalidPluginExecutionException("Cannot find ID of '" + (string)en_pms["bsd_name"] + "' in Installment array!");

                                        #endregion

                                        #region   ------------------------------------ update -----------------------------------------

                                        psd_amountPaid -= d_ampay;
                                        psd_bsd_balance += d_ampay;

                                        d_oe_bsd_totalamountpaid -= d_ampay;
                                        i_bsd_actualgracedays -= i_outstanding_AppDtl;
                                        i_bsd_totallatedays -= i_outstanding_AppDtl;
                                        if (d_oe_bsd_totalamountpaid <= 0) d_oe_bsd_totalamountpaid = 0;
                                        psd_statuscode = 100000000; // not paid

                                        #region check & get status of Unit & OE
                                        if (i_phaseNum == 1)
                                        {
                                            if (en_OE.Contains("bsd_signedcontractdate"))
                                            {
                                                throw new InvalidPluginExecutionException("Contract has been signed. Cannot revert Void payment " + (string)en_voidPM["bsd_name"] + "!");
                                            }
                                            else
                                            {
                                                if (psd_statuscode == 100000000) // not paid
                                                {
                                                    f_check_1st = false;
                                                    sttOE = 100000000; // option
                                                    sttUnit = 100000003; // deposit

                                                }
                                                else // paid
                                                {
                                                    f_check_1st = true;
                                                    sttOE = 100000001;//1st
                                                    sttUnit = 100000001; // 1st
                                                }
                                            }
                                        }
                                        else
                                        {
                                            f_check_1st = true;
                                            if (!en_OE.Contains("bsd_signedcontractdate"))
                                            {
                                                sttOE = 100000001; // if OE not signcontract - status code still is 1st Installment
                                                sttUnit = 100000001; // 1st
                                            }
                                            else
                                            {
                                                sttUnit = 100000002; // sold
                                                sttOE = 100000003; //Being Payment (khi da sign contract)
                                                if (detailLastID == en_pms.Id.ToString() && psd_statuscode == 100000001)
                                                    sttOE = 100000004; //Complete Payment
                                            }
                                        }

                                        if (f_check_1st == false)
                                        {
                                            sttOE = i_oeStatus;
                                            sttUnit = i_Ustatus;
                                        }
                                        #endregion

                                        Entity en_InsUp = new Entity(en_pms.LogicalName);
                                        en_InsUp.Id = en_pms.Id;

                                        en_InsUp["bsd_amountwaspaid"] = new Money(psd_amountPaid > 0 ? psd_amountPaid : 0);
                                        en_InsUp["bsd_balance"] = new Money(psd_bsd_balance);
                                        en_InsUp["statuscode"] = new OptionSetValue(psd_statuscode);
                                        en_InsUp["bsd_paiddate"] = null;

                                        en_InsUp["bsd_actualgracedays"] = i_bsd_actualgracedays;
                                        if (psd_d_bsd_interestchargeamount - d_interAM > 0)
                                            en_InsUp["bsd_interestchargeamount"] = new Money(psd_d_bsd_interestchargeamount -= d_interAM);
                                        else
                                            en_InsUp["bsd_interestchargeamount"] = new Money(0);
                                        service.Update(en_InsUp);

                                        Entity en_OEup = new Entity(en_OE.LogicalName);
                                        en_OEup.Id = en_OE.Id;

                                        en_OEup["bsd_totalamountpaid"] = new Money(d_oe_bsd_totalamountpaid);

                                        //  en_OEup["bsd_totalpercent"] = (d_oe_bsd_totalamountpaid / d_oe_amountCalcPercent) * 100;
                                        en_OEup["statuscode"] = new OptionSetValue(sttOE);
                                        service.Update(en_OEup);

                                        Entity Unit = service.Retrieve("product", ((EntityReference)en_OE["bsd_unitnumber"]).Id, new ColumnSet(new string[] { "bsd_handovercondition", "statuscode" }));
                                        Entity en_Unit_up = new Entity(Unit.LogicalName);
                                        en_Unit_up.Id = Unit.Id;
                                        en_Unit_up["statuscode"] = new OptionSetValue(sttUnit); // Unit statuscode = 1st Installment
                                        service.Update(en_Unit_up);
                                        #endregion

                                    } // end for each installmnet from array


                                } // end  if (s_bsd_arraypsdid != "")

                                #endregion

                                #region --------------------------------  interest charge -----------------------------
                                if (s_bsd_arrayinstallmentinterest != "")
                                {
                                    //string[] s_interID = s_bsd_arrayinstallmentinterest.Split(',');
                                    //string[] s_interAM = s_bsd_arrayinterestamount.Split(',');
                                    decimal d_ampay = 0;

                                    EntityCollection ec_appDtl_Inter = get_app_Inter(service, en_app.Id);

                                    for (int j = 0; j < ec_appDtl_Inter.Entities.Count; j++)
                                    {
                                        d_ampay = ec_appDtl_Inter.Entities[j].Contains("bsd_amountapply") ? ((Money)ec_appDtl_Inter.Entities[j]["bsd_amountapply"]).Value : 0;
                                        if (d_ampay == 0)
                                            throw new InvalidPluginExecutionException("Apply amount of Apply document detail " + (string)ec_appDtl_Inter.Entities[j]["bsd_name"] + " is equal 0. Please check again!");
                                        Entity en_insTmp = service.Retrieve(((EntityReference)ec_appDtl_Inter[j]["bsd_installment"]).LogicalName, ((EntityReference)ec_appDtl_Inter[j]["bsd_installment"]).Id, new ColumnSet(new string[] {
                                        "bsd_name",
                                        "bsd_actualgracedays",
                                        "bsd_interestchargestatus",
                                        "bsd_interestchargeamount",
                                        "statuscode",
                                        "bsd_interestwaspaid",
                                        "bsd_paymentschemedetailid"
                                    }));

                                        int i_actualday = en_insTmp.Contains("bsd_actualgracedays") ? (int)en_insTmp["bsd_actualgracedays"] : 0;
                                        decimal d_bsd_interestchargeamount = en_insTmp.Contains("bsd_interestchargeamount") ? ((Money)en_insTmp["bsd_interestchargeamount"]).Value : 0;
                                        decimal d_bsd_interestwaspaid = en_insTmp.Contains("bsd_interestwaspaid") ? ((Money)en_insTmp["bsd_interestwaspaid"]).Value : 0;

                                        d_bsd_interestwaspaid -= d_ampay;
                                        Entity en_up = new Entity(en_insTmp.LogicalName);
                                        en_up.Id = en_insTmp.Id;
                                        en_up["bsd_interestchargestatus"] = new OptionSetValue(100000000); // not paid
                                        en_up["bsd_interestwaspaid"] = new Money(d_bsd_interestwaspaid);
                                        service.Update(en_up);
                                    }


                                } // end of if s_arrayins interest !=""
                                #endregion

                                #region -------------------fees ----------------------------------
                                if (s_fees != "")
                                {
                                    string[] arrId = s_fees.Split(',');
                                    string[] arrAmount = s_feesAM.Split(',');
                                    bool newtype = true;
                                    for (int i = 0; i < arrId.Length; i++)
                                    {
                                        decimal voidAmount = Convert.ToDecimal(arrAmount[i]);
                                        string[] arr = arrId[i].Split('_');
                                        if (arr.Length == 2)
                                        {
                                            string installmentId = arr[0];
                                            string type = arr[1];
                                            Entity enInstallment = null;
                                            switch (type)
                                            {
                                                case "main":
                                                    enInstallment = service.Retrieve("bsd_paymentschemedetail", new Guid(installmentId), new ColumnSet(new string[] {
                                                    "bsd_maintenancefeesstatus", "bsd_maintenancefeepaid" }));
                                                    bool bsd_maintenancefeesstatus = enInstallment.Contains("bsd_maintenancefeesstatus") ? (bool)enInstallment["bsd_maintenancefeesstatus"] : false;
                                                    decimal bsd_maintenancefeepaid = enInstallment.Contains("bsd_maintenancefeepaid") ? ((Money)enInstallment["bsd_maintenancefeepaid"]).Value : 0;
                                                    bsd_maintenancefeesstatus = false;
                                                    bsd_maintenancefeepaid -= voidAmount;
                                                    d_oe_bsd_totalamountpaid -= voidAmount;
                                                    enInstallment["bsd_maintenancefeesstatus"] = bsd_maintenancefeesstatus;
                                                    enInstallment["bsd_maintenancefeepaid"] = new Money(bsd_maintenancefeepaid);

                                                    break;
                                                case "mana":
                                                    enInstallment = service.Retrieve("bsd_paymentschemedetail", new Guid(installmentId), new ColumnSet(new string[] {
                                                    "bsd_managementfeesstatus", "bsd_managementfeepaid" }));
                                                    bool bsd_managementfeesstatus = enInstallment.Contains("bsd_managementfeesstatus") ? (bool)enInstallment["bsd_managementfeesstatus"] : false;
                                                    decimal bsd_managementfeepaid = enInstallment.Contains("bsd_managementfeepaid") ? ((Money)enInstallment["bsd_managementfeepaid"]).Value : 0;
                                                    bsd_managementfeesstatus = false;
                                                    bsd_managementfeepaid -= voidAmount;
                                                    d_oe_bsd_totalamountpaid -= voidAmount;
                                                    enInstallment["bsd_managementfeesstatus"] = bsd_managementfeesstatus;
                                                    enInstallment["bsd_managementfeepaid"] = new Money(bsd_managementfeepaid);

                                                    break;
                                            }
                                            service.Update(enInstallment);
                                        }
                                        else
                                        {
                                            newtype = false;
                                            break;
                                        }
                                    }
                                    if (newtype == false)
                                    {
                                        EntityCollection ec_Ins_ES = get_Ins_ES(service, en_OE.Id.ToString());
                                        for (int j = 0; j < ec_Ins_ES.Entities.Count; j++)
                                        {
                                            bool f_main = (ec_Ins_ES.Entities[j].Contains("bsd_maintenancefeesstatus")) ? (bool)ec_Ins_ES.Entities[j]["bsd_maintenancefeesstatus"] : false;
                                            bool f_mana = (ec_Ins_ES.Entities[j].Contains("bsd_managementfeesstatus")) ? (bool)ec_Ins_ES.Entities[j]["bsd_managementfeesstatus"] : false;

                                            decimal d_bsd_maintenanceamount = ec_Ins_ES.Entities[j].Contains("bsd_maintenanceamount") ? ((Money)ec_Ins_ES.Entities[j]["bsd_maintenanceamount"]).Value : 0;
                                            decimal d_bsd_managementamount = ec_Ins_ES.Entities[j].Contains("bsd_managementamount") ? ((Money)ec_Ins_ES.Entities[j]["bsd_managementamount"]).Value : 0;

                                            decimal d_bsd_maintenancefeepaid = ec_Ins_ES.Entities[j].Contains("bsd_maintenancefeepaid") ? ((Money)ec_Ins_ES.Entities[j]["bsd_maintenancefeepaid"]).Value : 0;
                                            decimal d_bsd_managementfeepaid = ec_Ins_ES.Entities[j].Contains("bsd_managementfeepaid") ? ((Money)ec_Ins_ES.Entities[j]["bsd_managementfeepaid"]).Value : 0;
                                            decimal d_mainBL = d_bsd_maintenanceamount - d_bsd_maintenancefeepaid;
                                            decimal d_manaBL = d_bsd_managementamount - d_bsd_managementfeepaid;

                                            #endregion

                                            string s1 = s_fees.Substring(0, 1);
                                            string[] s_am = s_feesAM.Split(',');
                                            decimal d_am1 = decimal.Parse(s_am[0]);

                                            string s2 = "";
                                            decimal d_am2 = 0;
                                            if (s_fees.Length > 1)
                                            {
                                                s2 = s_fees.Substring(2, 1);
                                                d_am2 = decimal.Parse(s_am[1]);
                                            }
                                            #region ------------------------- Maintenance -----------------
                                            if (s1 == "1") // maintenance fees
                                            {
                                                d_bsd_maintenancefeepaid -= d_am1;
                                                f_main = false;
                                                d_oe_bsd_totalamountpaid -= d_am1;
                                            }
                                            else if (s1 == "2") // management
                                            {
                                                d_bsd_managementfeepaid -= d_am1;
                                                f_mana = false;
                                            }
                                            #endregion -------------------------end Maintenance -----------------

                                            if (s2 != "")
                                            {
                                                #region ------------------------- Maintenance -----------------
                                                if (s2 == "1") // maintenance fees
                                                {
                                                    d_bsd_maintenancefeepaid -= d_am2;
                                                    f_main = false;
                                                    d_oe_bsd_totalamountpaid -= d_am2;
                                                }
                                                #endregion -------------------------  end Maintenance -----------------

                                                #region ------------------------- Management -----------------
                                                else if (s2 == "2") // management
                                                {
                                                    d_bsd_managementfeepaid -= d_am2;
                                                    f_mana = false;
                                                }

                                            }
                                            Entity en_INS_update = new Entity(ec_Ins_ES.Entities[0].LogicalName);
                                            en_INS_update.Id = ec_Ins_ES.Entities[0].Id;
                                            en_INS_update["bsd_maintenancefeesstatus"] = f_main;
                                            en_INS_update["bsd_managementfeesstatus"] = f_mana;
                                            en_INS_update["bsd_maintenancefeepaid"] = new Money(d_bsd_maintenancefeepaid);
                                            en_INS_update["bsd_managementfeepaid"] = new Money(d_bsd_managementfeepaid);

                                            service.Update(en_INS_update);
                                        }
                                    }
                                    //#region retreive INS es
                                    //EntityCollection ec_Ins_ES = get_Ins_ES(service, en_OE.Id.ToString());
                                    ////if (ec_Ins_ES.Entities.Count < 0) throw new InvalidPluginExecutionException("Cannot find Estimate Handover Installment. Please check again!");
                                    //for (int i = 0; i < ec_Ins_ES.Entities.Count; i++)
                                    //{
                                    //    bool f_main = (ec_Ins_ES.Entities[i].Contains("bsd_maintenancefeesstatus")) ? (bool)ec_Ins_ES.Entities[i]["bsd_maintenancefeesstatus"] : false;
                                    //    bool f_mana = (ec_Ins_ES.Entities[i].Contains("bsd_managementfeesstatus")) ? (bool)ec_Ins_ES.Entities[i]["bsd_managementfeesstatus"] : false;

                                    //    decimal d_bsd_maintenanceamount = ec_Ins_ES.Entities[i].Contains("bsd_maintenanceamount") ? ((Money)ec_Ins_ES.Entities[i]["bsd_maintenanceamount"]).Value : 0;
                                    //    decimal d_bsd_managementamount = ec_Ins_ES.Entities[i].Contains("bsd_managementamount") ? ((Money)ec_Ins_ES.Entities[i]["bsd_managementamount"]).Value : 0;

                                    //    decimal d_bsd_maintenancefeepaid = ec_Ins_ES.Entities[i].Contains("bsd_maintenancefeepaid") ? ((Money)ec_Ins_ES.Entities[i]["bsd_maintenancefeepaid"]).Value : 0;
                                    //    decimal d_bsd_managementfeepaid = ec_Ins_ES.Entities[i].Contains("bsd_managementfeepaid") ? ((Money)ec_Ins_ES.Entities[i]["bsd_managementfeepaid"]).Value : 0;

                                    //    decimal d_mainBL = d_bsd_maintenanceamount - d_bsd_maintenancefeepaid;
                                    //    decimal d_manaBL = d_bsd_managementamount - d_bsd_managementfeepaid;

                                    //    #endregion

                                    //string s1 = s_fees.Substring(0, 1);
                                    //string[] s_am = s_feesAM.Split(',');
                                    //decimal d_am1 = decimal.Parse(s_am[0]);

                                    //string s2 = "";
                                    //decimal d_am2 = 0;
                                    //if (s_fees.Length > 1)
                                    //{
                                    //    s2 = s_fees.Substring(2, 1);
                                    //    d_am2 = decimal.Parse(s_am[1]);
                                    //}

                                    //#region ------------------------- Maintenance -----------------
                                    //if (s1 == "1") // maintenance fees
                                    //{
                                    //    d_bsd_maintenancefeepaid -= d_am1;
                                    //    f_main = false;
                                    //    d_oe_bsd_totalamountpaid -= d_am1;
                                    //}
                                    //else if (s1 == "2") // management
                                    //{
                                    //    d_bsd_managementfeepaid -= d_am1;
                                    //    f_mana = false;
                                    //}

                                    //#endregion -------------------------end Maintenance -----------------

                                    //if (s2 != "")
                                    //{
                                    //    #region ------------------------- Maintenance -----------------
                                    //    if (s2 == "1") // maintenance fees
                                    //    {
                                    //        d_bsd_maintenancefeepaid -= d_am2;
                                    //        f_main = false;
                                    //        d_oe_bsd_totalamountpaid -= d_am2;
                                    //    }
                                    //    #endregion -------------------------  end Maintenance -----------------

                                    //    #region ------------------------- Management -----------------
                                    //    else if (s2 == "2") // management
                                    //    {
                                    //        d_bsd_managementfeepaid -= d_am2;
                                    //        f_mana = false;
                                    //    }
                                    //    #endregion ------------------------- end Management -----------------
                                    //}
                                    //Entity en_INS_update = new Entity(ec_Ins_ES.Entities[0].LogicalName);
                                    //en_INS_update.Id = ec_Ins_ES.Entities[0].Id;
                                    //en_INS_update["bsd_maintenancefeesstatus"] = f_main;
                                    //en_INS_update["bsd_managementfeesstatus"] = f_mana;
                                    //en_INS_update["bsd_maintenancefeepaid"] = new Money(d_bsd_maintenancefeepaid);
                                    //en_INS_update["bsd_managementfeepaid"] = new Money(d_bsd_managementfeepaid);
                                    //service.Update(en_INS_update);
                                    //}



                                    // OE
                                    Entity en_oeUPdate = new Entity(en_OE.LogicalName);
                                    en_oeUPdate.Id = en_OE.Id;
                                    en_oeUPdate["bsd_totalamountpaid"] = new Money(d_oe_bsd_totalamountpaid);

                                    service.Update(en_oeUPdate);
                                }

                                #endregion --------------------------------

                                #region ----------------------------MISS---------------------------
                                if (s_bsd_arraymicellaneousid != "")
                                {
                                    //string[] s_MissID = s_bsd_arraymicellaneousid.Split(',');
                                    //string[] s_MissAM = s_bsd_arraymicellaneousamount.Split(',');
                                    decimal d_ampay = 0;

                                    EntityCollection ec_appDtl_Mis = get_app_MISS(service, en_app.Id);
                                    if (ec_appDtl_Mis.Entities.Count > 0)
                                    {
                                        for (int j = 0; j < ec_appDtl_Mis.Entities.Count; j++)
                                        {
                                            d_ampay = ec_appDtl_Mis.Entities[j].Contains("bsd_amountapply") ? ((Money)ec_appDtl_Mis.Entities[j]["bsd_amountapply"]).Value : 0;
                                            if (d_ampay == 0)
                                                throw new InvalidPluginExecutionException("Apply amount of Apply document detail " + (string)ec_appDtl_Mis.Entities[j]["bsd_name"] + " is equal 0. Please check again!");
                                            Entity en_misTmp = service.Retrieve(((EntityReference)ec_appDtl_Mis[j]["bsd_miscellaneous"]).LogicalName, ((EntityReference)ec_appDtl_Mis[j]["bsd_miscellaneous"]).Id,
                                                new ColumnSet(new string[] {
                                                "bsd_balance",
                                                "statuscode",
                                                "bsd_miscellaneousnumber",
                                                "bsd_units",
                                                "bsd_optionentry",
                                                "bsd_miscellaneousid",
                                                "bsd_amount",
                                                "bsd_paidamount",
                                                "bsd_installment",
                                                "bsd_name",
                                                "bsd_project",
                                                "bsd_installmentnumber"
                                            }));

                                            decimal d_bsd_paidamount = en_misTmp.Contains("bsd_paidamount") ? ((Money)en_misTmp["bsd_paidamount"]).Value : 0;
                                            decimal d_bsd_balance = en_misTmp.Contains("bsd_balance") ? ((Money)en_misTmp["bsd_balance"]).Value : 0;

                                            d_bsd_paidamount -= d_ampay;
                                            d_bsd_balance += d_ampay;

                                            Entity en_up = new Entity(en_misTmp.LogicalName);
                                            en_up.Id = en_misTmp.Id;
                                            en_up["bsd_paidamount"] = new Money(d_bsd_paidamount);
                                            en_up["bsd_balance"] = new Money(d_bsd_balance);
                                            en_up["statuscode"] = new OptionSetValue(1); // not paid
                                            service.Update(en_up);
                                        }

                                    }
                                }

                                #endregion ----------------------- end MISS ---------------------

                            } // end of transaction type = installment
                            #endregion

                            #region ---------------  update advance payment list -------------------
                            decimal d_tmp = d_bsd_totalapplyamount;
                            for (int n = 0; n < i_adv; n++)
                            {
                                if (d_tmp == 0) break;
                                // so tien can tra cho applydoc = so tien cua adv check dau tien - k can update nua - chi update cho adv nay thoi
                                if (d_tmp == decimal.Parse(s_amAdv[n]))
                                {
                                    revert_Up_adv(Guid.Parse(s_eachAdv[n]), decimal.Parse(s_amAdv[n])); // payoff
                                    d_tmp = 0;
                                    break;
                                }
                                if (d_tmp < decimal.Parse(s_amAdv[n]))
                                {
                                    revert_Up_adv(Guid.Parse(s_eachAdv[n]), d_tmp);
                                    d_tmp = 0;
                                    break;
                                }
                                if (d_tmp > decimal.Parse(s_amAdv[n]))
                                {
                                    revert_Up_adv(Guid.Parse(s_eachAdv[n]), decimal.Parse(s_amAdv[n])); // payoff
                                    d_tmp -= decimal.Parse(s_amAdv[n]);
                                }
                            }
                            #endregion --------------- end update adv ----------------

                            #region update apply document
                            Entity en_appUp = new Entity(en_app.LogicalName);
                            en_appUp.Id = en_app.Id;
                            en_appUp["statuscode"] = new OptionSetValue(100000004);
                            service.Update(en_appUp);
                            #endregion

                            #region update apply dtl
                            EntityCollection ec_appDtl = get_appDtl(service, en_app.Id);
                            if (ec_appDtl.Entities.Count > 0)
                            {
                                for (int k = 0; k < ec_appDtl.Entities.Count; k++)
                                {
                                    Entity en_dtl = new Entity(ec_appDtl.Entities[k].LogicalName);
                                    en_dtl.Id = ec_appDtl.Entities[k].Id;
                                    en_dtl["statuscode"] = new OptionSetValue(100000002);
                                    service.Update(en_dtl);
                                }
                            }
                            #endregion

                        } // end region of en_voidPM contain apply document
                        #endregion ------------------ end update voidpm for apply ---------------------------

                    }

                    //throw new InvalidPluginExecutionException("HanHan");
                    #endregion ------------ end approve ---------------------

                    #region  -------------------------------Reject ------------------------------------------
                    if (((OptionSetValue)target["statuscode"]).Value == 100000001)   // = reject
                    {
                        Entity en_tmp = new Entity(en_voidPM.Contains("bsd_payment") ? ((EntityReference)en_voidPM["bsd_payment"]).LogicalName : ((EntityReference)en_voidPM["bsd_applydocument"]).LogicalName);
                        en_tmp.Id = en_voidPM.Contains("bsd_payment") ? ((EntityReference)en_voidPM["bsd_payment"]).Id : ((EntityReference)en_voidPM["bsd_applydocument"]).Id;
                        en_tmp["statuscode"] = new OptionSetValue(100000000);
                        service.Update(en_tmp);
                    }

                    #endregion --------------- end reject --------------------------------------------

                    //throw new InvalidPluginExecutionException("aaaaaaaaaaaaa");
                } // END OF CONTEXT = UPDATE

                #endregion

                #region ------- create New VoidPM -------------
                // create new record - update statuscode of payment to pending revert
                if (context.MessageName == "Create")
                {
                    // khi create cho payment thi change status cua payment thanh pending revert - tuong tu cho apply document
                    #region ------ void for payment  -----------------
                    if (en_voidPM.Contains("bsd_payment"))
                    {
                        traceService.Trace("a");
                        Entity en_PM = service.Retrieve(((EntityReference)en_voidPM["bsd_payment"]).LogicalName, ((EntityReference)en_voidPM["bsd_payment"]).Id,
                            new ColumnSet(true));
                        Entity en_Unit = service.Retrieve(((EntityReference)en_PM["bsd_units"]).LogicalName, ((EntityReference)en_PM["bsd_units"]).Id,
                            new ColumnSet(true));
                        traceService.Trace("b");
                        #region  ----------- check status of unit when revert payment --------------------
                        if (en_PM.Contains("bsd_reservation"))
                        {
                            traceService.Trace("c");
                            Entity en_quote = service.Retrieve(((EntityReference)en_PM["bsd_reservation"]).LogicalName,
                                              ((EntityReference)en_PM["bsd_reservation"]).Id, new ColumnSet(true));
                            traceService.Trace("iii");
                            int i_ResvStatus = ((OptionSetValue)en_quote["statuscode"]).Value;
                            traceService.Trace("d");
                            switch (i_ResvStatus)
                            {
                                case 4:
                                    throw new InvalidPluginExecutionException("Reservation " + (string)en_quote["name"] + " has already attached with an Option Entry! Can not create void payment!");
                                    break;
                                case 6:
                                    throw new InvalidPluginExecutionException("Reservation " + (string)en_quote["name"] + " has been Canceled. Can not create void payment!");
                                    break;
                                case 100000001:
                                    throw new InvalidPluginExecutionException("Reservation " + (string)en_quote["name"] + " has been Terminated. Can not create void payment!");
                                    break;
                                case 100000003:
                                    throw new InvalidPluginExecutionException("Reservation " + (string)en_quote["name"] + " has been Reject. Can not create void payment!");
                                    break;
                                case 100000002:
                                    throw new InvalidPluginExecutionException("Reservation " + (string)en_quote["name"] + " has been Pending Cancel Deposit. Can not create void payment!");
                            }
                            traceService.Trace("e");
                        }
                        else
                        {
                            traceService.Trace("f");
                            if (en_PM.Contains("bsd_paymentschemedetail"))
                            {
                                Entity en_Ins = service.Retrieve(((EntityReference)en_PM["bsd_paymentschemedetail"]).LogicalName,
                                     ((EntityReference)en_PM["bsd_paymentschemedetail"]).Id, new ColumnSet(
                                         new string[] {
                                        "bsd_ordernumber",
                                        "statuscode"
                                         }));
                                if (en_Ins.Contains("bsd_ordernumber") && (int)en_Ins["bsd_ordernumber"] == 1)
                                {
                                    if (((OptionSetValue)en_Unit["statuscode"]).Value == 100000002) // sold
                                        throw new InvalidPluginExecutionException("Contract has been signed. Cannot create Void payment for " + (string)en_PM["bsd_name"] + "!");
                                }
                            }

                            traceService.Trace("g");
                        }
                        #endregion
                        traceService.Trace("h");
                        Entity en_tmp = new Entity(en_PM.LogicalName);
                        en_tmp.Id = en_PM.Id;
                        en_tmp["statuscode"] = new OptionSetValue(100000001); // pending revert
                        service.Update(en_tmp);

                        // find advance payment connect with current payment of voidPM
                        EntityCollection ec_adv = get_advFrompmID(service, en_PM.Id);
                        traceService.Trace("i");
                        if (ec_adv.Entities.Count > 0)
                        {
                            traceService.Trace("k");
                            foreach (Entity en_AdvUp in ec_adv.Entities)
                            {
                                Entity en_Up = new Entity(en_AdvUp.LogicalName);
                                en_Up.Id = en_AdvUp.Id;
                                en_Up["statuscode"] = new OptionSetValue(100000003); // pending revert advance payment
                                traceService.Trace("aaaaaaa");
                                service.Update(en_Up);
                                traceService.Trace("bbbbbbb");
                            }
                        }
                        //Update date pending revert Transaction Payments Thạnh Đỗ
                        //EntityCollection encolTransactionPM = get_TransactionPM(service, en_PM.Id);
                        //if (encolTransactionPM.Entities.Count > 0)
                        //{
                        //    foreach (Entity enTransactionPMUP in encolTransactionPM.Entities)
                        //    {
                        //        Entity enUp = new Entity(enTransactionPMUP.LogicalName);
                        //        enUp.Id = enTransactionPMUP.Id;
                        //        enUp["statuscode"] = new OptionSetValue(100000003); // pending revert advance payment
                        //        service.Update(enUp);
                        //    }
                        //}

                    }
                    #endregion -----------end create void pm ----------------

                    #region ------ apply payment -----------
                    if (en_voidPM.Contains("bsd_applydocument"))
                    {
                        Entity en_app = service.Retrieve(((EntityReference)en_voidPM["bsd_applydocument"]).LogicalName, ((EntityReference)en_voidPM["bsd_applydocument"]).Id,
                         new ColumnSet(new string[] { "statuscode", "bsd_name",
                            "bsd_units",
                            "bsd_optionentry",
                            "bsd_reservation",
                            "bsd_totalapplyamount"
                         }));

                        if (en_app.Contains("bsd_reservation"))
                        {
                            Entity en_quote = service.Retrieve(((EntityReference)en_app["bsd_reservation"]).LogicalName, ((EntityReference)en_app["bsd_reservation"]).Id,
                                new ColumnSet(new string[] {
                                    "name","statuscode"
                                }));
                            if (en_quote.Contains("statuscode") && ((OptionSetValue)en_quote["statuscode"]).Value == 4) // won ( da convert sang OE)
                                throw new InvalidPluginExecutionException("Quotation Reservation " + (string)en_quote["name"] + " has been converted to Option Entry. Cannot create Void Payment!");
                        }

                        Entity en_tmp = new Entity(en_app.LogicalName);
                        en_tmp.Id = en_app.Id;
                        en_tmp["statuscode"] = new OptionSetValue(100000003); // pending revert
                        service.Update(en_tmp);

                        // update for app dtl
                        EntityCollection ec_appDtl = get_appDtl(service, en_app.Id);
                        if (ec_appDtl.Entities.Count > 0)
                        {
                            for (int k = 0; k < ec_appDtl.Entities.Count; k++)
                            {
                                Entity en_dtl = new Entity(ec_appDtl.Entities[k].LogicalName);
                                en_dtl.Id = ec_appDtl.Entities[k].Id;
                                en_dtl["statuscode"] = new OptionSetValue(100000003); // pending revert

                                service.Update(en_dtl);
                            }
                        }

                        Entity en_targetUp = new Entity(en_voidPM.LogicalName);
                        en_targetUp.Id = en_voidPM.Id;

                        decimal bsd_amount = en_app.Contains("bsd_totalapplyamount") ? ((Money)en_app["bsd_totalapplyamount"]).Value : 0;
                        en_targetUp["bsd_amount"] = new Money(bsd_amount);
                        en_targetUp["bsd_totalapplyamount"] = new Money(bsd_amount);
                        service.Update(en_targetUp);

                    }
                    #endregion  ----------- create void apply ---------------
                }
                #endregion


            } // end of target = "bsd_voidpayment"

        }

        #region Funtion

        private EntityCollection get_ec_InsNew(IOrganizationService crmservices, Guid oeId)
        {
            string fetchXml =
                @"<fetch version='1.0' output-format='xml-platform' mapping='logical' distinct='false'>
                 <entity name='bsd_paymentschemedetail' >
                    <attribute name='bsd_name' />
                    <attribute name='bsd_optionentry' />
                    <attribute name='bsd_ordernumber' />
                    <attribute name='bsd_duedatecalculatingmethod' />
                    <attribute name='statuscode' />
                    <attribute name='bsd_lastinstallment' />
                    <attribute name='bsd_paymentschemedetailid' />
                    <filter type='and' >
                      <condition attribute='bsd_optionentry' operator='eq' value='{0}' />
                      <condition attribute='statuscode' operator='eq' value='100000001' />
                    </filter>
                    <order attribute='bsd_ordernumber' />
                  </entity>
                </fetch>";
            fetchXml = string.Format(fetchXml, oeId);
            EntityCollection entc = crmservices.RetrieveMultiple(new FetchExpression(fetchXml));
            return entc;
        }

        private decimal update_transactionPM(IOrganizationService crmser, Guid pmID, Entity en_OE)
        {
            decimal oePaid = 0;
            EntityCollection ec_transactionPM = get_TransactionPM(service, pmID);

            if (ec_transactionPM.Entities.Count > 0)
            {
                foreach (Entity en_trans in ec_transactionPM.Entities)
                {
                    Entity en_tmp_trans = new Entity(en_trans.LogicalName);
                    en_tmp_trans.Id = en_trans.Id;
                    en_tmp_trans["statuscode"] = new OptionSetValue(100000002); // revert
                    service.Update(en_tmp_trans);

                    //Han
                    int i_trans_bsd_transactiontype = en_trans.Contains("bsd_transactiontype") ? ((OptionSetValue)en_trans["bsd_transactiontype"]).Value : 0;
                    decimal d_trans_bsd_amount = en_trans.Contains("bsd_amount") ? ((Money)en_trans["bsd_amount"]).Value : 0;
                    int i_bsd_feetype = en_trans.Contains("bsd_feetype") ? ((OptionSetValue)en_trans["bsd_feetype"]).Value : 0;

                    // update amount paid of installment or interestcharge of installment in savegriddata
                    if (i_trans_bsd_transactiontype != 100000002)
                        oePaid += update_INS_transc(crmser, en_trans, en_OE);
                    //Thạnh Đỗ
                    //else //Fee = 100000002
                    //{
                    //    #region  ----------------- retrive installment in transaction payment---------------
                    //    Entity en_Ins = crmser.Retrieve("bsd_paymentschemedetail", ((EntityReference)en_trans["bsd_installment"]).Id,
                    //        new ColumnSet(true));
                    //    #endregion

                    //    bool f_main = en_Ins.Contains("bsd_maintenancefeesstatus") ? (bool)en_Ins["bsd_maintenancefeesstatus"] : false;
                    //    bool f_mana = en_Ins.Contains("bsd_managementfeesstatus") ? (bool)en_Ins["bsd_managementfeesstatus"] : false;

                    //    Entity en_Ins_Update = new Entity(en_Ins.LogicalName);
                    //    en_Ins_Update.Id = en_Ins.Id;

                    //    if (i_bsd_feetype == 100000000) // main
                    //    {
                    //        decimal d_bsd_maintenancefeepaid = en_Ins.Contains("bsd_maintenancefeepaid") ? ((Money)en_Ins["bsd_maintenancefeepaid"]).Value : 0;
                    //        f_main = false;
                    //        d_bsd_maintenancefeepaid -= d_trans_bsd_amount;
                    //        en_Ins_Update["bsd_maintenancefeepaid"] = new Money(d_bsd_maintenancefeepaid);
                    //        en_Ins_Update["bsd_maintenancefeesstatus"] = f_main;
                    //    }
                    //    if (i_bsd_feetype == 100000001)// mana
                    //    {
                    //        decimal d_bsd_managementfeepaid = en_Ins.Contains("bsd_managementfeepaid") ? ((Money)en_Ins["bsd_managementfeepaid"]).Value : 0;

                    //        f_mana = false;
                    //        d_bsd_managementfeepaid -= d_trans_bsd_amount;
                    //        en_Ins_Update["bsd_managementfeepaid"] = new Money(d_bsd_managementfeepaid);
                    //        en_Ins_Update["bsd_managementfeesstatus"] = f_mana;
                    //    }
                    //    crmser.Update(en_Ins_Update);

                    //}
                }
            }
            return oePaid;
        }

        private decimal update_INS_transc(IOrganizationService serv, Entity en_trans, Entity en_OE)
        {
            decimal d_oePiad = 0;
            #region  ----------------- retrive installment in transaction payment---------------
            Entity en_Ins = serv.Retrieve("bsd_paymentschemedetail", ((EntityReference)en_trans["bsd_installment"]).Id,
                new ColumnSet(new string[]{
                "bsd_amountofthisphase",
                "bsd_amountwaspaid",
                "bsd_depositamount",
                "statuscode",
                "bsd_duedatecalculatingmethod",
                "bsd_maintenancefees",
                "bsd_managementfee",
                "bsd_paymentscheme",
                "bsd_lastinstallment",
                "bsd_balance",
                "bsd_actualgracedays",
                "bsd_interestchargeamount",
                "bsd_interestchargestatus",
                "bsd_interestwaspaid",
                "bsd_maintenancefeesstatus",
                "bsd_managementfeesstatus",
                "bsd_managementamount",
                "bsd_maintenanceamount",
                "bsd_maintenancefeepaid",
                "bsd_managementfeepaid",
                "bsd_paiddate"
            }));
            #endregion

            decimal d_bsd_amountofthisphase = en_Ins.Contains("bsd_amountofthisphase") ? ((Money)en_Ins["bsd_amountofthisphase"]).Value : 0;
            decimal d_bsd_amountwaspaid = en_Ins.Contains("bsd_amountwaspaid") ? ((Money)en_Ins["bsd_amountwaspaid"]).Value : 0;
            int i_INS_statuscode = en_Ins.Contains("statuscode") ? ((OptionSetValue)en_Ins["statuscode"]).Value : 0;
            decimal d_bsd_balance = en_Ins.Contains("bsd_balance") ? ((Money)en_Ins["bsd_balance"]).Value : 0;

            decimal d_bsd_interestchargeamount = en_Ins.Contains("bsd_interestchargeamount") ? ((Money)en_Ins["bsd_interestchargeamount"]).Value : 0;
            decimal d_bsd_interestwaspaid = en_Ins.Contains("bsd_interestwaspaid") ? ((Money)en_Ins["bsd_interestwaspaid"]).Value : 0;
            int i_bsd_interestchargestatus = en_Ins.Contains("bsd_interestchargestatus") ? ((OptionSetValue)en_Ins["bsd_interestchargestatus"]).Value : 100000000;
            int i_bsd_actualgracedays = en_Ins.Contains("bsd_actualgracedays") ? (int)en_Ins["bsd_actualgracedays"] : 0;

            decimal d_bsd_maintenanceamount = en_Ins.Contains("bsd_maintenanceamount") ? ((Money)en_Ins["bsd_maintenanceamount"]).Value : 0;
            decimal d_bsd_managementamount = en_Ins.Contains("bsd_managementamount") ? ((Money)en_Ins["bsd_managementamount"]).Value : 0;
            decimal d_bsd_maintenancefeepaid = en_Ins.Contains("bsd_maintenancefeepaid") ? ((Money)en_Ins["bsd_maintenancefeepaid"]).Value : 0;
            decimal d_bsd_managementfeepaid = en_Ins.Contains("bsd_managementfeepaid") ? ((Money)en_Ins["bsd_managementfeepaid"]).Value : 0;

            bool f_main = en_Ins.Contains("bsd_maintenancefeesstatus") ? (bool)en_Ins["bsd_maintenancefeesstatus"] : false;
            bool f_mana = en_Ins.Contains("bsd_managementfeesstatus") ? (bool)en_Ins["bsd_managementfeesstatus"] : false;


            Entity en_Ins_Update = new Entity(en_Ins.LogicalName);
            en_Ins_Update.Id = en_Ins.Id;

            // kiem tra transaction type
            //Installments    100000000
            //Interest        100000001
            //Fees            100000002
            //Other           100000003
            int i_trans_bsd_transactiontype = en_trans.Contains("bsd_transactiontype") ? ((OptionSetValue)en_trans["bsd_transactiontype"]).Value : 0;
            decimal d_trans_bsd_amount = en_trans.Contains("bsd_amount") ? ((Money)en_trans["bsd_amount"]).Value : 0;
            int i_bsd_feetype = en_trans.Contains("bsd_feetype") ? ((OptionSetValue)en_trans["bsd_feetype"]).Value : 0;
            decimal d_IC_am = en_trans.Contains("bsd_interestchargeamount") ? ((Money)en_trans["bsd_interestchargeamount"]).Value : 0;
            int i_outstandingday = en_trans.Contains("bsd_actualgracedays") ? (int)en_trans["bsd_actualgracedays"] : 0;

            #region --------- transaction type = interestcharge = 10000001 -----------
            if (i_trans_bsd_transactiontype == 100000001) // update interestcharge trong installment
            {
                // revert - thi ins da tre- k can revert lai so ngay outstanding day
                if (i_bsd_interestchargestatus == 100000001) i_bsd_interestchargestatus = 100000000;
                d_bsd_interestwaspaid -= d_trans_bsd_amount;

                // k update lai lateday cua installment & OE vi interestcharge nay co san trong installment r
                en_Ins_Update["bsd_interestchargestatus"] = new OptionSetValue(i_bsd_interestchargestatus);
                en_Ins_Update["bsd_interestwaspaid"] = new Money(d_bsd_interestwaspaid);

                serv.Update(en_Ins_Update);
            }
            #endregion

            #region ----------- transaction type = Installment = 100000000 -------------------------
            if (i_trans_bsd_transactiontype == 100000000) // update installment
            {
                if (i_INS_statuscode == 100000001)
                {
                    i_INS_statuscode = 100000000;
                }
                d_bsd_amountwaspaid -= d_trans_bsd_amount;
                d_bsd_balance = d_bsd_amountofthisphase - d_bsd_amountwaspaid;
                d_oePiad -= d_trans_bsd_amount;
                if (d_IC_am > 0)
                {
                    if (d_bsd_interestchargeamount > d_IC_am)
                        d_bsd_interestchargeamount -= d_IC_am;
                    else d_bsd_interestchargeamount = 0;
                    i_bsd_actualgracedays -= i_outstandingday;
                }

                en_Ins_Update["statuscode"] = new OptionSetValue(i_INS_statuscode);
                en_Ins_Update["bsd_paiddate"] = null;
                en_Ins_Update["bsd_amountwaspaid"] = new Money(d_bsd_amountwaspaid > 0 ? d_bsd_amountwaspaid : 0);
                en_Ins_Update["bsd_interestchargeamount"] = new Money(d_bsd_interestchargeamount);
                en_Ins_Update["bsd_balance"] = new Money(d_bsd_balance);
                en_Ins_Update["bsd_actualgracedays"] = i_bsd_actualgracedays;

                serv.Update(en_Ins_Update);
            }
            #endregion ------------

            #region  -------- transaction type = Fees = 100000002 -----------------
            //if (i_trans_bsd_transactiontype == 100000002)
            //{
            //    if (i_bsd_feetype == 100000000) // main
            //    {
            //        f_main = false;
            //        d_bsd_maintenancefeepaid -= d_trans_bsd_amount;
            //        en_Ins_Update["bsd_maintenancefeepaid"] = new Money(d_bsd_maintenancefeepaid);
            //        en_Ins_Update["bsd_maintenancefeesstatus"] = f_main;
            //    }
            //    if (i_bsd_feetype == 100000001)// mana
            //    {
            //        f_mana = false;
            //        d_bsd_managementfeepaid -= d_trans_bsd_amount;
            //        en_Ins_Update["bsd_managementfeepaid"] = new Money(d_bsd_managementfeepaid);
            //        en_Ins_Update["bsd_managementfeesstatus"] = f_mana;
            //    }

            //    //en_Ins_Update["bsd_managementfeepaid"] = new Money(d_bsd_managementfeepaid);
            //    //en_Ins_Update["bsd_managementfeesstatus"] = f_mana;

            //    //en_Ins_Update["bsd_maintenancefeepaid"] = new Money(d_bsd_maintenancefeepaid);
            //    //en_Ins_Update["bsd_maintenancefeesstatus"] = f_main;

            //    serv.Update(en_Ins_Update);
            //}
            #endregion ------------

            #region  ---------------- transaction type = Other / MISS = 100000003 -----------------
            if (i_trans_bsd_transactiontype == 100000002)
            {
                Entity en_MIS = serv.Retrieve(((EntityReference)en_trans["bsd_miscellaneous"]).LogicalName, ((EntityReference)en_trans["bsd_miscellaneous"]).Id,
                new ColumnSet(new string[] { "bsd_balance","statuscode", "bsd_miscellaneousnumber","bsd_units","bsd_miscellaneousid","bsd_amount","bsd_paidamount",
                "bsd_installment","bsd_name","bsd_project"}));

                int f_paid = en_MIS.Contains("statuscode") ? ((OptionSetValue)en_MIS["statuscode"]).Value : 1;
                decimal d_MI_paid = en_MIS.Contains("bsd_paidamount") ? ((Money)en_MIS["bsd_paidamount"]).Value : 0;
                decimal d_MI_balance = en_MIS.Contains("bsd_balance") ? ((Money)en_MIS["bsd_balance"]).Value : 0;
                decimal d_MI_am = en_MIS.Contains("bsd_amount") ? ((Money)en_MIS["bsd_amount"]).Value : 0;

                Entity en_MIS_up = new Entity(en_MIS.LogicalName);
                en_MIS_up.Id = en_MIS.Id;
                en_MIS_up["bsd_paidamount"] = new Money(d_MI_paid - d_trans_bsd_amount);
                en_MIS_up["bsd_balance"] = new Money(d_MI_balance + d_trans_bsd_amount);
                en_MIS_up["statuscode"] = new OptionSetValue(1);

                serv.Update(en_MIS_up);


            }
            #endregion ----------------------------------

            return d_oePiad;
        }

        #region --------- transaciton payment new ------------------
        private void trans_INS(IOrganizationService serv, Entity en_trans, Entity en_OE)
        {
            #region  ----------------- retrive installment in transaction payment---------------
            Entity en_Ins = serv.Retrieve("bsd_paymentschemedetail", ((EntityReference)en_trans["bsd_installment"]).Id,
                new ColumnSet(new string[]{
                "bsd_amountofthisphase",
                "bsd_amountwaspaid",
                "bsd_depositamount",
                "statuscode",
                "bsd_duedatecalculatingmethod",
                "bsd_maintenancefees",
                "bsd_managementfee",
                "bsd_paymentscheme",
                "bsd_lastinstallment",
                "bsd_balance",
                "bsd_actualgracedays",
                "bsd_interestchargeamount",
                "bsd_interestchargestatus",
                "bsd_interestwaspaid",
                "bsd_maintenancefeesstatus",
                "bsd_managementfeesstatus",
                "bsd_managementamount",
                "bsd_maintenanceamount",
                "bsd_maintenancefeepaid",
                "bsd_managementfeepaid",
                "bsd_paiddate"
            }));

            decimal d_bsd_amountofthisphase = en_Ins.Contains("bsd_amountofthisphase") ? ((Money)en_Ins["bsd_amountofthisphase"]).Value : 0;
            decimal d_bsd_amountwaspaid = en_Ins.Contains("bsd_amountwaspaid") ? ((Money)en_Ins["bsd_amountwaspaid"]).Value : 0;
            int i_INS_statuscode = en_Ins.Contains("statuscode") ? ((OptionSetValue)en_Ins["statuscode"]).Value : 0;
            decimal d_bsd_balance = en_Ins.Contains("bsd_balance") ? ((Money)en_Ins["bsd_balance"]).Value : 0;

            decimal d_bsd_interestchargeamount = en_Ins.Contains("bsd_interestchargeamount") ? ((Money)en_Ins["bsd_interestchargeamount"]).Value : 0;
            decimal d_bsd_interestwaspaid = en_Ins.Contains("bsd_interestwaspaid") ? ((Money)en_Ins["bsd_interestwaspaid"]).Value : 0;
            int i_bsd_interestchargestatus = en_Ins.Contains("bsd_interestchargestatus") ? ((OptionSetValue)en_Ins["bsd_interestchargestatus"]).Value : 100000000;
            int i_bsd_actualgracedays = en_Ins.Contains("bsd_actualgracedays") ? (int)en_Ins["bsd_actualgracedays"] : 0;

            decimal d_bsd_maintenanceamount = en_Ins.Contains("bsd_maintenanceamount") ? ((Money)en_Ins["bsd_maintenanceamount"]).Value : 0;
            decimal d_bsd_managementamount = en_Ins.Contains("bsd_managementamount") ? ((Money)en_Ins["bsd_managementamount"]).Value : 0;
            decimal d_bsd_maintenancefeepaid = en_Ins.Contains("bsd_maintenancefeepaid") ? ((Money)en_Ins["bsd_maintenancefeepaid"]).Value : 0;
            decimal d_bsd_managementfeepaid = en_Ins.Contains("bsd_managementfeepaid") ? ((Money)en_Ins["bsd_managementfeepaid"]).Value : 0;

            bool f_main = en_Ins.Contains("bsd_maintenancefeesstatus") ? (bool)en_Ins["bsd_maintenancefeesstatus"] : false;
            bool f_mana = en_Ins.Contains("bsd_managementfeesstatus") ? (bool)en_Ins["bsd_managementfeesstatus"] : false;

            #endregion


        }
        #endregion

        private void update_OE(IOrganizationService crmservices, Entity en_OE, int i_OE_status, decimal d_totalPaid, decimal d_totalPercent, int i_totalLateday)
        {
            Entity tmp_OE = new Entity(en_OE.LogicalName);
            tmp_OE.Id = en_OE.Id;
            tmp_OE["statuscode"] = new OptionSetValue(i_OE_status);
            tmp_OE["bsd_totalamountpaid"] = new Money(d_totalPaid);
            // tmp_OE["bsd_totalpercent"] = new Money(d_totalPercent);
            service.Update(tmp_OE);
        }

        private void update_Ins(IOrganizationService crmservices, Entity en_INS, int i_InsStatus, decimal d_amountPaid, decimal d_balance)
        {
            Entity tmp = new Entity(en_INS.LogicalName);
            tmp.Id = en_INS.Id;
            tmp["statuscode"] = new OptionSetValue(i_InsStatus);
            tmp["bsd_amountwaspaid"] = new Money(d_amountPaid);
            tmp["bsd_balance"] = new Money(d_balance);
            crmservices.Update(tmp);
        }

        private void update_AdvPM(Guid pmID)
        {
            EntityCollection ec_advPM = get_AdvPM(service, pmID);
            if (ec_advPM.Entities.Count > 0)
            {
                Entity en_Adv_up = new Entity(ec_advPM.Entities[0].LogicalName);
                en_Adv_up.Id = ec_advPM.Entities[0].Id;
                en_Adv_up["statuscode"] = new OptionSetValue(100000002); // revert
                service.Update(en_Adv_up);
            }
        }

        #endregion ---------- end function ----------

        private EntityCollection get_1stinstallment(IOrganizationService crmservices, Guid resvID)
        {
            string fetchXml =
                @"<fetch version='1.0' output-format='xml-platform' mapping='logical' distinct='false'>
                <entity name='bsd_paymentschemedetail' >
                    <attribute name='bsd_balance' />
                    <attribute name='bsd_amountwaspaid' />
                    <attribute name='bsd_reservation' />
                    <attribute name='statuscode' />
                    <attribute name='bsd_amountofthisphase' />
                    <attribute name='bsd_ordernumber' />
                    <attribute name='bsd_paiddate' />
                    <filter type='and' >
                      <condition attribute='bsd_ordernumber' operator='eq' value='1' />
                      <condition attribute='bsd_reservation' operator='eq' value='{0}' />
                    </filter>
              </entity>
            </fetch>";
            fetchXml = string.Format(fetchXml, resvID);
            EntityCollection entc = crmservices.RetrieveMultiple(new FetchExpression(fetchXml));
            return entc;
        }
        private EntityCollection get_AdvPM(IOrganizationService crmservices, Guid pmID)
        {
            string fetchXml =
                @"<fetch version='1.0' output-format='xml-platform' mapping='logical' distinct='false'>
                <entity name='bsd_advancepayment' >
                <attribute name='bsd_remainingamount' />
                <attribute name='statuscode' />
                <attribute name='bsd_transactiondate' />
                <attribute name='bsd_payment' />
                <attribute name='bsd_amount' />
                <attribute name='bsd_paidamount' />
                <filter type='and' >
                  <condition attribute='bsd_payment' operator='eq' value='{0}' />
                </filter>
              </entity>
                </fetch>";
            //< condition attribute = 'statuscode' operator= 'eq' value = '100000000' />
            fetchXml = string.Format(fetchXml, pmID);
            EntityCollection entc = crmservices.RetrieveMultiple(new FetchExpression(fetchXml));
            return entc;
        }
        private EntityCollection get_previous_PM(IOrganizationService crmservices, Guid pmsID, Guid pmID, Guid oeID)
        {
            string fetchXml =
                @"<fetch version='1.0' output-format='xml-platform' mapping='logical' distinct='false'>
                <entity name='bsd_payment' >
                <attribute name='bsd_latepayment' />
	            <attribute name='bsd_interestcharge' />
                <attribute name='bsd_paymentactualtime' />
                <attribute name='bsd_latedays' />
	            <attribute name='bsd_confirmeddate' />
                <attribute name='bsd_optionentry' />
                <attribute name='bsd_paymentschemedetail' />
                <attribute name='statuscode' />
                <filter type='and' >
                  <condition attribute='bsd_optionentry' operator='eq' value='{2}' />
                  <condition attribute='bsd_paymentschemedetail' operator='eq' value='{1}' />
                  <condition attribute='statuscode' operator='eq' value='100000000' />
                  <condition attribute='bsd_paymentid' operator='neq' value='{0}' />
                </filter>
                <order attribute='bsd_confirmeddate' descending='true' />
              </entity>
                </fetch>";
            fetchXml = string.Format(fetchXml, pmID, pmsID, oeID);
            EntityCollection entc = crmservices.RetrieveMultiple(new FetchExpression(fetchXml));
            return entc;
        }
        private EntityCollection get_TransactionPM(IOrganizationService crmservices, Guid pmID)
        {
            string fetchXml =
                @"<fetch version='1.0' output-format='xml-platform' mapping='logical' distinct='false'>
                <entity name='bsd_transactionpayment' >
                    <attribute name='bsd_transactionpaymentid' />
                    <attribute name='statecode' />
                    <attribute name='bsd_payment' />
                    <attribute name='bsd_amount' />
                    <attribute name='bsd_name' />
                    <attribute name='statuscode' />
                    <attribute name='bsd_installment' />
                    <attribute name='bsd_transactiontype' />
                    <attribute name='bsd_feetype' />
                    <attribute name='bsd_actualgracedays' />
                    <attribute name='bsd_interestchargeamount' />
                    <attribute name='bsd_miscellaneous' />
                <filter type='and' >
                  <condition attribute='bsd_payment' operator='eq' value='{0}' />
                </filter>
              </entity>
                </fetch>";
            fetchXml = string.Format(fetchXml, pmID);
            EntityCollection entc = crmservices.RetrieveMultiple(new FetchExpression(fetchXml));
            return entc;
        }

        private EntityCollection get_totalLateday(IOrganizationService crmservices, Guid oeID)
        {
            string fetchXml =
                @"<fetch version='1.0' output-format='xml-platform' mapping='logical' distinct='false'>
                 <entity name = 'bsd_paymentschemedetail' >
                    <attribute name='bsd_actualgracedays' />
                    <filter type = 'and' >
                      < condition attribute='bsd_optionentry' operator='eq' value='{0}' />
                    </filter>
                  </entity>
                </fetch>";
            fetchXml = string.Format(fetchXml, oeID);
            EntityCollection entc = crmservices.RetrieveMultiple(new FetchExpression(fetchXml));
            return entc;
        }

        private EntityCollection get_advFrompmID(IOrganizationService crmservices, Guid pmID)
        {
            string fetchXml =
                @"<fetch version='1.0' output-format='xml-platform' mapping='logical' distinct='false'>
                 <entity name='bsd_advancepayment' >
                    <attribute name='bsd_name' />
                    <attribute name='bsd_advancepaymentid' />
                    <attribute name='bsd_payment' />
                    <attribute name='statuscode' />
                    <filter type='and' >
                      <condition attribute='bsd_payment' operator='eq' value='{0}' />
                    </filter>
                  </entity>
                </fetch>";
            //<condition attribute='statuscode' operator='eq' value='1' />
            fetchXml = string.Format(fetchXml, pmID);
            EntityCollection entc = crmservices.RetrieveMultiple(new FetchExpression(fetchXml));
            return entc;
        }

        #region -------- revert apply document ------------------------------------
        private void Up_payment_deposit(Guid quoteId, decimal d_amp, DateTime d_now, Entity en_app)
        {
            Entity en_Resv = service.Retrieve("quote", quoteId, new ColumnSet(new string[] {
                                "bsd_deposittime", "statuscode", "bsd_depositfee", "name", "bsd_totalamountpaid","bsd_projectid",
                                "customerid","bsd_isdeposited","bsd_salesdepartmentreceiveddeposit","bsd_unitno"
            }));

            if (((OptionSetValue)en_Resv["statuscode"]).Value == 6)
                throw new InvalidPluginExecutionException("Reservation " + (string)en_Resv["name"] + " had been cancelled, cannot revert!");
            if (((OptionSetValue)en_Resv["statuscode"]).Value == 4)
                throw new InvalidPluginExecutionException("Reservation " + (string)en_Resv["name"] + " had been convert to Option Entry, cannot revert!");
            decimal d_QO_bsd_totalamountpaid = en_Resv.Contains("bsd_totalamountpaid") ? ((Money)en_Resv["bsd_totalamountpaid"]).Value : 0;

            Entity en_tmp = new Entity(en_Resv.LogicalName);
            en_tmp.Id = en_Resv.Id;

            // 161012 - amount pay > deposit amount
            EntityCollection ec_pm1st = Get1st_Resv(en_Resv.Id.ToString());
            Entity en_pm1st = new Entity(ec_pm1st.Entities[0].LogicalName);
            en_pm1st.Id = ec_pm1st[0].Id;

            decimal d_amPhase1st = ec_pm1st.Entities[0].Contains("bsd_amountofthisphase") ? ((Money)ec_pm1st.Entities[0]["bsd_amountofthisphase"]).Value : 0;
            decimal d_bsd_amountwaspaid = ec_pm1st.Entities[0].Contains("bsd_amountwaspaid") ? ((Money)ec_pm1st.Entities[0]["bsd_amountwaspaid"]).Value : 0;
            if (d_amPhase1st == 0)
                throw new InvalidPluginExecutionException("Amount of Installment 1 - Reservation " + (string)en_Resv["name"] + " is null. Please check again!");

            en_pm1st["bsd_amountwaspaid"] = new Money(0);
            en_pm1st["bsd_balance"] = new Money(d_amPhase1st);
            en_pm1st["bsd_depositamount"] = new Money(0);
            service.Update(en_pm1st);

            #region ----------- open state of reservation ----------
            SetStateRequest setStateRequest = new SetStateRequest()
            {
                EntityMoniker = new EntityReference
                {
                    Id = en_Resv.Id,                 //id form
                    LogicalName = en_Resv.LogicalName        //localname
                },
                State = new OptionSetValue(0),          //state value = draft
                Status = new OptionSetValue(100000000)      //status value = reservation
            };
            service.Execute(setStateRequest);

            #endregion

            // total amount paid of Resv

            en_tmp["bsd_totalamountpaid"] = new Money(0);
            en_tmp["bsd_deposittime"] = null;
            en_tmp["bsd_isdeposited"] = false;
            service.Update(en_tmp);

            Entity en_Unit = new Entity(((EntityReference)en_Resv["bsd_unitno"]).LogicalName);
            en_Unit.Id = ((EntityReference)en_Resv["bsd_unitno"]).Id;

            if (en_Resv.Contains("bsd_salesdepartmentreceiveddeposit"))
            {
                if ((bool)en_Resv["bsd_salesdepartmentreceiveddeposit"] == true)
                {
                    en_tmp["statuscode"] = new OptionSetValue(100000006); // collect
                    en_Unit["statuscode"] = new OptionSetValue(100000005); // collect
                }
                else
                {
                    en_tmp["statuscode"] = new OptionSetValue(100000000); //reser
                    en_Unit["statuscode"] = new OptionSetValue(100000006); // reserve
                }
            }
            else
            {
                en_tmp["statuscode"] = new OptionSetValue(100000000);
                en_Unit["statuscode"] = new OptionSetValue(100000006); // reserve
            }
            service.Update(en_tmp);
            service.Update(en_Unit);

        }

        private EntityCollection Get1st_Resv(string resvID)
        {
            QueryExpression query = new QueryExpression("bsd_paymentschemedetail");

            query.ColumnSet = new ColumnSet(new string[] { "bsd_depositamount", "bsd_ordernumber", "bsd_paymentschemedetailid", "statuscode", "bsd_amountwaspaid", "bsd_balance", "bsd_amountofthisphase", "bsd_paiddate" });
            query.Distinct = true;
            query.Criteria = new FilterExpression();
            query.Criteria.AddCondition("bsd_reservation", ConditionOperator.Equal, resvID);
            query.AddOrder("bsd_ordernumber", OrderType.Ascending);
            query.TopCount = 1;

            EntityCollection psdFirst = service.RetrieveMultiple(query);
            return psdFirst;
        }

        private EntityCollection get_ecIns(IOrganizationService crmservices, string[] s_id)
        {
            string fetchXml =
              @"<fetch version='1.0' output-format='xml-platform' mapping='logical' distinct='true' >
                <entity name='bsd_paymentschemedetail' >
                <attribute name='bsd_name' />
                <attribute name='bsd_interchargeprecalc' />
                <attribute name='bsd_ordernumber' />
                <attribute name='bsd_maintenanceamount' />
                <attribute name='bsd_balance' />
                <attribute name='bsd_amountwaspaid' />
                <attribute name='bsd_managementfee' />
                <attribute name='bsd_actualgracedays' />
                <attribute name='bsd_waiveramount' />
                <attribute name='bsd_managementamount' />
                <attribute name='bsd_duedate' />
                <attribute name='bsd_maintenancefees' />
                <attribute name='bsd_interestchargestatus' />
                <attribute name='bsd_optionentry' />
                <attribute name='bsd_amountpay' />
                <attribute name='bsd_interestchargeamount' />
                <attribute name='statuscode' />
                <attribute name='bsd_depositamount' />
                <attribute name='bsd_amountofthisphase' />
                <attribute name='bsd_interestwaspaid' />
                <attribute name='bsd_paymentscheme' />
                <attribute name='bsd_paymentschemedetailid' />
                <attribute name='bsd_paiddate' />
                <filter type='and' >
                  <condition attribute='bsd_paymentschemedetailid' operator='in' >";
            for (int i = 0; i < s_id.Length; i++)
            {
                fetchXml += @"<value>" + Guid.Parse(s_id[i]) + "</value>";
            }
            fetchXml += @"</condition>
                            </filter>
                            <order attribute='bsd_ordernumber' />
                          </entity>
                        </fetch>";

            EntityCollection entc = crmservices.RetrieveMultiple(new FetchExpression(fetchXml));
            return entc;
        }

        private void revert_Up_adv(Guid advID, decimal trans_am)
        {
            Entity en_up = service.Retrieve("bsd_advancepayment", advID, new ColumnSet(new string[] {
               "bsd_name",
                        "bsd_remainingamount",
                        "bsd_payment",
                        "bsd_amount",
                        "bsd_paidamount",
                        "bsd_customer",
                        "statuscode",
                        "bsd_project",
                        "bsd_transferredamount"
            }));
            decimal d_bsd_paidamount = en_up.Contains("bsd_paidamount") ? ((Money)en_up["bsd_paidamount"]).Value : 0;
            decimal d_bsd_remainingamount = en_up.Contains("bsd_remainingamount") ? ((Money)en_up["bsd_remainingamount"]).Value : 0;
            decimal d_bsd_amount = en_up.Contains("bsd_amount") ? ((Money)en_up["bsd_amount"]).Value : 0;
            if (d_bsd_amount == 0) throw new InvalidPluginExecutionException("Cannot find Advance payment amount of " + (string)en_up["bsd_name"] + "!");
            if (d_bsd_amount < trans_am) throw new InvalidPluginExecutionException("Transaction amount is larger than Advance payment amount!");

            d_bsd_paidamount -= trans_am;
            d_bsd_remainingamount += trans_am;
            int i_status = en_up.Contains("statuscode") ? ((OptionSetValue)en_up["statuscode"]).Value : 100000000;
            if (i_status == 100000001) // revert
                i_status = 100000000;
            Entity en_adv = new Entity("bsd_advancepayment");
            en_adv.Id = advID;
            en_adv["bsd_remainingamount"] = new Money(d_bsd_remainingamount);
            en_adv["bsd_paidamount"] = new Money(d_bsd_paidamount);
            en_adv["statuscode"] = new OptionSetValue(i_status);
            service.Update(en_adv);
        }
        private EntityCollection get_appDtl(IOrganizationService crmservices, Guid appID)
        {
            string fetchXml =
               @"<fetch version='1.0' output-format='xml-platform' mapping='logical' distinct='false'>
                 <entity name='bsd_applydocumentdetail' >
                <attribute name='statuscode' />
                <attribute name='bsd_amountapply' />
                <attribute name='bsd_optionentry' />
                <attribute name='bsd_applydocumentname' />
                <attribute name='bsd_reservation' />
                <attribute name='bsd_paymenttype' />
                <attribute name='bsd_installment' />
                <attribute name='bsd_applydocumentdetailid' />
                <attribute name='bsd_name' />
                <attribute name='bsd_applydocument' />
                <attribute name='bsd_actualgracedays' />
                <filter type='and' >
                  <condition attribute='bsd_applydocument' operator='eq' value='{0}' />
                </filter>
              </entity>
                </fetch>";
            fetchXml = string.Format(fetchXml, appID);
            EntityCollection entc = crmservices.RetrieveMultiple(new FetchExpression(fetchXml));
            return entc;
        }

        private EntityCollection get_appDtl_Ins(IOrganizationService crmservices, Guid appID, Guid InsID)
        {
            string fetchXml =
               @"<fetch version='1.0' output-format='xml-platform' mapping='logical' distinct='false'>
                 <entity name='bsd_applydocumentdetail' >
                <attribute name='statuscode' />
                <attribute name='bsd_amountapply' />
                <attribute name='bsd_optionentry' />
                <attribute name='bsd_applydocumentname' />
                <attribute name='bsd_reservation' />
                <attribute name='bsd_paymenttype' />
                <attribute name='bsd_installment' />
                <attribute name='bsd_applydocumentdetailid' />
                <attribute name='bsd_name' />
                <attribute name='bsd_applydocument' />
                <attribute name='bsd_actualgracedays' />
                <attribute name='bsd_interestchargeamount' />
                <filter type='and' >
                  <condition attribute='bsd_applydocument' operator='eq' value='{0}' />
                 <condition attribute='bsd_paymenttype' operator='eq' value='100000001' />
                <condition attribute='bsd_installment' operator='eq' value='{1}' />
                </filter>
              </entity>
                </fetch>";
            fetchXml = string.Format(fetchXml, appID, InsID);
            EntityCollection entc = crmservices.RetrieveMultiple(new FetchExpression(fetchXml));
            return entc;
        }


        private EntityCollection GetPSD(string OptionEntryID)
        {
            QueryExpression query = new QueryExpression("bsd_paymentschemedetail");
            query.ColumnSet = new ColumnSet(new string[] { "bsd_ordernumber", "bsd_paymentschemedetailid", "statuscode", "bsd_amountwaspaid", "bsd_paiddate" });
            query.Distinct = true;
            query.Criteria = new FilterExpression();
            query.Criteria.AddCondition("bsd_optionentry", ConditionOperator.Equal, OptionEntryID);
            query.AddOrder("bsd_ordernumber", OrderType.Ascending);
            //query.TopCount = 1;
            EntityCollection psdFirst = service.RetrieveMultiple(query);
            return psdFirst;
        }

        private EntityCollection get_app_Inter(IOrganizationService crmservices, Guid appID)
        {
            string fetchXml =
               @"<fetch version='1.0' output-format='xml-platform' mapping='logical' distinct='false'>
                 <entity name='bsd_applydocumentdetail' >
                <attribute name='statuscode' />
                <attribute name='bsd_amountapply' />
                <attribute name='bsd_optionentry' />
                <attribute name='bsd_reservation' />
                <attribute name='bsd_paymenttype' />
                <attribute name='bsd_installment' />
                <attribute name='bsd_applydocumentdetailid' />
                <attribute name='bsd_actualgracedays' />
                <filter type='and' >
                  <condition attribute='bsd_paymenttype' operator='eq' value='100000003' />
                  <condition attribute='bsd_applydocument' operator='eq' value='{0}' />
                </filter>
                </entity>
                </fetch>";
            fetchXml = string.Format(fetchXml, appID);
            EntityCollection entc = crmservices.RetrieveMultiple(new FetchExpression(fetchXml));
            return entc;
        }
        private EntityCollection get_app_MISS(IOrganizationService crmservices, Guid appID)
        {
            string fetchXml =
               @"<fetch version='1.0' output-format='xml-platform' mapping='logical' distinct='false'>
                 <entity name='bsd_applydocumentdetail' >
                <attribute name='statuscode' />
                <attribute name='bsd_amountapply' />
                <attribute name='bsd_optionentry' />
                <attribute name='bsd_paymenttype' />
                <attribute name='bsd_reservation' />
                <attribute name='bsd_miscellaneous' />
                <attribute name='bsd_applydocumentdetailid' />
                <attribute name='bsd_actualgracedays' />
                <attribute name='bsd_installment' />
                <filter type='and' >
                  <condition attribute='bsd_paymenttype' operator='eq' value='100000004' />
                  <condition attribute='bsd_applydocument' operator='eq' value='{0}' />
                </filter>
                </entity>
                </fetch>";
            fetchXml = string.Format(fetchXml, appID);
            EntityCollection entc = crmservices.RetrieveMultiple(new FetchExpression(fetchXml));
            return entc;
        }

        private bool check_Ins_Paid(IOrganizationService crmservices, Guid oeID, int i_ordernumber)
        {
            bool res = false;
            // check neu installment truoc do da paid hay chua , neu paid r thi cho excute, neu chua paid thi thong bao ra
            string fetchXml =
              @"<fetch version='1.0' output-format='xml-platform' mapping='logical' distinct='true' >
                <entity name='bsd_paymentschemedetail' >
                <attribute name='bsd_amountpay' />
                <attribute name='statuscode' />
                <attribute name='bsd_name' />
                <attribute name='bsd_ordernumber' />
                <attribute name='bsd_paymentschemedetailid' />
                <attribute name='bsd_paiddate' />
                <filter type='and' >
                <condition attribute='bsd_ordernumber' operator='eq' value='{0}' />
                  <condition attribute='bsd_optionentry' operator='eq' value='{1}' />
                </filter>
              </entity>
            </fetch>";
            fetchXml = string.Format(fetchXml, i_ordernumber, oeID);

            EntityCollection entc = crmservices.RetrieveMultiple(new FetchExpression(fetchXml));

            if (entc.Entities.Count > 0)
            {
                Entity en_tmp = entc.Entities[0];
                en_tmp.Id = entc.Entities[0].Id;
                if (((OptionSetValue)en_tmp["statuscode"]).Value == 100000001) res = true;
                if (((OptionSetValue)en_tmp["statuscode"]).Value == 100000000) res = false;
            }
            return res;
        }

        private EntityCollection get_Ins_ES(IOrganizationService crmservices, string oeID)
        {
            string fetchXml =
               @"<fetch version='1.0' output-format='xml-platform' mapping='logical' distinct='true' >
                <entity name='bsd_paymentschemedetail' >
                <attribute name='bsd_duedatecalculatingmethod' />
                <attribute name='bsd_maintenanceamount' />
                <attribute name='bsd_maintenancefeepaid' />
                <attribute name='bsd_ordernumber' />
                <attribute name='statuscode' />
                <attribute name='bsd_managementfeepaid' />
                <attribute name='bsd_amountpay' />
                <attribute name='bsd_maintenancefees' />
                <attribute name='bsd_optionentry' />
                <attribute name='bsd_managementfee' />
                <attribute name='bsd_managementfeesstatus' />
                <attribute name='bsd_managementamount' />
                <attribute name='bsd_amountwaspaid' />
                <attribute name='bsd_maintenancefeesstatus' />
                <attribute name='bsd_name' />
                <attribute name='bsd_paymentschemedetailid' />
                <attribute name='bsd_paiddate' />
                <filter type='and' >
                  <condition attribute='bsd_optionentry' operator='eq' value='{0}' />
                  <condition attribute='bsd_duedatecalculatingmethod' operator='eq' value='100000002' />
                </filter>
              </entity>
            </fetch>";
            fetchXml = string.Format(fetchXml, oeID);
            EntityCollection entc = crmservices.RetrieveMultiple(new FetchExpression(fetchXml));
            return entc;
        }
        #endregion


        #region Datetime ------------
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

        #endregion

        #region Get Miscellaneous Thạnh Đỗ
        private Entity getMiscellaneous(IOrganizationService crmservices, string miscellaneousid)
        {
            string fetchXmlMiscellaneous =
              @"<fetch version='1.0' output-format='xml-platform' mapping='logical' distinct='true' >
                <entity name='bsd_miscellaneous' >
                <attribute name='bsd_balance' />
                <attribute name='statuscode' />
                <attribute name='bsd_miscellaneousnumber' />
                <attribute name='bsd_units' />
                <attribute name='bsd_optionentry' />
                <attribute name='bsd_miscellaneousid' />
                <attribute name='bsd_amount' />
                <attribute name='bsd_totalamount' />
                <attribute name='bsd_paidamount' />
                <attribute name='bsd_installment' />
                <attribute name='bsd_name' />
                <attribute name='bsd_project' />
                <attribute name='bsd_installmentnumber' />
                <filter type='and' >
                    <condition attribute='bsd_miscellaneousid' operator='eq' value='{0}'/>;
                </filter>                           
                </entity>
                </fetch>";
            fetchXmlMiscellaneous = string.Format(fetchXmlMiscellaneous, miscellaneousid);
            traceService.Trace("fetchXmlMiscellaneous" + fetchXmlMiscellaneous.ToString());
            EntityCollection entc = crmservices.RetrieveMultiple(new FetchExpression(fetchXmlMiscellaneous));
            if (entc.Entities.Count > 0)
            {
                return entc.Entities[0];
            }
            else
            {
                return null;
            }
        }
        #endregion

        private EntityCollection getTracsactionPaymentByPayment(Entity enPayment)
        {
            var QEbsd_transactionpayment = new QueryExpression("bsd_transactionpayment");
            QEbsd_transactionpayment.ColumnSet.AllColumns = true;
            QEbsd_transactionpayment.Criteria.AddCondition("bsd_payment", ConditionOperator.Equal, enPayment.Id);
            EntityCollection encl = service.RetrieveMultiple(QEbsd_transactionpayment);
            return encl;
        }
        private EntityCollection getTransactionPaymentByInterest(Entity enInstallment,Entity enPayment)
        {
            var QEbsd_transactionpayment_bsd_transactiontype = 100000001;
            var QEbsd_transactionpayment = new QueryExpression("bsd_transactionpayment");
            QEbsd_transactionpayment.ColumnSet.AllColumns = true;
            QEbsd_transactionpayment.Criteria.AddCondition("bsd_transactiontype", ConditionOperator.Equal, QEbsd_transactionpayment_bsd_transactiontype);
            QEbsd_transactionpayment.Criteria.AddCondition("bsd_payment", ConditionOperator.Equal, enPayment.Id);
            QEbsd_transactionpayment.Criteria.AddCondition("bsd_installment", ConditionOperator.Equal, enInstallment.Id);
            EntityCollection encl = service.RetrieveMultiple(QEbsd_transactionpayment);
            return encl;
        }
    }
}


