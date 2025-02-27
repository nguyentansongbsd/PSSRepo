// Decompiled with JetBrains decompiler
// Type: Microsoft.Dynamics.AcceleratedSales.SandboxPlugins.SequenceTargetIdForSampleData
// Assembly: Microsoft.Dynamics.AcceleratedSales.SandboxPlugins, Version=1.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35
// MVID: 7BBE7132-7E30-4424-B38D-27667CFBCEFA
// Assembly location: C:\Users\ngoct\Downloads\New folder\Microsoft.Dynamics.AcceleratedSales.SandboxPlugins_1.0.0.0.dll

using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.PluginTelemetry;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

#nullable disable
namespace Microsoft.Dynamics.AcceleratedSales.SandboxPlugins
{
  [ExcludeFromCodeCoverage]
  public static class SequenceTargetIdForSampleData
  {
    private const string SalesCadenceTargetEntity = "msdyn_sequencetarget";
    private static List<Guid> sampleDataSalesCadenceRecordIds = new List<Guid>()
    {
      Guid.Parse("2a69ec72-542f-eb11-a813-002248029f77"),
      Guid.Parse("faa0eebb-542f-eb11-a813-002248029f77"),
      Guid.Parse("ffa0eebb-542f-eb11-a813-002248029f77"),
      Guid.Parse("8502813a-562f-eb11-a813-002248029f77"),
      Guid.Parse("9002813a-562f-eb11-a813-002248029f77"),
      Guid.Parse("9502813a-562f-eb11-a813-002248029f77"),
      Guid.Parse("a002813a-562f-eb11-a813-002248029f77"),
      Guid.Parse("80467940-562f-eb11-a813-002248029f77"),
      Guid.Parse("93467940-562f-eb11-a813-002248029f77"),
      Guid.Parse("99467940-562f-eb11-a813-002248029f77"),
      Guid.Parse("a4467940-562f-eb11-a813-002248029f77"),
      Guid.Parse("c6467940-562f-eb11-a813-002248029f77"),
      Guid.Parse("cc467940-562f-eb11-a813-002248029f77"),
      Guid.Parse("a80c5ad4-8e34-eb11-a813-0022480a9ae2"),
      Guid.Parse("9e0c5ad4-8e34-eb11-a813-0022480a9ae2"),
      Guid.Parse("df6b3671-562f-eb11-a813-002248029f77"),
      Guid.Parse("e96b3671-562f-eb11-a813-002248029f77"),
      Guid.Parse("1480b5b9-562f-eb11-a813-002248029f77"),
      Guid.Parse("265fedcb-562f-eb11-a813-002248029f77"),
      Guid.Parse("fae8ddd7-562f-eb11-a813-002248029f77"),
      Guid.Parse("bdc3e2ef-562f-eb11-a813-002248029f77"),
      Guid.Parse("6013d8fb-562f-eb11-a813-002248029f77"),
      Guid.Parse("63dadb07-572f-eb11-a813-002248029f77"),
      Guid.Parse("54f7f319-572f-eb11-a813-002248029f77"),
      Guid.Parse("af1c2026-572f-eb11-a813-002248029f77"),
      Guid.Parse("7b11605c-572f-eb11-a813-002248029f77"),
      Guid.Parse("f1ac656e-572f-eb11-a813-002248029f77"),
      Guid.Parse("6d889680-572f-eb11-a813-002248029f77"),
      Guid.Parse("39bdae92-572f-eb11-a813-002248029f77"),
      Guid.Parse("8ddeda9e-572f-eb11-a813-002248029f77"),
      Guid.Parse("814e91c5-8f17-ee11-8f6e-000d3a9903e9"),
      Guid.Parse("1882e5d1-8f17-ee11-8f6e-000d3a9903e9"),
      Guid.Parse("72fe1ce0-8f17-ee11-8f6e-000d3a9903e9"),
      Guid.Parse("b6d7433b-9017-ee11-8f6e-000d3a9903e9"),
      Guid.Parse("7e3a1a43-9017-ee11-8f6e-000d3a9903e9"),
      Guid.Parse("fcaba34e-9017-ee11-8f6e-000d3a9903e9"),
      Guid.Parse("4f3c3a56-9017-ee11-8f6e-000d3a9903e9"),
      Guid.Parse("3fb1b166-9017-ee11-8f6e-000d3a9903e9"),
      Guid.Parse("df6b3671-562f-eb11-a813-002248029f77"),
      Guid.Parse("e96b3671-562f-eb11-a813-002248029f77"),
      Guid.Parse("63dadb07-572f-eb11-a813-002248029f77"),
      Guid.Parse("7b11605c-572f-eb11-a813-002248029f77"),
      Guid.Parse("f1ac656e-572f-eb11-a813-002248029f77"),
      Guid.Parse("6d889680-572f-eb11-a813-002248029f77"),
      Guid.Parse("8609a8ce-e5d0-ed11-a7c7-00224804994f"),
      Guid.Parse("8d09a8ce-e5d0-ed11-a7c7-00224804994f"),
      Guid.Parse("9209a8ce-e5d0-ed11-a7c7-00224804994f"),
      Guid.Parse("9709a8ce-e5d0-ed11-a7c7-00224804994f"),
      Guid.Parse("9c09a8ce-e5d0-ed11-a7c7-00224804994f"),
      Guid.Parse("ae3a47d5-e5d0-ed11-a7c7-00224804994f"),
      Guid.Parse("e8e2a629-13db-ed11-a7c7-00224804994f"),
      Guid.Parse("8298927c-13db-ed11-a7c7-00224804994f"),
      Guid.Parse("689eacd5-33e6-ed11-a7c7-00224804994f"),
      Guid.Parse("0632af15-35e6-ed11-a7c7-00224804994f"),
      Guid.Parse("84546c5a-19cf-ed11-b596-00224804994f"),
      Guid.Parse("3d0b31ae-19cf-ed11-b596-00224804994f"),
      Guid.Parse("c02ffb37-1acf-ed11-b596-00224804994f"),
      Guid.Parse("12f10998-1acf-ed11-b596-00224804994f"),
      Guid.Parse("7289ffc1-1acf-ed11-b596-00224804994f"),
      Guid.Parse("0cf9f3eb-1acf-ed11-b596-00224804994f"),
      Guid.Parse("40fcda15-1bcf-ed11-b596-00224804994f"),
      Guid.Parse("d3bae151-1bcf-ed11-b596-00224804994f"),
      Guid.Parse("036bd47b-1bcf-ed11-b596-00224804994f"),
      Guid.Parse("9de241a2-1bcf-ed11-b596-00224804994f"),
      Guid.Parse("ad225d1a-1ccf-ed11-b596-00224804994f"),
      Guid.Parse("5b755074-1ccf-ed11-b596-00224804994f"),
      Guid.Parse("28633d92-1ccf-ed11-b596-00224804994f"),
      Guid.Parse("b4d740d4-1ccf-ed11-b596-00224804994f"),
      Guid.Parse("9660365e-1dcf-ed11-b596-00224804994f"),
      Guid.Parse("271d3314-1fcf-ed11-b596-00224804994f"),
      Guid.Parse("6a95a1c7-24cf-ed11-b596-00224804994f"),
      Guid.Parse("e2909f93-25cf-ed11-b596-00224804994f"),
      Guid.Parse("5f4069f3-25cf-ed11-b596-00224804994f"),
      Guid.Parse("d052d37d-26cf-ed11-b596-00224804994f"),
      Guid.Parse("f452d37d-26cf-ed11-b596-00224804994f"),
      Guid.Parse("a8438553-36e6-ed11-a7c5-6045bd06d473"),
      Guid.Parse("88722b90-37e6-ed11-a7c5-6045bd06dfde"),
      Guid.Parse("b33ed03b-38e6-ed11-a7c5-6045bd06dfde"),
      Guid.Parse("61dd8876-3be6-ed11-a7c5-6045bd06dfde"),
      Guid.Parse("1a6a6934-3ce6-ed11-a7c5-6045bd06dfde"),
      Guid.Parse("d9296fbd-3de6-ed11-a7c5-6045bd06dfde"),
      Guid.Parse("bf843e00-3fe6-ed11-a7c5-6045bd06dfde"),
      Guid.Parse("9c2252ef-3fe6-ed11-a7c5-6045bd06dfde"),
      Guid.Parse("1c0d985d-41e6-ed11-a7c5-6045bd06dfde"),
      Guid.Parse("c8b65af4-94e7-ed11-a7c5-6045bd06dfde"),
      Guid.Parse("be86cde6-9ce7-ed11-a7c5-6045bd06dfde"),
      Guid.Parse("8188c7fd-9de7-ed11-a7c5-6045bd06dfde"),
      Guid.Parse("994ec339-9fe7-ed11-a7c5-6045bd06dfde"),
      Guid.Parse("9e2e5ab9-a0e7-ed11-a7c5-6045bd06dfde"),
      Guid.Parse("05b29a50-a1e7-ed11-a7c5-6045bd06dfde"),
      Guid.Parse("986385dc-a2e7-ed11-a7c5-6045bd06dfde"),
      Guid.Parse("068eef5d-d4e7-ed11-a7c5-6045bd06dfde"),
      Guid.Parse("b7248f05-d8e7-ed11-a7c5-6045bd06dfde"),
      Guid.Parse("930def59-d9e7-ed11-a7c5-6045bd06dfde"),
      Guid.Parse("b5966734-3be8-ed11-a7c5-6045bd06dfde"),
      Guid.Parse("72dc2bd5-3de8-ed11-a7c5-6045bd06dfde"),
      Guid.Parse("d9d34026-3ee8-ed11-a7c5-6045bd06dfde"),
      Guid.Parse("013a8fe6-3ee8-ed11-a7c5-6045bd06dfde"),
      Guid.Parse("c2b4117d-3fe8-ed11-a7c5-6045bd06dfde"),
      Guid.Parse("ebd2ff0f-4ae8-ed11-a7c5-6045bd06dfde"),
      Guid.Parse("e958fb7e-57e8-ed11-a7c5-6045bd06dfde"),
      Guid.Parse("40ed08db-21ea-ed11-a7c5-6045bd06dfde"),
      Guid.Parse("f04d6a28-83ea-ed11-a7c5-6045bd06dfde"),
      Guid.Parse("fa3b5be5-83ea-ed11-a7c5-6045bd06dfde"),
      Guid.Parse("dd991899-84ea-ed11-a7c5-6045bd06dfde"),
      Guid.Parse("d83d0e6f-87ea-ed11-a7c5-6045bd06dfde"),
      Guid.Parse("528a4065-99ea-ed11-a7c5-6045bd06dfde"),
      Guid.Parse("b9b96175-9bea-ed11-a7c5-6045bd06dfde"),
      Guid.Parse("e0f9b0b6-9cea-ed11-a7c5-6045bd06dfde"),
      Guid.Parse("e5d81441-9eea-ed11-a7c5-6045bd06dfde"),
      Guid.Parse("6bb08cf5-a4ea-ed11-a7c5-6045bd06dfde"),
      Guid.Parse("d73a2b3e-aaea-ed11-a7c5-6045bd06dfde"),
      Guid.Parse("464c9629-aeea-ed11-a7c5-6045bd06dfde"),
      Guid.Parse("22a1bae2-afea-ed11-a7c5-6045bd06dfde"),
      Guid.Parse("400e7f7f-b2ea-ed11-a7c5-6045bd06dfde"),
      Guid.Parse("f3e792cf-b2ea-ed11-a7c5-6045bd06dfde"),
      Guid.Parse("b705548c-b4ea-ed11-a7c5-6045bd06dfde"),
      Guid.Parse("4b32da04-b6ea-ed11-a7c5-6045bd06dfde"),
      Guid.Parse("7bb332a7-b6ea-ed11-a7c5-6045bd06dfde"),
      Guid.Parse("2e579540-b8ea-ed11-a7c5-6045bd06dfde"),
      Guid.Parse("f1bb95df-b8ea-ed11-a7c5-6045bd06dfde"),
      Guid.Parse("1f54bd79-b9ea-ed11-a7c5-6045bd06dfde"),
      Guid.Parse("4f49511c-baea-ed11-a7c5-6045bd06dfde"),
      Guid.Parse("8247b1ce-baea-ed11-a7c5-6045bd06dfde"),
      Guid.Parse("0d7c347a-e0ea-ed11-a7c5-6045bd06dfde"),
      Guid.Parse("69021616-e7ea-ed11-a7c5-6045bd06dfde"),
      Guid.Parse("fc885d69-e7ea-ed11-a7c5-6045bd06dfde"),
      Guid.Parse("e62fa183-e7ea-ed11-a7c5-6045bd06dfde"),
      Guid.Parse("eddcf2bb-e7ea-ed11-a7c5-6045bd06dfde"),
      Guid.Parse("42e535dd-e7ea-ed11-a7c5-6045bd06dfde"),
      Guid.Parse("f88e0df8-e7ea-ed11-a7c5-6045bd06dfde"),
      Guid.Parse("3940a221-e8ea-ed11-a7c5-6045bd06dfde"),
      Guid.Parse("b19a9107-e9ea-ed11-a7c5-6045bd06dfde"),
      Guid.Parse("3c68614a-e9ea-ed11-a7c5-6045bd06dfde"),
      Guid.Parse("ae710967-e9ea-ed11-a7c5-6045bd06dfde"),
      Guid.Parse("1b292739-eaea-ed11-a7c5-6045bd06dfde"),
      Guid.Parse("41628860-eaea-ed11-a7c5-6045bd06dfde"),
      Guid.Parse("69da349e-eaea-ed11-a7c5-6045bd06dfde"),
      Guid.Parse("ff06fbd2-eaea-ed11-a7c5-6045bd06dfde"),
      Guid.Parse("ff2460ed-eaea-ed11-a7c5-6045bd06dfde"),
      Guid.Parse("953bb128-ebea-ed11-a7c5-6045bd06dfde"),
      Guid.Parse("ffce0941-ebea-ed11-a7c5-6045bd06dfde"),
      Guid.Parse("5852129a-ebea-ed11-a7c5-6045bd06dfde"),
      Guid.Parse("dc774fa7-ebea-ed11-a7c5-6045bd06dfde"),
      Guid.Parse("7b7ecbbe-ebea-ed11-a7c5-6045bd06dfde"),
      Guid.Parse("52d3bee3-ebea-ed11-a7c5-6045bd06dfde"),
      Guid.Parse("9dfe5018-ecea-ed11-a7c5-6045bd06dfde"),
      Guid.Parse("2111ef38-ecea-ed11-a7c5-6045bd06dfde"),
      Guid.Parse("ae90cc68-ecea-ed11-a7c5-6045bd06dfde"),
      Guid.Parse("1fbaaa09-7ceb-ed11-a7c5-6045bd06dfde"),
      Guid.Parse("6240dedc-7feb-ed11-a7c5-6045bd06dfde")
    };

