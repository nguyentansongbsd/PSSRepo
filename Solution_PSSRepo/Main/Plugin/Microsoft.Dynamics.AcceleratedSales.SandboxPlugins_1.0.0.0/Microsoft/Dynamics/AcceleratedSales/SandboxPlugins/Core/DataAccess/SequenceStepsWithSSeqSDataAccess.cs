// Decompiled with JetBrains decompiler
// Type: Microsoft.Dynamics.AcceleratedSales.SandboxPlugins.Core.DataAccess.SequenceStepsWithSSeqSDataAccess
// Assembly: Microsoft.Dynamics.AcceleratedSales.SandboxPlugins, Version=1.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35
// MVID: 7BBE7132-7E30-4424-B38D-27667CFBCEFA
// Assembly location: C:\Users\ngoct\Downloads\New folder\Microsoft.Dynamics.AcceleratedSales.SandboxPlugins_1.0.0.0.dll

using Microsoft.Dynamics.AcceleratedSales.SandboxPlugins.Core.Model;
using Microsoft.Dynamics.AcceleratedSales.SandboxPlugins.Core.Model.Interface;
using Microsoft.Dynamics.AcceleratedSales.SandboxPlugins.Core.Services;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Query;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;

#nullable disable
namespace Microsoft.Dynamics.AcceleratedSales.SandboxPlugins.Core.DataAccess
{
  public class SequenceStepsWithSSeqSDataAccess : ISequenceStepsDataAccess
  {
    private const string SequenceTargetStepsEntityName = "msdyn_sequencetargetstep";
    private const string SequenceTargetStepIdRef = "msdyn_sequencetargetstep.msdyn_sequencetargetstepid";
    private const string SequenceStepIdRef = "msdyn_sequencetargetstep.msdyn_sequencestepid";
    private const string WaitStateRef = "msdyn_sequencetargetstep.msdyn_waitstate";
    private const string ErrorStateRef = "msdyn_sequencetargetstep.msdyn_errorstate";
    private const string StateCodeRef = "msdyn_sequencetargetstep.statecode";
    private const string StatusCodeRef = "msdyn_sequencetargetstep.statuscode";
    private const string StepNameRef = "msdyn_sequencetargetstep.msdyn_name";
    private const string StepTypeRef = "msdyn_sequencetargetstep.msdyn_type";
    private const string StepSubTypeRef = "msdyn_sequencetargetstep.msdyn_subtype";
    private const string LinkedActivityIdRef = "msdyn_sequencetargetstep.msdyn_linkedactivityid";
    private const string DescriptionRef = "msdyn_sequencetargetstep.msdyn_description";
    private const string DueTimeRef = "msdyn_sequencetargetstep.msdyn_duetime";
    private const string StatusCode = "statuscode";
    private const string SalesCadenceTargetEntity = "msdyn_sequencetarget";
    private const string TargetRef = "msdyn_target";
    private const string OwnerIdRef = "msdyn_sequencetargetstep.ownerid";
    private const string CreatedByRef = "msdyn_sequencetargetstep.createdby";
    private const string SnoozeCountRef = "msdyn_sequencetargetstep.msdyn_snoozecount";
    private const string ModifiedOnAttrRef = "msdyn_sequencetargetstep.modifiedon";
    private const string SequenceEntity = "msdyn_sequence";
    private const string SequenceTargetStepId = "msdyn_sequencetargetstepid";
    private const string SequenceStepId = "msdyn_sequencestepid";
    private const string WaitState = "msdyn_waitstate";
    private const string ErrorState = "msdyn_errorstate";
    private const string StepName = "msdyn_name";
    private const string StepType = "msdyn_type";
    private const string StepSubType = "msdyn_subtype";
    private const string LinkedActivityId = "msdyn_linkedactivityid";
    private const string Description = "msdyn_description";
    private const string DueTime = "msdyn_duetime";
    private const string CreatedBy = "createdby";
    private const string SnoozeCount = "msdyn_snoozecount";
    private const string StateCode = "statecode";
    private const string ModifiedOnAttr = "modifiedon";
    private const string CJODefinition = "msdyn_cjodefinition";
    private const string SalesCadenceTargetId = "msdyn_sequencetargetid";
    private const string SequenceTarget = "msdyn_target";
    private const string ParentSequence = "msdyn_parentsequence";
    private const string AppliedSequenceAttribute = "msdyn_appliedsequenceinstance";
    private const string OwnerId = "ownerid";
    private const int ActiveStateCode = 0;
    private const string ConnectingStatusCode = "1";
    private const string ConnectedStatusCode = "2";
    private const string CompletedStatusCode = "3";
    private const string CompletedOn = "msdyn_completedon";
    private const string CompletedOnRef = "msdyn_sequencetargetstep.msdyn_completedon";
    private const int OpenStateCode = 0;
    private const int ClosedStateCode = 1;
    private readonly string[] sequenceAttributes = new string[1]
    {
      "msdyn_cjodefinition"
    };
    private readonly string[] sequenceTargetAttributes = new string[5]
    {
      "msdyn_sequencetargetid",
      "msdyn_target",
      "msdyn_parentsequence",
      "msdyn_appliedsequenceinstance",
      "ownerid"
    };
    private readonly string[] sequenceStepAttributes = new string[16]
    {
      "msdyn_sequencetargetstepid",
      "msdyn_sequencestepid",
      "msdyn_waitstate",
      "msdyn_errorstate",
      "msdyn_name",
      "msdyn_type",
      "msdyn_subtype",
      "msdyn_duetime",
      "ownerid",
      "createdby",
      "msdyn_snoozecount",
      "msdyn_description",
      "modifiedon",
      "statecode",
      "statuscode",
      "msdyn_completedon"
    };
    private readonly string[] statusCodes = new string[3]
    {
      "1",
      "2",
      "3"
    };
    private readonly IAcceleratedSalesLogger logger;
    private readonly IDataStore dataStore;
    private readonly ITeamMembershipDataAccess teamMembership;
    private EntityCollection sequenceStepCollection = (EntityCollection) null;
    private Guid? stIdIfSSeqS = new Guid?();

