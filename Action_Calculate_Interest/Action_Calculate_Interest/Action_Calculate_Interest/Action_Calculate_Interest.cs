using Microsoft.Crm.Sdk.Messages;
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
        public static IOrganizationService service = null;
        IOrganizationServiceFactory factory = null;
        ITracingService TracingSe = null;
        public static Installment objIns = new Installment();
        public static StringBuilder strMess = new StringBuilder();
        private static Entity enInstallment;

        public void Execute(IServiceProvider serviceProvider)
        {

            IPluginExecutionContext context = (IPluginExecutionContext)serviceProvider.GetService(typeof(IPluginExecutionContext));
            factory = (IOrganizationServiceFactory)serviceProvider.GetService(typeof(IOrganizationServiceFactory));
            service = factory.CreateOrganizationService(context.UserId);
            TracingSe = (ITracingService)serviceProvider.GetService(typeof(ITracingService));
            try
            {
                string installmentid = context.InputParameters["installmentid"].ToString();
                string stramountpay = context.InputParameters["amountpay"].ToString();

                string receiptdateimport = (string)context.InputParameters["receiptdate"];
                var serializedResult = "";
                Main(installmentid, stramountpay, receiptdateimport, ref serializedResult);
                context.OutputParameters["result"] = serializedResult.ToString();
                TracingSe.Trace(strMess.ToString());
            }
            catch (InvalidPluginExecutionException ex)
            {
                strMess.AppendLine(ex.ToString());
                context.OutputParameters["result"] = strMess.ToString();
                throw new InvalidPluginExecutionException(ex.ToString());

            }

        }
        public static void Main(string installmentid, string stramountpay, string receiptdateimport, ref string serializedResult)
        {
            decimal amountpay = Convert.ToDecimal(stramountpay);
            DateTime receiptdate = Convert.ToDateTime(receiptdateimport);

            receiptdate = RetrieveLocalTimeFromUTCTime(receiptdate);
            enInstallment = service.Retrieve("bsd_paymentschemedetail", new Guid(installmentid), new ColumnSet(true));
            strMess.AppendLine("2");
            getInterestStartDate();
            objIns.LateDays = getLateDays(receiptdate);
            objIns.InterestCharge = calc_InterestCharge(receiptdate, amountpay);
            var serializer = new JavaScriptSerializer();
            serializedResult = serializer.Serialize(objIns);
        }
        private static void getInterestStartDate()
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

                    objIns.Intereststartdatetype = ((OptionSetValue)enInterestrateMaster["bsd_intereststartdatetype"]).Value;//100000000: Due date;100000001: Grace period
                                                                                                                             //throw new InvalidPluginExecutionException(((DateTime)enInstallment["bsd_duedate"]).ToString());
                    objIns.Duedate = enInstallment.Contains("bsd_duedate") ? RetrieveLocalTimeFromUTCTime((DateTime)enInstallment["bsd_duedate"]) : DateTime.Now;

                    objIns.MaxPercent = enInterestrateMaster.Contains("bsd_toleranceinterestpercentage") ? (decimal)enInterestrateMaster["bsd_toleranceinterestpercentage"] : 100;
                    objIns.MaxAmount = enInterestrateMaster.Contains("bsd_toleranceinterestamount") ? ((Money)enInterestrateMaster["bsd_toleranceinterestamount"]).Value : 0;
                    objIns.InterestPercent = enInterestrateMaster.Contains("bsd_termsinterestpercentage") ? (decimal)enInterestrateMaster["bsd_termsinterestpercentage"] : 0;
                    objIns.InterestStarDate = objIns.Duedate.AddDays(objIns.Gracedays + 1);
                    strMess.AppendLine("InterestStarDate: " + objIns.InterestStarDate.ToString());
                }
            }
            catch (InvalidPluginExecutionException ex)
            {
                throw new InvalidPluginExecutionException(ex.ToString());
            }


        }
        public static int getLateDays(DateTime dateCalculate)
        {
            try
            {
                //int lateDays = (int)dateCalculate.Date.Subtract(InterestStarDate.Date).TotalDays;
                //if (Intereststartdatetype == 100000000)// Interest Start Date type: 100000000--> DueDate
                //{
                //    lateDays = lateDays + Gracedays;
                //}
                int lateDays = (int)dateCalculate.Date.Subtract(objIns.Duedate.Date).TotalDays;
                if (objIns.Intereststartdatetype == 100000001)// Grace Period
                {
                    lateDays = lateDays - objIns.Gracedays;
                }
                else//DueDate
                {
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
                strMess.AppendLine(ex.ToString());
                throw new InvalidPluginExecutionException(ex.ToString());
            }

        }
        /// <summary>
        /// Điều chỉnh logic code 18.09.2019
        /// </summary>
        /// <param name="dateCalculate"></param>
        /// <param name="amountPay"></param>
        /// <returns></returns>
        public static decimal calc_InterestCharge(DateTime dateCalculate, decimal amountPay)
        {
            try
            {
                var result = 0m;
                strMess.AppendLine("[calc_InterestCharge2] Tham số đầu vào:");
                strMess.AppendLine(string.Format("- amountPay: {0}", format_Money(amountPay)));
                strMess.AppendLine("11111111");
                EntityReference OE_ref = enInstallment.Contains("bsd_optionentry") ? (EntityReference)enInstallment["bsd_optionentry"] : null;
                if (OE_ref == null)
                {
                    strMess.AppendLine(string.Format("Không có dữ liệu record [bsd_optionentry]"));
                    throw new Exception(strMess.ToString());
                }
                #region --- Body Code ---
                if (OE_ref != null)
                {
                    strMess.AppendLine(string.Format("ID record [bsd_optionentry]: {0}", OE_ref.Id));
                    decimal interestcharge_amount = 0;
                    Entity OE = service.Retrieve(OE_ref.LogicalName, OE_ref.Id, new ColumnSet(true));
                    if (OE.Contains("bsd_signeddadate") || OE.Contains("bsd_signedcontractdate"))
                    {
                        Entity Project = service.Retrieve("bsd_project", ((EntityReference)OE["bsd_project"]).Id, new ColumnSet(new string[] { "bsd_name", "bsd_dailyinterestchargebank" }));
                        bool bsd_dailyinterestchargebank = Project.Contains("bsd_dailyinterestchargebank") ? (bool)Project["bsd_dailyinterestchargebank"] : false;
                        decimal d_dailyinterest = 0;

                        strMess.AppendLine(string.Format("- bsd_dailyinterestchargebank: {0}", bsd_dailyinterestchargebank));
                        if (bsd_dailyinterestchargebank)
                        {
                            #region --- Lấy thông tin setup bsd_dailyinterestrate, dòng đầu tiên ---
                            EntityCollection entc = get_ec_bsd_dailyinterestrate(Project.Id);
                            Entity ent = entc.Entities[0];
                            ent.Id = entc.Entities[0].Id;
                            if (!ent.Contains("bsd_interestrate"))
                                throw new InvalidPluginExecutionException("Can not find Daily Interestrate in Project " + (string)Project["bsd_name"] + " in master data. Please check again!");
                            d_dailyinterest = (decimal)ent["bsd_interestrate"];
                            #endregion
                        }
                        strMess.AppendLine("InterestPercent" + objIns.InterestPercent);
                        strMess.AppendLine("d_dailyinterest" + d_dailyinterest);

                        objIns.InterestPercent = (objIns.InterestPercent + d_dailyinterest);
                        decimal interestcharge_percent = ((objIns.InterestPercent / 30) / 100) * objIns.LateDays;
                        interestcharge_amount = Convert.ToDecimal(amountPay) * interestcharge_percent;
                        strMess.AppendLine("LateDays" + objIns.LateDays);
                        strMess.AppendLine("amountPay" + amountPay);
                        #region --- Trace đợt đang xét ---
                        strMess.AppendLine(string.Format("----------------------------------------------------"));
                        strMess.AppendLine(string.Format("#. [Đợt đang xét] "));
                        strMess.AppendLine(string.Format("- InterestPercent: {0}", format_Money(objIns.InterestPercent)));
                        strMess.AppendLine(string.Format("- interestcharge_percent: {0}", format_Money(interestcharge_percent)));
                        strMess.AppendLine(string.Format("- interestcharge_amount 'tiền lãi': {0}", format_Money(interestcharge_amount)));
                        strMess.AppendLine(string.Format("----------------------------------------------------"));
                        #endregion

                        decimal sum_bsd_waiverinterest = sumWaiverInterest(OE);
                        decimal sum_Inr_AM = SumInterestAM_OE_New(OE.Id, dateCalculate, amountPay) - sum_bsd_waiverinterest;
                        decimal sum_temp = sum_Inr_AM + interestcharge_amount;

                        #region --- Trace kết quả tổng đợt: tạm tính ---
                        strMess.AppendLine(string.Format("----------------------------------------------------"));
                        strMess.AppendLine(string.Format("##. Kết quả tổng đợt TẠM TÍNH: "));
                        strMess.AppendLine(string.Format("- Số tiền trả trước [waiverinterest]: {0}", format_Money(sum_bsd_waiverinterest)));
                        strMess.AppendLine(string.Format("- Tổng lãi phát sinh, không tính lãi đợt đang xét [sum_Inr_AM]: {0}", format_Money(sum_Inr_AM)));
                        strMess.AppendLine(string.Format("- Tổng lãi phát sinh [sum_temp] (bao gồm đợt đang xét): {0}", format_Money(sum_temp)));
                        strMess.AppendLine(string.Format("----------------------------------------------------"));
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
                        strMess.AppendLine("#. Tính số tiền CAP:");
                        strMess.AppendLine(string.Format("- MaxPercent: {0}", format_Money(objIns.MaxPercent)));
                        strMess.AppendLine(string.Format("- Maxamount: {0}", format_Money(objIns.MaxAmount)));

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

                        strMess.AppendLine(string.Format("- Calc Amout of MaxPercent: {0}", format_Money(range_enOptionEntryAM)));
                        strMess.AppendLine(string.Format("@ CAP: {0}", format_Money(cap)));
                        #endregion

                        #region --- @@ Nghiệp vụ tính tiền lãi ---
                        /* ------------------------------------------------------------------------------------------
                         * Khái niệm: 
                         * - Tổng tiền trễ tất cả các đợt. bao gồm đợt hiện tại             : sum_temp
                         * - Cap : số tiền chạm móc đầu tiên ()                             : cap    
                         * - Tổng tiền trễ không tính đợt hiện tại - sum(waiverinterest)    : sum_Inr_AM
                         * - Tiền trễ đợt hiện tại                                          : interestcharge_amount
                         --------------------------------------------------------------------------------------------*/
                        strMess.AppendLine("222222222");
                        if (cap <= 0)
                        {
                            var rs = check_Data_Setup();
                            strMess.AppendLine(string.Format("Case: cap <= 0"));
                            if (rs)
                            {
                                strMess.AppendLine("Không giới hạn tiền lãi");
                                result = interestcharge_amount;
                            }
                            else
                            {
                                strMess.AppendLine("Cap thiết lặp chạm móc là 0. return 0");
                                result = 0m;
                            }
                        }
                        else if (sum_temp > cap)
                        {
                            strMess.AppendLine(string.Format("Case: sum_temp > cap"));
                            if (cap > sum_Inr_AM)
                            {
                                strMess.AppendLine(string.Format("@ cap > sum_Inr_AM"));
                                result = cap - sum_Inr_AM;
                            }
                            else
                            {
                                strMess.AppendLine(string.Format("@ cap <= sum_Inr_AM"));
                                result = 0m;
                            }

                        }
                        else if (sum_temp == cap)
                        {
                            strMess.AppendLine(string.Format("Case: sum_temp == cap"));
                            if (sum_Inr_AM < cap)
                                result = cap - sum_Inr_AM;
                            else
                                result = 0m;
                        }
                        else if (sum_temp < cap)
                        {
                            strMess.AppendLine(string.Format("Case: sum_temp < cap"));
                            result = interestcharge_amount;
                        }
                        else
                        {
                            strMess.AppendLine(string.Format("Case: Chưa xác định, lấy nguyên lãi của đợt này"));
                            result = interestcharge_amount;
                        }
                        #endregion
                    }
                    else result = 0;
                }
                #endregion

                strMess.AppendLine(string.Format("result : {0}", format_Money(result)));
                //throw new InvalidPluginExecutionException(strMess.ToString());
                return result;
                //Test trace ##############################

            }
            catch (Exception ex)
            {
                strMess.AppendLine(ex.ToString());
                throw new InvalidPluginExecutionException(ex.ToString());
            }
        }
        public static decimal sumWaiverInterest(Entity enOptionEntry)
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
            strMess.AppendLine("Count encolInstallment: " + encolInstallment.Entities.Count.ToString());
            //decimal sum = encolInstallment.Entities.Sum(x => ((Money)x.Attributes["bsd_waiverinterest"]).Value);
            decimal sum = 0;
            foreach (Entity en in encolInstallment.Entities)
            {
                decimal bsd_waiverinterest = en.Contains("bsd_waiverinterest") ? ((Money)en["bsd_waiverinterest"]).Value : 0;
                strMess.AppendLine("bsd_waiverinterest: " + bsd_waiverinterest.ToString());
                sum += bsd_waiverinterest;
            }
            strMess.AppendLine("Sum bsd_waiverinterest: " + sum.ToString());
            return sum;
        }
        public static decimal getInterestSimulation(Entity enIns, DateTime dateCalculate, decimal amountpay)
        {
            strMess.AppendLine("aaaaaaaaaaaaaaaaaaaaaamountPay:" + amountpay);
            enInstallment = service.Retrieve(enIns.LogicalName, enIns.Id, new ColumnSet(true));
            strMess.AppendLine("2");
            //getInterestStartDate();
            //objIns.LateDays = getLateDays(dateCalculate);
            //Lãi ước tính = số ngày tre * lai suat * so tien trể = balance
            decimal interestcharge_percent = ((objIns.InterestPercent / 30) / 100) * objIns.LateDays;
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
        public static decimal SumInterestAM_OE_New(Guid OEID, DateTime dateCalculate, decimal amountpay)
        {
            decimal result = 0m;
            try
            {
                if (enInstallment == null || enInstallment.Contains("bsd_ordernumber"))
                {
                    strMess.AppendLine("Reocord [Installment] is null or Field [bsd_ordernumber] not data, Please check code Func [SumInterestAM_OE_New]");
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
                        strMess.AppendLine(string.Format("- Dòng thứ i = {0}", count));
                        if (item.Contains("bsd_interestchargeamount"))
                        {
                            sumAmount += ((Money)item["bsd_interestchargeamount"]).Value;
                            strMess.AppendLine(string.Format("+ lãi phát sinh: {0}", ((Money)item["bsd_interestchargeamount"]).Value));

                            if (count != entc.Entities.Count)
                            {
                                if (item.Contains("bsd_balance") && ((Money)item["bsd_balance"]).Value > 0)
                                {
                                    decimal bsd_balance = ((Money)item["bsd_balance"]).Value;
                                    sumSimulation += getInterestSimulation(item, dateCalculate, bsd_balance);
                                    strMess.AppendLine(string.Format("+ lãi ước tính đợt trước: {0}", getInterestSimulation(item, dateCalculate, bsd_balance)));
                                }
                            }
                        }
                    }
                    result = sumSimulation + sumAmount;

                    #region --- Trace debug ---
                    strMess.AppendLine(string.Format("Tổng lãi phát sinh: {0}", sumAmount));
                    strMess.AppendLine(string.Format("Tổng lãi ước tính đợt trước: {0}", sumSimulation));
                    strMess.AppendLine(string.Format("result: {0}", result));
                    #endregion
                }
                else
                {
                    strMess.AppendLine("Fetch bsd_paymentschemedetail is not data!");
                }
            }
            catch (Exception ex)
            {
                result = 0;
                throw new Exception(string.Format(ex.ToString()));
            }
            return result;
        }
        private static bool check_Data_Setup()
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
                        strMess.AppendLine(string.Format("Chưa setup {tỷ lệ / số tiền} tính lãi cho CAP"));
                        return true;
                    }
                }
                return false;
            }
            catch (Exception)
            {
                strMess.AppendLine(string.Format("sys exception ..."));
                return false;
            }

        }
        private static string format_Money(decimal money)
        {
            string result = string.Format("{0:#,##0.00}", money);
            return result;
        }
        public static EntityCollection get_ec_bsd_dailyinterestrate(Guid projID)
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
    public class Installment
    {
        public DateTime InterestStarDate { get; set; }
        public int Intereststartdatetype { get; set; }
        public int Gracedays { get; set; }
        public int LateDays { get; set; }
        public decimal MaxPercent { get; set; }
        public decimal MaxAmount { get; set; }
        public decimal InterestPercent { get; set; }
        public decimal InterestCharge { get; set; }
        public DateTime Duedate { get; set; }
    }
}
