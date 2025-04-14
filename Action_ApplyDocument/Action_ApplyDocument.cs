// 170217 - Apply document
// khi chon 1 row trong list - luu du lieu vao cac field nay - khi click buttom
// tong hop cac advance payment cua customer
// customer dung amount trong cac advance payment cua minh de thanh toan cho cac installment , deposit, interestcharge khac
// cac du lieu advance payment & amount - installment, interestcharge chua trong cac field array
// dua vao so tien con lai trong field amount cua advance payment ma tinh toan de tra cho cac list installment or interest charege or deposit

// kiem tra so tien transfer money co lon hon remaining amount cua advance payment hay k - neu lon hon thi thong bao
// kiem tra so tien bsd_amountadvancepayment (so tien trong cac advance payment co san)so voi so tien  bsd_totalapplyamount ( so tien user chon de thanh toan cac installment
// hoac deposit , interest charge) neu so tien chon nho hon so tien can thanh toan thi thong bao la k thanh toan duoc
// neu thoa dieu kien thanh toan - dua vao type of payment
// truy van ve quote hoac OE ma thanh toan cho deposit hoac installment & interestcharge
// cap nhat cac so lieu status reason cua deposit, quote, installment , OE, unit , interest charge
// ! luu y khi thanh toan du cho installmment moi thanh toan cho interesst charge duoc.

// type = deposit hay installment thi array : bsd_arraypsdid se chua day ID cua Quote hoac installment cua user da chon
// bsd_arrayamountpay chua du lieu so tien can thanh toan cua deposit hoac installment
// 170308 - Han require k can kiem tra neu installment truoc do chua paid thi k duoc thanh toan cho installment tiep theo
//  170316  them fan kiem tra du lieu waiver amount of installment vao truoc khi tinh toan

/// 170520 aaaaaaaaaaaaaaaaaaaaaaaa
// neu unit k chua field OP Date thi

/// 170608
// them bsd_paiddate - chua du lieu - khi payment cho INS - chuyen trang thai Paid thi update payment date vao field nay

/// 170807 - add them fan Miscellaneous - khi load subgrid nay thi check status cua MIS - check dieu kien tra tien nhu cac truong hop khac
using Microsoft.Xrm.Sdk;
using System;
using Microsoft.Xrm.Sdk.Query;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Crm.Sdk.Messages;
using System.Web.Script.Serialization;

namespace Action_ApplyDocument
{
    public class Action_ApplyDocument : IPlugin
    {
        IOrganizationService service = null;
        IOrganizationServiceFactory factory = null;
        ApplyDocument applyDocument;
        StringBuilder strMess = new StringBuilder();
        void IPlugin.Execute(IServiceProvider serviceProvider)
        {

            IPluginExecutionContext context = (IPluginExecutionContext)serviceProvider.GetService(typeof(IPluginExecutionContext));
            EntityReference target = (EntityReference)context.InputParameters["Target"];

            factory = (IOrganizationServiceFactory)serviceProvider.GetService(typeof(IOrganizationServiceFactory));
            service = factory.CreateOrganizationService(context.UserId);
            applyDocument = new ApplyDocument(serviceProvider);
            if (target.LogicalName == "bsd_applydocument")
            {
                //  ------------------------------- retrieve apply document -------------------------------------
                Entity en_app = service.Retrieve(target.LogicalName, target.Id, new ColumnSet(true));
                int i_bsd_transactiontype = en_app.Contains("bsd_transactiontype") ? ((OptionSetValue)en_app["bsd_transactiontype"]).Value : 0;
                strMess.AppendLine("chuẩn bị check_DueDate_Installment");
                if (i_bsd_transactiontype == 2)
                {
                    check_DueDate_Installment(service, en_app); //Task jira CLV-1446
                }
                applyDocument.checkInput(en_app);

                DateTime d_now = applyDocument.RetrieveLocalTimeFromUTCTime(DateTime.Now);
                strMess.AppendLine(d_now.ToString());

                strMess.AppendLine("Apply bsd_transactiontype: " + i_bsd_transactiontype);
                string s_bsd_arraypsdid = en_app.Contains("bsd_arraypsdid") ? (string)en_app["bsd_arraypsdid"] : "";
                string s_bsd_arrayamountpay = en_app.Contains("bsd_arrayamountpay") ? (string)en_app["bsd_arrayamountpay"] : "";
                strMess.AppendLine("99999999999");
                // --------------------- transaction type = 1 - deposit ------------------
                if (i_bsd_transactiontype == 1)
                {
                    string[] s_psd = s_bsd_arraypsdid.Split(',');
                    string[] s_Amp = s_bsd_arrayamountpay.Split(',');
                    int i_psd = s_psd.Length;
                    // list of s_bsd_arraypsdid chua cac id cua Quote
                    // chay vong lap

                    // update deposit ( Quote)
                    for (int m = 0; m < i_psd; m++)
                    {
                        applyDocument.paymentDeposit(Guid.Parse(s_psd[m]), decimal.Parse(s_Amp[m]), d_now, en_app);

                    }

                } // end of transaction type = deposit

                strMess.AppendLine("i_bsd_transactiontype = installment");
                //  --------------------- ! deposit -------------------------------
                if (i_bsd_transactiontype == 2) // installment
                {
                    applyDocument.paymentInstallment(en_app);
                }

                //---- end of INS ------

                // Create Applydocument Remaining COA By Thạnh Đỗ
                strMess.AppendLine("createCOA");
                applyDocument.createCOA(en_app);
                strMess.AppendLine("createCOA done");
                //Tạo Applydocument Remaining COA

                // update advance payment
                // su dung tong so tien can fai tra - so sanh voi so tien cua tung advance payment mang ra tra
                // neu so tien can tra lon hon advAM thi sotienconlai = amp - advAM
                // lay tien conlai so voi so tien cua adv tiep theo... den khi tienconlai =0
                strMess.AppendLine("1000000000000");
                applyDocument.updateApplyDocument(en_app);
                if (i_bsd_transactiontype == 2)
                {
                    OrganizationRequest req = new OrganizationRequest("bsd_Action_Create_Invoice");
                    //parameter:
                    req["EnCallFrom"] = en_app;
                    //execute the request
                    OrganizationResponse response = service.Execute(req);
                }
            }
        }
        public void check_DueDate_Installment(IOrganizationService service, Entity applydocument)
        {
            try
            {
                var value = applydocument.Contains("bsd_arraypsdid");
                // Case . Click chọn Installment, chưa save trên js sẽ không thấy arrayID dưới db
                if (value)
                {
                    var str_array = applydocument["bsd_arraypsdid"].ToString();
                    if (str_array != "")
                    {
                        string[] arr = str_array.Split(',');
                        #region --- Kiểm tra duedate in Installment : Task jira CLV-1446 -
                        for (int i = 0; i < arr.Length; i++)
                        {
                            var item = arr[i];
                            var guid = new Guid(item);
                            var EnIns = service.Retrieve("bsd_paymentschemedetail", guid, new ColumnSet(new string[] { "bsd_name", "bsd_duedate" }));
                            if (!EnIns.Contains("bsd_duedate"))
                            {
                                throw new Exception("The Installment you are paying has no due date. Please update due date before confirming payment! " + EnIns["bsd_name"].ToString());
                            }
                        }
                        #endregion
                    }
                }
            }
            catch (Exception ex)
            {
                throw new InvalidPluginExecutionException(string.Format("{0}", ex.Message));
            }
        }
    }
}