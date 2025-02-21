// Decompiled with JetBrains decompiler
// Type: Microsoft.Dynamics.AcceleratedSales.SandboxPlugins.Core.DataAccess.SuggestionsDataAccess
// Assembly: Microsoft.Dynamics.AcceleratedSales.SandboxPlugins, Version=1.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35
// MVID: 7BBE7132-7E30-4424-B38D-27667CFBCEFA
// Assembly location: C:\Users\ngoct\Downloads\New folder\Microsoft.Dynamics.AcceleratedSales.SandboxPlugins_1.0.0.0.dll

using Microsoft.Dynamics.AcceleratedSales.SandboxPlugins.Core.Model;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Query;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;

#nullable disable
namespace Microsoft.Dynamics.AcceleratedSales.SandboxPlugins.Core.DataAccess
{
  public class SuggestionsDataAccess
  {
    public const string Suggestion = "msdyn_salessuggestion";
    private const string SuggestionEntityName = "msdyn_salessuggestion";
    private const string SalesAccelerationInsightsEntityName = "msdyn_salesaccelerationinsight";
    private const string RelatedRecord = "msdyn_relatedrecord";
    private const string RelatedInsights = "msdyn_relatedinsights";
    private const string PotentialRevenue = "msdyn_potentialrevenue";
    private const string ExpiryDate = "msdyn_expirydate";
    private const string SuggestionReasonAttr = "msdyn_suggestionreason";
    private const string SuggestionInsights = "msdyn_insight";
    private const string SuggestionName = "msdyn_name";
    private const string SuggestionId = "msdyn_salessuggestionid";
    private const string SuggestedDateAttribute = "msdyn_suggesteddate";
    private const string PotentialRevenueBase = "msdyn_potentialrevenue_base";
    private const string TransactionCurrencyId = "transactioncurrencyid";
    private const string ExchangeRate = "exchangerate";
    private const string StateCode = "statecode";
    private const string StatusCode = "statuscode";
    private const string SalesPlay = "msdyn_salesplay";
    private const string SolutionArea = "msdyn_solutionarea";
    private const string SalesMotion = "msdyn_salesmotion";
    private const string OwnerId = "ownerid";
    private const int OpenStateCode = 0;
    private const string WorkspaceFCSNamespace = "SalesService.Workspace";
    private const string MsxViewFcs = "MSXView";
    private static readonly string[] SuggestionAttributes = new string[17]
    {
      "msdyn_potentialrevenue",
      "msdyn_expirydate",
      "msdyn_suggestionreason",
      "msdyn_insight",
      "msdyn_name",
      "msdyn_suggesteddate",
      "msdyn_potentialrevenue_base",
      "transactioncurrencyid",
      "exchangerate",
      "statecode",
      "statuscode",
      "msdyn_salesplay",
      "msdyn_solutionarea",
      "msdyn_salesmotion",
      "msdyn_salessuggestionid",
      "msdyn_relatedrecord",
      "ownerid"
    };
    private readonly IAcceleratedSalesLogger logger;
    private readonly IDataStore dataStore;

    public SuggestionsDataAccess(IAcceleratedSalesLogger logger, IDataStore dataStore)
    {
      this.dataStore = dataStore;
      this.logger = logger;
    }

