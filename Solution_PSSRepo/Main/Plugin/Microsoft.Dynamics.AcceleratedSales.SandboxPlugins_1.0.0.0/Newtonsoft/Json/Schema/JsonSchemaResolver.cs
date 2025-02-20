// Decompiled with JetBrains decompiler
// Type: Newtonsoft.Json.Schema.JsonSchemaResolver
// Assembly: Microsoft.Dynamics.AcceleratedSales.SandboxPlugins, Version=1.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35
// MVID: 7BBE7132-7E30-4424-B38D-27667CFBCEFA
// Assembly location: C:\Users\ngoct\Downloads\New folder\Microsoft.Dynamics.AcceleratedSales.SandboxPlugins_1.0.0.0.dll

using System;
using System.Collections.Generic;
using System.Linq;

#nullable disable
namespace Newtonsoft.Json.Schema
{
  [Obsolete("JSON Schema validation has been moved to its own package. See https://www.newtonsoft.com/jsonschema for more details.")]
  internal class JsonSchemaResolver
  {
    public IList<JsonSchema> LoadedSchemas { get; protected set; }

    public JsonSchemaResolver() => this.LoadedSchemas = (IList<JsonSchema>) new List<JsonSchema>();

    public virtual JsonSchema GetSchema(string reference)
    {
      return this.LoadedSchemas.SingleOrDefault<JsonSchema>((Func<JsonSchema, bool>) (s => string.Equals(s.Id, reference, StringComparison.Ordinal))) ?? this.LoadedSchemas.SingleOrDefault<JsonSchema>((Func<JsonSchema, bool>) (s => string.Equals(s.Location, reference, StringComparison.Ordinal)));
    }
  }
}
