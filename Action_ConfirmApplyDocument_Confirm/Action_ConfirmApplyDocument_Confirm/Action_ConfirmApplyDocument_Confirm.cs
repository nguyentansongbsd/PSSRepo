using Microsoft.Crm.Sdk.Messages;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Query;
using System;
using System.Collections;
using System.Collections.ObjectModel;
using System.Web.Script.Serialization;

namespace Action_ConfirmApplyDocument_Confirm
{
    public class Action_ConfirmApplyDocument_Confirm : IPlugin
    {
        IOrganizationService service = null;
        IOrganizationServiceFactory factory = null;
        ApplyDocument applyDocument;
        ITracingService TracingSe = null;
        void IPlugin.Execute(IServiceProvider serviceProvider)
        {
            IPluginExecutionContext context = (IPluginExecutionContext)serviceProvider.GetService(typeof(IPluginExecutionContext));
            TracingSe = (ITracingService)serviceProvider.GetService(typeof(ITracingService));
            factory = (IOrganizationServiceFactory)serviceProvider.GetService(typeof(IOrganizationServiceFactory));
            service = factory.CreateOrganizationService(context.UserId);
            string input01 = "";
            if (!string.IsNullOrEmpty((string)context.InputParameters["input01"]))
            {
                input01 = context.InputParameters["input01"].ToString();
            }
            string input02 = "";
            if (!string.IsNullOrEmpty((string)context.InputParameters["input02"]))
            {
                input02 = context.InputParameters["input02"].ToString();
            }
            string input03 = "";
            if (!string.IsNullOrEmpty((string)context.InputParameters["input03"]))
            {
                input03 = context.InputParameters["input03"].ToString();
            }
            string input04 = "";
            if (!string.IsNullOrEmpty((string)context.InputParameters["input04"]))
            {
                input04 = context.InputParameters["input04"].ToString();
            }
            if (input01 == "Bước 01" && input02 != "")
            {
                TracingSe.Trace("Bước 01");
                var fetchXml = $@"<?xml version=""1.0"" encoding=""utf-16""?>
                <fetch top=""1"">
                  <entity name=""bsd_bsd_confirmapplydocument_bsd_applydocum"">
                    <attribute name=""bsd_applydocumentid"" />
                    <filter>
                      <condition attribute=""bsd_confirmapplydocumentid"" operator=""eq"" value=""{input02}"" />
                    </filter>
                  </entity>
                </fetch>";
                EntityCollection rs = service.RetrieveMultiple(new FetchExpression(fetchXml));
                if (rs.Entities.Count == 0) throw new InvalidPluginExecutionException("The list of payments to be processed is currently empty. Please check again.");
                Entity enTarget = new Entity("bsd_confirmapplydocument");
                enTarget.Id = Guid.Parse(input02);
                enTarget["bsd_powerautomate"] = true;
                service.Update(enTarget);
                context.OutputParameters["output01"] = context.UserId.ToString();
                string url = "";
                EntityCollection configGolive = RetrieveMultiRecord(service, "bsd_configgolive",
                    new ColumnSet(new string[] { "bsd_url" }), "bsd_name", "Confirm Apply Document Confirm");
                foreach (Entity item in configGolive.Entities)
                {
                    if (item.Contains("bsd_url")) url = (string)item["bsd_url"];
                }
                if (url == "") throw new InvalidPluginExecutionException("Link to run PA not found. Please check again.");
                context.OutputParameters["output02"] = url;
            }
            else if (input01 == "Bước 02" && input02 != "" && input03 != "" && input04 != "")
            {
                TracingSe.Trace("Bước 02");
                service = factory.CreateOrganizationService(Guid.Parse(input04));
                applyDocument = new ApplyDocument(serviceProvider);
                Entity en_app = service.Retrieve("bsd_applydocument", Guid.Parse(input03), new ColumnSet(true));
                TracingSe.Trace("checkInput");
                applyDocument.checkInput(en_app);
                TracingSe.Trace("ra checkInput");
                int i_bsd_transactiontype = en_app.Contains("bsd_transactiontype") ? ((OptionSetValue)en_app["bsd_transactiontype"]).Value : 0;
                decimal bsd_advancepaymentamount = en_app.Contains("bsd_advancepaymentamount") ? ((Money)en_app["bsd_advancepaymentamount"]).Value : 0;
                decimal totalapplyamout = bsd_advancepaymentamount;
                ArrayList s_eachAdv = new ArrayList();
                ArrayList s_amAdv = new ArrayList();
                ArrayList listCheckFee = new ArrayList();
                string str1 = "";
                string str2 = "";
                string str3 = "";
                string str4 = "";
                string strRong1 = "";
                string strRong2 = "";
                if (i_bsd_transactiontype == 2)//Installments
                {
                    applyDocument.paymentInstallment(en_app, ref totalapplyamout, "Installments", ref str1, ref str2, listCheckFee);
                    processApplyDocument(en_app, str1, str2, str3, str4, listCheckFee);
                }
                else if (i_bsd_transactiontype == 3)//Interest
                {
                    applyDocument.paymentInstallment(en_app, ref totalapplyamout, "Interest", ref strRong1, ref strRong2, listCheckFee);
                }
                else if (i_bsd_transactiontype == 4)//Fees
                {
                    TracingSe.Trace("Fees");
                    applyDocument.paymentInstallment(en_app, ref totalapplyamout, "Fees", ref str3, ref str4, listCheckFee);
                    TracingSe.Trace("ra Fees");
                    processApplyDocument(en_app, str1, str2, str3, str4, listCheckFee);
                    TracingSe.Trace("processApplyDocument");
                }
                else if (i_bsd_transactiontype == 5)//Miscellaneous
                {
                    applyDocument.paymentInstallment(en_app, ref totalapplyamout, "Miscellaneous", ref strRong1, ref strRong2, listCheckFee);
                }
                if (i_bsd_transactiontype != 1)
                {
                    if (totalapplyamout != 0) totalapplyamout = bsd_advancepaymentamount - totalapplyamout;
                    else totalapplyamout = bsd_advancepaymentamount;
                }
                TracingSe.Trace("Create Applydocument Remaining COA");
                applyDocument.createCOA(en_app, totalapplyamout, s_eachAdv, s_amAdv);
                TracingSe.Trace("Tạo Applydocument Remaining COA");
                applyDocument.updateApplyDocument(en_app, totalapplyamout, s_eachAdv, s_amAdv);
                TracingSe.Trace("Xong bước 02");
            }
            else if (input01 == "Bước 03" && input02 != "" && input04 != "")
            {
                TracingSe.Trace("Bước 03");
                service = factory.CreateOrganizationService(Guid.Parse(input04));
                Entity enTarget = new Entity("bsd_confirmapplydocument");
                enTarget.Id = Guid.Parse(input02);
                enTarget["bsd_powerautomate"] = false;
                enTarget["statuscode"] = new OptionSetValue(100000000);
                service.Update(enTarget);
            }
        }
        EntityCollection RetrieveMultiRecord(IOrganizationService crmservices, string entity, ColumnSet column, string condition, object value)
        {
            QueryExpression q = new QueryExpression(entity);
            q.ColumnSet = column;
            q.Criteria = new FilterExpression();
            q.Criteria.AddCondition(new ConditionExpression(condition, ConditionOperator.Equal, value));
            q.Criteria.AddCondition(new ConditionExpression("statecode", ConditionOperator.Equal, 0));
            EntityCollection entc = service.RetrieveMultiple(q);

            return entc;
        }
        public void processApplyDocument(Entity EnCallFrom, string str1, string str2, string str3, string str4, ArrayList listCheckFee)
        {
            bool flag1 = false;
            JavaScriptSerializer scriptSerializer = new JavaScriptSerializer();
            Guid guid2 = new Guid();
            EntityCollection entityCollection1 = this.service.RetrieveMultiple((QueryBase)new FetchExpression(string.Format("\r\n                <fetch>\r\n                  <entity name='bsd_taxcode'>\r\n                    <attribute name='bsd_name' />\r\n                    <attribute name='bsd_value' />\r\n                    <filter type='or'>\r\n                      <condition attribute='bsd_name' operator='eq' value='{0}'/>\r\n                      <condition attribute='bsd_name' operator='eq' value='{1}'/>\r\n                    </filter>\r\n                    <order attribute='createdon' descending='true' />\r\n                  </entity>\r\n                </fetch>", (object)10, (object)-1).ToString()));
            Entity entity2 = (Entity)null;
            Entity entity3 = (Entity)null;
            foreach (Entity entity4 in (Collection<Entity>)entityCollection1.Entities)
            {
                if (entity4.Contains("bsd_name") && entity4["bsd_name"].ToString() == "-1" && entity2 == null)
                    entity2 = entity4;
                else if (entity4.Contains("bsd_name") && entity4["bsd_name"].ToString() == "10" && entity3 == null)
                    entity3 = entity4;
            }
            TracingSe.Trace("processApplyDocument B1");
            DateTime utcTime;
            EntityReference entityReference2;
            utcTime = EnCallFrom.Contains("bsd_receiptdate") ? (DateTime)EnCallFrom["bsd_receiptdate"] : new DateTime();
            entityReference2 = EnCallFrom.Contains("bsd_customer") ? (EntityReference)EnCallFrom["bsd_customer"] : (EntityReference)null;
            if (str1 != "")
            {
                bool flag2 = false;
                bool flag3 = false;
                string[] strArray1 = new string[0];
                string[] strArray2 = new string[0];
                Decimal num1 = 0M;
                Decimal num2 = 0M;
                Decimal num3 = 0M;
                Decimal num4 = 0M;
                Guid guid3 = new Guid();
                Guid guid4;
                if (str1 != "")
                {
                    strArray1 = str1.Split(',');
                    strArray2 = str2.Split(',');
                }
                EntityCollection ecIns = this.get_ecINS(this.service, strArray1);
                if (((Collection<Entity>)ecIns.Entities).Count <= 0)
                    throw new InvalidPluginExecutionException("Cannot find any Installment from Installment list. Please check again!");
                for (int index = 0; index < strArray2.Length; ++index)
                    num1 += Decimal.Parse(strArray2[index]);
                string str5 = "";
                EntityReference entityReference3 = (EntityReference)EnCallFrom["bsd_units"];
                Entity entity5 = new Entity("bsd_invoice");
                entity5["bsd_type"] = (object)new OptionSetValue(100000000);
                entity5["bsd_lastinstallmentamount"] = (object)new Money(0M);
                entity5["bsd_lastinstallmentvatamount"] = (object)new Money(0M);
                entity5["bsd_depositamount"] = (object)new Money(0M);
                Entity entity6 = entity5;
                guid4 = new Guid();
                Guid guid5 = guid4;
                entity6.Id = guid5;
                TracingSe.Trace("processApplyDocument B2");
                foreach (Entity entity7 in (Collection<Entity>)ecIns.Entities)
                {
                    int num5 = entity7.Contains("bsd_ordernumber") ? (int)entity7["bsd_ordernumber"] : 0;
                    Entity entity8 = this.service.Retrieve("bsd_paymentschemedetail", entity7.Id, new ColumnSet(true));
                    flag1 = entity8.Contains("bsd_ordernumber") && entity8["bsd_ordernumber"].ToString() == "1";
                    if (((!entity8.Contains("statuscode") ? 0 : (((OptionSetValue)entity8["statuscode"]).Value == 100000000 ? 1 : 0)) & (flag1 ? 1 : 0)) == 0 && (!entity8.Contains("bsd_lastinstallment") || !(bool)entity8["bsd_lastinstallment"]))
                    {
                        if (flag1)
                        {
                            guid2 = entity8.Id;
                            num4 = entity8.Contains("bsd_depositamount") ? ((Money)entity8["bsd_depositamount"]).Value : 0M;
                            num3 = ((Money)entity8["bsd_amountwaspaid"]).Value + num4;
                            int index = Array.IndexOf<string>(strArray1, guid2.ToString());
                            Decimal num6 = Decimal.Parse(strArray2[index]);
                            num1 -= num6;
                        }
                        else
                        {
                            int num7 = entity7.Contains("bsd_duedatecalculatingmethod") ? ((OptionSetValue)entity7["bsd_duedatecalculatingmethod"]).Value : 0;
                            if (num7 == 100000002)
                            {
                                string[] array = strArray1;
                                guid4 = entity7.Id;
                                string str6 = guid4.ToString();
                                int index = Array.IndexOf<string>(array, str6);
                                Decimal num8 = Decimal.Parse(strArray2[index]);
                                num1 -= num8;
                                guid3 = entity7.Id;
                                num2 = ((Money)entity7["bsd_amountofthisphase"]).Value;
                            }
                            if (!flag3)
                            {
                                if (num7 != 100000002)
                                    str5 += num5.ToString();
                            }
                            else if (num7 != 100000002)
                                str5 = str5 + " + " + num5.ToString();
                            flag3 = true;
                        }
                    }
                }
                TracingSe.Trace("processApplyDocument B3");
                Entity entity9 = this.service.Retrieve(entityReference3.LogicalName, entityReference3.Id, new ColumnSet(new string[1]
                {
          "name"
                }));
                entity5["bsd_name"] = (object)("Thu tiền đợt " + str5 + " của căn hộ " + (string)entity9["name"]);
                entity5["bsd_project"] = EnCallFrom.Contains("bsd_project") ? EnCallFrom["bsd_project"] : (object)null;
                entity5["bsd_optionentry"] = EnCallFrom.Contains("bsd_optionentry") ? EnCallFrom["bsd_optionentry"] : (object)0;
                if (EnCallFrom.LogicalName == "bsd_applydocument")
                    entity5["bsd_applydocument"] = (object)EnCallFrom.ToEntityReference();
                else
                    entity5["bsd_payment"] = (object)EnCallFrom.ToEntityReference();
                entity5["bsd_issueddate"] = (object)utcTime;
                entity5["bsd_units"] = (object)entityReference3;
                entity5["bsd_purchaser"] = (object)entityReference2;
                if (entityReference2 != null)
                {
                    if (this.service.Retrieve(entityReference2.LogicalName, entityReference2.Id, new ColumnSet(true)).LogicalName == "contact")
                        entity5["bsd_purchasernamecustomer"] = (object)entityReference2;
                    else
                        entity5["bsd_purchasernamecompany"] = (object)entityReference2;
                }
                if (utcTime != new DateTime())
                {
                    DateTime dateTime2 = this.RetrieveLocalTimeFromUTCTime(utcTime);
                    entity5["bsd_issueddate"] = (object)dateTime2;
                }
                entity5["bsd_paymentmethod"] = (object)new OptionSetValue(100000000);
                Entity optionentry = this.service.Retrieve("salesorder", ((EntityReference)EnCallFrom["bsd_optionentry"]).Id, new ColumnSet(new string[1]
                {
          "bsd_landvaluededuction"
                }));
                Entity entity10 = this.service.Retrieve("bsd_project", ((EntityReference)EnCallFrom["bsd_project"]).Id, new ColumnSet(new string[2]
                {
          "bsd_formno",
          "bsd_serialno"
                }));
                string str7 = entity10.Contains("bsd_formno") ? (string)entity10["bsd_formno"] : "";
                string str8 = entity10.Contains("bsd_serialno") ? (string)entity10["bsd_serialno"] : "";
                entity5["bsd_formno"] = (object)str7;
                entity5["bsd_serialno"] = (object)str8;
                Decimal num9 = Math.Round(num1 / 11M, MidpointRounding.AwayFromZero);
                Decimal num10 = 0M;
                Decimal num11 = num9 - num10;
                Decimal num12 = num1 - num11;
                Decimal num13 = ((Money)optionentry["bsd_landvaluededuction"]).Value;
                Decimal num14 = 0M;
                if (flag2)
                    num14 = Math.Round(num13 / 11M, MidpointRounding.AwayFromZero);
                entity5["bsd_invoiceamount"] = (object)new Money(num1);
                entity5["bsd_vatamount"] = (object)new Money(num9);
                entity5["bsd_vatadjamount"] = (object)new Money(num14);
                entity5["bsd_vatamounthandover"] = (object)new Money(num9 - num14);
                entity5["bsd_invoiceamountb4vat"] = (object)new Money(num12 - num9 - num14);
                entity5["statuscode"] = (object)new OptionSetValue(100000000);
                entity5["bsd_taxcode"] = (object)entity3.ToEntityReference();
                entity5["bsd_taxcodevalue"] = (object)(Decimal)entity3["bsd_value"];
                if (str5 != "")
                {
                    this.service.Create(entity5);
                }
                TracingSe.Trace("processApplyDocument B4");
                if (guid2 != new Guid())
                {
                    Entity entity11 = new Entity();
                    Entity entity12 = entity5;
                    entity12["bsd_name"] = (object)("Thu tiền đợt 1 của căn hộ " + (string)entity9["name"]);
                    entity12["bsd_depositamount"] = (object)new Money(num4);
                    entity12["bsd_type"] = (object)new OptionSetValue(100000003);
                    Decimal num15 = Math.Round(num3 / 11M, MidpointRounding.AwayFromZero);
                    Decimal num16 = 0M;
                    Decimal num17 = num15 - num16;
                    Decimal num18 = num3 - num17;
                    Decimal num19 = ((Money)optionentry["bsd_landvaluededuction"]).Value;
                    Decimal num20 = 0M;
                    entity12["bsd_invoiceamount"] = (object)new Money(num3);
                    entity12["bsd_vatamount"] = (object)new Money(num15);
                    entity12["bsd_vatadjamount"] = (object)new Money(num20);
                    entity12["bsd_vatamounthandover"] = (object)new Money(num15 - num20);
                    entity12["bsd_invoiceamountb4vat"] = (object)new Money(num12 - num15 - num20);
                    entity12["bsd_taxcode"] = (object)entity3.ToEntityReference();
                    entity12["bsd_taxcodevalue"] = (object)(Decimal)entity3["bsd_value"];
                    this.service.Create(entity12);
                }
                TracingSe.Trace("processApplyDocument B5");
                if (guid3 != new Guid() && num2 != 0M)
                {
                    Entity entity13 = EnCallFrom.Contains("bsd_optionentry") ? this.service.Retrieve("salesorder", ((EntityReference)EnCallFrom["bsd_optionentry"]).Id, new ColumnSet(true)) : (Entity)null;
                    Decimal num21 = 0M;
                    Decimal num22 = 0M;
                    if (entity13 != null)
                    {
                        num21 = entity13.Contains("totaltax") ? ((Money)entity13["totaltax"]).Value : 0M;
                        EntityCollection entityCollection2 = this.service.RetrieveMultiple((QueryBase)new FetchExpression(string.Format("\r\n                            <fetch aggregate='true'>\r\n                              <entity name='bsd_paymentschemedetail'>\r\n                                <attribute name='bsd_amountwaspaid' alias='totalamountwaspaidins' aggregate='sum' />\r\n                                <filter type='and'>\r\n                                  <condition attribute='bsd_optionentry' operator='eq' value='{0}'/>\r\n                                  <condition attribute='bsd_lastinstallment' operator='neq' value='{1}'/>\r\n                                  <condition attribute='statecode' operator='eq' value='{2}'/>\r\n                                  <condition attribute='bsd_duedatecalculatingmethod' operator='neq' value='{3}'/>\r\n                                </filter>\r\n                              </entity>\r\n                            </fetch>", (object)entity13.Id, (object)1, (object)0, (object)100000002)));
                        if (entityCollection2 != null && ((Collection<Entity>)entityCollection2.Entities).Count > 0)
                            num22 = ((Money)((AliasedValue)entityCollection2[0]["totalamountwaspaidins"]).Value).Value;
                    }
                    Entity lastInstallment = this.getLastInstallment(this.service, optionentry);
                    Decimal num23 = lastInstallment.Contains("bsd_amountofthisphase") ? ((Money)lastInstallment["bsd_amountofthisphase"]).Value : 0M;
                    Decimal num24 = Math.Round(num23 / 11M, MidpointRounding.AwayFromZero);
                    Entity entity14 = this.service.Retrieve("bsd_paymentschemedetail", guid3, new ColumnSet(true));
                    Decimal num25 = Math.Round((entity14.Contains("bsd_amountwaspaid") ? ((Money)entity14["bsd_amountwaspaid"]).Value : 0M) / 11M, MidpointRounding.AwayFromZero);
                    int index = Array.IndexOf<string>(strArray1, guid3.ToString());
                    Decimal num26 = Decimal.Parse(strArray2[index]);
                    Decimal num27 = num26;
                    Decimal num28 = Math.Round(num26 / 11M, MidpointRounding.AwayFromZero);
                    string str9 = (entity14.Contains("bsd_ordernumber") ? (int)entity14["bsd_ordernumber"] : 0).ToString();
                    bool flag4 = entity14.Contains("statuscode") && ((OptionSetValue)entity14["statuscode"]).Value == 100000001;
                    if (num21 >= Math.Round(num22 / 11M, MidpointRounding.AwayFromZero) + num24 + num25)
                    {
                        if (flag4)
                        {
                            Math.Round(num13 / 11M, MidpointRounding.AwayFromZero);
                            entity5["bsd_name"] = (object)("Thu tiền đợt " + str9 + " của căn hộ " + (string)entity9["name"]);
                            entity5["bsd_invoiceamount"] = (object)new Money(num27 - num13);
                            entity5["bsd_type"] = (object)new OptionSetValue(100000000);
                            entity5["statuscode"] = (object)new OptionSetValue(100000000);
                            entity5["bsd_vatamount"] = (object)new Money(Math.Round((num27 - num13) / 11M, MidpointRounding.AwayFromZero));
                            entity5["bsd_vatadjamount"] = (object)new Money(0M);
                            entity5["bsd_vatamounthandover"] = (object)new Money(Math.Round((num27 - num13) / 11M, MidpointRounding.AwayFromZero));
                            entity5["bsd_invoiceamountb4vat"] = (object)new Money(Math.Round(num27 - num13 - (num27 - num13) / 11M, MidpointRounding.AwayFromZero));
                            entity5["bsd_taxcode"] = (object)entity3.ToEntityReference();
                            entity5["bsd_taxcodevalue"] = (object)(Decimal)entity3["bsd_value"];
                            this.service.Create(entity5);
                            Entity entity15 = new Entity("bsd_invoice");
                            Entity entity16 = entity5;
                            entity16["bsd_name"] = (object)("Giá trị quyền sử dụng đất (không chịu Thuế GTGT) của căn hộ " + (string)entity9["name"]);
                            entity16["bsd_invoiceamount"] = (object)new Money(num13);
                            entity16["bsd_type"] = (object)new OptionSetValue(100000006);
                            entity16["statuscode"] = (object)new OptionSetValue(100000000);
                            entity16["bsd_vatamount"] = (object)new Money(0M);
                            entity16["bsd_vatadjamount"] = (object)new Money(0M);
                            entity16["bsd_vatamounthandover"] = (object)new Money(0M);
                            entity16["bsd_invoiceamountb4vat"] = (object)new Money(num13);
                            entity16["bsd_taxcode"] = (object)entity2.ToEntityReference();
                            entity16["bsd_taxcodevalue"] = (object)(Decimal)entity2["bsd_value"];
                            this.service.Create(entity16);
                            Entity entity17 = new Entity("bsd_invoice");
                            Entity entity18 = entity5;
                            entity18["bsd_name"] = (object)("5% giá trị của căn hộ " + (string)entity9["name"] + " (Đợt cuối)");
                            entity18["bsd_invoiceamount"] = (object)new Money(num23);
                            entity18["bsd_type"] = (object)new OptionSetValue(100000004);
                            entity18["statuscode"] = (object)new OptionSetValue(100000000);
                            entity18["bsd_vatamount"] = (object)new Money(num24);
                            entity18["bsd_vatadjamount"] = (object)new Money(0M);
                            entity18["bsd_vatamounthandover"] = (object)new Money(num24);
                            entity18["bsd_invoiceamountb4vat"] = (object)new Money(num23 - num24);
                            entity18["bsd_taxcode"] = (object)entity3.ToEntityReference();
                            entity18["bsd_taxcodevalue"] = (object)(Decimal)entity3["bsd_value"];
                            this.service.Create(entity18);
                        }
                        else
                        {
                            Decimal num29 = 0M;
                            entity5["bsd_name"] = (object)("Thu tiền đợt " + str9 + " của căn hộ " + (string)entity9["name"]);
                            entity5["bsd_type"] = (object)new OptionSetValue(100000000);
                            entity5["statuscode"] = (object)new OptionSetValue(100000000);
                            entity5["bsd_invoiceamount"] = (object)new Money(num26);
                            entity5["bsd_vatamount"] = (object)new Money(num28);
                            entity5["bsd_vatadjamount"] = (object)new Money(num29);
                            entity5["bsd_vatamounthandover"] = (object)new Money(num28 - num29);
                            entity5["bsd_invoiceamountb4vat"] = (object)new Money(num26 - num28 - num29);
                            entity5["bsd_taxcode"] = (object)entity3.ToEntityReference();
                            entity5["bsd_taxcodevalue"] = (object)(Decimal)entity3["bsd_value"];
                            this.service.Create(entity5);
                        }
                    }
                    else
                    {
                        if (flag4)
                        {
                            entity5["bsd_name"] = (object)("Giá trị quyền sử dụng đất (không chịu Thuế GTGT) của căn hộ " + (string)entity9["name"]);
                            entity5["bsd_type"] = (object)new OptionSetValue(100000005);
                            entity5["statuscode"] = (object)new OptionSetValue(100000000);
                            entity5["bsd_invoiceamount"] = (object)new Money(num26);
                            entity5["bsd_vatamount"] = (object)new Money(num28);
                            entity5["bsd_vatadjamount"] = (object)new Money(num14);
                            entity5["bsd_vatamounthandover"] = (object)new Money(num28 - num14);
                            entity5["bsd_invoiceamountb4vat"] = (object)new Money(num26 - num28 - num14);
                            entity5["bsd_taxcode"] = (object)entity2.ToEntityReference();
                            entity5["bsd_taxcodevalue"] = (object)(Decimal)entity2["bsd_value"];
                            this.service.Create(entity5);
                            Entity entity19 = new Entity("bsd_invoice");
                            Entity entity20 = entity5;
                            entity20["bsd_name"] = (object)("5% giá trị của căn hộ " + (string)entity9["name"] + " (Đợt cuối)");
                            entity20["bsd_invoiceamount"] = (object)new Money(num23);
                            entity20["bsd_type"] = (object)new OptionSetValue(100000004);
                            entity20["statuscode"] = (object)new OptionSetValue(100000000);
                            entity20["bsd_vatamount"] = (object)new Money(num24);
                            entity20["bsd_vatadjamount"] = (object)new Money(0M);
                            entity20["bsd_vatamounthandover"] = (object)new Money(num24);
                            entity20["bsd_invoiceamountb4vat"] = (object)new Money(num23 - num24);
                            entity20["bsd_taxcode"] = (object)entity3.ToEntityReference();
                            entity20["bsd_taxcodevalue"] = (object)(Decimal)entity3["bsd_value"];
                            this.service.Create(entity20);
                        }
                        else
                        {
                            Decimal num30 = 0M;
                            entity5["bsd_name"] = (object)("Giá trị quyền sử dụng đất (không chịu Thuế GTGT) của căn hộ " + (string)entity9["name"]);
                            entity5["bsd_type"] = (object)new OptionSetValue(100000005);
                            entity5["statuscode"] = (object)new OptionSetValue(100000000);
                            entity5["bsd_invoiceamount"] = (object)new Money(num26);
                            entity5["bsd_vatamount"] = (object)new Money(num28);
                            entity5["bsd_vatadjamount"] = (object)new Money(num30);
                            entity5["bsd_vatamounthandover"] = (object)new Money(num28 - num30);
                            entity5["bsd_invoiceamountb4vat"] = (object)new Money(num26 - num28 - num30);
                            entity5["bsd_taxcode"] = (object)entity2.ToEntityReference();
                            entity5["bsd_taxcodevalue"] = (object)(Decimal)entity2["bsd_value"];
                            this.service.Create(entity5);
                        }
                    }
                }
            }
            TracingSe.Trace("processApplyDocumentB6");
            if (str3 != "")
            {
                string[] strArray3 = str3.Split(',');
                str4.Split(',');
                bool flag5 = false;
                Entity entity21 = new Entity("bsd_invoice");
                entity21.Id = new Guid();
                entity21["bsd_type"] = (object)new OptionSetValue(100000001);
                entity21["bsd_lastinstallmentamount"] = (object)new Money(0M);
                entity21["bsd_lastinstallmentvatamount"] = (object)new Money(0M);
                entity21["bsd_depositamount"] = (object)new Money(0M);
                Decimal num31 = 0M;
                Decimal num32 = 0M;
                string str10 = "";
                EntityReference entityReference4 = EnCallFrom.Contains("bsd_units") ? (EntityReference)EnCallFrom["bsd_units"] : (EntityReference)null;
                bool flag6 = false;
                TracingSe.Trace("processApplyDocumentB7");
                for (int index = 0; index < strArray3.Length; ++index)
                {
                    string g = strArray3[index];
                    string str11 = (string)listCheckFee[index];
                    Entity entity22 = this.service.Retrieve("bsd_paymentschemedetail", new Guid(g), new ColumnSet(true));
                    bool flag7 = entity22.Contains("bsd_ordernumber") && entity22["bsd_ordernumber"].ToString() == "1";
                    if (((!entity22.Contains("bsd_maintenancefeesstatus") ? 0 : ((bool)entity22["bsd_maintenancefeesstatus"] ? 1 : 0)) & (flag7 ? 1 : 0)) != 0)
                    {

                    }
                    else if (entity22.Contains("bsd_lastinstallment") && (bool)entity22["bsd_lastinstallment"])
                    {

                    }
                    else
                    {
                        if (str11 == "main" && entity22.Contains("bsd_maintenancefeesstatus") && (bool)entity22["bsd_maintenancefeesstatus"])
                        {
                            if (flag7)
                            {
                                Decimal num33 = entity22.Contains("bsd_depositamount") ? ((Money)entity22["bsd_depositamount"]).Value : 0M;
                                num31 += num33;
                                entity21["bsd_depositamount"] = (object)new Money(num33);
                                entity21["bsd_type"] = (object)new OptionSetValue(100000003);
                            }
                            flag5 = true;
                            num31 += ((Money)entity22["bsd_maintenanceamount"]).Value;
                            int num34 = entity22.Contains("bsd_ordernumber") ? (int)entity22["bsd_ordernumber"] : 0;
                            if ((entity22.Contains("bsd_duedatecalculatingmethod") ? ((OptionSetValue)entity22["bsd_duedatecalculatingmethod"]).Value : 0) == 100000002)
                                num32 = entity22.Contains("bsd_amountofthisphase") ? ((Money)entity22["bsd_amountofthisphase"]).Value : 0M;
                            str10 = !flag6 ? str10 + num34.ToString() : str10 + " + " + num34.ToString();
                            flag6 = true;
                        }
                    }
                }
                TracingSe.Trace("processApplyDocument B8");
                if (str10 != "")
                {
                    if (entityReference4 != null)
                    {
                        Entity entity23 = this.service.Retrieve(entityReference4.LogicalName, entityReference4.Id, new ColumnSet(new string[1]
                        {
              "name"
                        }));
                        entity21["bsd_name"] = (object)("Thu tiền Phí bảo trì của căn hộ " + (string)entity23["name"]);
                    }
                    else
                        entity21["bsd_name"] = (object)("Thu tiền Phí bảo trì của căn hộ đợt " + str10);
                    entity21["bsd_project"] = EnCallFrom.Contains("bsd_project") ? EnCallFrom["bsd_project"] : (object)null;
                    entity21["bsd_optionentry"] = EnCallFrom.Contains("bsd_optionentry") ? EnCallFrom["bsd_optionentry"] : (object)null;
                    if (EnCallFrom.LogicalName == "bsd_applydocument")
                        entity21["bsd_applydocument"] = (object)EnCallFrom.ToEntityReference();
                    else
                        entity21["bsd_payment"] = (object)EnCallFrom.ToEntityReference();
                    entity21["bsd_purchaser"] = (object)entityReference2;
                    if (entityReference2 != null)
                    {
                        if (this.service.Retrieve(entityReference2.LogicalName, entityReference2.Id, new ColumnSet(true)).LogicalName == "contact")
                            entity21["bsd_purchasernamecustomer"] = (object)entityReference2;
                        else
                            entity21["bsd_purchasernamecompany"] = (object)entityReference2;
                    }
                    if (utcTime != new DateTime())
                    {
                        DateTime dateTime3 = this.RetrieveLocalTimeFromUTCTime(utcTime);
                        entity21["bsd_issueddate"] = (object)dateTime3;
                    }
                    entity21["bsd_paymentmethod"] = (object)new OptionSetValue(100000000);
                    Entity entity24 = this.service.Retrieve("salesorder", ((EntityReference)EnCallFrom["bsd_optionentry"]).Id, new ColumnSet(new string[1]
                    {
            "bsd_landvaluededuction"
                    }));
                    Entity entity25 = this.service.Retrieve("bsd_project", ((EntityReference)EnCallFrom["bsd_project"]).Id, new ColumnSet(new string[2]
                    {
            "bsd_formno",
            "bsd_serialno"
                    }));
                    string str12 = entity25.Contains("bsd_formno") ? (string)entity25["bsd_formno"] : "";
                    string str13 = entity25.Contains("bsd_serialno") ? (string)entity25["bsd_serialno"] : "";
                    entity21["bsd_formno"] = (object)str12;
                    entity21["bsd_serialno"] = (object)str13;
                    entity21["bsd_issueddate"] = (object)utcTime;
                    entity21["bsd_units"] = (object)entityReference4;
                    Decimal num35 = entity24.Contains("bsd_landvaluededuction") ? ((Money)entity24["bsd_landvaluededuction"]).Value : 0M;
                    entity21["bsd_invoiceamount"] = (object)new Money(num31);
                    Decimal num36 = 0M;
                    Decimal num37 = 0M;
                    entity21["bsd_vatamount"] = (object)new Money(num36);
                    entity21["bsd_vatadjamount"] = (object)new Money(num37);
                    entity21["bsd_vatamounthandover"] = (object)new Money(num37);
                    entity21["bsd_invoiceamountb4vat"] = (object)new Money(num31 - num36 - num37);
                    entity21["bsd_taxcode"] = (object)entity2.ToEntityReference();
                    entity21["bsd_taxcodevalue"] = (object)(Decimal)entity2["bsd_value"];
                    if (flag5)
                    {
                        this.service.Create(entity21);
                    }
                }
            }
            TracingSe.Trace("processApplyDocumentB9");
            if (!(str3 != ""))
                return;
            string[] strArray5 = str3.Split(',');
            str4.Split(',');
            bool flag8 = false;
            Entity entity26 = new Entity("bsd_invoice");
            entity26.Id = new Guid();
            entity26["bsd_type"] = (object)new OptionSetValue(100000002);
            entity26["bsd_lastinstallmentamount"] = (object)new Money(0M);
            entity26["bsd_lastinstallmentvatamount"] = (object)new Money(0M);
            entity26["bsd_depositamount"] = (object)new Money(0M);
            bool flag9 = false;
            Decimal num38 = 0M;
            Decimal num39 = 0M;
            string str14 = "";
            EntityReference entityReference5 = (EntityReference)EnCallFrom["bsd_units"];
            bool flag10 = false;
            TracingSe.Trace("processApplyDocument B10");
            for (int index = 0; index < strArray5.Length; ++index)
            {
                string g = strArray5[index];
                string str15 = (string)listCheckFee[index];
                Entity entity27 = this.service.Retrieve("bsd_paymentschemedetail", new Guid(g), new ColumnSet(true));
                bool flag11 = entity27.Contains("bsd_ordernumber") && entity27["bsd_ordernumber"].ToString() == "1";
                if (((!entity27.Contains("bsd_managementfeesstatus") ? 0 : ((bool)entity27["bsd_managementfeesstatus"] ? 1 : 0)) & (flag11 ? 1 : 0)) != 0)
                {

                }
                else if (entity27.Contains("bsd_lastinstallment") && (bool)entity27["bsd_lastinstallment"])
                {

                }
                else
                {
                    if (str15 == "mana" && entity27.Contains("bsd_managementfeesstatus") && (bool)entity27["bsd_managementfeesstatus"])
                    {
                        if (flag11)
                        {
                            Decimal num40 = entity27.Contains("bsd_depositamount") ? ((Money)entity27["bsd_depositamount"]).Value : 0M;
                            num38 += num40;
                            entity26["bsd_depositamount"] = (object)new Money(num40);
                            entity26["bsd_type"] = (object)new OptionSetValue(100000003);
                        }
                        flag8 = true;
                        num38 += ((Money)entity27["bsd_managementamount"]).Value;
                        int num41 = entity27.Contains("bsd_ordernumber") ? (int)entity27["bsd_ordernumber"] : 0;
                        if ((entity27.Contains("bsd_duedatecalculatingmethod") ? ((OptionSetValue)entity27["bsd_duedatecalculatingmethod"]).Value : 0) == 100000002)
                        {
                            flag9 = true;
                            num39 = entity27.Contains("bsd_amountofthisphase") ? ((Money)entity27["bsd_amountofthisphase"]).Value : 0M;
                        }
                        str14 = !flag10 ? str14 + num41.ToString() : str14 + " + " + num41.ToString();
                        flag10 = true;
                    }
                }
            }
            TracingSe.Trace("processApplyDocument B11");
            if (str14 != "")
            {
                if (entityReference5 != null)
                {
                    Entity entity28 = this.service.Retrieve(entityReference5.LogicalName, entityReference5.Id, new ColumnSet(new string[1]
                    {
            "name"
                    }));
                    entity26["bsd_name"] = (object)("Thu tiền Phí quản lý của căn hộ đợt " + str14 + " của căn hộ " + (string)entity28["name"]);
                }
                else
                    entity26["bsd_name"] = (object)("Thu tiền Phí quản lý của căn hộ đợt " + str14);
                entity26["bsd_project"] = EnCallFrom.Contains("bsd_project") ? EnCallFrom["bsd_project"] : (object)null;
                entity26["bsd_optionentry"] = EnCallFrom.Contains("bsd_optionentry") ? EnCallFrom["bsd_optionentry"] : (object)0;
                if (EnCallFrom.LogicalName == "bsd_applydocument")
                    entity26["bsd_applydocument"] = (object)EnCallFrom.ToEntityReference();
                else
                    entity26["bsd_payment"] = (object)EnCallFrom.ToEntityReference();
                entity26["bsd_purchaser"] = (object)entityReference2;
                if (entityReference2 != null)
                {
                    if (this.service.Retrieve(entityReference2.LogicalName, entityReference2.Id, new ColumnSet(true)).LogicalName == "contact")
                        entity26["bsd_purchasernamecustomer"] = (object)entityReference2;
                    else
                        entity26["bsd_purchasernamecompany"] = (object)entityReference2;
                }
                if (utcTime != new DateTime())
                {
                    DateTime dateTime4 = this.RetrieveLocalTimeFromUTCTime(utcTime);
                    entity26["bsd_issueddate"] = (object)dateTime4;
                }
                entity26["bsd_paymentmethod"] = (object)new OptionSetValue(100000000);
                Entity entity29 = this.service.Retrieve("salesorder", ((EntityReference)EnCallFrom["bsd_optionentry"]).Id, new ColumnSet(new string[1]
                {
          "bsd_landvaluededuction"
                }));
                Entity entity30 = this.service.Retrieve("bsd_project", ((EntityReference)EnCallFrom["bsd_project"]).Id, new ColumnSet(new string[2]
                {
          "bsd_formno",
          "bsd_serialno"
                }));
                string str16 = entity30.Contains("bsd_formno") ? (string)entity30["bsd_formno"] : "";
                string str17 = entity30.Contains("bsd_serialno") ? (string)entity30["bsd_serialno"] : "";
                entity26["bsd_formno"] = (object)str16;
                entity26["bsd_serialno"] = (object)str17;
                entity26["bsd_issueddate"] = (object)utcTime;
                entity26["bsd_units"] = (object)entityReference5;
                Decimal num42 = Math.Round(num38 / 11M, MidpointRounding.AwayFromZero);
                Decimal num43 = 0M;
                Decimal num44 = num42 - num43;
                Decimal num45 = num38 - num44;
                Decimal num46 = entity29.Contains("bsd_landvaluededuction") ? ((Money)entity29["bsd_landvaluededuction"]).Value : 0M;
                Decimal num47 = 0M;
                if (flag9)
                    num47 = Math.Round(num46 / 11M, MidpointRounding.AwayFromZero);
                entity26["bsd_invoiceamount"] = (object)new Money(num38);
                entity26["bsd_vatamount"] = (object)new Money(num42);
                entity26["bsd_vatadjamount"] = (object)new Money(num43);
                entity26["bsd_vatamounthandover"] = (object)new Money(num42 - num43);
                entity26["bsd_invoiceamountb4vat"] = (object)new Money(num38 - num42 - num43);
                entity26["bsd_taxcode"] = (object)entity3.ToEntityReference();
                entity26["bsd_taxcodevalue"] = (object)(Decimal)entity3["bsd_value"];
                if (flag8)
                {
                    this.service.Create(entity26);
                }
            }
        }

