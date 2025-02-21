// Decompiled with JetBrains decompiler
// Type: Microsoft.Dynamics.AcceleratedSales.SandboxPlugins.Core.Model.Layout.LinkEntityLayoutExtensions
// Assembly: Microsoft.Dynamics.AcceleratedSales.SandboxPlugins, Version=1.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35
// MVID: 7BBE7132-7E30-4424-B38D-27667CFBCEFA
// Assembly location: C:\Users\ngoct\Downloads\New folder\Microsoft.Dynamics.AcceleratedSales.SandboxPlugins_1.0.0.0.dll

#nullable disable
namespace Microsoft.Dynamics.AcceleratedSales.SandboxPlugins.Core.Model.Layout
{
  public static class LinkEntityLayoutExtensions
  {
    private const string SalesEntityAliasPrefix = "msdyn_salesdisplayattribute_";

    public static string GetLinkEntityKey(string schemaName)
    {
      return "msdyn_salesdisplayattribute_" + schemaName;
    }

    public static bool TryGetDisplayKey(string schemaName, out string displayName)
    {
      displayName = string.Empty;
      if (string.IsNullOrEmpty(schemaName) || !schemaName.StartsWith("msdyn_salesdisplayattribute_") || schemaName == LinkEntityLayoutExtensions.GetLinkEntityKey("PriorityScore"))
        return false;
      displayName = schemaName.Substring("msdyn_salesdisplayattribute_".Length);
      return true;
    }
  }
}
