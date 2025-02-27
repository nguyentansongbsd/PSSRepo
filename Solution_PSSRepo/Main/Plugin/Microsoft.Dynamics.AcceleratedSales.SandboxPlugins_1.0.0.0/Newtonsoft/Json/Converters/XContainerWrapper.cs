// Decompiled with JetBrains decompiler
// Type: Newtonsoft.Json.Converters.XContainerWrapper
// Assembly: Microsoft.Dynamics.AcceleratedSales.SandboxPlugins, Version=1.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35
// MVID: 7BBE7132-7E30-4424-B38D-27667CFBCEFA
// Assembly location: C:\Users\ngoct\Downloads\New folder\Microsoft.Dynamics.AcceleratedSales.SandboxPlugins_1.0.0.0.dll

using System.Collections.Generic;
using System.Xml.Linq;

#nullable enable
namespace Newtonsoft.Json.Converters
{
  internal class XContainerWrapper(XContainer container) : XObjectWrapper((XObject) container)
  {
    private List<IXmlNode>? _childNodes;

    private XContainer Container => (XContainer) this.WrappedNode;

    public override List<IXmlNode> ChildNodes
    {
      get
      {
        if (this._childNodes == null)
        {
          if (!this.HasChildNodes)
          {
            this._childNodes = XmlNodeConverter.EmptyChildNodes;
          }
          else
          {
            this._childNodes = new List<IXmlNode>();
            foreach (XObject node in this.Container.Nodes())
              this._childNodes.Add(XContainerWrapper.WrapNode(node));
          }
        }
        return this._childNodes;
      }
    }

    protected virtual bool HasChildNodes => this.Container.LastNode != null;

    public override IXmlNode? ParentNode
    {
      get
      {
        return this.Container.Parent == null ? (IXmlNode) null : XContainerWrapper.WrapNode((XObject) this.Container.Parent);
      }
    }

    internal static IXmlNode WrapNode(XObject node)
    {
      switch (node)
      {
        case XDocument document:
          return (IXmlNode) new XDocumentWrapper(document);
        case XElement element:
          return (IXmlNode) new XElementWrapper(element);
        case XContainer container:
          return (IXmlNode) new XContainerWrapper(container);
        case XProcessingInstruction processingInstruction:
          return (IXmlNode) new XProcessingInstructionWrapper(processingInstruction);
        case XText text1:
          return (IXmlNode) new XTextWrapper(text1);
        case XComment text2:
          return (IXmlNode) new XCommentWrapper(text2);
        case XAttribute attribute:
          return (IXmlNode) new XAttributeWrapper(attribute);
        case XDocumentType documentType:
          return (IXmlNode) new XDocumentTypeWrapper(documentType);
        default:
          return (IXmlNode) new XObjectWrapper(node);
      }
    }

    public override IXmlNode AppendChild(IXmlNode newChild)
    {
      this.Container.Add(newChild.WrappedNode);
      this._childNodes = (List<IXmlNode>) null;
      return newChild;
    }
  }
}
