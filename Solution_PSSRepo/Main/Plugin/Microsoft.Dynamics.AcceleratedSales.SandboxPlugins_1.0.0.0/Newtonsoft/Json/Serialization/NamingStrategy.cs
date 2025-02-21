// Decompiled with JetBrains decompiler
// Type: Newtonsoft.Json.Serialization.NamingStrategy
// Assembly: Microsoft.Dynamics.AcceleratedSales.SandboxPlugins, Version=1.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35
// MVID: 7BBE7132-7E30-4424-B38D-27667CFBCEFA
// Assembly location: C:\Users\ngoct\Downloads\New folder\Microsoft.Dynamics.AcceleratedSales.SandboxPlugins_1.0.0.0.dll

#nullable enable
namespace Newtonsoft.Json.Serialization
{
  internal abstract class NamingStrategy
  {
    public bool ProcessDictionaryKeys { get; set; }

    public bool ProcessExtensionDataNames { get; set; }

    public bool OverrideSpecifiedNames { get; set; }

    public virtual string GetPropertyName(string name, bool hasSpecifiedName)
    {
      return hasSpecifiedName && !this.OverrideSpecifiedNames ? name : this.ResolvePropertyName(name);
    }

    public virtual string GetExtensionDataName(string name)
    {
      return !this.ProcessExtensionDataNames ? name : this.ResolvePropertyName(name);
    }

    public virtual string GetDictionaryKey(string key)
    {
      return !this.ProcessDictionaryKeys ? key : this.ResolvePropertyName(key);
    }

    protected abstract string ResolvePropertyName(string name);

    public override int GetHashCode()
    {
      return ((this.GetType().GetHashCode() * 397 ^ this.ProcessDictionaryKeys.GetHashCode()) * 397 ^ this.ProcessExtensionDataNames.GetHashCode()) * 397 ^ this.OverrideSpecifiedNames.GetHashCode();
    }

    public override bool Equals(object obj) => this.Equals(obj as NamingStrategy);

    protected bool Equals(NamingStrategy? other)
    {
      return other != null && this.GetType() == other.GetType() && this.ProcessDictionaryKeys == other.ProcessDictionaryKeys && this.ProcessExtensionDataNames == other.ProcessExtensionDataNames && this.OverrideSpecifiedNames == other.OverrideSpecifiedNames;
    }
  }
}
