// Decompiled with JetBrains decompiler
// Type: Microsoft.Dynamics.AcceleratedSales.SandboxPlugins.Core.Model.Tags.DisplayConfiguration
// Assembly: Microsoft.Dynamics.AcceleratedSales.SandboxPlugins, Version=1.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35
// MVID: 7BBE7132-7E30-4424-B38D-27667CFBCEFA
// Assembly location: C:\Users\ngoct\Downloads\New folder\Microsoft.Dynamics.AcceleratedSales.SandboxPlugins_1.0.0.0.dll

using Newtonsoft.Json;

#nullable disable
namespace Microsoft.Dynamics.AcceleratedSales.SandboxPlugins.Core.Model.Tags
{
  public class DisplayConfiguration
  {
    [JsonProperty(PropertyName = "displayType")]
    public DisplayType DisplayType { get; set; }

    [JsonProperty(PropertyName = "displayText")]
    public string DisplayText { get; set; }

    [JsonProperty(PropertyName = "iconConfig")]
    public IconConfig IconConfig { get; set; }

    [JsonProperty(PropertyName = "tagStyle")]
    public TagStyle TagStyle { get; set; }

    [JsonProperty(PropertyName = "textStyle")]
    public TextStyle TextStyle { get; set; }

    [JsonProperty(PropertyName = "tooltipType")]
    public TooltipType TooltipType { get; set; }

    [JsonProperty(PropertyName = "tooltipText")]
    public string TooltipText { get; set; }
  }
}
