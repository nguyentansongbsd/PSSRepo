// Decompiled with JetBrains decompiler
// Type: Microsoft.Dynamics.AcceleratedSales.SandboxPlugins.Platform.Logger
// Assembly: Microsoft.Dynamics.AcceleratedSales.SandboxPlugins, Version=1.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35
// MVID: 7BBE7132-7E30-4424-B38D-27667CFBCEFA
// Assembly location: C:\Users\ngoct\Downloads\New folder\Microsoft.Dynamics.AcceleratedSales.SandboxPlugins_1.0.0.0.dll

using Microsoft.Crm.AcceleratedSales.Shared.SandboxServices.Platform;
using Microsoft.Dynamics.AcceleratedSales.SandboxPlugins.Core.Model;
using Microsoft.Xrm.Sdk.PluginTelemetry;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Runtime.CompilerServices;

#nullable disable
namespace Microsoft.Dynamics.AcceleratedSales.SandboxPlugins.Platform
{
  public class Logger : IAcceleratedSalesLogger
  {
    private readonly ILogger loggerService;
    private readonly Stopwatch stopwatch;
    private int count = 0;

    public Logger(LocalPluginContext context)
    {
      this.loggerService = (ILogger) context.GetServiceProvider.GetService(typeof (ILogger));
      this.stopwatch = Stopwatch.StartNew();
    }

    public void LogError(
      string errorMessage,
      Dictionary<string, object> customProperties = null,
      [CallerMemberName] string callerName = "",
      [CallerFilePath] string callerFile = "")
    {
      this.LogError(1879506953, errorMessage, customProperties, callerName, callerFile);
    }

    public void LogError(
      string errorMessage,
      Exception exception,
      Dictionary<string, object> customProperties = null,
      [CallerMemberName] string callerName = "",
      [CallerFilePath] string callerFile = "")
    {
      this.loggerService.LogError(exception, this.GetFileName(callerFile) + " " + callerName + " " + errorMessage, new object[1]
      {
        (object) customProperties
      });
    }

    public void LogError(
      int errorCode,
      string errorMessage,
      Dictionary<string, object> customProperties = null,
      [CallerMemberName] string callerName = "",
      [CallerFilePath] string callerFile = "")
    {
      this.AppendErrorCodeToCustomProperties(errorCode, customProperties);
      this.loggerService.LogError(this.GetFileName(callerFile) + " " + callerName + " " + errorMessage, new object[1]
      {
        (object) customProperties
      });
    }

    public void LogWarning(
      string message,
      Dictionary<string, object> customProperties = null,
      [CallerMemberName] string callerName = "",
      [CallerFilePath] string callerFile = "")
    {
      this.loggerService.LogWarning(this.GetFileName(callerFile) + " " + callerName + " " + message, new object[1]
      {
        (object) customProperties
      });
    }

    public void LogInfo(
      string message,
      Dictionary<string, object> customProperties = null,
      [CallerMemberName] string callerName = "",
      [CallerFilePath] string callerFile = "")
    {
      this.loggerService.Log((LogLevel) 2, this.GetFileName(callerFile) + " " + callerName + " " + message, new object[1]
      {
        (object) customProperties
      });
      this.loggerService.LogInformation(message, Array.Empty<object>());
      this.AddCustomProperties(customProperties);
    }

    public void Execute(string activityType, Action action)
    {
      this.loggerService.Execute(activityType, action, (IEnumerable<KeyValuePair<string, string>>) null);
    }

    public void AddCustomProperties(Dictionary<string, object> customProperties)
    {
      // ISSUE: explicit non-virtual call
      if (customProperties == null || __nonvirtual (customProperties.Count) <= 0)
        return;
      foreach (KeyValuePair<string, object> customProperty in customProperties)
        this.AddCustomProperty(customProperty.Key, (object) customProperty);
    }

    public void AddCustomProperty(string propertyName, object propertyValue)
    {
      this.loggerService.AddCustomProperty(string.Format("[{0}] {1}", (object) this.count++, (object) propertyName), propertyValue?.ToString() ?? "null");
    }

    private void AppendErrorCodeToCustomProperties(
      int errorCode,
      Dictionary<string, object> customProperties)
    {
      if (customProperties == null)
        customProperties = new Dictionary<string, object>();
      if (customProperties.ContainsKey("errorcode"))
        return;
      customProperties.Add("errorcode", (object) errorCode.ToString());
    }

    private string GetFileName(string filePath) => Path.GetFileName(filePath);
  }
}
