//@@ TriCM - 16.07.12
//## config duedate = fix_date or duedate= auto period - for generate pms from reservation

//&& 16 08 12
//## k lay gia tri total amount trong Unit ma lay bsd_totalamountlessfreight trong Resv ;
//## dong thoi lay gia tri landvalue trong Resv: bsd_landvaluededuction
//## cong thuc tinh amount for each phase: bsd_totalamountlessfreight * % percent + 10% cua bsd_totalamountlessfreight
//## dot Handover(ke cuoi) amount this phase phai tru them landvalue * tax - tax lay tu Resv

// $$ 160918 new requirement - bo sung cac field theo PMSDTL master - bỏ require va block field

//@@ 161006
//%% CONG THUC GENERATE PMS DTL CHO RESERVATION
// totalamount generate  = net selling price Total VAT Tax - dot 1 den truoc dot ke cuoi: percent * totalAM
// dot ke cuoi: dung cong thuc tren và tru them percent * landValue
// dot cuoi = totalAM - tong cac dot truoc do

//&& 161011 add field bsd_balance
//&& 161203 add field bsd_numberofmonthspaidmf - neu co chua field nay tren Resv - thi co check field managefees khi generate PMS cho dot ES handover

// 170111
// chuyen code workflow gene pms QUOTE sang action
// chay org dnmic 365 k chay
// 170311
// management fees tinh theo cong thuc: tien management lay tren project , * actual area ( tren unit) neu k co actual area thi lay net usualable (unit) * so thang number of paid management ( tren Quote)

// 170325 khi deposit confirm payment  - se update field deposit time ( deposit date) vao QUOTE
// khi generate pms detail - kiem tra neu 1st master la auto date thi kiem tra field isdeposited co check hay k, neu k check thi khi deposit se generate lai ins detail cho quote
// neu co check thi se generate lai duedate all ins detail cua OE khi sign contract - dua vao ngay contract cho 1st

// 170428
// luc generate thi field deposit fees tren QUOTE - gan vao field deposit fees cho dot 1st
// field balance - include Installment  = amount of this phase cua INS do
// field balance ( include interest ) = 0

// 170505 - generate pms detail cho all - neu la auto date - thi lay reservation time , deposit time or sign contract date làm moc bat dau- auto date + them baonhieu ngay hoac thang
// thi cong vao duedate cho chinh dot do , dot tiep theo bang duedate cua dot truoc do + voi ngay / thang quy dinh trong pms detail do


using System;
using System.Text;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Query;

namespace Action_Resv_GenPMS
{
    public class Action_Resv_GenPMS : IPlugin
    {

        IOrganizationService service = null;
        IOrganizationServiceFactory factory = null;
        ITracingService traceService = null;
        decimal priceperunit = 0;
        decimal tax = 0;
        decimal TaxAmount = 0;//(Price before VAT - Land value)*10%
        decimal landValue = 0;

        int statusCode = -1;
        int totalNumber = 0;
        decimal totalTax = 0;
        decimal bsd_freightamount = 0;
        decimal bsd_managementfee = 0;
        void IPlugin.Execute(IServiceProvider serviceProvider)
        {

            IPluginExecutionContext context = (IPluginExecutionContext)serviceProvider.GetService(typeof(IPluginExecutionContext));
            EntityReference target = (EntityReference)context.InputParameters["Target"];

            if (target.LogicalName == "quote")
            {
                traceService = (ITracingService)serviceProvider.GetService(typeof(ITracingService));
                factory = (IOrganizationServiceFactory)serviceProvider.GetService(typeof(IOrganizationServiceFactory));
                service = factory.CreateOrganizationService(context.UserId);
                traceService.Trace("1");
                Entity QO = service.Retrieve(target.LogicalName, target.Id, new ColumnSet(new string[] {
                        "name",
                        "statuscode",
                        "statecode",
                        "totalamount",
                        "bsd_reservationtime",
                        "bsd_paymentscheme",
                        "bsd_taxcode",
                        "bsd_projectid",
                        "customerid",
                        "bsd_totalamountlessfreight",
                        "bsd_landvaluededuction",
                        "totaltax",
                        "bsd_freightamount",
                        "bsd_managementfee",
                        "bsd_numberofmonthspaidmf",
                        "createdon",
                        "bsd_netusablearea",
                        "bsd_unitno",
                        "bsd_quotecodesams"
                }));


                statusCode = QO.Attributes.Contains("statuscode") ? ((OptionSetValue)QO["statuscode"]).Value : -1;
                if (statusCode == -1)
                    throw new InvalidPluginExecutionException("Status reason could not be null!");
                if (!QO.Contains("bsd_quotecodesams"))
                {
                    if (statusCode != 100000011)
                    {
                        if (!QO.Contains("customerid"))
                            throw new InvalidPluginExecutionException("Please input customer!");
                    }
                    if (!QO.Contains("createdon"))
                        throw new InvalidPluginExecutionException("Can not find Reservation created time!");
                    //if (!QO.Attributes.Contains("bsd_taxcode"))
                    //    throw new InvalidPluginExecutionException("Please select tax for Quotation reservation " +(string)QO["name"]+"!");
                    if (!QO.Attributes.Contains("bsd_paymentscheme"))
                        throw new InvalidPluginExecutionException("Please select payment scheme!");
                    if (!QO.Contains("bsd_totalamountlessfreight")) throw new InvalidPluginExecutionException("Please check 'Net Selling Price' on Reservation!");
                    if (!QO.Contains("totalamount")) throw new InvalidPluginExecutionException("Please check 'Total Amount' on Reservation!");
                    if (!QO.Contains("totaltax")) throw new InvalidPluginExecutionException("Please check 'Total VAT Tax' on Reservation!");
                    if (!QO.Contains("bsd_projectid")) throw new InvalidPluginExecutionException("Cannot find project information on Reservation!");
                } 
                traceService.Trace("2");
                traceService.Trace("2.1");
                DateTime date = (DateTime)QO["createdon"];
                if (QO.Contains("statuscode") && ((OptionSetValue)QO["statuscode"]).Value == 100000000) //reservation
                {
                    if (!QO.Contains("bsd_reservationtime")) throw new InvalidPluginExecutionException("Reservation did not contain Reservation time! Please check again!");
                    else date = (DateTime)QO["bsd_reservationtime"];
                }
                traceService.Trace("2.2");
                if (QO.Contains("bsd_quotecodesams")) date = (DateTime)QO["bsd_reservationtime"];
                //   DeletePaymentPhase(QO.Id);
                QueryExpression q = new QueryExpression("bsd_paymentschemedetail");
                q.ColumnSet = new ColumnSet(new string[] { "bsd_name" });
                q.Criteria = new FilterExpression(LogicalOperator.And);
                q.Criteria.AddCondition(new ConditionExpression("bsd_reservation", ConditionOperator.Equal, QO.Id));
                //q.Criteria.AddCondition(new ConditionExpression("statuscode", ConditionOperator.Equal, 100000000));
                //q.Criteria.AddCondition(new ConditionExpression("statecode", ConditionOperator.Equal, 0));
                traceService.Trace("2.3");
                EntityCollection entc = service.RetrieveMultiple(q);
                traceService.Trace("2.4");
                foreach (Entity en in entc.Entities)
                    service.Delete(en.LogicalName, en.Id);
                traceService.Trace("3");
                GenPaymentScheme(ref QO, ref date, traceService);
                traceService.Trace("4");
                //Cập nhật tiên thừ vào đợt kế cuối
                updateRemainMoney(QO);
                traceService.Trace("5");
            }

        }
        private void updateRemainMoney(Entity quote)
        {
            EntityReference paymentScheme = (EntityReference)quote["bsd_paymentscheme"];
            QueryExpression q = new QueryExpression("bsd_paymentschemedetail");
            q.ColumnSet = new ColumnSet(true);
            q.AddOrder("bsd_ordernumber", OrderType.Ascending);
            FilterExpression filter = new FilterExpression(LogicalOperator.And);
            filter.AddCondition(new ConditionExpression("bsd_paymentscheme", ConditionOperator.Equal, paymentScheme.Id));
            filter.AddCondition(new ConditionExpression("bsd_optionentry", ConditionOperator.Null));
            filter.AddCondition(new ConditionExpression("bsd_reservation", ConditionOperator.Null));
            filter.AddCondition(new ConditionExpression("bsd_quotation", ConditionOperator.Null));
            q.Criteria = filter;
            EntityCollection encolInstallmentMaster = service.RetrieveMultiple(q);

            q = new QueryExpression("bsd_paymentschemedetail");
            q.ColumnSet = new ColumnSet(new string[] { "bsd_amountofthisphase", "bsd_duedatecalculatingmethod" });
            q.Criteria = new FilterExpression(LogicalOperator.And);
            q.Criteria.AddCondition(new ConditionExpression("bsd_reservation", ConditionOperator.Equal, quote.Id));
            q.Orders.Add(new OrderExpression("bsd_ordernumber", OrderType.Ascending));
            EntityCollection encolInstallment = service.RetrieveMultiple(q);
            traceService.Trace("updateRemainMoney Count bsd_paymentschemedetail: "+ encolInstallment.Entities.Count.ToString());
            if(encolInstallment.Entities.Count > 2)
            {
                decimal sum = 0;
                Entity enPaymentdetailHandover = new Entity();

                foreach (Entity en in encolInstallment.Entities)
                {
                    decimal bsd_amountofthisphase = en.Contains("bsd_amountofthisphase") ? ((Money)en["bsd_amountofthisphase"]).Value : 0;
                    sum += bsd_amountofthisphase;
                    if (en.Attributes.Contains("bsd_duedatecalculatingmethod"))
                    {
                        if (((OptionSetValue)en["bsd_duedatecalculatingmethod"]).Value == 100000002)
                        {
                            enPaymentdetailHandover = en;
                        }
                    }

                }
                //throw new InvalidPluginExecutionException(((OptionSetValue)enPaymentdetailHandover["bsd_duedatecalculatingmethod"]).Value.ToString());
                decimal genfee = ((Money)quote["totalamount"]).Value - ((Money)quote["bsd_freightamount"]).Value;
                traceService.Trace("genfee: " + genfee.ToString());
                decimal remain = genfee - sum;
                traceService.Trace("remain: " + remain.ToString());
                //Entity tmp = entc.Entities[entc.Entities.Count - 2];
                decimal amountofthisphase = enPaymentdetailHandover.Contains("bsd_amountofthisphase") ? ((Money)enPaymentdetailHandover["bsd_amountofthisphase"]).Value : 0;
                decimal fee = amountofthisphase + remain;
                //if(((Money)enPaymentdetailHandover["bsd_amountofthisphase"]).Value > 0){
                traceService.Trace("fee: "+fee.ToString());
                if (fee > 0 && ((Money)enPaymentdetailHandover["bsd_amountofthisphase"]).Value > 0)
                {
                    enPaymentdetailHandover["bsd_amountofthisphase"] = new Money(fee);
                    service.Update(enPaymentdetailHandover);
                }
            }
            if(encolInstallment.Entities.Count <= 2)
            {
                for (int i = 0; i < encolInstallment.Entities.Count; i++)
                {
                    Entity enIntallment = new Entity(encolInstallment[i].LogicalName,encolInstallment[i].Id);
                    OptionSetValue bsd_duedatecalculatingmethod = encolInstallmentMaster[i].Contains("bsd_duedatecalculatingmethod") ?(OptionSetValue)encolInstallmentMaster[i]["bsd_duedatecalculatingmethod"]: null;
                    enIntallment["bsd_duedatecalculatingmethod"] = bsd_duedatecalculatingmethod;
                    bool bsd_lastinstallment = encolInstallmentMaster[i].Contains("bsd_lastinstallment") ? (bool)encolInstallmentMaster[i]["bsd_lastinstallment"] : false;
                    enIntallment["bsd_lastinstallment"] = bsd_lastinstallment;
                    service.Update(enIntallment);
                }
            }
        }

