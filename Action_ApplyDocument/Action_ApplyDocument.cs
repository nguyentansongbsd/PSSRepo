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
            //try
            //{
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
                    //processPayment(en_app);
                }
                //update status apply document

            } // end if (target.LogicalName == "bsd_applydocument")
              //}
              //catch(Exception ex)
              //{
              //    strMess.AppendLine(ex.ToString());
              //    throw new InvalidPluginExecutionException(strMess.ToString());
              //}

            //throw new InvalidPluginExecutionException("ApplyDocument Done ssssssssssss");
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
        public void processPayment(Entity ApplyDoc)
        {
            bool bolDot1 = false;
            string strResult1 = "";
            string strResult2 = "";
            string strResult3 = "";
            var serializer = new JavaScriptSerializer();
            strMess.AppendLine("vào invoice và type = ");

            strMess.AppendLine("vào check");

            string s_bsd_arraypsdid = ApplyDoc.Contains("bsd_arraypsdid") ? (string)ApplyDoc["bsd_arraypsdid"] : "";
            string s_bsd_arrayamountpay = ApplyDoc.Contains("bsd_arrayamountpay") ? (string)ApplyDoc["bsd_arrayamountpay"] : "";
            EntityReference ins_ivcoice = ApplyDoc.Contains("bsd_paymentschemedetail") ? (EntityReference)ApplyDoc["bsd_paymentschemedetail"] : null;
            #region--thành
            string IV_arrayfees = ApplyDoc.Contains("bsd_arrayfeesid") ? (string)ApplyDoc["bsd_arrayfeesid"] : "";
            string IV_arrayfeesamount = ApplyDoc.Contains("bsd_arrayfeesamount") ? (string)ApplyDoc["bsd_arrayfeesamount"] : "";

            Guid InsId1st = new Guid();


            var fetchXmltaxcode = $@"
                <fetch>
                  <entity name='bsd_taxcode'>
                    <attribute name='bsd_name' />
                    <attribute name='bsd_value' />
                    <filter type='or'>
                      <condition attribute='bsd_name' operator='eq' value='{10}'/>
                      <condition attribute='bsd_name' operator='eq' value='{-1}'/>
                    </filter>
                    <order attribute='createdon' descending='true' />
                  </entity>
                </fetch>";
            var EnColtaxcode = service.RetrieveMultiple(new FetchExpression(fetchXmltaxcode.ToString()));
            Entity EnTaxcode_1 = null;
            Entity EnTaxcode10 = null;
            foreach (var item in EnColtaxcode.Entities)
            {
                if (item.Contains("bsd_name") && item["bsd_name"].ToString() == "-1" && EnTaxcode_1 == null)
                {
                    EnTaxcode_1 = item;
                    continue;
                }
                if (item.Contains("bsd_name") && item["bsd_name"].ToString() == "10" && EnTaxcode10 == null)
                {
                    EnTaxcode10 = item;
                    continue;
                }
            }



            if (s_bsd_arraypsdid != "")
            {
                Boolean flag = false;
                Boolean flag1 = false;
                string[] ivoid_idINS = { };
                string[] ivoid_amINS = { };
                decimal d_amp = 0;
                decimal decAmountInsHandover = 0;
                decimal decAmount1st = 0;
                decimal depositamount = 0;
                Guid InsHandoverId = new Guid();
                strMess.AppendLine("s_bsd_arraypsdid " + s_bsd_arraypsdid);
                strMess.AppendLine("s_bsd_arrayamountpay " + s_bsd_arrayamountpay);
                if (s_bsd_arraypsdid != "")
                {
                    ivoid_idINS = s_bsd_arraypsdid.Split(',');
                    ivoid_amINS = s_bsd_arrayamountpay.Split(',');
                }
                strMess.AppendLine("invoice1");
                EntityCollection ec_ins = get_ecINS(service, ivoid_idINS);
                if (ec_ins.Entities.Count <= 0) throw new InvalidPluginExecutionException("Cannot find any Installment from Installment list. Please check again!");

                for (int m = 0; m < ivoid_amINS.Length; m++)
                {
                    d_amp += decimal.Parse(ivoid_amINS[m]);
                }
                strMess.AppendLine("d_amp" + d_amp);
                string chuoi_name = "";
                decimal amount_lastins = 0;
                //EntityReference ins = (EntityReference)enPayment["bsd_paymentschemedetail"];
                EntityReference units = (EntityReference)ApplyDoc["bsd_units"];
                //Entity ec_ins_pay = service.Retrieve(ins.LogicalName, ins.Id, new ColumnSet(true));
                strMess.AppendLine("invoice2");
                DateTime actualtime_iv = ApplyDoc.Contains("bsd_receiptdate") ? (DateTime)ApplyDoc["bsd_receiptdate"] : new DateTime();
                //int so_pay = ec_ins_pay.Contains("bsd_ordernumber") ? (int)ec_ins_pay["bsd_ordernumber"] : 0;
                #region code cũ
                strMess.AppendLine("invoice3");
                Entity invoice = new Entity("bsd_invoice");
                invoice["bsd_type"] = new OptionSetValue(100000000);
                invoice["bsd_lastinstallmentamount"] = new Money(0);
                invoice["bsd_lastinstallmentvatamount"] = new Money(0);
                invoice["bsd_depositamount"] = new Money(0);
                invoice.Id = new Guid();
                foreach (Entity paymentsheme in ec_ins.Entities)
                {
                    var enIns = service.Retrieve("bsd_paymentschemedetail", paymentsheme.Id, new ColumnSet(true));
                    bolDot1 = enIns.Contains("bsd_ordernumber") && enIns["bsd_ordernumber"].ToString() == "1" ? true : false;
                    //Check thanh toán chưa hoàn tất 1st Installment (Thanh toán thiếu tiền đợt 01)
                    if (enIns.Contains("statuscode") && ((OptionSetValue)enIns["statuscode"]).Value == 100000000 && bolDot1)
                    {
                        continue;
                    }
                    //Thanh toán với Installment là đợt cuối
                    if (enIns.Contains("bsd_lastinstallment") && (bool)enIns["bsd_lastinstallment"] == true)
                    {
                        continue;
                    }
                    if (bolDot1)//nếu đợt 1, thì tiền= tiền đợt 1 + depositamount
                    {
                        InsId1st = enIns.Id;
                        depositamount = enIns.Contains("bsd_depositamount") ? ((Money)enIns["bsd_depositamount"]).Value : 0;
                        decAmount1st = ((Money)enIns["bsd_amountwaspaid"]).Value + depositamount;
                        d_amp = d_amp - ((Money)enIns["bsd_amountwaspaid"]).Value;
                        continue;
                    }

                    int last = paymentsheme.Contains("bsd_duedatecalculatingmethod") ? ((OptionSetValue)paymentsheme["bsd_duedatecalculatingmethod"]).Value : 0;
                    if (last == 100000002)
                    {
                        d_amp = d_amp - ((Money)enIns["bsd_amountwaspaid"]).Value;
                        InsHandoverId = paymentsheme.Id;
                        decAmountInsHandover = ((Money)paymentsheme["bsd_amountofthisphase"]).Value;
                    }
                    int so = paymentsheme.Contains("bsd_ordernumber") ? (int)paymentsheme["bsd_ordernumber"] : 0;
                    if (flag1 == false)
                    {
                        if (last != 100000002)
                            chuoi_name += so.ToString();
                    }
                    else
                    {
                        if (last != 100000002)
                            chuoi_name += " + " + so.ToString();
                    }
                    flag1 = true;

                }
                //if(chuoi_name!="")
                //{
                //decimal amountpay = enPayment.Contains("bsd_amountpay") ? ((Money)enPayment["bsd_amountpay"]).Value : 0;
                decimal amountpay = d_amp;
                Entity iv_units = service.Retrieve(units.LogicalName, units.Id, new ColumnSet(new string[] { "name" }));
                invoice["bsd_name"] = "Thu tiền đợt " + chuoi_name + " của căn hộ " + (string)iv_units["name"];
                invoice["bsd_project"] = ApplyDoc.Contains("bsd_project") ? ApplyDoc["bsd_project"] : null;
                invoice["bsd_optionentry"] = ApplyDoc.Contains("bsd_optionentry") ? ApplyDoc["bsd_optionentry"] : 0;
                invoice["bsd_applydocument"] = ApplyDoc.ToEntityReference();

                invoice["bsd_issueddate"] = actualtime_iv;
                invoice["bsd_units"] = units;
                strMess.AppendLine("invoice4");
                EntityReference Pay_Perchaser = ApplyDoc.Contains("bsd_customer") ? (EntityReference)ApplyDoc["bsd_customer"] : null;
                invoice["bsd_purchaser"] = Pay_Perchaser;
                if (Pay_Perchaser != null)
                {
                    Entity iv_Perchaser = service.Retrieve(Pay_Perchaser.LogicalName, Pay_Perchaser.Id, new ColumnSet(true));
                    if (iv_Perchaser.LogicalName == "contact")
                    {
                        invoice["bsd_purchasernamecustomer"] = Pay_Perchaser;
                    }
                    else
                    {
                        invoice["bsd_purchasernamecompany"] = Pay_Perchaser;
                    }
                }
                if (actualtime_iv != new DateTime())
                {
                    DateTime bsd_paymentactualtime = RetrieveLocalTimeFromUTCTime(actualtime_iv);
                    invoice["bsd_issueddate"] = bsd_paymentactualtime;
                }
                invoice["bsd_paymentmethod"] = new OptionSetValue(100000000);
                Entity optionentry_invoive = service.Retrieve("salesorder", ((EntityReference)ApplyDoc["bsd_optionentry"]).Id,
                      new ColumnSet(new string[] {
                            "bsd_landvaluededuction",
                      }));
                Entity project_invoive = service.Retrieve("bsd_project", ((EntityReference)ApplyDoc["bsd_project"]).Id,
                    new ColumnSet(new string[] {
                            "bsd_formno","bsd_serialno"
                    }));
                strMess.AppendLine("invoice5");
                string formno = project_invoive.Contains("bsd_formno") ? (string)project_invoive["bsd_formno"] : "";
                string serialno = project_invoive.Contains("bsd_serialno") ? (string)project_invoive["bsd_serialno"] : "";
                invoice["bsd_formno"] = formno;
                invoice["bsd_serialno"] = serialno;
                decimal vat_amout = Math.Round(d_amp / 11, MidpointRounding.AwayFromZero);
                strMess.AppendLine("d_amp" + d_amp);
                strMess.AppendLine("vat_amout" + vat_amout);
                decimal vat_adj = 0;
                decimal vat_amount_ho = vat_amout - vat_adj;
                decimal invoiamountb4 = d_amp - vat_amount_ho;
                decimal land_value = ((Money)optionentry_invoive["bsd_landvaluededuction"]).Value;
                decimal vat_Handover_Installment = 0;

                if (flag == true)
                {
                    vat_Handover_Installment = Math.Round(land_value / 11, MidpointRounding.AwayFromZero);
                }
                strMess.AppendLine("invoice6");
                invoice["bsd_invoiceamount"] = new Money(d_amp);

                invoice["bsd_vatamount"] = new Money(vat_amout);
                invoice["bsd_vatadjamount"] = new Money(vat_Handover_Installment);
                invoice["bsd_vatamounthandover"] = new Money(vat_amout - vat_Handover_Installment);
                invoice["bsd_invoiceamountb4vat"] = new Money(invoiamountb4 - vat_amout - vat_Handover_Installment);

                invoice["statuscode"] = new OptionSetValue(100000000);//Confirm
                invoice["bsd_taxcode"] = EnTaxcode10.ToEntityReference();
                invoice["bsd_taxcodevalue"] = (decimal)EnTaxcode10["bsd_value"];
                if (chuoi_name != "")
                    service.Create(invoice);
                #endregion
                #region Invoice cho 1st Ins
                strMess.AppendLine("bolDot1 " + bolDot1);
                if (InsId1st != new Guid())
                {
                    Entity Invoice1st = new Entity();
                    Invoice1st = invoice;
                    Invoice1st["bsd_name"] = "Thu tiền đợt 1 của căn hộ " + (string)iv_units["name"];
                    Invoice1st["bsd_depositamount"] = new Money(depositamount);
                    Invoice1st["bsd_type"] = new OptionSetValue(100000003);
                    decimal vat_amout1st = Math.Round(decAmount1st / 11, MidpointRounding.AwayFromZero);
                    strMess.AppendLine("code tạo invoice đợt 1");
                    decimal vat_adj1st = 0;
                    decimal vat_amount_ho1st = vat_amout1st - vat_adj1st;
                    decimal invoiamountb41st = decAmount1st - vat_amount_ho1st;
                    decimal land_value1st = ((Money)optionentry_invoive["bsd_landvaluededuction"]).Value;
                    decimal vat_Handover_Installment1st = 0;
                    Invoice1st["bsd_invoiceamount"] = new Money(decAmount1st);
                    Invoice1st["bsd_vatamount"] = new Money(vat_amout1st);
                    Invoice1st["bsd_vatadjamount"] = new Money(vat_Handover_Installment1st);
                    Invoice1st["bsd_vatamounthandover"] = new Money(vat_amout1st - vat_Handover_Installment1st);
                    Invoice1st["bsd_invoiceamountb4vat"] = new Money(invoiamountb4 - vat_amout1st - vat_Handover_Installment1st);
                    Invoice1st["bsd_taxcode"] = EnTaxcode10.ToEntityReference();
                    Invoice1st["bsd_taxcodevalue"] = (decimal)EnTaxcode10["bsd_value"];
                    service.Create(Invoice1st);
                }
                #endregion
                #region Invoice cho Handover
                if (InsHandoverId != new Guid() && decAmountInsHandover != 0)//trường hợp có cấn trừ ins handover
                {


                    #region tạo invoice cho ins handover
                    Entity optionentryEn = ApplyDoc.Contains("bsd_optionentry") ? service.Retrieve("salesorder", ((EntityReference)ApplyDoc["bsd_optionentry"]).Id, new ColumnSet(true)) : null;
                    decimal totaltax = 0;//Total VAT Tax trong Option Entry
                    decimal totalamountwaspaidins = 0;
                    if (optionentryEn != null)
                    {
                        totaltax = optionentryEn.Contains("totaltax") ? ((Money)optionentryEn["totaltax"]).Value : 0;
                        var fetchXml = $@"
                            <fetch aggregate='true'>
                              <entity name='bsd_paymentschemedetail'>
                                <attribute name='bsd_amountwaspaid' alias='totalamountwaspaidins' aggregate='sum' />
                                <filter type='and'>
                                  <condition attribute='bsd_optionentry' operator='eq' value='{optionentryEn.Id}'/>
                                  <condition attribute='bsd_lastinstallment' operator='neq' value='{1}'/>
                                  <condition attribute='statecode' operator='eq' value='{0}'/>
                                  <condition attribute='bsd_duedatecalculatingmethod' operator='neq' value='{100000002}'/>
                                </filter>
                              </entity>
                            </fetch>";
                        var enResult = service.RetrieveMultiple(new FetchExpression(fetchXml));
                        if (enResult != null && enResult.Entities.Count > 0)
                        {
                            totalamountwaspaidins = ((Money)((AliasedValue)enResult[0]["totalamountwaspaidins"]).Value).Value;
                        }
                    }
                    Entity Get_Last_Ins = getLastInstallment(service, optionentry_invoive);
                    amount_lastins = Get_Last_Ins.Contains("bsd_amountofthisphase") ? ((Money)Get_Last_Ins["bsd_amountofthisphase"]).Value : 0; ;//Amount of this phase của Last Installment
                    decimal vatamount_lastins = Math.Round(amount_lastins / 11, MidpointRounding.AwayFromZero);//rút gọn của  amount_lastins/ 1.1 * 10%   //VAT Last Installment
                    Entity ec_insHandover = service.Retrieve("bsd_paymentschemedetail", InsHandoverId, new ColumnSet(true));
                    decimal amount_waspaid = ec_insHandover.Contains("bsd_amountwaspaid") ? ((Money)ec_insHandover["bsd_amountwaspaid"]).Value : 0;//Amount was paid" của Đợt bàn giao 
                    decimal vatHandoverPayment = Math.Round(amount_waspaid / 11, MidpointRounding.AwayFromZero);//VAT Handover Payment
                    //if (chuoi_name == "")
                    //{
                    //    d_amp = decAmountInsHandover;
                    //}
                    int index = Array.IndexOf(ivoid_idINS, InsHandoverId.ToString());
                    d_amp = decimal.Parse(ivoid_amINS[index]);

                    vat_amout = Math.Round(d_amp / 11, MidpointRounding.AwayFromZero);
                    //strMess.AppendLine(s_bsd_arraypsdid);
                    //strMess.AppendLine(InsHandoverId.ToString());
                    strMess.AppendLine("Index của handover: " + index);
                    int so = ec_insHandover.Contains("bsd_ordernumber") ? (int)ec_insHandover["bsd_ordernumber"] : 0;
                    chuoi_name = so.ToString();
                    bool bolThanhToanDu = ec_insHandover.Contains("statuscode") && ((OptionSetValue)ec_insHandover["statuscode"]).Value == 100000001 ? true : false;
                    strMess.AppendLine("totaltax " + totaltax);
                    strMess.AppendLine("vat_amout " + vat_amout);
                    strMess.AppendLine("vatamount_lastins " + vatamount_lastins);
                    strMess.AppendLine("vatHandoverPayment " + vatHandoverPayment);
                    if (totaltax >= Math.Round(totalamountwaspaidins / 11, MidpointRounding.AwayFromZero) + vatamount_lastins + vatHandoverPayment)
                    {
                        //vat_amout = Math.Round(totalamountwaspaidins / 11, MidpointRounding.AwayFromZero);
                        //vat_adj = 0;
                        //vat_amount_ho = vat_amout - vat_adj;
                        //invoiamountb4 = d_amp - vat_amount_ho;
                        strMess.AppendLine("case c >= 44444");
                        if (bolThanhToanDu)
                        {
                            strMess.AppendLine("case thanh toán đủ và dư");
                            vat_Handover_Installment = Math.Round(land_value / 11, MidpointRounding.AwayFromZero);
                            invoice["bsd_name"] = "Thu tiền đợt " + chuoi_name + " của căn hộ " + (string)iv_units["name"];
                            invoice["bsd_invoiceamount"] = new Money(amountpay - land_value);
                            invoice["bsd_type"] = new OptionSetValue(100000000);//Installment
                            invoice["statuscode"] = new OptionSetValue(100000000);//Confirm
                            invoice["bsd_vatamount"] = new Money(Math.Round((amountpay - land_value) / 11, MidpointRounding.AwayFromZero));
                            invoice["bsd_vatadjamount"] = new Money(0);
                            invoice["bsd_vatamounthandover"] = new Money(Math.Round((amountpay - land_value) / 11, MidpointRounding.AwayFromZero));
                            invoice["bsd_invoiceamountb4vat"] = new Money(Math.Round(amountpay - land_value - (amountpay - land_value) / 11, MidpointRounding.AwayFromZero));
                            invoice["bsd_taxcode"] = EnTaxcode10.ToEntityReference();
                            invoice["bsd_taxcodevalue"] = (decimal)EnTaxcode10["bsd_value"];
                            service.Create(invoice);
                            strResult3 = serializer.Serialize(invoice);

                            Entity invoiceLandValue = new Entity("bsd_invoice");
                            invoiceLandValue = invoice;
                            invoiceLandValue["bsd_name"] = "Giá trị quyền sử dụng đất (không chịu Thuế GTGT) của căn hộ " + (string)iv_units["name"];
                            invoiceLandValue["bsd_invoiceamount"] = new Money(land_value);
                            invoiceLandValue["bsd_type"] = new OptionSetValue(100000006);//Land Value
                            invoiceLandValue["statuscode"] = new OptionSetValue(100000000);//Confirm
                            invoiceLandValue["bsd_vatamount"] = new Money(0);
                            invoiceLandValue["bsd_vatadjamount"] = new Money(0);
                            invoiceLandValue["bsd_vatamounthandover"] = new Money(0);
                            invoiceLandValue["bsd_invoiceamountb4vat"] = new Money(land_value);
                            invoiceLandValue["bsd_taxcode"] = EnTaxcode_1.ToEntityReference();
                            invoiceLandValue["bsd_taxcodevalue"] = (decimal)EnTaxcode_1["bsd_value"];
                            service.Create(invoiceLandValue);

                            strResult1 = serializer.Serialize(invoiceLandValue);

                            Entity invoiceLast = new Entity("bsd_invoice");
                            invoiceLast = invoice;
                            invoiceLast["bsd_name"] = "5% giá trị của căn hộ " + (string)iv_units["name"] + " (Đợt cuối)";
                            invoiceLast["bsd_invoiceamount"] = new Money(amount_lastins);
                            invoiceLast["bsd_type"] = new OptionSetValue(100000004);//Last Installment
                            invoiceLast["statuscode"] = new OptionSetValue(100000000);//Confirm
                            invoiceLast["bsd_vatamount"] = new Money(vatamount_lastins);
                            invoiceLast["bsd_vatadjamount"] = new Money(0);
                            invoiceLast["bsd_vatamounthandover"] = new Money(vatamount_lastins);
                            invoiceLast["bsd_invoiceamountb4vat"] = new Money(amount_lastins - vatamount_lastins);
                            invoiceLast["bsd_taxcode"] = EnTaxcode10.ToEntityReference();
                            invoiceLast["bsd_taxcodevalue"] = (decimal)EnTaxcode10["bsd_value"];
                            service.Create(invoiceLast);

                            strResult2 = serializer.Serialize(invoiceLast);
                        }
                        else
                        {
                            strMess.AppendLine("case thanh toán thiếu");
                            vat_Handover_Installment = 0;
                            invoice["bsd_name"] = "Thu tiền đợt " + chuoi_name + " của căn hộ " + (string)iv_units["name"];
                            invoice["bsd_type"] = new OptionSetValue(100000000);//Installment
                            invoice["statuscode"] = new OptionSetValue(100000000);//yêu cầu mới, cứ tạo invoice thì sẽ confirm
                            invoice["bsd_invoiceamount"] = new Money(d_amp);
                            invoice["bsd_vatamount"] = new Money(vat_amout);
                            invoice["bsd_vatadjamount"] = new Money(vat_Handover_Installment);
                            invoice["bsd_vatamounthandover"] = new Money(vat_amout - vat_Handover_Installment);
                            invoice["bsd_invoiceamountb4vat"] = new Money(d_amp - vat_amout - vat_Handover_Installment);
                            invoice["bsd_taxcode"] = EnTaxcode10.ToEntityReference();
                            invoice["bsd_taxcodevalue"] = (decimal)EnTaxcode10["bsd_value"];
                            service.Create(invoice);
                            strResult3 = serializer.Serialize(invoice);
                        }
                    }
                    else
                    {
                        strMess.AppendLine("case c < 4444444444444444");
                        if (bolThanhToanDu)
                        {
                            strMess.AppendLine("case thanh toán đủ và dư");
                            invoice["bsd_name"] = "Giá trị quyền sử dụng đất (không chịu Thuế GTGT) của căn hộ " + (string)iv_units["name"];
                            invoice["bsd_type"] = new OptionSetValue(100000005);//type: Installment Land Value
                            invoice["statuscode"] = new OptionSetValue(100000000);//yêu cầu mới, cứ tạo invoice thì sẽ confirm
                            invoice["bsd_invoiceamount"] = new Money(d_amp);
                            invoice["bsd_vatamount"] = new Money(vat_amout);
                            invoice["bsd_vatadjamount"] = new Money(vat_Handover_Installment);
                            invoice["bsd_vatamounthandover"] = new Money(vat_amout - vat_Handover_Installment);
                            invoice["bsd_invoiceamountb4vat"] = new Money(d_amp - vat_amout - vat_Handover_Installment);
                            invoice["bsd_taxcode"] = EnTaxcode_1.ToEntityReference();
                            invoice["bsd_taxcodevalue"] = (decimal)EnTaxcode_1["bsd_value"];
                            service.Create(invoice);
                            strResult1 = serializer.Serialize(invoice);

                            Entity invoiceLast = new Entity("bsd_invoice");
                            invoiceLast = invoice;
                            invoiceLast["bsd_name"] = "5% giá trị của căn hộ " + (string)iv_units["name"] + " (Đợt cuối)";
                            invoiceLast["bsd_invoiceamount"] = new Money(amount_lastins);
                            invoiceLast["bsd_type"] = new OptionSetValue(100000004);//Last Installment
                            invoiceLast["statuscode"] = new OptionSetValue(100000000);//Confirm
                            invoiceLast["bsd_vatamount"] = new Money(vatamount_lastins);
                            invoiceLast["bsd_vatadjamount"] = new Money(0);
                            invoiceLast["bsd_vatamounthandover"] = new Money(vatamount_lastins);
                            invoiceLast["bsd_invoiceamountb4vat"] = new Money(amount_lastins - vatamount_lastins);
                            invoiceLast["bsd_taxcode"] = EnTaxcode10.ToEntityReference();
                            invoiceLast["bsd_taxcodevalue"] = (decimal)EnTaxcode10["bsd_value"];
                            service.Create(invoiceLast);
                            strResult2 = serializer.Serialize(invoiceLast);
                        }
                        else
                        {
                            strMess.AppendLine("case thanh toán thiếu");
                            vat_Handover_Installment = 0;
                            invoice["bsd_name"] = "Giá trị quyền sử dụng đất (không chịu Thuế GTGT) của căn hộ " + (string)iv_units["name"];
                            invoice["bsd_type"] = new OptionSetValue(100000005);//type: Installment Land Value
                            invoice["statuscode"] = new OptionSetValue(100000000);//yêu cầu mới, cứ tạo invoice thì sẽ confirm
                            invoice["bsd_invoiceamount"] = new Money(d_amp);
                            invoice["bsd_vatamount"] = new Money(vat_amout);
                            invoice["bsd_vatadjamount"] = new Money(vat_Handover_Installment);
                            invoice["bsd_vatamounthandover"] = new Money(vat_amout - vat_Handover_Installment);
                            invoice["bsd_invoiceamountb4vat"] = new Money(d_amp - vat_amout - vat_Handover_Installment);
                            invoice["bsd_taxcode"] = EnTaxcode_1.ToEntityReference();
                            invoice["bsd_taxcodevalue"] = (decimal)EnTaxcode_1["bsd_value"];
                            service.Create(invoice);
                            strResult3 = serializer.Serialize(invoice);
                        }
                    }
                    #endregion
                }
                #endregion
                //}

            }
            #region 
            if (IV_arrayfees != "")
            {
                string[] arrId_iv = IV_arrayfees.Split(',');
                string[] arrFeeAmount_iv = IV_arrayfeesamount.Split(',');
                strMess.AppendLine("IV_arrayfees");
                Boolean create = false;
                Entity invoice = new Entity("bsd_invoice");
                invoice.Id = new Guid();
                invoice["bsd_type"] = new OptionSetValue(100000001);
                invoice["bsd_lastinstallmentamount"] = new Money(0);
                invoice["bsd_lastinstallmentvatamount"] = new Money(0);
                invoice["bsd_depositamount"] = new Money(0);
                Boolean flag = false;
                decimal fee = 0;
                decimal amount_lastins = 0;
                string chuoi_name = "";
                EntityReference units = ApplyDoc.Contains("bsd_units") ? (EntityReference)ApplyDoc["bsd_units"] : null;
                DateTime actualtime_iv = ApplyDoc.Contains("bsd_receiptdate") ? (DateTime)ApplyDoc["bsd_receiptdate"] : new DateTime();
                bool flag2 = false;
                for (int i = 0; i < arrId_iv.Length; i++)
                {
                    string[] arr = arrId_iv[i].Split('_');
                    string installmentid = arr[0];
                    string type1 = arr[1];
                    strMess.AppendLine("t.1");
                    Entity enInstallment = service.Retrieve("bsd_paymentschemedetail", new Guid(installmentid), new ColumnSet(true));
                    bolDot1 = enInstallment.Contains("bsd_ordernumber") && enInstallment["bsd_ordernumber"].ToString() == "1" ? true : false;
                    //Check thanh toán chưa hoàn tất 1st Installment (Thanh toán thiếu tiền đợt 01)
                    if (enInstallment.Contains("bsd_maintenancefeesstatus") && (bool)enInstallment["bsd_maintenancefeesstatus"] && bolDot1)
                    {
                        strMess.AppendLine("t.2");
                        continue;
                    }
                    //Thanh toán với Installment là đợt cuối
                    if (enInstallment.Contains("bsd_lastinstallment") && (bool)enInstallment["bsd_lastinstallment"] == true)
                    {
                        strMess.AppendLine("t.3");
                        continue;
                    }
                    strMess.AppendLine("statuscode: " + ((bool)enInstallment["bsd_maintenancefeesstatus"]));
                    if (type1 == "main" && enInstallment.Contains("bsd_maintenancefeesstatus") && (bool)enInstallment["bsd_maintenancefeesstatus"])
                    {
                        strMess.AppendLine("t.4");
                        if (bolDot1)//nếu đợt 1, thì tiền= tiền đợt 1 + depositamount
                        {
                            decimal depositamount = enInstallment.Contains("bsd_depositamount") ? ((Money)enInstallment["bsd_depositamount"]).Value : 0;
                            fee = fee + depositamount;
                            invoice["bsd_depositamount"] = new Money(depositamount);
                            invoice["bsd_type"] = new OptionSetValue(100000003);
                        }
                        create = true;
                        fee += ((Money)enInstallment["bsd_maintenanceamount"]).Value;
                        int so = enInstallment.Contains("bsd_ordernumber") ? (int)enInstallment["bsd_ordernumber"] : 0;
                        int last_pay = enInstallment.Contains("bsd_duedatecalculatingmethod") ? ((OptionSetValue)enInstallment["bsd_duedatecalculatingmethod"]).Value : 0;
                        if (last_pay == 100000002)
                        {
                            flag = true;
                            amount_lastins = enInstallment.Contains("bsd_amountofthisphase") ? ((Money)enInstallment["bsd_amountofthisphase"]).Value : 0;
                        }

                        if (flag2 == true)
                        {
                            chuoi_name += " + " + so.ToString();
                        }
                        else
                        {
                            chuoi_name += so.ToString();
                        }
                        flag2 = true;
                    }
                }
                strMess.AppendLine("chuoi_name " + chuoi_name);
                if (chuoi_name != "")
                {
                    if (units != null)
                    {
                        Entity iv_units = service.Retrieve(units.LogicalName, units.Id, new ColumnSet(new string[] { "name" }));
                        invoice["bsd_name"] = "Thu tiền Phí bảo trì của căn hộ đợt " + chuoi_name + " của căn hộ " + (string)iv_units["name"]; ;
                    }
                    else
                    {
                        invoice["bsd_name"] = "Thu tiền Phí bảo trì của căn hộ đợt " + chuoi_name;
                    }
                    invoice["bsd_project"] = ApplyDoc.Contains("bsd_project") ? ApplyDoc["bsd_project"] : null;
                    invoice["bsd_optionentry"] = ApplyDoc.Contains("bsd_optionentry") ? ApplyDoc["bsd_optionentry"] : null;
                    invoice["bsd_applydocument"] = ApplyDoc.ToEntityReference();

                    EntityReference Pay_Perchaser = ApplyDoc.Contains("bsd_customer") ? (EntityReference)ApplyDoc["bsd_customer"] : null;
                    invoice["bsd_purchaser"] = Pay_Perchaser;
                    if (Pay_Perchaser != null)
                    {
                        Entity iv_Perchaser = service.Retrieve(Pay_Perchaser.LogicalName, Pay_Perchaser.Id, new ColumnSet(true));
                        if (iv_Perchaser.LogicalName == "contact")
                        {
                            invoice["bsd_purchasernamecustomer"] = Pay_Perchaser;
                        }
                        else
                        {
                            invoice["bsd_purchasernamecompany"] = Pay_Perchaser;
                        }
                    }
                    if (actualtime_iv != new DateTime())
                    {
                        DateTime bsd_paymentactualtime = RetrieveLocalTimeFromUTCTime(actualtime_iv);
                        invoice["bsd_issueddate"] = bsd_paymentactualtime;
                    }
                    invoice["bsd_paymentmethod"] = new OptionSetValue(100000000);
                    Entity optionentry_invoive = service.Retrieve("salesorder", ((EntityReference)ApplyDoc["bsd_optionentry"]).Id,
                          new ColumnSet(new string[] {
                            "bsd_landvaluededuction",
                          }));
                    Entity project_invoive = service.Retrieve("bsd_project", ((EntityReference)ApplyDoc["bsd_project"]).Id,
               new ColumnSet(new string[] {
                            "bsd_formno","bsd_serialno"
               }));

                    string formno = project_invoive.Contains("bsd_formno") ? (string)project_invoive["bsd_formno"] : "";
                    string serialno = project_invoive.Contains("bsd_serialno") ? (string)project_invoive["bsd_serialno"] : "";
                    invoice["bsd_formno"] = formno;
                    invoice["bsd_serialno"] = serialno;
                    invoice["bsd_issueddate"] = actualtime_iv;
                    invoice["bsd_units"] = units;
                    decimal vat_amout = Math.Round(fee / 11, MidpointRounding.AwayFromZero);
                    decimal vat_adj = 0;
                    decimal vat_amount_ho = vat_amout - vat_adj;
                    decimal invoiamountb4 = fee - vat_amount_ho;
                    decimal land_value = optionentry_invoive.Contains("bsd_landvaluededuction") ? ((Money)optionentry_invoive["bsd_landvaluededuction"]).Value : 0;
                    decimal vat_Handover_Installment = 0;
                    if (flag == true)
                    {
                        vat_Handover_Installment = Math.Round(land_value / 11, MidpointRounding.AwayFromZero);
                    }
                    invoice["bsd_invoiceamount"] = new Money(fee);
                    vat_amout = 0;
                    vat_adj = 0;
                    invoice["bsd_vatamount"] = new Money(vat_amout);
                    invoice["bsd_vatadjamount"] = new Money(vat_adj);
                    invoice["bsd_vatamounthandover"] = new Money(vat_adj);
                    invoice["bsd_invoiceamountb4vat"] = new Money(fee - vat_amout - vat_adj);
                    invoice["bsd_taxcode"] = EnTaxcode_1.ToEntityReference();
                    invoice["bsd_taxcodevalue"] = (decimal)EnTaxcode_1["bsd_value"];
                    if (create == true)
                    {
                        strMess.AppendLine("tạo ra invoice 1");
                        service.Create(invoice);
                    }

                }

                #endregion
            }
            #region
            if (IV_arrayfees != "")
            {
                string[] arrId_iv = IV_arrayfees.Split(',');
                string[] arrFeeAmount_iv = IV_arrayfeesamount.Split(',');
                Boolean create = false;
                Entity invoice = new Entity("bsd_invoice");
                invoice.Id = new Guid();
                invoice["bsd_type"] = new OptionSetValue(100000002);
                invoice["bsd_lastinstallmentamount"] = new Money(0);
                invoice["bsd_lastinstallmentvatamount"] = new Money(0);
                invoice["bsd_depositamount"] = new Money(0);
                strMess.AppendLine("IV_arrayfees 2");
                Boolean flag = false;
                decimal fee = 0;
                decimal amount_lastins = 0;
                string chuoi_name = "";
                EntityReference units = (EntityReference)ApplyDoc["bsd_units"];
                DateTime actualtime_iv = ApplyDoc.Contains("bsd_receiptdate") ? (DateTime)ApplyDoc["bsd_receiptdate"] : new DateTime();
                bool flag3 = false;
                for (int i = 0; i < arrId_iv.Length; i++)
                {
                    string[] arr = arrId_iv[i].Split('_');
                    string installmentid = arr[0];
                    string type1 = arr[1];
                    strMess.AppendLine("t.1");
                    Entity enInstallment = service.Retrieve("bsd_paymentschemedetail", new Guid(installmentid), new ColumnSet(true));
                    bolDot1 = enInstallment.Contains("bsd_ordernumber") && enInstallment["bsd_ordernumber"].ToString() == "1" ? true : false;
                    //Check thanh toán chưa hoàn tất 1st Installment (Thanh toán thiếu tiền đợt 01)
                    if (enInstallment.Contains("bsd_managementfeesstatus") && (bool)enInstallment["bsd_managementfeesstatus"] && bolDot1)
                    {
                        strMess.AppendLine("t.2");
                        continue;
                    }
                    //Thanh toán với Installment là đợt cuối
                    if (enInstallment.Contains("bsd_lastinstallment") && (bool)enInstallment["bsd_lastinstallment"] == true)
                    {
                        strMess.AppendLine("t.3");
                        continue;
                    }
                    strMess.AppendLine("statuscode: " + ((bool)enInstallment["bsd_managementfeesstatus"]));
                    if (type1 == "mana" && enInstallment.Contains("bsd_managementfeesstatus") && (bool)enInstallment["bsd_managementfeesstatus"])
                    {
                        strMess.AppendLine("t.4");
                        if (bolDot1)//nếu đợt 1, thì tiền= tiền đợt 1 + depositamount
                        {
                            decimal depositamount = enInstallment.Contains("bsd_depositamount") ? ((Money)enInstallment["bsd_depositamount"]).Value : 0;
                            fee = fee + depositamount;
                            invoice["bsd_depositamount"] = new Money(depositamount);
                            invoice["bsd_type"] = new OptionSetValue(100000003);
                        }
                        create = true;
                        fee += ((Money)enInstallment["bsd_managementamount"]).Value;
                        int so = enInstallment.Contains("bsd_ordernumber") ? (int)enInstallment["bsd_ordernumber"] : 0;
                        int last_pay = enInstallment.Contains("bsd_duedatecalculatingmethod") ? ((OptionSetValue)enInstallment["bsd_duedatecalculatingmethod"]).Value : 0;
                        if (last_pay == 100000002)
                        {
                            flag = true;
                            amount_lastins = enInstallment.Contains("bsd_amountofthisphase") ? ((Money)enInstallment["bsd_amountofthisphase"]).Value : 0;
                        }

                        if (flag3 == true)
                        {
                            chuoi_name += " + " + so.ToString();
                        }
                        else
                        {
                            chuoi_name += so.ToString();
                        }
                        flag3 = true;
                    }
                }
                strMess.AppendLine("chuoi_name 2 " + chuoi_name);
                if (chuoi_name != "")
                {
                    if (units != null)
                    {
                        Entity iv_units = service.Retrieve(units.LogicalName, units.Id, new ColumnSet(new string[] { "name" }));
                        invoice["bsd_name"] = "Thu tiền Phí quản lý của căn hộ đợt " + chuoi_name + " của căn hộ " + (string)iv_units["name"]; ;
                    }
                    else
                    {
                        invoice["bsd_name"] = "Thu tiền Phí quản lý của căn hộ đợt " + chuoi_name;
                    }

                    invoice["bsd_project"] = ApplyDoc.Contains("bsd_project") ? ApplyDoc["bsd_project"] : null;
                    invoice["bsd_optionentry"] = ApplyDoc.Contains("bsd_optionentry") ? ApplyDoc["bsd_optionentry"] : 0;
                    invoice["bsd_applydocument"] = ApplyDoc.ToEntityReference();

                    EntityReference Pay_Perchaser = ApplyDoc.Contains("bsd_customer") ? (EntityReference)ApplyDoc["bsd_customer"] : null;
                    invoice["bsd_purchaser"] = Pay_Perchaser;
                    if (Pay_Perchaser != null)
                    {
                        Entity iv_Perchaser = service.Retrieve(Pay_Perchaser.LogicalName, Pay_Perchaser.Id, new ColumnSet(true));
                        if (iv_Perchaser.LogicalName == "contact")
                        {
                            invoice["bsd_purchasernamecustomer"] = Pay_Perchaser;
                        }
                        else
                        {
                            invoice["bsd_purchasernamecompany"] = Pay_Perchaser;
                        }
                    }
                    if (actualtime_iv != new DateTime())
                    {
                        DateTime bsd_paymentactualtime = RetrieveLocalTimeFromUTCTime(actualtime_iv);
                        invoice["bsd_issueddate"] = bsd_paymentactualtime;
                    }
                    invoice["bsd_paymentmethod"] = new OptionSetValue(100000000);
                    Entity optionentry_invoive = service.Retrieve("salesorder", ((EntityReference)ApplyDoc["bsd_optionentry"]).Id,
                          new ColumnSet(new string[] {
                            "bsd_landvaluededuction",
                          }));
                    Entity project_invoive = service.Retrieve("bsd_project", ((EntityReference)ApplyDoc["bsd_project"]).Id,
               new ColumnSet(new string[] {
                            "bsd_formno","bsd_serialno"
               }));

                    string formno = project_invoive.Contains("bsd_formno") ? (string)project_invoive["bsd_formno"] : "";
                    string serialno = project_invoive.Contains("bsd_serialno") ? (string)project_invoive["bsd_serialno"] : "";
                    invoice["bsd_formno"] = formno;
                    invoice["bsd_serialno"] = serialno;
                    invoice["bsd_issueddate"] = actualtime_iv;
                    invoice["bsd_units"] = units;
                    decimal vat_amout = Math.Round(fee / 11, MidpointRounding.AwayFromZero);
                    decimal vat_adj = 0;
                    decimal vat_amount_ho = vat_amout - vat_adj;
                    decimal invoiamountb4 = fee - vat_amount_ho;
                    decimal land_value = optionentry_invoive.Contains("bsd_landvaluededuction") ? ((Money)optionentry_invoive["bsd_landvaluededuction"]).Value : 0;
                    decimal vat_Handover_Installment = 0;
                    if (flag == true)
                    {
                        vat_Handover_Installment = Math.Round(land_value / 11, MidpointRounding.AwayFromZero);
                    }
                    invoice["bsd_invoiceamount"] = new Money(fee);
                    invoice["bsd_vatamount"] = new Money(vat_amout);
                    invoice["bsd_vatadjamount"] = new Money(vat_adj);
                    invoice["bsd_vatamounthandover"] = new Money(vat_amout - vat_adj);
                    invoice["bsd_invoiceamountb4vat"] = new Money(fee - vat_amout - vat_adj);
                    invoice["bsd_taxcode"] = EnTaxcode10.ToEntityReference();
                    invoice["bsd_taxcodevalue"] = (decimal)EnTaxcode10["bsd_value"];
                    if (create == true)
                    {
                        strMess.AppendLine("tạo ra invoice 2");
                        service.Create(invoice);
                    }
                }

                #endregion
            }
            strMess.AppendLine(strResult1);
            strMess.AppendLine("------------------------------------------------");
            strMess.AppendLine(strResult2);
            strMess.AppendLine("------------------------------------------------");
            strMess.AppendLine(strResult3);
            //throw new InvalidPluginExecutionException(strMess.ToString());
            #endregion thành
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
        public Entity getLastInstallment(IOrganizationService service, Entity optionentry)
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
                  <condition attribute='bsd_optionentry' operator='eq' value='{0}'/>       
                  <condition attribute='bsd_lastinstallment' operator='eq' value='1'/>  
                  <condition attribute='statecode' operator='eq' value='0'/> 
                </filter>
              </entity>
            </fetch>";
            //<condition attribute='bsd_duedatecalculatingmethod' operator='eq' value='100000002' />
            fetchXml = string.Format(fetchXml, optionentry.Id);
            EntityCollection entc = service.RetrieveMultiple(new FetchExpression(fetchXml));
            return entc.Entities[0];
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