using Microsoft.Crm.Sdk.Messages;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Query;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;

namespace Plugin_Create_Invoice_Payment
{
    public class Plugin_Create_Invoice_Payment : IPlugin
    {
        private IOrganizationService service;
        private IPluginExecutionContext context;
        private ITracingService traceService;

        private int? _timeZoneCode;

        public void Execute(IServiceProvider serviceProvider)
        {
            context = (IPluginExecutionContext)serviceProvider.GetService(typeof(IPluginExecutionContext));

            if (!context.InputParameters.Contains("Target"))
                return;

            if (!(context.InputParameters["Target"] is Entity target))
                return;

            if (context.Depth > 2)
                return;

            traceService = (ITracingService)serviceProvider.GetService(typeof(ITracingService));

            IOrganizationServiceFactory factory =
                (IOrganizationServiceFactory)serviceProvider.GetService(typeof(IOrganizationServiceFactory));

            service = factory.CreateOrganizationService(context.UserId);

            try
            {
                traceService.Trace("Start Plugin_Create_Invoice_Payment");

                int statusCode =
                    target.GetAttributeValue<OptionSetValue>("statuscode")?.Value ?? 0;

                if (statusCode != 100000000)
                    return;

                Entity payment = service.Retrieve(
                    target.LogicalName,
                    target.Id,
                    new ColumnSet(
                        "bsd_project",
                        "bsd_paymentactualtime",
                        "bsd_paymenttype",
                        "bsd_optionentry",
                        "bsd_units",
                        "bsd_paymentschemedetail",
                        "bsd_amountpay",
                        "bsd_documentno",
                        "bsd_balance"
                    ));

                ProcessApplyDocument(payment);

                traceService.Trace("End Plugin_Create_Invoice_Payment");
            }
            catch (Exception ex)
            {
                traceService.Trace(ex.ToString());
                throw;
            }
        }

