// Decompiled with JetBrains decompiler
// Type: Microsoft.Dynamics.AcceleratedSales.SandboxPlugins.Core.Model.Layout.DesignerLayoutExtensions
// Assembly: Microsoft.Dynamics.AcceleratedSales.SandboxPlugins, Version=1.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35
// MVID: 7BBE7132-7E30-4424-B38D-27667CFBCEFA
// Assembly location: C:\Users\ngoct\Downloads\New folder\Microsoft.Dynamics.AcceleratedSales.SandboxPlugins_1.0.0.0.dll

using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Metadata;
using Microsoft.Xrm.Sdk.Query;
using System;
using System.Collections.Generic;
using System.Linq;

#nullable disable
namespace Microsoft.Dynamics.AcceleratedSales.SandboxPlugins.Core.Model.Layout
{
  public static class DesignerLayoutExtensions
  {
    public static QueryExpression ToQueryExpression(
      this DesignerLayout layout,
      string entityName,
      IEntityMetadataProvider entityMetadata)
    {
      QueryExpression query = new QueryExpression(entityName);
      AttributeMetadata[] attributes = entityMetadata.GetAttributes(entityName);
      OneToManyRelationshipMetadata[] manyRelationships = entityMetadata.GetOneToManyRelationships(entityName);
      OneToManyRelationshipMetadata[] oneRelationships = entityMetadata.GetManyToOneRelationships(entityName);
      layout.AddPersonaAttributesToQuery(entityName, entityMetadata, ref query);
      layout.AddFieldAttributesToQuery(entityName, attributes, manyRelationships, oneRelationships, ref query);
      return query;
    }

    public static Dictionary<string, string> ToDisplayAttributes(
      this DesignerLayout layout,
      Entity record,
      IEntityMetadataProvider entityMetadata)
    {
      string logicalName = record.LogicalName;
      Dictionary<string, string> displayAttributes = new Dictionary<string, string>();
      foreach (KeyValuePair<string, object> attribute in (DataCollection<string, object>) record.Attributes)
      {
        string displayName;
        if (LinkEntityLayoutExtensions.TryGetDisplayKey(attribute.Key, out displayName))
        {
          string str;
          if (record.FormattedValues.TryGetValue(attribute.Key, out str))
          {
            displayAttributes[displayName] = str;
          }
          else
          {
            AliasedValue aliasedValue;
            if (record.TryGetAttributeValue<AliasedValue>(attribute.Key, ref aliasedValue))
              displayAttributes.Add(displayName, aliasedValue.Value.ToString());
          }
        }
      }
      string str1 = string.Empty;
      if (layout.PersonaOption.Equals("RecordType", StringComparison.InvariantCulture))
      {
        string iconVectorUrl = entityMetadata.GetIconVectorUrl(logicalName);
        if (!string.IsNullOrEmpty(iconVectorUrl))
          str1 = iconVectorUrl;
        string objectTypeIconUrl = entityMetadata.GetObjectTypeIconUrl(logicalName);
        if (!string.IsNullOrEmpty(objectTypeIconUrl))
          str1 = objectTypeIconUrl;
      }
      if (layout.PersonaOption.Equals("RecordImage", StringComparison.InvariantCulture))
      {
        string urlAttributeName = entityMetadata.GetPrimaryImageUrlAttributeName(logicalName);
        string str2;
        if (urlAttributeName != null && record.TryGetAttributeValue<string>(urlAttributeName, ref str2))
          str1 = str2;
      }
      displayAttributes["IndexViewRecordImageURL"] = str1;
      return displayAttributes;
    }

    private static void AddPersonaAttributesToQuery(
      this DesignerLayout layout,
      string entityName,
      IEntityMetadataProvider metadata,
      ref QueryExpression query)
    {
      if (!layout.PersonaOption.Equals("RecordImage", StringComparison.InvariantCulture))
        return;
      string urlAttributeName = metadata.GetPrimaryImageUrlAttributeName(entityName);
      if (string.IsNullOrEmpty(urlAttributeName))
        return;
      query.ColumnSet.AddColumn(urlAttributeName);
    }

