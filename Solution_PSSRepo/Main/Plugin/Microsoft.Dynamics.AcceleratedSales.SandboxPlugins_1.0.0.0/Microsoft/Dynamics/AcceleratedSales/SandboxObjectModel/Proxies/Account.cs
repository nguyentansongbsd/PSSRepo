// Decompiled with JetBrains decompiler
// Type: Microsoft.Dynamics.AcceleratedSales.SandboxObjectModel.Proxies.Account
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
  [Microsoft.Xrm.Sdk.Client.EntityLogicalName("account")]
  [DataContract]
  internal class Account : Entity, INotifyPropertyChanging, INotifyPropertyChanged
  {
    public const string EntityLogicalName = "account";
    public const string EntitySchemaName = "Account";
    public const string PrimaryIdAttribute = "accountid";
    public const string PrimaryNameAttribute = "name";
    public const string EntityLogicalCollectionName = "accounts";
    public const string EntitySetName = "accounts";
    public const int EntityTypeCode = 1;

    [DebuggerNonUserCode]
    public Account()
      : base("account")
    {
    }

    [DebuggerNonUserCode]
    public Account(Guid id)
      : base("account", id)
    {
    }

    [DebuggerNonUserCode]
    public Account(string keyName, object keyValue)
      : base("account", keyName, keyValue)
    {
    }

    [DebuggerNonUserCode]
    public Account(KeyAttributeCollection keyAttributes)
      : base("account", keyAttributes)
    {
    }

    public event PropertyChangedEventHandler PropertyChanged;

    public event PropertyChangingEventHandler PropertyChanging;

    public static class Fields
    {
      public const string AccountCategoryCode = "accountcategorycode";
      public const string AccountClassificationCode = "accountclassificationcode";
      public const string AccountId = "accountid";
      public const string Id = "accountid";
      public const string AccountNumber = "accountnumber";
      public const string AccountRatingCode = "accountratingcode";
      public const string Address1_AddressId = "address1_addressid";
      public const string Address1_AddressTypeCode = "address1_addresstypecode";
      public const string Address1_City = "address1_city";
      public const string Address1_Composite = "address1_composite";
      public const string Address1_Country = "address1_country";
      public const string Address1_County = "address1_county";
      public const string Address1_Fax = "address1_fax";
      public const string Address1_FreightTermsCode = "address1_freighttermscode";
      public const string Address1_Latitude = "address1_latitude";
      public const string Address1_Line1 = "address1_line1";
      public const string Address1_Line2 = "address1_line2";
      public const string Address1_Line3 = "address1_line3";
      public const string Address1_Longitude = "address1_longitude";
      public const string Address1_Name = "address1_name";
      public const string Address1_PostalCode = "address1_postalcode";
      public const string Address1_PostOfficeBox = "address1_postofficebox";
      public const string Address1_PrimaryContactName = "address1_primarycontactname";
      public const string Address1_ShippingMethodCode = "address1_shippingmethodcode";
      public const string Address1_StateOrProvince = "address1_stateorprovince";
      public const string Address1_Telephone1 = "address1_telephone1";
      public const string Address1_Telephone2 = "address1_telephone2";
      public const string Address1_Telephone3 = "address1_telephone3";
      public const string Address1_UPSZone = "address1_upszone";
      public const string Address1_UTCOffset = "address1_utcoffset";
      public const string Address2_AddressId = "address2_addressid";
      public const string Address2_AddressTypeCode = "address2_addresstypecode";
      public const string Address2_City = "address2_city";
      public const string Address2_Composite = "address2_composite";
      public const string Address2_Country = "address2_country";
      public const string Address2_County = "address2_county";
      public const string Address2_Fax = "address2_fax";
      public const string Address2_FreightTermsCode = "address2_freighttermscode";
      public const string Address2_Latitude = "address2_latitude";
      public const string Address2_Line1 = "address2_line1";
      public const string Address2_Line2 = "address2_line2";
      public const string Address2_Line3 = "address2_line3";
      public const string Address2_Longitude = "address2_longitude";
      public const string Address2_Name = "address2_name";
      public const string Address2_PostalCode = "address2_postalcode";
      public const string Address2_PostOfficeBox = "address2_postofficebox";
      public const string Address2_PrimaryContactName = "address2_primarycontactname";
      public const string Address2_ShippingMethodCode = "address2_shippingmethodcode";
      public const string Address2_StateOrProvince = "address2_stateorprovince";
      public const string Address2_Telephone1 = "address2_telephone1";
      public const string Address2_Telephone2 = "address2_telephone2";
      public const string Address2_Telephone3 = "address2_telephone3";
      public const string Address2_UPSZone = "address2_upszone";
      public const string Address2_UTCOffset = "address2_utcoffset";
      public const string Aging30 = "aging30";
      public const string Aging30_Base = "aging30_base";
      public const string Aging60 = "aging60";
      public const string Aging60_Base = "aging60_base";
      public const string Aging90 = "aging90";
      public const string Aging90_Base = "aging90_base";
      public const string BusinessTypeCode = "businesstypecode";
      public const string CreatedBy = "createdby";
      public const string CreatedByExternalParty = "createdbyexternalparty";
      public const string CreatedOn = "createdon";
      public const string CreatedOnBehalfBy = "createdonbehalfby";
      public const string CreditLimit = "creditlimit";
      public const string CreditLimit_Base = "creditlimit_base";
      public const string CreditOnHold = "creditonhold";
      public const string CustomerSizeCode = "customersizecode";
      public const string CustomerTypeCode = "customertypecode";
      public const string DefaultPriceLevelId = "defaultpricelevelid";
      public const string Description = "description";
      public const string DoNotBulkEMail = "donotbulkemail";
      public const string DoNotBulkPostalMail = "donotbulkpostalmail";
      public const string DoNotEMail = "donotemail";
      public const string DoNotFax = "donotfax";
      public const string DoNotPhone = "donotphone";
      public const string DoNotPostalMail = "donotpostalmail";
      public const string DoNotSendMM = "donotsendmm";
      public const string EMailAddress1 = "emailaddress1";
      public const string EMailAddress2 = "emailaddress2";
      public const string EMailAddress3 = "emailaddress3";
      public const string EntityImage = "entityimage";
      public const string EntityImage_Timestamp = "entityimage_timestamp";
      public const string EntityImage_URL = "entityimage_url";
      public const string EntityImageId = "entityimageid";
      public const string ExchangeRate = "exchangerate";
      public const string Fax = "fax";
      public const string FollowEmail = "followemail";
      public const string FtpSiteURL = "ftpsiteurl";
      public const string ImportSequenceNumber = "importsequencenumber";
      public const string IndustryCode = "industrycode";
      public const string LastOnHoldTime = "lastonholdtime";
      public const string LastUsedInCampaign = "lastusedincampaign";
      public const string MarketCap = "marketcap";
      public const string MarketCap_Base = "marketcap_base";
      public const string MarketingOnly = "marketingonly";
      public const string MasterId = "masterid";
      public const string Merged = "merged";
      public const string ModifiedBy = "modifiedby";
      public const string ModifiedByExternalParty = "modifiedbyexternalparty";
      public const string ModifiedOn = "modifiedon";
      public const string ModifiedOnBehalfBy = "modifiedonbehalfby";
      public const string msdyn_salesaccelerationinsightid = "msdyn_salesaccelerationinsightid";
      public const string Name = "name";
      public const string NumberOfEmployees = "numberofemployees";
      public const string OnHoldTime = "onholdtime";
      public const string OpenDeals = "opendeals";
      public const string OpenDeals_Date = "opendeals_date";
      public const string OpenDeals_State = "opendeals_state";
      public const string OpenRevenue = "openrevenue";
      public const string OpenRevenue_Base = "openrevenue_base";
      public const string OpenRevenue_Date = "openrevenue_date";
      public const string OpenRevenue_State = "openrevenue_state";
      public const string OriginatingLeadId = "originatingleadid";
      public const string OverriddenCreatedOn = "overriddencreatedon";
      public const string OwnerId = "ownerid";
      public const string OwnershipCode = "ownershipcode";
      public const string OwningBusinessUnit = "owningbusinessunit";
      public const string OwningTeam = "owningteam";
      public const string OwningUser = "owninguser";
      public const string ParentAccountId = "parentaccountid";
      public const string ParticipatesInWorkflow = "participatesinworkflow";
      public const string PaymentTermsCode = "paymenttermscode";
      public const string PreferredAppointmentDayCode = "preferredappointmentdaycode";
      public const string PreferredAppointmentTimeCode = "preferredappointmenttimecode";
      public const string PreferredContactMethodCode = "preferredcontactmethodcode";
      public const string PreferredEquipmentId = "preferredequipmentid";
      public const string PreferredServiceId = "preferredserviceid";
      public const string PreferredSystemUserId = "preferredsystemuserid";
      public const string PrimaryContactId = "primarycontactid";
      public const string PrimarySatoriId = "primarysatoriid";
      public const string PrimaryTwitterId = "primarytwitterid";
      public const string ProcessId = "processid";
      public const string Revenue = "revenue";
      public const string Revenue_Base = "revenue_base";
      public const string SharesOutstanding = "sharesoutstanding";
      public const string ShippingMethodCode = "shippingmethodcode";
      public const string SIC = "sic";
      public const string SLAId = "slaid";
      public const string SLAInvokedId = "slainvokedid";
      public const string StageId = "stageid";
      public const string StateCode = "statecode";
      public const string StatusCode = "statuscode";
      public const string StockExchange = "stockexchange";
      public const string TeamsFollowed = "teamsfollowed";
      public const string Telephone1 = "telephone1";
      public const string Telephone2 = "telephone2";
      public const string Telephone3 = "telephone3";
      public const string TerritoryCode = "territorycode";
      public const string TerritoryId = "territoryid";
      public const string TickerSymbol = "tickersymbol";
      public const string TimeSpentByMeOnEmailAndMeetings = "timespentbymeonemailandmeetings";
      public const string TimeZoneRuleVersionNumber = "timezoneruleversionnumber";
      public const string TransactionCurrencyId = "transactioncurrencyid";
      public const string TraversedPath = "traversedpath";
      public const string UTCConversionTimeZoneCode = "utcconversiontimezonecode";
      public const string VersionNumber = "versionnumber";
      public const string WebSiteURL = "websiteurl";
      public const string YomiName = "yominame";
    }
  }
}
