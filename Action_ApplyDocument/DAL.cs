using Microsoft.Crm.Sdk.Messages;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Query;
using System;
using System.Linq;

namespace Action_ApplyDocument
{
    public class DAL
    {
        public info_Error FetchXML(IOrganizationService sv, string xml)
        {
            info_Error infoError = new info_Error();
            try
            {
                infoError.createMessageAdd("Begin fetch xml");
                EntityCollection entityCollection = sv.RetrieveMultiple((QueryBase)new FetchExpression(xml));
                infoError.index = 1;
                if (entityCollection.Entities.Any<Entity>())
                {
                    infoError.result = true;
                    infoError.ent_First = entityCollection.Entities[0];
                    infoError.entc = entityCollection;
                    infoError.count = entityCollection.Entities.Count;
                    infoError.createMessageAdd(string.Format("List count = {0}. Done", (object)infoError.count));
                }
                else
                {
                    infoError.result = false;
                    infoError.createMessageAdd(string.Format("List count = 0. Done"));
                }
            }
            catch (Exception ex)
            {
                infoError.result = false;
                infoError.createMessageAdd(string.Format("#Format. Index[{0}]. Error sys {1}", (object)infoError.index, (object)ex.Message));
            }
            return infoError;
        }

        public DateTime RetrieveLocalTimeFromUTCTime(IOrganizationService sv, DateTime utcTime)
        {
            LocalTimeFromUtcTimeRequest request = new LocalTimeFromUtcTimeRequest()
            {
                TimeZoneCode = (this.RetrieveCurrentUsersSettings(sv) ?? throw new InvalidPluginExecutionException("Can't find time zone code")),
                UtcTime = utcTime.ToUniversalTime()
            };
            return ((LocalTimeFromUtcTimeResponse)sv.Execute((OrganizationRequest)request)).LocalTime;
        }

        private int? RetrieveCurrentUsersSettings(IOrganizationService sv)
        {
            IOrganizationService organizationService = sv;
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
    }
}
