// Decompiled with JetBrains decompiler
// Type: Microsoft.Dynamics.AcceleratedSales.SandboxPlugins.Core.Services.WorklistDataProviderService
// Assembly: Microsoft.Dynamics.AcceleratedSales.SandboxPlugins, Version=1.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35
// MVID: 7BBE7132-7E30-4424-B38D-27667CFBCEFA
// Assembly location: C:\Users\ngoct\Downloads\New folder\Microsoft.Dynamics.AcceleratedSales.SandboxPlugins_1.0.0.0.dll

using Microsoft.Crm.AcceleratedSales.Shared.SandboxServices.Platform;
using Microsoft.Dynamics.AcceleratedSales.SandboxObjectModel.Model;
using Microsoft.Dynamics.AcceleratedSales.SandboxPlugins.Core.DataAccess;
using Microsoft.Dynamics.AcceleratedSales.SandboxPlugins.Core.Model;
using Microsoft.Dynamics.AcceleratedSales.SandboxPlugins.Core.Model.DefaultSort;
using Microsoft.Dynamics.AcceleratedSales.SandboxPlugins.Core.Model.Filter;
using Microsoft.Dynamics.AcceleratedSales.SandboxPlugins.Core.Model.Layout;
using Microsoft.Dynamics.AcceleratedSales.SandboxPlugins.Core.Model.Sort;
using Microsoft.Dynamics.AcceleratedSales.SandboxPlugins.Core.Model.Tags;
using Microsoft.Dynamics.AcceleratedSales.SandboxPlugins.Core.Model.ViewPicker;
using Microsoft.Dynamics.AcceleratedSales.SandboxPlugins.Core.Services.Commands;
using Microsoft.Dynamics.AcceleratedSales.SandboxPlugins.Core.Services.DataAggregation;
using Microsoft.Dynamics.AcceleratedSales.SandboxPlugins.Core.Services.Extensions;
using Microsoft.Dynamics.AcceleratedSales.SandboxPlugins.Core.Services.Views;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Metadata;
using Microsoft.Xrm.Sdk.Query;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

#nullable disable
namespace Microsoft.Dynamics.AcceleratedSales.SandboxPlugins.Core.Services
{
  public class WorklistDataProviderService
  {
    private readonly IDataStore dataStore;
    private readonly IAcceleratedSalesLogger logger;
    private readonly IEntityMetadataProvider entityMetadataProvider;
    private readonly Dictionary<string, IDataExtension> primaryExtensions;
    private readonly Dictionary<string, IDataExtension> relatedExtensions;
    private WorklistSettingsProviderService worklistSettingsProviderService;

    public WorklistDataProviderService(
      IDataStore dataStore,
      IAcceleratedSalesLogger logger,
      IEntityMetadataProvider metadataProvider,
      Dictionary<string, IDataExtension> primaryExtensions,
      Dictionary<string, IDataExtension> relatedExtensions)
    {
      this.dataStore = dataStore;
      this.logger = logger;
      this.entityMetadataProvider = metadataProvider;
      this.primaryExtensions = primaryExtensions ?? new Dictionary<string, IDataExtension>();
      this.relatedExtensions = relatedExtensions ?? new Dictionary<string, IDataExtension>();
      this.logger.AddCustomProperty("DataExtensions.PrimaryExtensions.Count", (object) primaryExtensions?.Count);
      this.logger.AddCustomProperty("DataExtensions.RelatedExtensions.Count", (object) relatedExtensions?.Count);
      this.worklistSettingsProviderService = new WorklistSettingsProviderService(dataStore, logger);
    }

