// Decompiled with JetBrains decompiler
// Type: Newtonsoft.Json.Bson.BsonArray
// Assembly: Microsoft.Dynamics.AcceleratedSales.SandboxPlugins, Version=1.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35
// MVID: 7BBE7132-7E30-4424-B38D-27667CFBCEFA
// Assembly location: C:\Users\ngoct\Downloads\New folder\Microsoft.Dynamics.AcceleratedSales.SandboxPlugins_1.0.0.0.dll

using System.Collections;
using System.Collections.Generic;

#nullable disable
namespace Newtonsoft.Json.Bson
{
  internal class BsonArray : BsonToken, IEnumerable<BsonToken>, IEnumerable
  {
    private readonly List<BsonToken> _children = new List<BsonToken>();

    public void Add(BsonToken token)
    {
      this._children.Add(token);
      token.Parent = (BsonToken) this;
    }

    public override BsonType Type => BsonType.Array;

    public IEnumerator<BsonToken> GetEnumerator()
    {
      return (IEnumerator<BsonToken>) this._children.GetEnumerator();
    }

    IEnumerator IEnumerable.GetEnumerator() => (IEnumerator) this.GetEnumerator();
  }
}
