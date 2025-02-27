// Decompiled with JetBrains decompiler
// Type: Microsoft.Dynamics.AcceleratedSales.SandboxPlugins.Core.Model.QueryFilterHelpers
// Assembly: Microsoft.Dynamics.AcceleratedSales.SandboxPlugins, Version=1.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35
// MVID: 7BBE7132-7E30-4424-B38D-27667CFBCEFA
// Assembly location: C:\Users\ngoct\Downloads\New folder\Microsoft.Dynamics.AcceleratedSales.SandboxPlugins_1.0.0.0.dll

using System.Collections.Generic;

#nullable disable
namespace Microsoft.Dynamics.AcceleratedSales.SandboxPlugins.Core.Model
{
  public class QueryFilterHelpers
  {
    public static string FormInConditionString(List<string> values)
    {
      string str1 = string.Empty;
      if (values == null || values.Count < 1)
        return str1;
      foreach (string str2 in values)
        str1 = str1 + "<value>" + str2 + "</value>";
      return str1;
    }
  }
}
