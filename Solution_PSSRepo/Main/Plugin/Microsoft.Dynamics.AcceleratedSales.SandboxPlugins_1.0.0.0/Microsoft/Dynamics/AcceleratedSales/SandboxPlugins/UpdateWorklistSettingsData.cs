// Decompiled with JetBrains decompiler
// Type: Microsoft.Dynamics.AcceleratedSales.SandboxPlugins.UpdateWorklistSettingsData
// Assembly: Microsoft.Dynamics.AcceleratedSales.SandboxPlugins, Version=1.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35
// MVID: 7BBE7132-7E30-4424-B38D-27667CFBCEFA
// Assembly location: C:\Users\ngoct\Downloads\New folder\Microsoft.Dynamics.AcceleratedSales.SandboxPlugins_1.0.0.0.dll

using Microsoft.Crm.AcceleratedSales.Shared.SandboxServices.Platform;
using Microsoft.Dynamics.AcceleratedSales.SandboxPlugins.Core;
using Microsoft.Dynamics.AcceleratedSales.SandboxPlugins.Core.Model;
using Microsoft.Dynamics.AcceleratedSales.SandboxPlugins.Platform;
using System;
using System.Collections.Generic;

#nullable disable
namespace Microsoft.Dynamics.AcceleratedSales.SandboxPlugins
{
  public class UpdateWorklistSettingsData(string unsecureConfiguration, string secureConfiguration) : 
    PluginBase(typeof (UpdateWorklistSettingsData))
  {
    protected override void ExecuteCrmPlugin(LocalPluginContext localContext)
    {
      IAcceleratedSalesLogger logger = (IAcceleratedSalesLogger) new Logger(localContext);
      logger.Execute("AcceleratedSales.UpdateWorklistSettingsData", (Action) (() => this.ExecutePlugin(localContext, logger)));
    }

    private void ExecutePlugin(
      LocalPluginContext localPluginContext,
      IAcceleratedSalesLogger logger)
    {
      string inputParameter = localPluginContext.GetInputParameter<string>("msdyn_payload");
      UpdateWorklistSettingsDataRequest requestPayload = new UpdateWorklistSettingsDataRequest()
      {
        Payload = inputParameter
      };
      Dictionary<string, object> customProperties = new Dictionary<string, object>()
      {
        {
          "UpdateWorklistSettingsData.OwnerId",
          (object) localPluginContext.PluginExecutionContext.UserId
        }
      };
      logger.LogInfo("UpdateWorklistSettingsData API: starting execution.", customProperties, nameof (ExecutePlugin), "C:\\__w\\1\\s\\solutions\\AcceleratedSales\\AcceleratedSalesSandboxPlugins\\CustomActions\\UpdateWorklistSettingsData.cs");
      IDataStore dataStore = (IDataStore) new DataverseStore(localPluginContext);
      IEntityMetadataProvider entityMetadataProvider = (IEntityMetadataProvider) new DataverseEntityMetadataProvider(dataStore, logger);
      UpdateWorklistSettingsDataResponse settingsDataResponse = new UpdateWorklistSettingsDataCommand(dataStore, entityMetadataProvider, logger, requestPayload).Execute();
      localPluginContext.PluginExecutionContext.OutputParameters["msdyn_response"] = (object) settingsDataResponse?.Payload;
    }
  }
}
