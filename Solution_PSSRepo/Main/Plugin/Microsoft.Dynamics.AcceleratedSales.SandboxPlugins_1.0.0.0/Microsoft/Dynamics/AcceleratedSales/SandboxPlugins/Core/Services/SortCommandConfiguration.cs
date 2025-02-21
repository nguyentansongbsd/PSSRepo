// Decompiled with JetBrains decompiler
// Type: Microsoft.Dynamics.AcceleratedSales.SandboxPlugins.Core.Services.SortCommandConfiguration
// Assembly: Microsoft.Dynamics.AcceleratedSales.SandboxPlugins, Version=1.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35
// MVID: 7BBE7132-7E30-4424-B38D-27667CFBCEFA
// Assembly location: C:\Users\ngoct\Downloads\New folder\Microsoft.Dynamics.AcceleratedSales.SandboxPlugins_1.0.0.0.dll

using Microsoft.Dynamics.AcceleratedSales.SandboxPlugins.Core.Model;
using Microsoft.Dynamics.AcceleratedSales.SandboxPlugins.Core.Model.Sort;
using Microsoft.Xrm.Sdk.Metadata;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

#nullable disable
namespace Microsoft.Dynamics.AcceleratedSales.SandboxPlugins.Core.Services
{
  public class SortCommandConfiguration
  {
    private const string DefaultConfiguration = "{\"Version\":\"8899d163-6781-4af4-b850-0d8a88e9ed1e\",\"SortOptions\":[{\"Id\":\"b00000b0-b0b0-000b-b000-000bb0bbb000\",\"Name\":\"Due date\",\"Position\":0,\"Visibility\":true,\"IsDefault\":true,\"IsSystemDefined\":true,\"IsCustomName\":true,\"Metadata\":{\"msdyn_workqueuerecord\":{\"msdyn_duetime\":{\"AttributeTypeCode\":\"\"}}}},{\"Id\":\"b00000b0-b0b0-000b-b000-000bb0bbb001\",\"Name\":\"Score\",\"Position\":1,\"Visibility\":true,\"IsDefault\":false,\"IsSystemDefined\":true,\"IsCustomName\":true,\"Metadata\":{\"msdyn_workqueuerecord\":{\"msdyn_priorityscore\":{\"AttributeTypeCode\":\"\"}}}},{\"Id\":\"b00000b0-b0b0-000b-b000-000bb0bbb002\",\"Name\":\"Name\",\"Position\":2,\"Visibility\":true,\"IsDefault\":false,\"IsSystemDefined\":true,\"IsCustomName\":true,\"Metadata\":{\"msdyn_workqueuerecord\":{\"msdyn_primaryname\":{\"AttributeTypeCode\":\"\"}}}},{\"Id\":\"b00000b0-b0b0-000b-b000-000bb0bbb003\",\"Name\":\"Activity type\",\"Position\":3,\"Visibility\":true,\"IsDefault\":false,\"IsSystemDefined\":true,\"IsCustomName\":true,\"Metadata\":{\"msdyn_workqueuerecord\":{\"msdyn_nextactionsource\":{\"AttributeTypeCode\":\"\"}}}},{\"Id\":\"b00000b0-b0b0-000b-b000-000bb0bbb004\",\"Name\":\"Record type\",\"Position\":4,\"Visibility\":true,\"IsDefault\":false,\"IsSystemDefined\":true,\"IsCustomName\":true,\"Metadata\":{\"msdyn_workqueuerecord\":{\"msdyn_entitytypelogicalname\":{\"AttributeTypeCode\":\"\"}}}},{\"Id\":\"b00000b0-b0b0-000b-b000-000bb0bbb005\",\"Name\":\"Sequence name\",\"Position\":5,\"Visibility\":true,\"IsDefault\":false,\"IsSystemDefined\":true,\"IsCustomName\":true,\"Metadata\":{\"msdyn_workqueuerecord\":{\"msdyn_sequencename\":{\"AttributeTypeCode\":\"\"}}}}]}";
    private const string CustomEntityDefaultConfiguration = "{\"Version\":\"8899d163-6781-4af4-b850-0d8a88e9ed1e\",\"SortOptions\":[{\"Id\":\"b00000b0-b0b0-000b-b000-000bb0bbb000\",\"Name\":\"Due date\",\"Position\":0,\"Visibility\":true,\"IsDefault\":true,\"IsSystemDefined\":true,\"IsCustomName\":true,\"Metadata\":{\"msdyn_workqueuerecord\":{\"msdyn_duetime\":{\"AttributeTypeCode\":\"\"}}}}]}";
    private readonly IAcceleratedSalesLogger logger;
    private readonly IEntityMetadataProvider metadataProvider;

