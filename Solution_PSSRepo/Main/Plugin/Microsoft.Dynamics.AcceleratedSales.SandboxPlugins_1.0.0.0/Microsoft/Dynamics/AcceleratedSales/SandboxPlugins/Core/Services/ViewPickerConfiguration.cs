// Decompiled with JetBrains decompiler
// Type: Microsoft.Dynamics.AcceleratedSales.SandboxPlugins.Core.Services.ViewPickerConfiguration
// Assembly: Microsoft.Dynamics.AcceleratedSales.SandboxPlugins, Version=1.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35
// MVID: 7BBE7132-7E30-4424-B38D-27667CFBCEFA
// Assembly location: C:\Users\ngoct\Downloads\New folder\Microsoft.Dynamics.AcceleratedSales.SandboxPlugins_1.0.0.0.dll

using Microsoft.Dynamics.AcceleratedSales.SandboxPlugins.Core.DataAccess;
using Microsoft.Dynamics.AcceleratedSales.SandboxPlugins.Core.Model;
using Microsoft.Dynamics.AcceleratedSales.SandboxPlugins.Core.Model.ViewPicker;
using System.Collections.Generic;

#nullable disable
namespace Microsoft.Dynamics.AcceleratedSales.SandboxPlugins.Core.Services
{
  public class ViewPickerConfiguration
  {
    private readonly IDataStore dataStore;
    private readonly IAcceleratedSalesLogger logger;

    public ViewPickerConfiguration(IDataStore dataStore, IAcceleratedSalesLogger logger)
    {
      this.dataStore = dataStore;
      this.logger = logger;
    }

    public Dictionary<string, List<SavedView>> GetViewPickerConfiguration(List<string> entities)
    {
      Dictionary<string, List<SavedView>> pickerConfiguration = new Dictionary<string, List<SavedView>>();
      SavedQueryDataAccess savedQueryDataAccess = new SavedQueryDataAccess(this.dataStore, this.logger);
      UserQueryDataAccess userQueryDataAccess = new UserQueryDataAccess(this.dataStore, this.logger);
      foreach (string entity in entities)
      {
        pickerConfiguration[entity] = new List<SavedView>();
        pickerConfiguration[entity].AddRange((IEnumerable<SavedView>) savedQueryDataAccess.GetSystemViews(entity));
        pickerConfiguration[entity].AddRange((IEnumerable<SavedView>) userQueryDataAccess.GetUserViews(entity));
      }
      return pickerConfiguration;
    }
  }
}
