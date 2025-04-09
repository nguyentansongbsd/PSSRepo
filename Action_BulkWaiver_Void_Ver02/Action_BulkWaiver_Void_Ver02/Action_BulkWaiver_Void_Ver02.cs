using Microsoft.Crm.Sdk.Messages;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Messages;
using Microsoft.Xrm.Sdk.Query;
using System;
using System.Linq;
using System.Runtime.ConstrainedExecution;
using System.Text;

namespace Action_BulkWaiver_Void_Ver02
{
    public class Action_BulkWaiver_Void_Ver02 : IPlugin
    {
        public static IOrganizationService service = null;
        static IOrganizationServiceFactory factory = null;
        public static ITracingService traceService = null;
        void IPlugin.Execute(IServiceProvider serviceProvider)
        {
            IPluginExecutionContext context = (IPluginExecutionContext)serviceProvider.GetService(typeof(IPluginExecutionContext));
            traceService = (ITracingService)serviceProvider.GetService(typeof(ITracingService));
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
            string input05 = "";
            if (!string.IsNullOrEmpty((string)context.InputParameters["input05"]))
            {
                input05 = context.InputParameters["input05"].ToString();
            }
            if (input01 == "Bước 01" && input02 != "")
            {
                traceService.Trace("Bước 01");
                Entity enBulkWaiver = service.Retrieve("bsd_bulkwaiver", Guid.Parse(input02), new ColumnSet(true));
                int statuscode = ((OptionSetValue)enBulkWaiver["statuscode"]).Value;
                if (statuscode != 100000000 && statuscode != 100000003) throw new InvalidPluginExecutionException("The status of the Bulk Waiver is invalid. Please check again.");
                bool bsd_powerautomate = enBulkWaiver.Contains("bsd_powerautomate") ? (bool)enBulkWaiver["bsd_powerautomate"] : false;
                if (bsd_powerautomate) throw new InvalidPluginExecutionException("The Record Bulk Waiver is running Power Automate. Please check again.");
                Entity enTarget = new Entity(enBulkWaiver.LogicalName, enBulkWaiver.Id);
                enTarget["bsd_powerautomate"] = true;
                service.Update(enTarget);
                EntityCollection list = find(input02, 100000000);
                if (list.Entities.Count == 0) throw new InvalidPluginExecutionException("The list of waiver to be processed is currently empty. Please check again.");
                context.OutputParameters["output01"] = context.UserId.ToString();
                string url = "";
                EntityCollection configGolive = RetrieveMultiRecord(service, "bsd_configgolive",
                    new ColumnSet(new string[] { "bsd_url" }), "bsd_name", "Bulk Waiver Void");
                foreach (Entity item in configGolive.Entities)
                {
                    if (item.Contains("bsd_url")) url = (string)item["bsd_url"];
                }
                if (url == "") throw new InvalidPluginExecutionException("Link to run PA not found. Please check again.");
                context.OutputParameters["output02"] = url;
            }
            else if (input01 == "Bước 02" && input02 != "" && input03 != "" && input04 != "")
            {
                traceService.Trace("Bước 02");
                service = factory.CreateOrganizationService(Guid.Parse(input04));
                Entity enBulkWaiver = service.Retrieve("bsd_bulkwaiver", Guid.Parse(input02), new ColumnSet(true));
                Entity enBulkWaiverDetail = service.Retrieve("bsd_bulkwaiverdetail", Guid.Parse(input03), new ColumnSet(true));
                voidBulkWaiverDetail(enBulkWaiverDetail, enBulkWaiver);
            }
            else if (input01 == "Bước 03" && input02 != "" && input04 != "")
            {
                traceService.Trace("Bước 03");
                service = factory.CreateOrganizationService(Guid.Parse(input04));
                EntityCollection list = find(input02, 100000000);
                Entity enBulkWaiver = new Entity("bsd_bulkwaiver");
                enBulkWaiver.Id = Guid.Parse(input02);
                enBulkWaiver["bsd_powerautomate"] = false;
                if (list.Entities.Count > 0)
                {
                    enBulkWaiver["statuscode"] = new OptionSetValue(100000003);
                    enBulkWaiver["bsd_error"] = "The detail list is invalid. Please check again.";
                }
                else
                {
                    enBulkWaiver["statuscode"] = new OptionSetValue(100000002);
                    enBulkWaiver["bsd_voidwaiverapprover"] = new EntityReference("systemuser", Guid.Parse(input04));
                    enBulkWaiver["bsd_voidwaiverdate"] = RetrieveLocalTimeFromUTCTime(DateTime.Now);
                    enBulkWaiver["bsd_reasons"] = input05;
                }
                service.Update(enBulkWaiver);
            }
        }
        public DateTime getLastConfirmPaymentDate(Entity enInstallment, int bsd_paymenttype)
        {
            traceService.Trace("getLastConfirmPaymentDate");

            Installment installment = new Installment(service, enInstallment);
            Entity enPaymentLast = installment.getLastConfirmPayment(bsd_paymenttype);
            DateTime lastConfirmedDatePayment = new DateTime(0);
            if (enPaymentLast != null)
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
            Entity enTransactionPaymentLast = installment.getLastConfirmTransactionPayment(bsd_transactiontype, bsd_feetype);
            DateTime lastconfirmedDateTransactionPayment = new DateTime(0);
            if (enTransactionPaymentLast != null)
            {
                if (enTransactionPaymentLast.Contains("createdon"))
                {
                    lastconfirmedDateTransactionPayment = (DateTime)enTransactionPaymentLast["createdon"];
                }
            }
            return lastconfirmedDateTransactionPayment;
        }
        private bool validateInstallment(Entity enInstallment, Entity enBulkWaiver)
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
            traceService.Trace("lastConfirmedDatePayment: " + lastConfirmedDatePayment.ToString());
            //Installments = 100000000
            //Interest = 100000001
            //Fees = 100000002
            //Other = 100000003
            var bsd_transactiontype = 100000000;
            DateTime lastconfirmedDateTransactionPayment = getLastConfirmTransactionPaymentDate(enInstallment, bsd_transactiontype, 0);
            traceService.Trace("lastconfirmedDateTransactionPayment: " + lastconfirmedDateTransactionPayment.ToString());
            DateTime lastPaidDate = DateTime.Compare(lastConfirmedDatePayment, lastconfirmedDateTransactionPayment) > 0 ? lastConfirmedDatePayment : lastconfirmedDateTransactionPayment;
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
        private bool validateInterest(Entity enInstallment, Entity enBulkWaiver)
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
            DateTime lastconfirmedDateTransactionPayment = getLastConfirmTransactionPaymentDate(enInstallment, bsd_transactiontype, 0);
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
        private bool validateManagementFee(Entity enInstallment, Entity enBulkWaiver)
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
        private bool validateMaintenanceFee(Entity enInstallment, Entity enBulkWaiver)
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
        private void voidBulkWaiverDetail(Entity enBulkWaiverDetail, Entity enBulkWaiver)
        {
            int bsd_waivertype = enBulkWaiverDetail.Contains("bsd_waivertype") ? ((OptionSetValue)enBulkWaiverDetail["bsd_waivertype"]).Value : 0;
            traceService.Trace("bsd_waivertype: " + bsd_waivertype.ToString());
            EntityReference enrefInstallment = enBulkWaiverDetail.Contains("bsd_installment") ? (EntityReference)enBulkWaiverDetail["bsd_installment"] : null;
            if (enrefInstallment != null)
            {
                traceService.Trace("Kiểm tra có payment đã đươc confirm sau thời diểm waiver thì không được void");
                Entity enInstallment = service.Retrieve(enrefInstallment.LogicalName, enrefInstallment.Id, new ColumnSet(true));
                Installment installment = new Installment(service, enInstallment);
                decimal bsd_waiveramount = enBulkWaiverDetail.Contains("bsd_waiveramount") ? ((Money)enBulkWaiverDetail["bsd_waiveramount"]).Value : 0;
                switch (bsd_waivertype)
                {
                    case 100000000://Installment
                        traceService.Trace("Void Installment");
                        if (validateInstallment(enInstallment, enBulkWaiver))
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
                        if (validateInterest(enInstallment, enBulkWaiver))
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
                        if (validateManagementFee(enInstallment, enBulkWaiver))
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
                        if (validateMaintenanceFee(enInstallment, enBulkWaiver))
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
                enBulkWaiverDetailUpdate["bsd_error"] = "";
                service.Update(enBulkWaiverDetailUpdate);
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
        private EntityCollection find(string id, int sts)
        {
            string fetXml = @"<fetch  top='1'>
                      <entity name='bsd_bulkwaiverdetail'>
                        <attribute name='bsd_bulkwaiverdetailid' />
                        <filter type='and'>
                          <condition attribute='bsd_bulkwaiver' operator='eq' value='{0}' />
                          <condition attribute='statecode' operator='eq' value='0' />
                          <condition attribute='statuscode' operator='eq' value='{1}' />
                        </filter>
                      </entity>
                    </fetch>";
            fetXml = string.Format(fetXml, id, sts);
            EntityCollection entc = service.RetrieveMultiple(new FetchExpression(fetXml));
            return entc;
        }
        public static DateTime RetrieveLocalTimeFromUTCTime(DateTime utcTime)
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
        private static int? RetrieveCurrentUsersSettings(IOrganizationService service)
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