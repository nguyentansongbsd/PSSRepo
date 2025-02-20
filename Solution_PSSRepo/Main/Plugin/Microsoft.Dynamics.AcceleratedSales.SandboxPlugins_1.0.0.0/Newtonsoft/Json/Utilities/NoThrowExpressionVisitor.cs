// Decompiled with JetBrains decompiler
// Type: Newtonsoft.Json.Utilities.NoThrowExpressionVisitor
// Assembly: Microsoft.Dynamics.AcceleratedSales.SandboxPlugins, Version=1.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35
// MVID: 7BBE7132-7E30-4424-B38D-27667CFBCEFA
// Assembly location: C:\Users\ngoct\Downloads\New folder\Microsoft.Dynamics.AcceleratedSales.SandboxPlugins_1.0.0.0.dll

using System.Linq.Expressions;

#nullable enable
namespace Newtonsoft.Json.Utilities
{
  internal class NoThrowExpressionVisitor : ExpressionVisitor
  {
    internal static readonly object ErrorResult = new object();

    protected override Expression VisitConditional(ConditionalExpression node)
    {
      return node.IfFalse.NodeType == ExpressionType.Throw ? (Expression) Expression.Condition(node.Test, node.IfTrue, (Expression) Expression.Constant(NoThrowExpressionVisitor.ErrorResult)) : base.VisitConditional(node);
    }
  }
}
