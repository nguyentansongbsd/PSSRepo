using Microsoft.Crm.Sdk.Messages;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Query;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Remoting.Contexts;
using System.Runtime.Remoting.Services;
using System.Text;
using System.Threading.Tasks;

namespace Plugin_Updateestimatehandoverdate
{
    public class Plugin_Updateestimatehandoverdate : IPlugin
    {
        IPluginExecutionContext context = null;
        IOrganizationServiceFactory factory = null;
        IOrganizationService service = null;
        ITracingService tracingService = null;
        Entity en = new Entity();
        public void Execute(IServiceProvider serviceProvider)
        {
            context = serviceProvider.GetService(typeof(IPluginExecutionContext)) as IPluginExecutionContext;
            factory = (IOrganizationServiceFactory)serviceProvider.GetService(typeof(IOrganizationServiceFactory));
            service = (IOrganizationService)factory.CreateOrganizationService(context.UserId);
            tracingService = (ITracingService)serviceProvider.GetService(typeof(ITracingService));
            //get entity
            Entity entity = (Entity)context.InputParameters["Target"];
            Guid recordId = entity.Id;
            en = service.Retrieve(entity.LogicalName, entity.Id, new Microsoft.Xrm.Sdk.Query.ColumnSet(true));
            var status = ((OptionSetValue)en["statuscode"]).Value;
            tracingService.Trace("start :" + status);
            //check status
            if (status == 100000001)
            {
                var result = true;
                var rs = ExistDetail(ref result);
                tracingService.Trace("count: " + rs.Entities.Count);
                Entity enDetailUpdate = new Entity(entity.LogicalName, entity.Id);
                enDetailUpdate["bsd_processing_pa"] = true; //
                enDetailUpdate["bsd_error"] = false;
                enDetailUpdate["bsd_errordetail"] = "";
                enDetailUpdate["bsd_approvedrejectedperson"] = (object)new EntityReference("systemuser", this.context.UserId);
                enDetailUpdate["bsd_approvedrejecteddate"] = (object)RetrieveLocalTimeFromUTCTime(DateTime.Now);

                service.Update(enDetailUpdate);
                var request = new OrganizationRequest("bsd_Action_Active_Approved_Updateestimatehandoverdate_Detail");
                string listid = string.Join(",", rs.Entities.Select(x => x.Id.ToString()));
                request["listid"] = listid;
                request["idmaster"] = entity.Id.ToString();
                service.Execute(request);
            }
        }
        private int? RetrieveCurrentUsersSettings(IOrganizationService service)
        {
            IOrganizationService organizationService = service;
            QueryExpression queryExpression1 = new QueryExpression("usersettings");
            queryExpression1.ColumnSet = new ColumnSet(new string[2]
            {
        "localeid",
        "timezonecode"
            });
            QueryExpression queryExpression2 = queryExpression1;
            FilterExpression filterExpression = new FilterExpression();
            filterExpression.Conditions.Add(new ConditionExpression("systemuserid", ConditionOperator.EqualUserId));
            queryExpression2.Criteria = filterExpression;
            QueryExpression query = queryExpression1;
            return (int?)organizationService.RetrieveMultiple((QueryBase)query).Entities[0].ToEntity<Entity>().Attributes["timezonecode"];
        }

        public DateTime RetrieveLocalTimeFromUTCTime(DateTime utcTime)
        {
            return ((LocalTimeFromUtcTimeResponse)this.service.Execute((OrganizationRequest)new LocalTimeFromUtcTimeRequest()
            {
                TimeZoneCode = this.RetrieveCurrentUsersSettings(this.service) == null ? throw new InvalidPluginExecutionException("Can't find time zone code") : this.RetrieveCurrentUsersSettings(this.service).Value,
                UtcTime = utcTime.ToUniversalTime()
            })).LocalTime;
        }

        /// <summary>
        /// kiểm tra xem có danh sách chi tiết của master không 1
        /// </summary>
        public EntityCollection ExistDetail(ref bool result)
        {
            var query = new QueryExpression("bsd_updateestimatehandoverdatedetail");
            query.ColumnSet.AllColumns = true;
            query.Criteria.AddCondition("bsd_updateestimatehandoverdate", ConditionOperator.Equal, en.Id.ToString());

            var rs = service.RetrieveMultiple(query);
            if (rs.Entities.Count == 0)
            {
                var mess = "The record contains an invalid batch. Please check again.";
                tracingService.Trace(mess);
                throw new InvalidPluginExecutionException(mess);
            }
            return rs;
        }

    }
}
