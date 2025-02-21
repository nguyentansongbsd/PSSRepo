// Decompiled with JetBrains decompiler
// Type: Newtonsoft.Json.Linq.JsonPath.ArrayIndexFilter
// Assembly: Microsoft.Dynamics.AcceleratedSales.SandboxPlugins, Version=1.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35
// MVID: 7BBE7132-7E30-4424-B38D-27667CFBCEFA
// Assembly location: C:\Users\ngoct\Downloads\New folder\Microsoft.Dynamics.AcceleratedSales.SandboxPlugins_1.0.0.0.dll

using Newtonsoft.Json.Utilities;
using System;
using System.Collections.Generic;
using System.Globalization;

#nullable enable
namespace Newtonsoft.Json.Linq.JsonPath
{
  internal class ArrayIndexFilter : PathFilter
  {
    public int? Index { get; set; }

    public override IEnumerable<JToken> ExecuteFilter(
      JToken root,
      IEnumerable<JToken> current,
      JsonSelectSettings? settings)
    {
      foreach (JToken jtoken1 in current)
      {
        int? index = this.Index;
        if (index.HasValue)
        {
          JToken t = jtoken1;
          JsonSelectSettings settings1 = settings;
          index = this.Index;
          int valueOrDefault = index.GetValueOrDefault();
          JToken tokenIndex = PathFilter.GetTokenIndex(t, settings1, valueOrDefault);
          if (tokenIndex != null)
            yield return tokenIndex;
        }
        else
        {
          switch (jtoken1)
          {
            case JArray _:
            case JConstructor _:
              foreach (JToken jtoken2 in (IEnumerable<JToken>) jtoken1)
                yield return jtoken2;
              continue;
            default:
              JsonSelectSettings jsonSelectSettings = settings;
              if ((jsonSelectSettings != null ? (jsonSelectSettings.ErrorWhenNoMatch ? 1 : 0) : 0) != 0)
                throw new JsonException("Index * not valid on {0}.".FormatWith((IFormatProvider) CultureInfo.InvariantCulture, (object) jtoken1.GetType().Name));
              continue;
          }
        }
      }
    }
  }
}
