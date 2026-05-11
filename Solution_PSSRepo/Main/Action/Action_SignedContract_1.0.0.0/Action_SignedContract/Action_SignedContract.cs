// Decompiled with JetBrains decompiler
// Type: Action_SignedContract.Action_SignedContract
// Assembly: Action_SignedContract, Version=1.0.0.0, Culture=neutral, PublicKeyToken=91af1975bd46f505
// MVID: 64A057F8-04D7-4937-A84E-D4EF3DDC89DB
// Assembly location: C:\Users\ngoct\Downloads\Action_SignedContract_1.0.0.0.dll

using BSDLibrary;
using Microsoft.Crm.Sdk.Messages;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Query;
using System;
using System.IdentityModel.Metadata;
using System.Runtime.InteropServices;

namespace Action_SignedContract
{
    public class Action_SignedContract : IPlugin
    {
        private IPluginExecutionContext context;
        private IOrganizationService service = (IOrganizationService)null;
        private IOrganizationServiceFactory factory = (IOrganizationServiceFactory)null;
        public ITracingService traceService = (ITracingService)null;
        private ParameterCollection target = (ParameterCollection)null;
        public Entity enBulkWaiver;
        private Common common;
        private IServiceProvider serviceProvider;

        public void Execute(IServiceProvider serviceProvider)
        {
            this.serviceProvider = serviceProvider;
            this.context = (IPluginExecutionContext)serviceProvider.GetService(typeof(IPluginExecutionContext));
            this.factory = (IOrganizationServiceFactory)serviceProvider.GetService(typeof(IOrganizationServiceFactory));
            this.service = this.factory.CreateOrganizationService(new Guid?(this.context.UserId));
            this.traceService = (ITracingService)serviceProvider.GetService(typeof(ITracingService));
            this.target = this.context.InputParameters;
            EntityReference entityReference = this.target["Target"] as EntityReference;
            this.common = new Common(this.service);
            Entity enOptionEntry = this.service.Retrieve(entityReference.LogicalName, entityReference.Id, new ColumnSet(true));
            int num1 = ((OptionSetValue)enOptionEntry["statuscode"]).Value;
            bool flag = new OptionEntry(serviceProvider, enOptionEntry).checkShortFallAmount(enOptionEntry);
            Guid id = ((EntityReference)enOptionEntry["bsd_paymentscheme"]).Id;
            if (num1 == 100000001 | flag)
            {
                if (!enOptionEntry.Contains("customerid"))
                    throw new InvalidPluginExecutionException("Contract does not contain Purchaser!");
                if (!enOptionEntry.Contains("ordernumber"))
                    throw new InvalidPluginExecutionException("Contract does not contain 'Option Number'!");
                if (!enOptionEntry.Contains("bsd_project"))
                    throw new InvalidPluginExecutionException("Contract does not contain 'Project'!");
                if (!enOptionEntry.Contains("bsd_contractprinteddate"))
                    throw new InvalidPluginExecutionException("Option Entry must be printed before signing!");
                DateTime dateTime1 = this.common.RetrieveLocalTimeFromUTCTime(DateTime.Now);
                this.service.Update(new Entity(enOptionEntry.LogicalName)
                {
                    Id = enOptionEntry.Id,
                    ["statuscode"] = (object)new OptionSetValue(100000002)
                });
                int num2 = enOptionEntry.Contains("bsd_numberofmonthspaidmf") ? (int)enOptionEntry["bsd_numberofmonthspaidmf"] : 0;
                if (!enOptionEntry.Contains("bsd_project"))
                    throw new InvalidPluginExecutionException("Cannot find project information on Option Entry: " + (string)enOptionEntry["name"] + "!");
                Entity entity1 = this.service.Retrieve(((EntityReference)enOptionEntry["bsd_unitnumber"]).LogicalName, ((EntityReference)enOptionEntry["bsd_unitnumber"]).Id, new ColumnSet(new string[2]
                {
          "bsd_numberofmonthspaidmf",
          "bsd_managementamountmonth"
                }));
                Entity entity2 = this.service.Retrieve(((EntityReference)enOptionEntry["bsd_project"]).LogicalName, ((EntityReference)enOptionEntry["bsd_project"]).Id, new ColumnSet(new string[2]
                {
          "bsd_name",
          "bsd_managementamount"
                }));
                Decimal num3 = !entity1.Contains("bsd_managementamountmonth") ? (entity2.Contains("bsd_managementamount") ? ((Money)entity2["bsd_managementamount"]).Value : 0M) : (entity1.Contains("bsd_managementamountmonth") ? ((Money)entity1["bsd_managementamountmonth"]).Value : 0M);
                DateTime dateTime2 = this.common.RetrieveLocalTimeFromUTCTime(enOptionEntry.Contains("bsd_signedcontractdate") ? (DateTime)enOptionEntry["bsd_signedcontractdate"] : dateTime1);
                Entity entity3 = this.service.Retrieve(((EntityReference)enOptionEntry["bsd_unitnumber"]).LogicalName, ((EntityReference)enOptionEntry["bsd_unitnumber"]).Id, new ColumnSet(new string[6]
                {
          "name",
          "statuscode",
          "bsd_signedcontractdate",
          "bsd_actualarea",
          "bsd_netsaleablearea",
          "bsd_optionnumber"
                }));
                Decimal num4 = entity3.Contains("bsd_netsaleablearea") ? (Decimal)entity3["bsd_netsaleablearea"] : 0M;
                Entity entity4 = this.service.Retrieve(((EntityReference)enOptionEntry["customerid"]).LogicalName, ((EntityReference)enOptionEntry["customerid"]).Id, new ColumnSet(new string[1]
                {
          "bsd_totaltransaction"
                }));
                int num5 = (entity4.Contains("bsd_totaltransaction") ? (int)entity4["bsd_totaltransaction"] : 0) + 1;
                this.service.Update(new Entity(entity3.LogicalName)
                {
                    Id = entity3.Id,
                    ["statuscode"] = (object)new OptionSetValue(100000002),
                    ["bsd_signedcontractdate"] = (object)dateTime2,
                    ["bsd_optionnumber"] = enOptionEntry["bsd_optionno"]
                });
                this.service.Update(new Entity(entity4.LogicalName)
                {
                    Id = entity4.Id,
                    ["bsd_totaltransaction"] = (object)num5
                });
                EntityCollection instFee = this.get_Inst_Fee(this.service, enOptionEntry.Id, id);
                if (instFee.Entities.Count > 0)
                {
                    Entity entity5 = instFee.Entities[0];
                    entity5.Id = instFee.Entities[0].Id;
                    Decimal num6 = instFee.Entities[0].Contains("bsd_managementamount") ? ((Money)instFee.Entities[0]["bsd_managementamount"]).Value : 0M;
                    Decimal num7 = num3 * (Decimal)num2 * num4;
                    this.service.Update(new Entity(entity5.LogicalName)
                    {
                        Id = entity5.Id,
                        ["bsd_managementamount"] = (object)new Money(num7)
                    });
                }
            }
            #region  Qua lại TĐTT kiểm tra đợt nào có sts = Paid => Update field Due Date wordtemplate trong đợt = null

            var query_bsd_optionentry = enOptionEntry.Id.ToString();

            var query = new QueryExpression("bsd_paymentschemedetail");
            query.ColumnSet.AllColumns = true;
            query.Criteria.AddCondition("bsd_optionentry", ConditionOperator.Equal, query_bsd_optionentry);
            var rs = service.RetrieveMultiple(query);
            if (rs.Entities.Count > 0)
            {
                foreach (var item in rs.Entities)
                {
                    if (((OptionSetValue)item["statuscode"]).Value == 100000001)
                    {
                        var itemEnUpdate = new Entity(item.LogicalName, item.Id);
                        itemEnUpdate["bsd_duedatewordtemplate"] = null;
                        service.Update(itemEnUpdate);
                    }
                }
            }
            enOptionEntry = this.service.Retrieve(entityReference.LogicalName, entityReference.Id, new ColumnSet(
                    "bsd_landvaluededuction",
                    "bsd_taxcode",
                    "customerid",
                    "bsd_contractnumber",
                    "bsd_contracttypedescription",
                    "bsd_contractdate",
                    "bsd_signedcontractdate",
                    "bsd_project",
                    "bsd_unitnumber"
                ));
            create_Invoice(enOptionEntry);
            #endregion
            this.context.OutputParameters["output"] = (object)"done";
        }

