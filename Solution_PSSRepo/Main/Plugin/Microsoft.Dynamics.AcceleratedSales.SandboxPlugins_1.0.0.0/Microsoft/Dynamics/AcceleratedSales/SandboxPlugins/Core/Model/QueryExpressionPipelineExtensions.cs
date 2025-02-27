// Decompiled with JetBrains decompiler
// Type: Microsoft.Dynamics.AcceleratedSales.SandboxPlugins.Core.Model.QueryExpressionPipelineExtensions
// Assembly: Microsoft.Dynamics.AcceleratedSales.SandboxPlugins, Version=1.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35
// MVID: 7BBE7132-7E30-4424-B38D-27667CFBCEFA
// Assembly location: C:\Users\ngoct\Downloads\New folder\Microsoft.Dynamics.AcceleratedSales.SandboxPlugins_1.0.0.0.dll

using Microsoft.Dynamics.AcceleratedSales.SandboxPlugins.Core.Model.Layout;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Metadata;
using Microsoft.Xrm.Sdk.Query;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Xml;

#nullable disable
namespace Microsoft.Dynamics.AcceleratedSales.SandboxPlugins.Core.Model
{
  public static class QueryExpressionPipelineExtensions
  {
    public static QueryExpression Merge(
      this QueryExpression query,
      List<QueryExpression> queries,
      IEntityMetadataProvider metadata)
    {
      if (!queries.Any<QueryExpression>())
        return query;
      string entityName = query.EntityName;
      HashSet<string> attributes = new HashSet<string>(((IEnumerable<AttributeMetadata>) metadata.GetAttributes(entityName)).Select<AttributeMetadata, string>((Func<AttributeMetadata, string>) (a => a.LogicalName)));
      List<QueryExpression> list = queries.Where<QueryExpression>((Func<QueryExpression, bool>) (q => q.EntityName.Equals(entityName, StringComparison.OrdinalIgnoreCase))).ToList<QueryExpression>();
      HashSet<string> initialColumnSet = new HashSet<string>((IEnumerable<string>) query.ColumnSet.Columns);
      HashSet<string> source = new HashSet<string>(list.SelectMany<QueryExpression, string>((Func<QueryExpression, IEnumerable<string>>) (q => (IEnumerable<string>) q.ColumnSet.Columns)).Where<string>((Func<string, bool>) (c => !initialColumnSet.Contains(c))));
      query.ColumnSet.AddColumns(source.Where<string>((Func<string, bool>) (c => attributes.Contains(c))).ToArray<string>());
      Dictionary<string, List<LinkEntity>> dictionary = query.LinkEntities.Where<LinkEntity>((Func<LinkEntity, bool>) (l => !string.IsNullOrEmpty(l.LinkToEntityName))).GroupBy<LinkEntity, string>((Func<LinkEntity, string>) (l => l.GetLinkEntityKey())).ToDictionary<IGrouping<string, LinkEntity>, string, List<LinkEntity>>((Func<IGrouping<string, LinkEntity>, string>) (g => g.Key), (Func<IGrouping<string, LinkEntity>, List<LinkEntity>>) (g => g.ToList<LinkEntity>()));
      foreach (LinkEntity linkEntity1 in list.SelectMany<QueryExpression, LinkEntity>((Func<QueryExpression, IEnumerable<LinkEntity>>) (q => (IEnumerable<LinkEntity>) q.LinkEntities)).ToList<LinkEntity>())
      {
        LinkEntity linkEntity = linkEntity1;
        List<LinkEntity> linkEntityList;
        if (dictionary.TryGetValue(linkEntity.GetLinkEntityKey(), out linkEntityList))
        {
          linkEntityList.ForEach((Action<LinkEntity>) (link =>
          {
            HashSet<string> aliases = new HashSet<string>(link.Columns.AttributeExpressions.Select<XrmAttributeExpression, string>((Func<XrmAttributeExpression, string>) (a => a.Alias)));
            link.Columns.AttributeExpressions.AddRange(linkEntity.Columns.AttributeExpressions.Where<XrmAttributeExpression>((Func<XrmAttributeExpression, bool>) (a => !aliases.Contains(a.Alias))));
          }));
        }
        else
        {
          query.LinkEntities.Add(linkEntity);
          dictionary.Add(linkEntity.GetLinkEntityKey(), new List<LinkEntity>()
          {
            linkEntity
          });
        }
      }
      return query;
    }

