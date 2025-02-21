// Decompiled with JetBrains decompiler
// Type: Microsoft.Dynamics.AcceleratedSales.SandboxPlugins.Core.Services.Views.SalesAcceleratorView
// Assembly: Microsoft.Dynamics.AcceleratedSales.SandboxPlugins, Version=1.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35
// MVID: 7BBE7132-7E30-4424-B38D-27667CFBCEFA
// Assembly location: C:\Users\ngoct\Downloads\New folder\Microsoft.Dynamics.AcceleratedSales.SandboxPlugins_1.0.0.0.dll

using Microsoft.Dynamics.AcceleratedSales.SandboxPlugins.Core.DataAccess;
using Microsoft.Dynamics.AcceleratedSales.SandboxPlugins.Core.Model;
using Microsoft.Dynamics.AcceleratedSales.SandboxPlugins.Core.Model.DefaultSort;
using Microsoft.Dynamics.AcceleratedSales.SandboxPlugins.Core.Model.Filter;
using Microsoft.Dynamics.AcceleratedSales.SandboxPlugins.Core.Model.Sort;
using Microsoft.Dynamics.AcceleratedSales.SandboxPlugins.Core.Model.Tags;
using Microsoft.Dynamics.AcceleratedSales.SandboxPlugins.Core.Services.Commands;
using Microsoft.Dynamics.AcceleratedSales.SandboxPlugins.Core.Services.Extensions;
using Microsoft.Xrm.Sdk.Metadata;
using Microsoft.Xrm.Sdk.Query;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;

