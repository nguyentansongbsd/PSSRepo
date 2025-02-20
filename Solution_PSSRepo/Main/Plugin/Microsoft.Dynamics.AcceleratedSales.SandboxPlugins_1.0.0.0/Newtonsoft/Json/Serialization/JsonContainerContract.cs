// Decompiled with JetBrains decompiler
// Type: Newtonsoft.Json.Serialization.JsonContainerContract
// Assembly: Microsoft.Dynamics.AcceleratedSales.SandboxPlugins, Version=1.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35
// MVID: 7BBE7132-7E30-4424-B38D-27667CFBCEFA
// Assembly location: C:\Users\ngoct\Downloads\New folder\Microsoft.Dynamics.AcceleratedSales.SandboxPlugins_1.0.0.0.dll

using Newtonsoft.Json.Utilities;
using System;

#nullable enable
namespace Newtonsoft.Json.Serialization
{
  internal class JsonContainerContract : JsonContract
  {
    private JsonContract? _itemContract;
    private JsonContract? _finalItemContract;

    internal JsonContract? ItemContract
    {
      get => this._itemContract;
      set
      {
        this._itemContract = value;
        if (this._itemContract != null)
          this._finalItemContract = this._itemContract.UnderlyingType.IsSealed() ? this._itemContract : (JsonContract) null;
        else
          this._finalItemContract = (JsonContract) null;
      }
    }

    internal JsonContract? FinalItemContract => this._finalItemContract;

    public JsonConverter? ItemConverter { get; set; }

    public bool? ItemIsReference { get; set; }

    public ReferenceLoopHandling? ItemReferenceLoopHandling { get; set; }

    public TypeNameHandling? ItemTypeNameHandling { get; set; }

    internal JsonContainerContract(Type underlyingType)
      : base(underlyingType)
    {
      JsonContainerAttribute cachedAttribute = JsonTypeReflector.GetCachedAttribute<JsonContainerAttribute>((object) underlyingType);
      if (cachedAttribute == null)
        return;
      if (cachedAttribute.ItemConverterType != (Type) null)
        this.ItemConverter = JsonTypeReflector.CreateJsonConverterInstance(cachedAttribute.ItemConverterType, cachedAttribute.ItemConverterParameters);
      this.ItemIsReference = cachedAttribute._itemIsReference;
      this.ItemReferenceLoopHandling = cachedAttribute._itemReferenceLoopHandling;
      this.ItemTypeNameHandling = cachedAttribute._itemTypeNameHandling;
    }
  }
}
