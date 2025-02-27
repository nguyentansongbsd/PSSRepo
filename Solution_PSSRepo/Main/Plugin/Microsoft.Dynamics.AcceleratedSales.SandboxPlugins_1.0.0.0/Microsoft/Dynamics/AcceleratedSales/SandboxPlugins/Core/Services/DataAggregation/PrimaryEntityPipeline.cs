// Decompiled with JetBrains decompiler
// Type: Microsoft.Dynamics.AcceleratedSales.SandboxPlugins.Core.Services.DataAggregation.PrimaryEntityPipeline
// Assembly: Microsoft.Dynamics.AcceleratedSales.SandboxPlugins, Version=1.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35
// MVID: 7BBE7132-7E30-4424-B38D-27667CFBCEFA
// Assembly location: C:\Users\ngoct\Downloads\New folder\Microsoft.Dynamics.AcceleratedSales.SandboxPlugins_1.0.0.0.dll

using Microsoft.Crm.AcceleratedSales.Shared.SandboxServices.Platform;
using Microsoft.Crm.Sdk.Messages;
using Microsoft.Dynamics.AcceleratedSales.SandboxObjectModel.Model;
using Microsoft.Dynamics.AcceleratedSales.SandboxPlugins.Core.DataAccess;
using Microsoft.Dynamics.AcceleratedSales.SandboxPlugins.Core.Model;
using Microsoft.Dynamics.AcceleratedSales.SandboxPlugins.Core.Model.Layout;
using Microsoft.Dynamics.AcceleratedSales.SandboxPlugins.Core.Services.Extensions;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Query;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Xml.Linq;

#nullable disable
namespace Microsoft.Dynamics.AcceleratedSales.SandboxPlugins.Core.Services.DataAggregation
{
  public class PrimaryEntityPipeline
  {
    public static readonly string ActivityPointerEntityName = "activitypointer";
    public static readonly string ActivityTypeCodeAttributeName = "activitytypecode";
    private readonly IDataStore dataStore;
    private readonly IAcceleratedSalesLogger logger;
    private readonly IEntityMetadataProvider entityMetadataProvider;
    private WorklistViewConfiguration viewConfig;
    private Dictionary<string, IDataExtension> primaryExtensions;
    private Dictionary<string, IDataExtension> relatedExtensions;
    private Dictionary<string, DesignerConfiguration> cardLayout;
    private Dictionary<string, QueryExpression> queryExpressions;
    private Dictionary<string, PrimaryEntityPipelineData> primaryEntities;
    private Dictionary<string, QueryExpression> inititalQueryExpressions;
    private string mergedFetchXml;
    private string viewLayoutXml;
    private Dictionary<string, List<Dictionary<string, string>>> primaryFormattedRecords;
    private Dictionary<string, List<QueryFilter>> queryFilters;
    private Dictionary<string, QuerySort> querySort;
    private string querySearch;
    private Dictionary<string, HashSet<Guid>> primaryRecordIds;
    private SASettings settings;
    private Dictionary<string, HashSet<string>> additionalAttributes;
    private bool fetchFromPrimaryRecordIds;
    private Dictionary<string, HashSet<string>> skipFormattingAttributes;
    private bool continueOnError;

    private PrimaryEntityPipeline(
      IDataStore dataStore,
      IAcceleratedSalesLogger logger,
      IEntityMetadataProvider entityMetadataProvider,
      bool continueOnError = false)
    {
      this.dataStore = dataStore;
      this.logger = logger;
      this.entityMetadataProvider = entityMetadataProvider;
      this.primaryExtensions = new Dictionary<string, IDataExtension>();
      this.relatedExtensions = new Dictionary<string, IDataExtension>();
      this.primaryEntities = new Dictionary<string, PrimaryEntityPipelineData>();
      this.primaryFormattedRecords = new Dictionary<string, List<Dictionary<string, string>>>();
      this.queryFilters = new Dictionary<string, List<QueryFilter>>();
      this.querySort = new Dictionary<string, QuerySort>();
      this.primaryRecordIds = new Dictionary<string, HashSet<Guid>>();
      this.additionalAttributes = new Dictionary<string, HashSet<string>>();
      this.fetchFromPrimaryRecordIds = false;
      this.skipFormattingAttributes = new Dictionary<string, HashSet<string>>();
      this.inititalQueryExpressions = new Dictionary<string, QueryExpression>();
      this.continueOnError = continueOnError;
    }

