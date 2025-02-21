// Decompiled with JetBrains decompiler
// Type: Microsoft.Dynamics.AcceleratedSales.SandboxPlugins.Core.DataAccess.WorklistViewConfigurationDataAccess
// Assembly: Microsoft.Dynamics.AcceleratedSales.SandboxPlugins, Version=1.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35
// MVID: 7BBE7132-7E30-4424-B38D-27667CFBCEFA
// Assembly location: C:\Users\ngoct\Downloads\New folder\Microsoft.Dynamics.AcceleratedSales.SandboxPlugins_1.0.0.0.dll

using Microsoft.Dynamics.AcceleratedSales.SandboxPlugins.Core.Model;
using Microsoft.Dynamics.AcceleratedSales.SandboxPlugins.Core.Model.DefaultSort;
using Microsoft.Dynamics.AcceleratedSales.SandboxPlugins.Core.Model.Filter;
using Microsoft.Dynamics.AcceleratedSales.SandboxPlugins.Core.Model.Layout;
using Microsoft.Dynamics.AcceleratedSales.SandboxPlugins.Core.Model.Sort;
using Microsoft.Dynamics.AcceleratedSales.SandboxPlugins.Core.Model.Tags;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Query;
using Newtonsoft.Json;
using System;
using System.Diagnostics;

#nullable disable
namespace Microsoft.Dynamics.AcceleratedSales.SandboxPlugins.Core.DataAccess
{
  public class WorklistViewConfigurationDataAccess
  {
    private const string WorklistConfigEntityName = "msdyn_worklistviewconfiguration";
    private const string ConfigViewType = "msdyn_viewtype";
    private const string AdminDesignerLayout = "msdyn_cardlayout";
    private const string AdminFilterConfig = "msdyn_filterconfiguration";
    private const string AdminSortConfig = "msdyn_adminsortconfiguration";
    private const string TagsConfig = "msdyn_tagsconfiguration";
    private const string DefaultSortConfig = "msdyn_defaultsortconfiguration";
    private const string AutoRefreshEnable = "msdyn_autorefreshenable";
    private const string AutoRefeshInterval = "msdyn_autorefreshinterval";
    private readonly IDataStore dataStore;
    private readonly IAcceleratedSalesLogger logger;

    public WorklistViewConfigurationDataAccess(IDataStore dataStore, IAcceleratedSalesLogger logger)
    {
      this.dataStore = dataStore;
      this.logger = logger;
    }

    public WorklistAdminConfiguration GetAdminConfiguration(ViewType viewType)
    {
      Stopwatch stopwatch = Stopwatch.StartNew();
      QueryExpression query = new QueryExpression();
      query.EntityName = "msdyn_worklistviewconfiguration";
      query.ColumnSet = new ColumnSet(new string[8]
      {
        "msdyn_viewtype",
        "msdyn_cardlayout",
        "msdyn_filterconfiguration",
        "msdyn_adminsortconfiguration",
        "msdyn_tagsconfiguration",
        "msdyn_defaultsortconfiguration",
        "msdyn_autorefreshenable",
        "msdyn_autorefreshinterval"
      });
      FilterExpression childFilter = new FilterExpression(LogicalOperator.Or);
      childFilter.AddCondition("msdyn_viewtype", ConditionOperator.Equal, (object) (int) viewType);
      query.Criteria.AddFilter(childFilter);
      query.Criteria.AddCondition("statecode", ConditionOperator.Equal, (object) 1);
      query.Criteria.AddCondition("statuscode", ConditionOperator.Equal, (object) 2);
      query.AddOrder("modifiedon", OrderType.Descending);
      WorklistAdminConfiguration adminConfiguration = new WorklistAdminConfiguration();
      try
      {
        EntityCollection entityCollection = this.dataStore.Elevate().RetrieveMultiple(query);
        this.logger.LogWarning("GetAdminConfiguration.Entities.Count: " + entityCollection?.Entities?.Count.ToString(), callerName: nameof (GetAdminConfiguration), callerFile: "C:\\__w\\1\\s\\solutions\\AcceleratedSales\\AcceleratedSalesSandboxPlugins\\Core\\DataAccess\\WorklistViewConfigurationDataAccess.cs");
        this.logger.LogWarning("GetAdminConfiguration.Duration: " + stopwatch.ElapsedMilliseconds.ToString(), callerName: nameof (GetAdminConfiguration), callerFile: "C:\\__w\\1\\s\\solutions\\AcceleratedSales\\AcceleratedSalesSandboxPlugins\\Core\\DataAccess\\WorklistViewConfigurationDataAccess.cs");
        adminConfiguration.AutoRefreshInterval = 15;
        adminConfiguration.AutoRefreshEnable = 1;
        int num1;
        if (entityCollection == null)
        {
          num1 = 0;
        }
        else
        {
          int? count = entityCollection.Entities?.Count;
          int num2 = 0;
          num1 = count.GetValueOrDefault() > num2 & count.HasValue ? 1 : 0;
        }
        if (num1 != 0)
        {
          string attributeValue1 = entityCollection.Entities[0].TryGetAttributeValue<string>("msdyn_cardlayout", string.Empty);
          string attributeValue2 = entityCollection.Entities[0].TryGetAttributeValue<string>("msdyn_filterconfiguration", string.Empty);
          string attributeValue3 = entityCollection.Entities[0].TryGetAttributeValue<string>("msdyn_adminsortconfiguration", string.Empty);
          string attributeValue4 = entityCollection.Entities[0].TryGetAttributeValue<string>("msdyn_tagsconfiguration", string.Empty);
          string attributeValue5 = entityCollection.Entities[0].TryGetAttributeValue<string>("msdyn_defaultsortconfiguration", string.Empty);
          adminConfiguration.AutoRefreshEnable = entityCollection.Entities[0].TryGetAttributeValue<int>("msdyn_autorefreshenable", 1);
          adminConfiguration.AutoRefreshInterval = entityCollection.Entities[0].TryGetAttributeValue<int>("msdyn_autorefreshinterval", 15);
          adminConfiguration.ViewId = entityCollection.Entities[0].Id;
          if (!string.IsNullOrEmpty(attributeValue1))
            adminConfiguration.CardLayout = JsonConvert.DeserializeObject<AdminDesignerConfiguration>(attributeValue1);
          if (!string.IsNullOrEmpty(attributeValue2))
            adminConfiguration.FilterConfiguration = JsonConvert.DeserializeObject<FilterConfiguration>(attributeValue2);
          if (!string.IsNullOrEmpty(attributeValue3))
            adminConfiguration.SortConfiguration = JsonConvert.DeserializeObject<SortConfiguration>(attributeValue3);
          if (!string.IsNullOrEmpty(attributeValue4))
            adminConfiguration.TagsConfiguration = JsonConvert.DeserializeObject<TagsConfiguration>(attributeValue4);
          if (!string.IsNullOrEmpty(attributeValue5))
            adminConfiguration.DefaultSortConfiguration = JsonConvert.DeserializeObject<DefaultSortConfiguration>(attributeValue5);
        }
      }
      catch (Exception ex)
      {
        this.logger.LogError("GetAdminConfiguration.Exception", ex, callerName: nameof (GetAdminConfiguration), callerFile: "C:\\__w\\1\\s\\solutions\\AcceleratedSales\\AcceleratedSalesSandboxPlugins\\Core\\DataAccess\\WorklistViewConfigurationDataAccess.cs");
      }
      return adminConfiguration;
    }

