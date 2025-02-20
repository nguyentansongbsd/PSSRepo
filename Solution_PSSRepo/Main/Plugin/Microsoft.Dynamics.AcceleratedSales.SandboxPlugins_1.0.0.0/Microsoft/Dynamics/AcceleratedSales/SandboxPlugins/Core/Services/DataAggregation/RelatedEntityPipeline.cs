// Decompiled with JetBrains decompiler
// Type: Microsoft.Dynamics.AcceleratedSales.SandboxPlugins.Core.Services.DataAggregation.RelatedEntityPipeline
// Assembly: Microsoft.Dynamics.AcceleratedSales.SandboxPlugins, Version=1.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35
// MVID: 7BBE7132-7E30-4424-B38D-27667CFBCEFA
// Assembly location: C:\Users\ngoct\Downloads\New folder\Microsoft.Dynamics.AcceleratedSales.SandboxPlugins_1.0.0.0.dll

using Microsoft.Dynamics.AcceleratedSales.SandboxObjectModel.Model;
using Microsoft.Dynamics.AcceleratedSales.SandboxPlugins.Core.DataAccess;
using Microsoft.Dynamics.AcceleratedSales.SandboxPlugins.Core.Model;
using Microsoft.Dynamics.AcceleratedSales.SandboxPlugins.Core.Services.Commands;
using Microsoft.Dynamics.AcceleratedSales.SandboxPlugins.Core.Services.Extensions;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Metadata;
using Microsoft.Xrm.Sdk.Query;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

#nullable disable
namespace Microsoft.Dynamics.AcceleratedSales.SandboxPlugins.Core.Services.DataAggregation
{
  public class RelatedEntityPipeline
  {
    private readonly IDataStore dataStore;
    private readonly IAcceleratedSalesLogger logger;
    private readonly IEntityMetadataProvider entityMetadataProvider;
    private readonly List<string> primaryRecordIds;
    private WorklistViewConfiguration viewConfig;
    private Dictionary<string, IDataExtension> relatedExtensions;
    private Dictionary<string, List<Entity>> relatedEntitiesMap;
    private Dictionary<string, List<Dictionary<string, string>>> relatedFormattedRecords;
    private Dictionary<string, List<QueryFilter>> queryFilters;
    private Dictionary<string, QuerySort> querySort;
    private SASettings settings;
    private List<Guid> linkedActivityIds;
    private List<string> entityList;
    private Dictionary<string, HashSet<string>> additionalAttributes;
    private bool userOwned;
    private Dictionary<string, HashSet<string>> skipFormattingAttributes;

    private RelatedEntityPipeline(
      IDataStore dataStore,
      IAcceleratedSalesLogger logger,
      IEntityMetadataProvider metadataProvider)
    {
      this.dataStore = dataStore ?? throw new ArgumentNullException(nameof (dataStore));
      this.logger = logger ?? throw new ArgumentNullException(nameof (logger));
      this.entityMetadataProvider = metadataProvider;
      this.relatedExtensions = new Dictionary<string, IDataExtension>();
      this.primaryRecordIds = new List<string>();
      this.relatedEntitiesMap = new Dictionary<string, List<Entity>>();
      this.queryFilters = new Dictionary<string, List<QueryFilter>>();
      this.querySort = new Dictionary<string, QuerySort>();
      this.linkedActivityIds = new List<Guid>();
      this.entityList = new List<string>();
      this.additionalAttributes = new Dictionary<string, HashSet<string>>();
      this.userOwned = true;
      this.skipFormattingAttributes = new Dictionary<string, HashSet<string>>();
    }

    public Dictionary<string, List<Dictionary<string, string>>> RelatedRecords
    {
      get => this.relatedFormattedRecords;
    }

    public Dictionary<string, List<Entity>> RelatedEntityMap => this.relatedEntitiesMap;

