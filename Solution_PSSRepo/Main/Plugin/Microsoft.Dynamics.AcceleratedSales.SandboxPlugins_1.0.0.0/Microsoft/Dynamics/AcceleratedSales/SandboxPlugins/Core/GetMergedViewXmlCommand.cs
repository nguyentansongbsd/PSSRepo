// Decompiled with JetBrains decompiler
// Type: Microsoft.Dynamics.AcceleratedSales.SandboxPlugins.Core.GetMergedViewXmlCommand
// Assembly: Microsoft.Dynamics.AcceleratedSales.SandboxPlugins, Version=1.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35
// MVID: 7BBE7132-7E30-4424-B38D-27667CFBCEFA
// Assembly location: C:\Users\ngoct\Downloads\New folder\Microsoft.Dynamics.AcceleratedSales.SandboxPlugins_1.0.0.0.dll

using Microsoft.Dynamics.AcceleratedSales.SandboxPlugins.Core.Model;
using Microsoft.Dynamics.AcceleratedSales.SandboxPlugins.Core.Services;
using Microsoft.Dynamics.AcceleratedSales.SandboxPlugins.Core.Services.Extensions;
using Newtonsoft.Json;
using System.Collections.Generic;

#nullable disable
namespace Microsoft.Dynamics.AcceleratedSales.SandboxPlugins.Core
{
  public class GetMergedViewXmlCommand
  {
    private IDataStore dataStore;
    private IAcceleratedSalesLogger logger;
    private WorklistDataProviderService worklistDataProviderService;
    private DataExtension dataExtension;
    private GetWorklistFilteredDataRequest requestPayload;

    public GetMergedViewXmlCommand(
      IDataStore dataStore,
      IEntityMetadataProvider entityMetadataProvider,
      IAcceleratedSalesLogger logger,
      GetWorklistFilteredDataRequest requestPayload)
    {
      this.requestPayload = requestPayload;
      this.dataExtension = new DataExtension();
      WorklistViewConfiguration viewConfiguration = WorklistViewConfiguration.FromFilteredDataRequestPayload(requestPayload, logger);
      string primaryNameAttribute = entityMetadataProvider.GetEntityMetadata(viewConfiguration?.EntityName)?.PrimaryNameAttribute;
      if (primaryNameAttribute != null && !DataExtension.CheckForColaEntities(viewConfiguration?.EntityName))
        this.dataExtension.AddCustomExtensions(viewConfiguration?.EntityName, primaryNameAttribute);
      Dictionary<string, IDataExtension> primaryExtensions = this.dataExtension.GetPrimaryExtensions();
      this.worklistDataProviderService = new WorklistDataProviderService(dataStore, logger, entityMetadataProvider, primaryExtensions, (Dictionary<string, IDataExtension>) null);
      this.dataStore = dataStore;
      this.logger = logger;
    }

    public GetWorklistFilteredDataResponse Execute()
    {
      this.logger.AddCustomProperty("GetMergedViewXmlCommand Execute method starting.", (object) "true");
      string str = JsonConvert.SerializeObject((object) this.worklistDataProviderService.GetMergedViewXml(this.requestPayload), Formatting.None, new JsonSerializerSettings()
      {
        NullValueHandling = NullValueHandling.Ignore
      });
      return new GetWorklistFilteredDataResponse()
      {
        Payload = str
      };
    }
  }
}
