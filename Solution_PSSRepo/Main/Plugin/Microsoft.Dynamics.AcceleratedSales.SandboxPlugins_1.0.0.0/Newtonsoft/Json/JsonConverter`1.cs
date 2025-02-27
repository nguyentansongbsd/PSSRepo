// Decompiled with JetBrains decompiler
// Type: Newtonsoft.Json.JsonConverter`1
// Assembly: Microsoft.Dynamics.AcceleratedSales.SandboxPlugins, Version=1.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35
// MVID: 7BBE7132-7E30-4424-B38D-27667CFBCEFA
// Assembly location: C:\Users\ngoct\Downloads\New folder\Microsoft.Dynamics.AcceleratedSales.SandboxPlugins_1.0.0.0.dll

using Newtonsoft.Json.Utilities;
using System;
using System.Globalization;

#nullable enable
namespace Newtonsoft.Json
{
  internal abstract class JsonConverter<T> : JsonConverter
  {
    public override sealed void WriteJson(
      JsonWriter writer,
      object? value,
      JsonSerializer serializer)
    {
      if ((value != null ? (value is T ? 1 : 0) : (ReflectionUtils.IsNullable(typeof (T)) ? 1 : 0)) == 0)
        throw new JsonSerializationException("Converter cannot write specified value to JSON. {0} is required.".FormatWith((IFormatProvider) CultureInfo.InvariantCulture, (object) typeof (T)));
      this.WriteJson(writer, (T) value, serializer);
    }

    public abstract void WriteJson(JsonWriter writer, T? value, JsonSerializer serializer);

    public override sealed object? ReadJson(
      JsonReader reader,
      Type objectType,
      object? existingValue,
      JsonSerializer serializer)
    {
      bool flag = existingValue == null;
      if (!flag && !(existingValue is T))
        throw new JsonSerializationException("Converter cannot read JSON with the specified existing value. {0} is required.".FormatWith((IFormatProvider) CultureInfo.InvariantCulture, (object) typeof (T)));
      return (object) this.ReadJson(reader, objectType, flag ? default (T) : (T) existingValue, !flag, serializer);
    }

    public abstract T? ReadJson(
      JsonReader reader,
      Type objectType,
      T? existingValue,
      bool hasExistingValue,
      JsonSerializer serializer);

    public override sealed bool CanConvert(Type objectType)
    {
      return typeof (T).IsAssignableFrom(objectType);
    }
  }
}