    public SequenceStepsWithSSeqSDataAccess(
      IAcceleratedSalesLogger logger,
      IDataStore dataStore,
      ITeamMembershipDataAccess teamMembership)
    {
      this.dataStore = dataStore;
      this.logger = logger;
      this.teamMembership = teamMembership;
    }

    public SequenceStepRecordsData GetSequenceStepRecords(
      Guid primaryRecordId,
      List<Guid> secondaryRecordIds,
      bool isStepAndActivityLinkEnabled)
    {
      Stopwatch stopwatch = Stopwatch.StartNew();
      try
      {
        EntityCollection sequenceStepCollection = this.RetrieveSequenceStepsMemo(primaryRecordId, secondaryRecordIds, isStepAndActivityLinkEnabled);
        this.logger.LogWarning("SequenceStepsWithSSeqSDataAccess.GetSequenceStepRecords.sequenceStepCollection.Count: " + sequenceStepCollection.Entities.Count.ToString(), callerName: nameof (GetSequenceStepRecords), callerFile: "C:\\__w\\1\\s\\solutions\\AcceleratedSales\\AcceleratedSalesSandboxPlugins\\Core\\DataAccess\\SequenceStepsWithSSeqSDataAccess.cs");
        return this.ParseSequenceStepRecords(sequenceStepCollection, primaryRecordId, secondaryRecordIds, isStepAndActivityLinkEnabled);
      }
      catch (Exception ex)
      {
        this.logger.LogError("SequenceStepsWithSSeqSDataAccess.GetSequenceStepRecords.Exception", ex, callerName: nameof (GetSequenceStepRecords), callerFile: "C:\\__w\\1\\s\\solutions\\AcceleratedSales\\AcceleratedSalesSandboxPlugins\\Core\\DataAccess\\SequenceStepsWithSSeqSDataAccess.cs");
        return new SequenceStepRecordsData();
      }
      finally
      {
        stopwatch.Stop();
        this.logger.LogWarning("SequenceStepsWithSSeqSDataAccess.GetSequenceStepRecords.Duration: " + stopwatch.ElapsedMilliseconds.ToString(), callerName: nameof (GetSequenceStepRecords), callerFile: "C:\\__w\\1\\s\\solutions\\AcceleratedSales\\AcceleratedSalesSandboxPlugins\\Core\\DataAccess\\SequenceStepsWithSSeqSDataAccess.cs");
      }
    }

    public List<Guid> GetSequenceStepRecordsLinkedActivityIds(List<Guid> activityIds)
    {
      Stopwatch stopwatch = Stopwatch.StartNew();
      try
      {
        EntityCollection sequenceSteps = this.dataStore.RetrieveMultiple(this.GetSequenceStepLinkedActivityFetchQueryExpression(activityIds));
        this.logger.LogWarning("SequenceStepsWithSSeqSDataAccess.linkedSequenceStepCollection.Count: " + sequenceSteps.Entities.Count.ToString(), callerName: nameof (GetSequenceStepRecordsLinkedActivityIds), callerFile: "C:\\__w\\1\\s\\solutions\\AcceleratedSales\\AcceleratedSalesSandboxPlugins\\Core\\DataAccess\\SequenceStepsWithSSeqSDataAccess.cs");
        List<Guid> linkedActivityIds = this.ParseAndGetSequenceStepRecordLinkedActivityIds(sequenceSteps);
        this.logger.LogWarning("SequenceStepsWithSSeqSDataAccess.GetSequenceStepRecordsLinkedActivityIds.Count: " + linkedActivityIds.Count.ToString(), callerName: nameof (GetSequenceStepRecordsLinkedActivityIds), callerFile: "C:\\__w\\1\\s\\solutions\\AcceleratedSales\\AcceleratedSalesSandboxPlugins\\Core\\DataAccess\\SequenceStepsWithSSeqSDataAccess.cs");
        return linkedActivityIds;
      }
      catch (Exception ex)
      {
        this.logger.LogError("SequenceStepsWithSSeqSDataAccess.GetSequenceStepRecordsLinkedActivityIds.Exception", ex, callerName: nameof (GetSequenceStepRecordsLinkedActivityIds), callerFile: "C:\\__w\\1\\s\\solutions\\AcceleratedSales\\AcceleratedSalesSandboxPlugins\\Core\\DataAccess\\SequenceStepsWithSSeqSDataAccess.cs");
        return new List<Guid>();
      }
      finally
      {
        stopwatch.Stop();
        this.logger.LogWarning("SequenceStepsWithSSeqSDataAccess.GetSequenceStepRecordsLinkedActivityIds.Duration: " + stopwatch.ElapsedMilliseconds.ToString(), callerName: nameof (GetSequenceStepRecordsLinkedActivityIds), callerFile: "C:\\__w\\1\\s\\solutions\\AcceleratedSales\\AcceleratedSalesSandboxPlugins\\Core\\DataAccess\\SequenceStepsWithSSeqSDataAccess.cs");
      }
    }

