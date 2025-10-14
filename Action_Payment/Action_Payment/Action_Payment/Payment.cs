using Microsoft.Crm.Sdk.Messages;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Query;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.Script.Serialization;

namespace Action_Payment
{
    class Payment
    {
        IOrganizationService service = null;
        IOrganizationServiceFactory factory = null;
        IPluginExecutionContext context = null;
        IServiceProvider serviceProvider;
        ITracingService TracingSe = null;
        public Payment(IServiceProvider serviceProvider)
        {
            this.serviceProvider = serviceProvider;
            context = (IPluginExecutionContext)serviceProvider.GetService(typeof(IPluginExecutionContext));
            factory = (IOrganizationServiceFactory)serviceProvider.GetService(typeof(IOrganizationServiceFactory));
            service = factory.CreateOrganizationService(context.UserId);
            TracingSe = (ITracingService)serviceProvider.GetService(typeof(ITracingService));
        }
        public void deposit(Entity paymentEn)
        {
            Entity en_payment = new Entity(paymentEn.LogicalName);
            en_payment.Id = paymentEn.Id;
            TracingSe.Trace("vào deposit");

            if (paymentEn.Contains("bsd_reservation"))
            {

                Entity en_Resv = service.Retrieve(((EntityReference)paymentEn["bsd_reservation"]).LogicalName, ((EntityReference)paymentEn["bsd_reservation"]).Id
                    , new ColumnSet(true));


                if (((OptionSetValue)en_Resv["statuscode"]).Value == 6)
                    throw new InvalidPluginExecutionException("Reservation " + (string)en_Resv["name"] + " had been cancelled, cannot deposit!");
                if (((OptionSetValue)en_Resv["statuscode"]).Value == 3)
                    throw new InvalidPluginExecutionException("Quotation Reservation " + (string)en_Resv["name"] + " had been depositted, cannot payment!");
                decimal bsd_differentamount = paymentEn.Contains("bsd_differentamount") ? ((Money)paymentEn["bsd_differentamount"]).Value : 0;
                if (bsd_differentamount != 0)
                    throw new InvalidPluginExecutionException("Invalid deposit fee.");
                Entity en_tmp = new Entity(en_Resv.LogicalName);
                en_tmp.Id = en_Resv.Id;

                // 170316 kiem tra neu 1st installment co check vao bsd_typeofstartdate = deposit - thi generate lai pms - else generate lai
                EntityCollection encolInstallment = Get1st_Resv(en_Resv.Id.ToString());
                Entity enInstallment1st = new Entity(encolInstallment.Entities[0].LogicalName);
                enInstallment1st.Id = encolInstallment[0].Id;

                // 161012 - amount pay > deposit amount
                decimal d_amPhase1st = encolInstallment.Entities[0].Contains("bsd_amountofthisphase") ? ((Money)encolInstallment.Entities[0]["bsd_amountofthisphase"]).Value : 0;
                decimal d_depositAM = encolInstallment.Entities[0].Contains("bsd_depositamount") ? ((Money)encolInstallment.Entities[0]["bsd_depositamount"]).Value : 0;
                if (d_depositAM != 0) throw new InvalidPluginExecutionException("This Quotation Reservation has been deposited. PLease check again!");
                if (d_amPhase1st == 0)
                    throw new InvalidPluginExecutionException("Amount of Installment 1 - Reservation " + (string)en_Resv["name"] + " is null. Please check again!");
                // if amount pay large than amount of 1st - push to advance pm


                decimal pm_amountpay = paymentEn.Contains("bsd_amountpay") ? ((Money)paymentEn["bsd_amountpay"]).Value : 0;
                decimal pm_sotiendot = paymentEn.Contains("bsd_totalamountpayablephase") ? ((Money)paymentEn["bsd_totalamountpayablephase"]).Value : 0;
                DateTime d_now = RetrieveLocalTimeFromUTCTime(DateTime.Now);
                // if amount pay > 1st am phase - create advance pm
                //------------------ calc ------------------------
                if (pm_amountpay >= d_amPhase1st)
                {
                    enInstallment1st["statuscode"] = new OptionSetValue(100000001);
                    enInstallment1st["bsd_paiddate"] = d_now; // update paid date cua 1st ins = ngay pm paid
                    enInstallment1st["bsd_amountwaspaid"] = new Money(d_amPhase1st - pm_sotiendot - d_depositAM);
                    enInstallment1st["bsd_balance"] = new Money(0);
                    enInstallment1st["bsd_depositamount"] = new Money(pm_sotiendot);
                    service.Update(enInstallment1st);

                    // total amount paid of Resv
                    en_tmp["bsd_totalamountpaid"] = new Money(d_amPhase1st);

                    if (pm_amountpay > d_amPhase1st)
                    {
                        // create_AdvPM(en_Resv, (EntityReference)paymentEn["bsd_purchaser"], paymentEn, pm_amountpay- d_amPhase1st);

                        Entity en_AdvancePM = new Entity("bsd_advancepayment");
                        en_AdvancePM["bsd_name"] = "Advance Payment of " + en_Resv["name"].ToString();
                        //if (((EntityReference)paymentEn["bsd_purchaser"]).LogicalName == "account")//customertype = 1
                        //{
                        //    //en_AdvancePM["bsd_customertype"] = new OptionSetValue(1);
                        //    en_AdvancePM["bsd_account"] = ((EntityReference)paymentEn["bsd_purchaser"]);
                        //}
                        //else // contact customertype =2
                        //{
                        //    //en_AdvancePM["bsd_customertype"] = new OptionSetValue(2);
                        //    en_AdvancePM["bsd_contact"] = ((EntityReference)paymentEn["bsd_purchaser"]);
                        //}

                        en_AdvancePM["bsd_customer"] = en_Resv["customerid"];
                        en_AdvancePM["bsd_project"] = en_Resv["bsd_projectid"];
                        en_AdvancePM["bsd_payment"] = paymentEn.ToEntityReference();
                        en_AdvancePM["bsd_amount"] = new Money(pm_amountpay - d_amPhase1st);
                        en_AdvancePM["statuscode"] = new OptionSetValue(100000000); // collected
                        en_AdvancePM["bsd_paidamount"] = new Money(0);
                        en_AdvancePM["bsd_refundamount"] = new Money(0);
                        en_AdvancePM["bsd_remainingamount"] = new Money(pm_amountpay - d_amPhase1st);
                        service.Create(en_AdvancePM);
                    }
                } //if (pm_amountpay >= d_amPhase1st)
                else
                {
                    enInstallment1st["bsd_amountwaspaid"] = new Money(pm_amountpay > pm_sotiendot ? (pm_amountpay - pm_sotiendot - d_depositAM) : 0);
                    enInstallment1st["bsd_balance"] = new Money(d_amPhase1st - pm_amountpay);
                    enInstallment1st["bsd_depositamount"] = new Money(pm_amountpay > pm_sotiendot ? pm_sotiendot : pm_amountpay);
                    service.Update(enInstallment1st);
                    // total amount paid of Resv
                    en_tmp["bsd_totalamountpaid"] = new Money(pm_amountpay);
                }

                //------end calc -----------

                // ---------  update quote  & payment --------------
                // 161227 - k gan lai reservation time - lam can cu generate lai pms detail trong quote
                en_tmp["bsd_deposittime"] = paymentEn["bsd_paymentactualtime"];
                en_tmp["bsd_isdeposited"] = true;

                service.Update(en_tmp);

                // update payment
                en_payment["bsd_differentamount"] = new Money(pm_amountpay - pm_sotiendot);
                en_payment["statuscode"] = new OptionSetValue(100000000);  //paid
                en_payment["bsd_confirmeddate"] = d_now;
                en_payment["bsd_confirmperson"] = new EntityReference("systemuser", context.UserId);
                en_payment["bsd_totalamountpaidphase"] = new Money(0);
                en_payment["bsd_waiveramount"] = new Money(0);

                service.Update(en_payment);


                // re-generate all ins duedate if pass condition not check is deposited & 1st master is auto date - start date is deposited time
                if (encolInstallment.Entities[0].Contains("bsd_duedatecalculatingmethod") && ((OptionSetValue)encolInstallment.Entities[0]["bsd_duedatecalculatingmethod"]).Value == 100000001)
                {
                    bool f_bsd_typeofstartdate = encolInstallment.Entities[0].Contains("bsd_typeofstartdate") ? (bool)encolInstallment.Entities[0]["bsd_typeofstartdate"] : false;
                    // type of start date not check - ( is deposited ) not check - when deposit payment - re-generate pms detail
                    if (f_bsd_typeofstartdate == true)
                    {
                        // regenerate old - ko su dung
                        // re-generate payment scheme detail
                        //EntityCollection etnc = get_Workflow(service); // get Workflow Generate payment scheme detail for Resv
                        //foreach (Entity pro in etnc.Entities)
                        //{
                        //    ExecuteWorkflowRequest request = new ExecuteWorkflowRequest()
                        //    {
                        //        WorkflowId = pro.Id,
                        //        EntityId = ((EntityReference)paymentEn["bsd_reservation"]).Id
                        //    };
                        //    // Execute the workflow.
                        //    ExecuteWorkflowResponse response = (ExecuteWorkflowResponse)service.Execute(request);
                        //    break;
                        //}

                        // 170221 - change from call workflow to call action generate pms
                        // 170121 - call action not call workflow
                        //OrganizationRequest req = new OrganizationRequest("bsd_Action_Resv_Gene_PMS");
                        ////req["ProjectName"] = "This is a test operation for using Actions in CRM 2013 ";
                        //req["Target"] = new EntityReference(en_Resv.LogicalName, en_Resv.Id);
                        ////execute the request
                        //OrganizationResponse response = service.Execute(req);
                        //

                        // 170325 - call function re-gerate all duedate of Installment of Quote
                        //DateTime dt_date = (DateTime)paymentEn["bsd_paymentactualtime"];
                        DateTime bsd_paymentactualtime = RetrieveLocalTimeFromUTCTime((DateTime)paymentEn["bsd_paymentactualtime"]);
                        //reGenerate(service, en_Resv, dt_date, encolInstallment); //reGenerateDeposit
                        reGenerateDeposit(en_Resv, bsd_paymentactualtime);
                    }
                }
                // -- regenerate -----------

                //SetStateRequest setStateRequest1 = new SetStateRequest()
                //{
                //    EntityMoniker = en_Resv.ToEntityReference(),
                //    State = new OptionSetValue(0),
                //    Status = new OptionSetValue(100000000),
                //};
                //service.Execute(setStateRequest1);
                //Cập nhật Res -> deposit
                SetStateRequest setStateRequest = new SetStateRequest()
                {
                    EntityMoniker = en_Resv.ToEntityReference(),
                    State = new OptionSetValue(1),
                    Status = new OptionSetValue(3),
                };
                service.Execute(setStateRequest);

                // get all quote have quotation status without this quote - change status all to Close - Lost
                EntityCollection ec_resv_quotation = get_resv_Quotation(service, ((EntityReference)en_Resv["bsd_unitno"]).Id);
                if (ec_resv_quotation.Entities.Count > 0)
                {
                    foreach (Entity en_resv_quotation in ec_resv_quotation.Entities)
                    {
                        SetStateRequest setStateRequest2 = new SetStateRequest()
                        {
                            EntityMoniker = en_resv_quotation.ToEntityReference(),
                            State = new OptionSetValue(1),
                            Status = new OptionSetValue(2),
                        };
                        service.Execute(setStateRequest2);

                        CloseQuoteRequest req = new CloseQuoteRequest();
                        Entity QuoteClose = new Entity("quoteclose");
                        QuoteClose.Attributes.Add("quoteid", new EntityReference("quote", new Guid(en_resv_quotation.Id.ToString())));
                        QuoteClose.Attributes.Add("subject", "Closes the quote by Units successfully deposited from another quotation!");
                        req.QuoteClose = QuoteClose;
                        req.RequestName = "CloseQuote";
                        req.Status = new OptionSetValue(5);
                        service.Execute(req);
                    }
                }
                // ---------------- check FUL cua RESV trong FUL tuong ung cua Unit ---------------------
                /// 170811
                // kiem tra Unit trong payment co ton tai trong FUL:
                // voi status FUL =Active
                // Type FUL = OE/INS - INS FUL = INS payment ; RESV / Deposited ;
                //--> update FUL status = complete
                //--> Finance Comment = "Customers have paid the full amount of the payment. in <Confirm Payment Date>."
                //--> Cập nhật field "Follow up list" trong Option Entry = No
                // check if FUL exist Unit in this PM

                Entity Units = service.Retrieve("product", ((EntityReference)paymentEn["bsd_units"]).Id, new ColumnSet(new string[] { "name" }));

                EntityCollection ec_FUL = get_ecFUL(service, Units.Id);
                if (ec_FUL.Entities.Count > 0)
                {
                    //Update FUL
                    Entity fulEn = ec_FUL.Entities[0];
                    Entity en_FUL_up = new Entity(fulEn.LogicalName);
                    en_FUL_up.Id = fulEn.Id;
                    en_FUL_up["statuscode"] = new OptionSetValue(100000000);
                    string s_dNow = d_now.Day.ToString() + "/" + d_now.Month.ToString() + "/" + d_now.Year.ToString();
                    en_FUL_up["bsd_fincomment"] = "Customers have paid the full amount of the payment in " + s_dNow;
                    service.Update(en_FUL_up);
                    //throw new InvalidPluginExecutionException("ok");

                    //Update Quote
                    Entity en_quote_ful = new Entity(en_Resv.LogicalName);
                    en_quote_ful.Id = en_Resv.Id;

                    SetStateRequest setStateRequest2 = new SetStateRequest()
                    {
                        EntityMoniker = en_quote_ful.ToEntityReference(),
                        State = new OptionSetValue(0),
                        Status = new OptionSetValue(100000000),
                    };
                    service.Execute(setStateRequest2);

                    en_quote_ful["bsd_followuplist"] = false;
                    service.Update(en_quote_ful);

                    SetStateRequest setStateRequest3 = new SetStateRequest()
                    {
                        EntityMoniker = en_quote_ful.ToEntityReference(),
                        State = new OptionSetValue(1),
                        Status = new OptionSetValue(3),
                    };
                    service.Execute(setStateRequest3);
                }
                //------------------- end check FUL tuong ung voi RESV ---------------------------------
            }

            else throw new InvalidPluginExecutionException("Please choose Reservation for this payment!");
            //160918

        }
        public void miscellaneous(Entity paymentEn)
        {
            info_Error info = new info_Error();
            string s_bsd_arraymicellaneousid = paymentEn.Contains("bsd_arraymicellaneousid") ? (string)paymentEn["bsd_arraymicellaneousid"] : "";
            decimal pm_amountpay = paymentEn.Contains("bsd_amountpay") ? ((Money)paymentEn["bsd_amountpay"]).Value : 0;
            Entity optionentryEn = service.Retrieve("salesorder", ((EntityReference)paymentEn["bsd_optionentry"]).Id, new ColumnSet(true));
            EntityReference customerRef = (EntityReference)optionentryEn["customerid"];
            DateTime d_now = RetrieveLocalTimeFromUTCTime(DateTime.Now);
            decimal d_bsd_assignamount = paymentEn.Contains("bsd_assignamount") ? ((Money)paymentEn["bsd_assignamount"]).Value : 0;
            DateTime pm_ReceiptDate = RetrieveLocalTimeFromUTCTime((DateTime)paymentEn["bsd_paymentactualtime"]);
            //  if (!paymentEn.Contains("bsd_miscellaneous")) throw new InvalidPluginExecutionException("Please check field Miscellaneous!");
            // if (psd_statuscode != 100000001) throw new InvalidPluginExecutionException("Installment " + (string)PaymentDetailEn["bsd_name"] + " has not been paid, cannot excute this payment!");
            TracingSe.Trace("1");
            if (s_bsd_arraymicellaneousid == "") throw new InvalidPluginExecutionException("You must check at least one row on Miscellaneous sub-grid!");
            TracingSe.Trace("2");
            if (s_bsd_arraymicellaneousid != "")
            {
                TracingSe.Trace("2.1");
                string missID = paymentEn.Contains("bsd_arraymicellaneousid") ? (string)paymentEn["bsd_arraymicellaneousid"] : "";
                string missAM = paymentEn.Contains("bsd_arraymicellaneousamount") ? (string)paymentEn["bsd_arraymicellaneousamount"] : "";
                decimal pm_balance = paymentEn.Contains("bsd_balance") ? ((Money)paymentEn["bsd_balance"]).Value : 0;
                Entity Product = service.Retrieve("product", ((EntityReference)optionentryEn["bsd_unitnumber"]).Id, new ColumnSet(true));
                TracingSe.Trace("2.2");
                if (missID != "")
                {
                    TracingSe.Trace("2.3");
                    string[] arrId = missID.Split(',');
                    string[] arrMissAmount = missAM.Split(',');
                    TracingSe.Trace("2.4");
                    for (int i = 0; i < arrId.Length; i++)
                    {
                        string[] arr = arrId[i].Split('_');
                        string micellaneousid = arr[0];
                        TracingSe.Trace("2.5");
                        decimal miss = decimal.Parse(arrMissAmount[i].ToString());
                        TracingSe.Trace("miss: " + miss.ToString());
                        TracingSe.Trace("micellaneousid: " + micellaneousid.ToString());
                        Entity enMiss = getMiscellaneous(service, micellaneousid);
                        check_ins_notpaid(((EntityReference)enMiss["bsd_installment"]).Id, optionentryEn.Id);
                        decimal d_MI_paid = enMiss.Contains("bsd_paidamount") ? ((Money)enMiss["bsd_paidamount"]).Value : 0;
                        decimal d_MI_balance = enMiss.Contains("bsd_balance") ? ((Money)enMiss["bsd_balance"]).Value : 0;

                        int f_MIS_status = enMiss.Contains("statuscode") ? ((OptionSetValue)enMiss["statuscode"]).Value : 1;
                        if (f_MIS_status == 100000000) throw new InvalidPluginExecutionException("Miscellaneous " + (string)enMiss["bsd_name"] + " has been paid!");
                        TracingSe.Trace("f_MIS_status: " + f_MIS_status.ToString());
                        Entity en_MIS_up = new Entity(enMiss.LogicalName, enMiss.Id);

                        TracingSe.Trace("2.6");
                        decimal tiendu = pm_amountpay - pm_balance;
                        if (miss == d_MI_balance)
                        {
                            TracingSe.Trace("2.6.1");
                            f_MIS_status = 100000000;
                            d_MI_balance = 0;
                            d_MI_paid += miss;
                            tiendu -= miss;
                            TracingSe.Trace("2.6.1.1");
                            createTransPMMiss(paymentEn, optionentryEn, 100000003, enMiss, miss, d_now.Date, service, (string)Product["name"], 0, 0);

                        } // end d_amp == d_MI_balance
                        else if (miss < d_MI_balance)
                        {
                            TracingSe.Trace("2.6.2");
                            f_MIS_status = 1;
                            d_MI_balance -= miss;
                            d_MI_paid += miss;
                            tiendu -= miss;
                            createTransPMMiss(paymentEn, optionentryEn, 100000003, enMiss, miss, d_now.Date, service, (string)Product["name"], 0, 0);
                        } // end d_amp < d_MI_balance

                        else throw new InvalidPluginExecutionException("Input amount is larger than balance amount of Miscellaneous " + (string)enMiss["bsd_name"]);
                        // update
                        en_MIS_up["bsd_paidamount"] = new Money(d_MI_paid);
                        en_MIS_up["bsd_balance"] = new Money(d_MI_balance);
                        en_MIS_up["statuscode"] = new OptionSetValue(f_MIS_status);

                        service.Update(en_MIS_up);
                    }
                    if (d_bsd_assignamount > 0)
                    {
                        create_AdvPM(optionentryEn, customerRef, paymentEn, d_bsd_assignamount, (EntityReference)optionentryEn["bsd_project"], pm_ReceiptDate);
                    }
                }
            }
        }
        public void fees(Entity paymentEn)
        {
            info_Error info = new info_Error();
            Entity optionentryEn = service.Retrieve("salesorder", ((EntityReference)paymentEn["bsd_optionentry"]).Id, new ColumnSet(true));
            Entity Product = service.Retrieve("product", ((EntityReference)optionentryEn["bsd_unitnumber"]).Id, new ColumnSet(true));
            Entity PaymentDetailEn = new Entity("bsd_paymentschemedetail");
            if (paymentEn.Contains("bsd_paymentschemedetail"))
            {
                PaymentDetailEn = service.Retrieve("bsd_paymentschemedetail", ((EntityReference)paymentEn["bsd_paymentschemedetail"]).Id, new ColumnSet(true));
            }
            string s_bsd_arrayfees = paymentEn.Contains("bsd_arrayfees") ? (string)paymentEn["bsd_arrayfees"] : "";
            string s_bsd_arrayfeesamount = paymentEn.Contains("bsd_arrayfeesamount") ? (string)paymentEn["bsd_arrayfeesamount"] : "";
            // 170515
            // @ Han - if Unit was contain by PM havent field OP DATE -
            //if (!Product.Contains("bsd_opdate") || (DateTime)Product["bsd_opdate"] == null)
            //    throw new InvalidPluginExecutionException("Product " + (string)Product["name"] + " has not contain OP Date. Cannot Payment fees. Please check again!");
            // 170503
            // fees - duoc tra nhieu lan
            // them field fees was paid
            // check if Paid for Fees
            //if (i_duedateMethod != 100000002)
            //    throw new InvalidPluginExecutionException((string)PaymentDetailEn["bsd_name"] + " is not Estimate handover Installment. Please check again!");
            bool f_main = (PaymentDetailEn.Contains("bsd_maintenancefeesstatus")) ? (bool)PaymentDetailEn["bsd_maintenancefeesstatus"] : false;
            bool f_mana = (PaymentDetailEn.Contains("bsd_managementfeesstatus")) ? (bool)PaymentDetailEn["bsd_managementfeesstatus"] : false;
            decimal d_bsd_assignamount = paymentEn.Contains("bsd_assignamount") ? ((Money)paymentEn["bsd_assignamount"]).Value : 0;
            if (f_main == true && f_mana == true)
                throw new InvalidPluginExecutionException("Maintenance & Management fees has been Paid!");
            // check amount pay
            //  check amount pay & balance --------
            //decimal d_bsd_maintenanceamount = PaymentDetailEn.Contains("bsd_maintenanceamount") ? ((Money)PaymentDetailEn["bsd_maintenanceamount"]).Value : 0;
            //decimal d_bsd_managementamount = PaymentDetailEn.Contains("bsd_managementamount") ? ((Money)PaymentDetailEn["bsd_managementamount"]).Value : 0;

            //decimal d_bsd_maintenancefeepaid = PaymentDetailEn.Contains("bsd_maintenancefeepaid") ? ((Money)PaymentDetailEn["bsd_maintenancefeepaid"]).Value : 0;
            //decimal d_bsd_managementfeepaid = PaymentDetailEn.Contains("bsd_managementfeepaid") ? ((Money)PaymentDetailEn["bsd_managementfeepaid"]).Value : 0;

            //decimal d_bsd_maintenancefeewaiver = PaymentDetailEn.Contains("bsd_maintenancefeewaiver") ? ((Money)PaymentDetailEn["bsd_maintenancefeewaiver"]).Value : 0;
            //decimal d_bsd_managementfeewaiver = PaymentDetailEn.Contains("bsd_managementfeewaiver") ? ((Money)PaymentDetailEn["bsd_managementfeewaiver"]).Value : 0;

            //decimal d_mainBL = d_bsd_maintenanceamount - d_bsd_maintenancefeepaid - d_bsd_maintenancefeewaiver;
            //decimal d_manaBL = d_bsd_managementamount - d_bsd_managementfeepaid - d_bsd_managementfeewaiver;
            //

            //Entity psdTmp = new Entity("bsd_paymentschemedetail");
            //psdTmp.Id = ((EntityReference)paymentEn["bsd_paymentschemedetail"]).Id;

            if (s_bsd_arrayfees == "") throw new InvalidPluginExecutionException("You must check at least one row on Fees sub-grid!");

            if (s_bsd_arrayfees != "")
            {
                /*string[] s_feeAM = s_bsd_arrayfeesamount.Split(',');
                string s1 = s_bsd_arrayfees.Substring(0, 1);
                decimal d_am1 = decimal.Parse(s_feeAM[0]);

                string s2 = "";
                decimal d_am2 = 0;
                if (s_bsd_arrayfees.Length > 1)
                {
                    s2 = s_bsd_arrayfees.Substring(2, 1);
                    d_am2 = decimal.Parse(s_feeAM[1]);
                }
                // quy dinh khi luu du lieu subgrid - 1: maintenance fee; 2: management fee
                // ------------------------- Maintenance -----------------
                if (s1 == "1") // maintenance fees
                {
                    if (f_main == true) throw new InvalidPluginExecutionException("Maintenance fees had been paid. Please check again!");
                    if (d_am1 < d_mainBL)
                    {
                        d_bsd_maintenancefeepaid += d_am1;
                        f_main = false;
                        d_oe_bsd_totalamountpaid += d_am1;

                        createTransPM(paymentEn, optionentryEn, 100000002, PaymentDetailEn.Id, d_am1, d_now.Date, service, (string)Product["name"], 1, 0, 0);

                    }
                    else if (d_am1 == d_mainBL)
                    {
                        d_bsd_maintenancefeepaid += d_am1;
                        f_main = true;
                        d_oe_bsd_totalamountpaid += d_am1;
                        createTransPM(paymentEn, optionentryEn, 100000002, PaymentDetailEn.Id, d_am1, d_now.Date, service, (string)Product["name"], 1, 0, 0);

                    }
                    else if (d_am1 > d_mainBL) throw new InvalidPluginExecutionException("Amount pay larger than balance of Maintenance fees amount. Please check again!");

                }
                // ------------------------- Maintenance -----------------

                // ------------------------- Management -----------------
                else if (s1 == "2") // management
                {
                    if (f_mana == true) throw new InvalidPluginExecutionException("Management fees had been paid. Please check again!");
                    if (d_am1 < d_manaBL)
                    {
                        d_bsd_managementfeepaid += d_am1;
                        f_mana = false;
                        createTransPM(paymentEn, optionentryEn, 100000002, PaymentDetailEn.Id, d_am1, d_now.Date, service, (string)Product["name"], 2, 0, 0);

                    }
                    else if (d_am1 == d_manaBL)
                    {
                        d_bsd_managementfeepaid += d_am1;
                        f_mana = true;
                        createTransPM(paymentEn, optionentryEn, 100000002, PaymentDetailEn.Id, d_am1, d_now.Date, service, (string)Product["name"], 2, 0, 0);

                    }
                    else if (d_am1 > d_manaBL) throw new InvalidPluginExecutionException("Amount pay larger than balance of Management fees amount. Please check again!");

                }
                // ------------------------- Management -----------------

                //  ------------------- if s2 !="" ----------------
                if (s2 != "")
                {
                    // ------------------------- Maintenance -----------------
                    if (s2 == "1") // maintenance fees
                    {
                        if (f_main == true) throw new InvalidPluginExecutionException("Maintenance fees had been paid. Please check again!");
                        if (d_am2 < d_mainBL)
                        {
                            d_bsd_maintenancefeepaid += d_am2;
                            f_main = false;
                            d_oe_bsd_totalamountpaid += d_am2;
                            createTransPM(paymentEn, optionentryEn, 100000002, PaymentDetailEn.Id, d_am2, d_now.Date, service, (string)Product["name"], 1, 0, 0);

                        }
                        else if (d_am2 == d_mainBL)
                        {
                            d_bsd_maintenancefeepaid += d_am2;
                            f_main = true;
                            d_oe_bsd_totalamountpaid += d_am2;
                            createTransPM(paymentEn, optionentryEn, 100000002, PaymentDetailEn.Id, d_am2, d_now.Date, service, (string)Product["name"], 1, 0, 0);

                        }
                        else if (d_am2 > d_mainBL) throw new InvalidPluginExecutionException("Amount pay larger than balance of Maintenance fees amount. Please check again!");


                    }
                    // ------------------------- Maintenance -----------------

                    // ------------------------- Management -----------------
                    else if (s2 == "2") // management
                    {
                        if (f_mana == true) throw new InvalidPluginExecutionException("Management fees had been paid. Please check again!");
                        if (d_am2 < d_manaBL)
                        {
                            d_bsd_managementfeepaid += d_am2;
                            f_mana = false;
                            createTransPM(paymentEn, optionentryEn, 100000002, PaymentDetailEn.Id, d_am2, d_now.Date, service, (string)Product["name"], 2, 0, 0);
                        }
                        else if (d_am2 == d_manaBL)
                        {
                            d_bsd_managementfeepaid += d_am2;
                            f_mana = true;
                            createTransPM(paymentEn, optionentryEn, 100000002, PaymentDetailEn.Id, d_am2, d_now.Date, service, (string)Product["name"], 2, 0, 0);

                        }
                        else if (d_am2 > d_manaBL) throw new InvalidPluginExecutionException("Amount pay larger than balance of Management fees amount. Please check again!");


                    }
                    // ------------------------- Management -----------------

                }
                // ---------------------------------------------------------

                // ------ update INS & OE -------------------------
                Entity en_INS_update = new Entity(PaymentDetailEn.LogicalName);
                en_INS_update.Id = PaymentDetailEn.Id;
                en_INS_update["bsd_maintenancefeesstatus"] = f_main;
                en_INS_update["bsd_managementfeesstatus"] = f_mana;
                en_INS_update["bsd_maintenancefeepaid"] = new Money(d_bsd_maintenancefeepaid);
                en_INS_update["bsd_managementfeepaid"] = new Money(d_bsd_managementfeepaid);
                service.Update(en_INS_update);

                // OE
                Entity en_oeUPdate = new Entity(optionentryEn.LogicalName);
                en_oeUPdate.Id = optionentryEn.Id;
                en_oeUPdate["bsd_totalamountpaid"] = new Money(d_oe_bsd_totalamountpaid);
                service.Update(en_oeUPdate);

                //
                */
                //------------------------------FEES Hồ Code 20180811------------------------
                string s_fees = paymentEn.Contains("bsd_arrayfees") ? (string)paymentEn["bsd_arrayfees"] : "";
                string s_feesAM = paymentEn.Contains("bsd_arrayfeesamount") ? (string)paymentEn["bsd_arrayfeesamount"] : "";
                decimal d_oe_bsd_totalamountpaid = optionentryEn.Contains("bsd_totalamountpaid") ? ((Money)optionentryEn["bsd_totalamountpaid"]).Value : 0;
                DateTime d_now = RetrieveLocalTimeFromUTCTime(DateTime.Now);
                if (s_fees != "")
                {
                    TracingSe.Trace("5");
                    string[] arrId = s_fees.Split(',');
                    string[] arrFeeAmount = s_feesAM.Split(',');
                    for (int i = 0; i < arrId.Length; i++)
                    {
                        string[] arr = arrId[i].Split('_');
                        string installmentid = arr[0];
                        string type1 = arr[1];

                        decimal fee = decimal.Parse(arrFeeAmount[i].ToString());
                        TracingSe.Trace("6.6");
                        Entity enInstallment = getInstallment(service, installmentid);
                        TracingSe.Trace("enInstallment: " + enInstallment.Id.ToString());
                        bool f_mains = (enInstallment.Contains("bsd_maintenancefeesstatus")) ? (bool)enInstallment["bsd_maintenancefeesstatus"] : false;
                        bool f_manas = (enInstallment.Contains("bsd_managementfeesstatus")) ? (bool)enInstallment["bsd_managementfeesstatus"] : false;

                        decimal bsd_maintenanceamount = enInstallment.Contains("bsd_maintenanceamount") ? ((Money)enInstallment["bsd_maintenanceamount"]).Value : 0;
                        decimal bsd_managementamount = enInstallment.Contains("bsd_managementamount") ? ((Money)enInstallment["bsd_managementamount"]).Value : 0;

                        decimal bsd_maintenancefeepaid = enInstallment.Contains("bsd_maintenancefeepaid") ? ((Money)enInstallment["bsd_maintenancefeepaid"]).Value : 0;
                        decimal bsd_managementfeepaid = enInstallment.Contains("bsd_managementfeepaid") ? ((Money)enInstallment["bsd_managementfeepaid"]).Value : 0;

                        decimal bsd_maintenancefeewaiver = enInstallment.Contains("bsd_maintenancefeewaiver") ? ((Money)enInstallment["bsd_maintenancefeewaiver"]).Value : 0;
                        decimal bsd_managementfeewaiver = enInstallment.Contains("bsd_managementfeewaiver") ? ((Money)enInstallment["bsd_managementfeewaiver"]).Value : 0;

                        decimal mainBL = bsd_maintenanceamount - bsd_maintenancefeepaid - bsd_maintenancefeewaiver;
                        decimal manaBL = bsd_managementamount - bsd_managementfeepaid - bsd_managementfeewaiver;
                        Entity en_INS_update = new Entity(enInstallment.LogicalName);
                        en_INS_update.Id = enInstallment.Id;
                        TracingSe.Trace("en_INS_update: " + en_INS_update.Id.ToString());
                        switch (type1)
                        {
                            case "main":
                                if (f_mains == true) throw new InvalidPluginExecutionException("Maintenance fees had been paid. Please check again!");
                                if (fee < mainBL)
                                {
                                    bsd_maintenancefeepaid += fee;
                                    f_mains = false;
                                    d_oe_bsd_totalamountpaid += fee;

                                }
                                else if (fee == mainBL)
                                {
                                    bsd_maintenancefeepaid += fee;
                                    f_mains = true;
                                    d_oe_bsd_totalamountpaid += fee;

                                }
                                else if (fee > mainBL) throw new InvalidPluginExecutionException("Amount pay larger than balance of Maintenance fees amount. Please check again!");
                                createTransPM(paymentEn, optionentryEn, 100000002, enInstallment.Id, fee, d_now.Date, service, (string)Product["name"], type1, 0, 0);
                                en_INS_update["bsd_maintenancefeesstatus"] = f_mains;
                                en_INS_update["bsd_maintenancefeepaid"] = new Money(bsd_maintenancefeepaid);
                                break;
                            case "mana":
                                if (f_manas == true) throw new InvalidPluginExecutionException("Management fees had been paid. Please check again!");
                                if (fee < manaBL)
                                {
                                    bsd_managementfeepaid += fee;
                                    f_manas = false;
                                }
                                else if (fee == manaBL)
                                {
                                    bsd_managementfeepaid += fee;
                                    f_manas = true;
                                }
                                else if (fee > manaBL) throw new InvalidPluginExecutionException("Amount pay larger than balance of Management fees amount. Please check again!");
                                createTransPM(paymentEn, optionentryEn, 100000002, enInstallment.Id, fee, d_now.Date, service, (string)Product["name"], type1, 0, 0);
                                en_INS_update["bsd_managementfeesstatus"] = f_manas;
                                en_INS_update["bsd_managementfeepaid"] = new Money(bsd_managementfeepaid);
                                break;
                        }
                        service.Update(en_INS_update);

                    }
                }

                //----------------------------FEES Hồ Code 20180811
                //throw new InvalidPluginExecutionException("ADV case 2");
                EntityReference customerRef = (EntityReference)optionentryEn["customerid"];
                if (d_bsd_assignamount > 0)
                {
                    DateTime pm_ReceiptDate = d_now;
                    if (!paymentEn.Contains("bsd_paymentactualtime"))
                    {
                        pm_ReceiptDate = RetrieveLocalTimeFromUTCTime((DateTime)paymentEn["bsd_paymentactualtime"]);
                    }
                    create_AdvPM(optionentryEn, customerRef, paymentEn, d_bsd_assignamount, (EntityReference)optionentryEn["bsd_project"], pm_ReceiptDate);
                }


            }
        }
        public void intallment(Entity paymentEn, ref decimal pm_balancemaster, ref decimal d_bsd_assignamountmaster, ref decimal pm_differentamountmaster)
        {
            info_Error info = new info_Error();
            info.index = 1;
            TracingSe.Trace(String.Format("#.Trace function intallment: "));
            DateTime d_now = RetrieveLocalTimeFromUTCTime(DateTime.Now);
            decimal d_inter = 0;
            int i_lateday = 0;
            DateTime i_intereststartdate = d_now;

            #region ---#1. Get dữ liệu [bsd_payment] ---
            decimal pm_balance = paymentEn.Contains("bsd_balance") ? ((Money)paymentEn["bsd_balance"]).Value : 0;
            decimal pm_amountpay = paymentEn.Contains("bsd_amountpay") ? ((Money)paymentEn["bsd_amountpay"]).Value : 0;
            bool f_bsd_latepayment = paymentEn.Contains("bsd_latepayment") ? (bool)paymentEn["bsd_latepayment"] : false;
            DateTime pm_ReceiptDate = paymentEn.Contains("bsd_paymentactualtime") ? RetrieveLocalTimeFromUTCTime((DateTime)paymentEn["bsd_paymentactualtime"]) : d_now;
            decimal d_bsd_totalapplyamount = paymentEn.Contains("bsd_totalapplyamount") ? ((Money)paymentEn["bsd_totalapplyamount"]).Value : 0;
            d_bsd_assignamountmaster = paymentEn.Contains("bsd_assignamount") ? ((Money)paymentEn["bsd_assignamount"]).Value : 0;
            string s_bsd_arraypsdid = paymentEn.Contains("bsd_arraypsdid") ? (string)paymentEn["bsd_arraypsdid"] : "";
            string s_bsd_arrayamountpay = paymentEn.Contains("bsd_arrayamountpay") ? (string)paymentEn["bsd_arrayamountpay"] : "";
            string s_bsd_arrayinstallmentinterest = paymentEn.Contains("bsd_arrayinstallmentinterest") ? (string)paymentEn["bsd_arrayinstallmentinterest"] : "";
            string s_bsd_arrayinterestamount = paymentEn.Contains("bsd_arrayinterestamount") ? (string)paymentEn["bsd_arrayinterestamount"] : "";
            string s_bsd_arrayfees = paymentEn.Contains("bsd_arrayfees") ? (string)paymentEn["bsd_arrayfees"] : "";
            string s_bsd_arrayfeesamount = paymentEn.Contains("bsd_arrayfeesamount") ? (string)paymentEn["bsd_arrayfeesamount"] : "";
            string s_bsd_arraymicellaneousid = paymentEn.Contains("bsd_arraymicellaneousid") ? (string)paymentEn["bsd_arraymicellaneousid"] : "";
            string s_bsd_arraymicellaneousamount = paymentEn.Contains("bsd_arraymicellaneousamount") ? (string)paymentEn["bsd_arraymicellaneousamount"] : "";
            #endregion

            #region ---#2. Get dữ liệu [bsd_paymentschemedetail] ---
            Entity PaymentDetailEn = service.Retrieve("bsd_paymentschemedetail", ((EntityReference)paymentEn["bsd_paymentschemedetail"]).Id, new ColumnSet(true));
            int phaseNum = PaymentDetailEn.Contains("bsd_ordernumber") ? (int)PaymentDetailEn["bsd_ordernumber"] : 0;
            decimal psd_amountPhase = PaymentDetailEn.Contains("bsd_amountofthisphase") ? ((Money)PaymentDetailEn["bsd_amountofthisphase"]).Value : 0;
            decimal psd_amountPaid = PaymentDetailEn.Contains("bsd_amountwaspaid") ? ((Money)PaymentDetailEn["bsd_amountwaspaid"]).Value : 0;
            decimal psd_deposit = PaymentDetailEn.Contains("bsd_depositamount") ? ((Money)PaymentDetailEn["bsd_depositamount"]).Value : 0;
            decimal psd_waiverIns = PaymentDetailEn.Contains("bsd_waiverinstallment") ? ((Money)PaymentDetailEn["bsd_waiverinstallment"]).Value : 0;
            int psd_statuscode = PaymentDetailEn.Contains("statuscode") ? ((OptionSetValue)PaymentDetailEn["statuscode"]).Value : 100000000;
            decimal pm_differentamount = paymentEn.Contains("bsd_differentamount") ? ((Money)paymentEn["bsd_differentamount"]).Value : 0;
            decimal psd_bsd_balance = PaymentDetailEn.Contains("bsd_balance") ? ((Money)PaymentDetailEn["bsd_balance"]).Value : 0;
            decimal psd_bsd_interestchargeamount = PaymentDetailEn.Contains("bsd_interestchargeamount") ? ((Money)PaymentDetailEn["bsd_interestchargeamount"]).Value : 0;
            int psd_actualgracedays = PaymentDetailEn.Contains("bsd_actualgracedays") ? (int)PaymentDetailEn["bsd_actualgracedays"] : 0;
            int i_bsd_previousdelays = PaymentDetailEn.Contains("bsd_previousdelays") ? (int)PaymentDetailEn["bsd_previousdelays"] : 0;
            decimal psd_bsd_interestwaspaid = PaymentDetailEn.Contains("bsd_interestwaspaid") ? ((Money)PaymentDetailEn["bsd_interestwaspaid"]).Value : 0;
            int psd_bsd_interestchargestatus = PaymentDetailEn.Contains("bsd_interestchargestatus") ? ((OptionSetValue)PaymentDetailEn["bsd_interestchargestatus"]).Value : 0;
            #endregion

            #region --- Kiểm tra duedate in Installment : Task jira CLV-1445 ---
            if (PaymentDetailEn != null)
            {
                var isCheck = new bsd_paymentschemedetail().checkValue_Field(PaymentDetailEn, "bsd_duedate");
                if (isCheck.count != 0)
                {
                    info.createMessageNew("The Installment you are paying has no due date.");
                    TracingSe.Trace("Please update due date before confirming payment!");
                    throw new InvalidPluginExecutionException("Please update due date before confirming payment!");
                }
                else if (!isCheck.result)
                {
                    info.createMessageNew("The Installment you are paying has no due date.");
                    TracingSe.Trace("Please update due date before confirming payment!");
                    info.count = -1000;
                    throw new InvalidPluginExecutionException("Please update due date before confirming payment!");
                }
            }
            #endregion

            Entity optionentryEn = service.Retrieve("salesorder", ((EntityReference)paymentEn["bsd_optionentry"]).Id, new ColumnSet(true));
            check_ins_notpaid(((EntityReference)paymentEn["bsd_paymentschemedetail"]).Id, optionentryEn.Id);
            decimal d_oe_bsd_totalamountpaid = optionentryEn.Contains("bsd_totalamountpaid") ? ((Money)optionentryEn["bsd_totalamountpaid"]).Value : 0;
            EntityReference customerRef = (EntityReference)optionentryEn["customerid"];

            Entity Product = service.Retrieve("product", ((EntityReference)optionentryEn["bsd_unitnumber"]).Id, new ColumnSet(true));

            if (phaseNum == 1)
                // 170314 thay the waiver amount = waiver installment
                // pm_balance = psd_amountPhase - psd_amountPaid - psd_deposit - psd_waiver;// psd_balance - psd_deposit
                pm_balance = psd_amountPhase - psd_amountPaid - psd_deposit - psd_waiverIns;// psd_balance - psd_deposit
            else
            {
                pm_balance = psd_amountPhase - psd_amountPaid - psd_waiverIns; // = psd_balance
            }
            pm_differentamount = pm_amountpay - pm_balance;
            pm_balancemaster = pm_balance;
            pm_differentamountmaster = pm_differentamount;
            if (pm_differentamount > 0) d_bsd_assignamountmaster = pm_differentamount - d_bsd_totalapplyamount;
            if (d_bsd_assignamountmaster < 0)
            {
                ;
                throw new InvalidPluginExecutionException("The amount you pay exceeds the amount paid. Please choose another Advance Payment or other Payment Phase!");
            }
            if (psd_statuscode == 100000001)
                throw new InvalidPluginExecutionException((string)PaymentDetailEn["bsd_name"] + " has been Paid!");
            if (pm_differentamount <= 0 && f_bsd_latepayment == true)
                throw new InvalidPluginExecutionException("The difference amount is " + (pm_differentamount == 0 ? " equal 0" : " less than 0") + ". Cannot payment for Intesrest charge amount. PLease uncheck field 'Late payment'!");
            TracingSe.Trace("6.9");
            decimal tiendu = 0;
            // --------------- difference AM = 0 -------------------
            if (pm_amountpay == pm_balance)
            {
                info.index = 5;
                TracingSe.Trace(String.Format("- Case: pm_amountpay == pm_balance"));
                #region --- pm_amountpay == pm_balance ---
                psd_statuscode = 100000001;  // paid
                psd_amountPaid += pm_balance;
                // deposit payment had updated total AM of Quote - already had deposit amount
                // update installment & OE
                d_oe_bsd_totalamountpaid += pm_balance;
                psd_bsd_balance = 0;
                // interst charge amount for this INS

                TracingSe.Trace(String.Format("+ CheckInterestCharge"));
                CheckInterestCharge(service, ref d_inter, ref i_lateday, PaymentDetailEn, paymentEn, pm_amountpay, optionentryEn, ref i_intereststartdate);
                decimal d_sumTmp_IC_amount = psd_bsd_interestchargeamount + Convert.ToDecimal(d_inter); // Intest charge amount moi + IC san co trong INS

                //>>> Trace Test ===================================================================================
                TracingSe.Trace(String.Format(string.Format("+ d_sumTmp_IC_amount: {0}", d_sumTmp_IC_amount)));
                //var result = CheckInterestCharge_New(service, ref d_inter, ref i_lateday, PaymentDetailEn, paymentEn, pm_amountpay, optionentryEn, ref i_intereststartdate);
                //TracingSe.Trace(String.Format(string.Format("####. result function CheckInterestCharge_New")));
                //TracingSe.Trace(result.message);
                //==================================================================================================


                if (d_sumTmp_IC_amount < 0) d_sumTmp_IC_amount = 0;
                // check outstanding day
                if (i_lateday < psd_actualgracedays) i_lateday = psd_actualgracedays;
                i_bsd_previousdelays = i_lateday;

                TracingSe.Trace(String.Format(string.Format("#. Begin update PaymentDetailEn")));
                Entity en_INS_up = new Entity(PaymentDetailEn.LogicalName);
                en_INS_up.Id = PaymentDetailEn.Id;

                //en_INS_up["bsd_amountwaspaid"] = new Money(psd_amountPaid + psd_deposit);
                en_INS_up["bsd_amountwaspaid"] = new Money(psd_amountPaid);

                en_INS_up["bsd_balance"] = new Money(psd_bsd_balance);
                //en_INS_up["bsd_balance"] = new Money(psd_amountPhase - (psd_amountPaid + psd_deposit) - psd_waiverIns);
                //en_INS_up["bsd_balance"] = new Money(psd_amountPhase - psd_amountPaid - psd_waiverIns);
                en_INS_up["statuscode"] = new OptionSetValue(psd_statuscode);
                TracingSe.Trace("6.9.5");
                // Han_28072018: Khong can payment sts = paid moi update
                //if (psd_statuscode == 100000001)
                en_INS_up["bsd_paiddate"] = pm_ReceiptDate; //  d_now;  // update ngay paid cua INS

                en_INS_up["bsd_actualgracedays"] = i_lateday;
                en_INS_up["bsd_previousdelays"] = i_lateday;
                en_INS_up["bsd_interestchargeamount"] = new Money(d_sumTmp_IC_amount);
                en_INS_up["bsd_interestchargestatus"] = new OptionSetValue(100000000); // not paid
                TracingSe.Trace("6.9.6");
                //Han_28072018: Update Interest Start Date khi thanh toan
                DateTime psd_intereststartdate = RetrieveLocalTimeFromUTCTime(i_intereststartdate);
                en_INS_up["bsd_intereststartdate"] = psd_intereststartdate;

                TracingSe.Trace(String.Format(string.Format("saving ...")));
                service.Update(en_INS_up);
                TracingSe.Trace(String.Format(string.Format("Saved. Done")));
                #endregion
            }
            if (pm_amountpay < pm_balance)
            {
                info.index = 6;
                TracingSe.Trace(String.Format("- Case: pm_amountpay < pm_balance"));
                #region --- pm_amountpay < pm_balance ---
                TracingSe.Trace("6.10");
                psd_statuscode = 100000000;  // not paid
                psd_amountPaid += pm_amountpay;

                d_oe_bsd_totalamountpaid += pm_amountpay; // deposit payment had updated total AM of Quote - already had deposit amount
                                                          // update installment & OE
                                                          //psd_bsd_balance -= (pm_amountpay+ psd_deposit);
                psd_bsd_balance -= (pm_amountpay);
                TracingSe.Trace("6.10.1");
                #region -------------- interest charge amount for this payment & INS -----------------
                CheckInterestCharge(service, ref d_inter, ref i_lateday, PaymentDetailEn, paymentEn, pm_amountpay, optionentryEn, ref i_intereststartdate);
                decimal d_sumTmp_IC_amount = psd_bsd_interestchargeamount + Convert.ToDecimal(d_inter); // Intest charge amount moi + IC san co trong INS
                if (d_sumTmp_IC_amount < 0) d_sumTmp_IC_amount = 0;
                // check outstanding day
                if (i_lateday < psd_actualgracedays) i_lateday = psd_actualgracedays;
                i_bsd_previousdelays = i_lateday;
                TracingSe.Trace("6.10.2");

                Entity en_INS_up = new Entity(PaymentDetailEn.LogicalName);
                en_INS_up.Id = PaymentDetailEn.Id;

                //en_INS_up["bsd_amountwaspaid"] = new Money(psd_amountPaid+ psd_deposit);
                en_INS_up["bsd_amountwaspaid"] = new Money(psd_amountPaid);
                en_INS_up["bsd_balance"] = new Money(psd_bsd_balance);
                en_INS_up["statuscode"] = new OptionSetValue(psd_statuscode);
                if (psd_statuscode == 100000001)
                    en_INS_up["bsd_paiddate"] = pm_ReceiptDate; // d_now;  // update ngay paid cua INS
                en_INS_up["bsd_actualgracedays"] = i_lateday;
                en_INS_up["bsd_previousdelays"] = i_lateday;
                en_INS_up["bsd_interestchargeamount"] = new Money(d_sumTmp_IC_amount);
                en_INS_up["bsd_interestchargestatus"] = new OptionSetValue(100000000); // not paid
                TracingSe.Trace("6.10.3");
                //Han_28072018: Update Interest Start Date khi thanh toan
                DateTime psd_intereststartdate = RetrieveLocalTimeFromUTCTime(i_intereststartdate);
                en_INS_up["bsd_intereststartdate"] = psd_intereststartdate;
                TracingSe.Trace("6.10.4");
                service.Update(en_INS_up);
                TracingSe.Trace("6.10.5");
                #endregion
                #endregion
            }
            if (pm_amountpay > pm_balance)
            {
                info.index = 7;
                TracingSe.Trace(String.Format("- Case: pm_amountpay > pm_balance"));
                #region --- pm_amountpay > pm_balance ---
                TracingSe.Trace("pm_amountpay > pm_balance");
                TracingSe.Trace("7");
                tiendu = pm_amountpay - pm_balance;
                psd_statuscode = 100000001;  //  paid
                psd_amountPaid += pm_balance;
                TracingSe.Trace("7.1");
                d_oe_bsd_totalamountpaid += pm_balance; // deposit payment had updated total AM of Quote - already had deposit amount
                                                        // update installment & OE
                psd_bsd_balance = 0;
                TracingSe.Trace("7.2");
                #region -------------- interest charge amount for this payment & INS -----------------
                TracingSe.Trace("CheckInterestCharge");
                TracingSe.Trace("pm_balance: " + pm_balance.ToString());
                CheckInterestCharge(service, ref d_inter, ref i_lateday, PaymentDetailEn, paymentEn, pm_balance, optionentryEn, ref i_intereststartdate);
                decimal d_sumTmp_IC_amount = psd_bsd_interestchargeamount + Convert.ToDecimal(d_inter); // Intest charge amount moi + IC san co trong INS
                TracingSe.Trace("8");
                if (d_sumTmp_IC_amount < 0) d_sumTmp_IC_amount = 0;
                // check outstanding day
                if (i_lateday < psd_actualgracedays) i_lateday = psd_actualgracedays;
                i_bsd_previousdelays = i_lateday;
                Entity en_INS_up = new Entity(PaymentDetailEn.LogicalName);
                en_INS_up.Id = PaymentDetailEn.Id;
                TracingSe.Trace("9");
                en_INS_up["bsd_interestchargeamount"] = new Money(d_sumTmp_IC_amount);
                //en_INS_up["bsd_amountwaspaid"] = new Money(psd_amountPaid+ psd_deposit);
                en_INS_up["bsd_amountwaspaid"] = new Money(psd_amountPaid);
                en_INS_up["bsd_balance"] = new Money(psd_bsd_balance);
                en_INS_up["statuscode"] = new OptionSetValue(psd_statuscode);
                if (psd_statuscode == 100000001)
                    en_INS_up["bsd_paiddate"] = pm_ReceiptDate; // d_now;  // update ngay paid cua INS
                en_INS_up["bsd_actualgracedays"] = i_lateday;
                en_INS_up["bsd_previousdelays"] = i_lateday;

                en_INS_up["bsd_interestchargestatus"] = new OptionSetValue(100000000); // not paid
                TracingSe.Trace("10");
                //Han_28072018: Update Interest Start Date khi thanh toan
                DateTime psd_intereststartdate = RetrieveLocalTimeFromUTCTime(i_intereststartdate);
                en_INS_up["bsd_intereststartdate"] = psd_intereststartdate;

                service.Update(en_INS_up);

                #endregion -------------------
                TracingSe.Trace("11");
                // difference amount > 0
                #region ---------------------- Late payment & Transaction AM  -------------------------
                //---------------------------------------------------------------------------------------------------------------------------------------------

                if (tiendu > 0)
                {
                    #region ----- Late pm = true -----------

                    if (f_bsd_latepayment == true)
                    {
                        // tiendu = interest charge
                        if (tiendu == d_inter)
                        {
                            psd_bsd_interestwaspaid += tiendu;
                            if (psd_bsd_interestwaspaid == d_sumTmp_IC_amount)// sudung field d_sumTmp_IC_amount de lay interst charge amount moi nhat sau khi tinh interest charge cho payment lan nay
                                psd_bsd_interestchargestatus = 100000001; // paid
                            else psd_bsd_interestchargestatus = 100000000; // not paid
                            up_InterestAM_Ins(service, PaymentDetailEn, psd_bsd_interestwaspaid, psd_bsd_interestchargestatus, d_now);
                            tiendu -= d_inter;
                        }
                        else if (tiendu < d_inter)
                        {
                            psd_bsd_interestwaspaid += tiendu;
                            psd_bsd_interestchargestatus = 100000000; // not paid

                            up_InterestAM_Ins(service, PaymentDetailEn, psd_bsd_interestwaspaid, psd_bsd_interestchargestatus, d_now);
                            tiendu = 0;
                        }
                        else
                        {
                            tiendu -= d_inter;
                            // update installment
                            psd_bsd_interestwaspaid += d_inter;

                            if (psd_bsd_interestwaspaid == d_sumTmp_IC_amount)
                            {
                                psd_bsd_interestchargestatus = 100000001;// paid
                            }
                            else psd_bsd_interestchargestatus = 100000000;// not paid
                            up_InterestAM_Ins(service, PaymentDetailEn, psd_bsd_interestwaspaid, psd_bsd_interestchargestatus, d_now);

                            #region ------- transaction pm ------------
                            if (s_bsd_arrayamountpay != "" || s_bsd_arrayinstallmentinterest != "" || s_bsd_arrayfees != "" || s_bsd_arraymicellaneousid != "")
                            {
                                if (tiendu < d_bsd_totalapplyamount)
                                    throw new InvalidPluginExecutionException("The remaining amount is not enough to pay for transaction payments have been selected!");
                                else
                                    //Han-23.03.2018: Doi d_now = pm_ReceiptDate
                                    transactionPM_INS(service, s_bsd_arraypsdid, s_bsd_arrayamountpay, s_bsd_arrayinstallmentinterest, s_bsd_arrayinterestamount,
                                    s_bsd_arrayfees, s_bsd_arrayfeesamount, tiendu, ref d_oe_bsd_totalamountpaid, paymentEn, optionentryEn, Product, pm_ReceiptDate, customerRef,
                                     s_bsd_arraymicellaneousid, s_bsd_arraymicellaneousamount);
                            }
                            else
                            {
                                //throw new InvalidPluginExecutionException("ADV case 4");
                                //create_AdvPM(optionentryEn, customerRef, paymentEn, tiendu, (EntityReference)optionentryEn["bsd_project"], pm_ReceiptDate);
                                if (d_bsd_assignamountmaster > 0)
                                {
                                    create_AdvPM(optionentryEn, customerRef, paymentEn, d_bsd_assignamountmaster, (EntityReference)optionentryEn["bsd_project"], pm_ReceiptDate);
                                }
                                else if (tiendu > 0)
                                {
                                    create_AdvPM(optionentryEn, customerRef, paymentEn, tiendu, (EntityReference)optionentryEn["bsd_project"], pm_ReceiptDate);
                                }
                            }
                            #endregion --------------------------------

                        } // end else ( tiendu > d_inter ) -- late pm = true

                    } // end if f_bsd_latepayment =  true
                    #endregion-----------
                    TracingSe.Trace("12");
                    #region ----- Late pm = false -----------
                    if (f_bsd_latepayment == false)
                    {
                        //thanh---------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------

                        TracingSe.Trace("13");
                        if (s_bsd_arrayamountpay != "" || s_bsd_arrayinstallmentinterest != "" || s_bsd_arrayfees != "" || s_bsd_arraymicellaneousid != "")
                        {
                            TracingSe.Trace("14");
                            if (tiendu < d_bsd_totalapplyamount)
                                throw new InvalidPluginExecutionException("The remaining amount is not enough to pay for transaction payments have been selected!");

                            else
                            {
                                //Han-23.03.2018: Doi d_now = pm_ReceiptDate
                                //transactionPM_INS(service, s_bsd_arraypsdid, s_bsd_arrayamountpay, s_bsd_arrayinstallmentinterest, s_bsd_arrayinterestamount,
                                //s_bsd_arrayfees, s_bsd_arrayfeesamount, tiendu, ref d_oe_bsd_totalamountpaid, paymentEn, optionentryEn, Product, pm_ReceiptDate, customerRef,
                                // s_bsd_arraymicellaneousid, s_bsd_arraymicellaneousamount);
                                TracingSe.Trace("14.1");
                                transactionPM_INS(
                                    service,
                                    s_bsd_arraypsdid,
                                    s_bsd_arrayamountpay,
                                    s_bsd_arrayinstallmentinterest,
                                    s_bsd_arrayinterestamount,
                                    s_bsd_arrayfees,
                                    s_bsd_arrayfeesamount,
                                    tiendu,
                                    ref d_oe_bsd_totalamountpaid,
                                    paymentEn,
                                    optionentryEn,
                                    Product,
                                    pm_ReceiptDate,
                                    customerRef,
                                    s_bsd_arraymicellaneousid,
                                    s_bsd_arraymicellaneousamount
                                );
                                TracingSe.Trace("14.2");

                            }

                        }
                        else
                        {
                            //throw new InvalidPluginExecutionException("ADV case 5");
                            //create_AdvPM(optionentryEn, customerRef, paymentEn, tiendu, (EntityReference)optionentryEn["bsd_project"], pm_ReceiptDate);
                            if (d_bsd_assignamountmaster > 0)
                            {
                                create_AdvPM(optionentryEn, customerRef, paymentEn, d_bsd_assignamountmaster, (EntityReference)optionentryEn["bsd_project"], pm_ReceiptDate);
                            }
                            else if (tiendu > 0)
                            {
                                create_AdvPM(optionentryEn, customerRef, paymentEn, tiendu, (EntityReference)optionentryEn["bsd_project"], pm_ReceiptDate);
                            }
                        }
                        TracingSe.Trace("14.3");
                    }
                    #endregion-----------------------------
                    TracingSe.Trace("15");
                } // end if tiendu > 0

                #endregion -------------- end late pm & transaction am ----------------- 
                #endregion
            }

            //>>> Trace Test
            TracingSe.Trace(String.Format("- pm_amountpay: {0}", pm_amountpay));
            TracingSe.Trace(String.Format("- pm_balance: {0}", pm_balance));
            TracingSe.Trace(String.Format("- bsd_interestchargeamount: {0}", psd_bsd_interestchargeamount));
            TracingSe.Trace(String.Format("- bsd_interestwaspaid: {0}", psd_bsd_interestwaspaid));
            TracingSe.Trace(String.Format("- bsd_actualgracedays: {0}", psd_bsd_interestwaspaid));

            //Throw #######################################
            //throw new Exception(strMess.ToString());
        }
        private void check_ins_notpaid(Guid inIns, Guid inOE)
        {
            int orderNumber = 0;
            var fetchXml = $@"<?xml version=""1.0"" encoding=""utf-16""?>
            <fetch>
              <entity name=""bsd_paymentschemedetail"">
                <attribute name=""bsd_ordernumber"" />
                <filter>
                  <condition attribute=""bsd_paymentschemedetailid"" operator=""eq"" value=""{inIns}"" />
                  <condition attribute=""bsd_ordernumber"" operator=""not-null"" />
                </filter>
              </entity>
            </fetch>";
            EntityCollection rs = service.RetrieveMultiple(new FetchExpression(fetchXml));
            foreach (Entity item in rs.Entities)
            {
                orderNumber = (int)item["bsd_ordernumber"];
            }
            if (orderNumber != 0)
            {
                fetchXml = $@"<?xml version=""1.0"" encoding=""utf-16""?>
                <fetch>
                  <entity name=""bsd_paymentschemedetail"">
                    <attribute name=""bsd_ordernumber"" />
                    <attribute name=""statuscode"" />
                    <filter>
                      <condition attribute=""bsd_optionentry"" operator=""eq"" value=""{inOE}"" />
                      <condition attribute=""bsd_ordernumber"" operator=""lt"" value=""{orderNumber}"" />
                      <condition attribute=""statuscode"" operator=""eq"" value=""{100000000}"" />
                    </filter>
                  </entity>
                </fetch>";
                rs = service.RetrieveMultiple(new FetchExpression(fetchXml));
                if (rs.Entities.Count > 0) throw new InvalidPluginExecutionException("The previous installment not paid.");
            }
        }
        public void interestCharge(Entity paymentEn)
        {
            info_Error info = new info_Error();
            DateTime d_now = RetrieveLocalTimeFromUTCTime(DateTime.Now);

            DateTime i_intereststartdate = d_now;

            decimal pm_balance = paymentEn.Contains("bsd_balance") ? ((Money)paymentEn["bsd_balance"]).Value : 0;
            decimal pm_amountpay = paymentEn.Contains("bsd_amountpay") ? ((Money)paymentEn["bsd_amountpay"]).Value : 0;
            bool f_bsd_latepayment = paymentEn.Contains("bsd_latepayment") ? (bool)paymentEn["bsd_latepayment"] : false;
            DateTime pm_ReceiptDate = paymentEn.Contains("bsd_paymentactualtime") ? RetrieveLocalTimeFromUTCTime((DateTime)paymentEn["bsd_paymentactualtime"]) : d_now;
            decimal d_bsd_totalapplyamount = paymentEn.Contains("bsd_totalapplyamount") ? ((Money)paymentEn["bsd_totalapplyamount"]).Value : 0;
            decimal d_bsd_assignamount = paymentEn.Contains("bsd_assignamount") ? ((Money)paymentEn["bsd_assignamount"]).Value : 0;
            string s_bsd_arraypsdid = paymentEn.Contains("bsd_arraypsdid") ? (string)paymentEn["bsd_arraypsdid"] : "";
            string s_bsd_arrayamountpay = paymentEn.Contains("bsd_arrayamountpay") ? (string)paymentEn["bsd_arrayamountpay"] : "";
            string s_bsd_arrayinstallmentinterest = paymentEn.Contains("bsd_arrayinstallmentinterest") ? (string)paymentEn["bsd_arrayinstallmentinterest"] : "";
            string s_bsd_arrayinterestamount = paymentEn.Contains("bsd_arrayinterestamount") ? (string)paymentEn["bsd_arrayinterestamount"] : "";
            string s_bsd_arrayfees = paymentEn.Contains("bsd_arrayfees") ? (string)paymentEn["bsd_arrayfees"] : "";
            string s_bsd_arrayfeesamount = paymentEn.Contains("bsd_arrayfeesamount") ? (string)paymentEn["bsd_arrayfeesamount"] : "";
            string s_bsd_arraymicellaneousid = paymentEn.Contains("bsd_arraymicellaneousid") ? (string)paymentEn["bsd_arraymicellaneousid"] : "";
            string s_bsd_arraymicellaneousamount = paymentEn.Contains("bsd_arraymicellaneousamount") ? (string)paymentEn["bsd_arraymicellaneousamount"] : "";

            EntityReference enrefInstallment = paymentEn.Contains("bsd_paymentschemedetail") ? (EntityReference)paymentEn["bsd_paymentschemedetail"] : null;
            Entity PaymentDetailEn = new Entity();
            if (enrefInstallment != null)
            {
                PaymentDetailEn = service.Retrieve("bsd_paymentschemedetail", ((EntityReference)paymentEn["bsd_paymentschemedetail"]).Id, new ColumnSet(true));
            }

            int phaseNum = PaymentDetailEn.Contains("bsd_ordernumber") ? (int)PaymentDetailEn["bsd_ordernumber"] : 0;
            decimal psd_amountPhase = PaymentDetailEn.Contains("bsd_amountofthisphase") ? ((Money)PaymentDetailEn["bsd_amountofthisphase"]).Value : 0;
            decimal psd_amountPaid = PaymentDetailEn.Contains("bsd_amountwaspaid") ? ((Money)PaymentDetailEn["bsd_amountwaspaid"]).Value : 0;
            decimal psd_deposit = PaymentDetailEn.Contains("bsd_depositamount") ? ((Money)PaymentDetailEn["bsd_depositamount"]).Value : 0;
            decimal psd_waiverIns = PaymentDetailEn.Contains("bsd_waiverinstallment") ? ((Money)PaymentDetailEn["bsd_waiverinstallment"]).Value : 0;
            int psd_statuscode = PaymentDetailEn.Contains("statuscode") ? ((OptionSetValue)PaymentDetailEn["statuscode"]).Value : 100000000;
            decimal pm_differentamount = paymentEn.Contains("bsd_differentamount") ? ((Money)paymentEn["bsd_differentamount"]).Value : 0;
            decimal psd_bsd_balance = PaymentDetailEn.Contains("bsd_balance") ? ((Money)PaymentDetailEn["bsd_balance"]).Value : 0;
            decimal psd_bsd_interestchargeamount = PaymentDetailEn.Contains("bsd_interestchargeamount") ? ((Money)PaymentDetailEn["bsd_interestchargeamount"]).Value : 0;
            int psd_actualgracedays = PaymentDetailEn.Contains("bsd_actualgracedays") ? (int)PaymentDetailEn["bsd_actualgracedays"] : 0;
            int i_bsd_previousdelays = PaymentDetailEn.Contains("bsd_previousdelays") ? (int)PaymentDetailEn["bsd_previousdelays"] : 0;
            decimal psd_bsd_interestwaspaid = PaymentDetailEn.Contains("bsd_interestwaspaid") ? ((Money)PaymentDetailEn["bsd_interestwaspaid"]).Value : 0;
            int psd_bsd_interestchargestatus = PaymentDetailEn.Contains("bsd_interestchargestatus") ? ((OptionSetValue)PaymentDetailEn["bsd_interestchargestatus"]).Value : 0;

            TracingSe.Trace("Load option entry");
            Entity optionentryEn = service.Retrieve("salesorder", ((EntityReference)paymentEn["bsd_optionentry"]).Id, new ColumnSet(true));
            decimal d_oe_bsd_totalamountpaid = optionentryEn.Contains("bsd_totalamountpaid") ? ((Money)optionentryEn["bsd_totalamountpaid"]).Value : 0;
            EntityReference customerRef = (EntityReference)optionentryEn["customerid"];

            Entity Product = service.Retrieve("product", ((EntityReference)optionentryEn["bsd_unitnumber"]).Id, new ColumnSet(true));
            decimal tiendu = 0;
            TracingSe.Trace("7");

            if (psd_bsd_interestchargestatus == 100000001)
            {
                if (PaymentDetailEn.Contains("bsd_name"))
                {
                    throw new InvalidPluginExecutionException("Interest charge of " + (string)PaymentDetailEn["bsd_name"] + " has been Paid!");
                }

            }

            if (psd_statuscode == 100000000)
            {
                if (PaymentDetailEn.Contains("bsd_name"))
                {
                    throw new InvalidPluginExecutionException("Installment " + (string)PaymentDetailEn["bsd_name"] + " haven't Paid. Can not payment Insterest charge");
                }
            }
            if (pm_amountpay == pm_balance)
            {
                // ------------------------  pm_differentamount = 0 ---------------------------
                psd_bsd_interestchargestatus = 100000001; // paid
                psd_bsd_interestwaspaid = psd_bsd_interestwaspaid + pm_amountpay;
                //up_InterestAM_Ins(service, PaymentDetailEn, psd_bsd_interestwaspaid, psd_bsd_interestchargestatus, d_now);
                //
            } // end if (pm_amountpay == pm_balance)
            else if (pm_amountpay < pm_balance)
            {
                // -------------------------   pm_amoutpay < pm_balance  ---------------------------
                psd_bsd_interestchargestatus = 100000000; // not paid
                psd_bsd_interestwaspaid += pm_amountpay;
                //
            }
            else if (pm_amountpay > pm_balance) // pm_different am > 0
            {
                // -------------------------   pm_amoutpay > pm_balance  ---------------------------
                psd_bsd_interestwaspaid += pm_balance;
                psd_bsd_interestchargestatus = 100000001;// paid

                // transaction payment
                // ------------ TRANSACTION ARRAY -------------------
                // call function
                tiendu = pm_amountpay - pm_balance;

                // tra ve totalamount paid of OE
                if (s_bsd_arrayamountpay != "" || s_bsd_arrayinstallmentinterest != "" || s_bsd_arrayfees != "" || s_bsd_arraymicellaneousid != "")
                {
                    if (tiendu < d_bsd_totalapplyamount)
                        throw new InvalidPluginExecutionException("The remaining amount is not enough to pay for transaction payments have been selected!");
                    else
                        //Han-23.03.2018: Doi d_now = pm_ReceiptDate
                        transactionPM_INS(service, s_bsd_arraypsdid, s_bsd_arrayamountpay, s_bsd_arrayinstallmentinterest, s_bsd_arrayinterestamount,
                        s_bsd_arrayfees, s_bsd_arrayfeesamount, tiendu, ref d_oe_bsd_totalamountpaid, paymentEn, optionentryEn, Product, pm_ReceiptDate, customerRef,
                        s_bsd_arraymicellaneousid, s_bsd_arraymicellaneousamount);
                }
                else
                {
                    //throw new InvalidPluginExecutionException("ADV case 3");
                    //d_bsd_assignamount = paymentEn.Contains("bsd_assignamount") ? ((Money)paymentEn["bsd_assignamount"]).Value : 0;
                    //create_AdvPM(optionentryEn, customerRef, paymentEn, tiendu, (EntityReference)optionentryEn["bsd_project"], pm_ReceiptDate);
                    if (d_bsd_assignamount > 0)
                    {
                        create_AdvPM(optionentryEn, customerRef, paymentEn, d_bsd_assignamount, (EntityReference)optionentryEn["bsd_project"], pm_ReceiptDate);
                    }

                }


                // --------- end transaction array ---------------

                // ------------- end pm_amount pay > pm_balance ----------------------------------
            }
            // update IC amount was paid in current INS

            //up_InterestAM_Ins(service, PaymentDetailEn, psd_bsd_interestwaspaid, psd_bsd_interestchargestatus, d_now);
        }
        public void update(Entity paymentEn, decimal pm_amountpay, decimal pm_sotiendot, decimal pm_sotiendatra, DateTime d_now, decimal pm_balance, decimal pm_differentamount, decimal d_inter, int i_lateday, int type, decimal d_bsd_assignamountmaster)
        {
            info_Error info = new info_Error();
            TracingSe.Trace("16 Update payment");
            Entity enUpPay = new Entity(paymentEn.LogicalName, paymentEn.Id);
            enUpPay["bsd_amountpay"] = new Money(pm_amountpay);
            enUpPay["bsd_totalamountpayablephase"] = new Money(pm_sotiendot);
            enUpPay["bsd_totalamountpaidphase"] = new Money(pm_sotiendatra);
            enUpPay["statuscode"] = new OptionSetValue(100000000);
            enUpPay["bsd_confirmeddate"] = d_now;
            enUpPay["bsd_confirmperson"] = new EntityReference("systemuser", context.UserId);
            TracingSe.Trace("17");
            enUpPay["bsd_balance"] = new Money(pm_balance);
            enUpPay["bsd_differentamount"] = new Money(pm_differentamount);
            if (type == 100000002)
            {
                enUpPay["bsd_assignamount"] = new Money(d_bsd_assignamountmaster);
            }
            TracingSe.Trace("18");
            service.Update(enUpPay);
            TracingSe.Trace("19 End Update payment");
        }
        public EntityCollection Get1st_Resv(string resvID)
        {
            string fetchXml =
                  @"<fetch version='1.0' output-format='xml-platform' mapping='logical' distinct='true' >
                  <entity name='bsd_paymentschemedetail' >
                    <attribute name='bsd_typeofstartdate' />
                    <attribute name='bsd_duedate' />
                    <attribute name='bsd_nextdaysofendphase' />
                    <attribute name='bsd_duedatecalculatingmethod' />
                    <attribute name='bsd_estimateamount' />
                    <attribute name='bsd_depositamount' />
                    <attribute name='bsd_nextperiodtype' />
                    <attribute name='statuscode' />
                    <attribute name='bsd_ordernumber' />
                    <attribute name='bsd_fixeddate' />
                    <attribute name='bsd_datepaymentofmonthly' />
                    <attribute name='bsd_paymentschemedetailid' />
                    <attribute name='bsd_amountpay' />
                    <attribute name='bsd_withindate' />
                    <attribute name='bsd_amountofthisphase' />
                    <attribute name='bsd_numberofnextmonth' />
                    <attribute name='bsd_emailreminderforeigner' />
                    <attribute name='bsd_numberofnextdays' />
                    <attribute name='bsd_balance' />
                    <attribute name='bsd_amountpercent' />
                    <attribute name='bsd_amountwaspaid' />
                    <attribute name='bsd_reservation' />
                    <attribute name='bsd_typepayment' />
                    <filter type='and' >
                      <condition attribute='bsd_reservation' operator='eq' value='{0}' />
                      <condition attribute='bsd_optionentry' operator='null' />
                      <condition attribute='bsd_ordernumber' operator='eq' value='1' />
                    </filter>
                    <order attribute='bsd_ordernumber' />
                  </entity>
            </fetch>";

            fetchXml = string.Format(fetchXml, resvID);
            EntityCollection entc = service.RetrieveMultiple(new FetchExpression(fetchXml));
            return entc;
        }
        public void reGenerateDeposit(Entity enQuote, DateTime dt_date)
        {
            QueryExpression q = new QueryExpression("bsd_paymentschemedetail");
            q.ColumnSet = new ColumnSet(true);
            q.Criteria = new FilterExpression(LogicalOperator.And);
            q.Criteria.AddCondition(new ConditionExpression("bsd_reservation", ConditionOperator.Equal, enQuote.Id));
            q.AddOrder("bsd_ordernumber", OrderType.Ascending);
            EntityCollection InstallList = service.RetrieveMultiple(q);

            if (InstallList.Entities.Count > 0)
            {
                foreach (Entity InstUp in InstallList.Entities)
                {
                    bool LastInstall = InstUp.Contains("bsd_lastinstallment") ? (bool)InstUp["bsd_lastinstallment"] : false;
                    bool TypeStart = InstUp.Contains("bsd_typeofstartdate") ? (bool)InstUp["bsd_typeofstartdate"] : false;
                    int OrderNumber = InstUp.Contains("bsd_ordernumber") ? (int)InstUp["bsd_ordernumber"] : 0;

                    int DayLocal = InstUp.Contains("bsd_withindate") ? (int)InstUp["bsd_withindate"] : 0;
                    int DayForeigner = InstUp.Contains("bsd_emailreminderforeigner") ? (int)InstUp["bsd_emailreminderforeigner"] : 0;

                    if (InstUp.Contains("bsd_duedatecalculatingmethod") && LastInstall == false)
                    {
                        int Inst_Method = (int)((OptionSetValue)InstUp["bsd_duedatecalculatingmethod"]).Value;

                        // 100000000 = FixDate ; 100000002 = Estimate handover date --> Khong cap nhat ngay
                        if (Inst_Method == 100000002 || Inst_Method == 100000000)
                            break;


                        if (Inst_Method == 100000001) // 100000001 = AutoDate
                        {
                            int i_bsd_nextperiodtype = (int)((OptionSetValue)InstUp["bsd_nextperiodtype"]).Value;
                            int payment_type = InstUp.Contains("bsd_typepayment") ? (int)((OptionSetValue)InstUp["bsd_typepayment"]).Value : 0;
                            int DefautlDays = InstUp.Contains("bsd_datepaymentofmonthly") ? (int)InstUp["bsd_datepaymentofmonthly"] : 0;
                            int Soluong = 0;

                            if (payment_type == 0 || payment_type == 1)//default or month
                            {
                                int type = (int)((OptionSetValue)InstUp["bsd_nextperiodtype"]).Value;
                                if (type == 1)//month
                                {
                                    Soluong = InstUp.Contains("bsd_numberofnextmonth") ? (int)InstUp["bsd_numberofnextmonth"] : 0;
                                    dt_date = dt_date.AddMonths(Soluong);
                                }
                                if (type == 2)//day
                                {
                                    double extraDay = 0;
                                    extraDay = double.Parse(InstUp["bsd_numberofnextdays"].ToString());
                                    dt_date = dt_date.AddDays(extraDay);
                                }

                                //Cong them ngay voi khach hang trong nuoc va nuoc ngoai
                                if (TypeStart == true && OrderNumber == 1)
                                {
                                    if (DayLocal != 0)
                                        dt_date = dt_date.AddDays(DayLocal);
                                    if (DayForeigner != 0)
                                        dt_date = dt_date.AddDays(DayForeigner);
                                }

                                //Set ngay mac dinh truoc khi update
                                if (DefautlDays != 0)
                                {
                                    int NgayCuoiThang = DateTime.DaysInMonth(dt_date.Year, dt_date.Month);
                                    if (DefautlDays > NgayCuoiThang)
                                        DefautlDays = NgayCuoiThang;
                                    dt_date = new DateTime(dt_date.Year, dt_date.Month, DefautlDays);
                                }

                                Entity UpdateEn = new Entity(InstUp.LogicalName);
                                UpdateEn.Id = InstUp.Id;
                                UpdateEn["bsd_duedate"] = dt_date;
                                service.Update(UpdateEn);
                            }
                            else
                            {
                                //Copy Code Anh Trí
                                if (InstUp.Contains("bsd_number"))
                                {
                                    int NumberTimes = InstUp.Contains("bsd_number") ? (int)InstUp["bsd_number"] : 0;
                                    int Nextdaysofendphase = InstUp.Contains("bsd_nextdaysofendphase") ? (int)InstUp["bsd_nextdaysofendphase"] : 0;

                                    for (int j = 0; j < NumberTimes; j++)
                                    {
                                        if (j == NumberTimes - 1)
                                            dt_date = dt_date.AddDays(Nextdaysofendphase);

                                        Entity UpdateEn = new Entity(InstUp.LogicalName);
                                        UpdateEn.Id = InstUp.Id;
                                        UpdateEn["bsd_duedate"] = dt_date;
                                        service.Update(UpdateEn);
                                    }
                                }
                                else
                                    throw new InvalidPluginExecutionException("Please setup 'Number of times' on Payment Scheme Detail.");
                            }
                        }
                    }
                }
            }
        }
        public EntityCollection get_ecFUL(IOrganizationService crmservices, Guid unit_id)
        {
            string fetchXml =
               @"<fetch version='1.0' output-format='xml-platform' mapping='logical' distinct='true' >
                <entity name='bsd_followuplist' >
                    <attribute name='bsd_followuplistid' />
                    <attribute name='bsd_name' />
                    <attribute name='bsd_units' />
                    <attribute name='bsd_optionentry' />
                    <attribute name='bsd_type' />
                    <attribute name='bsd_installment' />
                    <attribute name='bsd_reservation' />
                    <attribute name='statuscode' />
                    <attribute name='bsd_project' />
                    <attribute name='bsd_fincomment' />
                    <filter type='and' >
                      <condition attribute='statuscode' operator='eq' value='1' />
                      <condition attribute='bsd_units' operator='eq' value='{0}' />
                    </filter>
                </entity>
                </fetch>";
            fetchXml = string.Format(fetchXml, unit_id.ToString());
            TracingSe.Trace("fetch: " + fetchXml);
            EntityCollection entc = crmservices.RetrieveMultiple(new FetchExpression(fetchXml));
            return entc;
        }
        public Entity getMiscellaneous(IOrganizationService crmservices, string miscellaneousid)
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
                    <condition attribute='bsd_miscellaneousid' operator='eq' value='{0}'/>;
                </filter>                           
                </entity>
                </fetch>";
            fetchXml = string.Format(fetchXml, miscellaneousid);
            TracingSe.Trace(fetchXml);
            EntityCollection entc = crmservices.RetrieveMultiple(new FetchExpression(fetchXml));
            if (entc.Entities.Count > 0)
            {
                return entc.Entities[0];
            }
            else
            {
                return null;
            }
        }
        public void createTransPMMiss(Entity payment, Entity optionEntry, int i_transactionType, Entity enMiss, decimal amount, DateTime d_now, IOrganizationService serv,
            string producName, decimal d_ICamount, int i_outDays)
        {
            info_Error info = new info_Error();
            // transaction type :Other
            Entity en_pro = serv.Retrieve(((EntityReference)optionEntry["bsd_project"]).LogicalName, ((EntityReference)optionEntry["bsd_project"]).Id,
                new ColumnSet(new string[] { "bsd_name" }));
            TracingSe.Trace("2.6.1.1.2");
            Entity en_TransPM = new Entity("bsd_transactionpayment");
            en_TransPM["bsd_name"] = (string)en_pro["bsd_name"] + "-" + producName + "-" + (string)optionEntry["name"] + "-" + (string)payment["bsd_name"];
            en_TransPM["statuscode"] = new OptionSetValue(100000000); // confirm
            en_TransPM["bsd_payment"] = payment.ToEntityReference();
            en_TransPM["bsd_transactiontype"] = new OptionSetValue(i_transactionType);
            en_TransPM["bsd_installment"] = (EntityReference)enMiss["bsd_installment"];
            TracingSe.Trace("2.6.1.1.3");
            en_TransPM["bsd_miscellaneous"] = enMiss.ToEntityReference();
            TracingSe.Trace("2.6.1.1.4");
            en_TransPM["bsd_amount"] = new Money(amount);
            en_TransPM["createdon"] = d_now;
            TracingSe.Trace("2.6.1.1.5");
            serv.Create(en_TransPM);
        }
        public void create_AdvPM(Entity optionentryEn, EntityReference customerRef, Entity paymentEn, decimal tiendu, EntityReference project, DateTime d_transDate)
        {
            Entity en_AdvancePM = new Entity("bsd_advancepayment");
            en_AdvancePM["bsd_name"] = "Advance Payment of " + optionentryEn["name"].ToString();

            en_AdvancePM["bsd_customer"] = customerRef;
            en_AdvancePM["bsd_project"] = project;
            en_AdvancePM["bsd_payment"] = paymentEn.ToEntityReference();
            en_AdvancePM["bsd_optionentry"] = optionentryEn.ToEntityReference();
            en_AdvancePM["bsd_amount"] = new Money(tiendu);
            en_AdvancePM["statuscode"] = new OptionSetValue(100000000); // collected
            en_AdvancePM["bsd_paidamount"] = new Money(0);
            en_AdvancePM["bsd_refundamount"] = new Money(0);
            en_AdvancePM["bsd_remainingamount"] = new Money(tiendu);
            en_AdvancePM["bsd_transactiondate"] = d_transDate.Date;

            service.Create(en_AdvancePM);
        }
        public void createTransPM(Entity payment, Entity optionEntry, int i_transactionType, Guid pmsdtlID, decimal amount, DateTime d_now, IOrganizationService serv,
            string producName, string fee_type, decimal d_ICamount, int i_outDays)
        {
            // transaction type :
            // Installments	   100000000
            // Interest        100000001
            // Fees            100000002
            // Other           100000003
            Entity en_pro = serv.Retrieve(((EntityReference)optionEntry["bsd_project"]).LogicalName, ((EntityReference)optionEntry["bsd_project"]).Id,
                new ColumnSet(new string[] { "bsd_name" }));

            Entity en_TransPM = new Entity("bsd_transactionpayment");
            en_TransPM["bsd_name"] = (string)en_pro["bsd_name"] + "-" + producName + "-" + (string)optionEntry["name"] + "-" + (string)payment["bsd_name"];
            en_TransPM["statuscode"] = new OptionSetValue(100000000); // confirm
            en_TransPM["bsd_payment"] = payment.ToEntityReference();
            en_TransPM["bsd_transactiontype"] = new OptionSetValue(i_transactionType);
            //mainternence fee:100000000
            if (i_transactionType == 100000002)
            {
                if (fee_type == "main")
                    en_TransPM["bsd_feetype"] = new OptionSetValue(100000000);
                else en_TransPM["bsd_feetype"] = new OptionSetValue(100000001);
            }
            if (i_transactionType == 100000000)
            {
                en_TransPM["bsd_actualgracedays"] = i_outDays;
                en_TransPM["bsd_interestchargeamount"] = new Money(d_ICamount);
            }

            en_TransPM["bsd_installment"] = new EntityReference("bsd_paymentschemedetail", pmsdtlID);
            en_TransPM["bsd_amount"] = new Money(amount);
            en_TransPM["createdon"] = d_now;

            serv.Create(en_TransPM);


        }
        public Entity getInstallment(IOrganizationService crmservices, string installmentid)
        {
            string fetchXml =
               @"<fetch version='1.0' output-format='xml-platform' mapping='logical' distinct='true' >
                <entity name='bsd_paymentschemedetail' >
                <attribute name='bsd_duedatecalculatingmethod' />
                <attribute name='bsd_maintenanceamount' />
                <attribute name='bsd_maintenancefeepaid' />
                <attribute name='bsd_maintenancefeewaiver' />
                <attribute name='bsd_ordernumber' />
                <attribute name='statuscode' />
                <attribute name='bsd_managementfeepaid' />
                <attribute name='bsd_managementfeewaiver' />
                <attribute name='bsd_amountpay' />
                <attribute name='bsd_maintenancefees' />
                <attribute name='bsd_optionentry' />
                <attribute name='bsd_managementfee' />
                <attribute name='bsd_managementfeesstatus' />
                <attribute name='bsd_managementamount' />
                <attribute name='bsd_amountwaspaid' />
                <attribute name='bsd_maintenancefeesstatus' />
                <attribute name='bsd_name' />
                <attribute name='bsd_maintenancefees' />
                <attribute name='bsd_managementfee' />
                <attribute name='bsd_duedatecalculatingmethod' />
                <attribute name='bsd_paymentschemedetailid' />
                <attribute name='bsd_amountofthisphase' />
                <filter type='and' >
                  <condition attribute='bsd_paymentschemedetailid' operator='eq' value='{0}' />                  
                </filter>
              </entity>
            </fetch>";
            //<condition attribute='bsd_duedatecalculatingmethod' operator='eq' value='100000002' />

            fetchXml = string.Format(fetchXml, installmentid);
            TracingSe.Trace("getInstallment");
            TracingSe.Trace(fetchXml);
            EntityCollection entc = crmservices.RetrieveMultiple(new FetchExpression(fetchXml));
            return entc.Entities[0];
        }

