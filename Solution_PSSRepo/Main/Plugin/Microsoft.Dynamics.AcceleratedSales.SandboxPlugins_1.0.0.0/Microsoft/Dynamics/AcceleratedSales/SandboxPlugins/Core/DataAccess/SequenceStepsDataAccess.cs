// Decompiled with JetBrains decompiler
// Type: Microsoft.Dynamics.AcceleratedSales.SandboxPlugins.Core.DataAccess.SequenceStepsDataAccess
// Assembly: Microsoft.Dynamics.AcceleratedSales.SandboxPlugins, Version=1.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35
// MVID: 7BBE7132-7E30-4424-B38D-27667CFBCEFA
// Assembly location: C:\Users\ngoct\Downloads\New folder\Microsoft.Dynamics.AcceleratedSales.SandboxPlugins_1.0.0.0.dll

using Microsoft.Dynamics.AcceleratedSales.SandboxPlugins.Core.Model;
using Microsoft.Dynamics.AcceleratedSales.SandboxPlugins.Core.Model.Interface;
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
  public class SequenceStepsDataAccess : ISequenceStepsDataAccess
  {
    private const string SequenceTargetStepsEntityName = "msdyn_sequencetargetstep";
    private const string SequenceTargetStepIdRef = "msdyn_sequencetargetstep.msdyn_sequencetargetstepid";
    private const string SequenceStepIdRef = "msdyn_sequencetargetstep.msdyn_sequencestepid";
    private const string WaitStateRef = "msdyn_sequencetargetstep.msdyn_waitstate";
    private const string ErrorStateRef = "msdyn_sequencetargetstep.msdyn_errorstate";
    private const string StateCodeRef = "msdyn_sequencetargetstep.statecode";
    private const string StepNameRef = "msdyn_sequencetargetstep.msdyn_name";
    private const string StepTypeRef = "msdyn_sequencetargetstep.msdyn_type";
    private const string StepSubTypeRef = "msdyn_sequencetargetstep.msdyn_subtype";
    private const string LinkedActivityIdRef = "msdyn_sequencetargetstep.msdyn_linkedactivityid";
    private const string DescriptionRef = "msdyn_sequencetargetstep.msdyn_description";
    private const string DueTimeRef = "msdyn_sequencetargetstep.msdyn_duetime";
    private const int OpenStateCode = 0;
    private const string StatusCode = "statuscode";
    private const string SequenceTarget = "msdyn_target";
    private const string SalesCadenceTargetEntity = "msdyn_sequencetarget";
    private const string SalesCadenceTargetId = "msdyn_sequencetargetid";
    private const string ParentSequence = "msdyn_parentsequence";
    private const string AppliedSequenceAttribute = "msdyn_appliedsequenceinstance";
    private const string SequenceTargetRef = "msdyn_target";
    private const string OwnerIdRef = "msdyn_sequencetargetstep.ownerid";
    private const string CreatedByRef = "msdyn_sequencetargetstep.createdby";
    private const string SnoozeCountRef = "msdyn_sequencetargetstep.msdyn_snoozecount";
    private const string ModifiedOnAttrRef = "msdyn_sequencetargetstep.modifiedon";
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
    private const string OwnerId = "ownerid";
    private const string CreatedBy = "createdby";
    private const string SnoozeCount = "msdyn_snoozecount";
    private const string StateCode = "statecode";
    private const string ModifiedOnAttr = "modifiedon";
    private const string ConnectingStatusCode = "1";
    private const string ConnectedStatusCode = "2";
    private const string CompletedStatusCode = "3";
    private readonly string[] sequenceTargetAttributes = new string[4]
    {
      "msdyn_sequencetargetid",
      "msdyn_target",
      "msdyn_parentsequence",
      "msdyn_appliedsequenceinstance"
    };
    private readonly string[] statusCodes = new string[3]
    {
      "1",
      "2",
      "3"
    };
    private readonly IAcceleratedSalesLogger logger;
    private readonly IDataStore dataStore;
    private readonly string[] sequenceStepAttributes = new string[14]
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
      "statecode"
    };

    public SequenceStepsDataAccess(IAcceleratedSalesLogger logger, IDataStore dataStore)
    {
      this.dataStore = dataStore;
      this.logger = logger;
    }

    public SequenceStepRecordsData GetSequenceStepRecords(
      Guid primaryRecordId,
      List<Guid> records,
      bool isStepAndActivityLinkEnabled)
    {
      Stopwatch stopwatch = Stopwatch.StartNew();
      try
      {
        List<Guid> list = records.Concat<Guid>((IEnumerable<Guid>) new List<Guid>()
        {
          primaryRecordId
        }).ToList<Guid>();
        EntityCollection sequenceStepCollection = this.dataStore.RetrieveMultiple(this.GetSequenceTargetRecordFetchQueryExpression(list, isStepAndActivityLinkEnabled));
        this.logger.LogWarning("SequenceStepsDataAccess.sequenceStepCollection.Count: " + sequenceStepCollection.Entities.Count.ToString(), callerName: nameof (GetSequenceStepRecords), callerFile: "C:\\__w\\1\\s\\solutions\\AcceleratedSales\\AcceleratedSalesSandboxPlugins\\Core\\DataAccess\\SequenceStepsDataAccess.cs");
        SequenceStepRecordsData sequenceStepRecords = this.ParseSequenceStepRecords(sequenceStepCollection, list, isStepAndActivityLinkEnabled);
        stopwatch.Stop();
        this.logger.LogWarning("SequenceStepsDataAccess.GetSequenceStepRecords.Success.Duration: " + stopwatch.ElapsedMilliseconds.ToString(), callerName: nameof (GetSequenceStepRecords), callerFile: "C:\\__w\\1\\s\\solutions\\AcceleratedSales\\AcceleratedSalesSandboxPlugins\\Core\\DataAccess\\SequenceStepsDataAccess.cs");
        return sequenceStepRecords;
      }
      catch (Exception ex)
      {
        stopwatch.Stop();
        this.logger.LogError("SequenceStepsDataAccess.GetSequenceStepRecords.Failure.Exception", ex, callerName: nameof (GetSequenceStepRecords), callerFile: "C:\\__w\\1\\s\\solutions\\AcceleratedSales\\AcceleratedSalesSandboxPlugins\\Core\\DataAccess\\SequenceStepsDataAccess.cs");
        return new SequenceStepRecordsData();
      }
    }

    public List<Guid> GetSequenceStepRecordsLinkedActivityIds(List<Guid> activityIds)
    {
      Stopwatch stopwatch = Stopwatch.StartNew();
      try
      {
        EntityCollection sequenceSteps = this.dataStore.RetrieveMultiple(this.GetSequenceStepLinkedActivityFetchQueryExpression(activityIds));
        this.logger.AddCustomProperty("SequenceStepsDataAccess.linkedSequenceStepCollection.Count", (object) sequenceSteps.Entities.Count);
        List<Guid> linkedActivityIds = this.ParseAndGetSequenceStepRecordLinkedActivityIds(sequenceSteps);
        stopwatch.Stop();
        this.logger.LogWarning("SequenceStepsDataAccess.GetSequenceStepRecordsLinkedActivityIds.Duration: " + stopwatch.ElapsedMilliseconds.ToString(), callerName: nameof (GetSequenceStepRecordsLinkedActivityIds), callerFile: "C:\\__w\\1\\s\\solutions\\AcceleratedSales\\AcceleratedSalesSandboxPlugins\\Core\\DataAccess\\SequenceStepsDataAccess.cs");
        return linkedActivityIds;
      }
      catch (Exception ex)
      {
        stopwatch.Stop();
        this.logger.LogWarning("SequenceStepsDataAccess.GetSequenceStepRecordsLinkedActivityIds.Duration: " + stopwatch.ElapsedMilliseconds.ToString(), callerName: nameof (GetSequenceStepRecordsLinkedActivityIds), callerFile: "C:\\__w\\1\\s\\solutions\\AcceleratedSales\\AcceleratedSalesSandboxPlugins\\Core\\DataAccess\\SequenceStepsDataAccess.cs");
        this.logger.LogError("SequenceStepsDataAccess.GetSequenceStepRecordsLinkedActivityIds.Exception", ex, callerName: nameof (GetSequenceStepRecordsLinkedActivityIds), callerFile: "C:\\__w\\1\\s\\solutions\\AcceleratedSales\\AcceleratedSalesSandboxPlugins\\Core\\DataAccess\\SequenceStepsDataAccess.cs");
        return new List<Guid>();
      }
    }

    public AllStepsForSSeqS GetAllStepsForSSeqS(
      Guid primaryRecordId,
      List<Guid> records,
      bool isStepAndActivityLinkEnabled)
    {
      return (AllStepsForSSeqS) null;
    }

    private QueryExpression GetSequenceTargetRecordFetchQueryExpression(
      List<Guid> records,
      bool isStepAndActivityLinkEnabled)
    {
      QueryExpression fetchQueryExpression = new QueryExpression()
      {
        EntityName = "msdyn_sequencetarget"
      };
      fetchQueryExpression.ColumnSet.AddColumns(((IEnumerable<string>) this.sequenceTargetAttributes).ToArray<string>());
      fetchQueryExpression.LinkEntities.Add(this.GetSequenceTargetStepsLinkedEntity(isStepAndActivityLinkEnabled));
      FilterExpression childFilter = new FilterExpression();
      childFilter.AddCondition(new ConditionExpression("msdyn_target", ConditionOperator.In, (ICollection) records?.ToArray()));
      childFilter.AddCondition("statuscode", ConditionOperator.In, (object[]) ((IEnumerable<string>) this.statusCodes).ToArray<string>());
      fetchQueryExpression.Criteria.AddFilter(childFilter);
      return fetchQueryExpression;
    }

    private LinkEntity GetSequenceTargetStepsLinkedEntity(bool isStepAndActivityLinkEnabled)
    {
      ColumnSet columnSet = new ColumnSet(((IEnumerable<string>) this.sequenceStepAttributes).ToArray<string>());
      if (isStepAndActivityLinkEnabled)
        columnSet.AddColumns("msdyn_linkedactivityid");
      LinkEntity stepsLinkedEntity = new LinkEntity("msdyn_sequencetarget", "msdyn_sequencetargetstep", "msdyn_sequencetargetid", "msdyn_sequencetarget", JoinOperator.LeftOuter)
      {
        Columns = columnSet,
        EntityAlias = "msdyn_sequencetargetstep"
      };
      FilterExpression childFilter = new FilterExpression();
      childFilter.AddFilter(LogicalOperator.And);
      childFilter.AddCondition("statecode", ConditionOperator.Equal, (object) 0);
      stepsLinkedEntity.LinkCriteria.AddFilter(childFilter);
      return stepsLinkedEntity;
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
        Guid attributeValue = entity.GetAttributeValue<Guid>("msdyn_linkedactivityid");
        linkedActivityIds.Add(attributeValue);
      }
      return linkedActivityIds;
    }

    private SequenceStepRecordsData ParseSequenceStepRecords(
      EntityCollection sequenceStepCollection,
      List<Guid> records,
      bool isStepAndActivityLinkEnabled)
    {
      List<SequenceStepRecord> sequenceStepRecordList = new List<SequenceStepRecord>();
      List<Guid> guidList = new List<Guid>();
      Dictionary<string, bool> dictionary1 = new Dictionary<string, bool>();
      Dictionary<string, bool> dictionary2 = records.ToDictionary<Guid, string, bool>((Func<Guid, string>) (record => record.ToString()), (Func<Guid, bool>) (value => false));
      foreach (Entity entity in (Collection<Entity>) sequenceStepCollection.Entities)
      {
        Guid fieldValue1 = this.GetFieldValue<Guid>(entity, "msdyn_sequencetargetstep.msdyn_sequencetargetstepid");
        Guid empty = Guid.Empty;
        Guid guid = fieldValue1;
        Guid id = entity.GetAttributeValue<EntityReference>("msdyn_target").Id;
        dictionary2[id.ToString()] = true;
        if (!dictionary1.ContainsKey(id.ToString()) && guid != Guid.Empty)
        {
          if (this.GetFieldValue<OptionSetValue>(entity, "msdyn_sequencetargetstep.statecode")?.Value.Value == 0)
          {
            SequenceStepRecord sequenceStepRecord = this.GetFormattedSequenceStepRecord(entity, id);
            if (sequenceStepRecord != null)
            {
              sequenceStepRecordList.Add(sequenceStepRecord);
              dictionary1[id.ToString()] = true;
            }
          }
          if (isStepAndActivityLinkEnabled)
          {
            Guid fieldValue2 = this.GetFieldValue<Guid>(entity, "msdyn_sequencetargetstep.msdyn_linkedactivityid");
            if (fieldValue2 != Guid.Empty)
              guidList.Add(fieldValue2);
          }
        }
      }
      foreach (Guid record in records)
      {
        if (!dictionary1.ContainsKey(record.ToString()) && dictionary2[record.ToString()])
          sequenceStepRecordList.Add(new SequenceStepRecord()
          {
            RegardingRecordId = record,
            StateCode = 1
          });
      }
      this.logger.LogWarning("SequenceStepsDataAccess.ParseSequenceStepRecords.DummyRecordsForCompletedSequences: Added", callerName: nameof (ParseSequenceStepRecords), callerFile: "C:\\__w\\1\\s\\solutions\\AcceleratedSales\\AcceleratedSalesSandboxPlugins\\Core\\DataAccess\\SequenceStepsDataAccess.cs");
      return new SequenceStepRecordsData()
      {
        SequenceStepRecords = sequenceStepRecordList,
        LinkedActivityIds = guidList
      };
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
        SequenceStepRecord sequenceStepRecord1 = new SequenceStepRecord();
        sequenceStepRecord1.StepRecordId = this.GetFieldValue<Guid>(sequenceStepRecord, "msdyn_sequencetargetstep.msdyn_sequencetargetstepid");
        sequenceStepRecord1.SalesCadenceStepId = this.GetFieldValue<Guid>(sequenceStepRecord, "msdyn_sequencetargetstep.msdyn_sequencestepid");
        sequenceStepRecord1.WaitState = this.GetFieldValue<OptionSetValue>(sequenceStepRecord, "msdyn_sequencetargetstep.msdyn_waitstate")?.Value;
        int? nullable1 = this.GetFieldValue<OptionSetValue>(sequenceStepRecord, "msdyn_sequencetargetstep.statecode")?.Value;
        sequenceStepRecord1.StateCode = nullable1.Value;
        OptionSetValue fieldValue5 = this.GetFieldValue<OptionSetValue>(sequenceStepRecord, "msdyn_sequencetargetstep.msdyn_errorstate");
        int? nullable2;
        if (fieldValue5 == null)
        {
          nullable1 = new int?();
          nullable2 = nullable1;
        }
        else
          nullable2 = new int?(fieldValue5.Value);
        sequenceStepRecord1.ErrorState = nullable2;
        sequenceStepRecord1.StepName = this.GetFieldValue<string>(sequenceStepRecord, "msdyn_sequencetargetstep.msdyn_name");
        sequenceStepRecord1.Description = this.GetFieldValue<string>(sequenceStepRecord, "msdyn_sequencetargetstep.msdyn_description");
        OptionSetValue fieldValue6 = this.GetFieldValue<OptionSetValue>(sequenceStepRecord, "msdyn_sequencetargetstep.msdyn_type");
        int? nullable3;
        if (fieldValue6 == null)
        {
          nullable1 = new int?();
          nullable3 = nullable1;
        }
        else
          nullable3 = new int?(fieldValue6.Value);
        sequenceStepRecord1.StepType = nullable3;
        OptionSetValue fieldValue7 = this.GetFieldValue<OptionSetValue>(sequenceStepRecord, "msdyn_sequencetargetstep.msdyn_subtype");
        int? nullable4;
        if (fieldValue7 == null)
        {
          nullable1 = new int?();
          nullable4 = nullable1;
        }
        else
          nullable4 = new int?(fieldValue7.Value);
        sequenceStepRecord1.StepSubType = nullable4;
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
        return sequenceStepRecord1;
      }
      catch (Exception ex)
      {
        this.logger.LogError("SequenceStepsDataAccess.GetFormattedSequenceStepRecord", ex, callerName: nameof (GetFormattedSequenceStepRecord), callerFile: "C:\\__w\\1\\s\\solutions\\AcceleratedSales\\AcceleratedSalesSandboxPlugins\\Core\\DataAccess\\SequenceStepsDataAccess.cs");
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
        this.logger.LogError("SequenceStepsDataAccess.GetFormattedSequenceStepRecord.GetFieldValue.Exception", ex, callerName: nameof (GetFieldValue), callerFile: "C:\\__w\\1\\s\\solutions\\AcceleratedSales\\AcceleratedSalesSandboxPlugins\\Core\\DataAccess\\SequenceStepsDataAccess.cs");
      }
      return default (T);
    }
  }
}
