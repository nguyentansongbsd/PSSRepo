using Microsoft.Crm.Sdk.Messages;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Query;
using System;
using System.Text;
namespace Action_UpdateLoyaty_contact_account
{
    public class Action_UpdateLoyaty_contact_account : IPlugin
    {

        string id = string.Empty;

        public void Execute(IServiceProvider serviceProvider)
        {
            StringBuilder trmess = new StringBuilder();
            ITracingService tracingService = (ITracingService)serviceProvider.GetService(typeof(ITracingService));
            IPluginExecutionContext context = (IPluginExecutionContext)serviceProvider.GetService(typeof(IPluginExecutionContext));
            IOrganizationServiceFactory serviceFactory = (IOrganizationServiceFactory)serviceProvider.GetService(typeof(IOrganizationServiceFactory));
            IOrganizationService service = serviceFactory.CreateOrganizationService(context.UserId);
            tracingService.Trace("Action_UpdateLoyaty_contact_account");
            id = context.InputParameters["id"].ToString();
            tracingService.Trace("1_" + id);

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
                  <condition attribute='customerid' operator='eq' value='{Guid.Parse(id)}' />
                </filter>
              </entity>
            </fetch>";

            EntityCollection allOrders = service.RetrieveMultiple(new FetchExpression(fetchXml_all));
            tracingService.Trace("FetchXml all: " + fetchXml_all);

            if (allOrders.Entities.Count > 0 && allOrders[0].Attributes.Contains("sumtotalamount"))
            {
                var aliasedValue = allOrders[0]["sumtotalamount"] as AliasedValue;
                if (aliasedValue != null && aliasedValue.Value is Money moneyValue && moneyValue.Value != 0)
                {
                    chiaChoMotPhayMot_all = moneyValue.Value;
                }
            }
            tracingService.Trace("chiaChoMotPhayMot_all = " + chiaChoMotPhayMot_all);

            tracingService.Trace("2_");
            // Đếm số lượng giao dịch trong 3 năm
            var fetchXml_count = $@"
                <fetch>
                  <entity name='salesorder'>
                    <attribute name='salesorderid' />
                    <filter type='and'>
                      <condition attribute='statuscode' operator='ne' value='100000006' />
                      <condition attribute='statuscode' operator='ne' value='100000001' />
                      <condition attribute='statuscode' operator='ne' value='100000000' />
                      <condition attribute='customerid' operator='eq' value='{Guid.Parse(id)}' />
                      <condition attribute='bsd_signedcontractdate' operator='on-or-after' value='{fromDate:yyyy-MM-dd}' />
                      <condition attribute='bsd_signedcontractdate' operator='on-or-before' value='{today:yyyy-MM-dd}' />
                    </filter>
                  </entity>
                </fetch>";
            EntityCollection orderCountResult = service.RetrieveMultiple(new FetchExpression(fetchXml_count));
            int orderCount = orderCountResult.Entities.Count;
            tracingService.Trace("3_");
            // Lấy tổng amount giao dịch trong 3 năm
            var fetchXml_3y = $@"
                <fetch aggregate='true'>
                  <entity name='salesorder'>
                    <attribute name='totalamount' alias='sumtotalamount' aggregate='sum' />
                    <filter type='and'>
                      <condition attribute='statuscode' operator='ne' value='100000006' />
                      <condition attribute='statuscode' operator='ne' value='100000001' />
                      <condition attribute='statuscode' operator='ne' value='100000000' />
                      <condition attribute='customerid' operator='eq' value='{Guid.Parse(id)}' />
                      <condition attribute='bsd_signedcontractdate' operator='on-or-after' value='{fromDate:yyyy-MM-dd}' />
                      <condition attribute='bsd_signedcontractdate' operator='on-or-before' value='{today:yyyy-MM-dd}' />
                    </filter>
                  </entity>
                </fetch>";
            EntityCollection orders3Y = service.RetrieveMultiple(new FetchExpression(fetchXml_3y));
            tracingService.Trace("xml_" + fetchXml_3y);
            if (orders3Y.Entities.Count > 0 && orders3Y[0].Attributes.Contains("sumtotalamount"))
            {
                var aliasedValue = orders3Y[0]["sumtotalamount"] as AliasedValue;
                if (aliasedValue != null && aliasedValue.Value is Money moneyValue && moneyValue.Value != 0)
                {
                    decimal totalAmount3Y = moneyValue.Value;
                    tracingService.Trace("totalAmount3Y_ " + totalAmount3Y);
                    chiaChoMotPhayMot = totalAmount3Y;
                }
            }
            tracingService.Trace("4_");
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
            if (loyaltyResults.Entities.Count > 0)
            {
                tracingService.Trace("5");
                Entity matchedProgram = loyaltyResults[0];
                if (matchedProgram.Contains("bsd_membershiptier"))
                {
                    EntityReference membershipTierRef = (EntityReference)matchedProgram["bsd_membershiptier"];
                    Entity updateCustomer = new Entity("contact", Guid.Parse(id))
                    {
                        ["bsd_membershiptier"] = membershipTierRef,
                        ["bsd_totalamountofownership"] = new Money(chiaChoMotPhayMot_all),
                        ["bsd_totalamountofownership3years"] = new Money(chiaChoMotPhayMot),
                        ["bsd_loyaltystatus"] = new OptionSetValue(100000001),
                        ["bsd_loyaltydate"] = today,
                        ["bsd_totaltransaction"] = orderCount,
                        ["bsd_uployal"] = true
                    };
                    service.Update(updateCustomer);
                    tracingService.Trace("end");
                }
            }
            else
            {
                tracingService.Trace("Vào else");
                Entity updateCustomer = new Entity("contact", Guid.Parse(id))
                {
                    ["bsd_membershiptier"] = null,
                    ["bsd_totalamountofownership"] = new Money(chiaChoMotPhayMot_all),
                    ["bsd_totalamountofownership3years"] = new Money(chiaChoMotPhayMot),
                    ["bsd_loyaltystatus"] = new OptionSetValue(100000000),
                    ["bsd_loyaltydate"] = null,
                    ["bsd_totaltransaction"] = orderCount,
                    ["bsd_uployal"] = true
                };
                service.Update(updateCustomer);
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

