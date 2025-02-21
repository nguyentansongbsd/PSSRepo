// Decompiled with JetBrains decompiler
// Type: Newtonsoft.Json.Serialization.SerializationBinderAdapter
// Assembly: Microsoft.Dynamics.AcceleratedSales.SandboxPlugins, Version=1.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35
// MVID: 7BBE7132-7E30-4424-B38D-27667CFBCEFA
// Assembly location: C:\Users\ngoct\Downloads\New folder\Microsoft.Dynamics.AcceleratedSales.SandboxPlugins_1.0.0.0.dll

using System;
using System.Runtime.Serialization;

#nullable enable
namespace Newtonsoft.Json.Serialization
{
  internal class SerializationBinderAdapter : ISerializationBinder
  {
    public readonly SerializationBinder SerializationBinder;

    public SerializationBinderAdapter(SerializationBinder serializationBinder)
    {
      this.SerializationBinder = serializationBinder;
    }

    public Type BindToType(string? assemblyName, string typeName)
    {
      return this.SerializationBinder.BindToType(assemblyName, typeName);
    }

    public void BindToName(Type serializedType, out string? assemblyName, out string? typeName)
    {
      this.SerializationBinder.BindToName(serializedType, out assemblyName, out typeName);
    }
  }
}