        public void CheckInterestCharge(IOrganizationService serv, ref decimal d_inter, ref int i_late, Entity PaymentDetailEn, Entity paymentEn, decimal pm_amountpay, Entity optionentryEn, ref DateTime i_intereststartdate)
        {
            TracingSe.Trace("pm_amountpay: " + pm_amountpay.ToString());
            TracingSe.Trace("CheckInterestCharge");
            TracingSe.Trace("Input d_inter: " + d_inter);
            TracingSe.Trace("Input i_late: " + i_late);
            if (!PaymentDetailEn.Contains("bsd_duedate"))  // if pmsdtl not contain duedate( last or ES installment )
            {
                d_inter = 0;
                i_late = 0;
            }
            else
            {
                DateTime d_duedate = (DateTime)PaymentDetailEn["bsd_duedate"];
                DateTime d_receipt = (DateTime)paymentEn["bsd_paymentactualtime"];
                TracingSe.Trace("d_duedate: " + d_duedate);
                TracingSe.Trace("d_receiptz: " + d_receipt);
                OrganizationRequest req = new OrganizationRequest("bsd_Action_Calculate_Interest");
                //parameter:
                req["installmentid"] = PaymentDetailEn.Id.ToString();
                req["amountpay"] = pm_amountpay.ToString();
                req["receiptdate"] = d_receipt.ToString("MM/dd/yyyy");
                //execute the request
                OrganizationResponse response = service.Execute(req);
                TracingSe.Trace("response.Results.Count: " + response.Results.Count);
                foreach (var item in response.Results)
                {
                    var serializer = new JavaScriptSerializer();
                    var result = serializer.Deserialize<Installment>(item.Value.ToString());
                    TracingSe.Trace("item.Value: " + item.Value.ToString());
                    i_late = result.LateDays;
                    i_intereststartdate = result.InterestStarDate;
                    if (i_late < 0) i_late = 0;
                    if (i_late > 0)
                    {
                        if (i_late > 0)
                        {
                            decimal d_percent = result.InterestPercent;
                            TracingSe.Trace("d_percent: " + d_percent.ToString());
                            decimal rangeAmount = result.MaxAmount;
                            TracingSe.Trace("rangeAmount: " + rangeAmount.ToString());
                            decimal rangePercent = result.MaxPercent;
                            d_inter = result.InterestCharge;
                            TracingSe.Trace("installment.calc_InterestCharge: " + d_inter);
                        }
                    }
                }
            }
        }

