// Decompiled with JetBrains decompiler
// Type: Newtonsoft.Json.Schema.JsonSchemaNodeCollection
// Assembly: Microsoft.Dynamics.AcceleratedSales.SandboxPlugins, Version=1.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35
// MVID: 7BBE7132-7E30-4424-B38D-27667CFBCEFA
// Assembly location: C:\Users\ngoct\Downloads\New folder\Microsoft.Dynamics.AcceleratedSales.SandboxPlugins_1.0.0.0.dll

using System;
using System.Collections.ObjectModel;

#nullable disable
namespace Newtonsoft.Json.Schema
{
  [Obsolete("JSON Schema validation has been moved to its own package. See https://www.newtonsoft.com/jsonschema for more details.")]
  internal class JsonSchemaNodeCollection : KeyedCollection<string, JsonSchemaNode>
  {
    protected override string GetKeyForItem(JsonSchemaNode item) => item.Id;
  }
}
