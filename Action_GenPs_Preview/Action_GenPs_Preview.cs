using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Query;
using Microsoft.Crm.Sdk.Messages;

namespace Action_GenPs_Preview
{
    public class Action_GenPs_Preview : IPlugin
    {
        IOrganizationService service = null;
        IOrganizationServiceFactory factory = null;
        ITracingService traceService = null;
        IPluginExecutionContext context = null;
        int totalNumber = 0;
        void IPlugin.Execute(IServiceProvider serviceProvider)
        {
            context = (IPluginExecutionContext)serviceProvider.GetService(typeof(IPluginExecutionContext));
            factory = (IOrganizationServiceFactory)serviceProvider.GetService(typeof(IOrganizationServiceFactory));
            service = factory.CreateOrganizationService(context.UserId);
            traceService = (ITracingService)serviceProvider.GetService(typeof(ITracingService));
            EntityReference target = (EntityReference)context.InputParameters["Target"];
            if (target.LogicalName == "bsd_paymentscheme")
            {
                // En lịch thanh toán master
                Entity enPaymentScheme = service.Retrieve(target.LogicalName, target.Id, new ColumnSet(true));
                // kiểm tra xem có đợt detail master chưa
                invalidDetailMaster(enPaymentScheme);
                // lấy ngày preview tính duedate
                DateTime date = RetrieveLocalTimeFromUTCTime(DateTime.Now, service);
                // xóa lịch thanh toán preview trước đó có statecode là deactive
                deleteDetailPreview(enPaymentScheme);
                // tạo lịch thanh toán detail với ngày preview = ngày hiện tại
                genPaymentSchemePreview(enPaymentScheme, ref date);
            }
        }

