// Decompiled with JetBrains decompiler
// Type: Microsoft.Dynamics.AcceleratedSales.SandboxPlugins.Core.Services.Commands.QuerySortExtensions
// Assembly: Microsoft.Dynamics.AcceleratedSales.SandboxPlugins, Version=1.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35
// MVID: 7BBE7132-7E30-4424-B38D-27667CFBCEFA
// Assembly location: C:\Users\ngoct\Downloads\New folder\Microsoft.Dynamics.AcceleratedSales.SandboxPlugins_1.0.0.0.dll

using Microsoft.Dynamics.AcceleratedSales.SandboxPlugins.Core.Model;
using System.Collections.Generic;

#nullable disable
namespace Microsoft.Dynamics.AcceleratedSales.SandboxPlugins.Core.Services.Commands
{
  public static class QuerySortExtensions
  {
    public static Dictionary<string, QuerySort> MergeQuerySort(
      this Dictionary<string, QuerySort> existingSortItems,
      Dictionary<string, QuerySort> newSortItems)
    {
      Dictionary<string, QuerySort> dictionary = new Dictionary<string, QuerySort>();
      if (existingSortItems == null || newSortItems == null)
        return existingSortItems ?? newSortItems ?? dictionary;
      foreach (KeyValuePair<string, QuerySort> existingSortItem in existingSortItems)
      {
        if (!newSortItems.ContainsKey(existingSortItem.Key))
          dictionary.Add(existingSortItem.Key, existingSortItem.Value);
      }
      foreach (KeyValuePair<string, QuerySort> newSortItem in newSortItems)
        dictionary.Add(newSortItem.Key, newSortItem.Value);
      return dictionary;
    }
  }
}
