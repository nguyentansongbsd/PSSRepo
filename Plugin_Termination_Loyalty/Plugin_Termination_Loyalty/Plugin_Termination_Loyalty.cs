using Microsoft.Crm.Sdk.Messages;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Query;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Plugin_Termination_Loyalty
{
    public class Plugin_Termination_Loyalty : IPlugin
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

            Entity Terminate = service.Retrieve(target.LogicalName, target.Id, new ColumnSet(true));
            int status_Terminate = ((OptionSetValue)Terminate["statuscode"]).Value;
            if (status_Terminate == 100000001)
            {
                decimal chiaChoMotPhayMot_all = 0;
                decimal chiaChoMotPhayMot = 0;
                trace.Trace("vào Plugin_Termination_Loyalty");
                EntityReference salesOrderRef = (EntityReference)Terminate["bsd_optionentry"];
                Entity salesOrder = service.Retrieve(salesOrderRef.LogicalName, salesOrderRef.Id, new ColumnSet("customerid"));
                EntityReference customerRef = (EntityReference)salesOrder["customerid"];
                Guid purchaserId = customerRef.Id;

                //throw new InvalidPluginExecutionException("test Thinh" + purchaserId);
                decimal yearCount = 3;
                //DateTime today = DateTime.UtcNow;
                DateTime today1 = RetrieveLocalTimeFromUTCTime(DateTime.UtcNow).Date;
                DateTime fromDate = today1.AddYears(-(int)Math.Floor(yearCount));
                
                //Lấy giá trị totalamount all giao dịch trên oe
                var fetchXml_all_GD = $@"<?xml version=""1.0"" encoding=""utf-16""?>
                <fetch aggregate=""true"">
                  <entity name=""salesorder"">
                    <attribute name=""totalamount"" alias=""sumtotalamount"" aggregate=""sum"" />
                    <filter type='and'>
                      <condition attribute='customerid' operator='eq' value='{purchaserId}' />
                      <condition attribute='statuscode' operator='in'>
                        <value>100000002</value>
                        <value>100000003</value>
                        <value>100000004</value>
                        <value>100000005</value>
                        <value>100001</value>
                      </condition>
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
                var fetchXml_countOrders = $@"
                <fetch distinct='false' mapping='logical' aggregate='false'>
                  <entity name='salesorder'>
                    <attribute name='salesorderid' />
                    <filter type='and'>
                      <condition attribute='customerid' operator='eq' value='{purchaserId}' />
                      <condition attribute='statuscode' operator='in'>
                        <value>100000002</value>
                        <value>100000003</value>
                        <value>100000004</value>
                        <value>100000005</value>
                        <value>100001</value>
                      </condition>
                      <condition attribute=""bsd_signedcontractdate"" operator=""on-or-after"" value=""{fromDate:yyyy-MM-dd}"" />
                      <condition attribute=""bsd_signedcontractdate"" operator=""on-or-before"" value=""{today1:yyyy-MM-dd}"" />
                    </filter>
                  </entity>
                </fetch>";
                EntityCollection orders = service.RetrieveMultiple(new FetchExpression(fetchXml_countOrders));
                int orderCount = orders.Entities.Count;
                //throw new InvalidPluginExecutionException("test Thinh" + orders.Entities.Count);
                trace.Trace("số lượng giao dịch trong 3 năm" + orderCount);
                //Lấy giá trị totalamount giao dịch trong 3 năm kể từ thời điểm hiện tại trsở về trước trên oe
                var fetchXml = $@"<?xml version=""1.0"" encoding=""utf-16""?>
                <fetch aggregate=""true"">
                  <entity name=""salesorder"">
                    <attribute name=""totalamount"" alias=""sumtotalamount"" aggregate=""sum"" />
                    <filter type='and'>
                      <condition attribute='customerid' operator='eq' value='{purchaserId}' />
                      <condition attribute='statuscode' operator='in'>
                        <value>100000002</value>
                        <value>100000003</value>
                        <value>100000004</value>
                        <value>100000005</value>
                        <value>100001</value>
                      </condition>
                      <condition attribute=""bsd_signedcontractdate"" operator=""on-or-after"" value=""{fromDate:yyyy-MM-dd}"" />
                      <condition attribute=""bsd_signedcontractdate"" operator=""on-or-before"" value=""{today1:yyyy-MM-dd}"" />
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
                              <condition attribute=""statuscode"" operator=""eq"" value=""{100000000}"" />
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
                        //EntityReference customerRef1 = (EntityReference)salesOrder["bsd_customer"];
                        Entity updateCustomer = new Entity(customerRef.LogicalName, customerRef.Id);
                        updateCustomer["bsd_membershiptier"] = membershipTierRef;
                        updateCustomer["bsd_totalamountofownership"] = new Money(chiaChoMotPhayMot_all);
                        updateCustomer["bsd_totalamountofownership3years"] = new Money(chiaChoMotPhayMot);
                        updateCustomer["bsd_loyaltystatus"] = new OptionSetValue(100000001);
                        updateCustomer["bsd_loyaltydate"] = today1;
                        updateCustomer["bsd_totaltransaction"] = orderCount;
                        service.Update(updateCustomer);
                        trace.Trace("end up contact" + chiaChoMotPhayMot);
                    }
                }
                else
                {
                    Entity updateCustomer = new Entity(customerRef.LogicalName, customerRef.Id);
                    updateCustomer["bsd_membershiptier"] = null;
                    updateCustomer["bsd_totalamountofownership"] = new Money(0);
                    updateCustomer["bsd_totalamountofownership3years"] = new Money(0);
                    updateCustomer["bsd_loyaltystatus"] = new OptionSetValue(100000001);
                    updateCustomer["bsd_loyaltydate"] = today1;
                    updateCustomer["bsd_totaltransaction"] = orderCount;
                    service.Update(updateCustomer);
                }
            }
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
}
