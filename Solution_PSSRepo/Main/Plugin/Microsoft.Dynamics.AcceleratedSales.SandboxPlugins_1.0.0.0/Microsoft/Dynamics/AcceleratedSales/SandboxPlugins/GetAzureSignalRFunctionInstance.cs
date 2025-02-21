// Decompiled with JetBrains decompiler
// Type: Microsoft.Dynamics.AcceleratedSales.SandboxPlugins.GetAzureSignalRFunctionInstance
// Assembly: Microsoft.Dynamics.AcceleratedSales.SandboxPlugins, Version=1.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35
// MVID: 7BBE7132-7E30-4424-B38D-27667CFBCEFA
// Assembly location: C:\Users\ngoct\Downloads\New folder\Microsoft.Dynamics.AcceleratedSales.SandboxPlugins_1.0.0.0.dll

using Microsoft.Crm.AcceleratedSales.Shared.SandboxServices.Platform;
using Microsoft.Dynamics.AcceleratedSales.SandboxObjectModel.Helper;
using Microsoft.Xrm.Sdk;
using System;
using System.Globalization;

#nullable disable
namespace Microsoft.Dynamics.AcceleratedSales.SandboxPlugins
{
  public class GetAzureSignalRFunctionInstance : PluginBase
  {
    public GetAzureSignalRFunctionInstance()
      : base(typeof (GetAzureSignalRFunctionInstance))
    {
    }

    protected override void ExecuteCrmPlugin(LocalPluginContext localcontext)
    {
      IServiceProvider getServiceProvider = localcontext.GetServiceProvider;
      IPluginExecutionContext service = (IPluginExecutionContext) getServiceProvider.GetService(typeof (IPluginExecutionContext));
      localcontext.TraceOnPlugInTraceLog(string.Format((IFormatProvider) CultureInfo.InvariantCulture, "Retrieving signalR instance for serviceName {0}, webApiName {1}", service.InputParameters["ServiceName"], service.InputParameters["WebApiName"]));
      new InvokeAzureFunctionApp(getServiceProvider, service.UserId).GetSignalRInstance();
      localcontext.TraceOnPlugInTraceLog(string.Format((IFormatProvider) CultureInfo.InvariantCulture, "Retrieved signalR instance for serviceName {0}, webApiName {1}", service.InputParameters["ServiceName"], service.InputParameters["WebApiName"]));
    }
  }
}
