// Decompiled with JetBrains decompiler
// Type: Microsoft.Dynamics.AcceleratedSales.SandboxPlugins.Core.Services.TimelineActivityFeedServices
// Assembly: Microsoft.Dynamics.AcceleratedSales.SandboxPlugins, Version=1.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35
// MVID: 7BBE7132-7E30-4424-B38D-27667CFBCEFA
// Assembly location: C:\Users\ngoct\Downloads\New folder\Microsoft.Dynamics.AcceleratedSales.SandboxPlugins_1.0.0.0.dll

using Microsoft.Crm.AcceleratedSales.Shared.SandboxServices.Platform;
using Microsoft.Dynamics.AcceleratedSales.SandboxObjectModel.Proxies;
using Microsoft.Dynamics.AcceleratedSales.SandboxPlugins.Core.Model;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Messages;
using Microsoft.Xrm.Sdk.Query;
using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;

#nullable disable
namespace Microsoft.Dynamics.AcceleratedSales.SandboxPlugins.Core.Services
{
  public class TimelineActivityFeedServices
  {
    private readonly LocalPluginContext localPluginContext;
    private readonly IAcceleratedSalesLogger logger;

    public TimelineActivityFeedServices(
      LocalPluginContext pluginLocalContext,
      IAcceleratedSalesLogger acceleratedSalesLogger)
    {
      this.localPluginContext = pluginLocalContext;
      this.logger = acceleratedSalesLogger;
    }

    public void CreateAutoPost(EntityReference relatedRecordReference, string autoPostText)
    {
      if (relatedRecordReference == null)
      {
        this.logger.LogError(-2137980921, "Entity reference is null", new Dictionary<string, object>()
        {
          {
            "EntityReference",
            (object) "null"
          }
        }, nameof (CreateAutoPost), "C:\\__w\\1\\s\\solutions\\AcceleratedSales\\AcceleratedSalesSandboxPlugins\\Core\\Services\\TimelineActivityFeedServices.cs");
      }
      else
      {
        try
        {
          this.localPluginContext.OrganizationService.Create(new Entity("post")
          {
            ["regardingobjectid"] = (object) relatedRecordReference,
            ["source"] = (object) new OptionSetValue(1),
            ["text"] = (object) autoPostText
          });
        }
        catch (Exception ex)
        {
          this.logger.LogError("TimelineActivityFeedServices: Post creation failed", new Dictionary<string, object>()
          {
            {
              ex.Message,
              (object) ex.Message
            },
            {
              ex.StackTrace,
              (object) ex.StackTrace
            }
          }, nameof (CreateAutoPost), "C:\\__w\\1\\s\\solutions\\AcceleratedSales\\AcceleratedSalesSandboxPlugins\\Core\\Services\\TimelineActivityFeedServices.cs");
        }
      }
    }

    public string GetEntityLocalizedName(string entityName)
    {
      try
      {
        return ((RetrieveEntityResponse) this.localPluginContext.OrganizationService.Execute((OrganizationRequest) new RetrieveEntityRequest()
        {
          LogicalName = entityName
        }) ?? throw new ArgumentNullException("Record do not exist")).EntityMetadata.DisplayName.LocalizedLabels[0].Label;
      }
      catch (Exception ex)
      {
        this.logger.LogError("TimelineActivityFeedServices: Fetching entity's localized display name failed.", new Dictionary<string, object>()
        {
          {
            ex.Message,
            (object) ex.Message
          },
          {
            ex.StackTrace,
            (object) ex.StackTrace
          }
        }, nameof (GetEntityLocalizedName), "C:\\__w\\1\\s\\solutions\\AcceleratedSales\\AcceleratedSalesSandboxPlugins\\Core\\Services\\TimelineActivityFeedServices.cs");
        return entityName;
      }
    }

    public Entity RetrieveRecord(EntityReference entityRef, string[] columns)
    {
      try
      {
        return this.localPluginContext.OrganizationService.Retrieve(entityRef.LogicalName, entityRef.Id, new ColumnSet(columns));
      }
      catch (Exception ex)
      {
        this.logger.LogError(-2138046455, "Entity reference is null", new Dictionary<string, object>()
        {
          {
            ex.Message,
            (object) ex.Message
          },
          {
            ex.StackTrace,
            (object) ex.StackTrace
          }
        }, nameof (RetrieveRecord), "C:\\__w\\1\\s\\solutions\\AcceleratedSales\\AcceleratedSalesSandboxPlugins\\Core\\Services\\TimelineActivityFeedServices.cs");
        throw new ArgumentNullException("Entity reference is null");
      }
    }

