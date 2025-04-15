using Microsoft.Xrm.Sdk;
using System;
using Microsoft.Xrm.Sdk.Query;
using System.Text;

namespace Action_ApplyDocument
{
    public class Action_ApplyDocument : IPlugin
    {
        IOrganizationService service = null;
        IOrganizationServiceFactory factory = null;
        ApplyDocument applyDocument;
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
                if (i_bsd_transactiontype == 1)//Deposit
                {

                }
                else if (i_bsd_transactiontype == 2)//Installments
                {

                }
                else if (i_bsd_transactiontype == 3)//Interest
                {

                }
                else if (i_bsd_transactiontype == 4)//Fees
                {

                }
                else if (i_bsd_transactiontype == 5)//Miscellaneous
                {

                }
                if (i_bsd_transactiontype == 2)
                {
                    check_DueDate_Installment(service, en_app); //Task jira CLV-1446
                }
                applyDocument.checkInput(en_app);

                DateTime d_now = applyDocument.RetrieveLocalTimeFromUTCTime(DateTime.Now);
                string s_bsd_arraypsdid = en_app.Contains("bsd_arraypsdid") ? (string)en_app["bsd_arraypsdid"] : "";
                string s_bsd_arrayamountpay = en_app.Contains("bsd_arrayamountpay") ? (string)en_app["bsd_arrayamountpay"] : "";
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
                //  --------------------- ! deposit -------------------------------
                if (i_bsd_transactiontype == 2) // installment
                {
                    applyDocument.paymentInstallment(en_app);
                }

                //---- end of INS ------

                // Create Applydocument Remaining COA By Thạnh Đỗ
                applyDocument.createCOA(en_app);
                //Tạo Applydocument Remaining COA
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