    public static bool IsSampleDataRecord(
      Entity inputEntity,
      ILogger logger,
      IPluginExecutionContext pluginExecutionContext)
    {
      logger.AddCustomProperty("inputEntityId", inputEntity.Id.ToString());
      logger.AddCustomProperty("orgnizationId", pluginExecutionContext.OrganizationId.ToString());
      if (inputEntity.Attributes.ContainsKey("msdyn_sequencetarget"))
      {
        EntityReference attribute = (EntityReference) inputEntity.Attributes["msdyn_sequencetarget"];
        logger.AddCustomProperty("entityReferenceId", attribute.Id.ToString());
        logger.AddCustomProperty("isSampleDataRecord", SequenceTargetIdForSampleData.sampleDataSalesCadenceRecordIds.Contains(attribute.Id).ToString());
        logger.LogWarning("Sample data check is completed", Array.Empty<object>());
        return SequenceTargetIdForSampleData.sampleDataSalesCadenceRecordIds.Contains(attribute.Id);
      }
      logger.AddCustomProperty("isSampleDataRecord", SequenceTargetIdForSampleData.sampleDataSalesCadenceRecordIds.Contains(inputEntity.Id).ToString());
      logger.LogWarning("Sample data check is completed", Array.Empty<object>());
      return SequenceTargetIdForSampleData.sampleDataSalesCadenceRecordIds.Contains(inputEntity.Id);
    }
  }
}
