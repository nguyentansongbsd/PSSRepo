// Decompiled with JetBrains decompiler
// Type: Newtonsoft.Json.JsonObjectAttribute
// Assembly: Microsoft.Dynamics.AcceleratedSales.SandboxPlugins, Version=1.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35
// MVID: 7BBE7132-7E30-4424-B38D-27667CFBCEFA
// Assembly location: C:\Users\ngoct\Downloads\New folder\Microsoft.Dynamics.AcceleratedSales.SandboxPlugins_1.0.0.0.dll

using System;

#nullable enable
namespace Newtonsoft.Json
{
  [AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct | AttributeTargets.Interface, AllowMultiple = false)]
  internal sealed class JsonObjectAttribute : JsonContainerAttribute
  {
    private MemberSerialization _memberSerialization;
    internal MissingMemberHandling? _missingMemberHandling;
    internal Required? _itemRequired;
    internal NullValueHandling? _itemNullValueHandling;

    public MemberSerialization MemberSerialization
    {
      get => this._memberSerialization;
      set => this._memberSerialization = value;
    }

    public MissingMemberHandling MissingMemberHandling
    {
      get => this._missingMemberHandling.GetValueOrDefault();
      set => this._missingMemberHandling = new MissingMemberHandling?(value);
    }

    public NullValueHandling ItemNullValueHandling
    {
      get => this._itemNullValueHandling.GetValueOrDefault();
      set => this._itemNullValueHandling = new NullValueHandling?(value);
    }

    public Required ItemRequired
    {
      get => this._itemRequired.GetValueOrDefault();
      set => this._itemRequired = new Required?(value);
    }

    public JsonObjectAttribute()
    {
    }

    public JsonObjectAttribute(MemberSerialization memberSerialization)
    {
      this.MemberSerialization = memberSerialization;
    }

    public JsonObjectAttribute(string id)
      : base(id)
    {
    }
  }
}
