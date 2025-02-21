// Decompiled with JetBrains decompiler
// Type: Microsoft.Dynamics.AcceleratedSales.SandboxPlugins.Core.Model.Layout.DesignerLayout
// Assembly: Microsoft.Dynamics.AcceleratedSales.SandboxPlugins, Version=1.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35
// MVID: 7BBE7132-7E30-4424-B38D-27667CFBCEFA
// Assembly location: C:\Users\ngoct\Downloads\New folder\Microsoft.Dynamics.AcceleratedSales.SandboxPlugins_1.0.0.0.dll

using Newtonsoft.Json;
using System.Collections.Generic;

#nullable disable
namespace Microsoft.Dynamics.AcceleratedSales.SandboxPlugins.Core.Model.Layout
{
  public class DesignerLayout
  {
    [JsonProperty(PropertyName = "personaOption")]
    public string PersonaOption { get; set; }

    [JsonProperty(PropertyName = "hiddenCommands")]
    public List<string> HiddenCommands { get; set; }

    [JsonProperty(PropertyName = "rows")]
    public List<DesignerRow> Rows { get; set; }

    public static class PersonaOptions
    {
      public const string RecordInitials = "RecordInitials";
      public const string RecordImage = "RecordImage";
      public const string RecordType = "RecordType";
      public const string ActivityType = "ActivityType";
    }
  }
}
