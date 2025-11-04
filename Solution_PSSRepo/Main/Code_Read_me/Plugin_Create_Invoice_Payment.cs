using Microsoft.Crm.Sdk.Messages;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Query;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.Script.Serialization;

namespace Plugin_Create_Invoice_Payment
{
    public class Plugin_Create_Invoice_Payment : IPlugin
    {
        IOrganizationService service = null;
        IOrganizationServiceFactory factory = null;
        IPluginExecutionContext context = null;
        StringBuilder strMess = new StringBuilder();

        public void Execute(IServiceProvider serviceProvider)
        {
            strMess.AppendLine("2222222222222222222222222222222222222");
            context = (IPluginExecutionContext)serviceProvider.GetService(typeof(IPluginExecutionContext));
            Entity target = (Entity)context.InputParameters["Target"];
            factory = (IOrganizationServiceFactory)serviceProvider.GetService(typeof(IOrganizationServiceFactory));
            service = factory.CreateOrganizationService(context.UserId);
            strMess.AppendLine("context.Depth " + context.Depth);
            if (context.Depth > 2)
                return;
            strMess.AppendLine("abcdef");
            //if (target.Contains("statuscode"))
            //{
            //    strMess.AppendLine(((OptionSetValue)target["statuscode"]).Value.ToString());
            //    throw new InvalidPluginExecutionException("Vào Plugin_Create_Invoice_Payment" + strMess.ToString());
            //}
            //else
            //{
            //    strMess.AppendLine("ko có stt code");
            //}
            if (target.Contains("statuscode") && ((OptionSetValue)target["statuscode"]).Value == 100000000)
            {
                strMess.AppendLine("vào code tạo invoice");
                var fetchXml = $@"
                <fetch>
                  <entity name='bsd_invoice'>
                    <filter type='and'>
                      <condition attribute='bsd_payment' operator='eq' value='{target.Id}'/>
                    </filter>
                  </entity>
                </fetch>";
                var checkInvoice = service.RetrieveMultiple(new FetchExpression(fetchXml.ToString()));
                if(checkInvoice!= null && checkInvoice.Entities.Count==0)
                {
                    var EnPayment = service.Retrieve(target.LogicalName, target.Id, new ColumnSet(true));
                    processApplyDocument(EnPayment);
                }
                //throw new InvalidPluginExecutionException("Vào Plugin_Create_Invoice_Payment" + strMess.ToString());
            }
            
        }
        public void processApplyDocument(Entity EnPayment)
        {
            bool bolDot1 = false;
            var serializer = new JavaScriptSerializer();
            strMess.AppendLine("vào invoice và type = ");
            Entity enIns = new Entity();
            Guid guiInsId = EnPayment.Contains("bsd_paymentschemedetail") ? ((EntityReference)EnPayment["bsd_paymentschemedetail"]).Id : new Guid();
            if (guiInsId != new Guid())
            {
                enIns = service.Retrieve("bsd_paymentschemedetail", guiInsId, new ColumnSet(true));
                //var enOptionEntry = service.Retrieve("salesorder", OptionEntryId, new ColumnSet(true));
                bolDot1 = enIns.Contains("bsd_ordernumber") && enIns["bsd_ordernumber"].ToString() == "1" ? true : false;
                //Check thanh toán chưa hoàn tất 1st Installment (Thanh toán thiếu tiền đợt 01)
                if (enIns.Contains("statuscode") && ((OptionSetValue)enIns["statuscode"]).Value == 100000000 && bolDot1)
                {
                    return;
                }
                //Thanh toán với Installment là đợt cuối
                if (enIns.Contains("bsd_lastinstallment") && (bool)enIns["bsd_lastinstallment"] == true)
                {
                    return;
                }
            }
            strMess.AppendLine("vào check");
            string s_bsd_arraypsdid = EnPayment.Contains("bsd_arraypsdid") ? (string)EnPayment["bsd_arraypsdid"] : "";
            string s_bsd_arrayamountpay = EnPayment.Contains("bsd_arrayamountpay") ? (string)EnPayment["bsd_arrayamountpay"] : "";
            EntityReference ins_ivcoice = EnPayment.Contains("bsd_paymentschemedetail") ? (EntityReference)EnPayment["bsd_paymentschemedetail"] : null;
            string IV_arrayfees = "";
            string IV_arrayfeesamount = "";
            if (EnPayment.LogicalName == "bsd_applydocument")
            {
                IV_arrayfees = EnPayment.Contains("bsd_arrayfeesid") ? (string)EnPayment["bsd_arrayfeesid"] : "";
                IV_arrayfeesamount = EnPayment.Contains("bsd_arrayfeesamount") ? (string)EnPayment["bsd_arrayfeesamount"] : "";
            }
            else
            {
                IV_arrayfees = EnPayment.Contains("bsd_arrayfees") ? (string)EnPayment["bsd_arrayfees"] : "";
                IV_arrayfeesamount = EnPayment.Contains("bsd_arrayfeesamount") ? (string)EnPayment["bsd_arrayfeesamount"] : "";
            }
            #region--thành
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
            DateTime actualtime_iv = new DateTime();
            EntityReference Pay_Perchaser = null;
            if (EnPayment.LogicalName == "bsd_applydocument")
            {
                actualtime_iv = EnPayment.Contains("bsd_receiptdate") ? (DateTime)EnPayment["bsd_receiptdate"] : new DateTime();
                Pay_Perchaser = EnPayment.Contains("bsd_customer") ? (EntityReference)EnPayment["bsd_customer"] : null;
            }
            else
            {
                actualtime_iv = EnPayment.Contains("bsd_paymentactualtime") ? (DateTime)EnPayment["bsd_paymentactualtime"] : new DateTime();
                Pay_Perchaser = EnPayment.Contains("bsd_purchaser") ? (EntityReference)EnPayment["bsd_purchaser"] : null;
            }
            if (ins_ivcoice != null && s_bsd_arraypsdid == "")
            {
                //Boolean flag_last = false;
                Entity invoice = new Entity("bsd_invoice");
                invoice.Id = new Guid();
                string chuoi_name = "";
                decimal amount_lastins = 0;
                decimal balance = ((Money)EnPayment["bsd_balance"]).Value;
                decimal amountpay = ((Money)EnPayment["bsd_amountpay"]).Value;
                decimal depositamount = EnPayment.Contains("bsd_depositamount") ? ((Money)EnPayment["bsd_depositamount"]).Value : 0;
                if (!bolDot1)
                    depositamount = 0;
                decimal d_amp = amountpay < balance ? amountpay : balance;
                d_amp = d_amp + depositamount;
                if (bolDot1 && enIns.Contains("bsd_amountwaspaid"))//nếu đợt 1, thì tiền= tiền đợt 1 + depositamount
                    d_amp = ((Money)enIns["bsd_amountwaspaid"]).Value + depositamount;
                invoice["bsd_depositamount"] = new Money(depositamount);
                invoice["bsd_lastinstallmentamount"] = new Money(0);
                invoice["bsd_lastinstallmentvatamount"] = new Money(0);
                EntityReference ins = (EntityReference)EnPayment["bsd_paymentschemedetail"];

                Entity ec_ins = service.Retrieve(ins.LogicalName, ins.Id, new ColumnSet(true));
                int so = ec_ins.Contains("bsd_ordernumber") ? (int)ec_ins["bsd_ordernumber"] : 0;
                chuoi_name += so.ToString();
                EntityReference units = (EntityReference)EnPayment["bsd_units"];
                Entity iv_units = service.Retrieve(units.LogicalName, units.Id, new ColumnSet(new string[] { "name" }));
                invoice["bsd_name"] = "Thu tiền đợt " + chuoi_name + " của căn hộ " + (string)iv_units["name"];
                invoice["bsd_project"] = EnPayment.Contains("bsd_project") ? EnPayment["bsd_project"] : null;
                invoice["bsd_optionentry"] = EnPayment.Contains("bsd_optionentry") ? EnPayment["bsd_optionentry"] : null;
                invoice["bsd_payment"] = EnPayment.ToEntityReference();
                invoice["bsd_type"] = new OptionSetValue(100000000);
                Entity optionentry_invoive = service.Retrieve("salesorder", ((EntityReference)EnPayment["bsd_optionentry"]).Id,
                      new ColumnSet(new string[] {
                            "bsd_landvaluededuction",
                      }));
                Entity project_invoive = service.Retrieve("bsd_project", ((EntityReference)EnPayment["bsd_project"]).Id,
                    new ColumnSet(new string[] {
                            "bsd_formno","bsd_serialno"
                    }));
                string formno = project_invoive.Contains("bsd_formno") ? (string)project_invoive["bsd_formno"] : "";
                string serialno = project_invoive.Contains("bsd_serialno") ? (string)project_invoive["bsd_serialno"] : "";
                invoice["bsd_formno"] = formno;
                invoice["bsd_serialno"] = serialno;
                invoice["bsd_issueddate"] = actualtime_iv;
                invoice["bsd_units"] = units;
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
                if (EnPayment.Contains("bsd_paymentactualtime"))
                {
                    DateTime bsd_paymentactualtime = RetrieveLocalTimeFromUTCTime((DateTime)EnPayment["bsd_paymentactualtime"]);
                    invoice["bsd_issueddate"] = bsd_paymentactualtime;
                }
                invoice["bsd_paymentmethod"] = new OptionSetValue(100000000);
                int last_pay = ec_ins.Contains("bsd_duedatecalculatingmethod") ? ((OptionSetValue)ec_ins["bsd_duedatecalculatingmethod"]).Value : 0;
                if (last_pay == 100000002)
                {
                    #region tạo invoice cho ins handover
                    Entity optionentryEn = EnPayment.Contains("bsd_optionentry") ? service.Retrieve("salesorder", ((EntityReference)EnPayment["bsd_optionentry"]).Id, new ColumnSet(true)) : null;
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
                    decimal amount_waspaid = ec_ins.Contains("bsd_amountwaspaid") ? ((Money)ec_ins["bsd_amountwaspaid"]).Value : 0;//Amount was paid" của Đợt bàn giao 
                    decimal vatHandoverPayment = Math.Round(amount_waspaid / 11, MidpointRounding.AwayFromZero);//VAT Handover Payment


                    decimal vat_amout = Math.Round(totalamountwaspaidins / 11, MidpointRounding.AwayFromZero);
                    decimal vat_adj = 0;
                    decimal vat_amount_ho = vat_amout - vat_adj;
                    decimal invoiamountb4 = d_amp - vat_amount_ho;
                    decimal land_value = optionentry_invoive.Contains("bsd_landvaluededuction") ? ((Money)optionentry_invoive["bsd_landvaluededuction"]).Value : 0;
                    decimal vat_Handover_Installment = 0;
                    bool bolThanhToanDu = EnPayment.Contains("bsd_differentamount") && ((Money)EnPayment["bsd_differentamount"]).Value >= 0 ? true : false;

                    if (totaltax >= vat_amout + vatamount_lastins + vatHandoverPayment)
                    {
                        strMess.AppendLine("case c >= 4");
                        if (bolThanhToanDu)
                        {
                            invoice["bsd_depositamount"] = new Money(0);
                            strMess.AppendLine("case thanh toán đủ và dư");
                            vat_Handover_Installment = Math.Round(land_value / 11, MidpointRounding.AwayFromZero);
                            invoice["bsd_name"] = "Thu tiền đợt " + chuoi_name + " của căn hộ " + (string)iv_units["name"];
                            invoice["bsd_invoiceamount"] = new Money(amountpay - land_value);
                            invoice["bsd_type"] = new OptionSetValue(100000000);//Installment
                            invoice["statuscode"] = new OptionSetValue(100000000);//Confirm
                            vat_amout = Math.Round((amountpay - land_value) / 11, MidpointRounding.AwayFromZero);
                            invoice["bsd_vatamount"] = new Money(vat_amout);
                            invoice["bsd_vatadjamount"] = new Money(0);
                            invoice["bsd_vatamounthandover"] = new Money(vat_amout);
                            invoice["bsd_invoiceamountb4vat"] = new Money(amountpay - land_value - vat_amout);
                            invoice["bsd_taxcode"] = EnTaxcode10.ToEntityReference();
                            invoice["bsd_taxcodevalue"] = EnTaxcode10["bsd_value"];
                            service.Create(invoice);
                            strMess.AppendLine("Tạo invoice dòng 246");
                            //strMess.AppendLine(serializer.Serialize(invoice));
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
                            invoiceLandValue["bsd_taxcodevalue"] = EnTaxcode_1["bsd_value"];
                            service.Create(invoiceLandValue);
                            strMess.AppendLine("Tạo invoice dòng 261");
                            //strMess.AppendLine(serializer.Serialize(invoice));

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
                            invoiceLast["bsd_taxcodevalue"] = EnTaxcode10["bsd_value"];
                            service.Create(invoiceLast);
                            strMess.AppendLine("Tạo invoice dòng 277");
                            //strMess.AppendLine(serializer.Serialize(invoiceLast));
                        }
                        else
                        {
                            strMess.AppendLine("case thanh toán thiếu");
                            vat_Handover_Installment = 0;
                            invoice["bsd_type"] = new OptionSetValue(100000000);//Installment
                            invoice["statuscode"] = new OptionSetValue(100000000);//yêu cầu mới, cứ tạo invoice thì sẽ confirm
                            invoice["bsd_invoiceamount"] = new Money(d_amp);
                            invoice["bsd_vatamount"] = new Money(vat_amout);
                            invoice["bsd_vatadjamount"] = new Money(vat_Handover_Installment);
                            invoice["bsd_vatamounthandover"] = new Money(vat_amout - vat_Handover_Installment);
                            invoice["bsd_invoiceamountb4vat"] = new Money(invoiamountb4 - vat_amout - vat_Handover_Installment);
                            invoice["bsd_taxcode"] = EnTaxcode10.ToEntityReference();
                            invoice["bsd_taxcodevalue"] = EnTaxcode10["bsd_value"];
                            service.Create(invoice);
                            strMess.AppendLine("Tạo invoice dòng 294");
                            //strMess.AppendLine(serializer.Serialize(invoice));
                        }
                    }
                    else
                    {
                        strMess.AppendLine("case c < 4");
                        vat_amout = Math.Round(d_amp / 11, MidpointRounding.AwayFromZero);
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
                            invoice["bsd_taxcodevalue"] = EnTaxcode_1["bsd_value"];
                            service.Create(invoice);
                            strMess.AppendLine("Tạo invoice dòng 316");
                            //strMess.AppendLine(serializer.Serialize(invoice));

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
                            invoiceLast["bsd_taxcodevalue"] = EnTaxcode10["bsd_value"];
                            service.Create(invoiceLast);
                            strMess.AppendLine("Tạo invoice dòng 332");
                            //strMess.AppendLine(serializer.Serialize(invoiceLast));
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
                            invoice["bsd_invoiceamountb4vat"] = new Money(invoiamountb4 - vat_amout - vat_Handover_Installment);
                            invoice["bsd_taxcode"] = EnTaxcode_1.ToEntityReference();
                            invoice["bsd_taxcodevalue"] = EnTaxcode_1["bsd_value"];
                            service.Create(invoice);
                            strMess.AppendLine("Tạo invoice dòng 350");
                            //strMess.AppendLine(serializer.Serialize(invoice));
                        }
                    }
                    #endregion
                    //if ((amount_lastins - (amount_waspaid + d_amp)) < 1)
                    //{
                    //    invoice["statuscode"] = new OptionSetValue(100000000);//Confirm
                    //}
                    //throw new InvalidPluginExecutionException("totaltax: " + totaltax + " vat_amout: " + vat_amout + " vatamount_lastins: " + vatamount_lastins + " vatHandoverPayment: " + vatHandoverPayment);
                }
                else
                {
                    #region Code cũ
                    strMess.AppendLine("code cũ");
                    decimal vat_amout = Math.Round(d_amp / 11, MidpointRounding.AwayFromZero);
                    decimal vat_adj = 0;
                    decimal vat_amount_ho = vat_amout - vat_adj;
                    decimal invoiamountb4 = d_amp - vat_amount_ho;
                    decimal land_value = optionentry_invoive.Contains("bsd_landvaluededuction") ? ((Money)optionentry_invoive["bsd_landvaluededuction"]).Value : 0;
                    decimal vat_Handover_Installment = 0;
                    invoice["statuscode"] = new OptionSetValue(100000000);//yêu cầu mới, cứ tạo invoice thì sẽ confirm
                    invoice["bsd_invoiceamount"] = new Money(d_amp);
                    invoice["bsd_vatamount"] = new Money(vat_amout);
                    invoice["bsd_vatadjamount"] = new Money(vat_Handover_Installment);
                    invoice["bsd_vatamounthandover"] = new Money(vat_amout - vat_Handover_Installment);
                    invoice["bsd_invoiceamountb4vat"] = new Money(invoiamountb4 - vat_amout - vat_Handover_Installment);
                    invoice["bsd_taxcode"] = EnTaxcode10.ToEntityReference();
                    invoice["bsd_taxcodevalue"] = EnTaxcode10["bsd_value"];
                    service.Create(invoice);
                    strMess.AppendLine("Tạo invoice dòng 380");
                    //strMess.AppendLine(serializer.Serialize(invoice));
                    #endregion
                }

            }//thanh toán cho ins, ko có dư

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
                if (ins_ivcoice != null && enIns != new Entity())
                {
                    s_bsd_arraypsdid = ins_ivcoice.Id.ToString() + "," + s_bsd_arraypsdid;
                    s_bsd_arrayamountpay = ((Money)enIns["bsd_amountwaspaid"]).Value.ToString() + "," + s_bsd_arrayamountpay;
                    strMess.AppendLine("s_bsd_arrayamountpay " + s_bsd_arrayamountpay);
                }
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
                EntityReference units = (EntityReference)EnPayment["bsd_units"];
                strMess.AppendLine("invoice2");

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
                    enIns = service.Retrieve("bsd_paymentschemedetail", paymentsheme.Id, new ColumnSet(true));
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
                //decimal amountpay = EnCallFrom.Contains("bsd_amountpay") ? ((Money)EnCallFrom["bsd_amountpay"]).Value : 0;
                decimal amountpay = d_amp;
                Entity iv_units = service.Retrieve(units.LogicalName, units.Id, new ColumnSet(new string[] { "name" }));
                invoice["bsd_name"] = "Thu tiền đợt " + chuoi_name + " của căn hộ " + (string)iv_units["name"];
                invoice["bsd_project"] = EnPayment.Contains("bsd_project") ? EnPayment["bsd_project"] : null;
                invoice["bsd_optionentry"] = EnPayment.Contains("bsd_optionentry") ? EnPayment["bsd_optionentry"] : 0;
                if (EnPayment.LogicalName == "bsd_applydocument")
                    invoice["bsd_applydocument"] = EnPayment.ToEntityReference();
                else
                    invoice["bsd_payment"] = EnPayment.ToEntityReference();

