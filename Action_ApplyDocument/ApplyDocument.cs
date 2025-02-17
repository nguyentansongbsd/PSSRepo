using Microsoft.Crm.Sdk.Messages;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Query;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Text;
using System.Web.Script.Serialization;

namespace Action_ApplyDocument
{
    internal class ApplyDocument
    {
        private IOrganizationService service = (IOrganizationService)null;
        private IOrganizationServiceFactory factory = (IOrganizationServiceFactory)null;
        private IPluginExecutionContext context = (IPluginExecutionContext)null;
        private StringBuilder strMess = new StringBuilder();
        private StringBuilder strMess1 = new StringBuilder();
        private IServiceProvider serviceProvider;
        private Decimal d_oe_bsd_totalamountpaid = 0M;

        public ApplyDocument(IServiceProvider serviceProvider)
        {
            this.serviceProvider = serviceProvider;
            this.context = (IPluginExecutionContext)serviceProvider.GetService(typeof(IPluginExecutionContext));
            this.factory = (IOrganizationServiceFactory)serviceProvider.GetService(typeof(IOrganizationServiceFactory));
            this.service = this.factory.CreateOrganizationService(new Guid?(this.context.UserId));
        }

        public void checkInput(Entity en_app)
        {
            if ((en_app.Contains("bsd_differenceamount") ? ((Money)en_app["bsd_differenceamount"]).Value : 0M) < 0M)
                throw new InvalidPluginExecutionException("The amount you pay exceeds the amount paid. Please choose another Advance Payment or other Payment Phase!");
            if (!en_app.Contains("bsd_transactiontype"))
                throw new InvalidPluginExecutionException("Please choose 'Type of payment'!");
            int num1 = en_app.Contains("bsd_receiptdate") ? ((OptionSetValue)en_app["bsd_transactiontype"]).Value : throw new InvalidPluginExecutionException("Please select payment actual time");
            this.strMess.AppendLine("111111111111111");
            Decimal num2 = en_app.Contains("bsd_amountadvancepayment") ? ((Money)en_app["bsd_amountadvancepayment"]).Value : 0M;
            if (num2 == 0M)
                throw new InvalidPluginExecutionException("Please choose at least one row of Advance Payment List!");
            if (!en_app.Contains("bsd_arrayadvancepayment"))
                throw new InvalidPluginExecutionException("Please choose at least one row of Advance Payment List!");
            if (!en_app.Contains("bsd_arrayamountadvance"))
                throw new InvalidPluginExecutionException("Cannot find any amount of advance payment list!");
            this.strMess.AppendLine("2222222222222");
            string str1 = en_app.Contains("bsd_arrayadvancepayment") ? (string)en_app["bsd_arrayadvancepayment"] : "";
            string str2 = en_app.Contains("bsd_arrayamountadvance") ? (string)en_app["bsd_arrayamountadvance"] : "";
            this.strMess.AppendLine("s_bsd_arrayadvancepayment: " + str1);
            this.strMess.AppendLine("s_bsd_arrayamountadvance: " + str2);
            string[] strArray1 = str1.Split(',');
            string[] strArray2 = str2.Split(',');
            int length = strArray1.Length;
            this.strMess.AppendLine("33333333333");
            Decimal num3 = en_app.Contains("bsd_totalapplyamount") ? ((Money)en_app["bsd_totalapplyamount"]).Value : 0M;
            if (num3 == 0M)
                throw new InvalidPluginExecutionException("Please choose at least one row of " + (num1 == 1 ? " Deposit list!" : " Installment or Interest List!"));
            if (!en_app.Contains("bsd_arraypsdid") && !en_app.Contains("bsd_arrayinstallmentinterest") && !en_app.Contains("bsd_arrayfeesid") && !en_app.Contains("bsd_arraymicellaneousid"))
                throw new InvalidPluginExecutionException("Please choose at least one row of " + (num1 == 1 ? " Deposit list!" : " Installment or Interest List or Fees list or Miscellaneous list!"));
            if (!en_app.Contains("bsd_arrayamountpay") && !en_app.Contains("bsd_arrayinterestamount") && !en_app.Contains("bsd_arrayfeesamount") && !en_app.Contains("bsd_arraymicellaneousamount"))
                throw new InvalidPluginExecutionException("Cannot find any amount of " + (num1 == 1 ? " Deposit list!" : " Installment or Interest List or Fees list or Miscellaneous list!"));
            this.strMess.AppendLine("4444444444444");
            if (num3 > num2)
                throw new InvalidPluginExecutionException("The amout you pay exceeds the amount paid. Please choose another Advance Payment or other Payment Phase!");
            this.strMess.AppendLine("55555555555");
            Decimal num4 = 0M;
            for (int index = 0; index < strArray1.Length; ++index)
            {
                this.strMess.AppendLine("Adv ID: " + strArray1[index].ToString());
                Entity entity = this.service.Retrieve("bsd_advancepayment", Guid.Parse(strArray1[index]), new ColumnSet(true));
                if (!entity.Contains("statuscode") || ((OptionSetValue)entity["statuscode"]).Value == 100000002)
                    throw new InvalidPluginExecutionException("Advance payment " + (string)entity["bsd_name"] + " has been Revert!");
                if (((OptionSetValue)entity["statuscode"]).Value == 100000001)
                    throw new InvalidPluginExecutionException("Advance payment " + (string)entity["bsd_name"] + " has been Pay off!");
                this.strMess.AppendLine("6666666666");
                Decimal num5 = entity.Contains("bsd_amount") ? ((Money)entity["bsd_amount"]).Value : throw new InvalidPluginExecutionException("Advance payment " + (string)entity["bsd_name"] + " does not contain 'Amount'. Please check again!");
                Decimal num6 = entity.Contains("bsd_paidamount") ? ((Money)entity["bsd_paidamount"]).Value : 0M;
                Decimal num7 = entity.Contains("bsd_refundamount") ? ((Money)entity["bsd_refundamount"]).Value : 0M;
                this.strMess.AppendLine("d_bsd_paidamount: " + num6.ToString());
                this.strMess.AppendLine("7777777777");
                Decimal num8 = num5 - num7 - num6;
                this.strMess.AppendLine("8888888888");
                num4 += num8;
                if (Decimal.Parse(strArray2[index]) > num8)
                    throw new InvalidPluginExecutionException("Remaining amount of Advance payment " + (string)entity["bsd_name"] + " is less than require amount. Cannot excute payment!");
            }
            if (num4 < num3)
                throw new InvalidPluginExecutionException("Amount Advance Payment must larger than Total Apply Amount!");
            this.strMess.AppendLine("checkInput done");
        }

        public void paymentDeposit(Guid quoteId, Decimal d_amp, DateTime d_now, Entity en_app)
        {
            Entity entity1 = this.service.Retrieve("quote", quoteId, new ColumnSet(new string[8]
            {
        "bsd_deposittime",
        "statuscode",
        "bsd_depositfee",
        "name",
        "bsd_totalamountpaid",
        "bsd_projectid",
        "customerid",
        "bsd_isdeposited"
            }));
            if (((OptionSetValue)entity1["statuscode"]).Value == 6)
                throw new InvalidPluginExecutionException("Reservation " + (string)entity1["name"] + " had been cancelled, cannot deposit!");
            Entity entity2 = ((OptionSetValue)entity1["statuscode"]).Value != 3 ? new Entity(entity1.LogicalName) : throw new InvalidPluginExecutionException("Quotation Reservation " + (string)entity1["name"] + " had been depositted, cannot payment!");
            entity2.Id = entity1.Id;
            EntityCollection entityCollection = this.Get1st_Resv(entity1.Id.ToString());
            Entity entity3 = new Entity(entityCollection.Entities[0].LogicalName);
            entity3.Id = entityCollection[0].Id;
            Decimal num = entityCollection.Entities[0].Contains("bsd_amountofthisphase") ? ((Money)entityCollection.Entities[0]["bsd_amountofthisphase"]).Value : 0M;
            if (num == 0M)
                throw new InvalidPluginExecutionException("Amount of Installment 1 - Reservation " + (string)entity1["name"] + " is null. Please check again!");
            bool flag = entity3.Contains("bsd_typeofstartdate") && (bool)entity3["bsd_typeofstartdate"];
            if (entityCollection.Entities[0].Contains("bsd_duedatecalculatingmethod") && ((OptionSetValue)entityCollection.Entities[0]["bsd_duedatecalculatingmethod"]).Value == 100000001 && flag)
            {
                EntityCollection ecInsResv = this.Get_ec_Ins_Resv(entity1.Id.ToString());
                this.reGenerate(this.service, entity1, d_now, ecInsResv);
            }
            entity3["bsd_balance"] = (object)new Money(num - d_amp);
            entity3["bsd_depositamount"] = (object)new Money(d_amp);
            this.service.Update(entity3);
            entity2["bsd_totalamountpaid"] = (object)new Money(d_amp);
            entity2["bsd_deposittime"] = en_app["bsd_receiptdate"];
            entity2["bsd_isdeposited"] = (object)true;
            this.service.Update(entity2);
            this.service.Execute((OrganizationRequest)new SetStateRequest()
            {
                EntityMoniker = entity1.ToEntityReference(),
                State = new OptionSetValue(0),
                Status = new OptionSetValue(100000000)
            });
            this.service.Execute((OrganizationRequest)new SetStateRequest()
            {
                EntityMoniker = entity1.ToEntityReference(),
                State = new OptionSetValue(1),
                Status = new OptionSetValue(3)
            });
            this.create_app_detail(this.service, en_app, entityCollection.Entities[0], entity1, 100000000, d_amp, 0, 0M, 0);
        }

