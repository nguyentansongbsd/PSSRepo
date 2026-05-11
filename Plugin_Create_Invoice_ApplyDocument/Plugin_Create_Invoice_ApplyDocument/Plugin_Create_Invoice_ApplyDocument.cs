using Microsoft.Crm.Sdk.Messages;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Query;
using System;
using System.Collections.Generic;

namespace Plugin_Create_Invoice_ApplyDocument
{
    public class Plugin_Create_Invoice_ApplyDocument : IPlugin
    {
        private IOrganizationService service = null;
        private IOrganizationServiceFactory factory = null;
        private IPluginExecutionContext context = null;
        private ITracingService traceService = null;

        public void Execute(IServiceProvider serviceProvider)
        {
            context = (IPluginExecutionContext)serviceProvider.GetService(typeof(IPluginExecutionContext));

            if (!context.InputParameters.Contains("Target") ||
                !(context.InputParameters["Target"] is Entity target))
            {
                return;
            }

            factory = (IOrganizationServiceFactory)serviceProvider.GetService(typeof(IOrganizationServiceFactory));
            service = factory.CreateOrganizationService(context.UserId);
            traceService = (ITracingService)serviceProvider.GetService(typeof(ITracingService));

            if (context.Depth > 2)
                return;

            traceService.Trace("vào Plugin_Create_Invoice_ApplyDocument");

            if (target.Contains("statuscode") &&
                ((OptionSetValue)target["statuscode"]).Value == 100000002)
            {
                Entity EnApplyDocument = service.Retrieve(
                    target.LogicalName,
                    target.Id,
                    new ColumnSet(true));

                ProcessApplyDocument(EnApplyDocument);
            }
        }

