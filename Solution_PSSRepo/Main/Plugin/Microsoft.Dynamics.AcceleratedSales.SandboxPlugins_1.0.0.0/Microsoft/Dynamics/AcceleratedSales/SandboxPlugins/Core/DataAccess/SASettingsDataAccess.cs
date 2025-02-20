// Decompiled with JetBrains decompiler
// Type: Microsoft.Dynamics.AcceleratedSales.SandboxPlugins.Core.DataAccess.SASettingsDataAccess
// Assembly: Microsoft.Dynamics.AcceleratedSales.SandboxPlugins, Version=1.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35
// MVID: 7BBE7132-7E30-4424-B38D-27667CFBCEFA
// Assembly location: C:\Users\ngoct\Downloads\New folder\Microsoft.Dynamics.AcceleratedSales.SandboxPlugins_1.0.0.0.dll

using Microsoft.Dynamics.AcceleratedSales.SandboxObjectModel.Model;
using Microsoft.Dynamics.AcceleratedSales.SandboxPlugins.Core.Model;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Query;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.ServiceModel;

#nullable disable
namespace Microsoft.Dynamics.AcceleratedSales.SandboxPlugins.Core.DataAccess
{
  public class SASettingsDataAccess
  {
    private readonly IDataStore dataStore;
    private readonly IAcceleratedSalesLogger logger;

    public SASettingsDataAccess(IDataStore dataStore, IAcceleratedSalesLogger logger)
    {
      this.dataStore = dataStore;
      this.logger = logger;
    }

    public static string GetWQUserSettingsFetchXml()
    {
      return "<fetch mapping='logical'>\r\n\t\t\t\t\t<entity name='msdyn_workqueueusersetting'>\r\n\t\t\t\t\t\t<attribute name='msdyn_actiononmarkcomplete'/>\r\n\t\t\t\t\t\t<attribute name='msdyn_actiononskip'/>\r\n\t\t\t\t\t\t<attribute name='msdyn_workqueueusersettingid'/>\r\n\t\t\t\t\t\t<attribute name='msdyn_linkingconfiguration'/>\r\n\t\t\t\t\t\t<filter type='and'>\r\n\t\t\t\t\t\t\t<condition attribute = 'ownerid' operator='eq-userid' />\r\n\t\t\t\t\t\t</filter>\r\n\t\t\t\t\t</entity>\r\n\t\t\t\t</fetch>";
    }

