// Decompiled with JetBrains decompiler
// Type: Newtonsoft.Json.Serialization.DefaultSerializationBinder
// Assembly: Microsoft.Dynamics.AcceleratedSales.SandboxPlugins, Version=1.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35
// MVID: 7BBE7132-7E30-4424-B38D-27667CFBCEFA
// Assembly location: C:\Users\ngoct\Downloads\New folder\Microsoft.Dynamics.AcceleratedSales.SandboxPlugins_1.0.0.0.dll

using Newtonsoft.Json.Utilities;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Reflection;
using System.Runtime.Serialization;

#nullable enable
namespace Newtonsoft.Json.Serialization
{
  internal class DefaultSerializationBinder : SerializationBinder, ISerializationBinder
  {
    internal static readonly DefaultSerializationBinder Instance = new DefaultSerializationBinder();
    private readonly ThreadSafeStore<StructMultiKey<string?, string>, Type> _typeCache;

    public DefaultSerializationBinder()
    {
      this._typeCache = new ThreadSafeStore<StructMultiKey<string, string>, Type>(new Func<StructMultiKey<string, string>, Type>(this.GetTypeFromTypeNameKey));
    }

    private Type GetTypeFromTypeNameKey(StructMultiKey<string?, string> typeNameKey)
    {
      string partialName = typeNameKey.Value1;
      string str = typeNameKey.Value2;
      if (partialName == null)
        return Type.GetType(str);
      Assembly assembly1 = Assembly.LoadWithPartialName(partialName);
      if (assembly1 == (Assembly) null)
      {
        foreach (Assembly assembly2 in AppDomain.CurrentDomain.GetAssemblies())
        {
          if (assembly2.FullName == partialName || assembly2.GetName().Name == partialName)
          {
            assembly1 = assembly2;
            break;
          }
        }
      }
      Type typeFromTypeNameKey = !(assembly1 == (Assembly) null) ? assembly1.GetType(str) : throw new JsonSerializationException("Could not load assembly '{0}'.".FormatWith((IFormatProvider) CultureInfo.InvariantCulture, (object) partialName));
      if (typeFromTypeNameKey == (Type) null)
      {
        if (str.IndexOf('`') >= 0)
        {
          try
          {
            typeFromTypeNameKey = this.GetGenericTypeFromTypeName(str, assembly1);
          }
          catch (Exception ex)
          {
            throw new JsonSerializationException("Could not find type '{0}' in assembly '{1}'.".FormatWith((IFormatProvider) CultureInfo.InvariantCulture, (object) str, (object) assembly1.FullName), ex);
          }
        }
        if (typeFromTypeNameKey == (Type) null)
          throw new JsonSerializationException("Could not find type '{0}' in assembly '{1}'.".FormatWith((IFormatProvider) CultureInfo.InvariantCulture, (object) str, (object) assembly1.FullName));
      }
      return typeFromTypeNameKey;
    }

    private Type? GetGenericTypeFromTypeName(string typeName, Assembly assembly)
    {
      Type typeFromTypeName = (Type) null;
      int length = typeName.IndexOf('[');
      if (length >= 0)
      {
        string name = typeName.Substring(0, length);
        Type type = assembly.GetType(name);
        if (type != (Type) null)
        {
          List<Type> typeList = new List<Type>();
          int num1 = 0;
          int startIndex = 0;
          int num2 = typeName.Length - 1;
          for (int index = length + 1; index < num2; ++index)
          {
            switch (typeName[index])
            {
              case '[':
                if (num1 == 0)
                  startIndex = index + 1;
                ++num1;
                break;
              case ']':
                --num1;
                if (num1 == 0)
                {
                  StructMultiKey<string, string> typeNameKey = ReflectionUtils.SplitFullyQualifiedTypeName(typeName.Substring(startIndex, index - startIndex));
                  typeList.Add(this.GetTypeByName(typeNameKey));
                  break;
                }
                break;
            }
          }
          typeFromTypeName = type.MakeGenericType(typeList.ToArray());
        }
      }
      return typeFromTypeName;
    }

    private Type GetTypeByName(StructMultiKey<string?, string> typeNameKey)
    {
      return this._typeCache.Get(typeNameKey);
    }

    public override Type BindToType(string? assemblyName, string typeName)
    {
      return this.GetTypeByName(new StructMultiKey<string, string>(assemblyName, typeName));
    }

    public override void BindToName(
      Type serializedType,
      out string? assemblyName,
      out string? typeName)
    {
      assemblyName = serializedType.Assembly.FullName;
      typeName = serializedType.FullName;
    }
  }
}