        private void GenPaymentScheme(ref Entity QO, ref DateTime date, ITracingService trac)
        {
            traceService.Trace("a");
            EntityReference productId = null;
            priceperunit = GetProductPrice(QO.Id, out productId);
            traceService.Trace("b");
            // landValue = GetLandvalueOfProduct(productId);
            // Tri cm 16 08 12  User require - change calc method get land value on Reservation not get landvalue from Units
            landValue = QO.Contains("bsd_landvaluededuction") ? ((Money)QO["bsd_landvaluededuction"]).Value : 0;
            traceService.Trace("c");
            if (!QO.Contains("bsd_taxcode")) throw new InvalidPluginExecutionException("Please input Tax Code");
            tax = GetTax((EntityReference)QO["bsd_taxcode"]);
            TaxAmount = (priceperunit - landValue) * tax / 100;
            traceService.Trace("d");
            bsd_freightamount = QO.Contains("bsd_freightamount") ? ((Money)QO["bsd_freightamount"]).Value : 0;
            bsd_managementfee = QO.Contains("bsd_managementfee") ? ((Money)QO["bsd_managementfee"]).Value : 0;
            traceService.Trace("e");
            totalTax = QO.Contains("totaltax") ? ((Money)QO["totaltax"]).Value : 0;
            traceService.Trace("e1");
            decimal total_TMP = QO.Contains("totalamount") ? ((Money)QO["totalamount"]).Value : 0;  // use net selling price
            decimal totalAmount = total_TMP + tax * landValue / 100 - bsd_freightamount;//
            //decimal totalAmount = total_TMP - bsd_freightamount;
            traceService.Trace("f");
            // @@TriCm 16.07.15 - check localization if internal or external
            EntityReference customerRef = QO.Contains("customerid") ? (EntityReference)QO["customerid"] : null;
            int i_localization = 0;
            if (customerRef != null)
            {
                Entity customer = service.Retrieve(customerRef.LogicalName, customerRef.Id, new ColumnSet(new string[] {
                (customerRef.LogicalName=="contact"?"fullname":"name"),
                "bsd_localization"
                }));
                traceService.Trace("g");
                i_localization = customer.Contains("bsd_localization") ? ((OptionSetValue)customer["bsd_localization"]).Value : -1;
                if (i_localization == -1)
                {
                    if (!QO.Contains("bsd_quotecodesams"))
                        throw new InvalidPluginExecutionException("Please check 'Localization' field on Customer " + (customerRef.LogicalName == "contact" ? customer["fullname"] : customer["name"]) + " information!");
                }
            }
           
           

            EntityReference paymentScheme = (EntityReference)QO["bsd_paymentscheme"];
            // ## TriCM no need calculate deposit more - 16.07.12- Han require
            //deposit = GetDepositAmount(paymentScheme);

            QueryExpression q = new QueryExpression("bsd_paymentschemedetail");
            q.ColumnSet = new ColumnSet(true);
            q.AddOrder("bsd_ordernumber", OrderType.Ascending);
            FilterExpression filter = new FilterExpression(LogicalOperator.And);
            filter.AddCondition(new ConditionExpression("bsd_paymentscheme", ConditionOperator.Equal, paymentScheme.Id));
            filter.AddCondition(new ConditionExpression("bsd_optionentry", ConditionOperator.Null));
            filter.AddCondition(new ConditionExpression("bsd_reservation", ConditionOperator.Null));
            filter.AddCondition(new ConditionExpression("bsd_quotation", ConditionOperator.Null));
            filter.AddCondition(new ConditionExpression("bsd_appendixcontract", ConditionOperator.Null));
            filter.AddCondition(new ConditionExpression("bsd_changeinformation", ConditionOperator.Null));
            q.Criteria = filter;
            EntityCollection ents = service.RetrieveMultiple(q);

            Entity PM = service.Retrieve(paymentScheme.LogicalName, paymentScheme.Id,
                 new ColumnSet(new string[] { "bsd_paymentschemecode", "bsd_startdate", "bsd_name", "bsd_paymentschemeid" }));

            int len = ents.Entities.Count;
            int orderNumber = 0;
            if (len == 0)
                throw new InvalidPluginExecutionException("No payment scheme detail in payment scheme '" + (string)PM["bsd_paymentschemecode"] + "'. Please setup payment scheme again!");

            //bsd_duedatecalculatingmethod
            //100,000,000 - fix
            //100,000,001 // auto
            //100,000,002 // Estimate Handover Date
            bool f_lastinstallment = false;
            bool f_es = false;
            int i_ESmethod = 0;
            decimal d_ESpercent = 0;
            bool f_ESmaintenancefees = false;
            bool f_ESmanagementfee = false;
            bool f_signcontractinstallment = false;
            DateTime d_estimate = get_EstimatehandoverDate(QO);

            bool f_last_ES = true;
            int i_dueCalMethod = -1;


            Entity en_Unit = service.Retrieve(((EntityReference)QO["bsd_unitno"]).LogicalName, ((EntityReference)QO["bsd_unitno"]).Id, new ColumnSet(new string[] {
                "name",
                "statuscode",
                "bsd_signedcontractdate",
                "bsd_actualarea",
                "bsd_netsaleablearea"
                }));
            // 170311 neu tren unit k tim duoc actual area thi su dung net sale able arae

            decimal d_dientich = 0;
            if (en_Unit.Contains("bsd_actualarea"))
                d_dientich = (decimal)en_Unit["bsd_actualarea"];
            else d_dientich = en_Unit.Contains("bsd_netsaleablearea") ? (decimal)en_Unit["bsd_netsaleablearea"] : 0;

            Entity en_project = service.Retrieve(((EntityReference)QO["bsd_projectid"]).LogicalName, ((EntityReference)QO["bsd_projectid"]).Id,
                            new ColumnSet(new string[] { "bsd_name", "bsd_managementamount" }));
            decimal d_bsd_managementamount_pro = en_project.Contains("bsd_managementamount") ? ((Money)en_project["bsd_managementamount"]).Value : 0;

            for (int i = 0; i < len; i++) // len = so luong INS detail
            {
                f_ESmaintenancefees = false;
                f_ESmanagementfee = false;
                if(ents.Entities[i].Contains("bsd_maintenancefees"))
                {
                    f_ESmaintenancefees = (bool)ents.Entities[i]["bsd_maintenancefees"];
                }
                if (ents.Entities[i].Contains("bsd_managementfee"))
                {
                    f_ESmanagementfee = (bool)ents.Entities[i]["bsd_managementfee"];
                }
                if (ents.Entities[i].Contains("bsd_signcontractinstallment"))
                {
                    f_signcontractinstallment = (bool)ents.Entities[i]["bsd_signcontractinstallment"];
                }
                traceService.Trace(i.ToString());
                traceService.Trace("f_ESmanagementfee: "+ f_ESmanagementfee.ToString());
                traceService.Trace("bsd_managementfee: " + bsd_managementfee.ToString());

                if (!ents.Entities[i].Contains("bsd_amountpercent"))
                    throw new InvalidPluginExecutionException("Please select field 'Percent (%)' for '" + (string)ents.Entities[i]["bsd_name"] + "' of Payment Scheme " + (string)PM["bsd_paymentschemecode"] + " on Master data!");
                decimal percent = (decimal)ents.Entities[i]["bsd_amountpercent"];

                if (ents.Entities[i].Contains("bsd_duedatecalculatingmethod"))
                {
                    i_dueCalMethod = ((OptionSetValue)ents.Entities[i]["bsd_duedatecalculatingmethod"]).Value;
                    if (i_dueCalMethod == 100000002)
                        f_es = true;
                    else f_es = false;
                }
                else f_es = false;
                if (ents.Entities[i].Contains("bsd_lastinstallment") && (bool)ents.Entities[i]["bsd_lastinstallment"] == true)
                    f_lastinstallment = true;
                else f_lastinstallment = false;

                if (f_es)
                {
                }

                if ((f_lastinstallment) || (f_es))
                {

                    CreatePaymentPhase_fixDate(PM, ref orderNumber, bsd_managementfee, total_TMP, bsd_freightamount, ref f_last_ES, ents.Entities[i], QO, productId, totalAmount,
                        percent, ref date, false, i_localization, f_lastinstallment, f_es, d_estimate, i_ESmethod, d_ESpercent, f_ESmaintenancefees, f_ESmanagementfee, len, trac, f_signcontractinstallment);

                }
                else
                {
                    if (!ents.Entities[i].Contains("bsd_duedatecalculatingmethod"))
                        throw new InvalidPluginExecutionException("Please choose Duedate Calculating Method for '" + (string)ents.Entities[i]["bsd_name"] + "' of Payment Scheme " + (string)PM["bsd_paymentschemecode"] + " on Master data!");

                    i_dueCalMethod = ((OptionSetValue)ents.Entities[i]["bsd_duedatecalculatingmethod"]).Value;
                    if (i_dueCalMethod == 100000001) // auto
                    {
                        // auto chia lam 2 truong hop
                        #region notice
                        // neu payment type = default or null
                        // chi set duedate cho 1 installment - duedate dua vao dieukien type of start date ( is deposit or not )
                        // next period type = day - set duedate cua dot nay tu ngay deposit or start date cua pms master ( neu number of next day = null )
                        // neu number of nextday != null ( vi du =14) thi duedate cua dot nay = ngay startdate + 14  ( hoac duedate cua dot truoc do + 14  - neu k fai dot 1)
                        // tuong tu voi month ( + them 30 ngay vao duedate input)

                        // neu payment type = times - auto gen cho nhieu dot
                        // duedate cua cac dot duoc tinh toan boi cac field phia tren
                        // neu Payment date monthly = null ( giu nguyen duedate - thay doi thang va nam)
                        // neu payment date monthly != null ( dua vao number of times ma bit duoc tong so installment can generate ra)
                        // next day of end phase = 14 - neu payment date monthly = 1 thi cong them next day of end phase cho dot cuoi cung trong chuoi instalment can generate ra
                        #endregion

                        #region repair data --------------
                        f_lastinstallment = false;
                        f_es = false;
                        if (!ents.Entities[i].Contains("bsd_nextperiodtype")) throw new InvalidPluginExecutionException("Please choose 'Next period type' for " + (string)ents.Entities[i]["bsd_name"] + " of Payment scheme " + (string)PM["bsd_paymentschemecode"] + " on Master data!");

                        int i_bsd_nextperiodtype = (int)((OptionSetValue)ents.Entities[i]["bsd_nextperiodtype"]).Value;
                        int i_paymentdatemonthly = 0;
                        if (ents.Entities[i].Contains("bsd_datepaymentofmonthly"))
                        {
                            i_paymentdatemonthly = (int)ents.Entities[i]["bsd_datepaymentofmonthly"];//bsd_datepaymentofmonthly
                            //throw new InvalidPluginExecutionException(i_paymentdatemonthly.ToString());
                        }

                        //default or null
                        int? payment_type = ents.Entities[i].Contains("bsd_typepayment") ? (int?)((OptionSetValue)ents.Entities[i]["bsd_typepayment"]).Value : null;

                        // check if order number ==1 - if order number = 1 - get startdate of QuoInfor for base to calc duedate of 1st
                        // else get the duedate of instalment with order number -1 to calc new duedate for new ins
                        if (orderNumber == 1)
                        {
                            if (QO.Contains("bsd_deposittime"))
                            {
                                date = (DateTime)QO["bsd_deposittime"];
                            }
                        }

                        //if (orderNumber >= 1)
                        //{
                        //    int i_tpm = orderNumber > 1 ? orderNumber - 1 : 1;

                        //    EntityCollection ec_PreviousIns = get_PreviousIns(service, i_tpm, PM.Id, QO.Id);
                        //    if (ec_PreviousIns.Entities.Count < 0) throw new InvalidPluginExecutionException("Cannot find previous Installment duedate. Please check Payment Scheme master again!");
                        //    DateTime dt_PrevInsDuedate = (DateTime)ec_PreviousIns.Entities[0]["bsd_duedate"];

                        //    date = dt_PrevInsDuedate;
                        //}

                        #region -------- calc duedate first time -------------
                        // 06.12 - Hân
                        int i_paymentdatemonthly_def = i_paymentdatemonthly;

                        if (i_paymentdatemonthly != 0)
                        {
                            int NgayCuoiThang = DateTime.DaysInMonth(date.Year, date.Month);
                            if (i_paymentdatemonthly > NgayCuoiThang)
                                i_paymentdatemonthly = NgayCuoiThang;
                            date = new DateTime(date.Year, date.Month, i_paymentdatemonthly);
                        }

                        // confirm 170412 - Han - khi deposit time hoac sign contract thi moi su dung toi extra date - khi generate lan dau tien chi su dung reservation time
                        #endregion
                        #endregion

                        if (payment_type == null || payment_type == 1)//default or month
                        {
                            CreatePaymentPhase(PM, ref orderNumber, ents.Entities[i], QO, i_localization, totalAmount, percent, ref date, trac, i_paymentdatemonthly_def, f_ESmaintenancefees, f_ESmanagementfee, bsd_managementfee, bsd_freightamount, len, f_signcontractinstallment);
                        }
                        else if (payment_type == 2)//times
                        {

                            if (!ents.Entities[i].Contains("bsd_number"))
                                throw new InvalidPluginExecutionException("Please select field 'Number of Times' on '" + (string)ents.Entities[i]["bsd_name"] + "' of Payment Scheme " + (string)PM["bsd_paymentschemecode"] + " on Master data!");
                            int number = (int)ents.Entities[i]["bsd_number"];
                            int i_bsd_nextdaysofendphase = 0;
                            // if bsd_nextdaysofendphase not null - the last installment of array auto Ins must  be plus the day of this field
                            if (ents.Entities[i].Contains("bsd_nextdaysofendphase"))
                            {
                                i_bsd_nextdaysofendphase = (int)ents.Entities[i]["bsd_nextdaysofendphase"];
                            }

                            for (int j = 0; j < number; j++)
                            {
                                if (j == number - 1)
                                    date = date.AddDays(i_bsd_nextdaysofendphase);

                                CreatePaymentPhase(PM, ref orderNumber, ents.Entities[i], QO, i_localization, totalAmount, percent, ref date, trac, i_paymentdatemonthly_def, f_ESmaintenancefees, f_ESmanagementfee, bsd_managementfee, bsd_freightamount, len, f_signcontractinstallment);
                            }

                        }

                    } // end of if i_duedateCal =100000000
                    else if (i_dueCalMethod == 100000000) // fixx
                    {
                        //CreatePaymentPhase_fixDate(0, total_TMP, bsd_freightamount, ref f_last_ES, PM, ref orderNumber, ents.Entities[i], QO, productId, totalAmount, percent, ref date, false, i_localization, f_lastinstallment, f_es, d_estimate, i_ESmethod, d_ESpercent, f_ESmaintenancefees, f_ESmanagementfee, trac);
                        CreatePaymentPhase_fixDate(PM, ref orderNumber, bsd_managementfee, total_TMP, bsd_freightamount, ref f_last_ES, ents.Entities[i], QO, productId, totalAmount, percent, ref date, false, i_localization, f_lastinstallment, f_es, d_estimate, i_ESmethod, d_ESpercent, f_ESmaintenancefees, f_ESmanagementfee, len, trac, f_signcontractinstallment);
                    }
                }
            }
        }

