// Decompiled with JetBrains decompiler
// Type: Microsoft.Dynamics.AcceleratedSales.SandboxPlugins.Core.Model.WorklistDataResponse
// Assembly: Microsoft.Dynamics.AcceleratedSales.SandboxPlugins, Version=1.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35
// MVID: 7BBE7132-7E30-4424-B38D-27667CFBCEFA
// Assembly location: C:\Users\ngoct\Downloads\New folder\Microsoft.Dynamics.AcceleratedSales.SandboxPlugins_1.0.0.0.dll

using Microsoft.Dynamics.AcceleratedSales.SandboxObjectModel.Model;
using Microsoft.Dynamics.AcceleratedSales.SandboxPlugins.Core.Model.DefaultSort;
using Microsoft.Dynamics.AcceleratedSales.SandboxPlugins.Core.Model.Layout;
using Microsoft.Dynamics.AcceleratedSales.SandboxPlugins.Core.Model.Tags;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;

#nullable disable
namespace Microsoft.Dynamics.AcceleratedSales.SandboxPlugins.Core.Model
{
  public class WorklistDataResponse
  {
    [JsonProperty(PropertyName = "paginationInfo")]
    public PaginationInfo PaginationInfo { get; set; }

    [JsonProperty(PropertyName = "records")]
    public Dictionary<string, List<Dictionary<string, string>>> Records { get; set; }

    [JsonProperty(PropertyName = "settings")]
    public SASettings Settings { get; set; }

    [JsonProperty(PropertyName = "relatedRecords")]
    public Dictionary<string, List<Dictionary<string, string>>> RelatedRecords { get; set; }

    [JsonProperty(PropertyName = "commands")]
    public WorklistDataCommands Commands { get; set; }

    [JsonProperty(PropertyName = "cardlayout")]
    public Dictionary<string, DesignerConfiguration> Cardlayout { get; set; }

    [JsonProperty(PropertyName = "defaultSortConfig")]
    public DefaultSortConfiguration DefaultSortConfiguration { get; set; }

    [JsonProperty(PropertyName = "tagsConfig")]
    public TagsConfiguration TagsConfig { get; set; }

    [JsonProperty(PropertyName = "viewId")]
    public Guid ViewId { get; set; }

    [JsonProperty(PropertyName = "fetchXml")]
    public string FetchXml { get; set; }

    [JsonProperty(PropertyName = "isCardLayoutLocked")]
    public bool IsCardLayoutLocked { get; set; }

    [JsonProperty(PropertyName = "fetchXmlInUse")]
    public string FetchXmlInUse { get; set; }

    [JsonProperty(PropertyName = "isAutoRefreshEnable")]
    public int IsAutoRefreshEnable { get; set; }

    [JsonProperty(PropertyName = "autoRefreshInterval")]
    public int AutoRefreshInterval { get; set; }

    [JsonProperty(PropertyName = "isActivityTypeEntity")]
    public bool IsActivityTypeEntity { get; set; }
  }
}
