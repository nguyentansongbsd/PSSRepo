// Decompiled with JetBrains decompiler
// Type: Newtonsoft.Json.Serialization.CachedAttributeGetter`1
// Assembly: Microsoft.Dynamics.AcceleratedSales.SandboxPlugins, Version=1.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35
// MVID: 7BBE7132-7E30-4424-B38D-27667CFBCEFA
// Assembly location: C:\Users\ngoct\Downloads\New folder\Microsoft.Dynamics.AcceleratedSales.SandboxPlugins_1.0.0.0.dll

using Newtonsoft.Json.Utilities;
using System;

#nullable enable
namespace Newtonsoft.Json.Serialization
{
  internal static class CachedAttributeGetter<T> where T : 
  #nullable disable
  Attribute
  {
    private static readonly 
    #nullable enable
    ThreadSafeStore<object, T?> TypeAttributeCache = new ThreadSafeStore<object, T>(new Func<object, T>(JsonTypeReflector.GetAttribute<T>));

    public static T? GetAttribute(object type)
    {
      return CachedAttributeGetter<T>.TypeAttributeCache.Get(type);
    }
  }
}
