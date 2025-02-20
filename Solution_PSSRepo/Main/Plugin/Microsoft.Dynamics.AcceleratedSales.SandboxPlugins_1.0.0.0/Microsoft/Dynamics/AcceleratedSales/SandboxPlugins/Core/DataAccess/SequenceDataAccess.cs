// Decompiled with JetBrains decompiler
// Type: Microsoft.Dynamics.AcceleratedSales.SandboxPlugins.Core.DataAccess.SequenceDataAccess
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
using System.Diagnostics;

#nullable disable
namespace Microsoft.Dynamics.AcceleratedSales.SandboxPlugins.Core.DataAccess
{
  public class SequenceDataAccess
  {
    public const string SequenceTargetStep = "msdyn_sequencetargetstep";
    public const string SalesCadenceTargetEntity = "msdyn_sequencetarget";
    private const string SequenceTargetStepId = "msdyn_sequencetargetstepid";
    private const string SequenceStepId = "msdyn_sequencestepid";
    private const string WaitState = "msdyn_waitstate";
    private const string ErrorState = "msdyn_errorstate";
    private const string StepName = "msdyn_name";
    private const string StepType = "msdyn_type";
    private const string StepSubType = "msdyn_subtype";
    private const string LinkedActivityId = "msdyn_linkedactivityid";
    private const string DueTime = "msdyn_duetime";
    private const string Statecode = "statecode";
    private const string SequenceTarget = "msdyn_target";
    private const string Regarding = "msdyn_regarding";
    private const string SalesCadenceTargetId = "msdyn_sequencetargetid";
    private const string ParentSequence = "msdyn_parentsequence";
    private const string OwnerId = "ownerid";
    private const string ModifiedTime = "modifiedon";
    private const string CreatedTime = "createdon";
    private const string SequenceTargetStepOperationParameter = "msdyn_operationparameter";
    private readonly IDataStore dataStore;
    private readonly IAcceleratedSalesLogger logger;
    private readonly string[] sequenceStepAttributes = new string[13]
    {
      "msdyn_sequencetargetstepid",
      "msdyn_sequencestepid",
      "msdyn_waitstate",
      "msdyn_errorstate",
      "msdyn_name",
      "msdyn_type",
      "msdyn_subtype",
      "msdyn_linkedactivityid",
      "msdyn_duetime",
      "ownerid",
      "createdon",
      "modifiedon",
      "msdyn_operationparameter"
    };
    private readonly string[] sequenceTargetAttributes = new string[4]
    {
      "msdyn_sequencetargetid",
      "msdyn_target",
      "msdyn_parentsequence",
      "msdyn_regarding"
    };

    public SequenceDataAccess(IDataStore dataStore, IAcceleratedSalesLogger logger)
    {
      this.dataStore = dataStore;
      this.logger = logger;
    }

    public static List<string> GetSequenceStepAttributes()
    {
      return new List<string>()
      {
        "createdon",
        "msdyn_operationparameter",
        "msdyn_sequencetargetstepid",
        "msdyn_sequencestepid",
        "msdyn_waitstate",
        "msdyn_errorstate",
        "msdyn_name",
        "msdyn_type",
        "msdyn_subtype",
        "msdyn_linkedactivityid",
        "msdyn_duetime",
        "msdyn_target",
        "msdyn_regarding",
        "msdyn_sequencetargetid",
        "msdyn_parentsequence",
        "modifiedon",
        "ownerid",
        "msdyn_sequenceduetime"
      };
    }

    public static string GetAvailableStepsFetchXml(string entityName, List<string> entityIDs)
    {
      string str = QueryFilterHelpers.FormInConditionString(entityIDs);
      DateTime dateTime1 = DateTime.UtcNow;
      dateTime1 = dateTime1.Date;
      DateTime dateTime2 = dateTime1.AddDays(-120.0);
      return string.Format("<fetch mapping='logical'>  \r\n        <entity name='msdyn_sequencetargetstep'>  \r\n            <all-attributes/>\r\n            <link-entity name='msdyn_sequencetarget' from='msdyn_sequencetargetid' to='msdyn_sequencetarget'>\r\n                <attribute name='msdyn_regarding' />\r\n                <filter type='and'>\r\n                    <condition attribute='statecode' operator='eq' value='0' />\r\n                    <condition attribute='msdyn_target' operator='in'>\r\n                        {0}\r\n                    </condition >\r\n                </filter>\r\n            </link-entity>\r\n            <filter type='and'>\r\n                <filter type='or'>\r\n                    <condition attribute='msdyn_type' operator='eq' value='4202' />\r\n                    <condition attribute='msdyn_type' operator='eq' value='4212' />\r\n                    <condition attribute='msdyn_type' operator='eq' value='4210' />\r\n                    <condition attribute='msdyn_type' operator='eq' value='1' />\r\n                    <condition attribute='msdyn_type' operator='eq' value='3' />\r\n                    <condition attribute='msdyn_type' operator='eq' value='4' />\r\n                    <condition attribute='msdyn_type' operator='eq' value='5' />\r\n                    <condition attribute='msdyn_type' operator='eq' value='4213' />\r\n                    <condition attribute='msdyn_type' operator='eq' value='6' />\r\n                </filter>\r\n                <condition attribute='msdyn_duetime' operator='on-or-after' value='{1}' />\r\n                <condition attribute='statecode' operator='eq' value='0' />\r\n                <condition attribute='ownerid' operator='eq-useroruserteams' />\r\n            </filter>\r\n            <order attribute='msdyn_duetime' descending='true' />\r\n        </entity>  \r\n        </fetch>", (object) str, (object) dateTime2);
    }