#nullable disable
namespace Microsoft.Dynamics.AcceleratedSales.SandboxPlugins.Core.Services.Views
{
  public class SalesAcceleratorView
  {
    private static string defaultFilterConfiguration = "{\"Version\":\"d74438b3-b2e4-446e-b894-914ce2caa741\",\"Groups\":[{\"Name\":\"Default Filters\",\"Id\":\"a00000a0-a0a0-000a-a000-000aa0aaa000\",\"Position\":0,\"IsDefaultSelected\":true,\"GroupType\":0,\"Visibility\":true,\"Filters\":[{\"Name\":\"Unopened\",\"Position\":0,\"Id\":\"a00000a0-a0a0-000a-a000-000aa0aaa001\",\"Filtertype\":0,\"Icon\":\"Hide3\",\"IsCustomName\":true,\"Visibility\":true,\"Metadata\":{\"msdyn_workqueuerecord\":{\"isRead\":{\"ControlType\":4}}}},{\"Name\":\"Followed\",\"Position\":1,\"Id\":\"a00000a0-a0a0-000a-a000-000aa0aaa002\",\"Filtertype\":0,\"Icon\":\"FavoriteStarFill\",\"IsCustomName\":true,\"Visibility\":true,\"Metadata\":{\"msdyn_workqueuerecord\":{\"postFollowId\":{\"ControlType\":4}}}},{\"Name\":\"Due by\",\"Position\":2,\"Id\":\"a00000a0-a0a0-000a-a000-000aa0aaa003\",\"Filtertype\":0,\"IsCustomName\":true,\"Visibility\":true,\"Metadata\":{\"msdyn_workqueuerecord\":{\"dueTime\":{\"ControlType\":8}}}},{\"Name\":\"Record type\",\"Position\":3,\"Id\":\"a00000a0-a0a0-000a-a000-000aa0aaa004\",\"Filtertype\":0,\"IsCustomName\":true,\"Visibility\":true,\"Metadata\":{\"msdyn_workqueuerecord\":{\"entityType\":{\"ControlType\":3}}}},{\"Name\":\"Activity type\",\"Position\":4,\"Id\":\"a00000a0-a0a0-000a-a000-000aa0aaa005\",\"Filtertype\":0,\"IsCustomName\":true,\"Visibility\":true,\"Metadata\":{\"msdyn_workqueuerecord\":{\"nextActionType\":{\"ControlType\":3}}}},{\"Name\":\"Account name\",\"Position\":5,\"Id\":\"a00000a0-a0a0-000a-a000-000aa0aaa006\",\"Filtertype\":0,\"IsCustomName\":true,\"Visibility\":true,\"Metadata\":{\"msdyn_salessuggestion\":{\"relatedRecordId\":{\"ControlType\":2,\"AttributeLocalizedName\":\"Account Name\"}}}},{\"Name\":\"Potential revenue\",\"Position\":6,\"Id\":\"a00000a0-a0a0-000a-a000-000aa0aaa009\",\"Filtertype\":0,\"IsCustomName\":true,\"Visibility\":true,\"Metadata\":{\"msdyn_salessuggestion\":{\"potentialRevenue\":{\"ControlType\":3}}}},{\"Name\":\"View type\",\"Position\":7,\"Id\":\"a00000a0-a0a0-000a-a000-000aa0aaa012\",\"Filtertype\":0,\"IsCustomName\":true,\"Visibility\":true,\"Metadata\":{\"msdyn_workqueuerecord\":{\"workQueueRecordType\":{\"ControlType\":8}}}},{\"Name\":\"Suggestion status\",\"Position\":8,\"Id\":\"a00000a0-a0a0-000a-a000-000aa0aaa013\",\"Filtertype\":0,\"IsCustomName\":true,\"Visibility\":true,\"Metadata\":{\"msdyn_salessuggestion\":{\"statusCode\":{\"ControlType\":8}}}},{\"Name\":\"Suggestions with activities\",\"Position\":9,\"Id\":\"a00000a0-a0a0-000a-a000-000aa0aaa014\",\"Filtertype\":0,\"IsCustomName\":true,\"Visibility\":true,\"Metadata\":{\"msdyn_salessuggestion\":{\"relatedRecord\":{\"ControlType\":4}}}},{\"Name\":\"Suggestions assigned to others\",\"Position\":10,\"Id\":\"a00000a0-a0a0-000a-a000-000aa0aaa011\",\"Filtertype\":0,\"IsCustomName\":true,\"Visibility\":true,\"Metadata\":{\"msdyn_salessuggestion\":{\"accessRightsMask\":{\"ControlType\":4}}}}]}]}";
    private static string defaultSortConfiguration = "{\"Version\":\"8899d163-6781-4af4-b850-0d8a88e9ed1e\",\"SortOptions\":[{\"Name\":\"Due date\",\"Position\":0,\"Id\":\"b00000b0-b0b0-000b-b000-000bb0bbb000\",\"IsCustomName\":true,\"IsDefault\":true,\"Visibility\":true,\"IsSystemDefined\":true,\"Metadata\":{\"msdyn_workqueuerecord\":{\"msdyn_duetime\":{\"AttributeTypeCode\":\"\"}}}},{\"Name\":\"Score\",\"Position\":1,\"Id\":\"b00000b0-b0b0-000b-b000-000bb0bbb001\",\"IsCustomName\":true,\"IsDefault\":false,\"Visibility\":true,\"IsSystemDefined\":true,\"Metadata\":{\"msdyn_workqueuerecord\":{\"msdyn_priorityscore\":{\"AttributeTypeCode\":\"\"}}}},{\"Name\":\"Name\",\"Position\":2,\"Id\":\"b00000b0-b0b0-000b-b000-000bb0bbb002\",\"IsCustomName\":true,\"IsDefault\":false,\"Visibility\":true,\"IsSystemDefined\":true,\"Metadata\":{\"msdyn_workqueuerecord\":{\"msdyn_primaryname\":{\"AttributeTypeCode\":\"\"}}}},{\"Name\":\"Activity type\",\"Position\":3,\"Id\":\"b00000b0-b0b0-000b-b000-000bb0bbb003\",\"IsCustomName\":true,\"IsDefault\":false,\"Visibility\":true,\"IsSystemDefined\":true,\"Metadata\":{\"msdyn_workqueuerecord\":{\"msdyn_nextactionsource\":{\"AttributeTypeCode\":\"\"}}}},{\"Name\":\"Record type\",\"Position\":4,\"Id\":\"b00000b0-b0b0-000b-b000-000bb0bbb004\",\"IsCustomName\":true,\"IsDefault\":false,\"Visibility\":true,\"IsSystemDefined\":true,\"Metadata\":{\"msdyn_workqueuerecord\":{\"msdyn_entitytypelogicalname\":{\"AttributeTypeCode\":\"\"}}}},{\"Name\":\"Sequence name\",\"Position\":5,\"Id\":\"b00000b0-b0b0-000b-b000-000bb0bbb005\",\"IsCustomName\":true,\"IsDefault\":false,\"Visibility\":true,\"IsSystemDefined\":true,\"Metadata\":{\"msdyn_workqueuerecord\":{\"msdyn_sequencename\":{\"AttributeTypeCode\":\"\"}}}},{\"Id\":\"b00000b0-b0b0-000b-b000-000bb0bbb009\",\"Name\":\"Potential revenue\",\"Position\":6,\"IsCustomName\":true,\"IsDefault\":false,\"IsSystemDefined\":true,\"Visibility\":true,\"Metadata\":{\"msdyn_salessuggestion\":{\"msdyn_potentialrevenue\":{\"AttributeTypeCode\":\"8\"}}}}]}";
    private static List<string> actionsPriorityOrder = new List<string>()
    {
      "0",
      "1",
      "3",
      "4202",
      "4212",
      "4210",
      "4201"
    };
    private static List<string> activityActionsPriorityOrder = new List<string>()
    {
      "wait",
      "simplecondition",
      "automatedemail",
      "email",
      "task",
      "phonecall",
      "appointment"
    };
    private static string logicalname = nameof (logicalname);

