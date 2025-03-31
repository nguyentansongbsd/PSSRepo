using Microsoft.Crm.Sdk.Messages;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Query;
using System;
using System.Collections.ObjectModel;
using System.Text;
using System.Web.Script.Serialization;

namespace Action_ApplyDocument
{
    public class Action_ApplyDocument : IPlugin
    {
        private IOrganizationService service = (IOrganizationService)null;
        private IOrganizationServiceFactory factory = (IOrganizationServiceFactory)null;
        private ApplyDocument applyDocument;
        private StringBuilder strMess = new StringBuilder();

        void IPlugin.Execute(IServiceProvider serviceProvider)
        {
            IPluginExecutionContext service = (IPluginExecutionContext)serviceProvider.GetService(typeof(IPluginExecutionContext));
            EntityReference inputParameter = (EntityReference)service.InputParameters["Target"];
            this.factory = (IOrganizationServiceFactory)serviceProvider.GetService(typeof(IOrganizationServiceFactory));
            this.service = this.factory.CreateOrganizationService(new Guid?(service.UserId));
            this.applyDocument = new ApplyDocument(serviceProvider);
            if (!(inputParameter.LogicalName == "bsd_applydocument"))
                return;
            Entity entity = this.service.Retrieve(inputParameter.LogicalName, inputParameter.Id, new ColumnSet(true));
            int num = entity.Contains("bsd_transactiontype") ? ((OptionSetValue)entity["bsd_transactiontype"]).Value : 0;
            this.strMess.AppendLine("chuẩn bị check_DueDate_Installment");
            if (num == 2)
                this.check_DueDate_Installment(this.service, entity);
            this.applyDocument.checkInput(entity);
            DateTime d_now = this.applyDocument.RetrieveLocalTimeFromUTCTime(DateTime.Now);
            this.strMess.AppendLine(d_now.ToString());
            this.strMess.AppendLine("Apply bsd_transactiontype: " + num.ToString());
            string str1 = entity.Contains("bsd_arraypsdid") ? (string)entity["bsd_arraypsdid"] : "";
            string str2 = entity.Contains("bsd_arrayamountpay") ? (string)entity["bsd_arrayamountpay"] : "";
            this.strMess.AppendLine("99999999999");
            if (num == 1)
            {
                string[] strArray1 = str1.Split(',');
                string[] strArray2 = str2.Split(',');
                int length = strArray1.Length;
                for (int index = 0; index < length; ++index)
                    this.applyDocument.paymentDeposit(Guid.Parse(strArray1[index]), Decimal.Parse(strArray2[index]), d_now, entity);
            }
            this.strMess.AppendLine("i_bsd_transactiontype = installment");
            if (num == 2)
                this.applyDocument.paymentInstallment(entity);
            this.strMess.AppendLine("createCOA");
            this.applyDocument.createCOA(entity);
            this.strMess.AppendLine("createCOA done");
            this.strMess.AppendLine("1000000000000");
            this.applyDocument.updateApplyDocument(entity);
            if (num == 2)
                this.service.Execute(new OrganizationRequest("bsd_Action_Create_Invoice")
                {
                    ["EnCallFrom"] = (object)entity
                });
        }

        public void check_DueDate_Installment(IOrganizationService service, Entity applydocument)
        {
            try
            {
                if (!applydocument.Contains("bsd_arraypsdid"))
                    return;
                string str1 = applydocument["bsd_arraypsdid"].ToString();
                if (str1 != "")
                {
                    string str2 = str1;
                    char[] chArray = new char[1] { ',' };
                    foreach (string g in str2.Split(chArray))
                    {
                        Guid id = new Guid(g);
                        Entity entity = service.Retrieve("bsd_paymentschemedetail", id, new ColumnSet(new string[2]
                        {
              "bsd_name",
              "bsd_duedate"
                        }));
                        if (!entity.Contains("bsd_duedate"))
                            throw new Exception("The Installment you are paying has no due date. Please update due date before confirming payment! " + entity["bsd_name"].ToString());
                    }
                }
            }
            catch (Exception ex)
            {
                throw new InvalidPluginExecutionException(string.Format("{0}", (object)ex.Message));
            }
        }