    public Dictionary<string, PrimaryEntityPipelineData> PrimaryEntities => this.primaryEntities;

    public Dictionary<string, List<Dictionary<string, string>>> PrimaryRecords
    {
      get => this.primaryFormattedRecords;
    }

    public Dictionary<string, QueryExpression> PrimaryQueryExpressions => this.queryExpressions;

    private List<string> SupportedPrimaryEntities
    {
      get
      {
        if (string.IsNullOrEmpty(this.viewConfig.EntityName))
          return this.primaryExtensions.Keys.ToList<string>();
        return new List<string>()
        {
          this.viewConfig.EntityName
        };
      }
    }

    public static PrimaryEntityPipeline Create(
      IDataStore dataStore,
      IAcceleratedSalesLogger logger,
      IEntityMetadataProvider entityMetadataProvider,
      bool continueOnError = false)
    {
      return new PrimaryEntityPipeline(dataStore, logger, entityMetadataProvider, continueOnError);
    }

    public static string SanitizeFetchXml(string fetchXml, IAcceleratedSalesLogger logger)
    {
      try
      {
        if (string.IsNullOrEmpty(fetchXml))
          return fetchXml;
        XDocument xdocument = XDocument.Parse(fetchXml);
        List<XElement> list = xdocument.Descendants((XName) "filter").Where<XElement>((Func<XElement, bool>) (filter =>
        {
          if (!(filter.Attribute((XName) "isquickfindfields")?.Value == "1") && !(filter.Attribute((XName) "isquickfindfields")?.Value == "true"))
            return false;
          IEnumerable<XElement> source = filter.Descendants((XName) "condition");
          return source != null && source.Any<XElement>((Func<XElement, bool>) (condition => condition.Attribute((XName) "value")?.Value != null && Regex.IsMatch(condition.Attribute((XName) "value")?.Value, "^\\{\\d\\}$")));
        })).ToList<XElement>();
        logger.AddCustomProperty("PrEP.SanitizeFetchXml.FiltersToRemove.Length", (object) list?.Count);
        if (list == null || list.Count == 0)
        {
          if (xdocument.Descendants((XName) "filter").Where<XElement>((Func<XElement, bool>) (filter => filter.Attribute((XName) "isquickfindfields")?.Value == "1" || filter.Attribute((XName) "isquickfindfields")?.Value == "true")).FirstOrDefault<XElement>() != null)
            logger.LogWarning("PrimaryEntityPipeline.SanitizeFetchXml.HasQuickSearchFilters: true", callerName: nameof (SanitizeFetchXml), callerFile: "C:\\__w\\1\\s\\solutions\\AcceleratedSales\\AcceleratedSalesSandboxPlugins\\Core\\Services\\DataAggregation\\PrimaryEntityPipeline.cs");
          return fetchXml;
        }
        foreach (XNode xnode in list)
          xnode.Remove();
        return xdocument.ToString();
      }
      catch (Exception ex)
      {
        logger.LogError("PrimaryEntityPipeline.SanitizeFetchXml.Exception", ex, callerName: nameof (SanitizeFetchXml), callerFile: "C:\\__w\\1\\s\\solutions\\AcceleratedSales\\AcceleratedSalesSandboxPlugins\\Core\\Services\\DataAggregation\\PrimaryEntityPipeline.cs");
        return fetchXml;
      }
    }

    public PrimaryEntityPipeline WithSkipFormattingAttributes(
      Dictionary<string, HashSet<string>> skipFormattingAttributes)
    {
      this.skipFormattingAttributes = skipFormattingAttributes;
      return this;
    }

