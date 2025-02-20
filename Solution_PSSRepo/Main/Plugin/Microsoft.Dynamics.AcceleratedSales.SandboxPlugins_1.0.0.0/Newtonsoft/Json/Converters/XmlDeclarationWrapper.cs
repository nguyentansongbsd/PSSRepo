// Decompiled with JetBrains decompiler
// Type: Newtonsoft.Json.Converters.XmlDeclarationWrapper
// Assembly: Microsoft.Dynamics.AcceleratedSales.SandboxPlugins, Version=1.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35
// MVID: 7BBE7132-7E30-4424-B38D-27667CFBCEFA
// Assembly location: C:\Users\ngoct\Downloads\New folder\Microsoft.Dynamics.AcceleratedSales.SandboxPlugins_1.0.0.0.dll

using System.Xml;

#nullable enable
namespace Newtonsoft.Json.Converters
{
  internal class XmlDeclarationWrapper : XmlNodeWrapper, IXmlDeclaration, IXmlNode
  {
    private readonly XmlDeclaration _declaration;

    public XmlDeclarationWrapper(XmlDeclaration declaration)
      : base((XmlNode) declaration)
    {
      this._declaration = declaration;
    }

    public string Version => this._declaration.Version;

    public string Encoding
    {
      get => this._declaration.Encoding;
      set => this._declaration.Encoding = value;
    }

    public string Standalone
    {
      get => this._declaration.Standalone;
      set => this._declaration.Standalone = value;
    }
  }
}
