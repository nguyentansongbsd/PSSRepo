// Decompiled with JetBrains decompiler
// Type: BSDLibrary.Common
// Assembly: Action_SignedContract, Version=1.0.0.0, Culture=neutral, PublicKeyToken=91af1975bd46f505
// MVID: 64A057F8-04D7-4937-A84E-D4EF3DDC89DB
// Assembly location: C:\Users\ngoct\Downloads\Action_SignedContract_1.0.0.0.dll

using Microsoft.Crm.Sdk.Messages;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Query;
using System;

namespace BSDLibrary
{
  public class Common
  {
    private IOrganizationService service;

    public Common(IOrganizationService service) => this.service = service;

    public DateTime RetrieveLocalTimeFromUTCTime(DateTime utcTime)
    {
      return ((LocalTimeFromUtcTimeResponse) this.service.Execute((OrganizationRequest) new LocalTimeFromUtcTimeRequest()
      {
        TimeZoneCode = this.RetrieveCurrentUsersSettings(this.service),
        UtcTime = utcTime.ToUniversalTime()
      })).LocalTime;
    }

    private int RetrieveCurrentUsersSettings(IOrganizationService service)
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
      return (int) organizationService.RetrieveMultiple((QueryBase) query).Entities[0].ToEntity<Entity>().Attributes["timezonecode"];
    }
  }
}
