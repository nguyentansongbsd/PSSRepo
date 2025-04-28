using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Query;
using Microsoft.Crm.Sdk.Messages;
using System.Web.Script.Serialization;
using System.Diagnostics;
using System.Collections;

namespace Action_ConfirmApplyDocument_Confirm
{
    class ApplyDocument
    {
        IOrganizationService service = null;
        IOrganizationServiceFactory factory = null;
        IPluginExecutionContext context = null;
        ITracingService TracingSe = null;
        StringBuilder strMess = new StringBuilder();
        StringBuilder strMess1 = new StringBuilder();
        IServiceProvider serviceProvider;
        decimal d_oe_bsd_totalamountpaid = 0;
        public ApplyDocument(IServiceProvider serviceProvider)
        {
            this.serviceProvider = serviceProvider;
            context = (IPluginExecutionContext)serviceProvider.GetService(typeof(IPluginExecutionContext));
            factory = (IOrganizationServiceFactory)serviceProvider.GetService(typeof(IOrganizationServiceFactory));
            service = factory.CreateOrganizationService(context.UserId);
            TracingSe = (ITracingService)serviceProvider.GetService(typeof(ITracingService));
        }

        public void checkInput(Entity en_app)
        {
            int bsd_transactiontype = en_app.Contains("bsd_transactiontype") ? ((OptionSetValue)en_app["bsd_transactiontype"]).Value : 0;
            decimal bsd_advancepaymentamount = en_app.Contains("bsd_advancepaymentamount") ? ((Money)en_app["bsd_advancepaymentamount"]).Value : 0;
            if (bsd_transactiontype != 0)
            {
                if (!en_app.Contains("bsd_customer") || !en_app.Contains("bsd_project") || !en_app.Contains("bsd_optionentry"))
                {
                    throw new InvalidPluginExecutionException("Missing information. Please check again.");
                }
                Entity enOE = service.Retrieve(((EntityReference)en_app["bsd_optionentry"]).LogicalName, ((EntityReference)en_app["bsd_optionentry"]).Id, new ColumnSet(true));
                int statuscode = enOE.Contains("statuscode") ? ((OptionSetValue)enOE["statuscode"]).Value : 0;
                if (statuscode == 100000006)
                {
                    throw new InvalidPluginExecutionException("Status is incorrect. Please check again.");
                }
                checkAmountAdvance(((EntityReference)en_app["bsd_customer"]).Id, ((EntityReference)en_app["bsd_project"]).Id, ((EntityReference)en_app["bsd_optionentry"]).Id, bsd_advancepaymentamount);
            }
        }
        private void checkAmountAdvance(Guid KH, Guid DA, Guid OE, decimal AmountAdvance)
        {
            var fetchXml = $@"<?xml version=""1.0"" encoding=""utf-16""?>
                                <fetch>
                                  <entity name=""bsd_advancepayment"">
                                    <attribute name=""bsd_remainingamount"" />
                                    <filter>
                                      <condition attribute=""bsd_customer"" operator=""eq"" value=""{KH}"" />
                                      <condition attribute=""bsd_project"" operator=""eq"" value=""{DA}"" />
                                      <condition attribute=""bsd_optionentry"" operator=""eq"" value=""{OE}"" />
                                      <condition attribute=""bsd_remainingamount"" operator=""gt"" value=""{0}"" />
                                      <condition attribute=""statuscode"" operator=""eq"" value=""{100000000}"" />
                                    </filter>
                                  </entity>
                                </fetch>";
            EntityCollection rs = service.RetrieveMultiple(new FetchExpression(fetchXml));
            if (rs.Entities.Count == 0 || rs.Entities.Sum(x => ((Money)x["bsd_remainingamount"]).Value) < AmountAdvance)
            {
                throw new InvalidPluginExecutionException("Advance payment is excess amount remaining. Please check again.");
            }
        }
        public void paymentInstallment(Entity en_app, ref decimal totalapplyamout, string type, ref string str0, ref string str00, ArrayList listCheckFee)
        {
            Entity en_OE = service.Retrieve("salesorder", ((EntityReference)en_app["bsd_optionentry"]).Id, new ColumnSet(true));
            string optionentryID = en_OE.Id.ToString();
            if (!en_OE.Contains("bsd_unitnumber")) throw new InvalidPluginExecutionException("Cannot find Unit information in Option Entry " + (string)en_OE["name"] + "!");
            d_oe_bsd_totalamountpaid = en_OE.Contains("bsd_totalamountpaid") ? ((Money)en_OE["bsd_totalamountpaid"]).Value : 0;
            decimal d_oe_bsd_totalamountlessfreight = en_OE.Contains("bsd_totalamountlessfreight") ? ((Money)en_OE["bsd_totalamountlessfreight"]).Value : 0;
            if (d_oe_bsd_totalamountlessfreight == 0) throw new InvalidPluginExecutionException("'Net Selling Price' must be larger than 0");
            TracingSe.Trace("trước if type");
            if (type == "Installments")
            {
                applyIntallment(en_app, en_OE, ref totalapplyamout, ref str0, ref str00);
            }
            else if (type == "Interest")
            {
                applyInterestcharge(en_app, en_OE, ref totalapplyamout);
            }
            else if (type == "Fees")
            {
                applyFees(en_app, en_OE, ref totalapplyamout, ref str0, ref str00, listCheckFee);
            }
            else if (type == "Miscellaneous")
            {
                applyMicellaneous(en_app, en_OE, ref totalapplyamout);
            }
        }
        public void createCOA(Entity en_app, decimal totalapplyamout, ArrayList s_eachAdv, ArrayList s_amAdv)
        {
            EntityCollection ecAdvance = get_ecAdvance(service, ((EntityReference)en_app["bsd_customer"]).Id, ((EntityReference)en_app["bsd_optionentry"]).Id, ((EntityReference)en_app["bsd_project"]).Id);
            foreach (Entity en_Adv in ecAdvance.Entities)
            {
                decimal totalavancedamount = en_Adv.Contains("bsd_amount") ? ((Money)en_Adv["bsd_amount"]).Value : 0;
                decimal bsd_paidamount = en_Adv.Contains("bsd_paidamount") ? ((Money)en_Adv["bsd_paidamount"]).Value : 0;
                decimal bsd_refundamount = en_Adv.Contains("bsd_refundamount") ? ((Money)en_Adv["bsd_refundamount"]).Value : 0;
                decimal bsd_remainingamount = en_Adv.Contains("bsd_remainingamount") ? ((Money)en_Adv["bsd_remainingamount"]).Value : 0;
                decimal bsd_avPMPaid = bsd_paidamount + bsd_refundamount;
                decimal apply = 0;
                decimal delta = 0;
                decimal remain = 0;
                if (totalapplyamout == 0)
                {
                    break;
                }
                else if (totalapplyamout > 0)
                {
                    s_eachAdv.Add(en_Adv.Id);
                    delta = totalapplyamout - bsd_remainingamount;
                    if (delta >= 0)
                    {
                        apply = bsd_remainingamount;
                        s_amAdv.Add(apply);
                        remain = totalavancedamount - bsd_refundamount - bsd_paidamount - apply;
                    }
                    else
                    {
                        apply = totalapplyamout;
                        s_amAdv.Add(apply);
                        remain = totalavancedamount - bsd_refundamount - bsd_paidamount - apply;
                    }
                    // Tính remaining
                    totalapplyamout -= apply;
                }
                create_Applydocument_RemainingCOA(service, en_app, en_Adv, totalavancedamount, apply, remain, bsd_avPMPaid);
            }
        }
        public void updateApplyDocument(Entity en_app, decimal d_tmp, ArrayList s_eachAdv, ArrayList s_amAdv)
        {
            decimal bsd_actualamountspent = d_tmp;
            for (int n = 0; n < s_eachAdv.Count; n++)
            {
                if (d_tmp == 0) break;
                // so tien can tra cho applydoc = so tien cua adv check dau tien - k can update nua - chi update cho adv nay thoi
                if (d_tmp == (decimal)s_amAdv[n])
                {
                    strMess.AppendLine("Up_adv 1");
                    Up_adv((Guid)s_eachAdv[n], (decimal)s_amAdv[n]); // payoff
                    d_tmp = 0;
                    break;
                }
                if (d_tmp < (decimal)s_amAdv[n])
                {
                    strMess.AppendLine("Up_adv 2");
                    Up_adv((Guid)s_eachAdv[n], d_tmp); // collect
                    d_tmp = 0;
                    break;
                }
                if (d_tmp > (decimal)s_amAdv[n])
                {
                    strMess.AppendLine("Up_adv 3");
                    Up_adv((Guid)s_eachAdv[n], (decimal)s_amAdv[n]); // payoff -- tiep tu for toi advance pm tiep theo
                    d_tmp -= (decimal)s_amAdv[n];

                }
            }
            DateTime d_now = RetrieveLocalTimeFromUTCTime(DateTime.Now);
            // update status code cua apply document = approve
            Entity en_appTmp = new Entity(en_app.LogicalName);
            en_appTmp.Id = en_app.Id;
            en_appTmp["bsd_actualamountspent"] = new Money(bsd_actualamountspent);
            en_appTmp["statuscode"] = new OptionSetValue(100000002);
            en_appTmp["bsd_approvaldate"] = d_now;
            en_appTmp["bsd_approver"] = new EntityReference("systemuser", context.UserId);
            service.Update(en_appTmp);
            strMess.AppendLine("aaaaa");
        }
        private void applyIntallment(Entity en_app, Entity en_OE, ref decimal totalapplyamout, ref string str1, ref string str2)
        {
            EntityCollection ec_ins = get_ecIns(service, en_OE.Id);
            if (ec_ins.Entities.Count == 0) throw new InvalidPluginExecutionException("The list is empty. Please check again.");
            ArrayList listID = new ArrayList();
            ArrayList listAmount = new ArrayList();
            for (int i = 0; i < ec_ins.Entities.Count; i++)
            {
                if (totalapplyamout == 0) break;
                Entity en_pms = ec_ins.Entities[i];
                en_pms.Id = ec_ins.Entities[i].Id;
                int psd_statuscode = ((OptionSetValue)ec_ins.Entities[i]["statuscode"]).Value;
                if (psd_statuscode == 100000001) throw new InvalidPluginExecutionException((string)ec_ins.Entities[i]["bsd_name"] + " has been Paid!");
                if (!en_pms.Contains("bsd_duedate"))
                {
                    throw new Exception("The Installment you are paying has no due date. Please update due date before confirming payment! " + en_pms["bsd_name"].ToString());
                }
                int i_duedateMethod = ec_ins.Entities[i].Contains("bsd_duedatecalculatingmethod") ? ((OptionSetValue)ec_ins.Entities[i]["bsd_duedatecalculatingmethod"]).Value : 0;
                if (!ec_ins.Entities[i].Contains("bsd_amountofthisphase")) throw new InvalidPluginExecutionException("Installment " + (string)ec_ins.Entities[i]["bsd_name"] + " did not contain 'Amount of this phase'!");
                decimal psd_amountPhase = ((Money)ec_ins.Entities[i]["bsd_amountofthisphase"]).Value;
                decimal psd_amountPaid = ec_ins.Entities[i].Contains("bsd_amountwaspaid") ? ((Money)ec_ins.Entities[i]["bsd_amountwaspaid"]).Value : 0;
                decimal psd_deposit = ec_ins.Entities[i].Contains("bsd_depositamount") ? ((Money)ec_ins.Entities[i]["bsd_depositamount"]).Value : 0;
                decimal psd_waiver = ec_ins.Entities[i].Contains("bsd_waiveramount") ? ((Money)ec_ins.Entities[i]["bsd_waiveramount"]).Value : 0;
                decimal psd_waiverIns = ec_ins.Entities[i].Contains("bsd_waiverinstallment") ? ((Money)ec_ins.Entities[i]["bsd_waiverinstallment"]).Value : 0;
                decimal psd_bsd_balance = ec_ins.Entities[i].Contains("bsd_balance") ? ((Money)ec_ins.Entities[i]["bsd_balance"]).Value : 0;
                bool psd_bsd_maintenancefeesstatus = ec_ins.Entities[i].Contains("bsd_maintenancefeesstatus") ? (bool)ec_ins.Entities[i]["bsd_maintenancefeesstatus"] : false;
                bool psd_bsd_managementfeesstatus = ec_ins.Entities[i].Contains("bsd_managementfeesstatus") ? (bool)ec_ins.Entities[i]["bsd_managementfeesstatus"] : false;
                int i_phaseNum = (int)ec_ins.Entities[i]["bsd_ordernumber"];
                // --------------------- Update ------------------------------------
                decimal d_ampay = 0;
                bool f_check_1st = false;
                f_check_1st = check_Ins_Paid(service, en_OE.Id, 1);
                // --------------------------- payment --------------------------------------------------
                int i_late = 0;
                decimal d_outstandingAM = 0;
                if (i_phaseNum == 1)
                    psd_bsd_balance = psd_amountPhase - psd_amountPaid - psd_deposit - psd_waiverIns;// psd_balance - psd_deposit
                else psd_bsd_balance = psd_amountPhase - psd_amountPaid - psd_waiverIns; // = psd_balance
                if (totalapplyamout > psd_bsd_balance)
                {
                    d_ampay = psd_bsd_balance;
                    totalapplyamout -= psd_bsd_balance;
                }
                else
                {
                    d_ampay = totalapplyamout;
                    totalapplyamout = 0;
                }
                listID.Add(en_pms.Id);
                listAmount.Add(d_ampay);
                DateTime d_receipt1 = new DateTime();
                if (en_pms.Contains("bsd_duedate"))
                {
                    d_receipt1 = (DateTime)en_app["bsd_receiptdate"];
                }
                if (d_ampay >= psd_bsd_balance) // cap nhat installment
                {
                    psd_amountPaid += psd_bsd_balance;
                    psd_statuscode = 100000001;
                    if (ec_ins.Entities[i].Contains("bsd_duedate"))
                    {
                        DateTime d_receipt = (DateTime)en_app["bsd_receiptdate"];
                        checkInterest(ec_ins.Entities[i], d_receipt, psd_bsd_balance, ref i_late, ref d_outstandingAM);
                    }
                    d_oe_bsd_totalamountpaid += psd_bsd_balance;
                    DateTime d_now = RetrieveLocalTimeFromUTCTime(DateTime.Now);
                    Up_Ins_OE(en_pms, en_OE, psd_amountPaid, 0, psd_statuscode, psd_bsd_balance, f_check_1st, d_now, i_late, d_outstandingAM);
                    create_app_detail(service, en_app, en_pms, en_OE, 100000001, psd_bsd_balance, i_late, d_outstandingAM, 0);
                }
                else if (d_ampay < psd_bsd_balance) // not paid
                {

                    psd_statuscode = 100000000;
                    psd_amountPaid += d_ampay;
                    psd_bsd_balance -= d_ampay;
                    if (en_pms.Contains("bsd_duedate"))
                    {
                        DateTime d_receipt = (DateTime)en_app["bsd_receiptdate"];
                        checkInterest(en_pms, d_receipt, d_ampay, ref i_late, ref d_outstandingAM);
                    }
                    d_oe_bsd_totalamountpaid += d_ampay;
                    DateTime d_now = RetrieveLocalTimeFromUTCTime(DateTime.Now);
                    Up_Ins_OE(en_pms, en_OE, psd_amountPaid, psd_bsd_balance, psd_statuscode, d_ampay, f_check_1st, d_now, i_late, d_outstandingAM);
                    create_app_detail(service, en_app, en_pms, en_OE, 100000001, d_ampay, i_late, d_outstandingAM, 0);
                }
            } // end for each installment
            if (listID.Count > 0)
            {
                str1 = String.Join(",", listID.ToArray());
                str2 = String.Join(",", listAmount.ToArray());
            }
        }
        private void checkInterest(Entity enInstallment, DateTime d_receipt, decimal amountPay, ref int i_late, ref decimal d_outstandingAM)
        {
            DateTime d_duedate = RetrieveLocalTimeFromUTCTime((DateTime)enInstallment["bsd_duedate"]);
            d_receipt = RetrieveLocalTimeFromUTCTime(d_receipt);

            OrganizationRequest req = new OrganizationRequest("bsd_Action_Calculate_Interest");
            //parameter:
            req["installmentid"] = enInstallment.Id.ToString();
            req["amountpay"] = amountPay.ToString();
            req["receiptdate"] = d_receipt.ToString("MM/dd/yyyy");
            //execute the request
            OrganizationResponse response = service.Execute(req);
            foreach (var item in response.Results)
            {
                var serializer = new JavaScriptSerializer();
                var result = serializer.Deserialize<Installment>(item.Value.ToString());
                i_late = result.LateDays;
                d_outstandingAM = result.InterestCharge;
            }
        }
        private void applyInterestcharge(Entity en_app, Entity en_OE, ref decimal totalapplyamout)
        {
            EntityCollection ec_PMSDTL = get_ecIns_Int(service, en_OE.Id);
            if (ec_PMSDTL.Entities.Count < 0) throw new InvalidPluginExecutionException("The list is empty. Please check again.");
            for (int j = 0; j < ec_PMSDTL.Entities.Count; j++)
            {
                Entity en_interIns = ec_PMSDTL.Entities[j];
                en_interIns.Id = ec_PMSDTL.Entities[j].Id;
                if (totalapplyamout == 0) break;
                if (!en_interIns.Contains("bsd_duedate"))
                {
                    throw new Exception("The Installment you are paying has no due date. Please update due date before confirming payment! " + en_interIns["bsd_name"].ToString());
                }
                decimal psd_bsd_interestchargeamount = en_interIns.Contains("bsd_interestchargeamount") ? ((Money)en_interIns["bsd_interestchargeamount"]).Value : 0;
                decimal psd_bsd_interestwaspaid = en_interIns.Contains("bsd_interestwaspaid") ? ((Money)en_interIns["bsd_interestwaspaid"]).Value : 0;
                decimal psd_bsd_waiveramount = en_interIns.Contains("bsd_waiverinterest") ? ((Money)en_interIns["bsd_waiverinterest"]).Value : 0;
                int psd_bsd_interestchargestatus = en_interIns.Contains("bsd_interestchargestatus") ? ((OptionSetValue)en_interIns["bsd_interestchargestatus"]).Value : 0;
                decimal psd_actualgracedays = en_interIns.Contains("bsd_actualgracedays") ? (int)en_interIns["bsd_actualgracedays"] : 0;
                int statuscode = en_interIns.Contains("statuscode") ? ((OptionSetValue)en_interIns["statuscode"]).Value : 0;
                if (psd_bsd_interestchargestatus == 100000001) throw new InvalidPluginExecutionException("The Interest charge of " + (string)en_interIns["bsd_name"] + " has been paid!");
                decimal d_ampay = 0;
                decimal d_balance = psd_bsd_interestchargeamount - psd_bsd_interestwaspaid - psd_bsd_waiveramount;
                if (totalapplyamout > d_balance)
                {
                    d_ampay = d_balance;
                    totalapplyamout -= d_balance;
                }
                else
                {
                    d_ampay = totalapplyamout;
                    totalapplyamout = 0;
                }
                if (d_ampay == d_balance) // cap nhat installment
                {
                    psd_bsd_interestwaspaid += d_balance;
                    if (statuscode == 100000001) psd_bsd_interestchargestatus = 100000001;
                    else psd_bsd_interestchargestatus = 100000000;
                    Update_Interest(service, en_interIns, psd_bsd_interestwaspaid, psd_bsd_interestchargestatus);
                    create_app_detail(service, en_app, en_interIns, en_OE, 100000003, d_balance, 0, 0, 0);
                }
                else if (d_ampay < d_balance) // not paid
                {
                    psd_bsd_interestwaspaid += d_ampay;
                    psd_bsd_interestchargestatus = 100000000;
                    Update_Interest(service, en_interIns, psd_bsd_interestwaspaid, psd_bsd_interestchargestatus);
                    create_app_detail(service, en_app, en_interIns, en_OE, 100000003, d_ampay, 0, 0, 0);
                }
            }// end for int j = 0 ; j < ec_PMSDTL.count
        }
        private void applyFees(Entity en_app, Entity en_OE, ref decimal totalapplyamout, ref string str3, ref string str4, ArrayList listCheckFee)
        {
            if (!en_app.Contains("bsd_units"))
                throw new InvalidPluginExecutionException("Please check Units of Apply Document!");
            //Entity en_product = service.Retrieve(((EntityReference)en_app["bsd_units"]).LogicalName, ((EntityReference)en_app["bsd_units"]).Id, new ColumnSet(new string[] { "bsd_handovercondition", "name", "bsd_opdate" }));
            EntityCollection ecFee_Main = get_ecFee_Main(service, en_OE.Id);
            EntityCollection ecFee_Mana = get_ecFee_Mana(service, en_OE.Id);
            TracingSe.Trace("trước ecFee_Main");
            if (ecFee_Main.Entities.Count == 0 && ecFee_Mana.Entities.Count == 0) throw new InvalidPluginExecutionException("The list is empty. Please check again.");
            ArrayList listID = new ArrayList();
            ArrayList listAmount = new ArrayList();
            foreach (Entity enInstallment in ecFee_Main.Entities)
            {
                if (totalapplyamout == 0) break;
                if (!enInstallment.Contains("bsd_duedate"))
                {
                    throw new Exception("The Installment you are paying has no due date. Please update due date before confirming payment! " + enInstallment["bsd_name"].ToString());
                }
                decimal fee = 0;
                bool f_main = (enInstallment.Contains("bsd_maintenancefeesstatus")) ? (bool)enInstallment["bsd_maintenancefeesstatus"] : false;
                decimal d_bsd_maintenanceamount = enInstallment.Contains("bsd_maintenanceamount") ? ((Money)enInstallment["bsd_maintenanceamount"]).Value : 0;
                decimal d_bsd_maintenancefeepaid = enInstallment.Contains("bsd_maintenancefeepaid") ? ((Money)enInstallment["bsd_maintenancefeepaid"]).Value : 0;
                decimal d_bsd_maintenancefeewaiver = enInstallment.Contains("bsd_maintenancefeewaiver") ? ((Money)enInstallment["bsd_maintenancefeewaiver"]).Value : 0;
                decimal d_mainBL = d_bsd_maintenanceamount - d_bsd_maintenancefeepaid - d_bsd_maintenancefeewaiver;
                if (totalapplyamout > d_mainBL)
                {
                    fee = d_mainBL;
                    totalapplyamout -= d_mainBL;
                }
                else
                {
                    fee = totalapplyamout;
                    totalapplyamout = 0;
                }
                listID.Add(enInstallment.Id);
                listAmount.Add(fee);
                Entity en_INS_update = new Entity(enInstallment.LogicalName);
                en_INS_update.Id = enInstallment.Id;
                if (f_main == true) throw new InvalidPluginExecutionException("Maintenance fees had been paid. Please check again!");
                if (fee < d_mainBL)
                {
                    d_bsd_maintenancefeepaid += fee;
                    f_main = false;
                    d_oe_bsd_totalamountpaid += fee;

                }
                else if (fee == d_mainBL)
                {
                    d_bsd_maintenancefeepaid += fee;
                    f_main = true;
                    d_oe_bsd_totalamountpaid += fee;

                }
                else if (fee > d_mainBL) throw new InvalidPluginExecutionException("Amount pay larger than balance of Maintenance fees amount. Please check again!");
                create_app_detail(service, en_app, enInstallment, en_OE, 100000002, fee, 0, 0, 1);
                en_INS_update["bsd_maintenancefeesstatus"] = f_main;
                en_INS_update["bsd_maintenancefeepaid"] = new Money(d_bsd_maintenancefeepaid);
                service.Update(en_INS_update);
                listCheckFee.Add("main");
            }
            TracingSe.Trace("trước ecFee_Mana");
            foreach (Entity enInstallment in ecFee_Mana.Entities)
            {
                if (totalapplyamout == 0) break;
                if (!enInstallment.Contains("bsd_duedate"))
                {
                    throw new Exception("The Installment you are paying has no due date. Please update due date before confirming payment! " + enInstallment["bsd_name"].ToString());
                }
                decimal fee = 0;
                bool f_mana = (enInstallment.Contains("bsd_managementfeesstatus")) ? (bool)enInstallment["bsd_managementfeesstatus"] : false;
                decimal d_bsd_managementamount = enInstallment.Contains("bsd_managementamount") ? ((Money)enInstallment["bsd_managementamount"]).Value : 0;
                decimal d_bsd_managementfeepaid = enInstallment.Contains("bsd_managementfeepaid") ? ((Money)enInstallment["bsd_managementfeepaid"]).Value : 0;
                decimal d_bsd_managementfeewaiver = enInstallment.Contains("bsd_managementfeewaiver") ? ((Money)enInstallment["bsd_managementfeewaiver"]).Value : 0;
                decimal d_manaBL = d_bsd_managementamount - d_bsd_managementfeepaid - d_bsd_managementfeewaiver;
                if (totalapplyamout > d_manaBL)
                {
                    fee = d_manaBL;
                    totalapplyamout -= d_manaBL;
                }
                else
                {
                    fee = totalapplyamout;
                    totalapplyamout = 0;
                }
                listID.Add(enInstallment.Id);
                listAmount.Add(fee);
                Entity en_INS_update = new Entity(enInstallment.LogicalName);
                en_INS_update.Id = enInstallment.Id;
                if (f_mana == true) throw new InvalidPluginExecutionException("Management fees had been paid. Please check again!");
                if (fee < d_manaBL)
                {
                    d_bsd_managementfeepaid += fee;
                    f_mana = false;
                }
                else if (fee == d_manaBL)
                {
                    d_bsd_managementfeepaid += fee;
                    f_mana = true;
                }
                else if (fee > d_manaBL) throw new InvalidPluginExecutionException("Amount pay larger than balance of Management fees amount. Please check again!");
                create_app_detail(service, en_app, enInstallment, en_OE, 100000002, fee, 0, 0, 2);
                en_INS_update["bsd_managementfeesstatus"] = f_mana;
                en_INS_update["bsd_managementfeepaid"] = new Money(d_bsd_managementfeepaid);
                service.Update(en_INS_update);
                listCheckFee.Add("mana");
            }
            if (listID.Count > 0)
            {
                str3 = String.Join(",", listID.ToArray());
                str4 = String.Join(",", listAmount.ToArray());
            }
        }
        private void applyMicellaneous(Entity en_app, Entity en_OE, ref decimal totalapplyamout)
        {
            EntityCollection ec_MIS = get_ecMIS(service, en_OE.Id);
            if (ec_MIS.Entities.Count <= 0) throw new InvalidPluginExecutionException("The list is empty. Please check again.");
            for (int i = 0; i < ec_MIS.Entities.Count; i++)
            {
                if (totalapplyamout == 0) break;
                Entity en_Ins_MIS = service.Retrieve(((EntityReference)ec_MIS.Entities[i]["bsd_installment"]).LogicalName, ((EntityReference)ec_MIS.Entities[i]["bsd_installment"]).Id,
                        new ColumnSet(new string[] { "bsd_paymentschemedetailid", "bsd_name", "statuscode" }));
                int i_INS_statuscode = en_Ins_MIS.Contains("statuscode") ? ((OptionSetValue)en_Ins_MIS["statuscode"]).Value : 100000000;
                // MISS has been paid
                int f_paid = ec_MIS.Entities[i].Contains("statuscode") ? ((OptionSetValue)ec_MIS.Entities[i]["statuscode"]).Value : 1;

                if (f_paid == 100000000)
                    throw new InvalidPluginExecutionException("Miscellaneous " + (string)ec_MIS.Entities[i]["bsd_name"] + " has been paid");
                decimal d_amp = 0;
                decimal d_MI_paid = ec_MIS.Entities[i].Contains("bsd_paidamount") ? ((Money)ec_MIS.Entities[i]["bsd_paidamount"]).Value : 0;
                decimal d_MI_balance = ec_MIS.Entities[i].Contains("bsd_balance") ? ((Money)ec_MIS.Entities[i]["bsd_balance"]).Value : 0;
                decimal d_MI_am = ec_MIS.Entities[i].Contains("bsd_totalamount") ? ((Money)ec_MIS.Entities[i]["bsd_totalamount"]).Value : 0;
                if (totalapplyamout > d_MI_balance)
                {
                    d_amp = d_MI_balance;
                    totalapplyamout -= d_MI_balance;
                }
                else
                {
                    d_amp = totalapplyamout;
                    totalapplyamout = 0;
                }
                // -- 29.03.2018 - Han - Update Misc
                if (d_amp == d_MI_balance)
                {
                    d_MI_balance = 0;
                    d_MI_paid = d_MI_am;
                    f_paid = 100000000;
                }
                else if (d_amp < d_MI_balance)
                {
                    f_paid = 1;
                    d_MI_balance -= d_amp;
                    d_MI_paid += d_amp;
                }
                else throw new InvalidPluginExecutionException("Input amount is larger than balance amount of Miscellaneous " + (string)ec_MIS.Entities[i]["bsd_name"]);
                // update
                Entity en_up_MIS = new Entity(ec_MIS.Entities[i].LogicalName);
                en_up_MIS.Id = ec_MIS.Entities[i].Id;
                en_up_MIS["bsd_paidamount"] = new Money(d_MI_paid);
                en_up_MIS["bsd_balance"] = new Money(d_MI_balance);
                en_up_MIS["statuscode"] = new OptionSetValue(f_paid);
                service.Update(en_up_MIS);
                create_app_detail_MISS(service, en_app, en_Ins_MIS, en_OE, 100000004, d_amp, 0, 0, 0, ec_MIS.Entities[i]);
            }// end for
        }
        private EntityCollection get_ecAdvance(IOrganizationService crmservices, Guid idCus, Guid Proj, Guid OE)
        {
            var fetchXml = $@"<?xml version=""1.0"" encoding=""utf-16""?>
            <fetch>
              <entity name=""bsd_advancepayment"">
                <attribute name=""bsd_advancepaymentid"" />
                <attribute name=""bsd_amount"" />
                <attribute name=""bsd_paidamount"" />
                <attribute name=""bsd_refundamount"" />
                <attribute name=""bsd_remainingamount"" />
                <attribute name=""bsd_name"" />
                <filter>
                  <condition attribute=""bsd_remainingamount"" operator=""gt"" value=""{0}"" />
                  <condition attribute=""bsd_customer"" operator=""eq"" value=""{idCus}"" />
                  <condition attribute=""bsd_project"" operator=""eq"" value=""{Proj}"" />
                  <condition attribute=""bsd_optionentry"" operator=""eq"" value=""{OE}"" />
                  <condition attribute=""statuscode"" operator=""eq"" value=""{100000000}"" />
                </filter>
                <order attribute=""bsd_remainingamount"" />
              </entity>
            </fetch>";
            EntityCollection entc = crmservices.RetrieveMultiple(new FetchExpression(fetchXml));
            return entc;
        }
        private EntityCollection get_ecIns(IOrganizationService crmservices, Guid idOE)
        {
            var fetchXml = $@"<?xml version=""1.0"" encoding=""utf-16""?>
            <fetch>
              <entity name=""bsd_paymentschemedetail"">
                <attribute name=""bsd_duedatecalculatingmethod"" />
                <attribute name=""bsd_amountofthisphase"" />
                <attribute name=""bsd_amountwaspaid"" />
                <attribute name=""bsd_depositamount"" />
                <attribute name=""bsd_waiveramount"" />
                <attribute name=""bsd_balance"" />
                <attribute name=""bsd_maintenancefeesstatus"" />
                <attribute name=""bsd_maintenancefeesstatus"" />
                <attribute name=""bsd_waiverinstallment"" />
                <attribute name=""bsd_ordernumber"" />
                <attribute name=""bsd_duedate"" />
                <attribute name=""statuscode"" />
                <attribute name=""bsd_name"" />
                <filter>
                  <condition attribute=""bsd_balance"" operator=""gt"" value=""{0}"" />
                  <condition attribute=""bsd_optionentry"" operator=""eq"" value=""{idOE}"" />
                  <condition attribute=""statecode"" operator=""eq"" value=""{0}"" />
                  <condition attribute=""statuscode"" operator=""eq"" value=""{100000000}"" />
                  <condition attribute=""bsd_ordernumber"" operator=""not-null"" />
                </filter>
                <order attribute=""bsd_ordernumber"" />
              </entity>
            </fetch>";
            EntityCollection entc = crmservices.RetrieveMultiple(new FetchExpression(fetchXml));
            return entc;
        }
        private EntityCollection get_ecIns_Int(IOrganizationService crmservices, Guid idOE)
        {
            var fetchXml = $@"<?xml version=""1.0"" encoding=""utf-16""?>
            <fetch>
              <entity name=""bsd_paymentschemedetail"">
                <attribute name=""bsd_interestchargeamount"" />
                <attribute name=""bsd_interestwaspaid"" />
                <attribute name=""bsd_waiverinterest"" />
                <attribute name=""bsd_interestchargestatus"" />
                <attribute name=""bsd_actualgracedays"" />
                <attribute name=""bsd_name"" />
                <attribute name=""bsd_duedate"" />
                <attribute name=""statuscode"" />
                <filter>
                  <condition attribute=""bsd_interestchargeamount"" operator=""gt"" value=""{0}"" />
                  <condition attribute=""bsd_optionentry"" operator=""eq"" value=""{idOE}"" />
                  <condition attribute=""statecode"" operator=""eq"" value=""{0}"" />
                  <condition attribute=""bsd_interestchargestatus"" operator=""ne"" value=""{100000001}"" />
                  <condition attribute=""bsd_ordernumber"" operator=""not-null"" />
                </filter>
                <order attribute=""bsd_ordernumber"" />
              </entity>
            </fetch>";
            EntityCollection entc = crmservices.RetrieveMultiple(new FetchExpression(fetchXml));
            return entc;
        }
        private EntityCollection get_ecFee_Main(IOrganizationService crmservices, Guid idOE)
        {
            var fetchXml = $@"<?xml version=""1.0"" encoding=""utf-16""?>
            <fetch>
              <entity name=""bsd_paymentschemedetail"">
                <attribute name=""bsd_maintenancefeesstatus"" />
                <attribute name=""bsd_maintenanceamount"" />
                <attribute name=""bsd_maintenancefeepaid"" />
                <attribute name=""bsd_maintenancefeewaiver"" />
                <attribute name=""bsd_name"" />
                <attribute name=""bsd_duedate"" />
                <filter>
                  <condition attribute=""bsd_maintenanceamount"" operator=""gt"" value=""{0}"" />
                  <condition attribute=""bsd_optionentry"" operator=""eq"" value=""{idOE}"" />
                  <condition attribute=""statecode"" operator=""eq"" value=""{0}"" />
                </filter>
              </entity>
            </fetch>";
            EntityCollection entc = crmservices.RetrieveMultiple(new FetchExpression(fetchXml));
            return entc;
        }
        private EntityCollection get_ecFee_Mana(IOrganizationService crmservices, Guid idOE)
        {
            var fetchXml = $@"<?xml version=""1.0"" encoding=""utf-16""?>
            <fetch>
              <entity name=""bsd_paymentschemedetail"">
                <attribute name=""bsd_managementfeesstatus"" />
                <attribute name=""bsd_managementamount"" />
                <attribute name=""bsd_managementfeepaid"" />
                <attribute name=""bsd_managementfeewaiver"" />
                <attribute name=""bsd_name"" />
                <attribute name=""bsd_duedate"" />
                <filter>
                  <condition attribute=""bsd_managementamount"" operator=""gt"" value=""{0}"" />
                  <condition attribute=""bsd_optionentry"" operator=""eq"" value=""{idOE}"" />
                  <condition attribute=""statecode"" operator=""eq"" value=""{0}"" />
                </filter>
              </entity>
            </fetch>";
            EntityCollection entc = crmservices.RetrieveMultiple(new FetchExpression(fetchXml));
            return entc;
        }
        private void Up_adv(Guid advID, decimal trans_am)
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
            d_bsd_paidamount += trans_am;
            d_bsd_remainingamount -= trans_am;
            int i_status = 100000000; // collect
            if (d_bsd_remainingamount == 0)
                i_status = 100000001; // payoff

