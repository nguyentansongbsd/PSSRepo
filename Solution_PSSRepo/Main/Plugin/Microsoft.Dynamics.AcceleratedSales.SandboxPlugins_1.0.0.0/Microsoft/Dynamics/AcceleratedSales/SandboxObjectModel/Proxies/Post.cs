// Decompiled with JetBrains decompiler
// Type: Microsoft.Dynamics.AcceleratedSales.SandboxObjectModel.Proxies.Post
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
  [Microsoft.Xrm.Sdk.Client.EntityLogicalName("post")]
  [DataContract]
  internal class Post : Entity, INotifyPropertyChanging, INotifyPropertyChanged
  {
    public const string EntityLogicalName = "post";
    public const string EntitySchemaName = "Post";
    public const string PrimaryIdAttribute = "postid";
    public const string PrimaryNameAttribute = "text";
    public const string EntityLogicalCollectionName = "posts";
    public const string EntitySetName = "posts";
    public const int EntityTypeCode = 8000;

    [DebuggerNonUserCode]
    public Post()
      : base("post")
    {
    }

    [DebuggerNonUserCode]
    public Post(Guid id)
      : base("post", id)
    {
    }

    [DebuggerNonUserCode]
    public Post(string keyName, object keyValue)
      : base("post", keyName, keyValue)
    {
    }

    [DebuggerNonUserCode]
    public Post(KeyAttributeCollection keyAttributes)
      : base("post", keyAttributes)
    {
    }

    public event PropertyChangedEventHandler PropertyChanged;

    public event PropertyChangingEventHandler PropertyChanging;

    public static class Fields
    {
      public const string CreatedBy = "createdby";
      public const string CreatedOn = "createdon";
      public const string CreatedOnBehalfBy = "createdonbehalfby";
      public const string LargeText = "largetext";
      public const string ModifiedBy = "modifiedby";
      public const string ModifiedOn = "modifiedon";
      public const string ModifiedOnBehalfBy = "modifiedonbehalfby";
      public const string OrganizationId = "organizationid";
      public const string PostId = "postid";
      public const string Id = "postid";
      public const string RegardingObjectId = "regardingobjectid";
      public const string RegardingObjectOwnerId = "regardingobjectownerid";
      public const string RegardingObjectOwningBusinessUnit = "regardingobjectowningbusinessunit";
      public const string Source = "source";
      public const string Text = "text";
      public const string TimeZoneRuleVersionNumber = "timezoneruleversionnumber";
      public const string Type = "type";
      public const string UTCConversionTimeZoneCode = "utcconversiontimezonecode";
    }
  }
}
