// Decompiled with JetBrains decompiler
// Type: Microsoft.Dynamics.AcceleratedSales.SandboxPlugins.Core.Model.Filter.FilterAttributeData
// Assembly: Microsoft.Dynamics.AcceleratedSales.SandboxPlugins, Version=1.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35
// MVID: 7BBE7132-7E30-4424-B38D-27667CFBCEFA
// Assembly location: C:\Users\ngoct\Downloads\New folder\Microsoft.Dynamics.AcceleratedSales.SandboxPlugins_1.0.0.0.dll

#nullable disable
namespace Microsoft.Dynamics.AcceleratedSales.SandboxPlugins.Core.Model.Filter
{
  public class FilterAttributeData
  {
    public FilterAttributeData(object value, string label = null, string logicalName = null)
    {
      this.Value = value;
      this.Label = label;
      this.LogicalName = logicalName;
    }

    public object Value { get; set; }

    public string Label { get; set; }

    public string LogicalName { get; set; }

    public string Format { get; set; }
  }
}
