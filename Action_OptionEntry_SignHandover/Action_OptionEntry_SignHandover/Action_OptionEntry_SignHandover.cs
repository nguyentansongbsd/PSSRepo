using Microsoft.Crm.Sdk.Messages;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Query;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Action_OptionEntry_SignHandover
{
    public class Action_OptionEntry_SignHandover : IPlugin
    {
        ITracingService tracingService = null;
        IPluginExecutionContext context = null;
        IOrganizationServiceFactory serviceFactory = null;
        IOrganizationService service = null;

        EntityReference target = null;
        public void Execute(IServiceProvider serviceProvider)
        {
            tracingService = (ITracingService)serviceProvider.GetService(typeof(ITracingService));
            context = (IPluginExecutionContext)serviceProvider.GetService(typeof(IPluginExecutionContext));
            serviceFactory = (IOrganizationServiceFactory)serviceProvider.GetService(typeof(IOrganizationServiceFactory));
            service = serviceFactory.CreateOrganizationService(context.UserId);
            target = (EntityReference)context.InputParameters["Target"];

            var date = (string)context.InputParameters["selectDate"];
            
            updateOptionEntry(date);
        }
        private void updateOptionEntry(string date)
        {
            tracingService.Trace("Start update option entry");
            SetStateRequest setStateRequest = new SetStateRequest
            {
                EntityMoniker = new EntityReference(this.target.LogicalName, this.target.Id),
                State = new OptionSetValue(3),
                Status = new OptionSetValue(100001)
            };
            service.Execute(setStateRequest);

            Entity enOptionEntry = new Entity(this.target.LogicalName, this.target.Id);
            enOptionEntry["bsd_actualhandoverdate"] = Convert.ToDateTime(date);
            this.service.Update(enOptionEntry);
            tracingService.Trace("End update option entry");
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
