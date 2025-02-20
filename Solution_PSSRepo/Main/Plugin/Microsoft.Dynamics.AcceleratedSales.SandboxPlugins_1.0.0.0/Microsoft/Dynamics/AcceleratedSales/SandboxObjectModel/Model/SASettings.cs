// Decompiled with JetBrains decompiler
// Type: Microsoft.Dynamics.AcceleratedSales.SandboxObjectModel.Model.SASettings
// Assembly: Microsoft.Dynamics.AcceleratedSales.SandboxPlugins, Version=1.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35
// MVID: 7BBE7132-7E30-4424-B38D-27667CFBCEFA
// Assembly location: C:\Users\ngoct\Downloads\New folder\Microsoft.Dynamics.AcceleratedSales.SandboxPlugins_1.0.0.0.dll

#nullable disable
namespace Microsoft.Dynamics.AcceleratedSales.SandboxObjectModel.Model
{
  internal class SASettings
  {
    public SettingsInstance settingsInstance { get; set; }

    public bool isEnabledForOrganization { get; set; }

    public bool isEnabledForUser { get; set; }

    public bool isViralTrial { get; set; }

    public bool isAdminUser { get; set; }
  }
}
