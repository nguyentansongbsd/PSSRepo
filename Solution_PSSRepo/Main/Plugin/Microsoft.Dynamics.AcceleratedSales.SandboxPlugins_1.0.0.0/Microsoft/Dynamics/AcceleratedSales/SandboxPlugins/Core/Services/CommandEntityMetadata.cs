// Decompiled with JetBrains decompiler
// Type: Microsoft.Dynamics.AcceleratedSales.SandboxPlugins.Core.Services.CommandEntityMetadata
// Assembly: Microsoft.Dynamics.AcceleratedSales.SandboxPlugins, Version=1.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35
// MVID: 7BBE7132-7E30-4424-B38D-27667CFBCEFA
// Assembly location: C:\Users\ngoct\Downloads\New folder\Microsoft.Dynamics.AcceleratedSales.SandboxPlugins_1.0.0.0.dll

using Microsoft.Dynamics.AcceleratedSales.SandboxObjectModel.Model;
using Microsoft.Dynamics.AcceleratedSales.SandboxPlugins.Core.Model;
using Microsoft.Xrm.Sdk.Metadata;
using System;
using System.Collections.Generic;

#nullable disable
namespace Microsoft.Dynamics.AcceleratedSales.SandboxPlugins.Core.Services
{
  public class CommandEntityMetadata
  {
    private const string SuggestionEntityName = "msdyn_salessuggestion";
    private readonly IAcceleratedSalesLogger logger;
    private readonly IEntityMetadataProvider metadataProvider;

    public CommandEntityMetadata(
      IAcceleratedSalesLogger logger,
      IEntityMetadataProvider metadataProvider)
    {
      this.logger = logger ?? throw new ArgumentNullException(nameof (logger));
      this.metadataProvider = metadataProvider ?? throw new ArgumentNullException(nameof (metadataProvider));
    }

    public Dictionary<string, CommandMetadata> GetCommandEntityMetadata(
      Dictionary<string, EntityConfiguration> entityConfigurations,
      bool isSuggestionEnabled,
      bool isActiveTypeListEnabled)
    {
      Dictionary<string, CommandMetadata> metadataDictionary = new Dictionary<string, CommandMetadata>();
      foreach (KeyValuePair<string, EntityConfiguration> entityConfiguration in entityConfigurations)
        this.AddEntityMetadata(metadataDictionary, entityConfiguration.Key);
      if (isSuggestionEnabled)
        this.AddEntityMetadata(metadataDictionary, "msdyn_salessuggestion");
      if (isActiveTypeListEnabled)
      {
        string[] strArray = new string[4]
        {
          "task",
          "email",
          "phonecall",
          "appointment"
        };
        foreach (string key in strArray)
          this.AddEntityMetadata(metadataDictionary, key);
      }
      return metadataDictionary;
    }

    private void AddEntityMetadata(
      Dictionary<string, CommandMetadata> metadataDictionary,
      string key)
    {
      if (metadataDictionary.ContainsKey(key))
        return;
      EntityMetadata entityMetadata = this.metadataProvider.GetEntityMetadata(key);
      metadataDictionary[key] = new CommandMetadata()
      {
        DisplayName = entityMetadata?.DisplayName,
        DisplayCollectionName = entityMetadata?.DisplayCollectionName
      };
    }
  }
}
