using Microsoft.Crm.Sdk.Messages;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Query;
using System;
using System.IdentityModel.Metadata;
using System.Text;
using System.Web.UI.WebControls;

namespace Plugin_VoidPayment_updatePendingPM
{
    class Payment
    {
        IOrganizationService service = null;
        IPluginExecutionContext context;
        AdvancePayment advancePayment;
        TransactionPayment transactionPayment;
        Installment installment;
        Miscellaneous miscellaneous;
        StringBuilder strMess = new StringBuilder();

        public Payment(IOrganizationService service, IPluginExecutionContext context, StringBuilder strMess1)
        {
            this.service = service;
            this.context = context;
            advancePayment = new AdvancePayment(service, context, strMess);
            transactionPayment = new TransactionPayment(service, context, strMess);
            installment = new Installment(service, context, strMess);
            miscellaneous = new Miscellaneous(service, context, strMess);
            strMess = strMess1;
        }
        public void voidPaymentAproval(Entity enVoidPayment)
        {
            #region ------------------------- update void PM for PM ---------------------------
            if (enVoidPayment.Contains("bsd_payment"))
            {
                strMess.AppendLine("2");
                #region ------------------ retrieve payment -----------------
                Entity enPayment = service.Retrieve(((EntityReference)enVoidPayment["bsd_payment"]).LogicalName, ((EntityReference)enVoidPayment["bsd_payment"]).Id, new ColumnSet(true));
                int pm_status = enPayment.Contains("statuscode") ? ((OptionSetValue)enPayment["statuscode"]).Value : 0;
                if (pm_status == 100000002)
                    throw new InvalidPluginExecutionException("Payment " + (string)enPayment["bsd_name"] + " has been revert!");
                if (!enPayment.Contains("bsd_paymentactualtime"))
                    throw new InvalidPluginExecutionException("Please choose 'Receipt Date'!");
                if (!enPayment.Contains("bsd_transactiontype"))
                    throw new InvalidPluginExecutionException("Please choose 'Transaction Type'!");
                if (!enPayment.Contains("bsd_paymenttype"))
                    throw new InvalidPluginExecutionException("Please choose 'Payment Type'!");
                if (!enPayment.Contains("bsd_paymentmode"))
                    throw new InvalidPluginExecutionException("Please choose 'Payment Mode'!");
                if (!enPayment.Contains("bsd_purchaser"))
                    throw new InvalidPluginExecutionException("Please choose 'Purchaser'!");
                if (!enPayment.Contains("bsd_units"))
                    throw new InvalidPluginExecutionException("Please choose 'Units'!");

                decimal d_pmAMpay = enPayment.Contains("bsd_amountpay") ? ((Money)enPayment["bsd_amountpay"]).Value : 0;
                decimal d_pmAMPhase = enPayment.Contains("bsd_totalamountpayablephase") ? ((Money)enPayment["bsd_totalamountpayablephase"]).Value : 0;
                decimal bsd_totalamountpaidphase = enPayment.Contains("bsd_totalamountpaidphase") ? ((Money)enPayment["bsd_totalamountpaidphase"]).Value : 0;
                decimal d_pmMaintenance = enPayment.Contains("bsd_maintenancefee") ? ((Money)enPayment["bsd_maintenancefee"]).Value : 0;
                decimal d_pmManagement = enPayment.Contains("bsd_managementfee") ? ((Money)enPayment["bsd_managementfee"]).Value : 0;
                decimal d_pmDeposit = enPayment.Contains("bsd_depositamount") ? ((Money)enPayment["bsd_depositamount"]).Value : 0;
                decimal d_pmDifference = enPayment.Contains("bsd_differentamount") ? ((Money)enPayment["bsd_differentamount"]).Value : 0;
                decimal d_pmBalance = enPayment.Contains("bsd_balance") ? ((Money)enPayment["bsd_balance"]).Value : 0;
                decimal d_pmInterestAm = enPayment.Contains("bsd_interestcharge") ? ((Money)enPayment["bsd_interestcharge"]).Value : 0;
                bool f_latePM = enPayment.Contains("bsd_latepayment") ? (bool)enPayment["bsd_latepayment"] : false;
                int i_PmLateday = enPayment.Contains("bsd_latedays") ? (int)enPayment["bsd_latedays"] : 0;

                string s_PMgrid = enPayment.Contains("bsd_savegridinstallment") ? (string)enPayment["bsd_savegridinstallment"] : "";
                // new subgrid 170506
                string s_bsd_arraypsdid = enPayment.Contains("bsd_arraypsdid") ? (string)enPayment["bsd_arraypsdid"] : "";
                string s_bsd_arrayamountpay = enPayment.Contains("bsd_arrayamountpay") ? (string)enPayment["bsd_arrayamountpay"] : "";

                string s_bsd_arrayinstallmentinterest = enPayment.Contains("bsd_arrayinstallmentinterest") ? (string)enPayment["bsd_arrayinstallmentinterest"] : "";
                string s_bsd_arrayinterestamount = enPayment.Contains("bsd_arrayinterestamount") ? (string)enPayment["bsd_arrayinterestamount"] : "";
                string s_bsd_arrayfees = enPayment.Contains("bsd_arrayfees") ? (string)enPayment["bsd_arrayfees"] : "";
                string s_bsd_arrayfeesamount = enPayment.Contains("bsd_arrayfeesamount") ? (string)enPayment["bsd_arrayfeesamount"] : "";
                string s_bsd_arraymicellaneousid = enPayment.Contains("bsd_arraymicellaneousid") ? (string)enPayment["bsd_arraymicellaneousid"] : "";
                string s_bsd_arraymicellaneousamount = enPayment.Contains("bsd_arraymicellaneousamount") ? (string)enPayment["bsd_arraymicellaneousamount"] : "";

                int i_pmType = enPayment.Contains("bsd_paymenttype") ? ((OptionSetValue)enPayment["bsd_paymenttype"]).Value : 0;
                // Thạnh đỗ
                EntityReference enrfPaymentSchemeDetail = enPayment.Contains("bsd_paymentschemedetail") ? (EntityReference)enPayment["bsd_paymentschemedetail"] : null;
                #endregion  // end retrieve payment
                strMess.AppendLine("3");

                if ((i_pmType == 100000002 || i_pmType == 100000003 || i_pmType == 100000004) && (!enPayment.Contains("bsd_optionentry")))
                    throw new InvalidPluginExecutionException("Payment " + (string)enPayment["bsd_name"] + " does not contain Option Entry information. Please check again!");

                EntityReference enrefOptionEntry = enPayment.Contains("bsd_optionentry") ? (EntityReference)enPayment["bsd_optionentry"] : null;
                Entity enOptionEntry = new Entity();
                if (enrefOptionEntry != null)
                {
                    enOptionEntry = service.Retrieve(((EntityReference)enPayment["bsd_optionentry"]).LogicalName, ((EntityReference)enPayment["bsd_optionentry"]).Id, new ColumnSet(new string[] {
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
                }
                int i_bsd_totallatedays = enOptionEntry.Contains("bsd_totallatedays") ? (int)enOptionEntry["bsd_totallatedays"] : 0;
                decimal d_oe_totalamount = enOptionEntry.Contains("totalamount") ? ((Money)enOptionEntry["totalamount"]).Value : 0;
                decimal d_oe_bsd_totalamountpaid = enOptionEntry.Contains("bsd_totalamountpaid") ? ((Money)enOptionEntry["bsd_totalamountpaid"]).Value : 0;
                int i_oe_statuscode = enOptionEntry.Contains("statuscode") ? ((OptionSetValue)enOptionEntry["statuscode"]).Value : 100000000;
                decimal d_oe_bsd_totalpercent = enOptionEntry.Contains("bsd_totalpercent") ? (decimal)enOptionEntry["bsd_totalpercent"] : 0;
                decimal d_oe_bsd_freightamount = enOptionEntry.Contains("bsd_freightamount") ? ((Money)enOptionEntry["bsd_freightamount"]).Value : 0;
                decimal d_oe_amountCalcPercent = d_oe_totalamount - d_oe_bsd_freightamount;
                bool f_OE_Signcontract = enOptionEntry.Contains("bsd_signedcontractdate") ? true : false;
                decimal d_TransactionAM = 0;

                Entity enOptionEntry_Up = new Entity(enOptionEntry.LogicalName);
                enOptionEntry_Up.Id = enOptionEntry.Id;
                #endregion
                strMess.AppendLine("5");
                if (enPayment.Contains("bsd_paymentschemedetail"))
                {
                    Entity en_Ins = service.Retrieve(((EntityReference)enPayment["bsd_paymentschemedetail"]).LogicalName,
                           ((EntityReference)enPayment["bsd_paymentschemedetail"]).Id, new ColumnSet(true));
                    strMess.AppendLine("5.1");
                    decimal d_PmsDtlDeposit = en_Ins.Contains("bsd_depositamount") ? ((Money)en_Ins["bsd_depositamount"]).Value : 0;
                    decimal d_PmsDtlAmPaid = en_Ins.Contains("bsd_amountwaspaid") ? ((Money)en_Ins["bsd_amountwaspaid"]).Value : 0;
                    decimal d_PmsDtlAmPhase = en_Ins.Contains("bsd_amountofthisphase") ? ((Money)en_Ins["bsd_amountofthisphase"]).Value : 0;
                    decimal d_PmsDtlBalance = en_Ins.Contains("bsd_balance") ? ((Money)en_Ins["bsd_balance"]).Value : 0;
                    strMess.AppendLine("5.2");
                    decimal d_PmsDtlWaiverAM = en_Ins.Contains("bsd_waiveramount") ? ((Money)en_Ins["bsd_waiveramount"]).Value : 0;

                    int i_PmsDtlStatuscode = en_Ins.Contains("statuscode") ? ((OptionSetValue)en_Ins["statuscode"]).Value : 100000000;
                    int i_Pms_bsd_ordernumber = en_Ins.Contains("bsd_ordernumber") ? (int)en_Ins["bsd_ordernumber"] : 1;
                    strMess.AppendLine("5.3");
                    decimal d_PmsDtlInterestcharge = en_Ins.Contains("bsd_interestchargeamount") ? ((Money)en_Ins["bsd_interestchargeamount"]).Value : 0;
                    decimal d_PmsDtlInterestchargePaid = en_Ins.Contains("bsd_interestwaspaid") ? ((Money)en_Ins["bsd_interestwaspaid"]).Value : 0;
                    int i_PmsDtlbsd_interestchargestatus = en_Ins.Contains("bsd_interestchargestatus") ? ((OptionSetValue)en_Ins["bsd_interestchargestatus"]).Value : 0;
                    int i_PmsDtlActualgraceday = en_Ins.Contains("bsd_actualgracedays") ? (int)en_Ins["bsd_actualgracedays"] : 0;
                    int i_bsd_previousdelays = en_Ins.Contains("bsd_previousdelays") ? (int)en_Ins["bsd_previousdelays"] : 0;
                    strMess.AppendLine("5.4");

                }

                switch (i_pmType)
                {
                    case 100000001://pm type = DEPOSIT PM
                        strMess.AppendLine("case DEPOSIT");
                        voidDeposit(enPayment);
                        #region  ------------------------------- advance payment ------------------------------------
                        //update_AdvPM(((EntityReference)enVoidPayment["bsd_payment"]).Id);


                        #region ------------------ update  payment ----------------------------

                        Entity enPaymentUp = new Entity(enPayment.LogicalName);
                        enPaymentUp.Id = enPayment.Id;
                        enPaymentUp["statuscode"] = new OptionSetValue(100000002);
                        service.Update(enPaymentUp);
                        #endregion
                        #endregion
                        break;
                    case 100000002://Installment
                        strMess.AppendLine("case //Installment");
                        Entity en_Ins = service.Retrieve(((EntityReference)enPayment["bsd_paymentschemedetail"]).LogicalName,
                                ((EntityReference)enPayment["bsd_paymentschemedetail"]).Id, new ColumnSet(true));
                        voidInstallment(enPayment, en_Ins);
                        #region  ---------------------- difference amount > 0  ------------------------------
                        if (d_pmDifference > 0)
                        {
                            #region ---------------- Transaction PM ---------------------
                            if (s_bsd_arraypsdid != "" || s_bsd_arrayinstallmentinterest != "" || s_bsd_arrayfees != "" || s_bsd_arraymicellaneousid != "")// update lai cac transaction pm cua pm
                            {
                                d_TransactionAM = transactionPayment.update_transactionPM(((EntityReference)enVoidPayment["bsd_payment"]).Id, enOptionEntry);
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
                        bool caseNew = false;
                        f_check_1st = installment.check_Ins_Paid(service, enOptionEntry.Id, 1);
                        EntityCollection psdFirst = installment.GetPSD(enOptionEntry.Id.ToString());
                        int t = psdFirst.Entities.Count;
                        Entity detailLast = psdFirst.Entities[t - 1]; // entity cuoi cung ( phase cuoi cung )
                        string detailLastID = detailLast.Id.ToString();
                        int i_PmsDtlStatuscode = en_Ins.Contains("statuscode") ? ((OptionSetValue)en_Ins["statuscode"]).Value : 100000000;
                        int i_Pms_bsd_ordernumber = en_Ins.Contains("bsd_ordernumber") ? (int)en_Ins["bsd_ordernumber"] : 1;
                        #region check & get status of Unit & OE
                        if (i_Pms_bsd_ordernumber == 1)
                        {
                            strMess.AppendLine("case i_Pms_bsd_ordernumber = 1 ");
                            if (enOptionEntry.Contains("bsd_signedcontractdate"))
                            {
                                caseNew = true;
                                //throw new InvalidPluginExecutionException("Contract has been signed. Cannot revert Void payment " + (string)enVoidPayment["bsd_name"] + "!");
                            }
                            else
                            {
                                strMess.AppendLine("case i_PmsDtlStatuscode not paid");
                                f_check_1st = false;
                                sttOE = 100000000; // option
                                sttUnit = 100000003; // deposit
                            }
                        }
                        else
                        {
                            strMess.AppendLine("case else");
                            f_check_1st = true;
                            if (!enOptionEntry.Contains("bsd_signedcontractdate"))
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

                        if (f_check_1st == false && !caseNew)
                        {
                            strMess.AppendLine("case f_check_1st false");
                            sttOE = i_oeStatus;
                            sttUnit = i_Ustatus;
                        }
                        #endregion


                        // update OE
                        // update lai total amount paid, total lateday
                        // tinh lai & update total percent paid

                        d_oe_bsd_totalamountpaid -= d_TransactionAM;

                        enOptionEntry_Up["bsd_totalamountpaid"] = new Money(d_oe_bsd_totalamountpaid > 0 ? d_oe_bsd_totalamountpaid : 0);
                        enOptionEntry_Up["statuscode"] = new OptionSetValue(sttOE);
                        decimal d_bsd_totalpercent = (d_oe_bsd_totalamountpaid / d_oe_amountCalcPercent * 100);
                        //   enOptionEntry_Up["bsd_totalpercent"] = d_bsd_totalpercent;

                        service.Update(enOptionEntry_Up);

                        Entity en_Unit = service.Retrieve(((EntityReference)enOptionEntry["bsd_unitnumber"]).LogicalName, ((EntityReference)enOptionEntry["bsd_unitnumber"]).Id,
                            new ColumnSet(new string[] { "statuscode", "name" }));
                        Entity en_Utmp = new Entity(en_Unit.LogicalName);
                        en_Utmp.Id = en_Unit.Id;
                        en_Utmp["statuscode"] = new OptionSetValue(sttUnit);
                        service.Update(en_Utmp);

                        #endregion
                        break;
                    case 100000003: // Interestcharge
                        strMess.AppendLine("case Interestcharge");
                        voidInterestcharge(enPayment);
                        if (s_bsd_arraypsdid != "" || s_bsd_arrayinstallmentinterest != "" || s_bsd_arrayfees != "" || s_bsd_arraymicellaneousid != "")// update lai cac transaction pm cua pm
                        {
                            d_TransactionAM = transactionPayment.update_transactionPM(((EntityReference)enVoidPayment["bsd_payment"]).Id, enOptionEntry);
                        }

                        // update_AdvPM(((EntityReference)enVoidPayment["bsd_payment"]).Id);

                        break;
                    case 100000004://Fees
                        strMess.AppendLine("case Fees");
                        strMess.AppendLine("Begin Void Fees");
                        EntityCollection encolTransationPayment = transactionPayment.getTracsactionPaymentByPayment(enPayment);
                        if (encolTransationPayment.Entities.Count == 0)
                        {
                            voidFees(enPayment);
                        }
                        else
                        {
                            if (s_bsd_arraypsdid != "" || s_bsd_arrayinstallmentinterest != "" || s_bsd_arrayfees != "" || s_bsd_arraymicellaneousid != "") // update lai cac transaction pm cua pm
                            {
                                d_TransactionAM = transactionPayment.update_transactionPM(((EntityReference)enVoidPayment["bsd_payment"]).Id, enOptionEntry);
                            }
                        }



                        strMess.AppendLine("9.5");
                        //update_AdvPM(((EntityReference)enVoidPayment["bsd_payment"]).Id);

                        strMess.AppendLine("End Void Fees");
                        break;
                    case 100000005://Misc
                        // retrieve MISS
                        strMess.AppendLine("9.1");
                        strMess.AppendLine("void mis type = mis");
                        voidMiscellaneous(enPayment);
                        //if (!enPayment.Contains("bsd_miscellaneous")) throw new InvalidPluginExecutionException("Can not find Misscellaneous information. Please check again!");


                        strMess.AppendLine("9.4");
                        #region ---------------- Transaction PM ---------------------
                        if (s_bsd_arraypsdid != "" || s_bsd_arrayinstallmentinterest != "" || s_bsd_arrayfees != "" || s_bsd_arraymicellaneousid != "") // update lai cac transaction pm cua pm
                        {
                            d_TransactionAM = transactionPayment.update_transactionPM(((EntityReference)enVoidPayment["bsd_payment"]).Id, enOptionEntry);
                        }
                        #endregion
                        strMess.AppendLine("9.5");
                        //update_AdvPM(((EntityReference)enVoidPayment["bsd_payment"]).Id);

                        strMess.AppendLine("9.6");
                        break;
                }
                advancePayment.voidAdvancePayment(enPayment);
                strMess.AppendLine("10");
                //update payment = revert
                Entity en_tmp = new Entity(enPayment.LogicalName);
                strMess.AppendLine("11");
                en_tmp.Id = enPayment.Id;
                en_tmp["statuscode"] = new OptionSetValue(100000002); // revert
                service.Update(en_tmp);
                strMess.AppendLine("12");
                strMess.AppendLine("13");
                createPaymentCopy(enVoidPayment, enPayment);
                //Revert Invoice
                revertInvoice(enPayment);
            }

        }
        private void voidDeposit(Entity enPayment)
        {
            if (!enPayment.Contains("bsd_reservation"))
                throw new InvalidPluginExecutionException("Payment " + (string)enPayment["bsd_name"] + " does not contain Reservation information. Please check again!");
            else
            {
                Entity en_Resv = new Entity(((EntityReference)enPayment["bsd_reservation"]).LogicalName);
                en_Resv = service.Retrieve(((EntityReference)enPayment["bsd_reservation"]).LogicalName, ((EntityReference)enPayment["bsd_reservation"]).Id, new ColumnSet(new string[] {
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
                EntityCollection en_1stinstallment = installment.get_1stinstallment(((EntityReference)enPayment["bsd_reservation"]).Id);

                if (en_1stinstallment.Entities.Count > 0)
                {
                    Entity en_1stTmp = new Entity(en_1stinstallment.Entities[0].LogicalName);
                    en_1stTmp.Id = en_1stinstallment.Entities[0].Id;
                    en_1stTmp["bsd_amountwaspaid"] = new Money(0);
                    decimal bsd_balance = en_1stinstallment.Entities[0].Contains("bsd_balance") ? ((Money)en_1stinstallment.Entities[0]["bsd_balance"]).Value : 0;
                    decimal bsd_amountofthisphase = en_1stinstallment.Entities[0].Contains("bsd_amountofthisphase") ? ((Money)en_1stinstallment.Entities[0]["bsd_amountofthisphase"]).Value : 0;
                    decimal bsd_amountpay = enPayment.Contains("bsd_amountpay") ? ((Money)enPayment["bsd_amountpay"]).Value : 0;
                    bsd_balance += bsd_amountpay;
                    en_1stTmp["bsd_balance"] = new Money(bsd_amountofthisphase);
                    en_1stTmp["bsd_depositamount"] = new Money(0);
                    en_1stTmp["statuscode"] = new OptionSetValue(100000000); // not paid
                    en_1stTmp["bsd_paiddate"] = null;
                    service.Update(en_1stTmp);
                }

                #endregion // end deposit




            }
        }
        private void voidFees(Entity enPayment)
        {
            strMess.AppendLine("6.1");
            string bsd_arrayfees = enPayment.Contains("bsd_arrayfees") ? (string)enPayment["bsd_arrayfees"] : "";
            string bsd_arrayfeesamount = enPayment.Contains("bsd_arrayfeesamount") ? (string)enPayment["bsd_arrayfeesamount"] : "";
            string[] arrId = bsd_arrayfees.Split(',');
            string[] arrAmount = bsd_arrayfeesamount.Split(',');
            strMess.AppendLine("6.2");
            Entity enOptionEntry = service.Retrieve(((EntityReference)enPayment["bsd_optionentry"]).LogicalName, ((EntityReference)enPayment["bsd_optionentry"]).Id, new ColumnSet(true));
            decimal bsd_totalamountpaid = enOptionEntry.Contains("bsd_totalamountpaid") ? ((Money)enOptionEntry["bsd_totalamountpaid"]).Value : 0;
            bool newtype = true;
            for (int i = 0; i < arrId.Length; i++)
            {
                strMess.AppendLine("6.3");
                decimal voidAmount = decimal.Parse(arrAmount[i].ToString());
                string[] arr = arrId[i].Split('_');
                if (arr.Length == 2)
                {
                    string installmentId = arr[0];
                    string type = arr[1];
                    Entity enInstallment = null;
                    strMess.AppendLine("6.4");
                    switch (type)
                    {
                        case "main":
                            enInstallment = service.Retrieve("bsd_paymentschemedetail", new Guid(installmentId), new ColumnSet(new string[] {
                                                    "bsd_maintenancefeesstatus", "bsd_maintenancefeepaid" }));
                            bool bsd_maintenancefeesstatus = enInstallment.Contains("bsd_maintenancefeesstatus") ? (bool)enInstallment["bsd_maintenancefeesstatus"] : false;
                            decimal bsd_maintenancefeepaid = enInstallment.Contains("bsd_maintenancefeepaid") ? ((Money)enInstallment["bsd_maintenancefeepaid"]).Value : 0;
                            bsd_maintenancefeesstatus = false;
                            bsd_maintenancefeepaid -= voidAmount;
                            bsd_totalamountpaid -= voidAmount;
                            enInstallment["bsd_maintenancefeesstatus"] = bsd_maintenancefeesstatus;
                            enInstallment["bsd_maintenancefeepaid"] = new Money(bsd_maintenancefeepaid);
                            strMess.AppendLine("Void bsd_maintenancefeepaid: " + bsd_maintenancefeepaid.ToString());
                            break;
                        case "mana":
                            enInstallment = service.Retrieve("bsd_paymentschemedetail", new Guid(installmentId), new ColumnSet(new string[] {
                                                    "bsd_managementfeesstatus", "bsd_managementfeepaid" }));
                            bool bsd_managementfeesstatus = enInstallment.Contains("bsd_managementfeesstatus") ? (bool)enInstallment["bsd_managementfeesstatus"] : false;
                            decimal bsd_managementfeepaid = enInstallment.Contains("bsd_managementfeepaid") ? ((Money)enInstallment["bsd_managementfeepaid"]).Value : 0;
                            bsd_managementfeesstatus = false;
                            bsd_managementfeepaid -= voidAmount;
                            bsd_totalamountpaid -= voidAmount;
                            enInstallment["bsd_managementfeesstatus"] = bsd_managementfeesstatus;
                            enInstallment["bsd_managementfeepaid"] = new Money(bsd_managementfeepaid);
                            strMess.AppendLine("Void bsd_managementfeepaid: " + bsd_managementfeepaid.ToString());
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
                if (enPayment.Contains("bsd_paymentschemedetail"))
                {
                    Entity en_Ins = service.Retrieve(((EntityReference)enPayment["bsd_paymentschemedetail"]).LogicalName,
                           ((EntityReference)enPayment["bsd_paymentschemedetail"]).Id, new ColumnSet(true));
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

                    EntityCollection ec_transactionPM = transactionPayment.get_TransactionPM(service, enPayment.Id);

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
                                    bsd_totalamountpaid -= d_trans_bsd_amount;
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
                        throw new InvalidPluginExecutionException("Cannot find any Transaction payment (Fees) for payment " + (string)enPayment["bsd_name"] + "!");
                }

            }

        }
        private void voidInterestcharge(Entity enPayment)
        {
            // Transaction Payment
            EntityCollection enclTransactionPayment = transactionPayment.getTracsactionPaymentByPayment(enPayment);
            decimal bsd_differentamount = enPayment.Contains("bsd_differentamount") ? ((Money)enPayment["bsd_differentamount"]).Value : 0;

            // update Interestcharge amount paid & status in installment
            if (enPayment.Contains("bsd_paymentschemedetail") && (enclTransactionPayment.Entities.Count == 0))
            {
                Entity en_Ins = service.Retrieve(((EntityReference)enPayment["bsd_paymentschemedetail"]).LogicalName,
                       ((EntityReference)enPayment["bsd_paymentschemedetail"]).Id, new ColumnSet(true));
                strMess.AppendLine("5.1");
                decimal d_PmsDtlDeposit = en_Ins.Contains("bsd_depositamount") ? ((Money)en_Ins["bsd_depositamount"]).Value : 0;
                decimal d_PmsDtlAmPaid = en_Ins.Contains("bsd_amountwaspaid") ? ((Money)en_Ins["bsd_amountwaspaid"]).Value : 0;
                decimal d_PmsDtlAmPhase = en_Ins.Contains("bsd_amountofthisphase") ? ((Money)en_Ins["bsd_amountofthisphase"]).Value : 0;
                decimal d_PmsDtlBalance = en_Ins.Contains("bsd_balance") ? ((Money)en_Ins["bsd_balance"]).Value : 0;
                strMess.AppendLine("5.2");
                decimal d_PmsDtlWaiverAM = en_Ins.Contains("bsd_waiverinterest") ? ((Money)en_Ins["bsd_waiverinterest"]).Value : 0;
                decimal d_pmAMpay = enPayment.Contains("bsd_amountpay") ? ((Money)enPayment["bsd_amountpay"]).Value : 0;
                decimal d_pmBalance = enPayment.Contains("bsd_balance") ? ((Money)enPayment["bsd_balance"]).Value : 0;
                int i_PmsDtlStatuscode = en_Ins.Contains("statuscode") ? ((OptionSetValue)en_Ins["statuscode"]).Value : 100000000;
                int i_Pms_bsd_ordernumber = en_Ins.Contains("bsd_ordernumber") ? (int)en_Ins["bsd_ordernumber"] : 1;
                strMess.AppendLine("5.3");
                decimal d_PmsDtlInterestcharge = en_Ins.Contains("bsd_interestchargeamount") ? ((Money)en_Ins["bsd_interestchargeamount"]).Value : 0;
                decimal d_PmsDtlInterestchargePaid = en_Ins.Contains("bsd_interestwaspaid") ? ((Money)en_Ins["bsd_interestwaspaid"]).Value : 0;
                int i_PmsDtlbsd_interestchargestatus = en_Ins.Contains("bsd_interestchargestatus") ? ((OptionSetValue)en_Ins["bsd_interestchargestatus"]).Value : 0;
                int i_PmsDtlActualgraceday = en_Ins.Contains("bsd_actualgracedays") ? (int)en_Ins["bsd_actualgracedays"] : 0;
                int i_bsd_previousdelays = en_Ins.Contains("bsd_previousdelays") ? (int)en_Ins["bsd_previousdelays"] : 0;
                strMess.AppendLine("5.4");
                strMess.AppendLine("i_bsd_previousdelays: " + i_bsd_previousdelays.ToString());
                d_PmsDtlInterestchargePaid -= d_pmAMpay < d_pmBalance ? d_pmAMpay : d_pmBalance;
                i_PmsDtlbsd_interestchargestatus = 100000000; // not paid
                Entity tmp_Ins = new Entity(en_Ins.LogicalName); // entity Installment(PMSDtl)
                tmp_Ins.Id = en_Ins.Id;
                tmp_Ins["bsd_interestwaspaid"] = new Money(d_PmsDtlInterestchargePaid);
                tmp_Ins["bsd_interestchargestatus"] = new OptionSetValue(i_PmsDtlbsd_interestchargestatus);
                service.Update(tmp_Ins);
            }
        }
        private void voidInstallment(Entity enPayment, Entity en_Ins)
        {

            strMess.AppendLine("5.1");
            checkPaidInstalment(enPayment);
            Entity enOptionEntry = service.Retrieve(((EntityReference)enPayment["bsd_optionentry"]).LogicalName, ((EntityReference)enPayment["bsd_optionentry"]).Id, new ColumnSet(true));
            decimal d_PmsDtlDeposit = en_Ins.Contains("bsd_depositamount") ? ((Money)en_Ins["bsd_depositamount"]).Value : 0;
            decimal bsd_amountwaspaid = en_Ins.Contains("bsd_amountwaspaid") ? ((Money)en_Ins["bsd_amountwaspaid"]).Value : 0;
            decimal d_PmsDtlAmPhase = en_Ins.Contains("bsd_amountofthisphase") ? ((Money)en_Ins["bsd_amountofthisphase"]).Value : 0;
            decimal d_PmsDtlBalance = en_Ins.Contains("bsd_balance") ? ((Money)en_Ins["bsd_balance"]).Value : 0;
            strMess.AppendLine("5.2");
            decimal d_PmsDtlWaiverAM = en_Ins.Contains("bsd_waiverinstallment") ? ((Money)en_Ins["bsd_waiverinstallment"]).Value : 0;

            int i_PmsDtlStatuscode = en_Ins.Contains("statuscode") ? ((OptionSetValue)en_Ins["statuscode"]).Value : 100000000;
            int i_Pms_bsd_ordernumber = en_Ins.Contains("bsd_ordernumber") ? (int)en_Ins["bsd_ordernumber"] : 1;
            strMess.AppendLine("5.3");
            decimal d_PmsDtlInterestcharge = en_Ins.Contains("bsd_interestchargeamount") ? ((Money)en_Ins["bsd_interestchargeamount"]).Value : 0;
            strMess.AppendLine("bsd_interestchargeamount:" + d_PmsDtlInterestcharge.ToString());
            decimal d_PmsDtlInterestchargePaid = en_Ins.Contains("bsd_interestwaspaid") ? ((Money)en_Ins["bsd_interestwaspaid"]).Value : 0;
            int i_PmsDtlbsd_interestchargestatus = en_Ins.Contains("bsd_interestchargestatus") ? ((OptionSetValue)en_Ins["bsd_interestchargestatus"]).Value : 0;
            int i_PmsDtlActualgraceday = en_Ins.Contains("bsd_actualgracedays") ? (int)en_Ins["bsd_actualgracedays"] : 0;
            strMess.AppendLine("Ins bsd_actualgracedays: " + i_PmsDtlActualgraceday.ToString());
            int i_bsd_previousdelays = en_Ins.Contains("bsd_previousdelays") ? (int)en_Ins["bsd_previousdelays"] : 0;

            strMess.AppendLine("5.4");
            strMess.AppendLine("i_bsd_previousdelays: " + i_bsd_previousdelays.ToString());
            if (d_PmsDtlInterestcharge > 0 && (i_PmsDtlbsd_interestchargestatus == 100000001 || d_PmsDtlInterestchargePaid > 0))//Paid
            {
                //EntityCollection enclPayment = getTransactionPaymentByInterest(en_Ins, enPayment);
                EntityCollection encolTransactionPayment = transactionPayment.getTransactionPaymentByInterest(en_Ins);
                bool allowvoid = true;
                if (encolTransactionPayment.Entities.Count == 1)
                {
                    Entity enTransactionPayment = encolTransactionPayment.Entities[0];
                    EntityReference enrefPayment = enTransactionPayment.Contains("bsd_payment") ? (EntityReference)enTransactionPayment["bsd_payment"] : null;
                    if (enrefPayment.Id != enPayment.Id)
                    {
                        allowvoid = false;
                    }
                }

                if (allowvoid == false)
                {
                    throw new InvalidPluginExecutionException("You can't approve this void payment! You must void payment for interest of this installment!");
                }
            }
            decimal bsd_totalamountpaidphase = enPayment.Contains("bsd_totalamountpaidphase") ? ((Money)enPayment["bsd_totalamountpaidphase"]).Value : 0;
            decimal d_pmAMpay = enPayment.Contains("bsd_amountpay") ? ((Money)enPayment["bsd_amountpay"]).Value : 0;
            decimal d_pmBalance = enPayment.Contains("bsd_balance") ? ((Money)enPayment["bsd_balance"]).Value : 0;
            decimal d_pmInterestAm = enPayment.Contains("bsd_interestcharge") ? ((Money)enPayment["bsd_interestcharge"]).Value : 0;
            int bsd_latedays = enPayment.Contains("bsd_latedays") ? (int)enPayment["bsd_latedays"] : 0;
            strMess.AppendLine("Payment bsd_interestcharge: " + d_pmInterestAm.ToString());
            bool f_latePM = enPayment.Contains("bsd_latepayment") ? (bool)enPayment["bsd_latepayment"] : false;
            decimal d_pmDifference = enPayment.Contains("bsd_differentamount") ? ((Money)enPayment["bsd_differentamount"]).Value : 0;
            decimal d_oe_bsd_totalamountpaid = enOptionEntry.Contains("bsd_totalamountpaid") ? ((Money)enOptionEntry["bsd_totalamountpaid"]).Value : 0;
            strMess.AppendLine("bsd_totalamountpaidphase: " + bsd_totalamountpaidphase);

            bsd_amountwaspaid -= d_pmAMpay > d_pmBalance ? d_pmBalance : d_pmAMpay;
            //bsd_amountwaspaid -= bsd_totalamountpaidphase;
            strMess.AppendLine("bsd_amountwaspaid: " + bsd_amountwaspaid);
            d_PmsDtlBalance = d_PmsDtlAmPhase - bsd_amountwaspaid - d_PmsDtlDeposit - d_PmsDtlWaiverAM;
            if (i_Pms_bsd_ordernumber == 1 && bsd_amountwaspaid == 0)
            {
                d_oe_bsd_totalamountpaid = d_PmsDtlDeposit; // luon luon cap nhat total amount of OE =
            }
            else
                d_oe_bsd_totalamountpaid -= d_pmAMpay > d_pmBalance ? d_pmBalance : d_pmAMpay;

            //tmp_Ins["bsd_amountwaspaid"] = new Money(d_PmsDtlAmPaid > 0 ? d_PmsDtlAmPaid : 0);

            if (d_pmInterestAm > 0)
            {

                if (f_latePM == true)
                {
                    i_PmsDtlbsd_interestchargestatus = 100000000;
                    d_PmsDtlInterestchargePaid -= d_pmInterestAm >= d_pmDifference ? d_pmDifference : d_pmInterestAm;


                }

                d_PmsDtlInterestcharge -= d_pmInterestAm;
                i_PmsDtlActualgraceday -= bsd_latedays;
                if (i_PmsDtlActualgraceday < 0)
                {
                    i_PmsDtlActualgraceday = 0;
                }
                strMess.AppendLine("bsd_interestchargeamount:" + d_PmsDtlInterestcharge.ToString());
                strMess.AppendLine("i_PmsDtlActualgraceday:" + i_PmsDtlActualgraceday.ToString());

                #region -------- actual late day -----------
                //EntityCollection ec_paymentPrevious = get_previous_PM(en_Ins.Id, enPayment.Id, enOptionEntry.Id);
                //strMess.AppendLine("ec_paymentPrevious:" + ec_paymentPrevious.Entities.Count.ToString());
                //strMess.AppendLine((ec_paymentPrevious.Entities.Count <= 0).ToString());

                //if (ec_paymentPrevious.Entities.Count <= 0)
                //{
                //    //if (i_bsd_previousdelays > 0)
                //    //{
                //    //    i_PmsDtlActualgraceday = i_bsd_previousdelays;
                //    //}
                //    //else
                //    //{
                //    //    i_PmsDtlActualgraceday = 0;
                //    //    strMess.AppendLine("==0");
                //    //}
                //    i_PmsDtlActualgraceday = 0;
                //}
                //else
                //{
                //    i_PmsDtlActualgraceday = ec_paymentPrevious.Entities[0].Contains("bsd_latedays") ? (int)ec_paymentPrevious.Entities[0]["bsd_latedays"] : 0;
                //}
                #endregion
                Entity tmp_Ins = new Entity(en_Ins.LogicalName); // entity Installment(PMSDtl)
                tmp_Ins.Id = en_Ins.Id;
                tmp_Ins["bsd_actualgracedays"] = i_PmsDtlActualgraceday;
                tmp_Ins["bsd_interestchargestatus"] = new OptionSetValue(i_PmsDtlbsd_interestchargestatus); // not paid
                tmp_Ins["bsd_interestchargeamount"] = new Money(d_PmsDtlInterestcharge > 0 ? d_PmsDtlInterestcharge : 0);
                tmp_Ins["bsd_interestwaspaid"] = new Money(d_PmsDtlInterestchargePaid > 0 ? d_PmsDtlInterestchargePaid : 0);
                //throw new InvalidPluginExecutionException("bsd_actualgracedays: "+ i_PmsDtlActualgraceday.ToString());

                service.Update(tmp_Ins);
            }
            // decimal bsd_amountwaspaid = en_Ins.Contains("bsd_amountwaspaid") ? ((Money)en_Ins["bsd_amountwaspaid"]).Value : 0;
            //bsd_amountwaspaid -= d_pmAMpay;
            //decimal d_PmsDtlBalance = en_Ins.Contains("bsd_balance") ? ((Money)en_Ins["bsd_balance"]).Value : 0;
            //d_PmsDtlBalance += d_pmAMpay;
            //decimal d_PmsDtlWaiverAM = en_Ins.Contains("bsd_waiverinstallment") ? ((Money)en_Ins["bsd_waiverinstallment"]).Value : 0;
            //update installment
            Entity en_tmp1 = new Entity(en_Ins.LogicalName);
            en_tmp1.Id = en_Ins.Id;
            en_tmp1["statuscode"] = new OptionSetValue(100000000); // not paid
            en_tmp1["bsd_paiddate"] = null;
            //Installment[Amount Was Paid] = Installment[Amount of This Phase] - Payment[Total amount paid (phase)]
            //throw new InvalidPluginExecutionException("aaaaaaaaaaaaaaa");
            en_tmp1["bsd_amountwaspaid"] = new Money(bsd_amountwaspaid > 0 ? bsd_amountwaspaid : 0);
            en_tmp1["bsd_balance"] = new Money(d_PmsDtlBalance);
            //tmp_Ins["bsd_actualgracedays"] = i_PmsDtlActualgraceday;
            //tmp_Ins["bsd_interestchargestatus"] = new OptionSetValue(i_PmsDtlbsd_interestchargestatus); // not paid
            //tmp_Ins["bsd_interestchargeamount"] = new Money(d_PmsDtlInterestcharge > 0 ? d_PmsDtlInterestcharge : 0);

            //tmp_Ins["bsd_interestwaspaid"] = new Money(d_PmsDtlInterestchargePaid > 0 ? d_PmsDtlInterestchargePaid:0);
            service.Update(en_tmp1);

        }
        private void checkPaidInstalment(Entity paymentEn)
        {
            if (paymentEn.Contains("bsd_paymentschemedetail") && paymentEn.Contains("bsd_optionentry"))
            {
                Entity PaymentDetailEn = service.Retrieve("bsd_paymentschemedetail", ((EntityReference)paymentEn["bsd_paymentschemedetail"]).Id, new ColumnSet(true));
                int bsd_ordernumber = PaymentDetailEn.Contains("bsd_ordernumber") ? (int)PaymentDetailEn["bsd_ordernumber"] : 0;
                if (paymentEn.Contains("bsd_arraypsdid"))
                {
                    int bsd_ordernumber2 = getNumberMax(((EntityReference)paymentEn["bsd_paymentschemedetail"]).Id, (string)paymentEn["bsd_arraypsdid"]);
                    if (bsd_ordernumber2 > bsd_ordernumber) bsd_ordernumber = bsd_ordernumber2;
                }
                if (bsd_ordernumber > 0)
                {
                    var fetchXml = $@"<?xml version=""1.0"" encoding=""utf-16""?>
                    <fetch top=""1"">
                      <entity name=""bsd_paymentschemedetail"">
                        <attribute name=""bsd_paymentschemedetailid"" />
                        <attribute name=""bsd_ordernumber"" />
                        <filter>
                          <condition attribute=""bsd_optionentry"" operator=""eq"" value=""{((EntityReference)paymentEn["bsd_optionentry"]).Id}"" />
                          <condition attribute=""bsd_ordernumber"" operator=""gt"" value=""{bsd_ordernumber}"" />
                          <condition attribute=""bsd_amountwaspaid"" operator=""gt"" value=""{0}"" />
                        </filter>
                      </entity>
                    </fetch>";
                    EntityCollection entc = service.RetrieveMultiple(new FetchExpression(fetchXml));
                    if (entc.Entities.Count > 0) throw new InvalidPluginExecutionException("Please cancel the payment for the receipts of the next installment first.");
                }
            }
            var fetchXml2 = $@"<?xml version=""1.0"" encoding=""utf-16""?>
                    <fetch>
                      <entity name=""bsd_advancepayment"">
                        <attribute name=""bsd_advancepaymentid"" />
                        <attribute name=""bsd_amount"" />
                        <attribute name=""bsd_remainingamount"" />
                        <filter>
                          <condition attribute=""bsd_payment"" operator=""eq"" value=""{paymentEn.Id}"" />
                        </filter>
                      </entity>
                    </fetch>";
            EntityCollection entc2 = service.RetrieveMultiple(new FetchExpression(fetchXml2));
            foreach (Entity entity in entc2.Entities)
            {
                decimal bsd_amount = entity.Contains("bsd_amount") ? ((Money)entity["bsd_amount"]).Value : 0;
                decimal bsd_remainingamount = entity.Contains("bsd_remainingamount") ? ((Money)entity["bsd_remainingamount"]).Value : 0;
                if (bsd_amount != bsd_remainingamount) throw new InvalidPluginExecutionException("COA has been paid off, please check the information again.");
            }
        }
        private int getNumberMax(Guid id, string listID)
        {
            int number = 0;
            string[] s_idINS = { };
            s_idINS = listID.Split(',');
            StringBuilder xml2 = new StringBuilder();
            xml2.AppendLine("<fetch top='1'>");
            xml2.AppendLine("<entity name='bsd_paymentschemedetail'>");
            xml2.AppendLine("<attribute name='bsd_ordernumber' />");
            xml2.AppendLine("<filter type='and'>");
            xml2.AppendLine("<condition attribute='bsd_paymentschemedetailid' operator='in'>");
            xml2.AppendLine(string.Format("<value>{0}</value>", id));
            foreach (string item in s_idINS)
                xml2.AppendLine(string.Format("<value>{0}</value>", item));
            xml2.AppendLine("</condition>");
            xml2.AppendLine("</filter>");
            xml2.AppendLine("<order attribute='bsd_ordernumber' descending='true' />");
            xml2.AppendLine("</entity>");
            xml2.AppendLine("</fetch>");
            EntityCollection entityCollection = service.RetrieveMultiple(new FetchExpression(xml2.ToString()));
            foreach (Entity entity in entityCollection.Entities)
            {
                number = (int)entity["bsd_ordernumber"];
            }
            return number;
        }
        private void voidMiscellaneous(Entity enPayment)
        {
            if (enPayment.Contains("bsd_miscellaneous") == true)
            {
                strMess.AppendLine("9.1.2");
                //Nghiệp vụ cũ
                Entity en_Mis = service.Retrieve(((EntityReference)enPayment["bsd_miscellaneous"]).LogicalName, ((EntityReference)enPayment["bsd_miscellaneous"]).Id,
                new ColumnSet(new string[] { "bsd_name", "bsd_balance", "bsd_miscellaneousnumber", "statuscode","bsd_units","bsd_optionentry","bsd_amount","bsd_paidamount",
                                            "bsd_installment","bsd_installmentnumber"}));
                int f_MIS_status = en_Mis.Contains("statuscode") ? ((OptionSetValue)en_Mis["statuscode"]).Value : 1;

                decimal d_bsd_amount = en_Mis.Contains("bsd_amount") ? ((Money)en_Mis["bsd_amount"]).Value : 0;
                decimal d_bsd_paidamount = en_Mis.Contains("bsd_paidamount") ? ((Money)en_Mis["bsd_paidamount"]).Value : 0;
                decimal d_bsd_balance = en_Mis.Contains("bsd_balance") ? ((Money)en_Mis["bsd_balance"]).Value : 0;
                Entity en_MIS_up = new Entity(en_Mis.LogicalName);
                en_MIS_up.Id = en_Mis.Id;
                strMess.AppendLine("9.2");
                decimal d_pmAMpay = enPayment.Contains("bsd_amountpay") ? ((Money)enPayment["bsd_amountpay"]).Value : 0;
                decimal d_pmBalance = enPayment.Contains("bsd_balance") ? ((Money)enPayment["bsd_balance"]).Value : 0;
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
                strMess.AppendLine("9.3");
                en_MIS_up["bsd_paidamount"] = new Money(d_bsd_paidamount);
                en_MIS_up["bsd_balance"] = new Money(d_bsd_balance);
                en_MIS_up["statuscode"] = new OptionSetValue(f_MIS_status);
                service.Update(en_MIS_up);
            }
            else
            {
                /*strMess.AppendLine("9.1.3");
                //Nghiệp vụ mới: Thạnh Đỗ
                //1 Lấy cái Miss trong Array Miscellaneous ID
                string missID = enPayment.Contains("bsd_arraymicellaneousid") ? (string)enPayment["bsd_arraymicellaneousid"] : "";
                string missAM = enPayment.Contains("bsd_arraymicellaneousamount") ? (string)enPayment["bsd_arraymicellaneousamount"] : "";
                strMess.AppendLine("missID: " + missID.ToString());
                strMess.AppendLine("missAM: " + missAM.ToString());
                if (missID != "")
                {
                    string[] arrId = missID.Split(',');
                    string[] arrMissAmount = missAM.Split(',');
                    strMess.AppendLine("arrId: " + arrId.ToString());
                    strMess.AppendLine("arrMissAmount: " + arrMissAmount.ToString());
                    for (int i = 0; i < arrId.Length; i++)
                    {
                        string[] arr = arrId[i].Split('_');
                        string micellaneousid = arr[0];
                        decimal miss = decimal.Parse(arrMissAmount[i].ToString());

                        Entity enMiss = miscellaneous.getMiscellaneous(micellaneousid);
                        strMess.AppendLine("arrId1");
                        strMess.AppendLine("miss: " + miss.ToString());
                        decimal bsd_paidamount = enMiss.Contains("bsd_paidamount") ? ((Money)enMiss["bsd_paidamount"]).Value : 0;
                        decimal bsd_totalamount = enMiss.Contains("bsd_totalamount") ? ((Money)enMiss["bsd_totalamount"]).Value : 0;
                        strMess.AppendLine("bsd_totalamount: " + bsd_totalamount.ToString());
                        bsd_paidamount -= miss;
                        strMess.AppendLine("bsd_paidamount after void:" + bsd_paidamount.ToString());
                        decimal bsd_balance = bsd_totalamount - bsd_paidamount;
                        strMess.AppendLine("bsd_balance after void:" + bsd_balance.ToString());
                        Entity en_MIS_up = new Entity(enMiss.LogicalName, enMiss.Id);
                        //2 Cập lại sô tiền paid và balance của từng miss
                        en_MIS_up["bsd_paidamount"] = new Money(bsd_paidamount);
                        en_MIS_up["bsd_balance"] = new Money(bsd_balance);
                        //3 Cập nhật lại trạng thái not paid
                        en_MIS_up["statuscode"] = new OptionSetValue(1);
                        //4 Lưu lại thông tin của miss
                        service.Update(en_MIS_up);
                        strMess.AppendLine("Update miss");
                    }
                }*/
            }
        }
        private void createPaymentCopy(Entity enVoidPayment, Entity enPayment)
        {
            //decimal d_pmAMpay = enPayment.Contains("bsd_amountpay") ? ((Money)enPayment["bsd_amountpay"]).Value : 0;
            //decimal d_pmAMPhase = enPayment.Contains("bsd_totalamountpayablephase") ? ((Money)enPayment["bsd_totalamountpayablephase"]).Value : 0;
            //decimal bsd_totalamountpaidphase = enPayment.Contains("bsd_totalamountpaidphase") ? ((Money)enPayment["bsd_totalamountpaidphase"]).Value : 0;
            //decimal d_pmMaintenance = enPayment.Contains("bsd_maintenancefee") ? ((Money)enPayment["bsd_maintenancefee"]).Value : 0;
            //decimal d_pmManagement = enPayment.Contains("bsd_managementfee") ? ((Money)enPayment["bsd_managementfee"]).Value : 0;
            //decimal d_pmDeposit = enPayment.Contains("bsd_depositamount") ? ((Money)enPayment["bsd_depositamount"]).Value : 0;
            //decimal d_pmDifference = enPayment.Contains("bsd_differentamount") ? ((Money)enPayment["bsd_differentamount"]).Value : 0;
            //decimal d_pmBalance = enPayment.Contains("bsd_balance") ? ((Money)enPayment["bsd_balance"]).Value : 0;
            //decimal d_pmInterestAm = enPayment.Contains("bsd_interestcharge") ? ((Money)enPayment["bsd_interestcharge"]).Value : 0;
            //bool f_latePM = enPayment.Contains("bsd_latepayment") ? (bool)enPayment["bsd_latepayment"] : false;
            //int i_PmLateday = enPayment.Contains("bsd_latedays") ? (int)enPayment["bsd_latedays"] : 0;
            //EntityReference bsd_paymentschemedetail = enPayment.Contains("bsd_paymentschemedetail") ? (EntityReference)enPayment["bsd_paymentschemedetail"] : null;
            //DateTime bsd_duedateinstallment = enPayment.Contains("bsd_duedateinstallment") ? (DateTime)enPayment["bsd_duedateinstallment"] : new DateTime(0);
            //strMess.AppendLine("bsd_duedateinstallment: " + bsd_duedateinstallment.ToString());
            //strMess.AppendLine("Payment bsd_latedays:" + ToString());
            //int i_pmType = enPayment.Contains("bsd_paymenttype") ? ((OptionSetValue)enPayment["bsd_paymenttype"]).Value : 0;
            //Entity en_NewPM = new Entity(enPayment.LogicalName);
            //en_NewPM.Id = Guid.NewGuid();
            //strMess.AppendLine("14");

            //en_NewPM["bsd_name"] = (string)enPayment["bsd_name"] + "_Copy"; // active
            //en_NewPM["statuscode"] = new OptionSetValue(1); // active

            //en_NewPM["bsd_amountpay"] = new Money(d_pmAMpay);
            //en_NewPM["bsd_totalamountpayablephase"] = new Money(d_pmAMPhase); // active
            //en_NewPM["bsd_totalamountpaidphase"] = new Money(bsd_totalamountpaidphase); // active
            //en_NewPM["bsd_maintenancefee"] = new Money(d_pmMaintenance); // active
            //en_NewPM["bsd_managementfee"] = new Money(d_pmManagement); // active
            //en_NewPM["bsd_depositamount"] = new Money(d_pmDeposit); // active
            //en_NewPM["bsd_differentamount"] = new Money(d_pmDifference); // active
            //en_NewPM["bsd_balance"] = new Money(d_pmBalance); // active
            //en_NewPM["bsd_interestcharge"] = new Money(d_pmInterestAm); // active
            //en_NewPM["bsd_latedays"] = i_PmLateday; // active
            //strMess.AppendLine("15");

            //en_NewPM["bsd_latepayment"] = (bool)f_latePM;
            //en_NewPM["bsd_paymenttype"] = new OptionSetValue(i_pmType); // active
            //en_NewPM["bsd_units"] = (EntityReference)enPayment["bsd_units"];
            //en_NewPM["bsd_purchaser"] = (EntityReference)enPayment["bsd_purchaser"];
            //en_NewPM["bsd_paymentmode"] = (EntityReference)enPayment["bsd_paymentmode"];
            //en_NewPM["bsd_project"] = (EntityReference)enPayment["bsd_project"];
            //en_NewPM["bsd_transactiontype"] = new OptionSetValue(((OptionSetValue)enPayment["bsd_transactiontype"]).Value);
            //strMess.AppendLine("16");

            //en_NewPM["bsd_arraypsdid"] = enPayment.Contains("bsd_arraypsdid") ? (string)enPayment["bsd_arraypsdid"] : "";
            //en_NewPM["bsd_arrayamountpay"] = enPayment.Contains("bsd_arrayamountpay") ? (string)enPayment["bsd_arrayamountpay"] : "";
            //en_NewPM["bsd_arrayinstallmentinterest"] = enPayment.Contains("bsd_arrayinstallmentinterest") ? (string)enPayment["bsd_arrayinstallmentinterest"] : "";
            //en_NewPM["bsd_arrayinterestamount"] = enPayment.Contains("bsd_arrayinterestamount") ? (string)enPayment["bsd_arrayinterestamount"] : "";
            //strMess.AppendLine("17");

            //en_NewPM["bsd_arrayfees"] = enPayment.Contains("bsd_arrayfees") ? (string)enPayment["bsd_arrayfees"] : "";
            //en_NewPM["bsd_arrayfeesamount"] = enPayment.Contains("bsd_arrayfeesamount") ? (string)enPayment["bsd_arrayfeesamount"] : "";
            //strMess.AppendLine("18");

            //decimal d_bsd_waiverinstallment = enPayment.Contains("bsd_waiverinstallment") ? ((Money)enPayment["bsd_waiverinstallment"]).Value : 0;
            //decimal d_bsd_waiverinterest = enPayment.Contains("bsd_waiverinterest") ? ((Money)enPayment["bsd_waiverinterest"]).Value : 0;
            //decimal d_bsd_waiveramount = enPayment.Contains("bsd_waiveramount") ? ((Money)enPayment["bsd_waiveramount"]).Value : 0;
            //en_NewPM["bsd_waiverinstallment"] = new Money(d_bsd_waiverinstallment);
            //en_NewPM["bsd_waiverinterest"] = new Money(d_bsd_waiverinterest);
            //en_NewPM["bsd_waiveramount"] = new Money(d_bsd_waiveramount);
            //strMess.AppendLine("19");

            //// 170516 - update field bsd_createbyrevert = yes
            //en_NewPM["bsd_createbyrevert"] = true;
            //strMess.AppendLine("19.1");
            //if (enPayment.Contains("bsd_optionentry"))
            //{
            //    strMess.AppendLine("19.2");
            //    en_NewPM["bsd_optionentry"] = (EntityReference)enPayment["bsd_optionentry"];
            //    strMess.AppendLine("19.2.1");
            //    // en_NewPM["bsd_reservation"] = (EntityReference)enPayment["bsd_reservation"];
            //    //holu
            //    en_NewPM["bsd_paymentschemedetail"] = bsd_paymentschemedetail;
            //    if (bsd_duedateinstallment.Ticks != 0)
            //    {
            //        en_NewPM["bsd_duedateinstallment"] = bsd_duedateinstallment;
            //    }

            //    strMess.AppendLine("19.3");
            //}
            //else
            //    en_NewPM["bsd_reservation"] = (EntityReference)enPayment["bsd_reservation"];

            //strMess.AppendLine("20");
            //en_NewPM["bsd_paymentactualtime"] = enPayment["bsd_paymentactualtime"];
            //en_NewPM["bsd_exchangeratetext"] = enPayment.Contains("bsd_exchangeratetext") ? enPayment["bsd_exchangeratetext"] : null;
            //en_NewPM["bsd_exchangemoneytext"] = enPayment.Contains("bsd_exchangemoneytext") ? enPayment["bsd_exchangemoneytext"] : null;
            //strMess.AppendLine("21");
            //// MISS 170814
            //if (enPayment.Contains("bsd_miscellaneous"))
            //    en_NewPM["bsd_miscellaneous"] = (EntityReference)enPayment["bsd_miscellaneous"];
            //en_NewPM["bsd_arraymicellaneousid"] = enPayment.Contains("bsd_arraymicellaneousid") ? enPayment["bsd_arraymicellaneousid"] : "";
            //en_NewPM["bsd_arraymicellaneousamount"] = enPayment.Contains("bsd_arraymicellaneousamount") ? enPayment["bsd_arraymicellaneousamount"] : "";
            //strMess.AppendLine("22");
            //service.Create(en_NewPM);
            DateTime d_now = RetrieveLocalTimeFromUTCTime(DateTime.Now);
            strMess.AppendLine("d_now: " + d_now.ToString());
            // update new payment to void payment entity
            Entity en_voidTmp = new Entity(enVoidPayment.LogicalName);
            en_voidTmp.Id = enVoidPayment.Id;
            //en_voidTmp["bsd_newpayment"] = en_NewPM.ToEntityReference();
            en_voidTmp["bsd_approvaldate"] = d_now;
            en_voidTmp["bsd_approver"] = new EntityReference("systemuser", context.UserId);
            strMess.AppendLine("Update Approver Void");
            service.Update(en_voidTmp);
            strMess.AppendLine("23");
            //throw new InvalidPluginExecutionException("aaaaaaaaaaaaaaaaaa");
        }
        private void revertInvoice(Entity enPayment)
        {
            strMess.AppendLine("revertInvoice");
            // Define Condition Values
            var QEbsd_invoice_bsd_payment = enPayment.Id;

            // Instantiate QueryExpression QEbsd_invoice
            var QEbsd_invoice = new QueryExpression("bsd_invoice");

            // Add all columns to QEbsd_invoice.ColumnSet
            QEbsd_invoice.ColumnSet.AllColumns = true;

            // Define filter QEbsd_invoice.Criteria
            QEbsd_invoice.Criteria.AddCondition("bsd_payment", ConditionOperator.Equal, QEbsd_invoice_bsd_payment);
            EntityCollection encolInvoice = service.RetrieveMultiple(QEbsd_invoice);
            foreach (Entity enInvoice in encolInvoice.Entities)
            {
                Entity enInvoiceUpdate = new Entity(enInvoice.LogicalName, enInvoice.Id);
                enInvoiceUpdate["statuscode"] = new OptionSetValue(100000001);//Revert
                service.Update(enInvoiceUpdate);
            }
        }
        public EntityCollection get_previous_PM(Guid pmsID, Guid pmID, Guid oeID)
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
            EntityCollection entc = service.RetrieveMultiple(new FetchExpression(fetchXml));
            return entc;
        }
        public DateTime RetrieveLocalTimeFromUTCTime(DateTime utcTime)
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
