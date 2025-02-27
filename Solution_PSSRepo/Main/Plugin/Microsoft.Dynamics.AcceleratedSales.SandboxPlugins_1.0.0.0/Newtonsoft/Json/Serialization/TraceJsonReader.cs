// Decompiled with JetBrains decompiler
// Type: Newtonsoft.Json.Serialization.TraceJsonReader
// Assembly: Microsoft.Dynamics.AcceleratedSales.SandboxPlugins, Version=1.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35
// MVID: 7BBE7132-7E30-4424-B38D-27667CFBCEFA
// Assembly location: C:\Users\ngoct\Downloads\New folder\Microsoft.Dynamics.AcceleratedSales.SandboxPlugins_1.0.0.0.dll

using System;
using System.Globalization;
using System.IO;

#nullable enable
namespace Newtonsoft.Json.Serialization
{
  internal class TraceJsonReader : JsonReader, IJsonLineInfo
  {
    private readonly JsonReader _innerReader;
    private readonly JsonTextWriter _textWriter;
    private readonly StringWriter _sw;

    public TraceJsonReader(JsonReader innerReader)
    {
      this._innerReader = innerReader;
      this._sw = new StringWriter((IFormatProvider) CultureInfo.InvariantCulture);
      this._sw.Write("Deserialized JSON: " + Environment.NewLine);
      this._textWriter = new JsonTextWriter((TextWriter) this._sw);
      this._textWriter.Formatting = Formatting.Indented;
    }

    public string GetDeserializedJsonMessage() => this._sw.ToString();

    public override bool Read()
    {
      int num = this._innerReader.Read() ? 1 : 0;
      this.WriteCurrentToken();
      return num != 0;
    }

    public override int? ReadAsInt32()
    {
      int? nullable = this._innerReader.ReadAsInt32();
      this.WriteCurrentToken();
      return nullable;
    }

    public override string? ReadAsString()
    {
      string str = this._innerReader.ReadAsString();
      this.WriteCurrentToken();
      return str;
    }

    public override byte[]? ReadAsBytes()
    {
      byte[] numArray = this._innerReader.ReadAsBytes();
      this.WriteCurrentToken();
      return numArray;
    }

    public override Decimal? ReadAsDecimal()
    {
      Decimal? nullable = this._innerReader.ReadAsDecimal();
      this.WriteCurrentToken();
      return nullable;
    }

    public override double? ReadAsDouble()
    {
      double? nullable = this._innerReader.ReadAsDouble();
      this.WriteCurrentToken();
      return nullable;
    }

    public override bool? ReadAsBoolean()
    {
      bool? nullable = this._innerReader.ReadAsBoolean();
      this.WriteCurrentToken();
      return nullable;
    }

    public override DateTime? ReadAsDateTime()
    {
      DateTime? nullable = this._innerReader.ReadAsDateTime();
      this.WriteCurrentToken();
      return nullable;
    }

    public override DateTimeOffset? ReadAsDateTimeOffset()
    {
      DateTimeOffset? nullable = this._innerReader.ReadAsDateTimeOffset();
      this.WriteCurrentToken();
      return nullable;
    }

    public void WriteCurrentToken()
    {
      this._textWriter.WriteToken(this._innerReader, false, false, true);
    }

    public override int Depth => this._innerReader.Depth;

    public override string Path => this._innerReader.Path;

    public override char QuoteChar
    {
      get => this._innerReader.QuoteChar;
      protected internal set => this._innerReader.QuoteChar = value;
    }

    public override JsonToken TokenType => this._innerReader.TokenType;

    public override object? Value => this._innerReader.Value;

    public override Type? ValueType => this._innerReader.ValueType;

    public override void Close() => this._innerReader.Close();

    bool IJsonLineInfo.HasLineInfo()
    {
      return this._innerReader is IJsonLineInfo innerReader && innerReader.HasLineInfo();
    }

    int IJsonLineInfo.LineNumber
    {
      get => !(this._innerReader is IJsonLineInfo innerReader) ? 0 : innerReader.LineNumber;
    }

    int IJsonLineInfo.LinePosition
    {
      get => !(this._innerReader is IJsonLineInfo innerReader) ? 0 : innerReader.LinePosition;
    }
  }
}