    public AllStepsForSSeqS GetAllStepsForSSeqS(
      Guid primaryRecordId,
      List<Guid> secondaryRecordIds,
      bool isStepAndActivityLinkEnabled)
    {
      Stopwatch stopwatch = Stopwatch.StartNew();
      try
      {
        EntityCollection sequenceStepCollection = this.RetrieveSequenceStepsMemo(primaryRecordId, secondaryRecordIds, isStepAndActivityLinkEnabled);
        this.logger.LogWarning("SequenceStepsWithSSeqSDataAccess.GetAllStepsForSSeqS.sequenceStepCollection.Count: " + sequenceStepCollection.Entities.Count.ToString(), callerName: nameof (GetAllStepsForSSeqS), callerFile: "C:\\__w\\1\\s\\solutions\\AcceleratedSales\\AcceleratedSalesSandboxPlugins\\Core\\DataAccess\\SequenceStepsWithSSeqSDataAccess.cs");
        AllStepsForSSeqS allStepsForSseqS = this.ParseAllStepsForSSeqS(sequenceStepCollection, primaryRecordId, secondaryRecordIds.Count);
        stopwatch.Stop();
        this.logger.AddCustomProperty(string.Format("SequenceStepsWithSSeqSDataAccess.GetAllStepsForSSeqS.IsAllStepsNotNull: {0} Duration: ", (object) (allStepsForSseqS != null)), (object) stopwatch.ElapsedMilliseconds);
        return allStepsForSseqS;
      }
      catch (Exception ex)
      {
        stopwatch.Stop();
        this.logger.LogError("SequenceStepsWithSSeqSDataAccess.GetAllStepsForSSeqS.Exception", ex, callerName: nameof (GetAllStepsForSSeqS), callerFile: "C:\\__w\\1\\s\\solutions\\AcceleratedSales\\AcceleratedSalesSandboxPlugins\\Core\\DataAccess\\SequenceStepsWithSSeqSDataAccess.cs");
        return (AllStepsForSSeqS) null;
      }
    }

    private EntityCollection RetrieveSequenceStepsMemo(
      Guid primaryRecordId,
      List<Guid> secondaryRecordsIds,
      bool isStepAndActivityLinkEnabled)
    {
      if (this.sequenceStepCollection == null)
        this.sequenceStepCollection = this.dataStore.RetrieveMultiple(this.GetSequenceTargetRecordFetchQueryExpression(primaryRecordId, secondaryRecordsIds, isStepAndActivityLinkEnabled));
      return this.sequenceStepCollection;
    }

    private QueryExpression GetSequenceTargetRecordFetchQueryExpression(
      Guid primaryRecordId,
      List<Guid> secondaryRecordsIds,
      bool isStepAndActivityLinkEnabled)
    {
      QueryExpression fetchQueryExpression = new QueryExpression()
      {
        EntityName = "msdyn_sequencetarget"
      };
      fetchQueryExpression.ColumnSet.AddColumns(((IEnumerable<string>) this.sequenceTargetAttributes).ToArray<string>());
      fetchQueryExpression.LinkEntities.Add(this.GetSequenceTargetStepsLinkedEntity(isStepAndActivityLinkEnabled));
      FilterExpression childFilter = new FilterExpression();
      List<Guid> list = new List<Guid>() { primaryRecordId }.Concat<Guid>((IEnumerable<Guid>) secondaryRecordsIds).ToList<Guid>();
      childFilter.AddCondition(new ConditionExpression("msdyn_target", ConditionOperator.In, (ICollection) list?.ToArray()));
      if (secondaryRecordsIds.Count > 0)
        childFilter.AddCondition("statuscode", ConditionOperator.In, (object[]) ((IEnumerable<string>) this.statusCodes).ToArray<string>());
      else
        childFilter.AddCondition("statecode", ConditionOperator.Equal, (object) 0);
      fetchQueryExpression.Criteria.AddFilter(childFilter);
      return fetchQueryExpression;
    }

    private LinkEntity GetSequenceTargetStepsLinkedEntity(bool isStepAndActivityLinkEnabled)
    {
      ColumnSet columnSet = new ColumnSet(((IEnumerable<string>) this.sequenceStepAttributes).ToArray<string>());
      if (isStepAndActivityLinkEnabled)
        columnSet.AddColumns("msdyn_linkedactivityid");
      return new LinkEntity("msdyn_sequencetarget", "msdyn_sequencetargetstep", "msdyn_sequencetargetid", "msdyn_sequencetarget", JoinOperator.LeftOuter)
      {
        Columns = columnSet,
        EntityAlias = "msdyn_sequencetargetstep"
      };
    }