    public static Dictionary<string, HashSet<string>> GetAdditionalAttributes(
      FilterConfiguration filterConfiguration,
      SortConfiguration sortConfiguration,
      TagsConfiguration tagsConfiguration,
      DefaultSortConfiguration defaultSortConfig,
      IEntityMetadataProvider entityMetadataProvider)
    {
      Dictionary<string, HashSet<string>> additionalAttributes = new Dictionary<string, HashSet<string>>();
      filterConfiguration.AddAttributes(entityMetadataProvider, ref additionalAttributes);
      sortConfiguration.AddAttributes(entityMetadataProvider, ref additionalAttributes);
      tagsConfiguration.AddAttributes(entityMetadataProvider, ref additionalAttributes);
      defaultSortConfig.AddAttributes(entityMetadataProvider, ref additionalAttributes);
      return additionalAttributes;
    }

    public static Dictionary<string, List<QueryFilter>> GetDefaultFilters(List<string> entityNames)
    {
      Dictionary<string, List<QueryFilter>> defaultFilters = new Dictionary<string, List<QueryFilter>>();
      foreach (string entityName in entityNames)
      {
        switch (entityName)
        {
          case "activitypointer":
            defaultFilters.Add(entityName, new List<QueryFilter>()
            {
              new QueryFilter()
              {
                AttributeName = "scheduledend",
                Operator = ConditionOperator.OnOrBefore,
                Values = new List<object>()
                {
                  (object) DateTime.UtcNow.AddDays(30.0)
                }
              },
              new QueryFilter()
              {
                AttributeName = "scheduledend",
                Operator = ConditionOperator.OnOrAfter,
                Values = new List<object>()
                {
                  (object) DateTime.UtcNow.AddDays(-30.0)
                }
              }
            });
            break;
          case "msdyn_sequencetargetstep":
            defaultFilters.Add(entityName, new List<QueryFilter>()
            {
              new QueryFilter()
              {
                AttributeName = "msdyn_duetime",
                Operator = ConditionOperator.OnOrBefore,
                Values = new List<object>()
                {
                  (object) DateTime.UtcNow.AddDays(30.0)
                }
              },
              new QueryFilter()
              {
                AttributeName = "msdyn_duetime",
                Operator = ConditionOperator.OnOrAfter,
                Values = new List<object>()
                {
                  (object) DateTime.UtcNow.AddDays(-30.0)
                }
              }
            });
            break;
        }
      }
      return defaultFilters;
    }

    public static Dictionary<string, QuerySort> GetDefaultSortItems(List<string> entityNames)
    {
      Dictionary<string, QuerySort> defaultSortItems = new Dictionary<string, QuerySort>();
      foreach (string entityName in entityNames)
      {
        switch (entityName)
        {
          case "activitypointer":
            defaultSortItems.Add(entityName, new QuerySort()
            {
              AttributeName = "scheduledend",
              OrderType = OrderType.Descending
            });
            break;
          case "msdyn_sequencetargetstep":
            defaultSortItems.Add(entityName, new QuerySort()
            {
              AttributeName = "msdyn_duetime",
              OrderType = OrderType.Descending
            });
            break;
        }
      }
      return defaultSortItems;
    }

