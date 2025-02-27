// Decompiled with JetBrains decompiler
// Type: Microsoft.Dynamics.AcceleratedSales.SandboxPlugins.Core.Model.IDataStore
// Assembly: Microsoft.Dynamics.AcceleratedSales.SandboxPlugins, Version=1.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35
// MVID: 7BBE7132-7E30-4424-B38D-27667CFBCEFA
// Assembly location: C:\Users\ngoct\Downloads\New folder\Microsoft.Dynamics.AcceleratedSales.SandboxPlugins_1.0.0.0.dll

using Microsoft.Crm.Sdk.Messages;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.PluginTelemetry;
using Microsoft.Xrm.Sdk.Query;
using System;

#nullable disable
namespace Microsoft.Dynamics.AcceleratedSales.SandboxPlugins.Core.Model
{
  public interface IDataStore
  {
    Guid Create(Entity entity);

    Entity Retrieve(string entityName, Guid id, ColumnSet columnSet);

    void Update(Entity entity);

    EntityCollection RetrieveMultiple(QueryExpression query);

    OrganizationResponse Execute(OrganizationRequest request);

    IDataStore Elevate();

    Guid RetrieveUserId();

    bool IsFCSEnabled(string fcsNamespace, string fcsName);

    bool IsFCBEnabled(string fcbName);

    SettingDetail GetAppSetting(string settingName);

    bool IsTemplateOrg(ILogger logger);
  }
}
