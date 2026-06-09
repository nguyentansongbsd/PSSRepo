using Microsoft.Xrm.Sdk;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace Plugin_Invoice_VNPT_SubmitInvoice.Helpers
{
    public class ApiHelper
    {
        public static void PostVNPT(
            string url,
            string data,
            ITracingService tracingService)
        {
            try
            {
                using (var httpClient = new HttpClient())
                {
                    httpClient.Timeout = TimeSpan.FromMinutes(5);

                    var content = new StringContent(
                        data,
                        Encoding.UTF8,
                        "text/xml");

                    //content.Headers.Add(
                    //    "SOAPAction",
                    //    "http://tempuri.org/ImportInvByPattern");

                    var response = httpClient
                        .PostAsync(url, content)
                        .Result;

                    var result = response.Content
                        .ReadAsStringAsync()
                        .Result;

                    tracingService.Trace(
                        $"VNPT Status: {response.StatusCode}");

                    tracingService.Trace(result);

                    if (!response.IsSuccessStatusCode)
                    {
                        throw new InvalidPluginExecutionException(
                            $"VNPT API Error: {response.StatusCode} - {result}");
                    }
                }
            }
            catch (Exception ex)
            {
                tracingService.Trace(ex.ToString());
                throw;
            }
        }
    }
}
