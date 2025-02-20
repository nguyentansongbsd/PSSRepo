// Decompiled with JetBrains decompiler
// Type: Microsoft.Dynamics.AcceleratedSales.SandboxPlugins.Core.Services.Extensions.SequenceExtension
// Assembly: Microsoft.Dynamics.AcceleratedSales.SandboxPlugins, Version=1.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35
// MVID: 7BBE7132-7E30-4424-B38D-27667CFBCEFA
// Assembly location: C:\Users\ngoct\Downloads\New folder\Microsoft.Dynamics.AcceleratedSales.SandboxPlugins_1.0.0.0.dll

using Microsoft.Dynamics.AcceleratedSales.SandboxPlugins.Core.DataAccess;
using Microsoft.Dynamics.AcceleratedSales.SandboxPlugins.Core.Model;
using Microsoft.Dynamics.AcceleratedSales.SandboxPlugins.Core.Model.Layout;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Metadata;
using Microsoft.Xrm.Sdk.Query;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

#nullable disable
namespace Microsoft.Dynamics.AcceleratedSales.SandboxPlugins.Core.Services.Extensions
{
  public class SequenceExtension : IDataExtension
  {
    private const string SequenceTargetStepEntityName = "msdyn_sequencetargetstep";

    public KeyValuePair<string, Microsoft.Dynamics.AcceleratedSales.SandboxPlugins.Core.Model.Layout.DesignerConfiguration> DesignerConfiguration
    {
      get => throw new NotImplementedException();
    }

    public QueryExpression CreateQuery(
      IEntityMetadataProvider metadata,
      Microsoft.Dynamics.AcceleratedSales.SandboxPlugins.Core.Model.Layout.DesignerConfiguration cardDesign)
    {
      throw new NotImplementedException();
    }

    public List<Dictionary<string, string>> FormatRecords(
      List<Entity> records,
      IEntityMetadataProvider metadata,
      HashSet<string> skipFormattingAttributes)
    {
      List<Dictionary<string, string>> dictionaryList = new List<Dictionary<string, string>>();
      Dictionary<string, AttributeTypeCode?> dictionary1 = ((IEnumerable<AttributeMetadata>) metadata.GetAttributes("msdyn_sequencetargetstep")).ToDictionary<AttributeMetadata, string, AttributeTypeCode?>((Func<AttributeMetadata, string>) (a => a.LogicalName), (Func<AttributeMetadata, AttributeTypeCode?>) (a => a.AttributeType));
      HashSet<string> attributesToSkipFormatting = new HashSet<string>((IEnumerable<string>) new string[7]
      {
        "msdyn_type",
        "statecode",
        "statuscode",
        "msdyn_waitstate",
        "msdyn_errorstate",
        "msdyn_duetime",
        "msdyn_sequenceduetime"
      });
      foreach (string formattingAttribute in skipFormattingAttributes)
        attributesToSkipFormatting.Add(formattingAttribute);
      foreach (Entity record in records)
      {
        Dictionary<string, string> dictionary2 = record.ToDictionary(dictionary1, attributesToSkipFormatting);
        dictionaryList.Add(dictionary2);
      }
      return dictionaryList;
    }

    public Entity UpdateRecord(
      Entity record,
      IEntityMetadataProvider metadata,
      Microsoft.Dynamics.AcceleratedSales.SandboxPlugins.Core.Model.Layout.DesignerConfiguration cardDesign)
    {
      throw new NotImplementedException();
    }

    public EntityExtensions.AttributeValue GetRegardingObjectForRecord(
      Dictionary<string, string> record)
    {
      string str;
      return record.TryGetValue("msdyn_sequencetarget.msdyn_target", out str) ? JsonConvert.DeserializeObject<EntityExtensions.AttributeValue>(str) : new EntityExtensions.AttributeValue();
    }

    public void PrepareMetadataQuery(
      QueryExpression queryExpression,
      Microsoft.Dynamics.AcceleratedSales.SandboxPlugins.Core.Model.Layout.DesignerConfiguration cardDesign,
      MetadataQueryParams metadataQueryParams)
    {
      metadataQueryParams.Entities.Add("msdyn_sequencetargetstep");
      foreach (string column in (Collection<string>) queryExpression.ColumnSet.Columns)
        metadataQueryParams.Attributes.Add(column);
      foreach (LinkEntity linkEntity in (Collection<LinkEntity>) queryExpression.LinkEntities)
      {
        metadataQueryParams.Entities.Add(linkEntity.LinkToEntityName);
        metadataQueryParams.Attributes.AddRange((IEnumerable<string>) linkEntity.Columns.Columns.ToList<string>());
      }
      if (cardDesign == null)
        return;
      DesignerLayout layout = cardDesign.Layout;
      if (layout == null)
        return;
      layout.PreprareLayoutMetadata("msdyn_sequencetargetstep", metadataQueryParams);
    }

    internal static KeyValuePair<string, IDataExtension> Create()
    {
      return new KeyValuePair<string, IDataExtension>("msdyn_sequencetargetstep", (IDataExtension) new SequenceExtension());
    }
  }
}