    public static RelatedEntityPipeline Create(
      IDataStore dataStore,
      IAcceleratedSalesLogger logger,
      IEntityMetadataProvider metadataProvider)
    {
      return new RelatedEntityPipeline(dataStore, logger, metadataProvider);
    }

    public RelatedEntityPipeline WithViewConfiguration(WorklistViewConfiguration config)
    {
      this.viewConfig = config ?? throw new ArgumentNullException(nameof (config));
      return this;
    }

    public RelatedEntityPipeline WithSkipFormattingAttributes(
      Dictionary<string, HashSet<string>> skipFormattingAttributes)
    {
      this.skipFormattingAttributes = skipFormattingAttributes;
      return this;
    }

    public RelatedEntityPipeline WithSettings(SASettings settings)
    {
      this.settings = settings ?? new SASettings();
      return this;
    }

    public RelatedEntityPipeline WithExtensions(
      Dictionary<string, IDataExtension> relatedExtensions)
    {
      this.relatedExtensions = relatedExtensions;
      this.logger.LogWarning(string.Format("RelatedEntityPipeline.RelatedExtensions.Count: {0}", (object) relatedExtensions.Count), callerName: nameof (WithExtensions), callerFile: "C:\\__w\\1\\s\\solutions\\AcceleratedSales\\AcceleratedSalesSandboxPlugins\\Core\\Services\\DataAggregation\\RelatedEntityPipeline.cs");
      return this;
    }

    public RelatedEntityPipeline WithPrimaryRecordIds(List<string> primaryRecordIds)
    {
      foreach (string primaryRecordId in primaryRecordIds)
        this.primaryRecordIds.Add(primaryRecordId);
      this.logger.LogWarning("RelatedEntityPipeline.PrimaryRecordIds.Count: " + primaryRecordIds.Count.ToString(), callerName: nameof (WithPrimaryRecordIds), callerFile: "C:\\__w\\1\\s\\solutions\\AcceleratedSales\\AcceleratedSalesSandboxPlugins\\Core\\Services\\DataAggregation\\RelatedEntityPipeline.cs");
      return this;
    }

    public RelatedEntityPipeline WithEntityList(List<string> entityList)
    {
      foreach (string entity in entityList)
        this.entityList.Add(entity);
      return this;
    }

    public RelatedEntityPipeline WithFilters(Dictionary<string, List<QueryFilter>> filters)
    {
      this.queryFilters = new Dictionary<string, List<QueryFilter>>();
      foreach (KeyValuePair<string, List<QueryFilter>> filter in filters)
        this.queryFilters.Add(filter.Key, filter.Value);
      return this;
    }

    public RelatedEntityPipeline WithAdditionalAttributes(
      Dictionary<string, HashSet<string>> additionalAttributes)
    {
      this.additionalAttributes = additionalAttributes ?? new Dictionary<string, HashSet<string>>();
      return this;
    }

    public RelatedEntityPipeline WithUserOwnership(bool userOwned)
    {
      this.userOwned = false;
      return this;
    }

    public RelatedEntityPipeline WithSort(Dictionary<string, QuerySort> sort)
    {
      this.querySort = new Dictionary<string, QuerySort>();
      foreach (KeyValuePair<string, QuerySort> keyValuePair in sort)
        this.querySort.Add(keyValuePair.Key, keyValuePair.Value);
      return this;
    }

