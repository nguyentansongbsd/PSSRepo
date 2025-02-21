// Decompiled with JetBrains decompiler
// Type: Microsoft.Crm.AcceleratedSales.Shared.SandboxServices.Platform.ExceptionExtensions
// Assembly: Microsoft.Dynamics.AcceleratedSales.SandboxPlugins, Version=1.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35
// MVID: 7BBE7132-7E30-4424-B38D-27667CFBCEFA
// Assembly location: C:\Users\ngoct\Downloads\New folder\Microsoft.Dynamics.AcceleratedSales.SandboxPlugins_1.0.0.0.dll

using Microsoft.Xrm.Sdk;
using System;

#nullable disable
namespace Microsoft.Crm.AcceleratedSales.Shared.SandboxServices.Platform
{
  internal static class ExceptionExtensions
  {
    public static OrganizationServiceFault ConvertToOrganizationServiceFault(
      this Exception exception)
    {
      OrganizationServiceFault organizationServiceFault = new OrganizationServiceFault();
      organizationServiceFault.Message = exception.Message;
      organizationServiceFault.ErrorCode = exception.HResult;
      ErrorDetailCollection detailCollection = new ErrorDetailCollection();
      detailCollection["CallStack"] = (object) exception.StackTrace;
      organizationServiceFault.ErrorDetails = detailCollection;
      Exception innerException = exception.InnerException;
      organizationServiceFault.InnerFault = innerException != null ? innerException.ConvertToOrganizationServiceFault() : (OrganizationServiceFault) null;
      return organizationServiceFault;
    }
  }
}
