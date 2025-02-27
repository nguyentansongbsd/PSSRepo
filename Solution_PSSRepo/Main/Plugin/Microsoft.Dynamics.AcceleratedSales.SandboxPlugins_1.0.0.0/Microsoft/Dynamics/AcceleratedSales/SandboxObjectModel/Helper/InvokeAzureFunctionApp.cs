// Decompiled with JetBrains decompiler
// Type: Microsoft.Dynamics.AcceleratedSales.SandboxObjectModel.Helper.InvokeAzureFunctionApp
// Assembly: Microsoft.Dynamics.AcceleratedSales.SandboxPlugins, Version=1.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35
// MVID: 7BBE7132-7E30-4424-B38D-27667CFBCEFA
// Assembly location: C:\Users\ngoct\Downloads\New folder\Microsoft.Dynamics.AcceleratedSales.SandboxPlugins_1.0.0.0.dll

using Microsoft.Dynamics.AcceleratedSales.SandboxObjectModel.Model;
using Microsoft.Xrm.Sdk;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Net.Http;

#nullable disable
namespace Microsoft.Dynamics.AcceleratedSales.SandboxObjectModel.Helper
{
  internal class InvokeAzureFunctionApp
  {
    private IServiceProvider serviceProvider;
    private IPluginExecutionContext context;
    private ITracingService tracingService;
    private Guid userId;

    public InvokeAzureFunctionApp(IServiceProvider serviceProvider, Guid userId)
    {
      this.serviceProvider = serviceProvider;
      this.context = (IPluginExecutionContext) serviceProvider.GetService(typeof (IPluginExecutionContext));
      this.tracingService = (ITracingService) serviceProvider.GetService(typeof (ITracingService));
      this.userId = userId;
    }

    public void GetSignalRInstance()
    {
      string inputParameter1 = this.context.InputParameters["ServiceName"] as string;
      string inputParameter2 = this.context.InputParameters["WebApiName"] as string;
      string inputParameter3 = this.context.InputParameters["RequestJson"] as string;
      this.CheckForInputParametersForNullOrEmpty(inputParameter1, inputParameter2, inputParameter3);
      string str = this.InvokeAzureFunctionAppService(inputParameter1, inputParameter2, inputParameter3);
      this.context.OutputParameters.Clear();
      this.context.OutputParameters.Add("response", (object) str);
    }

    public void InvokeNotificationFunction(SignalRNotificationRequest requestModel)
    {
      string serviceName = "mf-services";
      string webApiName = "forecast-signalr-notify";
      string requestJson = AzureFunctionHelper.Serialize<SignalRNotificationRequest>(requestModel);
      this.CheckForInputParametersForNullOrEmpty(serviceName, webApiName, requestJson);
      this.InvokeAzureFunctionAppService(serviceName, webApiName, requestJson);
    }

    public SignalRNotificationRequest CreateRequestForSignalR(
      Entity inputEntity,
      string eventname,
      string relatedEntity = "")
    {
      IPluginExecutionContext service = (IPluginExecutionContext) this.serviceProvider.GetService(typeof (IPluginExecutionContext));
      Dictionary<string, string> dictionary = new Dictionary<string, string>()
      {
        {
          "eventName",
          eventname
        },
        {
          "entityType",
          inputEntity.LogicalName
        },
        {
          "entityId",
          inputEntity.Id.ToString()
        }
      };
      if (inputEntity.Attributes.ContainsKey("msdyn_sequencetarget"))
      {
        object obj;
        inputEntity.Attributes.TryGetValue("msdyn_sequencetarget", out obj);
        if (obj != null)
        {
          EntityReference entityReference = (EntityReference) obj;
          dictionary.Add("sequenceTargetId", entityReference.Id.ToString());
        }
        else
          dictionary.Add("sequenceTargetId", "");
      }
      if (!string.IsNullOrWhiteSpace(relatedEntity))
        dictionary.Add(nameof (relatedEntity), relatedEntity);
      return new SignalRNotificationRequest()
      {
        HubName = "salesacceleration",
        MessageContent = JsonConvert.SerializeObject((object) dictionary),
        UserId = service.UserId
      };
    }

    private string InvokeAzureFunctionAppService(
      string serviceName,
      string webApiName,
      string requestJson)
    {
      try
      {
        return new AzureFunctionHelper(this.serviceProvider).InvokeAzureFunctionApp(serviceName, webApiName, HttpMethod.Post, requestJson, this.userId);
      }
      catch (Exception ex)
      {
        this.tracingService.Trace("Exception occurred while fn invocation: Exception message : {0} and stackTrace : {1}", (object) ex.Message, (object) ex.StackTrace);
        throw new InvalidPluginExecutionException(ex.Message, ex);
      }
    }

    private void CheckForInputParametersForNullOrEmpty(
      string serviceName,
      string webApiName,
      string requestJson)
    {
      AzureFunctionHelper.ThrowIfNullOrEmpty(nameof (serviceName), serviceName);
      AzureFunctionHelper.ThrowIfNullOrEmpty(nameof (webApiName), webApiName);
      AzureFunctionHelper.ThrowIfNullOrEmpty(nameof (requestJson), requestJson);
    }
  }
}