        private void genPaymentSchemePreview(Entity enPaymentScheme, ref DateTime date)
        {
            // lấy master về để gen
            EntityCollection enclDetailMaster = getAllDetailMaster(enPaymentScheme);
            int countMaster = enclDetailMaster.Entities.Count;
            int orderNumber = 0;
            bool f_lastinstallment = false;// đợt last installment
            bool f_es = false;// đợt estimas handoverdate
            int i_ESmethod = 0;
            decimal d_ESpercent = 0;
            bool f_signcontractinstallment = false;// đợt sign contract
            bool f_last_ES = true;
            int i_dueCalMethod = -1;
            for (int i = 0; i < countMaster; i++) // len = so luong INS detail
            {
                if (!enclDetailMaster.Entities[i].Contains("bsd_amountpercent"))
                    throw new InvalidPluginExecutionException("Please select field 'Percent (%)' for '" + (string)enclDetailMaster.Entities[i]["bsd_name"] + "' of Payment Scheme " + (string)enPaymentScheme["bsd_paymentschemecode"] + " on Master data!");
                decimal percent = (decimal)enclDetailMaster.Entities[i]["bsd_amountpercent"];

                if (enclDetailMaster.Entities[i].Contains("bsd_duedatecalculatingmethod"))
                {
                    i_dueCalMethod = ((OptionSetValue)enclDetailMaster.Entities[i]["bsd_duedatecalculatingmethod"]).Value;
                    if (i_dueCalMethod == 100000002)
                        f_es = true;
                    else f_es = false;
                }
                else f_es = false;
                if (enclDetailMaster.Entities[i].Contains("bsd_lastinstallment") && (bool)enclDetailMaster.Entities[i]["bsd_lastinstallment"] == true)
                    f_lastinstallment = true;
                else f_lastinstallment = false;
                if ((f_lastinstallment) || (f_es))
                {
                    CreatePaymentPhase_fixDate(enPaymentScheme, ref orderNumber, ref f_last_ES, enclDetailMaster.Entities[i], percent, ref date, f_lastinstallment, f_es, i_ESmethod, d_ESpercent, countMaster);
                }
                else
                {
                    if (!enclDetailMaster.Entities[i].Contains("bsd_duedatecalculatingmethod"))
                        throw new InvalidPluginExecutionException("Please choose Duedate Calculating Method for '" + (string)enclDetailMaster.Entities[i]["bsd_name"] + "' of Payment Scheme " + (string)enPaymentScheme["bsd_paymentschemecode"] + " on Master data!");
                    i_dueCalMethod = ((OptionSetValue)enclDetailMaster.Entities[i]["bsd_duedatecalculatingmethod"]).Value;
                    if (i_dueCalMethod == 100000001) // auto
                    {
                        f_lastinstallment = false;
                        f_es = false;
                        if (!enclDetailMaster.Entities[i].Contains("bsd_nextperiodtype")) throw new InvalidPluginExecutionException("Please choose 'Next period type' for " + (string)enclDetailMaster.Entities[i]["bsd_name"] + " of Payment scheme " + (string)enPaymentScheme["bsd_paymentschemecode"] + " on Master data!");
                        int i_bsd_nextperiodtype = (int)((OptionSetValue)enclDetailMaster.Entities[i]["bsd_nextperiodtype"]).Value;
                        int i_paymentdatemonthly = 0;
                        if (enclDetailMaster.Entities[i].Contains("bsd_datepaymentofmonthly"))
                        {
                            i_paymentdatemonthly = (int)enclDetailMaster.Entities[i]["bsd_datepaymentofmonthly"];//bsd_datepaymentofmonthly
                        }
                        int? payment_type = enclDetailMaster.Entities[i].Contains("bsd_typepayment") ? (int?)((OptionSetValue)enclDetailMaster.Entities[i]["bsd_typepayment"]).Value : null;

                        int i_paymentdatemonthly_def = i_paymentdatemonthly;

                        if (i_paymentdatemonthly != 0)
                        {
                            int NgayCuoiThang = DateTime.DaysInMonth(date.Year, date.Month);
                            if (i_paymentdatemonthly > NgayCuoiThang)
                                i_paymentdatemonthly = NgayCuoiThang;
                            date = new DateTime(date.Year, date.Month, i_paymentdatemonthly);
                        }

                        if (payment_type == null || payment_type == 1)//default or month
                        {
                            CreatePaymentPhase(enPaymentScheme, ref orderNumber, enclDetailMaster.Entities[i], percent, ref date, i_paymentdatemonthly_def, countMaster, f_signcontractinstallment);
                        }
                        else if (payment_type == 2)//times
                        {
                            if (!enclDetailMaster.Entities[i].Contains("bsd_number"))
                                throw new InvalidPluginExecutionException("Please select field 'Number of Times' on '" + (string)enclDetailMaster.Entities[i]["bsd_name"] + "' of Payment Scheme " + (string)enPaymentScheme["bsd_paymentschemecode"] + " on Master data!");
                            int number = (int)enclDetailMaster.Entities[i]["bsd_number"];
                            int i_bsd_nextdaysofendphase = 0;
                            if (enclDetailMaster.Entities[i].Contains("bsd_nextdaysofendphase"))
                            {
                                i_bsd_nextdaysofendphase = (int)enclDetailMaster.Entities[i]["bsd_nextdaysofendphase"];
                            }
                            for (int j = 0; j < number; j++)
                            {
                                if (j == number - 1)
                                    date = date.AddDays(i_bsd_nextdaysofendphase);

                                CreatePaymentPhase(enPaymentScheme, ref orderNumber, enclDetailMaster.Entities[i], percent, ref date, i_paymentdatemonthly_def, countMaster, f_signcontractinstallment);
                            }
                        }
                    }
                    else if (i_dueCalMethod == 100000000) // fixx
                    {
                        //CreatePaymentPhase_fixDate(0, total_TMP, bsd_freightamount, ref f_last_ES, PM, ref orderNumber, ents.Entities[i], QO, productId, totalAmount, percent, ref date, false, i_localization, f_lastinstallment, f_es, d_estimate, i_ESmethod, d_ESpercent, f_ESmaintenancefees, f_ESmanagementfee, trac);
                        CreatePaymentPhase_fixDate(enPaymentScheme, ref orderNumber, ref f_last_ES, enclDetailMaster.Entities[i], percent, ref date, f_lastinstallment, f_es, i_ESmethod, d_ESpercent, countMaster);
                    }
                }
            }
        }

