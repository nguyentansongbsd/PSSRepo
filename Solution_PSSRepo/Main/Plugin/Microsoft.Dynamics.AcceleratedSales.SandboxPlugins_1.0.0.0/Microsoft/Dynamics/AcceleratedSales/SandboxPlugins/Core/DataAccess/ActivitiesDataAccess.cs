// Decompiled with JetBrains decompiler
// Type: Microsoft.Dynamics.AcceleratedSales.SandboxPlugins.Core.DataAccess.ActivitiesDataAccess
// Assembly: Microsoft.Dynamics.AcceleratedSales.SandboxPlugins, Version=1.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35
// MVID: 7BBE7132-7E30-4424-B38D-27667CFBCEFA
// Assembly location: C:\Users\ngoct\Downloads\New folder\Microsoft.Dynamics.AcceleratedSales.SandboxPlugins_1.0.0.0.dll

using Microsoft.Dynamics.AcceleratedSales.SandboxPlugins.Core.Model;
using Microsoft.Dynamics.AcceleratedSales.SandboxPlugins.Core.Services;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Query;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;

#nullable disable
namespace Microsoft.Dynamics.AcceleratedSales.SandboxPlugins.Core.DataAccess
{
  public class ActivitiesDataAccess
  {
    public const string ActivityPointer = "activitypointer";
    public const string ActivityTypeCode = "activitytypecode";
    private const string Subject = "subject";
    private const string ActivityEntityName = "activitypointer";
    private const string ScheduleStart = "scheduledstart";
    private const string ScheduleEnd = "scheduledend";
    private const string StateCode = "statecode";
    private const string RegardingObjectId = "regardingobjectid";
    private const string Description = "description";
    private const string Ownerid = "ownerid";
    private const string ActivityId = "activityid";
    private const string CreatedBy = "createdby";
    private const string OwnerId = "ownerid";
    private const string CreatedOn = "createdon";
    private const string ModifiedOn = "modifiedon";
    private const int OpenStateCode = 0;
    private const int ClosedStateCode = 1;
    private const int ScheduledStateCode = 3;
    private readonly string[] activityRecordAttributes = new string[12]
    {
      "subject",
      "scheduledstart",
      "scheduledend",
      "statecode",
      "description",
      "activitytypecode",
      "regardingobjectid",
      "activityid",
      "createdby",
      "ownerid",
      "createdon",
      "modifiedon"
    };
    private readonly IDataStore dataStore;
    private readonly IAcceleratedSalesLogger logger;

    public ActivitiesDataAccess(IDataStore dataStore, IAcceleratedSalesLogger logger)
    {
      this.dataStore = dataStore;
      this.logger = logger;
    }

    public static string GetManualActivitiesFetchXml(string entityName, List<string> entityIDs)
    {
      string str = QueryFilterHelpers.FormInConditionString(entityIDs);
      DateTime dateTime1 = DateTime.UtcNow;
      dateTime1 = dateTime1.Date;
      DateTime dateTime2 = dateTime1.AddDays(-120.0);
      return string.Format("<fetch mapping='logical'>\n            <entity name='activitypointer'>\n            <attribute name='activityid'/>\n            <attribute name='subject'/>\n            <attribute name='scheduledstart'/>\n            <attribute name='regardingobjectid'/>\n            <attribute name='scheduledend'/>\n            <attribute name='statecode'/>\n            <attribute name='regardingobjectidname' />\n            <attribute name='description' />\n            <attribute name='createdby' alias='createdbyid' />\n            <attribute name='createdbyname' alias='createdbyname' />\n            <attribute name='activitytypecode'/>\n            <attribute name='createdon'/>\n            <filter type='and'>\n                <condition attribute='regardingobjectid' operator='in'>\n                    {0}\n                </condition >\n                <filter type='or'>\n                    <condition attribute='activitytypecode' operator='eq' value='4202' />\n                    <condition attribute='activitytypecode' operator='eq' value='4210' />\n                    <condition attribute='activitytypecode' operator='eq' value='4212' />\n                    <condition attribute='activitytypecode' operator='eq' value='4201' />\n                </filter>\n                <filter type='or'>\n                    <condition attribute='statecode' operator='eq' value='0' />\n                    <condition attribute='statecode' operator='eq' value='3' />\n                </filter>\n                <filter type='or'>\n                    <condition attribute='scheduledend' operator='not-null' />\n                        <filter type='and'>\n                            <condition attribute='scheduledend' operator='null' />\n                            <condition attribute='createdon' operator='on-or-after' value='{1}' />\n                        </filter>\n                </filter>\n                <condition attribute='ownerid' operator='eq-useroruserteams' />\n            </filter>\n        </entity>\n        </fetch>", (object) str, (object) dateTime2);
    }

