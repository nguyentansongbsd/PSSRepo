// Decompiled with JetBrains decompiler
// Type: Microsoft.Dynamics.AcceleratedSales.SandboxPlugins.Core.Model.SequenceStepRecord
// Assembly: Microsoft.Dynamics.AcceleratedSales.SandboxPlugins, Version=1.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35
// MVID: 7BBE7132-7E30-4424-B38D-27667CFBCEFA
// Assembly location: C:\Users\ngoct\Downloads\New folder\Microsoft.Dynamics.AcceleratedSales.SandboxPlugins_1.0.0.0.dll

using System;

#nullable disable
namespace Microsoft.Dynamics.AcceleratedSales.SandboxPlugins.Core.Model
{
  public class SequenceStepRecord
  {
    public Guid StepRecordId { get; set; }

    public Guid SalesCadenceStepId { get; set; }

    public int? WaitState { get; set; }

    public int? ErrorState { get; set; }

    public string StepName { get; set; }

    public int? StepType { get; set; }

    public int? StepSubType { get; set; }

    public Guid? LinkedActivityId { get; set; }

    public Guid? SalesCadenceId { get; set; }

    public string SalesCadenceName { get; set; }

    public Guid? AppliedCadenceId { get; set; }

    public string AppliedCadenceName { get; set; }

    public DateTime ExpiryDate { get; set; }

    public Guid RegardingRecordId { get; set; }

    public string CreatedByName { get; set; }

    public Guid? CreatedById { get; set; }

    public string OwnedByName { get; set; }

    public Guid? OwnedById { get; set; }

    public int SnoozeCount { get; set; }

    public string Description { get; set; }

    public DateTime ModifiedDate { get; set; }

    public int StateCode { get; set; }

    public int? StatusCode { get; set; }

    public DateTime? CompletedOn { get; set; }

    public bool IsOwnedByUserOrTeam { get; set; }
  }
}
