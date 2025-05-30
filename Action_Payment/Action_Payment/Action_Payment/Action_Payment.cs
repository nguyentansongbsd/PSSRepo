using System;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Query;
using System.Text;
using Microsoft.Crm.Sdk.Messages;
using System.Reflection.Emit;

namespace Action_Payment
{
    public class Action_Payment : IPlugin
    {
        decimal pm_balancemaster = 0;
        decimal d_bsd_assignamountmaster = 0;
        decimal pm_differentamountmaster = 0;
        IOrganizationService service = null;
        IOrganizationServiceFactory factory = null;
        IPluginExecutionContext context = null;
        ITracingService TracingSe = null;

        void IPlugin.Execute(IServiceProvider serviceProvider)
        {
            context = (IPluginExecutionContext)serviceProvider.GetService(typeof(IPluginExecutionContext));
            EntityReference target = (EntityReference)context.InputParameters["Target"];
            factory = (IOrganizationServiceFactory)serviceProvider.GetService(typeof(IOrganizationServiceFactory));
            service = factory.CreateOrganizationService(context.UserId);
            TracingSe = (ITracingService)serviceProvider.GetService(typeof(ITracingService));
            //try
            //{
            if (target.LogicalName == "bsd_payment")
            {
                TracingSe.Trace("vào 111111111111111");
                DateTime d_now = RetrieveLocalTimeFromUTCTime(DateTime.Now);
                DateTime pm_ReceiptDate = d_now;
                DateTime i_intereststartdate = d_now;
                TracingSe.Trace("vào action");
                // --- Retrieve Payment ---
                Entity paymentEn = service.Retrieve(target.LogicalName, target.Id, new ColumnSet(true));
                if (((OptionSetValue)paymentEn["statuscode"]).Value == 100000000)  // payment = paid
                    throw new InvalidPluginExecutionException("This payment has been paid!");
                var fetchXml = $@"<?xml version=""1.0"" encoding=""utf-16""?>
                <fetch>
                  <entity name=""bsd_confirmpayment"">
                    <attribute name=""bsd_confirmpaymentid"" />
                    <filter>
                      <condition attribute=""statuscode"" operator=""eq"" value=""{1}"" />
                    </filter>
                    <link-entity name=""bsd_bsd_confirmpayment_bsd_payment"" from=""bsd_confirmpaymentid"" to=""bsd_confirmpaymentid"" intersect=""true"">
                      <filter>
                        <condition attribute=""bsd_paymentid"" operator=""eq"" value=""{target.Id}"" />
                      </filter>
                    </link-entity>
                  </entity>
                </fetch>";
                EntityCollection rs99 = service.RetrieveMultiple(new FetchExpression(fetchXml));
                if (rs99.Entities.Count > 0) throw new InvalidPluginExecutionException("Payment is in the process of confirmation. Cannot it process step.");
                if (!paymentEn.Contains("bsd_paymenttype"))
                    throw new InvalidPluginExecutionException(string.Format("Please select payment type!"));
                if (!paymentEn.Contains("bsd_amountpay"))
                    throw new InvalidPluginExecutionException(string.Format("Please input amount pay!"));
                if (!paymentEn.Contains("bsd_paymentactualtime"))
                    throw new InvalidPluginExecutionException("Please select payment actual time");
                else
                    pm_ReceiptDate = RetrieveLocalTimeFromUTCTime((DateTime)paymentEn["bsd_paymentactualtime"]);

                int type = ((OptionSetValue)paymentEn["bsd_paymenttype"]).Value;
                decimal pm_amountpay = paymentEn.Contains("bsd_amountpay") ? ((Money)paymentEn["bsd_amountpay"]).Value : 0;
                decimal pm_sotiendot = paymentEn.Contains("bsd_totalamountpayablephase") ? ((Money)paymentEn["bsd_totalamountpayablephase"]).Value : 0;
                decimal pm_sotiendatra = paymentEn.Contains("bsd_totalamountpaidphase") ? ((Money)paymentEn["bsd_totalamountpaidphase"]).Value : 0;
                pm_balancemaster = paymentEn.Contains("bsd_balance") ? ((Money)paymentEn["bsd_balance"]).Value : 0;
                pm_differentamountmaster = paymentEn.Contains("bsd_differentamount") ? ((Money)paymentEn["bsd_differentamount"]).Value : 0;
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
                d_bsd_assignamountmaster = paymentEn.Contains("bsd_assignamount") ? ((Money)paymentEn["bsd_assignamount"]).Value : 0;
                if (pm_statuscode == 100000000) throw new InvalidPluginExecutionException("This payment has been paid!");

                TracingSe.Trace("type: " + type.ToString());
                if (pm_statuscode == 1)  // payment = active
                {
                    if (type != 100000004)
                        if (pm_amountpay <= 0) throw new InvalidPluginExecutionException("Amount pay must larger than 0!");
                    if (type != 100000005)
                        if (pm_amountpay <= 0) throw new InvalidPluginExecutionException("Amount pay must larger than 0!");
                    if (d_bsd_assignamountmaster < 0)
                    {
                        throw new InvalidPluginExecutionException("The amount you pay exceeds the amount paid. Please choose another Advance Payment or other Payment Phase!");
                    }
                    decimal d_inter = 0;
                    int i_lateday = 0;
                    Payment payment = new Payment(serviceProvider);

                    //>>> Test trace =======??????????.
                    info_Error info = new info_Error();
                    TracingSe.Trace(string.Format("Type: {0}", type));
                    //throw new Exception(info.message);

                    switch (type)
                    {
                        case 100000000://Queuing fee
                            break;
                        case 100000001://Reservation - Deposit
                            TracingSe.Trace("Reservation - Deposit");
                            payment.deposit(paymentEn); // check payment contain field fees
                            break;
                        case 100000002://Installment
                            TracingSe.Trace("Installment");
                            checkPmsDtl(paymentEn); // check payment contain field fees
                            checkPaidInstalment(paymentEn); // check đớt trước đã thanh toán paid chưa
                            TracingSe.Trace("checkPmsDtl complete");
                            payment.intallment(paymentEn, ref pm_balancemaster, ref d_bsd_assignamountmaster, ref pm_differentamountmaster);
                            break;
                        case 100000003://Interest Charge
                            TracingSe.Trace("Interest Charge");
                            checkPmsDtl(paymentEn); // check payment contain field fees
                            TracingSe.Trace("checkPmsDtl complete");
                            payment.interestCharge(paymentEn);
                            break;
                        case 100000004://Fees
                            payment.fees(paymentEn);

                            break;
                        case 100000005://Other
                            checkPmsDtl(paymentEn); // check payment contain field fees
                            TracingSe.Trace("checkPmsDtl complete");
                            payment.miscellaneous(paymentEn);
                            break;
                    }
                    TracingSe.Trace("16 Update payment");
                    if (type == 100000002)
                    {
                        if (paymentEn.Contains("bsd_optionentry"))
                        {
                            pm_differentamountmaster = pm_amountpay - pm_balancemaster;
                        }
                    }
                    payment.update(paymentEn, pm_amountpay, pm_sotiendot, pm_sotiendatra, d_now, pm_balancemaster, pm_differentamountmaster, d_inter, i_lateday, type, d_bsd_assignamountmaster);
                    TracingSe.Trace("18");
                    //service.Update(paymentEn);
                    TracingSe.Trace("19 End Update payment");
                    // -------------------- 1st Installment & Signcontract & OE ---------------------
                    Entity PaymentDetailEn = new Entity("bsd_paymentschemedetail");
                    if (paymentEn.Contains("bsd_paymentschemedetail"))
                    {
                        PaymentDetailEn = service.Retrieve("bsd_paymentschemedetail", ((EntityReference)paymentEn["bsd_paymentschemedetail"]).Id, new ColumnSet(true));
                    }
                    int psd_statuscodeInterest = PaymentDetailEn.Contains("bsd_interestchargestatus") ? ((OptionSetValue)PaymentDetailEn["bsd_interestchargestatus"]).Value : 100000000;//check interest đã thanh toán chưa
                    bool psd_statuscodeFeeMain = PaymentDetailEn.Contains("bsd_maintenancefeesstatus") ? ((bool)PaymentDetailEn["bsd_maintenancefeesstatus"]) : false;//check fee đã thanh toán chưa
                    bool psd_statuscodeFeeMana = PaymentDetailEn.Contains("bsd_managementfeesstatus") ? ((bool)PaymentDetailEn["bsd_managementfeesstatus"]) : false;//check fee đã thanh toán chưa
                    int psd_statuscode = PaymentDetailEn.Contains("statuscode") ? ((OptionSetValue)PaymentDetailEn["statuscode"]).Value : 100000000;
                    int phaseNum = PaymentDetailEn.Contains("bsd_ordernumber") ? (int)PaymentDetailEn["bsd_ordernumber"] : 0;

                    TracingSe.Trace("20");
                    Entity optionentryEn = paymentEn.Contains("bsd_optionentry") ? service.Retrieve("salesorder", ((EntityReference)paymentEn["bsd_optionentry"]).Id, new ColumnSet(true)) : null;
                    if (optionentryEn != null)
                    {
                        TracingSe.Trace("21");
                        var enmis = get_All_MIS_NotPaid(optionentryEn.Id.ToString());//dùng để kiểm tra xem có misc nào chưa thanh toán hay không
                        string optionentryID = optionentryEn.Id.ToString();
                        EntityReference customerRef = (EntityReference)optionentryEn["customerid"];
                        if (!optionentryEn.Contains("bsd_unitnumber")) throw new InvalidPluginExecutionException("Cannot find Unit information in Option Entry " + (string)optionentryEn["name"] + "!");
                        decimal d_oe_bsd_totalamount = optionentryEn.Contains("totalamount") ? ((Money)optionentryEn["totalamount"]).Value : 0;
                        decimal d_oe_bsd_totalamountpaid = optionentryEn.Contains("bsd_totalamountpaid") ? ((Money)optionentryEn["bsd_totalamountpaid"]).Value : 0;
                        decimal d_oe_bsd_totalamountlessfreight = optionentryEn.Contains("bsd_totalamountlessfreight") ? ((Money)optionentryEn["bsd_totalamountlessfreight"]).Value : 0;
                        if (d_oe_bsd_totalamountlessfreight == 0) throw new InvalidPluginExecutionException("'Net Selling Price' must be larger than 0");

                        decimal d_oe_bsd_freightamount = optionentryEn.Contains("bsd_freightamount") ? ((Money)optionentryEn["bsd_freightamount"]).Value : 0;


                        EntityCollection psdFirst = GetPSD(optionentryID);
                        Entity detailFirst = psdFirst.Entities[0];
                        //////  string detailFirstID = detailFirst.Id.ToString();

                        int t = psdFirst.Entities.Count;
                        Entity detailLast = psdFirst.Entities[t - 1]; // entity cuoi cung ( phase cuoi cung )
                        string detailLastID = detailLast.Id.ToString();

                        int sttOE = 100000001; // statuscode of OE= 1st installment
                        int sttUnit = 100000001; // statuscode of unit= 1st installment
                        Entity Unit = service.Retrieve("product", ((EntityReference)optionentryEn["bsd_unitnumber"]).Id, new ColumnSet(true));
                        TracingSe.Trace("22");
                        if (phaseNum == 1)
                        {
                            TracingSe.Trace("23");
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
                            TracingSe.Trace("24");
                            if (!optionentryEn.Contains("bsd_signedcontractdate"))
                            {
                                sttOE = 100000001; // if OE not signcontract - status code still is 1st Installment
                                sttUnit = 100000001; // 1st
                            }
                            else
                            {
                                sttUnit = 100000002;
                                sttOE = 100000003; //Being Payment (khi da sign contract)

                                if ((detailLastID == PaymentDetailEn.Id.ToString()) && psd_statuscode == 100000001 && psd_statuscodeInterest == 100000001 && psd_statuscodeFeeMain &&
                                        psd_statuscodeFeeMana && enmis != null && enmis.Entities.Count == 0)
                                    sttOE = 100000004; //Complete Payment
                            }
                        }
                        TracingSe.Trace("25");
                        Unit["statuscode"] = new OptionSetValue(sttUnit); // Unit statuscode = 1st Installment
                        service.Update(Unit);
                        Entity oe_tmp = new Entity(optionentryEn.LogicalName);
                        oe_tmp.Id = optionentryEn.Id;
                        oe_tmp["bsd_unitstatus"] = new OptionSetValue(sttUnit);
                        oe_tmp["statuscode"] = new OptionSetValue(sttOE);
                        oe_tmp["bsd_totalamountpaid"] = new Money(d_oe_bsd_totalamountpaid);
                        //oe_tmp["bsd_totalpercent"] = (d_oe_bsd_totalamountpaid / d_oe_amountCalcPercent) * 100;
                        service.Update(oe_tmp);
                        // -------------------- check FUL - OE -------------------------------
                        // check if FUL exist Unit in this PM
                        EntityCollection ec_FUL = payment.get_ecFUL(service, ((EntityReference)optionentryEn["bsd_unitnumber"]).Id);
                        TracingSe.Trace("8");

                        if (ec_FUL.Entities.Count > 0)
                        {
                            // check type of FUL is OE or RESV
                            int i_FULtype = ec_FUL.Entities[0].Contains("bsd_type") ? ((OptionSetValue)ec_FUL.Entities[0]["bsd_type"]).Value : 0;
                            TracingSe.Trace("9");
                            if (i_FULtype == 100000004)
                            {

                                // check INS trong FUL trung voi INS trong PM - neu INS trong PM va INS trong FUL trung thi moi execute
                                if (((EntityReference)ec_FUL.Entities[0]["bsd_installment"]).Id == PaymentDetailEn.Id)
                                {
                                    // update field FUL trong OE thanh No
                                    Entity en_OE_FUL = new Entity(optionentryEn.LogicalName);
                                    en_OE_FUL.Id = optionentryEn.Id;
                                    en_OE_FUL["bsd_followuplist"] = false;
                                    service.Update(en_OE_FUL);

                                    Entity en_FUL_up = new Entity(ec_FUL.Entities[0].LogicalName);
                                    en_FUL_up.Id = ec_FUL.Entities[0].Id;
                                    en_FUL_up["statuscode"] = new OptionSetValue(100000000);
                                    string s_dNow = d_now.Day.ToString() + "/" + d_now.Month.ToString() + "/" + d_now.Year.ToString();
                                    en_FUL_up["bsd_fincomment"] = "Customers have paid the full amount of the payment in " + s_dNow;
                                    service.Update(en_FUL_up);
                                }
                            }
                            TracingSe.Trace("10");
                            // check total late day of all INS compair with termination date and total late day on PMScheme ( if pass condition - create FUL )
                            // -----------------------check next INS ----------------------------------
                            Entity en_PMS = service.Retrieve(((EntityReference)optionentryEn["bsd_paymentscheme"]).LogicalName, ((EntityReference)optionentryEn["bsd_paymentscheme"]).Id,
                               new ColumnSet(true));
                            int i_bsd_terminationdate = en_PMS.Contains("bsd_terminationdate") ? (int)en_PMS["bsd_terminationdate"] : 0;
                            int i_bsd_latedaysforeachinstallment = en_PMS.Contains("bsd_latedaysforeachinstallment") ? (int)en_PMS["bsd_latedaysforeachinstallment"] : 0;

                            //  ------------------------ check next INS over duedate -------------------------------
                            // get next Ins - if next INS have duedate and over duedate
                            Entity en_pro = service.Retrieve(((EntityReference)optionentryEn["bsd_project"]).LogicalName, ((EntityReference)optionentryEn["bsd_project"]).Id,
                                                 new ColumnSet(true));
                            TracingSe.Trace("11");
                            TracingSe.Trace("bsd_lastinstallment1: " + PaymentDetailEn.Contains("bsd_lastinstallment").ToString());

                            bool islastinstallment = PaymentDetailEn.Contains("bsd_lastinstallment") ? (bool)PaymentDetailEn["bsd_lastinstallment"] : false;
                            //if (!PaymentDetailEn.Contains("bsd_lastinstallment") && (bool)PaymentDetailEn["bsd_lastinstallment"] == false)
                            if (islastinstallment == false && PaymentDetailEn.Contains("bsd_name"))
                            {
                                TracingSe.Trace("11.1");
                                int i_nextOrderNumber = phaseNum + 1;
                                TracingSe.Trace("11.2");
                                TracingSe.Trace("optionentryEn: " + optionentryEn.Id.ToString());
                                //thanhdovan
                                EntityCollection ec_nextINS = get_nextINS(service, i_nextOrderNumber, optionentryEn.Id);
                                TracingSe.Trace("11.3");
                                if (ec_nextINS.Entities.Count > 0)
                                {
                                    TracingSe.Trace("11.4");
                                    if (ec_nextINS.Entities[0].Contains("bsd_duedate"))
                                    {
                                        TracingSe.Trace("11.5");
                                        DateTime d_Ins_duedate = new DateTime();
                                        d_Ins_duedate = (DateTime)ec_nextINS.Entities[0]["bsd_duedate"];
                                        int i_late = (int)DateTime.Now.Date.Subtract(d_Ins_duedate.Date).TotalDays;
                                        TracingSe.Trace("11.6");
                                        TracingSe.Trace("PaymentDetailEn: " + PaymentDetailEn.Id.ToString());
                                        if (i_late > i_bsd_latedaysforeachinstallment)
                                        {
                                            TracingSe.Trace("11.7");
                                            string s_proCode = en_pro.Contains("bsd_projectcode") ? (string)en_pro["bsd_projectcode"] : "";
                                            if (check_Ins_FUL(service, optionentryEn.Id, 100000004, PaymentDetailEn.ToEntityReference()) == false)
                                            {
                                                TracingSe.Trace("11.8");
                                                create_FUL_Installment(optionentryEn, 100000004, (EntityReference)optionentryEn["bsd_unitnumber"], PaymentDetailEn, s_proCode, service, d_now, en_pro.ToEntityReference());
                                                TracingSe.Trace("11.8.1");
                                                Entity e_tmpOE = new Entity(optionentryEn.LogicalName);
                                                TracingSe.Trace("11.8.2");
                                                e_tmpOE.Id = optionentryEn.Id;
                                                TracingSe.Trace("11.8.3");
                                                e_tmpOE["bsd_followuplist"] = true;
                                                TracingSe.Trace("11.9");
                                                service.Update(e_tmpOE);
                                            }

                                        }
                                    }
                                } //end of  if (ec_nextINS.Entities.Count > 0)
                            } // end of if (!PaymentDetailEn.Contains("bsd_lastinstallment") && (bool)PaymentDetailEn["bsd_lastinstallment"] == false)
                            TracingSe.Trace("12");
                            // ---------------- end check next INS over duedate --------------------------------

                            // check total late day
                            // ----------------------- check total late day of all INS of OE ------------------
                            EntityCollection ec_InsOE = get_ecIns_OE(service, optionentryEn.Id);
                            int i_sumLate = 0;
                            int i_latedayEachIns = 0;
                            if (ec_InsOE.Entities.Count > 0)
                            {

                                for (int i = 0; i < ec_InsOE.Entities.Count; i++)
                                {
                                    if (!ec_InsOE.Entities[i].Contains("statuscode")) break;
                                    else
                                    {
                                        if (((OptionSetValue)ec_InsOE.Entities[i]["statuscode"]).Value == 100000001) // paid
                                        {
                                            i_sumLate += ec_InsOE.Entities[i].Contains("bsd_actualgracedays") ? (int)ec_InsOE.Entities[i]["bsd_actualgracedays"] : 0;
                                        }
                                        else // not paid
                                        {
                                            // tih actual late day of pmsdtl
                                            if (!ec_InsOE.Entities[i].Contains("bsd_duedate")) continue;
                                            DateTime dt_bsd_duedate = (DateTime)ec_InsOE.Entities[i]["bsd_duedate"];
                                            if (dt_bsd_duedate < DateTime.Now)
                                                i_latedayEachIns += (int)DateTime.Now.Date.Subtract(dt_bsd_duedate.Date).TotalDays;
                                            //i_latedayEachIns += DateTime.Now.Day- dt_bsd_duedate.Day;
                                        }
                                    }
                                }

                            }
                            TracingSe.Trace("13");
                            /// create FUL when sumlate day > late day of each INS
                            i_sumLate += i_latedayEachIns;
                            // tong so ngay tre all INS so sanh voi so ngya quy dinh tren PMScheme
                            if (i_sumLate >= i_bsd_terminationdate)
                            {
                                string s_proCode = en_pro.Contains("bsd_projectcode") ? (string)en_pro["bsd_projectcode"] : "";
                                if (check_ExistOEinFUL(service, optionentryEn.Id, ((EntityReference)optionentryEn["bsd_unitnumber"]).Id) == false)
                                {
                                    create_FUL(optionentryEn, 100000006, (EntityReference)optionentryEn["bsd_unitnumber"], 100000002, s_proCode, service, d_now); // FIN

                                    Entity e_tmpOE = new Entity(optionentryEn.LogicalName);
                                    e_tmpOE.Id = optionentryEn.Id;
                                    e_tmpOE["bsd_followuplist"] = true;
                                    service.Update(e_tmpOE);
                                }

                            }// end of if i_sum_Lateday >= totalLateday
                             // ------------------- end check total late day of all INS of OE --------------------
                            TracingSe.Trace("14");
                        }
                        //  ---------------- end check FUL --------------------------
                    }
                } // end if payment status = active    
            }
            // }
            // catch (Exception ex)
            // {
            //    TracingSe.Trace(ex.Message);
            //    throw new InvalidPluginExecutionException(strMess.ToString());
            //}
        }
        private void checkPaidInstalment(Entity paymentEn)
        {
            if (paymentEn.Contains("bsd_paymentschemedetail") && paymentEn.Contains("bsd_optionentry"))
            {
                Entity PaymentDetailEn = service.Retrieve("bsd_paymentschemedetail", ((EntityReference)paymentEn["bsd_paymentschemedetail"]).Id, new ColumnSet(true));
                int bsd_ordernumber = PaymentDetailEn.Contains("bsd_ordernumber") ? (int)PaymentDetailEn["bsd_ordernumber"] : 0;
                if (bsd_ordernumber > 0)
                {
                    var fetchXml = $@"<?xml version=""1.0"" encoding=""utf-16""?>
                    <fetch top=""1"">
                      <entity name=""bsd_paymentschemedetail"">
                        <attribute name=""bsd_paymentschemedetailid"" />
                        <attribute name=""bsd_ordernumber"" />
                        <filter>
                          <condition attribute=""bsd_optionentry"" operator=""eq"" value=""{((EntityReference)paymentEn["bsd_optionentry"]).Id}"" />
                          <condition attribute=""bsd_ordernumber"" operator=""lt"" value=""{bsd_ordernumber}"" />
                          <condition attribute=""statuscode"" operator=""eq"" value=""{100000000}"" />
                        </filter>
                      </entity>
                    </fetch>";
                    EntityCollection entc = service.RetrieveMultiple(new FetchExpression(fetchXml));
                    if (entc.Entities.Count > 0) throw new InvalidPluginExecutionException("Please make the payment for the previous installment.");
                }
            }
        }
        private void checkPmsDtl(Entity paymentEn)
        {
            int bsd_paymenttype = paymentEn.Contains("bsd_paymenttype") ? ((OptionSetValue)paymentEn["bsd_paymenttype"]).Value : 0;
            TracingSe.Trace("bsd_paymenttype: " + bsd_paymenttype.ToString());
            if (bsd_paymenttype != 100000003 && bsd_paymenttype != 100000005)
            {
                if (!paymentEn.Contains("bsd_paymentschemedetail"))
                {
                    throw new InvalidPluginExecutionException("Payment Scheme Detail cannot null!");
                }
                if (!paymentEn.Contains("bsd_totalamountpayablephase"))
                    throw new InvalidPluginExecutionException("Total amount payable (phase) must larger than 0!");
            }

            if (!paymentEn.Contains("bsd_optionentry"))
                throw new InvalidPluginExecutionException("Payment does not contain Option Entry information!");
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
        private EntityCollection get_nextINS(IOrganizationService crmservices, int i_order, Guid oeID)
        {
            string fetchXml =
               @"<fetch version='1.0' output-format='xml-platform' mapping='logical' distinct='true' >
                <entity name='bsd_paymentschemedetail' >
                    <attribute name='bsd_duedate' />
                    <attribute name='bsd_name' />
                    <attribute name='bsd_duedatecalculatingmethod' />
                    <attribute name='bsd_ordernumber' />
                    <attribute name='bsd_optionentry' />
                    <filter type='and' >
                      <condition attribute='bsd_optionentry' operator='eq' value='{0}' />
                      <condition attribute='bsd_ordernumber' operator='eq' value='{1}' />
                      <condition attribute='bsd_duedate' operator='not-null' />
                    </filter>
                  </entity>
            </fetch>";
            fetchXml = string.Format(fetchXml, oeID, i_order);
            TracingSe.Trace("fetchXml: " + fetchXml);
            EntityCollection entc = crmservices.RetrieveMultiple(new FetchExpression(fetchXml));
            return entc;
        }
        private bool check_Ins_FUL(IOrganizationService crmservices, Guid oeID, int i_type, EntityReference en_installment)
        {
            string fetchXml =
              @"<fetch version='1.0' output-format='xml-platform' mapping='logical' distinct='false'>
              <entity name='bsd_followuplist' >
                <attribute name='bsd_type' />
                <attribute name='bsd_optionentry' />
                <attribute name='statuscode' />
                <filter type='and' >
                  <condition attribute='bsd_optionentry' operator='eq' value='{0}' />
                  <condition attribute='bsd_type' operator='eq' value='{1}' />
                  <condition attribute='statuscode' operator='eq' value='1' />
                                    
                </filter>
              </entity>
            </fetch>";
            //<condition attribute='bsd_installment' operator='eq' value='{2}' />
            //fetchXml = string.Format(fetchXml, oeID, i_type, en_installment.Id);
            fetchXml = string.Format(fetchXml, oeID, i_type);
            EntityCollection entc = crmservices.RetrieveMultiple(new FetchExpression(fetchXml));
            if (entc.Entities.Count > 0)
                return true;
            return false;
        }
        private void create_FUL_Installment(Entity e_OE, int i_type, EntityReference en_Unit, Entity en_installment, string s_pro, IOrganizationService serv, DateTime d_date, EntityReference er_pro)
        {
            if (validateFUL(e_OE))
            {
                Entity en_U = serv.Retrieve(en_Unit.LogicalName, en_Unit.Id, new ColumnSet(new string[] { "name" }));
                Entity tmp = new Entity("bsd_followuplist");
                Entity customer = new Entity();
                TracingSe.Trace("en_installment: " + en_installment.Id.ToString());
                if (((EntityReference)e_OE["customerid"]).LogicalName == "contact")
                {
                    customer = service.Retrieve(((EntityReference)e_OE["customerid"]).LogicalName, ((EntityReference)e_OE["customerid"]).Id,
                   new ColumnSet(new string[] { "bsd_fullname", "fullname" }));
                    tmp["bsd_name"] = (string)en_U["name"] + "-" + s_pro + "-" + (string)e_OE["name"] + "-" + (customer.Contains("bsd_fullname") ? (string)customer["bsd_fullname"] : (string)customer["fullname"]) + "-FIN";
                }
                else
                {
                    customer = service.Retrieve(((EntityReference)e_OE["customerid"]).LogicalName, ((EntityReference)e_OE["customerid"]).Id,
                    new ColumnSet(new string[] { "bsd_name", "name" }));
                    tmp["bsd_name"] = (string)en_U["name"] + "-" + s_pro + "-" + (string)e_OE["name"] + "-" + (customer.Contains("bsd_name") ? (string)customer["bsd_name"] : (string)customer["name"]) + "-FIN";
                }

                // tmp["bsd_name"] = (string)en_U["name"]+ "-"+ s_pro + "-" +(string)e_OE["name"]+"-"+ (string)e_OE["customerid"] +"-FIN";

                tmp["bsd_date"] = DateTime.Now;
                tmp["bsd_group"] = new OptionSetValue(100000002); // FIN
                tmp["bsd_type"] = new OptionSetValue(i_type); // type = Optionentry_contract
                tmp["bsd_optionentry"] = e_OE.ToEntityReference();
                tmp["bsd_units"] = (EntityReference)en_Unit;

                tmp["bsd_installment"] = en_installment.ToEntityReference();
                tmp["bsd_sellingprice"] = e_OE["totalamount"];
                tmp["bsd_project"] = er_pro;
                tmp["bsd_expiredate"] = d_date;
                tmp["bsd_owneroptionreservation"] = (EntityReference)e_OE["ownerid"];
                TracingSe.Trace("create_FUL_Installment");
                service.Create(tmp);
            }
            else
            {
                throw new InvalidPluginExecutionException("The unit you have selected already has a Follow up list. Please check the Follow up list of Unit to continue processing.");
            }

        }
        private bool validateFUL(Entity enOptionEntry)
        {
            //Hồ Code 28-05-2019
            // Define Condition Values
            var QEbsd_followuplist_bsd_optionentry = enOptionEntry.Id;
            var QEbsd_followuplist_bsd_type = 100000006;//Option Entry - Terminate
            // Instantiate QueryExpression QEbsd_followuplist
            var QEbsd_followuplist = new QueryExpression("bsd_followuplist");
            var QEbsd_followuplist_statecode = 0;//Active

            // Add all columns to QEbsd_followuplist.ColumnSet
            QEbsd_followuplist.ColumnSet.AllColumns = true;

            // Define filter QEbsd_followuplist.Criteria
            QEbsd_followuplist.Criteria.AddCondition("bsd_optionentry", ConditionOperator.Equal, QEbsd_followuplist_bsd_optionentry);
            QEbsd_followuplist.Criteria.AddCondition("bsd_type", ConditionOperator.Equal, QEbsd_followuplist_bsd_type);
            QEbsd_followuplist.Criteria.AddCondition("statecode", ConditionOperator.Equal, QEbsd_followuplist_statecode);
            EntityCollection encolFollowUpList = service.RetrieveMultiple(QEbsd_followuplist);
            if (encolFollowUpList.Entities.Count > 0)
                return false;
            return true;
        }
        private EntityCollection get_ecIns_OE(IOrganizationService crmservices, Guid oeID)
        {
            string fetchXml =
              @"<fetch version='1.0' output-format='xml-platform' mapping='logical' distinct='false'>
              <entity name='bsd_paymentschemedetail' >
                <attribute name='bsd_name' />
                <attribute name='bsd_duedate' />
                <attribute name='bsd_ordernumber' />
                <attribute name='bsd_actualgracedays' />
                <attribute name='statuscode' />
                <filter type='and' >
                  <condition attribute='bsd_optionentry' operator='eq' value='{0}' />                    
                </filter>
              </entity>
            </fetch>";

            fetchXml = string.Format(fetchXml, oeID);
            EntityCollection entc = crmservices.RetrieveMultiple(new FetchExpression(fetchXml));
            return entc;
        }
        private bool check_ExistOEinFUL(IOrganizationService crmservices, Guid oeID, Guid productID)
        {
            string fetchXml =
              @"<fetch version='1.0' output-format='xml-platform' mapping='logical' distinct='false'>
                  <entity name='bsd_followuplist' >
                    <attribute name='bsd_units' />
                    <attribute name='bsd_followuplistid' />
                    <attribute name='bsd_type' />
                    <attribute name='bsd_reservation' />
                    <filter type='and' >
                      <condition attribute='bsd_units' operator='eq' value='{0}' />
                      <condition attribute='bsd_optionentry' operator='eq' value='{1}' />
                      <condition attribute='statuscode' operator='eq' value='1' />
                    </filter>
                  </entity>
            </fetch>";
            fetchXml = string.Format(fetchXml, productID, oeID);
            EntityCollection entc = crmservices.RetrieveMultiple(new FetchExpression(fetchXml));
            if (entc.Entities.Count > 0)
                return true;
            return false;
        }
        private void create_FUL(Entity e_OE, int i_type, EntityReference en_Unit, int bsd_group, string s_proCode, IOrganizationService serv, DateTime d_date)
        {

            string s_group = "";
            switch (bsd_group)
            {
                case 100000000:
                    s_group = "S&M";
                    break;
                case 100000001:
                    s_group = "CCR";
                    break;
                case 100000002:
                    s_group = "FIN";
                    break;
            }
            Entity tmp = new Entity("bsd_followuplist");
            Entity en_U = serv.Retrieve(en_Unit.LogicalName, en_Unit.Id, new ColumnSet(new string[] { "name" }));

            Entity customer = new Entity();

            if (((EntityReference)e_OE["customerid"]).LogicalName == "contact")
            {
                customer = service.Retrieve(((EntityReference)e_OE["customerid"]).LogicalName, ((EntityReference)e_OE["customerid"]).Id,
               new ColumnSet(new string[] { "bsd_fullname", "fullname" }));
                tmp["bsd_name"] = (string)en_U["name"] + "-" + s_proCode + "-" + (string)e_OE["name"] + "-" + (customer.Contains("bsd_fullname") ? (string)customer["bsd_fullname"] : (string)customer["fullname"]) + "-" + s_group;
            }
            else
            {
                customer = service.Retrieve(((EntityReference)e_OE["customerid"]).LogicalName, ((EntityReference)e_OE["customerid"]).Id,
                new ColumnSet(new string[] { "bsd_name", "name" }));
                tmp["bsd_name"] = (string)en_U["name"] + "-" + s_proCode + "-" + (string)e_OE["name"] + "-" + (customer.Contains("bsd_name") ? (string)customer["bsd_name"] : (string)customer["name"]) + "-" + s_group;
            }

            tmp["bsd_date"] = DateTime.Now;
            // type = 100000003 Optionentry_contract
            //100000002 OE - 1st installment
            // 100000004 OE - installment
            //100000006 OE - terminate
            tmp["bsd_type"] = new OptionSetValue(i_type);
            tmp["bsd_group"] = new OptionSetValue(bsd_group);
            tmp["bsd_optionentry"] = e_OE.ToEntityReference();
            tmp["bsd_units"] = en_Unit;
            tmp["bsd_sellingprice"] = e_OE["totalamount"];

            tmp["bsd_project"] = ((EntityReference)e_OE["bsd_project"]);

            tmp["bsd_expiredate"] = d_date;
            tmp["bsd_owneroptionreservation"] = ((EntityReference)e_OE["ownerid"]); //(EntityReference)e_OE["ownerid"];
            service.Create(tmp);
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