    public static void AddDefaultAttributes(
      List<string> entityNames,
      IEntityMetadataProvider entityMetadataProvider,
      ref Dictionary<string, HashSet<string>> additionalAttributes)
    {
      foreach (string entityName in entityNames)
      {
        if (!additionalAttributes.ContainsKey(entityName))
          additionalAttributes.Add(entityName, new HashSet<string>());
        additionalAttributes[entityName].Add(entityMetadataProvider.GetEntityMetadata(entityName).PrimaryIdAttribute);
        additionalAttributes[entityName].Add(entityMetadataProvider.GetEntityMetadata(entityName).PrimaryNameAttribute);
      }
    }

    public static Dictionary<string, List<Dictionary<string, string>>> AggregateRelatedRecordsByDueDate(
      Dictionary<string, IDataExtension> relatedExtensions,
      Dictionary<string, List<Dictionary<string, string>>> relatedRecords,
      int resultSize = 250)
    {
      if (relatedRecords == null || relatedExtensions == null)
        return new Dictionary<string, List<Dictionary<string, string>>>();
      List<Dictionary<string, string>> records1;
      relatedRecords.TryGetValue("msdyn_sequencetargetstep", out records1);
      List<Dictionary<string, string>> dictionaryList;
      relatedRecords.TryGetValue("activitypointer", out dictionaryList);
      records1 = records1 ?? new List<Dictionary<string, string>>();
      List<Dictionary<string, string>> records2 = dictionaryList ?? new List<Dictionary<string, string>>();
      Dictionary<Guid, Dictionary<string, string>> aggregatedRecords = new Dictionary<Guid, Dictionary<string, string>>();
      records1.ForEach((Action<Dictionary<string, string>>) (record =>
      {
        record.Add("due", string.Empty);
        record.Add(SalesAcceleratorView.logicalname, "msdyn_sequencetargetstep");
        string str;
        if (!record.TryGetValue("msdyn_duetime", out str))
          return;
        record["due"] = str;
      }));
      records2.ForEach((Action<Dictionary<string, string>>) (record =>
      {
        record.Add("due", string.Empty);
        record.Add(SalesAcceleratorView.logicalname, "activitypointer");
        string str1;
        if (!record.TryGetValue("activitytypecode", out str1))
          return;
        string str2;
        if (str1 == "appointment" && record.TryGetValue("scheduledstart", out str2))
        {
          record["due"] = str2;
        }
        else
        {
          string str3;
          if (record.TryGetValue("scheduledend", out str3))
            record["due"] = str3;
        }
      }));
      SalesAcceleratorView.AddRecords(records1, "msdyn_sequencetargetstep", relatedExtensions, ref aggregatedRecords);
      SalesAcceleratorView.AddRecords(records2, "activitypointer", relatedExtensions, ref aggregatedRecords);
      List<Dictionary<string, string>> list1 = aggregatedRecords.Values.ToList<Dictionary<string, string>>();
      list1.Sort((Comparison<Dictionary<string, string>>) ((v1, v2) =>
      {
        DateTime result1;
        DateTime.TryParse(v1["due"], out result1);
        DateTime result2;
        DateTime.TryParse(v2["due"], out result2);
        return result1.CompareTo(result2);
      }));
      List<Dictionary<string, string>> list2 = list1.Take<Dictionary<string, string>>(resultSize * 2).ToList<Dictionary<string, string>>();
      records1 = list2.Where<Dictionary<string, string>>((Func<Dictionary<string, string>, bool>) (record => record[SalesAcceleratorView.logicalname] == "msdyn_sequencetargetstep")).ToList<Dictionary<string, string>>();
      List<Dictionary<string, string>> list3 = list2.Where<Dictionary<string, string>>((Func<Dictionary<string, string>, bool>) (record => record[SalesAcceleratorView.logicalname] == "activitypointer")).ToList<Dictionary<string, string>>();
      if (!relatedRecords.ContainsKey("msdyn_sequencetargetstep"))
        relatedRecords.Add("msdyn_sequencetargetstep", records1);
      else
        relatedRecords["msdyn_sequencetargetstep"] = records1;
      if (!relatedRecords.ContainsKey("activitypointer"))
        relatedRecords.Add("activitypointer", list3);
      else
        relatedRecords["activitypointer"] = list3;
      return relatedRecords;
    }

