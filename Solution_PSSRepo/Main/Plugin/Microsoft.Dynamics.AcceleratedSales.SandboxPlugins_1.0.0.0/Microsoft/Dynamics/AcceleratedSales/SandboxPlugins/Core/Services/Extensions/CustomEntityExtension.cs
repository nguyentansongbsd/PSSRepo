// Decompiled with JetBrains decompiler
// Type: Microsoft.Dynamics.AcceleratedSales.SandboxPlugins.Core.Services.Extensions.CustomEntityExtension
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
  public class CustomEntityExtension : IDataExtension
  {
    private const string DefaultCustomEntityDesignerLayoutFormat = "{{\"personaOption\":\"RecordInitials\",\"hiddenCommands\":[],\"rows\":[{{\"id\":\"Row1\",\"type\":1,\"fields\":[{{\"id\":\"Slot1\",\"type\":1,\"key\":\"{0}\",\"position\":1}},{{\"id\":\"Slot2\",\"type\":3,\"key\":\"FollowIndicator\",\"position\":2}}]}},{{\"id\":\"Row2\",\"type\":2,\"key\":\"UpNextActivity\"}}]}}";
    private string entityName;
    private string primaryNameAttribute;

    public CustomEntityExtension(string entityName, string primaryNameAttribute)
    {
      this.entityName = entityName;
      this.primaryNameAttribute = primaryNameAttribute;
    }

    KeyValuePair<string, DesignerConfiguration> IDataExtension.DesignerConfiguration
    {
      get
      {
        return new KeyValuePair<string, DesignerConfiguration>(this.entityName, this.GetDesignerConfiguration(this.primaryNameAttribute));
      }
    }

    public DesignerConfiguration GetDesignerConfiguration(string primaryNameAttribute)
    {
      string str = string.Format("{{\"personaOption\":\"RecordInitials\",\"hiddenCommands\":[],\"rows\":[{{\"id\":\"Row1\",\"type\":1,\"fields\":[{{\"id\":\"Slot1\",\"type\":1,\"key\":\"{0}\",\"position\":1}},{{\"id\":\"Slot2\",\"type\":3,\"key\":\"FollowIndicator\",\"position\":2}}]}},{{\"id\":\"Row2\",\"type\":2,\"key\":\"UpNextActivity\"}}]}}", (object) primaryNameAttribute);
      return new DesignerConfiguration()
      {
        Layout = JsonConvert.DeserializeObject<DesignerLayout>(str)
      };
    }

    public QueryExpression CreateQuery(
      IEntityMetadataProvider metadata,
      DesignerConfiguration cardDesign)
    {
      QueryExpression queryExpression;
      if (cardDesign == null)
      {
        queryExpression = (QueryExpression) null;
      }
      else
      {
        DesignerLayout layout = cardDesign.Layout;
        queryExpression = layout != null ? layout.ToQueryExpression(this.entityName, metadata) : (QueryExpression) null;
      }
      return queryExpression ?? new QueryExpression(this.entityName);
    }

    public List<Dictionary<string, string>> FormatRecords(
      List<Entity> records,
      IEntityMetadataProvider metadata,
      HashSet<string> skipFormattingAttributes)
    {
      List<Dictionary<string, string>> dictionaryList = new List<Dictionary<string, string>>();
      Dictionary<string, AttributeTypeCode?> dictionary = ((IEnumerable<AttributeMetadata>) metadata.GetAttributes(this.entityName)).ToDictionary<AttributeMetadata, string, AttributeTypeCode?>((Func<AttributeMetadata, string>) (a => a.LogicalName), (Func<AttributeMetadata, AttributeTypeCode?>) (a => a.AttributeType));
      foreach (Entity record in records)
        dictionaryList.Add(record.ToDictionary(dictionary, skipFormattingAttributes));
      return dictionaryList;
    }

    public EntityExtensions.AttributeValue GetRegardingObjectForRecord(
      Dictionary<string, string> record)
    {
      throw new NotImplementedException();
    }

    public Entity UpdateRecord(
      Entity entity,
      IEntityMetadataProvider metadata,
      DesignerConfiguration cardLayout)
    {
      if (cardLayout != null)
      {
        Dictionary<string, string> displayAttributes = cardLayout.Layout.ToDisplayAttributes(entity, metadata);
        entity.Attributes["msdyn_displayattributes"] = (object) displayAttributes;
      }
      entity.Attributes["primaryIdAttribute"] = (object) metadata.GetEntityMetadata(this.entityName).PrimaryIdAttribute;
      entity.Attributes["primaryNameAttribute"] = (object) this.primaryNameAttribute;
      return entity;
    }

    public void PrepareMetadataQuery(
      QueryExpression queryExpression,
      DesignerConfiguration cardDesign,
      MetadataQueryParams metadataQueryParams)
    {
      if (queryExpression.ColumnSet.AllColumns)
      {
        metadataQueryParams.AllAttributesEntities.Add(queryExpression.EntityName);
      }
      else
      {
        metadataQueryParams.Entities.Add(queryExpression.EntityName);
        metadataQueryParams.Attributes.AddRange((IEnumerable<string>) queryExpression.ColumnSet.Columns.ToList<string>());
      }
      foreach (LinkEntity linkEntity in (Collection<LinkEntity>) queryExpression.LinkEntities)
        metadataQueryParams.AddLinkedEntityColumns(linkEntity);
      if (cardDesign == null)
        return;
      DesignerLayout layout = cardDesign.Layout;
      if (layout == null)
        return;
      layout.PreprareLayoutMetadata(queryExpression.EntityName, metadataQueryParams);
    }

    internal static KeyValuePair<string, IDataExtension> Create(
      string entityName,
      string primaryNameAttribute)
    {
      return new KeyValuePair<string, IDataExtension>(entityName, (IDataExtension) new CustomEntityExtension(entityName, primaryNameAttribute));
    }
  }
}