    public virtual SASettings RetrieveSASettings()
    {
      Stopwatch stopwatch = Stopwatch.StartNew();
      try
      {
        QueryExpression query = new QueryExpression()
        {
          EntityName = "msdyn_salesaccelerationsettings",
          ColumnSet = new ColumnSet(new string[16]
          {
            "msdyn_salesaccelerationsettingsid",
            "msdyn_entityconfiguration",
            "msdyn_linksequencesteptoactivity",
            "msdyn_isautocreatephonecallenabled",
            "msdyn_isfccenabled",
            "msdyn_calendartype",
            "msdyn_disablewqautorefreshonmarkcomplete",
            "msdyn_migrationstatus",
            "msdyn_isworkscheduleenabled",
            "msdyn_securityroles",
            "msdyn_securityrolesnew",
            "msdyn_recommendationsecurityroles",
            "msdyn_linkingconfiguration",
            "statecode",
            "statuscode",
            "msdyn_isdefaultsetting"
          })
        };
        EntityCollection entityCollection = this.dataStore.Elevate().RetrieveMultiple(query);
        this.logger.LogWarning(string.Format("SASettingsDataAccess.RetrieveSASettings.Entities.Count: {0}", (object) entityCollection?.Entities?.Count), callerName: nameof (RetrieveSASettings), callerFile: "C:\\__w\\1\\s\\solutions\\AcceleratedSales\\AcceleratedSalesSandboxPlugins\\Core\\DataAccess\\SASettingsDataAccess.cs");
        int? nullable1;
        int num1;
        if (entityCollection == null)
        {
          num1 = 0;
        }
        else
        {
          nullable1 = entityCollection.Entities?.Count;
          int num2 = 0;
          num1 = nullable1.GetValueOrDefault() > num2 & nullable1.HasValue ? 1 : 0;
        }
        SASettings saSettings;
        if (num1 != 0)
        {
          Entity entity = entityCollection.Entities[0];
          OptionSetValue attributeValue1 = entity.GetAttributeValue<OptionSetValue>("statecode");
          OptionSetValue attributeValue2 = entity.GetAttributeValue<OptionSetValue>("statuscode");
          bool flag = false;
          if (attributeValue1 != null && attributeValue1.Value == 1 && attributeValue2 != null && attributeValue2.Value == 2)
            flag = true;
          string attributeValue3 = entity.GetAttributeValue<string>("msdyn_entityconfiguration");
          string attributeValue4 = entity.GetAttributeValue<string>("msdyn_migrationstatus");
          string attributeValue5 = entity.GetAttributeValue<string>("msdyn_securityroles");
          string attributeValue6 = entity.GetAttributeValue<string>("msdyn_securityrolesnew");
          string attributeValue7 = entity.GetAttributeValue<string>("msdyn_recommendationsecurityroles");
          string attributeValue8 = entity.GetAttributeValue<string>("msdyn_linkingconfiguration");
          SettingsInstance settingsInstance1 = new SettingsInstance();
          settingsInstance1.settingsId = entity.GetAttributeValue<Guid>("msdyn_salesaccelerationsettingsid");
          settingsInstance1.entityConfigurations = attributeValue3 == null ? (Dictionary<string, EntityConfiguration>) null : this.DeserializeJsonOrDefault<Dictionary<string, EntityConfiguration>>(attributeValue3);
          settingsInstance1.isAutoCreatePhoneCallEnabled = entity.GetAttributeValue<bool>("msdyn_isautocreatephonecallenabled");
          settingsInstance1.shouldLinkSequenceStepToActivity = entity.GetAttributeValue<bool>("msdyn_linksequencesteptoactivity");
          settingsInstance1.isFCCEnabled = entity.GetAttributeValue<bool>("msdyn_isfccenabled");
          settingsInstance1.migrationStatus = attributeValue4 == null ? (MigrationStatus) null : JsonConvert.DeserializeObject<MigrationStatus>(attributeValue4);
          OptionSetValue attributeValue9 = entity.GetAttributeValue<OptionSetValue>("msdyn_calendartype");
          int? nullable2;
          if (attributeValue9 == null)
          {
            nullable1 = new int?();
            nullable2 = nullable1;
          }
          else
            nullable2 = new int?(attributeValue9.Value);
          settingsInstance1.calendarType = nullable2;
          settingsInstance1.isWorkScheduleEnabled = entity.GetAttributeValue<bool>("msdyn_isworkscheduleenabled");
          settingsInstance1.securityRoles = attributeValue5 == null ? new List<string>() : JsonConvert.DeserializeObject<List<string>>(attributeValue5);
          settingsInstance1.securityRolesNew = attributeValue6 == null ? new List<string>() : JsonConvert.DeserializeObject<List<string>>(attributeValue6);
          settingsInstance1.recommendationSecurityRoles = attributeValue7 == null ? new List<string>() : JsonConvert.DeserializeObject<List<string>>(attributeValue7);
          settingsInstance1.linkingConfiguration = attributeValue8 == null ? (AdminLinkingConfiguration) null : JsonConvert.DeserializeObject<AdminLinkingConfiguration>(attributeValue8);
          settingsInstance1.isDefaultSetting = entity.GetAttributeValue<bool>("msdyn_isdefaultsetting");
          SettingsInstance settingsInstance2 = settingsInstance1;
          saSettings = new SASettings()
          {
            isEnabledForOrganization = flag,
            settingsInstance = settingsInstance2
          };
        }
        else
          saSettings = new SASettings()
          {
            settingsInstance = (SettingsInstance) null,
            isEnabledForOrganization = false
          };
        stopwatch.Stop();
        this.logger.LogWarning(string.Format("RetrieveSASettings.End.Duration: {0}", (object) stopwatch.ElapsedMilliseconds), callerName: nameof (RetrieveSASettings), callerFile: "C:\\__w\\1\\s\\solutions\\AcceleratedSales\\AcceleratedSalesSandboxPlugins\\Core\\DataAccess\\SASettingsDataAccess.cs");
        return saSettings;
      }
      catch (Exception ex)
      {
        stopwatch.Stop();
        this.logger.LogError("SASettingsDataAccess.RetrieveSASettings.Exception", ex, callerName: nameof (RetrieveSASettings), callerFile: "C:\\__w\\1\\s\\solutions\\AcceleratedSales\\AcceleratedSalesSandboxPlugins\\Core\\DataAccess\\SASettingsDataAccess.cs");
        this.logger.LogWarning(string.Format("SASettingsDataAccess.RetrieveSASettings.Duration: {0}", (object) stopwatch.ElapsedMilliseconds), callerName: nameof (RetrieveSASettings), callerFile: "C:\\__w\\1\\s\\solutions\\AcceleratedSales\\AcceleratedSalesSandboxPlugins\\Core\\DataAccess\\SASettingsDataAccess.cs");
        throw;
      }
    }