        private void ProcessApplyDocument(Entity payment)
        {
            traceService.Trace("Start ProcessApplyDocument");

            EntityReference projectRef = payment.GetAttributeValue<EntityReference>("bsd_project");
            EntityReference optionEntryRef = payment.GetAttributeValue<EntityReference>("bsd_optionentry");
            EntityReference unitRef = payment.GetAttributeValue<EntityReference>("bsd_units");

            if (projectRef == null || optionEntryRef == null || unitRef == null)
                return;

            Entity project = service.Retrieve(
                "bsd_project",
                projectRef.Id,
                new ColumnSet(
                    "bsd_formno",
                    "bsd_serialno",
                    "bsd_optioncheckeinvoice",
                    "bsd_project_type"
                ));

            bool checkEInvoice =
                project.GetAttributeValue<bool>("bsd_optioncheckeinvoice");

            if (!checkEInvoice)
                return;

            Entity optionEntry = service.Retrieve(
                "salesorder",
                optionEntryRef.Id,
                new ColumnSet(
                    "bsd_landvaluededuction",
                    "bsd_taxcode",
                    "customerid",
                    "bsd_contractnumber",
                    "bsd_contracttypedescription",
                    "bsd_contractdate",
                    "bsd_signedcontractdate",
                    "bsd_totalpercent"
                ));

            Entity unit = service.Retrieve(
                unitRef.LogicalName,
                unitRef.Id,
                new ColumnSet("name"));

            EntityReference taxCodeRef =
                optionEntry.GetAttributeValue<EntityReference>("bsd_taxcode");

            if (taxCodeRef == null)
                return;

            Entity taxCode = service.Retrieve(
                "bsd_taxcode",
                taxCodeRef.Id,
                new ColumnSet("bsd_value"));

            _timeZoneCode = RetrieveCurrentUsersSettings();

            DateTime paymentActualTime = RetrieveLocalTimeFromUTCTime(
                payment.GetAttributeValue<DateTime>("bsd_paymentactualtime"));

            DateTime dateEDA = DateTime.Now;
            bool checkEDA = false;

            int contractType =
                optionEntry.GetAttributeValue<OptionSetValue>("bsd_contracttypedescription")?.Value ?? 0;

            if (optionEntry.Contains("bsd_contractnumber"))
            {
                if (contractType == 100000001 &&
                    optionEntry.Contains("bsd_contractdate"))
                {
                    checkEDA = true;

                    dateEDA = RetrieveLocalTimeFromUTCTime(
                        optionEntry.GetAttributeValue<DateTime>("bsd_contractdate"));
                }
                else if ((contractType == 100000002 ||
                          contractType == 100000003) &&
                          optionEntry.Contains("bsd_signedcontractdate"))
                {
                    checkEDA = true;

                    dateEDA = RetrieveLocalTimeFromUTCTime(
                        optionEntry.GetAttributeValue<DateTime>("bsd_signedcontractdate"));
                }
            }

            int projectType =
                project.GetAttributeValue<OptionSetValue>("bsd_project_type")?.Value ?? 0;

            decimal landValue =
                optionEntry.GetAttributeValue<Money>("bsd_landvaluededuction")?.Value ?? 0;

            string unitName =
                unit.GetAttributeValue<string>("name") ?? "";

            int paymentType =
                payment.GetAttributeValue<OptionSetValue>("bsd_paymenttype")?.Value ?? 0;

            List<Installment> edaInstallments = new List<Installment>();
            List<Installment> nonEdaInstallments = new List<Installment>();

            Dictionary<Guid, Entity> installmentCache =
                GetInstallmentCache(payment);

            // MAIN INSTALLMENT
            if (paymentType == 100000002 &&
                payment.Contains("bsd_paymentschemedetail"))
            {
                decimal amountPay =
                    payment.GetAttributeValue<Money>("bsd_amountpay")?.Value ?? 0;

                decimal balance =
                    payment.GetAttributeValue<Money>("bsd_balance")?.Value ?? 0;

                Guid installmentId =
                    payment.GetAttributeValue<EntityReference>("bsd_paymentschemedetail").Id;

                decimal finalAmount =
                    amountPay > balance ? balance : amountPay;

                Installment ins = new Installment
                {
                    Id = installmentId,
                    Amount = finalAmount
                };

                AddInstallment(ins, installmentCache, edaInstallments, nonEdaInstallments);
            }

            // TRANSACTION PAYMENT
            EntityCollection transactionPayments = GetTransactionPayments(payment.Id);

            foreach (Entity item in transactionPayments.Entities)
            {
                Installment ins = new Installment
                {
                    Id = item.GetAttributeValue<EntityReference>("bsd_installment").Id,
                    Amount = item.GetAttributeValue<Money>("bsd_amount")?.Value ?? 0
                };

                AddInstallment(ins, installmentCache, edaInstallments, nonEdaInstallments);
            }

            // EDA
            if (checkEDA)
            {
                // NON EDA
                if (edaInstallments.Count > 0)
                {
                    ProcessEDAInvoice(
                    project,
                    optionEntry,
                    unit,
                    payment,
                    taxCode,
                    projectType,
                    unitName,
                    dateEDA);
                }

                // NON EDA
                if (nonEdaInstallments.Count > 0)
                {
                    ProcessNonEDAInvoice(
                        nonEdaInstallments,
                        installmentCache,
                        project,
                        optionEntry,
                        unit,
                        payment,
                        taxCode,
                        projectType,
                        unitName,
                        landValue,
                        paymentActualTime,
                        checkEDA,
                        dateEDA);
                }

                // MAIN FEE
                ProcessMainFeeInvoice(
                    payment,
                    project,
                    optionEntry,
                    unit,
                    taxCode,
                    projectType,
                    unitName,
                    paymentActualTime);
            }
            traceService.Trace("End ProcessApplyDocument");
        }

