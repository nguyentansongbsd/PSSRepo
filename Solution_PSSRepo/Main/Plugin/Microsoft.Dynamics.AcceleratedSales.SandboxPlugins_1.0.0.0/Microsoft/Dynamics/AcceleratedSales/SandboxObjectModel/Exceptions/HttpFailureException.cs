// Decompiled with JetBrains decompiler
// Type: Microsoft.Dynamics.AcceleratedSales.SandboxObjectModel.Exceptions.HttpFailureException
// Assembly: Microsoft.Dynamics.AcceleratedSales.SandboxPlugins, Version=1.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35
// MVID: 7BBE7132-7E30-4424-B38D-27667CFBCEFA
// Assembly location: C:\Users\ngoct\Downloads\New folder\Microsoft.Dynamics.AcceleratedSales.SandboxPlugins_1.0.0.0.dll

using System;

#nullable disable
namespace Microsoft.Dynamics.AcceleratedSales.SandboxObjectModel.Exceptions
{
  internal class HttpFailureException : Exception
  {
    public HttpFailureException()
    {
    }

    public HttpFailureException(string message)
      : base(message)
    {
    }

    public HttpFailureException(string message, Exception innerException)
      : base(message, innerException)
    {
    }
  }
}