    public static Dictionary<string, HashSet<string>> GetSkipFormattingAttributes(
      Dictionary<string, HashSet<string>> attributes,
      IEntityMetadataProvider entityMetadataProvider,
      IAcceleratedSalesLogger logger)
    {
      Dictionary<string, HashSet<string>> formattingAttributes = new Dictionary<string, HashSet<string>>();
      foreach (KeyValuePair<string, HashSet<string>> attribute in attributes)
      {
        string key1 = attribute.Key;
        try
        {
          formattingAttributes.Add(key1, new HashSet<string>());
          Dictionary<string, AttributeTypeCode?> dictionary = ((IEnumerable<AttributeMetadata>) entityMetadataProvider.GetAttributes(key1)).ToDictionary<AttributeMetadata, string, AttributeTypeCode?>((Func<AttributeMetadata, string>) (a => a.LogicalName), (Func<AttributeMetadata, AttributeTypeCode?>) (a => a.AttributeType));
          foreach (string key2 in attribute.Value)
          {
            AttributeTypeCode? nullable1;
            if (dictionary.TryGetValue(key2, out nullable1))
            {
              AttributeTypeCode? nullable2 = nullable1;
              if (nullable2.HasValue)
              {
                switch (nullable2.GetValueOrDefault())
                {
                  case AttributeTypeCode.Boolean:
                  case AttributeTypeCode.Decimal:
                  case AttributeTypeCode.Double:
                  case AttributeTypeCode.Integer:
                  case AttributeTypeCode.Money:
                  case AttributeTypeCode.Picklist:
                  case AttributeTypeCode.State:
                  case AttributeTypeCode.Status:
                  case AttributeTypeCode.BigInt:
                    formattingAttributes[key1].Add(key2);
                    break;
                }
              }
            }
          }
        }
        catch (Exception ex)
        {
          logger.AddCustomProperty("SalesAcceleratorView.GetSkipFormattingAttributes." + key1 + ".Exception", (object) ex);
        }
      }
      return formattingAttributes;
    }

    public static FilterConfiguration GetDefaultFilterConfiguration(bool isSuggestionEnabled)
    {
      return isSuggestionEnabled ? JsonConvert.DeserializeObject<FilterConfiguration>(SalesAcceleratorView.defaultFilterConfiguration) : (FilterConfiguration) null;
    }

    public static SortConfiguration GetDefaultSortConfiguration(bool isSuggestionEnabled)
    {
      return isSuggestionEnabled ? JsonConvert.DeserializeObject<SortConfiguration>(SalesAcceleratorView.defaultSortConfiguration) : (SortConfiguration) null;
    }

    public static int GetPriorityOrder(
      Dictionary<string, string> existingRecord,
      Dictionary<string, string> newRecord)
    {
      int priorityOrder = SalesAcceleratorView.CompareBasedOnAppointmentPrioritization(existingRecord, newRecord);
      if (priorityOrder == 0)
        priorityOrder = SalesAcceleratorView.CompareBasedOnDueDate(existingRecord, newRecord);
      if (priorityOrder == 0)
        priorityOrder = SalesAcceleratorView.CompareBasedOnActivityType(existingRecord, newRecord);
      if (priorityOrder == 0)
        priorityOrder = SalesAcceleratorView.CompareBasedOnCreatedTime(existingRecord, newRecord);
      return priorityOrder;
    }

    private static void AddRecords(
      List<Dictionary<string, string>> records,
      string entityName,
      Dictionary<string, IDataExtension> relatedExtensions,
      ref Dictionary<Guid, Dictionary<string, string>> aggregatedRecords)
    {
      if (!relatedExtensions.ContainsKey(entityName))
        return;
      foreach (Dictionary<string, string> record in records)
      {
        EntityExtensions.AttributeValue regardingObjectForRecord = relatedExtensions[entityName].GetRegardingObjectForRecord(record);
        Guid result;
        if (regardingObjectForRecord.Entity != null && regardingObjectForRecord.Value != null && Guid.TryParse(regardingObjectForRecord.Value.ToString(), out result))
        {
          if (!aggregatedRecords.ContainsKey(result))
            aggregatedRecords.Add(result, record);
          else if (SalesAcceleratorView.GetPriorityOrder(aggregatedRecords[result], record) > 0)
            aggregatedRecords[result] = record;
        }
      }
    }

