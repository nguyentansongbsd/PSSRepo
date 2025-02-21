// Decompiled with JetBrains decompiler
// Type: Microsoft.Dynamics.AcceleratedSales.SandboxPlugins.Core.Services.FilterCommandConfiguration
// Assembly: Microsoft.Dynamics.AcceleratedSales.SandboxPlugins, Version=1.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35
// MVID: 7BBE7132-7E30-4424-B38D-27667CFBCEFA
// Assembly location: C:\Users\ngoct\Downloads\New folder\Microsoft.Dynamics.AcceleratedSales.SandboxPlugins_1.0.0.0.dll

using Microsoft.Dynamics.AcceleratedSales.SandboxPlugins.Core.Model;
using Microsoft.Dynamics.AcceleratedSales.SandboxPlugins.Core.Model.Filter;
using Microsoft.Dynamics.AcceleratedSales.SandboxPlugins.Localization;
using Microsoft.Xrm.Sdk.Metadata;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

#nullable disable
namespace Microsoft.Dynamics.AcceleratedSales.SandboxPlugins.Core.Services
{
  public class FilterCommandConfiguration
  {
    private const string DefaultConfiguration = "{\"Version\":\"d74438b3-b2e4-446e-b894-914ce2caa741\",\"Groups\":[{\"Id\":\"a00000a0-a0a0-000a-a000-000aa0aaa000\",\"Name\":\"Default filters\",\"Position\":0,\"IsDefaultSelected\":true,\"GroupType\":0,\"Visibility\":true,\"Filters\":[{\"Id\":\"a00000a0-a0a0-000a-a000-000aa0aaa001\",\"Name\":\"Unopened\",\"Filtertype\":0,\"IsCustomName\":true,\"Position\":0,\"Icon\":\"Hide3\",\"Visibility\":true,\"Metadata\":{\"msdyn_workqueuerecord\":{\"isRead\":{\"ControlType\":4}}}},{\"Id\":\"a00000a0-a0a0-000a-a000-000aa0aaa002\",\"Name\":\"Followed\",\"Filtertype\":0,\"IsCustomName\":true,\"Position\":1,\"Icon\":\"FavoriteStarFill\",\"Visibility\":true,\"Metadata\":{\"msdyn_workqueuerecord\":{\"postFollowId\":{\"ControlType\":4}}}},{\"Id\":\"a00000a0-a0a0-000a-a000-000aa0aaa003\",\"Name\":\"Due by\",\"Filtertype\":0,\"IsCustomName\":true,\"Position\":2,\"Visibility\":true,\"Metadata\":{\"msdyn_workqueuerecord\":{\"dueTime\":{\"ControlType\":8}}}},{\"Id\":\"a00000a0-a0a0-000a-a000-000aa0aaa004\",\"Name\":\"Record type\",\"Filtertype\":0,\"IsCustomName\":true,\"Position\":3,\"Visibility\":true,\"Metadata\":{\"msdyn_workqueuerecord\":{\"entityType\":{\"ControlType\":3}}}},{\"Id\":\"a00000a0-a0a0-000a-a000-000aa0aaa005\",\"Name\":\"Activity type\",\"Filtertype\":0,\"IsCustomName\":true,\"Position\":4,\"Visibility\":true,\"Metadata\":{\"msdyn_workqueuerecord\":{\"nextActionType\":{\"ControlType\":3}}}}]},{\"Id\":\"a10000a0-a0a0-000a-a000-000aa0aaa000\",\"Name\":\"More filters\",\"Position\":1,\"IsDefaultSelected\":false,\"GroupType\":1,\"Visibility\":true,\"Filters\":[]}]}";
    private readonly IAcceleratedSalesLogger logger;
    private readonly IEntityMetadataProvider metadataProvider;

    public FilterCommandConfiguration(
      IAcceleratedSalesLogger logger,
      IEntityMetadataProvider metadataProvider)
    {
      this.logger = logger ?? throw new ArgumentNullException(nameof (logger));
      this.metadataProvider = metadataProvider ?? throw new ArgumentNullException(nameof (metadataProvider));
    }

