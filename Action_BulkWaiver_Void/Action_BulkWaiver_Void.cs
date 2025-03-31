using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Query;
using BSDLibrary;

namespace Action_BulkWaiver_Void
{
    public class Action_BulkWaiver_Void : IPlugin
    {
        IPluginExecutionContext context;
        IOrganizationService service = null;
        IOrganizationServiceFactory factory = null;
        public ITracingService traceService = null;
        ParameterCollection target = null;
        public Entity enBulkWaiver;
        public void Execute(IServiceProvider serviceProvider)
        {
            context = (IPluginExecutionContext)serviceProvider.GetService(typeof(IPluginExecutionContext));
            factory = (IOrganizationServiceFactory)serviceProvider.GetService(typeof(IOrganizationServiceFactory));
            service = factory.CreateOrganizationService(context.UserId);
            traceService = (ITracingService)serviceProvider.GetService(typeof(ITracingService));
            target = context.InputParameters;
            EntityReference enRef = target["Target"] as EntityReference;
            traceService.Trace("Load enBulkWaiver");
            enBulkWaiver = service.Retrieve(enRef.LogicalName,enRef.Id,new ColumnSet(true));
            
            traceService.Trace("Load encolBulkWaiverDetail");
            EntityCollection encolBulkWaiverDetail = getBulkWaiverDetails(enRef);
           
            foreach (Entity enBulkWaiverDetail in encolBulkWaiverDetail.Entities)
            {
                voidBulkWaiverDetail(enBulkWaiverDetail);
            }
            traceService.Trace("Update status BulkWaiver = Revert - 100000002");
            Entity enBulkWaiverUpdate = new Entity(enRef.LogicalName, enRef.Id);
            enBulkWaiverUpdate["statuscode"] = new OptionSetValue(100000002);
            service.Update(enBulkWaiverUpdate);

        }
        private EntityCollection getBulkWaiverDetails(EntityReference enrefBulkWaiver)
        {
            // Define Condition Values
            var QEbsd_bulkwaiverdetail_bsd_bulkwaiver = enrefBulkWaiver.Id;
            
            // Instantiate QueryExpression QEbsd_bulkwaiverdetail
            var QEbsd_bulkwaiverdetail = new QueryExpression("bsd_bulkwaiverdetail");

            // Add all columns to QEbsd_bulkwaiverdetail.ColumnSet
            QEbsd_bulkwaiverdetail.ColumnSet.AllColumns = true;

            // Define filter QEbsd_bulkwaiverdetail.Criteria
            QEbsd_bulkwaiverdetail.Criteria.AddCondition("bsd_bulkwaiver", ConditionOperator.Equal, QEbsd_bulkwaiverdetail_bsd_bulkwaiver);
            return service.RetrieveMultiple(QEbsd_bulkwaiverdetail);
        }
        private void voidBulkWaiverDetail(Entity enBulkWaiverDetail)
        {
            int bsd_waivertype = enBulkWaiverDetail.Contains("bsd_waivertype") ? ((OptionSetValue)enBulkWaiverDetail["bsd_waivertype"]).Value : 0;
            traceService.Trace("bsd_waivertype: " + bsd_waivertype.ToString());
            EntityReference enrefInstallment = enBulkWaiverDetail.Contains("bsd_installment") ? (EntityReference)enBulkWaiverDetail["bsd_installment"] : null;
            if(enrefInstallment != null)
            {
                traceService.Trace("Kiểm tra có payment đã đươc confirm sau thời diểm waiver thì không được void");
                Entity enInstallment = service.Retrieve(enrefInstallment.LogicalName, enrefInstallment.Id,new ColumnSet(true));
                Installment installment = new Installment(service,enInstallment);
                decimal bsd_waiveramount = enBulkWaiverDetail.Contains("bsd_waiveramount") ? ((Money)enBulkWaiverDetail["bsd_waiveramount"]).Value : 0;
                switch (bsd_waivertype)
                {
                    case 100000000://Installment
                        traceService.Trace("Void Installment");
                        if (validateInstallment(enInstallment))
                        {
                            installment.voidInstallment(bsd_waiveramount);
                        }
                        else
                        {
                            throw new InvalidPluginExecutionException("You can't void this Bulk Waiver!");
                        }
                        break;
                    case 100000001://Interest
                        traceService.Trace("Void Interest");
                        if (validateInterest(enInstallment))
                        {
                            installment.voidInterest(bsd_waiveramount);
                        }
                        else
                        {
                            throw new InvalidPluginExecutionException("You can't void this Bulk Waiver!");
                        }
                        break;
                    case 100000002://Management Fee
                        traceService.Trace("Void Management Fee");
                        if (validateManagementFee(enInstallment))
                        {
                            installment.voidManagementFee(bsd_waiveramount);
                        }
                        else
                        {
                            throw new InvalidPluginExecutionException("You can't void this Bulk Waiver!");
                        }
                        break;
                    case 100000003://Maintenance Fee
                        traceService.Trace("Void Maintenance Fee");
                        if (validateMaintenanceFee(enInstallment))
                        {
                            installment.voidMaintenanceFee(bsd_waiveramount);
                        }
                        else
                        {
                            throw new InvalidPluginExecutionException("You can't void this Bulk Waiver!");
                        }
                        break;
                }
                traceService.Trace("Update status BulkWaiverDetail = Revert - 100000002");
                Entity enBulkWaiverDetailUpdate = new Entity(enBulkWaiverDetail.LogicalName, enBulkWaiverDetail.Id);
                enBulkWaiverDetailUpdate["statuscode"] = new OptionSetValue(100000002);
                service.Update(enBulkWaiverDetailUpdate);
            }
            
        }
        
