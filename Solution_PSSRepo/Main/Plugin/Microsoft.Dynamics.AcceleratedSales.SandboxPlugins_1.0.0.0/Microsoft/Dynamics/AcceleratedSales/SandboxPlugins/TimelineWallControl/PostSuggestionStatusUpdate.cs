// Decompiled with JetBrains decompiler
// Type: Microsoft.Dynamics.AcceleratedSales.SandboxPlugins.TimelineWallControl.PostSuggestionStatusUpdate
// Assembly: Microsoft.Dynamics.AcceleratedSales.SandboxPlugins, Version=1.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35
// MVID: 7BBE7132-7E30-4424-B38D-27667CFBCEFA
// Assembly location: C:\Users\ngoct\Downloads\New folder\Microsoft.Dynamics.AcceleratedSales.SandboxPlugins_1.0.0.0.dll

using Microsoft.Crm.AcceleratedSales.Shared.SandboxServices.Platform;
using Microsoft.Dynamics.AcceleratedSales.SandboxPlugins.Core.DataAccess;
using Microsoft.Dynamics.AcceleratedSales.SandboxPlugins.Core.Model;
using Microsoft.Dynamics.AcceleratedSales.SandboxPlugins.Core.Services;
using Microsoft.Dynamics.AcceleratedSales.SandboxPlugins.Platform;
using Microsoft.Xrm.Sdk;
using System;
using System.Collections.Generic;

#nullable disable
namespace Microsoft.Dynamics.AcceleratedSales.SandboxPlugins.TimelineWallControl
{
  public class PostSuggestionStatusUpdate : PluginBase
  {
    public PostSuggestionStatusUpdate()
      : base(typeof (PostSuggestionStatusUpdate))
    {
    }

    protected override void ExecuteCrmPlugin(LocalPluginContext localContext)
    {
      IAcceleratedSalesLogger logger = (IAcceleratedSalesLogger) new Logger(localContext);
      IDataStore dataStore = (IDataStore) new DataverseStore(localContext);
      logger.Execute("AcceleratedSales.PostSuggestionStatusUpdate", (Action) (() => this.ExecutePlugin(localContext, logger, dataStore)));
    }

    protected void ExecutePlugin(
      LocalPluginContext localPluginContext,
      IAcceleratedSalesLogger logger,
      IDataStore dataStore)
    {
      bool flag1 = dataStore.IsAppSettingEnabled("msdyn_EnhancedSuggestionsPreview", logger);
      bool flag2 = dataStore.IsAppSettingEnabled("msdyn_IsSellerInsightsEnabled", logger);
      bool isMSXViewFCSEnabled = dataStore.IsFCSEnabled("SalesService.Workspace", "MSXView");
      if (!flag2 || !flag1)
        return;
      Entity inputParameter = localPluginContext.GetInputParameter<Entity>("Target");
      if (inputParameter == null || inputParameter.LogicalName != "msdyn_salessuggestion")
      {
        logger.LogError(-2138046461, "Invalid target entity", new Dictionary<string, object>()
        {
          {
            "Target",
            inputParameter == null ? (object) "null" : (object) "notNull"
          },
          {
            "targetEntityName",
            inputParameter == null ? (object) "null" : (object) inputParameter.LogicalName
          }
        }, nameof (ExecutePlugin), "C:\\__w\\1\\s\\solutions\\AcceleratedSales\\AcceleratedSalesSandboxPlugins\\TimelineWallControl\\PostSuggestionStatusUpdate.cs");
        throw new CrmException("Invalid target entity", -2138046461);
      }
      TimelineActivityFeedServices activityFeedServices = new TimelineActivityFeedServices(localPluginContext, logger);
      try
      {
        bool flag3 = activityFeedServices.IsActivityFeedConfigurationExists("msdyn_salessuggestion");
        if (!flag3)
        {
          logger.LogError(-2137980927, "Plugin execution is returning due to insufficient configuration. Please check required FCS values and activityFeed configuration.", new Dictionary<string, object>()
          {
            {
              "ActivityFeedConfiguration",
              !flag3 ? (object) "false" : (object) "true"
            }
          }, nameof (ExecutePlugin), "C:\\__w\\1\\s\\solutions\\AcceleratedSales\\AcceleratedSalesSandboxPlugins\\TimelineWallControl\\PostSuggestionStatusUpdate.cs");
        }
        else
        {
          EntityReference entityReference = inputParameter.ToEntityReference();
          OptionSetValue attributeValue1 = inputParameter.Attributes.Contains("statuscode") ? inputParameter.GetAttributeValue<OptionSetValue>("statuscode") : (OptionSetValue) null;
          EntityReference attributeValue2 = inputParameter.Attributes.Contains("ownerid") ? inputParameter.GetAttributeValue<EntityReference>("ownerid") : (EntityReference) null;
          EntityReference attributeValue3 = inputParameter.Attributes.Contains("msdyn_qualifiedrecord") ? inputParameter.GetAttributeValue<EntityReference>("msdyn_qualifiedrecord") : (EntityReference) null;
          string updateAutoPostContent = activityFeedServices.GetStatusUpdateAutoPostContent(attributeValue1, entityReference, attributeValue3, attributeValue2, isMSXViewFCSEnabled);
          if (isMSXViewFCSEnabled)
          {
            activityFeedServices.CreateAutoPost(entityReference, updateAutoPostContent);
            logger.AddCustomProperty("AcceleratedSales.PostSuggestionStatusUpdate Plugin executed successfully", (object) "Post created successfully.");
          }
          else
          {
            string[] columns = new string[1]
            {
              "msdyn_relatedrecord"
            };
            EntityReference attributeValue4 = activityFeedServices.RetrieveRecord(entityReference, columns).GetAttributeValue<EntityReference>("msdyn_relatedrecord");
            activityFeedServices.CreateAutoPost(attributeValue4, updateAutoPostContent);
            logger.AddCustomProperty("AcceleratedSales.PostSuggestionStatusUpdate Plugin executed successfully", (object) "Post created successfully.");
          }
        }
      }
      catch (Exception ex)
      {
        logger.LogError("AcceleratedSales.PostSuggestionStatusUpdate Plugin execution failed", new Dictionary<string, object>()
        {
          {
            ex.Message,
            (object) ex.Message
          },
          {
            ex.StackTrace,
            (object) ex.StackTrace
          }
        }, nameof (ExecutePlugin), "C:\\__w\\1\\s\\solutions\\AcceleratedSales\\AcceleratedSalesSandboxPlugins\\TimelineWallControl\\PostSuggestionStatusUpdate.cs");
      }
    }
  }
}