    public FilterConfiguration DefaultFilterConfiguration
    {
      get
      {
        return JsonConvert.DeserializeObject<FilterConfiguration>("{\"Version\":\"d74438b3-b2e4-446e-b894-914ce2caa741\",\"Groups\":[{\"Id\":\"a00000a0-a0a0-000a-a000-000aa0aaa000\",\"Name\":\"Default filters\",\"Position\":0,\"IsDefaultSelected\":true,\"GroupType\":0,\"Visibility\":true,\"Filters\":[{\"Id\":\"a00000a0-a0a0-000a-a000-000aa0aaa001\",\"Name\":\"Unopened\",\"Filtertype\":0,\"IsCustomName\":true,\"Position\":0,\"Icon\":\"Hide3\",\"Visibility\":true,\"Metadata\":{\"msdyn_workqueuerecord\":{\"isRead\":{\"ControlType\":4}}}},{\"Id\":\"a00000a0-a0a0-000a-a000-000aa0aaa002\",\"Name\":\"Followed\",\"Filtertype\":0,\"IsCustomName\":true,\"Position\":1,\"Icon\":\"FavoriteStarFill\",\"Visibility\":true,\"Metadata\":{\"msdyn_workqueuerecord\":{\"postFollowId\":{\"ControlType\":4}}}},{\"Id\":\"a00000a0-a0a0-000a-a000-000aa0aaa003\",\"Name\":\"Due by\",\"Filtertype\":0,\"IsCustomName\":true,\"Position\":2,\"Visibility\":true,\"Metadata\":{\"msdyn_workqueuerecord\":{\"dueTime\":{\"ControlType\":8}}}},{\"Id\":\"a00000a0-a0a0-000a-a000-000aa0aaa004\",\"Name\":\"Record type\",\"Filtertype\":0,\"IsCustomName\":true,\"Position\":3,\"Visibility\":true,\"Metadata\":{\"msdyn_workqueuerecord\":{\"entityType\":{\"ControlType\":3}}}},{\"Id\":\"a00000a0-a0a0-000a-a000-000aa0aaa005\",\"Name\":\"Activity type\",\"Filtertype\":0,\"IsCustomName\":true,\"Position\":4,\"Visibility\":true,\"Metadata\":{\"msdyn_workqueuerecord\":{\"nextActionType\":{\"ControlType\":3}}}}]},{\"Id\":\"a10000a0-a0a0-000a-a000-000aa0aaa000\",\"Name\":\"More filters\",\"Position\":1,\"IsDefaultSelected\":false,\"GroupType\":1,\"Visibility\":true,\"Filters\":[]}]}");
      }
    }

    public FilterConfiguration GetFilterConfiguration(
      WorklistAdminConfiguration adminConfig,
      WorklistSellerConfiguration sellerConfig,
      int localeId,
      List<string> entityList = null)
    {
      Stopwatch stopwatch = Stopwatch.StartNew();
      FilterConfiguration config = this.DefaultFilterConfiguration;
      if (adminConfig?.FilterConfiguration != null)
        FilterCommandConfiguration.TryMerge(adminConfig.FilterConfiguration, sellerConfig?.FilterConfiguration, out config);
      this.FillLocalizedNames(config, localeId, entityList);
      stopwatch.Stop();
      this.logger.LogWarning("FilterCommandConfiguration.GetFilterConfiguration.End.Duration: " + stopwatch.ElapsedMilliseconds.ToString(), callerName: nameof (GetFilterConfiguration), callerFile: "C:\\__w\\1\\s\\solutions\\AcceleratedSales\\AcceleratedSalesSandboxPlugins\\Core\\Services\\Commands\\FilterCommandConfiguration.cs");
      return config;
    }