    public PrimaryEntityPipeline WithViewConfiguration(WorklistViewConfiguration config)
    {
      this.viewConfig = config ?? throw new ArgumentNullException(nameof (config));
      return this;
    }

    public PrimaryEntityPipeline WithExtensions(
      Dictionary<string, IDataExtension> primaryExtensions,
      Dictionary<string, IDataExtension> relatedExtensions)
    {
      this.primaryExtensions = primaryExtensions ?? throw new ArgumentNullException(nameof (primaryExtensions));
      this.relatedExtensions = relatedExtensions ?? throw new ArgumentNullException(nameof (relatedExtensions));
      return this;
    }

    public PrimaryEntityPipeline WithSettings(SASettings settings)
    {
      this.settings = settings;
      return this;
    }

    public PrimaryEntityPipeline WithAdditionalAttributes(
      Dictionary<string, HashSet<string>> additionalAttributes)
    {
      this.additionalAttributes = additionalAttributes ?? new Dictionary<string, HashSet<string>>();
      return this;
    }

    public PrimaryEntityPipeline WithPrimaryRecordIds(
      Dictionary<string, HashSet<Guid>> primaryRecordIds)
    {
      this.primaryRecordIds = primaryRecordIds ?? new Dictionary<string, HashSet<Guid>>();
      return this;
    }

    public PrimaryEntityPipeline WithFetchFromRecordIdsOnly(bool fetchFromPrimaryRecordIds)
    {
      this.fetchFromPrimaryRecordIds = fetchFromPrimaryRecordIds;
      return this;
    }

    public PrimaryEntityPipeline WithCardLayout(
      Dictionary<string, DesignerConfiguration> cardLayout)
    {
      this.cardLayout = cardLayout ?? throw new ArgumentNullException(nameof (cardLayout));
      return this;
    }

    public PrimaryEntityPipeline WithFilters(Dictionary<string, List<QueryFilter>> filters)
    {
      this.queryFilters = filters ?? new Dictionary<string, List<QueryFilter>>();
      return this;
    }

    public PrimaryEntityPipeline WithSort(Dictionary<string, QuerySort> sort)
    {
      this.querySort = sort ?? new Dictionary<string, QuerySort>();
      return this;
    }

    public PrimaryEntityPipeline WithSearch(string search)
    {
      this.querySearch = search;
      return this;
    }

