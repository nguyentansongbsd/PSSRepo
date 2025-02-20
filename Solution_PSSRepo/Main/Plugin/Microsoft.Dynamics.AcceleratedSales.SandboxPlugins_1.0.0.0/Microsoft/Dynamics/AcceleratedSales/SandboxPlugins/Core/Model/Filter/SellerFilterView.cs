// Decompiled with JetBrains decompiler
// Type: Microsoft.Dynamics.AcceleratedSales.SandboxPlugins.Core.Model.Filter.SellerFilterView
// Assembly: Microsoft.Dynamics.AcceleratedSales.SandboxPlugins, Version=1.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35
// MVID: 7BBE7132-7E30-4424-B38D-27667CFBCEFA
// Assembly location: C:\Users\ngoct\Downloads\New folder\Microsoft.Dynamics.AcceleratedSales.SandboxPlugins_1.0.0.0.dll

using System.Collections.Generic;

#nullable disable
namespace Microsoft.Dynamics.AcceleratedSales.SandboxPlugins.Core.Model.Filter
{
  public class SellerFilterView
  {
    public string AdminConfigVersion { get; set; }

    public Dictionary<string, SellerFilterGroup> Groups { get; set; }

    public Dictionary<string, SellerFilterItem> Filters { get; set; }
  }
}