        private void AddInstallment(
            Installment installment,
            Dictionary<Guid, Entity> cache,
            List<Installment> eda,
            List<Installment> nonEda)
        {
            if (!cache.ContainsKey(installment.Id))
                return;

            Entity enIns = cache[installment.Id];

            bool isEDA =
                enIns.GetAttributeValue<bool>("bsd_installmentforeda");

            bool isLast =
                enIns.GetAttributeValue<bool>("bsd_lastinstallment");

            if (isLast)
                return;

            if (isEDA)
                eda.Add(installment);
            else
                nonEda.Add(installment);
        }

        private Dictionary<Guid, Entity> GetInstallmentCache(Entity payment)
        {
            Dictionary<Guid, Entity> result =
                new Dictionary<Guid, Entity>();

            List<Guid> ids = new List<Guid>();

            if (payment.Contains("bsd_paymentschemedetail"))
            {
                ids.Add(
                    payment.GetAttributeValue<EntityReference>("bsd_paymentschemedetail").Id);
            }

            EntityCollection transactionPayments =
                GetTransactionPayments(payment.Id);

            foreach (Entity item in transactionPayments.Entities)
            {
                EntityReference installment =
                    item.GetAttributeValue<EntityReference>("bsd_installment");

                if (installment != null)
                    ids.Add(installment.Id);
            }

            ids = ids.Distinct().ToList();

            if (ids.Count == 0)
                return result;

            QueryExpression query =
                new QueryExpression("bsd_paymentschemedetail");

            query.ColumnSet = new ColumnSet(
                "bsd_installmentforeda",
                "bsd_lastinstallment",
                "statuscode",
                "bsd_depositamount",
                "bsd_duedatecalculatingmethod",
                "bsd_ordernumber",
                "bsd_amountofthisphase"
            );

            query.Criteria.AddCondition(
                "bsd_paymentschemedetailid",
                ConditionOperator.In,
                ids.Cast<object>().ToArray());

            EntityCollection list = service.RetrieveMultiple(query);

            foreach (Entity item in list.Entities)
            {
                result[item.Id] = item;
            }

            return result;
        }

        private EntityCollection GetTransactionPayments(Guid paymentId)
        {
            QueryExpression query =
                new QueryExpression("bsd_transactionpayment");

            query.ColumnSet = new ColumnSet(
                "bsd_installment",
                "bsd_amount");

            query.Criteria.AddCondition(
                "bsd_transactiontype",
                ConditionOperator.Equal,
                100000000);

            query.Criteria.AddCondition(
                "bsd_amount",
                ConditionOperator.GreaterThan,
                0);

            query.Criteria.AddCondition(
                "bsd_installment",
                ConditionOperator.NotNull);

            query.Criteria.AddCondition(
                "bsd_payment",
                ConditionOperator.Equal,
                paymentId);

            return service.RetrieveMultiple(query);
        }

        private string GetInvoiceName(int projectType, string unitName)
        {
            switch (projectType)
            {
                case 100000000:
                    return "Thu tiền căn nhà ở số " + unitName;

                case 100000001:
                    return "Thu tiền căn hộ " + unitName;

                default:
                    return "";
            }
        }

        private string GetMainFeeInvoiceName(int projectType, string unitName)
        {
            switch (projectType)
            {
                case 100000000:
                    return "Thu tiền kinh phí bảo trì căn nhà ở số " + unitName;

                case 100000001:
                    return "Thu tiền kinh phí bảo trì căn hộ " + unitName;

                default:
                    return "";
            }
        }

