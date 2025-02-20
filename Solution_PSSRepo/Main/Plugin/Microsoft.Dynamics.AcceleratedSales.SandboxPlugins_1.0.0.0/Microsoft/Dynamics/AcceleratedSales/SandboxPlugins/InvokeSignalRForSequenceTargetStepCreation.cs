// Decompiled with JetBrains decompiler
// Type: Microsoft.Dynamics.AcceleratedSales.SandboxPlugins.InvokeSignalRForSequenceTargetStepCreation
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
using System.Diagnostics.CodeAnalysis;

#nullable disable
namespace Microsoft.Dynamics.AcceleratedSales.SandboxPlugins
{
  [ExcludeFromCodeCoverage]
  public class InvokeSignalRForSequenceTargetStepCreation : PluginBase
  {
    private const string WorkspaceFCSNamespace = "SalesService.Workspace";
    private const string DisableSignalRInSAFCSName = "DisableSignalRInSA";

    public InvokeSignalRForSequenceTargetStepCreation()
      : base(typeof (InvokeSignalRForSequenceTargetStepCreation))
    {
    }

    protected override void ExecuteCrmPlugin(LocalPluginContext localContext)
    {
      IServiceProvider getServiceProvider = localContext.GetServiceProvider;
      ILogger service = (ILogger) getServiceProvider.GetService(typeof (ILogger));
      localContext.TraceOnPlugInTraceLog("Notifying signalR service after msdyn_sequencetargetstep");
      Entity fromInputParameters = localContext.GetTargetFromInputParameters<Entity>();
      bool flag1 = false;
      OrganizationRequest request = new OrganizationRequest("GetFeatureEnabledState")
      {
        ["FeatureName"] = (object) "FCB.DisableSAFeaturesNotSupportedInGCC"
      };
      OrganizationResponse organizationResponse = localContext.SystemUserOrganizationService.Execute(request);
      if (organizationResponse != null && organizationResponse.Results != null && organizationResponse.Results.ContainsKey("IsFeatureEnabled"))
        flag1 = (bool) organizationResponse.Results["IsFeatureEnabled"];
      IDataStore dataStore = (IDataStore) new DataverseStore(localContext);
      bool flag2 = dataStore.IsFCSEnabled("SalesService.Workspace", "DisableSignalRInSA");
      bool flag3 = dataStore.IsTemplateOrg(service);
      if (!SequenceTargetIdForSampleData.IsSampleDataRecord(fromInputParameters, service, localContext.PluginExecutionContext) && !flag1 && !flag2 && !flag3)
      {
        Entity entity1 = localContext.OrganizationService.Retrieve(fromInputParameters.LogicalName, fromInputParameters.Id, new ColumnSet(new string[2]
        {
          "ownerid",
          "msdyn_sequencetarget"
        }));
        EntityReference entityReference = (EntityReference) entity1.Attributes["ownerid"];
        EntityReference attribute = (EntityReference) entity1.Attributes["msdyn_sequencetarget"];
        if (entityReference != null && attribute != null && entityReference.LogicalName != "systemuser")
        {
          Entity entity2 = localContext.OrganizationService.Retrieve(attribute.LogicalName, attribute.Id, new ColumnSet(new string[1]
          {
            "msdyn_regarding"
          }));
          if (entity2.Contains("msdyn_regarding") && entity2["msdyn_regarding"] != null)
          {
            Regarding regardingObject = JsonConvert.DeserializeObject<Regarding>(entity2.GetAttributeValue<string>("msdyn_regarding"));
            EntityReference customOwner = MessageHelper.GetCustomOwner(localContext, regardingObject);
            if (customOwner != null)
              entityReference = customOwner;
          }
        }
        if (entityReference.LogicalName == "systemuser")
        {
          InvokeAzureFunctionApp azureFunctionApp = new InvokeAzureFunctionApp(getServiceProvider, entityReference.Id);
          SignalRNotificationRequest requestForSignalR = azureFunctionApp.CreateRequestForSignalR(fromInputParameters, "Create");
          azureFunctionApp.InvokeNotificationFunction(requestForSignalR);
          localContext.TraceOnPlugInTraceLog(string.Format("Notified signalR service after msdyn_sequencetargetstep for RecordId:{0}, {1}", (object) fromInputParameters.Id, (object) fromInputParameters.LogicalName));
        }
        else
          localContext.TraceOnPlugInTraceLog(string.Format("Not notified signalR service after msdyn_sequencetargetstep for RecordId:{0}, {1}. Record was owned by team", (object) fromInputParameters.Id, (object) fromInputParameters.LogicalName));
      }
      else
      {
        localContext.TraceOnPlugInTraceLog(string.Format("Ignore signalR notification for sample data records for RecordId:{0}, {1} as well as if feature FCB.DisableSAFeaturesNotSupportedInGCC is enabled: {2} or if feature FCS DisableSignalRInSA is enabled: {3}", (object) fromInputParameters.Id, (object) fromInputParameters.LogicalName, (object) flag1, (object) flag2));
        service.AddCustomProperty("IsDisableSAFeaturesNotSupportedInGCCEnabled", flag1.ToString());
        service.AddCustomProperty("DisableSignalRInSA", flag2.ToString());
        service.AddCustomProperty("IsTemplateOrg", flag3.ToString());
      }
    }
  }
}