        private void CreatePaymentPhase(Entity PM, ref int orderNumber, Entity en, Entity QO, int i_localization, decimal reservationAmount, decimal percent, ref DateTime date, ITracingService trac, int i_paymentdatemonthly, bool f_ESmaintenancefees, bool f_ESmanagementfee, decimal bsd_managementfee, decimal bsd_maintenancefees, int InstallmentCount,bool f_signcontractinstallment)
        {
            double extraDay = 0;
            int i_nextMonth = 1;
            if (!en.Contains("bsd_nextperiodtype"))
                if (!QO.Contains("bsd_quotecodesams"))
                    throw new InvalidPluginExecutionException("Please select field 'Next period type' on '" + en["bsd_name"].ToString() + "' of Payment Scheme " + (string)PM["bsd_paymentschemecode"] + " on Master data!");

            int type = ((OptionSetValue)en["bsd_nextperiodtype"]).Value;

            //bool b_typeofstartdate = false;
            //if (en.Contains("bsd_typeofstartdate") && orderNumber == 0)
            //{
            //    if ((bool)en["bsd_typeofstartdate"] == true)
            //        b_typeofstartdate = true;
            //}
            bool b_typeofstartdate = ((bool)en["bsd_typeofstartdate"]);
            if (b_typeofstartdate) // Nếu bsd_typeofstartdate = Yes => set lại ngày = bsd_reservationtime
            {
                date = (DateTime)QO["createdon"];
                if (QO.Contains("statuscode") && ((OptionSetValue)QO["statuscode"]).Value == 100000000) //reservation
                {
                    if (!QO.Contains("bsd_reservationtime")) throw new InvalidPluginExecutionException("Reservation did not contain Reservation time! Please check again!");
                    else date = (DateTime)QO["bsd_reservationtime"];
                }
                if (QO.Contains("bsd_quotecodesams")) date = (DateTime)QO["bsd_reservationtime"];
            }

            if (type == 1)//month
            {
                if (!en.Attributes.Contains("bsd_numberofnextmonth"))
                    if (!QO.Contains("bsd_quotecodesams"))
                        throw new InvalidPluginExecutionException("Please select field 'Number next month' on '" + en["bsd_name"].ToString() + "' of Payment Scheme " + (string)PM["bsd_paymentschemecode"] + " on Master data!");
                i_nextMonth = ((int)en["bsd_numberofnextmonth"]);
                //date = = new DateTime;
                // if (i_nextMonth > 12) throw new InvalidPluginExecutionException("Number of next month must be less than or equal 12!");
                date = date.AddMonths(i_nextMonth);
                //int tmp_month = date.Month;
                //if (i_nextMonth + date.Month > 12)
                //    if (date.Month + i_nextMonth == 2 && )

                //        date = new DateTime(date.Year + 1, (date.Month + i_nextMonth) - 12, date.Day);
                //    else date = new DateTime(date.Year, date.Month + i_nextMonth, date.Day);

            }
            else if (type == 2)//day
            {
                if (!en.Attributes.Contains("bsd_numberofnextdays"))
                    if (!QO.Contains("bsd_quotecodesams"))
                        throw new InvalidPluginExecutionException("Please select field 'Number of next days' on '" + en["bsd_name"].ToString() + "' of Payment Scheme " + (string)PM["bsd_paymentschemecode"] + " on Master data!");
                extraDay = double.Parse(en["bsd_numberofnextdays"].ToString());
                date = date.AddDays(extraDay);
            }

            //27.02.2018
            if (i_paymentdatemonthly != 0)
            {
                int NgayCuoiThang = DateTime.DaysInMonth(date.Year, date.Month);
                if (i_paymentdatemonthly > NgayCuoiThang)
                    i_paymentdatemonthly = NgayCuoiThang;
                date = new DateTime(date.Year, date.Month, i_paymentdatemonthly);
            }

            orderNumber++;

            Entity tmp = new Entity(en.LogicalName);
            tmp["bsd_ordernumber"] = orderNumber;
            tmp["bsd_name"] = "Installment " + orderNumber;
            tmp["bsd_code"] = string.Format("{0}-{1:ddMMyyyyhhmmssff}", tmp["bsd_name"], DateTime.Now);
            tmp["bsd_reservation"] = QO.ToEntityReference();
            tmp["bsd_paymentscheme"] = en["bsd_paymentscheme"];
            tmp["bsd_amountpercent"] = en["bsd_amountpercent"];
            tmp["bsd_amountwaspaid"] = new Money(0);

            tmp["bsd_waiveramount"] = new Money(0);
            tmp["bsd_waiverinterest"] = new Money(0);
            tmp["bsd_waiverinstallment"] = new Money(0);

            tmp["bsd_actualgracedays"] = 0;

            tmp["bsd_interestwaspaid"] = new Money(0);
            tmp["bsd_interestchargeamount"] = new Money(0);

            tmp["bsd_managementfeepaid"] = new Money(0);
            tmp["bsd_maintenancefeepaid"] = new Money(0);
            tmp["bsd_maintenanceamount"] = new Money(0);
            tmp["bsd_managementamount"] = new Money(0);
            tmp["bsd_maintenancefeewaiver"] = new Money(0);
            tmp["bsd_managementfeewaiver"] = new Money(0);

            tmp["bsd_estimateamount"] = new Money(0);
            tmp["bsd_taxlandvalue"] = new Money(0);
            tmp["bsd_tmpamount"] = new Money(0);

            tmp["bsd_depositamount"] = new Money(0);
            if (orderNumber == 1)
            {
                decimal depositfee = QO.Contains("bsd_depositfee") ? ((Money)QO["bsd_depositfee"]).Value : 0;
                tmp["bsd_depositamount"] = new Money(depositfee);
            }
            statusCode = QO.Attributes.Contains("statuscode") ? ((OptionSetValue)QO["statuscode"]).Value : -1;
            if (statusCode != 100000011)
            {
                if (b_typeofstartdate == true) //&& orderNumber == 1)
                {
                    if (i_localization == 100000000)
                    {
                        if (!en.Attributes.Contains("bsd_withindate"))
                            if (!QO.Contains("bsd_quotecodesams"))
                                throw new InvalidPluginExecutionException("Please select field 'Email reminder (local)' on '" + (string)en["bsd_name"] + "' of Payment Scheme " + (string)PM["bsd_paymentschemecode"] + " on Master data!");
                        date = date.AddDays(double.Parse(en["bsd_withindate"].ToString()));
                        tmp["bsd_withindate"] = (int)en["bsd_withindate"];
                    }
                    else
                    {
                        if (!en.Attributes.Contains("bsd_emailreminderforeigner"))
                            if (!QO.Contains("bsd_quotecodesams"))
                                throw new InvalidPluginExecutionException("Please select field 'Email reminder (foreigner)' on '" + (string)en["bsd_name"] + "' of Payment Scheme " + (string)PM["bsd_paymentschemecode"] + " on Master data!");
                        date = date.AddDays(double.Parse(en["bsd_emailreminderforeigner"].ToString()));
                        tmp["bsd_emailreminderforeigner"] = (int)en["bsd_emailreminderforeigner"];
                    }
                }
            }
            


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

            decimal tmpamount = Math.Round((percent * reservationAmount / 100), 0);
            tmp["bsd_amountofthisphase"] = new Money(tmpamount);
            tmp["bsd_balance"] = new Money(tmpamount);
            tmp["bsd_duedatecalculatingmethod"] = new OptionSetValue(100000001);
            if (!QO.Contains("bsd_quotecodesams"))
                tmp["bsd_duedate"] = date;
            else
                if (date != null)
                tmp["bsd_duedate"] = date;
            tmp.Id = Guid.NewGuid();
            #region if bsd_maintenancefees/ bsd_managementfee = yes => set amount
            tmp["bsd_maintenancefees"] = f_ESmaintenancefees;
            tmp["bsd_managementfee"] = f_ESmanagementfee;
            tmp["bsd_signcontractinstallment"] = f_signcontractinstallment;
            if (f_ESmanagementfee)
                tmp["bsd_managementamount"] = new Money(bsd_managementfee);
            else tmp["bsd_managementamount"] = new Money(0);
            if (f_ESmaintenancefees)
                tmp["bsd_maintenanceamount"] = new Money(bsd_maintenancefees);
            else tmp["bsd_maintenanceamount"] = new Money(0);
            #endregion

            #region Nếu InstallmentCount == 1 : Cập nhật thêm phần tính trừ 10% Land Value vào giá trị đợt 1
            if ((InstallmentCount == 1 && orderNumber == 1) || (InstallmentCount == 2 && orderNumber == 2))
            {
                decimal d_es_LandPercent = Math.Round((tax * landValue / 100), 0);
                if (tmpamount > d_es_LandPercent)
                {
                    tmp["bsd_amountofthisphase"] = new Money(tmpamount - d_es_LandPercent);
                    tmp["bsd_balance"] = new Money(tmpamount - d_es_LandPercent);
                }
                else
                {
                    tmp["bsd_amountofthisphase"] = new Money(tmpamount);
                    tmp["bsd_balance"] = new Money(tmpamount);
                }
                tmp["bsd_estimateamount"] = new Money(tmpamount);
                tmp["bsd_taxlandvalue"] = new Money(d_es_LandPercent);
                tmp["bsd_tmpamount"] = new Money(tmpamount - d_es_LandPercent);
            }
            #endregion

            service.Create(tmp);

        }

