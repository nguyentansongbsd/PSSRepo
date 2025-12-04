using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Query;
using System;

namespace Plugin_Subsale_Loyalty
{
    public class Plugin_Subsale_Loyalty : IPlugin
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

            Entity subsale = service.Retrieve(target.LogicalName, target.Id, new ColumnSet(true));
            int status_subsale = ((OptionSetValue)subsale["statuscode"]).Value;

            if (status_subsale == 100000001)
            {
                trace.Trace("Vào Plugin_Subsale_Loyalty");

                // Chạy loyalty cho cả current customer và new customer nếu có
                if (subsale.Contains("bsd_currentcustomer"))
                {
                    trace.Trace("Xử lý loyalty cho current customer");
                    ProcessLoyalty((EntityReference)subsale["bsd_currentcustomer"]);
                }

                if (subsale.Contains("bsd_newcustomer"))
                {
                    trace.Trace("Xử lý loyalty cho new customer");
                    ProcessLoyalty((EntityReference)subsale["bsd_newcustomer"]);
                }
            }
        }

        private void ProcessLoyalty(EntityReference customerRef)
        {
            Guid purchaserId = customerRef.Id;
            decimal chiaChoMotPhayMot_all = 0;
            decimal chiaChoMotPhayMot = 0;
            decimal yearCount = 3;

            DateTime today = DateTime.UtcNow;
            DateTime fromDate = today.AddYears(-(int)Math.Floor(yearCount));

            // Lấy tổng amount tất cả giao dịch
            var fetchXml_all = $@"
                <fetch aggregate='true'>
                  <entity name='salesorder'>
                    <attribute name='totalamount' alias='sumtotalamount' aggregate='sum' />
                    <filter type='and'>
                      <condition attribute='statuscode' operator='ne' value='100000006' />
                      <condition attribute='statuscode' operator='ne' value='100000001' />
                      <condition attribute='statuscode' operator='ne' value='100000000' />
                      <condition attribute='customerid' operator='eq' value='{purchaserId}' />
                    </filter>
                  </entity>
                </fetch>";
            EntityCollection allOrders = service.RetrieveMultiple(new FetchExpression(fetchXml_all));
            if (allOrders.Entities.Count > 0 && allOrders[0].Attributes.Contains("sumtotalamount"))
            {
                var aliasedValue = allOrders[0]["sumtotalamount"] as AliasedValue;
                if (aliasedValue != null && aliasedValue.Value is Money moneyValue && moneyValue.Value != 0)
                {
                    chiaChoMotPhayMot_all = moneyValue.Value;
                }
            }

            // Đếm số lượng giao dịch trong 3 năm
            var fetchXml_count = $@"
                <fetch>
                  <entity name='salesorder'>
                    <attribute name='salesorderid' />
                    <filter type='and'>
                      <condition attribute='statuscode' operator='eq' value='100000002' />
                      <condition attribute='statuscode' operator='eq' value='100000003' />
                      <condition attribute='statuscode' operator='eq' value='100000004' />
                      <condition attribute='statuscode' operator='eq' value='100000005' />
                      <condition attribute='statuscode' operator='eq' value='100001' />
                      <condition attribute='customerid' operator='eq' value='{purchaserId}' />
                    </filter>
                  </entity>
                </fetch>";
            EntityCollection orderCountResult = service.RetrieveMultiple(new FetchExpression(fetchXml_count));
            int orderCount = orderCountResult.Entities.Count;

            // Lấy tổng amount giao dịch trong 3 năm
            var fetchXml_3y = $@"
                <fetch aggregate='true'>
                  <entity name='salesorder'>
                    <attribute name='totalamount' alias='sumtotalamount' aggregate='sum' />
                    <filter type='and'>
                      <condition attribute='statuscode' operator='ne' value='100000006' />
                      <condition attribute='statuscode' operator='ne' value='100000001' />
                      <condition attribute='statuscode' operator='ne' value='100000000' />
                      <condition attribute='customerid' operator='eq' value='{purchaserId}' />
                      <condition attribute='bsd_signedcontractdate' operator='on-or-after' value='{fromDate:yyyy-MM-dd}' />
                      <condition attribute='bsd_signedcontractdate' operator='on-or-before' value='{today:yyyy-MM-dd}' />
                    </filter>
                  </entity>
                </fetch>";
            EntityCollection orders3Y = service.RetrieveMultiple(new FetchExpression(fetchXml_3y));
            if (orders3Y.Entities.Count > 0 && orders3Y[0].Attributes.Contains("sumtotalamount") && orders3Y[0]["sumtotalamount"] != null)
            {
                AliasedValue aliasedSum = orders3Y[0]["sumtotalamount"] as AliasedValue;
                if (aliasedSum != null && aliasedSum.Value is Money totalAmountAll && totalAmountAll.Value != 0)
                {
                    chiaChoMotPhayMot = totalAmountAll.Value;
                }
            }
            // Truy xuất Loyalty Program phù hợp
            var fetchLoyalty = $@"
                <fetch top='1'>
                  <entity name='bsd_purchaserloyaltyprogram'>
                    <attribute name='bsd_purchaserloyaltyprogramid' />
                    <attribute name='bsd_membershiptier' />
                    <filter type='and'>
                      <condition attribute='bsd_beginamountcur' operator='le' value='{chiaChoMotPhayMot}' />
                      <condition attribute='bsd_endamountcur' operator='ge' value='{chiaChoMotPhayMot}' />
                      <condition attribute='statuscode' operator='eq' value='100000000' />
                    </filter>
                    <order attribute='bsd_beginamountcur' descending='false' />
                  </entity>
                </fetch>";
            EntityCollection loyaltyResults = service.RetrieveMultiple(new FetchExpression(fetchLoyalty));
            trace.Trace("fet" + fetchLoyalty);
            if (loyaltyResults.Entities.Count > 0)
            {
                Entity matchedProgram = loyaltyResults[0];
                if (matchedProgram.Contains("bsd_membershiptier"))
                {
                    trace.Trace("vào if matchedProgram");
                    EntityReference membershipTierRef = (EntityReference)matchedProgram["bsd_membershiptier"];
                    Entity updateCustomer = new Entity(customerRef.LogicalName, customerRef.Id)
                    {
                        ["bsd_membershiptier"] = membershipTierRef,
                        ["bsd_totalamountofownership"] = new Money(chiaChoMotPhayMot_all),
                        ["bsd_totalamountofownership3years"] = new Money(chiaChoMotPhayMot),
                        ["bsd_loyaltystatus"] = new OptionSetValue(100000001),
                        ["bsd_loyaltydate"] = today,
                        ["bsd_totaltransaction"] = orderCount
                    };
                    service.Update(updateCustomer);
                }
            }
            else
            {
                trace.Trace("vào else");
                Entity updateCustomer = new Entity(customerRef.LogicalName, customerRef.Id)
                {
                    ["bsd_membershiptier"] = null,
                    ["bsd_totalamountofownership"] = new Money(0),
                    ["bsd_totalamountofownership3years"] = new Money(0),
                    ["bsd_loyaltystatus"] = new OptionSetValue(100000001),
                    ["bsd_loyaltydate"] = today,
                    ["bsd_totaltransaction"] = orderCount
                };
                service.Update(updateCustomer);
            }
        }
    }
}