    public static List<string> GetManualActivitiesAttributes()
    {
      return new List<string>()
      {
        "activityid",
        "subject",
        "scheduledstart",
        "regardingobjectid",
        "scheduledend",
        "statecode",
        "regardingobjectidname",
        "description",
        "createdby",
        "createdbyname",
        "activitytypecode",
        "createdon",
        "ownerid"
      };
    }

    public EntityCollection FetchRecords(
      string entityName,
      List<string> entityIDs,
      PaginationInfo paginationInfo = null,
      QueryFilter[] additionalFilters = null,
      string[] additionalAttributes = null,
      QuerySort sort = null)
    {
      this.logger.LogWarning("ActivitiesDataAccess.FetchRecords.EntityName: " + entityName, callerName: nameof (FetchRecords), callerFile: "C:\\__w\\1\\s\\solutions\\AcceleratedSales\\AcceleratedSalesSandboxPlugins\\Core\\DataAccess\\ActivitiesDataAccess.cs");
      Stopwatch stopwatch = Stopwatch.StartNew();
      try
      {
        QueryExpression queryExpression = this.dataStore.ConvertFetchXmlToQueryExpression(ActivitiesDataAccess.GetManualActivitiesFetchXml(entityName, entityIDs), this.logger);
        this.logger.LogWarning(string.Format("ActivitiesDataAccess.FetchRecords.ConvertFetchXmlToQueryExpression.Duration: {0}", (object) stopwatch.ElapsedMilliseconds), callerName: nameof (FetchRecords), callerFile: "C:\\__w\\1\\s\\solutions\\AcceleratedSales\\AcceleratedSalesSandboxPlugins\\Core\\DataAccess\\ActivitiesDataAccess.cs");
        if (additionalAttributes != null && additionalAttributes.Length != 0)
        {
          this.logger.LogWarning(string.Format("ActivitiesDataAccess.FetchRecords.AdditionalAttributes: {0}", (object) additionalAttributes.Length), callerName: nameof (FetchRecords), callerFile: "C:\\__w\\1\\s\\solutions\\AcceleratedSales\\AcceleratedSalesSandboxPlugins\\Core\\DataAccess\\ActivitiesDataAccess.cs");
          queryExpression.ColumnSet.AddColumns(additionalAttributes);
        }
        if (additionalFilters != null && additionalFilters.Length != 0)
        {
          this.logger.LogWarning(string.Format("ActivitiesDataAccess.FetchRecords.AdditionalFilters: {0}", (object) additionalFilters.Length), callerName: nameof (FetchRecords), callerFile: "C:\\__w\\1\\s\\solutions\\AcceleratedSales\\AcceleratedSalesSandboxPlugins\\Core\\DataAccess\\ActivitiesDataAccess.cs");
          foreach (QueryFilter additionalFilter in additionalFilters)
            queryExpression.Criteria.AddCondition(additionalFilter.ToConditionExpression());
        }
        if (sort != null)
          queryExpression.Orders.Add(sort.ToOrderExpression());
        if (paginationInfo != null)
        {
          queryExpression.PageInfo.Count = paginationInfo.PageCount;
          queryExpression.PageInfo.PageNumber = paginationInfo.PageNumber;
          queryExpression.PageInfo.PagingCookie = paginationInfo.PagingCookie;
          this.logger.LogWarning(string.Format("ActivitiesDataAccess.FetchRecords.PageInfo: PageCount: {0} PageNumber: {1}", (object) queryExpression.PageInfo.Count, (object) queryExpression.PageInfo.PageNumber), callerName: nameof (FetchRecords), callerFile: "C:\\__w\\1\\s\\solutions\\AcceleratedSales\\AcceleratedSalesSandboxPlugins\\Core\\DataAccess\\ActivitiesDataAccess.cs");
        }
        EntityCollection entityCollection = this.dataStore.RetrieveMultiple(queryExpression);
        stopwatch.Stop();
        this.logger.LogWarning(string.Format("ActivitiesDataAccess.FetchRecords.Success.Count: {0}", (object) entityCollection.Entities.Count), callerName: nameof (FetchRecords), callerFile: "C:\\__w\\1\\s\\solutions\\AcceleratedSales\\AcceleratedSalesSandboxPlugins\\Core\\DataAccess\\ActivitiesDataAccess.cs");
        return entityCollection;
      }
      catch (Exception ex)
      {
        this.logger.LogError("ActivitiesDataAccess.FetchRecords.Failed.Exception", ex, callerName: nameof (FetchRecords), callerFile: "C:\\__w\\1\\s\\solutions\\AcceleratedSales\\AcceleratedSalesSandboxPlugins\\Core\\DataAccess\\ActivitiesDataAccess.cs");
        throw;
      }
      finally
      {
        stopwatch.Stop();
        this.logger.LogWarning(string.Format("ActivitiesDataAccess.FetchRecords.Success.Duration: {0}", (object) stopwatch.ElapsedMilliseconds), callerName: nameof (FetchRecords), callerFile: "C:\\__w\\1\\s\\solutions\\AcceleratedSales\\AcceleratedSalesSandboxPlugins\\Core\\DataAccess\\ActivitiesDataAccess.cs");
      }
    }

