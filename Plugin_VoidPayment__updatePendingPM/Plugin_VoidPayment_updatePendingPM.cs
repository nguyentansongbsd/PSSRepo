
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
using System.Text;

namespace Plugin_VoidPayment_updatePendingPM
{
    public class Plugin_VoidPayment_updatePendingPM : IPlugin
    {
        IOrganizationService service = null;
        IOrganizationServiceFactory factory = null;
        Payment payment;
        AdvancePayment advancePayment;
        TransactionPayment transactionPayment;
        Installment installment;
        Miscellaneous miscellaneous;
        ApplyDocument applyDocument;
        StringBuilder strMess = new StringBuilder();
        void IPlugin.Execute(IServiceProvider serviceProvider)
        {
            IPluginExecutionContext context = (IPluginExecutionContext)serviceProvider.GetService(typeof(IPluginExecutionContext));
            factory = (IOrganizationServiceFactory)serviceProvider.GetService(typeof(IOrganizationServiceFactory));
            service = factory.CreateOrganizationService(context.UserId);
            Entity target = (Entity)context.InputParameters["Target"];
            payment = new Payment(service, context);
            applyDocument = new ApplyDocument(service, context);
            advancePayment = new AdvancePayment(service, context);
            transactionPayment = new TransactionPayment(service, context);
            installment = new Installment(service, context);
            miscellaneous = new Miscellaneous(service, context);
            if (target.LogicalName == "bsd_voidpayment")
            {
                if (context.Depth > 1)
                    return;
                DateTime d_now = RetrieveLocalTimeFromUTCTime(DateTime.Now);
                Entity en_voidPM = service.Retrieve(target.LogicalName, target.Id, new ColumnSet(true));
                // ---------------- UPDATE -------------------
                //update with status = approve - check installment and current payment - revert
                if ((context.MessageName == "Update") && (target.Contains("statuscode")))
                {
                    //  --------------------------------------- Approve ----------------------------------------------
                    if (((OptionSetValue)target["statuscode"]).Value == 100000000)   // = approve
                    {
                        if (en_voidPM.Contains("bsd_payment"))
                        {
                            strMess.AppendLine("payment.voidPaymentAproval(en_voidPM)");
                            payment.voidPaymentAproval(en_voidPM);
                        }
                        //  ------------------- update void pm for applydocument -------------------------------------
                        else if (en_voidPM.Contains("bsd_applydocument"))
                        {
                            //  ------------------------------- retrieve apply document -------------------------------------
                            Entity en_app = service.Retrieve(((EntityReference)en_voidPM["bsd_applydocument"]).LogicalName, ((EntityReference)en_voidPM["bsd_applydocument"]).Id, new ColumnSet(true));
                            applyDocument.voidApplyDocument(en_voidPM, en_app);
                        } // end region of en_voidPM contain apply document
                          // ------------------ end update voidpm for apply ---------------------------
                    }
                    // ------------ end approve ---------------------
                    //  -------------------------------Reject ------------------------------------------
                    if (((OptionSetValue)target["statuscode"]).Value == 100000001)   // = reject
                    {
                        Entity en_tmp = new Entity(en_voidPM.Contains("bsd_payment") ? ((EntityReference)en_voidPM["bsd_payment"]).LogicalName : ((EntityReference)en_voidPM["bsd_applydocument"]).LogicalName);
                        en_tmp.Id = en_voidPM.Contains("bsd_payment") ? ((EntityReference)en_voidPM["bsd_payment"]).Id : ((EntityReference)en_voidPM["bsd_applydocument"]).Id;
                        en_tmp["statuscode"] = new OptionSetValue(100000000);
                        service.Update(en_tmp);
                        if (en_tmp.LogicalName == "bsd_payment")
                        {
                            var fetchXml = $@"<?xml version=""1.0"" encoding=""utf-16""?>
                            <fetch>
                              <entity name=""bsd_advancepayment"">
                                <attribute name=""bsd_advancepaymentid"" />
                                <filter>
                                  <condition attribute=""statecode"" operator=""eq"" value=""{0}"" />
                                  <condition attribute=""bsd_payment"" operator=""eq"" value=""{en_tmp.Id}"" />
                                </filter>
                              </entity>
                            </fetch>";
                            EntityCollection entc = service.RetrieveMultiple(new FetchExpression(fetchXml));
                            foreach (Entity entity in entc.Entities)
                            {
                                entity["statuscode"] = new OptionSetValue(100000000);//Collected
                                service.Update(entity);
                            }
                        }
                    }
                    // --------------- end reject --------------------------------------------
                } // END OF CONTEXT = UPDATE
                // ------- create New VoidPM -------------
                // create new record - update statuscode of payment to pending revert
                if (context.MessageName == "Create")
                {
                    // khi create cho payment thi change status cua payment thanh pending revert - tuong tu cho apply document
                    // ------ void for payment  -----------------
                    if (en_voidPM.Contains("bsd_payment"))
                    {
                        strMess.AppendLine("a");
                        Entity en_PM = service.Retrieve(((EntityReference)en_voidPM["bsd_payment"]).LogicalName, ((EntityReference)en_voidPM["bsd_payment"]).Id,
                            new ColumnSet(true));
                        Entity en_Unit = service.Retrieve(((EntityReference)en_PM["bsd_units"]).LogicalName, ((EntityReference)en_PM["bsd_units"]).Id,
                            new ColumnSet(true));
                        strMess.AppendLine("b");
                        //  ----------- check status of unit when revert payment --------------------
                        if (en_PM.Contains("bsd_reservation"))
                        {
                            strMess.AppendLine("c");
                            Entity en_quote = service.Retrieve(((EntityReference)en_PM["bsd_reservation"]).LogicalName,
                                              ((EntityReference)en_PM["bsd_reservation"]).Id, new ColumnSet(true));
                            strMess.AppendLine("iii");
                            int i_ResvStatus = ((OptionSetValue)en_quote["statuscode"]).Value;
                            strMess.AppendLine("d");
                            switch (i_ResvStatus)
                            {
                                case 4:
                                    throw new InvalidPluginExecutionException("Reservation " + (string)en_quote["name"] + " has already attached with an Option Entry! Can not create void payment!");
                                case 6:
                                    throw new InvalidPluginExecutionException("Reservation " + (string)en_quote["name"] + " has been Canceled. Can not create void payment!");
                                case 100000001:
                                    throw new InvalidPluginExecutionException("Reservation " + (string)en_quote["name"] + " has been Terminated. Can not create void payment!");
                                case 100000003:
                                    throw new InvalidPluginExecutionException("Reservation " + (string)en_quote["name"] + " has been Reject. Can not create void payment!");
                                case 100000002:
                                    throw new InvalidPluginExecutionException("Reservation " + (string)en_quote["name"] + " has been Pending Cancel Deposit. Can not create void payment!");
                            }
                            strMess.AppendLine("e");
                        }
                        strMess.AppendLine("h");
                        Entity en_tmp = new Entity(en_PM.LogicalName);
                        en_tmp.Id = en_PM.Id;
                        en_tmp["statuscode"] = new OptionSetValue(100000001); // pending revert
                        service.Update(en_tmp);

                        // find advance payment connect with current payment of voidPM
                        advancePayment.pendingRevertAdvancePayment(en_PM);
                    }
                    // -----------end create void pm ----------------
                    // ------ apply payment -----------
                    else if (en_voidPM.Contains("bsd_applydocument"))
                    {
                        Entity en_app = service.Retrieve(((EntityReference)en_voidPM["bsd_applydocument"]).LogicalName, ((EntityReference)en_voidPM["bsd_applydocument"]).Id,
                         new ColumnSet(new string[] { "statuscode", "bsd_name",
                            "bsd_units",
                            "bsd_optionentry",
                            "bsd_reservation",
                            "bsd_actualamountspent"
                         }));

                        if (en_app.Contains("bsd_reservation"))
                        {
                            Entity en_quote = service.Retrieve(((EntityReference)en_app["bsd_reservation"]).LogicalName, ((EntityReference)en_app["bsd_reservation"]).Id, new ColumnSet(new string[] { "name", "statuscode" }));
                            if (en_quote.Contains("statuscode") && ((OptionSetValue)en_quote["statuscode"]).Value == 4) // won ( da convert sang OE)
                                throw new InvalidPluginExecutionException("Quotation Reservation " + (string)en_quote["name"] + " has been converted to Option Entry. Cannot create Void Payment!");
                        }
                        Entity en_tmp = new Entity(en_app.LogicalName);
                        en_tmp.Id = en_app.Id;
                        en_tmp["statuscode"] = new OptionSetValue(100000003); // pending revert
                        service.Update(en_tmp);
                        // update for app dtl
                        EntityCollection ec_appDtl = applyDocument.Get_appDtl(en_app.Id);
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
                        decimal bsd_amount = en_app.Contains("bsd_actualamountspent") ? ((Money)en_app["bsd_actualamountspent"]).Value : 0;
                        en_targetUp["bsd_amount"] = new Money(bsd_amount);
                        en_targetUp["bsd_totalapplyamount"] = new Money(bsd_amount);
                        service.Update(en_targetUp);
                    }
                    //  ----------- create void apply ---------------
                }
            }
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