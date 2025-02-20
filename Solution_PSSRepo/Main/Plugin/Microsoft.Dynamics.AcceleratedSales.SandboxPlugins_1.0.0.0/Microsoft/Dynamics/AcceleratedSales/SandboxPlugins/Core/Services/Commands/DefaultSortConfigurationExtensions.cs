// Decompiled with JetBrains decompiler
// Type: Microsoft.Dynamics.AcceleratedSales.SandboxPlugins.Core.Services.Commands.DefaultSortConfigurationExtensions
// Assembly: Microsoft.Dynamics.AcceleratedSales.SandboxPlugins, Version=1.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35
// MVID: 7BBE7132-7E30-4424-B38D-27667CFBCEFA
// Assembly location: C:\Users\ngoct\Downloads\New folder\Microsoft.Dynamics.AcceleratedSales.SandboxPlugins_1.0.0.0.dll

using Microsoft.Dynamics.AcceleratedSales.SandboxPlugins.Core.Model;
using Microsoft.Dynamics.AcceleratedSales.SandboxPlugins.Core.Model.DefaultSort;
using Microsoft.Xrm.Sdk.Metadata;
using System;
using System.Collections.Generic;
using System.Linq;

#nullable disable
namespace Microsoft.Dynamics.AcceleratedSales.SandboxPlugins.Core.Services.Commands
{
  public static class DefaultSortConfigurationExtensions
  {
    public static void AddAttributes(
      this DefaultSortConfiguration defaultSortConfiguration,
      IEntityMetadataProvider metadataProvider,
      ref Dictionary<string, HashSet<string>> additionalAttributes)
    {
      int num1;
      if (defaultSortConfiguration == null)
      {
        num1 = 0;
      }
      else
      {
        int? count = defaultSortConfiguration.DefaultSortKeys?.Count;
        int num2 = 0;
        num1 = count.GetValueOrDefault() > num2 & count.HasValue ? 1 : 0;
      }
      if (num1 == 0)
        return;
      foreach (DefaultSortKey defaultSortKey in defaultSortConfiguration.DefaultSortKeys)
      {
        string entity = defaultSortKey.Entity;
        string attribute = defaultSortKey.Attribute;
        if (entity != null && attribute != null)
        {
          if (!additionalAttributes.ContainsKey(entity))
            additionalAttributes.Add(entity, new HashSet<string>());
          if (metadataProvider.GetAttributes(entity) != null && ((IEnumerable<AttributeMetadata>) metadataProvider.GetAttributes(entity)).Any<AttributeMetadata>((Func<AttributeMetadata, bool>) (attr => attr.LogicalName == attribute)))
            additionalAttributes[entity].Add(attribute);
        }
      }
    }
  }
}
