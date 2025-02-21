// Decompiled with JetBrains decompiler
// Type: Microsoft.Dynamics.AcceleratedSales.SandboxPlugins.Core.Model.WorklistFilteredDataResponse
// Assembly: Microsoft.Dynamics.AcceleratedSales.SandboxPlugins, Version=1.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35
// MVID: 7BBE7132-7E30-4424-B38D-27667CFBCEFA
// Assembly location: C:\Users\ngoct\Downloads\New folder\Microsoft.Dynamics.AcceleratedSales.SandboxPlugins_1.0.0.0.dll

using Newtonsoft.Json;
using System.Collections.Generic;

#nullable disable
namespace Microsoft.Dynamics.AcceleratedSales.SandboxPlugins.Core.Model
{
  public class WorklistFilteredDataResponse
  {
    [JsonProperty(PropertyName = "paginationInfo")]
    public PaginationInfo PaginationInfo { get; set; }

    [JsonProperty(PropertyName = "records")]
    public Dictionary<string, List<Dictionary<string, string>>> Records { get; set; }

    [JsonProperty(PropertyName = "relatedRecords")]
    public Dictionary<string, List<Dictionary<string, string>>> RelatedRecords { get; set; }

    [JsonProperty(PropertyName = "fetchXml")]
    public string FetchXml { get; set; }

    [JsonProperty(PropertyName = "fetchXmlInUse")]
    public string FetchXmlInUse { get; set; }

    [JsonProperty(PropertyName = "isActivityTypeEntity")]
    public bool IsActivityTypeEntity { get; set; }
  }
}
