// Decompiled with JetBrains decompiler
// Type: Microsoft.Dynamics.AcceleratedSales.SandboxPlugins.Core.Model.DefaultSort.DefaultSortKey
// Assembly: Microsoft.Dynamics.AcceleratedSales.SandboxPlugins, Version=1.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35
// MVID: 7BBE7132-7E30-4424-B38D-27667CFBCEFA
// Assembly location: C:\Users\ngoct\Downloads\New folder\Microsoft.Dynamics.AcceleratedSales.SandboxPlugins_1.0.0.0.dll

using Microsoft.Dynamics.AcceleratedSales.SandboxPlugins.Core.Model.Sort;
using Newtonsoft.Json;
using System.Collections.Generic;

#nullable disable
namespace Microsoft.Dynamics.AcceleratedSales.SandboxPlugins.Core.Model.DefaultSort
{
  public class DefaultSortKey
  {
    [JsonProperty(PropertyName = "attribute")]
    public string Attribute { get; set; }

    [JsonProperty(PropertyName = "entity")]
    public string Entity { get; set; }

    [JsonProperty(PropertyName = "sequence")]
    public List<string> Sequence { get; set; }

    [JsonProperty(PropertyName = "sortOrder")]
    public SortOrder SortOrder { get; set; }

    public Dictionary<string, Dictionary<string, SortProperties>> Metadata { get; set; }
  }
}
