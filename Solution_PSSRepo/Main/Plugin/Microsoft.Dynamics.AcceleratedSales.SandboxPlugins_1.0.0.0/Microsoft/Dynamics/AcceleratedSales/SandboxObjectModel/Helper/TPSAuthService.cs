// Decompiled with JetBrains decompiler
// Type: Microsoft.Dynamics.AcceleratedSales.SandboxObjectModel.Helper.TPSAuthService
// Assembly: Microsoft.Dynamics.AcceleratedSales.SandboxPlugins, Version=1.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35
// MVID: 7BBE7132-7E30-4424-B38D-27667CFBCEFA
// Assembly location: C:\Users\ngoct\Downloads\New folder\Microsoft.Dynamics.AcceleratedSales.SandboxPlugins_1.0.0.0.dll

using Microsoft.Dynamics.AcceleratedSales.SandboxObjectModel.Model;
using Microsoft.Xrm.Sdk;
using System;

#nullable disable
namespace Microsoft.Dynamics.AcceleratedSales.SandboxObjectModel.Helper
{
  internal sealed class TPSAuthService
  {
    private const int KEY_EXPIRY_HOURS_CONFIGSTORE = 1;

    public static void StoreValueInConfigStore(
      ILocalConfigStore localConfigStore,
      string key,
      object value)
    {
      AzureFunctionHelper.ThrowIfNull(nameof (localConfigStore), (object) localConfigStore);
      localConfigStore.SetData(key, value);
    }

    public static T GetValueFromConfigStore<T>(ILocalConfigStore localConfigStore, string key)
    {
      AzureFunctionHelper.ThrowIfNull(nameof (localConfigStore), (object) localConfigStore);
      return localConfigStore.GetData<T>(key);
    }

    public static T FetchSecret<T>(IServiceProvider serviceProvider, string secretKey) where T : BaseVaultSettings
    {
      AzureFunctionHelper.ThrowIfNullOrEmpty(nameof (secretKey), secretKey);
      ITracingService service1 = serviceProvider.GetService(typeof (ITracingService)) as ITracingService;
      AzureFunctionHelper.ThrowIfNull("TracingService", (object) service1);
      ILocalConfigStore service2 = serviceProvider.GetService(typeof (ILocalConfigStore)) as ILocalConfigStore;
      string valueFromConfigStore = TPSAuthService.GetValueFromConfigStore<string>(service2, secretKey);
      T secretValue;
      if (!string.IsNullOrWhiteSpace(valueFromConfigStore))
      {
        secretValue = AzureFunctionHelper.Deserialize<T>(valueFromConfigStore);
        if (TPSAuthService.IsKeyExpiredFromConfigStore<T>(service1, secretValue))
        {
          service1.Trace(" key is expired, Fetching its value from keyvault.");
          secretValue = TPSAuthService.FetchFromKVAndUpdateInLocalConfig<T>(serviceProvider, service1, service2, secretKey);
        }
      }
      else
      {
        service1.Trace("key is not found, Fetching its value from keyvault.");
        secretValue = TPSAuthService.FetchFromKVAndUpdateInLocalConfig<T>(serviceProvider, service1, service2, secretKey);
      }
      return secretValue;
    }

    private static T FetchFromKVAndUpdateInLocalConfig<T>(
      IServiceProvider serviceProvider,
      ITracingService tracingService,
      ILocalConfigStore localConfigStore,
      string secretKey)
      where T : BaseVaultSettings
    {
      IKeyVaultClient service = serviceProvider.GetService(typeof (IKeyVaultClient)) as IKeyVaultClient;
      AzureFunctionHelper.ThrowIfNull("KeyVaultClient", (object) service);
      string secret = service.GetSecret(secretKey);
      tracingService.Trace("Value retrieved from KV");
      T message = AzureFunctionHelper.Deserialize<T>(secret);
      message.expirationTimeMs = AzureFunctionHelper.GetTimeStampInMilliSeconds(DateTime.UtcNow.AddHours(1.0));
      TPSAuthService.StoreValueInConfigStore(localConfigStore, secretKey, (object) AzureFunctionHelper.Serialize<T>(message));
      return message;
    }

    private static bool IsKeyExpiredFromConfigStore<T>(
      ITracingService tracingService,
      T secretValue)
      where T : BaseVaultSettings
    {
      long expirationTimeMs = secretValue.expirationTimeMs;
      if (AzureFunctionHelper.GetTimeStampInMilliSeconds(DateTime.UtcNow) <= expirationTimeMs)
        return false;
      tracingService.Trace("Key got expired");
      return true;
    }

    public static string FetchAccessTokenForResource(
      IServiceProvider serviceProvider,
      string authority,
      string resource)
    {
      AzureFunctionHelper.ThrowIfNullOrEmpty("Authority", authority);
      AzureFunctionHelper.ThrowIfNullOrEmpty("Resource", resource);
      IAssemblyAuthenticationContext service = serviceProvider.GetService(typeof (IAssemblyAuthenticationContext)) as IAssemblyAuthenticationContext;
      AzureFunctionHelper.ThrowIfNull("AssemblyAuthContext", (object) service);
      return service.AcquireToken(authority, resource, (AuthenticationType) 2);
    }
  }
}
