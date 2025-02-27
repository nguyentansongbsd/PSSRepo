// Decompiled with JetBrains decompiler
// Type: Microsoft.Dynamics.AcceleratedSales.SandboxPlugins.Core.Services.QueryExpressionCommandExtensions
// Assembly: Microsoft.Dynamics.AcceleratedSales.SandboxPlugins, Version=1.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35
// MVID: 7BBE7132-7E30-4424-B38D-27667CFBCEFA
// Assembly location: C:\Users\ngoct\Downloads\New folder\Microsoft.Dynamics.AcceleratedSales.SandboxPlugins_1.0.0.0.dll

using Microsoft.Dynamics.AcceleratedSales.SandboxPlugins.Core.Model;
using Microsoft.Dynamics.AcceleratedSales.SandboxPlugins.Core.Model.Filter;
using Microsoft.Dynamics.AcceleratedSales.SandboxPlugins.Core.Model.Layout;
using Microsoft.Dynamics.AcceleratedSales.SandboxPlugins.Core.Model.Sort;
using Microsoft.Xrm.Sdk.Metadata;
using Microsoft.Xrm.Sdk.Query;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;

#nullable disable
namespace Microsoft.Dynamics.AcceleratedSales.SandboxPlugins.Core.Services
{
  public static class QueryExpressionCommandExtensions
  {
    public const string ActivitiesDueFilterId = "a00000a0-a0a0-000a-a000-000aa0aaa043";
    public const string ActivitiesDueFilterName = "msdyn_d365salesactivityduefilter";
    private const string GridFilterGroupId = "a20000a0-a0a0-000a-a000-000aa0aaa000";

