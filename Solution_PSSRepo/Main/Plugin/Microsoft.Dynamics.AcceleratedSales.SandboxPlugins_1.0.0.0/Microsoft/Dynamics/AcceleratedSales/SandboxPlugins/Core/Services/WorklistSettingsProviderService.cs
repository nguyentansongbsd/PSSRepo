// Decompiled with JetBrains decompiler
// Type: Microsoft.Dynamics.AcceleratedSales.SandboxPlugins.Core.Services.WorklistSettingsProviderService
// Assembly: Microsoft.Dynamics.AcceleratedSales.SandboxPlugins, Version=1.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35
// MVID: 7BBE7132-7E30-4424-B38D-27667CFBCEFA
// Assembly location: C:\Users\ngoct\Downloads\New folder\Microsoft.Dynamics.AcceleratedSales.SandboxPlugins_1.0.0.0.dll

using Microsoft.Dynamics.AcceleratedSales.SandboxObjectModel.Model;
using Microsoft.Dynamics.AcceleratedSales.SandboxPlugins.Core.DataAccess;
using Microsoft.Dynamics.AcceleratedSales.SandboxPlugins.Core.Model;
using Microsoft.Xrm.Sdk;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

#nullable disable
namespace Microsoft.Dynamics.AcceleratedSales.SandboxPlugins.Core.Services
{
  public class WorklistSettingsProviderService
  {
    private const string DefaultAppConfig = "{\"settingsInstance\":{\"entityConfigurations\":{\"lead\":{\"IsEnabled\":true,\"phoneNumberSettings\":{\"defaultPhone\":{\"key\":\"telephone1\"},\"fallback1\":{\"key\":\"mobilephone\"},\"fallback2\":{\"key\":\"telephone2\"}}},\"opportunity\":{\"IsEnabled\":true,\"phoneNumberSettings\":{\"defaultPhone\":{\"expand\":\"parentcontact\",\"key\":\"telephone1\"},\"fallback1\":{\"expand\":\"parentcontact\",\"key\":\"mobilephone\"},\"fallback2\":{\"expand\":\"parentcontact\",\"key\":\"telephone2\"}}},\"account\":{\"IsEnabled\":true,\"phoneNumberSettings\":{\"defaultPhone\":{\"expand\":\"primarycontact\",\"key\":\"telephone1\"},\"fallback1\":{\"expand\":\"primarycontact\",\"key\":\"mobilephone\"},\"fallback2\":{\"expand\":\"primarycontact\",\"key\":\"telephone2\"}}},\"contact\":{\"IsEnabled\":true,\"phoneNumberSettings\":{\"defaultPhone\":{\"expand\":\"parentcontact\",\"key\":\"telephone1\"},\"fallback1\":{\"expand\":\"parentcontact\",\"key\":\"mobilephone\"},\"fallback2\":{\"expand\":\"parentcontact\",\"key\":\"telephone2\"}}}},\"isAutoCreatePhoneCallEnabled\":false,\"shouldLinkSequenceStepToActivity\":true,\"isOOBSalesPerson\":false, \"securityroles\": [], \"securityrolesnew\": [], \"recommendationsecurityroles\": [] },\"isEnabledForOrganization\":false}";
    private IDataStore dataStore;
    private IAcceleratedSalesLogger logger;

    public WorklistSettingsProviderService(IDataStore dataStore, IAcceleratedSalesLogger logger)
    {
      this.dataStore = dataStore;
      this.logger = logger;
    }

    public SASettings GetSASettings(WorklistViewConfiguration viewConfiguration)
    {
      Stopwatch stopwatch = Stopwatch.StartNew();
      SASettings defaultSaSettings = this.GetDefaultSASettings();
      SASettings saSettings = defaultSaSettings;
      try
      {
        SASettings sASettings = new SASettings();
        if (this.TryGetSASettings(viewConfiguration, out sASettings))
          saSettings = sASettings?.settingsInstance != null ? sASettings : saSettings;
        if (viewConfiguration.IsAdminUserCheckNeeded)
        {
          bool flag1 = this.CheckUserIsSystemAdminOrNot();
          bool flag2 = !flag1 && !sASettings.isEnabledForOrganization && (sASettings.settingsInstance == null || sASettings.settingsInstance.isDefaultSetting);
          if (flag2)
            saSettings = defaultSaSettings;
          saSettings.isAdminUser = flag1;
          saSettings.isViralTrial = flag2;
          saSettings.isEnabledForOrganization = flag1 || sASettings.isEnabledForOrganization;
        }
      }
      catch (Exception ex)
      {
        this.logger.LogError("WorklistSettingsProviderService: GetSASettings, SA Settings retrieve failed.Exception", ex, callerName: nameof (GetSASettings), callerFile: "C:\\__w\\1\\s\\solutions\\AcceleratedSales\\AcceleratedSalesSandboxPlugins\\Core\\Services\\WorklistSettingsProviderService.cs");
      }
      finally
      {
        stopwatch.Stop();
      }
      return saSettings;
    }

