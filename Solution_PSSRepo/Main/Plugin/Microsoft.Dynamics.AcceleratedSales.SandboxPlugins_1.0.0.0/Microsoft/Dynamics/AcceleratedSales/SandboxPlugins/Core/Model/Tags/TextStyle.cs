﻿// Decompiled with JetBrains decompiler
// Type: Microsoft.Dynamics.AcceleratedSales.SandboxPlugins.Core.Model.Tags.TextStyle
// Assembly: Microsoft.Dynamics.AcceleratedSales.SandboxPlugins, Version=1.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35
// MVID: 7BBE7132-7E30-4424-B38D-27667CFBCEFA
// Assembly location: C:\Users\ngoct\Downloads\New folder\Microsoft.Dynamics.AcceleratedSales.SandboxPlugins_1.0.0.0.dll

using Newtonsoft.Json;

#nullable disable
namespace Microsoft.Dynamics.AcceleratedSales.SandboxPlugins.Core.Model.Tags
{
  public class TextStyle
  {
    [JsonProperty(PropertyName = "fontFamily")]
    public string FontFamily { get; set; }

    [JsonProperty(PropertyName = "fontSize")]
    public string FontSize { get; set; }

    [JsonProperty(PropertyName = "color")]
    public string Color { get; set; }

    [JsonProperty(PropertyName = "fontWeight")]
    public string FontWeight { get; set; }
  }
}