        // fixx date
        private void CreatePaymentPhase_fixDate(Entity PM, ref int orderNumber, decimal bsd_managementfee, decimal totalTMP, decimal bsd_maintenancefees, ref bool f_last_ES, Entity en, Entity quoteEN, EntityReference productId,
            decimal reservationAmount, decimal percent, ref DateTime date, bool isLastTime, int i_localization, bool f_last, bool f_es, DateTime d_esDate, int i_ESmethod, decimal d_ESpercent, bool f_ESmaintenancefees, bool f_ESmanagementfee, int InstallmentCount, ITracingService trac,bool f_signcontractinstallment)
        {
            //throw new InvalidPluginExecutionException("CreatePaymentPhase_fixDate");
            Entity tmp = new Entity(en.LogicalName);

            if (f_last == false)
            {
                if (f_es == false)
                {
                    if (!en.Contains("bsd_fixeddate"))
                    {
                        if (!quoteEN.Contains("bsd_quotecodesams"))
                            throw new InvalidPluginExecutionException("Please choose field 'Fixeddate' on '" + (string)en["bsd_name"] + "' of Payment Scheme " + (string)PM["bsd_paymentschemecode"] + " on Master data!");
                    }
                    if (orderNumber < totalNumber - 1)
                    {
                        if (!quoteEN.Contains("bsd_quotecodesams"))
                        {
                            tmp["bsd_duedate"] = en["bsd_fixeddate"];
                            tmp["bsd_fixeddate"] = en["bsd_fixeddate"];
                            date = (DateTime)en["bsd_fixeddate"];
                        }
                        else
                        {
                            if (en.Contains("bsd_fixeddate"))
                            {
                                tmp["bsd_duedate"] = en["bsd_fixeddate"];
                                tmp["bsd_fixeddate"] = en["bsd_fixeddate"];

                            }
                        }
                    }
                }
            }

            orderNumber++;

            tmp["bsd_ordernumber"] = orderNumber;
            tmp["bsd_name"] = "Installment " + orderNumber;
            tmp["bsd_code"] = string.Format("{0}-{1:ddMMyyyyhhmmssff}", tmp["bsd_name"], DateTime.Now);
            tmp["bsd_reservation"] = quoteEN.ToEntityReference();
            tmp["bsd_paymentscheme"] = en["bsd_paymentscheme"];
            tmp["bsd_amountpercent"] = en["bsd_amountpercent"];
            tmp["bsd_amountwaspaid"] = new Money(0);
            tmp["bsd_depositamount"] = new Money(0);

            tmp["bsd_waiveramount"] = new Money(0);
            tmp["bsd_waiverinterest"] = new Money(0);
            tmp["bsd_waiverinstallment"] = new Money(0);

            tmp["bsd_actualgracedays"] = 0;

            tmp["bsd_interestwaspaid"] = new Money(0);
            tmp["bsd_interestchargeamount"] = new Money(0);

            tmp["bsd_managementfeepaid"] = new Money(0);
            tmp["bsd_maintenancefeepaid"] = new Money(0);
            tmp["bsd_maintenanceamount"] = new Money(0);
            tmp["bsd_managementamount"] = new Money(0);
            tmp["bsd_maintenancefeewaiver"] = new Money(0);
            tmp["bsd_managementfeewaiver"] = new Money(0);

            tmp["bsd_estimateamount"] = new Money(0);
            tmp["bsd_taxlandvalue"] = new Money(0);
            tmp["bsd_tmpamount"] = new Money(0);

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

            if (orderNumber == 1)
            {

                decimal depositfee = quoteEN.Contains("bsd_depositfee") ? ((Money)quoteEN["bsd_depositfee"]).Value : 0;
                tmp["bsd_depositamount"] = new Money(depositfee);
                decimal tmpamount = Math.Round((percent * reservationAmount / 100), 0);
                tmp["bsd_amountofthisphase"] = new Money(tmpamount);
                tmp["bsd_balance"] = new Money(tmpamount);

                if (!quoteEN.Contains("bsd_quotecodesams"))
                {
                    if (en.Contains("bsd_fixeddate"))
                    {
                        tmp["bsd_duedate"] = en["bsd_fixeddate"];
                        tmp["bsd_fixeddate"] = en["bsd_fixeddate"];
                        date = (DateTime)en["bsd_fixeddate"];
                    }
                }
                else
                {
                    if (en.Contains("bsd_fixeddate"))
                    {
                        tmp["bsd_duedate"] = en["bsd_fixeddate"];
                        tmp["bsd_fixeddate"] = en["bsd_fixeddate"];

                    }
                }

                tmp["bsd_duedatecalculatingmethod"] = new OptionSetValue(100000000);
                #region if bsd_maintenancefees/ bsd_managementfee = yes => set amount
                tmp["bsd_maintenancefees"] = f_ESmaintenancefees;
                tmp["bsd_managementfee"] = f_ESmanagementfee;
                tmp["bsd_signcontractinstallment"] = f_signcontractinstallment;
                if (f_ESmanagementfee)
                    tmp["bsd_managementamount"] = new Money(bsd_managementfee);
                else tmp["bsd_managementamount"] = new Money(0);
                if (f_ESmaintenancefees)
                    tmp["bsd_maintenanceamount"] = new Money(bsd_maintenancefees);
                else tmp["bsd_maintenanceamount"] = new Money(0);
                #endregion
                #region Nếu InstallmentCount == 1 : Cập nhật thêm phần tính trừ 10% Land Value vào giá trị đợt 1
                if (InstallmentCount == 1)
                {
                    decimal d_es_LandPercent = Math.Round((tax * landValue / 100), 0);
                    if (tmpamount > d_es_LandPercent)
                    {
                        f_last_ES = true;
                        tmp["bsd_amountofthisphase"] = new Money(tmpamount - d_es_LandPercent);
                        tmp["bsd_balance"] = new Money(tmpamount - d_es_LandPercent);
                    }
                    else
                    {
                        f_last_ES = false;
                        tmp["bsd_amountofthisphase"] = new Money(tmpamount);
                        tmp["bsd_balance"] = new Money(tmpamount);
                    }
                    tmp["bsd_estimateamount"] = new Money(tmpamount);
                    tmp["bsd_taxlandvalue"] = new Money(d_es_LandPercent);
                    tmp["bsd_tmpamount"] = new Money(tmpamount - d_es_LandPercent);
                }
                #endregion
                tmp.Id = Guid.NewGuid();

                service.Create(tmp);

            } //end of  if (orderNumber == 1)
            else
            {
                decimal tmpamount = Math.Round((percent * reservationAmount / 100), 0);

                if (f_last == false)
                {
                    tmp["bsd_duedatecalculatingmethod"] = new OptionSetValue(100000000);

                    if (f_es == true)
                    {
                        //decimal d_es_LandPercent = Math.Round((tax * landValue / 100), 0);
                        decimal d_es_LandPercent = 0;
                        if (tmpamount > d_es_LandPercent)
                        {
                            f_last_ES = true;
                            tmp["bsd_amountofthisphase"] = new Money(tmpamount - d_es_LandPercent);
                            tmp["bsd_balance"] = new Money(tmpamount - d_es_LandPercent);
                        }
                        else
                        {
                            f_last_ES = false;
                            tmp["bsd_amountofthisphase"] = new Money(tmpamount);
                            tmp["bsd_balance"] = new Money(tmpamount);
                        }


                        // new update 161109
                        tmp["bsd_estimateamount"] = new Money(tmpamount);
                        tmp["bsd_taxlandvalue"] = new Money(d_es_LandPercent);
                        tmp["bsd_tmpamount"] = new Money(tmpamount - d_es_LandPercent);
                        if (!quoteEN.Contains("bsd_quotecodesams"))
                            tmp["bsd_duedate"] = d_esDate;
                        else
                        {
                            if (d_esDate != null)
                                tmp["bsd_duedate"] = d_esDate;
                        }
                        tmp["bsd_duedatecalculatingmethod"] = new OptionSetValue(100000002);
                        // tmp["bsd_method"] = new OptionSetValue(i_ESmethod);
                        // tmp["bsd_percent"] = d_ESpercent;

                        //tmp["bsd_maintenancefees"] = f_ESmaintenancefees;
                        //tmp["bsd_managementfee"] = f_ESmanagementfee;
                        //if (f_ESmanagementfee)
                        //    tmp["bsd_managementamount"] = new Money(bsd_managementfee);
                        //else tmp["bsd_managementamount"] = new Money(0);
                        //if (f_ESmaintenancefees)
                        //    tmp["bsd_maintenanceamount"] = new Money(bsd_maintenancefees);
                        //else tmp["bsd_maintenanceamount"] = new Money(0);

                    }
                    else // f_es = falses
                    {

                        if (!quoteEN.Contains("bsd_quotecodesams"))
                        {
                            tmp["bsd_duedate"] = en["bsd_fixeddate"];
                            tmp["bsd_fixeddate"] = en["bsd_fixeddate"];
                            date = (DateTime)en["bsd_fixeddate"];
                        }
                        else
                        {
                            if (en.Contains("bsd_fixeddate"))
                            {
                                tmp["bsd_duedate"] = en["bsd_fixeddate"];
                                tmp["bsd_fixeddate"] = en["bsd_fixeddate"];
                            }
                        }

                        //tmp["bsd_amountofthisphase"] = new Money(tmpamount + (tmpamount * tax / 100));
                        tmp["bsd_amountofthisphase"] = new Money(tmpamount);
                        tmp["bsd_balance"] = new Money(tmpamount);
                    }

                    #region if bsd_maintenancefees/ bsd_managementfee = yes => set amount
                    tmp["bsd_maintenancefees"] = f_ESmaintenancefees;
                    tmp["bsd_managementfee"] = f_ESmanagementfee;
                    tmp["bsd_signcontractinstallment"] = f_signcontractinstallment;
                    if (f_ESmanagementfee)
                        tmp["bsd_managementamount"] = new Money(bsd_managementfee);
                    else tmp["bsd_managementamount"] = new Money(0);
                    if (f_ESmaintenancefees)
                        tmp["bsd_maintenanceamount"] = new Money(bsd_maintenancefees);
                    else tmp["bsd_maintenanceamount"] = new Money(0);
                    #endregion
                    #region Nếu InstallmentCount == 2 : Cập nhật thêm phần tính trừ 10% Land Value vào giá trị đợt 2
                    if (InstallmentCount == 2 && orderNumber == 2)
                    {
                        decimal d_es_LandPercent = Math.Round((tax * landValue / 100), 0);
                        if (tmpamount > d_es_LandPercent)
                        {
                            f_last_ES = true;
                            tmp["bsd_amountofthisphase"] = new Money(tmpamount - d_es_LandPercent);
                            tmp["bsd_balance"] = new Money(tmpamount - d_es_LandPercent);
                        }
                        else
                        {
                            f_last_ES = false;
                            tmp["bsd_amountofthisphase"] = new Money(tmpamount);
                            tmp["bsd_balance"] = new Money(tmpamount);
                        }
                        tmp["bsd_estimateamount"] = new Money(tmpamount);
                        tmp["bsd_taxlandvalue"] = new Money(d_es_LandPercent);
                        tmp["bsd_tmpamount"] = new Money(tmpamount - d_es_LandPercent);
                    }
                    #endregion

                    Guid guid = service.Create(tmp);


                }
                else  // f_last = true
                {

                    tmp["bsd_lastinstallment"] = true;
                    tmp["bsd_amountofthisphase"] = new Money(tmpamount);
                    tmp["bsd_balance"] = new Money(tmpamount);

                    #region if bsd_maintenancefees/ bsd_managementfee = yes => set amount
                    tmp["bsd_maintenancefees"] = f_ESmaintenancefees;
                    tmp["bsd_managementfee"] = f_ESmanagementfee;
                    tmp["bsd_signcontractinstallment"] = f_signcontractinstallment;
                    if (f_ESmanagementfee)
                        tmp["bsd_managementamount"] = new Money(bsd_managementfee);
                    else tmp["bsd_managementamount"] = new Money(0);
                    if (f_ESmaintenancefees)
                        tmp["bsd_maintenanceamount"] = new Money(bsd_maintenancefees);
                    else tmp["bsd_maintenanceamount"] = new Money(0);
                    #endregion

                    Guid guid = service.Create(tmp);

                    if (f_last_ES == true)
                    {
                        //decimal temp = SumAmountPhase(service, quoteEN.Id);

                        decimal d_SumTmp = 0;
                        EntityCollection ec_Ins = get_AMPhase_Ins(service, quoteEN.Id);

                        if (ec_Ins.Entities.Count > 0)
                        {
                            for (int i = 0; i < ec_Ins.Entities.Count; i++)
                            {
                                Entity en_ins = ec_Ins.Entities[i];
                                en_ins.Id = ec_Ins.Entities[i].Id;
                                d_SumTmp += en_ins.Contains("bsd_amountofthisphase") ? ((Money)en_ins["bsd_amountofthisphase"]).Value : 0;
                            }
                        }

                        Entity pmch = service.Retrieve("bsd_paymentschemedetail", guid, new ColumnSet(new string[] { "bsd_amountofthisphase", "bsd_balance" }));
                        Entity a = new Entity(pmch.LogicalName);
                        a.Id = pmch.Id;

                        //  throw new Exception(tmpamount.ToString() + "_" + totalTMP.ToString() + "_" + bsd_maintenancefees.ToString() + "_" + d_SumTmp.ToString());
                        if ((tmpamount + totalTMP - bsd_maintenancefees - d_SumTmp) < 0)
                        {
                            a["bsd_amountofthisphase"] = new Money(0);
                            a["bsd_balance"] = new Money(0);
                        }
                        else
                        {
                            a["bsd_amountofthisphase"] = new Money(tmpamount + totalTMP - bsd_maintenancefees - d_SumTmp);
                            a["bsd_balance"] = new Money(tmpamount + totalTMP - bsd_maintenancefees - d_SumTmp);
                        }
                        if (percent == 0)
                        {
                            a["bsd_amountofthisphase"] = new Money(0);
                            a["bsd_balance"] = new Money(0);
                        }
                        //  throw new Exception((tmpamount + totalTMP - bsd_maintenancefees - d_SumTmp).ToString());
                        //a["bsd_amountofthisphase"] = new Money(tmpamount + totalTMP - bsd_maintenancefees - temp);
                        //a["bsd_balance"] = new Money(tmpamount + totalTMP - bsd_maintenancefees - temp);
                        #region Nếu InstallmentCount == 2 : Cập nhật thêm phần tính trừ 10% Land Value vào giá trị đợt 2
                        if (InstallmentCount == 2 && orderNumber == 2)
                        {
                            decimal d_es_LandPercent = Math.Round((tax * landValue / 100), 0);
                            if (tmpamount > d_es_LandPercent)
                            {
                                f_last_ES = true;
                                tmp["bsd_amountofthisphase"] = new Money(tmpamount - d_es_LandPercent);
                                tmp["bsd_balance"] = new Money(tmpamount - d_es_LandPercent);
                            }
                            else
                            {
                                f_last_ES = false;
                                tmp["bsd_amountofthisphase"] = new Money(tmpamount);
                                tmp["bsd_balance"] = new Money(tmpamount);
                            }
                            tmp["bsd_estimateamount"] = new Money(tmpamount);
                            tmp["bsd_taxlandvalue"] = new Money(d_es_LandPercent);
                            tmp["bsd_tmpamount"] = new Money(tmpamount - d_es_LandPercent);
                        }
                        #endregion

                        service.Update(a);

                    }// end if f_last_ES == true

                } // else  // f_last = true

            } // end else order number =1
        }

