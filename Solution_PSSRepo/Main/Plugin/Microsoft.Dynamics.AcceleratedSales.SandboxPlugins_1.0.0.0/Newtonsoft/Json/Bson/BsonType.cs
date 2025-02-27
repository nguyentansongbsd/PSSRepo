// Decompiled with JetBrains decompiler
// Type: Newtonsoft.Json.Bson.BsonType
// Assembly: Microsoft.Dynamics.AcceleratedSales.SandboxPlugins, Version=1.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35
// MVID: 7BBE7132-7E30-4424-B38D-27667CFBCEFA
// Assembly location: C:\Users\ngoct\Downloads\New folder\Microsoft.Dynamics.AcceleratedSales.SandboxPlugins_1.0.0.0.dll

#nullable disable
namespace Newtonsoft.Json.Bson
{
  internal enum BsonType : sbyte
  {
    MinKey = -1, // 0xFF
    Number = 1,
    String = 2,
    Object = 3,
    Array = 4,
    Binary = 5,
    Undefined = 6,
    Oid = 7,
    Boolean = 8,
    Date = 9,
    Null = 10, // 0x0A
    Regex = 11, // 0x0B
    Reference = 12, // 0x0C
    Code = 13, // 0x0D
    Symbol = 14, // 0x0E
    CodeWScope = 15, // 0x0F
    Integer = 16, // 0x10
    TimeStamp = 17, // 0x11
    Long = 18, // 0x12
    MaxKey = 127, // 0x7F
  }
}
