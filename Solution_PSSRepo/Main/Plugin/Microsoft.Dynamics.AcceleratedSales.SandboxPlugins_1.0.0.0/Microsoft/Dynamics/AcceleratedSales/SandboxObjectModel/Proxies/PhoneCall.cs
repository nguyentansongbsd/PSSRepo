// Decompiled with JetBrains decompiler
// Type: Microsoft.Dynamics.AcceleratedSales.SandboxObjectModel.Proxies.PhoneCall
// Assembly: Microsoft.Dynamics.AcceleratedSales.SandboxPlugins, Version=1.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35
// MVID: 7BBE7132-7E30-4424-B38D-27667CFBCEFA
// Assembly location: C:\Users\ngoct\Downloads\New folder\Microsoft.Dynamics.AcceleratedSales.SandboxPlugins_1.0.0.0.dll

using Microsoft.Xrm.Sdk;
using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Runtime.Serialization;

#nullable disable
namespace Microsoft.Dynamics.AcceleratedSales.SandboxObjectModel.Proxies
{
  [Microsoft.Xrm.Sdk.Client.EntityLogicalName("phonecall")]
  [DataContract]
  internal class PhoneCall : Entity, INotifyPropertyChanging, INotifyPropertyChanged
  {
    public const string EntityLogicalName = "phonecall";
    public const string EntitySchemaName = "PhoneCall";
    public const string PrimaryIdAttribute = "activityid";
    public const string PrimaryNameAttribute = "subject";
    public const string EntityLogicalCollectionName = "phonecalls";
    public const string EntitySetName = "phonecalls";
    public const int EntityTypeCode = 4210;

    [DebuggerNonUserCode]
    public PhoneCall()
      : base("phonecall")
    {
    }

    [DebuggerNonUserCode]
    public PhoneCall(Guid id)
      : base("phonecall", id)
    {
    }

    [DebuggerNonUserCode]
    public PhoneCall(string keyName, object keyValue)
      : base("phonecall", keyName, keyValue)
    {
    }

    [DebuggerNonUserCode]
    public PhoneCall(KeyAttributeCollection keyAttributes)
      : base("phonecall", keyAttributes)
    {
    }

    public event PropertyChangedEventHandler PropertyChanged;

    public event PropertyChangingEventHandler PropertyChanging;

    public static class Fields
    {
      public const string ActivityAdditionalParams = "activityadditionalparams";
      public const string ActivityId = "activityid";
      public const string Id = "activityid";
      public const string ActivityTypeCode = "activitytypecode";
      public const string ActualDurationMinutes = "actualdurationminutes";
      public const string ActualEnd = "actualend";
      public const string ActualStart = "actualstart";
      public const string Category = "category";
      public const string CreatedBy = "createdby";
      public const string CreatedOn = "createdon";
      public const string CreatedOnBehalfBy = "createdonbehalfby";
      public const string Description = "description";
      public const string DirectionCode = "directioncode";
      public const string ExchangeRate = "exchangerate";
      public const string From = "from";
      public const string ImportSequenceNumber = "importsequencenumber";
      public const string IsBilled = "isbilled";
      public const string IsRegularActivity = "isregularactivity";
      public const string IsWorkflowCreated = "isworkflowcreated";
      public const string LastOnHoldTime = "lastonholdtime";
      public const string LeftVoiceMail = "leftvoicemail";
      public const string ModifiedBy = "modifiedby";
      public const string ModifiedOn = "modifiedon";
      public const string ModifiedOnBehalfBy = "modifiedonbehalfby";
      public const string OnHoldTime = "onholdtime";
      public const string OverriddenCreatedOn = "overriddencreatedon";
      public const string OwnerId = "ownerid";
      public const string OwningBusinessUnit = "owningbusinessunit";
      public const string OwningTeam = "owningteam";
      public const string OwningUser = "owninguser";
      public const string PhoneNumber = "phonenumber";
      public const string PriorityCode = "prioritycode";
      public const string ProcessId = "processid";
      public const string RegardingObjectId = "regardingobjectid";
      public const string ScheduledDurationMinutes = "scheduleddurationminutes";
      public const string ScheduledEnd = "scheduledend";
      public const string ScheduledStart = "scheduledstart";
      public const string ServiceId = "serviceid";
      public const string SLAId = "slaid";
      public const string SLAInvokedId = "slainvokedid";
      public const string SortDate = "sortdate";
      public const string StageId = "stageid";
      public const string StateCode = "statecode";
      public const string StatusCode = "statuscode";
      public const string Subcategory = "subcategory";
      public const string Subject = "subject";
      public const string SubscriptionId = "subscriptionid";
      public const string TimeZoneRuleVersionNumber = "timezoneruleversionnumber";
      public const string To = "to";
      public const string TransactionCurrencyId = "transactioncurrencyid";
      public const string TraversedPath = "traversedpath";
      public const string UTCConversionTimeZoneCode = "utcconversiontimezonecode";
      public const string VersionNumber = "versionnumber";
    }
  }
}