    public static void AddClientFilter(
      this QueryExpression query,
      List<QueryFilter> filters,
      QuerySort sort,
      bool removeEntityNameInFilterCondition = false)
    {
      List<QueryFilter> queryFilterList = filters ?? new List<QueryFilter>();
      string str1 = QueryExpressionPipelineExtensions.FetchAliasForLinkEntityIfSameEntityLinkExists(query.LinkEntities, query.EntityName);
      string str2 = str1 ?? query.EntityName;
      FilterExpression filterExpression1 = query.Criteria.AddFilter(LogicalOperator.And);
      Dictionary<string, FilterExpression> dictionary1 = new Dictionary<string, FilterExpression>();
      Dictionary<string, FilterExpression> dictionary2 = new Dictionary<string, FilterExpression>();
      foreach (QueryFilter queryFilter in queryFilterList)
      {
        if (!string.IsNullOrEmpty(queryFilter.EntityName))
        {
          if (!queryFilter.EntityName.Equals(query.EntityName, StringComparison.Ordinal))
          {
            if (queryFilter.EntityName == "msdyn_predictivescore")
            {
              LinkEntity linkEntity = query.LinkEntities.FirstOrDefault<LinkEntity>((Func<LinkEntity, bool>) (le => le.EntityAlias == LinkEntityLayoutExtensions.GetLinkEntityKey("PriorityScore")));
              if (linkEntity != null)
              {
                FilterExpression filterExpression2;
                if (!dictionary2.TryGetValue(queryFilter.AttributeName, out filterExpression2))
                {
                  filterExpression2 = linkEntity.LinkCriteria.AddFilter(LogicalOperator.Or);
                  dictionary2.Add(queryFilter.AttributeName, filterExpression2);
                }
                filterExpression2.AddCondition(queryFilter.ToConditionExpression(linkEntity.LinkToEntityName));
              }
            }
            else
              continue;
          }
          FilterExpression filterExpression3;
          if (!dictionary1.TryGetValue(queryFilter.AttributeName, out filterExpression3))
          {
            filterExpression3 = filterExpression1.AddFilter(LogicalOperator.Or);
            dictionary1.Add(queryFilter.AttributeName, filterExpression3);
          }
          filterExpression3.AddCondition(queryFilter.ToConditionExpression(queryFilter.EntityName == "msdyn_predictivescore" ? queryFilter.EntityName : str2, str1 != null & removeEntityNameInFilterCondition));
        }
      }
      QuerySort inputSort = sort ?? new QuerySort();
      if (string.IsNullOrEmpty(inputSort.AttributeName))
        return;
      if (string.IsNullOrEmpty(inputSort.EntityName) || inputSort.EntityName.Equals(query.EntityName, StringComparison.Ordinal))
      {
        query.Orders.Clear();
        query.Orders.Add(sort.ToOrderExpression());
      }
      else
      {
        LinkEntity linkEntity = query.LinkEntities.Where<LinkEntity>((Func<LinkEntity, bool>) (l => l.LinkToEntityName.Equals(inputSort.EntityName, StringComparison.Ordinal))).Where<LinkEntity>((Func<LinkEntity, bool>) (l => l.Columns.AttributeExpressions.Any<XrmAttributeExpression>((Func<XrmAttributeExpression, bool>) (c => c.AttributeName.Equals(inputSort.AttributeName, StringComparison.Ordinal))))).FirstOrDefault<LinkEntity>();
        query.Orders.Clear();
        linkEntity?.Orders?.Clear();
        linkEntity?.Orders?.Add(inputSort.ToOrderExpression());
      }
    }

