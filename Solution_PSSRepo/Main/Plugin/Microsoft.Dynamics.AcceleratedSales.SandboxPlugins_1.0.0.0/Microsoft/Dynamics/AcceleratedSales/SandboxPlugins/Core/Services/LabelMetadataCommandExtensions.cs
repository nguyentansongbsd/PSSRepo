// Decompiled with JetBrains decompiler
// Type: Microsoft.Dynamics.AcceleratedSales.SandboxPlugins.Core.Services.LabelMetadataCommandExtensions
// Assembly: Microsoft.Dynamics.AcceleratedSales.SandboxPlugins, Version=1.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35
// MVID: 7BBE7132-7E30-4424-B38D-27667CFBCEFA
// Assembly location: C:\Users\ngoct\Downloads\New folder\Microsoft.Dynamics.AcceleratedSales.SandboxPlugins_1.0.0.0.dll

using Microsoft.Xrm.Sdk;
using System;
using System.Linq;

#nullable disable
namespace Microsoft.Dynamics.AcceleratedSales.SandboxPlugins.Core.Services
{
  public static class LabelMetadataCommandExtensions
  {
    public static string GetLocalizedLabel(this Label label, int localeId)
    {
      if (label == null)
        return (string) null;
      if (!string.IsNullOrEmpty(label.UserLocalizedLabel?.Label))
        return label.UserLocalizedLabel.Label;
      LocalizedLabelCollection localizedLabels = label.LocalizedLabels;
      return localizedLabels != null && localizedLabels.Count > 0 ? label.LocalizedLabels.FirstOrDefault<LocalizedLabel>((Func<LocalizedLabel, bool>) (l => l.LanguageCode == localeId))?.Label ?? label.LocalizedLabels.First<LocalizedLabel>().Label : (string) null;
    }
  }
}