        private void ProcessEDAInvoice(
            Entity project,
            Entity optionEntry,
            Entity unit,
            Entity payment,
            Entity taxCode,
            int projectType,
            string unitName,
            DateTime dateEDA)
        {
            string name = GetInvoiceName(projectType, unitName);

            QueryExpression query =
                new QueryExpression("bsd_paymentschemedetail");

            query.ColumnSet = new ColumnSet(
                "bsd_amountofthisphase",
                "bsd_depositamount",
                "bsd_ordernumber");

            query.Criteria.AddCondition(
                "bsd_installmentforeda",
                ConditionOperator.Equal,
                true);

            query.Criteria.AddCondition(
                "bsd_optionentry",
                ConditionOperator.Equal,
                optionEntry.Id);

            query.Criteria.AddCondition(
                "statuscode",
                ConditionOperator.Equal,
                100000001);

            EntityCollection list = service.RetrieveMultiple(query);

            decimal amountPhase = 0;
            decimal depositAmount = 0;
            int invoiceType = 100000000;

            foreach (Entity item in list.Entities)
            {
                amountPhase +=
                    item.GetAttributeValue<Money>("bsd_amountofthisphase")?.Value ?? 0;

                depositAmount =
                    item.GetAttributeValue<Money>("bsd_depositamount")?.Value ?? 0;
                int order =
                    item.GetAttributeValue<int>("bsd_ordernumber");

                if (order == 1)
                    invoiceType = 100000003;
            }

            if (amountPhase <= 0)
                return;

            CreateInvoice(
                name,
                project,
                optionEntry,
                unit,
                payment,
                taxCode,
                invoiceType,
                dateEDA,
                depositAmount,
                amountPhase,
                0);
        }

