// Decompiled with JetBrains decompiler
// Type: Microsoft.Dynamics.AcceleratedSales.SandboxObjectModel.Proxies.SystemUser
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
  [Microsoft.Xrm.Sdk.Client.EntityLogicalName("systemuser")]
  [DataContract]
  internal class SystemUser : Entity, INotifyPropertyChanging, INotifyPropertyChanged
  {
    public const string AlternateKeys = "azureactivedirectoryobjectid";
    public const string EntityLogicalName = "systemuser";
    public const string EntitySchemaName = "SystemUser";
    public const string PrimaryIdAttribute = "systemuserid";
    public const string PrimaryNameAttribute = "fullname";
    public const string EntityLogicalCollectionName = "systemusers";
    public const string EntitySetName = "systemusers";
    public const int EntityTypeCode = 8;

    [DebuggerNonUserCode]
    public SystemUser()
      : base("systemuser")
    {
    }

    [DebuggerNonUserCode]
    public SystemUser(Guid id)
      : base("systemuser", id)
    {
    }

    [DebuggerNonUserCode]
    public SystemUser(string keyName, object keyValue)
      : base("systemuser", keyName, keyValue)
    {
    }

    [DebuggerNonUserCode]
    public SystemUser(KeyAttributeCollection keyAttributes)
      : base("systemuser", keyAttributes)
    {
    }

    public event PropertyChangedEventHandler PropertyChanged;

    public event PropertyChangingEventHandler PropertyChanging;

    public static class Fields
    {
      public const string AccessMode = "accessmode";
      public const string Address1_AddressId = "address1_addressid";
      public const string Address1_AddressTypeCode = "address1_addresstypecode";
      public const string Address1_City = "address1_city";
      public const string Address1_Composite = "address1_composite";
      public const string Address1_Country = "address1_country";
      public const string Address1_County = "address1_county";
      public const string Address1_Fax = "address1_fax";
      public const string Address1_Latitude = "address1_latitude";
      public const string Address1_Line1 = "address1_line1";
      public const string Address1_Line2 = "address1_line2";
      public const string Address1_Line3 = "address1_line3";
      public const string Address1_Longitude = "address1_longitude";
      public const string Address1_Name = "address1_name";
      public const string Address1_PostalCode = "address1_postalcode";
      public const string Address1_PostOfficeBox = "address1_postofficebox";
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
      public const string Address2_Latitude = "address2_latitude";
      public const string Address2_Line1 = "address2_line1";
      public const string Address2_Line2 = "address2_line2";
      public const string Address2_Line3 = "address2_line3";
      public const string Address2_Longitude = "address2_longitude";
      public const string Address2_Name = "address2_name";
      public const string Address2_PostalCode = "address2_postalcode";
      public const string Address2_PostOfficeBox = "address2_postofficebox";
      public const string Address2_ShippingMethodCode = "address2_shippingmethodcode";
      public const string Address2_StateOrProvince = "address2_stateorprovince";
      public const string Address2_Telephone1 = "address2_telephone1";
      public const string Address2_Telephone2 = "address2_telephone2";
      public const string Address2_Telephone3 = "address2_telephone3";
      public const string Address2_UPSZone = "address2_upszone";
      public const string Address2_UTCOffset = "address2_utcoffset";
      public const string ApplicationId = "applicationid";
      public const string ApplicationIdUri = "applicationiduri";
      public const string AzureActiveDirectoryObjectId = "azureactivedirectoryobjectid";
      public const string AzureDeletedOn = "azuredeletedon";
      public const string AzureState = "azurestate";
      public const string BusinessUnitId = "businessunitid";
      public const string CalendarId = "calendarid";
      public const string CALType = "caltype";
      public const string CreatedBy = "createdby";
      public const string CreatedOn = "createdon";
      public const string CreatedOnBehalfBy = "createdonbehalfby";
      public const string DefaultFiltersPopulated = "defaultfilterspopulated";
      public const string DefaultMailbox = "defaultmailbox";
      public const string DefaultOdbFolderName = "defaultodbfoldername";
      public const string DeletedState = "deletedstate";
      public const string DisabledReason = "disabledreason";
      public const string DisplayInServiceViews = "displayinserviceviews";
      public const string DomainName = "domainname";
      public const string EmailRouterAccessApproval = "emailrouteraccessapproval";
      public const string EmployeeId = "employeeid";
      public const string EntityImage = "entityimage";
      public const string EntityImage_Timestamp = "entityimage_timestamp";
      public const string EntityImage_URL = "entityimage_url";
      public const string EntityImageId = "entityimageid";
      public const string ExchangeRate = "exchangerate";
      public const string FirstName = "firstname";
      public const string FullName = "fullname";
      public const string GovernmentId = "governmentid";
      public const string HomePhone = "homephone";
      public const string IdentityId = "identityid";
      public const string ImportSequenceNumber = "importsequencenumber";
      public const string IncomingEmailDeliveryMethod = "incomingemaildeliverymethod";
      public const string InternalEMailAddress = "internalemailaddress";
      public const string InviteStatusCode = "invitestatuscode";
      public const string IsDisabled = "isdisabled";
      public const string IsEmailAddressApprovedByO365Admin = "isemailaddressapprovedbyo365admin";
      public const string IsIntegrationUser = "isintegrationuser";
      public const string IsLicensed = "islicensed";
      public const string IsSyncWithDirectory = "issyncwithdirectory";
      public const string JobTitle = "jobtitle";
      public const string LastName = "lastname";
      public const string MiddleName = "middlename";
      public const string MobileAlertEMail = "mobilealertemail";
      public const string MobileOfflineProfileId = "mobileofflineprofileid";
      public const string MobilePhone = "mobilephone";
      public const string ModifiedBy = "modifiedby";
      public const string ModifiedOn = "modifiedon";
      public const string ModifiedOnBehalfBy = "modifiedonbehalfby";
      public const string msdyn_gdproptout = "msdyn_gdproptout";
      public const string new_inuse = "new_inuse";
      public const string new_testauthenticationtype = "new_testauthenticationtype";
      public const string NickName = "nickname";
      public const string OrganizationId = "organizationid";
      public const string OutgoingEmailDeliveryMethod = "outgoingemaildeliverymethod";
      public const string OverriddenCreatedOn = "overriddencreatedon";
      public const string ParentSystemUserId = "parentsystemuserid";
      public const string PassportHi = "passporthi";
      public const string PassportLo = "passportlo";
      public const string PersonalEMailAddress = "personalemailaddress";
      public const string PhotoUrl = "photourl";
      public const string PositionId = "positionid";
      public const string PreferredAddressCode = "preferredaddresscode";
      public const string PreferredEmailCode = "preferredemailcode";
      public const string PreferredPhoneCode = "preferredphonecode";
      public const string ProcessId = "processid";
      public const string QueueId = "queueid";
      public const string Salutation = "salutation";
      public const string SetupUser = "setupuser";
      public const string SharePointEmailAddress = "sharepointemailaddress";
      public const string SiteId = "siteid";
      public const string Skills = "skills";
      public const string StageId = "stageid";
      public const string SystemUserId = "systemuserid";
      public const string Id = "systemuserid";
      public const string TerritoryId = "territoryid";
      public const string TimeZoneRuleVersionNumber = "timezoneruleversionnumber";
      public const string Title = "title";
      public const string TransactionCurrencyId = "transactioncurrencyid";
      public const string TraversedPath = "traversedpath";
      public const string UserLicenseType = "userlicensetype";
      public const string UserPuid = "userpuid";
      public const string UTCConversionTimeZoneCode = "utcconversiontimezonecode";
      public const string VersionNumber = "versionnumber";
      public const string WindowsLiveID = "windowsliveid";
      public const string YammerEmailAddress = "yammeremailaddress";
      public const string YammerUserId = "yammeruserid";
      public const string YomiFirstName = "yomifirstname";
      public const string YomiFullName = "yomifullname";
      public const string YomiLastName = "yomilastname";
      public const string YomiMiddleName = "yomimiddlename";
    }
  }
}