        public void processPayment(Entity ApplyDoc)
        {
            bool flag1 = false;
            string str1 = "";
            string str2 = "";
            string str3 = "";
            JavaScriptSerializer scriptSerializer = new JavaScriptSerializer();
            this.strMess.AppendLine("vào invoice và type = ");
            this.strMess.AppendLine("vào check");
            string str4 = ApplyDoc.Contains("bsd_arraypsdid") ? (string)ApplyDoc["bsd_arraypsdid"] : "";
            string str5 = ApplyDoc.Contains("bsd_arrayamountpay") ? (string)ApplyDoc["bsd_arrayamountpay"] : "";
            EntityReference entityReference1 = ApplyDoc.Contains("bsd_paymentschemedetail") ? (EntityReference)ApplyDoc["bsd_paymentschemedetail"] : (EntityReference)null;
            string str6 = ApplyDoc.Contains("bsd_arrayfeesid") ? (string)ApplyDoc["bsd_arrayfeesid"] : "";
            string str7 = ApplyDoc.Contains("bsd_arrayfeesamount") ? (string)ApplyDoc["bsd_arrayfeesamount"] : "";
            Guid guid = new Guid();
            EntityCollection entityCollection1 = this.service.RetrieveMultiple((QueryBase)new FetchExpression(string.Format("\n                <fetch>\n                  <entity name='bsd_taxcode'>\n                    <attribute name='bsd_name' />\n                    <attribute name='bsd_value' />\n                    <filter type='or'>\n                      <condition attribute='bsd_name' operator='eq' value='{0}'/>\n                      <condition attribute='bsd_name' operator='eq' value='{1}'/>\n                    </filter>\n                    <order attribute='createdon' descending='true' />\n                  </entity>\n                </fetch>", (object)10, (object)-1).ToString()));
            Entity entity1 = (Entity)null;
            Entity entity2 = (Entity)null;
            foreach (Entity entity3 in (Collection<Entity>)entityCollection1.Entities)
            {
                if (entity3.Contains("bsd_name") && entity3["bsd_name"].ToString() == "-1" && entity1 == null)
                    entity1 = entity3;
                else if (entity3.Contains("bsd_name") && entity3["bsd_name"].ToString() == "10" && entity2 == null)
                    entity2 = entity3;
            }
            if (str4 != "")
            {
                bool flag2 = false;
                bool flag3 = false;
                string[] strArray1 = new string[0];
                string[] strArray2 = new string[0];
                Decimal num1 = 0M;
                Decimal num2 = 0M;
                Decimal num3 = 0M;
                Decimal num4 = 0M;
                Guid id = new Guid();
                this.strMess.AppendLine("s_bsd_arraypsdid " + str4);
                this.strMess.AppendLine("s_bsd_arrayamountpay " + str5);
                if (str4 != "")
                {
                    strArray1 = str4.Split(',');
                    strArray2 = str5.Split(',');
                }
                this.strMess.AppendLine("invoice1");
                EntityCollection ecIns = this.get_ecINS(this.service, strArray1);
                if (ecIns.Entities.Count <= 0)
                    throw new InvalidPluginExecutionException("Cannot find any Installment from Installment list. Please check again!");
                for (int index = 0; index < strArray2.Length; ++index)
                    num1 += Decimal.Parse(strArray2[index]);
                this.strMess.AppendLine("d_amp" + num1.ToString());
                string str8 = "";
                EntityReference entityReference2 = (EntityReference)ApplyDoc["bsd_units"];
                this.strMess.AppendLine("invoice2");
                DateTime utcTime = ApplyDoc.Contains("bsd_receiptdate") ? (DateTime)ApplyDoc["bsd_receiptdate"] : new DateTime();
                this.strMess.AppendLine("invoice3");
                Entity entity4 = new Entity("bsd_invoice");
                entity4["bsd_type"] = (object)new OptionSetValue(100000000);
                entity4["bsd_lastinstallmentamount"] = (object)new Money(0M);
                entity4["bsd_lastinstallmentvatamount"] = (object)new Money(0M);
                entity4["bsd_depositamount"] = (object)new Money(0M);
                entity4.Id = new Guid();
                foreach (Entity entity5 in (Collection<Entity>)ecIns.Entities)
                {
                    Entity entity6 = this.service.Retrieve("bsd_paymentschemedetail", entity5.Id, new ColumnSet(true));
                    flag1 = entity6.Contains("bsd_ordernumber") && entity6["bsd_ordernumber"].ToString() == "1";
                    if (((!entity6.Contains("statuscode") ? 0 : (((OptionSetValue)entity6["statuscode"]).Value == 100000000 ? 1 : 0)) & (flag1 ? 1 : 0)) == 0 && (!entity6.Contains("bsd_lastinstallment") || !(bool)entity6["bsd_lastinstallment"]))
                    {
                        if (flag1)
                        {
                            guid = entity6.Id;
                            num4 = entity6.Contains("bsd_depositamount") ? ((Money)entity6["bsd_depositamount"]).Value : 0M;
                            num3 = ((Money)entity6["bsd_amountwaspaid"]).Value + num4;
                            num1 -= ((Money)entity6["bsd_amountwaspaid"]).Value;
                        }
                        else
                        {
                            int num5 = entity5.Contains("bsd_duedatecalculatingmethod") ? ((OptionSetValue)entity5["bsd_duedatecalculatingmethod"]).Value : 0;
                            if (num5 == 100000002)
                            {
                                num1 -= ((Money)entity6["bsd_amountwaspaid"]).Value;
                                id = entity5.Id;
                                num2 = ((Money)entity5["bsd_amountofthisphase"]).Value;
                            }
                            int num6 = entity5.Contains("bsd_ordernumber") ? (int)entity5["bsd_ordernumber"] : 0;
                            if (!flag3)
                            {
                                if (num5 != 100000002)
                                    str8 += num6.ToString();
                            }
                            else if (num5 != 100000002)
                                str8 = str8 + " + " + num6.ToString();
                            flag3 = true;
                        }
                    }
                }
                Decimal num7 = num1;
                Entity entity7 = this.service.Retrieve(entityReference2.LogicalName, entityReference2.Id, new ColumnSet(new string[1]
                {
          "name"
                }));
                entity4["bsd_name"] = (object)("Thu tiền đợt " + str8 + " của căn hộ " + (string)entity7["name"]);
                entity4["bsd_project"] = ApplyDoc.Contains("bsd_project") ? ApplyDoc["bsd_project"] : (object)null;
                entity4["bsd_optionentry"] = ApplyDoc.Contains("bsd_optionentry") ? ApplyDoc["bsd_optionentry"] : (object)0;
                entity4["bsd_applydocument"] = (object)ApplyDoc.ToEntityReference();
                entity4["bsd_issueddate"] = (object)utcTime;
                entity4["bsd_units"] = (object)entityReference2;
                this.strMess.AppendLine("invoice4");
                EntityReference entityReference3 = ApplyDoc.Contains("bsd_customer") ? (EntityReference)ApplyDoc["bsd_customer"] : (EntityReference)null;
                entity4["bsd_purchaser"] = (object)entityReference3;
                if (entityReference3 != null)
                {
                    if (this.service.Retrieve(entityReference3.LogicalName, entityReference3.Id, new ColumnSet(true)).LogicalName == "contact")
                        entity4["bsd_purchasernamecustomer"] = (object)entityReference3;
                    else
                        entity4["bsd_purchasernamecompany"] = (object)entityReference3;
                }
                if (utcTime != new DateTime())
                {
                    DateTime dateTime = this.RetrieveLocalTimeFromUTCTime(utcTime);
                    entity4["bsd_issueddate"] = (object)dateTime;
                }
                entity4["bsd_paymentmethod"] = (object)new OptionSetValue(100000000);
                Entity optionentry = this.service.Retrieve("salesorder", ((EntityReference)ApplyDoc["bsd_optionentry"]).Id, new ColumnSet(new string[1]
                {
          "bsd_landvaluededuction"
                }));
                Entity entity8 = this.service.Retrieve("bsd_project", ((EntityReference)ApplyDoc["bsd_project"]).Id, new ColumnSet(new string[2]
                {
          "bsd_formno",
          "bsd_serialno"
                }));
                this.strMess.AppendLine("invoice5");
                string str9 = entity8.Contains("bsd_formno") ? (string)entity8["bsd_formno"] : "";
                string str10 = entity8.Contains("bsd_serialno") ? (string)entity8["bsd_serialno"] : "";
                entity4["bsd_formno"] = (object)str9;
                entity4["bsd_serialno"] = (object)str10;
                Decimal num8 = Math.Round(num1 / 11M, MidpointRounding.AwayFromZero);
                this.strMess.AppendLine("d_amp" + num1.ToString());
                this.strMess.AppendLine("vat_amout" + num8.ToString());
                Decimal num9 = 0M;
                Decimal num10 = num8 - num9;
                Decimal num11 = num1 - num10;
                Decimal num12 = ((Money)optionentry["bsd_landvaluededuction"]).Value;
                Decimal num13 = 0M;
                if (flag2)
                    num13 = Math.Round(num12 / 11M, MidpointRounding.AwayFromZero);
                this.strMess.AppendLine("invoice6");
                entity4["bsd_invoiceamount"] = (object)new Money(num1);
                entity4["bsd_vatamount"] = (object)new Money(num8);
                entity4["bsd_vatadjamount"] = (object)new Money(num13);
                entity4["bsd_vatamounthandover"] = (object)new Money(num8 - num13);
                entity4["bsd_invoiceamountb4vat"] = (object)new Money(num11 - num8 - num13);
                entity4["statuscode"] = (object)new OptionSetValue(100000000);
                entity4["bsd_taxcode"] = (object)entity2.ToEntityReference();
                entity4["bsd_taxcodevalue"] = (object)(Decimal)entity2["bsd_value"];
                if (str8 != "")
                    this.service.Create(entity4);
                this.strMess.AppendLine("bolDot1 " + flag1.ToString());
                if (guid != new Guid())
                {
                    Entity entity9 = new Entity();
                    Entity entity10 = entity4;
                    entity10["bsd_name"] = (object)("Thu tiền đợt 1 của căn hộ " + (string)entity7["name"]);
                    entity10["bsd_depositamount"] = (object)new Money(num4);
                    entity10["bsd_type"] = (object)new OptionSetValue(100000003);
                    Decimal num14 = Math.Round(num3 / 11M, MidpointRounding.AwayFromZero);
                    this.strMess.AppendLine("code tạo invoice đợt 1");
                    Decimal num15 = 0M;
                    Decimal num16 = num14 - num15;
                    Decimal num17 = num3 - num16;
                    Decimal num18 = ((Money)optionentry["bsd_landvaluededuction"]).Value;
                    Decimal num19 = 0M;
                    entity10["bsd_invoiceamount"] = (object)new Money(num3);
                    entity10["bsd_vatamount"] = (object)new Money(num14);
                    entity10["bsd_vatadjamount"] = (object)new Money(num19);
                    entity10["bsd_vatamounthandover"] = (object)new Money(num14 - num19);
                    entity10["bsd_invoiceamountb4vat"] = (object)new Money(num11 - num14 - num19);
                    entity10["bsd_taxcode"] = (object)entity2.ToEntityReference();
                    entity10["bsd_taxcodevalue"] = (object)(Decimal)entity2["bsd_value"];
                    this.service.Create(entity10);
                }
                if (id != new Guid() && num2 != 0M)
                {
                    Entity entity11 = ApplyDoc.Contains("bsd_optionentry") ? this.service.Retrieve("salesorder", ((EntityReference)ApplyDoc["bsd_optionentry"]).Id, new ColumnSet(true)) : (Entity)null;
                    Decimal num20 = 0M;
                    Decimal num21 = 0M;
                    if (entity11 != null)
                    {
                        num20 = entity11.Contains("totaltax") ? ((Money)entity11["totaltax"]).Value : 0M;
                        EntityCollection entityCollection2 = this.service.RetrieveMultiple((QueryBase)new FetchExpression(string.Format("\n                            <fetch aggregate='true'>\n                              <entity name='bsd_paymentschemedetail'>\n                                <attribute name='bsd_amountwaspaid' alias='totalamountwaspaidins' aggregate='sum' />\n                                <filter type='and'>\n                                  <condition attribute='bsd_optionentry' operator='eq' value='{0}'/>\n                                  <condition attribute='bsd_lastinstallment' operator='neq' value='{1}'/>\n                                  <condition attribute='statecode' operator='eq' value='{2}'/>\n                                  <condition attribute='bsd_duedatecalculatingmethod' operator='neq' value='{3}'/>\n                                </filter>\n                              </entity>\n                            </fetch>", (object)entity11.Id, (object)1, (object)0, (object)100000002)));
                        if (entityCollection2 != null && entityCollection2.Entities.Count > 0)
                            num21 = ((Money)((AliasedValue)entityCollection2[0]["totalamountwaspaidins"]).Value).Value;
                    }
                    Entity lastInstallment = this.getLastInstallment(this.service, optionentry);
                    Decimal num22 = lastInstallment.Contains("bsd_amountofthisphase") ? ((Money)lastInstallment["bsd_amountofthisphase"]).Value : 0M;
                    Decimal num23 = Math.Round(num22 / 11M, MidpointRounding.AwayFromZero);
                    Entity entity12 = this.service.Retrieve("bsd_paymentschemedetail", id, new ColumnSet(true));
                    Decimal num24 = Math.Round((entity12.Contains("bsd_amountwaspaid") ? ((Money)entity12["bsd_amountwaspaid"]).Value : 0M) / 11M, MidpointRounding.AwayFromZero);
                    int index = Array.IndexOf<string>(strArray1, id.ToString());
                    Decimal num25 = Decimal.Parse(strArray2[index]);
                    Decimal num26 = Math.Round(num25 / 11M, MidpointRounding.AwayFromZero);
                    this.strMess.AppendLine("Index của handover: " + index.ToString());
                    string str11 = (entity12.Contains("bsd_ordernumber") ? (int)entity12["bsd_ordernumber"] : 0).ToString();
                    bool flag4 = entity12.Contains("statuscode") && ((OptionSetValue)entity12["statuscode"]).Value == 100000001;
                    this.strMess.AppendLine("totaltax " + num20.ToString());
                    this.strMess.AppendLine("vat_amout " + num26.ToString());
                    this.strMess.AppendLine("vatamount_lastins " + num23.ToString());
                    this.strMess.AppendLine("vatHandoverPayment " + num24.ToString());
                    if (num20 >= Math.Round(num21 / 11M, MidpointRounding.AwayFromZero) + num23 + num24)
                    {
                        this.strMess.AppendLine("case c >= 44444");
                        if (flag4)
                        {
                            this.strMess.AppendLine("case thanh toán đủ và dư");
                            Math.Round(num12 / 11M, MidpointRounding.AwayFromZero);
                            entity4["bsd_name"] = (object)("Thu tiền đợt " + str11 + " của căn hộ " + (string)entity7["name"]);
                            entity4["bsd_invoiceamount"] = (object)new Money(num7 - num12);
                            entity4["bsd_type"] = (object)new OptionSetValue(100000000);
                            entity4["statuscode"] = (object)new OptionSetValue(100000000);
                            entity4["bsd_vatamount"] = (object)new Money(Math.Round((num7 - num12) / 11M, MidpointRounding.AwayFromZero));
                            entity4["bsd_vatadjamount"] = (object)new Money(0M);
                            entity4["bsd_vatamounthandover"] = (object)new Money(Math.Round((num7 - num12) / 11M, MidpointRounding.AwayFromZero));
                            entity4["bsd_invoiceamountb4vat"] = (object)new Money(Math.Round(num7 - num12 - (num7 - num12) / 11M, MidpointRounding.AwayFromZero));
                            entity4["bsd_taxcode"] = (object)entity2.ToEntityReference();
                            entity4["bsd_taxcodevalue"] = (object)(Decimal)entity2["bsd_value"];
                            this.service.Create(entity4);
                            str3 = scriptSerializer.Serialize((object)entity4);
                            Entity entity13 = new Entity("bsd_invoice");
                            Entity entity14 = entity4;
                            entity14["bsd_name"] = (object)("Giá trị quyền sử dụng đất (không chịu Thuế GTGT) của căn hộ " + (string)entity7["name"]);
                            entity14["bsd_invoiceamount"] = (object)new Money(num12);
                            entity14["bsd_type"] = (object)new OptionSetValue(100000006);
                            entity14["statuscode"] = (object)new OptionSetValue(100000000);
                            entity14["bsd_vatamount"] = (object)new Money(0M);
                            entity14["bsd_vatadjamount"] = (object)new Money(0M);
                            entity14["bsd_vatamounthandover"] = (object)new Money(0M);
                            entity14["bsd_invoiceamountb4vat"] = (object)new Money(num12);
                            entity14["bsd_taxcode"] = (object)entity1.ToEntityReference();
                            entity14["bsd_taxcodevalue"] = (object)(Decimal)entity1["bsd_value"];
                            this.service.Create(entity14);
                            str1 = scriptSerializer.Serialize((object)entity14);
                            Entity entity15 = new Entity("bsd_invoice");
                            Entity entity16 = entity4;
                            entity16["bsd_name"] = (object)("5% giá trị của căn hộ " + (string)entity7["name"] + " (Đợt cuối)");
                            entity16["bsd_invoiceamount"] = (object)new Money(num22);
                            entity16["bsd_type"] = (object)new OptionSetValue(100000004);
                            entity16["statuscode"] = (object)new OptionSetValue(100000000);
                            entity16["bsd_vatamount"] = (object)new Money(num23);
                            entity16["bsd_vatadjamount"] = (object)new Money(0M);
                            entity16["bsd_vatamounthandover"] = (object)new Money(num23);
                            entity16["bsd_invoiceamountb4vat"] = (object)new Money(num22 - num23);
                            entity16["bsd_taxcode"] = (object)entity2.ToEntityReference();
                            entity16["bsd_taxcodevalue"] = (object)(Decimal)entity2["bsd_value"];
                            this.service.Create(entity16);
                            str2 = scriptSerializer.Serialize((object)entity16);
                        }
                        else
                        {
                            this.strMess.AppendLine("case thanh toán thiếu");
                            Decimal num27 = 0M;
                            entity4["bsd_name"] = (object)("Thu tiền đợt " + str11 + " của căn hộ " + (string)entity7["name"]);
                            entity4["bsd_type"] = (object)new OptionSetValue(100000000);
                            entity4["statuscode"] = (object)new OptionSetValue(100000000);
                            entity4["bsd_invoiceamount"] = (object)new Money(num25);
                            entity4["bsd_vatamount"] = (object)new Money(num26);
                            entity4["bsd_vatadjamount"] = (object)new Money(num27);
                            entity4["bsd_vatamounthandover"] = (object)new Money(num26 - num27);
                            entity4["bsd_invoiceamountb4vat"] = (object)new Money(num25 - num26 - num27);
                            entity4["bsd_taxcode"] = (object)entity2.ToEntityReference();
                            entity4["bsd_taxcodevalue"] = (object)(Decimal)entity2["bsd_value"];
                            this.service.Create(entity4);
                            str3 = scriptSerializer.Serialize((object)entity4);
                        }
                    }
                    else
                    {
                        this.strMess.AppendLine("case c < 4444444444444444");
                        if (flag4)
                        {
                            this.strMess.AppendLine("case thanh toán đủ và dư");
                            entity4["bsd_name"] = (object)("Giá trị quyền sử dụng đất (không chịu Thuế GTGT) của căn hộ " + (string)entity7["name"]);
                            entity4["bsd_type"] = (object)new OptionSetValue(100000005);
                            entity4["statuscode"] = (object)new OptionSetValue(100000000);
                            entity4["bsd_invoiceamount"] = (object)new Money(num25);
                            entity4["bsd_vatamount"] = (object)new Money(num26);
                            entity4["bsd_vatadjamount"] = (object)new Money(num13);
                            entity4["bsd_vatamounthandover"] = (object)new Money(num26 - num13);
                            entity4["bsd_invoiceamountb4vat"] = (object)new Money(num25 - num26 - num13);
                            entity4["bsd_taxcode"] = (object)entity1.ToEntityReference();
                            entity4["bsd_taxcodevalue"] = (object)(Decimal)entity1["bsd_value"];
                            this.service.Create(entity4);
                            str1 = scriptSerializer.Serialize((object)entity4);
                            Entity entity17 = new Entity("bsd_invoice");
                            Entity entity18 = entity4;
                            entity18["bsd_name"] = (object)("5% giá trị của căn hộ " + (string)entity7["name"] + " (Đợt cuối)");
                            entity18["bsd_invoiceamount"] = (object)new Money(num22);
                            entity18["bsd_type"] = (object)new OptionSetValue(100000004);
                            entity18["statuscode"] = (object)new OptionSetValue(100000000);
                            entity18["bsd_vatamount"] = (object)new Money(num23);
                            entity18["bsd_vatadjamount"] = (object)new Money(0M);
                            entity18["bsd_vatamounthandover"] = (object)new Money(num23);
                            entity18["bsd_invoiceamountb4vat"] = (object)new Money(num22 - num23);
                            entity18["bsd_taxcode"] = (object)entity2.ToEntityReference();
                            entity18["bsd_taxcodevalue"] = (object)(Decimal)entity2["bsd_value"];
                            this.service.Create(entity18);
                            str2 = scriptSerializer.Serialize((object)entity18);
                        }
                        else
                        {
                            this.strMess.AppendLine("case thanh toán thiếu");
                            Decimal num28 = 0M;
                            entity4["bsd_name"] = (object)("Giá trị quyền sử dụng đất (không chịu Thuế GTGT) của căn hộ " + (string)entity7["name"]);
                            entity4["bsd_type"] = (object)new OptionSetValue(100000005);
                            entity4["statuscode"] = (object)new OptionSetValue(100000000);
                            entity4["bsd_invoiceamount"] = (object)new Money(num25);
                            entity4["bsd_vatamount"] = (object)new Money(num26);
                            entity4["bsd_vatadjamount"] = (object)new Money(num28);
                            entity4["bsd_vatamounthandover"] = (object)new Money(num26 - num28);
                            entity4["bsd_invoiceamountb4vat"] = (object)new Money(num25 - num26 - num28);
                            entity4["bsd_taxcode"] = (object)entity1.ToEntityReference();
                            entity4["bsd_taxcodevalue"] = (object)(Decimal)entity1["bsd_value"];
                            this.service.Create(entity4);
                            str3 = scriptSerializer.Serialize((object)entity4);
                        }
                    }
                }
            }
            bool flag5;
            if (str6 != "")
            {
                string[] strArray3 = str6.Split(',');
                str7.Split(',');
                this.strMess.AppendLine("IV_arrayfees");
                bool flag6 = false;
                Entity entity19 = new Entity("bsd_invoice");
                entity19.Id = new Guid();
                entity19["bsd_type"] = (object)new OptionSetValue(100000001);
                entity19["bsd_lastinstallmentamount"] = (object)new Money(0M);
                entity19["bsd_lastinstallmentvatamount"] = (object)new Money(0M);
                entity19["bsd_depositamount"] = (object)new Money(0M);
                bool flag7 = false;
                Decimal num29 = 0M;
                Decimal num30 = 0M;
                string str12 = "";
                EntityReference entityReference4 = ApplyDoc.Contains("bsd_units") ? (EntityReference)ApplyDoc["bsd_units"] : (EntityReference)null;
                DateTime utcTime = ApplyDoc.Contains("bsd_receiptdate") ? (DateTime)ApplyDoc["bsd_receiptdate"] : new DateTime();
                bool flag8 = false;
                for (int index = 0; index < strArray3.Length; ++index)
                {
                    string[] strArray4 = strArray3[index].Split('_');
                    string g = strArray4[0];
                    string str13 = strArray4[1];
                    this.strMess.AppendLine("t.1");
                    Entity entity20 = this.service.Retrieve("bsd_paymentschemedetail", new Guid(g), new ColumnSet(true));
                    bool flag9 = entity20.Contains("bsd_ordernumber") && entity20["bsd_ordernumber"].ToString() == "1";
                    if (((!entity20.Contains("bsd_maintenancefeesstatus") ? 0 : ((bool)entity20["bsd_maintenancefeesstatus"] ? 1 : 0)) & (flag9 ? 1 : 0)) != 0)
                        this.strMess.AppendLine("t.2");
                    else if (entity20.Contains("bsd_lastinstallment") && (bool)entity20["bsd_lastinstallment"])
                    {
                        this.strMess.AppendLine("t.3");
                    }
                    else
                    {
                        StringBuilder strMess = this.strMess;
                        flag5 = (bool)entity20["bsd_maintenancefeesstatus"];
                        string str14 = "statuscode: " + flag5.ToString();
                        strMess.AppendLine(str14);
                        if (str13 == "main" && entity20.Contains("bsd_maintenancefeesstatus") && (bool)entity20["bsd_maintenancefeesstatus"])
                        {
                            this.strMess.AppendLine("t.4");
                            if (flag9)
                            {
                                Decimal num31 = entity20.Contains("bsd_depositamount") ? ((Money)entity20["bsd_depositamount"]).Value : 0M;
                                num29 += num31;
                                entity19["bsd_depositamount"] = (object)new Money(num31);
                                entity19["bsd_type"] = (object)new OptionSetValue(100000003);
                            }
                            flag6 = true;
                            num29 += ((Money)entity20["bsd_maintenanceamount"]).Value;
                            int num32 = entity20.Contains("bsd_ordernumber") ? (int)entity20["bsd_ordernumber"] : 0;
                            if ((entity20.Contains("bsd_duedatecalculatingmethod") ? ((OptionSetValue)entity20["bsd_duedatecalculatingmethod"]).Value : 0) == 100000002)
                            {
                                flag7 = true;
                                num30 = entity20.Contains("bsd_amountofthisphase") ? ((Money)entity20["bsd_amountofthisphase"]).Value : 0M;
                            }
                            str12 = !flag8 ? str12 + num32.ToString() : str12 + " + " + num32.ToString();
                            flag8 = true;
                        }
                    }
                }
                this.strMess.AppendLine("chuoi_name " + str12);
                if (str12 != "")
                {
                    if (entityReference4 != null)
                    {
                        Entity entity21 = this.service.Retrieve(entityReference4.LogicalName, entityReference4.Id, new ColumnSet(new string[1]
                        {
              "name"
                        }));
                        entity19["bsd_name"] = (object)("Thu tiền Phí bảo trì của căn hộ đợt " + str12 + " của căn hộ " + (string)entity21["name"]);
                    }
                    else
                        entity19["bsd_name"] = (object)("Thu tiền Phí bảo trì của căn hộ đợt " + str12);
                    entity19["bsd_project"] = ApplyDoc.Contains("bsd_project") ? ApplyDoc["bsd_project"] : (object)null;
                    entity19["bsd_optionentry"] = ApplyDoc.Contains("bsd_optionentry") ? ApplyDoc["bsd_optionentry"] : (object)null;
                    entity19["bsd_applydocument"] = (object)ApplyDoc.ToEntityReference();
                    EntityReference entityReference5 = ApplyDoc.Contains("bsd_customer") ? (EntityReference)ApplyDoc["bsd_customer"] : (EntityReference)null;
                    entity19["bsd_purchaser"] = (object)entityReference5;
                    if (entityReference5 != null)
                    {
                        if (this.service.Retrieve(entityReference5.LogicalName, entityReference5.Id, new ColumnSet(true)).LogicalName == "contact")
                            entity19["bsd_purchasernamecustomer"] = (object)entityReference5;
                        else
                            entity19["bsd_purchasernamecompany"] = (object)entityReference5;
                    }
                    if (utcTime != new DateTime())
                    {
                        DateTime dateTime = this.RetrieveLocalTimeFromUTCTime(utcTime);
                        entity19["bsd_issueddate"] = (object)dateTime;
                    }
                    entity19["bsd_paymentmethod"] = (object)new OptionSetValue(100000000);
                    Entity entity22 = this.service.Retrieve("salesorder", ((EntityReference)ApplyDoc["bsd_optionentry"]).Id, new ColumnSet(new string[1]
                    {
            "bsd_landvaluededuction"
                    }));
                    Entity entity23 = this.service.Retrieve("bsd_project", ((EntityReference)ApplyDoc["bsd_project"]).Id, new ColumnSet(new string[2]
                    {
            "bsd_formno",
            "bsd_serialno"
                    }));
                    string str15 = entity23.Contains("bsd_formno") ? (string)entity23["bsd_formno"] : "";
                    string str16 = entity23.Contains("bsd_serialno") ? (string)entity23["bsd_serialno"] : "";
                    entity19["bsd_formno"] = (object)str15;
                    entity19["bsd_serialno"] = (object)str16;
                    entity19["bsd_issueddate"] = (object)utcTime;
                    entity19["bsd_units"] = (object)entityReference4;
                    Decimal num33 = Math.Round(num29 / 11M, MidpointRounding.AwayFromZero) - 0M;
                    Decimal num34 = num29 - num33;
                    Decimal num35 = entity22.Contains("bsd_landvaluededuction") ? ((Money)entity22["bsd_landvaluededuction"]).Value : 0M;
                    Decimal num36 = 0M;
                    if (flag7)
                        num36 = Math.Round(num35 / 11M, MidpointRounding.AwayFromZero);
                    entity19["bsd_invoiceamount"] = (object)new Money(num29);
                    Decimal num37 = 0M;
                    Decimal num38 = 0M;
                    entity19["bsd_vatamount"] = (object)new Money(num37);
                    entity19["bsd_vatadjamount"] = (object)new Money(num38);
                    entity19["bsd_vatamounthandover"] = (object)new Money(num38);
                    entity19["bsd_invoiceamountb4vat"] = (object)new Money(num29 - num37 - num38);
                    entity19["bsd_taxcode"] = (object)entity1.ToEntityReference();
                    entity19["bsd_taxcodevalue"] = (object)(Decimal)entity1["bsd_value"];
                    if (flag6)
                    {
                        this.strMess.AppendLine("tạo ra invoice 1");
                        this.service.Create(entity19);
                    }
                }
            }
            if (str6 != "")
            {
                string[] strArray5 = str6.Split(',');
                str7.Split(',');
                bool flag10 = false;
                Entity entity24 = new Entity("bsd_invoice");
                entity24.Id = new Guid();
                entity24["bsd_type"] = (object)new OptionSetValue(100000002);
                entity24["bsd_lastinstallmentamount"] = (object)new Money(0M);
                entity24["bsd_lastinstallmentvatamount"] = (object)new Money(0M);
                entity24["bsd_depositamount"] = (object)new Money(0M);
                this.strMess.AppendLine("IV_arrayfees 2");
                bool flag11 = false;
                Decimal num39 = 0M;
                Decimal num40 = 0M;
                string str17 = "";
                EntityReference entityReference6 = (EntityReference)ApplyDoc["bsd_units"];
                DateTime utcTime = ApplyDoc.Contains("bsd_receiptdate") ? (DateTime)ApplyDoc["bsd_receiptdate"] : new DateTime();
                bool flag12 = false;
                for (int index = 0; index < strArray5.Length; ++index)
                {
                    string[] strArray6 = strArray5[index].Split('_');
                    string g = strArray6[0];
                    string str18 = strArray6[1];
                    this.strMess.AppendLine("t.1");
                    Entity entity25 = this.service.Retrieve("bsd_paymentschemedetail", new Guid(g), new ColumnSet(true));
                    bool flag13 = entity25.Contains("bsd_ordernumber") && entity25["bsd_ordernumber"].ToString() == "1";
                    if (((!entity25.Contains("bsd_managementfeesstatus") ? 0 : ((bool)entity25["bsd_managementfeesstatus"] ? 1 : 0)) & (flag13 ? 1 : 0)) != 0)
                        this.strMess.AppendLine("t.2");
                    else if (entity25.Contains("bsd_lastinstallment") && (bool)entity25["bsd_lastinstallment"])
                    {
                        this.strMess.AppendLine("t.3");
                    }
                    else
                    {
                        StringBuilder strMess = this.strMess;
                        flag5 = (bool)entity25["bsd_managementfeesstatus"];
                        string str19 = "statuscode: " + flag5.ToString();
                        strMess.AppendLine(str19);
                        if (str18 == "mana" && entity25.Contains("bsd_managementfeesstatus") && (bool)entity25["bsd_managementfeesstatus"])
                        {
                            this.strMess.AppendLine("t.4");
                            if (flag13)
                            {
                                Decimal num41 = entity25.Contains("bsd_depositamount") ? ((Money)entity25["bsd_depositamount"]).Value : 0M;
                                num39 += num41;
                                entity24["bsd_depositamount"] = (object)new Money(num41);
                                entity24["bsd_type"] = (object)new OptionSetValue(100000003);
                            }
                            flag10 = true;
                            num39 += ((Money)entity25["bsd_managementamount"]).Value;
                            int num42 = entity25.Contains("bsd_ordernumber") ? (int)entity25["bsd_ordernumber"] : 0;
                            if ((entity25.Contains("bsd_duedatecalculatingmethod") ? ((OptionSetValue)entity25["bsd_duedatecalculatingmethod"]).Value : 0) == 100000002)
                            {
                                flag11 = true;
                                num40 = entity25.Contains("bsd_amountofthisphase") ? ((Money)entity25["bsd_amountofthisphase"]).Value : 0M;
                            }
                            str17 = !flag12 ? str17 + num42.ToString() : str17 + " + " + num42.ToString();
                            flag12 = true;
                        }
                    }
                }
                this.strMess.AppendLine("chuoi_name 2 " + str17);
                if (str17 != "")
                {
                    if (entityReference6 != null)
                    {
                        Entity entity26 = this.service.Retrieve(entityReference6.LogicalName, entityReference6.Id, new ColumnSet(new string[1]
                        {
              "name"
                        }));
                        entity24["bsd_name"] = (object)("Thu tiền Phí quản lý của căn hộ đợt " + str17 + " của căn hộ " + (string)entity26["name"]);
                    }
                    else
                        entity24["bsd_name"] = (object)("Thu tiền Phí quản lý của căn hộ đợt " + str17);
                    entity24["bsd_project"] = ApplyDoc.Contains("bsd_project") ? ApplyDoc["bsd_project"] : (object)null;
                    entity24["bsd_optionentry"] = ApplyDoc.Contains("bsd_optionentry") ? ApplyDoc["bsd_optionentry"] : (object)0;
                    entity24["bsd_applydocument"] = (object)ApplyDoc.ToEntityReference();
                    EntityReference entityReference7 = ApplyDoc.Contains("bsd_customer") ? (EntityReference)ApplyDoc["bsd_customer"] : (EntityReference)null;
                    entity24["bsd_purchaser"] = (object)entityReference7;
                    if (entityReference7 != null)
                    {
                        if (this.service.Retrieve(entityReference7.LogicalName, entityReference7.Id, new ColumnSet(true)).LogicalName == "contact")
                            entity24["bsd_purchasernamecustomer"] = (object)entityReference7;
                        else
                            entity24["bsd_purchasernamecompany"] = (object)entityReference7;
                    }
                    if (utcTime != new DateTime())
                    {
                        DateTime dateTime = this.RetrieveLocalTimeFromUTCTime(utcTime);
                        entity24["bsd_issueddate"] = (object)dateTime;
                    }
                    entity24["bsd_paymentmethod"] = (object)new OptionSetValue(100000000);
                    Entity entity27 = this.service.Retrieve("salesorder", ((EntityReference)ApplyDoc["bsd_optionentry"]).Id, new ColumnSet(new string[1]
                    {
            "bsd_landvaluededuction"
                    }));
                    Entity entity28 = this.service.Retrieve("bsd_project", ((EntityReference)ApplyDoc["bsd_project"]).Id, new ColumnSet(new string[2]
                    {
            "bsd_formno",
            "bsd_serialno"
                    }));
                    string str20 = entity28.Contains("bsd_formno") ? (string)entity28["bsd_formno"] : "";
                    string str21 = entity28.Contains("bsd_serialno") ? (string)entity28["bsd_serialno"] : "";
                    entity24["bsd_formno"] = (object)str20;
                    entity24["bsd_serialno"] = (object)str21;
                    entity24["bsd_issueddate"] = (object)utcTime;
                    entity24["bsd_units"] = (object)entityReference6;
                    Decimal num43 = Math.Round(num39 / 11M, MidpointRounding.AwayFromZero);
                    Decimal num44 = 0M;
                    Decimal num45 = num43 - num44;
                    Decimal num46 = num39 - num45;
                    Decimal num47 = entity27.Contains("bsd_landvaluededuction") ? ((Money)entity27["bsd_landvaluededuction"]).Value : 0M;
                    Decimal num48 = 0M;
                    if (flag11)
                        num48 = Math.Round(num47 / 11M, MidpointRounding.AwayFromZero);
                    entity24["bsd_invoiceamount"] = (object)new Money(num39);
                    entity24["bsd_vatamount"] = (object)new Money(num43);
                    entity24["bsd_vatadjamount"] = (object)new Money(num44);
                    entity24["bsd_vatamounthandover"] = (object)new Money(num43 - num44);
                    entity24["bsd_invoiceamountb4vat"] = (object)new Money(num39 - num43 - num44);
                    entity24["bsd_taxcode"] = (object)entity2.ToEntityReference();
                    entity24["bsd_taxcodevalue"] = (object)(Decimal)entity2["bsd_value"];
                    if (flag10)
                    {
                        this.strMess.AppendLine("tạo ra invoice 2");
                        this.service.Create(entity24);
                    }
                }
            }
            this.strMess.AppendLine(str1);
            this.strMess.AppendLine("------------------------------------------------");
            this.strMess.AppendLine(str2);
            this.strMess.AppendLine("------------------------------------------------");
            this.strMess.AppendLine(str3);
        }

