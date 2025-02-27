// Decompiled with JetBrains decompiler
// Type: Microsoft.Dynamics.AcceleratedSales.SandboxPlugins.Core.Services.Commands.DefaultSortCommandConfiguration
// Assembly: Microsoft.Dynamics.AcceleratedSales.SandboxPlugins, Version=1.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35
// MVID: 7BBE7132-7E30-4424-B38D-27667CFBCEFA
// Assembly location: C:\Users\ngoct\Downloads\New folder\Microsoft.Dynamics.AcceleratedSales.SandboxPlugins_1.0.0.0.dll

using Microsoft.Dynamics.AcceleratedSales.SandboxPlugins.Core.Model;
using Microsoft.Dynamics.AcceleratedSales.SandboxPlugins.Core.Model.DefaultSort;
using Microsoft.Dynamics.AcceleratedSales.SandboxPlugins.Core.Model.Sort;
using Microsoft.Xrm.Sdk.Metadata;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

#nullable disable
namespace Microsoft.Dynamics.AcceleratedSales.SandboxPlugins.Core.Services.Commands
{
  public class DefaultSortCommandConfiguration
  {
    private readonly IAcceleratedSalesLogger logger;
    private readonly IEntityMetadataProvider metadataProvider;

    public DefaultSortCommandConfiguration(
      IAcceleratedSalesLogger logger,
      IEntityMetadataProvider metadataProvider)
    {
      this.logger = logger ?? throw new ArgumentNullException(nameof (logger));
      this.metadataProvider = metadataProvider ?? throw new ArgumentNullException(nameof (metadataProvider));
    }

    public DefaultSortConfiguration GetSortConfiguration(
      WorklistAdminConfiguration adminConfig,
      int localeId)
    {
      Stopwatch stopwatch = Stopwatch.StartNew();
      this.logger.AddCustomProperty("DefaultSortCommandConfiguration.GetSortConfiguration.Start", (object) "Success");
      DefaultSortConfiguration sortConfiguration = adminConfig?.DefaultSortConfiguration ?? new DefaultSortConfiguration();
      if (adminConfig?.DefaultSortConfiguration == null)
        return sortConfiguration;
      foreach (DefaultSortKey defaultSortKey in sortConfiguration.DefaultSortKeys)
        DefaultSortCommandConfiguration.TryUpdateProperties(defaultSortKey, this.metadataProvider, localeId);
      stopwatch.Stop();
      this.logger.AddCustomProperty("DefaultSortCommandConfiguration.GetSortConfiguration.Duration", (object) stopwatch.ElapsedMilliseconds);
      this.logger.AddCustomProperty("DefaultSortCommandConfiguration.GetSortConfiguration.End", (object) "Success");
      return sortConfiguration;
    }

    private static void TryUpdateProperties(
      DefaultSortKey sort,
      IEntityMetadataProvider metadataProvider,
      int localeId)
    {
      string entity = sort.Entity;
      string attribute = sort.Attribute;
      sort.Metadata = new Dictionary<string, Dictionary<string, SortProperties>>();
      sort.Metadata.Add(entity, new Dictionary<string, SortProperties>());
      sort.Metadata[entity].Add(attribute, new SortProperties());
      SortProperties sortProperties = new SortProperties();
      if (string.IsNullOrEmpty(entity) || string.IsNullOrEmpty(attribute))
        return;
      EntityMetadata entityMetadata = metadataProvider.GetEntityMetadata(entity);
      AttributeMetadata metadata;
      if (!((IEnumerable<AttributeMetadata>) metadataProvider.GetAttributes(entity)).ToDictionary<AttributeMetadata, string, AttributeMetadata>((Func<AttributeMetadata, string>) (a => a.LogicalName), (Func<AttributeMetadata, AttributeMetadata>) (a => a)).TryGetValue(attribute, out metadata))
        return;
      sortProperties.AttributeLocalizedName = metadata.DisplayName.GetLocalizedLabel(localeId) ?? metadata.LogicalName;
      sortProperties.AttributeTypeCode = metadata.AttributeType;
      sortProperties.EntityLocalizedName = (entityMetadata != null ? entityMetadata.DisplayName.GetLocalizedLabel(localeId) : (string) null) ?? entity;
      sortProperties.Options = metadata.ToFilterMetadataOptions(localeId);
      sort.Metadata[entity][attribute] = sortProperties;
    }
  }
}