    public string GetEntityLookupText(
      EntityReference entityReference,
      string attributeName,
      int entityTypeCode,
      bool showEntityDisplayName = true)
    {
      if (entityReference == null || string.IsNullOrEmpty(attributeName))
        return string.Empty;
      Guid id = entityReference.Id;
      string[] columns = new string[1]{ attributeName };
      string attributeValue = this.RetrieveRecord(entityReference, columns).GetAttributeValue<string>(attributeName);
      string entityLocalizedName = this.GetEntityLocalizedName(entityReference.LogicalName);
      string entityLookupText;
      if (!showEntityDisplayName)
        entityLookupText = string.Format("@[{0},{1},\"{2}\"]", (object) entityTypeCode, (object) id, (object) attributeValue);
      else
        entityLookupText = string.Format("{0} @[{1},{2},\"{3}\"]", (object) entityLocalizedName, (object) entityTypeCode, (object) id, (object) attributeValue);
      return entityLookupText;
    }

    public string GetSuggestionRecordTitleContent(
      EntityReference suggestionEntityReference,
      bool isMSXViewFCSEnabled,
      bool isTimelineActivityUpdate = false)
    {
      if (suggestionEntityReference == null)
        return string.Empty;
      string[] columns = new string[1]{ "msdyn_name" };
      string attributeValue = this.RetrieveRecord(suggestionEntityReference, columns).GetAttributeValue<string>("msdyn_name");
      string str = isTimelineActivityUpdate ? this.GetEntityLocalizedName("msdyn_salessuggestion").ToLower() : this.GetEntityLocalizedName("msdyn_salessuggestion");
      return !isMSXViewFCSEnabled ? string.Format("{0} \"{1}\"", (object) str, (object) attributeValue) : this.GetEntityLookupText(suggestionEntityReference, "msdyn_name", 10475);
    }

    public void CreateAutoPostUponActivityStatusChange(
      EntityReference activityEntityReference,
      EntityReference accountRecordEntityReference,
      EntityReference suggestionEntityReference,
      string activityFeedsSubject,
      OptionSetValue statusCode,
      bool isNewlyCreated,
      bool isMSXViewFCSEnabled)
    {
      try
      {
        string empty = string.Empty;
        string entityLocalizedName = this.GetEntityLocalizedName(activityEntityReference.LogicalName);
        string recordTitleContent = this.GetSuggestionRecordTitleContent(suggestionEntityReference, isMSXViewFCSEnabled);
        string[] source = new string[2]
        {
          "completed",
          "canceled"
        };
        (string status, int typecode) activityFeedsStatus = this.GetActivityFeedsStatus(activityEntityReference, statusCode);
        string str1 = string.Format("{0} @[{1},{2},\"{3}\"]", (object) entityLocalizedName, (object) activityFeedsStatus.typecode, (object) activityEntityReference.Id, (object) activityFeedsSubject);
        string str2 = isNewlyCreated ? "created" : activityFeedsStatus.status;
        if (isNewlyCreated)
        {
          string autoPostText = string.Format("{0} is {1} from {2}", (object) str1, (object) "created", (object) recordTitleContent);
          this.CreateAutoPost(accountRecordEntityReference, autoPostText);
        }
        else
        {
          if (!((IEnumerable<string>) source).Contains<string>(str2))
            return;
          string str3 = str2 == "completed" ? "completed" : "canceled";
          string autoPostText = string.Format("{0} is {1} from {2}", (object) str1, (object) str3, (object) recordTitleContent);
          this.CreateAutoPost(accountRecordEntityReference, autoPostText);
        }
      }
      catch (Exception ex)
      {
        this.logger.LogError("TimelineActivityFeedServices: Failed to get content for activity feed's current status", new Dictionary<string, object>()
        {
          {
            ex.Message,
            (object) ex.Message
          },
          {
            ex.StackTrace,
            (object) ex.StackTrace
          }
        }, nameof (CreateAutoPostUponActivityStatusChange), "C:\\__w\\1\\s\\solutions\\AcceleratedSales\\AcceleratedSalesSandboxPlugins\\Core\\Services\\TimelineActivityFeedServices.cs");
      }
    }

