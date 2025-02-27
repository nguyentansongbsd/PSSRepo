// Decompiled with JetBrains decompiler
// Type: Newtonsoft.Json.Linq.JsonPath.ArraySliceFilter
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
  internal class ArraySliceFilter : PathFilter
  {
    public int? Start { get; set; }

    public int? End { get; set; }

    public int? Step { get; set; }

    public override IEnumerable<JToken> ExecuteFilter(
      JToken root,
      IEnumerable<JToken> current,
      JsonSelectSettings? settings)
    {
      int? nullable = this.Step;
      int num1 = 0;
      if (nullable.GetValueOrDefault() == num1 & nullable.HasValue)
        throw new JsonException("Step cannot be zero.");
      foreach (JToken jtoken in current)
      {
        if (jtoken is JArray a)
        {
          nullable = this.Step;
          int stepCount = nullable ?? 1;
          nullable = this.Start;
          int val1 = nullable ?? (stepCount > 0 ? 0 : a.Count - 1);
          nullable = this.End;
          int stopIndex = nullable ?? (stepCount > 0 ? a.Count : -1);
          nullable = this.Start;
          int num2 = 0;
          if (nullable.GetValueOrDefault() < num2 & nullable.HasValue)
            val1 = a.Count + val1;
          nullable = this.End;
          int num3 = 0;
          if (nullable.GetValueOrDefault() < num3 & nullable.HasValue)
            stopIndex = a.Count + stopIndex;
          int index = Math.Min(Math.Max(val1, stepCount > 0 ? 0 : int.MinValue), stepCount > 0 ? a.Count : a.Count - 1);
          stopIndex = Math.Max(stopIndex, -1);
          stopIndex = Math.Min(stopIndex, a.Count);
          bool positiveStep = stepCount > 0;
          if (this.IsValid(index, stopIndex, positiveStep))
          {
            for (int i = index; this.IsValid(i, stopIndex, positiveStep); i += stepCount)
              yield return a[i];
          }
          else
          {
            JsonSelectSettings jsonSelectSettings = settings;
            if ((jsonSelectSettings != null ? (jsonSelectSettings.ErrorWhenNoMatch ? 1 : 0) : 0) != 0)
            {
              CultureInfo invariantCulture = CultureInfo.InvariantCulture;
              nullable = this.Start;
              string str1;
              if (!nullable.HasValue)
              {
                str1 = "*";
              }
              else
              {
                nullable = this.Start;
                str1 = nullable.GetValueOrDefault().ToString((IFormatProvider) CultureInfo.InvariantCulture);
              }
              nullable = this.End;
              string str2;
              if (!nullable.HasValue)
              {
                str2 = "*";
              }
              else
              {
                nullable = this.End;
                str2 = nullable.GetValueOrDefault().ToString((IFormatProvider) CultureInfo.InvariantCulture);
              }
              throw new JsonException("Array slice of {0} to {1} returned no results.".FormatWith((IFormatProvider) invariantCulture, (object) str1, (object) str2));
            }
          }
        }
        else
        {
          JsonSelectSettings jsonSelectSettings = settings;
          if ((jsonSelectSettings != null ? (jsonSelectSettings.ErrorWhenNoMatch ? 1 : 0) : 0) != 0)
            throw new JsonException("Array slice is not valid on {0}.".FormatWith((IFormatProvider) CultureInfo.InvariantCulture, (object) jtoken.GetType().Name));
        }
        a = (JArray) null;
      }
    }

    private bool IsValid(int index, int stopIndex, bool positiveStep)
    {
      return positiveStep ? index < stopIndex : index > stopIndex;
    }
  }
}
