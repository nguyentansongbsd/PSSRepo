// Decompiled with JetBrains decompiler
// Type: Newtonsoft.Json.Utilities.EnumInfo
// Assembly: Microsoft.Dynamics.AcceleratedSales.SandboxPlugins, Version=1.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35
// MVID: 7BBE7132-7E30-4424-B38D-27667CFBCEFA
// Assembly location: C:\Users\ngoct\Downloads\New folder\Microsoft.Dynamics.AcceleratedSales.SandboxPlugins_1.0.0.0.dll

#nullable enable
namespace Newtonsoft.Json.Utilities
{
  internal class EnumInfo
  {
    public readonly bool IsFlags;
    public readonly ulong[] Values;
    public readonly string[] Names;
    public readonly string[] ResolvedNames;

    public EnumInfo(bool isFlags, ulong[] values, string[] names, string[] resolvedNames)
    {
      this.IsFlags = isFlags;
      this.Values = values;
      this.Names = names;
      this.ResolvedNames = resolvedNames;
    }
  }
}
