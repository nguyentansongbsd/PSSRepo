// Decompiled with JetBrains decompiler
// Type: Microsoft.Dynamics.AcceleratedSales.SandboxPlugins.Core.Services.UpnextDataService
// Assembly: Microsoft.Dynamics.AcceleratedSales.SandboxPlugins, Version=1.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35
// MVID: 7BBE7132-7E30-4424-B38D-27667CFBCEFA
// Assembly location: C:\Users\ngoct\Downloads\New folder\Microsoft.Dynamics.AcceleratedSales.SandboxPlugins_1.0.0.0.dll

using Microsoft.Dynamics.AcceleratedSales.SandboxPlugins.Core.DataAccess;
using Microsoft.Dynamics.AcceleratedSales.SandboxPlugins.Core.Model;
using Microsoft.Dynamics.AcceleratedSales.SandboxPlugins.Core.Model.Interface;
using System;
using System.Collections.Generic;
using System.Linq;

#nullable disable
namespace Microsoft.Dynamics.AcceleratedSales.SandboxPlugins.Core.Services
{
  public class UpnextDataService
  {
    private const string AlwaysLinkActivityToStepDisabledFCB = "FCB.AlwaysLinkActivityToStepDisabled";
    private const string SellerInsightsAppSettingName = "msdyn_IsSellerInsightsEnabled";
    private readonly IDataStore dataStore;
    private readonly IAcceleratedSalesLogger logger;
    private readonly SuggestionsDataAccess suggestionsDataAccess;
    private readonly ActivitiesDataAccess activitiesDataAccess;
    private readonly SASettingsDataAccess saSettingsDataAccess;
    private readonly WorklistSettingsProviderService worklistSettingsProviderService;
    private readonly ISequenceStepsDataAccess sequenceStepsDataAccessMock;
    private readonly ITeamMembershipDataAccess teamMembership;

    public UpnextDataService(IAcceleratedSalesLogger logger, IDataStore dataStore)
    {
      this.dataStore = dataStore;
      this.logger = logger;
      this.suggestionsDataAccess = new SuggestionsDataAccess(logger, dataStore);
      this.activitiesDataAccess = new ActivitiesDataAccess(dataStore, logger);
      this.saSettingsDataAccess = new SASettingsDataAccess(this.dataStore, this.logger);
      this.worklistSettingsProviderService = new WorklistSettingsProviderService(dataStore, logger);
      this.teamMembership = (ITeamMembershipDataAccess) new TeamMembershipDataAccess(logger, dataStore);
    }

    public UpnextDataService(
      IAcceleratedSalesLogger logger,
      IDataStore dataStore,
      SuggestionsDataAccess suggestionsDataAccess,
      ActivitiesDataAccess activitiesDataAccess,
      SASettingsDataAccess saSettingsDataAccess,
      WorklistSettingsProviderService worklistSettingsProviderService,
      ISequenceStepsDataAccess sequenceStepsDataAccess,
      ITeamMembershipDataAccess teamMembership)
    {
      this.dataStore = dataStore;
      this.logger = logger;
      this.suggestionsDataAccess = suggestionsDataAccess;
      this.activitiesDataAccess = activitiesDataAccess;
      this.saSettingsDataAccess = saSettingsDataAccess;
      this.worklistSettingsProviderService = worklistSettingsProviderService;
      this.sequenceStepsDataAccessMock = sequenceStepsDataAccess;
      this.teamMembership = teamMembership;
    }

    public UpnextWidgetDataResponse GetUpnextWidgetData(
      GetUpnextDataRequestParameters getUpnextDataRequestParams)
    {
      UpnextSettings upnextSettings = new UpnextSettings()
      {
        SalesAcceleratorSettings = this.saSettingsDataAccess.RetrieveSASettings(),
        WQUserSettings = this.saSettingsDataAccess.RetrieveWQUserSettings()
      };
      List<ViewType> userAccessibleView = this.worklistSettingsProviderService.TryGetUserAccessibleView(upnextSettings.SalesAcceleratorSettings);
      // ISSUE: explicit non-virtual call
      upnextSettings.SalesAcceleratorSettings.isEnabledForUser = userAccessibleView != null && __nonvirtual (userAccessibleView.Count) > 0;
      List<SuggestionsRecord> suggestions = new List<SuggestionsRecord>();
      SequenceStepRecordsData sequenceStepRecordsData = new SequenceStepRecordsData();
      List<ActivityRecord> activities = new List<ActivityRecord>();
      AllStepsForSSeqS allStepsForSseqS = (AllStepsForSSeqS) null;
      bool activity = this.ShouldLinkSequenceStepToActivity(upnextSettings.SalesAcceleratorSettings?.settingsInstance?.shouldLinkSequenceStepToActivity);
      if (!string.IsNullOrEmpty(getUpnextDataRequestParams.EntityRecordId))
      {
        if (this.IsSuggestionEnabled(userAccessibleView))
          suggestions = this.suggestionsDataAccess.GetSuggestionRecords(getUpnextDataRequestParams);
        Guid primaryRecordId = Guid.Parse(getUpnextDataRequestParams.EntityRecordId);
        List<Guid> entityRecordIds = this.GetEntityRecordIds(suggestions, primaryRecordId);
        ISequenceStepsDataAccess sequenceStepsDataAccess = this.sequenceStepsDataAccessMock ?? (!getUpnextDataRequestParams.AdditionalParameters.ReturnAllStepsForSSeqS.HasValue || !getUpnextDataRequestParams.AdditionalParameters.ReturnAllStepsForSSeqS.GetValueOrDefault() ? (ISequenceStepsDataAccess) new SequenceStepsDataAccess(this.logger, this.dataStore) : (ISequenceStepsDataAccess) new SequenceStepsWithSSeqSDataAccess(this.logger, this.dataStore, this.teamMembership));
        sequenceStepRecordsData = sequenceStepsDataAccess.GetSequenceStepRecords(primaryRecordId, entityRecordIds, activity);
        allStepsForSseqS = sequenceStepsDataAccess.GetAllStepsForSSeqS(primaryRecordId, entityRecordIds, activity);
        activities = this.activitiesDataAccess.GetManualActivities(entityRecordIds.Concat<Guid>((IEnumerable<Guid>) new List<Guid>()
        {
          primaryRecordId
        }).ToList<Guid>(), sequenceStepRecordsData?.LinkedActivityIds);
        if (activities != null && activities.Count > 0)
          activities = this.ProcessActivityRecords(activities, sequenceStepsDataAccess.GetSequenceStepRecordsLinkedActivityIds(this.GetActivityIds(activities)));
      }
      return new UpnextWidgetDataResponse()
      {
        UpnextSettings = upnextSettings,
        Suggestions = suggestions,
        SequenceStepRecords = sequenceStepRecordsData?.SequenceStepRecords,
        ActivityRecords = activities,
        AllStepsForSSeqS = allStepsForSseqS
      };
    }