        public EntityCollection get_ecINS(IOrganizationService crmservices, string[] s_id)
        {
            string str = "<fetch version='1.0' output-format='xml-platform' mapping='logical' distinct='true' >\n                <entity name='bsd_paymentschemedetail' >\n                <attribute name='bsd_name' />\n                <attribute name='bsd_interchargeprecalc' />\n                <attribute name='bsd_ordernumber' />\n                <attribute name='bsd_maintenanceamount' />\n                <attribute name='bsd_balance' />\n                <attribute name='bsd_amountwaspaid' />\n                <attribute name='bsd_managementfee' />\n                <attribute name='bsd_actualgracedays' />\n                <attribute name='bsd_waiveramount' />\n                <attribute name='bsd_managementamount' />\n                <attribute name='bsd_duedate' />\n                <attribute name='bsd_maintenancefees' />\n                <attribute name='bsd_interestchargestatus' />\n                <attribute name='bsd_optionentry' />\n                <attribute name='bsd_amountpay' />\n                <attribute name='bsd_interestchargeamount' />\n                <attribute name='statuscode' />\n                <attribute name='bsd_depositamount' />\n                <attribute name='bsd_amountofthisphase' />\n                <attribute name='bsd_interestwaspaid' />\n                <attribute name='bsd_paymentscheme' />\n                <attribute name='bsd_paymentschemedetailid' />\n                <attribute name='bsd_waiverinstallment' />\n                <attribute name='bsd_waiverinterest' />\n                <attribute name='bsd_lastinstallment' />\n                <attribute name='bsd_duedatecalculatingmethod' />\n                <filter type='and'>\n                  <condition attribute='bsd_paymentschemedetailid' operator='in'>";
            for (int index = 0; index < s_id.Length; ++index)
                str = str + "<value>" + Guid.Parse(s_id[index]).ToString() + "</value>";
            string query = str + "</condition>\n                            </filter>\n                            <order attribute='bsd_ordernumber'/>\n                          </entity>\n                        </fetch>";
            return crmservices.RetrieveMultiple((QueryBase)new FetchExpression(query));
        }