    public void UpdateAdminConfiguration(WorklistAdminConfiguration adminConfig, Guid viewId)
    {
      try
      {
        this.logger.Execute("AcceleratedSales.UpdateWorklistSettingsData.Logging", (Action) (() => this.logger.AddCustomProperty("UpdateAdminConfiguration.CardLayout", (object) adminConfig.CardLayout)));
        Entity entity = new Entity("msdyn_worklistviewconfiguration", viewId);
        entity["msdyn_cardlayout"] = (object) JsonConvert.SerializeObject((object) adminConfig.CardLayout);
        if (adminConfig.AutoRefreshInterval != 0)
        {
          entity["msdyn_autorefreshenable"] = (object) adminConfig.AutoRefreshEnable;
          entity["msdyn_autorefreshinterval"] = (object) adminConfig.AutoRefreshInterval;
        }
        this.dataStore.Elevate().Update(entity);
        this.logger.LogWarning("UpdateAdminConfiguration.End", callerName: nameof (UpdateAdminConfiguration), callerFile: "C:\\__w\\1\\s\\solutions\\AcceleratedSales\\AcceleratedSalesSandboxPlugins\\Core\\DataAccess\\WorklistViewConfigurationDataAccess.cs");
      }
      catch (Exception ex)
      {
        this.logger.LogError("UpdateAdminConfiguration.Exception", ex, callerName: nameof (UpdateAdminConfiguration), callerFile: "C:\\__w\\1\\s\\solutions\\AcceleratedSales\\AcceleratedSalesSandboxPlugins\\Core\\DataAccess\\WorklistViewConfigurationDataAccess.cs");
      }
    }

    public Guid CreateAdminConfiguration()
    {
      Guid id = new Guid();
      try
      {
        this.logger.AddCustomProperty(nameof (CreateAdminConfiguration), (object) "msdyn_worklistviewconfiguration");
        id = this.dataStore.Elevate().Create(new Entity("msdyn_worklistviewconfiguration", Guid.NewGuid())
        {
          ["msdyn_viewtype"] = (object) new OptionSetValue(0),
          ["msdyn_inheritrolesfromparentsettings"] = (object) true,
          ["msdyn_salesaccelerationsettingsid"] = (object) null
        });
        this.dataStore.Elevate().Update(new Entity("msdyn_worklistviewconfiguration", id)
        {
          ["statecode"] = (object) new OptionSetValue(1),
          ["statuscode"] = (object) new OptionSetValue(2)
        });
        this.logger.LogWarning("CreateAdminConfiguration.End", callerName: nameof (CreateAdminConfiguration), callerFile: "C:\\__w\\1\\s\\solutions\\AcceleratedSales\\AcceleratedSalesSandboxPlugins\\Core\\DataAccess\\WorklistViewConfigurationDataAccess.cs");
      }
      catch (Exception ex)
      {
        this.logger.LogError("CreateAdminConfiguration.Exception", ex, callerName: nameof (CreateAdminConfiguration), callerFile: "C:\\__w\\1\\s\\solutions\\AcceleratedSales\\AcceleratedSalesSandboxPlugins\\Core\\DataAccess\\WorklistViewConfigurationDataAccess.cs");
      }
      return id;
    }
  }
}