        private bool validateInstallment(Entity enInstallment)
        {
            traceService.Trace("validateInstallment");
            DateTime bsd_approvedrejecteddate = enBulkWaiver.Contains("bsd_approvedrejecteddate") ? (DateTime)enBulkWaiver["bsd_approvedrejecteddate"] : new DateTime(0);
            traceService.Trace("bsd_approvedrejecteddate: " + bsd_approvedrejecteddate.ToString());
            //Nêu có phát sinh Payment và Transaction Payment đã được confirm thì không được void
            //Installment = 100000002
            //Interest Charge = 100000003
            //Fees = 100000004
            //Other = 100000005
            var bsd_paymenttype = 100000002;
            DateTime lastConfirmedDatePayment = getLastConfirmPaymentDate(enInstallment, bsd_paymenttype);
            traceService.Trace("lastConfirmedDatePayment: "+ lastConfirmedDatePayment.ToString());
            //Installments = 100000000
            //Interest = 100000001
            //Fees = 100000002
            //Other = 100000003
            var bsd_transactiontype = 100000000;
            DateTime lastconfirmedDateTransactionPayment = getLastConfirmTransactionPaymentDate(enInstallment, bsd_transactiontype,0);
            traceService.Trace("lastconfirmedDateTransactionPayment: " + lastconfirmedDateTransactionPayment.ToString());
            DateTime lastPaidDate = DateTime.Compare(lastConfirmedDatePayment, lastconfirmedDateTransactionPayment)>0 ? lastConfirmedDatePayment:lastconfirmedDateTransactionPayment;
            traceService.Trace("lastPaidDate: " + lastPaidDate.ToString());
            if (lastPaidDate.Ticks > 0)
            {
                //Có payment
                //Kiểm tra ngày bsd_approvedrejecteddate với lastConfirmedDatePayment
                //Nếu lastPaidDate > bsd_approvedrejecteddate thì không cho void
                return DateTime.Compare(bsd_approvedrejecteddate, lastPaidDate) > 0 ? true : false;
            }
            
            return true;
        }
        private bool validateInterest(Entity enInstallment)
        {
            DateTime bsd_approvedrejecteddate = enBulkWaiver.Contains("bsd_approvedrejecteddate") ? (DateTime)enBulkWaiver["bsd_approvedrejecteddate"] : new DateTime(0);
            //Nêu có phát sinh Payment và Transaction Payment đã được confirm thì không được void
            //Installment = 100000002
            //Interest Charge = 100000003
            //Fees = 100000004
            //Other = 100000005
            var bsd_paymenttype = 100000003;
            DateTime lastConfirmedDatePayment = getLastConfirmPaymentDate(enInstallment, bsd_paymenttype);
            //Installments = 100000000
            //Interest = 100000001
            //Fees = 100000002
            //Other = 100000003
            var bsd_transactiontype = 100000001;
            DateTime lastconfirmedDateTransactionPayment = getLastConfirmTransactionPaymentDate(enInstallment, bsd_transactiontype,0);
            DateTime lastPaidDate = DateTime.Compare(lastConfirmedDatePayment, lastconfirmedDateTransactionPayment) > 0 ? lastConfirmedDatePayment : lastconfirmedDateTransactionPayment;
            
            if (lastPaidDate.Ticks > 0)
            {
                //Có payment
                //Kiểm tra ngày bsd_approvedrejecteddate với lastConfirmedDatePayment
                //Nếu lastPaidDate > bsd_approvedrejecteddate thì không cho void
                return DateTime.Compare(bsd_approvedrejecteddate, lastPaidDate) > 0 ? true : false;
            }
            
            return true;
        }
        private bool validateManagementFee(Entity enInstallment)
        {
            DateTime bsd_approvedrejecteddate = enBulkWaiver.Contains("bsd_approvedrejecteddate") ? (DateTime)enBulkWaiver["bsd_approvedrejecteddate"] : new DateTime(0);

            //Installments = 100000000
            //Interest = 100000001
            //Fees = 100000002
            //Other = 100000003
            int bsd_transactiontype = 100000002;
            //Maintenance Fee = 100000000
            //Management Fee = 100000001
            int bsd_feetype = 100000001;
            DateTime lastconfirmedDateTransactionPayment = getLastConfirmTransactionPaymentDate(enInstallment, bsd_transactiontype, bsd_feetype);
            
           
            if (lastconfirmedDateTransactionPayment.Ticks > 0)
            {
                //Có payment
                //Kiểm tra ngày bsd_approvedrejecteddate với lastConfirmedDatePayment
                //Nếu lastconfirmedDateTransactionPayment > bsd_approvedrejecteddate thì không cho void
                return DateTime.Compare(bsd_approvedrejecteddate, lastconfirmedDateTransactionPayment) > 0 ? true : false;
            }
            
            return true;
        }
        private bool validateMaintenanceFee(Entity enInstallment)
        {
            DateTime bsd_approvedrejecteddate = enBulkWaiver.Contains("bsd_approvedrejecteddate") ? (DateTime)enBulkWaiver["bsd_approvedrejecteddate"] : new DateTime(0);

            //Installments = 100000000
            //Interest = 100000001
            //Fees = 100000002
            //Other = 100000003
            int bsd_transactiontype = 100000002;
            //Maintenance Fee = 100000000
            //Management Fee = 100000001
            int bsd_feetype = 100000000;
            DateTime lastconfirmedDateTransactionPayment = getLastConfirmTransactionPaymentDate(enInstallment, bsd_transactiontype, bsd_feetype);


            if (lastconfirmedDateTransactionPayment.Ticks > 0)
            {
                //Có payment
                //Kiểm tra ngày bsd_approvedrejecteddate với lastConfirmedDatePayment
                //Nếu lastconfirmedDateTransactionPayment > bsd_approvedrejecteddate thì không cho void
                return DateTime.Compare(bsd_approvedrejecteddate, lastconfirmedDateTransactionPayment) > 0 ? true : false;
            }

            return true;
        }
        public DateTime getLastConfirmPaymentDate(Entity enInstallment, int bsd_paymenttype)
        {
            traceService.Trace("getLastConfirmPaymentDate");
           
            Installment installment = new Installment(service, enInstallment);
            Entity enPaymentLast = installment.getLastConfirmPayment(bsd_paymenttype);
            DateTime lastConfirmedDatePayment = new DateTime(0);
            if (enPaymentLast!=null)
            {
                if (enPaymentLast.Contains("bsd_confirmeddate"))
                {
                    lastConfirmedDatePayment = (DateTime)enPaymentLast["bsd_confirmeddate"];
                }
            }
            return lastConfirmedDatePayment;
        }
        public DateTime getLastConfirmTransactionPaymentDate(Entity enInstallment, int bsd_transactiontype, int bsd_feetype)
        {
            Installment installment = new Installment(service, enInstallment);
            Entity enTransactionPaymentLast = installment.getLastConfirmTransactionPayment(bsd_transactiontype,bsd_feetype);
            DateTime lastconfirmedDateTransactionPayment = new DateTime(0);
            if (enTransactionPaymentLast!=null)
            {
                if (enTransactionPaymentLast.Contains("createdon"))
                {
                    lastconfirmedDateTransactionPayment = (DateTime)enTransactionPaymentLast["createdon"];
                }
            }
            return lastconfirmedDateTransactionPayment;
        }
    }
}
