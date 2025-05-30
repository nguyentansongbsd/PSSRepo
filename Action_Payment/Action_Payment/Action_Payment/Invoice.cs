using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.Script.Serialization;
using Microsoft.Crm.Sdk.Messages;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Query;

namespace Action_Payment
{
    class Invoice
    {
        IOrganizationService service = null;
        IOrganizationServiceFactory factory = null;
        IPluginExecutionContext context = null;
        Payment payment;
        ITracingService TracingSe = null;
        public Invoice(IServiceProvider serviceProvider)
        {
            context = (IPluginExecutionContext)serviceProvider.GetService(typeof(IPluginExecutionContext));
            EntityReference target = (EntityReference)context.InputParameters["Target"];
            factory = (IOrganizationServiceFactory)serviceProvider.GetService(typeof(IOrganizationServiceFactory));
            service = factory.CreateOrganizationService(context.UserId);
            payment = new Payment(serviceProvider);
            TracingSe = (ITracingService)serviceProvider.GetService(typeof(ITracingService));
            TracingSe.Trace("vào thiết lập invoice");
        }
        public void processPayment(Entity enPayment, Guid InsId)
        {
            bool bolDot1 = false;
            string strResult1 = "";
            string strResult2 = "";
            string strResult3 = "";
            var serializer = new JavaScriptSerializer();
            TracingSe.Trace("vào invoice và type = ");
            try
            {
                if (enPayment.Contains("bsd_paymenttype") && (((OptionSetValue)enPayment["bsd_paymenttype"]).Value == 100000002 || ((OptionSetValue)enPayment["bsd_paymenttype"]).Value == 100000004))
                {
                    TracingSe.Trace("vào check");
                    Entity enIns = new Entity();
                    if (InsId != new Guid())
                    {
                        enIns = service.Retrieve("bsd_paymentschemedetail", InsId, new ColumnSet(true));
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
                    TracingSe.Trace("vào check1");
                    EntityReference ins_ivcoice = enPayment.Contains("bsd_paymentschemedetail") ? (EntityReference)enPayment["bsd_paymentschemedetail"] : null;
                    #region--thành
                    TracingSe.Trace("vào check4");
                    string IV_arraypsdid = enPayment.Contains("bsd_arraypsdid") ? (string)enPayment["bsd_arraypsdid"] : "";
                    TracingSe.Trace("vào check5");
                    string IV_arrayamountpay = enPayment.Contains("bsd_arrayamountpay") ? (string)enPayment["bsd_arrayamountpay"] : "";
                    TracingSe.Trace("vào check6");
                    string IV_arrayfees = enPayment.Contains("bsd_arrayfees") ? (string)enPayment["bsd_arrayfees"] : "";
                    TracingSe.Trace("vào check7");
                    string IV_arrayfeesamount = enPayment.Contains("bsd_arrayfeesamount") ? (string)enPayment["bsd_arrayfeesamount"] : "";
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
                    TracingSe.Trace("vào check8");
                    var EnColtaxcode = service.RetrieveMultiple(new FetchExpression(fetchXmltaxcode.ToString()));
                    TracingSe.Trace("vào check9");
                    Entity EnTaxcode_1 = null;
                    Entity EnTaxcode10 = null;
                    if (EnColtaxcode == null)
                        throw new InvalidPluginExecutionException("Please check your Taxcode!");
                    foreach (var item in EnColtaxcode.Entities)
                    {
                        TracingSe.Trace("item: " + item["bsd_name"].ToString());
                        if (item.Contains("bsd_name") && item["bsd_name"].ToString() == "-1")
                        {
                            TracingSe.Trace("lấy được tax -1");
                            if (EnTaxcode_1 != null)
                                continue;
                            EnTaxcode_1 = item;
                        }
                        if (item.Contains("bsd_name") && item["bsd_name"].ToString() == "10")
                        {
                            TracingSe.Trace("lấy được tax 10");
                            if (EnTaxcode10 != null)
                                continue;
                            EnTaxcode10 = item;
                        }
                    }
                    EntityReference enCurrency = new EntityReference("transactioncurrency", new Guid("{6E9A954B-72B7-E611-80F6-3863BB360C48}"));
                    if (ins_ivcoice != null && IV_arraypsdid == "")
                    {
                        //Boolean flag_last = false;
                        Entity invoice = new Entity("bsd_invoice");
                        invoice.Id = new Guid();
                        string chuoi_name = "";
                        decimal amount_lastins = 0;
                        decimal balance = ((Money)enPayment["bsd_balance"]).Value;
                        decimal amountpay = ((Money)enPayment["bsd_amountpay"]).Value;
                        decimal depositamount = enPayment.Contains("bsd_depositamount") ? ((Money)enPayment["bsd_depositamount"]).Value : 0;
                        if (!bolDot1)
                            depositamount = 0;
                        decimal d_amp = amountpay < balance ? amountpay : balance;
                        d_amp = d_amp + depositamount;
                        if (bolDot1 && enIns.Contains("bsd_amountwaspaid"))//nếu đợt 1, thì tiền= tiền đợt 1 + depositamount
                            d_amp = ((Money)enIns["bsd_amountwaspaid"]).Value + depositamount;
                        invoice["bsd_depositamount"] = new Money(depositamount);
                        invoice["bsd_lastinstallmentamount"] = new Money(0);
                        invoice["bsd_lastinstallmentvatamount"] = new Money(0);
                        invoice["transactioncurrencyid"] = enCurrency;
                        EntityReference ins = (EntityReference)enPayment["bsd_paymentschemedetail"];
                        EntityReference units = (EntityReference)enPayment["bsd_units"];
                        DateTime actualtime_iv = (DateTime)enPayment["bsd_paymentactualtime"];
                        Entity ec_ins = service.Retrieve(ins.LogicalName, ins.Id, new ColumnSet(true));
                        int so = ec_ins.Contains("bsd_ordernumber") ? (int)ec_ins["bsd_ordernumber"] : 0;
                        chuoi_name += so.ToString();
                        Entity iv_units = service.Retrieve(units.LogicalName, units.Id, new ColumnSet(new string[] { "name" }));
                        invoice["bsd_name"] = "Thu tiền đợt " + chuoi_name + " của căn hộ " + (string)iv_units["name"];
                        invoice["bsd_project"] = enPayment.Contains("bsd_project") ? enPayment["bsd_project"] : null;
                        invoice["bsd_optionentry"] = enPayment.Contains("bsd_optionentry") ? enPayment["bsd_optionentry"] : null;
                        invoice["bsd_payment"] = enPayment.ToEntityReference();
                        invoice["bsd_type"] = new OptionSetValue(100000003);
                        Entity optionentry_invoive = service.Retrieve("salesorder", ((EntityReference)enPayment["bsd_optionentry"]).Id,
                              new ColumnSet(new string[] {
                            "bsd_landvaluededuction",
                              }));
                        Entity project_invoive = service.Retrieve("bsd_project", ((EntityReference)enPayment["bsd_project"]).Id,
                            new ColumnSet(new string[] {
                            "bsd_formno","bsd_serialno"
                            }));
                        string formno = project_invoive.Contains("bsd_formno") ? (string)project_invoive["bsd_formno"] : "";
                        string serialno = project_invoive.Contains("bsd_serialno") ? (string)project_invoive["bsd_serialno"] : "";
                        invoice["bsd_formno"] = formno;
                        invoice["bsd_serialno"] = serialno;
                        invoice["bsd_issueddate"] = actualtime_iv;
                        invoice["bsd_units"] = units;
                        EntityReference Pay_Perchaser = enPayment.Contains("bsd_purchaser") ? (EntityReference)enPayment["bsd_purchaser"] : null;
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
                        if (enPayment.Contains("bsd_paymentactualtime"))
                        {
                            DateTime bsd_paymentactualtime = RetrieveLocalTimeFromUTCTime((DateTime)enPayment["bsd_paymentactualtime"]);
                            invoice["bsd_issueddate"] = bsd_paymentactualtime;
                        }
                        invoice["bsd_paymentmethod"] = new OptionSetValue(100000000);
                        int last_pay = ec_ins.Contains("bsd_duedatecalculatingmethod") ? ((OptionSetValue)ec_ins["bsd_duedatecalculatingmethod"]).Value : 0;
                        if (last_pay == 100000002)
                        {
                            #region tạo invoice cho ins handover
                            Entity optionentryEn = enPayment.Contains("bsd_optionentry") ? service.Retrieve("salesorder", ((EntityReference)enPayment["bsd_optionentry"]).Id, new ColumnSet(true)) : null;
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
                            bool bolThanhToanDu = enPayment.Contains("bsd_differentamount") && ((Money)enPayment["bsd_differentamount"]).Value >= 0 ? true : false;

                            if (totaltax >= vat_amout + vatamount_lastins + vatHandoverPayment)
                            {
                                TracingSe.Trace("case c >= 4");
                                if (bolThanhToanDu)
                                {
                                    TracingSe.Trace("case thanh toán đủ và dư");
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
                                    invoiceLandValue["bsd_taxcodevalue"] = EnTaxcode_1["bsd_value"];
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
                                    invoiceLast["bsd_taxcodevalue"] = EnTaxcode10["bsd_value"];
                                    service.Create(invoiceLast);
                                    strResult2 = serializer.Serialize(invoiceLast);
                                }
                                else
                                {
                                    TracingSe.Trace("case thanh toán thiếu");
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
                                    strResult3 = serializer.Serialize(invoice);
                                }
                            }
                            else
                            {
                                TracingSe.Trace("case c < 4");
                                vat_amout = Math.Round(d_amp / 11, MidpointRounding.AwayFromZero);
                                if (bolThanhToanDu)
                                {
                                    TracingSe.Trace("case thanh toán đủ và dư");
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
                                    invoiceLast["bsd_taxcodevalue"] = EnTaxcode10["bsd_value"];
                                    service.Create(invoiceLast);
                                    strResult2 = serializer.Serialize(invoiceLast);
                                }
                                else
                                {
                                    TracingSe.Trace("case thanh toán thiếu");
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
                                    strResult3 = serializer.Serialize(invoice);
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
                            TracingSe.Trace("code cũ");
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
                            #endregion
                        }

                    }//thanh toán cho ins, ko có dư

                    if (IV_arraypsdid != "")//thanh toán cho ins, còn tiền dư, thanh toán tiếp cho các ins khác
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
                        Guid InsId1st = new Guid();
                        IV_arraypsdid = ins_ivcoice.Id.ToString() + "," + IV_arraypsdid;
                        IV_arrayamountpay = ((Money)enIns["bsd_amountwaspaid"]).Value.ToString() + "," + IV_arrayamountpay;
                        if (IV_arraypsdid != "")
                        {
                            ivoid_idINS = IV_arraypsdid.Split(',');
                            ivoid_amINS = IV_arrayamountpay.Split(',');
                        }
                        TracingSe.Trace("invoice1");
                        EntityCollection ec_ins = payment.get_ecINS(service, ivoid_idINS);
                        if (ec_ins.Entities.Count <= 0) throw new InvalidPluginExecutionException("Cannot find any Installment from Installment list. Please check again!");

                        for (int m = 0; m < ivoid_amINS.Length; m++)
                        {
                            d_amp += decimal.Parse(ivoid_amINS[m]);
                        }

                        string chuoi_name = "";
                        decimal amount_lastins = 0;
                        decimal pm_balance = enPayment.Contains("bsd_balance") ? ((Money)enPayment["bsd_balance"]).Value : 0;
                        decimal pm_amountpay = enPayment.Contains("bsd_amountpay") ? ((Money)enPayment["bsd_amountpay"]).Value : 0;
                        decimal check_Dk = pm_amountpay > pm_balance ? pm_balance : pm_amountpay;
                        EntityReference ins = (EntityReference)enPayment["bsd_paymentschemedetail"];
                        EntityReference units = (EntityReference)enPayment["bsd_units"];
                        Entity ec_ins_pay = service.Retrieve(ins.LogicalName, ins.Id, new ColumnSet(true));
                        TracingSe.Trace("invoice2");
                        DateTime actualtime_iv = (DateTime)enPayment["bsd_paymentactualtime"];
                        int so_pay = ec_ins_pay.Contains("bsd_ordernumber") ? (int)ec_ins_pay["bsd_ordernumber"] : 0;

                        #region code cũ
                        TracingSe.Trace("invoice3");
                        Entity invoice = new Entity("bsd_invoice");
                        invoice.Id = new Guid();
                        invoice["transactioncurrencyid"] = enCurrency;
                        invoice["bsd_lastinstallmentamount"] = new Money(0);
                        invoice["bsd_lastinstallmentvatamount"] = new Money(0);
                        invoice["bsd_depositamount"] = new Money(0);
                        foreach (Entity paymentsheme in ec_ins.Entities)
                        {
                            var enInsCheck = service.Retrieve("bsd_paymentschemedetail", paymentsheme.Id, new ColumnSet(true));
                            bolDot1 = enInsCheck.Contains("bsd_ordernumber") && enInsCheck["bsd_ordernumber"].ToString() == "1" ? true : false;
                            //Check thanh toán chưa hoàn tất 1st Installment (Thanh toán thiếu tiền đợt 01)
                            if (enInsCheck.Contains("statuscode") && ((OptionSetValue)enInsCheck["statuscode"]).Value == 100000000 && bolDot1)
                            {
                                continue;
                            }
                            //Thanh toán với Installment là đợt cuối
                            if (enInsCheck.Contains("bsd_lastinstallment") && (bool)enInsCheck["bsd_lastinstallment"] == true)
                            {
                                continue;
                            }
                            if (bolDot1)//nếu đợt 1, thì tiền= tiền đợt 1 + depositamount
                            {
                                InsId1st = enInsCheck.Id;
                                depositamount = enInsCheck.Contains("bsd_depositamount") ? ((Money)enInsCheck["bsd_depositamount"]).Value : 0;
                                decAmount1st = ((Money)enInsCheck["bsd_amountwaspaid"]).Value + depositamount;
                                d_amp = d_amp - ((Money)enInsCheck["bsd_amountwaspaid"]).Value;
                                continue;
                            }

                            int last = paymentsheme.Contains("bsd_duedatecalculatingmethod") ? ((OptionSetValue)paymentsheme["bsd_duedatecalculatingmethod"]).Value : 0;
                            if (last == 100000002)
                            {

                                InsHandoverId = paymentsheme.Id;
                                decAmountInsHandover = ((Money)paymentsheme["bsd_amountofthisphase"]).Value;
                                d_amp = d_amp - decAmountInsHandover;
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
                        decimal balance = enPayment.Contains("bsd_balance") ? ((Money)enPayment["bsd_balance"]).Value : 0;
                        decimal amountpay = enPayment.Contains("bsd_amountpay") ? ((Money)enPayment["bsd_amountpay"]).Value : 0;
                        Entity iv_units = service.Retrieve(units.LogicalName, units.Id, new ColumnSet(new string[] { "name" }));
                        invoice["bsd_name"] = "Thu tiền đợt " + chuoi_name + " của căn hộ " + (string)iv_units["name"];
                        invoice["bsd_project"] = enPayment.Contains("bsd_project") ? enPayment["bsd_project"] : null;
                        invoice["bsd_optionentry"] = enPayment.Contains("bsd_optionentry") ? enPayment["bsd_optionentry"] : 0;
                        invoice["bsd_payment"] = enPayment.ToEntityReference();
                        invoice["bsd_type"] = new OptionSetValue(100000000);
                        invoice["bsd_issueddate"] = actualtime_iv;
                        invoice["bsd_units"] = units;
                        TracingSe.Trace("invoice4");
                        EntityReference Pay_Perchaser = enPayment.Contains("bsd_purchaser") ? (EntityReference)enPayment["bsd_purchaser"] : null;
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
                        if (enPayment.Contains("bsd_paymentactualtime"))
                        {
                            DateTime bsd_paymentactualtime = RetrieveLocalTimeFromUTCTime((DateTime)enPayment["bsd_paymentactualtime"]);
                            invoice["bsd_issueddate"] = bsd_paymentactualtime;
                        }
                        invoice["bsd_paymentmethod"] = new OptionSetValue(100000000);
                        Entity optionentry_invoive = service.Retrieve("salesorder", ((EntityReference)enPayment["bsd_optionentry"]).Id,
                              new ColumnSet(new string[] {
                            "bsd_landvaluededuction",
                              }));
                        Entity project_invoive = service.Retrieve("bsd_project", ((EntityReference)enPayment["bsd_project"]).Id,
                            new ColumnSet(new string[] {
                            "bsd_formno","bsd_serialno"
                            }));
                        TracingSe.Trace("invoice5");
                        string formno = project_invoive.Contains("bsd_formno") ? (string)project_invoive["bsd_formno"] : "";
                        string serialno = project_invoive.Contains("bsd_serialno") ? (string)project_invoive["bsd_serialno"] : "";
                        invoice["bsd_formno"] = formno;
                        invoice["bsd_serialno"] = serialno;
                        decimal vat_amout = Math.Round(d_amp / 11, MidpointRounding.AwayFromZero);
                        TracingSe.Trace("d_amp" + d_amp);
                        TracingSe.Trace("vat_amout" + vat_amout);
                        decimal vat_adj = 0;
                        decimal vat_amount_ho = vat_amout - vat_adj;
                        decimal invoiamountb4 = d_amp - vat_amount_ho;
                        decimal land_value = ((Money)optionentry_invoive["bsd_landvaluededuction"]).Value;
                        decimal vat_Handover_Installment = 0;

                        if (flag == true)
                        {
                            vat_Handover_Installment = Math.Round(land_value / 11, MidpointRounding.AwayFromZero);
                        }
                        TracingSe.Trace("invoice6");
                        invoice["bsd_invoiceamount"] = new Money(d_amp);
                        invoice["bsd_vatamount"] = new Money(vat_amout);
                        invoice["bsd_vatadjamount"] = new Money(vat_Handover_Installment);
                        invoice["bsd_vatamounthandover"] = new Money(vat_amout - vat_Handover_Installment);
                        invoice["bsd_invoiceamountb4vat"] = new Money(invoiamountb4 - vat_amout - vat_Handover_Installment);

                        invoice["statuscode"] = new OptionSetValue(100000000);//Confirm
                        invoice["bsd_taxcode"] = EnTaxcode10.ToEntityReference();
                        invoice["bsd_taxcodevalue"] = EnTaxcode10["bsd_value"];
                        service.Create(invoice);
                        #endregion
                        #region Invoice cho 1st Ins
                        TracingSe.Trace("bolDot1 " + bolDot1);
                        if (InsId1st != new Guid())
                        {
                            Entity Invoice1st = new Entity();
                            Invoice1st = invoice;
                            Invoice1st["bsd_name"] = "Thu tiền đợt 1 của căn hộ " + (string)iv_units["name"];
                            Invoice1st["bsd_depositamount"] = new Money(depositamount);
                            Invoice1st["bsd_type"] = new OptionSetValue(100000003);
                            decimal vat_amout1st = Math.Round(decAmount1st / 11, MidpointRounding.AwayFromZero);
                            TracingSe.Trace("code tạo invoice đợt 1");
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
                            Invoice1st["bsd_taxcodevalue"] = EnTaxcode10["bsd_value"];
                            service.Create(Invoice1st);
                        }
                        #endregion
                        #region Invoice cho Handover
                        if (InsHandoverId != new Guid() && decAmountInsHandover != 0)//trường hợp có cấn trừ ins handover
                        {
                            #region tạo invoice cho ins handover
                            Entity optionentryEn = enPayment.Contains("bsd_optionentry") ? service.Retrieve("salesorder", ((EntityReference)enPayment["bsd_optionentry"]).Id, new ColumnSet(true)) : null;
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


                            //decimal vat_amout = Math.Round((((totalamountwaspaidins * 10 / 11) * 10) / 100), 0);
                            //decimal vat_adj = 0;
                            //decimal vat_amount_ho = vat_amout - vat_adj;
                            //decimal invoiamountb4 = d_amp - vat_amount_ho;
                            //decimal land_value = optionentry_invoive.Contains("bsd_landvaluededuction") ? ((Money)optionentry_invoive["bsd_landvaluededuction"]).Value : 0;
                            //decimal vat_Handover_Installment = 0;
                            bool bolThanhToanDu = enPayment.Contains("bsd_differentamount") && ((Money)enPayment["bsd_differentamount"]).Value >= 0 ? true : false;

                            if (totaltax >= vat_amout + vatamount_lastins + vatHandoverPayment)
                            {
                                TracingSe.Trace("case c >= 44444");
                                if (bolThanhToanDu)
                                {
                                    TracingSe.Trace("case thanh toán đủ và dư");
                                    vat_Handover_Installment = Math.Round(land_value / 11, MidpointRounding.AwayFromZero);
                                    invoice["bsd_name"] = "Giá trị quyền sử dụng đất (không chịu Thuế GTGT) của căn hộ " + (string)iv_units["name"];
                                    invoice["bsd_invoiceamount"] = new Money(amountpay - land_value);
                                    invoice["bsd_type"] = new OptionSetValue(100000000);//Installment
                                    invoice["statuscode"] = new OptionSetValue(100000000);//Confirm
                                    invoice["bsd_vatamount"] = new Money(Math.Round((amountpay - land_value) / 11, MidpointRounding.AwayFromZero));
                                    invoice["bsd_vatadjamount"] = new Money(0);
                                    invoice["bsd_vatamounthandover"] = new Money(Math.Round((amountpay - land_value) / 11, MidpointRounding.AwayFromZero));
                                    invoice["bsd_invoiceamountb4vat"] = new Money(Math.Round(amountpay - land_value - (amountpay - land_value) / 11, MidpointRounding.AwayFromZero));
                                    invoice["bsd_taxcode"] = EnTaxcode10.ToEntityReference();
                                    invoice["bsd_taxcodevalue"] = EnTaxcode10["bsd_value"];
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
                                    invoiceLandValue["bsd_taxcodevalue"] = EnTaxcode_1["bsd_value"];
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
                                    invoiceLast["bsd_taxcodevalue"] = EnTaxcode10["bsd_value"];
                                    service.Create(invoiceLast);

                                    strResult2 = serializer.Serialize(invoiceLast);
                                }
                                else
                                {
                                    TracingSe.Trace("case thanh toán thiếu");
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
                                    strResult3 = serializer.Serialize(invoice);
                                }
                            }
                            else
                            {
                                TracingSe.Trace("case c < 4444444444444444");
                                if (bolThanhToanDu)
                                {
                                    TracingSe.Trace("case thanh toán đủ và dư");
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
                                    invoiceLast["bsd_taxcodevalue"] = EnTaxcode10["bsd_value"];
                                    service.Create(invoiceLast);
                                    strResult2 = serializer.Serialize(invoiceLast);
                                }
                                else
                                {
                                    TracingSe.Trace("case thanh toán thiếu");
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
                                    strResult3 = serializer.Serialize(invoice);
                                }
                            }
                            #endregion
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
                        invoice["bsd_lastinstallmentamount"] = new Money(0);
                        invoice["bsd_lastinstallmentvatamount"] = new Money(0);
                        invoice["bsd_depositamount"] = new Money(0);
                        invoice["transactioncurrencyid"] = enCurrency;
                        Boolean flag = false;
                        decimal fee = 0;
                        decimal amount_lastins = 0;
                        string chuoi_name = "";
                        EntityReference units = enPayment.Contains("bsd_units") ? (EntityReference)enPayment["bsd_units"] : null;
                        DateTime actualtime_iv = enPayment.Contains("bsd_paymentactualtime") ? (DateTime)enPayment["bsd_paymentactualtime"] : new DateTime(0);
                        bool flag2 = false;
                        for (int i = 0; i < arrId_iv.Length; i++)
                        {
                            string[] arr = arrId_iv[i].Split('_');
                            string installmentid = arr[0];
                            string type1 = arr[1];
                            TracingSe.Trace("t.1");
                            //Entity enInstallment = getInstallment(service, installmentid);
                            Entity enInstallment = service.Retrieve("bsd_paymentschemedetail", new Guid(installmentid), new ColumnSet(true)); ;
                            //Boolean f_mains_iv = (enInstallment.Contains("bsd_maintenancefeesstatus")) ? (Boolean)enInstallment["bsd_maintenancefeesstatus"] : false;
                            //Boolean f_manas_iv = (enInstallment.Contains("bsd_managementfeesstatus")) ? (Boolean)enInstallment["bsd_managementfeesstatus"] : false;
                            if (type1 == "main")
                            {
                                create = true;
                                fee += decimal.Parse(arrFeeAmount_iv[i]);
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
                        if (units != null)
                        {
                            Entity iv_units = service.Retrieve(units.LogicalName, units.Id, new ColumnSet(new string[] { "name" }));
                            invoice["bsd_name"] = "Thu tiền Phí bảo trì của căn hộ đợt " + chuoi_name + " của căn hộ " + (string)iv_units["name"]; ;
                        }
                        else
                        {
                            invoice["bsd_name"] = "Thu tiền Phí bảo trì của căn hộ đợt " + chuoi_name;
                        }
                        invoice["bsd_project"] = enPayment.Contains("bsd_project") ? enPayment["bsd_project"] : null;
                        invoice["bsd_optionentry"] = enPayment.Contains("bsd_optionentry") ? enPayment["bsd_optionentry"] : null;
                        invoice["bsd_payment"] = enPayment.ToEntityReference();
                        invoice["bsd_type"] = new OptionSetValue(100000001);
                        EntityReference Pay_Perchaser = enPayment.Contains("bsd_purchaser") ? (EntityReference)enPayment["bsd_purchaser"] : null;
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
                        if (enPayment.Contains("bsd_paymentactualtime"))
                        {
                            DateTime bsd_paymentactualtime = RetrieveLocalTimeFromUTCTime((DateTime)enPayment["bsd_paymentactualtime"]);
                            invoice["bsd_issueddate"] = bsd_paymentactualtime;
                        }
                        invoice["bsd_paymentmethod"] = new OptionSetValue(100000000);
                        Entity optionentry_invoive = service.Retrieve("salesorder", ((EntityReference)enPayment["bsd_optionentry"]).Id,
                              new ColumnSet(new string[] {
                            "bsd_landvaluededuction",
                              }));
                        Entity project_invoive = service.Retrieve("bsd_project", ((EntityReference)enPayment["bsd_project"]).Id,
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
                        decimal handover_ins = 0;
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
                        invoice["bsd_vatamounthandover"] = new Money(vat_adj);
                        invoice["bsd_invoiceamountb4vat"] = new Money(fee - vat_amout - vat_adj);
                        invoice["bsd_taxcode"] = EnTaxcode_1.ToEntityReference();
                        invoice["bsd_taxcodevalue"] = EnTaxcode_1["bsd_value"];
                        if (create == true)
                            service.Create(invoice);
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
                        invoice["bsd_lastinstallmentamount"] = new Money(0);
                        invoice["bsd_lastinstallmentvatamount"] = new Money(0);
                        invoice["bsd_depositamount"] = new Money(0);
                        invoice["transactioncurrencyid"] = enCurrency;
                        Boolean flag = false;
                        decimal fee = 0;
                        decimal amount_lastins = 0;
                        string chuoi_name = "";
                        EntityReference units = (EntityReference)enPayment["bsd_units"];
                        DateTime actualtime_iv = (DateTime)enPayment["bsd_paymentactualtime"];
                        bool flag3 = false;
                        for (int i = 0; i < arrId_iv.Length; i++)
                        {
                            string[] arr = arrId_iv[i].Split('_');
                            string installmentid = arr[0];
                            string type1 = arr[1];
                            TracingSe.Trace("t.1");
                            //Entity enInstallment = getInstallment(service, installmentid);
                            Entity enInstallment = service.Retrieve("bsd_paymentschemedetail", new Guid(installmentid), new ColumnSet(true));
                            //Boolean f_mains_iv = (enInstallment.Contains("bsd_maintenancefeesstatus")) ? (Boolean)enInstallment["bsd_maintenancefeesstatus"] : false;
                            //Boolean f_manas_iv = (enInstallment.Contains("bsd_managementfeesstatus")) ? (Boolean)enInstallment["bsd_managementfeesstatus"] : false;
                            if (type1 == "mana")
                            {
                                create = true;
                                fee += decimal.Parse(arrFeeAmount_iv[i]);
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
                        if (units != null)
                        {
                            Entity iv_units = service.Retrieve(units.LogicalName, units.Id, new ColumnSet(new string[] { "name" }));
                            invoice["bsd_name"] = "Thu tiền Phí quản lý của căn hộ đợt " + chuoi_name + " của căn hộ " + (string)iv_units["name"]; ;
                        }
                        else
                        {
                            invoice["bsd_name"] = "Thu tiền Phí quản lý của căn hộ đợt " + chuoi_name;
                        }

                        invoice["bsd_project"] = enPayment.Contains("bsd_project") ? enPayment["bsd_project"] : null;
                        invoice["bsd_optionentry"] = enPayment.Contains("bsd_optionentry") ? enPayment["bsd_optionentry"] : 0;
                        invoice["bsd_payment"] = enPayment.ToEntityReference();
                        invoice["bsd_type"] = new OptionSetValue(100000002);
                        EntityReference Pay_Perchaser = enPayment.Contains("bsd_purchaser") ? (EntityReference)enPayment["bsd_purchaser"] : null;
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
                        if (enPayment.Contains("bsd_paymentactualtime"))
                        {
                            DateTime bsd_paymentactualtime = RetrieveLocalTimeFromUTCTime((DateTime)enPayment["bsd_paymentactualtime"]);
                            invoice["bsd_issueddate"] = bsd_paymentactualtime;
                        }
                        invoice["bsd_paymentmethod"] = new OptionSetValue(100000000);
                        Entity optionentry_invoive = service.Retrieve("salesorder", ((EntityReference)enPayment["bsd_optionentry"]).Id,
                              new ColumnSet(new string[] {
                            "bsd_landvaluededuction",
                              }));
                        Entity project_invoive = service.Retrieve("bsd_project", ((EntityReference)enPayment["bsd_project"]).Id,
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
                        decimal handover_ins = 0;
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
                        invoice["bsd_vatamounthandover"] = new Money(vat_adj);
                        invoice["bsd_invoiceamountb4vat"] = new Money(fee - vat_amout - vat_adj);
                        invoice["bsd_taxcode"] = EnTaxcode10.ToEntityReference();
                        invoice["bsd_taxcodevalue"] = EnTaxcode10["bsd_value"];
                        if (create == true)
                            service.Create(invoice);
                        #endregion
                    }

                    #endregion thành
                }
            }
            catch (Exception ex)
            {
                TracingSe.Trace(ex.ToString());
                throw new InvalidPluginExecutionException(ex.ToString());
            }

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
