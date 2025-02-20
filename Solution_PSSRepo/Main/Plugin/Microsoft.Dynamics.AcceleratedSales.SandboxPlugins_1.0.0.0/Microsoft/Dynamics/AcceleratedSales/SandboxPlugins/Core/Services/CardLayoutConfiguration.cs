// Decompiled with JetBrains decompiler
// Type: Microsoft.Dynamics.AcceleratedSales.SandboxPlugins.Core.Services.CardLayoutConfiguration
// Assembly: Microsoft.Dynamics.AcceleratedSales.SandboxPlugins, Version=1.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35
// MVID: 7BBE7132-7E30-4424-B38D-27667CFBCEFA
// Assembly location: C:\Users\ngoct\Downloads\New folder\Microsoft.Dynamics.AcceleratedSales.SandboxPlugins_1.0.0.0.dll

using Microsoft.Dynamics.AcceleratedSales.SandboxPlugins.Core.Model;
using Microsoft.Dynamics.AcceleratedSales.SandboxPlugins.Core.Model.Layout;
using Microsoft.Dynamics.AcceleratedSales.SandboxPlugins.Core.Services.Extensions;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

#nullable disable
namespace Microsoft.Dynamics.AcceleratedSales.SandboxPlugins.Core.Services
{
  public class CardLayoutConfiguration
  {
    private const int MaxColumns = 4;
    private readonly IDataStore dataStore;
    private readonly IAcceleratedSalesLogger logger;
    private readonly Dictionary<string, IDataExtension> primaryExtensions;

    public CardLayoutConfiguration(
      IDataStore dataStore,
      IAcceleratedSalesLogger logger,
      Dictionary<string, IDataExtension> primaryExtensions)
    {
      this.dataStore = dataStore;
      this.logger = logger;
      this.primaryExtensions = primaryExtensions;
    }

    public Dictionary<string, DesignerConfiguration> GetCardLayout(
      WorklistAdminConfiguration adminConfig,
      WorklistSellerConfiguration sellerConfig,
      bool optimisationEnabled = false,
      string entityName = "",
      List<ColumnsData> columns = null)
    {
      Stopwatch stopwatch = Stopwatch.StartNew();
      Dictionary<string, DesignerConfiguration> defaultCardLayout = this.GetDefaultCardLayout();
      if (optimisationEnabled && !string.IsNullOrEmpty(entityName))
      {
        foreach (DesignerRow row in defaultCardLayout[entityName].Layout.Rows)
          row.Fields?.RemoveAll((Predicate<DesignerField>) (field => field.Key == "FollowIndicator"));
      }
      // ISSUE: explicit non-virtual call
      if (this.dataStore.IsFCSEnabled("SalesService.Workspace", "IsNewDefaultLayoutForWorklistCardEnabled") && columns != null && __nonvirtual (columns.Count) > 0)
        this.UpdateDefaultLayoutWithColumnsData(defaultCardLayout, columns);
      AdminDesignerConfiguration cardLayout1 = adminConfig?.CardLayout;
      if (cardLayout1 == null && sellerConfig?.CardLayout == null)
      {
        stopwatch.Stop();
        this.logger.LogWarning(string.Format("CardLayoutConfiguration.GetCardLayout.DefaultLayout.Duration: {0}", (object) stopwatch.ElapsedMilliseconds), callerName: nameof (GetCardLayout), callerFile: "C:\\__w\\1\\s\\solutions\\AcceleratedSales\\AcceleratedSalesSandboxPlugins\\Core\\Services\\Layout\\CardLayoutConfiguration.cs");
        return defaultCardLayout;
      }
      if (cardLayout1 != null && cardLayout1.IsLocked.GetValueOrDefault(true))
      {
        Dictionary<string, DesignerConfiguration> cardLayout2 = this.MergeLayoutConfigurations(cardLayout1.Configuration ?? new Dictionary<string, DesignerConfiguration>(), new Dictionary<string, DesignerConfiguration>(), defaultCardLayout);
        stopwatch.Stop();
        this.logger.LogWarning(string.Format("CardLayoutConfiguration.GetCardLayout.MergedAdminDefaultlayout.Duration: {0}", (object) stopwatch.ElapsedMilliseconds), callerName: nameof (GetCardLayout), callerFile: "C:\\__w\\1\\s\\solutions\\AcceleratedSales\\AcceleratedSalesSandboxPlugins\\Core\\Services\\Layout\\CardLayoutConfiguration.cs");
        return cardLayout2;
      }
      Dictionary<string, DesignerConfiguration> sellerConfig1 = sellerConfig?.CardLayout ?? new Dictionary<string, DesignerConfiguration>();
      Dictionary<string, DesignerConfiguration> cardLayout3 = this.MergeLayoutConfigurations(cardLayout1?.Configuration ?? new Dictionary<string, DesignerConfiguration>(), sellerConfig1, defaultCardLayout);
      stopwatch.Stop();
      this.logger.LogWarning(string.Format("CardLayoutConfiguration.GetCardLayout.MergedAdminSellerDefaultLayout.Duration: {0}", (object) stopwatch.ElapsedMilliseconds), callerName: nameof (GetCardLayout), callerFile: "C:\\__w\\1\\s\\solutions\\AcceleratedSales\\AcceleratedSalesSandboxPlugins\\Core\\Services\\Layout\\CardLayoutConfiguration.cs");
      return cardLayout3;
    }

