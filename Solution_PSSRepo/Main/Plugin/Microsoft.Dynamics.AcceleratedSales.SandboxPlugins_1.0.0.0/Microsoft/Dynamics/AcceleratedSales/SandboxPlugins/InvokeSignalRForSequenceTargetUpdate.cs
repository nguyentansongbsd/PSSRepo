// Decompiled with JetBrains decompiler
// Type: Microsoft.Dynamics.AcceleratedSales.SandboxPlugins.InvokeSignalRForSequenceTargetUpdate
// Assembly: Microsoft.Dynamics.AcceleratedSales.SandboxPlugins, Version=1.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35
// MVID: 7BBE7132-7E30-4424-B38D-27667CFBCEFA
// Assembly location: C:\Users\ngoct\Downloads\New folder\Microsoft.Dynamics.AcceleratedSales.SandboxPlugins_1.0.0.0.dll

using Microsoft.Crm.AcceleratedSales.Shared.SandboxServices.Platform;
using Microsoft.Dynamics.AcceleratedSales.SandboxObjectModel.Helper;
using Microsoft.Dynamics.AcceleratedSales.SandboxObjectModel.Model;
using Microsoft.Dynamics.AcceleratedSales.SandboxPlugins.Core.Model;
using Microsoft.Dynamics.AcceleratedSales.SandboxPlugins.Platform;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.PluginTelemetry;
using Microsoft.Xrm.Sdk.Query;
using Newtonsoft.Json;
using System;

#nullable disable
namespace Microsoft.Dynamics.AcceleratedSales.SandboxPlugins
{
  public class InvokeSignalRForSequenceTargetUpdate : PluginBase
  {
    private const string WorkspaceFCSNamespace = "SalesService.Workspace";
    private const string DisableSignalRInSAFCSName = "DisableSignalRInSA";

    public InvokeSignalRForSequenceTargetUpdate()
      : base(typeof (InvokeSignalRForSequenceTargetUpdate))
    {
    }

    protected override void ExecuteCrmPlugin(LocalPluginContext localcontext)
    {
      IServiceProvider getServiceProvider = localcontext.GetServiceProvider;
      ILogger service = (ILogger) getServiceProvider.GetService(typeof (ILogger));
      localcontext.TraceOnPlugInTraceLog("Notifying signalR service after msdyn_sequencetarget update");
      bool flag1 = false;
      OrganizationRequest request = new OrganizationRequest("GetFeatureEnabledState")
      {
        ["FeatureName"] = (object) "FCB.DisableSAFeaturesNotSupportedInGCC"
      };
      OrganizationResponse organizationResponse = localcontext.SystemUserOrganizationService.Execute(request);
      if (organizationResponse != null && organizationResponse.Results != null && organizationResponse.Results.ContainsKey("IsFeatureEnabled"))
        flag1 = (bool) organizationResponse.Results["IsFeatureEnabled"];
      IDataStore dataStore = (IDataStore) new DataverseStore(localcontext);
      bool flag2 = dataStore.IsFCSEnabled("SalesService.Workspace", "DisableSignalRInSA");
      bool flag3 = dataStore.IsTemplateOrg(service);
      Entity fromInputParameters = localcontext.GetTargetFromInputParameters<Entity>();
      if (!SequenceTargetIdForSampleData.IsSampleDataRecord(fromInputParameters, service, localcontext.PluginExecutionContext) && !flag1 && !flag2 && !flag3)
      {
        Entity preImage = localcontext.GetPreImage<Entity>("PreImageSequenceTargetUpdate");
        string[] strArray = new string[3]
        {
          "ownerid",
          "statecode",
          "msdyn_regarding"
        };
        Entity entity = localcontext.OrganizationService.Retrieve(fromInputParameters.LogicalName, fromInputParameters.Id, new ColumnSet(strArray));
        EntityReference entityReference = (EntityReference) entity.Attributes["ownerid"];
        string attribute1 = (string) entity.Attributes["msdyn_regarding"];
        if (entityReference != null && !string.IsNullOrEmpty(attribute1) && entityReference.LogicalName != "systemuser")
        {
          Regarding regardingObject = JsonConvert.DeserializeObject<Regarding>(attribute1);
          EntityReference customOwner = MessageHelper.GetCustomOwner(localcontext, regardingObject);
          if (customOwner != null)
            entityReference = customOwner;
        }
        if (entityReference.LogicalName == "systemuser")
        {
          OptionSetValue attribute2 = (OptionSetValue) preImage.Attributes["statecode"];
          OptionSetValue attribute3 = (OptionSetValue) entity.Attributes["statecode"];
          if (attribute2.Value == 0 && attribute3.Value == 1)
          {
            InvokeAzureFunctionApp azureFunctionApp = new InvokeAzureFunctionApp(getServiceProvider, entityReference.Id);
            SignalRNotificationRequest requestForSignalR = azureFunctionApp.CreateRequestForSignalR(fromInputParameters, "Update", attribute1);
            azureFunctionApp.InvokeNotificationFunction(requestForSignalR);
            localcontext.TraceOnPlugInTraceLog("Completed notifying signalR service after msdyn_sequencetarget state update");
            return;
          }
        }
        localcontext.TraceOnPlugInTraceLog("Did not notify signalR service after msdyn_sequencetarget update");
      }
      else
      {
        localcontext.TraceOnPlugInTraceLog(string.Format("Ignore signalR notification as feature FCB.DisableSAFeaturesNotSupportedInGCC is enabled: {0} or feature FCB.DisableSignalRInSA is enabled: {1}", (object) flag1, (object) flag2));
        service.AddCustomProperty("IsDisableSAFeaturesNotSupportedInGCCEnabled", flag1.ToString());
        service.AddCustomProperty("DisableSignalRInSA", flag2.ToString());
        service.AddCustomProperty("IsTemplateOrg", flag3.ToString());
      }
    }
  }
}
