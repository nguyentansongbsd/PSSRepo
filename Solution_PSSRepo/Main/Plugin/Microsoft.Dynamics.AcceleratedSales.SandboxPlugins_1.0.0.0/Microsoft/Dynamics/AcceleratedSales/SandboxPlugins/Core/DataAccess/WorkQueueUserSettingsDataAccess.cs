// Decompiled with JetBrains decompiler
// Type: Microsoft.Dynamics.AcceleratedSales.SandboxPlugins.Core.DataAccess.WorkQueueUserSettingsDataAccess
// Assembly: Microsoft.Dynamics.AcceleratedSales.SandboxPlugins, Version=1.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35
// MVID: 7BBE7132-7E30-4424-B38D-27667CFBCEFA
// Assembly location: C:\Users\ngoct\Downloads\New folder\Microsoft.Dynamics.AcceleratedSales.SandboxPlugins_1.0.0.0.dll

using Microsoft.Dynamics.AcceleratedSales.SandboxObjectModel.Model;
using Microsoft.Dynamics.AcceleratedSales.SandboxPlugins.Core.Model;
using Microsoft.Dynamics.AcceleratedSales.SandboxPlugins.Core.Model.Filter;
using Microsoft.Dynamics.AcceleratedSales.SandboxPlugins.Core.Model.Layout;
using Microsoft.Dynamics.AcceleratedSales.SandboxPlugins.Core.Model.Sort;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Query;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics;

#nullable disable
namespace Microsoft.Dynamics.AcceleratedSales.SandboxPlugins.Core.DataAccess
{
  public class WorkQueueUserSettingsDataAccess
  {
    private const string WorkQueueUserSettingsEntityName = "msdyn_workqueueusersetting";
    private const string SellerCardLayoutAttributeName = "msdyn_sellercardlayout";
    private const string SellerFilterConfigAttributeName = "msdyn_sellerfilterconfiguration";
    private const string SellerSortConfigAttributeName = "msdyn_sellersortconfiguration";
    private const string SellerLinkingConfigAttributeName = "msdyn_linkingconfiguration";
    private const string SellerActionOnMarkCompleteAttributeName = "msdyn_actiononmarkcomplete";
    private const string SellerActionOnSkipAttributeName = "msdyn_actiononskip";
    private const string OwnerId = "ownerid";
    private const string UserId = "msdyn_userid";
    private readonly IDataStore dataStore;
    private readonly IAcceleratedSalesLogger logger;

    public WorkQueueUserSettingsDataAccess(IDataStore dataStore, IAcceleratedSalesLogger logger)
    {
      this.dataStore = dataStore;
      this.logger = logger;
    }

