// Decompiled with JetBrains decompiler
// Type: Microsoft.Dynamics.AcceleratedSales.SandboxPlugins.Core.Services.Extensions.LeadExtension
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
  public class LeadExtension : IDataExtension
  {
    private const string LeadEntityName = "lead";
    private const string DefaultDesignerLayout = "{\"personaOption\":\"RecordInitials\",\"hiddenCommands\":[],\"rows\":[{\"fields\":[{\"id\":\"Slot1\",\"type\":1,\"key\":\"fullname\",\"isLocked\":false,\"position\":1},{\"id\":\"Slot2\",\"type\":3,\"key\":\"FollowIndicator\",\"isLocked\":false,\"position\":2},{\"id\":\"Slot3\",\"type\":3,\"key\":\"PriorityScore\",\"isLocked\":false,\"position\":2}],\"id\":\"Row1\",\"type\":1,\"isLocked\":false},{\"fields\":[{\"id\":\"Slot1\",\"type\":1,\"key\":\"jobtitle\",\"isLocked\":false,\"position\":1},{\"id\":\"Slot2\",\"type\":1,\"key\":\"companyname\",\"isLocked\":false,\"position\":1}],\"id\":\"Row2\",\"type\":1,\"isLocked\":false},{\"id\":\"Row3\",\"type\":2,\"key\":\"UpNextActivity\",\"isLocked\":false}]}";

    public KeyValuePair<string, Microsoft.Dynamics.AcceleratedSales.SandboxPlugins.Core.Model.Layout.DesignerConfiguration> DesignerConfiguration
    {
      get
      {
        return new KeyValuePair<string, Microsoft.Dynamics.AcceleratedSales.SandboxPlugins.Core.Model.Layout.DesignerConfiguration>("lead", new Microsoft.Dynamics.AcceleratedSales.SandboxPlugins.Core.Model.Layout.DesignerConfiguration()
        {
          Layout = JsonConvert.DeserializeObject<DesignerLayout>("{\"personaOption\":\"RecordInitials\",\"hiddenCommands\":[],\"rows\":[{\"fields\":[{\"id\":\"Slot1\",\"type\":1,\"key\":\"fullname\",\"isLocked\":false,\"position\":1},{\"id\":\"Slot2\",\"type\":3,\"key\":\"FollowIndicator\",\"isLocked\":false,\"position\":2},{\"id\":\"Slot3\",\"type\":3,\"key\":\"PriorityScore\",\"isLocked\":false,\"position\":2}],\"id\":\"Row1\",\"type\":1,\"isLocked\":false},{\"fields\":[{\"id\":\"Slot1\",\"type\":1,\"key\":\"jobtitle\",\"isLocked\":false,\"position\":1},{\"id\":\"Slot2\",\"type\":1,\"key\":\"companyname\",\"isLocked\":false,\"position\":1}],\"id\":\"Row2\",\"type\":1,\"isLocked\":false},{\"id\":\"Row3\",\"type\":2,\"key\":\"UpNextActivity\",\"isLocked\":false}]}")
        });
      }
    }

    public QueryExpression CreateQuery(
      IEntityMetadataProvider metadata,
      Microsoft.Dynamics.AcceleratedSales.SandboxPlugins.Core.Model.Layout.DesignerConfiguration cardDesign)
    {
      QueryExpression queryExpression;
      if (cardDesign == null)
      {
        queryExpression = (QueryExpression) null;
      }
      else
      {
        DesignerLayout layout = cardDesign.Layout;
        queryExpression = layout != null ? layout.ToQueryExpression("lead", metadata) : (QueryExpression) null;
      }
      return queryExpression ?? new QueryExpression("lead");
    }

    public Entity UpdateRecord(
      Entity entity,
      IEntityMetadataProvider metadata,
      Microsoft.Dynamics.AcceleratedSales.SandboxPlugins.Core.Model.Layout.DesignerConfiguration cardLayout)
    {
      if (cardLayout != null)
      {
        Dictionary<string, string> displayAttributes = cardLayout.Layout.ToDisplayAttributes(entity, metadata);
        entity.Attributes["msdyn_displayattributes"] = (object) displayAttributes;
      }
      entity.Attributes["primaryIdAttribute"] = (object) "leadid";
      entity.Attributes["primaryNameAttribute"] = (object) "fullname";
      return entity;
    }

    public List<Dictionary<string, string>> FormatRecords(
      List<Entity> records,
      IEntityMetadataProvider metadata,
      HashSet<string> skipFormattingAttributes)
    {
      List<Dictionary<string, string>> dictionaryList = new List<Dictionary<string, string>>();
      Dictionary<string, AttributeTypeCode?> dictionary = ((IEnumerable<AttributeMetadata>) metadata.GetAttributes("lead")).ToDictionary<AttributeMetadata, string, AttributeTypeCode?>((Func<AttributeMetadata, string>) (a => a.LogicalName), (Func<AttributeMetadata, AttributeTypeCode?>) (a => a.AttributeType));
      foreach (Entity record in records)
        dictionaryList.Add(record.ToDictionary(dictionary, skipFormattingAttributes));
      return dictionaryList;
    }

    public EntityExtensions.AttributeValue GetRegardingObjectForRecord(
      Dictionary<string, string> record)
    {
      throw new NotImplementedException();
    }

    public void PrepareMetadataQuery(
      QueryExpression queryExpression,
      Microsoft.Dynamics.AcceleratedSales.SandboxPlugins.Core.Model.Layout.DesignerConfiguration cardDesign,
      MetadataQueryParams metadataQueryParams)
    {
      if (queryExpression.ColumnSet.AllColumns)
      {
        metadataQueryParams.AllAttributesEntities.Add("lead");
      }
      else
      {
        metadataQueryParams.Entities.Add("lead");
        metadataQueryParams.Attributes.Add("leadid");
        metadataQueryParams.Attributes.AddRange((IEnumerable<string>) queryExpression.ColumnSet.Columns.ToList<string>());
      }
      foreach (LinkEntity linkEntity in (Collection<LinkEntity>) queryExpression.LinkEntities)
        metadataQueryParams.AddLinkedEntityColumns(linkEntity);
      if (cardDesign == null)
        return;
      DesignerLayout layout = cardDesign.Layout;
      if (layout == null)
        return;
      layout.PreprareLayoutMetadata("lead", metadataQueryParams);
    }

    internal static KeyValuePair<string, IDataExtension> Create()
    {
      return new KeyValuePair<string, IDataExtension>("lead", (IDataExtension) new LeadExtension());
    }
  }
}
