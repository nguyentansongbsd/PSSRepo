// Decompiled with JetBrains decompiler
// Type: Microsoft.Crm.AcceleratedSales.Shared.SandboxServices.Platform.PluginBase
// Assembly: Microsoft.Dynamics.AcceleratedSales.SandboxPlugins, Version=1.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35
// MVID: 7BBE7132-7E30-4424-B38D-27667CFBCEFA
// Assembly location: C:\Users\ngoct\Downloads\New folder\Microsoft.Dynamics.AcceleratedSales.SandboxPlugins_1.0.0.0.dll

using Microsoft.Xrm.Sdk;
using System;
using System.Globalization;
using System.ServiceModel;

#nullable disable
namespace Microsoft.Crm.AcceleratedSales.Shared.SandboxServices.Platform
{
  internal abstract class PluginBase : IPlugin
  {
    public PluginBase(Type childClassName) => this.ChildClassName = childClassName.ToString();

    protected string ChildClassName { get; private set; }

    public void Execute(IServiceProvider serviceProvider)
    {
      LocalPluginContext localPluginContext = serviceProvider != null ? new LocalPluginContext(serviceProvider) : throw new InvalidPluginExecutionException(nameof (serviceProvider));
      localPluginContext.Trace(string.Format((IFormatProvider) CultureInfo.InvariantCulture, "Entered {0}.Execute()", (object) this.ChildClassName));
      Guid guid1 = localPluginContext.PluginExecutionContext == null ? Guid.NewGuid() : localPluginContext.PluginExecutionContext.CorrelationId;
      Guid guid2 = localPluginContext.PluginExecutionContext == null ? Guid.Empty : localPluginContext.PluginExecutionContext.OrganizationId;
      Guid guid3 = localPluginContext.PluginExecutionContext == null ? Guid.Empty : localPluginContext.PluginExecutionContext.InitiatingUserId;
      this.InitializeExecutionContext(localPluginContext);
      try
      {
        if (localPluginContext.IsSkipPluginSet())
          localPluginContext.Trace(string.Format((IFormatProvider) CultureInfo.InvariantCulture, "Skip plugin execution flag is set. Skipping {0}.Execute()", (object) this.ChildClassName));
        else
          this.ExecuteCrmPlugin(localPluginContext);
      }
      catch (FaultException<OrganizationServiceFault> ex)
      {
        localPluginContext.Trace(string.Format((IFormatProvider) CultureInfo.InvariantCulture, "Exception: {0}", (object) ex.ToString()));
        throw;
      }
      catch (Exception ex)
      {
        localPluginContext.Trace(string.Format((IFormatProvider) CultureInfo.InvariantCulture, "Exception: {0}", (object) ex.ToString()));
        throw;
      }
      finally
      {
        this.FinalizeExecutionContext();
        localPluginContext.Trace(string.Format((IFormatProvider) CultureInfo.InvariantCulture, "Exiting {0}.Execute()", (object) this.ChildClassName));
      }
    }

    protected virtual void InitializeExecutionContext(LocalPluginContext localContext)
    {
    }

    protected virtual void FinalizeExecutionContext()
    {
    }

    protected virtual void ExecuteCrmPlugin(LocalPluginContext localcontext)
    {
    }
  }
}
