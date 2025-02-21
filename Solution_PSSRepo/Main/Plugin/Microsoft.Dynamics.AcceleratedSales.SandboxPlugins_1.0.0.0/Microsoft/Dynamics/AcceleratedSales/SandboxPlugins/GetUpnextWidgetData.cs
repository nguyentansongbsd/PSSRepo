// Decompiled with JetBrains decompiler
// Type: Microsoft.Dynamics.AcceleratedSales.SandboxPlugins.GetUpnextWidgetData
// Assembly: Microsoft.Dynamics.AcceleratedSales.SandboxPlugins, Version=1.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35
// MVID: 7BBE7132-7E30-4424-B38D-27667CFBCEFA
// Assembly location: C:\Users\ngoct\Downloads\New folder\Microsoft.Dynamics.AcceleratedSales.SandboxPlugins_1.0.0.0.dll

using Microsoft.Crm.AcceleratedSales.Shared.SandboxServices.Platform;
using Microsoft.Dynamics.AcceleratedSales.SandboxPlugins.Core;
using Microsoft.Dynamics.AcceleratedSales.SandboxPlugins.Core.Model;
using Microsoft.Dynamics.AcceleratedSales.SandboxPlugins.Core.Model.Payload;
using Microsoft.Dynamics.AcceleratedSales.SandboxPlugins.Platform;
using Newtonsoft.Json;
using System;

#nullable disable
namespace Microsoft.Dynamics.AcceleratedSales.SandboxPlugins
{
  public class GetUpnextWidgetData(string unsecureConfiguration, string secureConfiguration) : 
    PluginBase(typeof (GetUpnextWidgetData))
  {
    private const string EntityLogicalName = "msdyn_entitylogicalname";
    private const string EntityRecordId = "msdyn_entityrecordid";
    private const string AdditionalParams = "msdyn_additionalparams";
    private const string Suggestions = "msdyn_suggestions";
    private const string SaSettings = "msdyn_sasettings";
    private const string StepRecords = "msdyn_sequencesteprecords";
    private const string ManualActivities = "msdyn_manualactivities";
    private const string AllStepsForSSeqS = "msdyn_allstepsforsseqs";

    protected override void ExecuteCrmPlugin(LocalPluginContext localContext)
    {
      IAcceleratedSalesLogger logger = (IAcceleratedSalesLogger) new Logger(localContext);
      logger.Execute("AcceleratedSales.GetUpnextWidgetData", (Action) (() => this.ExecutePlugin(localContext, logger)));
    }

    private void ExecutePlugin(
      LocalPluginContext localPluginContext,
      IAcceleratedSalesLogger logger)
    {
      try
      {
        GetUpnextDataRequestParameters getUpnextWidgetDataParams = new GetUpnextDataRequestParameters()
        {
          EntityLogicalName = localPluginContext.GetInputParameter<string>("msdyn_entitylogicalname"),
          EntityRecordId = localPluginContext.GetInputParameter<string>("msdyn_entityrecordid"),
          AdditionalParameters = JsonConvert.DeserializeObject<GetUpnextWidgetDataAPIAdditionalParams>(localPluginContext.GetInputParameter<string>("msdyn_additionalparams")),
          SASettings = localPluginContext.GetInputParameter<string>("msdyn_sasettings")
        };
        UpnextWidgetDataResponse widgetDataResponse = new GetUpnextWidgetDataCommand((IDataStore) new DataverseStore(localPluginContext), logger).Execute(getUpnextWidgetDataParams);
        logger.LogWarning("GetUpnextWidgetData.OwnerId: " + localPluginContext.PluginExecutionContext.UserId.ToString(), callerName: nameof (ExecutePlugin), callerFile: "C:\\__w\\1\\s\\solutions\\AcceleratedSales\\AcceleratedSalesSandboxPlugins\\CustomActions\\GetUpnextWidgetData.cs");
        logger.Execute("AcceleratedSales.GetUpnextWidgetData.Logging", (Action) (() => logger.AddCustomProperty("GetUpnextWidgetData API: starting execution. GetUpnextWidgetData.RequestPayload : ", (object) (JsonConvert.SerializeObject((object) getUpnextWidgetDataParams) ?? "null"))));
        if (widgetDataResponse == null)
          return;
        localPluginContext.PluginExecutionContext.OutputParameters["msdyn_suggestions"] = (object) JsonConvert.SerializeObject((object) widgetDataResponse.Suggestions);
        localPluginContext.PluginExecutionContext.OutputParameters["msdyn_sequencesteprecords"] = (object) JsonConvert.SerializeObject((object) widgetDataResponse.SequenceStepRecords);
        localPluginContext.PluginExecutionContext.OutputParameters["msdyn_manualactivities"] = (object) JsonConvert.SerializeObject((object) widgetDataResponse.ActivityRecords);
        localPluginContext.PluginExecutionContext.OutputParameters["msdyn_sasettings"] = (object) JsonConvert.SerializeObject((object) widgetDataResponse.UpnextSettings);
        localPluginContext.PluginExecutionContext.OutputParameters["msdyn_allstepsforsseqs"] = (object) JsonConvert.SerializeObject((object) widgetDataResponse.AllStepsForSSeqS);
      }
      catch (Exception ex)
      {
        logger.LogError("GetUpnextWidgetData API: execution failed.", ex, callerName: nameof (ExecutePlugin), callerFile: "C:\\__w\\1\\s\\solutions\\AcceleratedSales\\AcceleratedSalesSandboxPlugins\\CustomActions\\GetUpnextWidgetData.cs");
      }
    }
  }
}
