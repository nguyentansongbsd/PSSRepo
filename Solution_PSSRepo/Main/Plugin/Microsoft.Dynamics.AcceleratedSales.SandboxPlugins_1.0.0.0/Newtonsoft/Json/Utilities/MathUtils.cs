// Decompiled with JetBrains decompiler
// Type: Newtonsoft.Json.Utilities.MathUtils
// Assembly: Microsoft.Dynamics.AcceleratedSales.SandboxPlugins, Version=1.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35
// MVID: 7BBE7132-7E30-4424-B38D-27667CFBCEFA
// Assembly location: C:\Users\ngoct\Downloads\New folder\Microsoft.Dynamics.AcceleratedSales.SandboxPlugins_1.0.0.0.dll

using System;

#nullable disable
namespace Newtonsoft.Json.Utilities
{
  internal static class MathUtils
  {
    public static int IntLength(ulong i)
    {
      if (i < 10000000000UL)
      {
        if (i < 10UL)
          return 1;
        if (i < 100UL)
          return 2;
        if (i < 1000UL)
          return 3;
        if (i < 10000UL)
          return 4;
        if (i < 100000UL)
          return 5;
        if (i < 1000000UL)
          return 6;
        if (i < 10000000UL)
          return 7;
        if (i < 100000000UL)
          return 8;
        return i < 1000000000UL ? 9 : 10;
      }
      if (i < 100000000000UL)
        return 11;
      if (i < 1000000000000UL)
        return 12;
      if (i < 10000000000000UL)
        return 13;
      if (i < 100000000000000UL)
        return 14;
      if (i < 1000000000000000UL)
        return 15;
      if (i < 10000000000000000UL)
        return 16;
      if (i < 100000000000000000UL)
        return 17;
      if (i < 1000000000000000000UL)
        return 18;
      return i < 10000000000000000000UL ? 19 : 20;
    }

    public static char IntToHex(int n) => n <= 9 ? (char) (n + 48) : (char) (n - 10 + 97);

    public static int? Min(int? val1, int? val2)
    {
      if (!val1.HasValue)
        return val2;
      return !val2.HasValue ? val1 : new int?(Math.Min(val1.GetValueOrDefault(), val2.GetValueOrDefault()));
    }

    public static int? Max(int? val1, int? val2)
    {
      if (!val1.HasValue)
        return val2;
      return !val2.HasValue ? val1 : new int?(Math.Max(val1.GetValueOrDefault(), val2.GetValueOrDefault()));
    }

    public static double? Max(double? val1, double? val2)
    {
      if (!val1.HasValue)
        return val2;
      return !val2.HasValue ? val1 : new double?(Math.Max(val1.GetValueOrDefault(), val2.GetValueOrDefault()));
    }

    public static bool ApproxEquals(double d1, double d2)
    {
      if (d1 == d2)
        return true;
      double num1 = (Math.Abs(d1) + Math.Abs(d2) + 10.0) * 2.2204460492503131E-16;
      double num2 = d1 - d2;
      return -num1 < num2 && num1 > num2;
    }
  }
}
