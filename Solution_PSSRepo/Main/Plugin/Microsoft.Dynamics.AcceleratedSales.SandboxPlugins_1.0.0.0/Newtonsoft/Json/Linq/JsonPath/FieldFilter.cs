// Decompiled with JetBrains decompiler
// Type: Newtonsoft.Json.Linq.JsonPath.FieldFilter
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
  internal class FieldFilter : PathFilter
  {
    internal string? Name;

    public FieldFilter(string? name) => this.Name = name;

    public override IEnumerable<JToken> ExecuteFilter(
      JToken root,
      IEnumerable<JToken> current,
      JsonSelectSettings? settings)
    {
      foreach (JToken jtoken1 in current)
      {
        if (jtoken1 is JObject jobject)
        {
          if (this.Name != null)
          {
            JToken jtoken2 = jobject[this.Name];
            if (jtoken2 != null)
            {
              yield return jtoken2;
            }
            else
            {
              JsonSelectSettings jsonSelectSettings = settings;
              if ((jsonSelectSettings != null ? (jsonSelectSettings.ErrorWhenNoMatch ? 1 : 0) : 0) != 0)
                throw new JsonException("Property '{0}' does not exist on JObject.".FormatWith((IFormatProvider) CultureInfo.InvariantCulture, (object) this.Name));
            }
          }
          else
          {
            foreach (KeyValuePair<string, JToken> keyValuePair in jobject)
              yield return keyValuePair.Value;
          }
        }
        else
        {
          JsonSelectSettings jsonSelectSettings = settings;
          if ((jsonSelectSettings != null ? (jsonSelectSettings.ErrorWhenNoMatch ? 1 : 0) : 0) != 0)
            throw new JsonException("Property '{0}' not valid on {1}.".FormatWith((IFormatProvider) CultureInfo.InvariantCulture, (object) (this.Name ?? "*"), (object) jtoken1.GetType().Name));
        }
      }
    }
  }
}
