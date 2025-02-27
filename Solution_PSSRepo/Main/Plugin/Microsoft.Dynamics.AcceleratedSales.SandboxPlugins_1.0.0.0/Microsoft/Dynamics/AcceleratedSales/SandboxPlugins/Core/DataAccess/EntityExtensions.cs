// Decompiled with JetBrains decompiler
// Type: Microsoft.Dynamics.AcceleratedSales.SandboxPlugins.Core.DataAccess.EntityExtensions
// Assembly: Microsoft.Dynamics.AcceleratedSales.SandboxPlugins, Version=1.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35
// MVID: 7BBE7132-7E30-4424-B38D-27667CFBCEFA
// Assembly location: C:\Users\ngoct\Downloads\New folder\Microsoft.Dynamics.AcceleratedSales.SandboxPlugins_1.0.0.0.dll

using Microsoft.Crm.AcceleratedSales.Shared.SandboxServices.Platform;
using Microsoft.Dynamics.AcceleratedSales.SandboxPlugins.Core.Model.Layout;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Metadata;
using Microsoft.Xrm.Sdk.Query;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml;

#nullable disable
namespace Microsoft.Dynamics.AcceleratedSales.SandboxPlugins.Core.DataAccess
{
  public static class EntityExtensions
  {
    public static FetchExpression CreateExpression(string fetchXml)
    {
      XmlDocument xmlDocument = new XmlDocument();
      xmlDocument.LoadXml(fetchXml);
      return new FetchExpression(xmlDocument.OuterXml);
    }

    public static T TryGetAttributeValue<T>(
      this Entity entity,
      string attributeName,
      T defaultValue)
    {
      T obj;
      return entity.TryGetAttributeValue<T>(attributeName, ref obj) ? obj : defaultValue;
    }

    public static T GetValidAttributeValue<T>(this Entity entity, string attributeName)
    {
      T validAttributeValue;
      if (entity.TryGetAttributeValue<T>(attributeName, ref validAttributeValue))
        return validAttributeValue;
      throw new CrmException("GetValidAttributeValue missing for attribute: " + attributeName, 1879506951);
    }

    public static Dictionary<string, string> ToDictionary(
      this Entity record,
      Dictionary<string, AttributeTypeCode?> attributeTypes,
      HashSet<string> attributesToSkipFormatting = null)
    {
      Dictionary<string, string> dictionary1 = new Dictionary<string, string>();
      Dictionary<string, string> dictionary2 = record.FormattedValues.ToDictionary<KeyValuePair<string, string>, string, string>((Func<KeyValuePair<string, string>, string>) (f => f.Key), (Func<KeyValuePair<string, string>, string>) (f => f.Value));
      attributesToSkipFormatting = attributesToSkipFormatting ?? new HashSet<string>();
      foreach (KeyValuePair<string, object> attribute in (DataCollection<string, object>) record.Attributes)
      {
        string key = attribute.Key;
        if (record[key] != null && !dictionary1.ContainsKey(key) && !LinkEntityLayoutExtensions.TryGetDisplayKey(key, out string _))
          dictionary1[key] = EntityExtensions.GetAttributeValue(record, key, attributeTypes, dictionary2, attributesToSkipFormatting);
      }
      if (record.LogicalName == "activitypointer" && record.Contains("activitytypecode"))
        dictionary1["activitytypecoderaw"] = EntityExtensions.GetAttributeValue(record, "activitytypecode", attributeTypes, dictionary2, new HashSet<string>()
        {
          "activitytypecode"
        });
      return dictionary1;
    }

    private static string GetAttributeValue(
      Entity record,
      string fieldName,
      Dictionary<string, AttributeTypeCode?> attributes,
      Dictionary<string, string> formattedValues,
      HashSet<string> skipFormatting)
    {
      if (attributes == null)
        throw new Exception("FormatRecordsException: Entity Metadata attributes are null");
      AttributeTypeCode? nullable1;
      if (!attributes.TryGetValue(fieldName, out nullable1) || record[fieldName] is AliasedValue)
      {
        if (!(record[fieldName] is AliasedValue aliasedValue))
          return JsonConvert.SerializeObject(record[fieldName]);
        return aliasedValue.Value is EntityReference entityRef ? EntityExtensions.GetEntityRefValue(fieldName, entityRef, formattedValues) : aliasedValue.Value?.ToString();
      }
      string empty = string.Empty;
      AttributeTypeCode? nullable2 = nullable1;
      string str1;
      if (nullable2.HasValue)
      {
        switch (nullable2.GetValueOrDefault())
        {
          case AttributeTypeCode.Customer:
          case AttributeTypeCode.Lookup:
          case AttributeTypeCode.Owner:
            EntityReference attributeValue = record.GetAttributeValue<EntityReference>(fieldName);
            return EntityExtensions.GetEntityRefValue(fieldName, attributeValue, formattedValues);
          case AttributeTypeCode.DateTime:
            str1 = record.GetAttributeValue<DateTime>(fieldName).ToString("o");
            goto label_16;
          case AttributeTypeCode.Money:
            str1 = record.GetAttributeValue<Money>(fieldName).Value.ToString();
            goto label_16;
          case AttributeTypeCode.Picklist:
          case AttributeTypeCode.State:
          case AttributeTypeCode.Status:
            str1 = record.GetAttributeValue<OptionSetValue>(fieldName).Value.ToString();
            goto label_16;
          case AttributeTypeCode.ManagedProperty:
            str1 = record.GetAttributeValue<BooleanManagedProperty>(fieldName).Value.ToString();
            goto label_16;
        }
      }
      str1 = record[fieldName].ToString();
label_16:
      string str2;
      return skipFormatting.Contains(fieldName) ? str1 : (formattedValues.TryGetValue(fieldName, out str2) ? str2 : str1);
    }

    private static string GetEntityRefValue(
      string fieldName,
      EntityReference entityRef,
      Dictionary<string, string> formattedValues)
    {
      string str1;
      string str2 = formattedValues.TryGetValue(fieldName, out str1) ? str1 : entityRef?.Id.ToString();
      return JsonConvert.SerializeObject((object) new EntityExtensions.AttributeValue()
      {
        Value = (object) entityRef?.Id,
        FormattedValue = (object) str2,
        Entity = (object) entityRef?.LogicalName
      });
    }

    public class AttributeValue
    {
      public object Value { get; set; }

      public object FormattedValue { get; set; }

      public object Entity { get; set; }
    }
  }
}