        private EntityCollection get_Inst_Fee(IOrganizationService crmservices, Guid oeID, Guid pmsID)
        {
            string query = string.Format("<fetch version='1.0' output-format='xml-platform' mapping='logical' distinct='true' >\r\n                  <entity name='bsd_paymentschemedetail' >\r\n                    <attribute name='bsd_duedate' />\r\n                    <attribute name='bsd_name' />\r\n                    <attribute name='bsd_duedatecalculatingmethod' />\r\n                    <attribute name='bsd_maintenanceamount' />\r\n                    <attribute name='bsd_maintenancefees' />\r\n                    <attribute name='bsd_managementfee' />\r\n                    <attribute name='bsd_amountofthisphase' />\r\n                    <attribute name='bsd_managementfeesstatus' />\r\n                    <attribute name='bsd_managementamount' />\r\n                    <attribute name='bsd_maintenancefeesstatus' />\r\n                    <attribute name='bsd_paymentschemedetailid' />\r\n                    <filter type='and' >\r\n                      <condition attribute='bsd_optionentry' operator='eq' value='{0}' />\r\n                      <condition attribute='bsd_managementamount' operator='gt' value='0' />\r\n                      <condition attribute='statecode' operator='eq' value='0' />\r\n                    </filter>\r\n                  </entity>\r\n                </fetch>", (object)oeID, (object)pmsID);
            return crmservices.RetrieveMultiple((QueryBase)new FetchExpression(query));
        }
        private void create_Invoice(Entity enOptionEntry)
        {
            if (!checkInvaldInvoice1st(enOptionEntry.Id))//chưa có invoice type 1st
            {
                Entity project_invoive = service.Retrieve("bsd_project", ((EntityReference)enOptionEntry["bsd_project"]).Id,
                new ColumnSet(new string[] {
                            "bsd_formno","bsd_serialno", "bsd_project_type"
                }));
                int bsd_project_type = project_invoive.Contains("bsd_project_type") ? ((OptionSetValue)project_invoive["bsd_project_type"]).Value : 0;
                EntityReference units = (EntityReference)enOptionEntry["bsd_unitnumber"];
                Entity iv_units = service.Retrieve(units.LogicalName, units.Id, new ColumnSet(new string[] { "name" }));
                string unitName = (string)iv_units["name"];
                var fetchXmltaxcode = $@"
                <fetch>
                  <entity name='bsd_taxcode'>
                    <attribute name='bsd_name' />
                    <attribute name='bsd_value' />
                    <filter type='and'>
                      <condition attribute='bsd_taxcodeid' operator='eq' value='{((EntityReference)enOptionEntry["bsd_taxcode"]).Id}'/>
                    </filter>
                  </entity>
                </fetch>";
                var EnColtaxcode = service.RetrieveMultiple(new FetchExpression(fetchXmltaxcode.ToString()));
                Entity EnTaxcode = EnColtaxcode.Entities[0];
                DateTime date_EDA = DateTime.Now;
                bool checkEDA = false;
                if (enOptionEntry.Contains("bsd_contractnumber") && enOptionEntry.Contains("bsd_contracttypedescription"))
                {
                    int bsd_contracttypedescription = ((OptionSetValue)enOptionEntry["bsd_contracttypedescription"]).Value;
                    if (bsd_contracttypedescription == 100000001 && enOptionEntry.Contains("bsd_contractdate"))//Local SPA
                    {
                        checkEDA = true;
                        date_EDA = RetrieveLocalTimeFromUTCTime((DateTime)enOptionEntry["bsd_contractdate"]);
                    }
                    else if ((bsd_contracttypedescription == 100000002 || bsd_contracttypedescription == 100000003) && enOptionEntry.Contains("bsd_signedcontractdate"))
                    //Foreigner SPA or Local SPA (VK)
                    {
                        checkEDA = true;
                        date_EDA = RetrieveLocalTimeFromUTCTime((DateTime)enOptionEntry["bsd_signedcontractdate"]);
                    }
                }
                if (checkEDA)
                {
                    if (checkInstallmentEDAyes(enOptionEntry.Id))// installment có eda = yes
                    {
                        string name = "";
                        if (bsd_project_type == 100000000)//land
                        {
                            name = "Thu tiền căn nhà ở số " + unitName;
                        }
                        else if (bsd_project_type == 100000001)//higt
                        {
                            name = "Thu tiền căn hộ " + unitName;
                        }
                        decimal bsd_amountofthisphase = 0;
                        decimal bsd_depositamount = 0;
                        var fetchXml = $@"<?xml version=""1.0"" encoding=""utf-16""?>
                            <fetch>
                              <entity name=""bsd_paymentschemedetail"">
                                <attribute name=""bsd_paymentschemedetailid"" />
                                <filter>
                                  <condition attribute=""bsd_installmentforeda"" operator=""eq"" value=""{true}"" />
                                  <condition attribute=""bsd_optionentry"" operator=""eq"" value=""{enOptionEntry.Id}"" />
                                  <condition attribute=""statuscode"" operator=""eq"" value=""{100000000}"" />
                                </filter>
                              </entity>
                            </fetch>";
                        EntityCollection list = service.RetrieveMultiple(new FetchExpression(fetchXml));
                        if (list.Entities.Count == 0)
                        {
                            var fetchXml2 = $@"<?xml version=""1.0"" encoding=""utf-16""?>
                                <fetch>
                                  <entity name=""bsd_paymentschemedetail"">
                                    <attribute name=""bsd_paymentschemedetailid"" />
                                    <attribute name=""bsd_amountofthisphase"" />
                                    <attribute name=""bsd_depositamount"" />
                                    <attribute name=""bsd_ordernumber"" />
                                    <filter>
                                      <condition attribute=""bsd_installmentforeda"" operator=""eq"" value=""{true}"" />
                                      <condition attribute=""bsd_optionentry"" operator=""eq"" value=""{enOptionEntry.Id}"" />
                                      <condition attribute=""statuscode"" operator=""eq"" value=""{100000001}"" />
                                    </filter>
                                    <order descending=""true"" attribute=""bsd_ordernumber"" />
                                  </entity>
                                </fetch>";
                            EntityCollection list2 = service.RetrieveMultiple(new FetchExpression(fetchXml2));
                            string guidID = "";
                            foreach (Entity entity in list2.Entities)
                            {
                                bsd_amountofthisphase += entity.Contains("bsd_amountofthisphase") ? ((Money)entity["bsd_amountofthisphase"]).Value : 0;
                                if (entity.Contains("bsd_depositamount")) bsd_depositamount = ((Money)entity["bsd_depositamount"]).Value;
                                if (guidID == "") guidID = entity.Id.ToString();
                            }
                            createInvoice(name, project_invoive, enOptionEntry, iv_units, EnTaxcode, 100000003, date_EDA, bsd_depositamount, bsd_amountofthisphase, 0);
                        }
                    }
                    else//ko installment có eda = yes
                    {
                        var fetchXml = $@"<?xml version=""1.0"" encoding=""utf-16""?>
                            <fetch>
                              <entity name=""bsd_paymentschemedetail"">
                                <attribute name=""bsd_paymentschemedetailid"" />
                                <attribute name=""statuscode"" />
                                <attribute name=""bsd_depositamount"" />
                                <attribute name=""bsd_amountofthisphase"" />
                                <attribute name=""bsd_paiddate"" />
                                <filter>
                                  <condition attribute=""bsd_ordernumber"" operator=""eq"" value=""{1}"" />
                                  <condition attribute=""bsd_optionentry"" operator=""eq"" value=""{enOptionEntry.Id}"" />
                                  <condition attribute=""statuscode"" operator=""eq"" value=""{100000001}"" />
                                </filter>
                              </entity>
                            </fetch>";
                        EntityCollection list = service.RetrieveMultiple(new FetchExpression(fetchXml));
                        foreach (Entity enIns in list.Entities)
                        {
                            int statuscode = ((OptionSetValue)enIns["statuscode"]).Value;
                            int bsd_ordernumber = (int)enIns["bsd_ordernumber"];
                            decimal bsd_depositamount = enIns.Contains("bsd_depositamount") ? ((Money)enIns["bsd_depositamount"]).Value : 0;
                            decimal amountPay = enIns.Contains("bsd_amountofthisphase") ? ((Money)enIns["bsd_amountofthisphase"]).Value : 0;
                            string name = "";
                            if (bsd_project_type == 100000000)//land
                            {
                                name = "Thu tiền căn nhà ở số " + unitName;
                            }
                            else if (bsd_project_type == 100000001)//higt
                            {
                                name = "Thu tiền căn hộ " + unitName;
                            }
                            string guidID = enIns.Id.ToString();
                            DateTime bsd_paiddate = RetrieveLocalTimeFromUTCTime((DateTime)enIns["bsd_paiddate"]);
                            createInvoice(name, project_invoive, enOptionEntry, iv_units, EnTaxcode, 100000003, bsd_paiddate, bsd_depositamount, amountPay, 0);
                        }
                    }
                }
            }
        }
        private void createInvoice(string bsd_name, Entity project_invoive, Entity optionentry_invoive, Entity iv_units, Entity EnTaxcode, int bsd_type, DateTime bsd_issueddate
            , decimal bsd_depositamount, decimal bsd_invoiceamount, decimal bsd_handoveramount)
        {
            traceService.Trace("vào createInvoice");
            Entity invoice = new Entity("bsd_invoice");
            invoice.Id = new Guid();
            invoice["bsd_name"] = bsd_name;
            invoice["bsd_project"] = project_invoive.ToEntityReference();
            invoice["bsd_optionentry"] = optionentry_invoive.ToEntityReference();
            string formno = project_invoive.Contains("bsd_formno") ? (string)project_invoive["bsd_formno"] : "";
            string serialno = project_invoive.Contains("bsd_serialno") ? (string)project_invoive["bsd_serialno"] : "";
            invoice["bsd_formno"] = formno;
            invoice["bsd_serialno"] = serialno;
            invoice["bsd_issueddate"] = bsd_issueddate;
            invoice["bsd_units"] = iv_units.ToEntityReference();
            invoice["bsd_purchaser"] = (EntityReference)optionentry_invoive["customerid"];
            invoice["bsd_paymentmethod"] = new OptionSetValue(100000000);
            invoice["bsd_type"] = new OptionSetValue(bsd_type);
            invoice["statuscode"] = new OptionSetValue(1);
            invoice["bsd_depositamount"] = bsd_depositamount;
            if (bsd_invoiceamount > 0)
            {
                traceService.Trace("vào bsd_invoiceamount > 0");
                decimal bsd_vatamount = Math.Round(bsd_invoiceamount / (decimal)EnTaxcode["bsd_value"] * 100, MidpointRounding.AwayFromZero);
                invoice["bsd_invoiceamount"] = new Money(bsd_invoiceamount);
                invoice["bsd_vatamount"] = new Money(bsd_vatamount);
                invoice["bsd_invoiceamountb4vat"] = new Money(bsd_invoiceamount - bsd_vatamount);
            }
            invoice["bsd_handoveramount"] = new Money(bsd_handoveramount);
            if (bsd_handoveramount > 0 && bsd_invoiceamount > 0) invoice["bsd_namelandvalue"] = "Giá trị quyền sử dụng đất không chịu thuế GTGT";
            invoice["bsd_taxcodevalue"] = EnTaxcode["bsd_value"];
            service.Create(invoice);
            traceService.Trace("ra createInvoice");
        }
        private bool checkInvaldInvoice1st(Guid optionEntryId)
        {
            var query = new QueryExpression("bsd_invoice");
            query.TopCount = 1;
            query.ColumnSet.AddColumn("bsd_invoiceid");
            query.Criteria.AddCondition("statuscode", ConditionOperator.In, 1, 100000000);
            query.Criteria.AddCondition("bsd_type", ConditionOperator.Equal, 100000003);//1st
            query.Criteria.AddCondition("bsd_optionentry", ConditionOperator.Equal, optionEntryId);
            EntityCollection list = service.RetrieveMultiple(query);
            return list.Entities.Count > 0 ? true : false;
        }
        private bool checkInstallmentEDAyes(Guid optionEntryId)
        {
            QueryExpression query =
                new QueryExpression("bsd_paymentschemedetail");

            query.ColumnSet =
                new ColumnSet("bsd_paymentschemedetailid");

            query.Criteria.AddCondition(
                "bsd_optionentry",
                ConditionOperator.Equal,
                optionEntryId);

            query.Criteria.AddCondition(
                "bsd_ordernumber",
                ConditionOperator.Equal,
                1);
            query.Criteria.AddCondition(
                "bsd_installmentforeda",
                ConditionOperator.Equal,
                true);

            EntityCollection list = service.RetrieveMultiple(query);

            return list.Entities.Count > 0 ? true : false;
        }
        private DateTime RetrieveLocalTimeFromUTCTime(DateTime utcTime)
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