        public Entity getLastInstallment(IOrganizationService service, Entity optionentry)
        {
            string query = string.Format("<fetch version='1.0' output-format='xml-platform' mapping='logical' distinct='true' >\n                <entity name='bsd_paymentschemedetail' >\n                <attribute name='bsd_duedatecalculatingmethod' />\n                <attribute name='bsd_maintenanceamount' />\n                <attribute name='bsd_maintenancefeepaid' />\n                <attribute name='bsd_maintenancefeewaiver' />\n                <attribute name='bsd_ordernumber' />\n                <attribute name='statuscode' />\n                <attribute name='bsd_managementfeepaid' />\n                <attribute name='bsd_managementfeewaiver' />\n                <attribute name='bsd_amountpay' />\n                <attribute name='bsd_maintenancefees' />\n                <attribute name='bsd_optionentry' />\n                <attribute name='bsd_managementfee' />\n                <attribute name='bsd_managementfeesstatus' />\n                <attribute name='bsd_managementamount' />\n                <attribute name='bsd_amountwaspaid' />\n                <attribute name='bsd_maintenancefeesstatus' />\n                <attribute name='bsd_name' />\n                <attribute name='bsd_maintenancefees' />\n                <attribute name='bsd_managementfee' />\n                <attribute name='bsd_duedatecalculatingmethod' />\n                <attribute name='bsd_paymentschemedetailid' />\n                <attribute name='bsd_amountofthisphase' />\n                <filter type='and' >\n                  <condition attribute='bsd_optionentry' operator='eq' value='{0}'/>       \n                  <condition attribute='bsd_lastinstallment' operator='eq' value='1'/>  \n                  <condition attribute='statecode' operator='eq' value='0'/> \n                </filter>\n              </entity>\n            </fetch>", (object)optionentry.Id);
            return service.RetrieveMultiple((QueryBase)new FetchExpression(query)).Entities[0];
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
    }
}
