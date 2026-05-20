using Action_Invoice_VNPT_UpdateInvoiceNoBySchedule.Models;
using Microsoft.Xrm.Sdk;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Serialization;

namespace Action_Invoice_VNPT_UpdateInvoiceNoBySchedule.Helpers
{
    public class ApiHelper
    {
        public static SoapEnvelopeResponse PostVNPT(string url,string data,ITracingService tracingService)
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

                    if (!response.IsSuccessStatusCode)
                    {
                        throw new InvalidPluginExecutionException(
                            $"VNPT API Error: {response.StatusCode} - {result}");
                    }

                    tracingService.Trace(
                        $"VNPT Status: {response.StatusCode}");

                    tracingService.Trace(result);
                    var serializer = new XmlSerializer(typeof(SoapEnvelopeResponse));

                    SoapEnvelopeResponse soapEnvelopeResponse;

                    using (var reader = new StringReader(result))
                    {
                        soapEnvelopeResponse = (SoapEnvelopeResponse)serializer.Deserialize(reader);
                    }
                    return soapEnvelopeResponse;
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
