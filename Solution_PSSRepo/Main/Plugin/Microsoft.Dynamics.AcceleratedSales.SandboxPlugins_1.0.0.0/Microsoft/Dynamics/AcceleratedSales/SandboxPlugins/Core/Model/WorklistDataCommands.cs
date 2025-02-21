// Decompiled with JetBrains decompiler
// Type: Microsoft.Dynamics.AcceleratedSales.SandboxPlugins.Core.Model.WorklistDataCommands
// Assembly: Microsoft.Dynamics.AcceleratedSales.SandboxPlugins, Version=1.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35
// MVID: 7BBE7132-7E30-4424-B38D-27667CFBCEFA
// Assembly location: C:\Users\ngoct\Downloads\New folder\Microsoft.Dynamics.AcceleratedSales.SandboxPlugins_1.0.0.0.dll

using Microsoft.Dynamics.AcceleratedSales.SandboxPlugins.Core.Model.Filter;
using Microsoft.Dynamics.AcceleratedSales.SandboxPlugins.Core.Model.Sort;
using Microsoft.Dynamics.AcceleratedSales.SandboxPlugins.Core.Model.ViewPicker;
using Newtonsoft.Json;
using System.Collections.Generic;

#nullable disable
namespace Microsoft.Dynamics.AcceleratedSales.SandboxPlugins.Core.Model
{
  public class WorklistDataCommands
  {
    [JsonProperty(PropertyName = "filters")]
    public FilterConfiguration Filters { get; set; }

    [JsonProperty(PropertyName = "sort")]
    public SortConfiguration Sort { get; set; }

    [JsonProperty(PropertyName = "views")]
    public Dictionary<string, List<SavedView>> Views { get; set; }

    [JsonProperty(PropertyName = "appliedSort")]
    public Dictionary<string, List<QuerySort>> AppliedSort { get; set; }

    [JsonProperty(PropertyName = "commandMetadata")]
    public Dictionary<string, Microsoft.Dynamics.AcceleratedSales.SandboxPlugins.Core.Model.CommandMetadata> CommandMetadata { get; set; }
  }
}
