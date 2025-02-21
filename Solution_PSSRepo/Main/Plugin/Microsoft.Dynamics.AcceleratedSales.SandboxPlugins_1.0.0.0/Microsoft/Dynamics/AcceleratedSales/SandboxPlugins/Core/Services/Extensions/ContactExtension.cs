// Decompiled with JetBrains decompiler
// Type: Microsoft.Dynamics.AcceleratedSales.SandboxPlugins.Core.Services.Extensions.ContactExtension
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
  public class ContactExtension : IDataExtension
  {
    private const string ContactEntityName = "contact";
    private const string DefaultDesignerLayout = "{\"personaOption\":\"RecordInitials\",\"hiddenCommands\":[],\"rows\":[{\"id\":\"Row1\",\"type\":1,\"fields\":[{\"id\":\"Slot1\",\"type\":1,\"key\":\"fullname\",\"position\":1},{\"id\":\"Slot2\",\"type\":3,\"key\":\"FollowIndicator\",\"position\":2}]},{\"id\":\"Row2\",\"type\":1,\"fields\":[{\"id\":\"Slot1\",\"type\":1,\"key\":\"jobtitle\",\"position\":1},{\"id\":\"Slot2\",\"type\":1,\"key\":\"parentcustomerid\",\"position\":1}]},{\"id\":\"Row3\",\"type\":2,\"key\":\"UpNextActivity\"}]}";

    public KeyValuePair<string, Microsoft.Dynamics.AcceleratedSales.SandboxPlugins.Core.Model.Layout.DesignerConfiguration> DesignerConfiguration
    {
      get
      {
        return new KeyValuePair<string, Microsoft.Dynamics.AcceleratedSales.SandboxPlugins.Core.Model.Layout.DesignerConfiguration>("contact", new Microsoft.Dynamics.AcceleratedSales.SandboxPlugins.Core.Model.Layout.DesignerConfiguration()
        {
          Layout = JsonConvert.DeserializeObject<DesignerLayout>("{\"personaOption\":\"RecordInitials\",\"hiddenCommands\":[],\"rows\":[{\"id\":\"Row1\",\"type\":1,\"fields\":[{\"id\":\"Slot1\",\"type\":1,\"key\":\"fullname\",\"position\":1},{\"id\":\"Slot2\",\"type\":3,\"key\":\"FollowIndicator\",\"position\":2}]},{\"id\":\"Row2\",\"type\":1,\"fields\":[{\"id\":\"Slot1\",\"type\":1,\"key\":\"jobtitle\",\"position\":1},{\"id\":\"Slot2\",\"type\":1,\"key\":\"parentcustomerid\",\"position\":1}]},{\"id\":\"Row3\",\"type\":2,\"key\":\"UpNextActivity\"}]}")
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
        queryExpression = layout != null ? layout.ToQueryExpression("contact", metadata) : (QueryExpression) null;
      }
      return queryExpression ?? new QueryExpression("contact");
    }

    public Entity UpdateRecord(
      Entity record,
      IEntityMetadataProvider metadata,
      Microsoft.Dynamics.AcceleratedSales.SandboxPlugins.Core.Model.Layout.DesignerConfiguration cardDesign)
    {
      if (cardDesign != null)
      {
        Dictionary<string, string> displayAttributes = cardDesign.Layout.ToDisplayAttributes(record, metadata);
        record.Attributes["msdyn_displayattributes"] = (object) displayAttributes;
      }
      record.Attributes["primaryIdAttribute"] = (object) "contactid";
      record.Attributes["primaryNameAttribute"] = (object) "fullname";
      return record;
    }

    public List<Dictionary<string, string>> FormatRecords(
      List<Entity> records,
      IEntityMetadataProvider metadata,
      HashSet<string> skipFormattingAttributes)
    {
      List<Dictionary<string, string>> dictionaryList = new List<Dictionary<string, string>>();
      Dictionary<string, AttributeTypeCode?> dictionary = ((IEnumerable<AttributeMetadata>) metadata.GetAttributes("contact")).ToDictionary<AttributeMetadata, string, AttributeTypeCode?>((Func<AttributeMetadata, string>) (a => a.LogicalName), (Func<AttributeMetadata, AttributeTypeCode?>) (a => a.AttributeType));
      foreach (Entity record in records)
        dictionaryList.Add(record.ToDictionary(dictionary, skipFormattingAttributes));
      return dictionaryList;
    }

    public void PrepareMetadataQuery(
      QueryExpression queryExpression,
      Microsoft.Dynamics.AcceleratedSales.SandboxPlugins.Core.Model.Layout.DesignerConfiguration cardDesign,
      MetadataQueryParams metadataQueryParams)
    {
      if (queryExpression.ColumnSet.AllColumns)
      {
        metadataQueryParams.AllAttributesEntities.Add("contact");
      }
      else
      {
        metadataQueryParams.Entities.Add("contact");
        metadataQueryParams.Attributes.Add("contactid");
        metadataQueryParams.Attributes.AddRange((IEnumerable<string>) queryExpression.ColumnSet.Columns.ToList<string>());
      }
      foreach (LinkEntity linkEntity in (Collection<LinkEntity>) queryExpression.LinkEntities)
        metadataQueryParams.AddLinkedEntityColumns(linkEntity);
      if (cardDesign == null)
        return;
      DesignerLayout layout = cardDesign.Layout;
      if (layout == null)
        return;
      layout.PreprareLayoutMetadata("contact", metadataQueryParams);
    }

    public EntityExtensions.AttributeValue GetRegardingObjectForRecord(
      Dictionary<string, string> record)
    {
      throw new NotImplementedException();
    }

    internal static KeyValuePair<string, IDataExtension> Create()
    {
      return new KeyValuePair<string, IDataExtension>("contact", (IDataExtension) new ContactExtension());
    }
  }
}