    public virtual List<SuggestionsRecord> GetSuggestionRecords(
      GetUpnextDataRequestParameters getUpnextDataRequestParams)
    {
      Stopwatch.StartNew();
      try
      {
        Guid guid = Guid.Parse(getUpnextDataRequestParams.EntityRecordId);
        if (getUpnextDataRequestParams.EntityLogicalName == "msdyn_salessuggestion")
        {
          this.logger.LogWarning("SuggestionsDataAccess.PrimarySuggestionsRequired.GetSuggestionRecord: FetchBySuggestionId", callerName: nameof (GetSuggestionRecords), callerFile: "C:\\__w\\1\\s\\solutions\\AcceleratedSales\\AcceleratedSalesSandboxPlugins\\Core\\DataAccess\\SuggestionsDataAccess.cs");
          return this.GetSuggestionsById(guid);
        }
        int[] array = JsonConvert.DeserializeObject<int[]>(getUpnextDataRequestParams.AdditionalParameters?.SuggestionsOwnershipMaskFilter);
        bool flag1 = Array.Exists<int>(array, (Predicate<int>) (ownershipMask => ownershipMask == 1));
        this.logger.LogWarning("SuggestionsDataAccess.PrimarySuggestionsRequired: " + flag1.ToString(), callerName: nameof (GetSuggestionRecords), callerFile: "C:\\__w\\1\\s\\solutions\\AcceleratedSales\\AcceleratedSalesSandboxPlugins\\Core\\DataAccess\\SuggestionsDataAccess.cs");
        bool flag2 = Array.Exists<int>(array, (Predicate<int>) (ownershipMask => ownershipMask == 2));
        this.logger.LogWarning("SuggestionsDataAccess.SecondarySuggestionsRequired: " + flag2.ToString(), callerName: nameof (GetSuggestionRecords), callerFile: "C:\\__w\\1\\s\\solutions\\AcceleratedSales\\AcceleratedSalesSandboxPlugins\\Core\\DataAccess\\SuggestionsDataAccess.cs");
        if (flag1 & flag2)
        {
          this.logger.AddCustomProperty("SuggestionsDataAccess.PrimarySuggestionsRequired.Ownership", (object) "Both");
          List<SuggestionsRecord> basedOnOwnershipMask = this.GetAllSuggestionsForARecordBasedOnOwnershipMask(guid, new object[1]
          {
            (object) 2
          });
          List<SuggestionsRecord> suggestions = this.GetAllPrimaryOwnedSuggestionsForARecord(guid);
          basedOnOwnershipMask.ForEach((Action<SuggestionsRecord>) (suggestion => suggestions.Add(suggestion)));
          return suggestions;
        }
        if (flag1)
        {
          this.logger.AddCustomProperty("SuggestionsDataAccess.PrimarySuggestionsRequired.Ownership", (object) "Primary");
          return this.GetAllPrimaryOwnedSuggestionsForARecord(guid);
        }
        this.logger.AddCustomProperty("SuggestionsDataAccess.PrimarySuggestionsRequired.Ownership", (object) "Secondary");
        return this.GetAllSuggestionsForARecordBasedOnOwnershipMask(guid, new object[1]
        {
          (object) 2
        });
      }
      catch (Exception ex)
      {
        return new List<SuggestionsRecord>();
      }
    }

