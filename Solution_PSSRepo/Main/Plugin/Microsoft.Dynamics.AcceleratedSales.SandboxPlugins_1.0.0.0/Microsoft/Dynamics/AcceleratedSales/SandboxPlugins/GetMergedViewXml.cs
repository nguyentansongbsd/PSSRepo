// Decompiled with JetBrains decompiler
// Type: Microsoft.Dynamics.AcceleratedSales.SandboxPlugins.GetMergedViewXml
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
  public class GetMergedViewXml(string unsecureConfiguration, string secureConfiguration) : 
    PluginBase(typeof (GetMergedViewXml))
  {
    protected override void ExecuteCrmPlugin(LocalPluginContext localContext)
    {
      IAcceleratedSalesLogger logger = (IAcceleratedSalesLogger) new Logger(localContext);
      logger.Execute("AcceleratedSales.GetMergedViewXml", (Action) (() => this.ExecutePlugin(localContext, logger)));
    }

    private void ExecutePlugin(
      LocalPluginContext localPluginContext,
      IAcceleratedSalesLogger logger)
    {
      string inputParameter = localPluginContext.GetInputParameter<string>("msdyn_payload");
      GetWorklistFilteredDataRequest requestPayload = new GetWorklistFilteredDataRequest()
      {
        Payload = inputParameter
      };
      Dictionary<string, object> customProperties = new Dictionary<string, object>()
      {
        {
          "GetMergedViewXml API: starting execution.",
          (object) "true"
        },
        {
          "GetMergedViewXml.UserId",
          (object) localPluginContext.PluginExecutionContext.UserId
        }
      };
      logger.AddCustomProperties(customProperties);
      IDataStore dataStore = (IDataStore) new DataverseStore(localPluginContext);
      DataverseEntityMetadataProvider entityMetadataProvider = new DataverseEntityMetadataProvider(dataStore, logger);
      GetWorklistFilteredDataResponse filteredDataResponse = new GetMergedViewXmlCommand(dataStore, (IEntityMetadataProvider) entityMetadataProvider, logger, requestPayload).Execute();
      localPluginContext.PluginExecutionContext.OutputParameters["msdyn_response"] = (object) filteredDataResponse?.Payload;
    }
  }
}
