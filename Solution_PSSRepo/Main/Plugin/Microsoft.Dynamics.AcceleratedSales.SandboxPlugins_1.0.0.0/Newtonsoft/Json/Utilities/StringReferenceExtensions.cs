// Decompiled with JetBrains decompiler
// Type: Newtonsoft.Json.Utilities.StringReferenceExtensions
// Assembly: Microsoft.Dynamics.AcceleratedSales.SandboxPlugins, Version=1.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35
// MVID: 7BBE7132-7E30-4424-B38D-27667CFBCEFA
// Assembly location: C:\Users\ngoct\Downloads\New folder\Microsoft.Dynamics.AcceleratedSales.SandboxPlugins_1.0.0.0.dll

using System;

#nullable enable
namespace Newtonsoft.Json.Utilities
{
  internal static class StringReferenceExtensions
  {
    public static int IndexOf(this StringReference s, char c, int startIndex, int length)
    {
      int num = Array.IndexOf<char>(s.Chars, c, s.StartIndex + startIndex, length);
      return num == -1 ? -1 : num - s.StartIndex;
    }

    public static bool StartsWith(this StringReference s, string text)
    {
      if (text.Length > s.Length)
        return false;
      char[] chars = s.Chars;
      for (int index = 0; index < text.Length; ++index)
      {
        if ((int) text[index] != (int) chars[index + s.StartIndex])
          return false;
      }
      return true;
    }

    public static bool EndsWith(this StringReference s, string text)
    {
      if (text.Length > s.Length)
        return false;
      char[] chars = s.Chars;
      int num = s.StartIndex + s.Length - text.Length;
      for (int index = 0; index < text.Length; ++index)
      {
        if ((int) text[index] != (int) chars[index + num])
          return false;
      }
      return true;
    }
  }
}