            Entity en_adv = new Entity("bsd_advancepayment");
            en_adv.Id = advID;
            en_adv["bsd_remainingamount"] = new Money(d_bsd_remainingamount);
            en_adv["bsd_paidamount"] = new Money(d_bsd_paidamount);
            en_adv["statuscode"] = new OptionSetValue(i_status);
            service.Update(en_adv);
        }
        private EntityCollection get_ecMIS(IOrganizationService crmservices, Guid idOE)
        {
            var fetchXml = $@"<?xml version=""1.0"" encoding=""utf-16""?>
            <fetch>
              <entity name=""bsd_miscellaneous"">
                <attribute name=""bsd_name"" />
                <attribute name=""bsd_installment"" />
                <attribute name=""statuscode"" />
                <attribute name=""bsd_paidamount"" />
                <attribute name=""bsd_balance"" />
                <attribute name=""bsd_totalamount"" />
                <filter>
                  <condition attribute=""bsd_balance"" operator=""gt"" value=""{0}"" />
                  <condition attribute=""bsd_optionentry"" operator=""eq"" value=""{idOE}"" />
                  <condition attribute=""statecode"" operator=""eq"" value=""{0}"" />
                  <condition attribute=""statuscode"" operator=""eq"" value=""{1}"" />
                  <condition attribute=""bsd_installment"" operator=""not-null"" />
                </filter>
                <order attribute=""bsd_balance"" />
              </entity>
            </fetch>";
            EntityCollection entc = crmservices.RetrieveMultiple(new FetchExpression(fetchXml));
            return entc;
        }
        // installment func
        private EntityCollection GetPSD(string OptionEntryID)
        {
            QueryExpression query = new QueryExpression("bsd_paymentschemedetail");
            query.ColumnSet = new ColumnSet(new string[] { "bsd_ordernumber", "bsd_paymentschemedetailid", "statuscode", "bsd_amountwaspaid" });
            query.Distinct = true;
            query.Criteria = new FilterExpression();
            query.Criteria.AddCondition("bsd_optionentry", ConditionOperator.Equal, OptionEntryID);
            query.AddOrder("bsd_ordernumber", OrderType.Ascending);
            //query.TopCount = 1;
            EntityCollection psdFirst = service.RetrieveMultiple(query);
            return psdFirst;
        }
        private void Up_Ins_OE(Entity en_Ins, Entity en_OE, decimal psd_amountPaid, decimal psd_balance, int psd_statuscode, decimal ampay, bool f_1st, DateTime d_now, int lateday, decimal interestcharge)
        {
            // update ins, OE & Unit
            // total amount paid of OE + ampay

            int i_phaseNum = en_Ins.Contains("bsd_ordernumber") ? (int)en_Ins["bsd_ordernumber"] : 1;
            decimal psd_deposit = en_Ins.Contains("bsd_depositamount") ? ((Money)en_Ins["bsd_depositamount"]).Value : 0;
            int psd_statuscodeInterest = en_Ins.Contains("bsd_interestchargestatus") ? ((OptionSetValue)en_Ins["bsd_interestchargestatus"]).Value : 100000000;//check interest đã thanh toán chưa
            bool psd_statuscodeFeeMain = en_Ins.Contains("bsd_maintenancefeesstatus") ? ((bool)en_Ins["bsd_maintenancefeesstatus"]) : false;//check fee đã thanh toán chưa
            bool psd_statuscodeFeeMana = en_Ins.Contains("bsd_managementfeesstatus") ? ((bool)en_Ins["bsd_managementfeesstatus"]) : false;//check fee đã thanh toán chưa
            var enmis = get_All_MIS_NotPaid(en_OE.Id.ToString());//dùng để kiểm tra xem có misc nào chưa thanh toán hay không
            int sttOE = en_OE.Contains("statuscode") ? ((OptionSetValue)en_OE["statuscode"]).Value : 100000000; // option
                                                                                                                //tính trễ
            int sttUnit = 100000001; // statuscode of unit= 1st installment
            Entity Unit = service.Retrieve("product", ((EntityReference)en_OE["bsd_unitnumber"]).Id, new ColumnSet(new string[] { "bsd_handovercondition", "statuscode" }));

            EntityCollection psdFirst = GetPSD(en_OE.Id.ToString());
            Entity detailFirst = psdFirst.Entities[0];

            int t = psdFirst.Entities.Count;
            Entity detailLast = psdFirst.Entities[t - 1]; // entity cuoi cung ( phase cuoi cung )
            string detailLastID = detailLast.Id.ToString();

            Entity ins_tmp = new Entity(en_Ins.LogicalName);
            ins_tmp.Id = en_Ins.Id;

            ins_tmp["bsd_amountwaspaid"] = new Money(psd_amountPaid);
            ins_tmp["bsd_balance"] = new Money(psd_balance);
            ins_tmp["statuscode"] = new OptionSetValue(psd_statuscode);
            if (lateday > 0)
            {
                ins_tmp["bsd_actualgracedays"] = lateday;
            }
            var interestchargeamount_old = ins_tmp.Contains("bsd_interestchargeamount") ? ((Money)ins_tmp["bsd_interestchargeamount"]).Value : 0;
            if (interestcharge > 0)
            {
                ins_tmp["bsd_interestchargeamount"] = new Money(interestcharge + interestchargeamount_old);
            }
            if (psd_statuscode == 100000001)
                ins_tmp["bsd_paiddate"] = d_now;  // update ngay paid cua INS

            service.Update(ins_tmp);

            // update OE

            Entity en_OEup = new Entity(en_OE.LogicalName);
            en_OEup.Id = en_OE.Id;
            int i_Ustatus = 100000003; // deposit
            int i_oeStatus = 100000000;// option

            if (i_phaseNum == 1)
            {
                if (en_OE.Contains("bsd_signedcontractdate"))
                {
                    sttOE = 100000002; // sign contract OE
                    sttUnit = 100000002; // unit = sold
                    f_1st = true;
                }
                else
                { // khi 1st da Paid roi moi duoc chuyen sang 1st installment else van la option
                    if (psd_statuscode == 100000000) // not paid
                    {
                        f_1st = false;
                        sttOE = 100000000; // option
                        sttUnit = 100000003; // depositd
                    }
                    else // paid
                    {
                        f_1st = true;
                        sttOE = 100000001;//1st
                        sttUnit = 100000001; // 1st
                    }

                }
            }
            else
            {
                f_1st = true;
                if (!en_OE.Contains("bsd_signedcontractdate"))
                {
                    sttOE = 100000001; // if OE not signcontract - status code still is 1st Installment
                    sttUnit = 100000001; // 1st
                }
                else
                {
                    sttUnit = 100000002;
                    sttOE = 100000003; //Being Payment (khi da sign contract)
                    if (detailLastID == en_Ins.Id.ToString() && psd_statuscode == 100000001 && psd_statuscodeInterest == 100000001 && psd_statuscodeFeeMain &&
                                        psd_statuscodeFeeMana && enmis != null && enmis.Entities.Count == 0)
                        sttOE = 100000004; //Complete Payment
                }
            }

            if (f_1st == false)
            {
                sttOE = i_oeStatus;
                sttUnit = i_Ustatus;
            }

            Unit["statuscode"] = new OptionSetValue(sttUnit); // Unit statuscode = 1st Installment
            service.Update(Unit);

            en_OEup["statuscode"] = new OptionSetValue(sttOE);
            service.Update(en_OEup);
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
        // Intest charge function
        private void Update_Interest(IOrganizationService ser, Entity en_interIns, decimal psd_bsd_interestwaspaid, int psd_bsd_interestchargestatus)
        {
            Entity en_Up = new Entity(en_interIns.LogicalName);
            en_Up.Id = en_interIns.Id;
            en_Up["bsd_interestwaspaid"] = new Money(psd_bsd_interestwaspaid);
            en_Up["bsd_interestchargestatus"] = new OptionSetValue(psd_bsd_interestchargestatus);
            ser.Update(en_Up);
        }
        // fees function -----------
        private void create_app_detail(IOrganizationService serv, Entity en_app, Entity en_Ins, Entity en_OE, int i_pmtype, decimal appAM, int i_outstanding_AppDtl,
            decimal d_bsd_interestchargeamount, int fee_type)
        {
            Entity en_Up = new Entity("bsd_applydocumentdetail");
            en_Up.Id = Guid.NewGuid();
            string s_type = "";
            switch (i_pmtype)
            {
                case 100000000:
                    s_type = "Deposit";
                    break;
                case 100000001:
                    s_type = "Installment";
                    break;
                case 100000003:
                    s_type = "Interest charge";
                    break;
                case 100000002:
                    s_type = "Fees";
                    break;
                case 100000004:
                    s_type = "Other";
                    break;
            }

            en_Up["bsd_name"] = "Apply document detail-" + (string)en_app["bsd_name"] + "-" + (string)en_Ins["bsd_name"] + "-" + s_type;

            en_Up["bsd_applydocument"] = en_app.ToEntityReference();
            en_Up["statuscode"] = new OptionSetValue(100000000); // confirm - Nhung

            en_Up["bsd_paymenttype"] = new OptionSetValue(i_pmtype);
            en_Up["bsd_installment"] = en_Ins.ToEntityReference();

            en_Up["bsd_amountapply"] = new Money(appAM);

            // bsd_feetype, Maintenace fee = 100.000.000, Management fee = 100.000.001
            if (i_pmtype == 100000002) // Fees
            {
                en_Up["bsd_optionentry"] = en_OE.ToEntityReference(); // OE
                if (fee_type == 1) // main
                {
                    en_Up["bsd_name"] = "Apply document detail-" + (string)en_app["bsd_name"] + "-" + (string)en_Ins["bsd_name"] + "-" + "Maitenance Fees";
                    en_Up["bsd_feetype"] = new OptionSetValue(100000000);
                }
                else
                {
                    en_Up["bsd_name"] = "Apply document detail-" + (string)en_app["bsd_name"] + "-" + (string)en_Ins["bsd_name"] + "-" + "Management Fees";
                    en_Up["bsd_feetype"] = new OptionSetValue(100000001);
                }
            }
            if (i_pmtype == 100000000) // deposit
                en_Up["bsd_reservation"] = en_OE.ToEntityReference(); // quote


            if (i_pmtype == 100000001 || i_pmtype == 100000003) // installment or interest charge
            {
                en_Up["bsd_optionentry"] = en_OE.ToEntityReference(); // OE
                en_Up["bsd_actualgracedays"] = i_outstanding_AppDtl;
                en_Up["bsd_interestchargeamount"] = new Money(d_bsd_interestchargeamount);
            }
            service.Create(en_Up);
        }
        // create APP detail for MISS
        private void create_app_detail_MISS(IOrganizationService serv, Entity en_app, Entity en_Ins, Entity en_OE, int i_pmtype, decimal appAM, int i_outstanding_AppDtl,
            decimal d_bsd_interestchargeamount, int fee_type, Entity Miss_ID)
        {
            Entity en_Up = new Entity("bsd_applydocumentdetail");
            en_Up.Id = Guid.NewGuid();
            string s_type = "";
            switch (i_pmtype)
            {
                case 100000000:
                    s_type = "Deposit";
                    break;
                case 100000001:
                    s_type = "Installment";
                    break;
                case 100000003:
                    s_type = "Interest charge";
                    break;
                case 100000002:
                    s_type = "Fees";
                    break;
                case 100000004:
                    s_type = "Other";
                    break;
            }

            en_Up["bsd_name"] = "Apply document detail-" + (string)en_app["bsd_name"] + "-" + (string)en_Ins["bsd_name"] + "-" + s_type;

            en_Up["bsd_applydocument"] = en_app.ToEntityReference();
            en_Up["statuscode"] = new OptionSetValue(100000000); // confirm - Nhung

            en_Up["bsd_paymenttype"] = new OptionSetValue(i_pmtype);
            en_Up["bsd_installment"] = en_Ins.ToEntityReference();

            en_Up["bsd_amountapply"] = new Money(appAM);

            // bsd_feetype, Maintenace fee = 100.000.000, Management fee = 100.000.001
            if (i_pmtype == 100000002) // interest charge
            {
                en_Up["bsd_optionentry"] = en_OE.ToEntityReference(); // OE
                if (fee_type == 1) // main
                {
                    en_Up["bsd_name"] = "Apply document detail-" + (string)en_app["bsd_name"] + "-" + (string)en_Ins["bsd_name"] + "-" + "Maitenance Fees";
                    en_Up["bsd_feetype"] = new OptionSetValue(100000000);
                }
                else
                {
                    en_Up["bsd_name"] = "Apply document detail-" + (string)en_app["bsd_name"] + "-" + (string)en_Ins["bsd_name"] + "-" + "Management Fees";
                    en_Up["bsd_feetype"] = new OptionSetValue(100000001);
                }
            }
            if (i_pmtype == 100000000) // deposit
                en_Up["bsd_reservation"] = en_OE.ToEntityReference(); // quote
                                                                      // Miscellaneous
            if (i_pmtype == 100000004)
            {
                en_Up["bsd_optionentry"] = en_OE.ToEntityReference(); // OE
                en_Up["bsd_miscellaneous"] = Miss_ID.ToEntityReference();
            }

            if (i_pmtype == 100000001 || i_pmtype == 100000003) // installment or interest charge
            {
                en_Up["bsd_optionentry"] = en_OE.ToEntityReference(); // OE
                if (en_OE.Contains("bsd_signeddadate") || en_OE.Contains("bsd_signedcontractdate"))// OE
                {
                    en_Up["bsd_actualgracedays"] = i_outstanding_AppDtl;
                    en_Up["bsd_interestchargeamount"] = new Money(d_bsd_interestchargeamount);
                }
                else
                {
                    decimal gt = 0;
                    en_Up["bsd_actualgracedays"] = gt;
                    en_Up["bsd_interestchargeamount"] = new Money(gt);
                }
            }
            service.Create(en_Up);
        }
        // Create Applydocument Remaining COA By Thạnh Đỗ
        private void create_Applydocument_RemainingCOA(IOrganizationService service, Entity enApplyDocument, Entity enAdvancePayment,
                                                       decimal bsd_advancepaymentamount, decimal apply, decimal remain, decimal bsd_avPMPaid)
        {
            // Get entity Applydocument Remaining COA
            Entity enApplyDocumentRemainingCOA = new Entity("bsd_applydocumentremainingcoa");
            enApplyDocumentRemainingCOA.Id = new Guid();
            // Load giá trị
            DateTime dateNow = RetrieveLocalTimeFromUTCTime(DateTime.Now);
            enApplyDocumentRemainingCOA["bsd_name"] = "Apply Document Remaining COA-" + (string)enAdvancePayment["bsd_name"];//name
            enApplyDocumentRemainingCOA["bsd_applydate"] = dateNow;//applydate
            enApplyDocumentRemainingCOA["bsd_applydocument"] = enApplyDocument.ToEntityReference();//applydocument
            enApplyDocumentRemainingCOA["bsd_advancepayment"] = enAdvancePayment.ToEntityReference();//advancepayment
            enApplyDocumentRemainingCOA["bsd_advancepaymentamount"] = (Money)enAdvancePayment["bsd_amount"];//advancepaymentamount
            enApplyDocumentRemainingCOA["bsd_advancepaymentapply"] = new Money(apply);//advancepaymentapply
            enApplyDocumentRemainingCOA["bsd_advancepaymentpaid"] = new Money(bsd_avPMPaid);//advancepaymentpaid
            enApplyDocumentRemainingCOA["bsd_advancepaymentremaining"] = new Money(remain);//advancepaymentremaining
            // Tạo Applydocument Remaining COA
            service.Create(enApplyDocumentRemainingCOA);
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