    public EntityCollection FetchRecords(
      string entityName,
      PaginationInfo paginationInfo = null,
      QueryFilter[] additionalFilters = null,
      Dictionary<string, HashSet<string>> additionalAttributes = null,
      QuerySort sort = null,
      int topCount = 250)
    {
      Stopwatch stopwatch = Stopwatch.StartNew();
      try
      {
        List<object> suggestionIds = new List<object>();
        QueryFilter[] array1 = additionalFilters != null ? ((IEnumerable<QueryFilter>) additionalFilters).Where<QueryFilter>((Func<QueryFilter, bool>) (queryFilter => queryFilter.EntityName == "msdyn_suggestionprincipalobjectaccess")).ToArray<QueryFilter>() : (QueryFilter[]) null;
        additionalFilters = additionalFilters != null ? ((IEnumerable<QueryFilter>) additionalFilters).Where<QueryFilter>((Func<QueryFilter, bool>) (queryFilter => queryFilter.EntityName == "msdyn_salessuggestion")).ToArray<QueryFilter>() : (QueryFilter[]) null;
        if (array1 != null && array1.Length != 0)
        {
          DataCollection<Entity> entities = new SuggestionsPOADataAccess(this.logger, this.dataStore).FetchRecords("msdyn_suggestionprincipalobjectaccess", array1, topCount: topCount).Entities;
          if (entities != null)
            entities.ToList<Entity>().ForEach((Action<Entity>) (entity => suggestionIds.Add((object) entity.GetAttributeValue<EntityReference>("msdyn_salessuggestionid").Id)));
          if (suggestionIds.Count == 0)
            return new EntityCollection();
        }
        QueryExpression expressionForSuggestions = this.GetQueryExpressionForSuggestions();
        if (suggestionIds.Count > 0)
          expressionForSuggestions.Criteria.AddCondition("msdyn_salessuggestionid", ConditionOperator.In, suggestionIds.ToArray());
        else
          expressionForSuggestions.Criteria.AddCondition("ownerid", ConditionOperator.EqualUserOrUserTeams);
        // ISSUE: explicit non-virtual call
        if (additionalAttributes != null && __nonvirtual (additionalAttributes.Count) > 0)
        {
          HashSet<string> source1;
          if (additionalAttributes.TryGetValue("msdyn_salessuggestion", out source1))
          {
            string[] array2 = source1 != null ? source1.ToArray<string>() : (string[]) null;
            if (array2 != null && array2.Length != 0)
            {
              this.logger.LogWarning("SuggestionsDataAccess.FetchRecords.AdditionalSuggestionsAttributes: " + array2?.Length.ToString(), callerName: nameof (FetchRecords), callerFile: "C:\\__w\\1\\s\\solutions\\AcceleratedSales\\AcceleratedSalesSandboxPlugins\\Core\\DataAccess\\SuggestionsDataAccess.cs");
              expressionForSuggestions.ColumnSet.AddColumns(array2);
            }
          }
          HashSet<string> source2 = new HashSet<string>();
          additionalAttributes.TryGetValue("msdyn_salesaccelerationinsight", out source2);
          string[] array3 = source2 != null ? source2.ToArray<string>() : (string[]) null;
          if (this.dataStore.IsFCSEnabled("SalesService.Workspace", "MSXView") && array3 != null && array3.Length != 0)
          {
            this.logger.LogWarning("SuggestionsDataAccess.FetchRecords.AdditionalSalesAccelerationInsightsAttributesArray: " + array3?.Length.ToString(), callerName: nameof (FetchRecords), callerFile: "C:\\__w\\1\\s\\solutions\\AcceleratedSales\\AcceleratedSalesSandboxPlugins\\Core\\DataAccess\\SuggestionsDataAccess.cs");
            expressionForSuggestions.LinkEntities.Add(this.GetSalesAccelerationInsightsLinkedEntity(array3));
          }
        }
        int num;
        if (additionalFilters != null && additionalFilters.Length != 0)
        {
          IAcceleratedSalesLogger logger = this.logger;
          num = additionalFilters.Length;
          string message = "SuggestionsDataAccess.FetchRecords.AdditionalFilters: " + num.ToString();
          logger.LogWarning(message, callerName: nameof (FetchRecords), callerFile: "C:\\__w\\1\\s\\solutions\\AcceleratedSales\\AcceleratedSalesSandboxPlugins\\Core\\DataAccess\\SuggestionsDataAccess.cs");
          foreach (QueryFilter additionalFilter in additionalFilters)
            expressionForSuggestions.Criteria.AddCondition(additionalFilter.ToConditionExpression());
        }
        if (sort != null)
          expressionForSuggestions.Orders.Add(sort.ToOrderExpression());
        expressionForSuggestions.TopCount = new int?(topCount);
        if (paginationInfo != null)
        {
          expressionForSuggestions.PageInfo.Count = paginationInfo.PageCount;
          expressionForSuggestions.PageInfo.PageNumber = paginationInfo.PageNumber;
          expressionForSuggestions.PageInfo.PagingCookie = paginationInfo.PagingCookie;
          this.logger.LogWarning(string.Format("SuggestionsDataAccess.FetchRecords.PageInfo: PageCount: {0} PageNumber: {1}", (object) expressionForSuggestions.PageInfo.Count, (object) expressionForSuggestions.PageInfo.PageNumber), callerName: nameof (FetchRecords), callerFile: "C:\\__w\\1\\s\\solutions\\AcceleratedSales\\AcceleratedSalesSandboxPlugins\\Core\\DataAccess\\SuggestionsDataAccess.cs");
        }
        EntityCollection entityCollection = this.dataStore.RetrieveMultiple(expressionForSuggestions);
        stopwatch.Stop();
        IAcceleratedSalesLogger logger1 = this.logger;
        num = entityCollection.Entities.Count;
        string message1 = "SuggestionsDataAccess.FetchRecords.Count: " + num.ToString();
        logger1.LogWarning(message1, callerName: nameof (FetchRecords), callerFile: "C:\\__w\\1\\s\\solutions\\AcceleratedSales\\AcceleratedSalesSandboxPlugins\\Core\\DataAccess\\SuggestionsDataAccess.cs");
        return entityCollection;
      }
      catch (Exception ex)
      {
        this.logger.LogError("SuggestionsDataAccess.FetchRecords.Exception", ex, callerName: nameof (FetchRecords), callerFile: "C:\\__w\\1\\s\\solutions\\AcceleratedSales\\AcceleratedSalesSandboxPlugins\\Core\\DataAccess\\SuggestionsDataAccess.cs");
        throw;
      }
      finally
      {
        stopwatch.Stop();
        this.logger.LogWarning("SuggestionsDataAccess.FetchRecords.End.Duration: " + stopwatch.ElapsedMilliseconds.ToString(), callerName: nameof (FetchRecords), callerFile: "C:\\__w\\1\\s\\solutions\\AcceleratedSales\\AcceleratedSalesSandboxPlugins\\Core\\DataAccess\\SuggestionsDataAccess.cs");
      }
    }