    private static void AddFieldAttributesToQuery(
      this DesignerLayout layout,
      string entityName,
      AttributeMetadata[] attributes,
      OneToManyRelationshipMetadata[] oneToManyMetadata,
      OneToManyRelationshipMetadata[] manyToOneMetadata,
      ref QueryExpression query)
    {
      List<DesignerRow> rows = layout.Rows;
      IEnumerable<DesignerField> source = rows != null ? rows.Where<DesignerRow>((Func<DesignerRow, bool>) (r => r.Type == DesignerRowType.SimpleRow)).SelectMany<DesignerRow, DesignerField>((Func<DesignerRow, IEnumerable<DesignerField>>) (r => (IEnumerable<DesignerField>) r.Fields ?? (IEnumerable<DesignerField>) new List<DesignerField>())) : (IEnumerable<DesignerField>) null;
      if (source == null || !source.Any<DesignerField>())
        return;
      HashSet<string> stringSet1 = new HashSet<string>(((IEnumerable<AttributeMetadata>) attributes).Select<AttributeMetadata, string>((Func<AttributeMetadata, string>) (a => a.LogicalName)));
      HashSet<string> stringSet2 = new HashSet<string>((IEnumerable<string>) query.ColumnSet.Columns);
      foreach (DesignerField field in source)
      {
        switch (field.Type)
        {
          case DesignerFieldType.SimpleDataField:
            if (!stringSet2.Contains(field.Key))
            {
              query.ColumnSet.AddColumn(field.Key);
              stringSet2.Add(field.Key);
              break;
            }
            break;
          case DesignerFieldType.RelatedDataField:
            DesignerLayoutExtensions.AddLinkedAttributesToQuery(field, oneToManyMetadata, ref query);
            break;
          case DesignerFieldType.CustomField:
            DesignerLayoutExtensions.AddCustomAttributesToQuery(field, entityName, attributes, oneToManyMetadata, manyToOneMetadata, ref query);
            break;
        }
      }
    }

    private static void AddLinkedAttributesToQuery(
      DesignerField field,
      OneToManyRelationshipMetadata[] relationships,
      ref QueryExpression query)
    {
      OneToManyRelationshipMetadata relationship = ((IEnumerable<OneToManyRelationshipMetadata>) relationships).Where<OneToManyRelationshipMetadata>((Func<OneToManyRelationshipMetadata, bool>) (r => r.SchemaName.Equals(field.RelationshipKey, StringComparison.OrdinalIgnoreCase))).FirstOrDefault<OneToManyRelationshipMetadata>();
      if (relationship == null)
        return;
      LinkEntity linkEntity = query.LinkEntities.Where<LinkEntity>((Func<LinkEntity, bool>) (l => l.LinkFromAttributeName.Equals(relationship.ReferencingAttribute, StringComparison.OrdinalIgnoreCase) && l.LinkToEntityName.Equals(relationship.ReferencedEntity, StringComparison.OrdinalIgnoreCase))).FirstOrDefault<LinkEntity>();
      if (linkEntity == null)
      {
        linkEntity = query.AddLink(relationship.ReferencedEntity, relationship.ReferencingAttribute, relationship.ReferencedAttribute);
        linkEntity.JoinOperator = JoinOperator.LeftOuter;
      }
      string columnAlias = LinkEntityLayoutExtensions.GetLinkEntityKey(relationship.SchemaName) + "." + field.Key;
      if (linkEntity.Columns.AttributeExpressions.Any<XrmAttributeExpression>((Func<XrmAttributeExpression, bool>) (c => !string.IsNullOrEmpty(c.Alias) && c.Alias.Equals(columnAlias, StringComparison.Ordinal))))
        return;
      linkEntity.Columns.AttributeExpressions.Add(new XrmAttributeExpression()
      {
        AttributeName = field.Key,
        Alias = columnAlias
      });
    }

