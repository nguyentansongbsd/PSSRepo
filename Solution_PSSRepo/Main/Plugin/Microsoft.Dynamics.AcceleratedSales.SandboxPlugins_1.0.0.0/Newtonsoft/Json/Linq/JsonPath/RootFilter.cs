// Decompiled with JetBrains decompiler
// Type: Newtonsoft.Json.Linq.JsonPath.RootFilter
// Assembly: Microsoft.Dynamics.AcceleratedSales.SandboxPlugins, Version=1.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35
// MVID: 7BBE7132-7E30-4424-B38D-27667CFBCEFA
// Assembly location: C:\Users\ngoct\Downloads\New folder\Microsoft.Dynamics.AcceleratedSales.SandboxPlugins_1.0.0.0.dll

using System.Collections.Generic;

#nullable enable
namespace Newtonsoft.Json.Linq.JsonPath
{
  internal class RootFilter : PathFilter
  {
    public static readonly RootFilter Instance = new RootFilter();

    private RootFilter()
    {
    }

    public override IEnumerable<JToken> ExecuteFilter(
      JToken root,
      IEnumerable<JToken> current,
      JsonSelectSettings? settings)
    {
      return (IEnumerable<JToken>) new JToken[1]{ root };
    }
  }
}
