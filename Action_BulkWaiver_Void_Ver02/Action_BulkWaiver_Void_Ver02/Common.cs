// Decompiled with JetBrains decompiler
// Type: BSDLibrary.Common
// Assembly: Action_BulkWaiver_Void, Version=1.0.0.0, Culture=neutral, PublicKeyToken=75ea86a04a814594
// MVID: 3A65A6E8-584D-4322-BEC4-3FD1D76E2BF8
// Assembly location: C:\Users\BSD\Desktop\Action_BulkWaiver_Void.dll

using Microsoft.Crm.Sdk.Messages;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Query;
using System;
using System.Collections.ObjectModel;

namespace Action_BulkWaiver_Void_Ver02
{
    public class Common
    {
        public int Intereststartdatetype;
        public int Gracedays;
        private IOrganizationService service;

        public DateTime InterestStarDate { get; set; }

        public int LateDays { get; set; }

        public Common(IOrganizationService service) => this.service = service;
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