    private SequenceStepRecordsData ParseSequenceStepRecords(
      EntityCollection sequenceStepCollection,
      Guid primaryRecordId,
      List<Guid> secondaryRecordIds,
      bool isStepAndActivityLinkEnabled)
    {
      List<Guid> list = new List<Guid>() { primaryRecordId }.Concat<Guid>((IEnumerable<Guid>) secondaryRecordIds).ToList<Guid>();
      List<SequenceStepRecord> sequenceStepRecordList = new List<SequenceStepRecord>();
      List<Guid> guidList = new List<Guid>();
      Dictionary<string, bool> dictionary1 = new Dictionary<string, bool>();
      Dictionary<string, bool> dictionary2 = list.ToDictionary<Guid, string, bool>((Func<Guid, string>) (record => record.ToString()), (Func<Guid, bool>) (value => false));
      List<Entity> steps = this.FilterStepsOwnedByUserOrTeam(sequenceStepCollection);
      Guid stIdIfSseqSmemo = this.GetSTIdIfSSeqSMemo(steps, primaryRecordId, secondaryRecordIds.Count);
      foreach (Entity entity in steps)
      {
        this.GetFieldValue<Guid>(entity, "msdyn_sequencetargetstep.msdyn_sequencetargetstepid");
        Guid id = this.GetFieldValue<EntityReference>(entity, "msdyn_target").Id;
        dictionary2[id.ToString()] = true;
        if (isStepAndActivityLinkEnabled)
        {
          Guid fieldValue = this.GetFieldValue<Guid>(entity, "msdyn_sequencetargetstep.msdyn_linkedactivityid");
          if (fieldValue != Guid.Empty)
            guidList.Add(fieldValue);
        }
        if (stIdIfSseqSmemo != Guid.Empty && id == primaryRecordId)
          dictionary1[primaryRecordId.ToString()] = true;
        else if (this.GetFieldValue<OptionSetValue>(entity, "msdyn_sequencetargetstep.statecode")?.Value.Value == 0)
        {
          SequenceStepRecord sequenceStepRecord = this.GetFormattedSequenceStepRecord(entity, id);
          if (sequenceStepRecord != null)
          {
            sequenceStepRecordList.Add(sequenceStepRecord);
            dictionary1[id.ToString()] = true;
          }
        }
      }
      this.logger.LogWarning("SequenceStepsWithSSeqSDataAccess.ParseSequenceStepRecords.GetSequenceStepsPerRecord: Success", callerName: nameof (ParseSequenceStepRecords), callerFile: "C:\\__w\\1\\s\\solutions\\AcceleratedSales\\AcceleratedSalesSandboxPlugins\\Core\\DataAccess\\SequenceStepsWithSSeqSDataAccess.cs");
      foreach (Guid guid in list)
      {
        if (!dictionary1.ContainsKey(guid.ToString()) && dictionary2[guid.ToString()])
          sequenceStepRecordList.Add(new SequenceStepRecord()
          {
            RegardingRecordId = guid,
            StateCode = 1
          });
      }
      this.logger.LogWarning("SequenceStepsWithSSeqSDataAccess.ParseSequenceStepRecords.DummyRecordsForCompletedSequences: Added", callerName: nameof (ParseSequenceStepRecords), callerFile: "C:\\__w\\1\\s\\solutions\\AcceleratedSales\\AcceleratedSalesSandboxPlugins\\Core\\DataAccess\\SequenceStepsWithSSeqSDataAccess.cs");
      return new SequenceStepRecordsData()
      {
        SequenceStepRecords = sequenceStepRecordList,
        LinkedActivityIds = guidList
      };
    }

    private List<Entity> FilterStepsOwnedByUserOrTeam(EntityCollection sequenceStepCollection)
    {
      return sequenceStepCollection?.Entities == null ? new List<Entity>() : sequenceStepCollection.Entities.Where<Entity>((Func<Entity, bool>) (step => this.GetSTSOrSTOwnedByUserOrTeam(step))).ToList<Entity>();
    }

    private Guid GetSTIdIfSSeqSMemo(
      List<Entity> steps,
      Guid requiredTargetId,
      int secondaryRecordsCount)
    {
      if (!this.stIdIfSSeqS.HasValue)
        this.stIdIfSSeqS = new Guid?(this.GetSTIdIfSSeqS(steps, requiredTargetId, secondaryRecordsCount));
      return this.stIdIfSSeqS.Value;
    }

