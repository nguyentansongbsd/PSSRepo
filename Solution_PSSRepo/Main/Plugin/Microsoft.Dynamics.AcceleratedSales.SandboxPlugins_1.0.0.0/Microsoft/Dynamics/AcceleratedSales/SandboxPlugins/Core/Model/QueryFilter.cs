// Decompiled with JetBrains decompiler
// Type: Microsoft.Dynamics.AcceleratedSales.SandboxPlugins.Core.Model.QueryFilter
// Assembly: Microsoft.Dynamics.AcceleratedSales.SandboxPlugins, Version=1.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35
// MVID: 7BBE7132-7E30-4424-B38D-27667CFBCEFA
// Assembly location: C:\Users\ngoct\Downloads\New folder\Microsoft.Dynamics.AcceleratedSales.SandboxPlugins_1.0.0.0.dll

using Microsoft.Xrm.Sdk.Query;
using System;
using System.Collections.Generic;

#nullable disable
namespace Microsoft.Dynamics.AcceleratedSales.SandboxPlugins.Core.Model
{
  public class QueryFilter
  {
    public string EntityName { get; set; }

    public string AttributeName { get; set; }

    public ConditionOperator Operator { get; set; }

    public List<object> Values { get; set; }

    public ConditionExpression ToConditionExpression(
      string linkAlias = "",
      bool removeEntityNameInFilterCondition = false)
    {
      string entityName = this.EntityName;
      if (!string.IsNullOrWhiteSpace(linkAlias))
        entityName = linkAlias;
      if (removeEntityNameInFilterCondition)
        entityName = (string) null;
      switch (this.Operator)
      {
        case ConditionOperator.In:
        case ConditionOperator.Between:
          object[] array = this.Values.ToArray();
          return new ConditionExpression(entityName, this.AttributeName, this.Operator, array);
        default:
          if (this.Values == null || this.Values.Count <= 0)
            return new ConditionExpression(entityName, this.AttributeName, this.Operator);
          object obj = this.Values[0];
          if (!(this.AttributeName == "statecode") && (!(this.AttributeName == "activitytypecode") || !(this.EntityName == "activitypointer")))
            return new ConditionExpression(entityName, this.AttributeName, this.Operator, obj);
          int int32 = Convert.ToInt32(obj);
          return new ConditionExpression(entityName, this.AttributeName, this.Operator, (object) int32);
      }
    }
  }
}
