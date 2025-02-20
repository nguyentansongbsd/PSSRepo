// Decompiled with JetBrains decompiler
// Type: Microsoft.Dynamics.AcceleratedSales.SandboxPlugins.Core.Model.SettingsViewConfiguration
// Assembly: Microsoft.Dynamics.AcceleratedSales.SandboxPlugins, Version=1.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35
// MVID: 7BBE7132-7E30-4424-B38D-27667CFBCEFA
// Assembly location: C:\Users\ngoct\Downloads\New folder\Microsoft.Dynamics.AcceleratedSales.SandboxPlugins_1.0.0.0.dll

using Microsoft.Crm.AcceleratedSales.Shared.SandboxServices.Platform;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics;

#nullable disable
namespace Microsoft.Dynamics.AcceleratedSales.SandboxPlugins.Core.Model
{
  public class SettingsViewConfiguration
  {
    [JsonProperty(PropertyName = "entityNames")]
    public IReadOnlyList<string> EntityNames { get; set; }

    public static SettingsViewConfiguration FromRequestPayload(
      GetWorklistSettingsDataRequest requestPayload,
      IAcceleratedSalesLogger logger)
    {
      SettingsViewConfiguration viewConfiguration;
      try
      {
        Stopwatch stopwatch = Stopwatch.StartNew();
        viewConfiguration = JsonConvert.DeserializeObject<SettingsViewConfiguration>(requestPayload?.Payload);
        stopwatch.Stop();
      }
      catch (Exception ex)
      {
        logger.LogError("WorklistSettingsViewConfiguration.FromRequestPayload.Exception", ex, callerName: nameof (FromRequestPayload), callerFile: "C:\\__w\\1\\s\\solutions\\AcceleratedSales\\AcceleratedSalesSandboxPlugins\\Core\\Model\\SettingsViewConfiguration.cs");
        throw new CrmException("Invalid request.", ex, 1879506960);
      }
      SettingsViewConfiguration.ValidatedThrowErrorIfRequiredParametersMissing(viewConfiguration);
      SettingsViewConfiguration.SetSeetingsViewConfigDefaults(viewConfiguration);
      return viewConfiguration;
    }

    private static void SetSeetingsViewConfigDefaults(SettingsViewConfiguration settingsViewConfig)
    {
      settingsViewConfig.EntityNames = settingsViewConfig.EntityNames;
    }

    private static void ValidatedThrowErrorIfRequiredParametersMissing(
      SettingsViewConfiguration config)
    {
      if (config == null || config.EntityNames == null || config.EntityNames.Count == 0)
        throw new CrmException("WorklistSettingsViewConfiguration.RequiredRequestParametersMissing", 1879441411);
    }
  }
}
