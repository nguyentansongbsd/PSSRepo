using Microsoft.Crm.Sdk.Messages;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Query;
using System;
using System.Text;

namespace Plugin_VoidPayment_updatePendingPM
{
    class ApplyDocument
    {
        IOrganizationService service = null;
        IPluginExecutionContext context;
        AdvancePayment advancePayment;
        TransactionPayment transactionPayment;
        Installment installment;
        Miscellaneous miscellaneous;
        int i_Ustatus;
        int i_oeStatus;
        public ApplyDocument(IOrganizationService service, IPluginExecutionContext context)
        {
            this.service = service;
            this.context = context;
            advancePayment = new AdvancePayment(service, context);
            transactionPayment = new TransactionPayment(service, context);
            installment = new Installment(service, context);
            miscellaneous = new Miscellaneous(service, context);
        }
        public void voidApplyDocument(Entity en_voidPM, Entity en_app)
        {
            DateTime d_now = RetrieveLocalTimeFromUTCTime(DateTime.Now);
            int i_bsd_transactiontype = ((OptionSetValue)en_app["bsd_transactiontype"]).Value;
            checkInput(en_app);
            string s_bsd_arraypsdid = en_app.Contains("bsd_arraypsdid") ? (string)en_app["bsd_arraypsdid"] : "";
            string s_bsd_arrayamountpay = en_app.Contains("bsd_arrayamountpay") ? (string)en_app["bsd_arrayamountpay"] : "";
            if (i_bsd_transactiontype == 1)//deposit
            {
                string[] s_psd = s_bsd_arraypsdid.Split(',');
                string[] s_Amp = s_bsd_arrayamountpay.Split(',');
                int i_psd = s_psd.Length;

                for (int m = 0; m < i_psd; m++)
                {
                    voidDeposit(Guid.Parse(s_psd[m]), decimal.Parse(s_Amp[m]), d_now, en_app);
                }

            }
            else if (i_bsd_transactiontype == 2)//installment
            {
                voidInstallment(en_voidPM, en_app);
            }
            // ---------------  update advance payment list -------------------
            decimal d_bsd_totalapplyamount = en_app.Contains("bsd_totalapplyamount") ? ((Money)en_app["bsd_totalapplyamount"]).Value : 0;
            decimal d_tmp = d_bsd_totalapplyamount;
            string s_bsd_arrayadvancepayment = (string)en_app["bsd_arrayadvancepayment"];
            string s_bsd_arrayamountadvance = (string)en_app["bsd_arrayamountadvance"];
            string[] s_eachAdv = s_bsd_arrayadvancepayment.Split(',');
            string[] s_amAdv = s_bsd_arrayamountadvance.Split(',');
            int i_adv = s_eachAdv.Length;
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
            //--------------- end update adv ----------------
            Entity en_appUp = new Entity(en_app.LogicalName);
            en_appUp.Id = en_app.Id;
            en_appUp["statuscode"] = new OptionSetValue(100000004);
            service.Update(en_appUp);
            EntityCollection ec_appDtl = Get_appDtl(en_app.Id);
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

        }
        public void checkInput(Entity en_app)
        {
            if (((OptionSetValue)en_app["statuscode"]).Value == 100000004)
                throw new InvalidPluginExecutionException("Apply document '" + (string)en_app["bsd_name"] + "' has been revert!");
            if (!en_app.Contains("bsd_transactiontype"))
                throw new InvalidPluginExecutionException("Please choose 'Type of payment'!");
        }
        public void voidDeposit(Guid quoteId, decimal d_amp, DateTime d_now, Entity en_app)
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

