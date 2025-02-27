// Decompiled with JetBrains decompiler
// Type: Microsoft.Dynamics.AcceleratedSales.SandboxPlugins.Core.Services.WorklistSettingsDataProviderService
// Assembly: Microsoft.Dynamics.AcceleratedSales.SandboxPlugins, Version=1.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35
// MVID: 7BBE7132-7E30-4424-B38D-27667CFBCEFA
// Assembly location: C:\Users\ngoct\Downloads\New folder\Microsoft.Dynamics.AcceleratedSales.SandboxPlugins_1.0.0.0.dll

using Microsoft.Dynamics.AcceleratedSales.SandboxPlugins.Core.DataAccess;
using Microsoft.Dynamics.AcceleratedSales.SandboxPlugins.Core.Model;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Metadata;
using System;
using System.Collections.Generic;

#nullable disable
namespace Microsoft.Dynamics.AcceleratedSales.SandboxPlugins.Core.Services
{
  public class WorklistSettingsDataProviderService
  {
    private IDataStore dataStore;
    private IAcceleratedSalesLogger logger;
    private IEntityMetadataProvider entityMetadataProvider;
    private WorklistViewConfigurationDataAccess worklistViewConfigurationDataAccess;
    private WorkQueueUserSettingsDataAccess workQueueUserSettingsDataAccess;

    public WorklistSettingsDataProviderService(
      IDataStore dataStore,
      IAcceleratedSalesLogger logger,
      IEntityMetadataProvider metadataProvider)
    {
      this.dataStore = dataStore;
      this.logger = logger;
      this.entityMetadataProvider = metadataProvider;
      this.worklistViewConfigurationDataAccess = new WorklistViewConfigurationDataAccess(this.dataStore, this.logger);
      this.workQueueUserSettingsDataAccess = new WorkQueueUserSettingsDataAccess(this.dataStore, this.logger);
    }

    public WorklistSettingsDataResponse GetWorklistSettingsData(
      GetWorklistSettingsDataRequest requestPayload)
    {
      SettingsViewConfiguration viewConfiguration = SettingsViewConfiguration.FromRequestPayload(requestPayload, this.logger);
      WorklistAdminConfiguration adminConfiguration = this.worklistViewConfigurationDataAccess.GetAdminConfiguration(ViewType.Sequence);
      WorklistSellerConfiguration sellerConfiguration = this.workQueueUserSettingsDataAccess.GetSellerConfiguration();
      Dictionary<string, Label> entityMetadataCacheDisplayName = new Dictionary<string, Label>();
      Dictionary<string, List<EntityAttributes>> entityAttributeStore = new Dictionary<string, List<EntityAttributes>>();
      Dictionary<string, List<EntityRelationship>> entityRelationshipStore = new Dictionary<string, List<EntityRelationship>>();
      Dictionary<string, string> primaryNameAttributes = new Dictionary<string, string>();
      Dictionary<string, bool> isActivityMapping = new Dictionary<string, bool>();
      foreach (string entityName in (IEnumerable<string>) viewConfiguration.EntityNames)
      {
        HashSet<string> entityMetadataAndAttributesToFetch = new HashSet<string>();
        entityMetadataAndAttributesToFetch.Add(entityName);
        EntityMetadata entityMetadata = this.entityMetadataProvider.GetEntityMetadata(entityName);
        string primaryNameAttribute = entityMetadata?.PrimaryNameAttribute;
        isActivityMapping[entityName] = this.IsActivityEntity(entityMetadata, entityName);
        primaryNameAttributes.Add(entityName, primaryNameAttribute);
        OneToManyRelationshipMetadata[] oneRelationships = this.entityMetadataProvider.GetEntityMetadata(entityName)?.ManyToOneRelationships;
        if (oneRelationships != null)
        {
          foreach (OneToManyRelationshipMetadata relationshipMetadata in oneRelationships)
          {
            if (relationshipMetadata != null && relationshipMetadata.IsValidForAdvancedFind.Value)
            {
              if (!entityRelationshipStore.ContainsKey(entityName))
                entityRelationshipStore.Add(entityName, new List<EntityRelationship>());
              entityRelationshipStore[entityName].Add(new EntityRelationship()
              {
                ReferencedEntity = relationshipMetadata.ReferencedEntity,
                SchemaName = relationshipMetadata.SchemaName,
                ReferencingAttribute = relationshipMetadata.ReferencingAttribute
              });
            }
          }
        }
        if (entityRelationshipStore.ContainsKey(entityName))
          entityRelationshipStore[entityName]?.ForEach((Action<EntityRelationship>) (relationship =>
          {
            if (relationship == null)
              return;
            entityMetadataAndAttributesToFetch.Add(relationship.ReferencedEntity);
          }));
        foreach (string str in entityMetadataAndAttributesToFetch)
        {
          if (!entityMetadataCacheDisplayName.ContainsKey(str))
          {
            AttributeMetadata[] attributes = this.entityMetadataProvider.GetEntityMetadata(str)?.Attributes;
            entityMetadataCacheDisplayName[str] = this.entityMetadataProvider.GetEntityMetadata(str)?.DisplayName;
            if (attributes != null)
            {
              foreach (AttributeMetadata attributeToFetch in attributes)
              {
                if (!entityAttributeStore.ContainsKey(str))
                  entityAttributeStore.Add(str, new List<EntityAttributes>());
                bool valueOrDefault = attributeToFetch.IsValidForRead.GetValueOrDefault();
                bool flag1 = this.IsValidForForm(attributeToFetch);
                bool flag2 = attributeToFetch.AttributeType.GetValueOrDefault() != AttributeTypeCode.Virtual;
                if (valueOrDefault & flag2 && (flag1 || primaryNameAttribute == attributeToFetch.LogicalName))
                  entityAttributeStore[str].Add(new EntityAttributes()
                  {
                    DisplayName = attributeToFetch.DisplayName,
                    AttributeType = attributeToFetch.AttributeType,
                    LogicalName = attributeToFetch.LogicalName
                  });
              }
            }
          }
        }
      }
      return this.GetWorklistSettingsDataResponse(adminConfiguration, sellerConfiguration, entityAttributeStore, entityRelationshipStore, entityMetadataCacheDisplayName, primaryNameAttributes, isActivityMapping);
    }

