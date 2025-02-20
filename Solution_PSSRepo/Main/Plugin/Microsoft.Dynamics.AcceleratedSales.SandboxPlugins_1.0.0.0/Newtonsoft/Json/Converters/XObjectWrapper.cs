// Decompiled with JetBrains decompiler
// Type: Newtonsoft.Json.Converters.XObjectWrapper
// Assembly: Microsoft.Dynamics.AcceleratedSales.SandboxPlugins, Version=1.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35
// MVID: 7BBE7132-7E30-4424-B38D-27667CFBCEFA
// Assembly location: C:\Users\ngoct\Downloads\New folder\Microsoft.Dynamics.AcceleratedSales.SandboxPlugins_1.0.0.0.dll

using System;
using System.Collections.Generic;
using System.Xml;
using System.Xml.Linq;

#nullable enable
namespace Newtonsoft.Json.Converters
{
  internal class XObjectWrapper : IXmlNode
  {
    private readonly XObject? _xmlObject;

    public XObjectWrapper(XObject? xmlObject) => this._xmlObject = xmlObject;

    public object? WrappedNode => (object) this._xmlObject;

    public virtual XmlNodeType NodeType
    {
      get
      {
        XObject xmlObject = this._xmlObject;
        return xmlObject == null ? XmlNodeType.None : xmlObject.NodeType;
      }
    }

    public virtual string? LocalName => (string) null;

    public virtual List<IXmlNode> ChildNodes => XmlNodeConverter.EmptyChildNodes;

    public virtual List<IXmlNode> Attributes => XmlNodeConverter.EmptyChildNodes;

    public virtual IXmlNode? ParentNode => (IXmlNode) null;

    public virtual string? Value
    {
      get => (string) null;
      set => throw new InvalidOperationException();
    }

    public virtual IXmlNode AppendChild(IXmlNode newChild) => throw new InvalidOperationException();

    public virtual string? NamespaceUri => (string) null;
  }
}
