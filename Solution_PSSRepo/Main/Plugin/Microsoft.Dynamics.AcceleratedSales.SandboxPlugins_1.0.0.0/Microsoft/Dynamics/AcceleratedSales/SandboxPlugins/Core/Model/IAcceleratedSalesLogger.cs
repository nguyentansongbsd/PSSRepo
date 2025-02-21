// Decompiled with JetBrains decompiler
// Type: Microsoft.Dynamics.AcceleratedSales.SandboxPlugins.Core.Model.IAcceleratedSalesLogger
// Assembly: Microsoft.Dynamics.AcceleratedSales.SandboxPlugins, Version=1.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35
// MVID: 7BBE7132-7E30-4424-B38D-27667CFBCEFA
// Assembly location: C:\Users\ngoct\Downloads\New folder\Microsoft.Dynamics.AcceleratedSales.SandboxPlugins_1.0.0.0.dll

using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

#nullable disable
namespace Microsoft.Dynamics.AcceleratedSales.SandboxPlugins.Core.Model
{
  public interface IAcceleratedSalesLogger
  {
    void LogError(
      string errorMessage,
      Dictionary<string, object> customProperties = null,
      [CallerMemberName] string callerName = "",
      [CallerFilePath] string callerFile = "");

    void LogError(
      string errorMessage,
      Exception exception,
      Dictionary<string, object> customProperties = null,
      [CallerMemberName] string callerName = "",
      [CallerFilePath] string callerFile = "");

    void LogError(
      int errorCode,
      string errorMessage,
      Dictionary<string, object> customProperties = null,
      [CallerMemberName] string callerName = "",
      [CallerFilePath] string callerFile = "");

    void LogWarning(
      string message,
      Dictionary<string, object> customProperties = null,
      [CallerMemberName] string callerName = "",
      [CallerFilePath] string callerFile = "");

    void LogInfo(
      string message,
      Dictionary<string, object> customProperties = null,
      [CallerMemberName] string callerName = "",
      [CallerFilePath] string callerFile = "");

    void Execute(string activityType, Action action);

    void AddCustomProperties(Dictionary<string, object> customProperties);

    void AddCustomProperty(string propertyName, object propertyValue);
  }
}