    private Guid GetSTIdIfSSeqS(
      List<Entity> steps,
      Guid requiredTargetId,
      int secondaryRecordsCount)
    {
      if (secondaryRecordsCount > 0)
      {
        this.logger.LogWarning(string.Format("SequenceStepsWithSSeqSDataAccess.GetSTIdIfSSeqS.{0}.SecondaryRecordsCount: ", (object) requiredTargetId) + secondaryRecordsCount.ToString(), callerName: nameof (GetSTIdIfSSeqS), callerFile: "C:\\__w\\1\\s\\solutions\\AcceleratedSales\\AcceleratedSalesSandboxPlugins\\Core\\DataAccess\\SequenceStepsWithSSeqSDataAccess.cs");
        return Guid.Empty;
      }
      Guid empty = Guid.Empty;
      foreach (Entity step in steps)
      {
        EntityReference fieldValue1 = this.GetFieldValue<EntityReference>(step, "msdyn_target");
        Guid? nullable1;
        Guid? nullable2;
        if (fieldValue1 == null)
        {
          nullable1 = new Guid?();
          nullable2 = nullable1;
        }
        else
          nullable2 = new Guid?(fieldValue1.Id);
        nullable1 = nullable2;
        Guid guid = requiredTargetId;
        if (nullable1.HasValue && nullable1.GetValueOrDefault() == guid && this.GetFieldValue<OptionSetValue>(step, "msdyn_sequencetargetstep.statecode")?.Value.Value == 0)
        {
          if (empty == Guid.Empty)
          {
            Guid? fieldValue2 = this.GetFieldValue<Guid?>(step, "msdyn_sequencetargetid");
            if (!fieldValue2.HasValue)
              this.logger.LogWarning(string.Format("SequenceStepsWithSSeqSDataAccess.GetSTIdIfSSeqS.{0}.IsSTIdNull: True", (object) requiredTargetId), callerName: nameof (GetSTIdIfSSeqS), callerFile: "C:\\__w\\1\\s\\solutions\\AcceleratedSales\\AcceleratedSalesSandboxPlugins\\Core\\DataAccess\\SequenceStepsWithSSeqSDataAccess.cs");
            else
              empty = fieldValue2.Value;
          }
          else
          {
            this.logger.LogWarning(string.Format("SequenceStepsWithSSeqSDataAccess.GetSTIdIfSSeqS.{0}.IsMultipleSequencesScenario: True", (object) requiredTargetId), callerName: nameof (GetSTIdIfSSeqS), callerFile: "C:\\__w\\1\\s\\solutions\\AcceleratedSales\\AcceleratedSalesSandboxPlugins\\Core\\DataAccess\\SequenceStepsWithSSeqSDataAccess.cs");
            return Guid.Empty;
          }
        }
      }
      this.logger.LogWarning(string.Format("SequenceStepsWithSSeqSDataAccess.GetSTIdIfSSeqS.{0}.IsSingleSequenceScenario: ", (object) requiredTargetId) + (empty != Guid.Empty).ToString(), callerName: nameof (GetSTIdIfSSeqS), callerFile: "C:\\__w\\1\\s\\solutions\\AcceleratedSales\\AcceleratedSalesSandboxPlugins\\Core\\DataAccess\\SequenceStepsWithSSeqSDataAccess.cs");
      return empty;
    }

    private bool GetSTSOrSTOwnedByUserOrTeam(Entity sts)
    {
      return this.teamMembership.IsEqualUserOrTeam(this.GetFieldValue<EntityReference>(sts, "ownerid")?.Id.Value) || this.teamMembership.IsEqualUserOrTeam(this.GetFieldValue<EntityReference>(sts, "msdyn_sequencetargetstep.ownerid")?.Id.Value);
    }

