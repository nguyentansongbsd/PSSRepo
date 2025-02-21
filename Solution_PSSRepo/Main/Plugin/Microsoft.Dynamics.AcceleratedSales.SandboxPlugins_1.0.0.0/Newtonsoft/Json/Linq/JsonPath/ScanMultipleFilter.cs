// Decompiled with JetBrains decompiler
// Type: Newtonsoft.Json.Linq.JsonPath.ScanMultipleFilter
// Assembly: Microsoft.Dynamics.AcceleratedSales.SandboxPlugins, Version=1.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35
// MVID: 7BBE7132-7E30-4424-B38D-27667CFBCEFA
// Assembly location: C:\Users\ngoct\Downloads\New folder\Microsoft.Dynamics.AcceleratedSales.SandboxPlugins_1.0.0.0.dll

using System.Collections.Generic;

#nullable enable
namespace Newtonsoft.Json.Linq.JsonPath
{
  internal class ScanMultipleFilter : PathFilter
  {
    private List<string> _names;

    public ScanMultipleFilter(List<string> names) => this._names = names;

    public override IEnumerable<JToken> ExecuteFilter(
      JToken root,
      IEnumerable<JToken> current,
      JsonSelectSettings? settings)
    {
      foreach (JToken c in current)
      {
        JToken value = c;
        while (true)
        {
          value = PathFilter.GetNextScanValue(c, (JToken) (value as JContainer), value);
          if (value != null)
          {
            if (value is JProperty property)
            {
              foreach (string name in this._names)
              {
                if (property.Name == name)
                  yield return property.Value;
              }
            }
            property = (JProperty) null;
          }
          else
            break;
        }
        value = (JToken) null;
      }
    }
  }
}
