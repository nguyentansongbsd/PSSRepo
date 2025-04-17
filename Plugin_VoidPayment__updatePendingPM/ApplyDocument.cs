using Microsoft.Crm.Sdk.Messages;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Query;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IdentityModel.Metadata;
using System.Net;
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
            int i_bsd_transactiontype = ((OptionSetValue)en_app["bsd_transactiontype"]).Value;
            checkInput(en_app);
            if (i_bsd_transactiontype == 1)//deposit
            {
                if (!en_app.Contains("bsd_advancepaymentamount") || !en_app.Contains("bsd_quote"))
                {
                    throw new InvalidPluginExecutionException("Kindly provide the transaction information.");
                }
                voidDeposit(((EntityReference)en_app["bsd_quote"]).Id, ((Money)en_app["bsd_advancepaymentamount"]).Value);
            }
            else if (i_bsd_transactiontype == 2)//installment
            {
                if (!en_app.Contains("bsd_advancepaymentamount") || !en_app.Contains("bsd_optionentry"))
                {
                    throw new InvalidPluginExecutionException("Kindly provide the transaction information.");
                }
                voidInstallment(en_voidPM, en_app, i_bsd_transactiontype);
            }
            else if (i_bsd_transactiontype == 3)//Interest
            {
                if (!en_app.Contains("bsd_advancepaymentamount") || !en_app.Contains("bsd_optionentry"))
                {
                    throw new InvalidPluginExecutionException("Kindly provide the transaction information.");
                }
                voidInstallment(en_voidPM, en_app, i_bsd_transactiontype);
            }
            else if (i_bsd_transactiontype == 4)//Fees
            {
                if (!en_app.Contains("bsd_advancepaymentamount") || !en_app.Contains("bsd_optionentry"))
                {
                    throw new InvalidPluginExecutionException("Kindly provide the transaction information.");
                }
                voidInstallment(en_voidPM, en_app, i_bsd_transactiontype);
            }
            else if (i_bsd_transactiontype == 5)//Miscellaneous
            {
                if (!en_app.Contains("bsd_advancepaymentamount") || !en_app.Contains("bsd_optionentry"))
                {
                    throw new InvalidPluginExecutionException("Kindly provide the transaction information.");
                }
                voidInstallment(en_voidPM, en_app, i_bsd_transactiontype);
            }
            // ---------------  update advance payment list -------------------
            decimal d_tmp = en_app.Contains("bsd_actualamountspent") ? ((Money)en_app["bsd_actualamountspent"]).Value : 0;
            EntityCollection listAdvance = Get_appAdvance(en_app.Id);
            foreach (Entity item in listAdvance.Entities)
            {
                if (d_tmp == 0) break;
                // so tien can tra cho applydoc = so tien cua adv check dau tien - k can update nua - chi update cho adv nay thoi
                decimal bsd_advancepaymentapply = item.Contains("bsd_advancepaymentapply") ? ((Money)item["bsd_advancepaymentapply"]).Value : 0;
                if (d_tmp < bsd_advancepaymentapply)
                {
                    revert_Up_adv(((EntityReference)item["bsd_advancepayment"]).Id, d_tmp);
                    d_tmp = 0;
                    break;
                }
                else if (d_tmp > bsd_advancepaymentapply)
                {
                    revert_Up_adv(((EntityReference)item["bsd_advancepayment"]).Id, bsd_advancepaymentapply); // payoff
                    d_tmp -= bsd_advancepaymentapply;
                }
                else
                {
                    revert_Up_adv(((EntityReference)item["bsd_advancepayment"]).Id, bsd_advancepaymentapply); // payoff
                    d_tmp = 0;
                    break;
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
        public void voidDeposit(Guid quoteId, decimal d_amp)
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
        public void voidInstallment(Entity en_voidPM, Entity en_app, int bsd_transactiontype)
        {
            i_Ustatus = 100000003; // deposit
            i_oeStatus = 100000000;// option
            Entity en_OE = service.Retrieve(((EntityReference)en_app["bsd_optionentry"]).LogicalName, ((EntityReference)en_app["bsd_optionentry"]).Id, new ColumnSet(new string[] {
                                    "bsd_unitnumber",
                                    "bsd_totallatedays",
                                    "bsd_totalamountpaid",
                                    "bsd_signedcontractdate",
                                    "salesorderid"
                                    }));
            if (bsd_transactiontype == 2)//installment
            {
                voidPayInstallment(en_voidPM, en_app, en_OE);
            }
            else if (bsd_transactiontype == 3)//Interest
            {
                voidPayInterest(en_voidPM, en_app, en_OE);
            }
            else if (bsd_transactiontype == 4)//Fees
            {
                voidPayFees(en_voidPM, en_app, en_OE);
            }
            else if (bsd_transactiontype == 5)//Miscellaneous
            {
                voidPayMisc(en_voidPM, en_app, en_OE);
            }
        }
        private void checkPaidInstalment(int bsd_ordernumber, Guid oe)
        {
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
        private void getInstall(Guid idApply, List<Install> list)
        {
            var fetchXml = $@"<?xml version=""1.0"" encoding=""utf-16""?>
            <fetch>
              <entity name=""bsd_paymentschemedetail"">
                <attribute name=""bsd_paymentschemedetailid"" />
                <attribute name=""bsd_ordernumber"" />
                <order attribute=""bsd_ordernumber"" descending=""true"" />
                <link-entity name=""bsd_applydocumentdetail"" from=""bsd_installment"" to=""bsd_paymentschemedetailid"" alias=""app"">
                  <filter>
                    <condition attribute=""bsd_applydocument"" operator=""eq"" value=""{idApply}"" />
                  </filter>
                </link-entity>
              </entity>
            </fetch>";
            EntityCollection entc = service.RetrieveMultiple(new FetchExpression(fetchXml));
            foreach (Entity entity in entc.Entities)
            {
                Install newIns = new Install();
                newIns.idInstallment = entity.Id;
                newIns.orderNumber = (int)entity["bsd_ordernumber"];
                list.Add(newIns);
            }
        }
        private void voidPayInstallment(Entity en_voidPM, Entity en_app, Entity en_OE)
        {
            List<Install> list = new List<Install>();
            getInstall(en_app.Id, list);
            int lenght = list.Count;
            if (lenght > 0)
            {
                checkPaidInstalment(list[0].orderNumber, en_OE.Id);
                EntityCollection psdFirst = installment.GetPSD(en_OE.Id.ToString());
                EntityCollection ec_ins = get_ecIns(service, list);
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
        }
        private void voidPayFees(Entity en_voidPM, Entity en_app, Entity en_OE)
        {
            decimal d_oe_bsd_totalamountpaid = en_OE.Contains("bsd_totalamountpaid") ? ((Money)en_OE["bsd_totalamountpaid"]).Value : 0;
            EntityCollection ec_appDtl_Fees = get_app_Fees(service, en_app.Id);
            foreach (Entity item in ec_appDtl_Fees.Entities)
            {
                int bsd_feetype = item.Contains("bsd_feetype") ? ((OptionSetValue)item["bsd_feetype"]).Value : 0;
                decimal bsd_amountapply = item.Contains("bsd_amountapply") ? ((Money)item["bsd_amountapply"]).Value : 0;
                if (bsd_feetype == 100000000) // main
                {
                    Entity enInstallment = service.Retrieve(((EntityReference)item["bsd_installment"]).LogicalName, ((EntityReference)item["bsd_installment"]).Id, new ColumnSet(new string[] {
                                                    "bsd_maintenancefeesstatus", "bsd_maintenancefeepaid" }));
                    bool bsd_maintenancefeesstatus = enInstallment.Contains("bsd_maintenancefeesstatus") ? (bool)enInstallment["bsd_maintenancefeesstatus"] : false;
                    decimal bsd_maintenancefeepaid = enInstallment.Contains("bsd_maintenancefeepaid") ? ((Money)enInstallment["bsd_maintenancefeepaid"]).Value : 0;
                    bsd_maintenancefeesstatus = false;
                    bsd_maintenancefeepaid -= bsd_amountapply;
                    d_oe_bsd_totalamountpaid -= bsd_amountapply;
                    enInstallment["bsd_maintenancefeesstatus"] = bsd_maintenancefeesstatus;
                    enInstallment["bsd_maintenancefeepaid"] = new Money(bsd_maintenancefeepaid);
                    service.Update(enInstallment);
                }
                else if (bsd_feetype == 100000001) // mana
                {
                    Entity enInstallment = service.Retrieve(((EntityReference)item["bsd_installment"]).LogicalName, ((EntityReference)item["bsd_installment"]).Id, new ColumnSet(new string[] {
                                                    "bsd_managementfeesstatus", "bsd_managementfeepaid" }));
                    bool bsd_managementfeesstatus = enInstallment.Contains("bsd_managementfeesstatus") ? (bool)enInstallment["bsd_managementfeesstatus"] : false;
                    decimal bsd_managementfeepaid = enInstallment.Contains("bsd_managementfeepaid") ? ((Money)enInstallment["bsd_managementfeepaid"]).Value : 0;
                    bsd_managementfeesstatus = false;
                    bsd_managementfeepaid -= bsd_amountapply;
                    d_oe_bsd_totalamountpaid -= bsd_amountapply;
                    enInstallment["bsd_managementfeesstatus"] = bsd_managementfeesstatus;
                    enInstallment["bsd_managementfeepaid"] = new Money(bsd_managementfeepaid);
                    service.Update(enInstallment);
                }
            }
            if (ec_appDtl_Fees.Entities.Count > 0)
            {
                // OE
                Entity en_oeUPdate = new Entity(en_OE.LogicalName);
                en_oeUPdate.Id = en_OE.Id;
                en_oeUPdate["bsd_totalamountpaid"] = new Money(d_oe_bsd_totalamountpaid);
                service.Update(en_oeUPdate);
            }
        }
        private void voidPayMisc(Entity en_voidPM, Entity en_app, Entity en_OE)
        {
            EntityCollection ec_appDtl_Mis = get_app_MISS(service, en_app.Id);
            if (ec_appDtl_Mis.Entities.Count > 0)
            {
                for (int j = 0; j < ec_appDtl_Mis.Entities.Count; j++)
                {
                    decimal d_ampay = ec_appDtl_Mis.Entities[j].Contains("bsd_amountapply") ? ((Money)ec_appDtl_Mis.Entities[j]["bsd_amountapply"]).Value : 0;
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
        private EntityCollection get_ecIns(IOrganizationService crmservices, List<Install> list)
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
            for (int i = 0; i < list.Count; i++)
            {
                fetchXml += @"<value>" + list[i].idInstallment + "</value>";
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
        public EntityCollection Get_appAdvance(Guid appID)
        {
            var fetchXml = $@"<?xml version=""1.0"" encoding=""utf-16""?>
            <fetch>
              <entity name=""bsd_applydocumentremainingcoa"">
                <attribute name=""bsd_advancepayment"" />
                <attribute name=""bsd_advancepaymentapply"" />
                <filter>
                  <condition attribute=""bsd_applydocument"" operator=""eq"" value=""{appID}"" />
                </filter>
              </entity>
            </fetch>";
            EntityCollection entc = service.RetrieveMultiple(new FetchExpression(fetchXml));
            return entc;
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
        private EntityCollection get_app_Fees(IOrganizationService crmservices, Guid appID)
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
                  <condition attribute='bsd_paymenttype' operator='eq' value='100000002' />
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
    public class Install
    {
        public Guid idInstallment { get; set; }
        public int orderNumber { get; set; }
    }
}
