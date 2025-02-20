// Decompiled with JetBrains decompiler
// Type: Microsoft.Dynamics.AcceleratedSales.SandboxPlugins.Core.Model.SuggestionsRecord
// Assembly: Microsoft.Dynamics.AcceleratedSales.SandboxPlugins, Version=1.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35
// MVID: 7BBE7132-7E30-4424-B38D-27667CFBCEFA
// Assembly location: C:\Users\ngoct\Downloads\New folder\Microsoft.Dynamics.AcceleratedSales.SandboxPlugins_1.0.0.0.dll

using System;

#nullable disable
namespace Microsoft.Dynamics.AcceleratedSales.SandboxPlugins.Core.Model
{
  public class SuggestionsRecord
  {
    public Guid SuggestionId { get; set; }

    public Decimal? PotentialRevenue { get; set; }

    public string PotentialRevenueFormatted { get; set; }

    public string SuggestionReason { get; set; }

    public string SuggestionsInsights { get; set; }

    public string SuggestionName { get; set; }

    public DateTime ExpiryDate { get; set; }

    public DateTime SuggestedDate { get; set; }

    public int StateCode { get; set; }

    public int StatusCode { get; set; }

    public Guid? OwnerId { get; set; }

    public string OwnerName { get; set; }

    public int? OwnershipMask { get; set; }
  }
}