    public static void AddClientSearch(
      this QueryExpression query,
      string quickFindFetchXml,
      string searchText,
      IEntityMetadataProvider metadataProvider,
      IAcceleratedSalesLogger logger,
      string parsedSearchedDate = "")
    {
      logger.AddCustomProperty("QueryExpressionPipelineExtensions.AddClientSearch.Start", (object) "Success");
      Stopwatch stopwatch = Stopwatch.StartNew();
      string entityName = query.EntityName;
      Dictionary<string, AttributeMetadata> dictionary = ((IEnumerable<AttributeMetadata>) metadataProvider.GetAttributes(entityName)).ToDictionary<AttributeMetadata, string, AttributeMetadata>((Func<AttributeMetadata, string>) (a => a.LogicalName), (Func<AttributeMetadata, AttributeMetadata>) (a => a));
      XmlDocument xmlDocument = new XmlDocument();
      xmlDocument.LoadXml(quickFindFetchXml);
      QueryExpressionPipelineExtensions.RemoveOldSearchFilters(query, logger);
      FilterExpression filterExpression = query.Criteria.AddFilter(LogicalOperator.Or);
      filterExpression.IsQuickFindFilter = true;
      foreach (XmlNode selectNode in xmlDocument.SelectNodes("//filter[@isquickfindfields='1' or @isquickfindfields='true']"))
      {
        foreach (XmlNode childNode in selectNode.ChildNodes)
        {
          string innerText = childNode.Attributes["attribute"].InnerText;
          AttributeMetadata attributeMetadata1;
          if (!dictionary.TryGetValue(innerText, out attributeMetadata1))
            logger.AddCustomProperty("QueryExpressionPipelineExtensions.AddClientSearch.Skip. Attribute: " + innerText, (object) "No attribute metadata");
          else if (!QueryExpressionPipelineExtensions.SupportsNameAttribute(attributeMetadata1))
          {
            switch (attributeMetadata1.AttributeType.Value)
            {
              case AttributeTypeCode.DateTime:
                DateTime result1;
                if (DateTime.TryParse(parsedSearchedDate, out result1))
                {
                  filterExpression.AddCondition(innerText, ConditionOperator.On, (object) result1);
                  continue;
                }
                continue;
              case AttributeTypeCode.Decimal:
                Decimal result2;
                if (Decimal.TryParse(searchText, out result2))
                {
                  filterExpression.AddCondition(innerText, ConditionOperator.Equal, (object) result2);
                  continue;
                }
                continue;
              case AttributeTypeCode.Double:
                double result3;
                if (double.TryParse(searchText, out result3))
                {
                  filterExpression.AddCondition(innerText, ConditionOperator.Equal, (object) result3);
                  continue;
                }
                continue;
              case AttributeTypeCode.Integer:
                int result4;
                if (int.TryParse(searchText, out result4))
                {
                  filterExpression.AddCondition(innerText, ConditionOperator.Equal, (object) result4);
                  continue;
                }
                continue;
              case AttributeTypeCode.Money:
                Decimal result5;
                if (Decimal.TryParse(searchText, out result5))
                {
                  filterExpression.AddCondition(innerText, ConditionOperator.Equal, (object) result5);
                  continue;
                }
                continue;
              case AttributeTypeCode.Virtual:
                if ((AttributeTypeDisplayName) attributeMetadata1.AttributeTypeName.Value == AttributeTypeDisplayName.MultiSelectPicklistType)
                {
                  string[] valuesBasedOnText = QueryExpressionPipelineExtensions.GetOptionSetValuesBasedOnText(attributeMetadata1, searchText);
                  if (valuesBasedOnText != null && valuesBasedOnText.Length != 0)
                  {
                    filterExpression.AddCondition(innerText, ConditionOperator.ContainValues, (object[]) valuesBasedOnText);
                    continue;
                  }
                  continue;
                }
                logger.AddCustomProperty("IsVirtualNonMultiSelectPicklistType", (object) true);
                logger.LogError("Virtual NonMultiSelectPicklistType field encountered in quick find " + attributeMetadata1.LogicalName, callerName: nameof (AddClientSearch), callerFile: "C:\\__w\\1\\s\\solutions\\AcceleratedSales\\AcceleratedSalesSandboxPlugins\\Core\\Services\\DataAggregation\\QueryExpressionPipelineExtensions.cs");
                filterExpression.AddCondition(innerText, ConditionOperator.Like, (object) (QueryExpressionPipelineExtensions.ConvertUserFindToLike(searchText) ?? ""));
                continue;
              case AttributeTypeCode.BigInt:
                long result6;
                if (long.TryParse(searchText, out result6))
                {
                  filterExpression.AddCondition(innerText, ConditionOperator.Equal, (object) result6);
                  continue;
                }
                continue;
              default:
                filterExpression.AddCondition(innerText, ConditionOperator.Like, (object) (QueryExpressionPipelineExtensions.ConvertUserFindToLike(searchText) ?? ""));
                continue;
            }
          }
          else
          {
            string str = innerText + "name";
            AttributeMetadata attributeMetadata2;
            if (dictionary.TryGetValue(str, out attributeMetadata2) && !string.IsNullOrEmpty(attributeMetadata2.AttributeOf) && attributeMetadata2.AttributeOf.Equals(innerText, StringComparison.Ordinal))
              filterExpression.AddCondition(str, ConditionOperator.Like, (object) (QueryExpressionPipelineExtensions.ConvertUserFindToLike(searchText) ?? ""));
            else
              logger.AddCustomProperty("QueryExpressionPipelineExtensions.AddClientSearch.Skip. Attribute: " + innerText, (object) "No name attribute");
          }
        }
      }
      stopwatch.Stop();
      logger.AddCustomProperty("QueryExpressionPipelineExtensions.AddClientSearch.Duration", (object) stopwatch.ElapsedMilliseconds);
      logger.AddCustomProperty("QueryExpressionPipelineExtensions.AddClientSearch.End", (object) "Success");
    }

