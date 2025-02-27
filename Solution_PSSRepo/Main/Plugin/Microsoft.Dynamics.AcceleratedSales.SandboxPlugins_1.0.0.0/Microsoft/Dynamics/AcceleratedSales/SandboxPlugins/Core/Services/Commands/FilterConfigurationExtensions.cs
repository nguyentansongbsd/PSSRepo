// Decompiled with JetBrains decompiler
// Type: Microsoft.Dynamics.AcceleratedSales.SandboxPlugins.Core.Services.Commands.FilterConfigurationExtensions
// Assembly: Microsoft.Dynamics.AcceleratedSales.SandboxPlugins, Version=1.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35
// MVID: 7BBE7132-7E30-4424-B38D-27667CFBCEFA
// Assembly location: C:\Users\ngoct\Downloads\New folder\Microsoft.Dynamics.AcceleratedSales.SandboxPlugins_1.0.0.0.dll

using Microsoft.Dynamics.AcceleratedSales.SandboxPlugins.Core.Model;
using Microsoft.Dynamics.AcceleratedSales.SandboxPlugins.Core.Model.Filter;
using Microsoft.Xrm.Sdk.Metadata;
using System;
using System.Collections.Generic;
using System.Linq;

#nullable disable
namespace Microsoft.Dynamics.AcceleratedSales.SandboxPlugins.Core.Services.Commands
{
  public static class FilterConfigurationExtensions
  {
    public static void AddAttributes(
      this FilterConfiguration filterConfiguration,
      IEntityMetadataProvider metadataProvider,
      ref Dictionary<string, HashSet<string>> additionalAttributes)
    {
      int num;
      if (filterConfiguration != null)
      {
        List<FilterGroup> groups = filterConfiguration.Groups;
        // ISSUE: explicit non-virtual call
        num = groups != null ? (__nonvirtual (groups.Count) > 0 ? 1 : 0) : 0;
      }
      else
        num = 0;
      if (num == 0)
        return;
      foreach (FilterGroup group in filterConfiguration.Groups)
      {
        foreach (FilterItem filter in group.Filters)
        {
          string str = filter.Metadata.Keys.First<string>();
          if (str != null && filter.Metadata[str].Count > 0)
          {
            if (!additionalAttributes.ContainsKey(str))
              additionalAttributes.Add(str, new HashSet<string>());
            string attribute = filter.Metadata[str].Keys.First<string>();
            if (metadataProvider.GetAttributes(str) != null && ((IEnumerable<AttributeMetadata>) metadataProvider.GetAttributes(str)).Any<AttributeMetadata>((Func<AttributeMetadata, bool>) (attr => attr.LogicalName == attribute)))
              additionalAttributes[str].Add(attribute);
          }
        }
      }
    }
  }
}