    public WorklistSellerConfiguration GetSellerConfiguration()
    {
      Stopwatch stopwatch = Stopwatch.StartNew();
      QueryExpression query = new QueryExpression();
      query.EntityName = "msdyn_workqueueusersetting";
      query.ColumnSet = new ColumnSet(new string[6]
      {
        "msdyn_sellercardlayout",
        "msdyn_sellerfilterconfiguration",
        "msdyn_sellersortconfiguration",
        "msdyn_linkingconfiguration",
        "msdyn_actiononmarkcomplete",
        "msdyn_actiononskip"
      });
      FilterExpression childFilter = new FilterExpression(LogicalOperator.Or);
      childFilter.AddCondition("ownerid", ConditionOperator.Equal, (object) this.dataStore.RetrieveUserId());
      childFilter.AddCondition("msdyn_userid", ConditionOperator.Equal, (object) this.dataStore.RetrieveUserId().ToString());
      query.Criteria.AddFilter(childFilter);
      query.Criteria.AddCondition("statecode", ConditionOperator.Equal, (object) 0);
      query.Criteria.AddCondition("statuscode", ConditionOperator.Equal, (object) 1);
      query.AddOrder("modifiedon", OrderType.Descending);
      WorklistSellerConfiguration sellerConfiguration = new WorklistSellerConfiguration();
      try
      {
        EntityCollection entityCollection = this.dataStore.Elevate().RetrieveMultiple(query);
        this.logger.LogWarning("GetSellerConfiguration.Entities.Count" + entityCollection?.Entities?.Count.ToString(), callerName: nameof (GetSellerConfiguration), callerFile: "C:\\__w\\1\\s\\solutions\\AcceleratedSales\\AcceleratedSalesSandboxPlugins\\Core\\DataAccess\\WorkQueueUserSettingsDataAccess.cs");
        this.logger.LogWarning("GetSellerConfiguration.Duration" + stopwatch.ElapsedMilliseconds.ToString(), callerName: nameof (GetSellerConfiguration), callerFile: "C:\\__w\\1\\s\\solutions\\AcceleratedSales\\AcceleratedSalesSandboxPlugins\\Core\\DataAccess\\WorkQueueUserSettingsDataAccess.cs");
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
        if (num1 != 0)
        {
          string attributeValue1 = entityCollection.Entities[0].TryGetAttributeValue<string>("msdyn_sellercardlayout", string.Empty);
          string attributeValue2 = entityCollection.Entities[0].TryGetAttributeValue<string>("msdyn_sellerfilterconfiguration", string.Empty);
          string attributeValue3 = entityCollection.Entities[0].TryGetAttributeValue<string>("msdyn_sellersortconfiguration", string.Empty);
          string attributeValue4 = entityCollection.Entities[0].TryGetAttributeValue<string>("msdyn_linkingconfiguration", string.Empty);
          OptionSetValue attributeValue5 = entityCollection.Entities[0].TryGetAttributeValue<OptionSetValue>("msdyn_actiononmarkcomplete", (OptionSetValue) null);
          int? nullable2;
          if (attributeValue5 == null)
          {
            nullable1 = new int?();
            nullable2 = nullable1;
          }
          else
            nullable2 = new int?(attributeValue5.Value);
          int? nullable3 = nullable2;
          OptionSetValue attributeValue6 = entityCollection.Entities[0].TryGetAttributeValue<OptionSetValue>("msdyn_actiononskip", (OptionSetValue) null);
          int? nullable4;
          if (attributeValue6 == null)
          {
            nullable1 = new int?();
            nullable4 = nullable1;
          }
          else
            nullable4 = new int?(attributeValue6.Value);
          int? nullable5 = nullable4;
          WorkQueueAutoAdvanceSettings autoAdvanceSettings = new WorkQueueAutoAdvanceSettings();
          sellerConfiguration.UserSettingsId = entityCollection.Entities[0].Id;
          if (!string.IsNullOrEmpty(attributeValue1))
            sellerConfiguration.CardLayout = JsonConvert.DeserializeObject<Dictionary<string, DesignerConfiguration>>(attributeValue1);
          if (!string.IsNullOrEmpty(attributeValue2))
            sellerConfiguration.FilterConfiguration = JsonConvert.DeserializeObject<SellerFilterConfiguration>(attributeValue2);
          if (!string.IsNullOrEmpty(attributeValue3))
            sellerConfiguration.SortConfiguration = JsonConvert.DeserializeObject<SellerSortConfiguration>(attributeValue3);
          if (!string.IsNullOrEmpty(attributeValue4))
            sellerConfiguration.LinkingConfigurations = JsonConvert.DeserializeObject<Dictionary<string, LinkingConfiguration>>(attributeValue4);
          if (nullable3.HasValue)
            autoAdvanceSettings.ActionOnMarkComplete = nullable3;
          if (nullable5.HasValue)
            autoAdvanceSettings.ActionOnSkip = nullable5;
          sellerConfiguration.WorkQueueAutoAdvanceSettings = autoAdvanceSettings;
        }
      }
      catch (Exception ex)
      {
        this.logger.LogError("GetSellerConfiguration.Exception", ex, callerName: nameof (GetSellerConfiguration), callerFile: "C:\\__w\\1\\s\\solutions\\AcceleratedSales\\AcceleratedSalesSandboxPlugins\\Core\\DataAccess\\WorkQueueUserSettingsDataAccess.cs");
      }
      return sellerConfiguration;
    }