    public WorklistDataResponse GetWorklistDataForSA(GetWorklistDataRequest requestPayload)
    {
      Stopwatch stopwatch = Stopwatch.StartNew();
      try
      {
        WorklistViewConfiguration viewConfiguration = WorklistViewConfiguration.FromRequestPayloadForSA(requestPayload, this.logger);
        SASettings saSettings = this.worklistSettingsProviderService.GetSASettings(viewConfiguration);
        ViewType viewType = ViewType.Sequence;
        List<ViewType> userAccessibleView = this.worklistSettingsProviderService.TryGetUserAccessibleView(saSettings);
        bool flag = this.dataStore.IsAppSettingEnabled("msdyn_IsSellerInsightsEnabled", this.logger);
        this.logger.AddCustomProperty("WDPS.GetWorklistDataForSA.SuggestionsAppSettingValue", (object) flag);
        saSettings.isEnabledForUser = userAccessibleView.Contains(ViewType.Sequence) && saSettings.isEnabledForOrganization || userAccessibleView.Contains(ViewType.Suggestion) & flag;
        Dictionary<string, EntityConfiguration> entityConfigurations1 = saSettings.settingsInstance.entityConfigurations;
        List<string> stringList1;
        if (entityConfigurations1 == null)
        {
          stringList1 = (List<string>) null;
        }
        else
        {
          Dictionary<string, EntityConfiguration>.KeyCollection keys = entityConfigurations1.Keys;
          stringList1 = keys != null ? keys.ToList<string>() : (List<string>) null;
        }
        if (stringList1 == null)
          stringList1 = new List<string>();
        List<string> stringList2 = stringList1;
        this.logger.AddCustomProperty("WDPS.GetWorklistDataForSA.entityList.Count", (object) stringList2.Count);
        Dictionary<string, IDataExtension> primaryExtensions = this.GetCustomPrimaryExtensions(stringList2);
        WorklistAdminConfiguration adminConfiguration = new WorklistViewConfigurationDataAccess(this.dataStore, this.logger).GetAdminConfiguration(viewType);
        adminConfiguration.FilterConfiguration = adminConfiguration.FilterConfiguration ?? SalesAcceleratorView.GetDefaultFilterConfiguration(flag);
        adminConfiguration.SortConfiguration = adminConfiguration.SortConfiguration ?? SalesAcceleratorView.GetDefaultSortConfiguration(flag);
        WorklistSellerConfiguration sellerConfiguration = new WorkQueueUserSettingsDataAccess(this.dataStore, this.logger).GetSellerConfiguration();
        Dictionary<string, DesignerConfiguration> cardLayout1 = new CardLayoutConfiguration(this.dataStore, this.logger, primaryExtensions).GetCardLayout(adminConfiguration, sellerConfiguration);
        FilterCommandConfiguration commandConfiguration1 = new FilterCommandConfiguration(this.logger, this.entityMetadataProvider);
        FilterConfiguration filterConfiguration = adminConfiguration.FilterConfiguration ?? commandConfiguration1.DefaultFilterConfiguration;
        commandConfiguration1.FillLocalizedNames(filterConfiguration, viewConfiguration.UserLocaleId, stringList2);
        SortCommandConfiguration commandConfiguration2 = new SortCommandConfiguration(this.logger, this.entityMetadataProvider);
        SortConfiguration sortConfiguration1 = adminConfiguration.SortConfiguration ?? commandConfiguration2.DefaultSortConfigurationForCOLA;
        commandConfiguration2.FillLocalizedNames(sortConfiguration1, viewConfiguration.UserLocaleId);
        if (!flag)
        {
          filterConfiguration = commandConfiguration1.GetFilterConfigurationForSA(adminConfiguration, sellerConfiguration, viewConfiguration.UserLocaleId, stringList2);
          sortConfiguration1 = commandConfiguration2.GetSortConfigurationForSA(adminConfiguration, sellerConfiguration, viewConfiguration.UserLocaleId);
        }
        bool isActiveTypeListEnabled = true;
        Dictionary<string, EntityConfiguration> entityConfigurations2 = saSettings.settingsInstance.entityConfigurations;
        bool isSuggestionEnabled = this.IsSuggestionEnabled(viewConfiguration.RelatedEntities, stringList2, userAccessibleView, flag);
        Dictionary<string, CommandMetadata> commandEntityMetadata = new CommandEntityMetadata(this.logger, this.entityMetadataProvider).GetCommandEntityMetadata(entityConfigurations2, isSuggestionEnabled, isActiveTypeListEnabled);
        DefaultSortConfiguration sortConfiguration2 = new DefaultSortCommandConfiguration(this.logger, this.entityMetadataProvider).GetSortConfiguration(adminConfiguration, viewConfiguration.UserLocaleId);
        Dictionary<string, List<Dictionary<string, string>>> relatedRecords = new Dictionary<string, List<Dictionary<string, string>>>();
        Dictionary<string, HashSet<string>> additionalAttributes = this.GetAdditionalAttributes(stringList2, filterConfiguration, sortConfiguration1, adminConfiguration?.TagsConfiguration, sortConfiguration2);
        Dictionary<string, HashSet<string>> formattingAttributes = SalesAcceleratorView.GetSkipFormattingAttributes(additionalAttributes, this.entityMetadataProvider, this.logger);
        this.logger.AddCustomProperty("WDPS.GetWorklistDataForSA.additionalAttributes.Count", (object) additionalAttributes.Keys.Count);
        Dictionary<string, List<QueryFilter>> additionalFilters = this.GetAdditionalFilters(viewConfiguration);
        Dictionary<string, QuerySort> additionalSort = this.GetAdditionalSort(viewConfiguration);
        List<string> list = viewConfiguration.RelatedEntities.Where<string>((Func<string, bool>) (entity => entity != "msdyn_salessuggestion")).ToList<string>();
        if (isSuggestionEnabled)
        {
          relatedRecords = this.GetSuggestionsData(viewConfiguration, additionalAttributes, additionalFilters, additionalSort, saSettings, formattingAttributes);
          this.logger.AddCustomProperty("WDPS.GetWorklistDataForSA.SuggestionsFetchPipeline.Duration", (object) stopwatch.ElapsedMilliseconds);
        }
        viewConfiguration.RelatedEntities = list;
        this.GetSequenceAndActivityData(viewConfiguration, additionalAttributes, additionalFilters, additionalSort, saSettings, formattingAttributes, stringList2, ref relatedRecords);
        this.logger.AddCustomProperty("WDPS.GetWorklistDataForSA.RelatedRecordsFetchPipeline.Duration", (object) stopwatch.ElapsedMilliseconds);
        Dictionary<string, HashSet<Guid>> regardingIds = this.GetRegardingIds(this.relatedExtensions, relatedRecords);
        PrimaryEntityPipeline primaryEntityPipeline = PrimaryEntityPipeline.Create(this.dataStore, this.logger, this.entityMetadataProvider, true).WithViewConfiguration(viewConfiguration).WithFetchFromRecordIdsOnly(true).WithCardLayout(cardLayout1).WithSettings(saSettings).WithExtensions(primaryExtensions, this.relatedExtensions).WithPrimaryRecordIds(regardingIds).WithAdditionalAttributes(additionalAttributes).WithSkipFormattingAttributes(formattingAttributes).PrepareSAQuery().RemoveLinkEntitiesWithNoAccess().FetchRecords().FormatRecords();
        this.logger.AddCustomProperty("WDPS.GetWorklistDataForSA.PrimaryFetchPipleine.Duration", (object) stopwatch.ElapsedMilliseconds);
        WorklistDataCommands worklistDataCommands = new WorklistDataCommands()
        {
          Filters = filterConfiguration,
          Sort = sortConfiguration1,
          CommandMetadata = commandEntityMetadata
        };
        WorklistDataResponse worklistDataForSa = new WorklistDataResponse();
        worklistDataForSa.Settings = saSettings;
        worklistDataForSa.Records = primaryEntityPipeline.PrimaryRecords;
        worklistDataForSa.RelatedRecords = relatedRecords;
        worklistDataForSa.Cardlayout = cardLayout1;
        worklistDataForSa.Commands = worklistDataCommands;
        worklistDataForSa.DefaultSortConfiguration = sortConfiguration2;
        worklistDataForSa.TagsConfig = adminConfiguration.TagsConfiguration;
        worklistDataForSa.ViewId = adminConfiguration.ViewId;
        worklistDataForSa.FetchXmlInUse = (string) null;
        AdminDesignerConfiguration cardLayout2 = adminConfiguration.CardLayout;
        worklistDataForSa.IsCardLayoutLocked = cardLayout2 == null || cardLayout2.IsLocked.GetValueOrDefault(true);
        return worklistDataForSa;
      }
      catch (Exception ex)
      {
        this.logger.LogError("WorklistDataProviderService.GetWorklistDataForSA.Failed.Exception", ex, callerName: nameof (GetWorklistDataForSA), callerFile: "C:\\__w\\1\\s\\solutions\\AcceleratedSales\\AcceleratedSalesSandboxPlugins\\Core\\Services\\WorklistDataProviderService.cs");
        if (!(ex is CrmException) && string.IsNullOrEmpty(ex.Message))
          throw new CrmException("WorklistDataProviderService: GetWorklistDataForSA failed.", ex, 1879506947);
        throw;
      }
      finally
      {
        stopwatch.Stop();
        this.logger.AddCustomProperty("WDPS.GetWorklistDataForSA.End.Duration", (object) stopwatch.ElapsedMilliseconds);
      }
    }

