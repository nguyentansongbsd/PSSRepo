// Decompiled with JetBrains decompiler
// Type: Microsoft.Dynamics.AcceleratedSales.SandboxObjectModel.Constants.SuggestionTimelineConstants
// Assembly: Microsoft.Dynamics.AcceleratedSales.SandboxPlugins, Version=1.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35
// MVID: 7BBE7132-7E30-4424-B38D-27667CFBCEFA
// Assembly location: C:\Users\ngoct\Downloads\New folder\Microsoft.Dynamics.AcceleratedSales.SandboxPlugins_1.0.0.0.dll

#nullable disable
namespace Microsoft.Dynamics.AcceleratedSales.SandboxObjectModel.Constants
{
  internal static class SuggestionTimelineConstants
  {
    public const string TargetParam = "Target";
    public const string MsxViewFcs = "MSXView";
    public const string WorkspaceFCSNamespace = "SalesService.Workspace";
    public const string EnhancedSuggestionsAppSetting = "msdyn_EnhancedSuggestionsPreview";
    public const string SellerInsightsAppSetting = "msdyn_IsSellerInsightsEnabled";
    public const string ActivityFeedConfigurationLogicalName = "msdyn_postconfig";
    public const string RegardingObjectId = "regardingobjectid";
    public const string Subject = "subject";
    public const string StatusCode = "statuscode";
    public const string EntityLogicalName = "msdyn_entityname";
    public const string ConfigureWall = "msdyn_configurewall";
    public const string Completed = "completed";
    public const string Canceled = "canceled";
    public const string TimelineActivityFeedServices = "TimelineActivityFeedServices: ";
    public const string InvalidTargetEntity = "Invalid target entity";
    public const string EntityMissingInPostConfig = "Entity not present in msdyn_postconfig";
    public const string EntityPresentInPostConfig = "Entity is present in msdyn_postconfig";
    public const string ActivityFeedConfigurationExists = "ActivityFeedConfigurationExists: ";
    public const string ActivityFeedConfigurationDataRetreive = "ActivityFeedPostConfigDataRetrieve: ";
    public const string AttributeMissing = "Attributes missing in entity";
    public const string ActivityFeedPostCreateStarted = "Activity feed post create started for suggestion record";
    public const string ActivityFeedPostCreateCompleted = "Activity feed post create completed for suggestion record";
    public const string EntityReferenceIsNull = "Entity reference is null";
    public const string ActivityFeedStatusUpdateContent = "Failed to get content for activity feed's current status";
    public const string RetrieveCompleted = "Retrieve completed for entity: {0}";
    public const string RetrieveFailed = "Retrieve failed for entity: {0}";
    public const string StatusUpdateAutoPostContentFailed = "Failed to get auto-post content upon status change";
    public const string EntityName = "entityname";
    public const string NoRecordFound = "Record do not exist";
    public const string PostCreationSuccess = "Post created successfully.";
    public const string PostCreationFailed = "Post creation failed";
    public const string GetEntityLocalizedDisplayNameFailed = "Fetching entity's localized display name failed.";
    public const string ActivityUpdateSuccess = "Updated activity feed successfully.";
    public const string PluginExecutionReturn = "Plugin execution is returning due to insufficient configuration. Please check required FCS values and activityFeed configuration.";
    public const string PluginExecutionSucceeded = "Plugin executed successfully";
    public const string PluginExecutionFailed = "Plugin execution failed";
    public const string TargetEntityName = "targetEntityName";
    public const string EntityReference = "EntityReference";
    public const string IsNull = "null";
    public const string IsNotNull = "notNull";
    public const string False = "false";
    public const string True = "true";
    public const string ActivityFeedConfiguration = "ActivityFeedConfiguration";
    public const string ActivityStatusCanceled = "canceled";
    public const string ActivityStatusCompleted = "completed";
    public const string ActivityStatusCreated = "created";
    public const string Related = "Related ";
    public const string AutoPostAcceptedByContent = "{0} for {1} is accepted by {2}";
    public const string AutoPostActivityFeedContent = "{0} is {1} from {2}";
    public const string AutoPostClosedByContent = "{0} for {1} is closed by {2}";
    public const string AutoPostCreatedForContent = "{0} is created for {1}";
    public const string AutoPostDeclinedByContent = "{0} for {1} is declined by {2}";
    public const string AutoPostOpenedByContent = "{0} for {1} is accepted by {2}";
    public const string AutoPostQualifiedByContent = "{0} is qualified by {1} to {2}";
    public const string AutoPostUpdatedWithContent = "{0} is updated with {1}";
  }
}
