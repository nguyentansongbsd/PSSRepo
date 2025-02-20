// Decompiled with JetBrains decompiler
// Type: Microsoft.Dynamics.AcceleratedSales.SandboxPlugins.Core.Services.Commands.QueryFilterExtensions
// Assembly: Microsoft.Dynamics.AcceleratedSales.SandboxPlugins, Version=1.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35
// MVID: 7BBE7132-7E30-4424-B38D-27667CFBCEFA
// Assembly location: C:\Users\ngoct\Downloads\New folder\Microsoft.Dynamics.AcceleratedSales.SandboxPlugins_1.0.0.0.dll

using Microsoft.Dynamics.AcceleratedSales.SandboxPlugins.Core.Model;
using System;
using System.Collections.Generic;

#nullable disable
namespace Microsoft.Dynamics.AcceleratedSales.SandboxPlugins.Core.Services.Commands
{
  public static class QueryFilterExtensions
  {
    public static Dictionary<string, List<QueryFilter>> MergeQueryFilters(
      this Dictionary<string, List<QueryFilter>> existingQueryFilters,
      Dictionary<string, List<QueryFilter>> queryFilters)
    {
      Dictionary<string, List<QueryFilter>> dictionary = new Dictionary<string, List<QueryFilter>>();
      if (existingQueryFilters == null || queryFilters == null)
        return existingQueryFilters ?? queryFilters ?? dictionary;
      foreach (KeyValuePair<string, List<QueryFilter>> existingQueryFilter in existingQueryFilters)
      {
        if (queryFilters.ContainsKey(existingQueryFilter.Key))
          dictionary.Add(existingQueryFilter.Key, QueryFilterExtensions.GetCombinedFilterList(existingQueryFilter.Value, queryFilters[existingQueryFilter.Key]));
        else
          dictionary.Add(existingQueryFilter.Key, existingQueryFilter.Value);
      }
      foreach (KeyValuePair<string, List<QueryFilter>> queryFilter in queryFilters)
      {
        if (!existingQueryFilters.ContainsKey(queryFilter.Key))
          dictionary.Add(queryFilter.Key, queryFilter.Value);
      }
      return dictionary;
    }

    public static Dictionary<string, List<QueryFilter>> UpdateQueryFilter(
      this Dictionary<string, List<QueryFilter>> queryFilters,
      string entityName,
      List<QueryFilter> newQueryFilters)
    {
      if (newQueryFilters == null || newQueryFilters.Count == 0)
        return queryFilters;
      if (!queryFilters.ContainsKey(entityName))
        queryFilters.Add(entityName, newQueryFilters);
      else
        queryFilters[entityName] = QueryFilterExtensions.GetCombinedFilterList(queryFilters[entityName], newQueryFilters);
      return queryFilters;
    }

    private static List<QueryFilter> GetCombinedFilterList(
      List<QueryFilter> existingFilters,
      List<QueryFilter> filtersToAdd)
    {
      // ISSUE: explicit non-virtual call
      if (filtersToAdd != null && __nonvirtual (filtersToAdd.Count) == 0)
        return existingFilters;
      // ISSUE: explicit non-virtual call
      if (existingFilters != null && __nonvirtual (existingFilters.Count) == 0)
        return filtersToAdd;
      List<QueryFilter> combinedFilterList = new List<QueryFilter>();
      foreach (QueryFilter existingFilter in existingFilters)
      {
        if (!QueryFilterExtensions.IsFilterPresent(existingFilter, filtersToAdd))
          combinedFilterList.Add(existingFilter);
      }
      combinedFilterList.AddRange((IEnumerable<QueryFilter>) filtersToAdd);
      return combinedFilterList;
    }

    private static bool IsFilterPresent(QueryFilter filter, List<QueryFilter> filterList)
    {
      return filterList.Exists((Predicate<QueryFilter>) (f => f.AttributeName == filter.AttributeName));
    }
  }
}
