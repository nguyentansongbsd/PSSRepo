// Decompiled with JetBrains decompiler
// Type: Microsoft.Dynamics.AcceleratedSales.SandboxObjectModel.DebugClasses.EmptyFeatureControlService
// Assembly: Microsoft.Dynamics.AcceleratedSales.SandboxPlugins, Version=1.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35
// MVID: 7BBE7132-7E30-4424-B38D-27667CFBCEFA
// Assembly location: C:\Users\ngoct\Downloads\New folder\Microsoft.Dynamics.AcceleratedSales.SandboxPlugins_1.0.0.0.dll

using Microsoft.Xrm.Sdk;
using System;

#nullable disable
namespace Microsoft.Dynamics.AcceleratedSales.SandboxObjectModel.DebugClasses
{
  internal class EmptyFeatureControlService : IFeatureControlService, IDisposable
  {
    private bool disposedValue = false;

    public object GetFeatureControl(
      string namespaceValue,
      string featureControlName,
      out Type type)
    {
      type = typeof (bool);
      return (object) true;
    }

    protected virtual void Dispose(bool disposing)
    {
      if (this.disposedValue)
        return;
      if (!disposing)
        ;
      this.disposedValue = true;
    }

    public void Dispose() => this.Dispose(true);
  }
}