    public PrimaryEntityPipeline PrepareInitialQuery()
    {
      if (this.viewConfig == null)
        throw new NotSupportedException("PrepareInitialQuery: Please ensure ViewConfiguration is provided with WithViewConfigurations step.");
      this.logger.AddCustomProperty("IsFetchXmlProvidedInRequest", (object) !string.IsNullOrEmpty(this.viewConfig.FetchXml));
      if (string.IsNullOrEmpty(this.viewConfig.FetchXml))
      {
        Guid result;
        if (!Guid.TryParse(this.viewConfig.SavedQueryId, out result))
        {
          this.logger.LogWarning("PrepareInitialQuery.Failed.Status" + result.ToString(), callerName: nameof (PrepareInitialQuery), callerFile: "C:\\__w\\1\\s\\solutions\\AcceleratedSales\\AcceleratedSalesSandboxPlugins\\Core\\Services\\DataAggregation\\PrimaryEntityPipeline.cs");
          throw new Exception("PrepareInitialQuery failed.");
        }
        this.viewConfig.FetchXml = string.IsNullOrEmpty(this.viewConfig.SavedQueryEntityName) || !this.viewConfig.SavedQueryEntityName.Equals("4230", StringComparison.Ordinal) ? new SavedQueryDataAccess(this.dataStore, this.logger).GetFetchXml(result) : new UserQueryDataAccess(this.dataStore, this.logger).GetFetchXml(result);
        if (string.IsNullOrEmpty(this.viewConfig.FetchXml))
          throw new Exception("Invalid fetchxml provided. Query id: " + this.viewConfig.SavedQueryId + ", Entity type: " + (this.viewConfig.SavedQueryEntityName ?? (string) null));
      }
      this.viewConfig.FetchXml = PrimaryEntityPipeline.SanitizeFetchXml(this.viewConfig.FetchXml, this.logger);
      QueryExpression queryExpression = this.dataStore.ConvertFetchXmlToQueryExpression(this.viewConfig.FetchXml, this.logger);
      if (this.viewConfig.EntityName == PrimaryEntityPipeline.ActivityPointerEntityName)
        this.AddActivityTypeCodeAttributeIfNotAvailable(queryExpression);
      if (this.viewConfig.PaginationInfo != null)
      {
        queryExpression.PageInfo.Count = this.viewConfig.PaginationInfo.PageCount;
        queryExpression.PageInfo.PageNumber = this.viewConfig.PaginationInfo.PageNumber;
        queryExpression.PageInfo.PagingCookie = this.viewConfig.PaginationInfo.PagingCookie;
        queryExpression.PageInfo.ReturnTotalRecordCount = true;
        this.logger.LogWarning(string.Format("PrimaryEntityPipeline.PrepareInitialQuery.PageInfo: PageNumber: {0} PageCount: {1}", (object) queryExpression.PageInfo.PageNumber, (object) queryExpression.PageInfo.Count), callerName: nameof (PrepareInitialQuery), callerFile: "C:\\__w\\1\\s\\solutions\\AcceleratedSales\\AcceleratedSalesSandboxPlugins\\Core\\Services\\DataAggregation\\PrimaryEntityPipeline.cs");
      }
      this.inititalQueryExpressions = new Dictionary<string, QueryExpression>();
      this.inititalQueryExpressions[this.viewConfig.EntityName] = queryExpression;
      return this;
    }

    public PrimaryEntityPipeline PrepareMetadataQuery(MetadataQueryParams metadataQueryParams)
    {
      string entityName = this.viewConfig.EntityName;
      this.primaryExtensions[entityName].PrepareMetadataQuery(this.inititalQueryExpressions[entityName], this.cardLayout?[entityName], metadataQueryParams);
      return this;
    }

    public PrimaryEntityPipeline PrepareQuery()
    {
      if (this.viewConfig == null)
        throw new NotSupportedException("PrepareQuery: Please ensure ViewConfiguration is provided with WithViewConfigurations step.");
      this.queryExpressions = this.SupportedPrimaryEntities.ToDictionary<string, string, QueryExpression>((Func<string, string>) (k => k), (Func<string, QueryExpression>) (k => this.primaryExtensions[k].CreateQuery(this.entityMetadataProvider, this.cardLayout?[k])));
      string entityName = this.viewConfig.EntityName;
      Dictionary<string, QueryExpression> queryExpressions = this.queryExpressions;
      string key = entityName;
      QueryExpression inititalQueryExpression = this.inititalQueryExpressions[entityName];
      List<QueryExpression> queries = new List<QueryExpression>();
      queries.Add(this.queryExpressions[entityName]);
      IEntityMetadataProvider metadataProvider = this.entityMetadataProvider;
      QueryExpression queryExpression = inititalQueryExpression.Merge(queries, metadataProvider);
      queryExpressions[key] = queryExpression;
      List<QueryFilter> queryFilterList;
      List<QueryFilter> filters = this.queryFilters.TryGetValue(entityName, out queryFilterList) ? queryFilterList : (List<QueryFilter>) null;
      QuerySort querySort;
      QuerySort sort = this.querySort.TryGetValue(entityName, out querySort) ? querySort : (QuerySort) null;
      this.queryExpressions[entityName].AddClientFilter(filters, sort, this.dataStore.IsFCSEnabled("SalesService.Workspace", "RemoveEntityNameInFilterCondition"));
      this.logger.LogWarning("PrepareQuery.AddClientFilter.End: Success", callerName: nameof (PrepareQuery), callerFile: "C:\\__w\\1\\s\\solutions\\AcceleratedSales\\AcceleratedSalesSandboxPlugins\\Core\\Services\\DataAggregation\\PrimaryEntityPipeline.cs");
      if (!string.IsNullOrEmpty(this.querySearch))
      {
        string quickFindFetchXml = new SavedQueryDataAccess(this.dataStore, this.logger).GetQuickFindFetchXml(entityName);
        if (!string.IsNullOrEmpty(quickFindFetchXml))
        {
          this.queryExpressions[entityName].AddClientSearch(quickFindFetchXml, this.querySearch, this.entityMetadataProvider, this.logger, this.viewConfig.ParsedSearchedDate);
          this.logger.LogWarning("PrepareQuery.AddClientSearch.End: Success", callerName: nameof (PrepareQuery), callerFile: "C:\\__w\\1\\s\\solutions\\AcceleratedSales\\AcceleratedSalesSandboxPlugins\\Core\\Services\\DataAggregation\\PrimaryEntityPipeline.cs");
        }
      }
      return this;
    }

