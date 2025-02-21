// Decompiled with JetBrains decompiler
// Type: Newtonsoft.Json.Bson.BsonBinaryType
// Assembly: Microsoft.Dynamics.AcceleratedSales.SandboxPlugins, Version=1.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35
// MVID: 7BBE7132-7E30-4424-B38D-27667CFBCEFA
// Assembly location: C:\Users\ngoct\Downloads\New folder\Microsoft.Dynamics.AcceleratedSales.SandboxPlugins_1.0.0.0.dll

using System;

#nullable disable
namespace Newtonsoft.Json.Bson
{
  internal enum BsonBinaryType : byte
  {
    Binary = 0,
    Function = 1,
    [Obsolete("This type has been deprecated in the BSON specification. Use Binary instead.")] BinaryOld = 2,
    [Obsolete("This type has been deprecated in the BSON specification. Use Uuid instead.")] UuidOld = 3,
    Uuid = 4,
    Md5 = 5,
    UserDefined = 128, // 0x80
  }
}