        private decimal GetTax(EntityReference taxcode)
        {
            Entity tax = service.Retrieve(taxcode.LogicalName, taxcode.Id, new ColumnSet(new string[] { "bsd_name", "bsd_value" }));
            if (!tax.Attributes.Contains("bsd_value"))
                throw new InvalidPluginExecutionException("Please input tax value!");
            return (decimal)tax["bsd_value"];
        }

        private decimal GetProductPrice(Guid quoteId, out EntityReference productId)
        {
            QueryExpression q = new QueryExpression("quotedetail");
            q.ColumnSet = new ColumnSet("priceperunit", "productid");
            q.Criteria = new FilterExpression();
            q.Criteria.AddCondition(new ConditionExpression("quoteid", ConditionOperator.Equal, quoteId));
            q.Orders.Add(new OrderExpression("createdon", OrderType.Ascending));
            q.TopCount = 1;
            EntityCollection enc = service.RetrieveMultiple(q);
            if (enc.Entities.Count <= 0)
                throw new InvalidPluginExecutionException("No product is added into reservation!");
            Entity unit = enc.Entities[0];
            if (!unit.Contains("priceperunit"))
                throw new InvalidPluginExecutionException("Please set priceperunit of unit in reservation!");
            decimal price = ((Money)unit["priceperunit"]).Value;
            productId = (EntityReference)unit["productid"];
            return price;
        }