    public RelatedEntityPipeline FetchRecords(PaginationInfo paginationInfo = null)
    {
      Dictionary<string, List<Entity>> dictionary = new Dictionary<string, List<Entity>>();
      try
      {
        if (this.viewConfig.RelatedEntities == null)
          return this;
        foreach (string relatedEntity in this.viewConfig.RelatedEntities)
        {
          Stopwatch stopwatch = Stopwatch.StartNew();
          List<Entity> entityList = new List<Entity>();
          try
          {
            List<QueryFilter> queryFilterList1;
            List<QueryFilter> queryFilterList2 = this.queryFilters.TryGetValue(relatedEntity, out queryFilterList1) ? queryFilterList1 : (List<QueryFilter>) null;
            QuerySort querySort;
            QuerySort sort = this.querySort.TryGetValue(relatedEntity, out querySort) ? querySort : (QuerySort) null;
            entityList = this.GetRelatedEntityRecords(relatedEntity, this.primaryRecordIds, paginationInfo, queryFilterList2?.ToArray(), sort: sort);
          }
          catch (Exception ex)
          {
            this.logger.LogError("RelatedEntityPipeline.FetchRecords." + relatedEntity + ".Exception", ex, callerName: nameof (FetchRecords), callerFile: "C:\\__w\\1\\s\\solutions\\AcceleratedSales\\AcceleratedSalesSandboxPlugins\\Core\\Services\\DataAggregation\\RelatedEntityPipeline.cs");
          }
          finally
          {
            if (entityList != null)
              dictionary.Add(relatedEntity, entityList);
            stopwatch.Stop();
            this.logger.LogWarning("RelatedEntityPipeline.FetchRecords." + relatedEntity + ".Duration: " + stopwatch.ElapsedMilliseconds.ToString(), callerName: nameof (FetchRecords), callerFile: "C:\\__w\\1\\s\\solutions\\AcceleratedSales\\AcceleratedSalesSandboxPlugins\\Core\\Services\\DataAggregation\\RelatedEntityPipeline.cs");
          }
        }
      }
      catch (Exception ex)
      {
        this.logger.LogError("RelatedEntityPipeline.FetchRecords.Exception", ex, callerName: nameof (FetchRecords), callerFile: "C:\\__w\\1\\s\\solutions\\AcceleratedSales\\AcceleratedSalesSandboxPlugins\\Core\\Services\\DataAggregation\\RelatedEntityPipeline.cs");
      }
      finally
      {
        this.logger.LogWarning("RelatedEntityPipeline.FetchRecords.End: Success", callerName: nameof (FetchRecords), callerFile: "C:\\__w\\1\\s\\solutions\\AcceleratedSales\\AcceleratedSalesSandboxPlugins\\Core\\Services\\DataAggregation\\RelatedEntityPipeline.cs");
      }
      this.relatedEntitiesMap = dictionary;
      return this;
    }

