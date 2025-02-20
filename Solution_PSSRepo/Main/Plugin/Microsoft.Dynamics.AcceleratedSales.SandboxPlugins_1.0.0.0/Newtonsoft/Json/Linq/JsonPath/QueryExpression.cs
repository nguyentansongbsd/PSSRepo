// Decompiled with JetBrains decompiler
// Type: Newtonsoft.Json.Linq.JsonPath.QueryExpression
// Assembly: Microsoft.Dynamics.AcceleratedSales.SandboxPlugins, Version=1.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35
// MVID: 7BBE7132-7E30-4424-B38D-27667CFBCEFA
// Assembly location: C:\Users\ngoct\Downloads\New folder\Microsoft.Dynamics.AcceleratedSales.SandboxPlugins_1.0.0.0.dll

#nullable enable
namespace Newtonsoft.Json.Linq.JsonPath
{
  internal abstract class QueryExpression
  {
    internal QueryOperator Operator;

    public QueryExpression(QueryOperator @operator) => this.Operator = @operator;

    public bool IsMatch(JToken root, JToken t) => this.IsMatch(root, t, (JsonSelectSettings) null);

    public abstract bool IsMatch(JToken root, JToken t, JsonSelectSettings? settings);
  }
}
