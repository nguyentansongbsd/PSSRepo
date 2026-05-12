using Microsoft.Crm.Sdk.Messages;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Query;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IdentityModel.Metadata;
using System.Linq;
using System.Linq.Expressions;
using System.Runtime.Remoting.Services;
using System.Security.Policy;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Action_OE_PrintContractDate_CtrNumber
{
    public class Action_OE_PrintContractDate_CtrNumber : IPlugin
    {
        private IOrganizationService service = (IOrganizationService)null;
        private IOrganizationServiceFactory factory = (IOrganizationServiceFactory)null;
        public ITracingService traceService = null;
        void IPlugin.Execute(IServiceProvider serviceProvider)
        {
            traceService = (ITracingService)serviceProvider.GetService(typeof(ITracingService));
            IPluginExecutionContext service1 = (IPluginExecutionContext)serviceProvider.GetService(typeof(IPluginExecutionContext));
            EntityReference inputParameter = (EntityReference)service1.InputParameters["Target"];
            if (!(inputParameter.LogicalName == "salesorder"))
                return;
            ITracingService service2 = (ITracingService)serviceProvider.GetService(typeof(ITracingService));
            this.factory = (IOrganizationServiceFactory)serviceProvider.GetService(typeof(IOrganizationServiceFactory));
            this.service = this.factory.CreateOrganizationService(new Guid?(service1.UserId));
            traceService.Trace("Vào Action_OE_PrintContractDate_CtrNumber");
            Entity entity1 = this.service.Retrieve("salesorder", inputParameter.Id, new ColumnSet(new string[10]
            {
                "statuscode",
                "bsd_signedcontractdate",
                "bsd_contractprinteddate",
                "bsd_paymentscheme",
                "ordernumber",
                "bsd_unitnumber",
                "bsd_contractnumber",
                "bsd_identifynumber",
                "bsd_project",
                "bsd_optionno"
            }));
            if (entity1.Contains("bsd_contractnumber") && entity1["bsd_contractnumber"] == null || !entity1.Contains("bsd_contractnumber"))
            {
                int num1 = ((OptionSetValue)entity1["statuscode"]).Value;
                Guid id = ((EntityReference)entity1["bsd_paymentscheme"]).Id;
                if (!entity1.Contains("ordernumber"))
                    throw new InvalidPluginExecutionException("Contract does not contain 'Option Number'!");
                if (!entity1.Contains("bsd_project"))
                    throw new InvalidPluginExecutionException("Contract does not contain 'Project'!");
                if (entity1.Contains("bsd_signedcontractdate"))
                    throw new InvalidPluginExecutionException("Option Entry has already signed!");
                if (!entity1.Contains("bsd_contractnumber"))
                {
                    Entity entity2 = new Entity("salesorder");
                    entity2.Id = entity1.Id;
                    Entity entity3 = this.service.Retrieve(((EntityReference)entity1["bsd_project"]).LogicalName, ((EntityReference)entity1["bsd_project"]).Id, new ColumnSet(new string[4]
                    {
            "bsd_currentnumber",
            "bsd_name",
            "bsd_prefixcontract",
            "bsd_length"

                    }));

                    if (!entity3.Contains("bsd_prefixcontract"))
                        throw new InvalidPluginExecutionException("Can not find Prefix (Contract) data in Project " + (entity3.Contains("bsd_name") ? (string)entity3["bsd_name"] : "") + ". Please check again!");
                    int num2 = entity3.Contains("bsd_length") ? (int)entity3["bsd_length"] : throw new InvalidPluginExecutionException("Can not find Length data in Project " + (entity3.Contains("bsd_name") ? (string)entity3["bsd_name"] : "") + ". Please check again!");
                    this.service.Retrieve("product", new Entity("product")
                    {
                        Id = this.getProductId(inputParameter.Id)
                    }.Id, new ColumnSet(new string[3]
                    {
            "statuscode",
            "bsd_signedcontractdate",
            "name"
                    }));
                    if (entity1.Contains("bsd_project"))
                    {
                        traceService.Trace("Vào case dự án lumi lấy trường bsd_prefixcontract bên dự án + 4 ký tự cuối trường optiono bên salesoder để gán cho trường contract number Thịnh làm");
                        Guid idpro = ((EntityReference)entity1["bsd_project"]).Id;
                        Guid check = new Guid("{779F0E92-CC3D-EF11-A316-6045BD1BAA9C}");
                        Guid check1 = new Guid("{FC2C423E-FA85-EF11-AC20-0022485725FB}");
                        Guid check2 = new Guid("{1D227A77-66F5-EE11-A1FE-000D3AC96AE0}");
                        Guid check3 = new Guid("{2F0AC773-AEA1-EF11-8A69-000D3AA3F0A0}");// SENIQUE PREMIER
                        Guid check4 = new Guid("{941833DF-A7A1-EF11-8A69-000D3AA3F0A0}");//SENIQ HN 
                        if (idpro == check || idpro == check1 || idpro == check2 || idpro == check3 || idpro == check4)
                        {
                            string projectCode = (string)entity1["bsd_optionno"];
                            string lastFourCharacters = projectCode.Substring(projectCode.Length - 4);
                            entity2["bsd_contractnumber"] = (object)((string)entity3["bsd_prefixcontract"] + lastFourCharacters);
                            this.service.Update(entity2);
                        }
                        else
                        {
                            int num3 = (entity3.Contains("bsd_currentnumber") ? (int)entity3["bsd_currentnumber"] : 0) + 1;
                            int num4 = num3;
                            string str = num3.ToString();
                            int num5 = num2 - str.Length;
                            for (int index = 0; index < num5; ++index)
                                str = "0" + str;
                            entity2["bsd_contractnumber"] = (object)((string)entity3["bsd_prefixcontract"] + str);
                            this.service.Update(entity2);
                            this.service.Update(new Entity(entity3.LogicalName)
                            {
                                Id = entity3.Id,
                                ["bsd_currentnumber"] = (object)num4
                            });
                        }
                    }
                }
            }
            entity1 = this.service.Retrieve("salesorder", inputParameter.Id, new ColumnSet(
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
            create_Invoice(entity1);
        }
        private void create_Invoice(Entity enOptionEntry)
        {
            if (!checkInvaldInvoice1st(enOptionEntry.Id))//chưa có invoice type 1st
            {
                Entity project_invoive = service.Retrieve("bsd_project", ((EntityReference)enOptionEntry["bsd_project"]).Id,
                new ColumnSet(new string[] {
                            "bsd_formno","bsd_serialno", "bsd_project_type", "bsd_optioncheckeinvoice"
                }));
                bool checkEInvoice =
                project_invoive.GetAttributeValue<bool>("bsd_optioncheckeinvoice");

                if (!checkEInvoice)
                    return;
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
                    string name = "";
                    if (bsd_project_type == 100000000)//land
                    {
                        name = "Thu tiền căn nhà ở số " + unitName;
                    }
                    else if (bsd_project_type == 100000001)//higt
                    {
                        name = "Thu tiền căn hộ " + unitName;
                    }
                    decimal bsd_amountwaspaid = 0;
                    decimal bsd_depositamount = 0;
                    var fetchXml2 = $@"<?xml version=""1.0"" encoding=""utf-16""?>
                                <fetch>
                                  <entity name=""bsd_paymentschemedetail"">
                                    <attribute name=""bsd_paymentschemedetailid"" />
                                    <attribute name=""bsd_amountwaspaid"" />
                                    <attribute name=""bsd_depositamount"" />
                                    <attribute name=""bsd_ordernumber"" />
                                    <filter>
                                      <condition attribute=""bsd_optionentry"" operator=""eq"" value=""{enOptionEntry.Id}"" />
                                      <condition attribute=""statecode"" operator=""eq"" value=""{0}"" />
                                      <condition attribute=""bsd_amountwaspaid"" operator=""gt"" value=""{0}"" />
                                    </filter>
                                    <order descending=""true"" attribute=""bsd_ordernumber"" />
                                  </entity>
                                </fetch>";
                    EntityCollection list2 = service.RetrieveMultiple(new FetchExpression(fetchXml2));
                    foreach (Entity entity in list2.Entities)
                    {
                        bsd_amountwaspaid += entity.Contains("bsd_amountwaspaid") ? ((Money)entity["bsd_amountwaspaid"]).Value : 0;
                        if (entity.Contains("bsd_depositamount")) bsd_depositamount = ((Money)entity["bsd_depositamount"]).Value;
                    }
                    CreateInvoice(name, project_invoive, enOptionEntry, iv_units, EnTaxcode, 100000003, date_EDA, bsd_depositamount, bsd_amountwaspaid, 0);
                }
            }
        }
        private void CreateInvoice(
            string invoiceName,
            Entity project,
            Entity optionEntry,
            Entity unit,
            Entity taxCode,
            int invoiceType,
            DateTime issueDate,
            decimal depositAmount,
            decimal invoiceAmount,
        decimal handoverAmount)
        {
            traceService.Trace("CreateInvoice");

            Entity invoice = new Entity("bsd_invoice");

            invoice["bsd_name"] = invoiceName;
            invoice["bsd_project"] = project.ToEntityReference();
            invoice["bsd_optionentry"] = optionEntry.ToEntityReference();

            invoice["bsd_formno"] =
                project.GetAttributeValue<string>("bsd_formno");

            invoice["bsd_serialno"] =
                project.GetAttributeValue<string>("bsd_serialno");

            invoice["bsd_issueddate"] = issueDate;
            invoice["bsd_units"] = unit.ToEntityReference();

            EntityReference purchaser =
                optionEntry.GetAttributeValue<EntityReference>("customerid");

            if (purchaser != null)
            {
                invoice["bsd_purchaser"] = purchaser;

                if (purchaser.LogicalName == "contact")
                    invoice["bsd_purchasernamecustomer"] = purchaser;
                else
                    invoice["bsd_purchasernamecompany"] = purchaser;
            }

            invoice["bsd_paymentmethod"] =
                new OptionSetValue(100000000);

            invoice["bsd_type"] =
                new OptionSetValue(invoiceType);

            invoice["statuscode"] =
            new OptionSetValue(1);

            invoice["bsd_depositamount"] =
                new Money(depositAmount);

            decimal taxValue =
                taxCode.GetAttributeValue<decimal>("bsd_value");

            if (invoiceAmount > 0 && invoiceType != 100000006)
            {
                if (invoiceType == 100000001)
                {
                    invoice["bsd_invoiceamount"] =
                    new Money(invoiceAmount);

                    invoice["bsd_vatamount"] =
                    new Money(0);

                    invoice["bsd_invoiceamountb4vat"] =
                        new Money(invoiceAmount);
                }
                else
                {
                    decimal vatAmount =
                        Math.Round(
                            invoiceAmount * taxValue / 100,
                            MidpointRounding.AwayFromZero);

                    invoice["bsd_invoiceamount"] =
                        new Money(invoiceAmount);

                    invoice["bsd_vatamount"] =
                        new Money(vatAmount);

                    invoice["bsd_invoiceamountb4vat"] =
                        new Money(invoiceAmount - vatAmount);
                }
            }
            else if (handoverAmount > 0 && invoiceType == 100000006)
            {
                invoice["bsd_invoiceamount"] =
                new Money(handoverAmount);

                invoice["bsd_vatamount"] =
                new Money(0);

                invoice["bsd_invoiceamountb4vat"] =
                    new Money(handoverAmount);
            }

            if (invoiceType == 100000005)
            {
                invoice["bsd_handoveramount"] =
                    new Money(handoverAmount);

                invoice["bsd_namelandvalue"] =
                    "Giá trị quyền sử dụng đất không chịu thuế GTGT";
            }

            invoice["bsd_taxcodevalue"] =
                (invoiceType == 100000001 || invoiceType == 100000006)
                ? 0
                : taxValue;

            service.Create(invoice);
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
        private Guid getProductId(Guid salesorderId)
        {
            Guid productId = new Guid();
            StringBuilder stringBuilder = new StringBuilder();
            stringBuilder.AppendLine("<fetch version='1.0' output-format='xml-platform' mapping='logical' distinct='true'>");
            stringBuilder.AppendLine("<entity name='salesorderdetail'>");
            stringBuilder.AppendLine("<attribute name='productid' />");
            stringBuilder.AppendLine("<filter type='and'>");
            stringBuilder.AppendLine(string.Format("<condition attribute='salesorderid' operator='eq' value='{0}'/>", (object)salesorderId));
            stringBuilder.AppendLine("</filter>");
            stringBuilder.AppendLine("</entity>");
            stringBuilder.AppendLine("</fetch>");
            EntityCollection entityCollection = this.service.RetrieveMultiple((QueryBase)new FetchExpression(stringBuilder.ToString()));
            if (entityCollection.Entities.Count <= 0)
                throw new InvalidPluginExecutionException("Cannot find product information in this Contract!");
            foreach (Entity entity in (Collection<Entity>)entityCollection.Entities)
                productId = ((EntityReference)entity.Attributes["productid"]).Id;
            return productId;
        }
    }
}
