// Decompiled with JetBrains decompiler
// Type: Microsoft.Dynamics.AcceleratedSales.SandboxPlugins.Core.Model.Layout.DesignerLayoutMetadata
// Assembly: Microsoft.Dynamics.AcceleratedSales.SandboxPlugins, Version=1.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35
// MVID: 7BBE7132-7E30-4424-B38D-27667CFBCEFA
// Assembly location: C:\Users\ngoct\Downloads\New folder\Microsoft.Dynamics.AcceleratedSales.SandboxPlugins_1.0.0.0.dll

using System;
using System.Collections.Generic;
using System.Linq;

#nullable disable
namespace Microsoft.Dynamics.AcceleratedSales.SandboxPlugins.Core.Model.Layout
{
  public static class DesignerLayoutMetadata
  {
    public static void PreprareLayoutMetadata(
      this DesignerLayout layout,
      string entityName,
      MetadataQueryParams metadataQueryParams)
    {
      layout.AddPersonaAttributesToQuery(entityName, metadataQueryParams);
      layout.AddFieldAttributesToQuery(entityName, metadataQueryParams);
    }

    private static void AddPersonaAttributesToQuery(
      this DesignerLayout layout,
      string entityName,
      MetadataQueryParams metadataQueryParams)
    {
      if (layout.PersonaOption.Equals("RecordImage", StringComparison.InvariantCulture))
        ;
    }

    private static void AddFieldAttributesToQuery(
      this DesignerLayout layout,
      string entityName,
      MetadataQueryParams metadataQueryParams)
    {
      List<DesignerRow> rows = layout.Rows;
      IEnumerable<DesignerField> source = rows != null ? rows.Where<DesignerRow>((Func<DesignerRow, bool>) (r => r.Type == DesignerRowType.SimpleRow)).SelectMany<DesignerRow, DesignerField>((Func<DesignerRow, IEnumerable<DesignerField>>) (r => (IEnumerable<DesignerField>) r.Fields ?? (IEnumerable<DesignerField>) new List<DesignerField>())) : (IEnumerable<DesignerField>) null;
      if (source == null || !source.Any<DesignerField>())
        return;
      foreach (DesignerField field in source)
      {
        switch (field.Type)
        {
          case DesignerFieldType.SimpleDataField:
            metadataQueryParams.Attributes.Add(field.Key);
            break;
          case DesignerFieldType.RelatedDataField:
            DesignerLayoutMetadata.AddLinkedAttributesToQuery(field, metadataQueryParams);
            break;
          case DesignerFieldType.CustomField:
            DesignerLayoutMetadata.AddCustomAttributesToQuery(field, entityName, metadataQueryParams);
            break;
        }
      }
    }

    private static void AddLinkedAttributesToQuery(
      DesignerField field,
      MetadataQueryParams metadataQueryParams)
    {
      string relationshipKey = field.RelationshipKey;
      if (relationshipKey != null)
        metadataQueryParams.Relationships.Add(relationshipKey);
      string key = field.Key;
      if (key == null)
        return;
      metadataQueryParams.Attributes.Add(key);
    }

    private static void AddCustomAttributesToQuery(
      DesignerField field,
      string entityName,
      MetadataQueryParams metadataQueryParams)
    {
      if (field.Type != DesignerFieldType.CustomField)
        return;
      if (field.Key == "PriorityScore")
      {
        string str = "msdyn_msdyn_predictivescore_" + entityName;
        metadataQueryParams.Entities.Add("msdyn_predictivescore");
        switch (entityName)
        {
          case "lead":
            metadataQueryParams.Attributes.Add("msdyn_leadscore");
            metadataQueryParams.Attributes.Add("msdyn_leadgrade");
            break;
          case "opportunity":
            metadataQueryParams.Attributes.Add("msdyn_opportunityscore");
            metadataQueryParams.Attributes.Add("msdyn_opportunitygrade");
            break;
        }
        metadataQueryParams.Relationships.Add(str);
        metadataQueryParams.Attributes.Add("msdyn_score");
        metadataQueryParams.Attributes.Add("msdyn_grade");
      }
      if (!(field.Key == "FollowIndicator"))
        return;
      metadataQueryParams.Entities.Add("postfollow");
      string str1 = entityName + "_PostFollows";
      metadataQueryParams.Relationships.Add(str1);
      metadataQueryParams.Attributes.Add("postfollowid");
    }
  }
}
