// Decompiled with JetBrains decompiler
// Type: Microsoft.Dynamics.SalesCxpProvisioning.Plugins.Core.DataAccess.FCSCheckService
// Assembly: Microsoft.Dynamics.AcceleratedSales.SandboxPlugins, Version=1.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35
// MVID: 7BBE7132-7E30-4424-B38D-27667CFBCEFA
// Assembly location: C:\Users\ngoct\Downloads\New folder\Microsoft.Dynamics.AcceleratedSales.SandboxPlugins_1.0.0.0.dll

using Microsoft.Crm.AcceleratedSales.Shared.SandboxServices.Platform;
using Microsoft.Xrm.Sdk;
using System;

#nullable disable
namespace Microsoft.Dynamics.SalesCxpProvisioning.Plugins.Core.DataAccess
{
  internal class FCSCheckService
  {
    public static void ThrowIfFeatureControlSettingsDisabled(
      IServiceProvider serviceProvider,
      string fcsNamespace,
      string fcsName)
    {
      Type type;
      object featureControl = ((IFeatureControlService) serviceProvider.GetService(typeof (IFeatureControlService))).GetFeatureControl(fcsNamespace, fcsName, ref type);
      if (!(type == typeof (bool)) || !(bool) featureControl)
        throw new CrmException(string.Format("Feature {0}.{1} is not enabled in this org. Please contact support.", (object) fcsNamespace, (object) fcsName));
    }

    public static bool GetBooleanFCSValue(
      IServiceProvider serviceProvider,
      string fcsNamespace,
      string fcsName)
    {
      Type type;
      object featureControl = ((IFeatureControlService) serviceProvider.GetService(typeof (IFeatureControlService))).GetFeatureControl(fcsNamespace, fcsName, ref type);
      return type == typeof (bool) && (bool) featureControl;
    }
  }
}