    private void UpdateDefaultLayoutWithColumnsData(
      Dictionary<string, DesignerConfiguration> defaultLayout,
      List<ColumnsData> columns = null)
    {
      foreach (KeyValuePair<string, DesignerConfiguration> keyValuePair in defaultLayout)
      {
        string key = keyValuePair.Key;
        DesignerConfiguration designerConfiguration = keyValuePair.Value;
        if (designerConfiguration != null && designerConfiguration.Layout != null && designerConfiguration.Layout.Rows != null)
        {
          designerConfiguration.Layout.Rows.Clear();
          for (int index = 0; index < columns.Count && index < 4; ++index)
          {
            DesignerField designerField = new DesignerField()
            {
              Id = "Slot1",
              Type = DesignerFieldType.SimpleDataField,
              Position = FieldPosition.Start,
              DisplayName = columns[index].DisplayName,
              Key = columns[index].Name
            };
            designerConfiguration.Layout.Rows.Add(new DesignerRow()
            {
              Id = string.Format("Row{0}", (object) (index + 1)),
              Type = DesignerRowType.SimpleRow,
              Fields = new List<DesignerField>()
              {
                designerField
              }
            });
          }
          designerConfiguration.Layout.Rows.Add(new DesignerRow()
          {
            Id = string.Format("Row{0}", (object) columns.Count),
            Type = DesignerRowType.CustomRow,
            Key = "UpNextActivity",
            IsLocked = false
          });
        }
      }
    }

    private Dictionary<string, DesignerConfiguration> GetDefaultCardLayout()
    {
      Dictionary<string, IDataExtension> primaryExtensions = this.primaryExtensions;
      // ISSUE: explicit non-virtual call
      return primaryExtensions != null && __nonvirtual (primaryExtensions.Count) > 0 ? this.primaryExtensions.ToDictionary<KeyValuePair<string, IDataExtension>, string, DesignerConfiguration>((Func<KeyValuePair<string, IDataExtension>, string>) (e => e.Key), (Func<KeyValuePair<string, IDataExtension>, DesignerConfiguration>) (e => e.Value.DesignerConfiguration.Value)) : new DataExtension().GetPrimaryExtensions().ToDictionary<KeyValuePair<string, IDataExtension>, string, DesignerConfiguration>((Func<KeyValuePair<string, IDataExtension>, string>) (e => e.Key), (Func<KeyValuePair<string, IDataExtension>, DesignerConfiguration>) (e => e.Value.DesignerConfiguration.Value));
    }

    private Dictionary<string, DesignerConfiguration> MergeLayoutConfigurations(
      Dictionary<string, DesignerConfiguration> adminConfig,
      Dictionary<string, DesignerConfiguration> sellerConfig,
      Dictionary<string, DesignerConfiguration> defaultConfig)
    {
      Dictionary<string, DesignerConfiguration> dictionary = new Dictionary<string, DesignerConfiguration>();
      foreach (KeyValuePair<string, DesignerConfiguration> keyValuePair in adminConfig)
      {
        DesignerConfiguration designerConfiguration;
        dictionary[keyValuePair.Key] = !sellerConfig.TryGetValue(keyValuePair.Key, out designerConfiguration) ? keyValuePair.Value : designerConfiguration;
      }
      foreach (KeyValuePair<string, DesignerConfiguration> keyValuePair in sellerConfig)
        dictionary[keyValuePair.Key] = keyValuePair.Value;
      if (defaultConfig != null)
      {
        foreach (KeyValuePair<string, DesignerConfiguration> keyValuePair in defaultConfig)
        {
          if (!dictionary.ContainsKey(keyValuePair.Key))
            dictionary[keyValuePair.Key] = keyValuePair.Value;
        }
      }
      return dictionary;
    }
  }
}
