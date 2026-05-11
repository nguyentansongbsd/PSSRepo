using BSDLibrary;
using Microsoft.Crm.Sdk.Messages;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Query;
using System;

namespace Action_SignedContract
{
    public class Action_SignedContract : IPlugin
    {
        private IPluginExecutionContext context;
        private IOrganizationService service;
        private IOrganizationServiceFactory factory;
        private ITracingService traceService;
        private ParameterCollection target;
        private Common common;

        #region CONSTANT

        private const int STATUS_OPTION = 100000001;
        private const int STATUS_SIGNED = 100000002;
        private const int STATUS_PAID = 100000001;
        private const int STATUS_ACTIVE = 100000000;

        private const int PROJECT_LAND = 100000000;
        private const int PROJECT_HIGHRISE = 100000001;

        private const int CONTRACT_LOCAL_SPA = 100000001;
        private const int CONTRACT_FOREIGNER_SPA = 100000002;
        private const int CONTRACT_LOCAL_VK = 100000003;

        private const int INVOICE_TYPE_FIRST = 100000003;

        #endregion

        public void Execute(IServiceProvider serviceProvider)
        {
            context = (IPluginExecutionContext)serviceProvider.GetService(typeof(IPluginExecutionContext));

            if (context.Depth > 2)
                return;

            factory = (IOrganizationServiceFactory)serviceProvider.GetService(typeof(IOrganizationServiceFactory));
            service = factory.CreateOrganizationService(context.UserId);
            traceService = (ITracingService)serviceProvider.GetService(typeof(ITracingService));
            target = context.InputParameters;

            common = new Common(service);

            try
            {
                traceService.Trace("START Action_SignedContract");

                if (!target.Contains("Target"))
                    throw new InvalidPluginExecutionException("Target is missing.");

                EntityReference optionEntryRef = target["Target"] as EntityReference;

                if (optionEntryRef == null)
                    throw new InvalidPluginExecutionException("Target is invalid.");

                Entity optionEntry = service.Retrieve(
                    optionEntryRef.LogicalName,
                    optionEntryRef.Id,
                    new ColumnSet(true));

                ProcessSignedContract(optionEntry);

                context.OutputParameters["output"] = "done";

                traceService.Trace("END Action_SignedContract");
            }
            catch (Exception ex)
            {
                traceService.Trace("ERROR: " + ex.ToString());
                throw;
            }
        }

        #region MAIN PROCESS

        private void ProcessSignedContract(Entity optionEntry)
        {
            int statusCode =
                optionEntry.GetAttributeValue<OptionSetValue>("statuscode")?.Value ?? 0;

            bool isShortfall =
                new OptionEntry(GetServiceProvider(), optionEntry)
                .checkShortFallAmount(optionEntry);

            if (statusCode != STATUS_OPTION && !isShortfall)
                return;

            ValidateOptionEntry(optionEntry);

            DateTime localNow =
                common.RetrieveLocalTimeFromUTCTime(DateTime.Now);

            DateTime signedDate =
                RetrieveLocalTimeFromUTCTime(
                    optionEntry.Contains("bsd_signedcontractdate")
                    ? optionEntry.GetAttributeValue<DateTime>("bsd_signedcontractdate")
                    : localNow);

            Entity unit = service.Retrieve(
                optionEntry.GetAttributeValue<EntityReference>("bsd_unitnumber").LogicalName,
                optionEntry.GetAttributeValue<EntityReference>("bsd_unitnumber").Id,
                new ColumnSet(
                    "name",
                    "statuscode",
                    "bsd_signedcontractdate",
                    "bsd_actualarea",
                    "bsd_netsaleablearea",
                    "bsd_optionnumber",
                    "bsd_managementamountmonth"
                ));

            Entity project = service.Retrieve(
                optionEntry.GetAttributeValue<EntityReference>("bsd_project").LogicalName,
                optionEntry.GetAttributeValue<EntityReference>("bsd_project").Id,
                new ColumnSet(
                    "bsd_name",
                    "bsd_managementamount",
                    "bsd_formno",
                    "bsd_serialno",
                    "bsd_project_type",
                    "bsd_optioncheckeinvoice"
                ));

            Entity customer = service.Retrieve(
                optionEntry.GetAttributeValue<EntityReference>("customerid").LogicalName,
                optionEntry.GetAttributeValue<EntityReference>("customerid").Id,
                new ColumnSet("bsd_totaltransaction"));

            UpdateOptionEntryStatus(optionEntry);
            UpdateUnit(unit, optionEntry, signedDate);
            UpdateCustomer(customer);
            UpdateManagementFee(optionEntry, unit, project);

            ClearDueDateWordTemplate(optionEntry.Id);

            CreateFirstInvoice(optionEntry, unit, project);

            traceService.Trace("ProcessSignedContract DONE");
        }

