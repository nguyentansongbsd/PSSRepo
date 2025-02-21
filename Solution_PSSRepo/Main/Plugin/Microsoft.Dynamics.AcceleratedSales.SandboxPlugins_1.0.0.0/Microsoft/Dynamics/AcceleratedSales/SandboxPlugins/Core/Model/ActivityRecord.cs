// Decompiled with JetBrains decompiler
// Type: Microsoft.Dynamics.AcceleratedSales.SandboxPlugins.Core.Model.ActivityRecord
// Assembly: Microsoft.Dynamics.AcceleratedSales.SandboxPlugins, Version=1.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35
// MVID: 7BBE7132-7E30-4424-B38D-27667CFBCEFA
// Assembly location: C:\Users\ngoct\Downloads\New folder\Microsoft.Dynamics.AcceleratedSales.SandboxPlugins_1.0.0.0.dll

using System;

#nullable disable
namespace Microsoft.Dynamics.AcceleratedSales.SandboxPlugins.Core.Model
{
  public class ActivityRecord
  {
    public Guid ActivityId { get; set; }

    public string Subject { get; set; }

    public DateTime? ScheduledStart { get; set; }

    public DateTime? ScheduledEnd { get; set; }

    public int StateCode { get; set; }

    public string Description { get; set; }

    public string TypeCode { get; set; }

    public DateTime? CreatedOn { get; set; }

    public Guid RegardingRecordId { get; set; }

    public string CreatedByName { get; set; }

    public Guid? CreatedById { get; set; }

    public string OwnedByName { get; set; }

    public Guid? OwnedById { get; set; }

    public bool IsOwnedByUserOrTeam { get; set; }
  }
}
