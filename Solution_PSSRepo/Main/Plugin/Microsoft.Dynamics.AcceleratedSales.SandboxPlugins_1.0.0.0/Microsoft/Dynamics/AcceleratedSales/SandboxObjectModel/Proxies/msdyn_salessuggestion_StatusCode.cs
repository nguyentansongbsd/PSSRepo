// Decompiled with JetBrains decompiler
// Type: Microsoft.Dynamics.AcceleratedSales.SandboxObjectModel.Proxies.msdyn_salessuggestion_StatusCode
// Assembly: Microsoft.Dynamics.AcceleratedSales.SandboxPlugins, Version=1.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35
// MVID: 7BBE7132-7E30-4424-B38D-27667CFBCEFA
// Assembly location: C:\Users\ngoct\Downloads\New folder\Microsoft.Dynamics.AcceleratedSales.SandboxPlugins_1.0.0.0.dll

using System.Runtime.Serialization;

#nullable disable
namespace Microsoft.Dynamics.AcceleratedSales.SandboxObjectModel.Proxies
{
  [DataContract]
  internal enum msdyn_salessuggestion_StatusCode
  {
    [EnumMember] Open = 1,
    [EnumMember] Others_Inactive_2 = 2,
    [EnumMember] Others_Inactive_3 = 3,
    [EnumMember] CreatedOpportunity = 4,
    [EnumMember] Accepted = 5,
  }
}
