﻿// Decompiled with JetBrains decompiler
// Type: Microsoft.Dynamics.AcceleratedSales.SandboxPlugins.Core.Model.Sort.SortProperties
// Assembly: Microsoft.Dynamics.AcceleratedSales.SandboxPlugins, Version=1.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35
// MVID: 7BBE7132-7E30-4424-B38D-27667CFBCEFA
// Assembly location: C:\Users\ngoct\Downloads\New folder\Microsoft.Dynamics.AcceleratedSales.SandboxPlugins_1.0.0.0.dll

using System.Collections.Generic;

#nullable disable
namespace Microsoft.Dynamics.AcceleratedSales.SandboxPlugins.Core.Model.Sort
{
  public class SortProperties
  {
    public Microsoft.Xrm.Sdk.Metadata.AttributeTypeCode? AttributeTypeCode { get; set; }

    public string AttributeLocalizedName { get; set; }

    public string EntityLocalizedName { get; set; }

    public Dictionary<string, string> Options { get; set; }
  }
}
