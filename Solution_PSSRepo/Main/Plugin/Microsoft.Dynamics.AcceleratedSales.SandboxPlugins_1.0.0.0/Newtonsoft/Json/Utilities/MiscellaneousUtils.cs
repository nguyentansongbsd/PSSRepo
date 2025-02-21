// Decompiled with JetBrains decompiler
// Type: Newtonsoft.Json.Utilities.MiscellaneousUtils
// Assembly: Microsoft.Dynamics.AcceleratedSales.SandboxPlugins, Version=1.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35
// MVID: 7BBE7132-7E30-4424-B38D-27667CFBCEFA
// Assembly location: C:\Users\ngoct\Downloads\New folder\Microsoft.Dynamics.AcceleratedSales.SandboxPlugins_1.0.0.0.dll

using System;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Text.RegularExpressions;

#nullable enable
namespace Newtonsoft.Json.Utilities
{
  internal static class MiscellaneousUtils
  {
    [Conditional("DEBUG")]
    public static void Assert([DoesNotReturnIf(false)] bool condition, string? message = null)
    {
    }

    public static bool ValueEquals(object? objA, object? objB)
    {
      if (objA == objB)
        return true;
      if (objA == null || objB == null)
        return false;
      if (!(objA.GetType() != objB.GetType()))
        return objA.Equals(objB);
      if (ConvertUtils.IsInteger(objA) && ConvertUtils.IsInteger(objB))
        return Convert.ToDecimal(objA, (IFormatProvider) CultureInfo.CurrentCulture).Equals(Convert.ToDecimal(objB, (IFormatProvider) CultureInfo.CurrentCulture));
      switch (objA)
      {
        case double _:
        case float _:
        case Decimal _:
          switch (objB)
          {
            case double _:
            case float _:
            case Decimal _:
              return MathUtils.ApproxEquals(Convert.ToDouble(objA, (IFormatProvider) CultureInfo.CurrentCulture), Convert.ToDouble(objB, (IFormatProvider) CultureInfo.CurrentCulture));
          }
          break;
      }
      return false;
    }

    public static ArgumentOutOfRangeException CreateArgumentOutOfRangeException(
      string paramName,
      object actualValue,
      string message)
    {
      string message1 = message + Environment.NewLine + "Actual value was {0}.".FormatWith((IFormatProvider) CultureInfo.InvariantCulture, actualValue);
      return new ArgumentOutOfRangeException(paramName, message1);
    }

    public static string ToString(object? value)
    {
      if (value == null)
        return "{null}";
      return !(value is string str) ? value.ToString() : "\"" + str + "\"";
    }

    public static int ByteArrayCompare(byte[] a1, byte[] a2)
    {
      int num1 = a1.Length.CompareTo(a2.Length);
      if (num1 != 0)
        return num1;
      for (int index = 0; index < a1.Length; ++index)
      {
        int num2 = a1[index].CompareTo(a2[index]);
        if (num2 != 0)
          return num2;
      }
      return 0;
    }

    public static string? GetPrefix(string qualifiedName)
    {
      string prefix;
      MiscellaneousUtils.GetQualifiedNameParts(qualifiedName, out prefix, out string _);
      return prefix;
    }

    public static string GetLocalName(string qualifiedName)
    {
      string localName;
      MiscellaneousUtils.GetQualifiedNameParts(qualifiedName, out string _, out localName);
      return localName;
    }

    public static void GetQualifiedNameParts(
      string qualifiedName,
      out string? prefix,
      out string localName)
    {
      int length = qualifiedName.IndexOf(':');
      switch (length)
      {
        case -1:
        case 0:
          prefix = (string) null;
          localName = qualifiedName;
          break;
        default:
          if (qualifiedName.Length - 1 != length)
          {
            prefix = qualifiedName.Substring(0, length);
            localName = qualifiedName.Substring(length + 1);
            break;
          }
          goto case -1;
      }
    }

    internal static RegexOptions GetRegexOptions(string optionsText)
    {
      RegexOptions regexOptions = RegexOptions.None;
      for (int index = 0; index < optionsText.Length; ++index)
      {
        switch (optionsText[index])
        {
          case 'i':
            regexOptions |= RegexOptions.IgnoreCase;
            break;
          case 'm':
            regexOptions |= RegexOptions.Multiline;
            break;
          case 's':
            regexOptions |= RegexOptions.Singleline;
            break;
          case 'x':
            regexOptions |= RegexOptions.ExplicitCapture;
            break;
        }
      }
      return regexOptions;
    }
  }
}
