// Decompiled with JetBrains decompiler
// Type: Microsoft.Dynamics.AcceleratedSales.SandboxPlugins.Core.Model.PrimaryEntityPipelineData
// Assembly: Microsoft.Dynamics.AcceleratedSales.SandboxPlugins, Version=1.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35
// MVID: 7BBE7132-7E30-4424-B38D-27667CFBCEFA
// Assembly location: C:\Users\ngoct\Downloads\New folder\Microsoft.Dynamics.AcceleratedSales.SandboxPlugins_1.0.0.0.dll

using Microsoft.Xrm.Sdk;
using System.Collections.Generic;

#nullable disable
namespace Microsoft.Dynamics.AcceleratedSales.SandboxPlugins.Core.Model
{
  public class PrimaryEntityPipelineData
  {
    public List<Entity> PrimaryEntityRecords { get; set; }

    public string PagingCookie { get; set; }

    public int RecordsCount { get; set; }

    public bool HasMoreRecords { get; set; }

    public int TotalRecordsCount { get; set; }

    public bool TotalRecordsCountLimitExceeded { get; set; }
  }
}
