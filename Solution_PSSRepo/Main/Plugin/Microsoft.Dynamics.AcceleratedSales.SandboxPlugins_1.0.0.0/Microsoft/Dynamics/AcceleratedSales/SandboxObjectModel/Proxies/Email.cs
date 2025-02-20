// Decompiled with JetBrains decompiler
// Type: Microsoft.Dynamics.AcceleratedSales.SandboxObjectModel.Proxies.Email
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
  [Microsoft.Xrm.Sdk.Client.EntityLogicalName("email")]
  [DataContract]
  internal class Email : Entity, INotifyPropertyChanging, INotifyPropertyChanged
  {
    public const string EntityLogicalName = "email";
    public const string EntitySchemaName = "Email";
    public const string PrimaryIdAttribute = "activityid";
    public const string PrimaryNameAttribute = "subject";
    public const string EntityLogicalCollectionName = "emails";
    public const string EntitySetName = "emails";
    public const int EntityTypeCode = 4202;

    [DebuggerNonUserCode]
    public Email()
      : base("email")
    {
    }

    [DebuggerNonUserCode]
    public Email(Guid id)
      : base("email", id)
    {
    }

    [DebuggerNonUserCode]
    public Email(string keyName, object keyValue)
      : base("email", keyName, keyValue)
    {
    }

    [DebuggerNonUserCode]
    public Email(KeyAttributeCollection keyAttributes)
      : base("email", keyAttributes)
    {
    }

    public event PropertyChangedEventHandler PropertyChanged;

    public event PropertyChangingEventHandler PropertyChanging;

    public static class Fields
    {
      public const string AcceptingEntityId = "acceptingentityid";
      public const string ActivityAdditionalParams = "activityadditionalparams";
      public const string ActivityId = "activityid";
      public const string Id = "activityid";
      public const string ActivityTypeCode = "activitytypecode";
      public const string ActualDurationMinutes = "actualdurationminutes";
      public const string ActualEnd = "actualend";
      public const string ActualStart = "actualstart";
      public const string AttachmentCount = "attachmentcount";
      public const string AttachmentOpenCount = "attachmentopencount";
      public const string BaseConversationIndexHash = "baseconversationindexhash";
      public const string Bcc = "bcc";
      public const string Category = "category";
      public const string Cc = "cc";
      public const string Compressed = "compressed";
      public const string ConversationIndex = "conversationindex";
      public const string ConversationTrackingId = "conversationtrackingid";
      public const string CorrelatedActivityId = "correlatedactivityid";
      public const string CorrelationMethod = "correlationmethod";
      public const string CreatedBy = "createdby";
      public const string CreatedOn = "createdon";
      public const string CreatedOnBehalfBy = "createdonbehalfby";
      public const string DelayedEmailSendTime = "delayedemailsendtime";
      public const string DeliveryAttempts = "deliveryattempts";
      public const string DeliveryPriorityCode = "deliveryprioritycode";
      public const string DeliveryReceiptRequested = "deliveryreceiptrequested";
      public const string Description = "description";
      public const string DescriptionBlobId = "descriptionblobid";
      public const string DescriptionBlobId_Name = "descriptionblobid_name";
      public const string DirectionCode = "directioncode";
      public const string EmailReminderExpiryTime = "emailreminderexpirytime";
      public const string EmailReminderStatus = "emailreminderstatus";
      public const string EmailReminderText = "emailremindertext";
      public const string EmailReminderType = "emailremindertype";
      public const string EmailSender = "emailsender";
      public const string EmailTrackingId = "emailtrackingid";
      public const string ExchangeRate = "exchangerate";
      public const string FollowEmailUserPreference = "followemailuserpreference";
      public const string From = "from";
      public const string ImportSequenceNumber = "importsequencenumber";
      public const string InReplyTo = "inreplyto";
      public const string IsBilled = "isbilled";
      public const string IsEmailFollowed = "isemailfollowed";
      public const string IsEmailReminderSet = "isemailreminderset";
      public const string IsRegularActivity = "isregularactivity";
      public const string IsUnsafe = "isunsafe";
      public const string IsWorkflowCreated = "isworkflowcreated";
      public const string LastOnHoldTime = "lastonholdtime";
      public const string LastOpenedTime = "lastopenedtime";
      public const string LinksClickedCount = "linksclickedcount";
      public const string MessageId = "messageid";
      public const string MessageIdDupCheck = "messageiddupcheck";
      public const string MimeType = "mimetype";
      public const string ModifiedBy = "modifiedby";
      public const string ModifiedOn = "modifiedon";
      public const string ModifiedOnBehalfBy = "modifiedonbehalfby";
      public const string msdyn_RecipientList = "msdyn_recipientlist";
      public const string Notifications = "notifications";
      public const string OnHoldTime = "onholdtime";
      public const string OpenCount = "opencount";
      public const string OverriddenCreatedOn = "overriddencreatedon";
      public const string OwnerId = "ownerid";
      public const string OwningBusinessUnit = "owningbusinessunit";
      public const string OwningTeam = "owningteam";
      public const string OwningUser = "owninguser";
      public const string ParentActivityId = "parentactivityid";
      public const string PostponeEmailProcessingUntil = "postponeemailprocessinguntil";
      public const string PriorityCode = "prioritycode";
      public const string ProcessId = "processid";
      public const string ReadReceiptRequested = "readreceiptrequested";
      public const string ReceivingMailboxId = "receivingmailboxid";
      public const string RegardingObjectId = "regardingobjectid";
      public const string ReminderActionCardId = "reminderactioncardid";
      public const string ReplyCount = "replycount";
      public const string ReservedForInternalUse = "reservedforinternaluse";
      public const string ScheduledDurationMinutes = "scheduleddurationminutes";
      public const string ScheduledEnd = "scheduledend";
      public const string ScheduledStart = "scheduledstart";
      public const string Sender = "sender";
      public const string SenderMailboxId = "sendermailboxid";
      public const string SendersAccount = "sendersaccount";
      public const string SentOn = "senton";
      public const string ServiceId = "serviceid";
      public const string SLAId = "slaid";
      public const string SLAInvokedId = "slainvokedid";
      public const string SortDate = "sortdate";
      public const string StageId = "stageid";
      public const string StateCode = "statecode";
      public const string StatusCode = "statuscode";
      public const string Subcategory = "subcategory";
      public const string Subject = "subject";
      public const string SubmittedBy = "submittedby";
      public const string TemplateId = "templateid";
      public const string TimeZoneRuleVersionNumber = "timezoneruleversionnumber";
      public const string To = "to";
      public const string ToRecipients = "torecipients";
      public const string TrackingToken = "trackingtoken";
      public const string TransactionCurrencyId = "transactioncurrencyid";
      public const string TraversedPath = "traversedpath";
      public const string UTCConversionTimeZoneCode = "utcconversiontimezonecode";
      public const string VersionNumber = "versionnumber";
    }
  }
}