    public virtual List<ActivityRecord> GetManualActivities(
      List<Guid> records,
      List<Guid> linkedActivityIds)
    {
      Stopwatch stopwatch = Stopwatch.StartNew();
      try
      {
        EntityCollection manualActivityRecords = this.dataStore.RetrieveMultiple(this.GetActivityQueryExpression(records, linkedActivityIds, false, new object[1]
        {
          (object) 1
        }, true));
        this.logger.AddCustomProperty("ActivitiesDataAccess.manualActivityRecords.Count", (object) manualActivityRecords.Entities.Count);
        List<ActivityRecord> manualActivities = this.ParseManualActivities(manualActivityRecords);
        stopwatch.Stop();
        this.logger.LogWarning("ActivitiesDataAccess.GetManualActivities.Duration: " + stopwatch.ElapsedMilliseconds.ToString(), callerName: nameof (GetManualActivities), callerFile: "C:\\__w\\1\\s\\solutions\\AcceleratedSales\\AcceleratedSalesSandboxPlugins\\Core\\DataAccess\\ActivitiesDataAccess.cs");
        return manualActivities;
      }
      catch (Exception ex)
      {
        stopwatch.Stop();
        this.logger.LogWarning("ActivitiesDataAccess.GetManualActivities.Duration: " + stopwatch.ElapsedMilliseconds.ToString(), callerName: nameof (GetManualActivities), callerFile: "C:\\__w\\1\\s\\solutions\\AcceleratedSales\\AcceleratedSalesSandboxPlugins\\Core\\DataAccess\\ActivitiesDataAccess.cs");
        this.logger.LogError("ActivitiesDataAccess.GetManualActivities.Exception", ex, callerName: nameof (GetManualActivities), callerFile: "C:\\__w\\1\\s\\solutions\\AcceleratedSales\\AcceleratedSalesSandboxPlugins\\Core\\DataAccess\\ActivitiesDataAccess.cs");
        return new List<ActivityRecord>();
      }
    }