    public WorklistDataResponse GetWorklistData(GetWorklistDataRequest requestPayload)
    {
      Stopwatch stopwatch = Stopwatch.StartNew();
      try
      {
        WorklistViewConfiguration viewConfiguration = WorklistViewConfiguration.FromRequestPayload(requestPayload, this.logger);
        bool optimisationEnabled = viewConfiguration.ViewId == "entitylistview" && this.dataStore.IsFCSEnabled("SalesService.Workspace", "IsPrimaryEntityPipelineOptimisationEnabled");
        MetadataQueryParams metadataQueryParams = new MetadataQueryParams()
        {
          Attributes = new List<string>(),
          Entities = new List<string>(),
          Relationships = new List<string>(),
          LocalId = viewConfiguration.UserLocaleId,
          AllAttributesEntities = new List<string>()
        };
        WorklistDataResponse worklistData = new WorklistDataResponse();
        List<string> stringList = new List<string>();
        bool flag = false;
        if (viewConfiguration.PrimaryRecordIds == null || !optimisationEnabled || viewConfiguration.PrimaryRecordIds.Count == 0)
        {
          this.logger.AddCustomProperty("WDPS.GetWorklistData.isPrimaryRecordsCall", (object) true);
          WorklistAdminConfiguration adminConfiguration = new WorklistViewConfigurationDataAccess(this.dataStore, this.logger).GetAdminConfiguration(ViewType.Sequence);
          WorklistSellerConfiguration sellerConfiguration = new WorkQueueUserSettingsDataAccess(this.dataStore, this.logger).GetSellerConfiguration();
          Dictionary<string, DesignerConfiguration> cardLayout = new CardLayoutConfiguration(this.dataStore, this.logger, this.primaryExtensions).GetCardLayout(adminConfiguration, sellerConfiguration, optimisationEnabled, viewConfiguration.EntityName, viewConfiguration.Columns);
          PrimaryEntityPipeline primaryEntityPipeline1 = PrimaryEntityPipeline.Create(this.dataStore, this.logger, this.entityMetadataProvider).WithViewConfiguration(viewConfiguration).WithExtensions(this.primaryExtensions, this.relatedExtensions).WithCardLayout(cardLayout).PrepareInitialQuery().PrepareMetadataQuery(metadataQueryParams);
          metadataQueryParams.Entities.Add("msdyn_workqueuerecord");
          metadataQueryParams.Attributes.AddRange((IEnumerable<string>) WorkQueueRecordDataAccess.GetWorkQueueRecordAttributes());
          this.entityMetadataProvider.PreFetchEntityMetadata(metadataQueryParams);
          FilterConfiguration filterConfiguration = new FilterCommandConfiguration(this.logger, this.entityMetadataProvider).GetFilterConfiguration(adminConfiguration, sellerConfiguration, viewConfiguration.UserLocaleId);
          SortCommandConfiguration commandConfiguration = new SortCommandConfiguration(this.logger, this.entityMetadataProvider);
          bool isCustomEntity = !DataExtension.CheckForColaEntities(viewConfiguration.EntityName);
          SortConfiguration sortConfiguration = commandConfiguration.GetSortConfiguration(adminConfiguration, sellerConfiguration, viewConfiguration.UserLocaleId, isCustomEntity);
          PrimaryEntityPipeline primaryEntityPipeline2 = primaryEntityPipeline1.PrepareQuery().RemoveLinkEntitiesWithNoAccess().FetchRecords().FormatRecords();
          this.logger.LogWarning(string.Format("WDPS.GetWorklistData.PageInfo: PageNumber: {0} PageCount: {1}", (object) viewConfiguration.PaginationInfo?.PageNumber, (object) viewConfiguration.PaginationInfo?.PageCount), callerName: nameof (GetWorklistData), callerFile: "C:\\__w\\1\\s\\solutions\\AcceleratedSales\\AcceleratedSalesSandboxPlugins\\Core\\Services\\WorklistDataProviderService.cs");
          flag = this.IsActivityEntity(this.entityMetadataProvider.GetEntityMetadata(viewConfiguration.EntityName), viewConfiguration.EntityName);
          this.logger.LogWarning("isActivity: " + flag.ToString(), callerName: nameof (GetWorklistData), callerFile: "C:\\__w\\1\\s\\solutions\\AcceleratedSales\\AcceleratedSalesSandboxPlugins\\Core\\Services\\WorklistDataProviderService.cs");
          stringList = primaryEntityPipeline2.PrimaryEntities[viewConfiguration.EntityName].PrimaryEntityRecords.Select<Entity, string>((Func<Entity, string>) (r => r.Id.ToString())).ToList<string>();
          SASettings saSettings = this.worklistSettingsProviderService.GetSASettings(viewConfiguration);
          QueryExpression primaryQueryExpression = primaryEntityPipeline2.PrimaryQueryExpressions[viewConfiguration.EntityName];
          FilterGroup filterGroup;
          List<SortItem> sortItems;
          List<QuerySort> appliedSortItems;
          primaryQueryExpression.ToCommandGroup(this.logger, this.entityMetadataProvider, this.dataStore, viewConfiguration.UserLocaleId, out filterGroup, out sortItems, out appliedSortItems);
          filterConfiguration.Groups.Add(filterGroup);
          sortConfiguration.SortOptions.AddRange((IEnumerable<SortItem>) sortItems);
          string pagingCookie = primaryEntityPipeline2.PrimaryEntities[viewConfiguration.EntityName].PagingCookie;
          int recordsCount = primaryEntityPipeline2.PrimaryEntities[viewConfiguration.EntityName].RecordsCount;
          int totalRecordsCount = primaryEntityPipeline2.PrimaryEntities[viewConfiguration.EntityName].TotalRecordsCount;
          bool countLimitExceeded = primaryEntityPipeline2.PrimaryEntities[viewConfiguration.EntityName].TotalRecordsCountLimitExceeded;
          bool hasMoreRecords = primaryEntityPipeline2.PrimaryEntities[viewConfiguration.EntityName].HasMoreRecords;
          int pageCount = viewConfiguration.PaginationInfo.PageCount;
          int autoRefreshEnable = adminConfiguration.AutoRefreshEnable;
          int autoRefreshInterval = adminConfiguration.AutoRefreshInterval;
          WorklistDataCommands worklistDataCommands = new WorklistDataCommands()
          {
            Filters = filterConfiguration,
            Sort = sortConfiguration,
            Views = (Dictionary<string, List<SavedView>>) null,
            AppliedSort = new Dictionary<string, List<QuerySort>>()
            {
              {
                primaryQueryExpression.EntityName,
                appliedSortItems
              }
            }
          };
          if (string.IsNullOrEmpty(primaryEntityPipeline2.GetMergedFetchXml()))
            primaryEntityPipeline2.PrepareFetchXml();
          worklistData.Settings = saSettings;
          worklistData.Records = primaryEntityPipeline2.PrimaryRecords;
          worklistData.RelatedRecords = (Dictionary<string, List<Dictionary<string, string>>>) null;
          worklistData.Cardlayout = cardLayout;
          worklistData.Commands = worklistDataCommands;
          worklistData.FetchXml = primaryEntityPipeline2.GetFetchXmlUsed();
          worklistData.FetchXmlInUse = primaryEntityPipeline2.GetMergedFetchXml();
          worklistData.IsAutoRefreshEnable = autoRefreshEnable;
          worklistData.AutoRefreshInterval = autoRefreshInterval;
          worklistData.IsActivityTypeEntity = flag;
          if (viewConfiguration.PaginationInfo != null)
            worklistData.PaginationInfo = new PaginationInfo()
            {
              PagingCookie = pagingCookie == null ? string.Empty : pagingCookie,
              PageNumber = viewConfiguration.PaginationInfo.PageNumber,
              RecordsCount = recordsCount,
              PageCount = pageCount,
              HasMoreRecords = hasMoreRecords,
              TotalRecordsCount = totalRecordsCount,
              TotalRecordsCountLimitExceeded = countLimitExceeded
            };
        }
        if (!flag && (viewConfiguration.PrimaryRecordIds == null || !optimisationEnabled || viewConfiguration.PrimaryRecordIds.Count > 0))
        {
          metadataQueryParams.Entities.Add("activitypointer");
          metadataQueryParams.Attributes.AddRange((IEnumerable<string>) ActivitiesDataAccess.GetManualActivitiesAttributes());
          metadataQueryParams.Entities.Add("msdyn_sequencetargetstep");
          metadataQueryParams.Entities.Add("msdyn_sequencetarget");
          metadataQueryParams.Attributes.AddRange((IEnumerable<string>) SequenceDataAccess.GetSequenceStepAttributes());
          this.entityMetadataProvider.PreFetchEntityMetadata(metadataQueryParams);
          if (viewConfiguration.PrimaryRecordIds != null && viewConfiguration.PrimaryRecordIds.Count > 0)
          {
            this.logger.AddCustomProperty("WDPS.GetWorklistData.isRelatedRecordsCall", (object) true);
            stringList = viewConfiguration.PrimaryRecordIds;
          }
          else
            this.logger.AddCustomProperty("WDPS.GetWorklistData.isPrimaryAndRelatedRecordsCall", (object) true);
          Dictionary<string, List<Dictionary<string, string>>> dictionary = (Dictionary<string, List<Dictionary<string, string>>>) null;
          if (stringList.Count<string>() > 0)
            dictionary = RelatedEntityPipeline.Create(this.dataStore, this.logger, this.entityMetadataProvider).WithViewConfiguration(viewConfiguration).WithExtensions(this.relatedExtensions).WithPrimaryRecordIds(stringList).FetchRecords().FormatRecords().RelatedRecords;
          worklistData.RelatedRecords = dictionary;
        }
        return worklistData;
      }
      catch (Exception ex)
      {
        this.logger.LogError("WorklistDataProviderService.GetWorklistData.Failed.Exception", ex, callerName: nameof (GetWorklistData), callerFile: "C:\\__w\\1\\s\\solutions\\AcceleratedSales\\AcceleratedSalesSandboxPlugins\\Core\\Services\\WorklistDataProviderService.cs");
        if (!(ex is CrmException) && string.IsNullOrEmpty(ex.Message))
          throw new CrmException("WorklistDataProviderService: GetWorklistData failed.", ex, 1879506947);
        throw;
      }
      finally
      {
        stopwatch.Stop();
        this.logger.AddCustomProperty("WDPS.GetWorklistData.Completed.Duration", (object) stopwatch.ElapsedMilliseconds);
      }
    }