        public EntityCollection get_ecINS(IOrganizationService crmservices, string[] s_id)
        {
            string str1 = "<fetch version='1.0' output-format='xml-platform' mapping='logical' distinct='true' >\r\n                <entity name='bsd_paymentschemedetail' >\r\n                <attribute name='bsd_name' />\r\n                <attribute name='bsd_interchargeprecalc' />\r\n                <attribute name='bsd_ordernumber' />\r\n                <attribute name='bsd_maintenanceamount' />\r\n                <attribute name='bsd_balance' />\r\n                <attribute name='bsd_amountwaspaid' />\r\n                <attribute name='bsd_managementfee' />\r\n                <attribute name='bsd_actualgracedays' />\r\n                <attribute name='bsd_waiveramount' />\r\n                <attribute name='bsd_managementamount' />\r\n                <attribute name='bsd_duedate' />\r\n                <attribute name='bsd_maintenancefees' />\r\n                <attribute name='bsd_interestchargestatus' />\r\n                <attribute name='bsd_optionentry' />\r\n                <attribute name='bsd_amountpay' />\r\n                <attribute name='bsd_interestchargeamount' />\r\n                <attribute name='statuscode' />\r\n                <attribute name='bsd_depositamount' />\r\n                <attribute name='bsd_amountofthisphase' />\r\n                <attribute name='bsd_interestwaspaid' />\r\n                <attribute name='bsd_paymentscheme' />\r\n                <attribute name='bsd_paymentschemedetailid' />\r\n                <attribute name='bsd_waiverinstallment' />\r\n                <attribute name='bsd_waiverinterest' />\r\n                <attribute name='bsd_lastinstallment' />\r\n                <attribute name='bsd_duedatecalculatingmethod' />\r\n                <filter type='and'>\r\n                  <condition attribute='bsd_paymentschemedetailid' operator='in'>";
            for (int index = 0; index < s_id.Length; ++index)
                str1 = str1 + "<value>" + (object)Guid.Parse(s_id[index]) + "</value>";
            string str2 = str1 + "</condition>\r\n                            </filter>\r\n                            <order attribute='bsd_ordernumber'/>\r\n                          </entity>\r\n                        </fetch>";
            return crmservices.RetrieveMultiple((QueryBase)new FetchExpression(str2));
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
            string str = string.Format("<fetch version='1.0' output-format='xml-platform' mapping='logical' distinct='true' >\r\n                <entity name='bsd_paymentschemedetail' >\r\n                <attribute name='bsd_duedatecalculatingmethod' />\r\n                <attribute name='bsd_maintenanceamount' />\r\n                <attribute name='bsd_maintenancefeepaid' />\r\n                <attribute name='bsd_maintenancefeewaiver' />\r\n                <attribute name='bsd_ordernumber' />\r\n                <attribute name='statuscode' />\r\n                <attribute name='bsd_managementfeepaid' />\r\n                <attribute name='bsd_managementfeewaiver' />\r\n                <attribute name='bsd_amountpay' />\r\n                <attribute name='bsd_maintenancefees' />\r\n                <attribute name='bsd_optionentry' />\r\n                <attribute name='bsd_managementfee' />\r\n                <attribute name='bsd_managementfeesstatus' />\r\n                <attribute name='bsd_managementamount' />\r\n                <attribute name='bsd_amountwaspaid' />\r\n                <attribute name='bsd_maintenancefeesstatus' />\r\n                <attribute name='bsd_name' />\r\n                <attribute name='bsd_maintenancefees' />\r\n                <attribute name='bsd_managementfee' />\r\n                <attribute name='bsd_duedatecalculatingmethod' />\r\n                <attribute name='bsd_paymentschemedetailid' />\r\n                <attribute name='bsd_amountofthisphase' />\r\n                <filter type='and' >\r\n                  <condition attribute='bsd_optionentry' operator='eq' value='{0}'/>       \r\n                  <condition attribute='bsd_lastinstallment' operator='eq' value='1'/>  \r\n                  <condition attribute='statecode' operator='eq' value='0'/> \r\n                </filter>\r\n              </entity>\r\n            </fetch>", (object)optionentry.Id);
            return ((Collection<Entity>)service.RetrieveMultiple((QueryBase)new FetchExpression(str)).Entities)[0];
        }
    }
}