        private void CreatePaymentPhase_fixDate(Entity enPaymentScheme, ref int orderNumber, ref bool f_last_ES, Entity en, decimal percent, ref DateTime date
            , bool f_lastinstallment, bool f_es, int i_ESmethod, decimal d_ESpercent, int countMaster)
        {
            Entity tmp = new Entity(en.LogicalName);
            if (f_lastinstallment == false)
            {
                if (f_es == false)
                {
                    if (orderNumber < totalNumber - 1)
                    {
                        if (en.Contains("bsd_fixeddate"))
                        {
                            tmp["bsd_duedate"] = en["bsd_fixeddate"];
                            tmp["bsd_fixeddate"] = en["bsd_fixeddate"];

                        }
                    }
                }
            }
            decimal bsd_percentforcustomer = en.Contains("bsd_percentforcustomer") ? (decimal)en["bsd_percentforcustomer"] : 0;
            decimal bsd_percentforbank = en.Contains("bsd_percentforbank") ? (decimal)en["bsd_percentforbank"] : 0;
            string bsd_constructionprocess = en.Contains("bsd_constructionprocess") ? (string)en["bsd_constructionprocess"] : "";
            orderNumber++;
            tmp["bsd_ordernumber"] = orderNumber;
            tmp["bsd_name"] = "Installment " + orderNumber;
            tmp["bsd_constructionprocess"] = bsd_constructionprocess;
            tmp["bsd_pspreview"] = true;
            tmp["statecode"] = new OptionSetValue(1);
            tmp["bsd_paymentscheme"] = en["bsd_paymentscheme"];
            tmp["bsd_amountpercent"] = en["bsd_amountpercent"];
            tmp["bsd_percentforbank"] = bsd_percentforbank;
            tmp["bsd_percentforcustomer"] = bsd_percentforcustomer;
            if (orderNumber == 1)
            {
                if (en.Contains("bsd_fixeddate"))
                {
                    tmp["bsd_duedate"] = en["bsd_fixeddate"];
                    tmp["bsd_fixeddate"] = en["bsd_fixeddate"];
                }
                tmp["bsd_duedatecalculatingmethod"] = new OptionSetValue(100000000);
                tmp.Id = Guid.NewGuid();
                service.Create(tmp);
                date = (DateTime)en["bsd_fixeddate"];
            }
            else
            {
                if (f_lastinstallment == false)
                {
                    tmp["bsd_duedatecalculatingmethod"] = new OptionSetValue(100000000);
                    if (f_es == true)
                    {
                        EntityReference enrfProject = enPaymentScheme.Contains("bsd_project") ? (EntityReference)enPaymentScheme["bsd_project"] : null;
                        DateTime d_es = gesEstimatehandoverDate(enrfProject);
                        tmp["bsd_duedate"] = RetrieveLocalTimeFromUTCTime(d_es, service);
                        tmp["bsd_duedatecalculatingmethod"] = new OptionSetValue(100000002);
                    }
                    else//f_es == false
                    {
                        if (en.Contains("bsd_fixeddate"))
                        {
                            tmp["bsd_duedate"] = en["bsd_fixeddate"];
                            tmp["bsd_fixeddate"] = en["bsd_fixeddate"];
                            date = (DateTime)en["bsd_fixeddate"];
                        }
                    }
                    Guid guid = service.Create(tmp);
                }
                else//f_lastinstallment== true
                {
                    Guid guid = service.Create(tmp);
                }
            }

        }