    public EntityCollection FetchRecords(
      string entityName,
      List<string> entityIDs,
      List<string> entityList,
      QueryFilter[] additionalFilters = null,
      string[] additionalAttributes = null,
      QuerySort sort = null,
      int topCount = 250)
    {
      this.logger.LogWarning("ActivitiesDataAccess.FetchRecords.EntityName: " + entityName, callerName: nameof (FetchRecords), callerFile: "C:\\__w\\1\\s\\solutions\\AcceleratedSales\\AcceleratedSalesSandboxPlugins\\Core\\DataAccess\\ActivitiesDataAccess.cs");
      Stopwatch stopwatch = Stopwatch.StartNew();
      try
      {
        List<Guid> entityIdList = new List<Guid>();
        entityIDs?.ForEach((Action<string>) (entityId =>
        {
          Guid result;
          if (!Guid.TryParse(entityId, out result))
            return;
          entityIdList.Add(result);
        }));
        QueryExpression activityQueryExpression = this.GetActivityQueryExpression(entityIdList, (List<Guid>) null, false);
        if (additionalAttributes != null && additionalAttributes.Length != 0)
        {
          this.logger.LogWarning(string.Format("ActivitiesDataAccess.FetchRecords.AdditionalAttributes: {0}", (object) additionalAttributes.Length), callerName: nameof (FetchRecords), callerFile: "C:\\__w\\1\\s\\solutions\\AcceleratedSales\\AcceleratedSalesSandboxPlugins\\Core\\DataAccess\\ActivitiesDataAccess.cs");
          activityQueryExpression.ColumnSet.AddColumns(additionalAttributes);
        }
        if (additionalFilters != null && additionalFilters.Length != 0)
        {
          this.logger.LogWarning(string.Format("ActivitiesDataAccess.FetchRecords.AdditionalFilters: {0}", (object) additionalFilters.Length), callerName: nameof (FetchRecords), callerFile: "C:\\__w\\1\\s\\solutions\\AcceleratedSales\\AcceleratedSalesSandboxPlugins\\Core\\DataAccess\\ActivitiesDataAccess.cs");
          foreach (QueryFilter additionalFilter in additionalFilters)
            activityQueryExpression.Criteria.AddCondition(additionalFilter.ToConditionExpression());
        }
        if (sort != null)
          activityQueryExpression.AddOrderExpression(sort);
        activityQueryExpression.TopCount = new int?(topCount);
        EntityCollection entityCollection = this.dataStore.RetrieveMultiple(activityQueryExpression);
        stopwatch.Stop();
        this.logger.LogWarning(string.Format("ActivitiesDataAccess.FetchRecords.Success.Count: {0}", (object) entityCollection.Entities.Count), callerName: nameof (FetchRecords), callerFile: "C:\\__w\\1\\s\\solutions\\AcceleratedSales\\AcceleratedSalesSandboxPlugins\\Core\\DataAccess\\ActivitiesDataAccess.cs");
        return entityCollection;
      }
      catch (Exception ex)
      {
        this.logger.LogError("ActivitiesDataAccess.FetchRecords.Failed.Exception", ex, callerName: nameof (FetchRecords), callerFile: "C:\\__w\\1\\s\\solutions\\AcceleratedSales\\AcceleratedSalesSandboxPlugins\\Core\\DataAccess\\ActivitiesDataAccess.cs");
        throw;
      }
      finally
      {
        stopwatch.Stop();
        this.logger.LogWarning(string.Format("ActivitiesDataAccess.FetchRecords.Success.Duration: {0}", (object) stopwatch.ElapsedMilliseconds), callerName: nameof (FetchRecords), callerFile: "C:\\__w\\1\\s\\solutions\\AcceleratedSales\\AcceleratedSalesSandboxPlugins\\Core\\DataAccess\\ActivitiesDataAccess.cs");
      }
    }