        public void paymentInstallment(Entity en_app)
        {
            string s_bsd_arraypsdid = en_app.Contains("bsd_arraypsdid") ? (string)en_app["bsd_arraypsdid"] : "";
            string s_bsd_arrayamountpay = en_app.Contains("bsd_arrayamountpay") ? (string)en_app["bsd_arrayamountpay"] : "";
            this.strMess.AppendLine("10.1");
            Entity en_OE = this.service.Retrieve("salesorder", ((EntityReference)en_app["bsd_optionentry"]).Id, new ColumnSet(true));
            en_OE.Id.ToString();
            if (!en_OE.Contains("bsd_unitnumber"))
                throw new InvalidPluginExecutionException("Cannot find Unit information in Option Entry " + (string)en_OE["name"] + "!");
            this.strMess.AppendLine("10.2");
            this.d_oe_bsd_totalamountpaid = en_OE.Contains("bsd_totalamountpaid") ? ((Money)en_OE["bsd_totalamountpaid"]).Value : 0M;
            if ((en_OE.Contains("bsd_totalamountlessfreight") ? ((Money)en_OE["bsd_totalamountlessfreight"]).Value : 0M) == 0M)
                throw new InvalidPluginExecutionException("'Net Selling Price' must be larger than 0");
            this.strMess.AppendLine("10.3");
            this.strMess.AppendLine("10.4");
            if (s_bsd_arraypsdid != "")
                this.applyIntallment(en_app, en_OE, s_bsd_arraypsdid, s_bsd_arrayamountpay);
            this.strMess.AppendLine("10.5");
            string s_bsd_arrayinstallmentinterest = en_app.Contains("bsd_arrayinstallmentinterest") ? (string)en_app["bsd_arrayinstallmentinterest"] : "";
            string s_bsd_arrayinterestamount = en_app.Contains("bsd_arrayinterestamount") ? (string)en_app["bsd_arrayinterestamount"] : "";
            if (s_bsd_arrayinstallmentinterest != "")
                this.applyInterestcharge(en_app, en_OE, s_bsd_arrayinstallmentinterest, s_bsd_arrayinterestamount);
            this.strMess.AppendLine("10.6");
            string s_fees = en_app.Contains("bsd_arrayfeesid") ? (string)en_app["bsd_arrayfeesid"] : "";
            string s_feesAM = en_app.Contains("bsd_arrayfeesamount") ? (string)en_app["bsd_arrayfeesamount"] : "";
            if (s_fees != "")
                this.applyFees(en_app, en_OE, s_fees, s_feesAM);
            this.strMess.AppendLine("10.7");
            this.strMess.AppendLine("10.8");
            string s_bsd_arraymicellaneousid = en_app.Contains("bsd_arraymicellaneousid") ? (string)en_app["bsd_arraymicellaneousid"] : "";
            string s_bsd_arraymicellaneousamount = en_app.Contains("bsd_arraymicellaneousamount") ? (string)en_app["bsd_arraymicellaneousamount"] : "";
            if (s_bsd_arraymicellaneousid != "")
                this.applyMicellaneous(en_app, en_OE, s_bsd_arraymicellaneousid, s_bsd_arraymicellaneousamount);
            this.strMess.AppendLine("10.9");
        }

        public void createCOA(Entity en_app)
        {
            Decimal num1 = en_app.Contains("bsd_totalapplyamount") ? ((Money)en_app["bsd_totalapplyamount"]).Value : 0M;
            string str1 = en_app.Contains("bsd_arrayadvancepayment") ? (string)en_app["bsd_arrayadvancepayment"] : "";
            string str2 = en_app.Contains("bsd_arrayamountadvance") ? (string)en_app["bsd_arrayamountadvance"] : "";
            string[] strArray1 = str1.Split(',');
            string[] strArray2 = str2.Split(',');
            Decimal[] numArray1 = new Decimal[strArray1.Length];
            Decimal[] numArray2 = new Decimal[strArray1.Length];
            this.strMess.AppendLine("s_eachAdv: " + strArray1.Length.ToString());
            for (int index = 0; index < strArray1.Length; ++index)
            {
                Entity enAdvancePayment = this.service.Retrieve("bsd_advancepayment", Guid.Parse(strArray1[index]), new ColumnSet(true));
                this.strMess.AppendLine("ID: " + strArray1[index].ToString());
                Decimal bsd_advancepaymentamount = enAdvancePayment.Contains("bsd_amount") ? ((Money)enAdvancePayment["bsd_amount"]).Value : 0M;
                Decimal num2 = enAdvancePayment.Contains("bsd_paidamount") ? ((Money)enAdvancePayment["bsd_paidamount"]).Value : 0M;
                this.strMess.AppendLine("bsd_paidamount: " + num2.ToString());
                Decimal num3 = enAdvancePayment.Contains("bsd_refundamount") ? ((Money)enAdvancePayment["bsd_refundamount"]).Value : 0M;
                Decimal bsd_avPMPaid = num2 + num3;
                Decimal num4 = Decimal.Parse(strArray2[index]);
                if (num1 == 0M)
                {
                    numArray1[index] = 0M;
                    numArray2[index] = bsd_advancepaymentamount - num2;
                }
                else if (num1 > 0M)
                {
                    Decimal num5 = num1 - num4;
                    this.strMess.AppendLine("delta: " + num5.ToString());
                    if (num5 >= 0M)
                    {
                        numArray1[index] = num4;
                        numArray2[index] = bsd_advancepaymentamount - num3 - num2 - numArray1[index];
                    }
                    else if (num5 < 0M)
                    {
                        numArray1[index] = num1;
                        numArray2[index] = bsd_advancepaymentamount - num3 - num2 - numArray1[index];
                    }
                    num1 -= numArray1[index];
                }
                this.strMess.AppendLine("totalavancedamount: " + bsd_advancepaymentamount.ToString());
                this.strMess.AppendLine("apply: " + numArray1[index].ToString());
                this.strMess.AppendLine("remain: " + numArray2[index].ToString());
                this.strMess.AppendLine("bsd_avPMPaid: " + bsd_avPMPaid.ToString());
                this.create_Applydocument_RemainingCOA(this.service, en_app, enAdvancePayment, bsd_advancepaymentamount, numArray1[index], numArray2[index], bsd_avPMPaid);
            }
        }

        public void updateApplyDocument(Entity en_app)
        {
            Decimal trans_am = en_app.Contains("bsd_totalapplyamount") ? ((Money)en_app["bsd_totalapplyamount"]).Value : 0M;
            string str1 = en_app.Contains("bsd_arrayadvancepayment") ? (string)en_app["bsd_arrayadvancepayment"] : "";
            string str2 = en_app.Contains("bsd_arrayamountadvance") ? (string)en_app["bsd_arrayamountadvance"] : "";
            string[] strArray1 = str1.Split(',');
            string[] strArray2 = str2.Split(',');
            int length = strArray1.Length;
            for (int index = 0; index < length && !(trans_am == 0M); ++index)
            {
                Decimal num;
                if (trans_am == Decimal.Parse(strArray2[index]))
                {
                    this.strMess.AppendLine("Up_adv 1");
                    this.Up_adv(Guid.Parse(strArray1[index]), Decimal.Parse(strArray2[index]));
                    num = 0M;
                    break;
                }
                if (trans_am < Decimal.Parse(strArray2[index]))
                {
                    this.strMess.AppendLine("Up_adv 2");
                    this.Up_adv(Guid.Parse(strArray1[index]), trans_am);
                    num = 0M;
                    break;
                }
                if (trans_am > Decimal.Parse(strArray2[index]))
                {
                    this.strMess.AppendLine("Up_adv 3");
                    this.Up_adv(Guid.Parse(strArray1[index]), Decimal.Parse(strArray2[index]));
                    trans_am -= Decimal.Parse(strArray2[index]);
                }
                this.strMess.AppendLine("1111111111111111777777777777");
            }
            this.strMess.AppendLine("1111111111");
            DateTime dateTime = this.RetrieveLocalTimeFromUTCTime(DateTime.Now);
            this.service.Update(new Entity(en_app.LogicalName)
            {
                Id = en_app.Id,
                ["statuscode"] = (object)new OptionSetValue(100000002),
                ["bsd_approvaldate"] = (object)dateTime,
                ["bsd_approver"] = (object)new EntityReference("systemuser", this.context.UserId)
            });
            this.strMess.AppendLine("aaaaa");
        }

