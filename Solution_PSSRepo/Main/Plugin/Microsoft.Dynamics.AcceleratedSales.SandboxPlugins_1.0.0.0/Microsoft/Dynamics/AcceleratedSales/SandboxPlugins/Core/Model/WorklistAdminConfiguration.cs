// Decompiled with JetBrains decompiler
// Type: Microsoft.Dynamics.AcceleratedSales.SandboxPlugins.Core.Model.WorklistAdminConfiguration
// Assembly: Microsoft.Dynamics.AcceleratedSales.SandboxPlugins, Version=1.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35
// MVID: 7BBE7132-7E30-4424-B38D-27667CFBCEFA
// Assembly location: C:\Users\ngoct\Downloads\New folder\Microsoft.Dynamics.AcceleratedSales.SandboxPlugins_1.0.0.0.dll

using Microsoft.Dynamics.AcceleratedSales.SandboxPlugins.Core.Model.DefaultSort;
using Microsoft.Dynamics.AcceleratedSales.SandboxPlugins.Core.Model.Filter;
using Microsoft.Dynamics.AcceleratedSales.SandboxPlugins.Core.Model.Layout;
using Microsoft.Dynamics.AcceleratedSales.SandboxPlugins.Core.Model.Sort;
using Microsoft.Dynamics.AcceleratedSales.SandboxPlugins.Core.Model.Tags;
using Newtonsoft.Json;
using System;

#nullable disable
namespace Microsoft.Dynamics.AcceleratedSales.SandboxPlugins.Core.Model
{
  public class WorklistAdminConfiguration
  {
    public Guid ViewId { get; set; }

    public AdminDesignerConfiguration CardLayout { get; set; }

    public FilterConfiguration FilterConfiguration { get; set; }

    public SortConfiguration SortConfiguration { get; set; }

    public TagsConfiguration TagsConfiguration { get; set; }

    public DefaultSortConfiguration DefaultSortConfiguration { get; set; }

    [JsonProperty("IsAutoRefreshEnable")]
    public int AutoRefreshEnable { get; set; }

    [JsonProperty("AutoRefreshInterval")]
    public int AutoRefreshInterval { get; set; }
  }
}
