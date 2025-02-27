// Decompiled with JetBrains decompiler
// Type: Microsoft.Dynamics.AcceleratedSales.SandboxPlugins.Core.GetWorklistDataForSACommand
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
  public class GetWorklistDataForSACommand
  {
    private IDataStore dataStore;
    private IAcceleratedSalesLogger logger;
    private WorklistDataProviderService worklistDataProviderService;
    private DataExtension dataExtension;
    private GetWorklistDataRequest requestPayload;

    public GetWorklistDataForSACommand(
      IDataStore dataStore,
      IEntityMetadataProvider entityMetadataProvider,
      IAcceleratedSalesLogger logger,
      GetWorklistDataRequest requestPayload)
    {
      this.requestPayload = requestPayload;
      this.dataExtension = new DataExtension();
      WorklistViewConfiguration.FromRequestPayloadForSA(requestPayload, logger);
      Dictionary<string, IDataExtension> primaryExtensions = this.dataExtension.GetPrimaryExtensions();
      Dictionary<string, IDataExtension> relatedExtensions = this.dataExtension.GetRelatedExtensions();
      this.worklistDataProviderService = new WorklistDataProviderService(dataStore, logger, entityMetadataProvider, primaryExtensions, relatedExtensions);
      this.dataStore = dataStore;
      this.logger = logger;
    }

    public GetWorklistDataResponse Execute()
    {
      string str1 = "GetWorklistDataForSACommand.Execute started.";
      this.logger.LogInfo(str1, callerName: nameof (Execute), callerFile: "C:\\__w\\1\\s\\solutions\\AcceleratedSales\\AcceleratedSalesSandboxPlugins\\Core\\GetWorklistDataForSACommand.cs");
      this.logger.AddCustomProperty("GetWorklistDataForSACommand.Execute.Start", (object) str1);
      string str2 = JsonConvert.SerializeObject((object) this.worklistDataProviderService.GetWorklistDataForSA(this.requestPayload), Formatting.None, new JsonSerializerSettings()
      {
        NullValueHandling = NullValueHandling.Ignore
      });
      return new GetWorklistDataResponse()
      {
        Payload = str2
      };
    }
  }
}