    public RelatedEntityPipeline FetchSARecords()
    {
      Dictionary<string, List<Entity>> relatedRecordsMap = new Dictionary<string, List<Entity>>();
      try
      {
        if (this.viewConfig.RelatedEntities == null)
          return this;
        foreach (string relatedEntity in this.viewConfig.RelatedEntities)
        {
          Stopwatch stopwatch = Stopwatch.StartNew();
          List<Entity> entityList = new List<Entity>();
          try
          {
            this.UpdateLinkedActivityIds(relatedRecordsMap);
            this.UpdateFilterSortQueries(relatedEntity);
            List<QueryFilter> queryFilterList1;
            List<QueryFilter> queryFilterList2 = this.queryFilters.TryGetValue(relatedEntity, out queryFilterList1) ? queryFilterList1 : (List<QueryFilter>) null;
            QuerySort querySort;
            QuerySort sort = this.querySort.TryGetValue(relatedEntity, out querySort) ? querySort : (QuerySort) null;
            HashSet<string> source;
            this.additionalAttributes.TryGetValue(relatedEntity, out source);
            entityList = this.GetRelatedEntityRecords(relatedEntity, this.primaryRecordIds, this.entityList, queryFilterList2?.ToArray(), source != null ? source.ToArray<string>() : (string[]) null, sort, this.viewConfig.TopCount);
            this.logger.LogWarning("RelatedEntityPipeline.FetchSARecords." + relatedEntity + ".Count: " + entityList?.Count.ToString(), callerName: nameof (FetchSARecords), callerFile: "C:\\__w\\1\\s\\solutions\\AcceleratedSales\\AcceleratedSalesSandboxPlugins\\Core\\Services\\DataAggregation\\RelatedEntityPipeline.cs");
          }
          catch (Exception ex)
          {
            this.logger.LogError("RelatedEntityPipeline.FetchSARecords." + relatedEntity + ".Exception", ex, callerName: nameof (FetchSARecords), callerFile: "C:\\__w\\1\\s\\solutions\\AcceleratedSales\\AcceleratedSalesSandboxPlugins\\Core\\Services\\DataAggregation\\RelatedEntityPipeline.cs");
          }
          finally
          {
            if (entityList != null)
              relatedRecordsMap.Add(relatedEntity, entityList);
            stopwatch.Stop();
            this.logger.LogWarning("RelatedEntityPipeline.FetchSARecords." + relatedEntity + ".Duration: " + stopwatch.ElapsedMilliseconds.ToString(), callerName: nameof (FetchSARecords), callerFile: "C:\\__w\\1\\s\\solutions\\AcceleratedSales\\AcceleratedSalesSandboxPlugins\\Core\\Services\\DataAggregation\\RelatedEntityPipeline.cs");
          }
        }
      }
      catch (Exception ex)
      {
        this.logger.LogError("RelatedEntityPipeline.FetchSARecords.Exception", ex, callerName: nameof (FetchSARecords), callerFile: "C:\\__w\\1\\s\\solutions\\AcceleratedSales\\AcceleratedSalesSandboxPlugins\\Core\\Services\\DataAggregation\\RelatedEntityPipeline.cs");
      }
      finally
      {
        this.logger.LogWarning("RelatedEntityPipeline.FetchSARecords.End: Success", callerName: nameof (FetchSARecords), callerFile: "C:\\__w\\1\\s\\solutions\\AcceleratedSales\\AcceleratedSalesSandboxPlugins\\Core\\Services\\DataAggregation\\RelatedEntityPipeline.cs");
      }
      this.relatedEntitiesMap = relatedRecordsMap;
      return this;
    }

    public RelatedEntityPipeline FormatRecords()
    {
      Stopwatch stopwatch = Stopwatch.StartNew();
      this.relatedFormattedRecords = this.relatedEntitiesMap.ToDictionary<KeyValuePair<string, List<Entity>>, string, List<Dictionary<string, string>>>((Func<KeyValuePair<string, List<Entity>>, string>) (rm => rm.Key), (Func<KeyValuePair<string, List<Entity>>, List<Dictionary<string, string>>>) (rm =>
      {
        try
        {
          return !rm.Value.Any<Entity>() ? new List<Dictionary<string, string>>() : this.relatedExtensions[rm.Key].FormatRecords(rm.Value, this.entityMetadataProvider, this.skipFormattingAttributes.ContainsKey(rm.Key) ? this.skipFormattingAttributes[rm.Key] : new HashSet<string>());
        }
        catch (Exception ex)
        {
          this.logger.LogError("RelatedEntityPipeline.FormatRecords.Status.Failed.Entity name: " + rm.Key + ". Exception", ex, callerName: nameof (FormatRecords), callerFile: "C:\\__w\\1\\s\\solutions\\AcceleratedSales\\AcceleratedSalesSandboxPlugins\\Core\\Services\\DataAggregation\\RelatedEntityPipeline.cs");
        }
        return new List<Dictionary<string, string>>();
      }));
      stopwatch.Stop();
      this.logger.LogWarning("RelatedEntityPipeline.FormatRecords.End.Duration: " + stopwatch.ElapsedMilliseconds.ToString(), callerName: nameof (FormatRecords), callerFile: "C:\\__w\\1\\s\\solutions\\AcceleratedSales\\AcceleratedSalesSandboxPlugins\\Core\\Services\\DataAggregation\\RelatedEntityPipeline.cs");
      return this;
    }

