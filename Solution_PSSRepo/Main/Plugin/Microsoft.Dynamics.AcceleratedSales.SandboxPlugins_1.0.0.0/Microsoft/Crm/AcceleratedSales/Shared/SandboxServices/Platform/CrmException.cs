// Decompiled with JetBrains decompiler
// Type: Microsoft.Crm.AcceleratedSales.Shared.SandboxServices.Platform.CrmException
// Assembly: Microsoft.Dynamics.AcceleratedSales.SandboxPlugins, Version=1.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35
// MVID: 7BBE7132-7E30-4424-B38D-27667CFBCEFA
// Assembly location: C:\Users\ngoct\Downloads\New folder\Microsoft.Dynamics.AcceleratedSales.SandboxPlugins_1.0.0.0.dll

using Microsoft.Xrm.Sdk;
using System;
using System.Globalization;
using System.ServiceModel;
using System.Text;

#nullable disable
namespace Microsoft.Crm.AcceleratedSales.Shared.SandboxServices.Platform
{
  [Serializable]
  internal class CrmException : FaultException<OrganizationServiceFault>
  {
    private const int BadRequestStatusCode = 400;
    private const string Argument = "\nData[{0}] = \"{1}\"";
    private const int InternalError = 10001;

    public CrmException(string formattedErrorMessage)
      : this(formattedErrorMessage, 10001, 400)
    {
    }

    public CrmException(string message, int errorCode, params object[] arguments)
      : this(CrmException.FormatMessage(message, arguments), errorCode, 400)
    {
    }

    public CrmException(string formattedErrorMessage, int errorCode)
      : this(formattedErrorMessage, errorCode, 400)
    {
    }

    public CrmException(string message, Exception innerException)
      : this(message, innerException, 10001, 400, false)
    {
    }

    public CrmException(string message, Exception innerException, int errorCode)
      : this(message, innerException, errorCode, 400, false)
    {
    }

    public CrmException(string message, int errorCode, int statusCode, bool enableTrace)
      : this(message, (Exception) null, errorCode, statusCode, false, enableTrace)
    {
    }

    public CrmException(string message, Exception innerException, int errorCode, int statusCode)
      : this(message, innerException, errorCode, statusCode, false)
    {
    }

    protected CrmException(string formattedErrorMessage, int errorCode, int statusCode)
      : base(CrmException.BuildOrganizationServiceFault(errorCode, statusCode), new FaultReason(formattedErrorMessage))
    {
      this.StatusCode = statusCode;
      this.HResult = errorCode;
      this.Detail.Timestamp = DateTime.UtcNow;
    }

    protected CrmException(
      string message,
      Exception innerException,
      int errorCode,
      int statusCode,
      bool isFlowControlException)
      : this(message, innerException, errorCode, statusCode, isFlowControlException, true)
    {
    }

    protected CrmException(
      string message,
      Exception innerException,
      int errorCode,
      int statusCode,
      bool isFlowControlException,
      bool enableTrace)
      : base(CrmException.BuildOrganizationServiceFault(innerException, errorCode, statusCode, message), new FaultReason(message))
    {
      this.StatusCode = statusCode;
      this.HResult = errorCode;
      this.Detail.Timestamp = DateTime.UtcNow;
      if (innerException == null)
      {
        if (!enableTrace)
          ;
      }
      else if (!enableTrace)
        ;
    }

    public int StatusCode { get; set; }

    public int ErrorCode => this.HResult;

    private static string FormatMessage(string formattedErrorMessage, params object[] arguments)
    {
      string str;
      try
      {
        str = string.Format((IFormatProvider) CultureInfo.InvariantCulture, formattedErrorMessage, arguments);
      }
      catch (FormatException ex)
      {
        StringBuilder stringBuilder = new StringBuilder(formattedErrorMessage);
        for (int index = 0; index < arguments.Length; ++index)
          stringBuilder.Append(string.Format((IFormatProvider) CultureInfo.InvariantCulture, "\nData[{0}] = \"{1}\"", (object) index, arguments[index]));
        str = stringBuilder.ToString();
      }
      return str;
    }

    private static OrganizationServiceFault BuildOrganizationServiceFault(
      int errorCode,
      int statusCode)
    {
      OrganizationServiceFault organizationServiceFault = new OrganizationServiceFault();
      organizationServiceFault.ErrorCode = errorCode;
      organizationServiceFault.ErrorDetails["CallStack"] = (object) Environment.StackTrace;
      organizationServiceFault.ErrorDetails["HttpStatusCode"] = (object) statusCode;
      return organizationServiceFault;
    }

    private static OrganizationServiceFault BuildOrganizationServiceFault(
      Exception innerException,
      int errorCode,
      int statusCode,
      string message)
    {
      OrganizationServiceFault organizationServiceFault = new OrganizationServiceFault();
      organizationServiceFault.ErrorCode = errorCode;
      organizationServiceFault.Message = message;
      organizationServiceFault.InnerFault = innerException != null ? innerException.ConvertToOrganizationServiceFault() : (OrganizationServiceFault) null;
      organizationServiceFault.ErrorDetails["CallStack"] = (object) Environment.StackTrace;
      organizationServiceFault.ErrorDetails["HttpStatusCode"] = (object) statusCode;
      return organizationServiceFault;
    }
  }
}
