using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Query;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Plugin_Loyalty_Option_Entry
{
    public class Plugin_Loyalty_Option_Entry : IPlugin
    {
        IOrganizationService service = null;
        IOrganizationServiceFactory factory = null;
        ITracingService trace = null;
        public void Execute(IServiceProvider serviceProvider)
        {
            IPluginExecutionContext context = (IPluginExecutionContext)serviceProvider.GetService(typeof(IPluginExecutionContext));
            factory = (IOrganizationServiceFactory)serviceProvider.GetService(typeof(IOrganizationServiceFactory));
            service = factory.CreateOrganizationService(context.UserId);
            trace = (ITracingService)serviceProvider.GetService(typeof(ITracingService));
            Entity target = context.InputParameters["Target"] as Entity;

            Entity OE = service.Retrieve(target.LogicalName, target.Id, new ColumnSet(true));
            //int status_oe = ((OptionSetValue)OE["statuscode"]).Value;
            //if (status_oe == 100000002)
            //{
            decimal chiaChoMotPhayMot_all = 0;
            decimal chiaChoMotPhayMot = 0;
            trace.Trace("vào Plugin_Loyalty_Option_Entry");
            Guid purchaserId = ((EntityReference)OE["customerid"]).Id;
            //throw new InvalidPluginExecutionException("test Thinh" + purchaserId);
            decimal yearCount = 3;
            DateTime today = DateTime.UtcNow;
            DateTime fromDate = today.AddYears(-(int)Math.Floor(yearCount));
            //Lấy giá trị totalamount all giao dịch trên oe theo sts và 1 trong 2 field ký có giá trị
            var fetchXml_all_GD = $@"<?xml version='1.0' encoding='utf-16'?>
                <fetch aggregate='true'>
                  <entity name='salesorder'>
                    <attribute name='totalamount' alias='sumtotalamount' aggregate='sum' />
                    <filter type='and'>
                      <condition attribute='customerid' operator='eq' value='{purchaserId}' />
                      <condition attribute='statuscode' operator='in'>
                        <value>100000002</value>
                        <value>100000003</value>
                        <value>100000004</value>
                        <value>100000005</value>
                        <value>100001</value>
                      </condition>
                      <filter type='or'>
                          <condition attribute='bsd_signeddadate' operator='not-null' />
                          <condition attribute='bsd_signedcontractdate' operator='not-null' />
                      </filter>
                    </filter>
                  </entity>
                </fetch>";
            EntityCollection sum_oe_all = service.RetrieveMultiple(new FetchExpression(fetchXml_all_GD));
            if (sum_oe_all.Entities.Count > 0 && sum_oe_all.Entities[0].Attributes.Contains("sumtotalamount"))
            {
                var aliasedValue = sum_oe_all.Entities[0]["sumtotalamount"] as AliasedValue;
                if (aliasedValue != null && aliasedValue.Value is Money moneyValue && moneyValue.Value != 0)
                {
                    chiaChoMotPhayMot_all = moneyValue.Value;
                }
            }
            // đếm số lượng giao dịch của kh này trong 3 năm 
            var fetchXml_count = $@"
                <fetch>
                  <entity name='salesorder'>
                    <attribute name='salesorderid' />
                    <filter type='or'>
                          <filter type='and'>
                              <condition attribute='customerid' operator='eq' value='{purchaserId}' />
                              <condition attribute='statuscode' operator='in'>
                                <value>100000002</value>
                                <value>100000003</value>
                                <value>100000004</value>
                                <value>100000005</value>
                                <value>100001</value>
                              </condition>
                              <condition attribute='bsd_signedcontractdate' operator='on-or-after' value='{fromDate:yyyy-MM-dd}' />
                              <condition attribute='bsd_signedcontractdate' operator='on-or-before' value='{today:yyyy-MM-dd}' />
                    </filter>
                        <filter type='and'>
                              <condition attribute='customerid' operator='eq' value='{purchaserId}' />
                              <condition attribute='statuscode' operator='in'>
                                <value>100000002</value>
                                <value>100000003</value>
                                <value>100000004</value>
                                <value>100000005</value>
                                <value>100001</value>
                              </condition>
                              <condition attribute='bsd_signeddadate' operator='on-or-after' value='{fromDate:yyyy-MM-dd}' />
                              <condition attribute='bsd_signeddadate' operator='on-or-before' value='{today:yyyy-MM-dd}' />
                        </filter>
                      </filter>
                  </entity>
                </fetch>";

            EntityCollection orders = service.RetrieveMultiple(new FetchExpression(fetchXml_count));
            int orderCount = orders.Entities.Count;

            //Lấy giá trị totalamount giao dịch trong 3 năm kể từ thời điểm hiện tại trở về trước trên oe
            var fetchXml = $@"<?xml version='1.0' encoding='utf-16'?>
                <fetch aggregate='true'>
                  <entity name='salesorder'>
                    <attribute name='totalamount' alias='sumtotalamount' aggregate='sum' />
                    <filter type='or'>
                          <filter type='and'>
                              <condition attribute='customerid' operator='eq' value='{purchaserId}' />
                              <condition attribute='statuscode' operator='in'>
                                <value>100000002</value>
                                <value>100000003</value>
                                <value>100000004</value>
                                <value>100000005</value>
                                <value>100001</value>
                              </condition>
                              <condition attribute='bsd_signedcontractdate' operator='on-or-after' value='{fromDate:yyyy-MM-dd}' />
                              <condition attribute='bsd_signedcontractdate' operator='on-or-before' value='{today:yyyy-MM-dd}' />
                    </filter>
                        <filter type='and'>
                              <condition attribute='customerid' operator='eq' value='{purchaserId}' />
                              <condition attribute='statuscode' operator='in'>
                                <value>100000002</value>
                                <value>100000003</value>
                                <value>100000004</value>
                                <value>100000005</value>
                                <value>100001</value>
                              </condition>
                              <condition attribute='bsd_signeddadate' operator='on-or-after' value='{fromDate:yyyy-MM-dd}' />
                              <condition attribute='bsd_signeddadate' operator='on-or-before' value='{today:yyyy-MM-dd}' />
                        </filter>
                      </filter>
                  </entity>
                </fetch>";
            EntityCollection sum_oe = service.RetrieveMultiple(new FetchExpression(fetchXml));
            if (sum_oe.Entities.Count > 0 && sum_oe.Entities[0].Attributes.Contains("sumtotalamount"))
            {
                var aliasedValue = sum_oe.Entities[0]["sumtotalamount"] as AliasedValue;
                if (aliasedValue != null && aliasedValue.Value is Money moneyValue && moneyValue.Value != 0)
                {
                    chiaChoMotPhayMot = moneyValue.Value;
                    trace.Trace("chiaChoMotPhayMot " + chiaChoMotPhayMot);
                }
            }
            var fetchLoyaltyProgram = $@"<fetch top='1'>
                          <entity name='bsd_purchaserloyaltyprogram'>
                            <attribute name='bsd_purchaserloyaltyprogramid' />
                            <attribute name='bsd_membershiptier' />
                            <filter type='and'>
                              <condition attribute='bsd_beginamountcur' operator='le' value='{chiaChoMotPhayMot_all}' />
                              <condition attribute='bsd_endamountcur' operator='gt' value='{chiaChoMotPhayMot_all}' />
                              <condition attribute='statuscode' operator='eq' value='{100000000}' />
                            </filter>
                            <order attribute='bsd_beginamountcur' descending='false' />
                          </entity>
                        </fetch>";
            EntityCollection fetch_LoyaltyProgram = service.RetrieveMultiple(new FetchExpression(fetchLoyaltyProgram));
            if (fetch_LoyaltyProgram.Entities.Count > 0)
            {
                trace.Trace("1");
                //throw new InvalidPluginExecutionException("test" + fetch_LoyaltyProgram.Entities.Count);
                Entity matchedProgram = fetch_LoyaltyProgram.Entities[0];
                if (matchedProgram.Attributes.Contains("bsd_membershiptier"))
                {
                    EntityReference membershipTierRef = (EntityReference)matchedProgram["bsd_membershiptier"];
                    trace.Trace("2" + chiaChoMotPhayMot);
                    EntityReference customerRef = (EntityReference)OE["customerid"];
                    Entity updateCustomer = new Entity(customerRef.LogicalName, customerRef.Id);
                    updateCustomer["bsd_membershiptier"] = membershipTierRef;
                    updateCustomer["bsd_totalamountofownership"] = new Money(chiaChoMotPhayMot_all);
                    updateCustomer["bsd_totalamountofownership3years"] = new Money(chiaChoMotPhayMot);
                    updateCustomer["bsd_loyaltystatus"] = new OptionSetValue(100000001);
                    updateCustomer["bsd_loyaltydate"] = today;
                    updateCustomer["bsd_totaltransaction"] = orderCount;
                    service.Update(updateCustomer);
                    trace.Trace("end up contact" + chiaChoMotPhayMot);
                }
            }
            else
            {
                EntityReference customerRef = (EntityReference)OE["customerid"];
                Entity updateCustomer = new Entity(customerRef.LogicalName, customerRef.Id);
                updateCustomer["bsd_membershiptier"] = null;
                updateCustomer["bsd_totalamountofownership"] = new Money(0);
                updateCustomer["bsd_totalamountofownership3years"] = new Money(0);
                updateCustomer["bsd_loyaltystatus"] = new OptionSetValue(100000001);
                updateCustomer["bsd_loyaltydate"] = today;
                updateCustomer["bsd_totaltransaction"] = orderCount;
                service.Update(updateCustomer);
            }
            //}
        }
    }
}