    private SequenceStepRecord GetFormattedSequenceStepRecord(
      Entity sequenceStepRecord,
      Guid regardingRecordId)
    {
      try
      {
        EntityReference fieldValue1 = this.GetFieldValue<EntityReference>(sequenceStepRecord, "msdyn_parentsequence");
        EntityReference fieldValue2 = this.GetFieldValue<EntityReference>(sequenceStepRecord, "msdyn_appliedsequenceinstance");
        EntityReference fieldValue3 = this.GetFieldValue<EntityReference>(sequenceStepRecord, "msdyn_sequencetargetstep.ownerid");
        EntityReference fieldValue4 = this.GetFieldValue<EntityReference>(sequenceStepRecord, "msdyn_sequencetargetstep.createdby");
        bool flag = true;
        if (fieldValue3 != null)
          flag = this.teamMembership.IsEqualUserOrTeam(fieldValue3.Id);
        SequenceStepRecord sequenceStepRecord1 = new SequenceStepRecord();
        sequenceStepRecord1.StepRecordId = this.GetFieldValue<Guid>(sequenceStepRecord, "msdyn_sequencetargetstep.msdyn_sequencetargetstepid");
        sequenceStepRecord1.SalesCadenceStepId = this.GetFieldValue<Guid>(sequenceStepRecord, "msdyn_sequencetargetstep.msdyn_sequencestepid");
        sequenceStepRecord1.WaitState = this.GetFieldValue<OptionSetValue>(sequenceStepRecord, "msdyn_sequencetargetstep.msdyn_waitstate")?.Value;
        int? nullable1 = this.GetFieldValue<OptionSetValue>(sequenceStepRecord, "msdyn_sequencetargetstep.statecode")?.Value;
        sequenceStepRecord1.StateCode = nullable1.Value;
        OptionSetValue fieldValue5 = this.GetFieldValue<OptionSetValue>(sequenceStepRecord, "msdyn_sequencetargetstep.statuscode");
        int? nullable2;
        if (fieldValue5 == null)
        {
          nullable1 = new int?();
          nullable2 = nullable1;
        }
        else
          nullable2 = new int?(fieldValue5.Value);
        nullable1 = nullable2;
        sequenceStepRecord1.StatusCode = new int?(nullable1.Value);
        OptionSetValue fieldValue6 = this.GetFieldValue<OptionSetValue>(sequenceStepRecord, "msdyn_sequencetargetstep.msdyn_errorstate");
        int? nullable3;
        if (fieldValue6 == null)
        {
          nullable1 = new int?();
          nullable3 = nullable1;
        }
        else
          nullable3 = new int?(fieldValue6.Value);
        sequenceStepRecord1.ErrorState = nullable3;
        sequenceStepRecord1.StepName = this.GetFieldValue<string>(sequenceStepRecord, "msdyn_sequencetargetstep.msdyn_name");
        sequenceStepRecord1.Description = this.GetFieldValue<string>(sequenceStepRecord, "msdyn_sequencetargetstep.msdyn_description");
        OptionSetValue fieldValue7 = this.GetFieldValue<OptionSetValue>(sequenceStepRecord, "msdyn_sequencetargetstep.msdyn_type");
        int? nullable4;
        if (fieldValue7 == null)
        {
          nullable1 = new int?();
          nullable4 = nullable1;
        }
        else
          nullable4 = new int?(fieldValue7.Value);
        sequenceStepRecord1.StepType = nullable4;
        OptionSetValue fieldValue8 = this.GetFieldValue<OptionSetValue>(sequenceStepRecord, "msdyn_sequencetargetstep.msdyn_subtype");
        int? nullable5;
        if (fieldValue8 == null)
        {
          nullable1 = new int?();
          nullable5 = nullable1;
        }
        else
          nullable5 = new int?(fieldValue8.Value);
        sequenceStepRecord1.StepSubType = nullable5;
        sequenceStepRecord1.SalesCadenceId = fieldValue1?.Id;
        sequenceStepRecord1.SalesCadenceName = fieldValue1.Name;
        sequenceStepRecord1.AppliedCadenceId = fieldValue2?.Id;
        sequenceStepRecord1.AppliedCadenceName = fieldValue2.Name;
        sequenceStepRecord1.ExpiryDate = this.GetFieldValue<DateTime>(sequenceStepRecord, "msdyn_sequencetargetstep.msdyn_duetime");
        sequenceStepRecord1.LinkedActivityId = this.GetFieldValue<Guid?>(sequenceStepRecord, "msdyn_sequencetargetstep.msdyn_linkedactivityid");
        sequenceStepRecord1.RegardingRecordId = regardingRecordId;
        sequenceStepRecord1.OwnedById = fieldValue3?.Id;
        sequenceStepRecord1.OwnedByName = fieldValue3?.Name;
        sequenceStepRecord1.CreatedById = fieldValue4?.Id;
        sequenceStepRecord1.CreatedByName = fieldValue4?.Name;
        sequenceStepRecord1.SnoozeCount = this.GetFieldValue<int>(sequenceStepRecord, "msdyn_sequencetargetstep.msdyn_snoozecount");
        sequenceStepRecord1.ModifiedDate = this.GetFieldValue<DateTime>(sequenceStepRecord, "msdyn_sequencetargetstep.modifiedon");
        sequenceStepRecord1.CompletedOn = this.GetFieldValue<DateTime?>(sequenceStepRecord, "msdyn_sequencetargetstep.msdyn_completedon");
        sequenceStepRecord1.IsOwnedByUserOrTeam = flag;
        return sequenceStepRecord1;
      }
      catch (Exception ex)
      {
        this.logger.LogError("SequenceStepsWithSSeqSDataAccess.GetFormattedSequenceStepRecord.Exception", ex, callerName: nameof (GetFormattedSequenceStepRecord), callerFile: "C:\\__w\\1\\s\\solutions\\AcceleratedSales\\AcceleratedSalesSandboxPlugins\\Core\\DataAccess\\SequenceStepsWithSSeqSDataAccess.cs");
      }
      return (SequenceStepRecord) null;
    }

    private T GetFieldValue<T>(Entity record, string fieldName)
    {
      try
      {
        if (record.Contains(fieldName))
          return record[fieldName].GetType() == typeof (AliasedValue) ? (T) ((AliasedValue) record[fieldName]).Value : (T) record[fieldName];
      }
      catch (Exception ex)
      {
        this.logger.LogError("SequenceStepsWithSSeqSDataAccess.GetFormattedSequenceStepRecord.GetFieldValue.Exception", ex, callerName: nameof (GetFieldValue), callerFile: "C:\\__w\\1\\s\\solutions\\AcceleratedSales\\AcceleratedSalesSandboxPlugins\\Core\\DataAccess\\SequenceStepsWithSSeqSDataAccess.cs");
      }
      return default (T);
    }

    private QueryExpression GetSequenceStepLinkedActivityFetchQueryExpression(List<Guid> activityIds)
    {
      QueryExpression fetchQueryExpression = new QueryExpression()
      {
        EntityName = "msdyn_sequencetargetstep"
      };
      fetchQueryExpression.ColumnSet.AddColumns("msdyn_linkedactivityid");
      FilterExpression childFilter = new FilterExpression();
      childFilter.AddCondition(new ConditionExpression("msdyn_linkedactivityid", ConditionOperator.In, (ICollection) activityIds.ToArray()));
      fetchQueryExpression.Criteria.AddFilter(childFilter);
      return fetchQueryExpression;
    }