        public info_Error CheckInterestCharge_New(IOrganizationService serv, ref decimal d_inter, ref int i_late, Entity PaymentDetailEn, Entity paymentEn, decimal pm_amountpay, Entity optionentryEn, ref DateTime i_intereststartdate)
        {
            info_Error info = new info_Error();
            info.index = 1;
            TracingSe.Trace("[CheckInterestCharge_New] Tham số đầu vào:");
            TracingSe.Trace(string.Format(string.Format("- pm_amountpay: {0}", pm_amountpay.ToString())));
            TracingSe.Trace(string.Format("+ Input d_inter: {0}", d_inter));
            TracingSe.Trace(string.Format("+ Input i_late: {0}", i_late));
            try
            {
                TracingSe.Trace("có bsd_signeddadate || bsd_signedcontractdate ");
                if (!PaymentDetailEn.Contains("bsd_duedate"))
                {
                    TracingSe.Trace("#. Không có bsd_duedate");
                    d_inter = 0;
                    i_late = 0;
                }
                else
                {
                    TracingSe.Trace("#. Có bsd_duedate");
                    DateTime d_duedate = (DateTime)PaymentDetailEn["bsd_duedate"];
                    DateTime d_receipt = (DateTime)paymentEn["bsd_paymentactualtime"];

                    //>>> Trace
                    TracingSe.Trace(string.Format("+ d_duedate: {0}", d_duedate));
                    TracingSe.Trace(string.Format("+ d_receiptz: {0}", d_receipt));
                    OrganizationRequest req = new OrganizationRequest("bsd_Action_Calculate_Interest");
                    //parameter:
                    req["installmentid"] = PaymentDetailEn.Id.ToString();
                    req["amountpay"] = pm_amountpay.ToString();
                    req["receiptdate"] = d_receipt.ToString("MM/dd/yyyy");
                    //execute the request
                    OrganizationResponse response = service.Execute(req);
                    foreach (var item in response.Results)
                    {
                        var serializer = new JavaScriptSerializer();
                        var result = serializer.Deserialize<Installment>(item.Value.ToString());
                        i_late = result.LateDays;
                        i_intereststartdate = result.InterestStarDate;
                        TracingSe.Trace(string.Format("+ i_intereststartdate: {0}", i_intereststartdate));
                        TracingSe.Trace(string.Format("@. i_late: {0}", i_late));

                        if (i_late < 0) i_late = 0;
                        if (i_late > 0)
                        {
                            if (i_late > 0)
                            {
                                decimal d_percent = result.InterestPercent;
                                decimal rangeAmount = result.MaxAmount;
                                decimal rangePercent = result.MaxPercent;
                                d_inter = result.InterestCharge;

                                //>>> Trace
                                TracingSe.Trace(string.Format("------------------------------"));
                                TracingSe.Trace(string.Format("+ d_percent: {0}", d_percent.ToString()));
                                TracingSe.Trace(string.Format("+ rangeAmount: {0}", rangeAmount.ToString()));
                                TracingSe.Trace(string.Format("+ installment.calc_InterestCharge: {0}", d_inter.ToString()));
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                TracingSe.Trace(string.Format("sys ex: {0}", ex.Message));
            }
            return info;
        }

        public decimal SumInterestAM_OE(IOrganizationService crmservices, Guid OEID)
        {
            decimal sumAmount = 0;
            string fetchXml =
              @"<fetch version='1.0' output-format='xml-platform' mapping='logical' distinct='false'>
                  <entity name='bsd_paymentschemedetail' >
                    <attribute name='bsd_interestchargestatus' />
                    <attribute name='bsd_interestchargeamount' />
                    <filter type='and' >
                      <condition attribute='bsd_optionentry' operator='eq' value='{0}' />
                        <condition attribute='bsd_interestchargeamount' operator='not-null' />
                    </filter>
                    <order attribute='bsd_ordernumber' descending='true' />
                  </entity>
                </fetch>";

            fetchXml = string.Format(fetchXml, OEID);
            EntityCollection entc = crmservices.RetrieveMultiple(new FetchExpression(fetchXml));
            foreach (Entity en_r in entc.Entities)
            {
                sumAmount += en_r.Contains("bsd_interestchargeamount") ? ((Money)en_r["bsd_interestchargeamount"]).Value : 0;
            }
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
            return sumAmount;

        }
        private void up_InterestAM_Ins(IOrganizationService serv, Entity en_Ins, decimal bsd_interestwaspaid, int statuscode, DateTime d_now)
        {
            Entity ins_tmp = new Entity(en_Ins.LogicalName);
            ins_tmp.Id = en_Ins.Id;
            ins_tmp["bsd_interestwaspaid"] = new Money(bsd_interestwaspaid);
            ins_tmp["bsd_interestchargestatus"] = new OptionSetValue(statuscode);
            if (statuscode == 100000001)
                ins_tmp["bsd_interestchargepaiddate"] = d_now;

            serv.Update(ins_tmp);
        }
        public void transactionPM_INS(IOrganizationService serv, string s_bsd_arraypsdid, string s_bsd_arrayamountpay, string s_bsd_arrayinstallmentinterest,
            string s_bsd_arrayinterestamount, string s_fees, string s_feesAM, decimal tiendu, ref decimal d_totalAM_OE, Entity paymentEn, Entity optionentryEn,
            Entity product, DateTime d_now, EntityReference customerRef, string s_bsd_arraymicellaneousid, string s_bsd_arraymicellaneousamount)
        {
            info_Error info = new info_Error();
            TracingSe.Trace("a");
            string[] s_idINS = { };
            string[] s_amINS = { };
            string[] s_idIC = { };
            string[] s_amIC = { };
            //string[] s_idFee = { };
            //string[] s_feeAM = { };

            // 170808 Miscellinous
            string[] s_Miss_id = { };
            string[] s_Miss_AM = { };

            if (s_bsd_arraypsdid != "")
            {
                s_idINS = s_bsd_arraypsdid.Split(',');
                s_amINS = s_bsd_arrayamountpay.Split(',');
            }
            if (s_bsd_arrayinstallmentinterest != "")
            {
                s_idIC = s_bsd_arrayinstallmentinterest.Split(',');
                s_amIC = s_bsd_arrayinterestamount.Split(',');
            }
            TracingSe.Trace("b");

            if (tiendu > 0)
            {
                #region -------------------------- array installment ---------------------------------
                if (s_bsd_arraypsdid != "")
                {
                    TracingSe.Trace("s_bsd_arraypsdid != rỗng");
                    EntityCollection ec_INS = get_ecINS(service, s_idINS);
                    if (ec_INS.Entities.Count <= 0) throw new InvalidPluginExecutionException("Cannot find any Installment from Installment list. Please check again!");
                    for (int i = 0; i < ec_INS.Entities.Count; i++)
                    {
                        #region ------------------------------- retreive PMSDTL ------------------------------
                        Entity en_pms = ec_INS.Entities[i];
                        en_pms.Id = ec_INS.Entities[i].Id;

                        if (!en_pms.Contains("statuscode")) throw new InvalidPluginExecutionException("Please check status code of '" + (string)en_pms["bsd_name"] + "!");
                        int pms_statuscode = ((OptionSetValue)en_pms["statuscode"]).Value;
                        TracingSe.Trace("1111111111111111");
                        //
                        if (pms_statuscode == 100000001) throw new InvalidPluginExecutionException((string)en_pms["bsd_name"] + " has been Paid!");

                        //int i_duedateMethod = en_pms.Contains("bsd_duedatecalculatingmethod") ? ((OptionSetValue)en_pms["bsd_duedatecalculatingmethod"]).Value : 0;
                        if (!en_pms.Contains("bsd_amountofthisphase")) throw new InvalidPluginExecutionException("Installment " + (string)en_pms["bsd_name"] + " did not contain 'Amount of this phase'!");
                        decimal pms_amountPhase = ((Money)en_pms["bsd_amountofthisphase"]).Value;
                        decimal pms_amountPaid = en_pms.Contains("bsd_amountwaspaid") ? ((Money)en_pms["bsd_amountwaspaid"]).Value : 0;
                        decimal pms_deposit = en_pms.Contains("bsd_depositamount") ? ((Money)en_pms["bsd_depositamount"]).Value : 0;
                        decimal pms_waiver = en_pms.Contains("bsd_waiveramount") ? ((Money)en_pms["bsd_waiveramount"]).Value : 0;
                        decimal pms_waiverIns = en_pms.Contains("bsd_waiverinstallment") ? ((Money)en_pms["bsd_waiverinstallment"]).Value : 0;
                        decimal pms_bsd_balance = en_pms.Contains("bsd_balance") ? ((Money)en_pms["bsd_balance"]).Value : 0;
                        TracingSe.Trace("22222222222222");
                        bool pms_bsd_maintenancefeesstatus = en_pms.Contains("bsd_maintenancefeesstatus") ? (bool)en_pms["bsd_maintenancefeesstatus"] : false;
                        bool pms_bsd_managementfeesstatus = en_pms.Contains("bsd_managementfeesstatus") ? (bool)en_pms["bsd_managementfeesstatus"] : false;
                        int i_phaseNum = (int)en_pms["bsd_ordernumber"];

                        decimal pms_waiverIC = en_pms.Contains("bsd_waiverinterest") ? ((Money)en_pms["bsd_waiverinterest"]).Value : 0;
                        decimal pms_IC_am = en_pms.Contains("bsd_interestchargeamount") ? ((Money)en_pms["bsd_interestchargeamount"]).Value : 0;
                        decimal pms_IC_amPaid = en_pms.Contains("bsd_interestwaspaid") ? ((Money)en_pms["bsd_interestwaspaid"]).Value : 0;
                        int pms_IC_status = en_pms.Contains("bsd_interestchargestatus") ? ((OptionSetValue)en_pms["bsd_interestchargestatus"]).Value : 100000000;
                        int i_pms_bsd_actualgracedays = en_pms.Contains("bsd_actualgracedays") ? (int)en_pms["bsd_actualgracedays"] : 0;
                        TracingSe.Trace(s_bsd_arraypsdid.ToString());
                        TracingSe.Trace("3333333333333");
                        #endregion  --------- end retreive INS with id from array -------------

                        // k can kiem tra neu installment truoc do da paid hay chua van cho thanh toan installment nay
                        // 170308 - Han require
                        decimal d_amp = 0;
                        bool f_checkIDagain = false;
                        decimal d_IC_amount = 0;
                        int i_lateday = 0;
                        int i_oeLateday = optionentryEn.Contains("bsd_totallatedays") ? (int)optionentryEn["bsd_totallatedays"] : 0;
                        // get amount of INS ID
                        for (int m = 0; m < s_idINS.Length; m++)
                        {
                            TracingSe.Trace("vào for");
                            if (en_pms.Id.ToString() == s_idINS[m])
                            {
                                TracingSe.Trace(s_amINS[0].ToString());
                                d_amp = decimal.Parse(s_amINS[m]);
                                f_checkIDagain = true;
                                break;
                            }
                        }
                        TracingSe.Trace("44444444444444");
                        if (f_checkIDagain == false) throw new InvalidPluginExecutionException("Cannot find ID of '" + (string)en_pms["bsd_name"] + "' in Installment array!");

                        #region ------------ check balance of INS -----------
                        if (i_phaseNum == 1)
                            pms_bsd_balance = pms_amountPhase - pms_amountPaid - pms_deposit - pms_waiverIns;// psd_balance - psd_deposit
                        else pms_bsd_balance = pms_amountPhase - pms_amountPaid - pms_waiverIns;// psd_balance - psd_deposit
                        #endregion ---------- check balance -----------------
                        TracingSe.Trace("amount pay: " + d_amp.ToString());
                        TracingSe.Trace("pms_bsd_balance: " + pms_bsd_balance.ToString());
                        #region ----------------- d_amp = balance -----------
                        if (d_amp == pms_bsd_balance)
                        {
                            // ins paid
                            // update oe amount
                            // update status code OE
                            pms_amountPaid += d_amp;
                            pms_bsd_balance = 0;
                            pms_statuscode = 100000001; // paid

                            d_totalAM_OE += d_amp;
                            tiendu -= d_amp;
                        }
                        #endregion d_ampay = balance

                        #region ----------- ampay < balance -------------------
                        else if (d_amp < pms_bsd_balance)
                        {
                            // ins paid
                            // update oe amount
                            // update status code OE
                            pms_amountPaid += d_amp;
                            pms_bsd_balance -= d_amp;
                            pms_statuscode = 100000000; // not paid

                            d_totalAM_OE += d_amp;
                            tiendu -= d_amp;
                        }
                        #endregion -------------------------------------------

                        #region ---------------- d_amp > balance -------------
                        else //(d_amp > pms_bsd_balance)
                            throw new InvalidPluginExecutionException("Input amount was larger than balance amount of " + (string)en_pms["bsd_name"] + ". Please check again!");
                        #endregion ------------------------------------------
                        #region --------------- cacl interst charge amount for this INS -------------
                        DateTime i_intereststartdate = d_now;

                        CheckInterestCharge(serv, ref d_IC_amount, ref i_lateday, en_pms, paymentEn, d_amp, optionentryEn, ref i_intereststartdate);
                        decimal d_sumTmp_IC_amount = pms_IC_am + Convert.ToDecimal(d_IC_amount); // Intest charge amount moi + IC san co trong INS
                        if (d_sumTmp_IC_amount < 0) d_sumTmp_IC_amount = 0;
                        // check outstanding day
                        if (i_lateday < i_pms_bsd_actualgracedays) i_lateday = i_pms_bsd_actualgracedays;
                        i_oeLateday += i_lateday;
                        #endregion -------------------------------------------

                        #region --------------- update current INS -------------
                        Entity en_INS_up = new Entity(en_pms.LogicalName);
                        en_INS_up.Id = en_pms.Id;

                        en_INS_up["bsd_interestchargeamount"] = new Money(d_sumTmp_IC_amount);
                        en_INS_up["bsd_amountwaspaid"] = new Money(pms_amountPaid);
                        en_INS_up["bsd_balance"] = new Money(pms_bsd_balance);
                        en_INS_up["statuscode"] = new OptionSetValue(pms_statuscode);
                        if (pms_statuscode == 100000001)
                            en_INS_up["bsd_paiddate"] = d_now;  // update ngay paid cua INS

                        en_INS_up["bsd_actualgracedays"] = i_lateday;
                        en_INS_up["bsd_interestchargeamount"] = new Money(d_sumTmp_IC_amount);
                        en_INS_up["bsd_interestchargestatus"] = new OptionSetValue(100000000); // not paid

                        //Han_28072018: Update Interest Start Date khi thanh toan
                        DateTime psd_intereststartdate = RetrieveLocalTimeFromUTCTime(i_intereststartdate);
                        en_INS_up["bsd_intereststartdate"] = psd_intereststartdate;
                        //
                        TracingSe.Trace("update en_INS_up");
                        serv.Update(en_INS_up);
                        #endregion ---------------------------------------------

                        #region -------------------- create transaction payment detail -------------
                        TracingSe.Trace("create transaction payment detail");
                        createTransPM(paymentEn, optionentryEn, 100000000, en_pms.Id, d_amp, d_now, serv, (string)product["name"], "", d_IC_amount, i_lateday);
                        #endregion --------------------------------------------

                    } // end for each INS in INS array

                } // end if (s_bsd_arrayID !="")
                TracingSe.Trace("end if (s_bsd_arrayID !=)");
                #endregion ----------------- end array installment ----------------

                #region ------------------s_bsd_arrayinstallmentinterest!="" -----------------
                if (s_bsd_arrayinstallmentinterest != "")
                {
                    TracingSe.Trace("a.1");
                    EntityCollection ec_INS = get_ecINS(service, s_idIC);
                    TracingSe.Trace("a.1.2");
                    for (int i = 0; i < ec_INS.Entities.Count; i++)
                    {
                        TracingSe.Trace("a.2");
                        #region ------------------------------- retreive PMSDTL ------------------------------
                        Entity en_pms = ec_INS.Entities[i];
                        en_pms.Id = ec_INS.Entities[i].Id;
                        TracingSe.Trace("a.3");
                        if (!en_pms.Contains("statuscode")) throw new InvalidPluginExecutionException("Please check status code of '" + (string)en_pms["bsd_name"] + "!");
                        int pms_statuscode = ((OptionSetValue)en_pms["statuscode"]).Value;
                        int pms_IC_status = en_pms.Contains("bsd_interestchargestatus") ? ((OptionSetValue)en_pms["bsd_interestchargestatus"]).Value : 100000000;
                        // if statuscode of INS is not paid - can not payment for Interest charge
                        TracingSe.Trace("a.4");
                        //thanhdo
                        //if (pms_statuscode == 100000000) throw new InvalidPluginExecutionException((string)en_pms["bsd_name"] + " has not Paid yet. Can not payment for Interest charge amount!");
                        if (pms_IC_status == 100000001) throw new InvalidPluginExecutionException(" Interest charge amount of " + (string)en_pms["bsd_name"] + " has been Paid!");

                        //int i_duedateMethod = en_pms.Contains("bsd_duedatecalculatingmethod") ? ((OptionSetValue)en_pms["bsd_duedatecalculatingmethod"]).Value : 0;
                        if (!en_pms.Contains("bsd_amountofthisphase")) throw new InvalidPluginExecutionException("Installment " + (string)en_pms["bsd_name"] + " did not contain 'Amount of this phase'!");

                        int i_phaseNum = (int)en_pms["bsd_ordernumber"];
                        TracingSe.Trace("a.5");
                        decimal pms_waiverIC = en_pms.Contains("bsd_waiverinterest") ? ((Money)en_pms["bsd_waiverinterest"]).Value : 0;
                        decimal pms_IC_am = en_pms.Contains("bsd_interestchargeamount") ? ((Money)en_pms["bsd_interestchargeamount"]).Value : 0;
                        decimal pms_IC_amPaid = en_pms.Contains("bsd_interestwaspaid") ? ((Money)en_pms["bsd_interestwaspaid"]).Value : 0;
                        int i_pms_bsd_actualgracedays = en_pms.Contains("bsd_actualgracedays") ? (int)en_pms["bsd_actualgracedays"] : 0;
                        #endregion  --------- end retreive INS with id from array -------------
                        TracingSe.Trace("a.6");
                        decimal d_amp = 0;
                        bool f_checkIDagain = false;

                        // get amount of INS ID
                        for (int m = 0; m < s_idIC.Length; m++)
                        {
                            if (en_pms.Id.ToString() == s_idIC[m])
                            {
                                d_amp = decimal.Parse(s_amIC[m]);
                                f_checkIDagain = true;
                                break;
                            }
                        }

                        if (f_checkIDagain == false) throw new InvalidPluginExecutionException("Cannot find ID of '" + (string)en_pms["bsd_name"] + "' in Interest charge array!");

                        #region ------------ balance of IC amout -----------
                        decimal d_ICAM_balance = pms_IC_am - pms_IC_amPaid - pms_waiverIC;
                        #endregion ---------- check balance -----------------
                        #region ----------------- d_amp = balance -----------
                        if (d_amp == d_ICAM_balance)
                        {
                            // ins paid
                            // update oe amount
                            // update status code OE
                            pms_IC_amPaid += d_amp;
                            pms_IC_status = 100000001; // paid
                            tiendu -= d_amp;
                        }
                        #endregion --------------

                        #region ----------------- d_amp < balance -----------
                        else if (d_amp < d_ICAM_balance)
                        {
                            // ins paid
                            // update oe amount
                            // update status code OE
                            pms_IC_amPaid += d_amp;
                            pms_IC_status = 100000000; // not paid
                            tiendu -= d_amp;
                        }
                        #endregion --------------

                        #region ---------------- d_amp > balance -------------
                        else // if (d_amp > d_ICAM_balance)
                            throw new InvalidPluginExecutionException("Input amount was larger than balance Interest charge amount of " + (string)en_pms["bsd_name"] + ". Please check again!");
                        #endregion ------------------------------------------

                        #region --------------- update current INS -------------
                        Entity en_INS_up = new Entity(en_pms.LogicalName);
                        en_INS_up.Id = en_pms.Id;

                        en_INS_up["bsd_interestwaspaid"] = new Money(pms_IC_amPaid);
                        en_INS_up["bsd_interestchargestatus"] = new OptionSetValue(pms_IC_status); // not paid
                        serv.Update(en_INS_up);
                        #endregion ---------------------------------------------

                        #region create transaction payment detail -------------
                        createTransPM(paymentEn, optionentryEn, 100000001, en_pms.Id, d_amp, d_now, serv, (string)product["name"], "", 0, 0);
                        #endregion --------------------------------------------


                    } // end if s_bsd_ICID !=""

                } // end if s_bsd_array of IC !=""
                #endregion ------------------- s_bsd_arrayinstallmentinterest!="" -------------------

                #region  ---------------------------- fees ThanhDo---------------------------------------------
                if (s_fees != "")
                {
                    TracingSe.Trace("s_fees != rỗng");
                    decimal d_oe_bsd_totalamountpaid = optionentryEn.Contains("bsd_totalamountpaid") ? ((Money)optionentryEn["bsd_totalamountpaid"]).Value : 0;
                    Entity Product = service.Retrieve("product", ((EntityReference)optionentryEn["bsd_unitnumber"]).Id, new ColumnSet(true));
                    string[] arrId = s_fees.Split(',');
                    string[] arrFeeAmount = s_feesAM.Split(',');
                    for (int i = 0; i < arrId.Length; i++)
                    {
                        string[] arr = arrId[i].Split('_');
                        string installmentid = arr[0];
                        string type1 = arr[1];

                        decimal fee = decimal.Parse(arrFeeAmount[i].ToString());
                        TracingSe.Trace("6.6");
                        Entity enInstallment = getInstallment(service, installmentid);
                        TracingSe.Trace("enInstallment: " + enInstallment.Id.ToString());
                        bool f_mains = (enInstallment.Contains("bsd_maintenancefeesstatus")) ? (bool)enInstallment["bsd_maintenancefeesstatus"] : false;
                        bool f_manas = (enInstallment.Contains("bsd_managementfeesstatus")) ? (bool)enInstallment["bsd_managementfeesstatus"] : false;

                        decimal bsd_maintenanceamount = enInstallment.Contains("bsd_maintenanceamount") ? ((Money)enInstallment["bsd_maintenanceamount"]).Value : 0;
                        decimal bsd_managementamount = enInstallment.Contains("bsd_managementamount") ? ((Money)enInstallment["bsd_managementamount"]).Value : 0;

                        decimal bsd_maintenancefeepaid = enInstallment.Contains("bsd_maintenancefeepaid") ? ((Money)enInstallment["bsd_maintenancefeepaid"]).Value : 0;
                        decimal bsd_managementfeepaid = enInstallment.Contains("bsd_managementfeepaid") ? ((Money)enInstallment["bsd_managementfeepaid"]).Value : 0;

                        decimal bsd_maintenancefeewaiver = enInstallment.Contains("bsd_maintenancefeewaiver") ? ((Money)enInstallment["bsd_maintenancefeewaiver"]).Value : 0;
                        decimal bsd_managementfeewaiver = enInstallment.Contains("bsd_managementfeewaiver") ? ((Money)enInstallment["bsd_managementfeewaiver"]).Value : 0;

                        decimal mainBL = bsd_maintenanceamount - bsd_maintenancefeepaid - bsd_maintenancefeewaiver;
                        decimal manaBL = bsd_managementamount - bsd_managementfeepaid - bsd_managementfeewaiver;
                        Entity en_INS_update = new Entity(enInstallment.LogicalName);
                        en_INS_update.Id = enInstallment.Id;
                        TracingSe.Trace("en_INS_update: " + en_INS_update.Id.ToString());
                        switch (type1)
                        {
                            case "main":
                                if (f_mains == true) throw new InvalidPluginExecutionException("Maintenance fees had been paid. Please check again!");
                                if (fee < mainBL)
                                {
                                    tiendu -= fee;
                                    bsd_maintenancefeepaid += fee;
                                    f_mains = false;
                                    d_oe_bsd_totalamountpaid += fee;

                                }
                                else if (fee == mainBL)
                                {
                                    tiendu -= fee;
                                    bsd_maintenancefeepaid += fee;
                                    f_mains = true;
                                    d_oe_bsd_totalamountpaid += fee;

                                }
                                else if (fee > mainBL) throw new InvalidPluginExecutionException("Amount pay larger than balance of Maintenance fees amount. Please check again!");
                                createTransPM(paymentEn, optionentryEn, 100000002, enInstallment.Id, fee, d_now.Date, service, (string)Product["name"], type1, 0, 0);
                                en_INS_update["bsd_maintenancefeesstatus"] = f_mains;
                                en_INS_update["bsd_maintenancefeepaid"] = new Money(bsd_maintenancefeepaid);
                                break;
                            case "mana":
                                if (f_manas == true) throw new InvalidPluginExecutionException("Management fees had been paid. Please check again!");
                                if (fee < manaBL)
                                {
                                    tiendu -= fee;
                                    bsd_managementfeepaid += fee;
                                    f_manas = false;
                                }
                                else if (fee == manaBL)
                                {
                                    tiendu -= fee;
                                    bsd_managementfeepaid += fee;
                                    f_manas = true;
                                }
                                else if (fee > manaBL) throw new InvalidPluginExecutionException("Amount pay larger than balance of Management fees amount. Please check again!");
                                createTransPM(paymentEn, optionentryEn, 100000002, enInstallment.Id, fee, d_now.Date, service, (string)Product["name"], type1, 0, 0);
                                en_INS_update["bsd_managementfeesstatus"] = f_manas;
                                en_INS_update["bsd_managementfeepaid"] = new Money(bsd_managementfeepaid);
                                break;
                        }
                        service.Update(en_INS_update);

                    }

                    #endregion

                } // end if s_fees !=""


                #region  --------------------------- MISCELLINOUS-----------------------------
                if (s_bsd_arraymicellaneousid != "")
                {
                    TracingSe.Trace("s_bsd_arraymicellaneousid:" + s_bsd_arraymicellaneousid);
                    TracingSe.Trace("s_bsd_arraymicellaneousamount:" + s_bsd_arraymicellaneousamount);
                    s_Miss_id = s_bsd_arraymicellaneousid.Split(',');
                    s_Miss_AM = s_bsd_arraymicellaneousamount.Split(',');
                    string[] strSumAmountMis = s_bsd_arraymicellaneousamount.Split(',');
                    decimal sumMisCheck = 0;
                    foreach (string amount in strSumAmountMis)
                    {
                        var decimalCovertAmount = Convert.ToDecimal(amount);
                        TracingSe.Trace("decimalCovertAmount: " + decimalCovertAmount.ToString());
                        sumMisCheck += decimalCovertAmount;
                    }
                    //decimal amountpayablephaseMis = paymentEn.Contains("bsd_totalamountpayablephase") ? ((Money)paymentEn["bsd_totalamountpayablephase"]).Value : 0;
                    decimal amountpayMis = paymentEn.Contains("bsd_totalapplyamount") ? ((Money)paymentEn["bsd_totalapplyamount"]).Value : 0;
                    //sumMisCheck += amountpayablephaseMis;
                    //throw new InvalidPluginExecutionException("sumMisCheck:" + sumMisCheck.ToString()+ "amountpayMis:"+ amountpayMis.ToString());
                    TracingSe.Trace("amountpayMis: " + amountpayMis.ToString());
                    TracingSe.Trace("sumMisCheck: " + sumMisCheck.ToString());
                    if (amountpayMis < sumMisCheck)
                    {
                        throw new InvalidPluginExecutionException("Invalid amount!");
                    }

                    EntityCollection ec_MIS = get_ecMIS(service, s_Miss_id, optionentryEn.Id.ToString());
                    if (ec_MIS.Entities.Count < 0) throw new InvalidPluginExecutionException("There is not any Miscellaneous found!");
                    TracingSe.Trace(ec_MIS.Entities.Count.ToString());
                    for (int i = 0; i < ec_MIS.Entities.Count; i++)
                    {
                        TracingSe.Trace("i: " + i.ToString());
                        decimal d_amp = 0;
                        bool f_checkIDagain = false;
                        // MISS has been paid
                        int f_paid = ec_MIS.Entities[i].Contains("statuscode") ? ((OptionSetValue)ec_MIS.Entities[i]["statuscode"]).Value : 1;
                        if (f_paid == 100000000)
                            throw new InvalidPluginExecutionException("Miscellaneous " + (string)ec_MIS.Entities[i]["bsd_name"] + " has been paid");
                        TracingSe.Trace("1");
                        TracingSe.Trace(((EntityReference)ec_MIS.Entities[i]["bsd_installment"]).LogicalName);
                        TracingSe.Trace(((EntityReference)ec_MIS.Entities[i]["bsd_installment"]).Id.ToString());
                        Entity en_Ins_Mis = serv.Retrieve(((EntityReference)ec_MIS.Entities[i]["bsd_installment"]).LogicalName, ((EntityReference)ec_MIS.Entities[i]["bsd_installment"]).Id,
                            new ColumnSet(true));

                        int i_Ins_status = en_Ins_Mis.Contains("statuscode") ? ((OptionSetValue)en_Ins_Mis["statuscode"]).Value : 100000000; // not paid
                                                                                                                                             //if (i_Ins_status == 100000000) throw new InvalidPluginExecutionException("Installment " + (string)en_Ins_Mis["bsd_name"] + " has not been paid, cannot payment for Miscellaneous " + (string)ec_MIS.Entities[i]["bsd_name"] + "!");
                        TracingSe.Trace("2");
                        TracingSe.Trace("s_Miss_id: " + string.Join(",", s_Miss_id));
                        TracingSe.Trace("Miss_id: " + ec_MIS.Entities[i].Id.ToString());
                        for (int m = 0; m < s_Miss_id.Length; m++)
                        {

                            if (ec_MIS.Entities[i].Id.ToString() == s_Miss_id[m])
                            {
                                d_amp = decimal.Parse(s_Miss_AM[m]);
                                f_checkIDagain = true;
                                break;
                            }
                        }
                        TracingSe.Trace("3");
                        if (f_checkIDagain == false) throw new InvalidPluginExecutionException("Cannot find ID of Miscellaneous " + (string)ec_MIS.Entities[i]["bsd_name"] + "' in Miscellaneous array!");
                        decimal d_MI_paid = ec_MIS.Entities[i].Contains("bsd_paidamount") ? ((Money)ec_MIS.Entities[i]["bsd_paidamount"]).Value : 0;
                        decimal d_MI_balance = ec_MIS.Entities[i].Contains("bsd_balance") ? ((Money)ec_MIS.Entities[i]["bsd_balance"]).Value : 0;
                        decimal d_MI_am = ec_MIS.Entities[i].Contains("bsd_totalamount") ? ((Money)ec_MIS.Entities[i]["bsd_totalamount"]).Value : 0;
                        TracingSe.Trace("4");
                        if (d_amp == d_MI_balance)
                        {
                            f_paid = 100000000;
                            d_MI_balance = 0;
                            d_MI_paid += d_amp;
                            tiendu -= d_amp;
                            createTransPM_MIS(paymentEn, optionentryEn, 100000003, en_Ins_Mis.Id, d_amp, d_now, serv, (string)product["name"], ec_MIS.Entities[i]);

                        } // end d_amp == d_MI_balance
                        else if (d_amp < d_MI_balance)
                        {
                            f_paid = 1;
                            d_MI_balance -= d_amp;
                            d_MI_paid += d_amp;
                            tiendu -= d_amp;
                            createTransPM_MIS(paymentEn, optionentryEn, 100000003, en_Ins_Mis.Id, d_amp, d_now, serv, (string)product["name"], ec_MIS.Entities[i]);
                        } // end d_amp < d_MI_balance

                        else throw new InvalidPluginExecutionException("Input amount is larger than balance amount of Miscellaneous " + (string)ec_MIS.Entities[i]["bsd_name"]);
                        // update
                        Entity en_up_MIS = new Entity(ec_MIS.Entities[i].LogicalName);
                        en_up_MIS.Id = ec_MIS.Entities[i].Id;
                        TracingSe.Trace("d_MI_paid:" + d_MI_paid.ToString());
                        TracingSe.Trace("d_MI_balance:" + d_MI_balance.ToString());
                        TracingSe.Trace("f_paid:" + f_paid.ToString());
                        en_up_MIS["bsd_paidamount"] = new Money(d_MI_paid);
                        en_up_MIS["bsd_balance"] = new Money(d_MI_balance);
                        en_up_MIS["statuscode"] = new OptionSetValue(f_paid);

                        service.Update(en_up_MIS);
                        TracingSe.Trace("i: ssssssssssss");

                    }// end for

                } // end if s_miss !=""

                #endregion ------------------- end MISS ----------------

            } // end if tiendu > 0
            //decimal d_bsd_assignamount = paymentEn.Contains("bsd_assignamount") ? ((Money)paymentEn["bsd_assignamount"]).Value : 0;
            if (tiendu > 0)
            {
                //throw new InvalidPluginExecutionException("ADV case 6");
                DateTime pm_ReceiptDate = RetrieveLocalTimeFromUTCTime((DateTime)paymentEn["bsd_paymentactualtime"]);
                //create_AdvPM(optionentryEn, customerRef, paymentEn, tiendu, (EntityReference)optionentryEn["bsd_project"], pm_ReceiptDate);

                create_AdvPM(optionentryEn, customerRef, paymentEn, tiendu, (EntityReference)optionentryEn["bsd_project"], pm_ReceiptDate);

            }
        }
        public void createTransPM_MIS(Entity enPayment, Entity optionEntry, int i_transactionType, Guid pmsdtlID, decimal amount, DateTime d_now, IOrganizationService serv,
            string producName, Entity en_MIS)
        {
            // transaction type :
            // Installments	   100000000
            // Interest        100000001
            // Fees            100000002
            // Other           100000003
            Entity en_pro = serv.Retrieve(((EntityReference)optionEntry["bsd_project"]).LogicalName, ((EntityReference)optionEntry["bsd_project"]).Id,
                new ColumnSet(new string[] { "bsd_name" }));

            Entity en_TransPM = new Entity("bsd_transactionpayment");
            en_TransPM["bsd_name"] = (string)en_pro["bsd_name"] + "-" + producName + "-" + (string)optionEntry["name"] + "-" + (string)enPayment["bsd_name"];
            en_TransPM["statuscode"] = new OptionSetValue(100000000); // confirm
            en_TransPM["bsd_payment"] = enPayment.ToEntityReference();
            en_TransPM["bsd_transactiontype"] = new OptionSetValue(i_transactionType);

            en_TransPM["bsd_installment"] = new EntityReference("bsd_paymentschemedetail", pmsdtlID);
            en_TransPM["bsd_amount"] = new Money(amount);
            en_TransPM["createdon"] = d_now;
            en_TransPM["bsd_miscellaneous"] = en_MIS.ToEntityReference();

            serv.Create(en_TransPM);

        }
        public EntityCollection get_ecINS(IOrganizationService crmservices, string[] s_id)
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
                <attribute name='bsd_waiverinstallment' />
                <attribute name='bsd_waiverinterest' />
                <attribute name='bsd_lastinstallment' />
                <attribute name='bsd_duedatecalculatingmethod' />
                <filter type='and'>
                  <condition attribute='bsd_paymentschemedetailid' operator='in'>";
            for (int i = 0; i < s_id.Length; i++)
            {
                fetchXml += @"<value>" + Guid.Parse(s_id[i]) + "</value>";
            }

            fetchXml += @"</condition>
                            </filter>
                            <order attribute='bsd_ordernumber'/>
                          </entity>
                        </fetch>";

            EntityCollection entc = crmservices.RetrieveMultiple(new FetchExpression(fetchXml));
            return entc;
        }
        public EntityCollection get_ecMIS(IOrganizationService crmservices, string[] s_id, string oeID)
        {
            info_Error info = new info_Error();
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
                    <condition attribute='bsd_miscellaneousid' operator='in' >";
            for (int i = 0; i < s_id.Length; i++)
            {
                fetchXml += @"<value>" + Guid.Parse(s_id[i]) + "</value>";
            }

            fetchXml += @"</condition>
                            </filter>                           
                          </entity>
                        </fetch>";
            fetchXml = string.Format(fetchXml, oeID, s_id);
            TracingSe.Trace(fetchXml);
            EntityCollection entc = crmservices.RetrieveMultiple(new FetchExpression(fetchXml));
            return entc;
        }
        public void interestProcess(Entity paymentEn)
        {
            decimal pm_amountpay = paymentEn.Contains("bsd_amountpay") ? ((Money)paymentEn["bsd_amountpay"]).Value : 0;
            decimal pm_sotiendot = paymentEn.Contains("bsd_totalamountpayablephase") ? ((Money)paymentEn["bsd_totalamountpayablephase"]).Value : 0;
            decimal pm_sotiendatra = paymentEn.Contains("bsd_totalamountpaidphase") ? ((Money)paymentEn["bsd_totalamountpaidphase"]).Value : 0;
            decimal pm_balance = paymentEn.Contains("bsd_balance") ? ((Money)paymentEn["bsd_balance"]).Value : 0;
            decimal pm_differentamount = paymentEn.Contains("bsd_differentamount") ? ((Money)paymentEn["bsd_differentamount"]).Value : 0;
            decimal pm_deposit = paymentEn.Contains("bsd_depositamount") ? ((Money)paymentEn["bsd_depositamount"]).Value : 0;
            bool f_bsd_latepayment = paymentEn.Contains("bsd_latepayment") ? (bool)paymentEn["bsd_latepayment"] : false;
            decimal pm_bsd_managementfee = paymentEn.Contains("bsd_managementfee") ? ((Money)paymentEn["bsd_managementfee"]).Value : 0;
            decimal pm_bsd_maintenancefee = paymentEn.Contains("bsd_maintenancefee") ? ((Money)paymentEn["bsd_maintenancefee"]).Value : 0;
            int pm_statuscode = paymentEn.Contains("statuscode") ? ((OptionSetValue)paymentEn["statuscode"]).Value : 0;

            string s_bsd_arraypsdid = paymentEn.Contains("bsd_arraypsdid") ? (string)paymentEn["bsd_arraypsdid"] : "";
            string s_bsd_arrayamountpay = paymentEn.Contains("bsd_arrayamountpay") ? (string)paymentEn["bsd_arrayamountpay"] : "";

            string s_bsd_arrayinstallmentinterest = paymentEn.Contains("bsd_arrayinstallmentinterest") ? (string)paymentEn["bsd_arrayinstallmentinterest"] : "";
            string s_bsd_arrayinterestamount = paymentEn.Contains("bsd_arrayinterestamount") ? (string)paymentEn["bsd_arrayinterestamount"] : "";

            string s_bsd_arrayfees = paymentEn.Contains("bsd_arrayfees") ? (string)paymentEn["bsd_arrayfees"] : "";
            string s_bsd_arrayfeesamount = paymentEn.Contains("bsd_arrayfeesamount") ? (string)paymentEn["bsd_arrayfeesamount"] : "";

            string s_bsd_arraymicellaneousid = paymentEn.Contains("bsd_arraymicellaneousid") ? (string)paymentEn["bsd_arraymicellaneousid"] : "";
            string s_bsd_arraymicellaneousamount = paymentEn.Contains("bsd_arraymicellaneousamount") ? (string)paymentEn["bsd_arraymicellaneousamount"] : "";

            decimal d_bsd_totalapplyamount = paymentEn.Contains("bsd_totalapplyamount") ? ((Money)paymentEn["bsd_totalapplyamount"]).Value : 0;
            decimal d_bsd_assignamount = paymentEn.Contains("bsd_assignamount") ? ((Money)paymentEn["bsd_assignamount"]).Value : 0;

            Entity optionentryEn = service.Retrieve("salesorder", ((EntityReference)paymentEn["bsd_optionentry"]).Id,
                            new ColumnSet(true));
            string optionentryID = optionentryEn.Id.ToString();
            EntityReference customerRef = (EntityReference)optionentryEn["customerid"];
            if (!optionentryEn.Contains("bsd_unitnumber")) throw new InvalidPluginExecutionException("Cannot find Unit information in Option Entry " + (string)optionentryEn["name"] + "!");
            decimal d_oe_bsd_totalamount = optionentryEn.Contains("totalamount") ? ((Money)optionentryEn["totalamount"]).Value : 0;
            decimal d_oe_bsd_totalamountpaid = optionentryEn.Contains("bsd_totalamountpaid") ? ((Money)optionentryEn["bsd_totalamountpaid"]).Value : 0;
            decimal d_oe_totalpercent = optionentryEn.Contains("bsd_totalpercent") ? (decimal)optionentryEn["bsd_totalpercent"] : 0;
            decimal d_oe_bsd_totalamountlessfreight = optionentryEn.Contains("bsd_totalamountlessfreight") ? ((Money)optionentryEn["bsd_totalamountlessfreight"]).Value : 0;
            if (d_oe_bsd_totalamountlessfreight == 0) throw new InvalidPluginExecutionException("'Net Selling Price' must be larger than 0");

            decimal d_oe_bsd_managementfee = optionentryEn.Contains("bsd_managementfee") ? ((Money)optionentryEn["bsd_managementfee"]).Value : 0;
            decimal d_oe_bsd_freightamount = optionentryEn.Contains("bsd_freightamount") ? ((Money)optionentryEn["bsd_freightamount"]).Value : 0;
            decimal d_oe_amountCalcPercent = d_oe_bsd_totalamount - d_oe_bsd_freightamount;

            Entity Product = service.Retrieve("product", ((EntityReference)optionentryEn["bsd_unitnumber"]).Id, new ColumnSet(true));
            DateTime d_now = RetrieveLocalTimeFromUTCTime(DateTime.Now);
            DateTime pm_ReceiptDate = d_now;
            //Tạo reciep cho từng lãi xuat
            decimal tiendu = pm_amountpay;

            transactionPM_INS(service, s_bsd_arraypsdid, s_bsd_arrayamountpay, s_bsd_arrayinstallmentinterest, s_bsd_arrayinterestamount,
                                            s_bsd_arrayfees, s_bsd_arrayfeesamount, tiendu, ref d_oe_bsd_totalamountpaid, paymentEn, optionentryEn, Product, pm_ReceiptDate, customerRef,
                                            s_bsd_arraymicellaneousid, s_bsd_arraymicellaneousamount);
        }
        public EntityCollection get_resv_Quotation(IOrganizationService crmservices, Guid uID)
        {
            string fetchXml =
              @"<fetch version='1.0' output-format='xml-platform' mapping='logical' distinct='true' >
                 <entity name='quote' >
                <attribute name='quoteid' />
                <attribute name='statuscode' />
                <attribute name='bsd_unitno' />
                <filter type='and' >
                  <condition attribute='bsd_unitno' operator='eq' value='{0}' />
                  <condition attribute='statuscode' operator='eq' value='100000007' />
                </filter>
              </entity>
             </fetch>";

            fetchXml = string.Format(fetchXml, uID);
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
    public class Installment
    {
        public DateTime InterestStarDate { get; set; }
        public int Intereststartdatetype { get; set; }
        public int Gracedays { get; set; }
        public int LateDays { get; set; }
        public decimal MaxPercent { get; set; }
        public decimal MaxAmount { get; set; }
        public decimal InterestPercent { get; set; }
        public decimal InterestCharge { get; set; }
        public DateTime Duedate { get; set; }
    }
}
