// Decompiled with JetBrains decompiler
// Type: Newtonsoft.Json.JsonSerializationException
// Assembly: Microsoft.Dynamics.AcceleratedSales.SandboxPlugins, Version=1.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35
// MVID: 7BBE7132-7E30-4424-B38D-27667CFBCEFA
// Assembly location: C:\Users\ngoct\Downloads\New folder\Microsoft.Dynamics.AcceleratedSales.SandboxPlugins_1.0.0.0.dll

using System;
using System.Runtime.Serialization;

#nullable enable
namespace Newtonsoft.Json
{
  [Serializable]
  internal class JsonSerializationException : JsonException
  {
    public int LineNumber { get; }

    public int LinePosition { get; }

    public string? Path { get; }

    public JsonSerializationException()
    {
    }

    public JsonSerializationException(string message)
      : base(message)
    {
    }

    public JsonSerializationException(string message, Exception innerException)
      : base(message, innerException)
    {
    }

    public JsonSerializationException(SerializationInfo info, StreamingContext context)
      : base(info, context)
    {
    }

    public JsonSerializationException(
      string message,
      string path,
      int lineNumber,
      int linePosition,
      Exception? innerException)
      : base(message, innerException)
    {
      this.Path = path;
      this.LineNumber = lineNumber;
      this.LinePosition = linePosition;
    }

    internal static JsonSerializationException Create(JsonReader reader, string message)
    {
      return JsonSerializationException.Create(reader, message, (Exception) null);
    }

    internal static JsonSerializationException Create(
      JsonReader reader,
      string message,
      Exception? ex)
    {
      return JsonSerializationException.Create(reader as IJsonLineInfo, reader.Path, message, ex);
    }

    internal static JsonSerializationException Create(
      IJsonLineInfo? lineInfo,
      string path,
      string message,
      Exception? ex)
    {
      message = JsonPosition.FormatMessage(lineInfo, path, message);
      int lineNumber;
      int linePosition;
      if (lineInfo != null && lineInfo.HasLineInfo())
      {
        lineNumber = lineInfo.LineNumber;
        linePosition = lineInfo.LinePosition;
      }
      else
      {
        lineNumber = 0;
        linePosition = 0;
      }
      return new JsonSerializationException(message, path, lineNumber, linePosition, ex);
    }
  }
}