        private decimal GetDepositAmount(EntityReference paymentCheme)
        {
            decimal dps = 0;
            Entity pmch = service.Retrieve(paymentCheme.LogicalName, paymentCheme.Id, new ColumnSet(new string[] { "bsd_depositamount" }));
            if (pmch == null)
                throw new InvalidPluginExecutionException("Paymentscheme '" + paymentCheme.Name + "' is not available");
            else if (!pmch.Attributes.Contains("bsd_depositamount"))
                throw new InvalidPluginExecutionException("Please input deposit on paymentscheme '" + paymentCheme.Name + "'");
            else dps = ((Money)pmch["bsd_depositamount"]).Value;
            return dps;
        }

        private decimal GetLandvalueOfProduct(EntityReference productId)
        {
            decimal rs = 0;
            Entity pro = service.Retrieve(productId.LogicalName, productId.Id, new ColumnSet(new string[] { "bsd_landvalueofunit" }));
            if (pro == null)
                throw new InvalidPluginExecutionException("Product is not available!");
            rs = pro.Attributes.Contains("bsd_landvalueofunit") ? ((Money)pro["bsd_landvalueofunit"]).Value : 0;
            return rs;
        }

        private void DeletePaymentPhase(Guid quoteId)
        {
            QueryExpression q = new QueryExpression("bsd_paymentschemedetail");
            q.ColumnSet = new ColumnSet(new string[] { "bsd_name" });
            q.Criteria = new FilterExpression(LogicalOperator.And);
            q.Criteria.AddCondition(new ConditionExpression("bsd_reservation", ConditionOperator.Equal, quoteId));
            EntityCollection entc = service.RetrieveMultiple(q);
            foreach (Entity en in entc.Entities)
                service.Delete(en.LogicalName, en.Id);

        }

