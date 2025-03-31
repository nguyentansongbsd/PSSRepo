using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Query;
using Microsoft.Crm.Sdk.Messages;

namespace Action_ApplyDocument
{
    public class DAL
    {
        public info_Error FetchXML(IOrganizationService sv, string xml)
        {
            info_Error info = new info_Error();
            try
            {
                info.createMessageAdd("Begin fetch xml");
                EntityCollection entc = sv.RetrieveMultiple(new FetchExpression(@xml));
                info.index = 1;
                if (entc.Entities.Any())
                {
                    info.result = true;
                    info.ent_First = entc.Entities[0];
                    info.entc = entc;
                    info.count = entc.Entities.Count;
                    info.createMessageAdd(string.Format("List count = {0}. Done", info.count));
                }
                else
                {
                    info.result = false;
                    info.createMessageAdd(string.Format("List count = 0. Done"));
                }
            }
            catch (Exception ex)
            {
                info.result = false;
                info.createMessageAdd(string.Format("#Format. Index[{0}]. Error sys {1}", info.index, ex.Message));
            }
            return info;
        }

        public DateTime RetrieveLocalTimeFromUTCTime(IOrganizationService sv, DateTime utcTime)
        {
            int? timeZoneCode = RetrieveCurrentUsersSettings(sv);
            if (!timeZoneCode.HasValue)
                throw new InvalidPluginExecutionException("Can't find time zone code");
            var request = new LocalTimeFromUtcTimeRequest
            {
                TimeZoneCode = timeZoneCode.Value,
                UtcTime = utcTime.ToUniversalTime()
            };

            LocalTimeFromUtcTimeResponse response = (LocalTimeFromUtcTimeResponse)sv.Execute(request);
            return response.LocalTime;
        }
        private int? RetrieveCurrentUsersSettings(IOrganizationService sv)
        {
            var currentUserSettings = sv.RetrieveMultiple(
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