        private void applyIntallment(
          Entity en_app,
          Entity en_OE,
          string s_bsd_arraypsdid,
          string s_bsd_arrayamountpay)
        {
            this.strMess.AppendLine("s_bsd_arraypsdid: " + s_bsd_arraypsdid);
            this.strMess.AppendLine("s_bsd_arrayamountpay: " + s_bsd_arrayamountpay);
            string[] s_id = s_bsd_arraypsdid.Split(',');
            string[] strArray = s_bsd_arrayamountpay.Split(',');
            int length = s_id.Length;
            EntityCollection ecIns = this.get_ecIns(this.service, s_id);
            for (int index1 = 0; index1 < ecIns.Entities.Count; ++index1)
            {
                Entity entity = ecIns.Entities[index1];
                entity.Id = ecIns.Entities[index1].Id;
                this.strMess.AppendLine("bsd_name: " + entity["bsd_name"].ToString());
                if (!ecIns.Entities[index1].Contains("statuscode"))
                    throw new InvalidPluginExecutionException("Please check status code of '" + (string)ecIns.Entities[index1]["bsd_name"] + "!");
                if (((OptionSetValue)ecIns.Entities[index1]["statuscode"]).Value == 100000001)
                    throw new InvalidPluginExecutionException((string)ecIns.Entities[index1]["bsd_name"] + " has been Paid!");
                int num1 = ecIns.Entities[index1].Contains("bsd_duedatecalculatingmethod") ? ((OptionSetValue)ecIns.Entities[index1]["bsd_duedatecalculatingmethod"]).Value : 0;
                if (!ecIns.Entities[index1].Contains("bsd_amountofthisphase"))
                    throw new InvalidPluginExecutionException("Installment " + (string)ecIns.Entities[index1]["bsd_name"] + " did not contain 'Amount of this phase'!");
                Decimal num2 = ((Money)ecIns.Entities[index1]["bsd_amountofthisphase"]).Value;
                Decimal psd_amountPaid1 = ecIns.Entities[index1].Contains("bsd_amountwaspaid") ? ((Money)ecIns.Entities[index1]["bsd_amountwaspaid"]).Value : 0M;
                Decimal num3 = ecIns.Entities[index1].Contains("bsd_depositamount") ? ((Money)ecIns.Entities[index1]["bsd_depositamount"]).Value : 0M;
                Decimal num4 = ecIns.Entities[index1].Contains("bsd_waiveramount") ? ((Money)ecIns.Entities[index1]["bsd_waiveramount"]).Value : 0M;
                Decimal num5 = ecIns.Entities[index1].Contains("bsd_waiverinstallment") ? ((Money)ecIns.Entities[index1]["bsd_waiverinstallment"]).Value : 0M;
                Decimal num6 = ecIns.Entities[index1].Contains("bsd_balance") ? ((Money)ecIns.Entities[index1]["bsd_balance"]).Value : 0M;
                bool flag1 = ecIns.Entities[index1].Contains("bsd_maintenancefeesstatus") && (bool)ecIns.Entities[index1]["bsd_maintenancefeesstatus"];
                bool flag2 = ecIns.Entities[index1].Contains("bsd_managementfeesstatus") && (bool)ecIns.Entities[index1]["bsd_managementfeesstatus"];
                int num7 = (int)ecIns.Entities[index1]["bsd_ordernumber"];
                Decimal num8 = 0M;
                bool flag3 = false;
                for (int index2 = 0; index2 < s_id.Length; ++index2)
                {
                    if (entity.Id.ToString() == s_id[index2])
                    {
                        num8 = Decimal.Parse(strArray[index2]);
                        flag3 = true;
                        break;
                    }
                }
                if (!flag3)
                    throw new InvalidPluginExecutionException("Cannot find ID of '" + (string)ecIns.Entities[index1]["bsd_name"] + "' in Installment array!");
                bool f_1st = this.check_Ins_Paid(this.service, en_OE.Id, 1);
                this.strMess.AppendLine("so sanh ampay voi baslane");
                int i_late = 0;
                Decimal d_outstandingAM = 0M;
                Decimal num9 = num7 != 1 ? num2 - psd_amountPaid1 - num5 : num2 - psd_amountPaid1 - num3 - num5;
                this.strMess.AppendLine("psd_bsd_balance: " + num9.ToString());
                this.strMess.AppendLine("d_ampay: " + num8.ToString());
                DateTime dateTime = new DateTime();
                if (entity.Contains("bsd_duedate"))
                    dateTime = (DateTime)en_app["bsd_receiptdate"];
                if (num8 >= num9)
                {
                    psd_amountPaid1 += num9;
                    int psd_statuscode = 100000001;
                    if (ecIns.Entities[index1].Contains("bsd_duedate"))
                    {
                        DateTime d_receipt = (DateTime)en_app["bsd_receiptdate"];
                        this.checkInterest(ecIns.Entities[index1], d_receipt, num9, ref i_late, ref d_outstandingAM);
                    }
                    this.strMess.AppendLine("i_late: " + i_late.ToString());
                    this.strMess.AppendLine("d_outstandingAM: " + d_outstandingAM.ToString());
                    this.d_oe_bsd_totalamountpaid += num9;
                    DateTime d_now = this.RetrieveLocalTimeFromUTCTime(DateTime.Now);
                    this.Up_Ins_OE(entity, en_OE, psd_amountPaid1, 0M, psd_statuscode, num9, f_1st, d_now, i_late, d_outstandingAM);
                    this.create_app_detail(this.service, en_app, entity, en_OE, 100000001, num9, i_late, d_outstandingAM, 0);
                }
                if (num8 < num9)
                {
                    int psd_statuscode = 100000000;
                    Decimal psd_amountPaid2 = psd_amountPaid1 + num8;
                    Decimal psd_balance = num9 - num8;
                    if (entity.Contains("bsd_duedate"))
                    {
                        DateTime d_receipt = (DateTime)en_app["bsd_receiptdate"];
                        this.checkInterest(entity, d_receipt, num8, ref i_late, ref d_outstandingAM);
                    }
                    this.strMess.AppendLine("i_late: " + i_late.ToString());
                    this.strMess.AppendLine("d_outstandingAM: " + d_outstandingAM.ToString());
                    this.d_oe_bsd_totalamountpaid += num8;
                    DateTime d_now = this.RetrieveLocalTimeFromUTCTime(DateTime.Now);
                    this.Up_Ins_OE(entity, en_OE, psd_amountPaid2, psd_balance, psd_statuscode, num8, f_1st, d_now, i_late, d_outstandingAM);
                    this.create_app_detail(this.service, en_app, entity, en_OE, 100000001, num8, i_late, d_outstandingAM, 0);
                }
            }
        }

        private void checkInterest(
          Entity enInstallment,
          DateTime d_receipt,
          Decimal amountPay,
          ref int i_late,
          ref Decimal d_outstandingAM)
        {
            this.RetrieveLocalTimeFromUTCTime((DateTime)enInstallment["bsd_duedate"]);
            d_receipt = this.RetrieveLocalTimeFromUTCTime(d_receipt);
            foreach (KeyValuePair<string, object> result in (DataCollection<string, object>)this.service.Execute(new OrganizationRequest("bsd_Action_Calculate_Interest")
            {
                ["installmentid"] = (object)enInstallment.Id.ToString(),
                ["amountpay"] = (object)amountPay.ToString(),
                ["receiptdate"] = (object)d_receipt.ToString("MM/dd/yyyy")
            }).Results)
            {
                Installment installment = new JavaScriptSerializer().Deserialize<Installment>(result.Value.ToString());
                i_late = installment.LateDays;
                d_outstandingAM = installment.InterestCharge;
            }
        }

        private void applyInterestcharge(
          Entity en_app,
          Entity en_OE,
          string s_bsd_arrayinstallmentinterest,
          string s_bsd_arrayinterestamount)
        {
            if (s_bsd_arrayinterestamount == "")
                throw new InvalidPluginExecutionException("Cannot find any Intest Amount Array. Please check again!");
            string[] s_id = s_bsd_arrayinstallmentinterest.Split(',');
            string[] strArray = s_bsd_arrayinterestamount.Split(',');
            Decimal appAM1 = 0M;
            EntityCollection ecIns = this.get_ecIns(this.service, s_id);
            if (ecIns.Entities.Count < 0)
                throw new InvalidPluginExecutionException("Cannot find any Installment ID with Interest charge array!");
            for (int index1 = 0; index1 < ecIns.Entities.Count; ++index1)
            {
                Entity entity = ecIns.Entities[index1];
                entity.Id = ecIns.Entities[index1].Id;
                Decimal num1 = entity.Contains("bsd_interestchargeamount") ? ((Money)entity["bsd_interestchargeamount"]).Value : 0M;
                Decimal psd_bsd_interestwaspaid1 = entity.Contains("bsd_interestwaspaid") ? ((Money)entity["bsd_interestwaspaid"]).Value : 0M;
                Decimal num2 = entity.Contains("bsd_waiverinterest") ? ((Money)entity["bsd_waiverinterest"]).Value : 0M;
                int num3 = entity.Contains("bsd_interestchargestatus") ? ((OptionSetValue)entity["bsd_interestchargestatus"]).Value : 0;
                Decimal num4 = (Decimal)(entity.Contains("bsd_actualgracedays") ? (int)entity["bsd_actualgracedays"] : 0);
                int num5 = entity.Contains("statuscode") ? ((OptionSetValue)entity["statuscode"]).Value : 0;
                if (num3 == 100000001)
                    throw new InvalidPluginExecutionException("The Interest charge of " + (string)entity["bsd_name"] + " has been paid!");
                bool flag = false;
                Decimal appAM2 = num1 - psd_bsd_interestwaspaid1 - num2;
                for (int index2 = 0; index2 < s_id.Length; ++index2)
                {
                    if (entity.Id.ToString() == s_id[index2])
                    {
                        appAM1 = Decimal.Parse(strArray[index2]);
                        flag = true;
                        break;
                    }
                }
                if (!flag)
                    throw new InvalidPluginExecutionException("Cannot find '" + (string)entity["bsd_name"] + "' in Interest charge array!");
                if (appAM1 > appAM2)
                    throw new InvalidPluginExecutionException("Input amount was larger than balance Interest charge amount of " + (string)entity["bsd_name"] + ". Please check again!");
                if (appAM1 == appAM2)
                {
                    psd_bsd_interestwaspaid1 += appAM2;
                    int psd_bsd_interestchargestatus = num5 != 100000001 ? 100000000 : 100000001;
                    this.Update_Interest(this.service, entity, psd_bsd_interestwaspaid1, psd_bsd_interestchargestatus);
                    this.create_app_detail(this.service, en_app, entity, en_OE, 100000003, appAM2, 0, 0M, 0);
                }
                if (appAM1 < appAM2)
                {
                    Decimal psd_bsd_interestwaspaid2 = psd_bsd_interestwaspaid1 + appAM1;
                    int psd_bsd_interestchargestatus = 100000000;
                    this.Update_Interest(this.service, entity, psd_bsd_interestwaspaid2, psd_bsd_interestchargestatus);
                    this.create_app_detail(this.service, en_app, entity, en_OE, 100000003, appAM1, 0, 0M, 0);
                }
            }
        }

