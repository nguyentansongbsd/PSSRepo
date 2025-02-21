// Decompiled with JetBrains decompiler
// Type: Microsoft.Dynamics.AcceleratedSales.SandboxObjectModel.Helper.AzureFunctionHelper
// Assembly: Microsoft.Dynamics.AcceleratedSales.SandboxPlugins, Version=1.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35
// MVID: 7BBE7132-7E30-4424-B38D-27667CFBCEFA
// Assembly location: C:\Users\ngoct\Downloads\New folder\Microsoft.Dynamics.AcceleratedSales.SandboxPlugins_1.0.0.0.dll

using Microsoft.Crm.Sdk.Messages;
using Microsoft.Dynamics.AcceleratedSales.SandboxObjectModel.Model;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Organization;
using Microsoft.Xrm.Sdk.PluginTelemetry;
using Microsoft.Xrm.Sdk.Query;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Net.Http;
using System.Text.RegularExpressions;

#nullable disable
namespace Microsoft.Dynamics.AcceleratedSales.SandboxObjectModel.Helper
{
  internal class AzureFunctionHelper
  {
    private const string authResourceJSONKey = "authresourcejson";
    private const string bearerTokenHeaderFormat = "bearer {0}";
    private IServiceProvider serviceProvider;
    private ITracingService tracingService;
    private IPluginExecutionContext pluginContext;
    private IOrganizationServiceFactory serviceFactory;
    private IOrganizationService organizationService;
    private ILogger logger;
    private Guid UserId;
    private static readonly DateTime epochStartTime = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);

    public AzureFunctionHelper(IServiceProvider serviceProvider)
    {
      this.serviceProvider = serviceProvider;
      this.tracingService = serviceProvider.GetService(typeof (ITracingService)) as ITracingService;
      this.pluginContext = (IPluginExecutionContext) serviceProvider.GetService(typeof (IPluginExecutionContext));
      this.serviceFactory = (IOrganizationServiceFactory) serviceProvider.GetService(typeof (IOrganizationServiceFactory));
      this.organizationService = this.serviceFactory.CreateOrganizationService(new Guid?());
      this.logger = (ILogger) serviceProvider.GetService(typeof (ILogger));
      this.UserId = this.pluginContext.UserId;
    }

    public string InvokeAzureFunctionApp(
      string serviceName,
      string webApiName,
      HttpMethod httpMethod,
      string requestPayload,
      Guid userId,
      Dictionary<string, string> headers = null)
    {
      AzureFunctionHelper.ThrowIfNullOrEmpty(nameof (serviceName), serviceName);
      AzureFunctionHelper.ThrowIfNullOrEmpty(nameof (requestPayload), requestPayload);
      AzureFunctionHelper.ThrowIfNull(nameof (webApiName), (object) webApiName);
      AzureFunctionHelper.ThrowIfNull(nameof (userId), (object) userId);
      AzureFunctionHelper.ThrowIfNull("ITracingService", (object) this.tracingService);
      AzureFunctionHelper.ThrowIfNull("IPluginExecutionContext", (object) this.pluginContext);
      this.UserId = userId;
      this.logger.AddCustomProperty("orgId", this.pluginContext.OrganizationId.ToString());
      if (this.organizationService != null)
      {
        try
        {
          EntityCollection entityCollection = this.organizationService.RetrieveMultiple((QueryBase) new QueryExpression()
          {
            EntityName = "msdyn_salesaccelerationsettings",
            ColumnSet = new ColumnSet(new string[1]
            {
              "statecode"
            })
          });
          if (entityCollection == null || entityCollection.Entities == null || entityCollection.Entities.Count <= 0 || !entityCollection.Entities[0].Contains("statecode") || ((OptionSetValue) entityCollection.Entities[0]["statecode"]).Value != 1)
          {
            this.tracingService.Trace("Sales Accelerator setting is not published");
            this.logger.LogWarning("Sales Accelerator setting is not published", Array.Empty<object>());
            return "";
          }
          this.tracingService.Trace("Sales Accelerator setting is published");
          this.logger.LogWarning("Sales Accelerator setting is published", Array.Empty<object>());
        }
        catch (Exception ex)
        {
          this.tracingService.Trace("msdyn_salesaccelerationsettings retrieve failed");
          this.logger.LogError(ex, "msdyn_salesaccelerationsettings retrieve failed", Array.Empty<object>());
          return "";
        }
        string orgId;
        this.GetOrgDetails(out orgId, out string _);
        string userLanguage = this.GetUserLanguage();
        AzureFunctionHelper.ThrowIfNullOrEmpty("orgId", orgId);
        this.tracingService.Trace("Invoking Function app {0} with correlationid {1} for Sales Acceleration", (object) serviceName, (object) this.pluginContext.CorrelationId.ToString());
        AuthResourceSettings authResourceSettings = TPSAuthService.FetchSecret<AuthResourceSettings>(this.serviceProvider, "authresourcejson");
        AzureFunctionHelper.ValidateAuthResourceObj(authResourceSettings);
        string fnApiUrl;
        string fnApiKey;
        this.FetchFnAppDetailsFromOrgType(serviceName, webApiName, orgId, authResourceSettings, out fnApiUrl, out fnApiKey);
        string accToken = string.Format((IFormatProvider) CultureInfo.InvariantCulture, "bearer {0}", (object) TPSAuthService.FetchAccessTokenForResource(this.serviceProvider, authResourceSettings.authorityurl, authResourceSettings.resourceapp));
        Dictionary<string, string> headers1 = this.RetrieveHeadersMap(orgId, fnApiKey, accToken, userLanguage);
        if (headers != null)
        {
          foreach (string key in headers.Keys)
            headers1.Add(key, headers[key]);
        }
        return HttpClientHelper.InvokeHttpRequest(fnApiUrl, httpMethod, requestPayload, headers1).GetAwaiter().GetResult();
      }
      this.tracingService.Trace("OrganizationService is null");
      this.logger.LogWarning("OrganizationService is null", Array.Empty<object>());
      return "";
    }

