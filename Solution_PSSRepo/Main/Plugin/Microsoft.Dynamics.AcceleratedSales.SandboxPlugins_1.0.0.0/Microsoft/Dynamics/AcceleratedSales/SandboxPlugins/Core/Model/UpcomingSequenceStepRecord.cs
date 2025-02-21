// Decompiled with JetBrains decompiler
// Type: Microsoft.Dynamics.AcceleratedSales.SandboxPlugins.Core.Model.UpcomingSequenceStepRecord
// Assembly: Microsoft.Dynamics.AcceleratedSales.SandboxPlugins, Version=1.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35
// MVID: 7BBE7132-7E30-4424-B38D-27667CFBCEFA
// Assembly location: C:\Users\ngoct\Downloads\New folder\Microsoft.Dynamics.AcceleratedSales.SandboxPlugins_1.0.0.0.dll

#nullable disable
namespace Microsoft.Dynamics.AcceleratedSales.SandboxPlugins.Core.Model
{
  public class UpcomingSequenceStepRecord
  {
    public string StepName { get; set; }

    public int? StepType { get; set; }

    public int? StepSubType { get; set; }

    public string Description { get; set; }

    public string DispositionOn { get; set; }

    public string AutoActionType { get; set; }

    public string UpdateFieldAttributeDisplayName { get; set; }

    public string UpdateFieldAttributeDisplayValue { get; set; }
  }
}