        private void ProcessNonEDAInvoice(
            List<Installment> installments,
            Dictionary<Guid, Entity> cache,
            Entity project,
            Entity optionEntry,
            Entity unit,
            Entity payment,
            Entity taxCode,
            int projectType,
            string unitName,
            decimal landValue,
            DateTime paymentActualTime,
            bool checkEDA,
            DateTime dateEDA)
        {
            decimal sumTypeIns = 0;
            decimal is_handoveramount = 0;
            decimal is_depositamount = 0;
            foreach (Installment item in installments)
            {
                if (!cache.ContainsKey(item.Id))
                    continue;

                Entity enIns = cache[item.Id];

                int statusCode =
                    enIns.GetAttributeValue<OptionSetValue>("statuscode")?.Value ?? 0;

                int orderNumber =
                    enIns.GetAttributeValue<int>("bsd_ordernumber");

                int dueDateMethod =
                    enIns.GetAttributeValue<OptionSetValue>("bsd_duedatecalculatingmethod")?.Value ?? 0;

                decimal depositAmount =
                    enIns.GetAttributeValue<Money>("bsd_depositamount")?.Value ?? 0;

                decimal amountPhase =
                    enIns.GetAttributeValue<Money>("bsd_amountofthisphase")?.Value ?? 0;
                decimal amountPay = item.Amount;

                if (dueDateMethod == 100000002)
                {
                    decimal amountLand = amountPay;
                    decimal landValueInvoice =
                        SumLandValueInvoice(optionEntry.Id);

                    decimal handoverAmount =
                        landValue - landValueInvoice;

                    if (handoverAmount < 0)
                        handoverAmount = 0;

                    string name = "Giá trị quyền sử dụng đất không chịu thuế GTGT";

                    int invoiceType;
                    decimal bsd_totalpercent = optionEntry.GetAttributeValue<decimal>("bsd_totalpercent");
                    if (bsd_totalpercent >= 85)
                    {
                        if (amountLand <= handoverAmount)
                        {
                            handoverAmount = amountLand;
                            amountLand = 0;
                            invoiceType = 100000006;

                            CreateInvoice(
                            name,
                            project,
                            optionEntry,
                            unit,
                            payment,
                            taxCode,
                            invoiceType,
                            paymentActualTime,
                            depositAmount,
                            amountLand,
                            handoverAmount);
                        }
                        else if (handoverAmount > 0)
                        {
                            is_handoveramount += handoverAmount;
                            is_depositamount += depositAmount;
                            sumTypeIns += (amountLand - handoverAmount);

                        }
                        else
                        {
                            sumTypeIns += amountPay;
                        }
                    }
                    else
                    {
                        sumTypeIns += amountPay;
                    }
                    if (statusCode == 100000001)
                    {
                        CreateInvoice(
                            name,
                            project,
                            optionEntry,
                            unit,
                            payment,
                            taxCode,
                            100000004,
                            paymentActualTime,
                            0,
                            GetInstallmentLast(optionEntry.Id),
                            0);
                    }
                }
                else
                {
                    if (orderNumber == 1)
                    {
                        if (checkEDA && statusCode == 100000001)
                        {
                            CreateInvoice(
                                GetInvoiceName(projectType, unitName),
                                project,
                                optionEntry,
                                unit,
                                payment,
                                taxCode,
                                100000003,
                                dateEDA,
                                depositAmount,
                                amountPhase,
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
                if (is_handoveramount > 0)
                {
                    CreateInvoice(
                            GetInvoiceName(projectType, unitName),
                            project,
                            optionEntry,
                            unit,
                            payment,
                            taxCode,
                            100000005,
                            paymentActualTime,
                            is_depositamount,
                            sumTypeIns,
                            is_handoveramount);
                }
                else CreateInvoice(
                    GetInvoiceName(projectType, unitName),
                    project,
                    optionEntry,
                    unit,
                    payment,
                    taxCode,
                    100000000,
                    paymentActualTime,
                    0,
                    sumTypeIns,
                    0);
            }
        }

        private void ProcessMainFeeInvoice(
            Entity payment,
            Entity project,
            Entity optionEntry,
            Entity unit,
            Entity taxCode,
            int projectType,
            string unitName,
            DateTime paymentActualTime)
        {
            string fetchXml = $@"
            <fetch>
              <entity name='bsd_paymentschemedetail'>
                <attribute name='bsd_maintenanceamount' />
                <filter>
                  <condition attribute='bsd_maintenancefeesstatus' operator='eq' value='1' />
                  <condition attribute='bsd_maintenancefees' operator='eq' value='1' />
                </filter>

                <link-entity name='bsd_transactionpayment'
                             from='bsd_installment'
                             to='bsd_paymentschemedetailid'>

                    <filter>
                      <condition attribute='bsd_payment'
                                 operator='eq'
                                 value='{payment.Id}' />

                      <condition attribute='bsd_transactiontype'
                                 operator='eq'
                                 value='100000002' />

                      <condition attribute='bsd_feetype'
                                 operator='eq'
                                 value='100000000' />
                    </filter>

                </link-entity>
              </entity>
            </fetch>";

            EntityCollection list =
                service.RetrieveMultiple(new FetchExpression(fetchXml));

            foreach (Entity item in list.Entities)
            {
                decimal maintenanceAmount =
                    item.GetAttributeValue<Money>("bsd_maintenanceamount")?.Value ?? 0;

                CreateInvoice(
                    GetMainFeeInvoiceName(projectType, unitName),
                    project,
                    optionEntry,
                    unit,
                    payment,
                    taxCode,
                    100000001,
                    paymentActualTime,
                    0,
                    maintenanceAmount,
                    0);
            }
        }

        private decimal GetInstallmentLast(Guid optionEntryId)
        {
            decimal sum = 0;

            QueryExpression query =
                new QueryExpression("bsd_paymentschemedetail");

            query.ColumnSet =
                new ColumnSet("bsd_amountofthisphase");

            query.Criteria.AddCondition(
                "bsd_optionentry",
                ConditionOperator.Equal,
                optionEntryId);

            query.Criteria.AddCondition(
                "bsd_amountofthisphase",
                ConditionOperator.GreaterThan,
                0);

            query.Criteria.AddCondition(
                "bsd_lastinstallment",
                ConditionOperator.Equal,
                true);

            EntityCollection list = service.RetrieveMultiple(query);

            foreach (Entity item in list.Entities)
            {
                sum +=
                    item.GetAttributeValue<Money>("bsd_amountofthisphase")?.Value ?? 0;
            }

            return sum;
        }

        private decimal SumLandValueInvoice(Guid optionEntryId)
        {
            decimal sum = 0;

            QueryExpression query1 =
                new QueryExpression("bsd_invoice");

            query1.ColumnSet =
                new ColumnSet("bsd_handoveramount");

            query1.Criteria.AddCondition(
                "bsd_optionentry",
                ConditionOperator.Equal,
                optionEntryId);

            query1.Criteria.AddCondition(
                "bsd_handoveramount",
                ConditionOperator.GreaterThan,
                0);

            query1.Criteria.AddCondition(
                "statuscode",
                ConditionOperator.In,
                new object[] { 1, 100000000 });

            EntityCollection list1 =
                service.RetrieveMultiple(query1);

            foreach (Entity item in list1.Entities)
            {
                sum +=
                    item.GetAttributeValue<Money>("bsd_handoveramount")?.Value ?? 0;
            }

            QueryExpression query2 =
                new QueryExpression("bsd_invoice");

            query2.ColumnSet =
                new ColumnSet("bsd_invoiceamount");

            query2.Criteria.AddCondition(
                "bsd_optionentry",
                ConditionOperator.Equal,
                optionEntryId);

            query2.Criteria.AddCondition(
                "bsd_invoiceamount",
                ConditionOperator.GreaterThan,
                0);

            query2.Criteria.AddCondition(
                "bsd_type",
                ConditionOperator.Equal,
                100000006);

            query2.Criteria.AddCondition(
                "statuscode",
                ConditionOperator.In,
                new object[] { 1, 100000000 });

            EntityCollection list2 =
                service.RetrieveMultiple(query2);

            foreach (Entity item in list2.Entities)
            {
                sum +=
                    item.GetAttributeValue<Money>("bsd_invoiceamount")?.Value ?? 0;
            }

            return sum;
        }

        private void CreateInvoice(
            string invoiceName,
            Entity project,
            Entity optionEntry,
            Entity unit,
            Entity payment,
            Entity taxCode,
            int invoiceType,
            DateTime issueDate,
            decimal depositAmount,
            decimal invoiceAmount,
            decimal handoverAmount)
        {
            traceService.Trace("vào CreateInvoice");
            if (invoiceType == 100000003 && checkInvaldInvoice1st(optionEntry.Id)) return;
            traceService.Trace("CreateInvoice");
            Entity invoice = new Entity("bsd_invoice");

            invoice["bsd_name"] = invoiceName;
            invoice["bsd_project"] = project.ToEntityReference();
            invoice["bsd_optionentry"] = optionEntry.ToEntityReference();
            invoice["bsd_payment"] = payment.ToEntityReference();

            invoice["bsd_formno"] =
                project.GetAttributeValue<string>("bsd_formno");

            invoice["bsd_serialno"] =
                project.GetAttributeValue<string>("bsd_serialno");
            invoice["bsd_documentno"] =
                payment.GetAttributeValue<string>("bsd_documentno");

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
                    decimal vatAmount = taxValue == 0 ? 0 : Math.Round((invoiceAmount / (((100 + taxValue) / 100)) / 10), MidpointRounding.AwayFromZero);

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

        private DateTime RetrieveLocalTimeFromUTCTime(DateTime utcTime)
        {
            if (!_timeZoneCode.HasValue)
                throw new InvalidPluginExecutionException("Timezone not found");

            LocalTimeFromUtcTimeRequest request =
                new LocalTimeFromUtcTimeRequest
                {
                    TimeZoneCode = _timeZoneCode.Value,
                    UtcTime = utcTime.ToUniversalTime()
                };

            LocalTimeFromUtcTimeResponse response =
                (LocalTimeFromUtcTimeResponse)service.Execute(request);

            return response.LocalTime;
        }

        private int? RetrieveCurrentUsersSettings()
        {
            QueryExpression query =
                new QueryExpression("usersettings");

            query.ColumnSet =
                new ColumnSet("timezonecode");

            query.Criteria.AddCondition(
                "systemuserid",
                ConditionOperator.EqualUserId);

            Entity result =
                service.RetrieveMultiple(query)
                       .Entities
                       .FirstOrDefault();

            return result?.GetAttributeValue<int?>("timezonecode");
        }
    }

    public class Installment
    {
        public Guid Id { get; set; }

        public decimal Amount { get; set; }
    }
}