    private LinkEntity GetSalesAccelerationInsightsLinkedEntity(string[] attributes)
    {
      return new LinkEntity("msdyn_salessuggestion", "msdyn_salesaccelerationinsight", "msdyn_relatedrecord", "msdyn_relatedrecord", JoinOperator.LeftOuter)
      {
        Columns = new ColumnSet(attributes),
        EntityAlias = "msdyn_relatedinsights"
      };
    }

    private List<SuggestionsRecord> GetAllPrimaryOwnedSuggestionsForARecord(Guid regardingRecordId)
    {
      Stopwatch stopwatch = Stopwatch.StartNew();
      try
      {
        FilterExpression filterExpression = new FilterExpression();
        List<SuggestionsRecord> suggestionsForArecord = new List<SuggestionsRecord>();
        ConditionExpression condition = new ConditionExpression("msdyn_relatedrecord", ConditionOperator.Equal, (object) regardingRecordId);
        filterExpression.AddCondition(condition);
        filterExpression.AddCondition("statecode", ConditionOperator.Equal, (object) 0);
        filterExpression.AddCondition("msdyn_expirydate", ConditionOperator.GreaterEqual, (object) DateTime.UtcNow);
        filterExpression.AddCondition("ownerid", ConditionOperator.EqualUserOrUserTeams);
        EntityCollection entityCollection = this.dataStore.RetrieveMultiple(new QueryExpression()
        {
          EntityName = "msdyn_salessuggestion",
          ColumnSet = new ColumnSet(SuggestionsDataAccess.SuggestionAttributes),
          Criteria = filterExpression
        });
        this.logger.AddCustomProperty("SuggestionsDataAccess.GetAllPrimaryOwnedSuggestionsForARecord.Count", (object) entityCollection.Entities.Count);
        if (entityCollection.Entities.Count > 0)
        {
          foreach (Entity entity in (Collection<Entity>) entityCollection.Entities)
          {
            SuggestionsRecord suggestionRecord = this.ParseSuggestionRecord(entity, new int?(1));
            if (suggestionRecord != null)
              suggestionsForArecord.Add(suggestionRecord);
          }
        }
        stopwatch.Stop();
        this.logger.LogWarning("SuggestionsDataAccess.GetAllPrimaryOwnedSuggestionsForARecord.End.Duration: " + stopwatch.ElapsedMilliseconds.ToString(), callerName: nameof (GetAllPrimaryOwnedSuggestionsForARecord), callerFile: "C:\\__w\\1\\s\\solutions\\AcceleratedSales\\AcceleratedSalesSandboxPlugins\\Core\\DataAccess\\SuggestionsDataAccess.cs");
        this.logger.LogWarning("SuggestionsDataAccess.GetAllPrimaryOwnedSuggestionsForARecord.ProcessedCount: " + suggestionsForArecord.Count.ToString(), callerName: nameof (GetAllPrimaryOwnedSuggestionsForARecord), callerFile: "C:\\__w\\1\\s\\solutions\\AcceleratedSales\\AcceleratedSalesSandboxPlugins\\Core\\DataAccess\\SuggestionsDataAccess.cs");
        return suggestionsForArecord;
      }
      catch (Exception ex)
      {
        this.logger.LogError("SuggestionsDataAccess.GetAllPrimaryOwnedSuggestionsForARecord.Exception", ex, callerName: nameof (GetAllPrimaryOwnedSuggestionsForARecord), callerFile: "C:\\__w\\1\\s\\solutions\\AcceleratedSales\\AcceleratedSalesSandboxPlugins\\Core\\DataAccess\\SuggestionsDataAccess.cs");
        return new List<SuggestionsRecord>();
      }
    }