    private List<Guid> GetEntityRecordIds(List<SuggestionsRecord> suggestions, Guid primaryRecordId)
    {
      List<Guid> entityRecordIds = new List<Guid>();
      try
      {
        suggestions?.ForEach((Action<SuggestionsRecord>) (suggestion =>
        {
          if (entityRecordIds.Contains(suggestion.SuggestionId) || !(suggestion.SuggestionId != primaryRecordId))
            return;
          entityRecordIds.Add(suggestion.SuggestionId);
        }));
      }
      catch (Exception ex)
      {
        this.logger.LogError("UpnextDataService.GetEntityRecordIds.Exception", ex, callerName: nameof (GetEntityRecordIds), callerFile: "C:\\__w\\1\\s\\solutions\\AcceleratedSales\\AcceleratedSalesSandboxPlugins\\Core\\Services\\UpnextDataService.cs");
      }
      this.logger.LogWarning("UpnextDataService.GetEntityRecordIds.Count: " + entityRecordIds.Count.ToString(), callerName: nameof (GetEntityRecordIds), callerFile: "C:\\__w\\1\\s\\solutions\\AcceleratedSales\\AcceleratedSalesSandboxPlugins\\Core\\Services\\UpnextDataService.cs");
      return entityRecordIds;
    }

    private List<Guid> GetActivityIds(List<ActivityRecord> activities)
    {
      List<Guid> activityRecordIds = new List<Guid>();
      try
      {
        if (activities != null && activities.Count > 0)
          activities.ForEach((Action<ActivityRecord>) (activity =>
          {
            if (activityRecordIds.Contains(activity.ActivityId))
              return;
            activityRecordIds.Add(activity.ActivityId);
          }));
      }
      catch (Exception ex)
      {
        this.logger.LogError("UpnextDataService.GetActivityIds.Exception", ex, callerName: nameof (GetActivityIds), callerFile: "C:\\__w\\1\\s\\solutions\\AcceleratedSales\\AcceleratedSalesSandboxPlugins\\Core\\Services\\UpnextDataService.cs");
      }
      this.logger.LogWarning("UpnextDataService.GetActivityIds.Count: " + activityRecordIds.Count.ToString(), callerName: nameof (GetActivityIds), callerFile: "C:\\__w\\1\\s\\solutions\\AcceleratedSales\\AcceleratedSalesSandboxPlugins\\Core\\Services\\UpnextDataService.cs");
      return activityRecordIds;
    }

    private List<ActivityRecord> ProcessActivityRecords(
      List<ActivityRecord> activities,
      List<Guid> linkedActivityIds)
    {
      List<ActivityRecord> activityRecords = new List<ActivityRecord>();
      try
      {
        if (activities != null && activities.Count > 0)
          activities.ForEach((Action<ActivityRecord>) (activity =>
          {
            if (linkedActivityIds.Contains(activity.ActivityId))
              return;
            if (activity.OwnedById.HasValue)
              activity.IsOwnedByUserOrTeam = this.teamMembership.IsEqualUserOrTeam(activity.OwnedById.Value);
            activityRecords.Add(activity);
          }));
      }
      catch (Exception ex)
      {
        this.logger.LogError("UpnextDataService.ProcessActivityRecords.Exception", ex, callerName: nameof (ProcessActivityRecords), callerFile: "C:\\__w\\1\\s\\solutions\\AcceleratedSales\\AcceleratedSalesSandboxPlugins\\Core\\Services\\UpnextDataService.cs");
      }
      this.logger.LogWarning("UpnextDataService.ProcessActivityRecords.Count: " + activityRecords.Count.ToString(), callerName: nameof (ProcessActivityRecords), callerFile: "C:\\__w\\1\\s\\solutions\\AcceleratedSales\\AcceleratedSalesSandboxPlugins\\Core\\Services\\UpnextDataService.cs");
      return activityRecords;
    }

    private bool IsSuggestionEnabled(List<ViewType> viewList)
    {
      bool flag = false;
      if (viewList != null)
        flag = this.dataStore.IsAppSettingEnabled("msdyn_IsSellerInsightsEnabled", this.logger) && viewList.Contains(ViewType.Suggestion);
      return flag;
    }

    private bool ShouldLinkSequenceStepToActivity(bool? shouldLinkSequenceStepToActivity)
    {
      return shouldLinkSequenceStepToActivity.HasValue && shouldLinkSequenceStepToActivity.GetValueOrDefault() && !this.dataStore.IsFCBEnabled("FCB.AlwaysLinkActivityToStepDisabled");
    }
  }
}