    public PrimaryEntityPipeline PrepareFetchXml()
    {
      int num;
      if (this.queryExpressions != null)
      {
        Dictionary<string, QueryExpression>.ValueCollection values = this.queryExpressions.Values;
        num = values != null ? (values.Count < 1 ? 1 : 0) : 0;
      }
      else
        num = 1;
      if (num != 0)
        throw new Exception("Did you miss running the PrepareQuery step?");
      this.mergedFetchXml = this.dataStore.ConvertQueryExpressionToFetchXml(this.queryExpressions.Values.First<QueryExpression>(), this.logger);
      return this;
    }

    public PrimaryEntityPipeline PrepareLayoutXml()
    {
      if (this.viewConfig == null)
        throw new NotSupportedException("PrepareLayoutXml: Please ensure ViewConfiguration is provided with WithViewConfigurations step.");
      try
      {
        Guid result;
        if (!Guid.TryParse(this.viewConfig.SavedQueryId, out result))
        {
          this.logger.LogWarning("PrepareLayoutXml.Failed.Status: " + result.ToString(), callerName: nameof (PrepareLayoutXml), callerFile: "C:\\__w\\1\\s\\solutions\\AcceleratedSales\\AcceleratedSalesSandboxPlugins\\Core\\Services\\DataAggregation\\PrimaryEntityPipeline.cs");
          throw new Exception("PrepareLayoutXml failed.");
        }
        this.viewLayoutXml = string.IsNullOrEmpty(this.viewConfig.SavedQueryEntityName) || !this.viewConfig.SavedQueryEntityName.Equals("4230", StringComparison.Ordinal) ? new SavedQueryDataAccess(this.dataStore, this.logger).GetLayoutXml(result) : new UserQueryDataAccess(this.dataStore, this.logger).GetLayoutXml(result);
        if (this.viewLayoutXml == null)
          throw new CrmException("Unable to fetchViewLayoutXml", 1879506963);
      }
      catch (Exception ex)
      {
        this.logger.LogError("PrimaryEntityPipeline.PrepareLayoutXml.Exception", ex, callerName: nameof (PrepareLayoutXml), callerFile: "C:\\__w\\1\\s\\solutions\\AcceleratedSales\\AcceleratedSalesSandboxPlugins\\Core\\Services\\DataAggregation\\PrimaryEntityPipeline.cs");
        this.logger.LogWarning("PrepareLayoutXml.Failed.savedQueryEntityName." + this.viewConfig.SavedQueryEntityName + ".savedQueryId: " + this.viewConfig.SavedQueryId, callerName: nameof (PrepareLayoutXml), callerFile: "C:\\__w\\1\\s\\solutions\\AcceleratedSales\\AcceleratedSalesSandboxPlugins\\Core\\Services\\DataAggregation\\PrimaryEntityPipeline.cs");
        if (ex is CrmException)
          throw;
        else
          throw new CrmException("PrepareLayoutXml failed.", 1879506963, new object[1]
          {
            (object) ex
          });
      }
      return this;
    }