        private void ProcessApplyDocument(Entity EnApplyDocument)
        {
            traceService.Trace("vào ProcessApplyDocument");

            int bsd_paymenttype = EnApplyDocument.Contains("bsd_transactiontype")
                ? ((OptionSetValue)EnApplyDocument["bsd_transactiontype"]).Value
                : 0;

            Entity project_invoive = service.Retrieve(
                "bsd_project",
                ((EntityReference)EnApplyDocument["bsd_project"]).Id,
                new ColumnSet(
                    "bsd_formno",
                    "bsd_serialno",
                    "bsd_optioncheckeinvoice",
                    "bsd_project_type"));

            bool bsd_optioncheckeinvoice = project_invoive.Contains("bsd_optioncheckeinvoice")
                ? (bool)project_invoive["bsd_optioncheckeinvoice"]
                : false;

            if ((bsd_paymenttype != 2 && bsd_paymenttype != 4)
                || !EnApplyDocument.Contains("bsd_optionentry")
                || !bsd_optioncheckeinvoice)
            {
                return;
            }

            DateTime bsd_paymentactualtime =
                RetrieveLocalTimeFromUTCTime((DateTime)EnApplyDocument["bsd_receiptdate"]);

            Entity optionentry_invoive = service.Retrieve(
                "salesorder",
                ((EntityReference)EnApplyDocument["bsd_optionentry"]).Id,
                new ColumnSet(
                    "bsd_landvaluededuction",
                    "bsd_taxcode",
                    "bsd_depositamount",
                    "customerid",
                    "bsd_contractnumber",
                    "bsd_contracttypedescription",
                    "bsd_contractdate",
                    "bsd_signedcontractdate"));

            bool checkEDA = false;
            DateTime date_EDA = DateTime.Now;

            if (optionentry_invoive.Contains("bsd_contractnumber")
                && optionentry_invoive.Contains("bsd_contracttypedescription"))
            {
                int bsd_contracttypedescription =
                    ((OptionSetValue)optionentry_invoive["bsd_contracttypedescription"]).Value;

                if (bsd_contracttypedescription == 100000001
                    && optionentry_invoive.Contains("bsd_contractdate"))
                {
                    checkEDA = true;

                    date_EDA = RetrieveLocalTimeFromUTCTime(
                        (DateTime)optionentry_invoive["bsd_contractdate"]);
                }
                else if (
                    (bsd_contracttypedescription == 100000002
                    || bsd_contracttypedescription == 100000003)
                    && optionentry_invoive.Contains("bsd_signedcontractdate"))
                {
                    checkEDA = true;

                    date_EDA = RetrieveLocalTimeFromUTCTime(
                        (DateTime)optionentry_invoive["bsd_signedcontractdate"]);
                }
            }

            decimal land_value =
                optionentry_invoive.Contains("bsd_landvaluededuction")
                ? ((Money)optionentry_invoive["bsd_landvaluededuction"]).Value
                : 0;

            int bsd_project_type =
                project_invoive.Contains("bsd_project_type")
                ? ((OptionSetValue)project_invoive["bsd_project_type"]).Value
                : 0;

            EntityReference units = (EntityReference)EnApplyDocument["bsd_units"];

            Entity iv_units = service.Retrieve(
                units.LogicalName,
                units.Id,
                new ColumnSet("name"));

            string unitName = iv_units.Contains("name")
                ? iv_units["name"].ToString()
                : "";

            Entity EnTaxcode = GetTaxCode(optionentry_invoive);

            List<Installment> ins_EDA = new List<Installment>();
            List<Installment> ins_NOT_EDA = new List<Installment>();

            EntityCollection ListIns = GetInstallments(EnApplyDocument.Id);

            foreach (Entity itemIns in ListIns.Entities)
            {
                Installment arrIns = new Installment
                {
                    id = ((EntityReference)itemIns["bsd_installment"]).Id,
                    amount = ((Money)itemIns["bsd_amountapply"]).Value
                };

                int abd = Check_EDA(arrIns.id);

                if (abd == 0)
                    ins_EDA.Add(arrIns);
                else if (abd == 1)
                    ins_NOT_EDA.Add(arrIns);
            }

            ProcessEDAInvoice(
                checkEDA,
                ins_EDA,
                project_invoive,
                optionentry_invoive,
                iv_units,
                EnApplyDocument,
                EnTaxcode,
                date_EDA,
                bsd_project_type,
                unitName);

            ProcessNonEDAInvoice(
                ins_NOT_EDA,
                project_invoive,
                optionentry_invoive,
                iv_units,
                EnApplyDocument,
                EnTaxcode,
                bsd_paymentactualtime,
                bsd_project_type,
                unitName,
                checkEDA,
                date_EDA,
                land_value);

            ProcessMaintenanceFee(
                EnApplyDocument,
                project_invoive,
                optionentry_invoive,
                iv_units,
                EnTaxcode,
                bsd_paymentactualtime,
                bsd_project_type,
                unitName);

            traceService.Trace("ra ProcessApplyDocument");
        }

