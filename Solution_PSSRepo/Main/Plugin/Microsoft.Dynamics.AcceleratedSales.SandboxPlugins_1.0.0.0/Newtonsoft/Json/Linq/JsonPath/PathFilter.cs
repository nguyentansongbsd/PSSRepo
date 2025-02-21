// Decompiled with JetBrains decompiler
// Type: Newtonsoft.Json.Linq.JsonPath.PathFilter
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
  internal abstract class PathFilter
  {
    public abstract IEnumerable<JToken> ExecuteFilter(
      JToken root,
      IEnumerable<JToken> current,
      JsonSelectSettings? settings);

    protected static JToken? GetTokenIndex(JToken t, JsonSelectSettings? settings, int index)
    {
      if (t is JArray jarray)
      {
        if (jarray.Count > index)
          return jarray[index];
        if (settings != null && settings.ErrorWhenNoMatch)
          throw new JsonException("Index {0} outside the bounds of JArray.".FormatWith((IFormatProvider) CultureInfo.InvariantCulture, (object) index));
        return (JToken) null;
      }
      if (t is JConstructor jconstructor)
      {
        if (jconstructor.Count > index)
          return jconstructor[(object) index];
        if (settings != null && settings.ErrorWhenNoMatch)
          throw new JsonException("Index {0} outside the bounds of JConstructor.".FormatWith((IFormatProvider) CultureInfo.InvariantCulture, (object) index));
        return (JToken) null;
      }
      if (settings != null && settings.ErrorWhenNoMatch)
        throw new JsonException("Index {0} not valid on {1}.".FormatWith((IFormatProvider) CultureInfo.InvariantCulture, (object) index, (object) t.GetType().Name));
      return (JToken) null;
    }

    protected static JToken? GetNextScanValue(
      JToken originalParent,
      JToken? container,
      JToken? value)
    {
      if (container != null && container.HasValues)
      {
        value = container.First;
      }
      else
      {
        while (value != null && value != originalParent && value == value.Parent.Last)
          value = (JToken) value.Parent;
        if (value == null || value == originalParent)
          return (JToken) null;
        value = value.Next;
      }
      return value;
    }
  }
}