        private void CreatePaymentPhase(Entity enPaymentScheme, ref int orderNumber, Entity en, decimal percent, ref DateTime date, int i_paymentdatemonthly, int InstallmentCount, bool f_signcontractinstallment)
        {
            double extraDay = 0;
            int i_nextMonth = 1;
            if (!en.Contains("bsd_nextperiodtype"))
                throw new InvalidPluginExecutionException("Please select field 'Next period type' on '" + en["bsd_name"].ToString() + "' of Payment Scheme " + (string)enPaymentScheme["bsd_paymentschemecode"] + " on Master data!");
            int type = ((OptionSetValue)en["bsd_nextperiodtype"]).Value;
            bool b_typeofstartdate = ((bool)en["bsd_typeofstartdate"]);
            if (b_typeofstartdate) // Nếu bsd_typeofstartdate = Yes => set lại ngày = bsd_reservationtime
            {
                date = RetrieveLocalTimeFromUTCTime(DateTime.Now,service);
            }
            if (type == 1)//month
            {
                if (!en.Attributes.Contains("bsd_numberofnextmonth"))
                    throw new InvalidPluginExecutionException("Please select field 'Number next month' on '" + en["bsd_name"].ToString() + "' of Payment Scheme " + (string)enPaymentScheme["bsd_paymentschemecode"] + " on Master data!");
                i_nextMonth = ((int)en["bsd_numberofnextmonth"]);
                date = date.AddMonths(i_nextMonth);

            }
            else if (type == 2)//day
            {
                if (!en.Attributes.Contains("bsd_numberofnextdays"))
                    throw new InvalidPluginExecutionException("Please select field 'Number of next days' on '" + en["bsd_name"].ToString() + "' of Payment Scheme " + (string)enPaymentScheme["bsd_paymentschemecode"] + " on Master data!");
                extraDay = double.Parse(en["bsd_numberofnextdays"].ToString());
                date = date.AddDays(extraDay);
            }
            if (i_paymentdatemonthly != 0)
            {
                int NgayCuoiThang = DateTime.DaysInMonth(date.Year, date.Month);
                if (i_paymentdatemonthly > NgayCuoiThang)
                    i_paymentdatemonthly = NgayCuoiThang;
                date = new DateTime(date.Year, date.Month, i_paymentdatemonthly);
            }
            decimal bsd_percentforcustomer = en.Contains("bsd_percentforcustomer") ? (decimal)en["bsd_percentforcustomer"] : 0;
            decimal bsd_percentforbank = en.Contains("bsd_percentforbank") ? (decimal)en["bsd_percentforbank"] : 0;
            string bsd_constructionprocess = en.Contains("bsd_constructionprocess") ? (string)en["bsd_constructionprocess"] : "";
            orderNumber++;
            Entity tmp = new Entity(en.LogicalName);
            tmp["bsd_ordernumber"] = orderNumber;
            tmp["bsd_constructionprocess"] = bsd_constructionprocess;
            tmp["bsd_name"] = "Installment " + orderNumber;
            tmp["bsd_pspreview"] = true;
            tmp["statecode"] = new OptionSetValue(1);
            tmp["bsd_paymentscheme"] = en["bsd_paymentscheme"];
            tmp["bsd_amountpercent"] = en["bsd_amountpercent"];
            tmp["bsd_percentforbank"] = bsd_percentforbank;
            tmp["bsd_percentforcustomer"] = bsd_percentforcustomer;
            #region  extra field
            if (en.Contains("bsd_nextperiodtype"))
            {
                int bsd_nextperiodtype = ((OptionSetValue)en["bsd_nextperiodtype"]).Value;
                tmp["bsd_nextperiodtype"] = new OptionSetValue(bsd_nextperiodtype);
            }

            if (en.Contains("bsd_numberofnextdays"))
            {
                double bsd_numberofnextdays = double.Parse(en["bsd_numberofnextdays"].ToString());
                tmp["bsd_numberofnextdays"] = bsd_numberofnextdays;
            }

            if (en.Contains("bsd_numberofnextmonth"))
            {
                int bsd_numberofnextmonth = (int)en["bsd_numberofnextmonth"];
                tmp["bsd_numberofnextmonth"] = bsd_numberofnextmonth;
            }

            if (en.Contains("bsd_typepayment"))
            {
                int i_bsd_typepayment = ((OptionSetValue)en["bsd_typepayment"]).Value;
                tmp["bsd_typepayment"] = new OptionSetValue(i_bsd_typepayment);
            }
            if (en.Contains("bsd_datepaymentofmonthly"))
            {
                int bsd_datepaymentofmonthly = (int)en["bsd_datepaymentofmonthly"];
                tmp["bsd_datepaymentofmonthly"] = bsd_datepaymentofmonthly;
            }

            if (en.Contains("bsd_number"))
            {
                int bsd_number = (int)en["bsd_number"];
                tmp["bsd_number"] = bsd_number;
            }
            if (en.Contains("bsd_nextdaysofendphase"))
            {
                int bsd_nextdaysofendphase = (int)en["bsd_nextdaysofendphase"];
                tmp["bsd_nextdaysofendphase"] = bsd_nextdaysofendphase;
            }
            if (en.Contains("bsd_typeofstartdate"))
            {
                tmp["bsd_typeofstartdate"] = (bool)en["bsd_typeofstartdate"]; //bsd_typeofstartdate
            }
            //if (en.Contains("bsd_calculatedfromdepositdate"))
            //    tmp["bsd_calculatedfromdepositdate"] = (bool)en["bsd_calculatedfromdepositdate"];
            //09012018 - Add new
            if (en.Contains("bsd_description"))
            {
                tmp["bsd_description"] = en["bsd_description"]; //bsd_description
            }
            if (en.Contains("bsd_temporaryhandover"))
            {
                tmp["bsd_temporaryhandover"] = (bool)en["bsd_temporaryhandover"]; //bsd_temporaryhandover
            }
            //
            #endregion
            if (date != null)
                tmp["bsd_duedate"] = date;
            tmp.Id = Guid.NewGuid();
            service.Create(tmp);

        }

