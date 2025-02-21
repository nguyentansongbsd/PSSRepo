// Decompiled with JetBrains decompiler
// Type: Microsoft.Dynamics.AcceleratedSales.SandboxPlugins.GetWorklistData
// Assembly: Microsoft.Dynamics.AcceleratedSales.SandboxPlugins, Version=1.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35
// MVID: 7BBE7132-7E30-4424-B38D-27667CFBCEFA
// Assembly location: C:\Users\ngoct\Downloads\New folder\Microsoft.Dynamics.AcceleratedSales.SandboxPlugins_1.0.0.0.dll

using Microsoft.Crm.AcceleratedSales.Shared.SandboxServices.Platform;
using Microsoft.Dynamics.AcceleratedSales.SandboxPlugins.Core;
using Microsoft.Dynamics.AcceleratedSales.SandboxPlugins.Core.Model;
using Microsoft.Dynamics.AcceleratedSales.SandboxPlugins.Platform;
using System;

#nullable disable
namespace Microsoft.Dynamics.AcceleratedSales.SandboxPlugins
{
  public class GetWorklistData(string unsecureConfiguration, string secureConfiguration) : PluginBase(typeof (GetWorklistData))
  {
    protected override void ExecuteCrmPlugin(LocalPluginContext localContext)
    {
      IAcceleratedSalesLogger logger = (IAcceleratedSalesLogger) new Logger(localContext);
      logger.Execute("AcceleratedSales.GetWorklistData", (Action) (() => this.ExecutePlugin(localContext, logger)));
    }

    private void ExecutePlugin(
      LocalPluginContext localPluginContext,
      IAcceleratedSalesLogger logger)
    {
      string payload = localPluginContext.GetInputParameter<string>("msdyn_payload");
      GetWorklistDataRequest requestPayload = new GetWorklistDataRequest()
      {
        Payload = payload
      };
      logger.LogWarning("GetWorklistData.OwnerId: " + localPluginContext.PluginExecutionContext.UserId.ToString(), callerName: nameof (ExecutePlugin), callerFile: "C:\\__w\\1\\s\\solutions\\AcceleratedSales\\AcceleratedSalesSandboxPlugins\\CustomActions\\GetWorklistData.cs");
      logger.Execute("AcceleratedSales.GetWorklistData.Logging", (Action) (() => logger.AddCustomProperty("GetWorklistData API: starting execution. GetWorklistData.RequestPayload : ", (object) (payload ?? "is null"))));
      IDataStore dataStore = (IDataStore) new DataverseStore(localPluginContext);
      IEntityMetadataProvider entityMetadataProvider = (IEntityMetadataProvider) new DataverseEntityMetadataProvider(dataStore, logger);
      GetWorklistDataResponse worklistDataResponse = new GetWorklistDataCommand(dataStore, entityMetadataProvider, logger, requestPayload).Execute();
      localPluginContext.PluginExecutionContext.OutputParameters["msdyn_response"] = (object) worklistDataResponse?.Payload;
    }
  }
}
