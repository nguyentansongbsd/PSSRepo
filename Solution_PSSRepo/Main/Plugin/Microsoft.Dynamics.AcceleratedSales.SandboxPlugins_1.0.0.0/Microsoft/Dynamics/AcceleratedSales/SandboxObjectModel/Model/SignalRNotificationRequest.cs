// Decompiled with JetBrains decompiler
// Type: Microsoft.Dynamics.AcceleratedSales.SandboxObjectModel.Model.SignalRNotificationRequest
// Assembly: Microsoft.Dynamics.AcceleratedSales.SandboxPlugins, Version=1.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35
// MVID: 7BBE7132-7E30-4424-B38D-27667CFBCEFA
// Assembly location: C:\Users\ngoct\Downloads\New folder\Microsoft.Dynamics.AcceleratedSales.SandboxPlugins_1.0.0.0.dll

using System;
using System.Runtime.Serialization;

#nullable disable
namespace Microsoft.Dynamics.AcceleratedSales.SandboxObjectModel.Model
{
  [DataContract]
  internal class SignalRNotificationRequest
  {
    [DataMember]
    public string HubName { get; set; }

    [DataMember]
    public string MessageContent { get; set; }

    [DataMember]
    public Guid UserId { get; set; }
  }
}