    public virtual WorkQueueUserSettings RetrieveWQUserSettings()
    {
      Stopwatch stopwatch = Stopwatch.StartNew();
      try
      {
        EntityCollection entityCollection = this.dataStore.RetrieveMultiple(this.dataStore.ConvertFetchXmlToQueryExpression(SASettingsDataAccess.GetWQUserSettingsFetchXml(), this.logger));
        DataCollection<Entity> entities = entityCollection.Entities;
        // ISSUE: explicit non-virtual call
        Entity entity = (entities != null ? (__nonvirtual (entities.Count) > 0 ? 1 : 0) : 0) != 0 ? entityCollection.Entities[0] : (Entity) null;
        if (entity != null)
          return new WorkQueueUserSettings()
          {
            WorkQueueAutoAdvanceSettings = new WorkQueueAutoAdvanceSettings()
            {
              ActionOnMarkComplete = entity.GetAttributeValue<OptionSetValue>("msdyn_actiononmarkcomplete")?.Value,
              ActionOnSkip = entity.GetAttributeValue<OptionSetValue>("msdyn_actiononskip")?.Value
            },
            WorkQueueSettingsId = entity.GetAttributeValue<Guid>("msdyn_workqueueusersettingid"),
            LinkingConfigurations = entity.GetAttributeValue<string>("msdyn_linkingconfiguration")
          };
        stopwatch.Stop();
        this.logger.LogWarning(string.Format("RetrieveWQUserSettings.Success.Duration: {0}", (object) stopwatch.ElapsedMilliseconds), callerName: nameof (RetrieveWQUserSettings), callerFile: "C:\\__w\\1\\s\\solutions\\AcceleratedSales\\AcceleratedSalesSandboxPlugins\\Core\\DataAccess\\SASettingsDataAccess.cs");
        return new WorkQueueUserSettings();
      }
      catch (FaultException<OrganizationServiceFault> ex)
      {
        stopwatch.Stop();
        this.logger.LogWarning(string.Format("SASettingsDataAccess.RetrieveWQUserSettings.Failure.Duration: {0}", (object) stopwatch.ElapsedMilliseconds), callerName: nameof (RetrieveWQUserSettings), callerFile: "C:\\__w\\1\\s\\solutions\\AcceleratedSales\\AcceleratedSalesSandboxPlugins\\Core\\DataAccess\\SASettingsDataAccess.cs");
        if (ex.Detail.ErrorCode == -2147220960)
        {
          this.logger.LogWarning(string.Format("SASettingsDataAccess.RetrieveWQUserSettings.Exception.ErrorCode, {0}", (object) ex.Detail.ErrorCode), callerName: nameof (RetrieveWQUserSettings), callerFile: "C:\\__w\\1\\s\\solutions\\AcceleratedSales\\AcceleratedSalesSandboxPlugins\\Core\\DataAccess\\SASettingsDataAccess.cs");
          return new WorkQueueUserSettings();
        }
        this.logger.LogError("SASettingsDataAccess.RetrieveWQUserSettings.Exception", (Exception) ex, callerName: nameof (RetrieveWQUserSettings), callerFile: "C:\\__w\\1\\s\\solutions\\AcceleratedSales\\AcceleratedSalesSandboxPlugins\\Core\\DataAccess\\SASettingsDataAccess.cs");
        throw;
      }
      catch (Exception ex)
      {
        stopwatch.Stop();
        this.logger.LogError("SASettingsDataAccess.RetrieveWQUserSettings.Exception.CatchAll", ex, callerName: nameof (RetrieveWQUserSettings), callerFile: "C:\\__w\\1\\s\\solutions\\AcceleratedSales\\AcceleratedSalesSandboxPlugins\\Core\\DataAccess\\SASettingsDataAccess.cs");
        this.logger.LogWarning(string.Format("SASettingsDataAccess.RetrieveWQUserSettings.Failure.CatchAll.Duration: {0}", (object) stopwatch.ElapsedMilliseconds), callerName: nameof (RetrieveWQUserSettings), callerFile: "C:\\__w\\1\\s\\solutions\\AcceleratedSales\\AcceleratedSalesSandboxPlugins\\Core\\DataAccess\\SASettingsDataAccess.cs");
        throw;
      }
    }

    private T DeserializeJsonOrDefault<T>(string input)
    {
      try
      {
        return JsonConvert.DeserializeObject<T>(input);
      }
      catch (Exception ex)
      {
        this.logger.LogError("SASettingsDataAccess.DeserializeJsonOrDefault.Exception", ex, callerName: nameof (DeserializeJsonOrDefault), callerFile: "C:\\__w\\1\\s\\solutions\\AcceleratedSales\\AcceleratedSalesSandboxPlugins\\Core\\DataAccess\\SASettingsDataAccess.cs");
        return default (T);
      }
    }
  }
}
