// Decompiled with JetBrains decompiler
// Type: Newtonsoft.Json.Converters.XElementWrapper
// Assembly: Microsoft.Dynamics.AcceleratedSales.SandboxPlugins, Version=1.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35
// MVID: 7BBE7132-7E30-4424-B38D-27667CFBCEFA
// Assembly location: C:\Users\ngoct\Downloads\New folder\Microsoft.Dynamics.AcceleratedSales.SandboxPlugins_1.0.0.0.dll

using Newtonsoft.Json.Utilities;
using System.Collections.Generic;
using System.Xml.Linq;

#nullable enable
namespace Newtonsoft.Json.Converters
{
  internal class XElementWrapper(XElement element) : XContainerWrapper((XContainer) element), IXmlElement, IXmlNode
  {
    private List<IXmlNode>? _attributes;

    private XElement Element => (XElement) this.WrappedNode;

    public void SetAttributeNode(IXmlNode attribute)
    {
      this.Element.Add(((XObjectWrapper) attribute).WrappedNode);
      this._attributes = (List<IXmlNode>) null;
    }

    public override List<IXmlNode> Attributes
    {
      get
      {
        if (this._attributes == null)
        {
          if (!this.Element.HasAttributes && !this.HasImplicitNamespaceAttribute(this.NamespaceUri))
          {
            this._attributes = XmlNodeConverter.EmptyChildNodes;
          }
          else
          {
            this._attributes = new List<IXmlNode>();
            foreach (XAttribute attribute in this.Element.Attributes())
              this._attributes.Add((IXmlNode) new XAttributeWrapper(attribute));
            string namespaceUri = this.NamespaceUri;
            if (this.HasImplicitNamespaceAttribute(namespaceUri))
              this._attributes.Insert(0, (IXmlNode) new XAttributeWrapper(new XAttribute((XName) "xmlns", (object) namespaceUri)));
          }
        }
        return this._attributes;
      }
    }

    private bool HasImplicitNamespaceAttribute(string namespaceUri)
    {
      if (!StringUtils.IsNullOrEmpty(namespaceUri) && namespaceUri != this.ParentNode?.NamespaceUri && StringUtils.IsNullOrEmpty(this.GetPrefixOfNamespace(namespaceUri)))
      {
        bool flag = false;
        if (this.Element.HasAttributes)
        {
          foreach (XAttribute attribute in this.Element.Attributes())
          {
            if (attribute.Name.LocalName == "xmlns" && StringUtils.IsNullOrEmpty(attribute.Name.NamespaceName) && attribute.Value == namespaceUri)
              flag = true;
          }
        }
        if (!flag)
          return true;
      }
      return false;
    }

    public override IXmlNode AppendChild(IXmlNode newChild)
    {
      IXmlNode xmlNode = base.AppendChild(newChild);
      this._attributes = (List<IXmlNode>) null;
      return xmlNode;
    }

    public override string? Value
    {
      get => this.Element.Value;
      set => this.Element.Value = value;
    }

    public override string? LocalName => this.Element.Name.LocalName;

    public override string? NamespaceUri => this.Element.Name.NamespaceName;

    public string GetPrefixOfNamespace(string namespaceUri)
    {
      return this.Element.GetPrefixOfNamespace((XNamespace) namespaceUri);
    }

    public bool IsEmpty => this.Element.IsEmpty;
  }
}
