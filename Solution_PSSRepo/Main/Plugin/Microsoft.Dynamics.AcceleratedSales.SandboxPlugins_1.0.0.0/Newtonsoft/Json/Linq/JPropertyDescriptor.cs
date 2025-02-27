// Decompiled with JetBrains decompiler
// Type: Newtonsoft.Json.Linq.JPropertyDescriptor
// Assembly: Microsoft.Dynamics.AcceleratedSales.SandboxPlugins, Version=1.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35
// MVID: 7BBE7132-7E30-4424-B38D-27667CFBCEFA
// Assembly location: C:\Users\ngoct\Downloads\New folder\Microsoft.Dynamics.AcceleratedSales.SandboxPlugins_1.0.0.0.dll

using System;
using System.ComponentModel;

#nullable enable
namespace Newtonsoft.Json.Linq
{
  internal class JPropertyDescriptor(string name) : PropertyDescriptor(name, (Attribute[]) null)
  {
    private static JObject CastInstance(object instance) => (JObject) instance;

    public override bool CanResetValue(object component) => false;

    public override object? GetValue(object component)
    {
      // ISSUE: explicit non-virtual call
      return !(component is JObject jobject) ? (object) null : (object) __nonvirtual (jobject[this.Name]);
    }

    public override void ResetValue(object component)
    {
    }

    public override void SetValue(object component, object value)
    {
      if (!(component is JObject jobject))
        return;
      if (!(value is JToken jtoken1))
        jtoken1 = (JToken) new JValue(value);
      JToken jtoken2 = jtoken1;
      jobject[this.Name] = jtoken2;
    }

    public override bool ShouldSerializeValue(object component) => false;

    public override Type ComponentType => typeof (JObject);

    public override bool IsReadOnly => false;

    public override Type PropertyType => typeof (object);

    protected override int NameHashCode => base.NameHashCode;
  }
}