    private List<Entity> GetRelatedEntityRecords(
      string entityName,
      List<string> entityIDs,
      PaginationInfo paginationInfo = null,
      QueryFilter[] additionalFilters = null,
      string[] additionalAttributesForRelatedEntity = null,
      QuerySort sort = null)
    {
      EntityCollection entityCollection = new EntityCollection();
      switch (entityName)
      {
        case "activitypointer":
          entityCollection = new ActivitiesDataAccess(this.dataStore, this.logger).FetchRecords(entityName, entityIDs, paginationInfo, additionalFilters, additionalAttributesForRelatedEntity, sort);
          break;
        case "msdyn_workqueuestate":
          entityCollection = new WorkQueueStateDataAccess(this.dataStore, this.logger).FetchRecords(paginationInfo, additionalFilters, additionalAttributesForRelatedEntity, sort);
          break;
        case "msdyn_sequencetargetstep":
          entityCollection = new SequenceDataAccess(this.dataStore, this.logger).FetchRecords(entityName, entityIDs, paginationInfo, additionalFilters, additionalAttributesForRelatedEntity, sort);
          break;
        case "postfollow":
          entityCollection = new PostFollowsDataAccess(this.dataStore, this.logger).FetchRecords(entityName, entityIDs, paginationInfo, additionalFilters, additionalAttributesForRelatedEntity, sort);
          break;
        case "msdyn_salessuggestion":
          entityCollection = new SuggestionsDataAccess(this.logger, this.dataStore).FetchRecords(entityName, paginationInfo, additionalFilters, this.additionalAttributes, sort);
          break;
      }
      return entityCollection.Entities.ToList<Entity>();
    }

    private List<Entity> GetRelatedEntityRecords(
      string entityName,
      List<string> entityIDs,
      List<string> entityList,
      QueryFilter[] additionalFilters = null,
      string[] additionalAttributesForRelatedEntity = null,
      QuerySort sort = null,
      int topCount = 250)
    {
      EntityCollection entityCollection = new EntityCollection();
      switch (entityName)
      {
        case "activitypointer":
          entityCollection = new ActivitiesDataAccess(this.dataStore, this.logger).FetchRecords(entityName, entityIDs, entityList, additionalFilters, additionalAttributesForRelatedEntity, sort, topCount);
          break;
        case "msdyn_sequencetargetstep":
          entityCollection = new SequenceDataAccess(this.dataStore, this.logger).FetchRecords(entityName, entityIDs, entityList, additionalFilters, additionalAttributesForRelatedEntity, sort, topCount);
          break;
        case "msdyn_workqueuestate":
          entityCollection = new WorkQueueStateDataAccess(this.dataStore, this.logger).FetchRecords(additionalFilters: additionalFilters, additionalAttributes: additionalAttributesForRelatedEntity, sort: sort, topCount: topCount);
          break;
        case "msdyn_salessuggestion":
          entityCollection = new SuggestionsDataAccess(this.logger, this.dataStore).FetchRecords(entityName, additionalFilters: additionalFilters, additionalAttributes: this.additionalAttributes, sort: sort, topCount: topCount);
          break;
      }
      return entityCollection.Entities.ToList<Entity>();
    }

    private void UpdateLinkedActivityIds(Dictionary<string, List<Entity>> relatedRecordsMap)
    {
      if (!relatedRecordsMap.ContainsKey("msdyn_sequencetargetstep") || this.settings.settingsInstance == null || !this.settings.settingsInstance.shouldLinkSequenceStepToActivity || this.dataStore.IsFCBEnabled("FCB.AlwaysLinkActivityToStepDisabled"))
        return;
      relatedRecordsMap["msdyn_sequencetargetstep"].ForEach((Action<Entity>) (record =>
      {
        Guid? attributeValue = record.GetAttributeValue<Guid?>("msdyn_linkedactivityid");
        if (!attributeValue.HasValue)
          return;
        this.linkedActivityIds.Add(attributeValue.Value);
      }));
    }

