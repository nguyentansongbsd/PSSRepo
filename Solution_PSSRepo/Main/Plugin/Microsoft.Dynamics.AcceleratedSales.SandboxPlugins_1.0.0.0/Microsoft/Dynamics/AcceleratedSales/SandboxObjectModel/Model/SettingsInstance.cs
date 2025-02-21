// Decompiled with JetBrains decompiler
// Type: Microsoft.Dynamics.AcceleratedSales.SandboxObjectModel.Model.SettingsInstance
// Assembly: Microsoft.Dynamics.AcceleratedSales.SandboxPlugins, Version=1.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35
// MVID: 7BBE7132-7E30-4424-B38D-27667CFBCEFA
// Assembly location: C:\Users\ngoct\Downloads\New folder\Microsoft.Dynamics.AcceleratedSales.SandboxPlugins_1.0.0.0.dll

using System;
using System.Collections.Generic;

#nullable disable
namespace Microsoft.Dynamics.AcceleratedSales.SandboxObjectModel.Model
{
  internal class SettingsInstance
  {
    public Guid settingsId;
    public Dictionary<string, EntityConfiguration> entityConfigurations;

    public bool isAutoCreatePhoneCallEnabled { get; set; }

    public bool shouldLinkSequenceStepToActivity { get; set; }

    public bool isFCCEnabled { get; set; }

    public int? calendarType { get; set; }

    public MigrationStatus migrationStatus { get; set; }

    public bool isWorkScheduleEnabled { get; set; }

    public List<string> securityRoles { get; set; }

    public List<string> securityRolesNew { get; set; }

    public List<string> recommendationSecurityRoles { get; set; }

    public AdminLinkingConfiguration linkingConfiguration { get; set; }

    public bool isDefaultSetting { get; set; }
  }
}