        #endregion

        #region VALIDATE

        private void ValidateOptionEntry(Entity optionEntry)
        {
            if (!optionEntry.Contains("customerid"))
                throw new InvalidPluginExecutionException("Contract does not contain Purchaser!");

            if (!optionEntry.Contains("ordernumber"))
                throw new InvalidPluginExecutionException("Contract does not contain 'Option Number'!");

            if (!optionEntry.Contains("bsd_project"))
                throw new InvalidPluginExecutionException("Contract does not contain 'Project'!");

            if (!optionEntry.Contains("bsd_contractprinteddate"))
                throw new InvalidPluginExecutionException("Option Entry must be printed before signing!");
        }

        #endregion

        #region UPDATE DATA

        private void UpdateOptionEntryStatus(Entity optionEntry)
        {
            Entity update = new Entity(optionEntry.LogicalName, optionEntry.Id);
            update["statuscode"] = new OptionSetValue(STATUS_SIGNED);

            service.Update(update);
        }

        private void UpdateUnit(
            Entity unit,
            Entity optionEntry,
            DateTime signedDate)
        {
            Entity update = new Entity(unit.LogicalName, unit.Id);

            update["statuscode"] = new OptionSetValue(STATUS_SIGNED);
            update["bsd_signedcontractdate"] = signedDate;

            if (optionEntry.Contains("bsd_optionno"))
                update["bsd_optionnumber"] = optionEntry["bsd_optionno"];

            service.Update(update);
        }

        private void UpdateCustomer(Entity customer)
        {
            int totalTransaction =
                customer.GetAttributeValue<int>("bsd_totaltransaction");

            Entity update = new Entity(customer.LogicalName, customer.Id);

            update["bsd_totaltransaction"] = totalTransaction + 1;

            service.Update(update);
        }

        private void UpdateManagementFee(
            Entity optionEntry,
            Entity unit,
            Entity project)
        {
            int numberOfMonths =
                optionEntry.GetAttributeValue<int>("bsd_numberofmonthspaidmf");

            decimal netSaleableArea =
                unit.GetAttributeValue<decimal>("bsd_netsaleablearea");

            decimal managementAmountMonth =
                unit.Contains("bsd_managementamountmonth")
                ? unit.GetAttributeValue<Money>("bsd_managementamountmonth").Value
                : project.GetAttributeValue<Money>("bsd_managementamount").Value;

            decimal totalManagementFee =
                managementAmountMonth * numberOfMonths * netSaleableArea;

            EntityCollection installmentFees =
                GetInstallmentFee(optionEntry.Id);

            if (installmentFees.Entities.Count == 0)
                return;

            Entity firstInstallment = installmentFees.Entities[0];

            Entity update = new Entity(firstInstallment.LogicalName, firstInstallment.Id);

            update["bsd_managementamount"] =
                new Money(totalManagementFee);

            service.Update(update);
        }

        private void ClearDueDateWordTemplate(Guid optionEntryId)
        {
            QueryExpression query = new QueryExpression("bsd_paymentschemedetail");

            query.ColumnSet = new ColumnSet(
                "statuscode",
                "bsd_duedatewordtemplate");

            query.Criteria.AddCondition(
                "bsd_optionentry",
                ConditionOperator.Equal,
                optionEntryId);

            EntityCollection list = service.RetrieveMultiple(query);

            foreach (Entity item in list.Entities)
            {
                int status =
                    item.GetAttributeValue<OptionSetValue>("statuscode")?.Value ?? 0;

                if (status != STATUS_PAID)
                    continue;

                Entity update = new Entity(item.LogicalName, item.Id);

                update["bsd_duedatewordtemplate"] = null;

                service.Update(update);
            }
        }

        #endregion

        #region INVOICE