        private void ProcessEDAInvoice(
            bool checkEDA,
            List<Installment> ins_EDA,
            Entity project_invoive,
            Entity optionentry_invoive,
            Entity iv_units,
            Entity EnApplyDocument,
            Entity EnTaxcode,
            DateTime date_EDA,
            int bsd_project_type,
            string unitName)
        {
            traceService.Trace("vào ProcessEDAInvoice");

            if (!checkEDA || ins_EDA.Count <= 0)
                return;

            string name = GetInvoiceName(bsd_project_type, unitName);

            decimal bsd_amountofthisphase = 0;
            decimal bsd_depositamount = 0;
            int inType = 100000000;

            var fetchXml = $@"<?xml version=""1.0"" encoding=""utf-16""?>
            <fetch>
              <entity name=""bsd_paymentschemedetail"">
                <attribute name=""bsd_paymentschemedetailid"" />
                <filter>
                  <condition attribute=""bsd_installmentforeda"" operator=""eq"" value=""1"" />
                  <condition attribute=""bsd_optionentry"" operator=""eq"" value=""{optionentry_invoive.Id}"" />
                  <condition attribute=""statuscode"" operator=""eq"" value=""100000000"" />
                </filter>
              </entity>
            </fetch>";

            EntityCollection list =
                service.RetrieveMultiple(new FetchExpression(fetchXml));

            if (list.Entities.Count == 0)
            {
                var fetchXml2 = $@"<?xml version=""1.0"" encoding=""utf-16""?>
                <fetch>
                  <entity name=""bsd_paymentschemedetail"">
                    <attribute name=""bsd_amountofthisphase"" />
                    <attribute name=""bsd_depositamount"" />
                    <attribute name=""bsd_ordernumber"" />

                    <filter>
                      <condition attribute=""bsd_installmentforeda"" operator=""eq"" value=""1"" />
                      <condition attribute=""bsd_optionentry"" operator=""eq"" value=""{optionentry_invoive.Id}"" />
                      <condition attribute=""statuscode"" operator=""eq"" value=""100000001"" />
                    </filter>
                  </entity>
                </fetch>";

                EntityCollection list2 =
                    service.RetrieveMultiple(new FetchExpression(fetchXml2));

                foreach (Entity entity in list2.Entities)
                {
                    bsd_amountofthisphase += entity.Contains("bsd_amountofthisphase")
                        ? ((Money)entity["bsd_amountofthisphase"]).Value
                        : 0;

                    if (entity.Contains("bsd_depositamount"))
                    {
                        bsd_depositamount =
                            ((Money)entity["bsd_depositamount"]).Value;
                    }

                    int bsd_ordernumber = (int)entity["bsd_ordernumber"];

                    if (bsd_ordernumber == 1)
                        inType = 100000003;
                }

                CreateInvoice(
                    name,
                    project_invoive,
                    optionentry_invoive,
                    iv_units,
                    EnApplyDocument,
                    EnTaxcode,
                    inType,
                    date_EDA,
                    bsd_depositamount,
                    bsd_amountofthisphase,
                    0);
            }

            traceService.Trace("ra ProcessEDAInvoice");
        }

