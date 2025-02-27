// Decompiled with JetBrains decompiler
// Type: Microsoft.Crm.AcceleratedSales.Shared.SandboxServices.Platform.LocalPluginContext
// Assembly: Microsoft.Dynamics.AcceleratedSales.SandboxPlugins, Version=1.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35
// MVID: 7BBE7132-7E30-4424-B38D-27667CFBCEFA
// Assembly location: C:\Users\ngoct\Downloads\New folder\Microsoft.Dynamics.AcceleratedSales.SandboxPlugins_1.0.0.0.dll

using Microsoft.Xrm.Sdk;
using System;

#nullable disable
namespace Microsoft.Crm.AcceleratedSales.Shared.SandboxServices.Platform
{
  internal class LocalPluginContext
  {
    private const string InputParameterTarget = "Target";
    private IOrganizationService systemUserOrganizationService;
    private IOrganizationService systemService = (IOrganizationService) null;
    private IOrganizationServiceFactory organizationServiceFactory;

    public LocalPluginContext()
    {
    }

    public LocalPluginContext(IServiceProvider serviceProvider)
    {
      this.PluginExecutionContext = (IPluginExecutionContext) serviceProvider.GetService(typeof (IPluginExecutionContext));
      this.TracingService = (ITracingService) serviceProvider.GetService(typeof (ITracingService));
      this.NotificationService = (IServiceEndpointNotificationService) serviceProvider.GetService(typeof (IServiceEndpointNotificationService));
      this.organizationServiceFactory = (IOrganizationServiceFactory) serviceProvider.GetService(typeof (IOrganizationServiceFactory));
      this.OrganizationService = this.organizationServiceFactory.CreateOrganizationService(new Guid?(this.PluginExecutionContext.UserId));
      this.ServiceProvider = serviceProvider;
      this.FeatureControlService = (IFeatureControlService) this.ServiceProvider.GetService(typeof (IFeatureControlService));
    }

    public IServiceProvider ServiceProvider { get; private set; }

    public virtual IOrganizationService OrganizationService { get; private set; }

    public IOrganizationService SystemService
    {
      get
      {
        if (this.systemService == null)
          this.systemService = this.organizationServiceFactory.CreateOrganizationService(new Guid?());
        return this.systemService;
      }
    }

    public virtual IServiceProvider GetServiceProvider => this.ServiceProvider;

    public virtual IOrganizationService SystemUserOrganizationService
    {
      get
      {
        return this.systemUserOrganizationService ?? (this.systemUserOrganizationService = this.organizationServiceFactory.CreateOrganizationService(new Guid?()));
      }
    }

    public IFeatureControlService FeatureControlService { get; private set; }

    public virtual IPluginExecutionContext PluginExecutionContext { get; private set; }

    public IServiceEndpointNotificationService NotificationService { get; private set; }

    public ITracingService TracingService { get; private set; }

    public virtual T GetTargetFromInputParameters<T>() where T : Entity
    {
      T fromInputParameters = default (T);
      if (this.PluginExecutionContext.InputParameters.Contains("Target"))
      {
        object inputParameter = this.PluginExecutionContext.InputParameters["Target"];
        fromInputParameters = !(inputParameter is Entity) ? (T) inputParameter : ((Entity) inputParameter).ToEntity<T>();
      }
      return fromInputParameters;
    }

    public T GetPreImage<T>(string preImageName) where T : Entity
    {
      T preImage = default (T);
      if (this.PluginExecutionContext.PreEntityImages.Contains(preImageName))
      {
        Entity preEntityImage = this.PluginExecutionContext.PreEntityImages[preImageName];
        preImage = preEntityImage == null ? (T) preEntityImage : preEntityImage.ToEntity<T>();
      }
      return preImage;
    }

    public virtual void TraceOnPlugInTraceLog(string message)
    {
      if (string.IsNullOrWhiteSpace(message) || this.TracingService == null)
        return;
      if (this.PluginExecutionContext == null)
        this.TracingService.Trace(message);
      else
        this.TracingService.Trace("{0}, Correlation Id: {1}, Initiating User: {2}", (object) message, (object) this.PluginExecutionContext.CorrelationId, (object) this.PluginExecutionContext.InitiatingUserId);
    }

    public T GetSharedVariable<T>(string variableName, bool retrieveInParentContextChain = false)
    {
      T sharedVariable = default (T);
      if (this.PluginExecutionContext == null)
        return sharedVariable;
      if (!retrieveInParentContextChain)
      {
        if (this.PluginExecutionContext.SharedVariables.Contains(variableName))
          sharedVariable = (T) this.PluginExecutionContext.SharedVariables[variableName];
      }
      else
      {
        for (IPluginExecutionContext executionContext = this.PluginExecutionContext; executionContext != null; executionContext = executionContext.ParentContext)
        {
          if (executionContext.SharedVariables.Contains(variableName))
          {
            sharedVariable = (T) executionContext.SharedVariables[variableName];
            break;
          }
        }
      }
      return sharedVariable;
    }

    public virtual T GetInputParameter<T>(string inputParameterName)
    {
      T inputParameter = default (T);
      if (this.PluginExecutionContext.InputParameters.Contains(inputParameterName))
        inputParameter = (T) this.PluginExecutionContext.InputParameters[inputParameterName];
      return inputParameter;
    }

    public void SetOutputParameter<T>(
      string outputParameterName,
      T parameter,
      bool createParameter = true)
    {
      if (this.PluginExecutionContext.OutputParameters.Contains(outputParameterName))
      {
        this.PluginExecutionContext.OutputParameters[outputParameterName] = (object) parameter;
      }
      else
      {
        if (!createParameter)
          return;
        this.PluginExecutionContext.OutputParameters.Add(outputParameterName, (object) parameter);
      }
    }

    internal void Trace(string message)
    {
      if (string.IsNullOrWhiteSpace(message) || this.TracingService == null)
        return;
      if (this.PluginExecutionContext == null)
        this.TracingService.Trace(message);
      else
        this.TracingService.Trace("{0}, Correlation Id: {1}, Initiating User: {2}", (object) message, (object) this.PluginExecutionContext.CorrelationId, (object) this.PluginExecutionContext.InitiatingUserId);
    }

    internal void CreateOrganizationServiceAsInitiatingUser()
    {
      this.OrganizationService = this.organizationServiceFactory.CreateOrganizationService(new Guid?(this.PluginExecutionContext.InitiatingUserId));
    }

    internal void AddSharedVariable(string key, object value)
    {
      this.PluginExecutionContext.SharedVariables.Add(key, value);
    }

    internal bool IsSkipPluginSet() => false;
  }
}
