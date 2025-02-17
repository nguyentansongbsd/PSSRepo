using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Query;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Action_Collection_Report_btncalculate
{
    public class Action_Collection_Report_btncalculate : IPlugin
    {
        IOrganizationService service = null;
        IOrganizationServiceFactory factory = null;
        public ITracingService traceService = null;
        ParameterCollection target = null;
        public void Execute(IServiceProvider serviceProvider)
        {
            IPluginExecutionContext context = (IPluginExecutionContext)serviceProvider.GetService(typeof(IPluginExecutionContext));
            factory = (IOrganizationServiceFactory)serviceProvider.GetService(typeof(IOrganizationServiceFactory));
            service = factory.CreateOrganizationService(context.UserId);
            traceService = (ITracingService)serviceProvider.GetService(typeof(ITracingService));
            target = context.InputParameters;
            EntityReference enRef = target["Target"] as EntityReference;
            string optionEntryId = context.InputParameters["optionEntryId"].ToString();
            string action = context.InputParameters["action"].ToString();           
            traceService.Trace("action: " + action);
            switch (action)
            {
                case "delalldetail":
                    delCollectionReportDetail(enRef, null);
                    break;
                case "genreport":

                    Entity enOptionEntry = service.Retrieve("salesorder", new Guid(optionEntryId), new ColumnSet(true));
                    Entity enCollectionReport = service.Retrieve(enRef.LogicalName, enRef.Id, new ColumnSet(true));

                    DateTime fromdate = (DateTime)enCollectionReport["bsd_fromdate"];
                    DateTime todate = (DateTime)enCollectionReport["bsd_todate"];
                    int type = ((OptionSetValue)enCollectionReport["bsd_type"]).Value;
                    traceService.Trace("OptionEntryId: " + enOptionEntry.Id);
                    traceService.Trace("OptionEntry Name: " + enOptionEntry["name"]);
                    traceService.Trace("fromdate: " + fromdate.ToString());
                    traceService.Trace("todate: " + todate.ToString());
                    genCollectionReportDetail(enRef, enOptionEntry, type, fromdate, todate);
                    
                    break;
            }

        }
        private decimal getTotalSystemReceiptbyOptionEntry(Entity enOptionEntry)
        {
            string xml = @"<fetch version='1.0' output-format='xml-platform' mapping='logical' distinct='false'>
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
                  <condition attribute='statecode' operator='eq' value='0' />
                  <condition attribute='bsd_optionentry' operator='eq' uitype='salesorder' value='" + enOptionEntry.Id.ToString() + @"' />
                </filter>
              </entity>
            </fetch>";
            EntityCollection encol = service.RetrieveMultiple(new FetchExpression(xml));
            decimal sum = encol.Entities.AsEnumerable().Sum(x => ((Money)x.Attributes["bsd_amountpay"]).Value);
            return sum;
        }
        private SumPayments sumPaymentsbyInstallment(Entity enOptionEntry ,Entity enInstallment, DateTime fromdate, DateTime todate)
        {
            SumPayments sum = new SumPayments(enOptionEntry);
            string strfromdate = fromdate.Year + "-" + fromdate.Month + "-" + fromdate.Day;
            string strtodate = todate.Year + "-" + todate.Month + "-" + todate.Day;
            string xml = @"<fetch version='1.0' output-format='xml-platform' mapping='logical' distinct='false'>
              <entity name='bsd_payment'>
                <attribute name='bsd_name' />
                <attribute name='createdon' />
                <attribute name='bsd_units' />
                <attribute name='bsd_purchaser' />
                <attribute name='bsd_project' />
                <attribute name='ownerid' />
                <attribute name='bsd_paymentactualtime' />
                <attribute name='bsd_paymenttype' />
                <attribute name='bsd_paymentschemedetail' />
                <attribute name='bsd_paymentnumber' />
                <attribute name='bsd_paymentmode' />
                <attribute name='bsd_transactiontype' />
                <attribute name='bsd_amountpay' />
                <attribute name='bsd_projectbankaccount' />
                <attribute name='bsd_virtualaccount' />
                <attribute name='bsd_differentamount' />
                <attribute name='statuscode' />
                <attribute name='bsd_confirmeddate' />
                <attribute name='bsd_receiptprinteddate' />
                <attribute name='bsd_paymentid' />
                <attribute name='bsd_depositamount' />
                <attribute name='bsd_totalamountpayablephase' />
                <attribute name='bsd_totalamountpaidphase_base' />
                <attribute name='bsd_totalamountpaidphase' />
                <attribute name='bsd_paymentnumbersams' />
                <order attribute='bsd_paymentactualtime' descending='false' />
                <filter type='and'>
                  <condition attribute='bsd_paymentschemedetail' operator='eq' uitype='bsd_paymentschemedetail' value='" + enInstallment.Id.ToString() + @"' />
                    <condition attribute='statuscode' operator='eq' value='100000000' />
                    <condition attribute='bsd_paymentactualtime' operator='on-or-after' value='" + strfromdate + @"' />
			        <condition attribute='bsd_paymentactualtime' operator='on-or-before' value='" + todate + @"' />
                </filter>
              </entity>
            </fetch>";
            traceService.Trace("sumPaymentsbyInstallment");
            traceService.Trace(xml);
            int bsd_ordernumber = enInstallment.Contains("bsd_ordernumber") ? (int)enInstallment["bsd_ordernumber"] : 0;
            traceService.Trace(bsd_ordernumber.ToString());
            EntityCollection encolPayments = service.RetrieveMultiple(new FetchExpression(xml));
            traceService.Trace("Count: " + encolPayments.Entities.Count.ToString());
            foreach (Entity enPayments in encolPayments.Entities)
            {
                traceService.Trace("add");
                traceService.Trace(enPayments.Id.ToString());
                int bsd_paymenttype = enPayments.Contains("bsd_paymenttype") ? ((OptionSetValue)enPayments["bsd_paymenttype"]).Value : 0;
                decimal bsd_amountpay = enPayments.Contains("bsd_amountpay") ? ((Money)enPayments["bsd_amountpay"]).Value : 0;
                decimal bsd_differentamount = enPayments.Contains("bsd_differentamount") ? ((Money)enPayments["bsd_differentamount"]).Value : 0;
                decimal bsd_depositamount = enPayments.Contains("bsd_depositamount") ? ((Money)enPayments["bsd_depositamount"]).Value : 0;
                DateTime bsd_paymentactualtime = enPayments.Contains("bsd_paymentactualtime") ? (DateTime)enPayments["bsd_paymentactualtime"] : new DateTime(0);
                traceService.Trace("bsd_paymenttype: " + bsd_paymenttype);
                traceService.Trace("bsd_amountpay: " + bsd_amountpay.ToString());
                traceService.Trace("bsd_differentamount: " + bsd_differentamount.ToString());
                traceService.Trace("bsd_depositamount: " + bsd_depositamount.ToString());
                traceService.Trace("bsd_paymentactualtime: " + bsd_paymentactualtime.ToString());

                string bsd_paymentnumbersams = enPayments.Contains("bsd_paymentnumbersams") ? enPayments["bsd_paymentnumbersams"].ToString() : "";
                traceService.Trace("bsd_paymentnumbersams: "+ bsd_paymentnumbersams);
                sum.add(enPayments);
            }
            return sum;
        }
        private SumTransactionPayment sumTransactionPaymentByInstallment(Entity enInstallment, DateTime fromdate, DateTime todate)
        {
            SumTransactionPayment sumRe = new SumTransactionPayment();
            string strfromdate = fromdate.Year + "-" + fromdate.Month + "-" + fromdate.Day;
            string strtodate = todate.Year + "-" + todate.Month + "-" + todate.Day;
            string xml = @"<fetch version='1.0' output-format='xml-platform' mapping='logical' distinct='false'>
	            <entity name='bsd_transactionpayment'>
		            <attribute name='bsd_transactionpaymentid' />
		            <attribute name='bsd_name' />
		            <attribute name='bsd_amount' />
		            <attribute name='bsd_installment' />
		            <attribute name='bsd_transactiontype' />
                    <attribute name='statuscode' />
                    <attribute name='bsd_feetype' />
                    <attribute name='bsd_payment' />
                    <attribute name='bsd_miscellaneous' />
                    <attribute name='bsd_interestchargeamount_base' />
                    <attribute name='bsd_interestchargeamount' />
		            <attribute name='createdon' />
		            <order attribute='createdon' descending='false' />
		            <filter type='and'>
			            <condition attribute='bsd_installment' operator='eq' uitype='bsd_paymentschemedetail' value='" + enInstallment.Id + @"' />
			            <condition attribute='statuscode' operator='eq' value='100000000' />
                        <condition attribute='createdon' operator='on-or-after' value='" + strfromdate + @"' />
			            <condition attribute='createdon' operator='on-or-before' value='" + todate + @"' />
		            </filter>
	            </entity>
            </fetch>";
            traceService.Trace("SumTransactionPayment");
            traceService.Trace(xml);
            EntityCollection encolTransactionPayment = service.RetrieveMultiple(new FetchExpression(xml));
            foreach (Entity enTransactionPayment in encolTransactionPayment.Entities)
            {
                sumRe.add(enTransactionPayment);
            }
            return sumRe;
        }
        private SumApplyDocumentDetail sumApplyDocumentDetailByInstallment(Entity enOptionEntry, Entity enInstallment, DateTime fromdate, DateTime todate)
        {
            SumApplyDocumentDetail sum = new SumApplyDocumentDetail();
            string strfromdate = fromdate.Year + "-" + fromdate.Month + "-" + fromdate.Day;
            string strtodate = todate.Year + "-" + todate.Month + "-" + todate.Day;
            EntityCollection encol = getApplyDocumentDetail(enOptionEntry, fromdate, todate);
            EntityCollection encolApplyDocumentDetail = new EntityCollection();
            foreach (Entity en in encol.Entities)
            {
                if (((EntityReference)en["bsd_installment"]).Id == enInstallment.Id)
                {
                    encolApplyDocumentDetail.Entities.Add(en);
                }
            }
            foreach (Entity enApplyDocumentDetail in encolApplyDocumentDetail.Entities)
            {
                sum.add(enApplyDocumentDetail);
            }

            return sum;
        }
        private EntityCollection getApplyDocumentDetail(Entity enOptionEntry, DateTime fromdate, DateTime todate)
        {
            string strfromdate = fromdate.Year + "-" + fromdate.Month + "-" + fromdate.Day;
            string strtodate = todate.Year + "-" + todate.Month + "-" + todate.Day;
            string conditionfromdate = "";
            if (fromdate.Ticks != 0)
            {
                conditionfromdate = "<condition attribute='createdon' operator='on-or-after' value='" + strfromdate + @"' />";
            }
            string conditiontodate = "";
            if (todate.Ticks != 0)
            {
                conditiontodate = "<condition attribute='createdon' operator='on-or-before' value='" + strtodate + @"' />";
            }
            
            string xml = @"<fetch version='1.0' output-format='xml-platform' mapping='logical' distinct='false'>
              <entity name='bsd_applydocumentdetail'>
                <attribute name='bsd_applydocumentdetailid' />
                <attribute name='bsd_name' />
                <attribute name='createdon' />
                <attribute name='statuscode' />
                <attribute name='bsd_paymenttype' />
                <attribute name='bsd_optionentry' />
                <attribute name='bsd_miscellaneous' />
                <attribute name='bsd_installment' />
                <attribute name='bsd_feetype' />
                <attribute name='exchangerate' />
                <attribute name='bsd_amountapply' />
                <order attribute='createdon' descending='false' />
                <filter type='and'>
                    <condition attribute='statuscode' operator='eq' value='100000000' />
                  <condition attribute='bsd_optionentry' operator='eq' uitype='bsd_optionentry' value='" + enOptionEntry.Id.ToString() + @"' />
                  {0}
                  {1}
                </filter>
              </entity>
            </fetch>";
            xml = string.Format(xml, conditionfromdate, conditiontodate);
            traceService.Trace("getApplyDocumentDetail");
            traceService.Trace(xml);
            EntityCollection encolApplyDocumentDetail = service.RetrieveMultiple(new FetchExpression(xml));
            return encolApplyDocumentDetail;
        }
        private EntityCollection getAdvancePaymentbyOptionEntry(Entity enOptionEntry, DateTime fromdate, DateTime todate)
        {
            string strfromdate = fromdate.Year + "-" + fromdate.Month + "-" + fromdate.Day;
            string strtodate = todate.Year + "-" + todate.Month + "-" + todate.Day;
            string conditionfromdate = "";
            if (fromdate.Ticks != 0)
            {
                conditionfromdate = "<condition attribute='bsd_transactiondate' operator='on-or-after' value='" + strfromdate + @"' />";
            }
            string conditiontodate = "";
            if (todate.Ticks != 0)
            {
                conditiontodate = "<condition attribute='bsd_transactiondate' operator='on-or-before' value='" + strtodate + @"' />";
            }
            //Tong tông ADV Payment + Transfer
            string xml = @"<fetch version='1.0' output-format='xml-platform' mapping='logical' distinct='false'>
              <entity name='bsd_advancepayment'>
                <attribute name='bsd_name' />
                <attribute name='createdon' />
                <attribute name='bsd_transferredamount' />
                <attribute name='bsd_transfermoney' />
                <attribute name='statuscode' />
                <attribute name='bsd_remainingamount' />
                <attribute name='bsd_project' />
                <attribute name='bsd_paidamount' />
                <attribute name='bsd_customer' />
                <attribute name='bsd_amount' />
                <attribute name='bsd_advancepaymentcode' />
                <attribute name='bsd_advancepaymentid' />
                <attribute name='bsd_transactiondate' />
                <attribute name='statecode' />
                <order attribute='bsd_transactiondate' descending='false' />
                <filter type='and'>
                  <condition attribute='bsd_optionentry' operator='eq' uitype='salesorder' value='" + enOptionEntry.Id.ToString() + @"' />
                  <condition attribute='statuscode' operator='eq' value='100000000' />
                  {0}
                  {1}
                </filter>
              </entity>
            </fetch>";
            xml = string.Format(xml, conditionfromdate, conditiontodate);
            traceService.Trace("getAdvancePaymentbyOptionEntry");
            traceService.Trace(xml);
            EntityCollection encolAdvancePayment = service.RetrieveMultiple(new FetchExpression(xml));
            return encolAdvancePayment;
            //decimal sumadvancepaymentamount = encolAdvancePayment.Entities.AsEnumerable().Sum(x =>((Money)x.Attributes["bsd_amount"]).Value);
            //return sumadvancepaymentamount;
        }
        private decimal getCOAAmount(Entity enOptionEntry, DateTime fromdate, DateTime todate)
        {
            decimal coaamount = 0;
            fromdate = new DateTime(0);
            EntityCollection encolAdvancePayment = getAdvancePaymentbyOptionEntry(enOptionEntry, fromdate, todate);
            EntityCollection encolApplyDocumentDetail = getApplyDocumentDetail(enOptionEntry, fromdate, todate);
            decimal sumAdvancePayment = encolAdvancePayment.Entities.AsEnumerable().Sum(x => ((Money)x.Attributes["bsd_amount"]).Value);
            decimal sumApplyDocumentDetail = encolApplyDocumentDetail.Entities.AsEnumerable().Sum(x => ((Money)x.Attributes["bsd_amountapply"]).Value);
            coaamount = sumAdvancePayment - sumApplyDocumentDetail;
            //throw new InvalidPluginExecutionException("coaamount: "+ coaamount.ToString());
            return coaamount;
        }
        private void delCollectionReportDetail(EntityReference enrefCollectionReport, EntityReference enrefOptionEntry)
        {
            //Load CollectionReportDetail
            string condition = "";
            if (enrefOptionEntry != null)
            {
                condition += "<condition attribute='bsd_optionentry' operator='eq' uitype='salesorder' value='" + enrefOptionEntry.Id.ToString() + "' />";
            }
            if (enrefCollectionReport != null)
            {
                condition += "<condition attribute='bsd_collectionreport' operator='eq' uitype='bsd_collectionreport' value='" + enrefCollectionReport.Id.ToString() + "' />";
            }
            string xml = @"<fetch version='1.0' output-format='xml-platform' mapping='logical' distinct='false'>
              <entity name='bsd_collectionreportdetail'>
                <attribute name='bsd_collectionreportdetailid' />
                <attribute name='bsd_name' />
                <attribute name='createdon' />
                <order attribute='bsd_name' descending='false' />
                <filter type='and'>
                  " + condition + @"
                </filter>
              </entity>
            </fetch>";
            traceService.Trace(xml);
            EntityCollection encolCollectionReportDetail = service.RetrieveMultiple(new FetchExpression(xml));
            //Xóa CollectionReportDetail
            if (encolCollectionReportDetail.Entities.Count > 0)
            {
                foreach (Entity enCollectionReportDetail in encolCollectionReportDetail.Entities)
                {
                    service.Delete(enCollectionReportDetail.LogicalName, enCollectionReportDetail.Id);
                }
            }
        }
        //Trong OptionEntry có code SAM thì lấy theo payment ngược lại thì lấy ex trên Option Entry “Applying exchange rate”
        //Nếu cả 2 đều rỗng thì lấy theo Applying exchange rate mới nhất
        private decimal getExchangeRate(Entity enOptionEntry, Entity enPayment)
        {
            decimal bsd_exchangerate = 0;
            string bsd_optioncodesams = enOptionEntry.Contains("bsd_optioncodesams") ? (string)enOptionEntry["bsd_optioncodesams"] : "";
            traceService.Trace("bsd_optioncodesams: " + bsd_optioncodesams);
            if (bsd_optioncodesams != "")
            {
                bsd_exchangerate = enOptionEntry.Contains("bsd_exchangeratesams") ? ((Money)enOptionEntry["bsd_exchangeratesams"]).Value : 0;
                if (enPayment != null)
                {
                    bsd_exchangerate = enPayment.Contains("bsd_exchangerate") ? (decimal)enPayment["bsd_exchangerate"] : 0;
                }
            }
            else
            {
                traceService.Trace("bsd_applyingexchangerate: " + enOptionEntry.Contains("bsd_applyingexchangerate").ToString());
                if (enOptionEntry.Contains("bsd_applyingexchangerate"))
                {
                    EntityReference enrefApplyingexchangerate = (EntityReference)enOptionEntry["bsd_applyingexchangerate"];
                    traceService.Trace(enrefApplyingexchangerate.LogicalName + " " + enrefApplyingexchangerate.Id.ToString());
                    Entity enApplyingexchangerate = service.Retrieve(enrefApplyingexchangerate.LogicalName,
                        enrefApplyingexchangerate.Id,
                        new ColumnSet(new string[] { "bsd_rate" }));
                    bsd_exchangerate = enApplyingexchangerate.Contains("bsd_rate") ? ((Money)enApplyingexchangerate["bsd_rate"]).Value : 0;

                }
                else
                {
                    string xml = @"<fetch version='1.0' output-format='xml-platform' mapping='logical' distinct='false' count='1'>
                      <entity name='bsd_exchangeratedetail'>
                        <attribute name='bsd_exchangeratedetailid' />
                        <attribute name='bsd_name' />
                        <attribute name='createdon' />
                        <attribute name='bsd_rate' />
                        <attribute name='bsd_exchangerate' />
                        <order attribute='createdon' descending='true' />
                      </entity>
                    </fetch>";
                    EntityCollection encolApplyingexchangerate = service.RetrieveMultiple(new FetchExpression(xml));
                    bsd_exchangerate = encolApplyingexchangerate.Entities[0].Contains("bsd_rate") ? (decimal)encolApplyingexchangerate.Entities[0]["bsd_rate"] : 0;
                }
            }
            traceService.Trace("bsd_exchangerate: " + bsd_exchangerate.ToString());
            return bsd_exchangerate;
        }
        private EntityCollection getCoowner(Entity enOptionEntry)
        {
            string xml = @"<fetch version='1.0' output-format='xml-platform' mapping='logical' distinct='false'>
              <entity name='bsd_coowner'>
                <attribute name='bsd_coownerid' />
                <attribute name='bsd_name' />
                <attribute name='createdon' />
                <attribute name='ownerid' />
                <attribute name='bsd_optionentry' />
                <attribute name='bsd_customer' />
                <order attribute='bsd_name' descending='false' />
                <filter type='and'>
                  <condition attribute='bsd_optionentry' operator='eq' uitype='salesorder' value='" + enOptionEntry.Id + @"' />
                </filter>
                <link-entity name='contact' from='contactid' to='bsd_customer' visible='false' link-type='outer' alias='coownercontact'>
                  <attribute name='emailaddress1' />
                </link-entity>
                <link-entity name='account' from='accountid' to='bsd_customer' visible='false' link-type='outer' alias='coowneraccount'>
                  <attribute name='emailaddress1' />
                </link-entity>
              </entity>
            </fetch>";
            EntityCollection encol = service.RetrieveMultiple(new FetchExpression(xml));
            return encol;
        }
        private void genCollectionReportDetail(EntityReference enrefCollectionReport, Entity enOptionEntry, int type, DateTime fromdate, DateTime todate)
        {
            //Xóa CollectionReportDetail cũ
            traceService.Trace("Xóa CollectionReportDetail cũ");
            delCollectionReportDetail(enrefCollectionReport, enOptionEntry.ToEntityReference());
            //Lấy các dot thanh toan(Installment) của Option Entry
            traceService.Trace("Lấy các dot thanh toan(Installment) của Option Entry");


            string xml = @"<fetch version='1.0' output-format='xml-platform' mapping='logical' distinct='false'>
              <entity name='bsd_paymentschemedetail'>
                <attribute name='bsd_name' />
                <attribute name='bsd_ordernumber' />
                <attribute name='bsd_duedate' />
                <attribute name='statuscode' />
                <attribute name='bsd_amountofthisphase' />
                <attribute name='bsd_amountwaspaid' />
                <attribute name='bsd_depositamount' />
                <attribute name='bsd_waiveramount' />
                <attribute name='bsd_actualgracedays' />
                <attribute name='bsd_interestchargeamount' />
                <attribute name='bsd_interestwaspaid' />
                <attribute name='bsd_waiverinterest' />
                <attribute name='bsd_balance' />
                <attribute name='bsd_optionentry' />
                <attribute name='bsd_warningnotices2' />
                <attribute name='bsd_warningnotices1' />
                <attribute name='bsd_paymentnotices' />
                <attribute name='bsd_managementfeepaid' />
                <attribute name='bsd_managementamount' />
                <attribute name='bsd_maintenancefeepaid' />
                <attribute name='bsd_maintenanceamount' />
                <attribute name='bsd_paymentschemedetailid' />
                <attribute name='bsd_typepayment' />
                <attribute name='bsd_paiddate' />
                <attribute name='bsd_totalpaybilldiscount' />
                <attribute name='bsd_totalpaybilldiscountdeposit' />
                <attribute name='bsd_totalpaybilldiscountinstallment' />
                <attribute name='bsd_totalpaybilldiscountinstallmentusd' />
                <attribute name='bsd_totalpaybilldiscountfees' />
                <attribute name='bsd_paymentscheme' />
                <order attribute='bsd_ordernumber' descending='false' />
                <filter type='and'>
                  <condition attribute='bsd_optionentry' operator='eq' uitype='salesorder' value='" + enOptionEntry.Id.ToString() + @"' />
                </filter>
              </entity>
            </fetch>";
            traceService.Trace(xml);
            EntityCollection encolInstallment = service.RetrieveMultiple(new FetchExpression(xml));
            //Tạo CollectionReportDetail tương ứng cái đợt thanh toán(Installment)
            traceService.Trace("Lấy tỷ giá");
            decimal exchangerateOptionEntry = getExchangeRate(enOptionEntry, null);
            traceService.Trace("Tạo CollectionReportDetail tương ứng cái đợt thanh toán(Installment)");
            bool issetone = true;
            decimal coaamount = getCOAAmount(enOptionEntry, fromdate, todate);
            decimal bsd_totalsystemreceiptvnd = getTotalSystemReceiptbyOptionEntry(enOptionEntry);
            decimal sumInstallmentPaidAmount = 0;
            decimal totalbill = 0;
            decimal sumManagementFeePayAmountvnd = 0;
            decimal sumMaintenanceFeePayAmountVND = 0;
            decimal sumTotalPayBillDiscountfees = 0;
            //decimal sumTotalPayBillDiscountfees = 0;
            Guid collectionreportdetailidset = new Guid();
            EntityCollection encolCoower = getCoowner(enOptionEntry);
            foreach (Entity enInstallment in encolInstallment.Entities)
            {
                traceService.Trace(enInstallment["bsd_name"].ToString());
                //decimal bsd_totalpaybilldiscountfees = enInstallment.Contains("bsd_totalpaybilldiscountfees")?((Money)enInstallment["bsd_totalpaybilldiscountfees"]).Value:0;
                //sumTotalPayBillDiscountfees += bsd_totalpaybilldiscountfees;
                Entity enCollectionReportDetail = new Entity("bsd_collectionreportdetail");
                enCollectionReportDetail["bsd_name"] = "Collection Report" + enInstallment["bsd_name"];//Name
                enCollectionReportDetail["bsd_collectionreport"] = enrefCollectionReport;//Collection Report
                enCollectionReportDetail["bsd_optionentry"] = enOptionEntry.ToEntityReference();//Option Entry
                enCollectionReportDetail["bsd_units"] = (EntityReference)enOptionEntry["bsd_unitnumber"];//Units
                traceService.Trace("1");
                enCollectionReportDetail["bsd_paymentscheme"] = (EntityReference)enInstallment["bsd_paymentscheme"];//Payment Scheme
                enCollectionReportDetail["bsd_installment"] = enInstallment.ToEntityReference();//Installment
                enCollectionReportDetail["bsd_customer"] = (EntityReference)enOptionEntry["customerid"];//Customer
                traceService.Trace("2");
                if (encolCoower.Entities.Count > 0)
                {
                    enCollectionReportDetail["bsd_coownerp2"] = encolCoower.Entities[0]["bsd_customer"];
                    string email = encolCoower.Entities[0].Contains("coownercontact.emailaddress1") ? ((AliasedValue)encolCoower.Entities[0]["coownercontact.emailaddress1"]).Value.ToString() : "";
                    if (email == "")
                    {
                        email = encolCoower.Entities[0].Contains("coowneraccount.emailaddress1") ? ((AliasedValue)encolCoower.Entities[0]["coowneraccount.emailaddress1"]).Value.ToString() : "";
                    }
                    enCollectionReportDetail["bsd_email1p2"] = email;

                    if (encolCoower.Entities.Count > 1)
                    {
                        enCollectionReportDetail["bsd_coownerp3"] = encolCoower.Entities[1]["bsd_customer"];
                        email = encolCoower.Entities[1].Contains("coownercontact.emailaddress1") ? ((AliasedValue)encolCoower.Entities[1]["coownercontact.emailaddress1"]).Value.ToString() : "";
                        if (email == "")
                        {
                            email = encolCoower.Entities[1].Contains("coowneraccount.emailaddress1") ? ((AliasedValue)encolCoower.Entities[1]["coowneraccount.emailaddress1"]).Value.ToString() : "";
                        }
                        enCollectionReportDetail["bsd_email2p3"] = email;
                    }
                }
                decimal bsd_exchangerate = exchangerateOptionEntry;
                enCollectionReportDetail["bsd_exchangerate"] = bsd_exchangerate;//Exchange Rate
                enCollectionReportDetail["bsd_percentage"] = (decimal)0;//Percentage (%)
                if (issetone)
                {
                    enCollectionReportDetail["bsd_advancepaymentvnd"] = new Money(coaamount);//COA Amount
                    if (bsd_exchangerate > 0)
                        enCollectionReportDetail["bsd_advancepaymentusd"] = coaamount / bsd_exchangerate;//COA Amount

                    enCollectionReportDetail["bsd_totalsystemreceiptvnd"] = new Money(bsd_totalsystemreceiptvnd);//Total System Receipt (VND)
                    if (bsd_exchangerate > 0)
                        enCollectionReportDetail["bsd_totalsystemreceiptusd"] = (decimal)(bsd_totalsystemreceiptvnd / bsd_exchangerate);//Total System Receipt (USD)

                }
                else
                {
                    enCollectionReportDetail["bsd_advancepaymentvnd"] = new Money(0);//Advance Payment (VND)
                    enCollectionReportDetail["bsd_advancepaymentusd"] = (decimal)0;//Advance Payment (USD)

                    enCollectionReportDetail["bsd_totalsystemreceiptvnd"] = new Money(0);//Total System Receipt (VND)
                    enCollectionReportDetail["bsd_totalsystemreceiptusd"] = (decimal)(0);//Total System Receipt (USD)
                }


                traceService.Trace("3");

                decimal bsd_amountofphasevnd = enInstallment.Contains("bsd_amountofthisphase") ? ((Money)enInstallment["bsd_amountofthisphase"]).Value : 0;
                DateTime bsd_duedate = enInstallment.Contains("bsd_duedate") ? (DateTime)enInstallment["bsd_duedate"] : new DateTime(0);
                //Amount of phase (VND)  = [A] – Total Pay Bill Discount (Inst) - Total Pay Bill Discount (Deposit) – Total System Receipt (Inst)
                decimal bsd_ins_totalpaybilldiscount = enInstallment.Contains("bsd_totalpaybilldiscount") ? ((Money)enInstallment["bsd_totalpaybilldiscount"]).Value : 0;
                decimal bsd_ins_totalpaybilldiscountdeposit = enInstallment.Contains("bsd_totalpaybilldiscountdeposit") ? ((Money)enInstallment["bsd_totalpaybilldiscountdeposit"]).Value : 0;
                decimal bsd_ins_totalpaybilldiscountinstallment = enInstallment.Contains("bsd_totalpaybilldiscountinstallment") ? ((Money)enInstallment["bsd_totalpaybilldiscountinstallment"]).Value : 0;
                decimal bsd_ins_totalpaybilldiscountfees = enInstallment.Contains("bsd_totalpaybilldiscountfees") ? ((Money)enInstallment["bsd_totalpaybilldiscountfees"]).Value : 0;
                sumTotalPayBillDiscountfees += bsd_ins_totalpaybilldiscountfees;
                //if (type == 100000001)//HN
                //{
                //    enCollectionReportDetail["bsd_amountofphasevnd"] = new Money(bsd_amountofphasevnd - bsd_ins_totalpaybilldiscount - bsd_ins_totalpaybilldiscountinstallment);//Amount of phase (VND)
                //}
                //else
                //{
                //    enCollectionReportDetail["bsd_amountofphasevnd"] = new Money(bsd_amountofphasevnd);//Amount of phase (VND)
                //}
                enCollectionReportDetail["bsd_amountofphasevnd"] = new Money(bsd_amountofphasevnd);//Amount of phase (VND)
                if (bsd_duedate <= todate)
                {
                    totalbill += bsd_amountofphasevnd;
                }
                
                if (bsd_exchangerate > 0)
                {
                    Money bsd_amountofthisphaseusd = enInstallment.Contains("bsd_amountofthisphaseusd") ? (Money)enInstallment["bsd_amountofthisphaseusd"] : new Money(bsd_amountofphasevnd / exchangerateOptionEntry);
                    enCollectionReportDetail["bsd_amountofphaseusd"] = bsd_amountofthisphaseusd.Value;//Amount of phase (USD)
                }

                //Stage - Paid Amount (VND) = Sum(Payment) + Sum(Apply Doc)
                SumTransactionPayment sumTransactionPayment = sumTransactionPaymentByInstallment(enInstallment, fromdate, todate);
                SumApplyDocumentDetail sumApplyDocumentDetail = sumApplyDocumentDetailByInstallment(enOptionEntry, enInstallment, fromdate, todate);
                SumPayments sumPayments = sumPaymentsbyInstallment(enOptionEntry,enInstallment, fromdate, todate);

                if (type == 100000001)//HN
                {
                    if (enInstallment.Contains("bsd_paiddate"))
                    {
                        enCollectionReportDetail["bsd_receiptdate"] = enInstallment["bsd_paiddate"];//Receipt Date
                    }
                    else
                    {
                        traceService.Trace("SumPayments receiptDate:" + sumPayments.receiptDate.ToString());
                        traceService.Trace("SumTransactionPayment receiptDate:" + sumTransactionPayment.receiptDate.ToString());
                        traceService.Trace("SumApplyDocumentDetail receiptDate:" + sumApplyDocumentDetail.receiptDate.ToString());
                        long receiptDateTicks = Math.Max(sumPayments.receiptDate.Ticks, sumTransactionPayment.receiptDate.Ticks);
                        receiptDateTicks = Math.Max(receiptDateTicks, sumApplyDocumentDetail.receiptDate.Ticks);
                        traceService.Trace("receiptDateTicks: " + receiptDateTicks.ToString());
                        if (receiptDateTicks > 0)
                        {
                            DateTime receiptDate = new DateTime(receiptDateTicks);
                            traceService.Trace(receiptDate.ToString());

                            enCollectionReportDetail["bsd_receiptdate"] = receiptDate;//Receipt Date
                        }
                    }
                }
                else
                {//HCM
                    if (enInstallment.Contains("bsd_paiddate"))
                    {
                        enCollectionReportDetail["bsd_receiptdate"] = enInstallment["bsd_paiddate"];//Receipt Date
                    }
                    else
                    {
                        enCollectionReportDetail["bsd_receiptdate"] = null;//Receipt Date
                    }
                }

                //decimal bsd_depositamountins = enInstallment.Contains("bsd_depositamount") ? ((Money)enInstallment["bsd_depositamount"]).Value : 0;
                //traceService.Trace("bsd_depositamountins: "+ bsd_depositamountins.ToString());
      
                decimal bsd_installmentpaidamountvnd = sumPayments.sumBalance + sumTransactionPayment.sumInstallments + sumApplyDocumentDetail.sumInstallments;
                traceService.Trace("bsd_installmentpaidamountvnd:" + bsd_installmentpaidamountvnd.ToString());
                traceService.Trace("sumPayments:" + sumPayments.sumBalance.ToString());
                traceService.Trace("sumTransactionPayment:" + sumTransactionPayment.sumInstallments.ToString());
                traceService.Trace("sumApplyDocumentDetail:" + sumApplyDocumentDetail.sumInstallments.ToString());
                if (bsd_installmentpaidamountvnd > bsd_amountofphasevnd)
                {
                    bsd_installmentpaidamountvnd = bsd_amountofphasevnd;
                }
                if (type == 100000001)//HN
                {
                    decimal bsd_totalpaybilldiscountinstallment = enInstallment.Contains("bsd_totalpaybilldiscountinstallment") ? ((Money)enInstallment["bsd_totalpaybilldiscountinstallment"]).Value : 0;
                    decimal bsd_totalsystemreceiptinst = enInstallment.Contains("bsd_totalsystemreceiptinst") ? ((Money)enInstallment["bsd_totalsystemreceiptinst"]).Value : 0;
                    decimal bsd_waiverinstallment = enInstallment.Contains("bsd_waiverinstallment") ? ((Money)enInstallment["bsd_waiverinstallment"]).Value : 0;
                    bsd_installmentpaidamountvnd -= (bsd_totalpaybilldiscountinstallment + bsd_totalsystemreceiptinst + bsd_waiverinstallment);

                }
                
                enCollectionReportDetail["bsd_installmentpaidamountvnd"] = new Money(bsd_installmentpaidamountvnd);//Installment - Paid Amount (VND)
                if (bsd_exchangerate > 0)
                    enCollectionReportDetail["bsd_installmentpaidamountusd"] = new Money(bsd_installmentpaidamountvnd / bsd_exchangerate);//Installment - Paid Amount (USD)
                sumInstallmentPaidAmount += bsd_installmentpaidamountvnd;
                traceService.Trace("4");

                enCollectionReportDetail["bsd_paidamount"] = new Money(0);//Total Collection (Installment) (VND)
                enCollectionReportDetail["bsd_totalcollectioninstallmentusd"] = (decimal)(0);//Total Collection (Installment) (USD)

                traceService.Trace("5");
                decimal bsd_managementfeepayamountvnd = sumApplyDocumentDetail.sumManafee + sumTransactionPayment.sumManafee +sumPayments.sumManagementfee;
                enCollectionReportDetail["bsd_managementfeepayamountvnd"] = new Money(bsd_managementfeepayamountvnd);//Management Fee Pay Amount (VND)
                sumManagementFeePayAmountvnd += bsd_managementfeepayamountvnd;
                decimal bsd_maintenancefeepayamountvnd = sumApplyDocumentDetail.sumMainfee + sumTransactionPayment.sumMainfee + sumPayments.sumMaintenancefee;
                enCollectionReportDetail["bsd_maintenancefeepayamountvnd"] = new Money(bsd_maintenancefeepayamountvnd);//Maintenance Fee Pay Amount (VND)
                sumMaintenanceFeePayAmountVND += bsd_maintenancefeepayamountvnd;
                decimal bsd_interestpayamountvnd = sumApplyDocumentDetail.sumInterest + sumTransactionPayment.sumInterest;
                enCollectionReportDetail["bsd_interestpayamountvnd"] = new Money(bsd_interestpayamountvnd);//Interest Pay Amount (VND)
                enCollectionReportDetail["bsd_areapayamountvnd"] = new Money(0);//Area Pay Amount (VND)
                traceService.Trace("6");
                enCollectionReportDetail["bsd_totalcollectioninstallmentcoavnd"] = new Money(0);//Total Collection (Installment + COA) (VND)
                enCollectionReportDetail["bsd_totalcollectioninstallmentcoausd"] = (decimal)(0);//Total Collection (Installment + COA) (USD)
                enCollectionReportDetail["bsd_totalbillvnd"] = new Money(0);//Total Bill (VND)
                enCollectionReportDetail["bsd_totalbillusd"] = (decimal)(0);//Total Bill (USD)
                traceService.Trace("7");
                if (issetone)
                {
                    collectionreportdetailidset = service.Create(enCollectionReportDetail);
                }
                else
                {
                    service.Create(enCollectionReportDetail);
                }
                issetone = false;
            }
            //Total Collection (Installment) (VND)= Sum(Installment-Stage–Paid Amount (VND)) + Deposit Amount - Total Pay Bill Discount - VAT Adj Amount (SAMS)
            //Total Collection (Installment) (VND)= Sum(Installment-Stage–Paid Amount (VND)) + Deposit Amount (Công thư mới)
            decimal bsd_depositamount = enOptionEntry.Contains("bsd_depositamount") ? ((Money)enOptionEntry["bsd_depositamount"]).Value : 0;
            decimal bsd_totalpaybilldiscount = enOptionEntry.Contains("bsd_totalpaybilldiscount") ? ((Money)enOptionEntry["bsd_totalpaybilldiscount"]).Value : 0;
            decimal bsd_vatadjamountsams = enOptionEntry.Contains("bsd_vatadjamountsams") ? ((Money)enOptionEntry["bsd_vatadjamountsams"]).Value : 0;
            //decimal totalCollectionInstallmentVND = sumInstallmentPaidAmount + bsd_depositamount - bsd_totalpaybilldiscount - bsd_vatadjamountsams;
            decimal totalCollectionInstallmentVND = sumInstallmentPaidAmount + bsd_depositamount;
            Entity enCollectionReportDetail1 = new Entity("bsd_collectionreportdetail", collectionreportdetailidset);
            enCollectionReportDetail1["bsd_paidamount"] = new Money(totalCollectionInstallmentVND);//Total Collection (Installment) (VND)
            if (exchangerateOptionEntry > 0)
                enCollectionReportDetail1["bsd_totalcollectioninstallmentusd"] = totalCollectionInstallmentVND / exchangerateOptionEntry;//Total Collection (Installment) (USD)

            decimal bsd_totalcollectioninstallmentcoavnd = totalCollectionInstallmentVND + coaamount;
            enCollectionReportDetail1["bsd_totalcollectioninstallmentcoavnd"] = new Money(bsd_totalcollectioninstallmentcoavnd);//Total Collection (Installment + COA) (VND)
            if (exchangerateOptionEntry > 0)
                enCollectionReportDetail1["bsd_totalcollectioninstallmentcoausd"] = bsd_totalcollectioninstallmentcoavnd / exchangerateOptionEntry;//Total Collection (Installment + COA) (VND)
            //Percentage (%) = Total Collection (Installment + COA) / Net Selling Price (After VAT) * 100
            decimal bsd_totalamountlessfreight = enOptionEntry.Contains("bsd_totalamountlessfreight") ? ((Money)enOptionEntry["bsd_totalamountlessfreight"]).Value : 0;//Net Selling Price
            decimal totaltax = enOptionEntry.Contains("totaltax") ? ((Money)enOptionEntry["totaltax"]).Value : 0;//Net Selling Price
            decimal netSellingPriceafterVAT = bsd_totalamountlessfreight + totaltax;
            decimal bsd_percentage = bsd_totalcollectioninstallmentcoavnd / netSellingPriceafterVAT * 100;
            enCollectionReportDetail1["bsd_percentage"] = bsd_percentage;
            enCollectionReportDetail1["bsd_totalbillvnd"] = new Money(totalbill);
            if (exchangerateOptionEntry > 0)
                enCollectionReportDetail1["bsd_totalbillusd"] = totalbill / exchangerateOptionEntry;
            //Total Paid (VND) = Total Collection (Installment + COA) – Discount (SAMS) + "Maintenance Fee (All)_Pay Amount (VND)
            decimal bsd_discountsams = enOptionEntry.Contains("bsd_discountsams") ? ((Money)enOptionEntry["bsd_discountsams"]).Value : 0;
            decimal bsd_totalpaidvnd = bsd_totalcollectioninstallmentcoavnd - bsd_discountsams + sumMaintenanceFeePayAmountVND;
            enCollectionReportDetail1["bsd_totalpaidvnd"] = new Money(bsd_totalpaidvnd);
            if (exchangerateOptionEntry > 0)
                enCollectionReportDetail1["bsd_totalpaidusd"] = new Money(bsd_totalpaidvnd / exchangerateOptionEntry);
            enCollectionReportDetail1["bsd_maintenancefeeallpayamountvnd"] = new Money(sumMaintenanceFeePayAmountVND);
            enCollectionReportDetail1["bsd_managementfeeallpayamountvnd"] = new Money(sumManagementFeePayAmountvnd);
            enCollectionReportDetail1["bsd_discountmaintenancefeevnd"] = new Money(sumTotalPayBillDiscountfees);
            service.Update(enCollectionReportDetail1);

        }

    }
    class SumPayments
    {
        public decimal sumQueuingfee;//100000000
        public decimal sumDepositfee;//100000001
        public decimal sumInstallment;//100000002
        public decimal sumInterest;//100000003
        public decimal sumFees;//100000004
        public decimal sumOther;//100000005
        public decimal sumBalance;
        public decimal sumMaintenancefee;
        public decimal sumManagementfee;
        private Entity enOptionEntry; 
        public DateTime receiptDate;
        public SumPayments(Entity optionEntry)
        {
            sumQueuingfee = 0;
            sumDepositfee = 0;
            sumInstallment = 0;
            sumInterest = 0;
            sumFees = 0;
            sumOther = 0;
            sumBalance = 0;
            sumMaintenancefee = 0;
            sumManagementfee = 0;
            receiptDate = new DateTime(0);
            enOptionEntry = optionEntry;
        }
        public void add(Entity enPayments)
        {
            int bsd_paymenttype = enPayments.Contains("bsd_paymenttype") ? ((OptionSetValue)enPayments["bsd_paymenttype"]).Value : 0;
            decimal bsd_amountpay = enPayments.Contains("bsd_amountpay") ? ((Money)enPayments["bsd_amountpay"]).Value : 0;
            decimal bsd_differentamount = enPayments.Contains("bsd_differentamount") ? ((Money)enPayments["bsd_differentamount"]).Value : 0;
            decimal bsd_depositamount = enPayments.Contains("bsd_depositamount") ? ((Money)enPayments["bsd_depositamount"]).Value : 0;
            DateTime bsd_paymentactualtime = enPayments.Contains("bsd_paymentactualtime") ? (DateTime)enPayments["bsd_paymentactualtime"] : new DateTime(0);
            
            if (receiptDate < bsd_paymentactualtime)
            {
                receiptDate = bsd_paymentactualtime;
            }
            switch (bsd_paymenttype)
            {
                case 100000000:
                    sumQueuingfee += bsd_amountpay;
                    break;
                case 100000001:
                    sumDepositfee += bsd_depositamount;
                    break;
                case 100000002:
                    sumInstallment += bsd_amountpay;
                    sumBalance += bsd_amountpay - bsd_differentamount;
                    break;
                case 100000003:
                    break;
                case 100000004://Fees
                    string bsd_paymentnumbersams = enPayments.Contains("bsd_paymentnumbersams") ? enPayments["bsd_paymentnumbersams"].ToString() : "";
                    
                    if (bsd_paymentnumbersams != "")
                    {
                        decimal mainfee = enOptionEntry.Contains("bsd_freightamount") ? ((Money)enOptionEntry["bsd_freightamount"]).Value : 0;
                        decimal manafee = enOptionEntry.Contains("bsd_managementfee") ? ((Money)enOptionEntry["bsd_managementfee"]).Value : 0;
                        sumMaintenancefee = mainfee;
                        sumManagementfee = manafee;
                    }
                    break;

            }
        }
    }
    class SumTransactionPayment
    {
        public decimal sumInstallments;//100000000
        public decimal sumInterest;//100000001
        public decimal sumFees;//100000002
        public decimal sumOther;//100000003
        public decimal sumMainfee;
        public decimal sumManafee;
        
        public DateTime receiptDate;
        public SumTransactionPayment()
        {
            sumInstallments = 0;
            sumInterest = 0;
            sumFees = 0;
            sumOther = 0;
            sumMainfee = 0;
            sumManafee = 0;
            
            receiptDate = new DateTime(0);
        }
        public void add(Entity enTransactionPayment)
        {
            int bsd_transactiontype = enTransactionPayment.Contains("bsd_transactiontype") ? ((OptionSetValue)enTransactionPayment["bsd_transactiontype"]).Value : 0;
            decimal bsd_amount = enTransactionPayment.Contains("bsd_amount") ? ((Money)enTransactionPayment["bsd_amount"]).Value : 0;
            DateTime createdon = enTransactionPayment.Contains("createdon") ? (DateTime)enTransactionPayment["createdon"] : new DateTime(0);
            if (receiptDate < createdon)
            {
                receiptDate = createdon;
            }
            switch (bsd_transactiontype)
            {
                case 100000000:
                    sumInstallments += bsd_amount;
                    break;
                case 100000001:
                    sumInterest += bsd_amount;
                    break;
                case 100000002:
                    sumFees += bsd_amount;
                    int bsd_feetype = enTransactionPayment.Contains("bsd_feetype") ? ((OptionSetValue)enTransactionPayment["bsd_feetype"]).Value : 0;
                    if (bsd_feetype == 100000000)
                    {
                        sumMainfee += bsd_amount;
                    }
                    else
                    {
                        sumManafee += bsd_amount;
                    }
                    break;
                case 100000003:
                    sumOther += bsd_amount;
                    break;
            }
        }
    }
    class SumApplyDocumentDetail
    {
        public decimal sumDeposit;//100000000
        public decimal sumInstallments;//100000001
        public decimal sumInterest;//100000003
        public decimal sumFees;//100000002
        public decimal sumOther;//100000004
        public decimal sumMainfee;
        public decimal sumManafee;
        public DateTime receiptDate;
        public SumApplyDocumentDetail()
        {
            sumInstallments = 0;
            sumInterest = 0;
            sumFees = 0;
            sumOther = 0;
            sumMainfee = 0;
            sumManafee = 0;
            receiptDate = new DateTime(0);
        }
        public void add(Entity enApplyDocumentDetail)
        {
            int bsd_paymenttype = enApplyDocumentDetail.Contains("bsd_paymenttype") ? ((OptionSetValue)enApplyDocumentDetail["bsd_paymenttype"]).Value : 0;
            decimal bsd_amountapply = enApplyDocumentDetail.Contains("bsd_amountapply") ? ((Money)enApplyDocumentDetail["bsd_amountapply"]).Value : 0;
            DateTime createdon = enApplyDocumentDetail.Contains("createdon") ? (DateTime)enApplyDocumentDetail["createdon"] : new DateTime(0);
            if (receiptDate < createdon)
            {
                receiptDate = createdon;
            }
            switch (bsd_paymenttype)
            {
                case 100000000:
                    sumDeposit += bsd_amountapply;
                    break;
                case 100000001:
                    sumInstallments += bsd_amountapply;
                    break;
                case 100000003:
                    sumInterest += bsd_amountapply;
                    break;
                case 100000002:
                    sumFees += bsd_amountapply;
                    int bsd_feetype = enApplyDocumentDetail.Contains("bsd_feetype") ? ((OptionSetValue)enApplyDocumentDetail["bsd_feetype"]).Value : 0;
                    if (bsd_feetype == 100000000)
                    {
                        sumMainfee += bsd_amountapply;
                    }
                    else
                    {
                        sumManafee += bsd_amountapply;
                    }
                    break;
                case 100000004:
                    sumOther += bsd_amountapply;
                    break;
            }
        }
    }

}
