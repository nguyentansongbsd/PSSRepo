// Decompiled with JetBrains decompiler
// Type: Microsoft.Dynamics.AcceleratedSales.SandboxPlugins.Core.Model.CJOTreeNode
// Assembly: Microsoft.Dynamics.AcceleratedSales.SandboxPlugins, Version=1.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35
// MVID: 7BBE7132-7E30-4424-B38D-27667CFBCEFA
// Assembly location: C:\Users\ngoct\Downloads\New folder\Microsoft.Dynamics.AcceleratedSales.SandboxPlugins_1.0.0.0.dll

#nullable disable
namespace Microsoft.Dynamics.AcceleratedSales.SandboxPlugins.Core.Model
{
  public class CJOTreeNode
  {
    public CJOTreeNode.Parameters parameters { get; set; }

    public class Parameters
    {
      public string type { get; set; }

      public string subtype { get; set; }

      public string actionsubtype { get; set; }

      public CJOTreeNode.Inputs inputs { get; set; }
    }

    public class Inputs
    {
      public string name { get; set; }

      public string description { get; set; }

      public CJOTreeNode.Expression expression { get; set; }

      public string autoActionType { get; set; }

      public CJOTreeNode.AutoActionDetails autoActionDetails { get; set; }
    }

    public class Expression
    {
      public CJOTreeNode.Rule[] rules { get; set; }
    }

    public class Rule
    {
      public string dispositionOn { get; set; }
    }

    public class AutoActionDetails
    {
      public string attributeDisplayName { get; set; }

      public string optionSetDisplayName { get; set; }
    }
  }
}