        private void applyFees(Entity en_app, Entity en_OE, string s_fees, string s_feesAM)
        {
            this.strMess.AppendLine("0000000000000");
            if (!en_app.Contains("bsd_units"))
                throw new InvalidPluginExecutionException("Please check Units of Apply Document!");
            Entity entity1 = this.service.Retrieve(((EntityReference)en_app["bsd_units"]).LogicalName, ((EntityReference)en_app["bsd_units"]).Id, new ColumnSet(new string[3]
            {
        "bsd_handovercondition",
        "name",
        "bsd_opdate"
            }));
            this.strMess.AppendLine("111111111111");
            int num1;
            if (entity1.Contains("bsd_opdate"))
            {
                DateTime dateTime = (DateTime)entity1["bsd_opdate"];
                num1 = 0;
            }
            else
                num1 = 1;
            if (num1 != 0)
                throw new InvalidPluginExecutionException("Product " + (string)entity1["name"] + " have not contain OP Date. Cannot Payment fees. Please check again!");
            string[] strArray1 = s_fees.Split(',');
            string[] strArray2 = s_feesAM.Split(',');
            for (int index = 0; index < strArray1.Length; ++index)
            {
                this.strMess.AppendLine("22222222222");
                string[] strArray3 = strArray1[index].Split('_');
                string installmentid = strArray3[0];
                string str = strArray3[1];
                Decimal appAM = Decimal.Parse(strArray2[index].ToString());
                Entity installment = this.getInstallment(this.service, installmentid);
                bool flag1 = installment.Contains("bsd_maintenancefeesstatus") && (bool)installment["bsd_maintenancefeesstatus"];
                bool flag2 = installment.Contains("bsd_managementfeesstatus") && (bool)installment["bsd_managementfeesstatus"];
                Decimal num2 = installment.Contains("bsd_maintenanceamount") ? ((Money)installment["bsd_maintenanceamount"]).Value : 0M;
                Decimal num3 = installment.Contains("bsd_managementamount") ? ((Money)installment["bsd_managementamount"]).Value : 0M;
                Decimal num4 = installment.Contains("bsd_maintenancefeepaid") ? ((Money)installment["bsd_maintenancefeepaid"]).Value : 0M;
                Decimal num5 = installment.Contains("bsd_managementfeepaid") ? ((Money)installment["bsd_managementfeepaid"]).Value : 0M;
                Decimal num6 = installment.Contains("bsd_maintenancefeewaiver") ? ((Money)installment["bsd_maintenancefeewaiver"]).Value : 0M;
                Decimal num7 = installment.Contains("bsd_managementfeewaiver") ? ((Money)installment["bsd_managementfeewaiver"]).Value : 0M;
                Decimal num8 = num2 - num4 - num6;
                Decimal num9 = num3 - num5 - num7;
                Entity entity2 = new Entity(installment.LogicalName);
                entity2.Id = installment.Id;
                switch (str)
                {
                    case "main":
                        if (flag1)
                            throw new InvalidPluginExecutionException("Maintenance fees had been paid. Please check again!");
                        if (appAM < num8)
                        {
                            num4 += appAM;
                            flag1 = false;
                            this.d_oe_bsd_totalamountpaid += appAM;
                        }
                        else if (appAM == num8)
                        {
                            num4 += appAM;
                            flag1 = true;
                            this.d_oe_bsd_totalamountpaid += appAM;
                        }
                        else if (appAM > num8)
                            throw new InvalidPluginExecutionException("Amount pay larger than balance of Maintenance fees amount. Please check again!");
                        this.strMess.AppendLine("333333333333");
                        this.create_app_detail(this.service, en_app, installment, en_OE, 100000002, appAM, 0, 0M, 1);
                        entity2["bsd_maintenancefeesstatus"] = (object)flag1;
                        entity2["bsd_maintenancefeepaid"] = (object)new Money(num4);
                        this.strMess.AppendLine("444444444444");
                        break;
                    case "mana":
                        this.strMess.AppendLine("555555555555");
                        if (flag2)
                            throw new InvalidPluginExecutionException("Management fees had been paid. Please check again!");
                        this.strMess.AppendLine("66666666666666");
                        if (appAM < num9)
                        {
                            num5 += appAM;
                            flag2 = false;
                        }
                        else if (appAM == num9)
                        {
                            num5 += appAM;
                            flag2 = true;
                        }
                        else if (appAM > num9)
                            throw new InvalidPluginExecutionException("Amount pay larger than balance of Management fees amount. Please check again!");
                        this.create_app_detail(this.service, en_app, installment, en_OE, 100000002, appAM, 0, 0M, 2);
                        this.strMess.AppendLine("7777777");
                        entity2["bsd_managementfeesstatus"] = (object)flag2;
                        entity2["bsd_managementfeepaid"] = (object)new Money(num5);
                        break;
                }
                this.strMess.AppendLine("88888888888888");
                this.service.Update(entity2);
                this.strMess.AppendLine("999999999999");
            }
        }

        private void applyMicellaneous(
          Entity en_app,
          Entity en_OE,
          string s_bsd_arraymicellaneousid,
          string s_bsd_arraymicellaneousamount)
        {
            this.strMess.AppendLine("10.8");
            if (s_bsd_arraymicellaneousamount == "")
                throw new InvalidPluginExecutionException("Cannot find any Miscellaneous amount array!");
            string[] strArray1 = new string[0];
            string[] strArray2 = new string[0];
            string[] s_id = s_bsd_arraymicellaneousid.Split(',');
            string[] strArray3 = s_bsd_arraymicellaneousamount.Split(',');
            EntityCollection ecMis = this.get_ecMIS(this.service, s_id, en_OE.Id.ToString());
            if (ecMis.Entities.Count <= 0)
                throw new InvalidPluginExecutionException("There's not any Miscellaneous found!");
            for (int index1 = 0; index1 < ecMis.Entities.Count; ++index1)
            {
                if (!ecMis.Entities[index1].Contains("bsd_installment"))
                    throw new InvalidPluginExecutionException("Miscellaneous has Installment is null. Please check Miscellaneous - " + (string)ecMis.Entities[index1]["bsd_name"] + "!");
                Entity en_Ins = this.service.Retrieve(((EntityReference)ecMis.Entities[index1]["bsd_installment"]).LogicalName, ((EntityReference)ecMis.Entities[index1]["bsd_installment"]).Id, new ColumnSet(new string[3]
                {
          "bsd_paymentschemedetailid",
          "bsd_name",
          "statuscode"
                }));
                int num1 = en_Ins.Contains("statuscode") ? ((OptionSetValue)en_Ins["statuscode"]).Value : 100000000;
                if ((ecMis.Entities[index1].Contains("statuscode") ? ((OptionSetValue)ecMis.Entities[index1]["statuscode"]).Value : 1) == 100000000)
                    throw new InvalidPluginExecutionException("Miscellaneous " + (string)ecMis.Entities[index1]["bsd_name"] + " has been paid");
                Decimal appAM = 0M;
                bool flag = false;
                for (int index2 = 0; index2 < s_id.Length; ++index2)
                {
                    if (ecMis.Entities[index1].Id.ToString() == s_id[index2])
                    {
                        appAM = Decimal.Parse(strArray3[index2]);
                        flag = true;
                        break;
                    }
                }
                if (!flag)
                    throw new InvalidPluginExecutionException("Cannot find ID of Miscellaneous " + (string)ecMis.Entities[index1]["bsd_name"] + "' in Miscellaneous array!");
                Decimal num2 = ecMis.Entities[index1].Contains("bsd_paidamount") ? ((Money)ecMis.Entities[index1]["bsd_paidamount"]).Value : 0M;
                Decimal num3 = ecMis.Entities[index1].Contains("bsd_balance") ? ((Money)ecMis.Entities[index1]["bsd_balance"]).Value : 0M;
                Decimal num4 = ecMis.Entities[index1].Contains("bsd_totalamount") ? ((Money)ecMis.Entities[index1]["bsd_totalamount"]).Value : 0M;
                Decimal num5;
                Decimal num6;
                int num7;
                if (appAM == num3)
                {
                    num5 = 0M;
                    num6 = num4;
                    num7 = 100000000;
                }
                else
                {
                    if (!(appAM < num3))
                        throw new InvalidPluginExecutionException("Input amount is larger than balance amount of Miscellaneous " + (string)ecMis.Entities[index1]["bsd_name"]);
                    num7 = 1;
                    num5 = num3 - appAM;
                    num6 = num2 + appAM;
                }
                this.service.Update(new Entity(ecMis.Entities[index1].LogicalName)
                {
                    Id = ecMis.Entities[index1].Id,
                    ["bsd_paidamount"] = (object)new Money(num6),
                    ["bsd_balance"] = (object)new Money(num5),
                    ["statuscode"] = (object)new OptionSetValue(num7)
                });
                this.create_app_detail_MISS(this.service, en_app, en_Ins, en_OE, 100000004, appAM, 0, 0M, 0, ecMis.Entities[index1]);
            }
        }

