// Decompiled with JetBrains decompiler
// Type: Newtonsoft.Json.Converters.XAttributeWrapper
// Assembly: Microsoft.Dynamics.AcceleratedSales.SandboxPlugins, Version=1.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35
// MVID: 7BBE7132-7E30-4424-B38D-27667CFBCEFA
// Assembly location: C:\Users\ngoct\Downloads\New folder\Microsoft.Dynamics.AcceleratedSales.SandboxPlugins_1.0.0.0.dll

using System.Xml.Linq;

#nullable enable
namespace Newtonsoft.Json.Converters
{
  internal class XAttributeWrapper(XAttribute attribute) : XObjectWrapper((XObject) attribute)
  {
    private XAttribute Attribute => (XAttribute) this.WrappedNode;

    public override string? Value
    {
      get => this.Attribute.Value;
      set => this.Attribute.Value = value;
    }

    public override string? LocalName => this.Attribute.Name.LocalName;

    public override string? NamespaceUri => this.Attribute.Name.NamespaceName;

    public override IXmlNode? ParentNode
    {
      get
      {
        return this.Attribute.Parent == null ? (IXmlNode) null : XContainerWrapper.WrapNode((XObject) this.Attribute.Parent);
      }
    }
  }
}