    private void UpdateFilterSortQueries(string relatedEntity)
    {
      List<object> regardingObjectTypeCodes;
      switch (relatedEntity)
      {
        case "activitypointer":
          regardingObjectTypeCodes = new List<object>();
          List<QueryFilter> newQueryFilters1 = new List<QueryFilter>();
          if (this.entityList != null && this.entityList.Count > 0)
          {
            this.entityList.ForEach((Action<string>) (entity =>
            {
              EntityMetadata entityMetadata = this.entityMetadataProvider.GetEntityMetadata(entity);
              int? nullable1;
              int num;
              if (entityMetadata == null)
              {
                num = 0;
              }
              else
              {
                nullable1 = entityMetadata.ObjectTypeCode;
                num = nullable1.HasValue ? 1 : 0;
              }
              if (num == 0)
                return;
              List<object> objectList = regardingObjectTypeCodes;
              int? nullable2;
              if (entityMetadata == null)
              {
                nullable1 = new int?();
                nullable2 = nullable1;
              }
              else
                nullable2 = entityMetadata.ObjectTypeCode;
              // ISSUE: variable of a boxed type
              __Boxed<int?> local = (ValueType) nullable2;
              objectList.Add((object) local);
            }));
            newQueryFilters1.Add(new QueryFilter()
            {
              AttributeName = "regardingobjecttypecode",
              Operator = ConditionOperator.In,
              Values = regardingObjectTypeCodes
            });
          }
          if (this.linkedActivityIds != null && this.linkedActivityIds.Count > 0)
            newQueryFilters1.Add(new QueryFilter()
            {
              AttributeName = "activityid",
              Operator = ConditionOperator.NotIn,
              Values = this.linkedActivityIds.Select<Guid, object>((Func<Guid, object>) (x => (object) x)).ToList<object>()
            });
          if (this.userOwned)
            newQueryFilters1.Add(new QueryFilter()
            {
              AttributeName = "ownerid",
              Operator = ConditionOperator.EqualUserOrUserTeams
            });
          this.queryFilters = this.queryFilters.UpdateQueryFilter("activitypointer", newQueryFilters1);
          break;
        case "msdyn_sequencetargetstep":
          List<QueryFilter> newQueryFilters2 = new List<QueryFilter>();
          SettingsInstance settingsInstance = this.settings.settingsInstance;
          if (settingsInstance != null && settingsInstance.migrationStatus?.MigrationStage.GetValueOrDefault() == MigrationStage.Completed)
          {
            regardingObjectTypeCodes = new List<object>();
            this.entityList.ForEach((Action<string>) (entity =>
            {
              EntityMetadata entityMetadata1 = this.entityMetadataProvider.GetEntityMetadata(entity);
              int? nullable3;
              int num;
              if (entityMetadata1 == null)
              {
                num = 0;
              }
              else
              {
                nullable3 = entityMetadata1.ObjectTypeCode;
                num = nullable3.HasValue ? 1 : 0;
              }
              if (num == 0)
                return;
              List<object> objectList = regardingObjectTypeCodes;
              EntityMetadata entityMetadata2 = this.entityMetadataProvider.GetEntityMetadata(entity);
              int? nullable4;
              if (entityMetadata2 == null)
              {
                nullable3 = new int?();
                nullable4 = nullable3;
              }
              else
                nullable4 = entityMetadata2.ObjectTypeCode;
              // ISSUE: variable of a boxed type
              __Boxed<int?> local = (ValueType) nullable4;
              objectList.Add((object) local);
            }));
            newQueryFilters2.Add(new QueryFilter()
            {
              AttributeName = "msdyn_targetidtype",
              Operator = ConditionOperator.In,
              Values = regardingObjectTypeCodes,
              EntityName = "msdyn_sequencetarget"
            });
          }
          if (this.userOwned)
            newQueryFilters2.Add(new QueryFilter()
            {
              AttributeName = "ownerid",
              Operator = ConditionOperator.EqualUserOrUserTeams
            });
          this.queryFilters = this.queryFilters.UpdateQueryFilter("msdyn_sequencetargetstep", newQueryFilters2);
          break;
      }
    }
  }
}
