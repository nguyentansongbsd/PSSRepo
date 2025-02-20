// Decompiled with JetBrains decompiler
// Type: Microsoft.Dynamics.AcceleratedSales.SandboxPlugins.Core.Services.Extensions.IDataExtension
// Assembly: Microsoft.Dynamics.AcceleratedSales.SandboxPlugins, Version=1.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35
// MVID: 7BBE7132-7E30-4424-B38D-27667CFBCEFA
// Assembly location: C:\Users\ngoct\Downloads\New folder\Microsoft.Dynamics.AcceleratedSales.SandboxPlugins_1.0.0.0.dll

using Microsoft.Dynamics.AcceleratedSales.SandboxPlugins.Core.DataAccess;
using Microsoft.Dynamics.AcceleratedSales.SandboxPlugins.Core.Model;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Query;
using System.Collections.Generic;

#nullable disable
namespace Microsoft.Dynamics.AcceleratedSales.SandboxPlugins.Core.Services.Extensions
{
  public interface IDataExtension
  {
    KeyValuePair<string, Microsoft.Dynamics.AcceleratedSales.SandboxPlugins.Core.Model.Layout.DesignerConfiguration> DesignerConfiguration { get; }

    QueryExpression CreateQuery(IEntityMetadataProvider metadata, Microsoft.Dynamics.AcceleratedSales.SandboxPlugins.Core.Model.Layout.DesignerConfiguration cardDesign);

    List<Dictionary<string, string>> FormatRecords(
      List<Entity> records,
      IEntityMetadataProvider metadata,
      HashSet<string> skipFormattingAttributes);

    Entity UpdateRecord(
      Entity record,
      IEntityMetadataProvider metadata,
      Microsoft.Dynamics.AcceleratedSales.SandboxPlugins.Core.Model.Layout.DesignerConfiguration cardDesign);

    EntityExtensions.AttributeValue GetRegardingObjectForRecord(Dictionary<string, string> record);

    void PrepareMetadataQuery(
      QueryExpression queryExpression,
      Microsoft.Dynamics.AcceleratedSales.SandboxPlugins.Core.Model.Layout.DesignerConfiguration cardDesign,
      MetadataQueryParams metadataQueryParams);
  }
}