    private List<Guid> ParseAndGetSequenceStepRecordLinkedActivityIds(EntityCollection sequenceSteps)
    {
      List<Guid> linkedActivityIds = new List<Guid>();
      foreach (Entity entity in (Collection<Entity>) sequenceSteps?.Entities)
      {
        Guid fieldValue = this.GetFieldValue<Guid>(entity, "msdyn_linkedactivityid");
        linkedActivityIds.Add(fieldValue);
      }
      return linkedActivityIds;
    }

    private AllStepsForSSeqS ParseAllStepsForSSeqS(
      EntityCollection sequenceStepCollection,
      Guid requiredTargetId,
      int secondaryRecordsCount)
    {
      List<Entity> steps = this.FilterStepsOwnedByUserOrTeam(sequenceStepCollection);
      Guid stIdIfSseqSmemo = this.GetSTIdIfSSeqSMemo(steps, requiredTargetId, secondaryRecordsCount);
      if (stIdIfSseqSmemo == Guid.Empty)
        return (AllStepsForSSeqS) null;
      SequenceStepRecord activeStep = (SequenceStepRecord) null;
      List<UpcomingSequenceStepRecord> sequenceStepRecordList1 = (List<UpcomingSequenceStepRecord>) null;
      List<SequenceStepRecord> sequenceStepRecordList2 = new List<SequenceStepRecord>();
      foreach (Entity entity in steps)
      {
        if (!(this.GetFieldValue<Guid>(entity, "msdyn_sequencetargetid") != stIdIfSseqSmemo))
        {
          switch (this.GetFieldValue<OptionSetValue>(entity, "msdyn_sequencetargetstep.statecode")?.Value.Value)
          {
            case 0:
              if (activeStep != null)
              {
                this.logger.AddCustomProperty("SequenceStepsWithSSeqSDataAccess.ParseAllStepsForSSeqS.MultipleActiveSteps", (object) "True");
                continue;
              }
              activeStep = this.GetFormattedSequenceStepRecord(entity, requiredTargetId);
              sequenceStepRecordList1 = this.ParseUpcomingSteps(activeStep);
              break;
            case 1:
              sequenceStepRecordList2.Add(this.GetFormattedSequenceStepRecord(entity, requiredTargetId));
              break;
          }
        }
      }
      return new AllStepsForSSeqS()
      {
        ActiveStep = activeStep,
        UpcomingSteps = sequenceStepRecordList1,
        CompletedSteps = sequenceStepRecordList2
      };
    }

    private List<UpcomingSequenceStepRecord> ParseUpcomingSteps(SequenceStepRecord activeStep)
    {
      Stopwatch stopwatch = Stopwatch.StartNew();
      try
      {
        string cjoDefinition = this.GetCJODefinition(activeStep);
        if (cjoDefinition == null)
          return new List<UpcomingSequenceStepRecord>();
        OrganizationResponse organizationResponse = this.dataStore.Execute(new OrganizationRequest("msdyn_GetCJOTreeNodes")
        {
          ["msdyn_cjodefinition"] = (object) cjoDefinition,
          ["msdyn_startingnodeid"] = (object) activeStep.SalesCadenceStepId.ToString()
        });
        string str;
        if (organizationResponse == null || organizationResponse.Results == null || !organizationResponse.Results.TryGetValue<string>("msdyn_nodes", ref str))
        {
          this.logger.LogWarning("SequenceStepsWithSSeqSDataAccess.ParseUpcomingSteps.NullResponse: true", callerName: nameof (ParseUpcomingSteps), callerFile: "C:\\__w\\1\\s\\solutions\\AcceleratedSales\\AcceleratedSalesSandboxPlugins\\Core\\DataAccess\\SequenceStepsWithSSeqSDataAccess.cs");
          return new List<UpcomingSequenceStepRecord>();
        }
        List<UpcomingSequenceStepRecord> list = JsonService.DeserializeObject<List<CJOTreeNode>>(str).Select<CJOTreeNode, UpcomingSequenceStepRecord>((Func<CJOTreeNode, UpcomingSequenceStepRecord>) (node => this.ParseUpcomingStep(node))).ToList<UpcomingSequenceStepRecord>();
        this.logger.LogWarning("SequenceStepsWithSSeqSDataAccess.ParseUpcomingSteps.Count: " + list.Count.ToString(), callerName: nameof (ParseUpcomingSteps), callerFile: "C:\\__w\\1\\s\\solutions\\AcceleratedSales\\AcceleratedSalesSandboxPlugins\\Core\\DataAccess\\SequenceStepsWithSSeqSDataAccess.cs");
        return list;
      }
      catch (Exception ex)
      {
        this.logger.LogError("SequenceStepsWithSSeqSDataAccess.ParseUpcomingSteps.Exception", ex, callerName: nameof (ParseUpcomingSteps), callerFile: "C:\\__w\\1\\s\\solutions\\AcceleratedSales\\AcceleratedSalesSandboxPlugins\\Core\\DataAccess\\SequenceStepsWithSSeqSDataAccess.cs");
        return new List<UpcomingSequenceStepRecord>();
      }
      finally
      {
        stopwatch.Stop();
        this.logger.LogWarning("SequenceStepsWithSSeqSDataAccess.ParseUpcomingSteps.Duration: " + stopwatch.ElapsedMilliseconds.ToString(), callerName: nameof (ParseUpcomingSteps), callerFile: "C:\\__w\\1\\s\\solutions\\AcceleratedSales\\AcceleratedSalesSandboxPlugins\\Core\\DataAccess\\SequenceStepsWithSSeqSDataAccess.cs");
      }
    }