    public WorklistFilteredDataResponse GetWorklistFilteredData(
      GetWorklistFilteredDataRequest requestPayload)
    {
      Stopwatch stopwatch = Stopwatch.StartNew();
      try
      {
        this.logger.Execute("AcceleratedSales.GetWorklistFilteredData.Logging", (Action) (() => this.logger.AddCustomProperty("WorklistDataProviderService.GetWorklistFilteredData.RequestPayload", (object) JsonConvert.SerializeObject((object) requestPayload, Formatting.None, new JsonSerializerSettings()
        {
          NullValueHandling = NullValueHandling.Ignore
        }))));
        WorklistViewConfiguration viewConfig = WorklistViewConfiguration.FromFilteredDataRequestPayload(requestPayload, this.logger);
        this.logger.Execute("AcceleratedSales.GetWorklistFilteredData.Logging", (Action) (() => this.logger.AddCustomProperty("WorklistDataProviderService.GetWorklistFilteredData.ViewConfig", (object) JsonConvert.SerializeObject((object) viewConfig, Formatting.None, new JsonSerializerSettings()
        {
          NullValueHandling = NullValueHandling.Ignore
        }))));
        MetadataQueryParams metadataQueryParams = new MetadataQueryParams()
        {
          Attributes = new List<string>(),
          Entities = new List<string>(),
          Relationships = new List<string>(),
          LocalId = viewConfig.UserLocaleId,
          AllAttributesEntities = new List<string>()
        };
        WorklistFilteredDataResponse worklistFilteredData = new WorklistFilteredDataResponse();
        List<string> source = new List<string>();
        bool flag = false;
        bool optimisationEnabled = viewConfig.ViewId == "entitylistview" && this.dataStore.IsFCSEnabled("SalesService.Workspace", "IsPrimaryEntityPipelineOptimisationEnabled");
        if (viewConfig.PrimaryRecordIds == null || !optimisationEnabled || viewConfig.PrimaryRecordIds.Count == 0)
        {
          Dictionary<string, DesignerConfiguration> cardLayout = new CardLayoutConfiguration(this.dataStore, this.logger, this.primaryExtensions).GetCardLayout(new WorklistViewConfigurationDataAccess(this.dataStore, this.logger).GetAdminConfiguration(ViewType.Sequence), new WorkQueueUserSettingsDataAccess(this.dataStore, this.logger).GetSellerConfiguration(), optimisationEnabled, viewConfig.EntityName);
          PrimaryEntityPipeline primaryEntityPipeline1 = PrimaryEntityPipeline.Create(this.dataStore, this.logger, this.entityMetadataProvider).WithViewConfiguration(viewConfig).WithExtensions(this.primaryExtensions, this.relatedExtensions).WithCardLayout(cardLayout).WithFilters(viewConfig.Filters).WithSort(viewConfig.Sort).WithSearch(viewConfig.Search).PrepareInitialQuery().PrepareMetadataQuery(metadataQueryParams);
          metadataQueryParams.Entities.Add("msdyn_workqueuerecord");
          metadataQueryParams.Attributes.AddRange((IEnumerable<string>) WorkQueueRecordDataAccess.GetWorkQueueRecordAttributes());
          this.entityMetadataProvider.PreFetchEntityMetadata(metadataQueryParams);
          PrimaryEntityPipeline primaryEntityPipeline2 = primaryEntityPipeline1.PrepareQuery().RemoveLinkEntitiesWithNoAccess().FetchRecords().FormatRecords();
          flag = this.IsActivityEntity(this.entityMetadataProvider.GetEntityMetadata(viewConfig.EntityName), viewConfig.EntityName);
          this.logger.LogWarning("isActivity: " + flag.ToString(), callerName: nameof (GetWorklistFilteredData), callerFile: "C:\\__w\\1\\s\\solutions\\AcceleratedSales\\AcceleratedSalesSandboxPlugins\\Core\\Services\\WorklistDataProviderService.cs");
          source = primaryEntityPipeline2.PrimaryEntities[viewConfig.EntityName].PrimaryEntityRecords.Select<Entity, string>((Func<Entity, string>) (r => r.Id.ToString())).ToList<string>();
          string pagingCookie = primaryEntityPipeline2.PrimaryEntities[viewConfig.EntityName].PagingCookie;
          int recordsCount = primaryEntityPipeline2.PrimaryEntities[viewConfig.EntityName].RecordsCount;
          int totalRecordsCount = primaryEntityPipeline2.PrimaryEntities[viewConfig.EntityName].TotalRecordsCount;
          bool countLimitExceeded = primaryEntityPipeline2.PrimaryEntities[viewConfig.EntityName].TotalRecordsCountLimitExceeded;
          bool hasMoreRecords = primaryEntityPipeline2.PrimaryEntities[viewConfig.EntityName].HasMoreRecords;
          int pageCount = viewConfig.PaginationInfo.PageCount;
          if (string.IsNullOrEmpty(primaryEntityPipeline2.GetMergedFetchXml()))
            primaryEntityPipeline2.PrepareFetchXml();
          worklistFilteredData.Records = primaryEntityPipeline2.PrimaryRecords;
          worklistFilteredData.RelatedRecords = (Dictionary<string, List<Dictionary<string, string>>>) null;
          worklistFilteredData.FetchXml = primaryEntityPipeline2.GetFetchXmlUsed();
          worklistFilteredData.FetchXmlInUse = primaryEntityPipeline2.GetMergedFetchXml();
          worklistFilteredData.IsActivityTypeEntity = flag;
          if (viewConfig.PaginationInfo != null)
            worklistFilteredData.PaginationInfo = new PaginationInfo()
            {
              PagingCookie = pagingCookie == null ? string.Empty : pagingCookie,
              PageNumber = viewConfig.PaginationInfo.PageNumber,
              RecordsCount = recordsCount,
              PageCount = pageCount,
              HasMoreRecords = hasMoreRecords,
              TotalRecordsCount = totalRecordsCount,
              TotalRecordsCountLimitExceeded = countLimitExceeded
            };
        }
        if (!flag && (viewConfig.PrimaryRecordIds == null || !optimisationEnabled || viewConfig.PrimaryRecordIds.Count > 0))
        {
          metadataQueryParams.Entities.Add("activitypointer");
          metadataQueryParams.Attributes.AddRange((IEnumerable<string>) ActivitiesDataAccess.GetManualActivitiesAttributes());
          metadataQueryParams.Entities.Add("msdyn_sequencetargetstep");
          metadataQueryParams.Entities.Add("msdyn_sequencetarget");
          metadataQueryParams.Attributes.AddRange((IEnumerable<string>) SequenceDataAccess.GetSequenceStepAttributes());
          this.entityMetadataProvider.PreFetchEntityMetadata(metadataQueryParams);
          if (viewConfig.PrimaryRecordIds != null && viewConfig.PrimaryRecordIds.Count > 0)
            source = viewConfig.PrimaryRecordIds;
          Dictionary<string, List<Dictionary<string, string>>> dictionary = (Dictionary<string, List<Dictionary<string, string>>>) null;
          if (source.Count<string>() > 0)
            dictionary = RelatedEntityPipeline.Create(this.dataStore, this.logger, this.entityMetadataProvider).WithViewConfiguration(viewConfig).WithExtensions(this.relatedExtensions).WithPrimaryRecordIds(source.ToList<string>()).WithFilters(viewConfig.Filters).WithSort(viewConfig.Sort).FetchRecords().FormatRecords().RelatedRecords;
          worklistFilteredData.RelatedRecords = dictionary;
        }
        return worklistFilteredData;
      }
      catch (Exception ex)
      {
        this.logger.LogError("WorklistDataProviderService.GetWorklistFilteredData.Failed.Exception", ex, callerName: nameof (GetWorklistFilteredData), callerFile: "C:\\__w\\1\\s\\solutions\\AcceleratedSales\\AcceleratedSalesSandboxPlugins\\Core\\Services\\WorklistDataProviderService.cs");
        if (!(ex is CrmException) && string.IsNullOrEmpty(ex.Message))
          throw new CrmException("WorklistDataProviderService: GetWorklistFilteredData failed.", ex, 1879506947);
        throw;
      }
      finally
      {
        stopwatch.Stop();
        this.logger.AddCustomProperty("WDPS.GetWorklistFilteredData.Completed.Duration", (object) stopwatch.ElapsedMilliseconds);
      }
    }

