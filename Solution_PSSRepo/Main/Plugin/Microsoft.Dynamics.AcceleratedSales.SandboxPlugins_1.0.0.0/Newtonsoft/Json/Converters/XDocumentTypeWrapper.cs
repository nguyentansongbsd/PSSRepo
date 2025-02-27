// Decompiled with JetBrains decompiler
// Type: Newtonsoft.Json.Converters.XDocumentTypeWrapper
// Assembly: Microsoft.Dynamics.AcceleratedSales.SandboxPlugins, Version=1.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35
// MVID: 7BBE7132-7E30-4424-B38D-27667CFBCEFA
// Assembly location: C:\Users\ngoct\Downloads\New folder\Microsoft.Dynamics.AcceleratedSales.SandboxPlugins_1.0.0.0.dll

using System.Xml.Linq;

#nullable enable
namespace Newtonsoft.Json.Converters
{
  internal class XDocumentTypeWrapper : XObjectWrapper, IXmlDocumentType, IXmlNode
  {
    private readonly XDocumentType _documentType;

    public XDocumentTypeWrapper(XDocumentType documentType)
      : base((XObject) documentType)
    {
      this._documentType = documentType;
    }

    public string Name => this._documentType.Name;

    public string System => this._documentType.SystemId;

    public string Public => this._documentType.PublicId;

    public string InternalSubset => this._documentType.InternalSubset;

    public override string? LocalName => "DOCTYPE";
  }
}
