// Decompiled with JetBrains decompiler
// Type: Newtonsoft.Json.Converters.IXmlNode
// Assembly: Microsoft.Dynamics.AcceleratedSales.SandboxPlugins, Version=1.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35
// MVID: 7BBE7132-7E30-4424-B38D-27667CFBCEFA
// Assembly location: C:\Users\ngoct\Downloads\New folder\Microsoft.Dynamics.AcceleratedSales.SandboxPlugins_1.0.0.0.dll

using System.Collections.Generic;
using System.Xml;

#nullable enable
namespace Newtonsoft.Json.Converters
{
  internal interface IXmlNode
  {
    XmlNodeType NodeType { get; }

    string? LocalName { get; }

    List<IXmlNode> ChildNodes { get; }

    List<IXmlNode> Attributes { get; }

    IXmlNode? ParentNode { get; }

    string? Value { get; set; }

    IXmlNode AppendChild(IXmlNode newChild);

    string? NamespaceUri { get; }

    object? WrappedNode { get; }
  }
}
