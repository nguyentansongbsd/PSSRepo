// Decompiled with JetBrains decompiler
// Type: Newtonsoft.Json.JsonArrayAttribute
// Assembly: Microsoft.Dynamics.AcceleratedSales.SandboxPlugins, Version=1.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35
// MVID: 7BBE7132-7E30-4424-B38D-27667CFBCEFA
// Assembly location: C:\Users\ngoct\Downloads\New folder\Microsoft.Dynamics.AcceleratedSales.SandboxPlugins_1.0.0.0.dll

using System;

#nullable enable
namespace Newtonsoft.Json
{
  [AttributeUsage(AttributeTargets.Class | AttributeTargets.Interface, AllowMultiple = false)]
  internal sealed class JsonArrayAttribute : JsonContainerAttribute
  {
    private bool _allowNullItems;

    public bool AllowNullItems
    {
      get => this._allowNullItems;
      set => this._allowNullItems = value;
    }

    public JsonArrayAttribute()
    {
    }

    public JsonArrayAttribute(bool allowNullItems) => this._allowNullItems = allowNullItems;

    public JsonArrayAttribute(string id)
      : base(id)
    {
    }
  }
}