    public string GetStatusUpdateAutoPostContent(
      OptionSetValue suggestionRecordStatus,
      EntityReference suggestionRecordReference,
      EntityReference qualifiedRecordReference,
      EntityReference ownerEntityReference,
      bool isMSXViewFCSEnabled)
    {
      try
      {
        string updateAutoPostContent = string.Empty;
        EntityReference entityReference = (EntityReference) null;
        if (ownerEntityReference == null || qualifiedRecordReference == null || suggestionRecordStatus == null)
        {
          string[] columns = new string[4]
          {
            "ownerid",
            "msdyn_qualifiedrecord",
            "msdyn_relatedrecord",
            "statuscode"
          };
          Entity entity = this.RetrieveRecord(suggestionRecordReference, columns);
          ownerEntityReference = entity.GetAttributeValue<EntityReference>("ownerid");
          qualifiedRecordReference = entity.GetAttributeValue<EntityReference>("msdyn_qualifiedrecord");
          entityReference = entity.GetAttributeValue<EntityReference>("msdyn_relatedrecord");
          suggestionRecordStatus = entity.GetAttributeValue<OptionSetValue>("statuscode");
        }
        string entityLookupText1 = this.GetEntityLookupText(ownerEntityReference, "fullname", 8, false);
        string recordTitleContent = this.GetSuggestionRecordTitleContent(suggestionRecordReference, isMSXViewFCSEnabled);
        string entityLookupText2 = this.GetEntityLookupText(entityReference, "name", 1);
        switch (suggestionRecordStatus.Value)
        {
          case 1:
            updateAutoPostContent = string.Format("{0} for {1} is accepted by {2}", (object) recordTitleContent, (object) entityLookupText2, (object) entityLookupText1);
            break;
          case 2:
            updateAutoPostContent = string.Format("{0} for {1} is declined by {2}", (object) recordTitleContent, (object) entityLookupText2, (object) entityLookupText1);
            break;
          case 3:
            updateAutoPostContent = string.Format("{0} for {1} is closed by {2}", (object) recordTitleContent, (object) entityLookupText2, (object) entityLookupText1);
            break;
          case 4:
            string entityLookupText3 = this.GetEntityLookupText(qualifiedRecordReference, "name", 3);
            updateAutoPostContent = string.Format("{0} is qualified by {1} to {2}", (object) recordTitleContent, (object) entityLookupText1, (object) entityLookupText3);
            break;
          case 5:
            updateAutoPostContent = string.Format("{0} for {1} is accepted by {2}", (object) recordTitleContent, (object) entityLookupText2, (object) entityLookupText1);
            break;
        }
        return updateAutoPostContent;
      }
      catch (Exception ex)
      {
        this.logger.LogError("TimelineActivityFeedServices: Failed to get auto-post content upon status change", new Dictionary<string, object>()
        {
          {
            ex.Message,
            (object) ex.Message
          },
          {
            ex.StackTrace,
            (object) ex.StackTrace
          }
        }, nameof (GetStatusUpdateAutoPostContent), "C:\\__w\\1\\s\\solutions\\AcceleratedSales\\AcceleratedSalesSandboxPlugins\\Core\\Services\\TimelineActivityFeedServices.cs");
        return string.Empty;
      }
    }

    public bool IsActivityFeedConfigurationExists(string entityName)
    {
      EntityCollection entityCollection = this.ActivityFeedPostConfigDataRetrieve(entityName);
      if (entityCollection != null && entityCollection.Entities != null && entityCollection.Entities.Count > 0)
      {
        this.logger.AddCustomProperty("TimelineActivityFeedServices: ActivityFeedConfigurationExists: ", (object) "Entity is present in msdyn_postconfig");
        return true;
      }
      this.logger.AddCustomProperty("TimelineActivityFeedServices: ActivityFeedConfigurationExists: ", (object) "Entity not present in msdyn_postconfig");
      return false;
    }

    public EntityCollection ActivityFeedPostConfigDataRetrieve(string entityName)
    {
      EntityCollection entityCollection = (EntityCollection) null;
      try
      {
        QueryExpression query = new QueryExpression("msdyn_postconfig");
        string[] strArray = new string[2]
        {
          "msdyn_entityname",
          "msdyn_configurewall"
        };
        query.ColumnSet.AddColumns(strArray);
        FilterExpression childFilter = new FilterExpression();
        childFilter.AddCondition("msdyn_entityname", ConditionOperator.Equal, (object) entityName);
        query.Criteria.AddFilter(childFilter);
        return this.localPluginContext.OrganizationService.RetrieveMultiple((QueryBase) query);
      }
      catch (FaultException<OrganizationServiceFault> ex)
      {
        this.logger.LogError(string.Format("TimelineActivityFeedServices: Retrieve failed for entity: {0}", (object) "msdyn_postconfig"), new Dictionary<string, object>()
        {
          {
            "Message",
            (object) ex.Message
          },
          {
            "StackTrace",
            (object) ex.StackTrace
          }
        }, nameof (ActivityFeedPostConfigDataRetrieve), "C:\\__w\\1\\s\\solutions\\AcceleratedSales\\AcceleratedSalesSandboxPlugins\\Core\\Services\\TimelineActivityFeedServices.cs");
        return entityCollection;
      }
    }

    private (string status, int typecode) GetActivityFeedsStatus(
      EntityReference activityEntityReference,
      OptionSetValue statusCode)
    {
      string str = string.Empty;
      int num = -1;
      switch (activityEntityReference.LogicalName)
      {
        case "task":
          str = Enum.GetName(typeof (Task_StatusCode), (object) statusCode.Value);
          num = 4212;
          break;
        case "phonecall":
          str = Enum.GetName(typeof (PhoneCall_StatusCode), (object) statusCode.Value);
          num = 4210;
          break;
        case "appointment":
          str = Enum.GetName(typeof (Appointment_StatusCode), (object) statusCode.Value);
          num = 4201;
          break;
        case "email":
          str = Enum.GetName(typeof (Email_StatusCode), (object) statusCode.Value);
          num = 4202;
          break;
      }
      return (str.ToLower(), num);
    }
  }
}
