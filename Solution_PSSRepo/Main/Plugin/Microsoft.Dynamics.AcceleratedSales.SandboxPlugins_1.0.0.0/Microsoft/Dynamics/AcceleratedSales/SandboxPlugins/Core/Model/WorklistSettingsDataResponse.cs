// Decompiled with JetBrains decompiler
// Type: Microsoft.Dynamics.AcceleratedSales.SandboxPlugins.Core.Model.WorklistSettingsDataResponse
// Assembly: Microsoft.Dynamics.AcceleratedSales.SandboxPlugins, Version=1.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35
// MVID: 7BBE7132-7E30-4424-B38D-27667CFBCEFA
// Assembly location: C:\Users\ngoct\Downloads\New folder\Microsoft.Dynamics.AcceleratedSales.SandboxPlugins_1.0.0.0.dll

using Microsoft.Xrm.Sdk;
using Newtonsoft.Json;
using System.Collections.Generic;

#nullable disable
namespace Microsoft.Dynamics.AcceleratedSales.SandboxPlugins.Core.Model
{
  public class WorklistSettingsDataResponse
  {
    [JsonProperty(PropertyName = "adminConfig")]
    public WorklistAdminConfiguration AdminConfig { get; set; }

    [JsonProperty(PropertyName = "userConfig")]
    public WorklistSellerConfiguration UserConfig { get; set; }

    [JsonProperty(PropertyName = "entityRelationships")]
    public Dictionary<string, List<EntityRelationship>> EntityRelationships { get; set; }

    [JsonProperty(PropertyName = "entityAttributes")]
    public Dictionary<string, List<Microsoft.Dynamics.AcceleratedSales.SandboxPlugins.Core.Model.EntityAttributes>> EntityAttributes { get; set; }

    [JsonProperty(PropertyName = "entityDisplayName")]
    public Dictionary<string, Label> EntityDisplayName { get; set; }

    [JsonProperty(PropertyName = "primaryNameAttributes")]
    public Dictionary<string, string> PrimaryNameAttributes { get; set; }

    [JsonProperty(PropertyName = "isActivityMapping")]
    public Dictionary<string, bool> IsActivityMapping { get; set; }
  }
}
