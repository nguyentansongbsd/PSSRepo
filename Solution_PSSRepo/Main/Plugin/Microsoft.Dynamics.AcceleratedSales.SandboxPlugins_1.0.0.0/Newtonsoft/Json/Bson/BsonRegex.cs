// Decompiled with JetBrains decompiler
// Type: Newtonsoft.Json.Bson.BsonRegex
// Assembly: Microsoft.Dynamics.AcceleratedSales.SandboxPlugins, Version=1.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35
// MVID: 7BBE7132-7E30-4424-B38D-27667CFBCEFA
// Assembly location: C:\Users\ngoct\Downloads\New folder\Microsoft.Dynamics.AcceleratedSales.SandboxPlugins_1.0.0.0.dll

#nullable disable
namespace Newtonsoft.Json.Bson
{
  internal class BsonRegex : BsonToken
  {
    public BsonString Pattern { get; set; }

    public BsonString Options { get; set; }

    public BsonRegex(string pattern, string options)
    {
      this.Pattern = new BsonString((object) pattern, false);
      this.Options = new BsonString((object) options, false);
    }

    public override BsonType Type => BsonType.Regex;
  }
}
