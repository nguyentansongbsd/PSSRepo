// Decompiled with JetBrains decompiler
// Type: Microsoft.Dynamics.AcceleratedSales.SandboxPlugins.Platform.DataverseStore
// Assembly: Microsoft.Dynamics.AcceleratedSales.SandboxPlugins, Version=1.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35
// MVID: 7BBE7132-7E30-4424-B38D-27667CFBCEFA
// Assembly location: C:\Users\ngoct\Downloads\New folder\Microsoft.Dynamics.AcceleratedSales.SandboxPlugins_1.0.0.0.dll

using Microsoft.Crm.AcceleratedSales.Shared.SandboxServices.Platform;
using Microsoft.Crm.Sdk.Messages;
using Microsoft.Dynamics.AcceleratedSales.SandboxPlugins.Core.Model;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.PluginTelemetry;
using Microsoft.Xrm.Sdk.Query;
using System;
using System.Text.RegularExpressions;

#nullable disable
namespace Microsoft.Dynamics.AcceleratedSales.SandboxPlugins.Platform
{
  public class DataverseStore : IDataStore
  {
    private readonly LocalPluginContext localPluginContext;
    private readonly IOrganizationService organizationService;

    public DataverseStore(LocalPluginContext localPluginContext)
      : this(localPluginContext, false)
    {
    }

    protected DataverseStore(LocalPluginContext localPluginContext, bool elevateContext)
    {
      this.localPluginContext = localPluginContext;
      this.organizationService = elevateContext ? this.localPluginContext.SystemUserOrganizationService : this.localPluginContext.OrganizationService;
    }

    protected LocalPluginContext LocalPluginContext => this.localPluginContext;

    public Guid Create(Entity entity) => this.organizationService.Create(entity);

    public OrganizationResponse Execute(OrganizationRequest request)
    {
      return this.organizationService.Execute(request);
    }

    public Entity Retrieve(string entityName, Guid id, ColumnSet columnSet)
    {
      return this.organizationService.Retrieve(entityName, id, columnSet);
    }

    public EntityCollection RetrieveMultiple(QueryExpression query)
    {
      return this.organizationService.RetrieveMultiple((QueryBase) query);
    }

    public void Update(Entity entity) => this.organizationService.Update(entity);

    public IDataStore Elevate() => (IDataStore) new DataverseStore(this.localPluginContext, true);

    public Guid RetrieveUserId() => this.localPluginContext.PluginExecutionContext.UserId;

    public bool IsFCSEnabled(string fcsNamespace, string fcsName)
    {
      Type type;
      object featureControl = ((IFeatureControlService) this.localPluginContext?.ServiceProvider?.GetService(typeof (IFeatureControlService))).GetFeatureControl(fcsNamespace, fcsName, ref type);
      return type == typeof (bool) && (bool) featureControl;
    }

    public bool IsFCBEnabled(string fcbName)
    {
      bool flag = false;
      try
      {
        OrganizationRequest request = new OrganizationRequest("GetFeatureEnabledState")
        {
          ["FeatureName"] = (object) fcbName
        };
        OrganizationResponse organizationResponse = this.Elevate().Execute(request);
        if (organizationResponse != null && organizationResponse.Results != null && organizationResponse.Results.ContainsKey("IsFeatureEnabled"))
          flag = (bool) organizationResponse.Results["IsFeatureEnabled"];
      }
      catch (Exception ex)
      {
        throw new CrmException("IsFCBEnabled:" + fcbName + ".Exception", ex);
      }
      return flag;
    }

    public SettingDetail GetAppSetting(string settingName)
    {
      SettingDetail appSetting = new SettingDetail();
      try
      {
        OrganizationRequest request = new OrganizationRequest("RetrieveSetting")
        {
          ["SettingName"] = (object) settingName
        };
        OrganizationResponse organizationResponse = this.Elevate().Execute(request);
        SettingDetail settingDetail;
        if (organizationResponse != null && organizationResponse.Results != null && organizationResponse.Results.TryGetValue<SettingDetail>("SettingDetail", ref settingDetail))
          appSetting = settingDetail;
      }
      catch (Exception ex)
      {
        throw new CrmException("GetAppSettingValue:" + settingName + ".Exception", ex);
      }
      return appSetting;
    }

    public bool IsTemplateOrg(ILogger logger)
    {
      RetrieveCurrentOrganizationResponse organizationResponse = (RetrieveCurrentOrganizationResponse) this.Execute((OrganizationRequest) new RetrieveCurrentOrganizationRequest());
      if (organizationResponse == null || organizationResponse.Detail == null || string.IsNullOrWhiteSpace(organizationResponse.Detail.UniqueName))
      {
        logger.AddCustomProperty("CurrentOrganizationResponse.IsNull", "true");
        return true;
      }
      bool flag1 = new Regex("t\\d{12}z[a-f|\\d]{16}").IsMatch(organizationResponse.Detail.UniqueName);
      bool flag2 = string.IsNullOrWhiteSpace(organizationResponse.Detail.TenantId);
      if (!(flag1 & flag2))
        return false;
      logger.AddCustomProperty("CurrentOrganizationResponse.UniqueName", organizationResponse.Detail.UniqueName);
      logger.AddCustomProperty("CurrentOrganizationResponse.TenantId.IsNullOrWhiteSpace", flag2.ToString());
      return true;
    }
  }
}
