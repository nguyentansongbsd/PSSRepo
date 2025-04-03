using Microsoft.Crm.Sdk.Messages;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Query;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Plugin_Payment_Import
{
    public class Plugin_Payment_Import : IPlugin
    {
        IOrganizationService service = null;
        IOrganizationServiceFactory factory = null;
        public void Execute(IServiceProvider serviceProvider)
        {
            IPluginExecutionContext context = (IPluginExecutionContext)serviceProvider.GetService(typeof(IPluginExecutionContext));
            factory = (IOrganizationServiceFactory)serviceProvider.GetService(typeof(IOrganizationServiceFactory));
            service = factory.CreateOrganizationService(context.UserId);
            ITracingService traceService = (ITracingService)serviceProvider.GetService(typeof(ITracingService));
            traceService.Trace("Plugin_Plugin_Payment_Import");
            Entity target = context.InputParameters["Target"] as Entity;
            if (context.MessageName == "Create")
            {
                traceService.Trace("Vào Plugin_Payment_Import");
                if (!target.Contains("bsd_checkimport"))
                {
                    traceService.Trace("Vào Plugin_Payment_Import 1");
                    OptionSetValue Payment_Type = target.Contains("bsd_paymenttype") ? (OptionSetValue)target["bsd_paymenttype"] : null;
                    bool bsd_createbyrevert = target.Contains("bsd_createbyrevert") ? (bool)target["bsd_createbyrevert"] : false;
                    if (bsd_createbyrevert == false)
                    {
                        if (Payment_Type != null)
                        {
                            if (Payment_Type.Value == 100000001)//Deposit fee
                            {
                                if (target.Contains("bsd_units"))
                                {

                                    Entity en_unit = service.Retrieve(((EntityReference)target["bsd_units"]).LogicalName, ((EntityReference)target["bsd_units"]).Id, new ColumnSet(true));
                                    var fetchXml = $@"<?xml version=""1.0"" encoding=""utf-16""?>
                                <fetch>
                                  <entity name=""quote"">
                                    <attribute name=""quoteid"" />
                                    <attribute name=""customerid"" />
                                    <attribute name=""bsd_depositfee"" />
                                    <filter>
                                      <condition attribute=""bsd_unitno"" operator=""eq"" value=""{en_unit.Id}""/>
                                    </filter>
                                    <filter>
                                      <condition attribute=""statuscode"" operator=""in"">
                                        <value>{100000000}</value>
                                        <value>{100000006}</value>
                                      </condition>
                                    </filter>
                                  </entity>
                                </fetch>";
                                    EntityCollection rs = service.RetrieveMultiple(new FetchExpression(fetchXml));
                                    if (rs.Entities.Count > 0)
                                    {
                                        decimal amount_pay = target.Contains("bsd_amountpay") ? ((Money)target["bsd_amountpay"]).Value : 0;
                                        target["bsd_purchaser"] = rs.Entities[0]["customerid"];
                                        target["bsd_reservation"] = rs.Entities[0].ToEntityReference();
                                        decimal deposited = ((Money)rs.Entities[0]["bsd_depositfee"]).Value;
                                        target["bsd_totalamountpayablephase"] = deposited;
                                        target["bsd_differentamount"] = amount_pay - deposited;
                                        service.Update(target);
                                    }
                                    else
                                    {
                                        throw new InvalidPluginExecutionException("Giao dịch không thỏa tình trạng cho phép vào tiền đặt cọc.");
                                    }

                                }
                            }
                            else if (Payment_Type.Value == 100000002) //instalment
                            {
                                traceService.Trace("Vào case_instalment");
                                if (target.Contains("bsd_units") && target.Contains("bsd_odernumber"))
                                {
                                    int num = target.Contains("bsd_odernumber") ? (int)target["bsd_odernumber"] : 0;
                                    traceService.Trace("mun= " + num);
                                    Entity en_unit = service.Retrieve(((EntityReference)target["bsd_units"]).LogicalName, ((EntityReference)target["bsd_units"]).Id, new ColumnSet(true));
                                    var fetch_oe = $@"<?xml version=""1.0"" encoding=""utf-16""?>
                                    <fetch>
                                      <entity name=""salesorder"">
                                        <attribute name=""customerid"" />
                                        <attribute name=""bsd_paymentscheme"" />
                                        <filter>
                                          <condition attribute=""bsd_unitnumber"" operator=""eq"" value=""{en_unit.Id}""/>
                                        </filter>
                                        <filter>
                                          <condition attribute=""statuscode"" operator=""in"">
                                            <value>{100000000}</value>
                                            <value>{100000001}</value>
                                            <value>{100000002}</value>
                                            <value>{100000003}</value>
                                            <value>{100000004}</value>
                                            <value>{100000005}</value>
                                          </condition>
                                        </filter>
                                      </entity>
                                    </fetch>";
                                    EntityCollection rs_oe = service.RetrieveMultiple(new FetchExpression(fetch_oe));
                                    if (rs_oe.Entities.Count > 0)
                                    {
                                        traceService.Trace("saleoder > 0");

                                        Entity oe = service.Retrieve("salesorder", rs_oe.Entities[0].Id, new ColumnSet(true));

                                        var fetchXml_instalment = $@"<?xml version=""1.0"" encoding=""utf-16""?>
                                        <fetch>
                                          <entity name=""bsd_paymentschemedetail"">
                                            <attribute name=""bsd_paymentschemedetailid"" />
                                            <attribute name=""bsd_duedate"" />
                                            <attribute name=""bsd_amountofthisphase"" />
                                            <attribute name=""bsd_amountwaspaid"" />
                                            <attribute name=""bsd_waiverinstallment"" />
                                            <attribute name=""bsd_ordernumber"" />
                                            <attribute name=""bsd_balance"" />
                                            <attribute name=""bsd_paymentscheme"" />
                                            <attribute name=""bsd_depositamount"" />
                                            <attribute name=""bsd_interestchargeper"" />
                                            <attribute name=""bsd_gracedays"" />
                                            <filter>
                                              <condition attribute=""bsd_optionentry"" operator=""eq"" value=""{rs_oe.Entities[0].Id}"" />
                                              <condition attribute=""statuscode"" operator=""eq"" value=""{100000000}"" />
                                              <condition attribute=""bsd_ordernumber"" operator=""eq"" value=""{num}"" />
                                            </filter>
                                          </entity>
                                        </fetch>";
                                        EntityCollection rs = service.RetrieveMultiple(new FetchExpression(fetchXml_instalment));
                                        traceService.Trace("instalment" + rs.Entities.Count);
                                        if (rs.Entities.Count == 0)
                                        {
                                            throw new InvalidPluginExecutionException("Đợt thanh toán cần thực hiện không thỏa yêu cầu.");
                                        }
                                        else
                                        {
                                            decimal deposited = rs.Entities[0].Contains("bsd_depositamount") ? ((Money)rs.Entities[0]["bsd_depositamount"]).Value : 0;
                                            int bsd_ordernumber = rs.Entities[0].Contains("bsd_ordernumber") ? (int)rs.Entities[0]["bsd_ordernumber"] : 0;
                                            int bsd_gracedays = rs.Entities[0].Contains("bsd_gracedays") ? (int)rs.Entities[0]["bsd_gracedays"] : 0;
                                            decimal Termsinterest = rs[0].Contains("bsd_interestchargeper") ? (decimal)rs[0]["bsd_interestchargeper"] : 0;
                                            Termsinterest = Math.Round(Termsinterest, 3);
                                            traceService.Trace("tien lãi111" + Termsinterest);
                                            traceService.Trace("vào else s");
                                            EntityReference paymentscheme = rs_oe.Entities[0].Contains("bsd_paymentscheme") ? (EntityReference)rs_oe.Entities[0]["bsd_paymentscheme"] : null;
                                            traceService.Trace("lấy paymentscheme" + paymentscheme);
                                            target["bsd_optionentry"] = rs_oe.Entities[0].ToEntityReference();
                                            target["bsd_purchaser"] = rs_oe.Entities[0]["customerid"];
                                            traceService.Trace("ins1");
                                            if (!rs.Entities[0].Contains("bsd_duedate"))
                                            {
                                                throw new InvalidPluginExecutionException("Đợt thanh toán không có ngày đến hạn, vui lòng cập nhật ngày đến hạn trước.");
                                            }
                                            Entity paymentschemedetail = new Entity(rs.Entities[0].LogicalName, rs.Entities[0].Id);
                                            target["bsd_paymentschemedetail"] = paymentschemedetail.ToEntityReference();
                                            DateTime? Duedate_instlment = rs.Entities[0].Contains("bsd_duedate") && rs.Entities[0]["bsd_duedate"] != null ? ((DateTime?)rs.Entities[0]["bsd_duedate"])?.AddHours(7) : (DateTime?)null;  // Nếu không có giá trị, gán cho null
                                            target["bsd_duedateinstallment"] = Duedate_instlment;

                                            decimal amoutPayable = ((Money)rs.Entities[0]["bsd_amountofthisphase"]).Value;
                                            decimal amoutPaid = ((Money)rs.Entities[0]["bsd_amountwaspaid"]).Value;

                                            decimal waiver = ((Money)rs.Entities[0]["bsd_waiverinstallment"]).Value;
                                            target["bsd_totalamountpayablephase"] = amoutPayable;
                                            target["bsd_totalamountpaidphase"] = amoutPaid;
                                            target["bsd_waiverinstallment"] = waiver;
                                            target["bsd_depositamount"] = rs.Entities.Count > 0 && rs.Entities[0].Contains("bsd_depositamount") ? rs.Entities[0]["bsd_depositamount"] : new Money(0);
                                            traceService.Trace("ins2");
                                            decimal amount_pay = target.Contains("bsd_amountpay") ? ((Money)target["bsd_amountpay"]).Value : 0;
                                            traceService.Trace("amount_pay" + amount_pay);
                                            target["bsd_balance"] = amoutPayable - amoutPaid - deposited - waiver;
                                            var balane = amoutPayable - amoutPaid - deposited - waiver;
                                            target["bsd_differentamount"] = amount_pay - balane;
                                            DateTime Receipt_date = RetrieveLocalTimeFromUTCTime((DateTime)target["bsd_paymentactualtime"]);
                                            DateTime bsd_duedateDate = RetrieveLocalTimeFromUTCTime((DateTime)rs.Entities[0]["bsd_duedate"]);
                                            traceService.Trace("Duedate_instlment " + bsd_duedateDate);
                                            traceService.Trace("Receipt_date " + Receipt_date);
                                            TimeSpan difference = Receipt_date - bsd_duedateDate;
                                            int bsd_latedays = difference.Days - bsd_gracedays;
                                            traceService.Trace("bsd_latedays " + bsd_latedays);
                                            bsd_latedays = bsd_latedays < 0 ? 0 : bsd_latedays;
                                            traceService.Trace("bsd_latedays " + bsd_latedays);
                                            int orderNumberSightContract = getViTriDotSightContract(oe.Id);
                                            traceService.Trace("orderNumberSightContract " + orderNumberSightContract);
                                            traceService.Trace("bsd_ordernumber " + bsd_ordernumber);
                                            int numberOfDays2 = 0;
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
                                                        TimeSpan difference2 = Receipt_date - bsd_signeddadate;
                                                        numberOfDays2 = difference2.Days;
                                                        numberOfDays2 = numberOfDays2 < 0 ? 0 : numberOfDays2;
                                                        traceService.Trace("bsd_signeddadate " + bsd_signeddadate);
                                                    }
                                                }
                                                else numberOfDays2 = -100599;
                                            }
                                            if (numberOfDays2 != -100599 && numberOfDays2 < bsd_latedays) bsd_latedays = numberOfDays2;
                                            traceService.Trace("bsd_latedays " + bsd_latedays);
                                            if (amount_pay > balane)
                                            {
                                                traceService.Trace("amount_pay > balane");
                                                target["bsd_latedays"] = bsd_latedays;
                                                decimal tienlai = Termsinterest / 100;
                                                traceService.Trace("Termsinterest" + Termsinterest);
                                                traceService.Trace("tien lãi" + tienlai);
                                                decimal total = bsd_latedays * tienlai * balane;
                                                target["bsd_interestcharge"] = new Money(total);
                                            }
                                            else
                                            {
                                                traceService.Trace("amount_pay < balane");
                                                target["bsd_latedays"] = bsd_latedays;
                                                decimal tienlai = Termsinterest / 100;
                                                traceService.Trace("Termsinterest" + Termsinterest);
                                                traceService.Trace("tien lãi 2" + tienlai);
                                                decimal total = bsd_latedays * tienlai * amount_pay;
                                                target["bsd_interestcharge"] = new Money(total);
                                            }
                                            traceService.Trace("hết instalment");
                                            service.Update(target);
                                        }
                                    }

                                }
                            }
                            else if (Payment_Type.Value == 100000003)//interes chart
                            {
                                if (target.Contains("bsd_units"))
                                {
                                    int num = target.Contains("bsd_odernumber") ? (int)target["bsd_odernumber"] : 0;

                                    traceService.Trace("mun= " + num);
                                    Entity en_unit = service.Retrieve(((EntityReference)target["bsd_units"]).LogicalName, ((EntityReference)target["bsd_units"]).Id, new ColumnSet(true));
                                    var fetch_oe = $@"<?xml version=""1.0"" encoding=""utf-16""?>
                                <fetch>
                                  <entity name=""salesorder"">
                                    <attribute name=""customerid"" />
                                    <filter>
                                      <condition attribute=""bsd_unitnumber"" operator=""eq"" value=""{en_unit.Id}""/>
                                    </filter>
                                    <filter>
                                      <condition attribute=""statuscode"" operator=""in"">
                                        <value>{100000001}</value>
                                        <value>{100000002}</value>
                                        <value>{100000003}</value>
                                        <value>{100000004}</value>
                                        <value>{100000005}</value>
                                      </condition>
                                    </filter>
                                  </entity>
                                </fetch>";
                                    EntityCollection rs_oe = service.RetrieveMultiple(new FetchExpression(fetch_oe));
                                    if (rs_oe.Entities.Count > 0)
                                    {
                                        Entity oe = service.Retrieve("salesorder", rs_oe.Entities[0].Id, new ColumnSet(true));
                                        var fetchXml = $@"<?xml version=""1.0"" encoding=""utf-16""?>
                                <fetch>
                                  <entity name=""bsd_paymentschemedetail"">
                                    <attribute name=""bsd_paymentschemedetailid"" />
                                    <attribute name=""bsd_duedate"" />
                                    <attribute name=""bsd_amountofthisphase"" />
                                    <attribute name=""bsd_amountwaspaid"" />
                                    <attribute name=""bsd_waiverinstallment"" />
                                    <attribute name=""bsd_ordernumber"" />
                                    <attribute name=""bsd_maintenancefeeremaining"" />
                                    <attribute name=""bsd_managementfeeremaining"" />
                                    <attribute name=""bsd_interestchargeremaining"" />
                                    <filter>
                                      <condition attribute=""bsd_optionentry"" operator=""eq"" value=""{oe.Id}"" />
                                      <condition attribute=""bsd_ordernumber"" operator=""eq"" value=""{num}"" />
                                      <condition attribute=""bsd_interestchargeremaining"" operator=""ne"" value=""{0}"" />
                                      <condition attribute=""bsd_interestchargeremaining"" operator=""not-null"" />
                                    </filter>
                                  </entity>
                                </fetch>";
                                        EntityCollection rs = service.RetrieveMultiple(new FetchExpression(fetchXml));
                                        if (rs.Entities.Count == 0)
                                        {
                                            throw new InvalidPluginExecutionException("Đợt thanh toán cần thực hiện không thỏa yêu cầu.");
                                        }
                                        else
                                        {
                                            decimal amount_pay = target.Contains("bsd_amountpay") ? ((Money)target["bsd_amountpay"]).Value : 0;

                                            decimal bsd_interestchargeremaining = ((Money)rs.Entities[0]["bsd_interestchargeremaining"]).Value;

                                            if (amount_pay <= bsd_interestchargeremaining)
                                            {
                                                target["bsd_optionentry"] = rs_oe.Entities[0].ToEntityReference();
                                                target["bsd_arrayinstallmentinterest"] = rs.Entities[0]["bsd_paymentschemedetailid"].ToString();
                                                target["bsd_arrayinterestamount"] = amount_pay.ToString();
                                                target["bsd_totalapplyamount"] = new Money(amount_pay);
                                                target["bsd_purchaser"] = oe.Contains("customerid") ? oe["customerid"] : null;
                                                service.Update(target);
                                            }
                                            else
                                            {
                                                throw new InvalidPluginExecutionException("Giá trị thanh toán lớn hơn giá trị còn lại cần thanh toán.");
                                            }
                                        }
                                    }
                                }
                            }
                            else if (Payment_Type.Value == 100000004) //fees
                            {
                                traceService.Trace("Vào Fees");
                                if (target.Contains("bsd_units"))
                                {
                                    Entity en_unit = service.Retrieve(((EntityReference)target["bsd_units"]).LogicalName, ((EntityReference)target["bsd_units"]).Id, new ColumnSet(true));
                                    var fetch_oe = $@"<?xml version=""1.0"" encoding=""utf-16""?>
                                <fetch>
                                  <entity name=""salesorder"">
                                    <attribute name=""customerid"" />
                                    <filter>
                                      <condition attribute=""bsd_unitnumber"" operator=""eq"" value=""{en_unit.Id}""/>
                                    </filter>
                                    <filter>
                                      <condition attribute=""statuscode"" operator=""in"">
                                        <value>{100000001}</value>
                                        <value>{100000002}</value>
                                        <value>{100000003}</value>
                                        <value>{100000004}</value>
                                        <value>{100000005}</value>
                                      </condition>
                                    </filter>
                                  </entity>
                                </fetch>";
                                    EntityCollection rs_oe = service.RetrieveMultiple(new FetchExpression(fetch_oe));
                                    if (rs_oe.Entities.Count > 0)
                                    {
                                        Entity oe = service.Retrieve("salesorder", rs_oe.Entities[0].Id, new ColumnSet(true));
                                        var fetchXml = $@"<?xml version=""1.0"" encoding=""utf-16""?>
                                <fetch>
                                  <entity name=""bsd_paymentschemedetail"">
                                    <attribute name=""bsd_paymentschemedetailid"" />
                                    <attribute name=""bsd_duedate"" />
                                    <attribute name=""bsd_amountofthisphase"" />
                                    <attribute name=""bsd_amountwaspaid"" />
                                    <attribute name=""bsd_waiverinstallment"" />
                                    <attribute name=""bsd_ordernumber"" />
                                    <attribute name=""bsd_maintenancefeeremaining"" />
                                    <attribute name=""bsd_managementfeeremaining"" />
                                    <filter>
                                      <condition attribute=""bsd_optionentry"" operator=""eq"" value=""{oe.Id}"" />
                                      <condition attribute=""statuscode"" operator=""eq"" value=""{100000000}"" />
                                      <condition attribute=""bsd_duedatecalculatingmethod"" operator=""eq"" value=""{100000002}"" />
                                    </filter>
                                  </entity>
                                </fetch>";
                                        EntityCollection rs = service.RetrieveMultiple(new FetchExpression(fetchXml));
                                        if (rs.Entities.Count == 0)
                                        {
                                            throw new InvalidPluginExecutionException("Đợt thanh toán cần thực hiện không thỏa yêu cầu.");
                                        }
                                        else
                                        {
                                            decimal amount_pay = target.Contains("bsd_amountpay") ? ((Money)target["bsd_amountpay"]).Value : 0;
                                            int bsd_typefee = target.Contains("bsd_typefee") ? ((OptionSetValue)target["bsd_typefee"]).Value : 0;
                                            decimal maitainfree = ((Money)rs.Entities[0]["bsd_maintenancefeeremaining"]).Value;
                                            decimal managementfree = ((Money)rs.Entities[0]["bsd_managementfeeremaining"]).Value;

                                            if (bsd_typefee == 100000000)
                                            {
                                                if (amount_pay <= maitainfree)
                                                {
                                                    target["bsd_optionentry"] = rs_oe.Entities[0].ToEntityReference();
                                                    target["bsd_arrayfees"] = rs.Entities[0]["bsd_paymentschemedetailid"].ToString() + "_main";
                                                    target["bsd_arrayfeesamount"] = amount_pay.ToString();
                                                    target["bsd_totalapplyamount"] = new Money(amount_pay);
                                                    target["bsd_purchaser"] = oe.Contains("customerid") ? oe["customerid"] : null;
                                                    service.Update(target);
                                                }
                                                if (amount_pay > maitainfree)
                                                {
                                                    traceService.Trace("vào if 1");
                                                    throw new InvalidPluginExecutionException("Giá trị thanh toán lớn hơn giá trị còn lại cần thanh toán.");
                                                }
                                            }
                                            else
                                            {
                                                if (amount_pay <= managementfree)
                                                {
                                                    target["bsd_optionentry"] = rs_oe.Entities[0].ToEntityReference();
                                                    target["bsd_arrayfees"] = rs.Entities[0]["bsd_paymentschemedetailid"].ToString() + "_mana";
                                                    target["bsd_arrayfeesamount"] = amount_pay.ToString();
                                                    target["bsd_totalapplyamount"] = new Money(amount_pay);
                                                    target["bsd_purchaser"] = oe.Contains("customerid") ? oe["customerid"] : null;
                                                    service.Update(target);
                                                }
                                                if (amount_pay > maitainfree)
                                                {
                                                    traceService.Trace("vào if 2");
                                                    throw new InvalidPluginExecutionException("Giá trị thanh toán lớn hơn giá trị còn lại cần thanh toán.");
                                                }
                                            }
                                        }
                                    }
                                }
                            }
                            else if (Payment_Type.Value == 100000005)//other
                            {
                                traceService.Trace("vào case other");
                                if (target.Contains("bsd_units"))
                                {
                                    int num = target.Contains("bsd_odernumber") ? (int)target["bsd_odernumber"] : 0;
                                    int num_misc = target.Contains("bsd_numbermisc") ? (int)target["bsd_numbermisc"] : 0;
                                    Entity en_unit = service.Retrieve(((EntityReference)target["bsd_units"]).LogicalName, ((EntityReference)target["bsd_units"]).Id, new ColumnSet(true));
                                    var fetch_oe = $@"<?xml version=""1.0"" encoding=""utf-16""?>
                                <fetch>
                                  <entity name=""salesorder"">
                                    <attribute name=""customerid"" />
                                    <filter>
                                      <condition attribute=""bsd_unitnumber"" operator=""eq"" value=""{en_unit.Id}""/>
                                    </filter>
                                    <filter>
                                      <condition attribute=""statuscode"" operator=""in"">
                                        <value>{100000001}</value>
                                        <value>{100000002}</value>
                                        <value>{100000003}</value>
                                        <value>{100000004}</value>
                                        <value>{100000005}</value>
                                      </condition>
                                    </filter>
                                  </entity>
                                </fetch>";
                                    EntityCollection rs_oe = service.RetrieveMultiple(new FetchExpression(fetch_oe));
                                    if (rs_oe.Entities.Count > 0)
                                    {
                                        traceService.Trace("count_oe_" + rs_oe.Entities.Count);
                                        Entity oe = service.Retrieve("salesorder", rs_oe.Entities[0].Id, new ColumnSet(true));
                                        var fetchXml = $@"<?xml version=""1.0"" encoding=""utf-16""?>
                                <fetch>
                                  <entity name=""bsd_paymentschemedetail"">
                                    <attribute name=""bsd_paymentschemedetailid"" />
                                    <attribute name=""bsd_duedate"" />
                                    <attribute name=""bsd_amountofthisphase"" />
                                    <attribute name=""bsd_amountwaspaid"" />
                                    <attribute name=""bsd_waiverinstallment"" />
                                    <attribute name=""bsd_ordernumber"" />
                                    <attribute name=""bsd_maintenancefeeremaining"" />
                                    <attribute name=""bsd_managementfeeremaining"" />
                                    <filter>
                                      <condition attribute=""bsd_optionentry"" operator=""eq"" value=""{oe.Id}"" />
                                      <condition attribute=""statuscode"" operator=""eq"" value=""{100000000}"" />
                                      <condition attribute=""bsd_ordernumber"" operator=""eq"" value=""{num}"" />
                                    </filter>
                                  </entity>
                                </fetch>";
                                        EntityCollection rs = service.RetrieveMultiple(new FetchExpression(fetchXml));
                                        if (rs.Entities.Count == 0)
                                        {
                                            throw new InvalidPluginExecutionException("Đợt thanh toán cần thực hiện không thỏa yêu cầu.");
                                        }
                                        else
                                        {
                                            traceService.Trace("count_instalment_" + rs.Entities.Count);
                                            //Entity paymentschemedetail = new Entity(rs.Entities[0].LogicalName, rs.Entities[0].Id);
                                            var fetchXml_misc = $@"<?xml version=""1.0"" encoding=""utf-16""?>
                                    <fetch>
                                      <entity name=""bsd_miscellaneous"">
                                        <attribute name=""bsd_balance"" />
                                        <attribute name=""bsd_miscellaneousid"" />
                                        <filter>
                                          <condition attribute=""bsd_installment"" operator=""eq"" value=""{rs.Entities[0].Id}""/>
                                          <condition attribute=""bsd_number"" operator=""eq"" value=""{num_misc}"" />
                                        </filter>
                                      </entity>
                                    </fetch>";
                                            EntityCollection rs_misc = service.RetrieveMultiple(new FetchExpression(fetchXml_misc));
                                            if (rs_misc.Entities.Count > 0)
                                            {
                                                traceService.Trace("count_misc_" + rs_misc.Entities.Count);
                                                decimal amount_pay = target.Contains("bsd_amountpay") ? ((Money)target["bsd_amountpay"]).Value : 0;
                                                traceService.Trace("amount_pay" + amount_pay);
                                                decimal balance = ((Money)rs_misc.Entities[0]["bsd_balance"]).Value;
                                                traceService.Trace("balance" + balance);
                                                if (amount_pay <= balance)
                                                {

                                                    traceService.Trace("amount <=balance");
                                                    target["bsd_optionentry"] = rs_oe.Entities[0].ToEntityReference();
                                                    traceService.Trace("1");
                                                    target["bsd_arraymicellaneousid"] = rs_misc.Entities[0]["bsd_miscellaneousid"].ToString();
                                                    traceService.Trace("2");
                                                    target["bsd_arraymicellaneousamount"] = amount_pay.ToString();
                                                    traceService.Trace("3");
                                                    target["bsd_totalapplyamount"] = new Money(amount_pay);
                                                    traceService.Trace("4");
                                                    target["bsd_purchaser"] = oe.Contains("customerid") ? oe["customerid"] : null;
                                                    traceService.Trace("5");
                                                    service.Update(target);
                                                }
                                                else
                                                {
                                                    throw new InvalidPluginExecutionException("Giá trị thanh toán lớn hơn giá trị còn lại cần thanh toán.");
                                                }
                                            }

                                        }
                                    }
                                }
                            }

                        }
                    }
                    traceService.Trace("end");


                }
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
        private DateTime RetrieveLocalTimeFromUTCTime(DateTime utcTime)
        {
            int? timeZoneCode = RetrieveCurrentUsersSettings(service);
            if (!timeZoneCode.HasValue)
                throw new InvalidPluginExecutionException("Can't find time zone code");
            var request = new LocalTimeFromUtcTimeRequest
            {
                TimeZoneCode = timeZoneCode.Value,
                UtcTime = utcTime.ToUniversalTime()
            };
            var response = (LocalTimeFromUtcTimeResponse)service.Execute(request);
            return response.LocalTime;
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
}
