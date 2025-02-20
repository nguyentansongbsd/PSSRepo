// Decompiled with JetBrains decompiler
// Type: Microsoft.Dynamics.AcceleratedSales.SandboxPlugins.Core.UpdateWorklistSettingsDataCommand
// Assembly: Microsoft.Dynamics.AcceleratedSales.SandboxPlugins, Version=1.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35
// MVID: 7BBE7132-7E30-4424-B38D-27667CFBCEFA
// Assembly location: C:\Users\ngoct\Downloads\New folder\Microsoft.Dynamics.AcceleratedSales.SandboxPlugins_1.0.0.0.dll

using Microsoft.Dynamics.AcceleratedSales.SandboxPlugins.Core.Model;
using Microsoft.Dynamics.AcceleratedSales.SandboxPlugins.Core.Services;
using Microsoft.Dynamics.AcceleratedSales.SandboxPlugins.Core.Services.Extensions;
using Newtonsoft.Json;

#nullable disable
namespace Microsoft.Dynamics.AcceleratedSales.SandboxPlugins.Core
{
  public class UpdateWorklistSettingsDataCommand
  {
    private IDataStore dataStore;
    private IAcceleratedSalesLogger logger;
    private WorklistSettingsDataProviderService worklistSettingsDataProviderService;
    private DataExtension dataExtension;
    private UpdateWorklistSettingsDataRequest requestPayload;
    private IEntityMetadataProvider entityMetadataProvider;

    public UpdateWorklistSettingsDataCommand(
      IDataStore dataStore,
      IEntityMetadataProvider entityMetadataProvider,
      IAcceleratedSalesLogger logger,
      UpdateWorklistSettingsDataRequest requestPayload)
    {
      this.logger = logger;
      this.dataStore = dataStore;
      this.requestPayload = requestPayload;
      this.dataExtension = new DataExtension();
      this.entityMetadataProvider = entityMetadataProvider;
      this.worklistSettingsDataProviderService = new WorklistSettingsDataProviderService(dataStore, logger, entityMetadataProvider);
    }

    public UpdateWorklistSettingsDataResponse Execute()
    {
      string str = JsonConvert.SerializeObject((object) this.worklistSettingsDataProviderService.UpdateWorklistSettingsData(this.requestPayload), Formatting.None, new JsonSerializerSettings()
      {
        NullValueHandling = NullValueHandling.Ignore
      });
      return new UpdateWorklistSettingsDataResponse()
      {
        Payload = str
      };
    }
  }
}