            // ----------- open state of reservation ----------
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
        public void voidInstallment(Entity en_voidPM, Entity en_app)
        {

            i_Ustatus = 100000003; // deposit
            i_oeStatus = 100000000;// option

            // ------------ retrieve OE -----------

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
            //int i_Unitstatus = 100000003;

            decimal d_oe_bsd_totalpercent = en_OE.Contains("bsd_totalpercent") ? (decimal)en_OE["bsd_totalpercent"] : 0;

            decimal d_oe_bsd_freightamount = en_OE.Contains("bsd_freightamount") ? ((Money)en_OE["bsd_freightamount"]).Value : 0;
            decimal d_oe_amountCalcPercent = d_oe_totalamount - d_oe_bsd_freightamount;

            bool f_OE_Signcontract = en_OE.Contains("bsd_signedcontractdate") ? true : false;


            EntityCollection psdFirst = installment.GetPSD(en_OE.Id.ToString());
            //Entity detailFirst = psdFirst.Entities[0];

            //  ------------ installment ----------------
            voidPayInstallment(en_voidPM, en_app, en_OE);
            // --------------------------------  interest charge -----------------------------
            voidPayInterest(en_voidPM, en_app, en_OE);
            // -------------------fees ----------------------------------
            voidPayFees(en_voidPM, en_app, en_OE);
            // ----------------------------MISS---------------------------
            voidPayMisc(en_voidPM, en_app, en_OE);

            //----------------------- end MISS ---------------------


        }
        private void checkPaidInstalment(string[] s_psd, Guid oe)
        {
            int bsd_ordernumber = getNumberMax(s_psd);
            if (bsd_ordernumber > 0)
            {
                var fetchXml = $@"<?xml version=""1.0"" encoding=""utf-16""?>
                    <fetch top=""1"">
                      <entity name=""bsd_paymentschemedetail"">
                        <attribute name=""bsd_paymentschemedetailid"" />
                        <attribute name=""bsd_ordernumber"" />
                        <filter>
                          <condition attribute=""bsd_optionentry"" operator=""eq"" value=""{oe}"" />
                          <condition attribute=""bsd_ordernumber"" operator=""gt"" value=""{bsd_ordernumber}"" />
                          <condition attribute=""bsd_amountwaspaid"" operator=""gt"" value=""{0}"" />
                        </filter>
                      </entity>
                    </fetch>";
                EntityCollection entc = service.RetrieveMultiple(new FetchExpression(fetchXml));
                if (entc.Entities.Count > 0) throw new InvalidPluginExecutionException("Please cancel the payment for the receipts of the next installment first.");
            }
        }
        private int getNumberMax(string[] s_idINS)
        {
            int number = 0;
            StringBuilder xml2 = new StringBuilder();
            xml2.AppendLine("<fetch top='1'>");
            xml2.AppendLine("<entity name='bsd_paymentschemedetail'>");
            xml2.AppendLine("<attribute name='bsd_ordernumber' />");
            xml2.AppendLine("<filter type='and'>");
            xml2.AppendLine("<condition attribute='bsd_paymentschemedetailid' operator='in'>");
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
        private void voidPayInstallment(Entity en_voidPM, Entity en_app, Entity en_OE)
        {
            string s_bsd_arraypsdid = en_app.Contains("bsd_arraypsdid") ? (string)en_app["bsd_arraypsdid"] : "";
            string s_bsd_arrayamountpay = en_app.Contains("bsd_arrayamountpay") ? (string)en_app["bsd_arrayamountpay"] : "";
            if (s_bsd_arraypsdid != "")
            {
                string[] s_psd = s_bsd_arraypsdid.Split(',');
                checkPaidInstalment(s_psd, en_OE.Id);
                string[] s_Amp = s_bsd_arrayamountpay.Split(',');
                int i_psd = s_psd.Length;
                EntityCollection psdFirst = installment.GetPSD(en_OE.Id.ToString());
                EntityCollection ec_ins = get_ecIns(service, s_psd);
                for (int i = 0; i < ec_ins.Entities.Count; i++)
                {
                    bool f_check_1st = false;
                    bool caseNew = false;
                    f_check_1st = installment.check_Ins_Paid(service, en_OE.Id, 1);
                    int t = psdFirst.Entities.Count;
                    Entity detailLast = psdFirst.Entities[t - 1]; // entity cuoi cung ( phase cuoi cung )
                    string detailLastID = detailLast.Id.ToString();

                    int i_outstanding_AppDtl = 0;
                    decimal d_ampay = 0;
                    int sttOE = 100000002; // sign contract OE
                    int sttUnit = 100000002; // unit = sold

                    //  ------------ retrieve installment-----------------------
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


                    // ----------------- get oustanding day from appdtl of installment ( base on installment id ) --------------------
                    EntityCollection ec_appDetail = get_appDtl_Ins(en_app.Id, en_pms.Id);
                    decimal d_interAM = 0;
                    if (ec_appDetail.Entities.Count > 0)
                    {
                        i_outstanding_AppDtl = ec_appDetail.Entities[0].Contains("bsd_actualgracedays") ? (int)ec_appDetail.Entities[0]["bsd_actualgracedays"] : 0;
                        d_interAM = ec_appDetail.Entities[0].Contains("bsd_interestchargeamount") ? ((Money)ec_appDetail.Entities[0]["bsd_interestchargeamount"]).Value : 0;
                        d_ampay = ((Money)ec_appDetail.Entities[0]["bsd_amountapply"]).Value;
                    }


                    // check dieu kien

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



                    //   ------------------------------------ update -----------------------------------------

                    psd_amountPaid -= d_ampay;
                    psd_bsd_balance += d_ampay;
                    int i_bsd_totallatedays = en_OE.Contains("bsd_totallatedays") ? (int)en_OE["bsd_totallatedays"] : 0;
                    decimal d_oe_bsd_totalamountpaid = en_OE.Contains("bsd_totalamountpaid") ? ((Money)en_OE["bsd_totalamountpaid"]).Value : 0;
                    d_oe_bsd_totalamountpaid -= d_ampay;
                    i_bsd_actualgracedays -= i_outstanding_AppDtl;
                    i_bsd_totallatedays -= i_outstanding_AppDtl;
                    if (d_oe_bsd_totalamountpaid <= 0) d_oe_bsd_totalamountpaid = 0;
                    psd_statuscode = 100000000; // not paid

                    // check & get status of Unit & OE
                    if (i_phaseNum == 1)
                    {
                        if (en_OE.Contains("bsd_signedcontractdate"))
                        {
                            caseNew = true;
                            //throw new InvalidPluginExecutionException("Contract has been signed. Cannot revert Void payment " + (string)en_voidPM["bsd_name"] + "!");
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

                    if (f_check_1st == false & !caseNew)
                    {
                        sttOE = i_oeStatus;
                        sttUnit = i_Ustatus;
                    }


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
                    en_InsUp["bsd_actualgracedays"] = i_bsd_actualgracedays;
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


                } // end for each installmnet from array


            } // end  if (s_bsd_arraypsdid != "")
        }
        private void voidPayInterest(Entity en_voidPM, Entity en_app, Entity en_OE)
        {
            string s_bsd_arrayinstallmentinterest = en_app.Contains("bsd_arrayinstallmentinterest") ? (string)en_app["bsd_arrayinstallmentinterest"] : "";
            string s_bsd_arrayinterestamount = en_app.Contains("bsd_arrayinterestamount") ? (string)en_app["bsd_arrayinterestamount"] : "";
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
        }
        private void voidPayFees(Entity en_voidPM, Entity en_app, Entity en_OE)
        {
            string s_fees = en_app.Contains("bsd_arrayfeesid") ? (string)en_app["bsd_arrayfeesid"] : "";
            string s_feesAM = en_app.Contains("bsd_arrayfeesamount") ? (string)en_app["bsd_arrayfeesamount"] : "";
            decimal d_oe_bsd_totalamountpaid = en_OE.Contains("bsd_totalamountpaid") ? ((Money)en_OE["bsd_totalamountpaid"]).Value : 0;
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
                    EntityCollection ec_Ins_ES = installment.get_Ins_ES(en_OE.Id.ToString());
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
                        // ------------------------- Maintenance -----------------
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
                        // -------------------------end Maintenance -----------------

                        if (s2 != "")
                        {
                            // ------------------------- Maintenance -----------------
                            if (s2 == "1") // maintenance fees
                            {
                                d_bsd_maintenancefeepaid -= d_am2;
                                f_main = false;
                                d_oe_bsd_totalamountpaid -= d_am2;
                            }
                            //-------------------------  end Maintenance -----------------

                            // ------------------------- Management -----------------
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
                //// retreive INS es
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

                //    

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

                //// ------------------------- Maintenance -----------------
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

                // -------------------------end Maintenance -----------------

                //if (s2 != "")
                //{
                //    // ------------------------- Maintenance -----------------
                //    if (s2 == "1") // maintenance fees
                //    {
                //        d_bsd_maintenancefeepaid -= d_am2;
                //        f_main = false;
                //        d_oe_bsd_totalamountpaid -= d_am2;
                //    }
                //     -------------------------  end Maintenance -----------------

                //    // ------------------------- Management -----------------
                //    else if (s2 == "2") // management
                //    {
                //        d_bsd_managementfeepaid -= d_am2;
                //        f_mana = false;
                //    }
                //     ------------------------- end Management -----------------
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
        }
        private void voidPayMisc(Entity en_voidPM, Entity en_app, Entity en_OE)
        {
            string s_bsd_arraymicellaneousid = en_app.Contains("bsd_arraymicellaneousid") ? (string)en_app["bsd_arraymicellaneousid"] : "";
            string s_bsd_arraymicellaneousamount = en_app.Contains("bsd_arraymicellaneousamount") ? (string)en_app["bsd_arraymicellaneousamount"] : "";
            if (s_bsd_arraymicellaneousid != "")
            {
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
        public EntityCollection Get_appDtl(Guid appID)
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
            EntityCollection entc = service.RetrieveMultiple(new FetchExpression(fetchXml));
            return entc;
        }
        public EntityCollection get_appDtl_Ins(Guid appID, Guid InsID)
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
            EntityCollection entc = service.RetrieveMultiple(new FetchExpression(fetchXml));
            return entc;
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