    public void UpdateSellerConfiguration(
      WorklistSellerConfiguration userConfig,
      Guid userSettingsId)
    {
      this.logger.LogWarning("UpdateSellerConfiguration.userId: " + this.dataStore.RetrieveUserId().ToString(), callerName: nameof (UpdateSellerConfiguration), callerFile: "C:\\__w\\1\\s\\solutions\\AcceleratedSales\\AcceleratedSalesSandboxPlugins\\Core\\DataAccess\\WorkQueueUserSettingsDataAccess.cs");
      try
      {
        Guid guid = this.dataStore.RetrieveUserId();
        Entity entity1 = new Entity("msdyn_workqueueusersetting", userSettingsId);
        entity1["msdyn_userid"] = (object) guid.ToString();
        if (userConfig.CardLayout != null)
        {
          this.logger.LogWarning("UpdateSellerConfiguration.CardLayout :" + userConfig.CardLayout.ToString(), callerName: nameof (UpdateSellerConfiguration), callerFile: "C:\\__w\\1\\s\\solutions\\AcceleratedSales\\AcceleratedSalesSandboxPlugins\\Core\\DataAccess\\WorkQueueUserSettingsDataAccess.cs");
          entity1["msdyn_sellercardlayout"] = (object) JsonConvert.SerializeObject((object) userConfig.CardLayout);
        }
        WorkQueueAutoAdvanceSettings autoAdvanceSettings1 = userConfig.WorkQueueAutoAdvanceSettings;
        int? nullable;
        int num1;
        if (autoAdvanceSettings1 == null)
        {
          num1 = 0;
        }
        else
        {
          nullable = autoAdvanceSettings1.ActionOnMarkComplete;
          num1 = nullable.HasValue ? 1 : 0;
        }
        if (num1 != 0)
        {
          Entity entity2 = entity1;
          nullable = userConfig.WorkQueueAutoAdvanceSettings.ActionOnMarkComplete;
          OptionSetValue optionSetValue = new OptionSetValue(nullable.GetValueOrDefault());
          entity2["msdyn_actiononmarkcomplete"] = (object) optionSetValue;
        }
        WorkQueueAutoAdvanceSettings autoAdvanceSettings2 = userConfig.WorkQueueAutoAdvanceSettings;
        int num2;
        if (autoAdvanceSettings2 == null)
        {
          num2 = 0;
        }
        else
        {
          nullable = autoAdvanceSettings2.ActionOnSkip;
          num2 = nullable.HasValue ? 1 : 0;
        }
        if (num2 != 0)
        {
          Entity entity3 = entity1;
          nullable = userConfig.WorkQueueAutoAdvanceSettings.ActionOnSkip;
          OptionSetValue optionSetValue = new OptionSetValue(nullable.GetValueOrDefault());
          entity3["msdyn_actiononskip"] = (object) optionSetValue;
        }
        if (userConfig.FilterConfiguration != null)
          entity1["msdyn_sellerfilterconfiguration"] = (object) JsonConvert.SerializeObject((object) userConfig.FilterConfiguration);
        if (userConfig.SortConfiguration != null)
          entity1["msdyn_sellersortconfiguration"] = (object) JsonConvert.SerializeObject((object) userConfig.SortConfiguration);
        if (userConfig.LinkingConfigurations != null)
        {
          this.logger.LogWarning("UpdateSellerConfiguration.CardLayout :" + userConfig.LinkingConfigurations.ToString(), callerName: nameof (UpdateSellerConfiguration), callerFile: "C:\\__w\\1\\s\\solutions\\AcceleratedSales\\AcceleratedSalesSandboxPlugins\\Core\\DataAccess\\WorkQueueUserSettingsDataAccess.cs");
          entity1["msdyn_linkingconfiguration"] = (object) JsonConvert.SerializeObject((object) userConfig.LinkingConfigurations);
        }
        this.dataStore.Elevate().Update(entity1);
        this.logger.LogWarning("UpdateSellerConfiguration.End", callerName: nameof (UpdateSellerConfiguration), callerFile: "C:\\__w\\1\\s\\solutions\\AcceleratedSales\\AcceleratedSalesSandboxPlugins\\Core\\DataAccess\\WorkQueueUserSettingsDataAccess.cs");
      }
      catch (Exception ex)
      {
        this.logger.LogError("UpdateSellerConfiguration.Exception", ex, callerName: nameof (UpdateSellerConfiguration), callerFile: "C:\\__w\\1\\s\\solutions\\AcceleratedSales\\AcceleratedSalesSandboxPlugins\\Core\\DataAccess\\WorkQueueUserSettingsDataAccess.cs");
      }
    }

    public Guid CreateSellerConfiguration()
    {
      this.logger.LogWarning("CreateSellerConfiguration.Start", callerName: nameof (CreateSellerConfiguration), callerFile: "C:\\__w\\1\\s\\solutions\\AcceleratedSales\\AcceleratedSalesSandboxPlugins\\Core\\DataAccess\\WorkQueueUserSettingsDataAccess.cs");
      Guid sellerConfiguration = new Guid();
      try
      {
        this.dataStore.RetrieveUserId();
        sellerConfiguration = this.dataStore.Elevate().Create(new Entity("msdyn_workqueueusersetting", Guid.NewGuid())
        {
          ["msdyn_linkingconfiguration"] = (object) "{\"phonecall\":{\"activityToStep\":{\"markCompleteOptions\":2,\"skipOptions\":1},\"stepToActivity\":{\"markCompleteOptions\":2,\"skipOptions\":1,\"markCompleteStateCode\":1,\"markCompleteStatusCode\":2}},\"email\":{\"activityToStep\":{\"markCompleteOptions\":2,\"skipOptions\":1},\"stepToActivity\":{\"markCompleteOptions\":2,\"skipOptions\":1,\"markCompleteStateCode\":1,\"markCompleteStatusCode\":2}},\"task\":{\"activityToStep\":{\"markCompleteOptions\":2,\"skipOptions\":1},\"stepToActivity\":{\"markCompleteOptions\":2,\"skipOptions\":1,\"markCompleteStateCode\":1,\"markCompleteStatusCode\":5}}}"
        });
        this.logger.LogWarning("CreateSellerConfiguration.End", callerName: nameof (CreateSellerConfiguration), callerFile: "C:\\__w\\1\\s\\solutions\\AcceleratedSales\\AcceleratedSalesSandboxPlugins\\Core\\DataAccess\\WorkQueueUserSettingsDataAccess.cs");
      }
      catch (Exception ex)
      {
        this.logger.LogError("CreateSellerConfiguration.Exception", ex, callerName: nameof (CreateSellerConfiguration), callerFile: "C:\\__w\\1\\s\\solutions\\AcceleratedSales\\AcceleratedSalesSandboxPlugins\\Core\\DataAccess\\WorkQueueUserSettingsDataAccess.cs");
      }
      return sellerConfiguration;
    }
  }
}
