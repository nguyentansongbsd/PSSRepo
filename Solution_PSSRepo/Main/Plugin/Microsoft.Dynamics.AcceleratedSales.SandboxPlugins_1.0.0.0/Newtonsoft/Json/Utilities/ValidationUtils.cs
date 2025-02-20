// Decompiled with JetBrains decompiler
// Type: Newtonsoft.Json.Utilities.ValidationUtils
// Assembly: Microsoft.Dynamics.AcceleratedSales.SandboxPlugins, Version=1.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35
// MVID: 7BBE7132-7E30-4424-B38D-27667CFBCEFA
// Assembly location: C:\Users\ngoct\Downloads\New folder\Microsoft.Dynamics.AcceleratedSales.SandboxPlugins_1.0.0.0.dll

using System;
using System.Diagnostics.CodeAnalysis;

#nullable enable
namespace Newtonsoft.Json.Utilities
{
  internal static class ValidationUtils
  {
    public static void ArgumentNotNull([NotNull] object? value, string parameterName)
    {
      if (value == null)
        throw new ArgumentNullException(parameterName);
    }
  }
}
