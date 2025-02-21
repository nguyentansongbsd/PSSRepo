// Decompiled with JetBrains decompiler
// Type: Microsoft.Dynamics.AcceleratedSales.SandboxPlugins.TimelineWallControl.PostSuggestionTimelineAction
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
  public class PostSuggestionTimelineAction : PluginBase
  {
    public PostSuggestionTimelineAction()
      : base(typeof (PostSuggestionTimelineAction))
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
      if (inputParameter == null || !new List<string>()
      {
        "appointment",
        "task",
        "phonecall",
        "email"
      }.Contains(inputParameter.LogicalName))
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
        }, nameof (ExecutePlugin), "C:\\__w\\1\\s\\solutions\\AcceleratedSales\\AcceleratedSalesSandboxPlugins\\TimelineWallControl\\PostSuggestionTimelineAction.cs");
        throw new CrmException("Invalid target entity", -2138046461);
      }
      TimelineActivityFeedServices activityFeedServices = new TimelineActivityFeedServices(localPluginContext, logger);
      try
      {
        bool flag3 = activityFeedServices.IsActivityFeedConfigurationExists("msdyn_salessuggestion");
        if (!flag3)
          logger.LogInfo("Plugin execution is returning due to insufficient configuration. Please check required FCS values and activityFeed configuration.", new Dictionary<string, object>()
          {
            {
              "msdyn_EnhancedSuggestionsPreview",
              !flag1 ? (object) "false" : (object) "true"
            }
          }, nameof (ExecutePlugin), "C:\\__w\\1\\s\\solutions\\AcceleratedSales\\AcceleratedSalesSandboxPlugins\\TimelineWallControl\\PostSuggestionTimelineAction.cs");
        else if (!flag3)
        {
          logger.LogError(-2137980927, "Plugin execution is returning due to insufficient configuration. Please check required FCS values and activityFeed configuration.", new Dictionary<string, object>()
          {
            {
              "ActivityFeedConfiguration",
              !flag3 ? (object) "false" : (object) "true"
            }
          }, nameof (ExecutePlugin), "C:\\__w\\1\\s\\solutions\\AcceleratedSales\\AcceleratedSalesSandboxPlugins\\TimelineWallControl\\PostSuggestionTimelineAction.cs");
        }
        else
        {
          EntityReference entityReference1 = inputParameter.ToEntityReference();
          EntityReference attributeValue1 = inputParameter.Attributes.Contains("regardingobjectid") ? inputParameter.GetAttributeValue<EntityReference>("regardingobjectid") : (EntityReference) null;
          OptionSetValue attributeValue2 = inputParameter.Attributes.Contains("statuscode") ? inputParameter.GetAttributeValue<OptionSetValue>("statuscode") : (OptionSetValue) null;
          string activityFeedsSubject = inputParameter.Attributes.Contains("subject") ? inputParameter.GetAttributeValue<string>("subject") : string.Empty;
          bool isNewlyCreated = localPluginContext.PluginExecutionContext.MessageName.Equals("Create", StringComparison.OrdinalIgnoreCase);
          if (activityFeedsSubject == null || attributeValue1 == null)
          {
            string[] columns = new string[2]
            {
              "subject",
              "regardingobjectid"
            };
            Entity entity = activityFeedServices.RetrieveRecord(entityReference1, columns);
            activityFeedsSubject = entity.GetAttributeValue<string>(columns[0]);
            attributeValue1 = entity.GetAttributeValue<EntityReference>(columns[1]);
          }
          if (attributeValue1 == null || attributeValue1.LogicalName != "msdyn_salessuggestion")
            return;
          string[] columns1 = new string[1]
          {
            "msdyn_relatedrecord"
          };
          Entity entity1 = activityFeedServices.RetrieveRecord(attributeValue1, columns1);
          EntityReference attributeValue3 = entity1.GetAttributeValue<EntityReference>("msdyn_relatedrecord");
          EntityReference entityReference2 = entity1.ToEntityReference();
          activityFeedServices.CreateAutoPostUponActivityStatusChange(entityReference1, attributeValue3, entityReference2, activityFeedsSubject, attributeValue2, isNewlyCreated, isMSXViewFCSEnabled);
          logger.AddCustomProperty("AcceleratedSales.PostSuggestionTimelineAction Plugin executed successfully", (object) "Updated activity feed successfully.");
        }
      }
      catch (Exception ex)
      {
        logger.LogError("AcceleratedSales.PostSuggestionTimelineAction Plugin execution failed", new Dictionary<string, object>()
        {
          {
            ex.Message,
            (object) ex.Message
          },
          {
            ex.StackTrace,
            (object) ex.StackTrace
          }
        }, nameof (ExecutePlugin), "C:\\__w\\1\\s\\solutions\\AcceleratedSales\\AcceleratedSalesSandboxPlugins\\TimelineWallControl\\PostSuggestionTimelineAction.cs");
      }
    }
  }
}