    public SortCommandConfiguration(
      IAcceleratedSalesLogger logger,
      IEntityMetadataProvider metadataProvider)
    {
      this.logger = logger ?? throw new ArgumentNullException(nameof (logger));
      this.metadataProvider = metadataProvider ?? throw new ArgumentNullException(nameof (metadataProvider));
    }

    public SortConfiguration DefaultSortConfigurationForCOLA
    {
      get
      {
        return JsonConvert.DeserializeObject<SortConfiguration>("{\"Version\":\"8899d163-6781-4af4-b850-0d8a88e9ed1e\",\"SortOptions\":[{\"Id\":\"b00000b0-b0b0-000b-b000-000bb0bbb000\",\"Name\":\"Due date\",\"Position\":0,\"Visibility\":true,\"IsDefault\":true,\"IsSystemDefined\":true,\"IsCustomName\":true,\"Metadata\":{\"msdyn_workqueuerecord\":{\"msdyn_duetime\":{\"AttributeTypeCode\":\"\"}}}},{\"Id\":\"b00000b0-b0b0-000b-b000-000bb0bbb001\",\"Name\":\"Score\",\"Position\":1,\"Visibility\":true,\"IsDefault\":false,\"IsSystemDefined\":true,\"IsCustomName\":true,\"Metadata\":{\"msdyn_workqueuerecord\":{\"msdyn_priorityscore\":{\"AttributeTypeCode\":\"\"}}}},{\"Id\":\"b00000b0-b0b0-000b-b000-000bb0bbb002\",\"Name\":\"Name\",\"Position\":2,\"Visibility\":true,\"IsDefault\":false,\"IsSystemDefined\":true,\"IsCustomName\":true,\"Metadata\":{\"msdyn_workqueuerecord\":{\"msdyn_primaryname\":{\"AttributeTypeCode\":\"\"}}}},{\"Id\":\"b00000b0-b0b0-000b-b000-000bb0bbb003\",\"Name\":\"Activity type\",\"Position\":3,\"Visibility\":true,\"IsDefault\":false,\"IsSystemDefined\":true,\"IsCustomName\":true,\"Metadata\":{\"msdyn_workqueuerecord\":{\"msdyn_nextactionsource\":{\"AttributeTypeCode\":\"\"}}}},{\"Id\":\"b00000b0-b0b0-000b-b000-000bb0bbb004\",\"Name\":\"Record type\",\"Position\":4,\"Visibility\":true,\"IsDefault\":false,\"IsSystemDefined\":true,\"IsCustomName\":true,\"Metadata\":{\"msdyn_workqueuerecord\":{\"msdyn_entitytypelogicalname\":{\"AttributeTypeCode\":\"\"}}}},{\"Id\":\"b00000b0-b0b0-000b-b000-000bb0bbb005\",\"Name\":\"Sequence name\",\"Position\":5,\"Visibility\":true,\"IsDefault\":false,\"IsSystemDefined\":true,\"IsCustomName\":true,\"Metadata\":{\"msdyn_workqueuerecord\":{\"msdyn_sequencename\":{\"AttributeTypeCode\":\"\"}}}}]}");
      }
    }

    private SortConfiguration DefaultSortConfigurationForCustomEntity
    {
      get
      {
        return JsonConvert.DeserializeObject<SortConfiguration>("{\"Version\":\"8899d163-6781-4af4-b850-0d8a88e9ed1e\",\"SortOptions\":[{\"Id\":\"b00000b0-b0b0-000b-b000-000bb0bbb000\",\"Name\":\"Due date\",\"Position\":0,\"Visibility\":true,\"IsDefault\":true,\"IsSystemDefined\":true,\"IsCustomName\":true,\"Metadata\":{\"msdyn_workqueuerecord\":{\"msdyn_duetime\":{\"AttributeTypeCode\":\"\"}}}}]}");
      }
    }

    public SortConfiguration GetSortConfiguration(
      WorklistAdminConfiguration adminConfig,
      WorklistSellerConfiguration sellerConfig,
      int localeId,
      bool isCustomEntity = false)
    {
      Stopwatch stopwatch = Stopwatch.StartNew();
      SortConfiguration config = isCustomEntity ? this.DefaultSortConfigurationForCustomEntity : this.DefaultSortConfigurationForCOLA;
      if (adminConfig?.SortConfiguration != null)
        SortCommandConfiguration.TryMerge(adminConfig.SortConfiguration, sellerConfig?.SortConfiguration, out config);
      this.FillLocalizedNames(config, localeId);
      stopwatch.Stop();
      this.logger.LogWarning("SortCommandConfiguration.GetSortConfiguration.End.Duration: " + stopwatch.ElapsedMilliseconds.ToString(), callerName: nameof (GetSortConfiguration), callerFile: "C:\\__w\\1\\s\\solutions\\AcceleratedSales\\AcceleratedSalesSandboxPlugins\\Core\\Services\\Commands\\SortCommandConfiguration.cs");
      return config;
    }