    public void GetOrgDetails(out string orgId, out string orgUrl)
    {
      RetrieveCurrentOrganizationResponse organizationResponse = this.serviceFactory.CreateOrganizationService(new Guid?(this.UserId)).Execute((OrganizationRequest) new RetrieveCurrentOrganizationRequest()) as RetrieveCurrentOrganizationResponse;
      orgId = string.Empty;
      orgUrl = string.Empty;
      if (organizationResponse?.Detail == null)
        return;
      Guid organizationId = organizationResponse.Detail.OrganizationId;
      if (true)
        orgId = organizationResponse.Detail.OrganizationId.ToString();
      if (organizationResponse.Detail.Endpoints != null && organizationResponse.Detail.Endpoints.ContainsKey(EndpointType.WebApplication))
        orgUrl = organizationResponse.Detail.Endpoints[EndpointType.WebApplication];
    }

    public static long GetTimeStampInMilliSeconds(DateTime dateObj)
    {
      return (long) dateObj.ToUniversalTime().Subtract(AzureFunctionHelper.epochStartTime).TotalMilliseconds;
    }

    public static void ThrowIfNullOrEmpty(string name, string argument)
    {
      if (string.IsNullOrWhiteSpace(argument))
        throw new ArgumentException(name, "Null or empty value recieved for " + name + ". Value: " + argument);
    }

    public static void ThrowIfNull(string name, object argument)
    {
      if (argument == null)
        throw new ArgumentNullException(name, "Null value recieved for " + name);
    }

    public static T Deserialize<T>(string serializedJson)
    {
      AzureFunctionHelper.ThrowIfNullOrEmpty(typeof (T).FullName, serializedJson);
      return JsonConvert.DeserializeObject<T>(serializedJson);
    }

    public static string Serialize<T>(T message)
    {
      AzureFunctionHelper.ThrowIfNull(typeof (T).FullName, (object) message);
      return JsonConvert.SerializeObject((object) message);
    }

    private string GetUserLanguage()
    {
      IOrganizationService organizationService = this.serviceFactory.CreateOrganizationService(new Guid?(this.UserId));
      try
      {
        return organizationService.Retrieve("usersettings", this.UserId, new ColumnSet(new string[1]
        {
          "uilanguageid"
        }))["uilanguageid"].ToString();
      }
      catch (Exception ex)
      {
        this.tracingService.Trace("This user {0} doesnot contains usersettings", (object) this.UserId);
        return "0";
      }
    }