    public SASettings GetDefaultSASettings()
    {
      return JsonConvert.DeserializeObject<SASettings>("{\"settingsInstance\":{\"entityConfigurations\":{\"lead\":{\"IsEnabled\":true,\"phoneNumberSettings\":{\"defaultPhone\":{\"key\":\"telephone1\"},\"fallback1\":{\"key\":\"mobilephone\"},\"fallback2\":{\"key\":\"telephone2\"}}},\"opportunity\":{\"IsEnabled\":true,\"phoneNumberSettings\":{\"defaultPhone\":{\"expand\":\"parentcontact\",\"key\":\"telephone1\"},\"fallback1\":{\"expand\":\"parentcontact\",\"key\":\"mobilephone\"},\"fallback2\":{\"expand\":\"parentcontact\",\"key\":\"telephone2\"}}},\"account\":{\"IsEnabled\":true,\"phoneNumberSettings\":{\"defaultPhone\":{\"expand\":\"primarycontact\",\"key\":\"telephone1\"},\"fallback1\":{\"expand\":\"primarycontact\",\"key\":\"mobilephone\"},\"fallback2\":{\"expand\":\"primarycontact\",\"key\":\"telephone2\"}}},\"contact\":{\"IsEnabled\":true,\"phoneNumberSettings\":{\"defaultPhone\":{\"expand\":\"parentcontact\",\"key\":\"telephone1\"},\"fallback1\":{\"expand\":\"parentcontact\",\"key\":\"mobilephone\"},\"fallback2\":{\"expand\":\"parentcontact\",\"key\":\"telephone2\"}}}},\"isAutoCreatePhoneCallEnabled\":false,\"shouldLinkSequenceStepToActivity\":true,\"isOOBSalesPerson\":false, \"securityroles\": [], \"securityrolesnew\": [], \"recommendationsecurityroles\": [] },\"isEnabledForOrganization\":false}");
    }

    public bool TryGetSASettings(
      WorklistViewConfiguration viewConfiguration,
      out SASettings sASettings)
    {
      Stopwatch stopwatch = Stopwatch.StartNew();
      sASettings = new SASettings();
      try
      {
        SASettingsDataAccess settingsDataAccess = new SASettingsDataAccess(this.dataStore, this.logger);
        sASettings = settingsDataAccess.RetrieveSASettings();
        return true;
      }
      catch (Exception ex)
      {
        this.logger.LogError("WorklistSettingsProviderService: TryGetSASettings, SA Settings retrieve failed.Exception", ex, callerName: nameof (TryGetSASettings), callerFile: "C:\\__w\\1\\s\\solutions\\AcceleratedSales\\AcceleratedSalesSandboxPlugins\\Core\\Services\\WorklistSettingsProviderService.cs");
      }
      finally
      {
        stopwatch.Stop();
      }
      return false;
    }

