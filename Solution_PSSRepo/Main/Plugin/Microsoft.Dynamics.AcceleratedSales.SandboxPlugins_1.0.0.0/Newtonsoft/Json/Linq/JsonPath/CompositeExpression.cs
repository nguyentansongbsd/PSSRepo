// Decompiled with JetBrains decompiler
// Type: Newtonsoft.Json.Linq.JsonPath.CompositeExpression
// Assembly: Microsoft.Dynamics.AcceleratedSales.SandboxPlugins, Version=1.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35
// MVID: 7BBE7132-7E30-4424-B38D-27667CFBCEFA
// Assembly location: C:\Users\ngoct\Downloads\New folder\Microsoft.Dynamics.AcceleratedSales.SandboxPlugins_1.0.0.0.dll

using System;
using System.Collections.Generic;

#nullable enable
namespace Newtonsoft.Json.Linq.JsonPath
{
  internal class CompositeExpression : QueryExpression
  {
    public List<QueryExpression> Expressions { get; set; }

    public CompositeExpression(QueryOperator @operator)
      : base(@operator)
    {
      this.Expressions = new List<QueryExpression>();
    }

    public override bool IsMatch(JToken root, JToken t, JsonSelectSettings? settings)
    {
      switch (this.Operator)
      {
        case QueryOperator.And:
          foreach (QueryExpression expression in this.Expressions)
          {
            if (!expression.IsMatch(root, t, settings))
              return false;
          }
          return true;
        case QueryOperator.Or:
          foreach (QueryExpression expression in this.Expressions)
          {
            if (expression.IsMatch(root, t, settings))
              return true;
          }
          return false;
        default:
          throw new ArgumentOutOfRangeException();
      }
    }
  }
}