    private UpcomingSequenceStepRecord ParseUpcomingStep(CJOTreeNode node)
    {
      if (node?.parameters?.inputs == null)
        return (UpcomingSequenceStepRecord) null;
      int stepType;
      int stepSubType;
      this.GetSequenceStepTypesFromCJOTreeNodeTypes(node.parameters.type, node.parameters.subtype, node.parameters.actionsubtype, out stepType, out stepSubType);
      return new UpcomingSequenceStepRecord()
      {
        StepName = node.parameters.inputs.name,
        StepType = new int?(stepType),
        StepSubType = new int?(stepSubType),
        Description = node.parameters.inputs.description,
        DispositionOn = node.parameters.inputs.expression?.rules?[0]?.dispositionOn,
        AutoActionType = node.parameters.inputs.autoActionType,
        UpdateFieldAttributeDisplayName = node.parameters.inputs.autoActionDetails?.attributeDisplayName,
        UpdateFieldAttributeDisplayValue = node.parameters.inputs.autoActionDetails?.optionSetDisplayName
      };
    }

    private string GetCJODefinition(SequenceStepRecord step)
    {
      Guid? appliedCadenceId;
      int num;
      if (step == null)
      {
        num = 1;
      }
      else
      {
        appliedCadenceId = step.AppliedCadenceId;
        num = !appliedCadenceId.HasValue ? 1 : 0;
      }
      if (num != 0)
      {
        this.logger.LogWarning("SequenceStepsWithSSeqSDataAccess.GetCJODefinition.StepOrAppliedSequenceIdNull: true", callerName: nameof (GetCJODefinition), callerFile: "C:\\__w\\1\\s\\solutions\\AcceleratedSales\\AcceleratedSalesSandboxPlugins\\Core\\DataAccess\\SequenceStepsWithSSeqSDataAccess.cs");
        return (string) null;
      }
      Stopwatch stopwatch = Stopwatch.StartNew();
      IDataStore dataStore = this.dataStore;
      appliedCadenceId = step.AppliedCadenceId;
      Guid id = appliedCadenceId.Value;
      ColumnSet columnSet = new ColumnSet(this.sequenceAttributes);
      Entity record = dataStore.Retrieve("msdyn_sequence", id, columnSet);
      stopwatch.Stop();
      this.logger.LogWarning("SequenceStepsWithSSeqSDataAccess.GetCJODefinition.Retrieve.Duration: " + stopwatch.ElapsedMilliseconds.ToString(), callerName: nameof (GetCJODefinition), callerFile: "C:\\__w\\1\\s\\solutions\\AcceleratedSales\\AcceleratedSalesSandboxPlugins\\Core\\DataAccess\\SequenceStepsWithSSeqSDataAccess.cs");
      return this.GetFieldValue<string>(record, "msdyn_cjodefinition");
    }

    private void GetSequenceStepTypesFromCJOTreeNodeTypes(
      string type,
      string subtype,
      string actionSubType,
      out int stepType,
      out int stepSubType)
    {
      stepType = 9999999;
      stepSubType = 0;
      switch (type)
      {
        case "Condition":
          stepType = 1;
          break;
        case "Experimentation":
          stepType = 7;
          break;
        default:
          string s = subtype;
          // ISSUE: reference to a compiler-generated method
          switch (\u003C4163a359\u002Daa19\u002D444d\u002Db224\u002D74a045856c4c\u003E\u003CPrivateImplementationDetails\u003E.ComputeStringHash(s))
          {
            case 198508992:
              if (!(s == "Wait"))
                return;
              stepType = 0;
              return;
            case 612461970:
              if (!(s == "AutomatedSms"))
                return;
              stepType = 6;
              return;
            case 1036339050:
              if (!(s == "Sms"))
                return;
              stepType = 4213;
              return;
            case 1127555431:
              if (!(s == "Email"))
                return;
              stepType = 4202;
              return;
            case 1208378236:
              if (!(s == "AutoAction"))
                return;
              stepType = 4;
              return;
            case 1995390348:
              if (!(s == "Task"))
                return;
              stepType = 4212;
              return;
            case 3301837285:
              if (!(s == "LinkedInAction"))
                return;
              stepType = 5;
              switch (actionSubType)
              {
                case "LinkedInResearch":
                  stepSubType = 1;
                  return;
                case "LinkedInGetIntroduced":
                  stepSubType = 2;
                  return;
                case "LinkedInConnect":
                  stepSubType = 3;
                  return;
                case "LinkedInMail":
                  stepSubType = 4;
                  return;
                default:
                  return;
              }
            case 4117115599:
              if (!(s == "AutomatedEmail"))
                return;
              stepType = 3;
              return;
            case 4272880959:
              if (!(s == "PhoneCall"))
                return;
              stepType = 4210;
              return;
            default:
              return;
          }
      }
    }
  }
}
