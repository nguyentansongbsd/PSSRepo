// Decompiled with JetBrains decompiler
// Type: Microsoft.Dynamics.AcceleratedSales.SandboxObjectModel.Proxies.Opportunity
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
  [DataContract]
  [Microsoft.Xrm.Sdk.Client.EntityLogicalName("opportunity")]
  internal class Opportunity : Entity, INotifyPropertyChanging, INotifyPropertyChanged
  {
    public const string EntityLogicalName = "opportunity";
    public const string EntitySchemaName = "Opportunity";
    public const string PrimaryIdAttribute = "opportunityid";
    public const string PrimaryNameAttribute = "name";
    public const string EntityLogicalCollectionName = "opportunities";
    public const string EntitySetName = "opportunities";
    public const int EntityTypeCode = 3;

    [DebuggerNonUserCode]
    public Opportunity()
      : base("opportunity")
    {
    }

    [DebuggerNonUserCode]
    public Opportunity(Guid id)
      : base("opportunity", id)
    {
    }

    [DebuggerNonUserCode]
    public Opportunity(string keyName, object keyValue)
      : base("opportunity", keyName, keyValue)
    {
    }

    [DebuggerNonUserCode]
    public Opportunity(KeyAttributeCollection keyAttributes)
      : base("opportunity", keyAttributes)
    {
    }

    public event PropertyChangedEventHandler PropertyChanged;

    public event PropertyChangingEventHandler PropertyChanging;

    public static class Fields
    {
      public const string AccountId = "accountid";
      public const string ActualCloseDate = "actualclosedate";
      public const string ActualValue = "actualvalue";
      public const string ActualValue_Base = "actualvalue_base";
      public const string BudgetAmount = "budgetamount";
      public const string BudgetAmount_Base = "budgetamount_base";
      public const string BudgetStatus = "budgetstatus";
      public const string CampaignId = "campaignid";
      public const string CaptureProposalFeedback = "captureproposalfeedback";
      public const string CloseProbability = "closeprobability";
      public const string CompleteFinalProposal = "completefinalproposal";
      public const string CompleteInternalReview = "completeinternalreview";
      public const string ConfirmInterest = "confirminterest";
      public const string ContactId = "contactid";
      public const string CreatedBy = "createdby";
      public const string CreatedOn = "createdon";
      public const string CreatedOnBehalfBy = "createdonbehalfby";
      public const string CurrentSituation = "currentsituation";
      public const string CustomerId = "customerid";
      public const string CustomerNeed = "customerneed";
      public const string CustomerPainPoints = "customerpainpoints";
      public const string DecisionMaker = "decisionmaker";
      public const string Description = "description";
      public const string DevelopProposal = "developproposal";
      public const string DiscountAmount = "discountamount";
      public const string DiscountAmount_Base = "discountamount_base";
      public const string DiscountPercentage = "discountpercentage";
      public const string EmailAddress = "emailaddress";
      public const string EstimatedCloseDate = "estimatedclosedate";
      public const string EstimatedValue = "estimatedvalue";
      public const string EstimatedValue_Base = "estimatedvalue_base";
      public const string EvaluateFit = "evaluatefit";
      public const string ExchangeRate = "exchangerate";
      public const string FileDebrief = "filedebrief";
      public const string FinalDecisionDate = "finaldecisiondate";
      public const string FreightAmount = "freightamount";
      public const string FreightAmount_Base = "freightamount_base";
      public const string IdentifyCompetitors = "identifycompetitors";
      public const string IdentifyCustomerContacts = "identifycustomercontacts";
      public const string IdentifyPursuitTeam = "identifypursuitteam";
      public const string ImportSequenceNumber = "importsequencenumber";
      public const string InitialCommunication = "initialcommunication";
      public const string IsRevenueSystemCalculated = "isrevenuesystemcalculated";
      public const string LastOnHoldTime = "lastonholdtime";
      public const string ModifiedBy = "modifiedby";
      public const string ModifiedOn = "modifiedon";
      public const string ModifiedOnBehalfBy = "modifiedonbehalfby";
      public const string msdyn_segmentid = "msdyn_segmentid";
      public const string Name = "name";
      public const string Need = "need";
      public const string OnHoldTime = "onholdtime";
      public const string OpportunityId = "opportunityid";
      public const string Id = "opportunityid";
      public const string OpportunityRatingCode = "opportunityratingcode";
      public const string OriginatingLeadId = "originatingleadid";
      public const string OverriddenCreatedOn = "overriddencreatedon";
      public const string OwnerId = "ownerid";
      public const string OwningBusinessUnit = "owningbusinessunit";
      public const string OwningTeam = "owningteam";
      public const string OwningUser = "owninguser";
      public const string ParentAccountId = "parentaccountid";
      public const string ParentContactId = "parentcontactid";
      public const string ParticipatesInWorkflow = "participatesinworkflow";
      public const string PresentFinalProposal = "presentfinalproposal";
      public const string PresentProposal = "presentproposal";
      public const string PriceLevelId = "pricelevelid";
      public const string PricingErrorCode = "pricingerrorcode";
      public const string PriorityCode = "prioritycode";
      public const string ProcessId = "processid";
      public const string ProposedSolution = "proposedsolution";
      public const string PurchaseProcess = "purchaseprocess";
      public const string PurchaseTimeframe = "purchasetimeframe";
      public const string PursuitDecision = "pursuitdecision";
      public const string QualificationComments = "qualificationcomments";
      public const string QuoteComments = "quotecomments";
      public const string ResolveFeedback = "resolvefeedback";
      public const string SalesStage = "salesstage";
      public const string SalesStageCode = "salesstagecode";
      public const string ScheduleFollowup_Prospect = "schedulefollowup_prospect";
      public const string ScheduleFollowup_Qualify = "schedulefollowup_qualify";
      public const string ScheduleProposalMeeting = "scheduleproposalmeeting";
      public const string SendThankYouNote = "sendthankyounote";
      public const string SkipPriceCalculation = "skippricecalculation";
      public const string SLAId = "slaid";
      public const string SLAInvokedId = "slainvokedid";
      public const string StageId = "stageid";
      public const string StateCode = "statecode";
      public const string StatusCode = "statuscode";
      public const string StepId = "stepid";
      public const string StepName = "stepname";
      public const string TeamsFollowed = "teamsfollowed";
      public const string TimeLine = "timeline";
      public const string TimeSpentByMeOnEmailAndMeetings = "timespentbymeonemailandmeetings";
      public const string TimeZoneRuleVersionNumber = "timezoneruleversionnumber";
      public const string TotalAmount = "totalamount";
      public const string TotalAmount_Base = "totalamount_base";
      public const string TotalAmountLessFreight = "totalamountlessfreight";
      public const string TotalAmountLessFreight_Base = "totalamountlessfreight_base";
      public const string TotalDiscountAmount = "totaldiscountamount";
      public const string TotalDiscountAmount_Base = "totaldiscountamount_base";
      public const string TotalLineItemAmount = "totallineitemamount";
      public const string TotalLineItemAmount_Base = "totallineitemamount_base";
      public const string TotalLineItemDiscountAmount = "totallineitemdiscountamount";
      public const string TotalLineItemDiscountAmount_Base = "totallineitemdiscountamount_base";
      public const string TotalTax = "totaltax";
      public const string TotalTax_Base = "totaltax_base";
      public const string TransactionCurrencyId = "transactioncurrencyid";
      public const string TraversedPath = "traversedpath";
      public const string UTCConversionTimeZoneCode = "utcconversiontimezonecode";
      public const string VersionNumber = "versionnumber";
    }
  }
}
