// Decompiled with JetBrains decompiler
// Type: Newtonsoft.Json.JsonContainerAttribute
// Assembly: Microsoft.Dynamics.AcceleratedSales.SandboxPlugins, Version=1.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35
// MVID: 7BBE7132-7E30-4424-B38D-27667CFBCEFA
// Assembly location: C:\Users\ngoct\Downloads\New folder\Microsoft.Dynamics.AcceleratedSales.SandboxPlugins_1.0.0.0.dll

using Newtonsoft.Json.Serialization;
using System;

#nullable enable
namespace Newtonsoft.Json
{
  [AttributeUsage(AttributeTargets.Class | AttributeTargets.Interface, AllowMultiple = false)]
  internal abstract class JsonContainerAttribute : Attribute
  {
    internal bool? _isReference;
    internal bool? _itemIsReference;
    internal ReferenceLoopHandling? _itemReferenceLoopHandling;
    internal TypeNameHandling? _itemTypeNameHandling;
    private Type? _namingStrategyType;
    private object[]? _namingStrategyParameters;

    public string? Id { get; set; }

    public string? Title { get; set; }

    public string? Description { get; set; }

    public Type? ItemConverterType { get; set; }

    public object[]? ItemConverterParameters { get; set; }

    public Type? NamingStrategyType
    {
      get => this._namingStrategyType;
      set
      {
        this._namingStrategyType = value;
        this.NamingStrategyInstance = (NamingStrategy) null;
      }
    }

    public object[]? NamingStrategyParameters
    {
      get => this._namingStrategyParameters;
      set
      {
        this._namingStrategyParameters = value;
        this.NamingStrategyInstance = (NamingStrategy) null;
      }
    }

    internal NamingStrategy? NamingStrategyInstance { get; set; }

    public bool IsReference
    {
      get => this._isReference.GetValueOrDefault();
      set => this._isReference = new bool?(value);
    }

    public bool ItemIsReference
    {
      get => this._itemIsReference.GetValueOrDefault();
      set => this._itemIsReference = new bool?(value);
    }

    public ReferenceLoopHandling ItemReferenceLoopHandling
    {
      get => this._itemReferenceLoopHandling.GetValueOrDefault();
      set => this._itemReferenceLoopHandling = new ReferenceLoopHandling?(value);
    }

    public TypeNameHandling ItemTypeNameHandling
    {
      get => this._itemTypeNameHandling.GetValueOrDefault();
      set => this._itemTypeNameHandling = new TypeNameHandling?(value);
    }

    protected JsonContainerAttribute()
    {
    }

    protected JsonContainerAttribute(string id) => this.Id = id;
  }
}