        private void ProcessNonEDAInvoice(
            List<Installment> ins_NOT_EDA,
            Entity project_invoive,
            Entity optionentry_invoive,
            Entity iv_units,
            Entity EnApplyDocument,
            Entity EnTaxcode,
            DateTime bsd_paymentactualtime,
            int bsd_project_type,
            string unitName,
            bool checkEDA,
            DateTime date_EDA,
            decimal land_value)
        {
            traceService.Trace("vào ProcessNonEDAInvoice");

            if (ins_NOT_EDA.Count <= 0)
                return;

            decimal sumTypeIns = 0;

            foreach (Installment item in ins_NOT_EDA)
            {
                Entity enIns = service.Retrieve(
                    "bsd_paymentschemedetail",
                    item.id,
                    new ColumnSet(
                        "statuscode",
                        "bsd_depositamount",
                        "bsd_duedatecalculatingmethod",
                        "bsd_ordernumber",
                        "bsd_amountofthisphase"));

                int statuscode = ((OptionSetValue)enIns["statuscode"]).Value;

                int bsd_ordernumber = (int)enIns["bsd_ordernumber"];

                int bsd_duedatecalculatingmethod =
                    enIns.Contains("bsd_duedatecalculatingmethod")
                    ? ((OptionSetValue)enIns["bsd_duedatecalculatingmethod"]).Value
                    : 0;

                decimal bsd_depositamount =
                    enIns.Contains("bsd_depositamount")
                    ? ((Money)enIns["bsd_depositamount"]).Value
                    : 0;

                decimal bsd_amountofthisphase =
                    enIns.Contains("bsd_amountofthisphase")
                    ? ((Money)enIns["bsd_amountofthisphase"]).Value
                    : 0;

                decimal amountPay = item.amount;

                if (bsd_duedatecalculatingmethod == 100000002)
                {
                    decimal landvalueIN =
                        SumLandValueVoice(optionentry_invoive.Id);

                    decimal bsd_handoveramount =
                        land_value - landvalueIN;

                    if (bsd_handoveramount < 0)
                        bsd_handoveramount = 0;

                    string name = "Giá trị quyền sử dụng đất không chịu thuế GTGT";

                    int inType;

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

                        name = GetInvoiceName(bsd_project_type, unitName);
                    }

                    CreateInvoice(
                        name,
                        project_invoive,
                        optionentry_invoive,
                        iv_units,
                        EnApplyDocument,
                        EnTaxcode,
                        inType,
                        bsd_paymentactualtime,
                        bsd_depositamount,
                        amountPay,
                        bsd_handoveramount);

                    if (statuscode == 100000001)
                    {
                        CreateInvoice(
                            name,
                            project_invoive,
                            optionentry_invoive,
                            iv_units,
                            EnApplyDocument,
                            EnTaxcode,
                            100000004,
                            bsd_paymentactualtime,
                            0,
                            GetInstallmentLast(optionentry_invoive.Id),
                            0);
                    }
                }
                else
                {
                    if (bsd_ordernumber == 1)
                    {
                        if (checkEDA && statuscode == 100000001)
                        {
                            string name =
                                GetInvoiceName(bsd_project_type, unitName);

                            CreateInvoice(
                                name,
                                project_invoive,
                                optionentry_invoive,
                                iv_units,
                                EnApplyDocument,
                                EnTaxcode,
                                100000003,
                                date_EDA,
                                bsd_depositamount,
                                bsd_amountofthisphase,
                                0);
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
                string name = GetInvoiceName(bsd_project_type, unitName);

                CreateInvoice(
                    name,
                    project_invoive,
                    optionentry_invoive,
                    iv_units,
                    EnApplyDocument,
                    EnTaxcode,
                    100000000,
                    bsd_paymentactualtime,
                    0,
                    sumTypeIns,
                    0);
            }

            traceService.Trace("ra ProcessNonEDAInvoice");
        }

        private void ProcessMaintenanceFee(
            Entity EnApplyDocument,
            Entity project_invoive,
            Entity optionentry_invoive,
            Entity iv_units,
            Entity EnTaxcode,
            DateTime bsd_paymentactualtime,
            int bsd_project_type,
            string unitName)
        {
            traceService.Trace("vào ProcessMaintenanceFee");

            var fetchXmlMainFee = $@"<?xml version=""1.0"" encoding=""utf-16""?>
            <fetch>
              <entity name=""bsd_paymentschemedetail"">
                <attribute name=""bsd_maintenanceamount"" />

                <filter>
                  <condition attribute=""bsd_maintenancefeesstatus"" operator=""eq"" value=""1"" />
                  <condition attribute=""bsd_maintenancefees"" operator=""eq"" value=""1"" />
                </filter>

                <link-entity 
                    name=""bsd_applydocumentdetail"" 
                    from=""bsd_installment"" 
                    to=""bsd_paymentschemedetailid"" 
                    alias=""ADD"">

                  <filter>
                    <condition attribute=""bsd_applydocument"" operator=""eq"" value=""{EnApplyDocument.Id}"" />
                    <condition attribute=""bsd_paymenttype"" operator=""eq"" value=""100000002"" />
                    <condition attribute=""bsd_feetype"" operator=""eq"" value=""100000000"" />
                  </filter>

                </link-entity>
              </entity>
            </fetch>";

            EntityCollection listMainFee =
                service.RetrieveMultiple(new FetchExpression(fetchXmlMainFee));

            foreach (Entity item in listMainFee.Entities)
            {
                string name = "";

                if (bsd_project_type == 100000000)
                {
                    name = "Thu tiền kinh phí bảo trì căn nhà ở số " + unitName;
                }
                else if (bsd_project_type == 100000001)
                {
                    name = "Thu tiền kinh phí bảo trì căn hộ " + unitName;
                }

                decimal bsd_maintenanceamount =
                    item.Contains("bsd_maintenanceamount")
                    ? ((Money)item["bsd_maintenanceamount"]).Value
                    : 0;

                CreateInvoice(
                    name,
                    project_invoive,
                    optionentry_invoive,
                    iv_units,
                    EnApplyDocument,
                    EnTaxcode,
                    100000001,
                    bsd_paymentactualtime,
                    0,
                    bsd_maintenanceamount,
                    0);
            }

            traceService.Trace("ra ProcessMaintenanceFee");
        }

        private EntityCollection GetInstallments(Guid applyDocumentId)
        {
            var fetchXmlListIns = $@"<?xml version=""1.0"" encoding=""utf-16""?>
            <fetch>
              <entity name=""bsd_applydocumentdetail"">
                <attribute name=""bsd_installment"" />
                <attribute name=""bsd_amountapply"" />

                <filter>
                  <condition attribute=""bsd_paymenttype"" operator=""eq"" value=""100000001"" />
                  <condition attribute=""bsd_amountapply"" operator=""gt"" value=""0"" />
                  <condition attribute=""bsd_installment"" operator=""not-null"" />
                  <condition attribute=""bsd_applydocument"" operator=""eq"" value=""{applyDocumentId}"" />
                </filter>

              </entity>
            </fetch>";

            return service.RetrieveMultiple(
                new FetchExpression(fetchXmlListIns));
        }

        private Entity GetTaxCode(Entity optionentry_invoive)
        {
            var fetchXmltaxcode = $@"
            <fetch>
              <entity name='bsd_taxcode'>
                <attribute name='bsd_name' />
                <attribute name='bsd_value' />

                <filter type='and'>
                  <condition 
                    attribute='bsd_taxcodeid' 
                    operator='eq' 
                    value='{((EntityReference)optionentry_invoive["bsd_taxcode"]).Id}'/>
                </filter>

              </entity>
            </fetch>";

            EntityCollection EnColtaxcode =
                service.RetrieveMultiple(new FetchExpression(fetchXmltaxcode));

            if (EnColtaxcode.Entities.Count == 0)
            {
                throw new InvalidPluginExecutionException("Tax code not found.");
            }

            return EnColtaxcode.Entities[0];
        }

        private string GetInvoiceName(int bsd_project_type, string unitName)
        {
            if (bsd_project_type == 100000000)
            {
                return "Thu tiền căn nhà ở số " + unitName;
            }

            if (bsd_project_type == 100000001)
            {
                return "Thu tiền căn hộ " + unitName;
            }

            return "";
        }

        private int Check_EDA(Guid id)
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

            EntityCollection list =
                service.RetrieveMultiple(new FetchExpression(fetchXml));

            foreach (Entity item in list.Entities)
            {
                bool bsd_installmentforeda =
                    item.Contains("bsd_installmentforeda")
                    ? (bool)item["bsd_installmentforeda"]
                    : false;

                numb = bsd_installmentforeda ? 0 : 1;
            }

            return numb;
        }

        private decimal GetInstallmentLast(Guid enOE)
        {
            decimal sum = 0;

            var fetchXml = $@"<?xml version=""1.0"" encoding=""utf-16""?>
            <fetch>
              <entity name=""bsd_paymentschemedetail"">
                <attribute name=""bsd_amountofthisphase"" />

                <filter>
                  <condition attribute=""bsd_optionentry"" operator=""eq"" value=""{enOE}"" />
                  <condition attribute=""bsd_amountofthisphase"" operator=""gt"" value=""0"" />
                  <condition attribute=""bsd_lastinstallment"" operator=""eq"" value=""1"" />
                </filter>

              </entity>
            </fetch>";

            EntityCollection list =
                service.RetrieveMultiple(new FetchExpression(fetchXml));

            foreach (Entity item in list.Entities)
            {
                sum += item.Contains("bsd_amountofthisphase")
                    ? ((Money)item["bsd_amountofthisphase"]).Value
                    : 0;
            }

            return sum;
        }

        private decimal SumLandValueVoice(Guid enOE)
        {
            traceService.Trace("vào SumLandValueVoice");

            decimal sum = 0;

            var fetchXml = $@"<?xml version=""1.0"" encoding=""utf-16""?>
            <fetch>
              <entity name=""bsd_invoice"">
                <attribute name=""bsd_handoveramount"" />

                <filter>
                  <condition attribute=""bsd_optionentry"" operator=""eq"" value=""{enOE}"" />
                  <condition attribute=""bsd_handoveramount"" operator=""gt"" value=""0"" />

                  <condition attribute=""statuscode"" operator=""in"">
                    <value>1</value>
                    <value>100000000</value>
                  </condition>

                </filter>

              </entity>
            </fetch>";

            EntityCollection list =
                service.RetrieveMultiple(new FetchExpression(fetchXml));

            foreach (Entity item in list.Entities)
            {
                sum += item.Contains("bsd_handoveramount")
                    ? ((Money)item["bsd_handoveramount"]).Value
                    : 0;
            }

            fetchXml = $@"<?xml version=""1.0"" encoding=""utf-16""?>
            <fetch>
              <entity name=""bsd_invoice"">
                <attribute name=""bsd_invoiceamount"" />

                <filter>
                  <condition attribute=""bsd_optionentry"" operator=""eq"" value=""{enOE}"" />
                  <condition attribute=""bsd_invoiceamount"" operator=""gt"" value=""0"" />
                  <condition attribute=""bsd_type"" operator=""eq"" value=""100000006"" />

                  <condition attribute=""statuscode"" operator=""in"">
                    <value>1</value>
                    <value>100000000</value>
                  </condition>

                </filter>

              </entity>
            </fetch>";

            list = service.RetrieveMultiple(new FetchExpression(fetchXml));

            foreach (Entity item in list.Entities)
            {
                sum += item.Contains("bsd_invoiceamount")
                    ? ((Money)item["bsd_invoiceamount"]).Value
                    : 0;
            }

            traceService.Trace("ra SumLandValueVoice");

            return sum;
        }

        private void CreateInvoice(
            string bsd_name,
            Entity project_invoive,
            Entity optionentry_invoive,
            Entity iv_units,
            Entity EnApplyDocument,
            Entity EnTaxcode,
            int bsd_type,
            DateTime bsd_issueddate,
            decimal bsd_depositamount,
            decimal bsd_invoiceamount,
            decimal bsd_handoveramount)
        {
            traceService.Trace("vào CreateInvoice");
            if (bsd_type == 100000003 && checkInvaldInvoice1st(optionentry_invoive.Id)) return;
            traceService.Trace("CreateInvoice");
            Entity invoice = new Entity("bsd_invoice");

            invoice["bsd_name"] = bsd_name;
            invoice["bsd_project"] = project_invoive.ToEntityReference();
            invoice["bsd_optionentry"] = optionentry_invoive.ToEntityReference();
            invoice["bsd_applydocument"] = EnApplyDocument.ToEntityReference();

            invoice["bsd_formno"] =
                project_invoive.Contains("bsd_formno")
                ? project_invoive["bsd_formno"]
                : "";

            invoice["bsd_serialno"] =
                project_invoive.Contains("bsd_serialno")
                ? project_invoive["bsd_serialno"]
                : "";

            invoice["bsd_issueddate"] = bsd_issueddate;
            invoice["bsd_units"] = iv_units.ToEntityReference();

            EntityReference purchaser =
                (EntityReference)optionentry_invoive["customerid"];

            invoice["bsd_purchaser"] = purchaser;

            Entity iv_Perchaser = service.Retrieve(
                purchaser.LogicalName,
                purchaser.Id,
                new ColumnSet(true));

            if (iv_Perchaser.LogicalName == "contact")
            {
                invoice["bsd_purchasernamecustomer"] = purchaser;
            }
            else
            {
                invoice["bsd_purchasernamecompany"] = purchaser;
            }

            invoice["bsd_paymentmethod"] = new OptionSetValue(100000000);
            invoice["bsd_type"] = new OptionSetValue(bsd_type);
            invoice["statuscode"] = new OptionSetValue(1);
            invoice["bsd_depositamount"] = new Money(bsd_depositamount);

            if (bsd_invoiceamount > 0 && bsd_type != 100000006)
            {
                if (bsd_type == 100000001)
                {
                    invoice["bsd_invoiceamount"] =
                        new Money(bsd_invoiceamount);

                    invoice["bsd_vatamount"] =
                        new Money(0);

                    invoice["bsd_invoiceamountb4vat"] =
                        new Money(bsd_invoiceamount);
                }
                else
                {
                    decimal taxValue = Convert.ToDecimal(EnTaxcode["bsd_value"]);

                    decimal bsd_vatamount =
                        Math.Round(
                            bsd_invoiceamount * taxValue / 100,
                            MidpointRounding.AwayFromZero);

                    invoice["bsd_invoiceamount"] =
                        new Money(bsd_invoiceamount);

                    invoice["bsd_vatamount"] =
                        new Money(bsd_vatamount);

                    invoice["bsd_invoiceamountb4vat"] =
                        new Money(bsd_invoiceamount - bsd_vatamount);
                }
            }
            else if (bsd_handoveramount > 0 && bsd_type == 100000006)
            {
                invoice["bsd_invoiceamount"] =
                    new Money(bsd_handoveramount);

                invoice["bsd_vatamount"] =
                    new Money(0);

                invoice["bsd_invoiceamountb4vat"] =
                    new Money(bsd_handoveramount);
            }

            if (bsd_type == 100000005)
            {
                invoice["bsd_handoveramount"] =
                    new Money(bsd_handoveramount);

                invoice["bsd_namelandvalue"] =
                    "Giá trị quyền sử dụng đất không chịu thuế GTGT";
            }

            if (bsd_type == 100000001 || bsd_type == 100000006)
            {
                invoice["bsd_taxcodevalue"] = 0m;
            }
            else
            {
                invoice["bsd_taxcodevalue"] = EnTaxcode["bsd_value"];
            }

            service.Create(invoice);

            traceService.Trace("ra CreateInvoice");
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
        private DateTime RetrieveLocalTimeFromUTCTime(DateTime utcTime)
        {
            int? timeZoneCode = RetrieveCurrentUsersSettings(service);

            if (!timeZoneCode.HasValue)
            {
                throw new InvalidPluginExecutionException(
                    "Can't find time zone code");
            }

            LocalTimeFromUtcTimeRequest request =
                new LocalTimeFromUtcTimeRequest
                {
                    TimeZoneCode = timeZoneCode.Value,
                    UtcTime = utcTime.ToUniversalTime()
                };

            LocalTimeFromUtcTimeResponse response =
                (LocalTimeFromUtcTimeResponse)service.Execute(request);

            return response.LocalTime;
        }

        private int? RetrieveCurrentUsersSettings(IOrganizationService service)
        {
            Entity currentUserSettings = service.RetrieveMultiple(
                new QueryExpression("usersettings")
                {
                    ColumnSet = new ColumnSet(
                        "localeid",
                        "timezonecode"),

                    Criteria = new FilterExpression
                    {
                        Conditions =
                        {
                            new ConditionExpression(
                                "systemuserid",
                                ConditionOperator.EqualUserId)
                        }
                    }
                }).Entities[0];

            return (int?)currentUserSettings["timezonecode"];
        }
    }

    public class Installment
    {
        public Guid id { get; set; }

        public decimal amount { get; set; }
    }
}