    public GetMergedViewXmlResponse GetMergedViewXml(GetWorklistFilteredDataRequest requestPayload)
    {
      this.logger.AddCustomProperty("WorklistDataProviderService.GetMergedViewXmlResponse.Start", (object) "Success");
      Stopwatch stopwatch = Stopwatch.StartNew();
      try
      {
        this.logger.AddCustomProperty("WorklistDataProviderService.GetMergedViewXmlResponse.RequestPayload", (object) JsonConvert.SerializeObject((object) requestPayload, Formatting.None, new JsonSerializerSettings()
        {
          NullValueHandling = NullValueHandling.Ignore
        }));
        WorklistViewConfiguration config = WorklistViewConfiguration.FromFilteredDataRequestPayload(requestPayload, this.logger);
        this.logger.AddCustomProperty("WorklistDataProviderService.GetMergedViewXmlResponse.ViewConfig", (object) JsonConvert.SerializeObject((object) config, Formatting.None, new JsonSerializerSettings()
        {
          NullValueHandling = NullValueHandling.Ignore
        }));
        MetadataQueryParams metadataQueryParams = new MetadataQueryParams()
        {
          Attributes = new List<string>(),
          Entities = new List<string>(),
          Relationships = new List<string>(),
          LocalId = config.UserLocaleId,
          AllAttributesEntities = new List<string>()
        };
        PrimaryEntityPipeline primaryEntityPipeline1 = PrimaryEntityPipeline.Create(this.dataStore, this.logger, this.entityMetadataProvider, true).WithViewConfiguration(config).WithExtensions(this.primaryExtensions, this.relatedExtensions).WithFilters(config.Filters).WithSort(config.Sort).WithSearch(config.Search).PrepareInitialQuery().PrepareMetadataQuery(metadataQueryParams);
        this.entityMetadataProvider.PreFetchEntityMetadata(metadataQueryParams);
        PrimaryEntityPipeline primaryEntityPipeline2 = primaryEntityPipeline1.PrepareQuery().PrepareFetchXml().PrepareLayoutXml();
        return new GetMergedViewXmlResponse()
        {
          LayoutXml = primaryEntityPipeline2.GetLayoutXml(),
          FetchXml = primaryEntityPipeline2.GetMergedFetchXml()
        };
      }
      catch (Exception ex)
      {
        this.logger.AddCustomProperty("WorklistDataProviderService.GetMergedViewXmlResponse.Exception", (object) ex);
        this.logger.LogError("WorklistDataProviderService.GetMergedViewXmlResponse: failed", callerName: nameof (GetMergedViewXml), callerFile: "C:\\__w\\1\\s\\solutions\\AcceleratedSales\\AcceleratedSalesSandboxPlugins\\Core\\Services\\WorklistDataProviderService.cs");
        if (!(ex is CrmException))
          throw new CrmException("WorklistDataProviderService: GetMergedViewXmlResponse failed.", ex, 1879506947);
        throw;
      }
      finally
      {
        stopwatch.Stop();
        this.logger.AddCustomProperty("WorklistDataProviderService.GetMergedViewXmlResponse.Duration", (object) stopwatch.ElapsedMilliseconds);
        this.logger.AddCustomProperty("WorklistDataProviderService.GetMergedViewXmlResponse.End", (object) "Success");
        this.logger.LogWarning("WorklistDataProviderService.GetMergedViewXmlResponse: completed.", callerName: nameof (GetMergedViewXml), callerFile: "C:\\__w\\1\\s\\solutions\\AcceleratedSales\\AcceleratedSalesSandboxPlugins\\Core\\Services\\WorklistDataProviderService.cs");
      }
    }

