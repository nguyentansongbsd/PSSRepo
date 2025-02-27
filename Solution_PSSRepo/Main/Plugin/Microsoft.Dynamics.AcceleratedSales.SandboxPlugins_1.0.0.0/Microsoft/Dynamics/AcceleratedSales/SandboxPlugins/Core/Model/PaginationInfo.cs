// Decompiled with JetBrains decompiler
// Type: Microsoft.Dynamics.AcceleratedSales.SandboxPlugins.Core.Model.PaginationInfo
// Assembly: Microsoft.Dynamics.AcceleratedSales.SandboxPlugins, Version=1.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35
// MVID: 7BBE7132-7E30-4424-B38D-27667CFBCEFA
// Assembly location: C:\Users\ngoct\Downloads\New folder\Microsoft.Dynamics.AcceleratedSales.SandboxPlugins_1.0.0.0.dll

using Newtonsoft.Json;

#nullable disable
namespace Microsoft.Dynamics.AcceleratedSales.SandboxPlugins.Core.Model
{
  public class PaginationInfo
  {
    [JsonProperty(PropertyName = "pageCount")]
    public int PageCount { get; set; }

    [JsonProperty(PropertyName = "pageNumber")]
    public int PageNumber { get; set; }

    [JsonProperty(PropertyName = "pagingCookie")]
    public string PagingCookie { get; set; }

    [JsonProperty(PropertyName = "recordsCount")]
    public int RecordsCount { get; set; }

    [JsonProperty(PropertyName = "hasMoreRecords")]
    public bool HasMoreRecords { get; set; }

    [JsonProperty(PropertyName = "totalRecordsCount")]
    public int TotalRecordsCount { get; set; }

    [JsonProperty(PropertyName = "totalRecordsCountLimitExceeded")]
    public bool TotalRecordsCountLimitExceeded { get; set; }
  }
}
