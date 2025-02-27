// Decompiled with JetBrains decompiler
// Type: Newtonsoft.Json.Converters.IXmlDocument
// Assembly: Microsoft.Dynamics.AcceleratedSales.SandboxPlugins, Version=1.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35
// MVID: 7BBE7132-7E30-4424-B38D-27667CFBCEFA
// Assembly location: C:\Users\ngoct\Downloads\New folder\Microsoft.Dynamics.AcceleratedSales.SandboxPlugins_1.0.0.0.dll

#nullable enable
namespace Newtonsoft.Json.Converters
{
  internal interface IXmlDocument : IXmlNode
  {
    IXmlNode CreateComment(string? text);

    IXmlNode CreateTextNode(string? text);

    IXmlNode CreateCDataSection(string? data);

    IXmlNode CreateWhitespace(string? text);

    IXmlNode CreateSignificantWhitespace(string? text);

    IXmlNode CreateXmlDeclaration(string? version, string? encoding, string? standalone);

    IXmlNode CreateXmlDocumentType(
      string? name,
      string? publicId,
      string? systemId,
      string? internalSubset);

    IXmlNode CreateProcessingInstruction(string target, string? data);

    IXmlElement CreateElement(string elementName);

    IXmlElement CreateElement(string qualifiedName, string namespaceUri);

    IXmlNode CreateAttribute(string name, string? value);

    IXmlNode CreateAttribute(string qualifiedName, string namespaceUri, string? value);

    IXmlElement? DocumentElement { get; }
  }
}
