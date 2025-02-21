// Decompiled with JetBrains decompiler
// Type: Microsoft.Dynamics.AcceleratedSales.SandboxPlugins.Core.Model.QuerySort
// Assembly: Microsoft.Dynamics.AcceleratedSales.SandboxPlugins, Version=1.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35
// MVID: 7BBE7132-7E30-4424-B38D-27667CFBCEFA
// Assembly location: C:\Users\ngoct\Downloads\New folder\Microsoft.Dynamics.AcceleratedSales.SandboxPlugins_1.0.0.0.dll

using Microsoft.Xrm.Sdk.Query;

#nullable disable
namespace Microsoft.Dynamics.AcceleratedSales.SandboxPlugins.Core.Model
{
  public class QuerySort
  {
    public string EntityName { get; set; }

    public string AttributeName { get; set; }

    public OrderType OrderType { get; set; }

    public static QuerySort FromOrderExpression(OrderExpression order)
    {
      return new QuerySort()
      {
        EntityName = order.EntityName,
        AttributeName = order.AttributeName,
        OrderType = order.OrderType
      };
    }

    public OrderExpression ToOrderExpression()
    {
      return new OrderExpression(this.AttributeName, this.OrderType);
    }
  }
}