    private QueryExpression GetActivityQueryExpression(
      List<Guid> records,
      List<Guid> linkedActivityIds,
      bool userOwned = true,
      object[] additionalStatusCodes = null,
      bool getWithoutDueActivities = false)
    {
      QueryExpression activityQueryExpression = new QueryExpression()
      {
        EntityName = "activitypointer"
      };
      activityQueryExpression.ColumnSet.AddColumns(this.activityRecordAttributes);
      FilterExpression childFilter1 = new FilterExpression()
      {
        FilterOperator = LogicalOperator.Or
      };
      childFilter1.AddCondition("activitytypecode", ConditionOperator.In, (object) 4202, (object) 4210, (object) 4212, (object) 4201);
      FilterExpression childFilter2 = new FilterExpression()
      {
        FilterOperator = LogicalOperator.Or
      };
      List<object> objectList = new List<object>()
      {
        (object) 0,
        (object) 3
      };
      if (additionalStatusCodes != null && additionalStatusCodes.Length != 0)
        objectList.AddRange((IEnumerable<object>) additionalStatusCodes);
      childFilter2.AddCondition("statecode", ConditionOperator.In, objectList.ToArray());
      FilterExpression childFilter3 = new FilterExpression()
      {
        FilterOperator = LogicalOperator.And
      };
      childFilter3.AddFilter(childFilter2);
      childFilter3.AddFilter(childFilter1);
      if (!getWithoutDueActivities)
      {
        ConditionExpression condition = new ConditionExpression("scheduledend", ConditionOperator.NotNull);
        childFilter3.AddCondition(condition);
      }
      if (userOwned)
      {
        ConditionExpression condition = new ConditionExpression("ownerid", ConditionOperator.EqualUserOrUserTeams);
        childFilter3.AddCondition(condition);
      }
      // ISSUE: explicit non-virtual call
      if (records != null && __nonvirtual (records.Count) > 0)
      {
        ConditionExpression condition = new ConditionExpression("regardingobjectid", ConditionOperator.In, (ICollection) records.ToArray());
        childFilter3.AddCondition(condition);
      }
      // ISSUE: explicit non-virtual call
      if (linkedActivityIds != null && __nonvirtual (linkedActivityIds.Count) > 0)
        childFilter3.AddCondition(new ConditionExpression("activityid", ConditionOperator.NotIn, (ICollection) linkedActivityIds.ToArray()));
      OrderExpression orderExpression = new OrderExpression()
      {
        AttributeName = "scheduledend",
        OrderType = OrderType.Ascending
      };
      activityQueryExpression.Orders.Add(orderExpression);
      activityQueryExpression.Criteria.AddFilter(childFilter3);
      return activityQueryExpression;
    }

    private List<ActivityRecord> ParseManualActivities(EntityCollection manualActivityRecords)
    {
      List<ActivityRecord> manualActivities = new List<ActivityRecord>();
      foreach (Entity entity in (Collection<Entity>) manualActivityRecords.Entities)
      {
        Guid id = entity.TryGetAttributeValue<EntityReference>("regardingobjectid", new EntityReference()).Id;
        if (this.GetActivityFormattedRecord(entity, id) != null)
          manualActivities.Add(this.GetActivityFormattedRecord(entity, id));
      }
      return manualActivities;
    }

    private ActivityRecord GetActivityFormattedRecord(Entity activity, Guid regardingRecordId)
    {
      try
      {
        EntityReference attributeValue1 = activity.TryGetAttributeValue<EntityReference>("createdby", new EntityReference());
        EntityReference attributeValue2 = activity.TryGetAttributeValue<EntityReference>("ownerid", new EntityReference());
        return new ActivityRecord()
        {
          Subject = activity.TryGetAttributeValue<string>("subject", string.Empty),
          Description = activity.TryGetAttributeValue<string>("description", string.Empty),
          ScheduledStart = activity.TryGetAttributeValue<DateTime?>("scheduledstart", new DateTime?()),
          ScheduledEnd = activity.TryGetAttributeValue<DateTime?>("scheduledend", new DateTime?()),
          CreatedOn = activity.TryGetAttributeValue<DateTime?>("createdon", new DateTime?()),
          StateCode = activity.TryGetAttributeValue<OptionSetValue>("statecode", new OptionSetValue()).Value,
          TypeCode = activity.TryGetAttributeValue<string>("activitytypecode", string.Empty),
          RegardingRecordId = regardingRecordId,
          CreatedById = attributeValue1?.Id,
          CreatedByName = attributeValue1?.Name,
          ActivityId = activity.TryGetAttributeValue<Guid>("activityid", Guid.Empty),
          OwnedById = attributeValue2?.Id,
          OwnedByName = attributeValue2?.Name
        };
      }
      catch (Exception ex)
      {
        this.logger.LogError("ActivitiesDataAccess.GetActivityFormattedRecord", ex, callerName: nameof (GetActivityFormattedRecord), callerFile: "C:\\__w\\1\\s\\solutions\\AcceleratedSales\\AcceleratedSalesSandboxPlugins\\Core\\DataAccess\\ActivitiesDataAccess.cs");
      }
      return (ActivityRecord) null;
    }
  }
}