    public SortConfiguration GetSortConfigurationForSA(
      WorklistAdminConfiguration adminConfig,
      WorklistSellerConfiguration sellerConfig,
      int localeId)
    {
      if (adminConfig.SortConfiguration == null)
        adminConfig.SortConfiguration = this.DefaultSortConfigurationForCOLA;
      else
        adminConfig.SortConfiguration.SortOptions = adminConfig.SortConfiguration.SortOptions.Where<SortItem>((Func<SortItem, bool>) (sortOption => sortOption.Visibility)).ToList<SortItem>();
      return this.GetSortConfiguration(adminConfig, sellerConfig, localeId);
    }

    public void FillLocalizedNames(SortConfiguration sortConfig, int localeId)
    {
      Stopwatch stopwatch = Stopwatch.StartNew();
      foreach (SortItem sort in sortConfig.SortOptions.Where<SortItem>((Func<SortItem, bool>) (g => g.Visibility)))
        SortCommandConfiguration.TryUpdateProperties(sort, this.metadataProvider, localeId);
      stopwatch.Stop();
      this.logger.LogWarning("SortCommandConfiguration.FillLocalizedNames.End.Duration: " + stopwatch.ElapsedMilliseconds.ToString(), callerName: nameof (FillLocalizedNames), callerFile: "C:\\__w\\1\\s\\solutions\\AcceleratedSales\\AcceleratedSalesSandboxPlugins\\Core\\Services\\Commands\\SortCommandConfiguration.cs");
    }

    private static void TryUpdateProperties(
      SortItem sort,
      IEntityMetadataProvider metadataProvider,
      int localeId)
    {
      string str = sort.Metadata.Keys.FirstOrDefault<string>();
      Dictionary<string, SortProperties> dictionary1 = sort.Metadata.Values.FirstOrDefault<Dictionary<string, SortProperties>>();
      string key = dictionary1 != null ? dictionary1.Keys.FirstOrDefault<string>() : (string) null;
      Dictionary<string, SortProperties> dictionary2 = sort.Metadata.Values.FirstOrDefault<Dictionary<string, SortProperties>>();
      SortProperties sortProperties = (dictionary2 != null ? dictionary2.Values.FirstOrDefault<SortProperties>() : (SortProperties) null) ?? new SortProperties();
      if (string.IsNullOrEmpty(str) || string.IsNullOrEmpty(key))
        return;
      EntityMetadata entityMetadata = metadataProvider.GetEntityMetadata(str);
      AttributeMetadata metadata;
      if (!((IEnumerable<AttributeMetadata>) metadataProvider.GetAttributes(str)).ToDictionary<AttributeMetadata, string, AttributeMetadata>((Func<AttributeMetadata, string>) (a => a.LogicalName), (Func<AttributeMetadata, AttributeMetadata>) (a => a)).TryGetValue(key, out metadata))
        return;
      sortProperties.AttributeLocalizedName = metadata.DisplayName.GetLocalizedLabel(localeId) ?? metadata.LogicalName;
      sortProperties.AttributeTypeCode = metadata.AttributeType;
      sortProperties.EntityLocalizedName = (entityMetadata != null ? entityMetadata.DisplayName.GetLocalizedLabel(localeId) : (string) null) ?? str;
      sortProperties.Options = metadata.ToFilterMetadataOptions(localeId);
      sort.Metadata[str][key] = sortProperties;
    }

    private static bool TryMerge(
      SortConfiguration adminSort,
      SellerSortConfiguration sellerSort,
      out SortConfiguration config)
    {
      config = new SortConfiguration();
      config.Version = adminSort.Version;
      config.SortOptions = new List<SortItem>();
      Guid guid;
      int num;
      if (sellerSort != null)
      {
        guid = adminSort.Version;
        string str1 = guid.ToString();
        guid = sellerSort.View.AdminConfigVersion;
        string str2 = guid.ToString();
        num = !str1.Equals(str2, StringComparison.Ordinal) ? 1 : 0;
      }
      else
        num = 1;
      if (num != 0)
      {
        config = adminSort;
        return false;
      }
      foreach (SortItem sortOption in adminSort.SortOptions)
      {
        Dictionary<string, SellerSortOptionProperties> sortOptions = sellerSort.View.SortOptions;
        guid = sortOption.Id;
        string key = guid.ToString();
        SellerSortOptionProperties optionProperties;
        ref SellerSortOptionProperties local = ref optionProperties;
        if (sortOptions.TryGetValue(key, out local))
        {
          sortOption.Position = optionProperties.Position;
          sortOption.Visibility = optionProperties.Visibility;
        }
        config.SortOptions.Add(sortOption);
      }
      return true;
    }
  }
}
