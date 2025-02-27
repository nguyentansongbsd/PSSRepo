// Decompiled with JetBrains decompiler
// Type: Newtonsoft.Json.Linq.IJEnumerable`1
// Assembly: Microsoft.Dynamics.AcceleratedSales.SandboxPlugins, Version=1.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35
// MVID: 7BBE7132-7E30-4424-B38D-27667CFBCEFA
// Assembly location: C:\Users\ngoct\Downloads\New folder\Microsoft.Dynamics.AcceleratedSales.SandboxPlugins_1.0.0.0.dll

using System.Collections;
using System.Collections.Generic;

#nullable enable
namespace Newtonsoft.Json.Linq
{
  internal interface IJEnumerable<out T> : IEnumerable<T>, IEnumerable where T : JToken
  {
    IJEnumerable<JToken> this[object key] { get; }
  }
}