    private List<SuggestionsRecord> GetAllSuggestionsForARecordBasedOnOwnershipMask(
      Guid regardingRecordId,
      object[] accessRightsMaskFilter)
    {
      try
      {
        SuggestionsPOADataAccess suggestionsPoaDataAccess = new SuggestionsPOADataAccess(this.logger, this.dataStore);
        List<SuggestionsRecord> basedOnOwnershipMask = new List<SuggestionsRecord>();
        Dictionary<Guid, int> suggestionsToOwnershipMap = suggestionsPoaDataAccess.FetchSuggestionPrincipalObjectAccessRecords(accessRightsMaskFilter);
        // ISSUE: explicit non-virtual call
        if (suggestionsToOwnershipMap != null && __nonvirtual (suggestionsToOwnershipMap.Count) > 0)
          basedOnOwnershipMask = this.FetchSuggestionRecords(suggestionsToOwnershipMap, regardingRecordId);
        return basedOnOwnershipMask;
      }
      catch (Exception ex)
      {
        this.logger.LogError("SuggestionsPOADataAccess.FetchRecords.GetAllSuggestionsForARecordBasedOnOwnershipMask", ex, callerName: nameof (GetAllSuggestionsForARecordBasedOnOwnershipMask), callerFile: "C:\\__w\\1\\s\\solutions\\AcceleratedSales\\AcceleratedSalesSandboxPlugins\\Core\\DataAccess\\SuggestionsDataAccess.cs");
        return new List<SuggestionsRecord>();
      }
    }

    private List<SuggestionsRecord> FetchSuggestionRecords(
      Dictionary<Guid, int> suggestionsToOwnershipMap,
      Guid regardingRecordId)
    {
      Stopwatch stopwatch = Stopwatch.StartNew();
      List<string> stringList = new List<string>();
      foreach (KeyValuePair<Guid, int> suggestionsToOwnership in suggestionsToOwnershipMap)
        stringList.Add(suggestionsToOwnership.Key.ToString());
      FilterExpression filterExpression = new FilterExpression();
      List<SuggestionsRecord> suggestionsRecordList = new List<SuggestionsRecord>();
      filterExpression.AddCondition(new ConditionExpression("msdyn_relatedrecord", ConditionOperator.Equal, (object) regardingRecordId));
      filterExpression.AddCondition(new ConditionExpression("msdyn_salessuggestionid", ConditionOperator.In, (object[]) stringList.ToArray()));
      filterExpression.AddCondition("statecode", ConditionOperator.Equal, (object) 0);
      filterExpression.AddCondition("msdyn_expirydate", ConditionOperator.GreaterEqual, (object) DateTime.UtcNow);
      EntityCollection entityCollection = this.dataStore.RetrieveMultiple(new QueryExpression()
      {
        EntityName = "msdyn_salessuggestion",
        ColumnSet = new ColumnSet(SuggestionsDataAccess.SuggestionAttributes),
        Criteria = filterExpression
      });
      this.logger.AddCustomProperty("SuggestionsDataAccess.FetchSuggestionRecords.Count", (object) entityCollection.Entities.Count);
      if (entityCollection != null && entityCollection.Entities.Count > 0)
      {
        foreach (Entity entity in (Collection<Entity>) entityCollection.Entities)
        {
          if (suggestionsToOwnershipMap != null && entity != null && suggestionsToOwnershipMap.ContainsKey(entity.Id))
          {
            SuggestionsRecord suggestionRecord = this.ParseSuggestionRecord(entity, new int?(suggestionsToOwnershipMap[entity.Id]));
            if (suggestionRecord != null)
              suggestionsRecordList.Add(suggestionRecord);
          }
        }
      }
      stopwatch.Stop();
      this.logger.AddCustomProperty("SuggestionsDataAccess.FetchSuggestionRecords.Duration", (object) stopwatch.ElapsedMilliseconds);
      this.logger.AddCustomProperty("SuggestionsDataAccess.FetchSuggestionRecords.ProcessedCount", (object) suggestionsRecordList.Count);
      this.logger.AddCustomProperty("SuggestionsDataAccess.FetchSuggestionRecords", (object) "Success");
      return suggestionsRecordList;
    }

