// Decompiled with JetBrains decompiler
// Type: Newtonsoft.Json.Serialization.JsonPrimitiveContract
// Assembly: Microsoft.Dynamics.AcceleratedSales.SandboxPlugins, Version=1.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35
// MVID: 7BBE7132-7E30-4424-B38D-27667CFBCEFA
// Assembly location: C:\Users\ngoct\Downloads\New folder\Microsoft.Dynamics.AcceleratedSales.SandboxPlugins_1.0.0.0.dll

using Newtonsoft.Json.Utilities;
using System;
using System.Collections.Generic;

#nullable enable
namespace Newtonsoft.Json.Serialization
{
  internal class JsonPrimitiveContract : JsonContract
  {
    private static readonly Dictionary<Type, ReadType> ReadTypeMap = new Dictionary<Type, ReadType>()
    {
      [typeof (byte[])] = ReadType.ReadAsBytes,
      [typeof (byte)] = ReadType.ReadAsInt32,
      [typeof (short)] = ReadType.ReadAsInt32,
      [typeof (int)] = ReadType.ReadAsInt32,
      [typeof (Decimal)] = ReadType.ReadAsDecimal,
      [typeof (bool)] = ReadType.ReadAsBoolean,
      [typeof (string)] = ReadType.ReadAsString,
      [typeof (DateTime)] = ReadType.ReadAsDateTime,
      [typeof (DateTimeOffset)] = ReadType.ReadAsDateTimeOffset,
      [typeof (float)] = ReadType.ReadAsDouble,
      [typeof (double)] = ReadType.ReadAsDouble,
      [typeof (long)] = ReadType.ReadAsInt64
    };

    internal PrimitiveTypeCode TypeCode { get; set; }

    public JsonPrimitiveContract(Type underlyingType)
      : base(underlyingType)
    {
      this.ContractType = JsonContractType.Primitive;
      this.TypeCode = ConvertUtils.GetTypeCode(underlyingType);
      this.IsReadOnlyOrFixedSize = true;
      ReadType readType;
      if (!JsonPrimitiveContract.ReadTypeMap.TryGetValue(this.NonNullableUnderlyingType, out readType))
        return;
      this.InternalReadType = readType;
    }
  }
}