        private void invalidDetailMaster(Entity enPaymentScheme)
        {
            EntityCollection enclDetailMaster = getAllDetailMaster(enPaymentScheme);
            if (enclDetailMaster.Entities.Count < 0)
            {
                throw new InvalidPluginExecutionException("Vui lòng tạo lịch thanh toán chi tiết chung");
            }
        }

        private EntityCollection getAllDetailMaster(Entity enPaymentScheme)
        {
            var QEbsd_paymentschemedetail_statecode = 0;
            var QEbsd_paymentschemedetail_bsd_pspreview = false;
            var QEbsd_paymentschemedetail_bsd_paymentscheme = enPaymentScheme.Id;
            var QEbsd_paymentschemedetail = new QueryExpression("bsd_paymentschemedetail");
            QEbsd_paymentschemedetail.ColumnSet.AllColumns = true;
            QEbsd_paymentschemedetail.Criteria.AddCondition("statecode", ConditionOperator.Equal, QEbsd_paymentschemedetail_statecode);
            QEbsd_paymentschemedetail.Criteria.AddCondition("bsd_quotation", ConditionOperator.Null);
            QEbsd_paymentschemedetail.Criteria.AddCondition("bsd_reservation", ConditionOperator.Null);
            QEbsd_paymentschemedetail.Criteria.AddCondition("bsd_optionentry", ConditionOperator.Null);
            QEbsd_paymentschemedetail.Criteria.AddCondition("bsd_conversioncontractapproval", ConditionOperator.Null);
            QEbsd_paymentschemedetail.Criteria.AddCondition("bsd_appendixcontract", ConditionOperator.Null);
            QEbsd_paymentschemedetail.Criteria.AddCondition("bsd_changeinformation", ConditionOperator.Null);
            QEbsd_paymentschemedetail.Criteria.AddCondition("bsd_paymentscheme", ConditionOperator.Equal, QEbsd_paymentschemedetail_bsd_paymentscheme);
            QEbsd_paymentschemedetail.Criteria.AddCondition("bsd_pspreview", ConditionOperator.Equal, QEbsd_paymentschemedetail_bsd_pspreview);
            EntityCollection encl = service.RetrieveMultiple(QEbsd_paymentschemedetail);
            return encl;
        }