    public static void ToCommandGroup(
      this QueryExpression query,
      IAcceleratedSalesLogger logger,
      IEntityMetadataProvider metadataProvider,
      IDataStore dataStore,
      int localeId,
      out FilterGroup filterGroup,
      out List<SortItem> sortItems,
      out List<QuerySort> appliedSortItems)
    {
      Stopwatch stopwatch = Stopwatch.StartNew();
      filterGroup = new FilterGroup()
      {
        Id = Guid.Parse("a20000a0-a0a0-000a-a000-000aa0aaa000"),
        GroupType = FilterGroupType.Default,
        IsCustomName = true,
        IsDefaultSelected = false,
        Name = "Grid filters",
        Position = 3,
        Visibility = true
      };
      sortItems = new List<SortItem>();
      appliedSortItems = new List<QuerySort>();
      string entityName = query.EntityName;
      EntityMetadata entityMetadata1 = metadataProvider.GetEntityMetadata(entityName);
      bool flag = QueryExpressionCommandExtensions.IsActivityEntity(entityMetadata1, entityName);
      Dictionary<string, AttributeMetadata> dictionary1 = ((IEnumerable<AttributeMetadata>) metadataProvider.GetAttributes(query.EntityName)).ToDictionary<AttributeMetadata, string, AttributeMetadata>((Func<AttributeMetadata, string>) (a => a.LogicalName), (Func<AttributeMetadata, AttributeMetadata>) (a => a));
      string str1 = (entityMetadata1 != null ? entityMetadata1.DisplayName.GetLocalizedLabel(localeId) : (string) null) ?? entityMetadata1?.LogicalName ?? string.Empty;
      List<FilterItem> filterItemList = new List<FilterItem>();
      int num = 0;
      Dictionary<string, QuerySort> dictionary2 = query.Orders.ToDictionary<OrderExpression, string, QuerySort>((Func<OrderExpression, string>) (o => o.AttributeName), (Func<OrderExpression, QuerySort>) (o => QuerySort.FromOrderExpression(o)));
      if (dataStore.IsFCSEnabled("SalesService.Workspace", "FocusedViewSingleSortOrder") && query.Orders.Any<OrderExpression>())
      {
        OrderExpression order = query.Orders.First<OrderExpression>();
        dictionary2 = new Dictionary<string, QuerySort>()
        {
          {
            order.AttributeName,
            QuerySort.FromOrderExpression(order)
          }
        };
      }
      if (flag)
      {
        logger.LogWarning("QueryExpressionCommandExtensions.ToCommandGroup.IsActivity", callerName: nameof (ToCommandGroup), callerFile: "C:\\__w\\1\\s\\solutions\\AcceleratedSales\\AcceleratedSalesSandboxPlugins\\Core\\Services\\Commands\\QueryExpressionCommandExtensions.cs");
        FilterItem filterItem = new FilterItem()
        {
          Id = new Guid("a00000a0-a0a0-000a-a000-000aa0aaa043"),
          Name = "msdyn_d365salesactivityduefilter",
          Filtertype = FilterItemType.Simple,
          IsCustomName = new bool?(true),
          Visibility = true,
          Position = num++,
          Metadata = new Dictionary<string, Dictionary<string, FilterProperties>>()
          {
            {
              entityName,
              new Dictionary<string, FilterProperties>()
              {
                {
                  "msdyn_d365salesactivityduefilter",
                  new FilterProperties()
                  {
                    AttributeTypeCode = new AttributeTypeCode?(AttributeTypeCode.DateTime),
                    EntityLocalizedName = str1,
                    ControlType = new ControlType?(ControlType.SingleSelect)
                  }
                }
              }
            }
          }
        };
        filterItemList.Add(filterItem);
      }
      foreach (string column in (Collection<string>) query.ColumnSet.Columns)
      {
        FilterItem filterItem;
        if (QueryExpressionCommandExtensions.TryGetFilterForColumn(entityName, dictionary1[column], localeId, out filterItem))
        {
          LookupAttributeMetadata attributeMetadata = dictionary1[column] as LookupAttributeMetadata;
          filterItem.Name = column;
          filterItem.Position = num++;
          filterItem.Metadata[entityName][column].EntityLocalizedName = str1;
          if (attributeMetadata != null)
            filterItem.Metadata[entityName][column].Targets = attributeMetadata.Targets;
          filterItemList.Add(filterItem);
          sortItems.Add(QueryExpressionCommandExtensions.ConvertFilterToSort(filterItem, entityName, dictionary1[column]));
        }
        QuerySort querySort;
        if (dictionary2.TryGetValue(column, out querySort))
          appliedSortItems.Add(querySort);
      }
      foreach (LinkEntity linkEntity in (Collection<LinkEntity>) query.LinkEntities)
      {
        if (!LinkEntityLayoutExtensions.TryGetDisplayKey(linkEntity.EntityAlias, out string _) && !(linkEntity.LinkToEntityName != "msdyn_predictivescore"))
        {
          string linkToEntityName = linkEntity.LinkToEntityName;
          EntityMetadata entityMetadata2 = metadataProvider.GetEntityMetadata(linkToEntityName);
          string str2 = (entityMetadata2 != null ? entityMetadata2.DisplayName.GetLocalizedLabel(localeId) : (string) null) ?? entityMetadata2?.LogicalName ?? string.Empty;
          Dictionary<string, AttributeMetadata> dictionary3 = ((IEnumerable<AttributeMetadata>) metadataProvider.GetAttributes(linkToEntityName)).ToDictionary<AttributeMetadata, string, AttributeMetadata>((Func<AttributeMetadata, string>) (a => a.LogicalName), (Func<AttributeMetadata, AttributeMetadata>) (a => a));
          foreach (XrmAttributeExpression attributeExpression in (Collection<XrmAttributeExpression>) linkEntity.Columns.AttributeExpressions)
          {
            FilterItem filterItem;
            if (QueryExpressionCommandExtensions.TryGetFilterForColumn(linkToEntityName, dictionary3[attributeExpression.AttributeName], localeId, out filterItem))
            {
              filterItem.Name = attributeExpression.AttributeName;
              filterItem.Position = num++;
              filterItem.Metadata[linkToEntityName][attributeExpression.AttributeName].EntityLocalizedName = str2;
              filterItemList.Add(filterItem);
              sortItems.Add(QueryExpressionCommandExtensions.ConvertFilterToSort(filterItem, linkToEntityName, dictionary3[attributeExpression.AttributeName]));
            }
            QuerySort querySort;
            if (dictionary2.TryGetValue(attributeExpression.AttributeName, out querySort))
              appliedSortItems.Add(querySort);
          }
        }
      }
      filterGroup.Filters = filterItemList;
      stopwatch.Stop();
      logger.LogWarning("QueryExpressionCommandExtensions.ToCommandGroup.End.Duration: " + stopwatch.ElapsedMilliseconds.ToString(), callerName: nameof (ToCommandGroup), callerFile: "C:\\__w\\1\\s\\solutions\\AcceleratedSales\\AcceleratedSalesSandboxPlugins\\Core\\Services\\Commands\\QueryExpressionCommandExtensions.cs");
    }