    public EntityCollection FetchRecords(
      string entityName,
      List<string> entityIDs,
      PaginationInfo paginationInfo = null,
      QueryFilter[] additionalFilters = null,
      string[] additionalAttributes = null,
      QuerySort sort = null)
    {
      Stopwatch stopwatch = Stopwatch.StartNew();
      try
      {
        QueryExpression queryExpression = this.dataStore.ConvertFetchXmlToQueryExpression(SequenceDataAccess.GetAvailableStepsFetchXml(entityName, entityIDs), this.logger);
        this.logger.LogWarning("SequenceDataAccess.FetchRecords.ConvertFetchXmlToQueryExpression.Entity." + entityName + ".End.Duration: " + stopwatch.ElapsedMilliseconds.ToString(), callerName: nameof (FetchRecords), callerFile: "C:\\__w\\1\\s\\solutions\\AcceleratedSales\\AcceleratedSalesSandboxPlugins\\Core\\DataAccess\\SequenceDataAccess.cs");
        if (additionalAttributes != null && additionalAttributes.Length != 0)
        {
          this.logger.LogWarning("SequenceDataAccess.FetchRecords.AdditionalAttributes: " + additionalAttributes.Length.ToString(), callerName: nameof (FetchRecords), callerFile: "C:\\__w\\1\\s\\solutions\\AcceleratedSales\\AcceleratedSalesSandboxPlugins\\Core\\DataAccess\\SequenceDataAccess.cs");
          queryExpression.ColumnSet.AddColumns(additionalAttributes);
        }
        if (additionalFilters != null && additionalFilters.Length != 0)
        {
          this.logger.LogWarning("SequenceDataAccess.FetchRecords.AdditionalFilters: " + additionalFilters.Length.ToString(), callerName: nameof (FetchRecords), callerFile: "C:\\__w\\1\\s\\solutions\\AcceleratedSales\\AcceleratedSalesSandboxPlugins\\Core\\DataAccess\\SequenceDataAccess.cs");
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
          this.logger.LogWarning(string.Format("SequenceDataAccess.FetchRecords.PageInfo PageCount: {0} PageNumber: {1}", (object) queryExpression.PageInfo.Count, (object) queryExpression.PageInfo.PageNumber), callerName: nameof (FetchRecords), callerFile: "C:\\__w\\1\\s\\solutions\\AcceleratedSales\\AcceleratedSalesSandboxPlugins\\Core\\DataAccess\\SequenceDataAccess.cs");
        }
        EntityCollection entityCollection = this.dataStore.RetrieveMultiple(queryExpression);
        stopwatch.Stop();
        this.logger.LogWarning("SequenceDataAccess.FetchRecords.Count: " + entityCollection.Entities.Count.ToString(), callerName: nameof (FetchRecords), callerFile: "C:\\__w\\1\\s\\solutions\\AcceleratedSales\\AcceleratedSalesSandboxPlugins\\Core\\DataAccess\\SequenceDataAccess.cs");
        return entityCollection;
      }
      catch (Exception ex)
      {
        this.logger.LogError("SequenceDataAccess.FetchRecords.Exception", ex, callerName: nameof (FetchRecords), callerFile: "C:\\__w\\1\\s\\solutions\\AcceleratedSales\\AcceleratedSalesSandboxPlugins\\Core\\DataAccess\\SequenceDataAccess.cs");
        throw;
      }
      finally
      {
        stopwatch.Stop();
        this.logger.LogWarning("SequenceDataAccess.FetchRecords.End.Duration: " + stopwatch.ElapsedMilliseconds.ToString(), callerName: nameof (FetchRecords), callerFile: "C:\\__w\\1\\s\\solutions\\AcceleratedSales\\AcceleratedSalesSandboxPlugins\\Core\\DataAccess\\SequenceDataAccess.cs");
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
      Stopwatch stopwatch = Stopwatch.StartNew();
      try
      {
        QueryExpression queryExpression = new QueryExpression();
        queryExpression.EntityName = entityName;
        queryExpression.ColumnSet.AddColumns(this.sequenceStepAttributes);
        queryExpression.LinkEntities.Add(this.GetSalesCadenceTargetLinkedEntity(entityList, entityIDs));
        FilterExpression childFilter = new FilterExpression();
        childFilter.AddCondition("statecode", ConditionOperator.Equal, (object) 0);
        childFilter.AddCondition("msdyn_type", ConditionOperator.In, (object) 4, (object) 3, (object) 6, (object) 4202, (object) 5, (object) 4210, (object) 1, (object) 4213, (object) 4212);
        queryExpression.Criteria.AddFilter(childFilter);
        queryExpression.Orders.Add(new OrderExpression()
        {
          AttributeName = "msdyn_duetime",
          OrderType = OrderType.Ascending
        });
        int num;
        if (additionalAttributes != null && additionalAttributes.Length != 0)
        {
          IAcceleratedSalesLogger logger = this.logger;
          num = additionalAttributes.Length;
          string message = "SequenceDataAccess.FetchRecords.AdditionalAttributes: " + num.ToString();
          logger.LogWarning(message, callerName: nameof (FetchRecords), callerFile: "C:\\__w\\1\\s\\solutions\\AcceleratedSales\\AcceleratedSalesSandboxPlugins\\Core\\DataAccess\\SequenceDataAccess.cs");
          queryExpression.ColumnSet.AddColumns(additionalAttributes);
        }
        if (additionalFilters != null && additionalFilters.Length != 0)
        {
          IAcceleratedSalesLogger logger = this.logger;
          num = additionalFilters.Length;
          string message = "SequenceDataAccess.FetchRecords.AdditionalFilters: " + num.ToString();
          logger.LogWarning(message, callerName: nameof (FetchRecords), callerFile: "C:\\__w\\1\\s\\solutions\\AcceleratedSales\\AcceleratedSalesSandboxPlugins\\Core\\DataAccess\\SequenceDataAccess.cs");
          foreach (QueryFilter additionalFilter in additionalFilters)
            queryExpression.Criteria.AddCondition(additionalFilter.ToConditionExpression());
        }
        if (sort != null)
          queryExpression.AddOrderExpression(sort);
        queryExpression.TopCount = new int?(topCount);
        EntityCollection entityCollection = this.dataStore.RetrieveMultiple(queryExpression);
        stopwatch.Stop();
        IAcceleratedSalesLogger logger1 = this.logger;
        num = entityCollection.Entities.Count;
        string message1 = "SequenceDataAccess.FetchRecords.Count: " + num.ToString();
        logger1.LogWarning(message1, callerName: nameof (FetchRecords), callerFile: "C:\\__w\\1\\s\\solutions\\AcceleratedSales\\AcceleratedSalesSandboxPlugins\\Core\\DataAccess\\SequenceDataAccess.cs");
        return entityCollection;
      }
      catch (Exception ex)
      {
        this.logger.LogError("SequenceDataAccess.FetchRecords.Exception", ex, callerName: nameof (FetchRecords), callerFile: "C:\\__w\\1\\s\\solutions\\AcceleratedSales\\AcceleratedSalesSandboxPlugins\\Core\\DataAccess\\SequenceDataAccess.cs");
        throw;
      }
      finally
      {
        stopwatch.Stop();
        this.logger.LogWarning("SequenceDataAccess.FetchRecords.End.Duration: " + stopwatch.ElapsedMilliseconds.ToString(), callerName: nameof (FetchRecords), callerFile: "C:\\__w\\1\\s\\solutions\\AcceleratedSales\\AcceleratedSalesSandboxPlugins\\Core\\DataAccess\\SequenceDataAccess.cs");
      }
    }

    private LinkEntity GetSalesCadenceTargetLinkedEntity(
      List<string> entityList,
      List<string> entityIds)
    {
      LinkEntity targetLinkedEntity = new LinkEntity("msdyn_sequencetargetstep", "msdyn_sequencetarget", "msdyn_sequencetarget", "msdyn_sequencetargetid", JoinOperator.Inner)
      {
        Columns = new ColumnSet(this.sequenceTargetAttributes),
        EntityAlias = "msdyn_sequencetarget"
      };
      FilterExpression childFilter1 = new FilterExpression();
      childFilter1.AddFilter(LogicalOperator.And);
      childFilter1.AddCondition("statecode", ConditionOperator.Equal, (object) 0);
      if (entityIds != null && entityIds.Count > 0)
      {
        FilterExpression childFilter2 = new FilterExpression()
        {
          FilterOperator = LogicalOperator.Or
        };
        List<Guid> entityIdsList = new List<Guid>();
        entityIds.ForEach((Action<string>) (entityId =>
        {
          Guid result;
          if (!Guid.TryParse(entityId, out result))
            return;
          entityIdsList.Add(result);
        }));
        if (entityIdsList.Count > 0)
          childFilter2.AddCondition(new ConditionExpression("msdyn_target", ConditionOperator.In, (ICollection) entityIdsList.ToArray()));
        targetLinkedEntity.LinkCriteria.AddFilter(childFilter2);
      }
      targetLinkedEntity.LinkCriteria.AddFilter(childFilter1);
      return targetLinkedEntity;
    }
  }
}
