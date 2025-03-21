﻿using Microsoft.Crm.Sdk.Messages;
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

                            }
                        }
                        else if (Payment_Type.Value == 100000002) //instalment
                        {
                            //decimal deposited = 0;
                            traceService.Trace("Vào case_instalment");
                            //throw new InvalidPluginExecutionException("hello");
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
                                    decimal deposited = oe.Contains("bsd_depositamount") ? ((Money)oe["bsd_depositamount"]).Value : 0;
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
                                        int bsd_gracedays = 0;
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
                                        var fetchXmlpaymentscheme = $@"<?xml version=""1.0"" encoding=""utf-16""?>
                                        <fetch>
                                          <entity name=""bsd_paymentscheme"">
                                            <attribute name=""bsd_interestratemaster"" />
                                            <filter>
                                              <condition attribute=""bsd_paymentschemeid"" operator=""eq"" value=""{paymentscheme.Id}"" />
                                            </filter>
                                          </entity>
                                        </fetch>";
                                        EntityCollection rs_paymentscheme = service.RetrieveMultiple(new FetchExpression(fetchXmlpaymentscheme));
                                        if (rs_paymentscheme.Entities.Count > 0)
                                        {
                                            traceService.Trace("có paymentscheme ");
                                            EntityReference bsd_interestratemaster = rs_paymentscheme.Entities[0].Contains("bsd_interestratemaster") ? (EntityReference)rs_paymentscheme.Entities[0]["bsd_interestratemaster"] : null;
                                            var fetchXml_interestratemaster = $@"<?xml version=""1.0"" encoding=""utf-16""?>
                                            <fetch>
                                              <entity name=""bsd_interestratemaster"">
                                                <attribute name=""bsd_gracedays"" />
                                                <filter>
                                                  <condition attribute=""bsd_interestratemasterid"" operator=""eq"" value=""{bsd_interestratemaster.Id}"" />
                                                </filter>
                                              </entity>
                                            </fetch>";
                                            EntityCollection rs_interestratemaster = service.RetrieveMultiple(new FetchExpression(fetchXml_interestratemaster));
                                            if (rs_interestratemaster.Entities.Count > 0)
                                            {
                                                traceService.Trace("rs_interestratemaster > 0");
                                                bsd_gracedays = rs_interestratemaster[0].Contains("bsd_gracedays") ? (int)rs_interestratemaster[0]["bsd_gracedays"] : 0;
                                            }
                                        }
                                        Entity paymentschemedetail = new Entity(rs.Entities[0].LogicalName, rs.Entities[0].Id);
                                        target["bsd_paymentschemedetail"] = paymentschemedetail.ToEntityReference();
                                        target["bsd_duedateinstallment"] = (DateTime)rs.Entities[0]["bsd_duedate"];
                                        decimal amoutPayable = ((Money)rs.Entities[0]["bsd_amountofthisphase"]).Value;
                                        decimal amoutPaid = ((Money)rs.Entities[0]["bsd_amountwaspaid"]).Value;

                                        decimal waiver = ((Money)rs.Entities[0]["bsd_waiverinstallment"]).Value;
                                        target["bsd_totalamountpayablephase"] = amoutPayable;
                                        target["bsd_totalamountpaidphase"] = amoutPaid;
                                        target["bsd_waiverinstallment"] = waiver;
                                        target["bsd_depositamount"] = deposited;
                                        traceService.Trace("ins2");
                                        //int bsd_latedays = target.Contains("bsd_latedays") ? (int)target["bsd_latedays"] : 0;
                                        decimal amount_pay = target.Contains("bsd_amountpay") ? ((Money)target["bsd_amountpay"]).Value : 0;
                                        traceService.Trace("amount_pay" + amount_pay);
                                        //decimal amoutPayable = target.Contains("bsd_totalamountpayablephase") ? ((Money)target["bsd_totalamountpayablephase"]).Value : 0;
                                        //decimal amoutPaid = target.Contains("bsd_totalamountpaidphase") ? ((Money)target["bsd_totalamountpaidphase"]).Value : 0;
                                        //decimal deposited = target.Contains("bsd_depositamount") ? ((Money)target["bsd_depositamount"]).Value : 0;
                                        //decimal waiver = target.Contains("bsd_waiverinstallment") ? ((Money)target["bsd_waiverinstallment"]).Value : 0;
                                        target["bsd_balance"] = amoutPayable - amoutPaid - deposited - waiver;
                                        target["bsd_differentamount"] = amount_pay - amoutPayable - amoutPaid - deposited - waiver;
                                        DateTime bsd_waiverinstallmentDate = (DateTime)target["bsd_paymentactualtime"];
                                        DateTime bsd_duedateDate = (DateTime)rs.Entities[0]["bsd_duedate"];
                                        TimeSpan difference = bsd_waiverinstallmentDate - bsd_duedateDate;
                                        int numberOfDays = difference.Days;
                                        target["bsd_latedays"] = numberOfDays - bsd_gracedays;
                                        decimal interestCharge = amount_pay * (decimal)numberOfDays * 0.0005m;
                                        target["bsd_interestcharge"] = new Money(interestCharge);
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
        private DateTime RetrieveLocalTimeFromUTCTime(DateTime utcTime, IOrganizationService service)
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
