// Decompiled with JetBrains decompiler
// Type: Newtonsoft.Json.Serialization.CamelCasePropertyNamesContractResolver
// Assembly: Microsoft.Dynamics.AcceleratedSales.SandboxPlugins, Version=1.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35
// MVID: 7BBE7132-7E30-4424-B38D-27667CFBCEFA
// Assembly location: C:\Users\ngoct\Downloads\New folder\Microsoft.Dynamics.AcceleratedSales.SandboxPlugins_1.0.0.0.dll

using Newtonsoft.Json.Utilities;
using System;
using System.Collections.Generic;

#nullable enable
namespace Newtonsoft.Json.Serialization
{
  internal class CamelCasePropertyNamesContractResolver : DefaultContractResolver
  {
    private static readonly object TypeContractCacheLock = new object();
    private static readonly DefaultJsonNameTable NameTable = new DefaultJsonNameTable();
    private static Dictionary<StructMultiKey<Type, Type>, JsonContract>? _contractCache;

    public CamelCasePropertyNamesContractResolver()
    {
      CamelCaseNamingStrategy caseNamingStrategy = new CamelCaseNamingStrategy();
      caseNamingStrategy.ProcessDictionaryKeys = true;
      caseNamingStrategy.OverrideSpecifiedNames = true;
      this.NamingStrategy = (NamingStrategy) caseNamingStrategy;
    }

    public override JsonContract ResolveContract(Type type)
    {
      StructMultiKey<Type, Type> key = !(type == (Type) null) ? new StructMultiKey<Type, Type>(this.GetType(), type) : throw new ArgumentNullException(nameof (type));
      Dictionary<StructMultiKey<Type, Type>, JsonContract> contractCache1 = CamelCasePropertyNamesContractResolver._contractCache;
      JsonContract contract;
      if (contractCache1 == null || !contractCache1.TryGetValue(key, out contract))
      {
        contract = this.CreateContract(type);
        lock (CamelCasePropertyNamesContractResolver.TypeContractCacheLock)
        {
          Dictionary<StructMultiKey<Type, Type>, JsonContract> contractCache2 = CamelCasePropertyNamesContractResolver._contractCache;
          Dictionary<StructMultiKey<Type, Type>, JsonContract> dictionary = contractCache2 != null ? new Dictionary<StructMultiKey<Type, Type>, JsonContract>((IDictionary<StructMultiKey<Type, Type>, JsonContract>) contractCache2) : new Dictionary<StructMultiKey<Type, Type>, JsonContract>();
          dictionary[key] = contract;
          CamelCasePropertyNamesContractResolver._contractCache = dictionary;
        }
      }
      return contract;
    }

    internal override DefaultJsonNameTable GetNameTable()
    {
      return CamelCasePropertyNamesContractResolver.NameTable;
    }
  }
}
