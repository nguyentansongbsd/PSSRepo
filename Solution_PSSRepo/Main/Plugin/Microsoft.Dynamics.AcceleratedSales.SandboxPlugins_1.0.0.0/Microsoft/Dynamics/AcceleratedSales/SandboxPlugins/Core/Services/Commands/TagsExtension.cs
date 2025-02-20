// Decompiled with JetBrains decompiler
// Type: Microsoft.Dynamics.AcceleratedSales.SandboxPlugins.Core.Services.Commands.TagsExtension
// Assembly: Microsoft.Dynamics.AcceleratedSales.SandboxPlugins, Version=1.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35
// MVID: 7BBE7132-7E30-4424-B38D-27667CFBCEFA
// Assembly location: C:\Users\ngoct\Downloads\New folder\Microsoft.Dynamics.AcceleratedSales.SandboxPlugins_1.0.0.0.dll

using Microsoft.Dynamics.AcceleratedSales.SandboxPlugins.Core.Model;
using Microsoft.Dynamics.AcceleratedSales.SandboxPlugins.Core.Model.Tags;
using Microsoft.Xrm.Sdk.Metadata;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;

#nullable disable
namespace Microsoft.Dynamics.AcceleratedSales.SandboxPlugins.Core.Services.Commands
{
  public static class TagsExtension
  {
    public static void AddAttributes(
      this TagsConfiguration tagsConfiguration,
      IEntityMetadataProvider metadataProvider,
      ref Dictionary<string, HashSet<string>> additionalAttributes)
    {
      int num;
      if (tagsConfiguration != null && tagsConfiguration.IsTagsEnabled)
      {
        List<Tag> tagsList = tagsConfiguration.TagsList;
        // ISSUE: explicit non-virtual call
        num = tagsList != null ? (__nonvirtual (tagsList.Count) > 0 ? 1 : 0) : 0;
      }
      else
        num = 0;
      if (num == 0)
        return;
      foreach (Tag tags in tagsConfiguration.TagsList)
      {
        DisplayConfiguration displayConfiguration1 = tags.DisplayConfiguration;
        if (displayConfiguration1 != null && displayConfiguration1.DisplayType == DisplayType.AttributeValue)
        {
          Microsoft.Dynamics.AcceleratedSales.SandboxPlugins.Core.Model.Tags.Attribute attribute = JsonConvert.DeserializeObject<Microsoft.Dynamics.AcceleratedSales.SandboxPlugins.Core.Model.Tags.Attribute>(tags.DisplayConfiguration.DisplayText);
          TagsExtension.AddEntityAttributePair(attribute.Entity, attribute.AttributeName, metadataProvider, ref additionalAttributes);
        }
        Visibility visibilityConfiguration = tags.VisibilityConfiguration;
        if (visibilityConfiguration != null && visibilityConfiguration.Type == VisibilityType.ShowBasedOnAttribute)
        {
          Microsoft.Dynamics.AcceleratedSales.SandboxPlugins.Core.Model.Tags.Attribute attribute = JsonConvert.DeserializeObject<Microsoft.Dynamics.AcceleratedSales.SandboxPlugins.Core.Model.Tags.Attribute>(tags.VisibilityConfiguration.Value);
          TagsExtension.AddEntityAttributePair(attribute.Entity, attribute.AttributeName, metadataProvider, ref additionalAttributes);
        }
        DisplayConfiguration displayConfiguration2 = tags.DisplayConfiguration;
        if (displayConfiguration2 != null && displayConfiguration2.TooltipType == TooltipType.AttributeValue)
        {
          Microsoft.Dynamics.AcceleratedSales.SandboxPlugins.Core.Model.Tags.Attribute attribute = JsonConvert.DeserializeObject<Microsoft.Dynamics.AcceleratedSales.SandboxPlugins.Core.Model.Tags.Attribute>(tags.DisplayConfiguration.TooltipText);
          TagsExtension.AddEntityAttributePair(attribute.Entity, attribute.AttributeName, metadataProvider, ref additionalAttributes);
        }
      }
    }

    private static void AddEntityAttributePair(
      string entity,
      string attribute,
      IEntityMetadataProvider metadataProvider,
      ref Dictionary<string, HashSet<string>> additionalAttributes)
    {
      if (!additionalAttributes.ContainsKey(entity))
        additionalAttributes.Add(entity, new HashSet<string>());
      if (metadataProvider.GetAttributes(entity) == null || !((IEnumerable<AttributeMetadata>) metadataProvider.GetAttributes(entity)).Any<AttributeMetadata>((Func<AttributeMetadata, bool>) (attr => attr.LogicalName == attribute)))
        return;
      additionalAttributes[entity].Add(attribute);
    }
  }
}
