using Microsoft.Crm.Sdk.Messages;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Query;
using System;
using System.Collections.Generic;
namespace Plugin_Create_Invoice_ApplyDocument
{
    public class Plugin_Create_Invoice_ApplyDocument : IPlugin
    {
        IOrganizationService service = null;
        IOrganizationServiceFactory factory = null;
        IPluginExecutionContext context = null;
        ITracingService traceService = null;
        public void Execute(IServiceProvider serviceProvider)
        {
            context = (IPluginExecutionContext)serviceProvider.GetService(typeof(IPluginExecutionContext));
            Entity target = (Entity)context.InputParameters["Target"];
            factory = (IOrganizationServiceFactory)serviceProvider.GetService(typeof(IOrganizationServiceFactory));
            service = factory.CreateOrganizationService(context.UserId);
            traceService = (ITracingService)serviceProvider.GetService(typeof(ITracingService));
            if (context.Depth > 2)
                return;
            traceService.Trace("vào Plugin_Create_Invoice_ApplyDocument");
            if (target.Contains("statuscode") && ((OptionSetValue)target["statuscode"]).Value == 100000002)
            {
                var EnApplyDocument = service.Retrieve(target.LogicalName, target.Id, new ColumnSet(true));
                processApplyDocument(EnApplyDocument);
            }
        }
        public void processApplyDocument(Entity EnApplyDocument)
        {
            traceService.Trace("vào processApplyDocument");
            int bsd_paymenttype = EnApplyDocument.Contains("bsd_transactiontype") ? ((OptionSetValue)EnApplyDocument["bsd_transactiontype"]).Value : 0;
            if ((bsd_paymenttype == 2 || bsd_paymenttype == 4) && EnApplyDocument.Contains("bsd_optionentry"))
            {
                DateTime bsd_paymentactualtime = RetrieveLocalTimeFromUTCTime((DateTime)EnApplyDocument["bsd_receiptdate"]);
                DateTime date_EDA = DateTime.Now;
                Entity optionentry_invoive = service.Retrieve("salesorder", ((EntityReference)EnApplyDocument["bsd_optionentry"]).Id,
                new ColumnSet(new string[] { "bsd_landvaluededuction", "bsd_taxcode", "bsd_depositamount", "customerid", "bsd_contractnumber", "bsd_contracttypedescription",
                "bsd_contractdate","bsd_signedcontractdate"}));
                bool checkEDA = false;
                if (optionentry_invoive.Contains("bsd_contractnumber") && optionentry_invoive.Contains("bsd_contracttypedescription"))
                {
                    int bsd_contracttypedescription = ((OptionSetValue)optionentry_invoive["bsd_contracttypedescription"]).Value;
                    if (bsd_contracttypedescription == 100000001 && optionentry_invoive.Contains("bsd_contractdate"))//Local SPA
                    {
                        checkEDA = true;
                        date_EDA = RetrieveLocalTimeFromUTCTime((DateTime)optionentry_invoive["bsd_contractdate"]);
                    }
                    else if ((bsd_contracttypedescription == 100000002 || bsd_contracttypedescription == 100000002) && optionentry_invoive.Contains("bsd_signedcontractdate"))
                    //Foreigner SPA or Local SPA (VK)
                    {
                        checkEDA = true;
                        date_EDA = RetrieveLocalTimeFromUTCTime((DateTime)optionentry_invoive["bsd_signedcontractdate"]);
                    }
                }
                decimal land_value = optionentry_invoive.Contains("bsd_landvaluededuction") ? ((Money)optionentry_invoive["bsd_landvaluededuction"]).Value : 0;
                Entity project_invoive = service.Retrieve("bsd_project", ((EntityReference)EnApplyDocument["bsd_project"]).Id,
                    new ColumnSet(new string[] {
                            "bsd_formno","bsd_serialno", "bsd_project_type"
                    }));
                int bsd_project_type = project_invoive.Contains("bsd_project_type") ? ((OptionSetValue)project_invoive["bsd_project_type"]).Value : 0;
                EntityReference units = (EntityReference)EnApplyDocument["bsd_units"];
                Entity iv_units = service.Retrieve(units.LogicalName, units.Id, new ColumnSet(new string[] { "name" }));
                string unitName = (string)iv_units["name"];
                var fetchXmltaxcode = $@"
                <fetch>
                  <entity name='bsd_taxcode'>
                    <attribute name='bsd_name' />
                    <attribute name='bsd_value' />
                    <filter type='and'>
                      <condition attribute='bsd_taxcodeid' operator='eq' value='{((EntityReference)optionentry_invoive["bsd_taxcode"]).Id}'/>
                    </filter>
                  </entity>
                </fetch>";
                var EnColtaxcode = service.RetrieveMultiple(new FetchExpression(fetchXmltaxcode.ToString()));
                Entity EnTaxcode = EnColtaxcode.Entities[0];
                // set list Installment
                traceService.Trace("set list Installment");
                List<Installment> ins_EDA = new List<Installment>();
                List<Installment> ins_NOT_EDA = new List<Installment>();
                traceService.Trace("get list Installment");
                var fetchXmlListIns = $@"<?xml version=""1.0"" encoding=""utf-16""?>
                <fetch>
                  <entity name=""bsd_applydocumentdetail"">
                    <attribute name=""bsd_installment"" />
                    <attribute name=""bsd_amountapply"" />
                    <filter>
                      <condition attribute=""bsd_paymenttype"" operator=""eq"" value=""{100000001}"" />
                      <condition attribute=""bsd_amountapply"" operator=""gt"" value=""{0}"" />
                      <condition attribute=""bsd_installment"" operator=""not-null"" />
                      <condition attribute=""bsd_applydocument"" operator=""eq"" value=""{EnApplyDocument.Id}"" />
                    </filter>
                  </entity>
                </fetch>";
                EntityCollection ListIns = service.RetrieveMultiple(new FetchExpression(fetchXmlListIns));
                foreach (Entity itemIns in ListIns.Entities)
                {
                    Installment arrIns = new Installment();
                    arrIns.id = ((EntityReference)itemIns["bsd_installment"]).Id;
                    arrIns.amount = ((Money)itemIns["bsd_amountapply"]).Value;
                    int abd = check_EDA(arrIns.id);
                    if (abd == 0)//ins_EDA
                        ins_EDA.Add(arrIns);
                    else if (abd == 1)//ins_NOT_EDA
                        ins_NOT_EDA.Add(arrIns);
                }
                traceService.Trace("set list Installment 2");
                int inType = 100000000;
                // EDA = YES
                if (checkEDA && ins_EDA.Count > 0)
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
                                  <condition attribute=""bsd_installmentforeda"" operator=""eq"" value=""{1}"" />
                                  <condition attribute=""bsd_optionentry"" operator=""eq"" value=""{optionentry_invoive.Id}"" />
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
                                      <condition attribute=""bsd_installmentforeda"" operator=""eq"" value=""{1}"" />
                                      <condition attribute=""bsd_optionentry"" operator=""eq"" value=""{optionentry_invoive.Id}"" />
                                      <condition attribute=""statuscode"" operator=""eq"" value=""{100000001}"" />
                                    </filter>
                                  </entity>
                                </fetch>";
                        EntityCollection list2 = service.RetrieveMultiple(new FetchExpression(fetchXml2));
                        foreach (Entity entity in list2.Entities)
                        {
                            bsd_amountofthisphase += entity.Contains("bsd_amountofthisphase") ? ((Money)entity["bsd_amountofthisphase"]).Value : 0;
                            if (entity.Contains("bsd_depositamount")) bsd_depositamount = ((Money)entity["bsd_depositamount"]).Value;
                            int bsd_ordernumber = (int)entity["bsd_ordernumber"];
                            if (bsd_ordernumber == 1) inType = 100000003;
                        }
                        createInvoice(name, project_invoive, optionentry_invoive, iv_units, EnApplyDocument, EnTaxcode, inType, date_EDA, bsd_depositamount, bsd_amountofthisphase, 0);
                    }
                }
                traceService.Trace("ra eda yes");
                // EDA = NO
                if (ins_NOT_EDA.Count > 0)
                {
                    string name = "";
                    decimal sumTypeIns = 0;
                    foreach (Installment item in ins_NOT_EDA)
                    {
                        Entity enIns = service.Retrieve("bsd_paymentschemedetail", item.id, new ColumnSet(
                        new string[] { "statuscode", "bsd_depositamount", "bsd_duedatecalculatingmethod", "bsd_ordernumber", "bsd_amountofthisphase" }));
                        int statuscode = ((OptionSetValue)enIns["statuscode"]).Value;
                        int bsd_ordernumber = (int)enIns["bsd_ordernumber"];
                        int bsd_duedatecalculatingmethod = enIns.Contains("bsd_duedatecalculatingmethod") ? ((OptionSetValue)enIns["bsd_duedatecalculatingmethod"]).Value : 0;
                        decimal bsd_depositamount = enIns.Contains("bsd_depositamount") ? ((Money)enIns["bsd_depositamount"]).Value : 0;
                        decimal bsd_amountofthisphase = enIns.Contains("bsd_amountofthisphase") ? ((Money)enIns["bsd_amountofthisphase"]).Value : 0;
                        decimal amountPay = item.amount;
                        if (bsd_duedatecalculatingmethod == 100000002)// Estimate handover date
                        {
                            decimal landvalueIN = sumLandValueVoice(optionentry_invoive.Id);
                            decimal bsd_handoveramount = land_value - landvalueIN;
                            if (bsd_handoveramount < 0) bsd_handoveramount = 0;
                            name = "Giá trị quyền sử dụng đất không chịu thuế GTGT";
                            if (amountPay <= bsd_handoveramount)
                            {
                                bsd_handoveramount = amountPay;
                                amountPay = 0;
                                inType = 100000006;
                            }
                            else
                            {
                                inType = 100000005;
                                if (bsd_handoveramount == 0)
                                    inType = 100000007;
                                amountPay -= bsd_handoveramount;
                                if (bsd_project_type == 100000000)//land
                                {
                                    name = "Thu tiền căn nhà ở số " + unitName;
                                }
                                else if (bsd_project_type == 100000001)//higt
                                {
                                    name = "Thu tiền căn hộ " + unitName;
                                }
                                else name = "";
                            }
                            createInvoice(name, project_invoive, optionentry_invoive, iv_units, EnApplyDocument, EnTaxcode, inType, bsd_paymentactualtime, bsd_depositamount, amountPay, bsd_handoveramount);
                            if (statuscode == 100000001)//sts=paid
                            {
                                createInvoice(name, project_invoive, optionentry_invoive, iv_units, EnApplyDocument, EnTaxcode, 100000004, bsd_paymentactualtime, 0, getInstallmentLast(optionentry_invoive.Id), 0);
                            }
                        }
                        else
                        {
                            if (bsd_ordernumber == 1)
                            {
                                if (checkEDA && statuscode == 100000001)
                                {
                                    if (bsd_project_type == 100000000)//land
                                    {
                                        name = "Thu tiền căn nhà ở số " + unitName;
                                    }
                                    else if (bsd_project_type == 100000001)//higt
                                    {
                                        name = "Thu tiền căn hộ " + unitName;
                                    }
                                    else name = "";
                                    inType = 100000003;
                                    createInvoice(name, project_invoive, optionentry_invoive, iv_units, EnApplyDocument, EnTaxcode, inType, date_EDA, bsd_depositamount, bsd_amountofthisphase, 0);
                                }
                            }
                            else
                            {
                                sumTypeIns += amountPay;
                            }
                        }
                    }
                    if (sumTypeIns > 0)
                    {
                        if (bsd_project_type == 100000000)//land
                        {
                            name = "Thu tiền căn nhà ở số " + unitName;
                        }
                        else if (bsd_project_type == 100000001)//higt
                        {
                            name = "Thu tiền căn hộ " + unitName;
                        }
                        else name = "";
                        createInvoice(name, project_invoive, optionentry_invoive, iv_units, EnApplyDocument, EnTaxcode, 100000000, bsd_paymentactualtime, 0, sumTypeIns, 0);
                    }
                }
                traceService.Trace("ra eda no");
                //case thanh toán paid main fee
                var fetchXmlMainFee = $@"<?xml version=""1.0"" encoding=""utf-16""?>
                <fetch>
                  <entity name=""bsd_paymentschemedetail"">
                    <attribute name=""bsd_maintenanceamount"" />
                    <filter>
                      <condition attribute=""bsd_maintenancefeesstatus"" operator=""eq"" value=""{1}"" />
                      <condition attribute=""bsd_maintenancefees"" operator=""eq"" value=""{1}"" />
                    </filter>
                    <link-entity name=""bsd_applydocumentdetail"" from=""bsd_installment"" to=""bsd_paymentschemedetailid"" alias=""ADD"">
                      <filter>
                        <condition attribute=""bsd_applydocument"" operator=""eq"" value=""{EnApplyDocument.Id}"" />
                        <condition attribute=""bsd_paymenttype"" operator=""eq"" value=""{100000002}"" />
                        <condition attribute=""bsd_feetype"" operator=""eq"" value=""{100000000}"" />
                      </filter>
                    </link-entity>
                  </entity>
                </fetch>";
                EntityCollection listMainFee = service.RetrieveMultiple(new FetchExpression(fetchXmlMainFee));
                foreach (Entity item in listMainFee.Entities)
                {
                    string name = "";
                    if (bsd_project_type == 100000000)//land
                    {
                        name = "Thu tiền kinh phí bảo trì căn nhà ở số " + unitName;
                    }
                    else if (bsd_project_type == 100000001)//higt
                    {
                        name = "Thu tiền kinh phí bảo trì căn hộ " + unitName;
                    }
                    decimal bsd_maintenanceamount = item.Contains("bsd_maintenanceamount") ? ((Money)item["bsd_maintenanceamount"]).Value : 0;
                    createInvoice(name, project_invoive, optionentry_invoive, iv_units, EnApplyDocument, EnTaxcode, 100000001, bsd_paymentactualtime, 0, bsd_maintenanceamount, 0);
                }
                traceService.Trace("ra main");
            }
        }
        private int check_EDA(Guid id)
        {
            int numb = 2;
            var fetchXml = $@"<?xml version=""1.0"" encoding=""utf-16""?>
                            <fetch>
                              <entity name=""bsd_paymentschemedetail"">
                                <attribute name=""bsd_installmentforeda"" />
                                <filter>
                                  <condition attribute=""bsd_paymentschemedetailid"" operator=""eq"" value=""{id}"" />
                                  <condition attribute=""bsd_lastinstallment"" operator=""ne"" value=""1"" />
                                </filter>
                              </entity>
                            </fetch>";
            EntityCollection list = service.RetrieveMultiple(new FetchExpression(fetchXml));
            foreach (Entity item in list.Entities)
            {
                bool bsd_installmentforeda = item.Contains("bsd_installmentforeda") ? (bool)item["bsd_installmentforeda"] : false;
                if (bsd_installmentforeda) numb = 0;
                else numb = 1;
            }
            return numb;
        }
        private decimal getInstallmentLast(Guid enOE)
        {
            decimal sum = 0;
            var fetchXml = $@"<?xml version=""1.0"" encoding=""utf-16""?>
                            <fetch>
                              <entity name=""bsd_paymentschemedetail"">
                                <attribute name=""bsd_amountofthisphase"" />
                                <filter>
                                  <condition attribute=""bsd_optionentry"" operator=""ne"" value=""{enOE}"" />
                                  <condition attribute=""bsd_amountofthisphase"" operator=""gt"" value=""0"" />
                                  <condition attribute=""bsd_lastinstallment"" operator=""eq"" value=""1"" />
                                </filter>
                              </entity>
                            </fetch>";
            EntityCollection list = service.RetrieveMultiple(new FetchExpression(fetchXml));
            foreach (Entity item in list.Entities)
            {
                sum += item.Contains("bsd_amountofthisphase") ? ((Money)item["bsd_amountofthisphase"]).Value : 0;
            }
            return sum;
        }
        private decimal sumLandValueVoice(Guid enOE)
        {
            traceService.Trace("vào sumLandValueVoice");
            decimal sum = 0;
            var fetchXml = $@"<?xml version=""1.0"" encoding=""utf-16""?>
                            <fetch>
                              <entity name=""bsd_invoice"">
                                <attribute name=""bsd_handoveramount"" />
                                <filter>
                                  <condition attribute=""bsd_optionentry"" operator=""eq"" value=""{enOE}"" />
                                  <condition attribute=""bsd_handoveramount"" operator=""gt"" value=""0"" />
                                  <condition attribute=""statuscode"" operator=""in"">
                                    <value>{1}</value>
                                    <value>{100000000}</value>
                                  </condition>
                                </filter>
                              </entity>
                            </fetch>";
            EntityCollection list = service.RetrieveMultiple(new FetchExpression(fetchXml));
            foreach (Entity item in list.Entities)
            {
                sum += item.Contains("bsd_handoveramount") ? ((Money)item["bsd_handoveramount"]).Value : 0;
            }
            traceService.Trace("ra sumLandValueVoice");
            return sum;
        }
        private void createInvoice(string bsd_name, Entity project_invoive, Entity optionentry_invoive, Entity iv_units, Entity EnApplyDocument, Entity EnTaxcode, int bsd_type, DateTime bsd_issueddate
            , decimal bsd_depositamount, decimal bsd_invoiceamount, decimal bsd_handoveramount)
        {
            traceService.Trace("vào createInvoice");
            Entity invoice = new Entity("bsd_invoice");
            invoice.Id = new Guid();
            invoice["bsd_name"] = bsd_name;
            invoice["bsd_project"] = project_invoive.ToEntityReference();
            invoice["bsd_optionentry"] = optionentry_invoive.ToEntityReference();
            invoice["bsd_applydocument"] = EnApplyDocument.ToEntityReference();
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
            if (bsd_invoiceamount > 0 && bsd_type != 100000006)
            {
                traceService.Trace("vào bsd_invoiceamount > 0");
                if (bsd_type == 100000001)
                {
                    invoice["bsd_invoiceamount"] = new Money(bsd_invoiceamount);
                    invoice["bsd_vatamount"] = new Money(0);
                    invoice["bsd_invoiceamountb4vat"] = new Money(bsd_invoiceamount);
                }
                else
                {
                    decimal bsd_vatamount = Math.Round(bsd_invoiceamount * (decimal)EnTaxcode["bsd_value"] / 100, MidpointRounding.AwayFromZero);
                    invoice["bsd_invoiceamount"] = new Money(bsd_invoiceamount);
                    invoice["bsd_vatamount"] = new Money(bsd_vatamount);
                    invoice["bsd_invoiceamountb4vat"] = new Money(bsd_invoiceamount - bsd_vatamount);
                }
            }
            else if (bsd_handoveramount > 0 && bsd_type == 100000006)
            {
                invoice["bsd_invoiceamount"] = new Money(bsd_handoveramount);
                invoice["bsd_vatamount"] = new Money(0);
                invoice["bsd_invoiceamountb4vat"] = new Money(bsd_handoveramount);
            }
            if (bsd_type == 100000005)
            {
                invoice["bsd_handoveramount"] = new Money(bsd_handoveramount);
                invoice["bsd_namelandvalue"] = "Giá trị quyền sử dụng đất không chịu thuế GTGT";
            }
            if (bsd_type == 100000001 || bsd_type == 100000006) invoice["bsd_taxcodevalue"] = new decimal(0);
            else invoice["bsd_taxcodevalue"] = EnTaxcode["bsd_value"];
            service.Create(invoice);
            traceService.Trace("ra createInvoice");
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
    public class Installment
    {
        public Guid id { get; set; }
        public decimal amount { get; set; }
    }
}