        private EntityCollection Get1st_Resv(string resvID)
        {
            QueryExpression query = new QueryExpression("bsd_paymentschemedetail");
            query.ColumnSet = new ColumnSet(new string[8]
            {
        "bsd_name",
        "bsd_depositamount",
        "bsd_ordernumber",
        "bsd_paymentschemedetailid",
        "statuscode",
        "bsd_amountwaspaid",
        "bsd_balance",
        "bsd_amountofthisphase"
            });
            query.Distinct = true;
            query.Criteria = new FilterExpression();
            query.Criteria.AddCondition("bsd_reservation", ConditionOperator.Equal, (object)resvID);
            query.AddOrder("bsd_ordernumber", OrderType.Ascending);
            query.TopCount = new int?(1);
            return this.service.RetrieveMultiple((QueryBase)query);
        }

        private EntityCollection get_ecIns(IOrganizationService crmservices, string[] s_id)
        {
            string str = "<fetch version='1.0' output-format='xml-platform' mapping='logical' distinct='true' >\n                <entity name='bsd_paymentschemedetail' >\n                <all-attributes />\n                <filter type='and' >\n                  <condition attribute='bsd_paymentschemedetailid' operator='in' >";
            for (int index = 0; index < s_id.Length; ++index)
                str = str + "<value>" + Guid.Parse(s_id[index]).ToString() + "</value>";
            string query = str + "</condition>\n                            </filter>\n                            <order attribute='bsd_ordernumber' />\n                          </entity>\n                        </fetch>";
            return crmservices.RetrieveMultiple((QueryBase)new FetchExpression(query));
        }

