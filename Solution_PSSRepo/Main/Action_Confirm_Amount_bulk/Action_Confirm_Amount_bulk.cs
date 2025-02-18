using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Query;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Action_Confirm_Amount_bulk
{
    public class Action_Confirm_Amount_bulk : IPlugin
    {

        IPluginExecutionContext context = null;
        IOrganizationServiceFactory factory = null;
        IOrganizationService service = null;
        ITracingService tracingService = null;
        Entity en = new Entity();
        public void Execute(IServiceProvider serviceProvider)
        {

            context = serviceProvider.GetService(typeof(IPluginExecutionContext)) as IPluginExecutionContext;
            factory = (IOrganizationServiceFactory)serviceProvider.GetService(typeof(IOrganizationServiceFactory));
            service = (IOrganizationService)factory.CreateOrganizationService(context.UserId);
            tracingService = (ITracingService)serviceProvider.GetService(typeof(ITracingService));
            //bsd_confirmationamount =(bsd_remainingamount(advanced payment)+ bsd_amountwaspaid(installment) + bsd_depositamount(option entry)) -bsd_amountpay(bsd_systemreceipt) +bsd_maintenancefeepaid(bsd_paymentschemedetail)

            var optionentryid = context.InputParameters["id"].ToString();
            //Total Paid (include COA)
            var depositamount = GetInstallDepositAmount(optionentryid,service);
            tracingService.Trace($"depositamount {depositamount}");
            var advancepayment = GetAdvancePayment(optionentryid, service);
            tracingService.Trace($"advancepayment {advancepayment}");
            //INSTALLMENT (Amount was paid)
            var installmentamountwaspaid = GetInstallmentAmountwaspaid(optionentryid,service);
            tracingService.Trace($"installmentamountwaspaid {installmentamountwaspaid}");
            var totalPaidincludCOA = advancepayment + installmentamountwaspaid + depositamount;
            tracingService.Trace($"advancepayment {advancepayment}");
            //Total_SystemReceipt
            var totalSystemReceipt = GetTotalSystemReceipt(optionentryid, service);
            tracingService.Trace($"totalSystemReceipt {totalSystemReceipt}");
            //Total_Waiver Amount (Inst)
            var totalWaiverAmount = GetTotalWaiverAmount(optionentryid, service);
            tracingService.Trace($"totalWaiverAmount {totalWaiverAmount}");
            //Maintenance Fee
            var maintenanceFeePaid = GetMaintenanceFeePaid(optionentryid,service);
            tracingService.Trace($"maintenanceFeePaid {maintenanceFeePaid}");
            //Confirmation Amount
            var confirmationAmount = totalPaidincludCOA - totalSystemReceipt + maintenanceFeePaid;
            Entity enUpdate = new Entity("salesorder", new Guid(optionentryid));
            enUpdate["bsd_confirmationamount"] = new Money(confirmationAmount);
            service.Update(enUpdate);
            tracingService.Trace($"confirmationAmount {confirmationAmount}");
        }
        public decimal GetInstallDepositAmount(string optionEntryId, IOrganizationService service)
        {
            decimal sum = 0;

            // Tạo truy vấn FetchXML
            string fetchXml = $@"
            <fetch version='1.0' output-format='xml-platform' mapping='logical' distinct='false'>
                <entity name='bsd_paymentschemedetail'>
                    <attribute name='bsd_paymentschemedetailid' />
                    <attribute name='bsd_name' />
                    <attribute name='bsd_amountwaspaid' />
                    <attribute name='bsd_depositamount' />
                    <order attribute='bsd_name' descending='false' />
                    <filter type='and'>
                        <condition attribute='bsd_optionentry' operator='eq' uitype='salesorder' value='{optionEntryId}' />
                    </filter>
                </entity>
            </fetch>";

            // Thực hiện truy vấn
            EntityCollection results = service.RetrieveMultiple(new FetchExpression(fetchXml));

            // Tính tổng deposit amount
            foreach (var entity in results.Entities)
            {
                if (entity.Attributes.Contains("bsd_depositamount") && entity["bsd_depositamount"] is Money depositAmount)
                {
                    sum += depositAmount.Value;
                }
            }

            return sum;
        }
        public decimal GetAdvancePayment(string optionEntryId, IOrganizationService service)
        {
            decimal sum = 0;

            // Tạo truy vấn FetchXML
            string fetchXml = $@"
            <fetch version='1.0' output-format='xml-platform' mapping='logical' distinct='false'>
                <entity name='bsd_advancepayment'>
                    <attribute name='bsd_name' />
                    <attribute name='createdon' />
                    <attribute name='statuscode' />
                    <attribute name='bsd_amount' />
                    <attribute name='bsd_remainingamount' />
                    <attribute name='bsd_paidamount' />
                    <attribute name='bsd_project' />
                    <attribute name='bsd_customer' />
                    <attribute name='bsd_transferredamount' />
                    <attribute name='bsd_transfermoney' />
                    <attribute name='bsd_advancepaymentcode' />
                    <attribute name='bsd_transactiondate' />
                    <attribute name='bsd_optionentry' />
                    <attribute name='bsd_advancepaymentid' />
                    <order attribute='createdon' descending='true' />
                    <filter type='and'>
                        <condition attribute='statuscode' operator='eq' value='100000000' />
                        <condition attribute='bsd_optionentry' operator='eq' uitype='salesorder' value='{optionEntryId}' />
                    </filter>
                </entity>
            </fetch>";

            // Thực hiện truy vấn
            EntityCollection results = service.RetrieveMultiple(new FetchExpression(fetchXml));

            // Tính tổng remaining amount
            foreach (var entity in results.Entities)
            {
                if (entity.Attributes.Contains("bsd_remainingamount") && entity["bsd_remainingamount"] is Money remainingAmount)
                {
                    sum += remainingAmount.Value;
                }
            }

            return sum;
        }
        public decimal GetInstallmentAmountwaspaid(string optionEntryId, IOrganizationService service)
        {
            decimal sum = 0;

            // Tạo truy vấn FetchXML
            string fetchXml = $@"
            <fetch version='1.0' output-format='xml-platform' mapping='logical' distinct='false'>
                <entity name='bsd_paymentschemedetail'>
                    <attribute name='bsd_paymentschemedetailid' />
                    <attribute name='bsd_name' />
                    <attribute name='bsd_amountwaspaid' />
                    <attribute name='bsd_waiveramount' />
                    <order attribute='bsd_name' descending='false' />
                    <filter type='and'>
                        <condition attribute='bsd_optionentry' operator='eq' uitype='salesorder' value='{optionEntryId}' />
                    </filter>
                </entity>
            </fetch>";

            // Thực hiện truy vấn
            EntityCollection results = service.RetrieveMultiple(new FetchExpression(fetchXml));

            // Tính tổng amount was paid
            foreach (var entity in results.Entities)
            {
                if (entity.Attributes.Contains("bsd_amountwaspaid") && entity["bsd_amountwaspaid"] is Money amountWasPaid)
                {
                    sum += amountWasPaid.Value;
                }
            }

            return sum;
        }
        public decimal GetTotalSystemReceipt(string optionEntryId, IOrganizationService service)
        {
            decimal sum = 0;

            // Tạo truy vấn FetchXML
            string fetchXml = $@"
            <fetch version='1.0' output-format='xml-platform' mapping='logical' distinct='false'>
                <entity name='bsd_systemreceipt'>
                    <attribute name='bsd_name' />
                    <attribute name='createdon' />
                    <attribute name='bsd_units' />
                    <attribute name='statuscode' />
                    <attribute name='bsd_receiptdate' />
                    <attribute name='bsd_purchaser' />
                    <attribute name='bsd_project' />
                    <attribute name='bsd_paymenttype' />
                    <attribute name='bsd_paymentnumbersams' />
                    <attribute name='bsd_optionentry' />
                    <attribute name='bsd_installmentnumber' />
                    <attribute name='bsd_installment' />
                    <attribute name='bsd_exchangerate' />
                    <attribute name='bsd_exchangemoney' />
                    <attribute name='bsd_amountpay' />
                    <attribute name='bsd_paymentnumber' />
                    <attribute name='bsd_systemreceiptid' />
                    <order attribute='createdon' descending='true' />
                    <filter type='and'>
                        <condition attribute='statuscode' operator='eq' value='100000000' />
                        <condition attribute='bsd_optionentry' operator='eq' uitype='salesorder' value='{optionEntryId}' />
                        <condition attribute='bsd_paymenttype' operator='eq' value='100000003' />
                    </filter>
                </entity>
            </fetch>";

            // Thực hiện truy vấn
            EntityCollection results = service.RetrieveMultiple(new FetchExpression(fetchXml));

            // Tính tổng amount pay
            foreach (var entity in results.Entities)
            {
                if (entity.Attributes.Contains("bsd_amountpay") && entity["bsd_amountpay"] is Money amountPay)
                {
                    sum += amountPay.Value;
                }
            }

            return sum;
        }
        public decimal GetTotalWaiverAmount(string optionEntryId, IOrganizationService service)
        {
            decimal sum = 0;

            // Tạo truy vấn FetchXML
            string fetchXml = $@"
            <fetch version='1.0' output-format='xml-platform' mapping='logical' distinct='false'>
                <entity name='bsd_paymentschemedetail'>
                    <attribute name='bsd_paymentschemedetailid' />
                    <attribute name='bsd_name' />
                    <attribute name='bsd_amountwaspaid' />
                    <attribute name='bsd_waiverinstallment' />
                    <order attribute='bsd_name' descending='false' />
                    <filter type='and'>
                        <condition attribute='bsd_optionentry' operator='eq' uitype='salesorder' value='{optionEntryId}' />
                    </filter>
                </entity>
            </fetch>";

            // Thực hiện truy vấn
            EntityCollection results = service.RetrieveMultiple(new FetchExpression(fetchXml));

            // Tính tổng waiver installment
            foreach (var entity in results.Entities)
            {
                if (entity.Attributes.Contains("bsd_waiverinstallment") && entity["bsd_waiverinstallment"] is Money waiverInstallment)
                {
                    sum += waiverInstallment.Value;
                }
            }

            return sum;
        }
        public decimal GetMaintenanceFeePaid(string optionEntryId, IOrganizationService service)
        {
            decimal sum = 0;

            // Tạo truy vấn FetchXML
            string fetchXml = $@"
            <fetch version='1.0' output-format='xml-platform' mapping='logical' distinct='false'>
                <entity name='bsd_paymentschemedetail'>
                    <attribute name='bsd_paymentschemedetailid' />
                    <attribute name='bsd_name' />
                    <attribute name='bsd_maintenancefeeremaining' />
                    <attribute name='bsd_maintenancefeepaid' />
                    <attribute name='bsd_maintenanceamount' />
                    <order attribute='bsd_name' descending='false' />
                    <filter type='and'>
                        <condition attribute='bsd_optionentry' operator='eq' uitype='salesorder' value='{optionEntryId}' />
                    </filter>
                </entity>
            </fetch>";

            // Thực hiện truy vấn
            EntityCollection results = service.RetrieveMultiple(new FetchExpression(fetchXml));

            // Tính tổng maintenance fee paid
            foreach (var entity in results.Entities)
            {
                if (entity.Attributes.Contains("bsd_maintenancefeepaid") && entity["bsd_maintenancefeepaid"] is Money maintenanceFeePaid)
                {
                    sum += maintenanceFeePaid.Value;
                }
                else
                {
                    // Nếu không có giá trị, cộng thêm 0
                    sum += 0;
                }
            }

            return sum;
        }
    }
}