    public FilterConfiguration GetFilterConfigurationForSA(
      WorklistAdminConfiguration adminConfig,
      WorklistSellerConfiguration sellerConfig,
      int localeId,
      List<string> entityList)
    {
      if (adminConfig.FilterConfiguration == null)
        adminConfig.FilterConfiguration = this.DefaultFilterConfiguration;
      else
        adminConfig.FilterConfiguration.Groups = adminConfig.FilterConfiguration.Groups.Select<FilterGroup, FilterGroup>((Func<FilterGroup, FilterGroup>) (group =>
        {
          FilterGroup filterGroup = group;
          List<FilterItem> filters = group.Filters;
          List<FilterItem> list = filters != null ? filters.Where<FilterItem>((Func<FilterItem, bool>) (filter => filter.Visibility)).ToList<FilterItem>() : (List<FilterItem>) null;
          filterGroup.Filters = list;
          return group;
        })).Where<FilterGroup>((Func<FilterGroup, bool>) (group => group.Visibility && group.Filters != null && group.Filters.Count > 0)).ToList<FilterGroup>();
      return this.GetFilterConfiguration(adminConfig, sellerConfig, localeId, entityList);
    }

    public void FillLocalizedNames(
      FilterConfiguration filterConfig,
      int localeId,
      List<string> entityList = null)
    {
      Stopwatch stopwatch = Stopwatch.StartNew();
      this.logger.AddCustomProperty("FilterCommandConfiguration.FillLocalizedNames.Start", (object) "Success");
      IEnumerable<FilterItem> filterItems = filterConfig.Groups.Where<FilterGroup>((Func<FilterGroup, bool>) (g => g.Visibility)).SelectMany<FilterGroup, FilterItem>((Func<FilterGroup, IEnumerable<FilterItem>>) (g => g.Filters.Where<FilterItem>((Func<FilterItem, bool>) (f => f.Visibility))));
      this.logger.AddCustomProperty("CustomFiltersAdded", (object) JsonConvert.SerializeObject((object) filterItems));
      foreach (FilterItem filter in filterItems)
        FilterCommandConfiguration.TryUpdateProperties(filter, this.metadataProvider, localeId, entityList);
      stopwatch.Stop();
      this.logger.LogWarning("FilterCommandConfiguration.FillLocalizedNames.End.Duration" + stopwatch.ElapsedMilliseconds.ToString(), callerName: nameof (FillLocalizedNames), callerFile: "C:\\__w\\1\\s\\solutions\\AcceleratedSales\\AcceleratedSalesSandboxPlugins\\Core\\Services\\Commands\\FilterCommandConfiguration.cs");
    }

    private static bool TryMerge(
      FilterConfiguration adminFilter,
      SellerFilterConfiguration sellerFilter,
      out FilterConfiguration config)
    {
      config = new FilterConfiguration();
      config.Version = adminFilter.Version;
      config.Groups = new List<FilterGroup>();
      if (sellerFilter == null || !adminFilter.Version.Equals(sellerFilter.View.AdminConfigVersion, StringComparison.Ordinal))
      {
        config = adminFilter;
        return false;
      }
      foreach (FilterGroup group in adminFilter.Groups)
      {
        FilterGroup filterGroup = group;
        SellerFilterGroup sellerFilterGroup;
        if (sellerFilter.View.Groups.TryGetValue(group.Id.ToString(), out sellerFilterGroup))
        {
          filterGroup.Position = sellerFilterGroup.Position;
          filterGroup.Visibility = sellerFilterGroup.Visibility;
        }
        foreach (FilterItem filter in filterGroup.Filters)
        {
          SellerFilterItem sellerFilterItem;
          if (sellerFilter.View.Filters.TryGetValue(filter.Id.ToString(), out sellerFilterItem))
          {
            filter.Position = sellerFilterItem.Position;
            filter.Visibility = sellerFilterItem.Visibility;
          }
        }
        config.Groups.Add(filterGroup);
      }
      return true;
    }

