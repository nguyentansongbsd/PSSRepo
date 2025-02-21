// Decompiled with JetBrains decompiler
// Type: Microsoft.Dynamics.AcceleratedSales.SandboxPlugins.Core.DataAccess.DataStoreExtensions
// Assembly: Microsoft.Dynamics.AcceleratedSales.SandboxPlugins, Version=1.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35
// MVID: 7BBE7132-7E30-4424-B38D-27667CFBCEFA
// Assembly location: C:\Users\ngoct\Downloads\New folder\Microsoft.Dynamics.AcceleratedSales.SandboxPlugins_1.0.0.0.dll

using Microsoft.Crm.AcceleratedSales.Shared.SandboxServices.Platform;
using Microsoft.Crm.Sdk.Messages;
using Microsoft.Dynamics.AcceleratedSales.SandboxPlugins.Core.Model;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Query;
using System;
using System.ServiceModel;

#nullable disable
namespace Microsoft.Dynamics.AcceleratedSales.SandboxPlugins.Core.DataAccess
{
  public static class DataStoreExtensions
  {
    public static QueryExpression ConvertFetchXmlToQueryExpression(
      this IDataStore store,
      string fetchXml,
      IAcceleratedSalesLogger logger)
    {
      try
      {
        FetchXmlToQueryExpressionRequest request = new FetchXmlToQueryExpressionRequest()
        {
          FetchXml = fetchXml
        };
        return ((FetchXmlToQueryExpressionResponse) store.Execute((OrganizationRequest) request)).Query;
      }
      catch (FaultException<OrganizationServiceFault> ex)
      {
        if (ex.Detail.ErrorCode == -2147217149)
        {
          logger.AddCustomProperty("DataStoreExtensions.ConvertFetchXmlToQueryExpression.QueryBuilderNoAttribute.Exception", (object) (ex.InnerException ?? (Exception) ex));
          throw new CrmException("ConvertFetchXmlToQueryExpression failed QueryBuilderNoAttribute.", (Exception) ex, -2147217149);
        }
        logger.AddCustomProperty("DataStoreExtensions.ConvertFetchXmlToQueryExpression.FaultException", (object) (ex.InnerException ?? (Exception) ex));
        throw new CrmException("ConvertFetchXmlToQueryExpression failed FaultException.", (Exception) ex, 1879506950);
      }
      catch (Exception ex)
      {
        logger.AddCustomProperty("DataStoreExtensions.ConvertFetchXmlToQueryExpression.GenericException", (object) (ex.InnerException ?? ex));
        throw new CrmException("ConvertFetchXmlToQueryExpression failed GenericException.", ex, 1879506950);
      }
    }

    public static string ConvertQueryExpressionToFetchXml(
      this IDataStore store,
      QueryExpression query,
      IAcceleratedSalesLogger logger)
    {
      try
      {
        QueryExpressionToFetchXmlRequest request = new QueryExpressionToFetchXmlRequest()
        {
          Query = (QueryBase) query
        };
        return ((QueryExpressionToFetchXmlResponse) store.Execute((OrganizationRequest) request)).FetchXml;
      }
      catch (Exception ex)
      {
        logger.AddCustomProperty("DataStoreExtensions.ConvertQueryExpressionToFetchXml.Exception", (object) ex.InnerException);
        throw new CrmException("ConvertQueryExpressionToFetchXml failed.", 1879506962, new object[1]
        {
          (object) ex
        });
      }
    }

    public static bool IsFCBEnabled(this IDataStore store, string featureName)
    {
      bool flag = false;
      try
      {
        OrganizationRequest request = new OrganizationRequest("GetFeatureEnabledState")
        {
          ["FeatureName"] = (object) featureName
        };
        OrganizationResponse organizationResponse = store.Elevate().Execute(request);
        if (organizationResponse != null && organizationResponse.Results != null && organizationResponse.Results.ContainsKey("IsFeatureEnabled"))
          flag = (bool) organizationResponse.Results["IsFeatureEnabled"];
      }
      catch (Exception ex)
      {
        throw new CrmException("IsFCBEnabled:" + featureName + ".Exception", ex);
      }
      return flag;
    }

    public static bool IsAppSettingEnabled(
      this IDataStore store,
      string settingName,
      IAcceleratedSalesLogger logger)
    {
      try
      {
        SettingDetail appSetting = store.GetAppSetting(settingName);
        return appSetting != null && appSetting.DataType == 2 && appSetting?.Value?.ToLower() == "true";
      }
      catch (Exception ex)
      {
        logger.AddCustomProperty("DataStoreExtension.IsAppSettingEnabled.Exception", (object) ex);
      }
      return false;
    }
  }
}
