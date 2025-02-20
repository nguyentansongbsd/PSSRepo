// Decompiled with JetBrains decompiler
// Type: Microsoft.Dynamics.AcceleratedSales.SandboxPlugins.Core.Services.JsonService
// Assembly: Microsoft.Dynamics.AcceleratedSales.SandboxPlugins, Version=1.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35
// MVID: 7BBE7132-7E30-4424-B38D-27667CFBCEFA
// Assembly location: C:\Users\ngoct\Downloads\New folder\Microsoft.Dynamics.AcceleratedSales.SandboxPlugins_1.0.0.0.dll

using Newtonsoft.Json;

#nullable disable
namespace Microsoft.Dynamics.AcceleratedSales.SandboxPlugins.Core.Services
{
  public static class JsonService
  {
    private static readonly JsonSerializerSettings JsonSettings = new JsonSerializerSettings()
    {
      MaxDepth = new int?(int.MaxValue)
    };

    public static T DeserializeObject<T>(string value, JsonSerializerSettings settings = null)
    {
      settings = settings ?? JsonService.JsonSettings;
      settings.MaxDepth = new int?(int.MaxValue);
      return JsonConvert.DeserializeObject<T>(value, settings);
    }

    public static object DeserializeObject(string value, JsonSerializerSettings settings = null)
    {
      settings = settings ?? JsonService.JsonSettings;
      settings.MaxDepth = new int?(int.MaxValue);
      return JsonConvert.DeserializeObject(value, settings);
    }

    public static string SerializeObject(object value)
    {
      return JsonConvert.SerializeObject(value, JsonService.JsonSettings);
    }
  }
}