    private Dictionary<string, IDataExtension> GetCustomPrimaryExtensions(List<string> entities)
    {
      DataExtension dataExtensions = new DataExtension();
      entities.ForEach((Action<string>) (entity =>
      {
        if (!(entity != "msdyn_salessuggestion") || DataExtension.CheckForColaEntities(entity))
          return;
        dataExtensions.AddCustomExtensions(entity, this.entityMetadataProvider.GetEntityMetadata(entity)?.PrimaryNameAttribute);
      }));
      return dataExtensions.GetPrimaryExtensions();
    }

    private bool IsSuggestionEnabled(
      List<string> relatedEntities,
      List<string> entityList,
      List<ViewType> viewList,
      bool isSuggestionAppSettingEnabled)
    {
      return ((!relatedEntities.Contains("msdyn_salessuggestion") ? 0 : (entityList.Contains("account") ? 1 : 0)) & (isSuggestionAppSettingEnabled ? 1 : 0)) != 0 && viewList.Contains(ViewType.Suggestion);
    }

    private WorklistDataResponse GetWorklistDataResponseForInvalidSettings(SASettings settings)
    {
      return new WorklistDataResponse()
      {
        Settings = settings
      };
    }

    private Dictionary<string, HashSet<Guid>> GetRegardingIds(
      Dictionary<string, IDataExtension> relatedExtensions,
      Dictionary<string, List<Dictionary<string, string>>> relatedRecords)
    {
      Dictionary<string, HashSet<Guid>> result = new Dictionary<string, HashSet<Guid>>();
      foreach (KeyValuePair<string, List<Dictionary<string, string>>> relatedRecord in relatedRecords)
      {
        string entityName = relatedRecord.Key;
        List<Dictionary<string, string>> dictionaryList = relatedRecord.Value;
        if (relatedExtensions.ContainsKey(entityName))
          dictionaryList.ForEach((Action<Dictionary<string, string>>) (record =>
          {
            EntityExtensions.AttributeValue regardingObjectForRecord = relatedExtensions[entityName].GetRegardingObjectForRecord(record);
            if (regardingObjectForRecord.Entity == null || regardingObjectForRecord.Value == null)
              return;
            if (!result.ContainsKey(regardingObjectForRecord.Entity.ToString()))
              result.Add(regardingObjectForRecord.Entity.ToString(), new HashSet<Guid>());
            Guid result1;
            if (Guid.TryParse(regardingObjectForRecord.Value.ToString(), out result1))
              result[regardingObjectForRecord.Entity.ToString()].Add(result1);
          }));
      }
      return result;
    }