    private List<SuggestionsRecord> GetSuggestionsById(Guid suggestionId)
    {
      try
      {
        FilterExpression filterExpression = new FilterExpression();
        filterExpression.AddCondition("msdyn_salessuggestionid", ConditionOperator.Equal, (object) suggestionId);
        EntityCollection entityCollection = this.dataStore.RetrieveMultiple(new QueryExpression()
        {
          EntityName = "msdyn_salessuggestion",
          ColumnSet = new ColumnSet(SuggestionsDataAccess.SuggestionAttributes),
          Criteria = filterExpression
        });
        List<SuggestionsRecord> suggestionsById = new List<SuggestionsRecord>();
        if (entityCollection != null && entityCollection.Entities.Count > 0)
        {
          SuggestionsRecord suggestionRecord = this.ParseSuggestionRecord(entityCollection.Entities[0], new int?());
          if (suggestionRecord != null)
            suggestionsById.Add(suggestionRecord);
        }
        this.logger.AddCustomProperty("SuggestionsDataAccess.GetSuggestionsById", (object) "Success");
        return suggestionsById;
      }
      catch (Exception ex)
      {
        this.logger.LogError("SuggestionsDataAccess.GetSuggestionsById.Exception", ex, callerName: nameof (GetSuggestionsById), callerFile: "C:\\__w\\1\\s\\solutions\\AcceleratedSales\\AcceleratedSalesSandboxPlugins\\Core\\DataAccess\\SuggestionsDataAccess.cs");
        return new List<SuggestionsRecord>();
      }
    }

    private SuggestionsRecord ParseSuggestionRecord(Entity suggestionRecord, int? ownershipMask)
    {
      try
      {
        EntityReference attributeValue = suggestionRecord.TryGetAttributeValue<EntityReference>("ownerid", new EntityReference());
        string str;
        suggestionRecord.FormattedValues.TryGetValue("msdyn_potentialrevenue", out str);
        return new SuggestionsRecord()
        {
          SuggestionId = suggestionRecord.TryGetAttributeValue<Guid>("msdyn_salessuggestionid", Guid.Empty),
          PotentialRevenue = suggestionRecord.TryGetAttributeValue<Money>("msdyn_potentialrevenue", new Money(0M))?.Value,
          PotentialRevenueFormatted = str,
          SuggestionReason = suggestionRecord.TryGetAttributeValue<string>("msdyn_suggestionreason", string.Empty),
          SuggestionsInsights = suggestionRecord.TryGetAttributeValue<string>("msdyn_insight", string.Empty),
          SuggestionName = suggestionRecord.TryGetAttributeValue<string>("msdyn_name", string.Empty),
          ExpiryDate = suggestionRecord.TryGetAttributeValue<DateTime>("msdyn_expirydate", new DateTime()),
          SuggestedDate = suggestionRecord.TryGetAttributeValue<DateTime>("msdyn_suggesteddate", new DateTime()),
          StateCode = suggestionRecord.TryGetAttributeValue<OptionSetValue>("statecode", new OptionSetValue()).Value,
          StatusCode = suggestionRecord.TryGetAttributeValue<OptionSetValue>("statuscode", new OptionSetValue()).Value,
          OwnerId = attributeValue?.Id,
          OwnerName = attributeValue?.Name,
          OwnershipMask = ownershipMask
        };
      }
      catch (Exception ex)
      {
        this.logger.LogError("SuggestionsDataAccess.ParseSuggestionRecord", ex, callerName: nameof (ParseSuggestionRecord), callerFile: "C:\\__w\\1\\s\\solutions\\AcceleratedSales\\AcceleratedSalesSandboxPlugins\\Core\\DataAccess\\SuggestionsDataAccess.cs");
      }
      return (SuggestionsRecord) null;
    }

    private QueryExpression GetQueryExpressionForSuggestions()
    {
      QueryExpression expressionForSuggestions = new QueryExpression()
      {
        EntityName = "msdyn_salessuggestion"
      };
      expressionForSuggestions.ColumnSet.AddColumns(SuggestionsDataAccess.SuggestionAttributes);
      FilterExpression childFilter = new FilterExpression();
      childFilter.AddCondition("statecode", ConditionOperator.Equal, (object) 0);
      childFilter.AddCondition("msdyn_expirydate", ConditionOperator.GreaterEqual, (object) DateTime.UtcNow);
      expressionForSuggestions.Criteria.AddFilter(childFilter);
      OrderExpression orderExpression = new OrderExpression()
      {
        AttributeName = "msdyn_expirydate",
        OrderType = OrderType.Descending
      };
      expressionForSuggestions.Orders.Add(orderExpression);
      return expressionForSuggestions;
    }
  }
}
