// Decompiled with JetBrains decompiler
// Type: Newtonsoft.Json.JsonReaderException
// Assembly: Microsoft.Dynamics.AcceleratedSales.SandboxPlugins, Version=1.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35
// MVID: 7BBE7132-7E30-4424-B38D-27667CFBCEFA
// Assembly location: C:\Users\ngoct\Downloads\New folder\Microsoft.Dynamics.AcceleratedSales.SandboxPlugins_1.0.0.0.dll

using System;
using System.Runtime.Serialization;

#nullable enable
namespace Newtonsoft.Json
{
  [Serializable]
  internal class JsonReaderException : JsonException
  {
    public int LineNumber { get; }

    public int LinePosition { get; }

    public string? Path { get; }

    public JsonReaderException()
    {
    }

    public JsonReaderException(string message)
      : base(message)
    {
    }

    public JsonReaderException(string message, Exception innerException)
      : base(message, innerException)
    {
    }

    public JsonReaderException(SerializationInfo info, StreamingContext context)
      : base(info, context)
    {
    }

    public JsonReaderException(
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

    internal static JsonReaderException Create(JsonReader reader, string message)
    {
      return JsonReaderException.Create(reader, message, (Exception) null);
    }

    internal static JsonReaderException Create(JsonReader reader, string message, Exception? ex)
    {
      return JsonReaderException.Create(reader as IJsonLineInfo, reader.Path, message, ex);
    }

    internal static JsonReaderException Create(
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
      return new JsonReaderException(message, path, lineNumber, linePosition, ex);
    }
  }
}