    private Dictionary<string, string> RetrieveHeadersMap(
      string orgId,
      string functionAppHostkey,
      string accToken,
      string userLanguage)
    {
      return new Dictionary<string, string>()
      {
        {
          "x-functions-key",
          functionAppHostkey
        },
        {
          "Authorization",
          accToken
        },
        {
          "Request-Id",
          this.pluginContext.CorrelationId.ToString()
        },
        {
          "ClientType",
          "SalesAppUci"
        },
        {
          "OrgId",
          orgId
        },
        {
          "UserId",
          this.UserId.ToString()
        },
        {
          "UserLanguage",
          userLanguage
        }
      };
    }

    private void FetchFnAppDetailsFromOrgType(
      string serviceName,
      string webApiName,
      string orgId,
      AuthResourceSettings authResourceSettings,
      out string fnApiUrl,
      out string fnApiKey)
    {
      if (!authResourceSettings.ignoreorgtype)
        serviceName = this.fetchFnAppApiNameFromOrgType(serviceName, orgId, authResourceSettings);
      this.tracingService.Trace("Using ServiceName {0} and webApiName {1} with CorrelationId as {2}", (object) serviceName, (object) webApiName, (object) this.pluginContext.CorrelationId.ToString());
      FnAppAPIDetail fnAppObj = TPSAuthService.FetchSecret<FnAppAPIDetail>(this.serviceProvider, serviceName);
      AzureFunctionHelper.ValidateFnAppAPIObj(fnAppObj);
      fnApiKey = fnAppObj.fnapikey;
      fnApiUrl = Regex.Replace(fnAppObj.fnapiurl, "{.*?}", webApiName);
    }

    private string fetchFnAppApiNameFromOrgType(
      string funAppApiName,
      string orgId,
      AuthResourceSettings authResourceSettings)
    {
      string str = this.fetchOrgTypeFromExclusionList(orgId, authResourceSettings.orgexcllist);
      if (!string.IsNullOrWhiteSpace(str))
        funAppApiName = funAppApiName + "-" + str;
      return funAppApiName;
    }

    [ExcludeFromCodeCoverage]
    private string fetchOrgTypeFromExclusionList(string orgId, string orgIdExclusionList)
    {
      AzureFunctionHelper.ThrowIfNullOrEmpty(nameof (orgId), orgId);
      this.tracingService.Trace("Checking if org {0} is in exclusion list {1}", (object) orgId, (object) orgIdExclusionList);
      string str1 = "";
      if (orgIdExclusionList == null)
        return str1;
      string[] collection;
      if (orgIdExclusionList == null)
        collection = (string[]) null;
      else
        collection = orgIdExclusionList.Split(',');
      foreach (string str2 in new List<string>((IEnumerable<string>) collection))
      {
        if (!string.IsNullOrWhiteSpace(str2) && str2.StartsWith(orgId, StringComparison.OrdinalIgnoreCase))
        {
          int length = str2.LastIndexOf('-');
          if (length != -1 && string.Equals(orgId, str2.Substring(0, length), StringComparison.OrdinalIgnoreCase))
          {
            str1 = str2.Substring(length + 1);
            break;
          }
        }
      }
      if (string.IsNullOrWhiteSpace(str1))
        this.tracingService.Trace("This org {0} is NOT in exclusion list, Will be pointed to Default Endpoints", (object) orgId);
      else
        this.tracingService.Trace("This org {0} is in exclusion list, Will invoke orgType {1} endpoints.", (object) orgId, (object) str1);
      return str1;
    }

    private static void ValidateFnAppAPIObj(FnAppAPIDetail fnAppObj)
    {
      AzureFunctionHelper.ThrowIfNull("FnAppAPIDetail", (object) fnAppObj);
      AzureFunctionHelper.ThrowIfNullOrEmpty("fnapiurl", fnAppObj.fnapiurl);
      AzureFunctionHelper.ThrowIfNullOrEmpty("fnapikey", fnAppObj.fnapikey);
    }

    private static void ValidateAuthResourceObj(AuthResourceSettings authResourceSettings)
    {
      AzureFunctionHelper.ThrowIfNull("AuthResourceSettings", (object) authResourceSettings);
      AzureFunctionHelper.ThrowIfNullOrEmpty("resourceapp", authResourceSettings.resourceapp);
      AzureFunctionHelper.ThrowIfNullOrEmpty("authorityurl", authResourceSettings.authorityurl);
    }
  }
}
