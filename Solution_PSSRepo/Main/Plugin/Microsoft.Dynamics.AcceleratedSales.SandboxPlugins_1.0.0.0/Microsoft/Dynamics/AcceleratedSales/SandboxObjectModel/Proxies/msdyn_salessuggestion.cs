// Decompiled with JetBrains decompiler
// Type: Microsoft.Dynamics.AcceleratedSales.SandboxObjectModel.Proxies.msdyn_salessuggestion
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
  [Microsoft.Xrm.Sdk.Client.EntityLogicalName("msdyn_salessuggestion")]
  [DataContract]
  internal class msdyn_salessuggestion : Entity, INotifyPropertyChanging, INotifyPropertyChanged
  {
    public const string EntityLogicalName = "msdyn_salessuggestion";
    public const string EntitySchemaName = "msdyn_salessuggestion";
    public const string PrimaryIdAttribute = "msdyn_salessuggestionid";
    public const string PrimaryNameAttribute = "msdyn_name";
    public const string EntityLogicalCollectionName = "msdyn_salessuggestions";
    public const string EntitySetName = "msdyn_salessuggestions";
    public const int EntityTypeCode = 10475;

    [DebuggerNonUserCode]
    public msdyn_salessuggestion()
      : base(nameof (msdyn_salessuggestion))
    {
    }

    [DebuggerNonUserCode]
    public msdyn_salessuggestion(Guid id)
      : base(nameof (msdyn_salessuggestion), id)
    {
    }

    [DebuggerNonUserCode]
    public msdyn_salessuggestion(string keyName, object keyValue)
      : base(nameof (msdyn_salessuggestion), keyName, keyValue)
    {
    }

    [DebuggerNonUserCode]
    public msdyn_salessuggestion(KeyAttributeCollection keyAttributes)
      : base(nameof (msdyn_salessuggestion), keyAttributes)
    {
    }

    public event PropertyChangedEventHandler PropertyChanged;

    public event PropertyChangingEventHandler PropertyChanging;

    public static class Fields
    {
      public const string CreatedBy = "createdby";
      public const string CreatedOn = "createdon";
      public const string CreatedOnBehalfBy = "createdonbehalfby";
      public const string EmailAddress = "emailaddress";
      public const string ExchangeRate = "exchangerate";
      public const string ImportSequenceNumber = "importsequencenumber";
      public const string ModifiedBy = "modifiedby";
      public const string ModifiedOn = "modifiedon";
      public const string ModifiedOnBehalfBy = "modifiedonbehalfby";
      public const string msdyn_customdata = "msdyn_customdata";
      public const string msdyn_expirydate = "msdyn_expirydate";
      public const string msdyn_feedbackreason = "msdyn_feedbackreason";
      public const string msdyn_insight = "msdyn_insight";
      public const string msdyn_modelid = "msdyn_modelid";
      public const string msdyn_name = "msdyn_name";
      public const string msdyn_potentialrevenue = "msdyn_potentialrevenue";
      public const string msdyn_potentialrevenue_Base = "msdyn_potentialrevenue_base";
      public const string msdyn_qualifiedrecord = "msdyn_qualifiedrecord";
      public const string msdyn_relatedrecord = "msdyn_relatedrecord";
      public const string msdyn_salesmotion = "msdyn_salesmotion";
      public const string msdyn_salesplay = "msdyn_salesplay";
      public const string msdyn_salessuggestionId = "msdyn_salessuggestionid";
      public const string Id = "msdyn_salessuggestionid";
      public const string msdyn_score = "msdyn_score";
      public const string msdyn_segmentid = "msdyn_segmentid";
      public const string msdyn_sequencecontact = "msdyn_sequencecontact";
      public const string msdyn_solutionarea = "msdyn_solutionarea";
      public const string msdyn_suggesteddate = "msdyn_suggesteddate";
      public const string msdyn_suggestionreason = "msdyn_suggestionreason";
      public const string OverriddenCreatedOn = "overriddencreatedon";
      public const string OwnerId = "ownerid";
      public const string OwningBusinessUnit = "owningbusinessunit";
      public const string OwningTeam = "owningteam";
      public const string OwningUser = "owninguser";
      public const string StateCode = "statecode";
      public const string StatusCode = "statuscode";
      public const string TimeZoneRuleVersionNumber = "timezoneruleversionnumber";
      public const string TransactionCurrencyId = "transactioncurrencyid";
      public const string UTCConversionTimeZoneCode = "utcconversiontimezonecode";
      public const string VersionNumber = "versionnumber";
    }
  }
}