    public PrimaryEntityPipeline PrepareSAQuery()
    {
      try
      {
        if (this.viewConfig == null)
          throw new NotSupportedException("PrepareQuery: Please ensure ViewConfiguration is provided with WithViewConfigurations step.");
        this.queryExpressions = new Dictionary<string, QueryExpression>();
        this.SupportedPrimaryEntities.ForEach((Action<string>) (k =>
        {
          if (this.fetchFromPrimaryRecordIds && (!this.fetchFromPrimaryRecordIds || !this.primaryRecordIds.ContainsKey(k) || this.primaryRecordIds[k].Count <= 0) || this.queryExpressions.ContainsKey(k))
            return;
          QueryExpression query = this.primaryExtensions[k].CreateQuery(this.entityMetadataProvider, this.cardLayout?[k]);
          HashSet<string> source;
          if (this.additionalAttributes.TryGetValue(k, out source))
            query.ColumnSet.AddColumns(source != null ? source.ToArray<string>() : (string[]) null);
          List<QueryFilter> queryFilterList;
          List<QueryFilter> filters = this.queryFilters.TryGetValue(k, out queryFilterList) ? queryFilterList : (List<QueryFilter>) null;
          QuerySort querySort;
          QuerySort sort = this.querySort.TryGetValue(k, out querySort) ? querySort : (QuerySort) null;
          query.AddClientFilter(filters, sort);
          query.Criteria.AddCondition("statecode", ConditionOperator.Equal, (object) 0);
          if (this.primaryRecordIds.ContainsKey(k) && this.primaryRecordIds[k].Count > 0)
            query.Criteria.AddCondition(this.entityMetadataProvider.GetEntityMetadata(k)?.PrimaryIdAttribute, ConditionOperator.In, this.primaryRecordIds[k].ToList<Guid>().Select<Guid, object>((Func<Guid, object>) (x => (object) x)).ToArray<object>());
          if (this.settings != null && this.settings.settingsInstance != null && this.settings.settingsInstance.entityConfigurations.ContainsKey(k))
          {
            EntityConfiguration entityConfiguration = this.settings.settingsInstance.entityConfigurations[k];
            if (!string.IsNullOrWhiteSpace(entityConfiguration.customOwnerAttribute) && entityConfiguration.IsEnabled)
              query.Criteria.AddCondition(entityConfiguration.customOwnerAttribute, ConditionOperator.EqualUserOrUserTeams);
          }
          this.queryExpressions.Add(k, query);
        }));
      }
      catch (Exception ex)
      {
        this.logger.LogError("PrimaryEntityPipeline.PrepareSAQuery.Exception", ex, callerName: nameof (PrepareSAQuery), callerFile: "C:\\__w\\1\\s\\solutions\\AcceleratedSales\\AcceleratedSalesSandboxPlugins\\Core\\Services\\DataAggregation\\PrimaryEntityPipeline.cs");
      }
      return this;
    }

