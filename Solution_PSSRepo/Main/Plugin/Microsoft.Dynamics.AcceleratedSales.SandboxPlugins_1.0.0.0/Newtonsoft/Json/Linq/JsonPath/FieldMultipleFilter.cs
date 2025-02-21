// Decompiled with JetBrains decompiler
// Type: Newtonsoft.Json.Linq.JsonPath.FieldMultipleFilter
// Assembly: Microsoft.Dynamics.AcceleratedSales.SandboxPlugins, Version=1.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35
// MVID: 7BBE7132-7E30-4424-B38D-27667CFBCEFA
// Assembly location: C:\Users\ngoct\Downloads\New folder\Microsoft.Dynamics.AcceleratedSales.SandboxPlugins_1.0.0.0.dll

using Newtonsoft.Json.Utilities;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;

#nullable enable
namespace Newtonsoft.Json.Linq.JsonPath
{
  internal class FieldMultipleFilter : PathFilter
  {
    internal List<string> Names;

    public FieldMultipleFilter(List<string> names) => this.Names = names;

    public override IEnumerable<JToken> ExecuteFilter(
      JToken root,
      IEnumerable<JToken> current,
      JsonSelectSettings? settings)
    {
      foreach (JToken jtoken1 in current)
      {
        if (jtoken1 is JObject o)
        {
          foreach (string name in this.Names)
          {
            JToken jtoken2 = o[name];
            if (jtoken2 != null)
              yield return jtoken2;
            JsonSelectSettings jsonSelectSettings = settings;
            if ((jsonSelectSettings != null ? (jsonSelectSettings.ErrorWhenNoMatch ? 1 : 0) : 0) != 0)
              throw new JsonException("Property '{0}' does not exist on JObject.".FormatWith((IFormatProvider) CultureInfo.InvariantCulture, (object) name));
          }
        }
        else
        {
          JsonSelectSettings jsonSelectSettings = settings;
          if ((jsonSelectSettings != null ? (jsonSelectSettings.ErrorWhenNoMatch ? 1 : 0) : 0) != 0)
            throw new JsonException("Properties {0} not valid on {1}.".FormatWith((IFormatProvider) CultureInfo.InvariantCulture, (object) string.Join(", ", this.Names.Select<string, string>((Func<string, string>) (n => "'" + n + "'"))), (object) jtoken1.GetType().Name));
        }
        o = (JObject) null;
      }
    }
  }
}
