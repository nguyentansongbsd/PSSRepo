// Decompiled with JetBrains decompiler
// Type: Newtonsoft.Json.Utilities.ReflectionMember
// Assembly: Microsoft.Dynamics.AcceleratedSales.SandboxPlugins, Version=1.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35
// MVID: 7BBE7132-7E30-4424-B38D-27667CFBCEFA
// Assembly location: C:\Users\ngoct\Downloads\New folder\Microsoft.Dynamics.AcceleratedSales.SandboxPlugins_1.0.0.0.dll

using System;

#nullable enable
namespace Newtonsoft.Json.Utilities
{
  internal class ReflectionMember
  {
    public Type? MemberType { get; set; }

    public Func<object, object?>? Getter { get; set; }

    public Action<object, object?>? Setter { get; set; }
  }
}