    private static int CompareBasedOnAppointmentPrioritization(
      Dictionary<string, string> existingRecord,
      Dictionary<string, string> newRecord)
    {
      bool flag1 = existingRecord.ContainsKey("activitytypecode") && existingRecord["activitytypecode"] == "appointment";
      bool flag2 = newRecord.ContainsKey("activitytypecode") && newRecord["activitytypecode"] == "appointment";
      if (!flag1 && !flag2)
        return 0;
      DateTime result1;
      DateTime.TryParse(existingRecord["due"], out result1);
      DateTime result2;
      DateTime.TryParse(newRecord["due"], out result2);
      if (flag1 & flag2)
      {
        bool appointmentOngoing1 = SalesAcceleratorView.GetIsAppointmentOngoing(existingRecord);
        bool appointmentOngoing2 = SalesAcceleratorView.GetIsAppointmentOngoing(newRecord);
        if (appointmentOngoing1 & appointmentOngoing2)
          return result1 < result2 ? -1 : 1;
        if (appointmentOngoing1)
          return -1;
        if (appointmentOngoing2)
          return 1;
      }
      else
      {
        if (flag2 && SalesAcceleratorView.GetIsAppointmentOngoing(newRecord))
          return 1;
        if (flag1 && SalesAcceleratorView.GetIsAppointmentOngoing(existingRecord))
          return -1;
      }
      return 0;
    }

    private static bool GetIsAppointmentOngoing(Dictionary<string, string> record)
    {
      DateTime result1;
      DateTime.TryParse(record["due"], out result1);
      DateTime result2;
      DateTime.TryParse(record["scheduledend"], out result2);
      DateTime utcNow = DateTime.UtcNow;
      return result1 <= utcNow.AddMinutes(15.0) && result2 > utcNow;
    }

    private static int CompareBasedOnActivityType(
      Dictionary<string, string> existingRecord,
      Dictionary<string, string> newRecord)
    {
      int basedOnActivityType1 = SalesAcceleratorView.GetPriorityBasedOnActivityType(existingRecord);
      int basedOnActivityType2 = SalesAcceleratorView.GetPriorityBasedOnActivityType(newRecord);
      if (basedOnActivityType1 == -1 && basedOnActivityType2 == -1)
        return 0;
      if (basedOnActivityType1 == -1)
        return 1;
      return basedOnActivityType2 == -1 ? -1 : basedOnActivityType1 - basedOnActivityType2;
    }

    private static int GetPriorityBasedOnActivityType(Dictionary<string, string> record)
    {
      return record[SalesAcceleratorView.logicalname] == "activitypointer" ? SalesAcceleratorView.activityActionsPriorityOrder.FindIndex((Predicate<string>) (action => record.ContainsKey(SalesAcceleratorView.GetActivityTypeAttributeName(record[SalesAcceleratorView.logicalname])) && action == record[SalesAcceleratorView.GetActivityTypeAttributeName(record[SalesAcceleratorView.logicalname])])) : SalesAcceleratorView.actionsPriorityOrder.FindIndex((Predicate<string>) (action => record.ContainsKey(SalesAcceleratorView.GetActivityTypeAttributeName(record[SalesAcceleratorView.logicalname])) && action == record[SalesAcceleratorView.GetActivityTypeAttributeName(record[SalesAcceleratorView.logicalname])]));
    }

    private static int CompareBasedOnDueDate(
      Dictionary<string, string> record1,
      Dictionary<string, string> record2)
    {
      DateTime result1;
      if (!DateTime.TryParse(record1["due"], out result1))
        return 1;
      DateTime result2;
      return !DateTime.TryParse(record2["due"], out result2) ? -1 : (result2 < result1 ? 1 : (result2 == result1 ? 0 : -1));
    }

    private static int CompareBasedOnCreatedTime(
      Dictionary<string, string> record1,
      Dictionary<string, string> record2)
    {
      DateTime result1;
      if (!DateTime.TryParse(record1["createdon"], out result1))
        return 1;
      DateTime result2;
      return !DateTime.TryParse(record2["createdon"], out result2) ? -1 : (result2 < result1 ? 1 : (result2 == result1 ? 0 : -1));
    }

    private static string GetActivityTypeAttributeName(string logicalName)
    {
      return logicalName == "activitypointer" ? "activitytypecode" : "msdyn_type";
    }
  }
}
