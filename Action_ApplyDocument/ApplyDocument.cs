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

namespace Action_ApplyDocument
{
    class ApplyDocument
    {
        IOrganizationService service = null;
        IOrganizationServiceFactory factory = null;
        IPluginExecutionContext context = null;
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
        }

        public void checkInput(Entity en_app)
        {
            decimal bsd_differenceamount = en_app.Contains("bsd_differenceamount") ? ((Money)en_app["bsd_differenceamount"]).Value : 0;
            if (bsd_differenceamount < 0)
            {
                throw new InvalidPluginExecutionException("The amount you pay exceeds the amount paid. Please choose another Advance Payment or other Payment Phase!");
            }
            if (!en_app.Contains("bsd_transactiontype"))
                throw new InvalidPluginExecutionException("Please choose 'Type of payment'!");

            if (!en_app.Contains("bsd_receiptdate"))
                throw new InvalidPluginExecutionException("Please select payment actual time");

            // bsd_transactiontype : 1: deposit // 2: installment
            int i_bsd_transactiontype = ((OptionSetValue)en_app["bsd_transactiontype"]).Value;

            strMess.AppendLine("111111111111111");
            // ------------------------------------------- check array input and amount input --------------------------------------------
            // apply document - input advance payment
            decimal d_bsd_amountadvancepayment = en_app.Contains("bsd_amountadvancepayment") ? ((Money)en_app["bsd_amountadvancepayment"]).Value : 0;
            if (d_bsd_amountadvancepayment == 0) throw new InvalidPluginExecutionException("Please choose at least one row of Advance Payment List!");
            if (!en_app.Contains("bsd_arrayadvancepayment")) throw new InvalidPluginExecutionException("Please choose at least one row of Advance Payment List!");
            if (!en_app.Contains("bsd_arrayamountadvance")) throw new InvalidPluginExecutionException("Cannot find any amount of advance payment list!");
            strMess.AppendLine("2222222222222");
            string s_bsd_arrayadvancepayment = en_app.Contains("bsd_arrayadvancepayment") ? (string)en_app["bsd_arrayadvancepayment"] : "";
            string s_bsd_arrayamountadvance = en_app.Contains("bsd_arrayamountadvance") ? (string)en_app["bsd_arrayamountadvance"] : "";
            strMess.AppendLine("s_bsd_arrayadvancepayment: " + s_bsd_arrayadvancepayment);
            strMess.AppendLine("s_bsd_arrayamountadvance: " + s_bsd_arrayamountadvance);
            string[] s_eachAdv = s_bsd_arrayadvancepayment.Split(',');
            string[] s_amAdv = s_bsd_arrayamountadvance.Split(',');
            int i_adv = s_eachAdv.Length;
            strMess.AppendLine("33333333333");
            // installment & deposit list
            decimal d_bsd_totalapplyamount = en_app.Contains("bsd_totalapplyamount") ? ((Money)en_app["bsd_totalapplyamount"]).Value : 0;

            if (d_bsd_totalapplyamount == 0) throw new InvalidPluginExecutionException("Please choose at least one row of " + ((i_bsd_transactiontype == 1) ? " Deposit list!" : " Installment or Interest List!"));
            if (!en_app.Contains("bsd_arraypsdid") && !en_app.Contains("bsd_arrayinstallmentinterest") && !en_app.Contains("bsd_arrayfeesid") && !en_app.Contains("bsd_arraymicellaneousid"))
                throw new InvalidPluginExecutionException("Please choose at least one row of " + ((i_bsd_transactiontype == 1) ? " Deposit list!" : " Installment or Interest List or Fees list or Miscellaneous list!"));

            if (!en_app.Contains("bsd_arrayamountpay") && !en_app.Contains("bsd_arrayinterestamount") && !en_app.Contains("bsd_arrayfeesamount") && !en_app.Contains("bsd_arraymicellaneousamount"))
                throw new InvalidPluginExecutionException("Cannot find any amount of " + ((i_bsd_transactiontype == 1) ? " Deposit list!" : " Installment or Interest List or Fees list or Miscellaneous list!"));
            strMess.AppendLine("4444444444444");
            if (d_bsd_totalapplyamount > d_bsd_amountadvancepayment) throw new InvalidPluginExecutionException("The amout you pay exceeds the amount paid. Please choose another Advance Payment or other Payment Phase!");


            strMess.AppendLine("55555555555");
            //  ----------------------------------- retrieve advance payment   -------------------------------------------------------
            // bien kiem tra lai tong tien cua adv
            // sum remaining
            decimal sumRemainingamount = 0;
            for (int i = 0; i < s_eachAdv.Length; i++)
            {
                strMess.AppendLine("Adv ID: " + s_eachAdv[i].ToString());
                Entity en_Adv = service.Retrieve("bsd_advancepayment", Guid.Parse(s_eachAdv[i]), new ColumnSet(true));
                if (!en_Adv.Contains("statuscode") || ((OptionSetValue)en_Adv["statuscode"]).Value == 100000002)
                    throw new InvalidPluginExecutionException("Advance payment " + (string)en_Adv["bsd_name"] + " has been Revert!");

                if (((OptionSetValue)en_Adv["statuscode"]).Value == 100000001) // pay off
                    throw new InvalidPluginExecutionException("Advance payment " + (string)en_Adv["bsd_name"] + " has been Pay off!");
                strMess.AppendLine("6666666666");
                if (!en_Adv.Contains("bsd_amount")) throw new InvalidPluginExecutionException("Advance payment " + (string)en_Adv["bsd_name"] + " does not contain 'Amount'. Please check again!");
                decimal d_bsd_amount = ((Money)en_Adv["bsd_amount"]).Value;
                decimal d_bsd_paidamount = en_Adv.Contains("bsd_paidamount") ? ((Money)en_Adv["bsd_paidamount"]).Value : 0;
                decimal bsd_refundamount = en_Adv.Contains("bsd_refundamount") ? ((Money)en_Adv["bsd_refundamount"]).Value : 0;
                strMess.AppendLine("d_bsd_paidamount: " + d_bsd_paidamount.ToString());
                // if (!en_Adv.Contains("bsd_remainingamount")) throw new InvalidPluginExecutionException("Advance payment " + (string)en_Adv["bsd_name"] + " does not contain 'Remaining Amount'. Please check again!");
                //if (!en_Adv.Contains("bsd_transferredamount")) throw new InvalidPluginExecutionException("Advance payment " + (string)en_Adv["bsd_name"] + " does not contain 'Transfer Amount'. Please check again!");
                strMess.AppendLine("7777777777");
                //công thức tính lại remainningamount
                //decimal d_bsd_remainingamount = en_Adv.Contains("bsd_remainingamount") ? ((Money)en_Adv["bsd_remainingamount"]).Value : 0;
                // Remainingamount = amount- refundamount - paidamount
                decimal bsd_remainingamount = d_bsd_amount - bsd_refundamount - d_bsd_paidamount;
                strMess.AppendLine("8888888888");
                // Tổng remainningamount
                sumRemainingamount += bsd_remainingamount;
                if (decimal.Parse(s_amAdv[i]) > bsd_remainingamount)
                    throw new InvalidPluginExecutionException("Remaining amount of Advance payment " + (string)en_Adv["bsd_name"] + " is less than require amount. Cannot excute payment!");
            } // end for each s_eachADV
              //so sánh tổng remaing và tổng phải trả
              // Nếu tổng remaing < tổng phải trả thì message
              //throw new InvalidPluginExecutionException("sumRemainingamount: " + sumRemainingamount.ToString() + "d_bsd_totalapplyamount: " + d_bsd_totalapplyamount.ToString());
            if (sumRemainingamount < d_bsd_totalapplyamount)
            {
                throw new InvalidPluginExecutionException("Amount Advance Payment must larger than Total Apply Amount!");
            }
            strMess.AppendLine("checkInput done");
            //throw new InvalidPluginExecutionException("sumRemainingamount: " + sumRemainingamount.ToString());
            //decimal d_tienconlai = d_bsd_amountadvancepayment - d_bsd_totalapplyamount;
            //if (d_tienconlai < 0) throw new InvalidPluginExecutionException("Amount Advance Payment must larger than Total Apply Amount!");
        }
        public void paymentDeposit(Guid quoteId, decimal d_amp, DateTime d_now, Entity en_app)
        {
            Entity en_Resv = service.Retrieve("quote", quoteId, new ColumnSet(new string[] {
                                "bsd_deposittime", "statuscode", "bsd_depositfee", "name", "bsd_totalamountpaid","bsd_projectid",
                                "customerid","bsd_isdeposited"}));

            if (((OptionSetValue)en_Resv["statuscode"]).Value == 6)
                throw new InvalidPluginExecutionException("Reservation " + (string)en_Resv["name"] + " had been cancelled, cannot deposit!");
            if (((OptionSetValue)en_Resv["statuscode"]).Value == 3)
                throw new InvalidPluginExecutionException("Quotation Reservation " + (string)en_Resv["name"] + " had been depositted, cannot payment!");

            Entity en_tmp = new Entity(en_Resv.LogicalName);
            en_tmp.Id = en_Resv.Id;
            //OrganizationRequest req = new OrganizationRequest("bsd_Action_Resv_Gene_PMS");
            ////req["ProjectName"] = "This is a test operation for using Actions in CRM 2013 ";
            //req["Target"] = new EntityReference(en_Resv.LogicalName, en_Resv.Id);
            ////execute the request
            //OrganizationResponse response = service.Execute(req);

            // 161012 - amount pay > deposit amount
            EntityCollection ec_pm1st = Get1st_Resv(en_Resv.Id.ToString());
            Entity en_pm1st = new Entity(ec_pm1st.Entities[0].LogicalName);
            en_pm1st.Id = ec_pm1st[0].Id;

            decimal d_amPhase1st = ec_pm1st.Entities[0].Contains("bsd_amountofthisphase") ? ((Money)ec_pm1st.Entities[0]["bsd_amountofthisphase"]).Value : 0;
            if (d_amPhase1st == 0)
                throw new InvalidPluginExecutionException("Amount of Installment 1 - Reservation " + (string)en_Resv["name"] + " is null. Please check again!");
            bool f_bsd_typeofstartdate = en_pm1st.Contains("bsd_typeofstartdate") ? (bool)en_pm1st["bsd_typeofstartdate"] : false;

            // re-generate pms dtl for Quote
            if (ec_pm1st.Entities[0].Contains("bsd_duedatecalculatingmethod") && ((OptionSetValue)ec_pm1st.Entities[0]["bsd_duedatecalculatingmethod"]).Value == 100000001)
            {
                if (f_bsd_typeofstartdate == true)
                {
                    EntityCollection ec_pmst = Get_ec_Ins_Resv(en_Resv.Id.ToString());
                    reGenerate(service, en_Resv, d_now, ec_pmst);
                }
            }

            //if (f_bsd_typeofstartdate == true)
            //{
            //    // re-generate payment scheme detail
            //    //EntityCollection etnc = get_Workflow(service); // get Workflow Generate payment scheme detail for Resv
            //    //foreach (Entity pro in etnc.Entities)
            //    //{
            //    //    ExecuteWorkflowRequest request = new ExecuteWorkflowRequest()
            //    //    {
            //    //        WorkflowId = pro.Id,
            //    //        EntityId = ((EntityReference)paymentEn["bsd_reservation"]).Id
            //    //    };
            //    //    // Execute the workflow.
            //    //    ExecuteWorkflowResponse response = (ExecuteWorkflowResponse)service.Execute(request);
            //    //    break;
            //    //}

            //    // 170221 - change from call workflow to call action generate pms
            //    // 170121 - call action not call workflow
            //    OrganizationRequest orgreq = new OrganizationRequest("bsd_Action_Resv_Gene_PMS");
            //    //req["ProjectName"] = "This is a test operation for using Actions in CRM 2013 ";
            //    orgreq["Target"] = new EntityReference(en_Resv.LogicalName, en_Resv.Id);
            //    //execute the request
            //    OrganizationResponse respOrg = service.Execute(orgreq);
            //}
            //en_pm1st["bsd_amountwaspaid"] = new Money(d_amp);

            en_pm1st["bsd_balance"] = new Money(d_amPhase1st - d_amp);
            en_pm1st["bsd_depositamount"] = new Money(d_amp);
            service.Update(en_pm1st);

            // total amount paid of Resv
            en_tmp["bsd_totalamountpaid"] = new Money(d_amp);
            //en_tmp["bsd_deposittime"] = d_now;
            en_tmp["bsd_deposittime"] = en_app["bsd_receiptdate"];
            en_tmp["bsd_isdeposited"] = true;

            // en_tmp["bsd_depositfee"] = new Money(pm_amountpay > pm_sotiendot ? pm_sotiendot : pm_amountpay);
            service.Update(en_tmp);
            SetStateRequest setStateRequest1 = new SetStateRequest()
            {
                EntityMoniker = en_Resv.ToEntityReference(),
                State = new OptionSetValue(0),
                Status = new OptionSetValue(100000000),
            };
            service.Execute(setStateRequest1);

            SetStateRequest setStateRequest = new SetStateRequest()
            {
                EntityMoniker = en_Resv.ToEntityReference(),
                State = new OptionSetValue(1),
                Status = new OptionSetValue(3),
            };
            service.Execute(setStateRequest);

            create_app_detail(service, en_app, ec_pm1st.Entities[0], en_Resv, 100000000, d_amp, 0, 0, 0);
        }
        public void paymentInstallment(Entity en_app)
        {
            string s_bsd_arraypsdid = en_app.Contains("bsd_arraypsdid") ? (string)en_app["bsd_arraypsdid"] : "";
            string s_bsd_arrayamountpay = en_app.Contains("bsd_arrayamountpay") ? (string)en_app["bsd_arrayamountpay"] : "";
            //  ------------------------ retreive option entry --------------------------------------
            strMess.AppendLine("10.1");
            Entity en_OE = service.Retrieve("salesorder", ((EntityReference)en_app["bsd_optionentry"]).Id,
                    new ColumnSet(true));

            string optionentryID = en_OE.Id.ToString();
            //EntityReference customerRef = (EntityReference)en_OE["customerid"];
            if (!en_OE.Contains("bsd_unitnumber")) throw new InvalidPluginExecutionException("Cannot find Unit information in Option Entry " + (string)en_OE["name"] + "!");
            //Entity Product = service.Retrieve("product", ((EntityReference)en_OE["bsd_unitnumber"]).Id, new ColumnSet(new string[] { "bsd_handovercondition", "name" }));
            strMess.AppendLine("10.2");
            //decimal d_oe_bsd_totalamount = en_OE.Contains("totalamount") ? ((Money)en_OE["totalamount"]).Value : 0;
            d_oe_bsd_totalamountpaid = en_OE.Contains("bsd_totalamountpaid") ? ((Money)en_OE["bsd_totalamountpaid"]).Value : 0;
            //decimal d_oe_totalpercent = en_OE.Contains("bsd_totalpercent") ? (decimal)en_OE["bsd_totalpercent"] : 0;
            decimal d_oe_bsd_totalamountlessfreight = en_OE.Contains("bsd_totalamountlessfreight") ? ((Money)en_OE["bsd_totalamountlessfreight"]).Value : 0;
            if (d_oe_bsd_totalamountlessfreight == 0) throw new InvalidPluginExecutionException("'Net Selling Price' must be larger than 0");
            strMess.AppendLine("10.3");
            //decimal d_oe_bsd_managementfee = en_OE.Contains("bsd_managementfee") ? ((Money)en_OE["bsd_managementfee"]).Value : 0;
            //decimal d_oe_bsd_freightamount = en_OE.Contains("bsd_freightamount") ? ((Money)en_OE["bsd_freightamount"]).Value : 0;
            //decimal d_oe_amountCalcPercent = d_oe_bsd_totalamount;


            strMess.AppendLine("10.4");
            // --------------------------------------- INS ----------------------------------

            if (s_bsd_arraypsdid != "")
            {
                applyIntallment(en_app, en_OE, s_bsd_arraypsdid, s_bsd_arrayamountpay);
            }

            strMess.AppendLine("10.5");
            // ----------------------- Interestcharge ------------------------------------------
            // s_interest chua cac id cua installment
            // vi user co the click check cac row trong list k theo thu tu nen fai sort lai theo thu tu installment de kiem tra cac installment paid hay chua moi tinh ra
            // tu id nay - kiem tra installment da paid hay chua va lay ra cac thong so can thiet de update dulieu
            // update interest charge cho tat ca du lieu dua vao amount user qdih tra cho cac intestrest charge duoc chon

            string s_bsd_arrayinstallmentinterest = en_app.Contains("bsd_arrayinstallmentinterest") ? (string)en_app["bsd_arrayinstallmentinterest"] : "";
            string s_bsd_arrayinterestamount = en_app.Contains("bsd_arrayinterestamount") ? (string)en_app["bsd_arrayinterestamount"] : "";
            if (s_bsd_arrayinstallmentinterest != "")
            {
                applyInterestcharge(en_app, en_OE, s_bsd_arrayinstallmentinterest, s_bsd_arrayinterestamount);
            }

            strMess.AppendLine("10.6");
            //------------------------------FEES Hồ Code 20180811------------------------
            string s_fees = en_app.Contains("bsd_arrayfeesid") ? (string)en_app["bsd_arrayfeesid"] : "";
            string s_feesAM = en_app.Contains("bsd_arrayfeesamount") ? (string)en_app["bsd_arrayfeesamount"] : "";
            if (s_fees != "")
            {
                applyFees(en_app, en_OE, s_fees, s_feesAM);
            }

            strMess.AppendLine("10.7");
            // ---------------------------- FEES ----------------------------------------

            strMess.AppendLine("10.8");
            // neu MISS maps voi INS nao - kiem tra INS do da Paid hay chua - neu chua thi thong bao ra - & Ignore payment
            // ------------------------ MISS ---------------------------------
            string s_bsd_arraymicellaneousid = en_app.Contains("bsd_arraymicellaneousid") ? (string)en_app["bsd_arraymicellaneousid"] : "";
            string s_bsd_arraymicellaneousamount = en_app.Contains("bsd_arraymicellaneousamount") ? (string)en_app["bsd_arraymicellaneousamount"] : "";
            if (s_bsd_arraymicellaneousid != "")
            {
                applyMicellaneous(en_app, en_OE, s_bsd_arraymicellaneousid, s_bsd_arraymicellaneousamount);
            }


            //------------- end MISS ------------------
            strMess.AppendLine("10.9");
            // OE
            //Entity en_oeUPdate = new Entity(en_OE.LogicalName);
            //en_oeUPdate.Id = en_OE.Id;
            //en_oeUPdate["bsd_totalamountpaid"] = new Money(d_oe_bsd_totalamountpaid);
            //strMess.AppendLine("10.10");
            //service.Update(en_oeUPdate);
        }
        public void createCOA(Entity en_app)
        {
            decimal d_bsd_totalapplyamount = en_app.Contains("bsd_totalapplyamount") ? ((Money)en_app["bsd_totalapplyamount"]).Value : 0;
            //.1 Duyệt item
            decimal delta = 0;
            decimal totalapplyamout = d_bsd_totalapplyamount;
            string s_bsd_arrayadvancepayment = en_app.Contains("bsd_arrayadvancepayment") ? (string)en_app["bsd_arrayadvancepayment"] : "";
            string s_bsd_arrayamountadvance = en_app.Contains("bsd_arrayamountadvance") ? (string)en_app["bsd_arrayamountadvance"] : "";
            string[] s_eachAdv = s_bsd_arrayadvancepayment.Split(',');
            string[] s_amAdv = s_bsd_arrayamountadvance.Split(',');
            decimal[] apply = new decimal[s_eachAdv.Length];
            decimal[] remain = new decimal[s_eachAdv.Length];
            strMess.AppendLine("s_eachAdv: " + s_eachAdv.Length.ToString());
            for (int i = 0; i < s_eachAdv.Length; i++)
            {
                Entity en_Adv = service.Retrieve("bsd_advancepayment", Guid.Parse(s_eachAdv[i]), new ColumnSet(true));
                ;
                strMess.AppendLine("ID: " + s_eachAdv[i].ToString());
                //Số tiền có thể sử dụng
                decimal totalavancedamount = en_Adv.Contains("bsd_amount") ? ((Money)en_Adv["bsd_amount"]).Value : 0;

                decimal bsd_paidamount = en_Adv.Contains("bsd_paidamount") ? ((Money)en_Adv["bsd_paidamount"]).Value : 0;
                strMess.AppendLine("bsd_paidamount: " + bsd_paidamount.ToString());
                decimal bsd_refundamount = en_Adv.Contains("bsd_refundamount") ? ((Money)en_Adv["bsd_refundamount"]).Value : 0;

                decimal bsd_avPMPaid = bsd_paidamount + bsd_refundamount;

                decimal bsd_remainingamount = decimal.Parse(s_amAdv[i]);

                if (totalapplyamout == 0)
                {
                    apply[i] = 0;
                    remain[i] = totalavancedamount - bsd_paidamount;
                }
                else if (totalapplyamout > 0)
                {
                    delta = totalapplyamout - bsd_remainingamount;
                    strMess.AppendLine("delta: " + delta.ToString());
                    if (delta >= 0)
                    {
                        apply[i] = bsd_remainingamount;
                        remain[i] = totalavancedamount - bsd_refundamount - bsd_paidamount - apply[i];
                    }
                    else if (delta < 0)
                    {
                        apply[i] = totalapplyamout;
                        remain[i] = totalavancedamount - bsd_refundamount - bsd_paidamount - apply[i];
                    }
                    // Tính remaining
                    totalapplyamout -= apply[i];
                }
                strMess.AppendLine("totalavancedamount: " + totalavancedamount.ToString());
                strMess.AppendLine("apply: " + apply[i].ToString());
                strMess.AppendLine("remain: " + remain[i].ToString());
                strMess.AppendLine("bsd_avPMPaid: " + bsd_avPMPaid.ToString());
                create_Applydocument_RemainingCOA(service, en_app, en_Adv, totalavancedamount, apply[i], remain[i], bsd_avPMPaid);
            }
        }
        public void updateApplyDocument(Entity en_app)
        {
            decimal d_bsd_totalapplyamount = en_app.Contains("bsd_totalapplyamount") ? ((Money)en_app["bsd_totalapplyamount"]).Value : 0;
            decimal d_tmp = d_bsd_totalapplyamount;
            string s_bsd_arrayadvancepayment = en_app.Contains("bsd_arrayadvancepayment") ? (string)en_app["bsd_arrayadvancepayment"] : "";
            string s_bsd_arrayamountadvance = en_app.Contains("bsd_arrayamountadvance") ? (string)en_app["bsd_arrayamountadvance"] : "";
            string[] s_eachAdv = s_bsd_arrayadvancepayment.Split(',');
            string[] s_amAdv = s_bsd_arrayamountadvance.Split(',');
            int i_adv = s_eachAdv.Length;
            for (int n = 0; n < i_adv; n++)
            {
                if (d_tmp == 0) break;
                // so tien can tra cho applydoc = so tien cua adv check dau tien - k can update nua - chi update cho adv nay thoi
                if (d_tmp == decimal.Parse(s_amAdv[n]))
                {
                    strMess.AppendLine("Up_adv 1");
                    Up_adv(Guid.Parse(s_eachAdv[n]), decimal.Parse(s_amAdv[n])); // payoff
                    d_tmp = 0;
                    break;
                }
                if (d_tmp < decimal.Parse(s_amAdv[n]))
                {
                    strMess.AppendLine("Up_adv 2");
                    Up_adv(Guid.Parse(s_eachAdv[n]), d_tmp); // collect
                    d_tmp = 0;
                    break;
                }
                if (d_tmp > decimal.Parse(s_amAdv[n]))
                {
                    strMess.AppendLine("Up_adv 3");
                    Up_adv(Guid.Parse(s_eachAdv[n]), decimal.Parse(s_amAdv[n])); // payoff -- tiep tu for toi advance pm tiep theo
                    d_tmp -= decimal.Parse(s_amAdv[n]);

                }
                strMess.AppendLine("1111111111111111777777777777");
            }


            //  update status apply document
            strMess.AppendLine("1111111111");
            DateTime d_now = RetrieveLocalTimeFromUTCTime(DateTime.Now);
            // update status code cua apply document = approve
            Entity en_appTmp = new Entity(en_app.LogicalName);
            en_appTmp.Id = en_app.Id;
            en_appTmp["statuscode"] = new OptionSetValue(100000002);
            en_appTmp["bsd_approvaldate"] = d_now;
            en_appTmp["bsd_approver"] = new EntityReference("systemuser", context.UserId);
            service.Update(en_appTmp);
            strMess.AppendLine("aaaaa");
        }
        private void applyIntallment(Entity en_app, Entity en_OE, string s_bsd_arraypsdid, string s_bsd_arrayamountpay)
        {
            strMess.AppendLine("s_bsd_arraypsdid: " + s_bsd_arraypsdid);
            strMess.AppendLine("s_bsd_arrayamountpay: " + s_bsd_arrayamountpay);
            string[] s_psd = s_bsd_arraypsdid.Split(',');
            string[] s_Amp = s_bsd_arrayamountpay.Split(',');

            int i_psd = s_psd.Length;
            EntityCollection ec_ins = get_ecIns(service, s_psd);
            for (int i = 0; i < ec_ins.Entities.Count; i++)
            {
                // ------------------------------- retreive PMSDTL ------------------------------

                Entity en_pms = ec_ins.Entities[i];
                en_pms.Id = ec_ins.Entities[i].Id;
                strMess.AppendLine("bsd_name: " + en_pms["bsd_name"].ToString());
                if (!ec_ins.Entities[i].Contains("statuscode")) throw new InvalidPluginExecutionException("Please check status code of '" + (string)ec_ins.Entities[i]["bsd_name"] + "!");
                int psd_statuscode = ((OptionSetValue)ec_ins.Entities[i]["statuscode"]).Value;
                if (psd_statuscode == 100000001) throw new InvalidPluginExecutionException((string)ec_ins.Entities[i]["bsd_name"] + " has been Paid!");

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


                // k can kiem tra neu installment truoc do da paid hay chua van cho thanh toan installment nay
                // 170308 - Han require

                //// check if the previous installment has been paid or not
                //// kiem tra installment truoc duoc paid hay chua neu chua duoc paid thi thong bao installment nay k duoc payment
                //if (i_phaseNum > 1)
                //{
                //    int i_tmp = i_phaseNum - 1;
                //    if (i_phaseNum == 2)
                //     check_Ins_Paid(service, en_OE.Id, i_tmp);
                //    if (check_Ins_Paid(service, en_OE.Id, i_tmp) == false)
                //    {
                //        if (i_phaseNum == 2)
                //            throw new Exception(i_tmp.ToString() + "_" + check_Ins_Paid(service, en_OE.Id, i_tmp).ToString());
                //    }   //throw new InvalidPluginExecutionException("The previous installment has not paid. Cannot payment for '" + (string)en_pms["bsd_name"] + "'!");
                //}
                //



                // --------------------- Update ------------------------------------
                decimal d_ampay = 0;
                //d_ampay = so tien user nhap - ( installment con 500 tr chua tra - user co the nhap tu 0 den 500 tr)
                // do user co the thay doi so tien can phai tra cua tung installment nen phai kiem tra tu amount pay trong mang so voi bsd_balance cua installment
                // sap xep thu tu cac installment trong array - khi do thu tu trong mang installment va thu tu cua amount can phai tra k doi ()
                // dua vao id cua installment dag duoc tra - chay vong lap de lay id trung voi id installment nay trong mang ma lay ra so tien tuong ung trong mang amount
                // tinh toan amount pay & update vao cac field trong installment & OE

                bool f_checkIDagain = false;

                for (int m = 0; m < s_psd.Length; m++)
                {

                    if (en_pms.Id.ToString() == s_psd[m])
                    {
                        d_ampay = decimal.Parse(s_Amp[m]);
                        f_checkIDagain = true;
                        break;
                    }
                }

                if (f_checkIDagain == false) throw new InvalidPluginExecutionException("Cannot find ID of '" + (string)ec_ins.Entities[i]["bsd_name"] + "' in Installment array!");
                bool f_check_1st = false;
                f_check_1st = check_Ins_Paid(service, en_OE.Id, 1);
                strMess.AppendLine("so sanh ampay voi baslane");

                // --------------------------- payment --------------------------------------------------
                int i_late = 0;
                int i_outstanding_AppDtl = 0;
                decimal d_outstandingAM = 0;
                if (i_phaseNum == 1)
                    // 170314 thay the waiver amount = waiver installment
                    // pm_balance = psd_amountPhase - psd_amountPaid - psd_deposit - psd_waiver;// psd_balance - psd_deposit
                    psd_bsd_balance = psd_amountPhase - psd_amountPaid - psd_deposit - psd_waiverIns;// psd_balance - psd_deposit
                else psd_bsd_balance = psd_amountPhase - psd_amountPaid - psd_waiverIns; // = psd_balance
                strMess.AppendLine("psd_bsd_balance: " + psd_bsd_balance.ToString());
                strMess.AppendLine("d_ampay: " + d_ampay.ToString());
                DateTime d_receipt1 = new DateTime();
                if (en_pms.Contains("bsd_duedate"))
                {
                    //DateTime d_receipt = d_now;
                    d_receipt1 = (DateTime)en_app["bsd_receiptdate"];


                }
                // -------------------- d_ampay > psd_bsd_balance ----------------------------
                if (d_ampay >= psd_bsd_balance) // cap nhat installment
                {
                    psd_amountPaid += psd_bsd_balance;
                    psd_statuscode = 100000001;

                    // ------------------------------ interest charge amount for this payment time --------------------------
                    // check interest charge amount
                    if (ec_ins.Entities[i].Contains("bsd_duedate"))
                    {

                        //DateTime d_receipt = d_now;
                        DateTime d_receipt = (DateTime)en_app["bsd_receiptdate"];
                        checkInterest(ec_ins.Entities[i], d_receipt, psd_bsd_balance, ref i_late, ref d_outstandingAM);
                    } // end if (ec_ins.Entities[i].Contains("bsd_duedate"))
                    strMess.AppendLine("i_late: " + i_late.ToString());
                    strMess.AppendLine("d_outstandingAM: " + d_outstandingAM.ToString());

                    d_oe_bsd_totalamountpaid += psd_bsd_balance;
                    DateTime d_now = RetrieveLocalTimeFromUTCTime(DateTime.Now);
                    Up_Ins_OE(en_pms, en_OE, psd_amountPaid, 0, psd_statuscode, psd_bsd_balance, f_check_1st, d_now, i_late, d_outstandingAM);

                    // CREATE APP DETAIL
                    create_app_detail(service, en_app, en_pms, en_OE, 100000001, psd_bsd_balance, i_late, d_outstandingAM, 0);

                } //  end if (d_ampay >= psd_bsd_balance)
                  //-------------------- d_ampay > psd_bsd_balance -------------------------

                // -------------------- d_ampay < psd_bsd_balance ----------------------------
                if (d_ampay < psd_bsd_balance) // not paid
                {

                    psd_statuscode = 100000000;
                    psd_amountPaid += d_ampay;
                    psd_bsd_balance -= d_ampay;
                    // check interest charge amount

                    // interest charge amount for this payment time
                    // check interest charge amount
                    if (en_pms.Contains("bsd_duedate"))
                    {
                        //DateTime d_receipt = d_now;
                        DateTime d_receipt = (DateTime)en_app["bsd_receiptdate"];

                        checkInterest(en_pms, d_receipt, d_ampay, ref i_late, ref d_outstandingAM);
                    }
                    strMess.AppendLine("i_late: " + i_late.ToString());
                    strMess.AppendLine("d_outstandingAM: " + d_outstandingAM.ToString());

                    d_oe_bsd_totalamountpaid += d_ampay;
                    DateTime d_now = RetrieveLocalTimeFromUTCTime(DateTime.Now);
                    Up_Ins_OE(en_pms, en_OE, psd_amountPaid, psd_bsd_balance, psd_statuscode, d_ampay, f_check_1st, d_now, i_late, d_outstandingAM);
                    create_app_detail(service, en_app, en_pms, en_OE, 100000001, d_ampay, i_late, d_outstandingAM, 0);
                }
                //---- end ampay < psd_bsd_balance ------

                //----- end payment -----------

                //-------- Update --------------------
            } // end for each installment
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
        private void applyInterestcharge(Entity en_app, Entity en_OE, string s_bsd_arrayinstallmentinterest, string s_bsd_arrayinterestamount)
        {
            if (s_bsd_arrayinterestamount == "") throw new InvalidPluginExecutionException("Cannot find any Intest Amount Array. Please check again!");
            string[] s_interID = s_bsd_arrayinstallmentinterest.Split(',');
            string[] s_interAM = s_bsd_arrayinterestamount.Split(',');
            decimal d_ampay = 0;

            // ------------ retrieve installment & payment -------------------------------
            EntityCollection ec_PMSDTL = get_ecIns(service, s_interID);
            if (ec_PMSDTL.Entities.Count < 0) throw new InvalidPluginExecutionException("Cannot find any Installment ID with Interest charge array!");

            for (int j = 0; j < ec_PMSDTL.Entities.Count; j++)
            {
                Entity en_interIns = ec_PMSDTL.Entities[j];
                en_interIns.Id = ec_PMSDTL.Entities[j].Id;

                //if (en_interIns.Contains("statuscode") && ((OptionSetValue)en_interIns["statuscode"]).Value == 100000000)
                //    throw new InvalidPluginExecutionException((string)en_interIns["bsd_name"] + " has not been paid. Cannot payment for Interestcharge!");
                decimal psd_bsd_interestchargeamount = en_interIns.Contains("bsd_interestchargeamount") ? ((Money)en_interIns["bsd_interestchargeamount"]).Value : 0;
                decimal psd_bsd_interestwaspaid = en_interIns.Contains("bsd_interestwaspaid") ? ((Money)en_interIns["bsd_interestwaspaid"]).Value : 0;
                //thanhdo
                decimal psd_bsd_waiveramount = en_interIns.Contains("bsd_waiverinterest") ? ((Money)en_interIns["bsd_waiverinterest"]).Value : 0;

                int psd_bsd_interestchargestatus = en_interIns.Contains("bsd_interestchargestatus") ? ((OptionSetValue)en_interIns["bsd_interestchargestatus"]).Value : 0;
                decimal psd_actualgracedays = en_interIns.Contains("bsd_actualgracedays") ? (int)en_interIns["bsd_actualgracedays"] : 0;
                int statuscode = en_interIns.Contains("statuscode") ? ((OptionSetValue)en_interIns["statuscode"]).Value : 0;
                if (psd_bsd_interestchargestatus == 100000001) throw new InvalidPluginExecutionException("The Interest charge of " + (string)en_interIns["bsd_name"] + " has been paid!");

                bool f_checkIDagain = false;
                decimal d_balance = psd_bsd_interestchargeamount - psd_bsd_interestwaspaid - psd_bsd_waiveramount;
                //throw new InvalidPluginExecutionException("psd_bsd_interestchargeamount :" + psd_bsd_interestchargeamount.ToString()+ "psd_bsd_interestwaspaid: "+ psd_bsd_interestwaspaid.ToString()+ "psd_bsd_waiveramount :"+ psd_bsd_waiveramount.ToString());
                for (int m = 0; m < s_interID.Length; m++)
                {
                    if (en_interIns.Id.ToString() == s_interID[m])
                    {
                        d_ampay = decimal.Parse(s_interAM[m]);
                        f_checkIDagain = true;
                        break;
                    }
                }

                if (f_checkIDagain == false) throw new InvalidPluginExecutionException("Cannot find '" + (string)en_interIns["bsd_name"] + "' in Interest charge array!");
                //throw new InvalidPluginExecutionException(d_balance.ToString() + "---" + d_ampay.ToString());
                //  ---------------------------------- payment -------------------------------------
                if (d_ampay > d_balance)
                {
                    throw new InvalidPluginExecutionException("Input amount was larger than balance Interest charge amount of " + (string)en_interIns["bsd_name"] + ". Please check again!");
                }
                if (d_ampay == d_balance) // cap nhat installment
                {
                    psd_bsd_interestwaspaid += d_balance;
                    if (statuscode == 100000001)
                    {
                        psd_bsd_interestchargestatus = 100000001;
                    }
                    else
                    {
                        psd_bsd_interestchargestatus = 100000000;
                    }

                    Update_Interest(service, en_interIns, psd_bsd_interestwaspaid, psd_bsd_interestchargestatus);
                    create_app_detail(service, en_app, en_interIns, en_OE, 100000003, d_balance, 0, 0, 0);

                }

                if (d_ampay < d_balance) // not paid
                {
                    psd_bsd_interestwaspaid += d_ampay;
                    psd_bsd_interestchargestatus = 100000000;
                    Update_Interest(service, en_interIns, psd_bsd_interestwaspaid, psd_bsd_interestchargestatus);
                    create_app_detail(service, en_app, en_interIns, en_OE, 100000003, d_ampay, 0, 0, 0);
                }


            }// end for int j = 0 ; j < ec_PMSDTL.count
        }
        private void applyFees(Entity en_app, Entity en_OE, string s_fees, string s_feesAM)
        {
            strMess.AppendLine("0000000000000");
            if (!en_app.Contains("bsd_units"))
                throw new InvalidPluginExecutionException("Please check Units of Apply Document!");
            Entity en_product = service.Retrieve(((EntityReference)en_app["bsd_units"]).LogicalName, ((EntityReference)en_app["bsd_units"]).Id, new ColumnSet(new string[] {
                            "bsd_handovercondition", "name","bsd_opdate"
                            }));
            strMess.AppendLine("111111111111");
            //if (!en_product.Contains("bsd_opdate") || (DateTime)en_product["bsd_opdate"] == null)
            //    throw new InvalidPluginExecutionException("Product " + (string)en_product["name"] + " have not contain OP Date. Cannot Payment fees. Please check again!");
            string[] arrId = s_fees.Split(',');
            string[] arrFeeAmount = s_feesAM.Split(',');
            for (int i = 0; i < arrId.Length; i++)
            {
                strMess.AppendLine("22222222222");
                string[] arr = arrId[i].Split('_');
                string installmentid = arr[0];
                string type = arr[1];
                decimal fee = decimal.Parse(arrFeeAmount[i].ToString());
                Entity enInstallment = getInstallment(service, installmentid);
                bool f_main = (enInstallment.Contains("bsd_maintenancefeesstatus")) ? (bool)enInstallment["bsd_maintenancefeesstatus"] : false;
                bool f_mana = (enInstallment.Contains("bsd_managementfeesstatus")) ? (bool)enInstallment["bsd_managementfeesstatus"] : false;

                decimal d_bsd_maintenanceamount = enInstallment.Contains("bsd_maintenanceamount") ? ((Money)enInstallment["bsd_maintenanceamount"]).Value : 0;
                decimal d_bsd_managementamount = enInstallment.Contains("bsd_managementamount") ? ((Money)enInstallment["bsd_managementamount"]).Value : 0;

                decimal d_bsd_maintenancefeepaid = enInstallment.Contains("bsd_maintenancefeepaid") ? ((Money)enInstallment["bsd_maintenancefeepaid"]).Value : 0;
                decimal d_bsd_managementfeepaid = enInstallment.Contains("bsd_managementfeepaid") ? ((Money)enInstallment["bsd_managementfeepaid"]).Value : 0;

                decimal d_bsd_maintenancefeewaiver = enInstallment.Contains("bsd_maintenancefeewaiver") ? ((Money)enInstallment["bsd_maintenancefeewaiver"]).Value : 0;
                decimal d_bsd_managementfeewaiver = enInstallment.Contains("bsd_managementfeewaiver") ? ((Money)enInstallment["bsd_managementfeewaiver"]).Value : 0;

                decimal d_mainBL = d_bsd_maintenanceamount - d_bsd_maintenancefeepaid - d_bsd_maintenancefeewaiver;
                decimal d_manaBL = d_bsd_managementamount - d_bsd_managementfeepaid - d_bsd_managementfeewaiver;
                Entity en_INS_update = new Entity(enInstallment.LogicalName);
                en_INS_update.Id = enInstallment.Id;
                switch (type)
                {
                    case "main":
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
                        strMess.AppendLine("333333333333");
                        create_app_detail(service, en_app, enInstallment, en_OE, 100000002, fee, 0, 0, 1);
                        en_INS_update["bsd_maintenancefeesstatus"] = f_main;
                        en_INS_update["bsd_maintenancefeepaid"] = new Money(d_bsd_maintenancefeepaid);
                        strMess.AppendLine("444444444444");
                        break;
                    case "mana":
                        strMess.AppendLine("555555555555");
                        if (f_mana == true) throw new InvalidPluginExecutionException("Management fees had been paid. Please check again!");
                        strMess.AppendLine("66666666666666");
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
                        strMess.AppendLine("7777777");
                        en_INS_update["bsd_managementfeesstatus"] = f_mana;
                        en_INS_update["bsd_managementfeepaid"] = new Money(d_bsd_managementfeepaid);
                        break;
                }
                strMess.AppendLine("88888888888888");
                service.Update(en_INS_update);
                strMess.AppendLine("999999999999");

            }
        }
        private void applyMicellaneous(Entity en_app, Entity en_OE, string s_bsd_arraymicellaneousid, string s_bsd_arraymicellaneousamount)
        {
            strMess.AppendLine("10.8");
            if (s_bsd_arraymicellaneousamount == "") throw new InvalidPluginExecutionException("Cannot find any Miscellaneous amount array!");
            // 170808 Miscellinous
            string[] s_Miss_id = { };
            string[] s_Miss_AM = { };
            s_Miss_id = s_bsd_arraymicellaneousid.Split(',');
            s_Miss_AM = s_bsd_arraymicellaneousamount.Split(',');
            EntityCollection ec_MIS = get_ecMIS(service, s_Miss_id, en_OE.Id.ToString());

            if (ec_MIS.Entities.Count <= 0) throw new InvalidPluginExecutionException("There's not any Miscellaneous found!");

            for (int i = 0; i < ec_MIS.Entities.Count; i++)
            {
                if (!ec_MIS.Entities[i].Contains("bsd_installment"))
                {
                    throw new InvalidPluginExecutionException("Miscellaneous has Installment is null. Please check Miscellaneous - " + (string)ec_MIS.Entities[i]["bsd_name"] + "!");
                }
                else
                {
                    Entity en_Ins_MIS = service.Retrieve(((EntityReference)ec_MIS.Entities[i]["bsd_installment"]).LogicalName, ((EntityReference)ec_MIS.Entities[i]["bsd_installment"]).Id,
                        new ColumnSet(new string[] { "bsd_paymentschemedetailid", "bsd_name", "statuscode" }));
                    int i_INS_statuscode = en_Ins_MIS.Contains("statuscode") ? ((OptionSetValue)en_Ins_MIS["statuscode"]).Value : 100000000;
                    //if (i_INS_statuscode == 100000000) throw new InvalidPluginExecutionException("Installment "+(string)en_Ins_MIS["bsd_name"] +" has not been paid, cannot payment for Miscellaneous " + (string)ec_MIS.Entities[i]["bsd_name"]+"!");

                    // MISS has been paid
                    int f_paid = ec_MIS.Entities[i].Contains("statuscode") ? ((OptionSetValue)ec_MIS.Entities[i]["statuscode"]).Value : 1;

                    if (f_paid == 100000000)
                        throw new InvalidPluginExecutionException("Miscellaneous " + (string)ec_MIS.Entities[i]["bsd_name"] + " has been paid");

                    decimal d_amp = 0;
                    bool f_checkIDagain = false;
                    for (int m = 0; m < s_Miss_id.Length; m++)
                    {
                        if (ec_MIS.Entities[i].Id.ToString() == s_Miss_id[m])
                        {
                            d_amp = decimal.Parse(s_Miss_AM[m]);
                            f_checkIDagain = true;
                            break;
                        }
                    }
                    if (f_checkIDagain == false) throw new InvalidPluginExecutionException("Cannot find ID of Miscellaneous " + (string)ec_MIS.Entities[i]["bsd_name"] + "' in Miscellaneous array!");

                    decimal d_MI_paid = ec_MIS.Entities[i].Contains("bsd_paidamount") ? ((Money)ec_MIS.Entities[i]["bsd_paidamount"]).Value : 0;
                    decimal d_MI_balance = ec_MIS.Entities[i].Contains("bsd_balance") ? ((Money)ec_MIS.Entities[i]["bsd_balance"]).Value : 0;
                    //decimal d_MI_am = ec_MIS.Entities[i].Contains("bsd_amount") ? ((Money)ec_MIS.Entities[i]["bsd_amount"]).Value : 0;
                    decimal d_MI_am = ec_MIS.Entities[i].Contains("bsd_totalamount") ? ((Money)ec_MIS.Entities[i]["bsd_totalamount"]).Value : 0;

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


                    //if (d_amp == d_MI_balance)
                    //{
                    //    f_paid = true;
                    //    d_MI_balance = 0;
                    //    d_MI_paid = d_MI_am;

                    //} // end d_amp == d_MI_balance
                    //else if (d_amp < d_MI_balance)
                    //{
                    //    f_paid = false;
                    //    d_MI_balance -= d_amp;
                    //    d_MI_paid += d_amp;

                    //} // end d_amp < d_MI_balance

                    //else throw new InvalidPluginExecutionException("Input amount is larger than balance amount of Miscellaneous " + (string)ec_MIS.Entities[i]["bsd_name"]);

                    // update
                    Entity en_up_MIS = new Entity(ec_MIS.Entities[i].LogicalName);
                    en_up_MIS.Id = ec_MIS.Entities[i].Id;
                    en_up_MIS["bsd_paidamount"] = new Money(d_MI_paid);
                    en_up_MIS["bsd_balance"] = new Money(d_MI_balance);
                    en_up_MIS["statuscode"] = new OptionSetValue(f_paid);
                    service.Update(en_up_MIS);

                    //throw new InvalidPluginExecutionException(d_MI_paid.ToString());

                    // app detail for MISS
                    create_app_detail_MISS(service, en_app, en_Ins_MIS, en_OE, 100000004, d_amp, 0, 0, 0, ec_MIS.Entities[i]);
                }

            }// end for
        }
        private EntityCollection Get1st_Resv(string resvID)
        {
            QueryExpression query = new QueryExpression("bsd_paymentschemedetail");

            query.ColumnSet = new ColumnSet(new string[] { "bsd_name", "bsd_depositamount", "bsd_ordernumber", "bsd_paymentschemedetailid", "statuscode", "bsd_amountwaspaid", "bsd_balance", "bsd_amountofthisphase" });
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
                <all-attributes />
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
        private EntityCollection Get_ec_Ins_Resv(string resvID)
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
                    </filter>
                    <order attribute='bsd_ordernumber' />
                  </entity>
            </fetch>";

            fetchXml = string.Format(fetchXml, resvID);
            EntityCollection entc = service.RetrieveMultiple(new FetchExpression(fetchXml));
            return entc;
        }
        private EntityCollection get_ecMIS(IOrganizationService crmservices, string[] s_id, string oeID)
        {
            string fetchXml =
                          @"<fetch version='1.0' output-format='xml-platform' mapping='logical' distinct='false'>
                                  <entity name='bsd_miscellaneous'>
                                    <attribute name='bsd_miscellaneousid' />
                                    <attribute name='bsd_name' />
                                    <attribute name='createdon' />
                                    <attribute name='bsd_waiveramount' />
                                    <attribute name='bsd_vatamount' />
                                    <attribute name='bsd_units' />
                                    <attribute name='bsd_type' />
                                    <attribute name='bsd_totalamount' />
                                    <attribute name='statuscode' />
                                    <attribute name='bsd_project' />
                                    <attribute name='bsd_paidamount' />
                                    <attribute name='bsd_optionentry' />
                                    <attribute name='bsd_miscellaneousnumber' />
                                    <attribute name='bsd_miscellaneouscodesams' />
                                    <attribute name='bsd_installmentnumber' />
                                    <attribute name='bsd_installment' />
                                    <attribute name='bsd_duedate' />
                                    <attribute name='bsd_balance' />
                                    <attribute name='bsd_amount' />
                                    <order attribute='bsd_name' descending='false' />
                                    <filter type='and'>
                                      <condition attribute='bsd_optionentry' operator='eq' uitype='salesorder' value='{0}' />";
            fetchXml += "<condition attribute='bsd_miscellaneousid' operator='in' >";
            //fetchXml += "<filter type='or' >";
            for (int i = 0; i < s_id.Length; i++)
            {
                fetchXml += @"<value>" + Guid.Parse(s_id[i]) + "</value>";
                //fetchXml += "<condition attribute='bsd_miscellaneousid' operator='eq' value ='" + s_id[i].ToString() + "' />";
            }

            fetchXml += "</condition>";
            fetchXml += @"</filter>                       
                          </entity>
                        </fetch>";
            fetchXml = string.Format(fetchXml, oeID, s_id);
            EntityCollection entc = crmservices.RetrieveMultiple(new FetchExpression(fetchXml));
            return entc;
        }
        private void reGenerate(IOrganizationService serv, Entity enQuote, DateTime dt_date, EntityCollection ec_pms)
        {
            EntityReference paymentScheme = (EntityReference)enQuote["bsd_paymentscheme"];
            QueryExpression q = new QueryExpression("bsd_paymentschemedetail");

            q.ColumnSet = new ColumnSet(new string[] {
                "bsd_name",
                "bsd_paymentscheme",
                "bsd_withindate",
                "bsd_emailreminderforeigner",
                "bsd_nextperiodtype",
                "bsd_numberofnextmonth",
                "bsd_numberofnextdays",
                "bsd_datepaymentofmonthly",
                "bsd_typepayment",
                "bsd_number",
                "bsd_nextdaysofendphase",
                "bsd_duedatecalculatingmethod",
                "bsd_lastinstallment",
                "bsd_fixeddate",
                "bsd_method",
                "bsd_percent"

           });

            q.Criteria = new FilterExpression(LogicalOperator.And);
            q.Criteria.AddCondition(new ConditionExpression("bsd_paymentscheme", ConditionOperator.Equal, paymentScheme.Id));
            q.Criteria.AddCondition(new ConditionExpression("bsd_optionentry", ConditionOperator.Null));
            q.Criteria.AddCondition(new ConditionExpression("bsd_reservation", ConditionOperator.Null));
            q.Criteria.AddCondition(new ConditionExpression("bsd_quotation", ConditionOperator.Null));
            q.AddOrder("bsd_ordernumber", OrderType.Ascending);
            EntityCollection ec_ins = service.RetrieveMultiple(q);

            Entity pms = service.Retrieve(paymentScheme.LogicalName, paymentScheme.Id,
                 new ColumnSet(new string[] { "bsd_paymentschemecode", "bsd_startdate", "bsd_name", "bsd_paymentschemeid" }));
            int i_dueCalMethod = -1;
            int orderNumber = 0;
            int i_nextMonth = 1;
            for (int i = 0; i < ec_ins.Entities.Count; i++)
            {
                if (ec_ins.Entities[i].Contains("bsd_duedatecalculatingmethod"))
                {
                    i_dueCalMethod = ((OptionSetValue)ec_ins.Entities[i]["bsd_duedatecalculatingmethod"]).Value;
                    if (i == 0 && i_dueCalMethod != 100000001)
                        break;
                    if (i_dueCalMethod == 100000002 || i_dueCalMethod == 100000000)
                        break;
                    if (i_dueCalMethod == 100000001)
                    {
                        int i_bsd_nextperiodtype = (int)((OptionSetValue)ec_ins.Entities[i]["bsd_nextperiodtype"]).Value;
                        int i_paymentdatemonthly = 0;
                        if (ec_ins.Entities[i].Contains("bsd_datepaymentofmonthly"))
                            i_paymentdatemonthly = (int)ec_ins.Entities[i]["bsd_datepaymentofmonthly"];
                        //default or null
                        int? payment_type = ec_ins.Entities[i].Contains("bsd_typepayment") ? (int?)((OptionSetValue)ec_ins.Entities[i]["bsd_typepayment"]).Value : null;

                        if (i_paymentdatemonthly != 0)
                            dt_date = new DateTime(dt_date.Year, dt_date.Month, i_paymentdatemonthly);
                        Entity en_up = new Entity();
                        if (payment_type == null || payment_type == 1)//default or month
                        {
                            double extraDay = 0;
                            int type = ((OptionSetValue)ec_ins.Entities[i]["bsd_nextperiodtype"]).Value;

                            if (type == 1)//month
                            {
                                i_nextMonth = ((int)ec_ins.Entities[i]["bsd_numberofnextmonth"]);
                                // if (i_nextMonth > 12) throw new InvalidPluginExecutionException("Number of next month must be less than or equal 12!");
                                dt_date = dt_date.AddMonths(i_nextMonth);
                            }
                            else if (type == 2)//day
                            {
                                extraDay = double.Parse(ec_ins.Entities[i]["bsd_numberofnextdays"].ToString());
                                dt_date = dt_date.AddDays(extraDay);
                            }
                            en_up.LogicalName = ec_pms.Entities[orderNumber].LogicalName;
                            en_up.Id = ec_pms.Entities[orderNumber].Id;
                            en_up["bsd_duedate"] = dt_date;
                            service.Update(en_up);

                            orderNumber++;
                        }
                        else if (payment_type == 2)//times
                        {
                            int number = (int)ec_ins.Entities[i]["bsd_number"];
                            int i_bsd_nextdaysofendphase = 0;
                            // if bsd_nextdaysofendphase not null - the last installment of array auto Ins must  be plus the day of this field
                            if (ec_ins.Entities[i].Contains("bsd_nextdaysofendphase"))
                            {
                                i_bsd_nextdaysofendphase = (int)ec_ins.Entities[i]["bsd_nextdaysofendphase"];
                            }
                            for (int j = 0; j < number; j++)
                            {
                                if (j == number - 1)
                                    dt_date = dt_date.AddDays(i_bsd_nextdaysofendphase);
                                en_up.LogicalName = ec_pms.Entities[orderNumber].LogicalName;
                                en_up.Id = ec_pms.Entities[orderNumber].Id;
                                en_up["bsd_duedate"] = dt_date;
                                service.Update(en_up);
                                orderNumber++;
                            }
                        }
                    }
                }
            }
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
        private void calc_InterestCharge(Entity e_InterMaster, int i_day, decimal amountPay, Entity OE, Entity en_PmsDtl, IOrganizationService serv, out int i_late, out decimal d_interAM)
        {
            if (!e_InterMaster.Contains("bsd_termsinterestpercentage")) throw new InvalidPluginExecutionException("Please input 'Terms interest' data in Interest rate master:" + (string)e_InterMaster["bsd_name"]);
            decimal d_percent = (decimal)e_InterMaster["bsd_termsinterestpercentage"];
            Entity en_proj = serv.Retrieve("bsd_project", ((EntityReference)OE["bsd_project"]).Id, new ColumnSet(new string[] { "bsd_name", "bsd_dailyinterestchargebank" }));
            bool f_bsd_dailyinterestchargebank = en_proj.Contains("bsd_dailyinterestchargebank") ? (bool)en_proj["bsd_dailyinterestchargebank"] : false;

            decimal d_dailyinterest = 0;
            if (f_bsd_dailyinterestchargebank)
            {
                EntityCollection ec_bsd_dailyinterestrate = get_ec_bsd_dailyinterestrate(serv, en_proj.Id);

                Entity en_bsd_dailyinterestrate = ec_bsd_dailyinterestrate.Entities[0];
                en_bsd_dailyinterestrate.Id = ec_bsd_dailyinterestrate.Entities[0].Id;

                if (!en_bsd_dailyinterestrate.Contains("bsd_interestrate")) throw new InvalidPluginExecutionException("Can not find Daily Interestrate for Project "
                                                                               + (string)en_proj["bsd_name"] + " in master data. Please check again!");
                d_dailyinterest = (decimal)en_bsd_dailyinterestrate["bsd_interestrate"];
            }
            d_percent = (d_percent + d_dailyinterest);

            if (!e_InterMaster.Contains("bsd_intereststartdatetype")) throw new InvalidPluginExecutionException("Please input 'Interest Start Date Type' data in Interest rate master:" + (string)e_InterMaster["bsd_name"]);

            decimal rangeAmount = e_InterMaster.Contains("bsd_toleranceinterestamount") ? (((Money)e_InterMaster["bsd_toleranceinterestamount"]).Value) : 0;
            decimal rangePercent = e_InterMaster.Contains("bsd_toleranceinterestpercentage") ? ((decimal)e_InterMaster["bsd_toleranceinterestpercentage"]) : 0;

            decimal interestcharge_percent = d_percent / 100 * i_day;

            decimal interestcharge_amount = Convert.ToDecimal(amountPay) * interestcharge_percent;

            decimal sum_Inr_AM = SumInterestAM_OE(service, OE.Id);  // get sum interests charge in payment schemedetail of OE

            decimal sum_temp = sum_Inr_AM + interestcharge_amount; // tong interestcharge tinh luon dot nay
                                                                   // 170224 - @Han confirm - su dung field net selling price de tinh interest charge  - k dung total amount
                                                                   //decimal oeAM = OE.Contains("totalamount") ? ((Money)OE["totalamount"]).Value : 0;
            decimal d_oe_bsd_totalamountlessfreight = OE.Contains("totalamount") ? ((Money)OE["totalamount"]).Value : 0;
            if (d_oe_bsd_totalamountlessfreight == 0) throw new InvalidPluginExecutionException("'Net Selling Price' must be larger than 0");
            decimal range_oeAM = 0;

            decimal tmp_rangeAM = 0;
            // tinh total range interertcharge - dua vao % va so tien
            // neu range nao chạm mức trước thì tính range đó.
            int i_tmp1 = 0;
            decimal d_tmp2 = 0;
            if (rangePercent > 0)
            {
                range_oeAM = d_oe_bsd_totalamountlessfreight * rangePercent / 100;
                if (rangeAmount > 0)
                {
                    if (range_oeAM > rangeAmount) tmp_rangeAM = rangeAmount;
                    else tmp_rangeAM = range_oeAM;
                }
                else tmp_rangeAM = range_oeAM;
            }
            else tmp_rangeAM = rangeAmount > 0 ? rangeAmount : 0;

            if (sum_temp >= tmp_rangeAM)
            {
                interestcharge_amount = (tmp_rangeAM > sum_Inr_AM) ? (tmp_rangeAM - sum_Inr_AM) : 0;
            }

            // UPDATE interestcharge cua Installment ( payment shemedetail)
            if (interestcharge_amount > 0)
            {
                if (OE.Contains("bsd_signedcontractdate") || OE.Contains("bsd_signeddadate"))
                {
                    decimal d_interest = en_PmsDtl.Contains("bsd_interestchargeamount") ? ((Money)en_PmsDtl["bsd_interestchargeamount"]).Value : 0;

                    // update interestcharge / paymentdetail
                    Entity tmp = new Entity(en_PmsDtl.LogicalName);
                    tmp.Id = en_PmsDtl.Id;
                    decimal a = d_interest + Convert.ToDecimal(interestcharge_amount);
                    if (a < 0) a = 0;
                    d_tmp2 = a;
                    tmp["bsd_interestchargeamount"] = new Money(a);
                    int i_lateTmp = en_PmsDtl.Contains("bsd_actualgracedays") ? (int)en_PmsDtl["bsd_actualgracedays"] : 0;

                    if (i_day >= i_lateTmp)
                    {
                        tmp["bsd_actualgracedays"] = i_day;
                    }
                    serv.Update(tmp);
                }
                i_tmp1 = i_day;
                //// update OE
                //int lateday = OE.Contains("bsd_totallatedays") ? (int)OE["bsd_totallatedays"] : 0;

                //Entity oe_tmp = new Entity(OE.LogicalName);
                //oe_tmp.Id = OE.Id;
                //if (i_day >= i_lateTmp)
                //{
                //    i_day = i_day - i_lateTmp;
                //    oe_tmp["bsd_totallatedays"] = lateday + i_day;
                //    serv.Update(oe_tmp);
                //}
            }
            i_late = i_tmp1;
            d_interAM = interestcharge_amount;
        }
        private decimal SumInterestAM_OE(IOrganizationService crmservices, Guid OEID)
        {
            decimal sumAmount = 0;
            //StringBuilder xml = new StringBuilder();
            //xml.AppendLine("<fetch version='1.0' output-format='xml-platform' mapping='logical' distinct='false' aggregate='true'>");
            //xml.AppendLine("<entity name='bsd_paymentschemedetail'>");
            //xml.AppendLine("<attribute name='bsd_interestchargeamount' aggregate='sum' alias='sumInterestAmount'/>");
            //xml.AppendLine("<filter type='and'>");
            //xml.AppendLine(string.Format("<condition attribute='bsd_optionentry' operator='eq' value='{0}'/>", OEID));
            //xml.AppendLine("</filter>");
            //xml.AppendLine("</entity>");
            //xml.AppendLine("</fetch>");

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

            return sumAmount;
        }
        private EntityCollection get_ec_bsd_dailyinterestrate(IOrganizationService crmservices, Guid projID)
        {
            string fetchXml =
                          @"<fetch version='1.0' output-format='xml-platform' mapping='logical' distinct='false'>
                  <entity name='bsd_dailyinterestrate' >
                    <attribute name='bsd_date' />
                    <attribute name='bsd_project' />
                    <attribute name='bsd_interestrate' />
                    <attribute name='createdon' />
                    <filter type='and' >
                      <condition attribute='bsd_project' operator='eq' value='{0}' />
                        <condition attribute='statuscode' operator='eq' value='1' />
                    </filter>
                    <order attribute='createdon' descending='true' />
                  </entity>
                </fetch>";

            fetchXml = string.Format(fetchXml, projID);
            EntityCollection entc = crmservices.RetrieveMultiple(new FetchExpression(fetchXml));
            return entc;
        }
        // fees function -----------
        private Entity getInstallment(IOrganizationService crmservices, string installmentid)
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
                <attribute name='bsd_paymentschemedetailid' />
                <filter type='and' >
                  <condition attribute='bsd_paymentschemedetailid' operator='eq' value='{0}' />
                  
                </filter>
              </entity>
            </fetch>";
            //<condition attribute='bsd_duedatecalculatingmethod' operator='eq' value='100000002' />
            fetchXml = string.Format(fetchXml, installmentid);
            EntityCollection entc = crmservices.RetrieveMultiple(new FetchExpression(fetchXml));
            return entc.Entities[0];
        }
        // ------- create apply detail ---------------

        // app doc dtl status
        //100.000.000 - confirm
        //1: active
        // 100.000.002: cancel

        // pm type: deposit : 100000000
        // installment :100.000.001
        // fee:100.000.002
        // interest charge: 100.000.003
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

            //throw new InvalidPluginExecutionException("bsd_advancepaymentremaining: " + bsd_advancepaymentremaining.ToString());

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

        public string GetCurrentMethod()
        {
            var st = new StackTrace();
            var sf = st.GetFrame(1);
            return sf.GetMethod().Name;
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
