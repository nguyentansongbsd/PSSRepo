// Decompiled with JetBrains decompiler
// Type: Newtonsoft.Json.Serialization.JsonDictionaryContract
// Assembly: Microsoft.Dynamics.AcceleratedSales.SandboxPlugins, Version=1.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35
// MVID: 7BBE7132-7E30-4424-B38D-27667CFBCEFA
// Assembly location: C:\Users\ngoct\Downloads\New folder\Microsoft.Dynamics.AcceleratedSales.SandboxPlugins_1.0.0.0.dll

using Newtonsoft.Json.Utilities;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Reflection;

#nullable enable
namespace Newtonsoft.Json.Serialization
{
  internal class JsonDictionaryContract : JsonContainerContract
  {
    private readonly Type? _genericCollectionDefinitionType;
    private Type? _genericWrapperType;
    private ObjectConstructor<object>? _genericWrapperCreator;
    private Func<object>? _genericTemporaryDictionaryCreator;
    private readonly ConstructorInfo? _parameterizedConstructor;
    private ObjectConstructor<object>? _overrideCreator;
    private ObjectConstructor<object>? _parameterizedCreator;

    public Func<string, string>? DictionaryKeyResolver { get; set; }

    public Type? DictionaryKeyType { get; }

    public Type? DictionaryValueType { get; }

    internal JsonContract? KeyContract { get; set; }

    internal bool ShouldCreateWrapper { get; }

    internal ObjectConstructor<object>? ParameterizedCreator
    {
      get
      {
        if (this._parameterizedCreator == null && this._parameterizedConstructor != (ConstructorInfo) null)
          this._parameterizedCreator = JsonTypeReflector.ReflectionDelegateFactory.CreateParameterizedConstructor((MethodBase) this._parameterizedConstructor);
        return this._parameterizedCreator;
      }
    }

    public ObjectConstructor<object>? OverrideCreator
    {
      get => this._overrideCreator;
      set => this._overrideCreator = value;
    }

    public bool HasParameterizedCreator { get; set; }

    internal bool HasParameterizedCreatorInternal
    {
      get
      {
        return this.HasParameterizedCreator || this._parameterizedCreator != null || this._parameterizedConstructor != (ConstructorInfo) null;
      }
    }

    public JsonDictionaryContract(Type underlyingType)
      : base(underlyingType)
    {
      this.ContractType = JsonContractType.Dictionary;
      Type keyType;
      Type valueType;
      if (ReflectionUtils.ImplementsGenericDefinition(this.NonNullableUnderlyingType, typeof (IDictionary<,>), out this._genericCollectionDefinitionType))
      {
        keyType = this._genericCollectionDefinitionType.GetGenericArguments()[0];
        valueType = this._genericCollectionDefinitionType.GetGenericArguments()[1];
        if (ReflectionUtils.IsGenericDefinition(this.NonNullableUnderlyingType, typeof (IDictionary<,>)))
          this.CreatedType = typeof (Dictionary<,>).MakeGenericType(keyType, valueType);
        else if (this.NonNullableUnderlyingType.IsGenericType() && this.NonNullableUnderlyingType.GetGenericTypeDefinition().FullName == "System.Collections.Concurrent.ConcurrentDictionary`2")
          this.ShouldCreateWrapper = true;
        this.IsReadOnlyOrFixedSize = ReflectionUtils.InheritsGenericDefinition(this.NonNullableUnderlyingType, typeof (ReadOnlyDictionary<,>));
      }
      else if (ReflectionUtils.ImplementsGenericDefinition(this.NonNullableUnderlyingType, typeof (IReadOnlyDictionary<,>), out this._genericCollectionDefinitionType))
      {
        keyType = this._genericCollectionDefinitionType.GetGenericArguments()[0];
        valueType = this._genericCollectionDefinitionType.GetGenericArguments()[1];
        if (ReflectionUtils.IsGenericDefinition(this.NonNullableUnderlyingType, typeof (IReadOnlyDictionary<,>)))
          this.CreatedType = typeof (ReadOnlyDictionary<,>).MakeGenericType(keyType, valueType);
        this.IsReadOnlyOrFixedSize = true;
      }
      else
      {
        ReflectionUtils.GetDictionaryKeyValueTypes(this.NonNullableUnderlyingType, out keyType, out valueType);
        if (this.NonNullableUnderlyingType == typeof (IDictionary))
          this.CreatedType = typeof (Dictionary<object, object>);
      }
      if (keyType != (Type) null && valueType != (Type) null)
      {
        this._parameterizedConstructor = CollectionUtils.ResolveEnumerableCollectionConstructor(this.CreatedType, typeof (KeyValuePair<,>).MakeGenericType(keyType, valueType), typeof (IDictionary<,>).MakeGenericType(keyType, valueType));
        if (!this.HasParameterizedCreatorInternal && this.NonNullableUnderlyingType.Name == "FSharpMap`2")
        {
          FSharpUtils.EnsureInitialized(this.NonNullableUnderlyingType.Assembly());
          this._parameterizedCreator = FSharpUtils.Instance.CreateMap(keyType, valueType);
        }
      }
      if (!typeof (IDictionary).IsAssignableFrom(this.CreatedType))
        this.ShouldCreateWrapper = true;
      this.DictionaryKeyType = keyType;
      this.DictionaryValueType = valueType;
      Type createdType;
      ObjectConstructor<object> parameterizedCreator;
      if (!(this.DictionaryKeyType != (Type) null) || !(this.DictionaryValueType != (Type) null) || !ImmutableCollectionsUtils.TryBuildImmutableForDictionaryContract(this.NonNullableUnderlyingType, this.DictionaryKeyType, this.DictionaryValueType, out createdType, out parameterizedCreator))
        return;
      this.CreatedType = createdType;
      this._parameterizedCreator = parameterizedCreator;
      this.IsReadOnlyOrFixedSize = true;
    }

    internal IWrappedDictionary CreateWrapper(object dictionary)
    {
      if (this._genericWrapperCreator == null)
      {
        this._genericWrapperType = typeof (DictionaryWrapper<,>).MakeGenericType(this.DictionaryKeyType, this.DictionaryValueType);
        this._genericWrapperCreator = JsonTypeReflector.ReflectionDelegateFactory.CreateParameterizedConstructor((MethodBase) this._genericWrapperType.GetConstructor(new Type[1]
        {
          this._genericCollectionDefinitionType
        }));
      }
      return (IWrappedDictionary) this._genericWrapperCreator(dictionary);
    }

    internal IDictionary CreateTemporaryDictionary()
    {
      if (this._genericTemporaryDictionaryCreator == null)
      {
        Type type1 = typeof (Dictionary<,>);
        Type[] typeArray = new Type[2];
        Type type2 = this.DictionaryKeyType;
        if ((object) type2 == null)
          type2 = typeof (object);
        typeArray[0] = type2;
        Type type3 = this.DictionaryValueType;
        if ((object) type3 == null)
          type3 = typeof (object);
        typeArray[1] = type3;
        this._genericTemporaryDictionaryCreator = JsonTypeReflector.ReflectionDelegateFactory.CreateDefaultConstructor<object>(type1.MakeGenericType(typeArray));
      }
      return (IDictionary) this._genericTemporaryDictionaryCreator();
    }
  }
}
