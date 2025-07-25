﻿using Microsoft.Crm.Sdk.Messages;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Query;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.Script.Serialization;

namespace Action_Calculate_Interest
{
    public class Action_Calculate_Interest : IPlugin
    {
        public IOrganizationService service = null;
        IOrganizationServiceFactory factory = null;
        public ITracingService traceService = null;
        ITracingService TracingSe = null;
        public Installment objIns = new Installment();
        private Entity enInstallment;

        public void Execute(IServiceProvider serviceProvider)
        {

            IPluginExecutionContext context = (IPluginExecutionContext)serviceProvider.GetService(typeof(IPluginExecutionContext));
            factory = (IOrganizationServiceFactory)serviceProvider.GetService(typeof(IOrganizationServiceFactory));
            service = factory.CreateOrganizationService(context.UserId);
            traceService = (ITracingService)serviceProvider.GetService(typeof(ITracingService));
            TracingSe = (ITracingService)serviceProvider.GetService(typeof(ITracingService));
            try
            {
                string installmentid = context.InputParameters["installmentid"].ToString();
                string stramountpay = context.InputParameters["amountpay"].ToString();

                string receiptdateimport = (string)context.InputParameters["receiptdate"];
                var serializedResult = "";
                Main(installmentid, stramountpay, receiptdateimport, ref serializedResult);
                context.OutputParameters["result"] = serializedResult.ToString();
            }
            catch (InvalidPluginExecutionException ex)
            {
                traceService.Trace(ex.ToString());
                context.OutputParameters["result"] = ex.ToString();
                throw new InvalidPluginExecutionException(ex.ToString());

            }

        }
        public void Main(string installmentid, string stramountpay, string receiptdateimport, ref string serializedResult)
        {
            decimal amountpay = Convert.ToDecimal(stramountpay);
            DateTime receiptdate = Convert.ToDateTime(receiptdateimport);

            receiptdate = RetrieveLocalTimeFromUTCTime(receiptdate);
            enInstallment = service.Retrieve("bsd_paymentschemedetail", new Guid(installmentid), new ColumnSet(true));
            traceService.Trace("2");
            getInterestStartDate();
            objIns.LateDays = getLateDays(receiptdate);
            objIns.InterestCharge = calc_InterestCharge(receiptdate, amountpay);
            var serializer = new JavaScriptSerializer();
            serializedResult = serializer.Serialize(objIns);
        }
        private void getInterestStartDate()
        {
            try
            {
                EntityReference enrefOptionEntry = enInstallment.Contains("bsd_optionentry") ? (EntityReference)enInstallment["bsd_optionentry"] : null;
                Entity enOptionEntry = enInstallment.Contains("bsd_optionentry") ? service.Retrieve(enrefOptionEntry.LogicalName, enrefOptionEntry.Id, new ColumnSet(true)) : null;

                EntityReference enrefPaymentScheme = enOptionEntry.Contains("bsd_paymentscheme") ? (EntityReference)enOptionEntry["bsd_paymentscheme"] : null;
                Entity enPaymentScheme = service.Retrieve(enrefPaymentScheme.LogicalName, enrefPaymentScheme.Id, new ColumnSet(true));
                EntityReference enrefInterestrateMaster = enPaymentScheme.Contains("bsd_interestratemaster") ? (EntityReference)enPaymentScheme["bsd_interestratemaster"] : null;
                if (enrefInterestrateMaster != null)
                {
                    Entity enInterestrateMaster = service.Retrieve(enrefInterestrateMaster.LogicalName, enrefInterestrateMaster.Id, new ColumnSet(true));
                    objIns.Gracedays = enInterestrateMaster.Contains("bsd_gracedays") ? ((int)enInterestrateMaster["bsd_gracedays"]) : 0;
                    objIns.orderNumber = enInstallment.Contains("bsd_ordernumber") ? ((int)enInstallment["bsd_ordernumber"]) : 0;
                    objIns.idOE = enOptionEntry.Id;
                    objIns.Intereststartdatetype = ((OptionSetValue)enInterestrateMaster["bsd_intereststartdatetype"]).Value;//100000000: Due date;100000001: Grace period
                                                                                                                             //throw new InvalidPluginExecutionException(((DateTime)enInstallment["bsd_duedate"]).ToString());
                    objIns.Duedate = enInstallment.Contains("bsd_duedate") ? RetrieveLocalTimeFromUTCTime((DateTime)enInstallment["bsd_duedate"]) : DateTime.Now;

                    objIns.MaxPercent = enInterestrateMaster.Contains("bsd_toleranceinterestpercentage") ? (decimal)enInterestrateMaster["bsd_toleranceinterestpercentage"] : 100;
                    objIns.MaxAmount = enInterestrateMaster.Contains("bsd_toleranceinterestamount") ? ((Money)enInterestrateMaster["bsd_toleranceinterestamount"]).Value : 0;
                    objIns.InterestPercent = enInstallment.Contains("bsd_interestchargeper") ? (decimal)enInstallment["bsd_interestchargeper"] : 0;
                    objIns.InterestStarDate = objIns.Duedate.AddDays(objIns.Gracedays + 1);
                    traceService.Trace("InterestStarDate: " + objIns.InterestStarDate.ToString());
                }
            }
            catch (InvalidPluginExecutionException ex)
            {
                throw new InvalidPluginExecutionException(ex.ToString());
            }


        }
        public int getLateDays(DateTime dateCalculate)
        {
            try
            {
                int lateDays = (int)dateCalculate.Date.Subtract(objIns.Duedate.Date).TotalDays;
                lateDays = lateDays < 0 ? 0 : lateDays;
                int orderNumberSightContract = getViTriDotSightContract(objIns.idOE);
                Entity oe = service.Retrieve("salesorder", objIns.idOE, new ColumnSet(true));
                int bsd_ordernumber = objIns.orderNumber;
                int numberOfDays2 = 0;
                traceService.Trace("bsd_ordernumber " + bsd_ordernumber);
                traceService.Trace("orderNumberSightContract " + orderNumberSightContract);
                traceService.Trace("dateCalculate " + dateCalculate);
                if (orderNumberSightContract != -1)
                {
                    if (orderNumberSightContract <= bsd_ordernumber && oe.Contains("bsd_signedcontractdate"))
                    {
                        numberOfDays2 = -100599;
                    }
                    else if (orderNumberSightContract > bsd_ordernumber)
                    {
                        if (oe.Contains("bsd_signeddadate"))
                        {
                            DateTime bsd_signeddadate = RetrieveLocalTimeFromUTCTime((DateTime)oe["bsd_signeddadate"]);
                            TimeSpan difference2 = dateCalculate - bsd_signeddadate;
                            numberOfDays2 = difference2.Days;
                            numberOfDays2 = numberOfDays2 < 0 ? 0 : numberOfDays2;
                            traceService.Trace("bsd_signeddadate " + bsd_signeddadate);
                            traceService.Trace("numberOfDays2 " + numberOfDays2);
                        }
                    }
                    else numberOfDays2 = -100599;
                }
                if (objIns.Intereststartdatetype == 100000001)// Grace Period
                {
                    lateDays = lateDays - objIns.Gracedays;
                    if (numberOfDays2 != -100599 && numberOfDays2 < lateDays) lateDays = numberOfDays2;
                }
                else//DueDate
                {
                    if (numberOfDays2 != -100599 && numberOfDays2 < lateDays) lateDays = numberOfDays2;
                    if (objIns.InterestStarDate > dateCalculate)
                    {
                        lateDays = 0;
                    }
                }

                objIns.LateDays = lateDays;
                if (lateDays < 0)
                    lateDays = 0;
                return lateDays;
            }
            catch (InvalidPluginExecutionException ex)
            {
                traceService.Trace(ex.ToString());
                throw new InvalidPluginExecutionException(ex.ToString());
            }

        }
        private int getViTriDotSightContract(Guid idOE)
        {
            int location = -1;
            var fetchXml_instalment = $@"<?xml version=""1.0"" encoding=""utf-16""?>
                                        <fetch>
                                          <entity name=""bsd_paymentschemedetail"">
                                            <attribute name=""bsd_ordernumber"" />
                                            <filter>
                                              <condition attribute=""bsd_optionentry"" operator=""eq"" value=""{idOE}"" />
                                              <condition attribute=""bsd_signcontractinstallment"" operator=""eq"" value=""1"" />
                                              <condition attribute=""statecode"" operator=""eq"" value=""0"" />
                                            </filter>
                                          </entity>
                                        </fetch>";
            EntityCollection rs = service.RetrieveMultiple(new FetchExpression(fetchXml_instalment));
            foreach (Entity entity in rs.Entities)
            {
                location = entity.Contains("bsd_ordernumber") ? (int)entity["bsd_ordernumber"] : 0;
            }
            return location;
        }
        /// <summary>
        /// Điều chỉnh logic code 18.09.2019
        /// </summary>
        /// <param name="dateCalculate"></param>
        /// <param name="amountPay"></param>
        /// <returns></returns>
        public decimal calc_InterestCharge(DateTime dateCalculate, decimal amountPay)
        {
            try
            {
                var result = 0m;
                traceService.Trace("[calc_InterestCharge2] Tham số đầu vào:");
                traceService.Trace(string.Format("- amountPay: {0}", format_Money(amountPay)));
                traceService.Trace("11111111");
                EntityReference OE_ref = enInstallment.Contains("bsd_optionentry") ? (EntityReference)enInstallment["bsd_optionentry"] : null;
                if (OE_ref == null)
                {
                    traceService.Trace(string.Format("Không có dữ liệu record [bsd_optionentry]"));
                    throw new Exception("Không có dữ liệu record [bsd_optionentry]");
                }
                #region --- Body Code ---
                if (OE_ref != null)
                {
                    traceService.Trace(string.Format("ID record [bsd_optionentry]: {0}", OE_ref.Id));
                    decimal interestcharge_amount = 0;
                    Entity OE = service.Retrieve(OE_ref.LogicalName, OE_ref.Id, new ColumnSet(true));
                    Entity Project = service.Retrieve("bsd_project", ((EntityReference)OE["bsd_project"]).Id, new ColumnSet(new string[] { "bsd_name", "bsd_dailyinterestchargebank" }));
                    bool bsd_dailyinterestchargebank = Project.Contains("bsd_dailyinterestchargebank") ? (bool)Project["bsd_dailyinterestchargebank"] : false;
                    decimal d_dailyinterest = 0;

                    traceService.Trace(string.Format("- bsd_dailyinterestchargebank: {0}", bsd_dailyinterestchargebank));
                    if (bsd_dailyinterestchargebank)
                    {
                        #region --- Lấy thông tin setup bsd_dailyinterestrate, dòng đầu tiên ---
                        EntityCollection entc = get_ec_bsd_dailyinterestrate(Project.Id);
                        if (entc.Entities.Count > 0)
                        {
                            Entity ent = entc.Entities[0];
                            ent.Id = entc.Entities[0].Id;
                            if (!ent.Contains("bsd_interestrate"))
                                throw new InvalidPluginExecutionException("Can not find Daily Interestrate in Project " + (string)Project["bsd_name"] + " in master data. Please check again!");
                            d_dailyinterest = (decimal)ent["bsd_interestrate"];
                        }

                        #endregion
                    }
                    traceService.Trace("InterestPercent" + objIns.InterestPercent);
                    traceService.Trace("d_dailyinterest" + d_dailyinterest);

                    objIns.InterestPercent = (objIns.InterestPercent + d_dailyinterest);
                    decimal interestcharge_percent = objIns.InterestPercent / 100 * objIns.LateDays;
                    interestcharge_amount = Convert.ToDecimal(amountPay) * interestcharge_percent;
                    traceService.Trace("LateDays" + objIns.LateDays);
                    traceService.Trace("amountPay" + amountPay);
                    #region --- Trace đợt đang xét ---
                    traceService.Trace(string.Format("----------------------------------------------------"));
                    traceService.Trace(string.Format("#. [Đợt đang xét] "));
                    traceService.Trace(string.Format("- InterestPercent: {0}", format_Money(objIns.InterestPercent)));
                    traceService.Trace(string.Format("- interestcharge_percent: {0}", format_Money(interestcharge_percent)));
                    traceService.Trace(string.Format("- interestcharge_amount 'tiền lãi': {0}", format_Money(interestcharge_amount)));
                    traceService.Trace(string.Format("----------------------------------------------------"));
                    #endregion

                    decimal sum_bsd_waiverinterest = sumWaiverInterest(OE);
                    decimal sum_Inr_AM = SumInterestAM_OE_New(OE.Id, dateCalculate, amountPay) - sum_bsd_waiverinterest;
                    decimal sum_temp = sum_Inr_AM + interestcharge_amount;

                    #region --- Trace kết quả tổng đợt: tạm tính ---
                    traceService.Trace(string.Format("----------------------------------------------------"));
                    traceService.Trace(string.Format("##. Kết quả tổng đợt TẠM TÍNH: "));
                    traceService.Trace(string.Format("- Số tiền trả trước [waiverinterest]: {0}", format_Money(sum_bsd_waiverinterest)));
                    traceService.Trace(string.Format("- Tổng lãi phát sinh, không tính lãi đợt đang xét [sum_Inr_AM]: {0}", format_Money(sum_Inr_AM)));
                    traceService.Trace(string.Format("- Tổng lãi phát sinh [sum_temp] (bao gồm đợt đang xét): {0}", format_Money(sum_temp)));
                    traceService.Trace(string.Format("----------------------------------------------------"));
                    #endregion

                    #region --- @. Throw theo điều kiện: Hân note ---
                    // 170224 - @Han confirm - su dung field net selling price de tinh interest charge  - k dung total amount
                    decimal d_enOptionEntry_bsd_totalamountlessfreight = OE.Contains("bsd_totalamountlessfreight") ? ((Money)OE["bsd_totalamountlessfreight"]).Value : 0;
                    if (d_enOptionEntry_bsd_totalamountlessfreight == 0) throw new InvalidPluginExecutionException("'Net Selling Price' of " + (string)OE["name"] + " must be larger than 0");

                    // Han_28072018 - Update field tính Interest Charge = field Total Amount
                    decimal enOptionEntry_TotalAmount = OE.Contains("totalamount") ? ((Money)OE["totalamount"]).Value : 0;
                    if (enOptionEntry_TotalAmount == 0) throw new InvalidPluginExecutionException("'Total Amount' of " + (string)OE["name"] + " must be larger than 0");
                    #endregion

                    #region --- #. Tính số tiền CAP ---
                    decimal range_enOptionEntryAM = 0;
                    decimal cap = 0;
                    /* ------------------------------------------------------------------------
                     * Lưu ý: 
                     * - Tính total range interertcharge dựa vào tỷ lệ % và số tiền
                     * - Nếu range nào chạm mức trước thì tính range đó
                     *      + MaxPercent ->  Cài đặt %, tính ra số tiền theo %
                     *      + Maxamount  ->  Cài đặt trước số tiền
                     --------------------------------------------------------------------------*/
                    traceService.Trace("#. Tính số tiền CAP:");
                    traceService.Trace(string.Format("- MaxPercent: {0}", format_Money(objIns.MaxPercent)));
                    traceService.Trace(string.Format("- Maxamount: {0}", format_Money(objIns.MaxAmount)));

                    if (objIns.MaxPercent > 0)
                    {
                        //range_enOptionEntryAM = d_enOptionEntry_bsd_totalamountlessfreight * MaxPercent / 100;
                        range_enOptionEntryAM = enOptionEntry_TotalAmount * objIns.MaxPercent / 100;
                        if (objIns.MaxAmount > 0)
                        {
                            if (range_enOptionEntryAM > objIns.MaxAmount) cap = objIns.MaxAmount;
                            else cap = range_enOptionEntryAM;
                        }
                        else cap = range_enOptionEntryAM;
                    }
                    else
                    {
                        cap = objIns.MaxAmount > 0 ? objIns.MaxAmount : 0;
                    }

                    traceService.Trace(string.Format("- Calc Amout of MaxPercent: {0}", format_Money(range_enOptionEntryAM)));
                    traceService.Trace(string.Format("@ CAP: {0}", format_Money(cap)));
                    #endregion

                    #region --- @@ Nghiệp vụ tính tiền lãi ---
                    /* ------------------------------------------------------------------------------------------
                     * Khái niệm: 
                     * - Tổng tiền trễ tất cả các đợt. bao gồm đợt hiện tại             : sum_temp
                     * - Cap : số tiền chạm móc đầu tiên ()                             : cap    
                     * - Tổng tiền trễ không tính đợt hiện tại - sum(waiverinterest)    : sum_Inr_AM
                     * - Tiền trễ đợt hiện tại                                          : interestcharge_amount
                     --------------------------------------------------------------------------------------------*/
                    traceService.Trace("222222222");
                    if (cap <= 0)
                    {
                        var rs = check_Data_Setup();
                        traceService.Trace(string.Format("Case: cap <= 0"));
                        if (rs)
                        {
                            traceService.Trace("Không giới hạn tiền lãi");
                            result = interestcharge_amount;
                        }
                        else
                        {
                            traceService.Trace("Cap thiết lặp chạm móc là 0. return 0");
                            result = 0m;
                        }
                    }
                    else if (sum_temp > cap)
                    {
                        traceService.Trace(string.Format("Case: sum_temp > cap"));
                        if (cap > sum_Inr_AM)
                        {
                            traceService.Trace(string.Format("@ cap > sum_Inr_AM"));
                            result = cap - sum_Inr_AM;
                        }
                        else
                        {
                            traceService.Trace(string.Format("@ cap <= sum_Inr_AM"));
                            result = 0m;
                        }

                    }
                    else if (sum_temp == cap)
                    {
                        traceService.Trace(string.Format("Case: sum_temp == cap"));
                        if (sum_Inr_AM < cap)
                            result = cap - sum_Inr_AM;
                        else
                            result = 0m;
                    }
                    else if (sum_temp < cap)
                    {
                        traceService.Trace(string.Format("Case: sum_temp < cap"));
                        result = interestcharge_amount;
                    }
                    else
                    {
                        traceService.Trace(string.Format("Case: Chưa xác định, lấy nguyên lãi của đợt này"));
                        result = interestcharge_amount;
                    }
                    #endregion

                }
                #endregion

                traceService.Trace(string.Format("result : {0}", format_Money(result)));
                //throw new InvalidPluginExecutionException(strMess.ToString());
                return result;
                //Test trace ##############################

            }
            catch (Exception ex)
            {
                traceService.Trace(ex.ToString());
                throw new InvalidPluginExecutionException(ex.ToString());
            }
        }
        public decimal sumWaiverInterest(Entity enOptionEntry)
        {
            // Define Condition Values
            var QEbsd_paymentschemedetail_bsd_optionentry = enOptionEntry.Id;
            var QEbsd_paymentschemedetail_statecode = 0;

            // Instantiate QueryExpression QEbsd_paymentschemedetail
            var QEbsd_paymentschemedetail = new QueryExpression("bsd_paymentschemedetail");

            // Add columns to QEbsd_paymentschemedetail.ColumnSet
            QEbsd_paymentschemedetail.ColumnSet.AddColumns("bsd_name");
            QEbsd_paymentschemedetail.ColumnSet.AddColumns("bsd_waiverinterest");

            // Define filter QEbsd_paymentschemedetail.Criteria
            QEbsd_paymentschemedetail.Criteria.AddCondition("bsd_optionentry", ConditionOperator.Equal, QEbsd_paymentschemedetail_bsd_optionentry);
            QEbsd_paymentschemedetail.Criteria.AddCondition("statecode", ConditionOperator.Equal, QEbsd_paymentschemedetail_statecode);
            EntityCollection encolInstallment = service.RetrieveMultiple(QEbsd_paymentschemedetail);
            traceService.Trace("Count encolInstallment: " + encolInstallment.Entities.Count.ToString());
            //decimal sum = encolInstallment.Entities.Sum(x => ((Money)x.Attributes["bsd_waiverinterest"]).Value);
            decimal sum = 0;
            foreach (Entity en in encolInstallment.Entities)
            {
                decimal bsd_waiverinterest = en.Contains("bsd_waiverinterest") ? ((Money)en["bsd_waiverinterest"]).Value : 0;
                traceService.Trace("bsd_waiverinterest: " + bsd_waiverinterest.ToString());
                sum += bsd_waiverinterest;
            }
            traceService.Trace("Sum bsd_waiverinterest: " + sum.ToString());
            return sum;
        }
        public decimal getInterestSimulation(Entity enIns, DateTime dateCalculate, decimal amountpay)
        {
            traceService.Trace("aaaaaaaaaaaaaaaaaaaaaamountPay:" + amountpay);
            enInstallment = service.Retrieve(enIns.LogicalName, enIns.Id, new ColumnSet(true));
            traceService.Trace("2");
            //getInterestStartDate();
            //objIns.LateDays = getLateDays(dateCalculate);
            //Lãi ước tính = số ngày tre * lai suat * so tien trể = balance
            decimal interestcharge_percent = objIns.InterestPercent / 100 * objIns.LateDays;
            decimal interestSimulation = amountpay * interestcharge_percent;
            return interestSimulation;
        }
        /// <summary>
        /// Return tổng lãi phát sinh, không tính đợt hiện tại
        /// </summary>
        /// <param name="OEID"></param>
        /// <param name="dateCalculate"></param>
        /// <param name="amountpay"></param>
        /// <returns></returns>
        public decimal SumInterestAM_OE_New(Guid OEID, DateTime dateCalculate, decimal amountpay)
        {
            decimal result = 0m;
            try
            {
                if (enInstallment == null || enInstallment.Contains("bsd_ordernumber"))
                {
                    traceService.Trace("Reocord [Installment] is null or Field [bsd_ordernumber] not data, Please check code Func [SumInterestAM_OE_New]");
                }
                int bsd_ordernumber = (int)enInstallment["bsd_ordernumber"];

                #region --- Fetch XML: bsd_paymentschemedetail ---
                string fetchXml = @"<fetch version='1.0' output-format='xml-platform' mapping='logical' distinct='false'>
                                      <entity name='bsd_paymentschemedetail' >
                                        <all-attributes/>
                                        <filter type='and' >
                                          <condition attribute='bsd_optionentry' operator='eq' value='{0}' />
                                            <condition attribute='bsd_interestchargeamount' operator='not-null' />
                                            <condition attribute='bsd_ordernumber' operator='le' value='{1}' />
                                            <condition attribute='statecode' operator='eq' value='0' />
                                        </filter>
                                        <order attribute='bsd_ordernumber' descending='false' />
                                      </entity>
                                    </fetch>";
                fetchXml = string.Format(fetchXml, OEID, bsd_ordernumber.ToString());
                #endregion

                EntityCollection entc = service.RetrieveMultiple(new FetchExpression(fetchXml));
                if (entc.Entities.Any())
                {
                    decimal sumAmount = 0;
                    decimal sumSimulation = 0;
                    int count = 0;
                    foreach (var item in entc.Entities)
                    {
                        count += 1;
                        traceService.Trace(string.Format("- Dòng thứ i = {0}", count));
                        if (item.Contains("bsd_interestchargeamount"))
                        {
                            sumAmount += ((Money)item["bsd_interestchargeamount"]).Value;
                            traceService.Trace(string.Format("+ lãi phát sinh: {0}", ((Money)item["bsd_interestchargeamount"]).Value));

                            if (count != entc.Entities.Count)
                            {
                                if (item.Contains("bsd_balance") && ((Money)item["bsd_balance"]).Value > 0)
                                {
                                    decimal bsd_balance = ((Money)item["bsd_balance"]).Value;
                                    sumSimulation += getInterestSimulation(item, dateCalculate, bsd_balance);
                                    traceService.Trace(string.Format("+ lãi ước tính đợt trước: {0}", getInterestSimulation(item, dateCalculate, bsd_balance)));
                                }
                            }
                        }
                    }
                    result = sumSimulation + sumAmount;

                    #region --- Trace debug ---
                    traceService.Trace(string.Format("Tổng lãi phát sinh: {0}", sumAmount));
                    traceService.Trace(string.Format("Tổng lãi ước tính đợt trước: {0}", sumSimulation));
                    traceService.Trace(string.Format("result: {0}", result));
                    #endregion
                }
                else
                {
                    traceService.Trace("Fetch bsd_paymentschemedetail is not data!");
                }
            }
            catch (Exception ex)
            {
                result = 0;
                throw new Exception(string.Format(ex.ToString()));
            }
            return result;
        }
        private bool check_Data_Setup()
        {
            try
            {
                EntityReference OE_ref = enInstallment.Contains("bsd_optionentry") ? (EntityReference)enInstallment["bsd_optionentry"] : null;
                Entity OE = enInstallment.Contains("bsd_optionentry") ? service.Retrieve(OE_ref.LogicalName, OE_ref.Id, new ColumnSet(true)) : null;
                if (OE == null) return false;
                EntityReference paymentscheme_ref = OE.Contains("bsd_paymentscheme") ? (EntityReference)OE["bsd_paymentscheme"] : null;
                Entity PaymentScheme = service.Retrieve(paymentscheme_ref.LogicalName, paymentscheme_ref.Id, new ColumnSet(true));
                EntityReference interestratemaster_ref = PaymentScheme.Contains("bsd_interestratemaster") ? (EntityReference)PaymentScheme["bsd_interestratemaster"] : null;
                if (interestratemaster_ref != null)
                {
                    Entity InterestrateMaster = service.Retrieve(interestratemaster_ref.LogicalName, interestratemaster_ref.Id, new ColumnSet(true));
                    if (!InterestrateMaster.Contains("bsd_termsinterestpercentage") && !InterestrateMaster.Contains("bsd_toleranceinterestamount"))
                    {
                        traceService.Trace(string.Format("Chưa setup {tỷ lệ / số tiền} tính lãi cho CAP"));
                        return true;
                    }
                }
                return false;
            }
            catch (Exception)
            {
                traceService.Trace(string.Format("sys exception ..."));
                return false;
            }

        }
        private string format_Money(decimal money)
        {
            string result = string.Format("{0:#,##0.00}", money);
            return result;
        }
        public EntityCollection get_ec_bsd_dailyinterestrate(Guid projID)
        {
            string fetchXml =
              @"<fetch version='1.0' output-format='xml-platform' mapping='logical' distinct='false'>
                  <entity name='bsd_dailyinterestrate' >
                    <attribute name='bsd_date' />
                    <attribute name='bsd_project' />
                    <attribute name='bsd_interestrate' />
                    <attribute name='createdon' />
                    <filter type='and' >
                      <condition attribute='bsd_project' operator='eq' value='{0}' />
                        <condition attribute='statuscode' operator='eq' value='1' />
                    </filter>
                    <order attribute='createdon' descending='true' />
                  </entity>
                </fetch>";

            fetchXml = string.Format(fetchXml, projID);
            EntityCollection entc = service.RetrieveMultiple(new FetchExpression(fetchXml));
            return entc;
        }
        public DateTime RetrieveLocalTimeFromUTCTime(DateTime utcTime)
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
        public DateTime InterestStarDate { get; set; }
        public int Intereststartdatetype { get; set; }
        public int Gracedays { get; set; }
        public int LateDays { get; set; }
        public int orderNumber { get; set; }
        public Guid idOE { get; set; }
        public decimal MaxPercent { get; set; }
        public decimal MaxAmount { get; set; }
        public decimal InterestPercent { get; set; }
        public decimal InterestCharge { get; set; }
        public DateTime Duedate { get; set; }
    }
}
