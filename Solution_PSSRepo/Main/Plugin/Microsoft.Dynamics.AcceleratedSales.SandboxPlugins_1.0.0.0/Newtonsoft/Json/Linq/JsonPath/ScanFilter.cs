// Decompiled with JetBrains decompiler
// Type: Newtonsoft.Json.Linq.JsonPath.ScanFilter
// Assembly: Microsoft.Dynamics.AcceleratedSales.SandboxPlugins, Version=1.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35
// MVID: 7BBE7132-7E30-4424-B38D-27667CFBCEFA
// Assembly location: C:\Users\ngoct\Downloads\New folder\Microsoft.Dynamics.AcceleratedSales.SandboxPlugins_1.0.0.0.dll

using System.Collections.Generic;

#nullable enable
namespace Newtonsoft.Json.Linq.JsonPath
{
  internal class ScanFilter : PathFilter
  {
    internal string? Name;

    public ScanFilter(string? name) => this.Name = name;

    public override IEnumerable<JToken> ExecuteFilter(
      JToken root,
      IEnumerable<JToken> current,
      JsonSelectSettings? settings)
    {
      foreach (JToken c in current)
      {
        if (this.Name == null)
          yield return c;
        JToken value = c;
        while (true)
        {
          do
          {
            value = PathFilter.GetNextScanValue(c, (JToken) (value as JContainer), value);
            if (value != null)
            {
              if (value is JProperty jproperty)
              {
                if (jproperty.Name == this.Name)
                  yield return jproperty.Value;
              }
            }
            else
              goto label_10;
          }
          while (this.Name != null);
          yield return value;
        }
label_10:
        value = (JToken) null;
      }
    }
  }
}