    public WorklistSettingsDataResponse GetWorklistSettingsDataResponse(
      WorklistAdminConfiguration adminConfig,
      WorklistSellerConfiguration userConfig,
      Dictionary<string, List<EntityAttributes>> entityAttributeStore,
      Dictionary<string, List<EntityRelationship>> entityRelationshipStore,
      Dictionary<string, Label> entityMetadataCacheDisplayName,
      Dictionary<string, string> primaryNameAttributes,
      Dictionary<string, bool> isActivityMapping)
    {
      return new WorklistSettingsDataResponse()
      {
        AdminConfig = adminConfig,
        UserConfig = userConfig,
        EntityAttributes = entityAttributeStore,
        EntityRelationships = entityRelationshipStore,
        EntityDisplayName = entityMetadataCacheDisplayName,
        PrimaryNameAttributes = primaryNameAttributes,
        IsActivityMapping = isActivityMapping
      };
    }

    public WorklistSettingsDataResponseUpdate UpdateWorklistSettingsData(
      UpdateWorklistSettingsDataRequest requestPayload)
    {
      UpdateWorklistSettingsViewConfiguration viewConfiguration = UpdateWorklistSettingsViewConfiguration.FromRequestPayload(requestPayload, this.logger);
      if (viewConfiguration.AdminMode)
        this.UpdateWorklistAdminSettingsData(viewConfiguration.AdminConfig);
      this.UpdateWorklistUserSettingsData(viewConfiguration.UserConfig);
      return new WorklistSettingsDataResponseUpdate()
      {
        AdminConfig = this.worklistViewConfigurationDataAccess.GetAdminConfiguration(ViewType.Sequence),
        UserConfig = this.workQueueUserSettingsDataAccess.GetSellerConfiguration()
      };
    }

    public void UpdateWorklistAdminSettingsData(WorklistAdminConfiguration adminConfig)
    {
      Guid guid = adminConfig != null ? adminConfig.ViewId : new Guid();
      this.logger.AddCustomProperty("UpdateWorklistSettingsDataForAdmin.Start.viewId", (object) guid);
      if (guid == new Guid())
      {
        WorklistAdminConfiguration adminConfiguration = this.worklistViewConfigurationDataAccess.GetAdminConfiguration(ViewType.Sequence);
        guid = adminConfiguration != null ? adminConfiguration.ViewId : new Guid();
        if (guid == new Guid())
          guid = this.worklistViewConfigurationDataAccess.CreateAdminConfiguration();
      }
      if (!(guid != new Guid()) || adminConfig?.CardLayout?.Configuration == null)
        return;
      this.worklistViewConfigurationDataAccess.UpdateAdminConfiguration(adminConfig, guid);
    }

    public void UpdateWorklistUserSettingsData(WorklistSellerConfiguration userConfig)
    {
      Guid guid = userConfig != null ? userConfig.UserSettingsId : new Guid();
      this.logger.AddCustomProperty("UpdateWorklistSettingsDataForUser.Start.userSettingsId", (object) guid);
      if (guid == new Guid())
      {
        WorklistSellerConfiguration sellerConfiguration = this.workQueueUserSettingsDataAccess.GetSellerConfiguration();
        guid = sellerConfiguration != null ? sellerConfiguration.UserSettingsId : new Guid();
        if (guid == new Guid())
          guid = this.workQueueUserSettingsDataAccess.CreateSellerConfiguration();
      }
      if (!(guid != new Guid()))
        return;
      this.workQueueUserSettingsDataAccess.UpdateSellerConfiguration(userConfig, guid);
    }

    private bool IsValidForForm(AttributeMetadata attributeToFetch)
    {
      return attributeToFetch.IsValidForForm.GetValueOrDefault() || attributeToFetch.LogicalName == "activitytypecode";
    }

    private bool IsActivityEntity(EntityMetadata entityMetadata, string entityName)
    {
      return ((bool?) entityMetadata?.IsActivity).GetValueOrDefault() || entityName == "activitypointer" || entityMetadata?.PrimaryIdAttribute == "activityid";
    }
  }
}