    private static void TryUpdateProperties(
      FilterItem filter,
      IEntityMetadataProvider metadataProvider,
      int localeId,
      List<string> entityList)
    {
      string str = filter.Metadata.Keys.FirstOrDefault<string>();
      Dictionary<string, FilterProperties> dictionary1 = filter.Metadata.Values.FirstOrDefault<Dictionary<string, FilterProperties>>();
      string key = dictionary1 != null ? dictionary1.Keys.FirstOrDefault<string>() : (string) null;
      Dictionary<string, FilterProperties> dictionary2 = filter.Metadata.Values.FirstOrDefault<Dictionary<string, FilterProperties>>();
      FilterProperties filterProperties = (dictionary2 != null ? dictionary2.Values.FirstOrDefault<FilterProperties>() : (FilterProperties) null) ?? new FilterProperties();
      if (string.IsNullOrEmpty(str) || string.IsNullOrEmpty(key))
        return;
      if (str == "msdyn_workqueuerecord")
      {
        if (key == "dueTime")
          filterProperties.Options = new Dictionary<string, string>()
          {
            {
              "ByToday",
              Labels.DuebyTypeFilterTodayOptionTitle
            },
            {
              "FromTomorrow",
              Labels.DuebyTypeFilterTomorrowOptionTitle
            },
            {
              "Overdue",
              Labels.DuebyTypeFilterOverdueOptionTitle
            }
          };
        if (key == "nextActionType")
          filterProperties.Options = new Dictionary<string, string>()
          {
            {
              "task",
              Labels.ActivityTypeFilterTaskOptionTitle
            },
            {
              "email",
              Labels.ActivityTypeFilterEmailOptionTitle
            },
            {
              "phonecall",
              Labels.ActivityTypeFilterPhonecallOptionTitle
            },
            {
              "appointment",
              Labels.ActivityTypeFilterAppointmentOptionTitle
            },
            {
              "linkedinaction",
              Labels.ActivityTypeFilterLinkedinOptionTitle
            },
            {
              "sms",
              Labels.ActivityTypeFilterSMSOptionTitle
            }
          };
        if (key == "entityType")
          filterProperties.Options = entityList != null ? entityList.ToDictionary<string, string, string>((Func<string, string>) (entity => entity), (Func<string, string>) (entity =>
          {
            EntityMetadata entityMetadata = metadataProvider.GetEntityMetadata(entity);
            return entityMetadata == null ? (string) null : entityMetadata.DisplayName.GetLocalizedLabel(localeId);
          })) : (Dictionary<string, string>) null;
        if (key == "workQueueRecordType")
          filterProperties.Options = new Dictionary<string, string>()
          {
            {
              "sequence",
              Labels.SequenceLabel
            },
            {
              "insight",
              Labels.Insightlabel
            }
          };
        filter.Metadata[str][key].Options = filterProperties.Options ?? new Dictionary<string, string>();
      }
      else
      {
        EntityMetadata entityMetadata = metadataProvider.GetEntityMetadata(str);
        AttributeMetadata metadata;
        if (!((IEnumerable<AttributeMetadata>) metadataProvider.GetAttributes(str)).ToDictionary<AttributeMetadata, string, AttributeMetadata>((Func<AttributeMetadata, string>) (a => a.LogicalName), (Func<AttributeMetadata, AttributeMetadata>) (a => a)).TryGetValue(key, out metadata))
          return;
        filterProperties.AttributeLocalizedName = metadata.DisplayName.GetLocalizedLabel(localeId) ?? metadata.LogicalName;
        filterProperties.AttributeTypeCode = metadata.AttributeType;
        filterProperties.ControlType = new ControlType?(metadata.GetFilterControlType());
        filterProperties.EntityLocalizedName = (entityMetadata != null ? entityMetadata.DisplayName.GetLocalizedLabel(localeId) : (string) null) ?? str;
        filterProperties.Options = metadata.ToFilterMetadataOptions(localeId);
        filter.Metadata[str][key] = filterProperties;
      }
    }
  }
}