    public PrimaryEntityPipeline RemoveLinkEntitiesWithNoAccess()
    {
      SecurityRolesDataAccess securityRolesDataAccess = new SecurityRolesDataAccess(this.dataStore, this.logger);
      if (this.queryExpressions.Values.FirstOrDefault<QueryExpression>((Func<QueryExpression, bool>) (queryExpression =>
      {
        DataCollection<LinkEntity> linkEntities = queryExpression.LinkEntities;
        return (linkEntities != null ? linkEntities.FirstOrDefault<LinkEntity>((Func<LinkEntity, bool>) (le => le.EntityAlias == LinkEntityLayoutExtensions.GetLinkEntityKey("PriorityScore"))) : (LinkEntity) null) != null;
      })) != null)
      {
        RolePrivilege[] privilagesByName = securityRolesDataAccess.GetUserPrivilagesByName("prvReadmsdyn_predictivescore");
        if (privilagesByName == null || privilagesByName.Length == 0)
        {
          foreach (QueryExpression queryExpression in this.queryExpressions.Values)
          {
            DataCollection<LinkEntity> linkEntities = queryExpression.LinkEntities;
            LinkEntity linkEntity = linkEntities != null ? linkEntities.FirstOrDefault<LinkEntity>((Func<LinkEntity, bool>) (le => le.EntityAlias == LinkEntityLayoutExtensions.GetLinkEntityKey("PriorityScore"))) : (LinkEntity) null;
            if (linkEntity != null)
            {
              this.logger.LogWarning("RemovingLinkEntityForPredictiveScore: true", callerName: nameof (RemoveLinkEntitiesWithNoAccess), callerFile: "C:\\__w\\1\\s\\solutions\\AcceleratedSales\\AcceleratedSalesSandboxPlugins\\Core\\Services\\DataAggregation\\PrimaryEntityPipeline.cs");
              queryExpression.LinkEntities.Remove(linkEntity);
            }
          }
        }
      }
      if (this.queryExpressions.Values.FirstOrDefault<QueryExpression>((Func<QueryExpression, bool>) (queryExpression =>
      {
        DataCollection<LinkEntity> linkEntities = queryExpression.LinkEntities;
        return (linkEntities != null ? linkEntities.FirstOrDefault<LinkEntity>((Func<LinkEntity, bool>) (le => le.EntityAlias == LinkEntityLayoutExtensions.GetLinkEntityKey("FollowIndicator"))) : (LinkEntity) null) != null;
      })) != null)
      {
        RolePrivilege[] privilagesByName = securityRolesDataAccess.GetUserPrivilagesByName("prvReadPostFollow");
        if (privilagesByName == null || privilagesByName.Length == 0)
        {
          foreach (QueryExpression queryExpression in this.queryExpressions.Values)
          {
            DataCollection<LinkEntity> linkEntities = queryExpression.LinkEntities;
            LinkEntity linkEntity = linkEntities != null ? linkEntities.FirstOrDefault<LinkEntity>((Func<LinkEntity, bool>) (le => le.EntityAlias == LinkEntityLayoutExtensions.GetLinkEntityKey("FollowIndicator"))) : (LinkEntity) null;
            if (linkEntity != null)
            {
              this.logger.LogWarning("RemovingLinkEntityForFollow: true", callerName: nameof (RemoveLinkEntitiesWithNoAccess), callerFile: "C:\\__w\\1\\s\\solutions\\AcceleratedSales\\AcceleratedSalesSandboxPlugins\\Core\\Services\\DataAggregation\\PrimaryEntityPipeline.cs");
              queryExpression.LinkEntities.Remove(linkEntity);
            }
          }
        }
      }
      return this;
    }