        //private void CreateNotice(EntityReference oe, Entity pm)
        //{
        //    Entity notice = new Entity("bsd_notice");
        //    notice["subject"] = string.Format("{0} will due on {1}!", pm["bsd_name"], pm["bsd_duedate"]);
        //    notice["regardingobjectid"] = oe;
        //    notice["requiredattendees"] = GetPartyUsersByRoleName(new string[] { "accounting", "Customer Service Representative" });
        //    notice["scheduledend"] = pm["bsd_duedate"];
        //    service.Create(notice);
        //    //notice["ownerid"] = "";
        //}

        //private Entity[] GetPartyUsersByRoleName(string[] roleNames)
        //{
        //    StringBuilder fetch = new StringBuilder();
        //    fetch.AppendLine("<fetch mapping='logical' version='1.0'>");
        //    fetch.AppendLine("<entity name='systemuser'>");
        //    fetch.AppendLine("<attribute name='fullname'/>");
        //    fetch.AppendLine("<attribute name='systemuserid'/>");
        //    fetch.AppendLine("<link-entity name='systemuserroles' from='systemuserid' to='systemuserid' link-type='inner'>");
        //    fetch.AppendLine("<link-entity name='role' from='roleid' to='roleid' link-type='inner'>");
        //    fetch.AppendLine("<filter type='or'>");
        //    foreach (string name in roleNames)
        //        fetch.AppendLine("<condition attribute='name' operator='eq' value='" + name + "'></condition>");
        //    fetch.AppendLine("</filter>");
        //    fetch.AppendLine("</link-entity>");
        //    fetch.AppendLine("</link-entity>");
        //    fetch.AppendLine("</entity>");
        //    fetch.AppendLine("</fetch>");
        //    EntityCollection entc = service.RetrieveMultiple(new FetchExpression(fetch.ToString()));
        //    int len = entc.Entities.Count;
        //    if (len == 0)
        //        return new Entity[] { };
        //    else
        //    {
        //        Entity[] partyList = new Entity[len];
        //        for (int i = 0; i < len; i++)
        //        {
        //            partyList[i] = new Entity("activityparty");
        //            partyList[i]["partyid"] = entc.Entities[i].ToEntityReference();
        //        }
        //        return partyList;
        //    }
        //}