    public virtual List<ViewType> TryGetUserAccessibleView(SASettings settings)
    {
      List<ViewType> userAccessibleView = new List<ViewType>();
      if (settings == null || settings.settingsInstance == null)
        return userAccessibleView;
      Stopwatch stopwatch = Stopwatch.StartNew();
      try
      {
        SecurityRolesDataAccess securityRolesDataAccess = new SecurityRolesDataAccess(this.dataStore, this.logger);
        EntityCollection entityCollection1 = securityRolesDataAccess.FetchRolesForCurrentUser();
        this.logger.LogWarning("WorklistSettingsProviderService.TryGetUserAccessibleView.UserRoles.Count: " + entityCollection1?.Entities?.Count.ToString(), callerName: nameof (TryGetUserAccessibleView), callerFile: "C:\\__w\\1\\s\\solutions\\AcceleratedSales\\AcceleratedSalesSandboxPlugins\\Core\\Services\\WorklistSettingsProviderService.cs");
        List<Guid> userRoleList = new List<Guid>();
        if (entityCollection1 != null)
        {
          DataCollection<Entity> entities = entityCollection1.Entities;
          if (entities != null)
            entities.ToList<Entity>().ForEach((Action<Entity>) (userRole =>
            {
              EntityReference entityReference = (EntityReference) null;
              if (userRole.Contains("parentrootroleid"))
                entityReference = userRole.GetAttributeValue<EntityReference>("parentrootroleid");
              userRoleList.Add(entityReference != null ? entityReference.Id : userRole.GetAttributeValue<Guid>("roleid"));
            }));
        }
        bool flag1 = this.CheckIfUserHasARoleFromList(userRoleList, settings.settingsInstance.securityRolesNew);
        bool flag2 = this.CheckIfUserHasARoleFromList(userRoleList, settings.settingsInstance.recommendationSecurityRoles);
        this.logger.LogWarning(string.Format("WorklistSettingsProviderService.TryGetUserAccessibleView UserHasSequenceRole: {0} userHasSuggestionRole: {1}", (object) flag1, (object) flag2), callerName: nameof (TryGetUserAccessibleView), callerFile: "C:\\__w\\1\\s\\solutions\\AcceleratedSales\\AcceleratedSalesSandboxPlugins\\Core\\Services\\WorklistSettingsProviderService.cs");
        if (flag1 & flag2)
          return new List<ViewType>()
          {
            ViewType.Sequence,
            ViewType.Suggestion
          };
        EntityCollection entityCollection2 = securityRolesDataAccess.FetchTeamRolesForCurrentUser();
        if (entityCollection2 != null)
        {
          DataCollection<Entity> entities = entityCollection2.Entities;
          if (entities != null)
            entities.ToList<Entity>().ForEach((Action<Entity>) (userRole =>
            {
              EntityReference entityReference = (EntityReference) null;
              if (userRole.Contains("parentrootroleid"))
                entityReference = userRole.GetAttributeValue<EntityReference>("parentrootroleid");
              userRoleList.Add(entityReference != null ? entityReference.Id : userRole.GetAttributeValue<Guid>("roleid"));
            }));
        }
        bool flag3 = this.CheckIfUserHasARoleFromList(userRoleList, settings.settingsInstance.securityRolesNew);
        bool flag4 = this.CheckIfUserHasARoleFromList(userRoleList, settings.settingsInstance.recommendationSecurityRoles);
        this.logger.LogWarning(string.Format("WorklistSettingsProviderService.TryGetUserAccessibleView.Teams UserHasSequenceRole: {0} userHasSuggestionRole: {1}", (object) flag3, (object) flag4), callerName: nameof (TryGetUserAccessibleView), callerFile: "C:\\__w\\1\\s\\solutions\\AcceleratedSales\\AcceleratedSalesSandboxPlugins\\Core\\Services\\WorklistSettingsProviderService.cs");
        if (flag3)
          userAccessibleView.Add(ViewType.Sequence);
        if (flag4)
          userAccessibleView.Add(ViewType.Suggestion);
      }
      catch (Exception ex)
      {
        this.logger.LogError("WorklistSettingsProviderService.TryGetUserAccessibleView.Exception", ex, callerName: nameof (TryGetUserAccessibleView), callerFile: "C:\\__w\\1\\s\\solutions\\AcceleratedSales\\AcceleratedSalesSandboxPlugins\\Core\\Services\\WorklistSettingsProviderService.cs");
      }
      stopwatch.Stop();
      this.logger.LogWarning("WorklistSettingsProviderService.TryGetSASettings.Duration: " + stopwatch.ElapsedMilliseconds.ToString(), callerName: nameof (TryGetUserAccessibleView), callerFile: "C:\\__w\\1\\s\\solutions\\AcceleratedSales\\AcceleratedSalesSandboxPlugins\\Core\\Services\\WorklistSettingsProviderService.cs");
      return userAccessibleView;
    }

    private bool CheckUserIsSystemAdminOrNot()
    {
      Stopwatch stopwatch = Stopwatch.StartNew();
      bool flag = false;
      try
      {
        flag = new SecurityRolesDataAccess(this.dataStore, this.logger).TryCheckUserIsSystemAdminOrNot();
        this.logger.LogWarning("WorklistSettingsProviderService.CheckUserIsSystemAdminOrNot.isAdminUser: " + flag.ToString(), callerName: nameof (CheckUserIsSystemAdminOrNot), callerFile: "C:\\__w\\1\\s\\solutions\\AcceleratedSales\\AcceleratedSalesSandboxPlugins\\Core\\Services\\WorklistSettingsProviderService.cs");
      }
      catch (Exception ex)
      {
        this.logger.LogError("WorklistSettingsProviderService.CheckUserIsSystemAdminOrNot.Exception", ex, callerName: nameof (CheckUserIsSystemAdminOrNot), callerFile: "C:\\__w\\1\\s\\solutions\\AcceleratedSales\\AcceleratedSalesSandboxPlugins\\Core\\Services\\WorklistSettingsProviderService.cs");
      }
      stopwatch.Stop();
      this.logger.LogWarning("WorklistSettingsProviderService.CheckUserIsSystemAdminOrNot.Duration: " + stopwatch.ElapsedMilliseconds.ToString(), callerName: nameof (CheckUserIsSystemAdminOrNot), callerFile: "C:\\__w\\1\\s\\solutions\\AcceleratedSales\\AcceleratedSalesSandboxPlugins\\Core\\Services\\WorklistSettingsProviderService.cs");
      return flag;
    }

    private bool CheckIfUserHasARoleFromList(List<Guid> userRoles, List<string> requiredRoles)
    {
      int num;
      // ISSUE: explicit non-virtual call
      if (userRoles != null && __nonvirtual (userRoles.Count) > 0)
      {
        List<string> stringList = requiredRoles;
        // ISSUE: explicit non-virtual call
        if ((stringList != null ? (__nonvirtual (stringList.Count) > 0 ? 1 : 0) : 0) != 0)
        {
          num = userRoles.Exists((Predicate<Guid>) (userRole => requiredRoles.Contains(userRole.ToString()))) ? 1 : 0;
          goto label_4;
        }
      }
      num = 0;
label_4:
      return num != 0;
    }
  }
}