        private void CreateFirstInvoice(
            Entity optionEntry,
            Entity unit,
            Entity project)
        {
            if (CheckExistFirstInvoice(optionEntry.Id))
                return;

            bool enableEInvoice =
                project.GetAttributeValue<bool>("bsd_optioncheckeinvoice");

            if (!enableEInvoice)
                return;

            if (!optionEntry.Contains("bsd_taxcode"))
                return;

            Entity taxCode = service.Retrieve(
                "bsd_taxcode",
                optionEntry.GetAttributeValue<EntityReference>("bsd_taxcode").Id,
                new ColumnSet("bsd_name", "bsd_value"));

            bool checkEDA = false;
            DateTime invoiceDate = DateTime.Now;

            if (optionEntry.Contains("bsd_contractnumber")
                && optionEntry.Contains("bsd_contracttypedescription"))
            {
                int contractType =
                    optionEntry.GetAttributeValue<OptionSetValue>("bsd_contracttypedescription").Value;

                if (contractType == CONTRACT_LOCAL_SPA
                    && optionEntry.Contains("bsd_contractdate"))
                {
                    checkEDA = true;

                    invoiceDate = RetrieveLocalTimeFromUTCTime(
                        optionEntry.GetAttributeValue<DateTime>("bsd_contractdate"));
                }
                else if (
                    (contractType == CONTRACT_FOREIGNER_SPA
                    || contractType == CONTRACT_LOCAL_VK)
                    && optionEntry.Contains("bsd_signedcontractdate"))
                {
                    checkEDA = true;

                    invoiceDate = RetrieveLocalTimeFromUTCTime(
                        optionEntry.GetAttributeValue<DateTime>("bsd_signedcontractdate"));
                }
            }

            if (!checkEDA)
                return;

            bool hasInstallmentEDA =
                CheckInstallmentEDA(optionEntry.Id);

            if (hasInstallmentEDA)
            {
                CreateInvoiceForEDAInstallment(
                    optionEntry,
                    unit,
                    project,
                    taxCode,
                    invoiceDate);
            }
            else
            {
                CreateInvoiceForNormalInstallment(
                    optionEntry,
                    unit,
                    project,
                    taxCode);
            }
        }

        private void CreateInvoiceForEDAInstallment(
            Entity optionEntry,
            Entity unit,
            Entity project,
            Entity taxCode,
            DateTime invoiceDate)
        {
            var unpaidFetch = $@"
            <fetch>
              <entity name='bsd_paymentschemedetail'>
                <attribute name='bsd_paymentschemedetailid' />
                <filter>
                  <condition attribute='bsd_installmentforeda' operator='eq' value='1' />
                  <condition attribute='bsd_optionentry' operator='eq' value='{optionEntry.Id}' />
                  <condition attribute='statuscode' operator='eq' value='{STATUS_ACTIVE}' />
                </filter>
              </entity>
            </fetch>";

            EntityCollection unpaid =
                service.RetrieveMultiple(new FetchExpression(unpaidFetch));

            if (unpaid.Entities.Count > 0)
                return;

            var paidFetch = $@"
            <fetch>
              <entity name='bsd_paymentschemedetail'>
                <attribute name='bsd_amountofthisphase' />
                <attribute name='bsd_depositamount' />
                <filter>
                  <condition attribute='bsd_installmentforeda' operator='eq' value='1' />
                  <condition attribute='bsd_optionentry' operator='eq' value='{optionEntry.Id}' />
                  <condition attribute='statuscode' operator='eq' value='{STATUS_PAID}' />
                </filter>
              </entity>
            </fetch>";

            EntityCollection paidList =
                service.RetrieveMultiple(new FetchExpression(paidFetch));

            decimal amount = 0;
            decimal deposit = 0;

            foreach (Entity item in paidList.Entities)
            {
                amount +=
                    item.GetAttributeValue<Money>("bsd_amountofthisphase")?.Value ?? 0;

                deposit =
                    item.GetAttributeValue<Money>("bsd_depositamount")?.Value ?? 0;
            }

            string invoiceName =
                GetInvoiceName(
                    project.GetAttributeValue<OptionSetValue>("bsd_project_type")?.Value ?? 0,
                    unit.GetAttributeValue<string>("name"));

            CreateInvoice(
                invoiceName,
                project,
                optionEntry,
                unit,
                taxCode,
                INVOICE_TYPE_FIRST,
                invoiceDate,
                deposit,
                amount,
                0);
        }

