// Decompiled with JetBrains decompiler
// Type: Microsoft.Dynamics.AcceleratedSales.SandboxPlugins.Core.Services.Extensions.DataExtension
// Assembly: Microsoft.Dynamics.AcceleratedSales.SandboxPlugins, Version=1.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35
// MVID: 7BBE7132-7E30-4424-B38D-27667CFBCEFA
// Assembly location: C:\Users\ngoct\Downloads\New folder\Microsoft.Dynamics.AcceleratedSales.SandboxPlugins_1.0.0.0.dll

using System;
using System.Collections.Generic;
using System.Linq;

#nullable disable
namespace Microsoft.Dynamics.AcceleratedSales.SandboxPlugins.Core.Services.Extensions
{
  public class DataExtension
  {
    private List<KeyValuePair<string, IDataExtension>> primaryExtensions;

    public DataExtension()
    {
      this.primaryExtensions = new List<KeyValuePair<string, IDataExtension>>()
      {
        LeadExtension.Create(),
        OpportunityExtension.Create(),
        AccountExtension.Create(),
        ContactExtension.Create()
      };
    }

    public static bool CheckForColaEntities(string entityName)
    {
      return ((IEnumerable<string>) new string[4]
      {
        "contact",
        "account",
        "lead",
        "opportunity"
      }).Contains<string>(entityName);
    }

    public void AddCustomExtensions(string entityName, string primaryAttributeName)
    {
      if (this.primaryExtensions.Any<KeyValuePair<string, IDataExtension>>((Func<KeyValuePair<string, IDataExtension>, bool>) (entity => entity.Key == entityName)))
        return;
      this.primaryExtensions.Add(CustomEntityExtension.Create(entityName, primaryAttributeName));
    }

    public Dictionary<string, IDataExtension> GetPrimaryExtensions()
    {
      return this.primaryExtensions.ToDictionary<KeyValuePair<string, IDataExtension>, string, IDataExtension>((Func<KeyValuePair<string, IDataExtension>, string>) (kv => kv.Key), (Func<KeyValuePair<string, IDataExtension>, IDataExtension>) (kv => kv.Value));
    }

    public Dictionary<string, IDataExtension> GetRelatedExtensions()
    {
      return ((IEnumerable<KeyValuePair<string, IDataExtension>>) new KeyValuePair<string, IDataExtension>[4]
      {
        ActivityExtension.Create(),
        SequenceExtension.Create(),
        WorkQueueStateExtension.Create(),
        SuggestionExtension.Create()
      }).ToDictionary<KeyValuePair<string, IDataExtension>, string, IDataExtension>((Func<KeyValuePair<string, IDataExtension>, string>) (kv => kv.Key), (Func<KeyValuePair<string, IDataExtension>, IDataExtension>) (kv => kv.Value));
    }
  }
}
