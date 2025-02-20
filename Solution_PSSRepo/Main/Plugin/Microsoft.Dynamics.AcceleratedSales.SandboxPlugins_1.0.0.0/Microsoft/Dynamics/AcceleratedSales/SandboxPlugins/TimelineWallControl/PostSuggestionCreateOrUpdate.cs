// Decompiled with JetBrains decompiler
// Type: Microsoft.Dynamics.AcceleratedSales.SandboxPlugins.TimelineWallControl.PostSuggestionCreateOrUpdate
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
  public class PostSuggestionCreateOrUpdate : PluginBase
  {
    public PostSuggestionCreateOrUpdate()
      : base(typeof (PostSuggestionCreateOrUpdate))
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
        }, nameof (ExecutePlugin), "C:\\__w\\1\\s\\solutions\\AcceleratedSales\\AcceleratedSalesSandboxPlugins\\TimelineWallControl\\PostSuggestionCreateOrUpdate.cs");
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
          }, nameof (ExecutePlugin), "C:\\__w\\1\\s\\solutions\\AcceleratedSales\\AcceleratedSalesSandboxPlugins\\TimelineWallControl\\PostSuggestionCreateOrUpdate.cs");
        }
        else
        {
          EntityReference entityReference = inputParameter.ToEntityReference();
          EntityReference attributeValue = inputParameter.Attributes.Contains("msdyn_relatedrecord") ? inputParameter.GetAttributeValue<EntityReference>("msdyn_relatedrecord") : (EntityReference) null;
          if (attributeValue == null)
            return;
          string recordTitleContent = activityFeedServices.GetSuggestionRecordTitleContent(entityReference, isMSXViewFCSEnabled);
          string entityLookupText = activityFeedServices.GetEntityLookupText(attributeValue, "name", 1);
          string autoPostText = localPluginContext.PluginExecutionContext.MessageName.Equals("Create", StringComparison.OrdinalIgnoreCase) ? string.Format("{0} is created for {1}", (object) recordTitleContent, (object) entityLookupText) : string.Format("{0} is updated with {1}", (object) recordTitleContent, (object) entityLookupText);
          if (isMSXViewFCSEnabled)
          {
            activityFeedServices.CreateAutoPost(entityReference, autoPostText);
            logger.AddCustomProperty("AcceleratedSales.PostSuggestionCreateOrUpdate Plugin executed successfully", (object) "Post created successfully.");
          }
          else
          {
            activityFeedServices.CreateAutoPost(attributeValue, autoPostText);
            logger.AddCustomProperty("AcceleratedSales.PostSuggestionCreateOrUpdate Plugin executed successfully", (object) "Post created successfully.");
          }
        }
      }
      catch (Exception ex)
      {
        logger.LogError("AcceleratedSales.PostSuggestionCreateOrUpdate Plugin execution failed", new Dictionary<string, object>()
        {
          {
            ex.Message,
            (object) ex.Message
          },
          {
            ex.StackTrace,
            (object) ex.StackTrace
          }
        }, nameof (ExecutePlugin), "C:\\__w\\1\\s\\solutions\\AcceleratedSales\\AcceleratedSalesSandboxPlugins\\TimelineWallControl\\PostSuggestionCreateOrUpdate.cs");
      }
    }
  }
}