    private static void RemoveOldSearchFilters(
      QueryExpression query,
      IAcceleratedSalesLogger logger)
    {
      if (query.Criteria?.Filters == null)
        return;
      List<FilterExpression> list = query.Criteria.Filters.Where<FilterExpression>((Func<FilterExpression, bool>) (f => f.IsQuickFindFilter)).ToList<FilterExpression>();
      logger.AddCustomProperty(list.Count > 0 ? "QuickSearchFiltersAlreadyExists" : "QuickSearchFiltersNotFound", (object) (list != null ? new int?(list.Count<FilterExpression>()) : new int?()));
      foreach (FilterExpression filterExpression in list)
        query.Criteria.Filters.Remove(filterExpression);
    }

    private static string ConvertUserFindToLike(string searchValue)
    {
      StringBuilder stringBuilder = new StringBuilder();
      for (int index = 0; index < searchValue.Length; ++index)
      {
        switch (searchValue[index])
        {
          case '%':
            stringBuilder.Append("[%]");
            break;
          case '*':
            stringBuilder.Append("%");
            break;
          case '[':
            stringBuilder.Append("[[]");
            break;
          case '_':
            stringBuilder.Append("[_]");
            break;
          default:
            stringBuilder.Append(searchValue[index]);
            break;
        }
      }
      if (searchValue.Length == 0 || !searchValue.EndsWith("*"))
        stringBuilder.Append("%");
      return stringBuilder.ToString();
    }

    private static bool SupportsNameAttribute(AttributeMetadata attributeMetadata)
    {
      switch (attributeMetadata.AttributeType.Value)
      {
        case AttributeTypeCode.Boolean:
        case AttributeTypeCode.Customer:
        case AttributeTypeCode.Lookup:
        case AttributeTypeCode.Owner:
        case AttributeTypeCode.Picklist:
        case AttributeTypeCode.State:
        case AttributeTypeCode.Status:
          return true;
        default:
          return false;
      }
    }

    private static string[] GetOptionSetValuesBasedOnText(
      AttributeMetadata attributeMetadata,
      string searchText)
    {
      string[] valuesBasedOnText;
      if (!(attributeMetadata is EnumAttributeMetadata attributeMetadata1))
      {
        valuesBasedOnText = (string[]) null;
      }
      else
      {
        OptionSetMetadata optionSet = attributeMetadata1.OptionSet;
        if (optionSet == null)
        {
          valuesBasedOnText = (string[]) null;
        }
        else
        {
          OptionMetadataCollection options = optionSet.Options;
          if (options == null)
          {
            valuesBasedOnText = (string[]) null;
          }
          else
          {
            IEnumerable<OptionMetadata> source1 = options.Where<OptionMetadata>((Func<OptionMetadata, bool>) (option =>
            {
              Label label2 = option.Label;
              bool? nullable;
              if (label2 == null)
              {
                nullable = new bool?();
              }
              else
              {
                LocalizedLabelCollection localizedLabels = label2.LocalizedLabels;
                nullable = localizedLabels != null ? new bool?(localizedLabels.Any<LocalizedLabel>((Func<LocalizedLabel, bool>) (label =>
                {
                  string label3 = label.Label;
                  return label3 == null || label3.IndexOf(searchText, StringComparison.OrdinalIgnoreCase) != -1;
                }))) : new bool?();
              }
              return nullable.GetValueOrDefault();
            }));
            if (source1 == null)
            {
              valuesBasedOnText = (string[]) null;
            }
            else
            {
              IEnumerable<string> source2 = source1.Select<OptionMetadata, string>((Func<OptionMetadata, string>) (option => option.Value.Value.ToString()));
              valuesBasedOnText = source2 != null ? source2.ToArray<string>() : (string[]) null;
            }
          }
        }
      }
      return valuesBasedOnText;
    }

    private static string GetLinkEntityKey(this LinkEntity link)
    {
      return link.LinkToEntityName + "." + link.LinkFromAttributeName;
    }

    private static string FetchAliasForLinkEntityIfSameEntityLinkExists(
      DataCollection<LinkEntity> linkEntities,
      string baseEntityName)
    {
      foreach (LinkEntity linkEntity in (Collection<LinkEntity>) linkEntities)
      {
        if (string.Equals(linkEntity.LinkToEntityName, baseEntityName, StringComparison.OrdinalIgnoreCase) && !string.IsNullOrEmpty(linkEntity.EntityAlias))
          return linkEntity.EntityAlias;
        if (linkEntity.LinkEntities.Count > 0)
        {
          string str = QueryExpressionPipelineExtensions.FetchAliasForLinkEntityIfSameEntityLinkExists(linkEntity.LinkEntities, baseEntityName);
          if (!string.IsNullOrEmpty(str))
            return str;
        }
      }
      return (string) null;
    }
  }
}
