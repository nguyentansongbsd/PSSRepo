// Decompiled with JetBrains decompiler
// Type: Newtonsoft.Json.Schema.JsonSchemaType
// Assembly: Microsoft.Dynamics.AcceleratedSales.SandboxPlugins, Version=1.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35
// MVID: 7BBE7132-7E30-4424-B38D-27667CFBCEFA
// Assembly location: C:\Users\ngoct\Downloads\New folder\Microsoft.Dynamics.AcceleratedSales.SandboxPlugins_1.0.0.0.dll

using System;

#nullable disable
namespace Newtonsoft.Json.Schema
{
  [Flags]
  [Obsolete("JSON Schema validation has been moved to its own package. See https://www.newtonsoft.com/jsonschema for more details.")]
  internal enum JsonSchemaType
  {
    None = 0,
    String = 1,
    Float = 2,
    Integer = 4,
    Boolean = 8,
    Object = 16, // 0x00000010
    Array = 32, // 0x00000020
    Null = 64, // 0x00000040
    Any = Null | Array | Object | Boolean | Integer | Float | String, // 0x0000007F
  }
}
