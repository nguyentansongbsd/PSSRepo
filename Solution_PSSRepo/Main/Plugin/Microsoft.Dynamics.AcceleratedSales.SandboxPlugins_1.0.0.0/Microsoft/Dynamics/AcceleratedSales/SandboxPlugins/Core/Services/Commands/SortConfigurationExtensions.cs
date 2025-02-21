// Decompiled with JetBrains decompiler
// Type: Microsoft.Dynamics.AcceleratedSales.SandboxPlugins.Core.Services.Commands.SortConfigurationExtensions
// Assembly: Microsoft.Dynamics.AcceleratedSales.SandboxPlugins, Version=1.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35
// MVID: 7BBE7132-7E30-4424-B38D-27667CFBCEFA
// Assembly location: C:\Users\ngoct\Downloads\New folder\Microsoft.Dynamics.AcceleratedSales.SandboxPlugins_1.0.0.0.dll

using Microsoft.Dynamics.AcceleratedSales.SandboxPlugins.Core.Model;
using Microsoft.Dynamics.AcceleratedSales.SandboxPlugins.Core.Model.Sort;
using Microsoft.Xrm.Sdk.Metadata;
using System;
using System.Collections.Generic;
using System.Linq;

#nullable disable
namespace Microsoft.Dynamics.AcceleratedSales.SandboxPlugins.Core.Services.Commands
{
  public static class SortConfigurationExtensions
  {
    public static void AddAttributes(
      this SortConfiguration sortConfiguration,
      IEntityMetadataProvider metadataProvider,
      ref Dictionary<string, HashSet<string>> additionalAttributes)
    {
      int num1;
      if (sortConfiguration == null)
      {
        num1 = 0;
      }
      else
      {
        int? count = sortConfiguration.SortOptions?.Count;
        int num2 = 0;
        num1 = count.GetValueOrDefault() > num2 & count.HasValue ? 1 : 0;
      }
      if (num1 == 0)
        return;
      foreach (SortItem sortOption in sortConfiguration.SortOptions)
      {
        string str = sortOption.Metadata.Keys.First<string>();
        if (str != null && sortOption.Metadata[str].Count > 0)
        {
          if (!additionalAttributes.ContainsKey(str))
            additionalAttributes.Add(str, new HashSet<string>());
          string attribute = sortOption.Metadata[str].Keys.First<string>();
          if (metadataProvider.GetAttributes(str) != null && ((IEnumerable<AttributeMetadata>) metadataProvider.GetAttributes(str)).Any<AttributeMetadata>((Func<AttributeMetadata, bool>) (attr => attr.LogicalName == attribute)))
            additionalAttributes[str].Add(attribute);
        }
      }
    }
  }
}