        private void CreateInvoiceForNormalInstallment(
            Entity optionEntry,
            Entity unit,
            Entity project,
            Entity taxCode)
        {
            var fetch = $@"
            <fetch>
              <entity name='bsd_paymentschemedetail'>
                <attribute name='bsd_depositamount' />
                <attribute name='bsd_amountofthisphase' />
                <attribute name='bsd_paiddate' />
                <filter>
                  <condition attribute='bsd_ordernumber' operator='eq' value='1' />
                  <condition attribute='bsd_optionentry' operator='eq' value='{optionEntry.Id}' />
                  <condition attribute='statuscode' operator='eq' value='{STATUS_PAID}' />
                </filter>
              </entity>
            </fetch>";

            EntityCollection list =
                service.RetrieveMultiple(new FetchExpression(fetch));

            foreach (Entity item in list.Entities)
            {
                decimal deposit =
                    item.GetAttributeValue<Money>("bsd_depositamount")?.Value ?? 0;

                decimal amount =
                    item.GetAttributeValue<Money>("bsd_amountofthisphase")?.Value ?? 0;

                DateTime paidDate =
                    RetrieveLocalTimeFromUTCTime(
                        item.GetAttributeValue<DateTime>("bsd_paiddate"));

                string invoiceName =
                    GetInvoiceName(
                        project.GetAttributeValue<OptionSetValue>("bsd_project_type")?.Value ?? 0,
                        unit.GetAttributeValue<string>("name"));

                CreateInvoice(
                    invoiceName,
                    project,
                    optionEntry,
                    unit,
                    taxCode,
                    INVOICE_TYPE_FIRST,
                    paidDate,
                    deposit,
                    amount,
                    0);
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
            invoice["bsd_units"] = unit.ToEntityReference();

            invoice["bsd_formno"] =
                project.GetAttributeValue<string>("bsd_formno");

            invoice["bsd_serialno"] =
                project.GetAttributeValue<string>("bsd_serialno");

            invoice["bsd_issueddate"] = issueDate;

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

            if (invoiceAmount > 0)
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

            if (handoverAmount > 0)
            {
                invoice["bsd_handoveramount"] =
                    new Money(handoverAmount);
            }

            invoice["bsd_taxcodevalue"] = taxValue;

            service.Create(invoice);
        }

        #endregion

        #region QUERY

        private EntityCollection GetInstallmentFee(Guid optionEntryId)
        {
            string fetch = $@"
            <fetch>
              <entity name='bsd_paymentschemedetail'>
                <attribute name='bsd_paymentschemedetailid' />
                <filter>
                  <condition attribute='bsd_optionentry' operator='eq' value='{optionEntryId}' />
                  <condition attribute='bsd_managementamount' operator='gt' value='0' />
                  <condition attribute='statecode' operator='eq' value='0' />
                </filter>
              </entity>
            </fetch>";

            return service.RetrieveMultiple(
                new FetchExpression(fetch));
        }

        private bool CheckExistFirstInvoice(Guid optionEntryId)
        {
            QueryExpression query =
                new QueryExpression("bsd_invoice");

            query.TopCount = 1;

            query.ColumnSet =
                new ColumnSet("bsd_invoiceid");

            query.Criteria.AddCondition(
                "statuscode",
                ConditionOperator.In,
                1,
                STATUS_ACTIVE);

            query.Criteria.AddCondition(
                "bsd_type",
                ConditionOperator.Equal,
                INVOICE_TYPE_FIRST);

            query.Criteria.AddCondition(
                "bsd_optionentry",
                ConditionOperator.Equal,
                optionEntryId);

            EntityCollection list =
                service.RetrieveMultiple(query);

            return list.Entities.Count > 0;
        }

        private bool CheckInstallmentEDA(Guid optionEntryId)
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

            EntityCollection list =
                service.RetrieveMultiple(query);

            return list.Entities.Count > 0;
        }

        #endregion

        #region HELPER

        private string GetInvoiceName(
            int projectType,
            string unitName)
        {
            if (projectType == PROJECT_LAND)
                return "Thu tiền căn nhà ở số " + unitName;

            if (projectType == PROJECT_HIGHRISE)
                return "Thu tiền căn hộ " + unitName;

            return string.Empty;
        }

        private IServiceProvider GetServiceProvider()
        {
            return (IServiceProvider)context;
        }

        private DateTime RetrieveLocalTimeFromUTCTime(DateTime utcTime)
        {
            int? timeZoneCode =
                RetrieveCurrentUsersSettings(service);

            if (!timeZoneCode.HasValue)
                throw new InvalidPluginExecutionException("Can't find time zone code");

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
            Entity currentUserSettings =
                service.RetrieveMultiple(
                    new QueryExpression("usersettings")
                    {
                        ColumnSet = new ColumnSet("timezonecode"),
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

            return currentUserSettings.GetAttributeValue<int?>("timezonecode");
        }

        #endregion
    }
}