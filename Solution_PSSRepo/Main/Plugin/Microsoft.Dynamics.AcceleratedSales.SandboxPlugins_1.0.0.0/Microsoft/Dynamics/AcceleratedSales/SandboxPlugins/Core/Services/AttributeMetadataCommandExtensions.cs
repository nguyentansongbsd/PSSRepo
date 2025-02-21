// Decompiled with JetBrains decompiler
// Type: Microsoft.Dynamics.AcceleratedSales.SandboxPlugins.Core.Services.AttributeMetadataCommandExtensions
// Assembly: Microsoft.Dynamics.AcceleratedSales.SandboxPlugins, Version=1.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35
// MVID: 7BBE7132-7E30-4424-B38D-27667CFBCEFA
// Assembly location: C:\Users\ngoct\Downloads\New folder\Microsoft.Dynamics.AcceleratedSales.SandboxPlugins_1.0.0.0.dll

using Microsoft.Dynamics.AcceleratedSales.SandboxPlugins.Core.Model.Filter;
using Microsoft.Xrm.Sdk.Metadata;
using System;
using System.Collections.Generic;
using System.Linq;

#nullable disable
namespace Microsoft.Dynamics.AcceleratedSales.SandboxPlugins.Core.Services
{
  public static class AttributeMetadataCommandExtensions
  {
    private const string MultiSelectPicklistType = "MultiSelectPicklistType";

    public static ControlType GetFilterControlType(this AttributeMetadata metadata)
    {
      AttributeTypeCode? attributeType = metadata.AttributeType;
      if (!attributeType.HasValue)
        return ControlType.Text;
      AttributeTypeCode? nullable = attributeType;
      if (nullable.HasValue)
      {
        switch (nullable.GetValueOrDefault())
        {
          case AttributeTypeCode.Boolean:
            return ControlType.SingleSelect;
          case AttributeTypeCode.Customer:
          case AttributeTypeCode.Lookup:
          case AttributeTypeCode.Owner:
            return ControlType.Lookup;
          case AttributeTypeCode.DateTime:
            return ControlType.Date;
          case AttributeTypeCode.Decimal:
          case AttributeTypeCode.Money:
            return ControlType.Decimal;
          case AttributeTypeCode.Double:
          case AttributeTypeCode.Integer:
          case AttributeTypeCode.BigInt:
            return ControlType.Number;
          case AttributeTypeCode.Memo:
          case AttributeTypeCode.String:
            return ControlType.Text;
          case AttributeTypeCode.Picklist:
          case AttributeTypeCode.State:
          case AttributeTypeCode.Status:
          case AttributeTypeCode.EntityName:
            return ControlType.Multiselect;
          case AttributeTypeCode.Virtual:
            return metadata.AttributeTypeName == (AttributeTypeDisplayName) "MultiSelectPicklistType" ? ControlType.Multiselect : ControlType.Text;
        }
      }
      return ControlType.Text;
    }

    public static Dictionary<string, string> ToFilterMetadataOptions(
      this AttributeMetadata metadata,
      int localeId)
    {
      if (metadata.GetFilterControlType() != ControlType.Multiselect && metadata.GetFilterControlType() != ControlType.SingleSelect)
        return (Dictionary<string, string>) null;
      if (metadata.GetFilterControlType() == ControlType.SingleSelect)
      {
        Dictionary<string, string> filterMetadataOptions = new Dictionary<string, string>();
        if (!(metadata is BooleanAttributeMetadata attributeMetadata))
          return (Dictionary<string, string>) null;
        filterMetadataOptions.Add("true", attributeMetadata.OptionSet.TrueOption.Label.GetLocalizedLabel(localeId));
        filterMetadataOptions.Add("false", attributeMetadata.OptionSet.FalseOption.Label.GetLocalizedLabel(localeId));
        return filterMetadataOptions;
      }
      return !(metadata is EnumAttributeMetadata attributeMetadata1) ? (Dictionary<string, string>) null : attributeMetadata1.OptionSet.Options.ToDictionary<OptionMetadata, string, string>((Func<OptionMetadata, string>) (kv => kv.Value.GetValueOrDefault(0).ToString()), (Func<OptionMetadata, string>) (kv => kv.Label.GetLocalizedLabel(localeId) ?? kv.Value.GetValueOrDefault(0).ToString()));
    }
  }
}
