// Decompiled with JetBrains decompiler
// Type: Newtonsoft.Json.Linq.JsonPath.QueryScanFilter
// Assembly: Microsoft.Dynamics.AcceleratedSales.SandboxPlugins, Version=1.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35
// MVID: 7BBE7132-7E30-4424-B38D-27667CFBCEFA
// Assembly location: C:\Users\ngoct\Downloads\New folder\Microsoft.Dynamics.AcceleratedSales.SandboxPlugins_1.0.0.0.dll

using System.Collections.Generic;

#nullable enable
namespace Newtonsoft.Json.Linq.JsonPath
{
  internal class QueryScanFilter : PathFilter
  {
    internal QueryExpression Expression;

    public QueryScanFilter(QueryExpression expression) => this.Expression = expression;

    public override IEnumerable<JToken> ExecuteFilter(
      JToken root,
      IEnumerable<JToken> current,
      JsonSelectSettings? settings)
    {
      foreach (JToken t1 in current)
      {
        if (t1 is JContainer jcontainer)
        {
          foreach (JToken t2 in jcontainer.DescendantsAndSelf())
          {
            if (this.Expression.IsMatch(root, t2, settings))
              yield return t2;
          }
        }
        else if (this.Expression.IsMatch(root, t1, settings))
          yield return t1;
      }
    }
  }
}
