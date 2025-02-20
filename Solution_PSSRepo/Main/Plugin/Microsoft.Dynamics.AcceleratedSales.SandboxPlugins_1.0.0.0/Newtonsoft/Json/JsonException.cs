// Decompiled with JetBrains decompiler
// Type: Newtonsoft.Json.JsonException
// Assembly: Microsoft.Dynamics.AcceleratedSales.SandboxPlugins, Version=1.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35
// MVID: 7BBE7132-7E30-4424-B38D-27667CFBCEFA
// Assembly location: C:\Users\ngoct\Downloads\New folder\Microsoft.Dynamics.AcceleratedSales.SandboxPlugins_1.0.0.0.dll

using System;
using System.Runtime.Serialization;

#nullable enable
namespace Newtonsoft.Json
{
  [Serializable]
  internal class JsonException : Exception
  {
    public JsonException()
    {
    }

    public JsonException(string message)
      : base(message)
    {
    }

    public JsonException(string message, Exception? innerException)
      : base(message, innerException)
    {
    }

    public JsonException(SerializationInfo info, StreamingContext context)
      : base(info, context)
    {
    }

    internal static JsonException Create(IJsonLineInfo lineInfo, string path, string message)
    {
      message = JsonPosition.FormatMessage(lineInfo, path, message);
      return new JsonException(message);
    }
  }
}