    private Dictionary<string, HashSet<string>> GetAdditionalAttributes(
      List<string> entityList,
      FilterConfiguration filters,
      SortConfiguration sortItems,
      TagsConfiguration tagsConfig,
      DefaultSortConfiguration defaultSortConfig)
    {
      Dictionary<string, HashSet<string>> additionalAttributes = SalesAcceleratorView.GetAdditionalAttributes(filters, sortItems, tagsConfig, defaultSortConfig, this.entityMetadataProvider);
      SalesAcceleratorView.AddDefaultAttributes(entityList, this.entityMetadataProvider, ref additionalAttributes);
      return additionalAttributes;
    }

    private Dictionary<string, List<QueryFilter>> GetAdditionalFilters(
      WorklistViewConfiguration viewConfig)
    {
      return SalesAcceleratorView.GetDefaultFilters(viewConfig.RelatedEntities).MergeQueryFilters(viewConfig.Filters);
    }

    private Dictionary<string, QuerySort> GetAdditionalSort(WorklistViewConfiguration viewConfig)
    {
      return SalesAcceleratorView.GetDefaultSortItems(viewConfig.RelatedEntities).MergeQuerySort(viewConfig.Sort);
    }

    private Dictionary<string, List<Dictionary<string, string>>> GetSuggestionsData(
      WorklistViewConfiguration viewConfig,
      Dictionary<string, HashSet<string>> additionalAttributes,
      Dictionary<string, List<QueryFilter>> additionalFilters,
      Dictionary<string, QuerySort> additionalSortItems,
      SASettings settings,
      Dictionary<string, HashSet<string>> skipFormattingAttributes)
    {
      viewConfig.RelatedEntities = new List<string>()
      {
        "msdyn_salessuggestion"
      };
      RelatedEntityPipeline relatedEntityPipeline1 = RelatedEntityPipeline.Create(this.dataStore, this.logger, this.entityMetadataProvider).WithViewConfiguration(viewConfig).WithFilters(additionalFilters).WithSort(additionalSortItems).WithExtensions(this.relatedExtensions).WithAdditionalAttributes(additionalAttributes).WithSkipFormattingAttributes(skipFormattingAttributes).WithSettings(settings).FetchSARecords().FormatRecords();
      Dictionary<string, List<Dictionary<string, string>>> relatedRecords = relatedEntityPipeline1.RelatedRecords;
      List<Entity> relatedEntity = relatedEntityPipeline1.RelatedEntityMap["msdyn_salessuggestion"];
      viewConfig.RelatedEntities = new List<string>()
      {
        "msdyn_sequencetargetstep",
        "activitypointer"
      };
      // ISSUE: explicit non-virtual call
      if (relatedEntity != null && __nonvirtual (relatedEntity.Count) > 0)
      {
        RelatedEntityPipeline relatedEntityPipeline2 = RelatedEntityPipeline.Create(this.dataStore, this.logger, this.entityMetadataProvider).WithUserOwnership(false).WithViewConfiguration(viewConfig).WithSort(additionalSortItems).WithExtensions(this.relatedExtensions).WithPrimaryRecordIds(relatedEntity.Select<Entity, string>((Func<Entity, string>) (entity => entity.Id.ToString())).ToList<string>()).WithSettings(settings).WithAdditionalAttributes(additionalAttributes).WithSkipFormattingAttributes(skipFormattingAttributes);
        foreach (KeyValuePair<string, List<Dictionary<string, string>>> relatedRecord in relatedEntityPipeline2.WithEntityList(new List<string>()
        {
          "msdyn_salessuggestion"
        }).FetchSARecords().FormatRecords().RelatedRecords)
        {
          if (relatedRecord.Value.Count > 0)
            relatedRecords.Add(relatedRecord.Key, relatedRecord.Value);
        }
        relatedRecords = SalesAcceleratorView.AggregateRelatedRecordsByDueDate(this.relatedExtensions, relatedRecords, viewConfig.ResultSize);
      }
      return relatedRecords;
    }