        private DateTime get_EstimatehandoverDate(Entity OE)
        {
            DateTime d = DateTime.Now;
            QueryExpression q = new QueryExpression("product");
            q.ColumnSet = new ColumnSet(new string[] { "bsd_estimatehandoverdate" });

            LinkEntity linkToProduct = new LinkEntity("product", "quotedetail", "productid", "productid", JoinOperator.Inner);
            //linkToProduct.Columns = new ColumnSet(new string[] { "bsd_estimatehandoverdate" });
            linkToProduct.LinkCriteria = new FilterExpression(LogicalOperator.And);
            linkToProduct.LinkCriteria.AddCondition("quoteid", ConditionOperator.Equal, OE.Id);
            q.LinkEntities.Add(linkToProduct);
            q.TopCount = 1;
            EntityCollection enc = service.RetrieveMultiple(q);
            if (enc.Entities.Count <= 0)
                throw new InvalidPluginExecutionException("Cannot find Unit information in Quotation Reservation. Please check again!");
            else
            {
                Entity en = enc.Entities[0];
                if (!en.Contains("bsd_estimatehandoverdate"))
                    d = get_EstimateFromProject(OE);
                //throw new InvalidPluginExecutionException("Please provide 'Estimate handover date' in Product data!");
                else d = (DateTime)en["bsd_estimatehandoverdate"];
                // product = en.ToEntityReference();
            }
            return d;
        }

        private DateTime get_EstimateFromProject(Entity e_OE)
        {
            DateTime d = DateTime.Now;
            QueryExpression q = new QueryExpression("bsd_project");
            q.ColumnSet = new ColumnSet(new string[] { "bsd_estimatehandoverdate" });
            q.Criteria = new FilterExpression(LogicalOperator.And);
            q.Criteria.AddCondition(new ConditionExpression("bsd_projectid", ConditionOperator.Equal, ((EntityReference)e_OE["bsd_projectid"]).Id));
            q.TopCount = 1;
            EntityCollection entc = service.RetrieveMultiple(q);
            if (entc.Entities.Count <= 0)
                throw new InvalidPluginExecutionException("No project is added into option entry!");
            else
            {
                Entity en = entc.Entities[0];
                if (!en.Contains("bsd_estimatehandoverdate"))
                    throw new InvalidPluginExecutionException("Please provide 'Estimate handover date' in product data or project data!");
                d = (DateTime)en["bsd_estimatehandoverdate"];
                // product = en.ToEntityReference();
            }
            return d;
        }

        //160924 - sum amount phase for all installment before lastinstallment
        private decimal SumAmountPhase(IOrganizationService serv, Guid resvID)
        {
            //xml.AppendLine(string.Format("<condition attribute='bsd_paymentschemedetailid' operator='neq' value='{0}'/>", lastID));

            decimal sumAmount = 0;
            StringBuilder xml = new StringBuilder();
            xml.AppendLine("<fetch version='1.0' output-format='xml-platform' mapping='logical' distinct='false' aggregate='true'>");
            xml.AppendLine("<entity name='bsd_paymentschemedetail'>");
            xml.AppendLine("<attribute name='bsd_amountofthisphase' aggregate='sum' alias='sumAmount'/>");
            xml.AppendLine("<filter type='and'>");
            xml.AppendLine(string.Format("<condition attribute='bsd_reservation' operator='eq' value='{0}'/>", resvID));
            xml.AppendLine("</filter>");
            xml.AppendLine("</entity>");
            xml.AppendLine("</fetch>");

            EntityCollection result = serv.RetrieveMultiple(new FetchExpression(xml.ToString()));
            foreach (var c in result.Entities)
            {
                AliasedValue aValue = c.Contains("sumAmount") ? (AliasedValue)c["sumAmount"] : null;
                if (aValue != null && aValue.Value != null)
                    sumAmount = ((Money)aValue.Value).Value;
                else
                    sumAmount = 0;

                break;
                // sumAmount = ((decimal)((AliasedValue)c["sumAmount"]).Value);
            }
            return sumAmount;
        }

        private EntityCollection get_AMPhase_Ins(IOrganizationService crmservices, Guid quoID)
        {
            string fetchXml =
              @"<fetch version='1.0' output-format='xml-platform' mapping='logical' distinct='false'>
                  <entity name='bsd_paymentschemedetail' >
                    <attribute name='bsd_amountofthisphase' />
                    <filter type='and' >
                      <condition attribute='bsd_reservation' operator='eq' value='{0}' />
                    </filter>
                  </entity>
                </fetch>";
            fetchXml = string.Format(fetchXml, quoID);
            EntityCollection entc = crmservices.RetrieveMultiple(new FetchExpression(fetchXml));
            return entc;

        }


        //get amount of lastinstallment
        private decimal getAmountLastInstallment(Guid installID)
        {
            decimal amount = 0;
            Entity pmch = service.Retrieve("bsd_paymentschemedetail", installID, new ColumnSet(new string[] { "bsd_amountofthisphase" }));
            amount = ((Money)pmch["bsd_amountofthisphase"]).Value;
            return amount;
        }

        private EntityCollection get_PreviousIns(IOrganizationService crmservices, int ordernumber, Guid pmsID, Guid quoteID)
        {
            string fetchXml =
            @"<fetch version='1.0' output-format='xml-platform' mapping='logical' distinct='false'>
                  <entity name='bsd_paymentschemedetail' >
                    <attribute name='bsd_duedate' />
                    <attribute name='bsd_name' />
                    <attribute name ='bsd_paymentschemedetailid' />
                     <filter type='and' >
                      <condition attribute='bsd_ordernumber' operator='eq' value='{0}' />
                      <condition attribute='bsd_paymentscheme' operator='eq' value='{1}' />
                      <condition attribute='bsd_reservation' operator='eq' value='{2}' />
                     </filter>
                  </entity>
                </fetch>";
            fetchXml = string.Format(fetchXml, ordernumber, pmsID, quoteID);
            EntityCollection entc = crmservices.RetrieveMultiple(new FetchExpression(fetchXml));
            return entc;
        }
    }

}
