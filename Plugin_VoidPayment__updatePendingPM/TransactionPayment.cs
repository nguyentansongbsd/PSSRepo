using Microsoft.Crm.Sdk.Messages;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Query;
using System;
using System.Text;

namespace Plugin_VoidPayment_updatePendingPM
{
    class TransactionPayment
    {
        IOrganizationService service = null;
        IPluginExecutionContext context;
        StringBuilder strMess = new StringBuilder();
        public TransactionPayment(IOrganizationService service , IPluginExecutionContext context, StringBuilder strMess1)
        {
            this.service = service;
            this.context = context;
            strMess = strMess1;
        }
        public EntityCollection get_TransactionPM(IOrganizationService crmservices, Guid pmID)
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
                  <condition attribute='statuscode' operator='eq' value='100000000' />
                </filter>
              </entity>
                </fetch>";
            fetchXml = string.Format(fetchXml, pmID);
            EntityCollection entc = crmservices.RetrieveMultiple(new FetchExpression(fetchXml));
            return entc;
        }
        public decimal update_transactionPM(Guid pmID, Entity en_OE)
        {
            decimal oePaid = 0;
            EntityCollection ec_transactionPM = get_TransactionPM(service, pmID);
            strMess.AppendLine("update_transactionPM:" + ec_transactionPM.Entities.Count.ToString());
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

                    oePaid += update_INS_transc(en_trans, en_OE);
                    // update amount paid of installment or interestcharge of installment in savegriddata
                    //if (i_trans_bsd_transactiontype != 100000002)
                    //    oePaid += update_INS_transc(crmser, en_trans, en_OE);
                    //Thạnh Đỗ
                    //else //Fee = 100000002
                    //{
                    //    #region  ----------------- retrive installment in transaction payment---------------
                    //    Entity enInstallment = crmser.Retrieve("bsd_paymentschemedetail", ((EntityReference)en_trans["bsd_installment"]).Id,
                    //        new ColumnSet(true));
                    //    #endregion

                    //    bool f_main = enInstallment.Contains("bsd_maintenancefeesstatus") ? (bool)enInstallment["bsd_maintenancefeesstatus"] : false;
                    //    bool f_mana = enInstallment.Contains("bsd_managementfeesstatus") ? (bool)enInstallment["bsd_managementfeesstatus"] : false;

                    //    Entity enInstallment_Update = new Entity(enInstallment.LogicalName);
                    //    enInstallment_Update.Id = enInstallment.Id;

                    //    if (i_bsd_feetype == 100000000) // main
                    //    {
                    //        decimal d_bsd_maintenancefeepaid = enInstallment.Contains("bsd_maintenancefeepaid") ? ((Money)enInstallment["bsd_maintenancefeepaid"]).Value : 0;
                    //        f_main = false;
                    //        d_bsd_maintenancefeepaid -= d_trans_bsd_amount;
                    //        enInstallment_Update["bsd_maintenancefeepaid"] = new Money(d_bsd_maintenancefeepaid);
                    //        enInstallment_Update["bsd_maintenancefeesstatus"] = f_main;
                    //    }
                    //    if (i_bsd_feetype == 100000001)// mana
                    //    {
                    //        decimal d_bsd_managementfeepaid = enInstallment.Contains("bsd_managementfeepaid") ? ((Money)enInstallment["bsd_managementfeepaid"]).Value : 0;

                    //        f_mana = false;
                    //        d_bsd_managementfeepaid -= d_trans_bsd_amount;
                    //        enInstallment_Update["bsd_managementfeepaid"] = new Money(d_bsd_managementfeepaid);
                    //        enInstallment_Update["bsd_managementfeesstatus"] = f_mana;
                    //    }
                    //    crmser.Update(enInstallment_Update);

                    //}
                }
            }
            return oePaid;
        }
        public decimal update_INS_transc(Entity en_trans, Entity en_OE)
        {
            decimal d_oePiad = 0;
            #region  ----------------- retrive installment in transaction payment---------------
            //Entity enInstallment = service.Retrieve("bsd_paymentschemedetail", ((EntityReference)en_trans["bsd_installment"]).Id,
            //    new ColumnSet(new string[]{
            //    "bsd_amountofthisphase",
            //    "bsd_amountwaspaid",
            //    "bsd_depositamount",
            //    "statuscode",
            //    "bsd_duedatecalculatingmethod",
            //    "bsd_maintenancefees",
            //    "bsd_managementfee",
            //    "bsd_paymentscheme",
            //    "bsd_lastinstallment",
            //    "bsd_balance",
            //    "bsd_actualgracedays",
            //    "bsd_interestchargeamount",
            //    "bsd_interestchargestatus",
            //    "bsd_interestwaspaid",
            //    "bsd_maintenancefeesstatus",
            //    "bsd_managementfeesstatus",
            //    "bsd_managementamount",
            //    "bsd_maintenanceamount",
            //    "bsd_maintenancefeepaid",
            //    "bsd_managementfeepaid",
            //    "bsd_paiddate"
            //}));
            Entity enInstallment = service.Retrieve("bsd_paymentschemedetail", ((EntityReference)en_trans["bsd_installment"]).Id,new ColumnSet(true));
            #endregion
            strMess.AppendLine("Installment ID: "+ enInstallment.Id.ToString());
            decimal d_bsd_amountofthisphase = enInstallment.Contains("bsd_amountofthisphase") ? ((Money)enInstallment["bsd_amountofthisphase"]).Value : 0;
            decimal d_bsd_amountwaspaid = enInstallment.Contains("bsd_amountwaspaid") ? ((Money)enInstallment["bsd_amountwaspaid"]).Value : 0;
            int i_INS_statuscode = enInstallment.Contains("statuscode") ? ((OptionSetValue)enInstallment["statuscode"]).Value : 0;
            decimal d_bsd_balance = enInstallment.Contains("bsd_balance") ? ((Money)enInstallment["bsd_balance"]).Value : 0;

            decimal d_bsd_interestchargeamount = enInstallment.Contains("bsd_interestchargeamount") ? ((Money)enInstallment["bsd_interestchargeamount"]).Value : 0;
            decimal d_bsd_interestwaspaid = enInstallment.Contains("bsd_interestwaspaid") ? ((Money)enInstallment["bsd_interestwaspaid"]).Value : 0;
            int i_bsd_interestchargestatus = enInstallment.Contains("bsd_interestchargestatus") ? ((OptionSetValue)enInstallment["bsd_interestchargestatus"]).Value : 100000000;
            int i_bsd_actualgracedays = enInstallment.Contains("bsd_actualgracedays") ? (int)enInstallment["bsd_actualgracedays"] : 0;

            decimal d_bsd_maintenancefeepaid = enInstallment.Contains("bsd_maintenancefeepaid") ? ((Money)enInstallment["bsd_maintenancefeepaid"]).Value : 0;
            decimal d_bsd_managementfeepaid = enInstallment.Contains("bsd_managementfeepaid") ? ((Money)enInstallment["bsd_managementfeepaid"]).Value : 0;
            strMess.AppendLine("bsd_maintenancefeepaid befor voide : " + d_bsd_maintenancefeepaid.ToString());
            strMess.AppendLine("bsd_managementfeepaid befor voide : " + d_bsd_managementfeepaid.ToString());





            Entity enInstallment_Update = new Entity(enInstallment.LogicalName);
            enInstallment_Update.Id = enInstallment.Id;

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
            strMess.AppendLine("bsd_transactiontype: " + i_trans_bsd_transactiontype.ToString());
            strMess.AppendLine("bsd_feetype: " + i_bsd_feetype.ToString());
            strMess.AppendLine("transaction amount: " + d_trans_bsd_amount.ToString());
            #region --------- transaction type = interestcharge = 10000001 -----------
            if (i_trans_bsd_transactiontype == 100000001) // update interestcharge trong installment
            {
                // revert - thi ins da tre- k can revert lai so ngay outstanding day
                updateInterestcharge(en_trans,enInstallment);
            }
            #endregion

            #region ----------- transaction type = Installment = 100000000 -------------------------
            if (i_trans_bsd_transactiontype == 100000000) // update installment
            {
                updateInstallment(en_trans, enInstallment,ref d_oePiad);
                
            }
            #endregion ------------
            strMess.AppendLine("Fees = 100000002");
            #region  -------- transaction type = Fees = 100000002 -----------------
            if (i_trans_bsd_transactiontype == 100000002)
            {
                updateFees(en_trans, enInstallment, ref d_oePiad);
                
            }
            #endregion ------------

            #region  ---------------- transaction type = Other / MISS = 100000003 -----------------
            if (i_trans_bsd_transactiontype == 100000003)
            {
                updateMiscellaneous(en_trans, enInstallment, ref d_oePiad);
                


            }
            #endregion ----------------------------------

            return d_oePiad;
        }
        private void updateInterestcharge(Entity en_trans, Entity enInstallment)
        {
            int i_bsd_interestchargestatus = enInstallment.Contains("bsd_interestchargestatus") ? ((OptionSetValue)enInstallment["bsd_interestchargestatus"]).Value : 100000000;
            decimal d_bsd_interestwaspaid = enInstallment.Contains("bsd_interestwaspaid") ? ((Money)enInstallment["bsd_interestwaspaid"]).Value : 0;
            decimal d_trans_bsd_amount = en_trans.Contains("bsd_amount") ? ((Money)en_trans["bsd_amount"]).Value : 0;
            if (i_bsd_interestchargestatus == 100000001) i_bsd_interestchargestatus = 100000000;
            d_bsd_interestwaspaid -= d_trans_bsd_amount;
            Entity enInstallment_Update = new Entity(enInstallment.LogicalName,enInstallment.Id);
            // k update lai lateday cua installment & OE vi interestcharge nay co san trong installment r
            enInstallment_Update["bsd_interestchargestatus"] = new OptionSetValue(i_bsd_interestchargestatus);
            enInstallment_Update["bsd_interestwaspaid"] = new Money(d_bsd_interestwaspaid);
            service.Update(enInstallment_Update);
        }
        private void updateInstallment(Entity en_trans, Entity enInstallment,ref decimal d_oePiad)
        {
            int i_INS_statuscode = enInstallment.Contains("statuscode") ? ((OptionSetValue)enInstallment["statuscode"]).Value : 0;
            decimal d_bsd_amountwaspaid = enInstallment.Contains("bsd_amountwaspaid") ? ((Money)enInstallment["bsd_amountwaspaid"]).Value : 0;
            decimal d_bsd_interestchargeamount = enInstallment.Contains("bsd_interestchargeamount") ? ((Money)enInstallment["bsd_interestchargeamount"]).Value : 0;
            decimal d_bsd_balance = enInstallment.Contains("bsd_balance") ? ((Money)enInstallment["bsd_balance"]).Value : 0;
            decimal d_bsd_amountofthisphase = enInstallment.Contains("bsd_amountofthisphase") ? ((Money)enInstallment["bsd_amountofthisphase"]).Value : 0;
            int i_bsd_actualgracedays = enInstallment.Contains("bsd_actualgracedays") ? (int)enInstallment["bsd_actualgracedays"] : 0;

            decimal d_trans_bsd_amount = en_trans.Contains("bsd_amount") ? ((Money)en_trans["bsd_amount"]).Value : 0;
            decimal d_IC_am = en_trans.Contains("bsd_interestchargeamount") ? ((Money)en_trans["bsd_interestchargeamount"]).Value : 0;
            int i_outstandingday = en_trans.Contains("bsd_actualgracedays") ? (int)en_trans["bsd_actualgracedays"] : 0;
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
            Entity enInstallment_Update = new Entity(enInstallment.LogicalName, enInstallment.Id);
            enInstallment_Update["statuscode"] = new OptionSetValue(i_INS_statuscode);
            enInstallment_Update["bsd_paiddate"] = null;
            enInstallment_Update["bsd_amountwaspaid"] = new Money(d_bsd_amountwaspaid > 0 ? d_bsd_amountwaspaid : 0);
            enInstallment_Update["bsd_interestchargeamount"] = new Money(d_bsd_interestchargeamount);
            enInstallment_Update["bsd_balance"] = new Money(d_bsd_balance);
            enInstallment_Update["bsd_actualgracedays"] = i_bsd_actualgracedays;

            service.Update(enInstallment_Update);
        }
        private void updateFees(Entity en_trans, Entity enInstallment, ref decimal d_oePiad)
        {
            int i_bsd_feetype = en_trans.Contains("bsd_feetype") ? ((OptionSetValue)en_trans["bsd_feetype"]).Value : 0;
            decimal d_trans_bsd_amount = en_trans.Contains("bsd_amount") ? ((Money)en_trans["bsd_amount"]).Value : 0;

            bool f_main = enInstallment.Contains("bsd_maintenancefeesstatus") ? (bool)enInstallment["bsd_maintenancefeesstatus"] : false;
            bool f_mana = enInstallment.Contains("bsd_managementfeesstatus") ? (bool)enInstallment["bsd_managementfeesstatus"] : false;
            decimal d_bsd_maintenanceamount = enInstallment.Contains("bsd_maintenanceamount") ? ((Money)enInstallment["bsd_maintenanceamount"]).Value : 0;
            decimal d_bsd_managementamount = enInstallment.Contains("bsd_managementamount") ? ((Money)enInstallment["bsd_managementamount"]).Value : 0;
            decimal d_bsd_maintenancefeepaid = enInstallment.Contains("bsd_maintenancefeepaid") ? ((Money)enInstallment["bsd_maintenancefeepaid"]).Value : 0;
            decimal d_bsd_managementfeepaid = enInstallment.Contains("bsd_managementfeepaid") ? ((Money)enInstallment["bsd_managementfeepaid"]).Value : 0;
            strMess.AppendLine("bsd_maintenancefeepaid befor voide : "+ d_bsd_maintenancefeepaid.ToString());
            strMess.AppendLine("bsd_managementfeepaid befor voide : " + d_bsd_managementfeepaid.ToString());
            Entity enInstallment_Update = new Entity(enInstallment.LogicalName, enInstallment.Id);
            if (i_bsd_feetype == 100000000) // main
            {
                strMess.AppendLine("main");
                f_main = false;
                d_bsd_maintenancefeepaid -= d_trans_bsd_amount;
                strMess.AppendLine("bsd_maintenancefeepaid:" + d_bsd_maintenancefeepaid.ToString());
                enInstallment_Update["bsd_maintenancefeepaid"] = new Money(d_bsd_maintenancefeepaid);
                enInstallment_Update["bsd_maintenancefeesstatus"] = f_main;
            }
            if (i_bsd_feetype == 100000001)// mana
            {
                strMess.AppendLine("mana");
                f_mana = false;
                d_bsd_managementfeepaid -= d_trans_bsd_amount;
                strMess.AppendLine("bsd_managementfeepaid:" + d_bsd_managementfeepaid.ToString());
                enInstallment_Update["bsd_managementfeepaid"] = new Money(d_bsd_managementfeepaid);
                enInstallment_Update["bsd_managementfeesstatus"] = f_mana;
            }
            strMess.AppendLine("Update Fee");
            service.Update(enInstallment_Update);
            strMess.AppendLine("Update Fee done");
        }
        private void updateMiscellaneous(Entity en_trans, Entity enInstallment, ref decimal d_oePiad)
        {
            decimal d_trans_bsd_amount = en_trans.Contains("bsd_amount") ? ((Money)en_trans["bsd_amount"]).Value : 0;
            Entity en_MIS = service.Retrieve(((EntityReference)en_trans["bsd_miscellaneous"]).LogicalName, ((EntityReference)en_trans["bsd_miscellaneous"]).Id,
                new ColumnSet(new string[] { "bsd_balance","statuscode", "bsd_miscellaneousnumber","bsd_units","bsd_miscellaneousid","bsd_amount","bsd_paidamount",
                "bsd_installment","bsd_name","bsd_project"}));

            int f_paid = en_MIS.Contains("statuscode") ? ((OptionSetValue)en_MIS["statuscode"]).Value : 1;
            decimal d_MI_paid = en_MIS.Contains("bsd_paidamount") ? ((Money)en_MIS["bsd_paidamount"]).Value : 0;
            decimal d_MI_balance = en_MIS.Contains("bsd_balance") ? ((Money)en_MIS["bsd_balance"]).Value : 0;
            decimal d_MI_am = en_MIS.Contains("bsd_amount") ? ((Money)en_MIS["bsd_amount"]).Value : 0;
            strMess.AppendLine("updateMiscellaneous");
            
            Entity en_MIS_up = new Entity(en_MIS.LogicalName);
            en_MIS_up.Id = en_MIS.Id;
            en_MIS_up["bsd_paidamount"] = new Money(d_MI_paid - d_trans_bsd_amount);
            en_MIS_up["bsd_balance"] = new Money(d_MI_balance + d_trans_bsd_amount);
            en_MIS_up["statuscode"] = new OptionSetValue(1);
            service.Update(en_MIS_up);
            strMess.AppendLine("updateMiscellaneous done");
        }
        public EntityCollection getTracsactionPaymentByPayment(Entity enPayment)
        {
            var QEbsd_transactionpayment = new QueryExpression("bsd_transactionpayment");
            QEbsd_transactionpayment.ColumnSet.AllColumns = true;
            QEbsd_transactionpayment.Criteria.AddCondition("bsd_payment", ConditionOperator.Equal, enPayment.Id);
            EntityCollection encl = service.RetrieveMultiple(QEbsd_transactionpayment);
            return encl;
        }
        public EntityCollection getTransactionPaymentByInterest(Entity enInstallment)
        {
            var QEbsd_transactionpayment_bsd_transactiontype = 100000001;
            var QEbsd_transactionpayment = new QueryExpression("bsd_transactionpayment");
            QEbsd_transactionpayment.ColumnSet.AllColumns = true;
            QEbsd_transactionpayment.Criteria.AddCondition("bsd_transactiontype", ConditionOperator.Equal, QEbsd_transactionpayment_bsd_transactiontype);
            //QEbsd_transactionpayment.Criteria.AddCondition("bsd_payment", ConditionOperator.Equal, enPayment.Id);
            QEbsd_transactionpayment.Criteria.AddCondition("bsd_installment", ConditionOperator.Equal, enInstallment.Id);
            EntityCollection encl = service.RetrieveMultiple(QEbsd_transactionpayment);
            return encl;
        }
    }
}