    public static void AddOrderExpression(this QueryExpression queryExpression, QuerySort querySort)
    {
      foreach (OrderExpression order in (Collection<OrderExpression>) queryExpression.Orders)
      {
        if (QuerySort.FromOrderExpression(order).AttributeName == querySort.AttributeName)
        {
          queryExpression.Orders.Remove(order);
          break;
        }
      }
      queryExpression.Orders.Add(querySort.ToOrderExpression());
    }

    public static void AddLinkedEntityColumns(
      this MetadataQueryParams metadataQueryParams,
      LinkEntity linkEntity)
    {
      if (linkEntity.Columns.AllColumns)
      {
        metadataQueryParams.AllAttributesEntities.Add(linkEntity.LinkToEntityName);
      }
      else
      {
        metadataQueryParams.Entities.Add(linkEntity.LinkToEntityName);
        metadataQueryParams.Attributes.AddRange((IEnumerable<string>) linkEntity.Columns.Columns.ToList<string>());
      }
      foreach (LinkEntity linkEntity1 in (Collection<LinkEntity>) linkEntity.LinkEntities)
        metadataQueryParams.AddLinkedEntityColumns(linkEntity1);
    }

    private static bool TryGetFilterForColumn(
      string entityName,
      AttributeMetadata metadata,
      int localeId,
      out FilterItem filterItem)
    {
      filterItem = new FilterItem()
      {
        Id = metadata.MetadataId.GetValueOrDefault(Guid.NewGuid()),
        Filtertype = FilterItemType.Simple,
        IsCustomName = new bool?(false),
        Visibility = true
      };
      string str = (metadata != null ? metadata.DisplayName.GetLocalizedLabel(localeId) : (string) null) ?? metadata?.LogicalName ?? string.Empty;
      FilterProperties filterProperties = new FilterProperties()
      {
        AttributeTypeCode = metadata.AttributeType,
        AttributeLocalizedName = str,
        ControlType = new ControlType?(metadata.GetFilterControlType())
      };
      filterProperties.Options = metadata.ToFilterMetadataOptions(localeId);
      filterItem.Metadata = new Dictionary<string, Dictionary<string, FilterProperties>>()
      {
        {
          entityName,
          new Dictionary<string, FilterProperties>()
          {
            {
              metadata.LogicalName,
              filterProperties
            }
          }
        }
      };
      return true;
    }

    private static SortItem ConvertFilterToSort(
      FilterItem filterItem,
      string entityName,
      AttributeMetadata metadata)
    {
      SortItem sort = new SortItem()
      {
        Id = filterItem.Id,
        IsCustomName = filterItem.IsCustomName,
        IsDefault = false,
        IsSystemDefined = false,
        Name = filterItem.Name,
        Position = filterItem.Position,
        Visibility = filterItem.Visibility
      };
      FilterProperties filterProperties = filterItem.Metadata[entityName].First<KeyValuePair<string, FilterProperties>>().Value;
      SortProperties sortProperties = new SortProperties()
      {
        AttributeLocalizedName = filterProperties.AttributeLocalizedName,
        AttributeTypeCode = filterProperties.AttributeTypeCode,
        EntityLocalizedName = filterProperties.EntityLocalizedName,
        Options = filterProperties.Options
      };
      sort.Metadata = new Dictionary<string, Dictionary<string, SortProperties>>()
      {
        {
          entityName,
          new Dictionary<string, SortProperties>()
          {
            {
              metadata.LogicalName,
              sortProperties
            }
          }
        }
      };
      return sort;
    }

    private static bool IsActivityEntity(EntityMetadata entityMetadata, string entityName)
    {
      return ((bool?) entityMetadata?.IsActivity).GetValueOrDefault() || entityName == "activitypointer" || entityMetadata?.PrimaryIdAttribute == "activityid";
    }
  }
}