    public PrimaryEntityPipeline FetchRecords()
    {
      try
      {
        if (this.queryExpressions == null)
          throw new Exception("Did you miss running the PrepareQuery step?");
        foreach (string supportedPrimaryEntity in this.SupportedPrimaryEntities)
        {
          string entityName = supportedPrimaryEntity;
          try
          {
            EntityRecordDataAccess recordDataAccess = new EntityRecordDataAccess(this.dataStore, this.logger);
            if (this.queryExpressions.ContainsKey(entityName))
            {
              EntityCollection entityCollection = recordDataAccess.FetchEntityRecords(this.queryExpressions[entityName]);
              PrimaryEntityPipelineData entityPipelineData = new PrimaryEntityPipelineData()
              {
                PrimaryEntityRecords = entityCollection.Entities.Select<Entity, Entity>((Func<Entity, Entity>) (r => this.primaryExtensions[entityName].UpdateRecord(r, this.entityMetadataProvider, this.cardLayout?[entityName]))).ToList<Entity>(),
                PagingCookie = entityCollection.MoreRecords ? entityCollection.PagingCookie : (string) null,
                RecordsCount = entityCollection.Entities.Count,
                HasMoreRecords = entityCollection.MoreRecords,
                TotalRecordsCount = entityCollection.TotalRecordCount,
                TotalRecordsCountLimitExceeded = entityCollection.TotalRecordCountLimitExceeded
              };
              this.primaryEntities.Add(entityName, entityPipelineData);
              this.logger.LogWarning(string.Format("PrimaryEntityPipeline.FetchRecords.{0}: {1}", (object) entityName, (object) entityCollection.Entities.Count), callerName: nameof (FetchRecords), callerFile: "C:\\__w\\1\\s\\solutions\\AcceleratedSales\\AcceleratedSalesSandboxPlugins\\Core\\Services\\DataAggregation\\PrimaryEntityPipeline.cs");
            }
          }
          catch (Exception ex)
          {
            this.logger.LogError("PrimaryEntityPipeline.FetchRecords." + entityName + ".Exception", ex, callerName: nameof (FetchRecords), callerFile: "C:\\__w\\1\\s\\solutions\\AcceleratedSales\\AcceleratedSalesSandboxPlugins\\Core\\Services\\DataAggregation\\PrimaryEntityPipeline.cs");
            if (!this.continueOnError)
            {
              if (string.IsNullOrEmpty(ex.Message))
                throw new CrmException("PrimaryEntityPipeline.FetchRecords." + entityName + ".Failed", ex, 1879506965);
              throw;
            }
          }
        }
      }
      catch (Exception ex)
      {
        this.logger.LogError("PrimaryEntityPipeline.FetchRecords.Exception", ex, callerName: nameof (FetchRecords), callerFile: "C:\\__w\\1\\s\\solutions\\AcceleratedSales\\AcceleratedSalesSandboxPlugins\\Core\\Services\\DataAggregation\\PrimaryEntityPipeline.cs");
        throw;
      }
      return this;
    }

    public PrimaryEntityPipeline FormatRecords()
    {
      foreach (string supportedPrimaryEntity in this.SupportedPrimaryEntities)
      {
        if (this.primaryEntities.ContainsKey(supportedPrimaryEntity) && this.primaryEntities[supportedPrimaryEntity].PrimaryEntityRecords.Any<Entity>())
          this.primaryFormattedRecords[supportedPrimaryEntity] = this.primaryExtensions[supportedPrimaryEntity].FormatRecords(this.primaryEntities[supportedPrimaryEntity].PrimaryEntityRecords, this.entityMetadataProvider, this.skipFormattingAttributes.ContainsKey(supportedPrimaryEntity) ? this.skipFormattingAttributes[supportedPrimaryEntity] : new HashSet<string>());
      }
      return this;
    }

    public string GetFetchXmlUsed() => this.viewConfig.FetchXml;

    public string GetMergedFetchXml() => this.mergedFetchXml;

    public string GetLayoutXml() => this.viewLayoutXml;

    private void AddActivityTypeCodeAttributeIfNotAvailable(QueryExpression initialQuery)
    {
      this.logger.LogWarning("PrimaryEntityPipeline.IsActivityPointerEntity", callerName: nameof (AddActivityTypeCodeAttributeIfNotAvailable), callerFile: "C:\\__w\\1\\s\\solutions\\AcceleratedSales\\AcceleratedSalesSandboxPlugins\\Core\\Services\\DataAggregation\\PrimaryEntityPipeline.cs");
      if (initialQuery.ColumnSet.Columns.Contains(PrimaryEntityPipeline.ActivityTypeCodeAttributeName))
        return;
      this.logger.LogWarning("PrimaryEntityPipeline.AddActivityTypeCodeAttributeIfNotAvailable", callerName: nameof (AddActivityTypeCodeAttributeIfNotAvailable), callerFile: "C:\\__w\\1\\s\\solutions\\AcceleratedSales\\AcceleratedSalesSandboxPlugins\\Core\\Services\\DataAggregation\\PrimaryEntityPipeline.cs");
      initialQuery.ColumnSet.AddColumn(PrimaryEntityPipeline.ActivityTypeCodeAttributeName);
    }
  }
}
