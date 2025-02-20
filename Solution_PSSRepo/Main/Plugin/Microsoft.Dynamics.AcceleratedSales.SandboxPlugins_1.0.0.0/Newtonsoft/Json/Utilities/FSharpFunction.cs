// Decompiled with JetBrains decompiler
// Type: Newtonsoft.Json.Utilities.FSharpFunction
// Assembly: Microsoft.Dynamics.AcceleratedSales.SandboxPlugins, Version=1.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35
// MVID: 7BBE7132-7E30-4424-B38D-27667CFBCEFA
// Assembly location: C:\Users\ngoct\Downloads\New folder\Microsoft.Dynamics.AcceleratedSales.SandboxPlugins_1.0.0.0.dll

#nullable enable
namespace Newtonsoft.Json.Utilities
{
  internal class FSharpFunction
  {
    private readonly object? _instance;
    private readonly MethodCall<object?, object> _invoker;

    public FSharpFunction(object? instance, MethodCall<object?, object> invoker)
    {
      this._instance = instance;
      this._invoker = invoker;
    }

    public object Invoke(params object[] args) => this._invoker(this._instance, args);
  }
}
