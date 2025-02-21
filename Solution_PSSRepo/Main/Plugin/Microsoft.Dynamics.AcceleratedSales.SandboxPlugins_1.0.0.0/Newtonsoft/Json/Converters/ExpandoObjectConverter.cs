// Decompiled with JetBrains decompiler
// Type: Newtonsoft.Json.Converters.ExpandoObjectConverter
// Assembly: Microsoft.Dynamics.AcceleratedSales.SandboxPlugins, Version=1.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35
// MVID: 7BBE7132-7E30-4424-B38D-27667CFBCEFA
// Assembly location: C:\Users\ngoct\Downloads\New folder\Microsoft.Dynamics.AcceleratedSales.SandboxPlugins_1.0.0.0.dll

using Newtonsoft.Json.Utilities;
using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Globalization;

#nullable enable
namespace Newtonsoft.Json.Converters
{
  internal class ExpandoObjectConverter : JsonConverter
  {
    public override void WriteJson(JsonWriter writer, object? value, JsonSerializer serializer)
    {
    }

    public override object? ReadJson(
      JsonReader reader,
      Type objectType,
      object? existingValue,
      JsonSerializer serializer)
    {
      return this.ReadValue(reader);
    }

    private object? ReadValue(JsonReader reader)
    {
      if (!reader.MoveToContent())
        throw JsonSerializationException.Create(reader, "Unexpected end when reading ExpandoObject.");
      switch (reader.TokenType)
      {
        case JsonToken.StartObject:
          return this.ReadObject(reader);
        case JsonToken.StartArray:
          return this.ReadList(reader);
        default:
          return JsonTokenUtils.IsPrimitiveToken(reader.TokenType) ? reader.Value : throw JsonSerializationException.Create(reader, "Unexpected token when converting ExpandoObject: {0}".FormatWith((IFormatProvider) CultureInfo.InvariantCulture, (object) reader.TokenType));
      }
    }

    private object ReadList(JsonReader reader)
    {
      IList<object> objectList = (IList<object>) new List<object>();
      while (reader.Read())
      {
        switch (reader.TokenType)
        {
          case JsonToken.Comment:
            continue;
          case JsonToken.EndArray:
            return (object) objectList;
          default:
            object obj = this.ReadValue(reader);
            objectList.Add(obj);
            continue;
        }
      }
      throw JsonSerializationException.Create(reader, "Unexpected end when reading ExpandoObject.");
    }

    private object ReadObject(JsonReader reader)
    {
      IDictionary<string, object> dictionary = (IDictionary<string, object>) new ExpandoObject();
      while (reader.Read())
      {
        switch (reader.TokenType)
        {
          case JsonToken.PropertyName:
            string key = reader.Value.ToString();
            object obj = reader.Read() ? this.ReadValue(reader) : throw JsonSerializationException.Create(reader, "Unexpected end when reading ExpandoObject.");
            dictionary[key] = obj;
            continue;
          case JsonToken.EndObject:
            return (object) dictionary;
          default:
            continue;
        }
      }
      throw JsonSerializationException.Create(reader, "Unexpected end when reading ExpandoObject.");
    }

    public override bool CanConvert(Type objectType) => objectType == typeof (ExpandoObject);

    public override bool CanWrite => false;
  }
}
