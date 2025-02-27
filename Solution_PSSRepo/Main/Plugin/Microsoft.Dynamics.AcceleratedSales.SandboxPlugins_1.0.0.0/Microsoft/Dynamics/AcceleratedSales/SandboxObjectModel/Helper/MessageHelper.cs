// Decompiled with JetBrains decompiler
// Type: Microsoft.Dynamics.AcceleratedSales.SandboxObjectModel.Helper.MessageHelper
// Assembly: Microsoft.Dynamics.AcceleratedSales.SandboxPlugins, Version=1.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35
// MVID: 7BBE7132-7E30-4424-B38D-27667CFBCEFA
// Assembly location: C:\Users\ngoct\Downloads\New folder\Microsoft.Dynamics.AcceleratedSales.SandboxPlugins_1.0.0.0.dll

using Microsoft.Crm.AcceleratedSales.Shared.SandboxServices.Platform;
using Microsoft.Dynamics.AcceleratedSales.SandboxObjectModel.Model;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Query;
using Newtonsoft.Json;
using System.Collections.Generic;

#nullable disable
namespace Microsoft.Dynamics.AcceleratedSales.SandboxObjectModel.Helper
{
  internal class MessageHelper
  {
    public static EntityReference GetCustomOwner(
      LocalPluginContext localcontext,
      Regarding regardingObject)
    {
      if (regardingObject != null)
      {
        SASettings saSettings = MessageHelper.RetrieveSASettings(localcontext);
        if (saSettings != null && saSettings.isEnabledForOrganization && saSettings.settingsInstance != null && saSettings.settingsInstance.entityConfigurations != null && saSettings.settingsInstance.entityConfigurations.ContainsKey(regardingObject.etn))
        {
          string customOwnerAttribute = saSettings.settingsInstance.entityConfigurations[regardingObject.etn].customOwnerAttribute;
          if (!string.IsNullOrEmpty(customOwnerAttribute) && customOwnerAttribute != "ownerid")
            return localcontext.OrganizationService.Retrieve(regardingObject.etn, regardingObject.id, new ColumnSet(new string[1]
            {
              customOwnerAttribute
            })).GetAttributeValue<EntityReference>(customOwnerAttribute);
        }
      }
      return (EntityReference) null;
    }

    public static SASettings RetrieveSASettings(LocalPluginContext localcontext)
    {
      QueryExpression queryExpression = new QueryExpression();
      queryExpression.EntityName = "msdyn_salesaccelerationsettings";
      queryExpression.ColumnSet = new ColumnSet(new string[3]
      {
        "msdyn_entityconfiguration",
        "msdyn_linksequencesteptoactivity",
        "msdyn_isautocreatephonecallenabled"
      });
      queryExpression.Criteria.Conditions.Add(new ConditionExpression("statecode", ConditionOperator.Equal, (object) 1));
      queryExpression.Criteria.Conditions.Add(new ConditionExpression("statuscode", ConditionOperator.Equal, (object) 2));
      QueryExpression query = queryExpression;
      EntityCollection entityCollection = localcontext.SystemUserOrganizationService.RetrieveMultiple((QueryBase) query);
      SASettings saSettings;
      if (entityCollection != null && entityCollection.Entities != null && entityCollection.Entities.Count > 0)
      {
        Entity entity = entityCollection.Entities[0];
        string attributeValue = entity.GetAttributeValue<string>("msdyn_entityconfiguration");
        SettingsInstance settingsInstance = new SettingsInstance()
        {
          entityConfigurations = attributeValue == null ? (Dictionary<string, EntityConfiguration>) null : JsonConvert.DeserializeObject<Dictionary<string, EntityConfiguration>>(attributeValue),
          isAutoCreatePhoneCallEnabled = entity.GetAttributeValue<bool>("msdyn_isautocreatephonecallenabled"),
          shouldLinkSequenceStepToActivity = entity.GetAttributeValue<bool>("msdyn_linksequencesteptoactivity")
        };
        saSettings = new SASettings()
        {
          isEnabledForOrganization = true,
          settingsInstance = settingsInstance
        };
      }
      else
        saSettings = new SASettings()
        {
          settingsInstance = (SettingsInstance) null,
          isEnabledForOrganization = false
        };
      return saSettings;
    }
  }
}