                invoice["bsd_issueddate"] = actualtime_iv;
                invoice["bsd_units"] = units;
                strMess.AppendLine("invoice4");

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
                Entity optionentry_invoive = service.Retrieve("salesorder", ((EntityReference)EnPayment["bsd_optionentry"]).Id,
                      new ColumnSet(new string[] {
                            "bsd_landvaluededuction",
                      }));
                Entity project_invoive = service.Retrieve("bsd_project", ((EntityReference)EnPayment["bsd_project"]).Id,
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
                {
                    service.Create(invoice);
                    strMess.AppendLine("Tạo invoice dòng 547");
                    //strMess.AppendLine(serializer.Serialize(invoice));
                }

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
                    strMess.AppendLine("Tạo invoice dòng 576");
                    //strMess.AppendLine(serializer.Serialize(Invoice1st));
                }
                #endregion
                #region Invoice cho Handover
                if (InsHandoverId != new Guid() && decAmountInsHandover != 0)//trường hợp có cấn trừ ins handover
                {

                    #region tạo invoice cho ins handover
                    Entity optionentryEn = EnPayment.Contains("bsd_optionentry") ? service.Retrieve("salesorder", ((EntityReference)EnPayment["bsd_optionentry"]).Id, new ColumnSet(true)) : null;
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
                    int index = Array.IndexOf(ivoid_idINS, InsHandoverId.ToString());
                    d_amp = decimal.Parse(ivoid_amINS[index]);
                    amountpay = d_amp;
                    strMess.AppendLine("amountpay handover" + amountpay);
                    vat_amout = Math.Round(d_amp / 11, MidpointRounding.AwayFromZero);
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
                        strMess.AppendLine("case c >= 44444");
                        if (bolThanhToanDu)
                        {
                            invoice["bsd_depositamount"] = new Money(0);
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
                            strMess.AppendLine("Tạo invoice dòng 646");
                            //strMess.AppendLine(serializer.Serialize(invoice));

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
                            strMess.AppendLine("Tạo invoice dòng 662");
                            //strMess.AppendLine(serializer.Serialize(invoiceLandValue));


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
                            strMess.AppendLine("Tạo invoice dòng 679");
                            //strMess.AppendLine(serializer.Serialize(invoiceLast));
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
                            strMess.AppendLine("Tạo invoice dòng 696");
                            //strMess.AppendLine(serializer.Serialize(invoice));
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
                            strMess.AppendLine("Tạo invoice dòng 718");
                            //strMess.AppendLine(serializer.Serialize(invoice));

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
                            strMess.AppendLine("Tạo invoice dòng 734");
                            //strMess.AppendLine(serializer.Serialize(invoiceLast));
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
                            strMess.AppendLine("Tạo invoice dòng 752");
                            //strMess.AppendLine(serializer.Serialize(invoice));
                        }
                    }
                    #endregion
                }
                #endregion
                //}

            }
            #region 
            if (IV_arrayfees != "")//main
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
                EntityReference units = EnPayment.Contains("bsd_units") ? (EntityReference)EnPayment["bsd_units"] : null;
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
                    //if (units != null)
                    //{
                    //    Entity iv_units = service.Retrieve(units.LogicalName, units.Id, new ColumnSet(new string[] { "name" }));
                    //    invoice["bsd_name"] = "Thu tiền Phí bảo trì của căn hộ " + (string)iv_units["name"]; ;
                    //}
                    //else
                    //{
                    invoice["bsd_name"] = "Thu tiền Phí bảo trì của căn hộ đợt " + chuoi_name;
                    //}
                    invoice["bsd_project"] = EnPayment.Contains("bsd_project") ? EnPayment["bsd_project"] : null;
                    invoice["bsd_optionentry"] = EnPayment.Contains("bsd_optionentry") ? EnPayment["bsd_optionentry"] : null;
                    if (EnPayment.LogicalName == "bsd_applydocument")
                        invoice["bsd_applydocument"] = EnPayment.ToEntityReference();
                    else
                        invoice["bsd_payment"] = EnPayment.ToEntityReference();

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
                    Entity optionentry_invoive = service.Retrieve("salesorder", ((EntityReference)EnPayment["bsd_optionentry"]).Id,
                          new ColumnSet(new string[] {
                            "bsd_landvaluededuction",
                          }));
                    Entity project_invoive = service.Retrieve("bsd_project", ((EntityReference)EnPayment["bsd_project"]).Id,
               new ColumnSet(new string[] {
                            "bsd_formno","bsd_serialno"
               }));

                    string formno = project_invoive.Contains("bsd_formno") ? (string)project_invoive["bsd_formno"] : "";
                    string serialno = project_invoive.Contains("bsd_serialno") ? (string)project_invoive["bsd_serialno"] : "";
                    invoice["bsd_formno"] = formno;
                    invoice["bsd_serialno"] = serialno;
                    invoice["bsd_issueddate"] = actualtime_iv;
                    invoice["bsd_units"] = units;
                    //decimal vat_amout = Math.Round(fee / 11, MidpointRounding.AwayFromZero);
                    decimal vat_adj = 0;
                    decimal land_value = optionentry_invoive.Contains("bsd_landvaluededuction") ? ((Money)optionentry_invoive["bsd_landvaluededuction"]).Value : 0;
                    invoice["bsd_invoiceamount"] = new Money(fee);
                    decimal vat_amout = 0;
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
                        strMess.AppendLine("Tạo invoice dòng 899");
                        //strMess.AppendLine(serializer.Serialize(invoice));
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
                EntityReference units = (EntityReference)EnPayment["bsd_units"];
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
                    //if (units != null)
                    //{
                    //    Entity iv_units = service.Retrieve(units.LogicalName, units.Id, new ColumnSet(new string[] { "name" }));
                    //    invoice["bsd_name"] = "Thu tiền Phí quản lý của căn hộ đợt " + chuoi_name + " của căn hộ " + (string)iv_units["name"]; ;
                    //}
                    //else
                    //{
                    invoice["bsd_name"] = "Thu tiền Phí quản lý của căn hộ đợt " + chuoi_name;
                    //}

                    invoice["bsd_project"] = EnPayment.Contains("bsd_project") ? EnPayment["bsd_project"] : null;
                    invoice["bsd_optionentry"] = EnPayment.Contains("bsd_optionentry") ? EnPayment["bsd_optionentry"] : 0;
                    if (EnPayment.LogicalName == "bsd_applydocument")
                        invoice["bsd_applydocument"] = EnPayment.ToEntityReference();
                    else
                        invoice["bsd_payment"] = EnPayment.ToEntityReference();

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
                    Entity optionentry_invoive = service.Retrieve("salesorder", ((EntityReference)EnPayment["bsd_optionentry"]).Id,
                          new ColumnSet(new string[] {
                            "bsd_landvaluededuction",
                          }));
                    Entity project_invoive = service.Retrieve("bsd_project", ((EntityReference)EnPayment["bsd_project"]).Id,
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
                        strMess.AppendLine("Tạo invoice dòng 1050");
                        //strMess.AppendLine(serializer.Serialize(invoice));
                    }
                }

                #endregion
            }
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
    }
}
