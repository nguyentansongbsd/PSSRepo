// Decompiled with JetBrains decompiler
// Type: Microsoft.Dynamics.AcceleratedSales.SandboxPlugins.Core.Model.IEntityMetadataProvider
// Assembly: Microsoft.Dynamics.AcceleratedSales.SandboxPlugins, Version=1.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35
// MVID: 7BBE7132-7E30-4424-B38D-27667CFBCEFA
// Assembly location: C:\Users\ngoct\Downloads\New folder\Microsoft.Dynamics.AcceleratedSales.SandboxPlugins_1.0.0.0.dll

using Microsoft.Xrm.Sdk.Metadata;

#nullable disable
namespace Microsoft.Dynamics.AcceleratedSales.SandboxPlugins.Core.Model
{
  public interface IEntityMetadataProvider
  {
    AttributeMetadata[] GetAttributes(string entityName);

    EntityMetadata GetEntityMetadata(string entityName);

    string GetIconVectorUrl(string entityName);

    string GetObjectTypeIconUrl(string entityName);

    OneToManyRelationshipMetadata[] GetOneToManyRelationships(string entityName);

    OneToManyRelationshipMetadata[] GetManyToOneRelationships(string entityName);

    string GetPrimaryImageUrlAttributeName(string entityName);

    void PreFetchEntityMetadata(MetadataQueryParams metadataQueryParams);
  }
}
