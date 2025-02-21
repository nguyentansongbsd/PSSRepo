// Decompiled with JetBrains decompiler
// Type: Microsoft.Dynamics.AcceleratedSales.SandboxPlugins.Core.Model.UpdateWorklistSettingsViewConfiguration
// Assembly: Microsoft.Dynamics.AcceleratedSales.SandboxPlugins, Version=1.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35
// MVID: 7BBE7132-7E30-4424-B38D-27667CFBCEFA
// Assembly location: C:\Users\ngoct\Downloads\New folder\Microsoft.Dynamics.AcceleratedSales.SandboxPlugins_1.0.0.0.dll

using Microsoft.Crm.AcceleratedSales.Shared.SandboxServices.Platform;
using Newtonsoft.Json;
using System;

#nullable disable
namespace Microsoft.Dynamics.AcceleratedSales.SandboxPlugins.Core.Model
{
  public class UpdateWorklistSettingsViewConfiguration
  {
    [JsonProperty(PropertyName = "adminMode")]
    public bool AdminMode { get; set; }

    [JsonProperty(PropertyName = "adminConfig")]
    public WorklistAdminConfiguration AdminConfig { get; set; }

    [JsonProperty(PropertyName = "userConfig")]
    public WorklistSellerConfiguration UserConfig { get; set; }

    public static UpdateWorklistSettingsViewConfiguration FromRequestPayload(
      UpdateWorklistSettingsDataRequest requestPayload,
      IAcceleratedSalesLogger logger)
    {
      UpdateWorklistSettingsViewConfiguration viewConfiguration;
      try
      {
        viewConfiguration = JsonConvert.DeserializeObject<UpdateWorklistSettingsViewConfiguration>(requestPayload?.Payload);
      }
      catch (Exception ex)
      {
        logger.LogError("UpdateWorklistSettingsViewConfiguration.FromRequestPayload.Exception", ex, callerName: nameof (FromRequestPayload), callerFile: "C:\\__w\\1\\s\\solutions\\AcceleratedSales\\AcceleratedSalesSandboxPlugins\\Core\\Model\\UpdateWorklistSettingsViewConfiguration.cs");
        throw new CrmException("Invalid request.", ex, 1879506961);
      }
      UpdateWorklistSettingsViewConfiguration.ValidatedThrowErrorIfRequiredParametersMissing(viewConfiguration);
      UpdateWorklistSettingsViewConfiguration.SetSettingsViewConfigDefaults(viewConfiguration);
      return viewConfiguration;
    }

    private static void SetSettingsViewConfigDefaults(
      UpdateWorklistSettingsViewConfiguration updateWorklistSettingsViewConfiguration)
    {
      updateWorklistSettingsViewConfiguration.AdminMode = updateWorklistSettingsViewConfiguration.AdminMode;
      updateWorklistSettingsViewConfiguration.AdminConfig = updateWorklistSettingsViewConfiguration.AdminConfig ?? new WorklistAdminConfiguration();
      updateWorklistSettingsViewConfiguration.UserConfig = updateWorklistSettingsViewConfiguration.UserConfig ?? new WorklistSellerConfiguration();
    }

    private static void ValidatedThrowErrorIfRequiredParametersMissing(
      UpdateWorklistSettingsViewConfiguration config)
    {
      if (config.AdminMode && config.AdminConfig == null)
        throw new CrmException("UpdateWorklistSettingsViewConfiguration.RequiredRequestParametersMissing", 1879441414);
      if (config.UserConfig == null)
        throw new CrmException("UpdateWorklistSettingsViewConfiguration.RequiredRequestParametersMissing", 1879441415);
    }
  }
}
