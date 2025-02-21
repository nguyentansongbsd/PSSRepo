// Decompiled with JetBrains decompiler
// Type: Newtonsoft.Json.Converters.XTextWrapper
// Assembly: Microsoft.Dynamics.AcceleratedSales.SandboxPlugins, Version=1.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35
// MVID: 7BBE7132-7E30-4424-B38D-27667CFBCEFA
// Assembly location: C:\Users\ngoct\Downloads\New folder\Microsoft.Dynamics.AcceleratedSales.SandboxPlugins_1.0.0.0.dll

using System.Xml.Linq;

#nullable enable
namespace Newtonsoft.Json.Converters
{
  internal class XTextWrapper(XText text) : XObjectWrapper((XObject) text)
  {
    private XText Text => (XText) this.WrappedNode;

    public override string? Value
    {
      get => this.Text.Value;
      set => this.Text.Value = value;
    }

    public override IXmlNode? ParentNode
    {
      get
      {
        return this.Text.Parent == null ? (IXmlNode) null : XContainerWrapper.WrapNode((XObject) this.Text.Parent);
      }
    }
  }
}