    private void GetSequenceAndActivityData(
      WorklistViewConfiguration viewConfig,
      Dictionary<string, HashSet<string>> additionalAttributes,
      Dictionary<string, List<QueryFilter>> additionalFilters,
      Dictionary<string, QuerySort> additionalSortItems,
      SASettings settings,
      Dictionary<string, HashSet<string>> skipFormattingAttributes,
      List<string> entityList,
      ref Dictionary<string, List<Dictionary<string, string>>> relatedRecords)
    {
      entityList.Remove("msdyn_salessuggestion");
      viewConfig.RelatedEntities.Remove("msdyn_salessuggestion");
      foreach (KeyValuePair<string, List<Dictionary<string, string>>> keyValuePair in SalesAcceleratorView.AggregateRelatedRecordsByDueDate(this.relatedExtensions, RelatedEntityPipeline.Create(this.dataStore, this.logger, this.entityMetadataProvider).WithViewConfiguration(viewConfig).WithFilters(additionalFilters).WithSort(additionalSortItems).WithExtensions(this.relatedExtensions).WithEntityList(entityList).WithSettings(settings).WithAdditionalAttributes(additionalAttributes).WithSkipFormattingAttributes(skipFormattingAttributes).FetchSARecords().FormatRecords().RelatedRecords, viewConfig.ResultSize))
      {
        if (!relatedRecords.ContainsKey(keyValuePair.Key))
          relatedRecords.Add(keyValuePair.Key, keyValuePair.Value);
        else
          relatedRecords[keyValuePair.Key].AddRange((IEnumerable<Dictionary<string, string>>) keyValuePair.Value);
      }
    }

    private bool IsActivityEntity(EntityMetadata entityMetadata, string entityName)
    {
      return ((bool?) entityMetadata?.IsActivity).GetValueOrDefault() || entityName == "activitypointer" || entityMetadata?.PrimaryIdAttribute == "activityid";
    }
  }
}
