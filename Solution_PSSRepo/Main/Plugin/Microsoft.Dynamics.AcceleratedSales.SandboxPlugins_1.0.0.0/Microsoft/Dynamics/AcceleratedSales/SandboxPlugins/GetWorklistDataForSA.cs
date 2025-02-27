// Decompiled with JetBrains decompiler
// Type: Microsoft.Dynamics.AcceleratedSales.SandboxPlugins.GetWorklistDataForSA
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
  public class GetWorklistDataForSA(string unsecureConfiguration, string secureConfiguration) : 
    PluginBase(typeof (GetWorklistDataForSA))
  {
    protected override void ExecuteCrmPlugin(LocalPluginContext localContext)
    {
      IAcceleratedSalesLogger logger = (IAcceleratedSalesLogger) new Logger(localContext);
      logger.Execute("AcceleratedSales.GetWorklistDataForSA", (Action) (() => this.ExecutePlugin(localContext, logger)));
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
      logger.LogWarning("GetWorklistDataForSA.OwnerId: " + localPluginContext.PluginExecutionContext.UserId.ToString(), callerName: nameof (ExecutePlugin), callerFile: "C:\\__w\\1\\s\\solutions\\AcceleratedSales\\AcceleratedSalesSandboxPlugins\\CustomActions\\GetWorklistDataForSA.cs");
      logger.Execute("AcceleratedSales.GetWorklistDataForSA.Logging", (Action) (() => logger.AddCustomProperty("GetWorklistDataForSA API: starting execution. GetWorklistDataForSA.RequestPayload : ", (object) (payload ?? "is null"))));
      IDataStore dataStore = (IDataStore) new DataverseStore(localPluginContext);
      IEntityMetadataProvider entityMetadataProvider = (IEntityMetadataProvider) new DataverseEntityMetadataProvider(dataStore, logger);
      GetWorklistDataResponse worklistDataResponse = new GetWorklistDataForSACommand(dataStore, entityMetadataProvider, logger, requestPayload).Execute();
      localPluginContext.PluginExecutionContext.OutputParameters["msdyn_response"] = (object) worklistDataResponse?.Payload;
    }
  }
}
