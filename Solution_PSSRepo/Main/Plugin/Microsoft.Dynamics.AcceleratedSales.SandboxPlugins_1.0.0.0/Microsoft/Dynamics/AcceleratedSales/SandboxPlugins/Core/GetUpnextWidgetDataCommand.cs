// Decompiled with JetBrains decompiler
// Type: Microsoft.Dynamics.AcceleratedSales.SandboxPlugins.Core.GetUpnextWidgetDataCommand
// Assembly: Microsoft.Dynamics.AcceleratedSales.SandboxPlugins, Version=1.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35
// MVID: 7BBE7132-7E30-4424-B38D-27667CFBCEFA
// Assembly location: C:\Users\ngoct\Downloads\New folder\Microsoft.Dynamics.AcceleratedSales.SandboxPlugins_1.0.0.0.dll

using Microsoft.Dynamics.AcceleratedSales.SandboxPlugins.Core.Model;
using Microsoft.Dynamics.AcceleratedSales.SandboxPlugins.Core.Services;
using System;

#nullable disable
namespace Microsoft.Dynamics.AcceleratedSales.SandboxPlugins.Core
{
  public class GetUpnextWidgetDataCommand
  {
    private IDataStore dataStore;
    private IAcceleratedSalesLogger logger;
    private UpnextDataService upnextDataService;

    public GetUpnextWidgetDataCommand(IDataStore dataStore, IAcceleratedSalesLogger logger)
    {
      this.dataStore = dataStore;
      this.logger = logger;
      this.upnextDataService = new UpnextDataService(logger, dataStore);
    }

    public UpnextWidgetDataResponse Execute(
      GetUpnextDataRequestParameters getUpnextDataRequestParams)
    {
      return this.AreParamsValid(getUpnextDataRequestParams) ? this.upnextDataService.GetUpnextWidgetData(getUpnextDataRequestParams) : throw new Exception("Invalid Parameters");
    }

    private bool AreParamsValid(
      GetUpnextDataRequestParameters getUpnextDataRequestParams)
    {
      return getUpnextDataRequestParams != null && getUpnextDataRequestParams.EntityLogicalName != null;
    }
  }
}