        private void Up_adv(Guid advID, Decimal trans_am)
        {
            Entity entity = this.service.Retrieve("bsd_advancepayment", advID, new ColumnSet(new string[9]
            {
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
            Decimal num1 = entity.Contains("bsd_paidamount") ? ((Money)entity["bsd_paidamount"]).Value : 0M;
            Decimal num2 = entity.Contains("bsd_remainingamount") ? ((Money)entity["bsd_remainingamount"]).Value : 0M;
            Decimal num3 = entity.Contains("bsd_amount") ? ((Money)entity["bsd_amount"]).Value : 0M;
            if (num3 == 0M)
                throw new InvalidPluginExecutionException("Cannot find Advance payment amount of " + (string)entity["bsd_name"] + "!");
            if (num3 < trans_am)
                throw new InvalidPluginExecutionException("Transaction amount is larger than Advance payment amount!");
            Decimal num4 = num1 + trans_am;
            Decimal num5 = num2 - trans_am;
            int num6 = 100000000;
            if (num5 == 0M)
                num6 = 100000001;
            this.service.Update(new Entity("bsd_advancepayment")
            {
                Id = advID,
                ["bsd_remainingamount"] = (object)new Money(num5),
                ["bsd_paidamount"] = (object)new Money(num4),
                ["statuscode"] = (object)new OptionSetValue(num6)
            });
        }

        private EntityCollection Get_ec_Ins_Resv(string resvID)
        {
            return this.service.RetrieveMultiple((QueryBase)new FetchExpression(string.Format("<fetch version='1.0' output-format='xml-platform' mapping='logical' distinct='true' >\n                  <entity name='bsd_paymentschemedetail' >\n                    <attribute name='bsd_typeofstartdate' />\n                    <attribute name='bsd_duedate' />\n                    <attribute name='bsd_nextdaysofendphase' />\n                    <attribute name='bsd_duedatecalculatingmethod' />\n                    <attribute name='bsd_estimateamount' />\n                    <attribute name='bsd_depositamount' />\n                    <attribute name='bsd_nextperiodtype' />\n                    <attribute name='statuscode' />\n                    <attribute name='bsd_ordernumber' />\n                    <attribute name='bsd_fixeddate' />\n                    <attribute name='bsd_datepaymentofmonthly' />\n                    <attribute name='bsd_paymentschemedetailid' />\n                    <attribute name='bsd_amountpay' />\n                    <attribute name='bsd_withindate' />\n                    <attribute name='bsd_amountofthisphase' />\n                    <attribute name='bsd_numberofnextmonth' />\n                    <attribute name='bsd_emailreminderforeigner' />\n                    <attribute name='bsd_numberofnextdays' />\n                    <attribute name='bsd_balance' />\n                    <attribute name='bsd_amountpercent' />\n                    <attribute name='bsd_amountwaspaid' />\n                    <attribute name='bsd_reservation' />\n                    <attribute name='bsd_typepayment' />\n                    <filter type='and' >\n                      <condition attribute='bsd_reservation' operator='eq' value='{0}' />\n                      <condition attribute='bsd_optionentry' operator='null' />\n                    </filter>\n                    <order attribute='bsd_ordernumber' />\n                  </entity>\n            </fetch>", (object)resvID)));
        }

        private EntityCollection get_ecMIS(
          IOrganizationService crmservices,
          string[] s_id,
          string oeID)
        {
            string str = "<fetch version='1.0' output-format='xml-platform' mapping='logical' distinct='false'>\n                                  <entity name='bsd_miscellaneous'>\n                                    <attribute name='bsd_miscellaneousid' />\n                                    <attribute name='bsd_name' />\n                                    <attribute name='createdon' />\n                                    <attribute name='bsd_waiveramount' />\n                                    <attribute name='bsd_vatamount' />\n                                    <attribute name='bsd_units' />\n                                    <attribute name='bsd_type' />\n                                    <attribute name='bsd_totalamount' />\n                                    <attribute name='statuscode' />\n                                    <attribute name='bsd_project' />\n                                    <attribute name='bsd_paidamount' />\n                                    <attribute name='bsd_optionentry' />\n                                    <attribute name='bsd_miscellaneousnumber' />\n                                    <attribute name='bsd_miscellaneouscodesams' />\n                                    <attribute name='bsd_installmentnumber' />\n                                    <attribute name='bsd_installment' />\n                                    <attribute name='bsd_duedate' />\n                                    <attribute name='bsd_balance' />\n                                    <attribute name='bsd_amount' />\n                                    <order attribute='bsd_name' descending='false' />\n                                    <filter type='and'>\n                                      <condition attribute='bsd_optionentry' operator='eq' uitype='salesorder' value='{0}' />" + "<condition attribute='bsd_miscellaneousid' operator='in' >";
            for (int index = 0; index < s_id.Length; ++index)
                str = str + "<value>" + Guid.Parse(s_id[index]).ToString() + "</value>";
            string query = string.Format(str + "</condition>" + "</filter>                       \n                          </entity>\n                        </fetch>", (object)oeID, (object)s_id);
            return crmservices.RetrieveMultiple((QueryBase)new FetchExpression(query));
        }

        private void reGenerate(
          IOrganizationService serv,
          Entity enQuote,
          DateTime dt_date,
          EntityCollection ec_pms)
        {
            EntityReference entityReference = (EntityReference)enQuote["bsd_paymentscheme"];
            QueryExpression query = new QueryExpression("bsd_paymentschemedetail");
            query.ColumnSet = new ColumnSet(new string[16]
            {
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
            query.Criteria = new FilterExpression(LogicalOperator.And);
            query.Criteria.AddCondition(new ConditionExpression("bsd_paymentscheme", ConditionOperator.Equal, (object)entityReference.Id));
            query.Criteria.AddCondition(new ConditionExpression("bsd_optionentry", ConditionOperator.Null));
            query.Criteria.AddCondition(new ConditionExpression("bsd_reservation", ConditionOperator.Null));
            query.Criteria.AddCondition(new ConditionExpression("bsd_quotation", ConditionOperator.Null));
            query.AddOrder("bsd_ordernumber", OrderType.Ascending);
            EntityCollection entityCollection = this.service.RetrieveMultiple((QueryBase)query);
            this.service.Retrieve(entityReference.LogicalName, entityReference.Id, new ColumnSet(new string[4]
            {
        "bsd_paymentschemecode",
        "bsd_startdate",
        "bsd_name",
        "bsd_paymentschemeid"
            }));
            int index1 = 0;
            for (int index2 = 0; index2 < entityCollection.Entities.Count; ++index2)
            {
                if (entityCollection.Entities[index2].Contains("bsd_duedatecalculatingmethod"))
                {
                    int num1 = ((OptionSetValue)entityCollection.Entities[index2]["bsd_duedatecalculatingmethod"]).Value;
                    if (index2 == 0 && num1 != 100000001 || num1 == 100000002 || num1 == 100000000)
                        break;
                    if (num1 == 100000001)
                    {
                        int num2 = ((OptionSetValue)entityCollection.Entities[index2]["bsd_nextperiodtype"]).Value;
                        int day = 0;
                        if (entityCollection.Entities[index2].Contains("bsd_datepaymentofmonthly"))
                            day = (int)entityCollection.Entities[index2]["bsd_datepaymentofmonthly"];
                        int? nullable1;
                        int? nullable2;
                        if (!entityCollection.Entities[index2].Contains("bsd_typepayment"))
                        {
                            nullable1 = new int?();
                            nullable2 = nullable1;
                        }
                        else
                            nullable2 = new int?(((OptionSetValue)entityCollection.Entities[index2]["bsd_typepayment"]).Value);
                        int? nullable3 = nullable2;
                        if (day != 0)
                            dt_date = new DateTime(dt_date.Year, dt_date.Month, day);
                        Entity entity = new Entity();
                        int num3;
                        if (nullable3.HasValue)
                        {
                            nullable1 = nullable3;
                            int num4 = 1;
                            num3 = nullable1.GetValueOrDefault() == num4 & nullable1.HasValue ? 1 : 0;
                        }
                        else
                            num3 = 1;
                        if (num3 != 0)
                        {
                            switch (((OptionSetValue)entityCollection.Entities[index2]["bsd_nextperiodtype"]).Value)
                            {
                                case 1:
                                    int months = (int)entityCollection.Entities[index2]["bsd_numberofnextmonth"];
                                    dt_date = dt_date.AddMonths(months);
                                    break;
                                case 2:
                                    double num5 = double.Parse(entityCollection.Entities[index2]["bsd_numberofnextdays"].ToString());
                                    dt_date = dt_date.AddDays(num5);
                                    break;
                            }
                            entity.LogicalName = ec_pms.Entities[index1].LogicalName;
                            entity.Id = ec_pms.Entities[index1].Id;
                            entity["bsd_duedate"] = (object)dt_date;
                            this.service.Update(entity);
                            ++index1;
                        }
                        else
                        {
                            nullable1 = nullable3;
                            int num6 = 2;
                            if (nullable1.GetValueOrDefault() == num6 & nullable1.HasValue)
                            {
                                int num7 = (int)entityCollection.Entities[index2]["bsd_number"];
                                int num8 = 0;
                                if (entityCollection.Entities[index2].Contains("bsd_nextdaysofendphase"))
                                    num8 = (int)entityCollection.Entities[index2]["bsd_nextdaysofendphase"];
                                for (int index3 = 0; index3 < num7; ++index3)
                                {
                                    if (index3 == num7 - 1)
                                        dt_date = dt_date.AddDays((double)num8);
                                    entity.LogicalName = ec_pms.Entities[index1].LogicalName;
                                    entity.Id = ec_pms.Entities[index1].Id;
                                    entity["bsd_duedate"] = (object)dt_date;
                                    this.service.Update(entity);
                                    ++index1;
                                }
                            }
                        }
                    }
                }
            }
        }

        private EntityCollection GetPSD(string OptionEntryID)
        {
            QueryExpression query = new QueryExpression("bsd_paymentschemedetail");
            query.ColumnSet = new ColumnSet(new string[4]
            {
        "bsd_ordernumber",
        "bsd_paymentschemedetailid",
        "statuscode",
        "bsd_amountwaspaid"
            });
            query.Distinct = true;
            query.Criteria = new FilterExpression();
            query.Criteria.AddCondition("bsd_optionentry", ConditionOperator.Equal, (object)OptionEntryID);
            query.AddOrder("bsd_ordernumber", OrderType.Ascending);
            return this.service.RetrieveMultiple((QueryBase)query);
        }

        private void Up_Ins_OE(
          Entity en_Ins,
          Entity en_OE,
          Decimal psd_amountPaid,
          Decimal psd_balance,
          int psd_statuscode,
          Decimal ampay,
          bool f_1st,
          DateTime d_now,
          int lateday,
          Decimal interestcharge)
        {
            int num1 = en_Ins.Contains("bsd_ordernumber") ? (int)en_Ins["bsd_ordernumber"] : 1;
            Decimal num2 = en_Ins.Contains("bsd_depositamount") ? ((Money)en_Ins["bsd_depositamount"]).Value : 0M;
            int num3 = en_Ins.Contains("bsd_interestchargestatus") ? ((OptionSetValue)en_Ins["bsd_interestchargestatus"]).Value : 100000000;
            bool flag1 = en_Ins.Contains("bsd_maintenancefeesstatus") && (bool)en_Ins["bsd_maintenancefeesstatus"];
            bool flag2 = en_Ins.Contains("bsd_managementfeesstatus") && (bool)en_Ins["bsd_managementfeesstatus"];
            Guid id = en_OE.Id;
            EntityCollection allMisNotPaid = this.get_All_MIS_NotPaid(id.ToString());
            int num4 = en_OE.Contains("statuscode") ? ((OptionSetValue)en_OE["statuscode"]).Value : 100000000;
            Entity entity1 = this.service.Retrieve("product", ((EntityReference)en_OE["bsd_unitnumber"]).Id, new ColumnSet(new string[2]
            {
        "bsd_handovercondition",
        "statuscode"
            }));
            id = en_OE.Id;
            EntityCollection psd = this.GetPSD(id.ToString());
            Entity entity2 = psd.Entities[0];
            int count = psd.Entities.Count;
            id = psd.Entities[count - 1].Id;
            string str1 = id.ToString();
            Entity entity3 = new Entity(en_Ins.LogicalName);
            entity3.Id = en_Ins.Id;
            entity3["bsd_amountwaspaid"] = (object)new Money(psd_amountPaid);
            entity3["bsd_balance"] = (object)new Money(psd_balance);
            entity3["statuscode"] = (object)new OptionSetValue(psd_statuscode);
            if (lateday > 0)
                entity3["bsd_actualgracedays"] = (object)lateday;
            Decimal num5 = entity3.Contains("bsd_interestchargeamount") ? ((Money)entity3["bsd_interestchargeamount"]).Value : 0M;
            if (interestcharge > 0M)
                entity3["bsd_interestchargeamount"] = (object)new Money(interestcharge + num5);
            if (psd_statuscode == 100000001)
                entity3["bsd_paiddate"] = (object)d_now;
            this.service.Update(entity3);
            Entity entity4 = new Entity(en_OE.LogicalName);
            entity4.Id = en_OE.Id;
            int num6 = 100000003;
            int num7 = 100000000;
            int num8;
            int num9;
            if (num1 == 1)
            {
                if (en_OE.Contains("bsd_signedcontractdate"))
                {
                    num8 = 100000002;
                    num9 = 100000002;
                    f_1st = true;
                }
                else if (psd_statuscode == 100000000)
                {
                    f_1st = false;
                    num8 = 100000000;
                    num9 = 100000003;
                }
                else
                {
                    f_1st = true;
                    num8 = 100000001;
                    num9 = 100000001;
                }
            }
            else
            {
                f_1st = true;
                if (!en_OE.Contains("bsd_signedcontractdate"))
                {
                    num8 = 100000001;
                    num9 = 100000001;
                }
                else
                {
                    num9 = 100000002;
                    num8 = 100000003;
                    string str2 = str1;
                    id = en_Ins.Id;
                    string str3 = id.ToString();
                    if (((!(str2 == str3) || psd_statuscode != 100000001 ? 0 : (num3 == 100000001 ? 1 : 0)) & (flag1 ? 1 : 0) & (flag2 ? 1 : 0)) != 0 && allMisNotPaid != null && allMisNotPaid.Entities.Count == 0)
                        num8 = 100000004;
                }
            }
            if (!f_1st)
            {
                num8 = num7;
                num9 = num6;
            }
            entity1["statuscode"] = (object)new OptionSetValue(num9);
            this.service.Update(entity1);
            entity4["statuscode"] = (object)new OptionSetValue(num8);
            this.service.Update(entity4);
        }

        private bool check_Ins_Paid(IOrganizationService crmservices, Guid oeID, int i_ordernumber)
        {
            bool flag = false;
            string query = string.Format("<fetch version='1.0' output-format='xml-platform' mapping='logical' distinct='true' >\n                <entity name='bsd_paymentschemedetail' >\n                <attribute name='bsd_amountpay' />\n                <attribute name='statuscode' />\n                <attribute name='bsd_name' />\n                <attribute name='bsd_ordernumber' />\n                <attribute name='bsd_paymentschemedetailid' />\n                <filter type='and' >\n                <condition attribute='bsd_ordernumber' operator='eq' value='{0}' />\n                  <condition attribute='bsd_optionentry' operator='eq' value='{1}' />\n                </filter>\n              </entity>\n            </fetch>", (object)i_ordernumber, (object)oeID);
            EntityCollection entityCollection = crmservices.RetrieveMultiple((QueryBase)new FetchExpression(query));
            if (entityCollection.Entities.Count > 0)
            {
                Entity entity = entityCollection.Entities[0];
                entity.Id = entityCollection.Entities[0].Id;
                if (((OptionSetValue)entity["statuscode"]).Value == 100000001)
                    flag = true;
                if (((OptionSetValue)entity["statuscode"]).Value == 100000000)
                    flag = false;
            }
            return flag;
        }

        private void Update_Interest(
          IOrganizationService ser,
          Entity en_interIns,
          Decimal psd_bsd_interestwaspaid,
          int psd_bsd_interestchargestatus)
        {
            ser.Update(new Entity(en_interIns.LogicalName)
            {
                Id = en_interIns.Id,
                ["bsd_interestwaspaid"] = (object)new Money(psd_bsd_interestwaspaid),
                ["bsd_interestchargestatus"] = (object)new OptionSetValue(psd_bsd_interestchargestatus)
            });
        }

        private void calc_InterestCharge(
          Entity e_InterMaster,
          int i_day,
          Decimal amountPay,
          Entity OE,
          Entity en_PmsDtl,
          IOrganizationService serv,
          out int i_late,
          out Decimal d_interAM)
        {
            Decimal num1 = e_InterMaster.Contains("bsd_termsinterestpercentage") ? (Decimal)e_InterMaster["bsd_termsinterestpercentage"] : throw new InvalidPluginExecutionException("Please input 'Terms interest' data in Interest rate master:" + (string)e_InterMaster["bsd_name"]);
            Entity entity1 = serv.Retrieve("bsd_project", ((EntityReference)OE["bsd_project"]).Id, new ColumnSet(new string[2]
            {
        "bsd_name",
        "bsd_dailyinterestchargebank"
            }));
            bool flag = entity1.Contains("bsd_dailyinterestchargebank") && (bool)entity1["bsd_dailyinterestchargebank"];
            Decimal num2 = 0M;
            if (flag)
            {
                EntityCollection dailyinterestrate = this.get_ec_bsd_dailyinterestrate(serv, entity1.Id);
                Entity entity2 = dailyinterestrate.Entities[0];
                entity2.Id = dailyinterestrate.Entities[0].Id;
                num2 = entity2.Contains("bsd_interestrate") ? (Decimal)entity2["bsd_interestrate"] : throw new InvalidPluginExecutionException("Can not find Daily Interestrate for Project " + (string)entity1["bsd_name"] + " in master data. Please check again!");
            }
            Decimal num3 = num1 + num2;
            if (!e_InterMaster.Contains("bsd_intereststartdatetype"))
                throw new InvalidPluginExecutionException("Please input 'Interest Start Date Type' data in Interest rate master:" + (string)e_InterMaster["bsd_name"]);
            Decimal num4 = e_InterMaster.Contains("bsd_toleranceinterestamount") ? ((Money)e_InterMaster["bsd_toleranceinterestamount"]).Value : 0M;
            Decimal num5 = e_InterMaster.Contains("bsd_toleranceinterestpercentage") ? (Decimal)e_InterMaster["bsd_toleranceinterestpercentage"] : 0M;
            Decimal num6 = num3 / 30M / 100M * (Decimal)i_day;
            Decimal num7 = Convert.ToDecimal(amountPay) * num6;
            Decimal num8 = this.SumInterestAM_OE(this.service, OE.Id);
            Decimal num9 = num8 + num7;
            Decimal num10 = OE.Contains("totalamount") ? ((Money)OE["totalamount"]).Value : 0M;
            if (num10 == 0M)
                throw new InvalidPluginExecutionException("'Net Selling Price' must be larger than 0");
            int num11 = 0;
            Decimal num12 = 0M;
            Decimal num13;
            if (num5 > 0M)
            {
                Decimal num14 = num10 * num5 / 100M;
                num13 = !(num4 > 0M) ? num14 : (!(num14 > num4) ? num14 : num4);
            }
            else
                num13 = num4 > 0M ? num4 : 0M;
            if (num9 >= num13)
                num7 = num13 > num8 ? num13 - num8 : 0M;
            if (num7 > 0M)
            {
                if (OE.Contains("bsd_signedcontractdate") || OE.Contains("bsd_signeddadate"))
                {
                    Decimal num15 = en_PmsDtl.Contains("bsd_interestchargeamount") ? ((Money)en_PmsDtl["bsd_interestchargeamount"]).Value : 0M;
                    Entity entity3 = new Entity(en_PmsDtl.LogicalName);
                    entity3.Id = en_PmsDtl.Id;
                    Decimal num16 = num15 + Convert.ToDecimal(num7);
                    if (num16 < 0M)
                        num16 = 0M;
                    num12 = num16;
                    entity3["bsd_interestchargeamount"] = (object)new Money(num16);
                    int num17 = en_PmsDtl.Contains("bsd_actualgracedays") ? (int)en_PmsDtl["bsd_actualgracedays"] : 0;
                    if (i_day >= num17)
                        entity3["bsd_actualgracedays"] = (object)i_day;
                    serv.Update(entity3);
                }
                num11 = i_day;
            }
            i_late = num11;
            d_interAM = num7;
        }

        private Decimal SumInterestAM_OE(IOrganizationService crmservices, Guid OEID)
        {
            Decimal num = 0M;
            string query = string.Format("<fetch version='1.0' output-format='xml-platform' mapping='logical' distinct='false'>\n                  <entity name='bsd_paymentschemedetail' >\n                    <attribute name='bsd_interestchargestatus' />\n                    <attribute name='bsd_interestchargeamount' />\n                    <filter type='and' >\n                      <condition attribute='bsd_optionentry' operator='eq' value='{0}' />\n                        <condition attribute='bsd_interestchargeamount' operator='not-null' />\n                    </filter>\n                    <order attribute='bsd_ordernumber' descending='true' />\n                  </entity>\n                </fetch>", (object)OEID);
            foreach (Entity entity in (Collection<Entity>)crmservices.RetrieveMultiple((QueryBase)new FetchExpression(query)).Entities)
                num += entity.Contains("bsd_interestchargeamount") ? ((Money)entity["bsd_interestchargeamount"]).Value : 0M;
            return num;
        }

        private EntityCollection get_ec_bsd_dailyinterestrate(
          IOrganizationService crmservices,
          Guid projID)
        {
            string query = string.Format("<fetch version='1.0' output-format='xml-platform' mapping='logical' distinct='false'>\n                  <entity name='bsd_dailyinterestrate' >\n                    <attribute name='bsd_date' />\n                    <attribute name='bsd_project' />\n                    <attribute name='bsd_interestrate' />\n                    <attribute name='createdon' />\n                    <filter type='and' >\n                      <condition attribute='bsd_project' operator='eq' value='{0}' />\n                        <condition attribute='statuscode' operator='eq' value='1' />\n                    </filter>\n                    <order attribute='createdon' descending='true' />\n                  </entity>\n                </fetch>", (object)projID);
            return crmservices.RetrieveMultiple((QueryBase)new FetchExpression(query));
        }

        private Entity getInstallment(IOrganizationService crmservices, string installmentid)
        {
            string query = string.Format("<fetch version='1.0' output-format='xml-platform' mapping='logical' distinct='true' >\n                <entity name='bsd_paymentschemedetail' >\n                <attribute name='bsd_duedatecalculatingmethod' />\n                <attribute name='bsd_maintenanceamount' />\n                <attribute name='bsd_maintenancefeepaid' />\n                <attribute name='bsd_maintenancefeewaiver' />\n                <attribute name='bsd_ordernumber' />\n                <attribute name='statuscode' />\n                <attribute name='bsd_managementfeepaid' />\n                <attribute name='bsd_managementfeewaiver' />\n                <attribute name='bsd_amountpay' />\n                <attribute name='bsd_maintenancefees' />\n                <attribute name='bsd_optionentry' />\n                <attribute name='bsd_managementfee' />\n                <attribute name='bsd_managementfeesstatus' />\n                <attribute name='bsd_managementamount' />\n                <attribute name='bsd_amountwaspaid' />\n                <attribute name='bsd_maintenancefeesstatus' />\n                <attribute name='bsd_name' />\n                <attribute name='bsd_paymentschemedetailid' />\n                <filter type='and' >\n                  <condition attribute='bsd_paymentschemedetailid' operator='eq' value='{0}' />\n                  \n                </filter>\n              </entity>\n            </fetch>", (object)installmentid);
            return crmservices.RetrieveMultiple((QueryBase)new FetchExpression(query)).Entities[0];
        }

        private void create_app_detail(
          IOrganizationService serv,
          Entity en_app,
          Entity en_Ins,
          Entity en_OE,
          int i_pmtype,
          Decimal appAM,
          int i_outstanding_AppDtl,
          Decimal d_bsd_interestchargeamount,
          int fee_type)
        {
            Entity entity = new Entity("bsd_applydocumentdetail");
            entity.Id = Guid.NewGuid();
            string str = "";
            switch (i_pmtype)
            {
                case 100000000:
                    str = "Deposit";
                    break;
                case 100000001:
                    str = "Installment";
                    break;
                case 100000002:
                    str = "Fees";
                    break;
                case 100000003:
                    str = "Interest charge";
                    break;
                case 100000004:
                    str = "Other";
                    break;
            }
            entity["bsd_name"] = (object)("Apply document detail-" + (string)en_app["bsd_name"] + "-" + (string)en_Ins["bsd_name"] + "-" + str);
            entity["bsd_applydocument"] = (object)en_app.ToEntityReference();
            entity["statuscode"] = (object)new OptionSetValue(100000000);
            entity["bsd_paymenttype"] = (object)new OptionSetValue(i_pmtype);
            entity["bsd_installment"] = (object)en_Ins.ToEntityReference();
            entity["bsd_amountapply"] = (object)new Money(appAM);
            if (i_pmtype == 100000002)
            {
                entity["bsd_optionentry"] = (object)en_OE.ToEntityReference();
                if (fee_type == 1)
                {
                    entity["bsd_name"] = (object)("Apply document detail-" + (string)en_app["bsd_name"] + "-" + (string)en_Ins["bsd_name"] + "-Maitenance Fees");
                    entity["bsd_feetype"] = (object)new OptionSetValue(100000000);
                }
                else
                {
                    entity["bsd_name"] = (object)("Apply document detail-" + (string)en_app["bsd_name"] + "-" + (string)en_Ins["bsd_name"] + "-Management Fees");
                    entity["bsd_feetype"] = (object)new OptionSetValue(100000001);
                }
            }
            if (i_pmtype == 100000000)
                entity["bsd_reservation"] = (object)en_OE.ToEntityReference();
            if (i_pmtype == 100000001 || i_pmtype == 100000003)
            {
                entity["bsd_optionentry"] = (object)en_OE.ToEntityReference();
                if (en_OE.Contains("bsd_signeddadate") || en_OE.Contains("bsd_signedcontractdate"))
                {
                    entity["bsd_actualgracedays"] = (object)i_outstanding_AppDtl;
                    entity["bsd_interestchargeamount"] = (object)new Money(d_bsd_interestchargeamount);
                }
                else
                {
                    Decimal num = 0M;
                    entity["bsd_actualgracedays"] = (object)num;
                    entity["bsd_interestchargeamount"] = (object)new Money(num);
                }
            }
            this.service.Create(entity);
        }

        private void create_app_detail_MISS(
          IOrganizationService serv,
          Entity en_app,
          Entity en_Ins,
          Entity en_OE,
          int i_pmtype,
          Decimal appAM,
          int i_outstanding_AppDtl,
          Decimal d_bsd_interestchargeamount,
          int fee_type,
          Entity Miss_ID)
        {
            Entity entity = new Entity("bsd_applydocumentdetail");
            entity.Id = Guid.NewGuid();
            string str = "";
            switch (i_pmtype)
            {
                case 100000000:
                    str = "Deposit";
                    break;
                case 100000001:
                    str = "Installment";
                    break;
                case 100000002:
                    str = "Fees";
                    break;
                case 100000003:
                    str = "Interest charge";
                    break;
                case 100000004:
                    str = "Other";
                    break;
            }
            entity["bsd_name"] = (object)("Apply document detail-" + (string)en_app["bsd_name"] + "-" + (string)en_Ins["bsd_name"] + "-" + str);
            entity["bsd_applydocument"] = (object)en_app.ToEntityReference();
            entity["statuscode"] = (object)new OptionSetValue(100000000);
            entity["bsd_paymenttype"] = (object)new OptionSetValue(i_pmtype);
            entity["bsd_installment"] = (object)en_Ins.ToEntityReference();
            entity["bsd_amountapply"] = (object)new Money(appAM);
            if (i_pmtype == 100000002)
            {
                entity["bsd_optionentry"] = (object)en_OE.ToEntityReference();
                if (fee_type == 1)
                {
                    entity["bsd_name"] = (object)("Apply document detail-" + (string)en_app["bsd_name"] + "-" + (string)en_Ins["bsd_name"] + "-Maitenance Fees");
                    entity["bsd_feetype"] = (object)new OptionSetValue(100000000);
                }
                else
                {
                    entity["bsd_name"] = (object)("Apply document detail-" + (string)en_app["bsd_name"] + "-" + (string)en_Ins["bsd_name"] + "-Management Fees");
                    entity["bsd_feetype"] = (object)new OptionSetValue(100000001);
                }
            }
            if (i_pmtype == 100000000)
                entity["bsd_reservation"] = (object)en_OE.ToEntityReference();
            if (i_pmtype == 100000004)
            {
                entity["bsd_optionentry"] = (object)en_OE.ToEntityReference();
                entity["bsd_miscellaneous"] = (object)Miss_ID.ToEntityReference();
            }
            if (i_pmtype == 100000001 || i_pmtype == 100000003)
            {
                entity["bsd_optionentry"] = (object)en_OE.ToEntityReference();
                if (en_OE.Contains("bsd_signeddadate") || en_OE.Contains("bsd_signedcontractdate"))
                {
                    entity["bsd_actualgracedays"] = (object)i_outstanding_AppDtl;
                    entity["bsd_interestchargeamount"] = (object)new Money(d_bsd_interestchargeamount);
                }
                else
                {
                    Decimal num = 0M;
                    entity["bsd_actualgracedays"] = (object)num;
                    entity["bsd_interestchargeamount"] = (object)new Money(num);
                }
            }
            this.service.Create(entity);
        }

        private void create_Applydocument_RemainingCOA(
          IOrganizationService service,
          Entity enApplyDocument,
          Entity enAdvancePayment,
          Decimal bsd_advancepaymentamount,
          Decimal apply,
          Decimal remain,
          Decimal bsd_avPMPaid)
        {
            Entity entity = new Entity("bsd_applydocumentremainingcoa");
            entity.Id = new Guid();
            DateTime dateTime = this.RetrieveLocalTimeFromUTCTime(DateTime.Now);
            entity["bsd_name"] = (object)("Apply Document Remaining COA-" + (string)enAdvancePayment["bsd_name"]);
            entity["bsd_applydate"] = (object)dateTime;
            entity["bsd_applydocument"] = (object)enApplyDocument.ToEntityReference();
            entity["bsd_advancepayment"] = (object)enAdvancePayment.ToEntityReference();
            entity[nameof(bsd_advancepaymentamount)] = (object)(Money)enAdvancePayment["bsd_amount"];
            entity["bsd_advancepaymentapply"] = (object)new Money(apply);
            entity["bsd_advancepaymentpaid"] = (object)new Money(bsd_avPMPaid);
            entity["bsd_advancepaymentremaining"] = (object)new Money(remain);
            service.Create(entity);
        }

        public DateTime RetrieveLocalTimeFromUTCTime(DateTime utcTime)
        {
            return ((LocalTimeFromUtcTimeResponse)this.service.Execute((OrganizationRequest)new LocalTimeFromUtcTimeRequest()
            {
                TimeZoneCode = (this.RetrieveCurrentUsersSettings(this.service) ?? throw new InvalidPluginExecutionException("Can't find time zone code")),
                UtcTime = utcTime.ToUniversalTime()
            })).LocalTime;
        }

        private int? RetrieveCurrentUsersSettings(IOrganizationService service)
        {
            IOrganizationService organizationService = service;
            QueryExpression queryExpression1 = new QueryExpression("usersettings");
            queryExpression1.ColumnSet = new ColumnSet(new string[2]
            {
        "localeid",
        "timezonecode"
            });
            QueryExpression queryExpression2 = queryExpression1;
            FilterExpression filterExpression = new FilterExpression();
            filterExpression.Conditions.Add(new ConditionExpression("systemuserid", ConditionOperator.EqualUserId));
            queryExpression2.Criteria = filterExpression;
            QueryExpression query = queryExpression1;
            return (int?)organizationService.RetrieveMultiple((QueryBase)query).Entities[0].ToEntity<Entity>().Attributes["timezonecode"];
        }

        public EntityCollection get_All_MIS_NotPaid(string oeID)
        {
            return this.service.RetrieveMultiple((QueryBase)new FetchExpression(string.Format("<fetch version='1.0' output-format='xml-platform' mapping='logical' distinct='true' >\n                <entity name='bsd_miscellaneous' >\n                <attribute name='bsd_balance' />\n                <attribute name='statuscode' />\n                <attribute name='bsd_miscellaneousnumber' />\n                <attribute name='bsd_units' />\n                <attribute name='bsd_optionentry' />\n                <attribute name='bsd_miscellaneousid' />\n                <attribute name='bsd_amount' />\n                <attribute name='bsd_paidamount' />\n                <attribute name='bsd_installment' />\n                <attribute name='bsd_name' />\n                <attribute name='bsd_project' />\n                <attribute name='bsd_installmentnumber' />\n                <filter type='and' >\n                    <condition attribute='bsd_optionentry' operator='eq' value='{0}' />\n                    <condition attribute='statecode' operator='eq' value='0' />\n                    <condition attribute='statuscode' operator='eq' value='1' />\n                </filter>                           \n                </entity>\n            </fetch>", (object)oeID)));
        }

        public string GetCurrentMethod() => new StackTrace().GetFrame(1).GetMethod().Name;
    }
}