        private void deleteDetailPreview(Entity enPaymentScheme)
        {
            var QEbsd_paymentschemedetail_bsd_paymentscheme = enPaymentScheme.Id;
            var QEbsd_paymentschemedetail_bsd_pspreview = true;
            var QEbsd_paymentschemedetail = new QueryExpression("bsd_paymentschemedetail");
            QEbsd_paymentschemedetail.ColumnSet.AllColumns = true;
            QEbsd_paymentschemedetail.Criteria.AddCondition("bsd_reservation", ConditionOperator.Null);
            QEbsd_paymentschemedetail.Criteria.AddCondition("bsd_optionentry", ConditionOperator.Null);
            QEbsd_paymentschemedetail.Criteria.AddCondition("bsd_conversioncontractapproval", ConditionOperator.Null);
            QEbsd_paymentschemedetail.Criteria.AddCondition("bsd_appendixcontract", ConditionOperator.Null);
            QEbsd_paymentschemedetail.Criteria.AddCondition("bsd_changeinformation", ConditionOperator.Null);
            QEbsd_paymentschemedetail.Criteria.AddCondition("bsd_paymentscheme", ConditionOperator.Equal, QEbsd_paymentschemedetail_bsd_paymentscheme);
            QEbsd_paymentschemedetail.Criteria.AddCondition("bsd_pspreview", ConditionOperator.Equal, QEbsd_paymentschemedetail_bsd_pspreview);
            EntityCollection enclDetailPreview = service.RetrieveMultiple(QEbsd_paymentschemedetail);
            if (enclDetailPreview.Entities.Count > 0)
            {
                foreach (Entity en in enclDetailPreview.Entities)
                {
                    service.Delete(en.LogicalName, en.Id);
                }
            }
        }

        private DateTime gesEstimatehandoverDate(EntityReference enrfProject)
        {
            DateTime dateCEs = DateTime.Now;
            var QEbsd_project_bsd_projectid = enrfProject.Id;
            var QEbsd_project = new QueryExpression("bsd_project");
            QEbsd_project.ColumnSet.AddColumns("bsd_estimatehandoverdate");
            QEbsd_project.Criteria.AddCondition("bsd_projectid", ConditionOperator.Equal, QEbsd_project_bsd_projectid);
            EntityCollection encl = service.RetrieveMultiple(QEbsd_project);
            if (encl.Entities.Count == 0)
            {
                throw new InvalidPluginExecutionException("Vui lòng chọn ngày bàn giao trong dự án!");
            }
            else
            {
                Entity en = encl.Entities[0];
                if (en.Contains("bsd_estimatehandoverdate"))
                {
                    dateCEs = (DateTime)en["bsd_estimatehandoverdate"];
                }              
                
            }
            return dateCEs;
        }

        private DateTime RetrieveLocalTimeFromUTCTime(DateTime utcTime, IOrganizationService service)
        {

            int? timeZoneCode = RetrieveCurrentUsersSettings(service);


            if (!timeZoneCode.HasValue)
                throw new Exception("Can't find time zone code");



            var request = new LocalTimeFromUtcTimeRequest
            {
                TimeZoneCode = timeZoneCode.Value,
                UtcTime = utcTime.ToUniversalTime()
            };

            var response = (LocalTimeFromUtcTimeResponse)service.Execute(request);

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
                    Conditions =
            {
            new ConditionExpression("systemuserid", ConditionOperator.EqualUserId)
            }
                }
            }).Entities[0].ToEntity<Entity>();

            return (int?)currentUserSettings.Attributes["timezonecode"];
        }


    }
}