    private static void AddCustomAttributesToQuery(
      DesignerField field,
      string entityName,
      AttributeMetadata[] attributes,
      OneToManyRelationshipMetadata[] oneToManyMetadata,
      OneToManyRelationshipMetadata[] manyToOneMetadata,
      ref QueryExpression query)
    {
      if (field.Type != DesignerFieldType.CustomField)
        return;
      if (field.Key == "PriorityScore")
      {
        string schemaName = "msdyn_msdyn_predictivescore_" + entityName;
        OneToManyRelationshipMetadata relationshipMetadata = ((IEnumerable<OneToManyRelationshipMetadata>) oneToManyMetadata).Where<OneToManyRelationshipMetadata>((Func<OneToManyRelationshipMetadata, bool>) (r => r.SchemaName == schemaName)).FirstOrDefault<OneToManyRelationshipMetadata>();
        if (relationshipMetadata == null)
          return;
        LinkEntity link = new LinkEntity(relationshipMetadata.ReferencingEntity, relationshipMetadata.ReferencedEntity, relationshipMetadata.ReferencingAttribute, relationshipMetadata.ReferencedAttribute, JoinOperator.LeftOuter);
        link.EntityAlias = LinkEntityLayoutExtensions.GetLinkEntityKey("PriorityScore");
        DesignerLayoutExtensions.AddColumnAttributeExpressionsToQuery(field.Key, "msdyn_score", ref link);
        if (query.LinkEntities.Any<LinkEntity>((Func<LinkEntity, bool>) (le => le.EntityAlias == link.EntityAlias)))
          return;
        query.LinkEntities.Add(link);
      }
      else
      {
        if (!(field.Key == "FollowIndicator"))
          return;
        string schemaName = entityName + "_PostFollows";
        OneToManyRelationshipMetadata relationshipMetadata = ((IEnumerable<OneToManyRelationshipMetadata>) manyToOneMetadata).Where<OneToManyRelationshipMetadata>((Func<OneToManyRelationshipMetadata, bool>) (r => r.SchemaName == schemaName)).FirstOrDefault<OneToManyRelationshipMetadata>();
        if (relationshipMetadata == null)
          return;
        LinkEntity link = new LinkEntity(relationshipMetadata.ReferencedEntity, relationshipMetadata.ReferencingEntity, relationshipMetadata.ReferencedAttribute, relationshipMetadata.ReferencingAttribute, JoinOperator.LeftOuter);
        FilterExpression childFilter = new FilterExpression();
        childFilter.AddCondition("ownerid", ConditionOperator.EqualUserId);
        link.Columns.AddColumn("postfollowid");
        link.LinkCriteria.AddFilter(childFilter);
        link.EntityAlias = LinkEntityLayoutExtensions.GetLinkEntityKey("FollowIndicator");
        if (query.LinkEntities.Any<LinkEntity>((Func<LinkEntity, bool>) (le => le.EntityAlias == link.EntityAlias)))
          return;
        query.LinkEntities.Add(link);
      }
    }

    private static void AddColumnAttributeExpressionsToQuery(
      string schemaName,
      string logicalName,
      ref LinkEntity link)
    {
      string columnAlias = LinkEntityLayoutExtensions.GetLinkEntityKey(schemaName) + "." + logicalName;
      if (link.Columns.AttributeExpressions.Any<XrmAttributeExpression>((Func<XrmAttributeExpression, bool>) (c => !string.IsNullOrEmpty(c.Alias) && c.Alias.Equals(columnAlias, StringComparison.Ordinal))))
        return;
      link.Columns.AttributeExpressions.Add(new XrmAttributeExpression()
      {
        AttributeName = logicalName,
        Alias = columnAlias
      });
    }
  }
}
