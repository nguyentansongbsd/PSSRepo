// Decompiled with JetBrains decompiler
// Type: Microsoft.Dynamics.AcceleratedSales.SandboxObjectModel.Helper.HttpClientHelper
// Assembly: Microsoft.Dynamics.AcceleratedSales.SandboxPlugins, Version=1.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35
// MVID: 7BBE7132-7E30-4424-B38D-27667CFBCEFA
// Assembly location: C:\Users\ngoct\Downloads\New folder\Microsoft.Dynamics.AcceleratedSales.SandboxPlugins_1.0.0.0.dll

using Microsoft.Dynamics.AcceleratedSales.SandboxObjectModel.Exceptions;
using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

#nullable disable
namespace Microsoft.Dynamics.AcceleratedSales.SandboxObjectModel.Helper
{
  internal class HttpClientHelper
  {
    private const int Max_Retry_Attempts = 3;
    private const string SalesAcceleration_HttpException = "Exception occurred during http call execution";
    private const string SalesAcceleration_Http_FailureStatus = "HTTPResponse is not success : Status code {0}";
    private static readonly HttpClient Client = HttpClientHelper.CreateCompliantHttpClient();
    private static List<HttpStatusCode> httpStatusCodeListForRetry = new List<HttpStatusCode>()
    {
      HttpStatusCode.RequestTimeout,
      HttpStatusCode.InternalServerError,
      HttpStatusCode.BadGateway,
      HttpStatusCode.ServiceUnavailable,
      HttpStatusCode.GatewayTimeout
    };

    private static HttpRequestMessage CreateRequestMessage(
      HttpMethod method,
      string requestUri,
      Dictionary<string, string> headers,
      HttpContent content = null)
    {
      HttpRequestMessage requestMessage = new HttpRequestMessage();
      requestMessage.Method = method;
      requestMessage.RequestUri = new Uri(requestUri);
      foreach (KeyValuePair<string, string> header in headers)
        requestMessage.Headers.Add(header.Key, header.Value);
      if (content != null)
        requestMessage.Content = content;
      return requestMessage;
    }

    public static async Task<string> InvokeHttpRequest(
      string requestUrl,
      HttpMethod httpMethod,
      string requestPayload,
      Dictionary<string, string> headers)
    {
      HttpContent httpContent = (HttpContent) null;
      if (requestPayload != null)
        httpContent = (HttpContent) new StringContent(requestPayload, Encoding.UTF8, "application/json");
      HttpRequestMessage requestMessage = HttpClientHelper.CreateRequestMessage(httpMethod, requestUrl, headers, httpContent);
      HttpResponseMessage response = (HttpResponseMessage) null;
      string responseString = string.Empty;
      for (int i = 1; i <= 3; ++i)
      {
        string str;
        try
        {
          response = await HttpClientHelper.Client.SendAsync(requestMessage);
          if (response != null)
          {
            if (response.IsSuccessStatusCode)
            {
              HttpContent responseContent = response.Content;
              responseString = responseContent.ReadAsStringAsync().Result;
              str = responseString;
            }
            else
            {
              if (HttpClientHelper.IsTransientError(response))
                continue;
              break;
            }
          }
          else
            continue;
        }
        catch (Exception ex)
        {
          throw new HttpFailureException("Exception occurred during http call execution:" + ex.Message, ex);
        }
        httpContent = (HttpContent) null;
        requestMessage = (HttpRequestMessage) null;
        response = (HttpResponseMessage) null;
        responseString = (string) null;
        return str;
      }
      throw new HttpFailureException(string.Format("HTTPResponse is not success : Status code {0}", (object) response?.StatusCode));
    }

    private static bool IsTransientError(HttpResponseMessage responseMessage)
    {
      return HttpClientHelper.httpStatusCodeListForRetry.Contains(responseMessage.StatusCode);
    }

    public static HttpClient CreateCompliantHttpClient(HttpClientHandler handler = null)
    {
      if (handler == null)
        handler = new HttpClientHandler();
      try
      {
        HttpClientHandlerHelper.SetCheckCertificateRevocationList(handler);
      }
      catch (MissingMethodException ex)
      {
      }
      catch (PlatformNotSupportedException ex)
      {
      }
      return new HttpClient((HttpMessageHandler) handler);
    }
  }
}
