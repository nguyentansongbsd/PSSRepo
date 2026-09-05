using Microsoft.Xrm.Sdk;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace Action_Retransfer_Contact_Account
{
    public class Action_Retransfer_Contact_Account : IPlugin
    {
        public void Execute(IServiceProvider serviceProvider)
        {
            ITracingService tracing = (ITracingService)serviceProvider.GetService(typeof(ITracingService));
            IPluginExecutionContext context = (IPluginExecutionContext)serviceProvider.GetService(typeof(IPluginExecutionContext));

            try
            {
                // 1. Ép hệ thống dùng TLS 1.2 (Bắt buộc đối với Azure)
                System.Net.ServicePointManager.SecurityProtocol = System.Net.SecurityProtocolType.Tls12;

                // 2. Lấy tham số truyền từ JS
                string targetId = context.InputParameters.Contains("TargetId") ? context.InputParameters["TargetId"].ToString() : "";
                string entityName = context.InputParameters.Contains("EntityName") ? context.InputParameters["EntityName"].ToString() : "";
                string environment = context.InputParameters.Contains("Environment") ? context.InputParameters["Environment"].ToString() : "";

                // 3. Khai báo URL (Thêm Function Key nếu Azure có yêu cầu)
                string url = $"https://functionapp-cldvncapitaone-prod-fdezg4fwgphzcuef.southeastasia-01.azurewebsites.net/api/{environment}/upsertcontract?id={targetId}&entity={entityName}";

                tracing.Trace($"Goi URL: {url}");

                // 4. Thực hiện Request
                using (HttpClient client = new HttpClient())
                {
                    // Set Timeout tránh ngâm connection
                    client.Timeout = TimeSpan.FromSeconds(30);

                    HttpResponseMessage response = client.GetAsync(url).Result;
                    string responseContent = response.Content.ReadAsStringAsync().Result;

                    if (!response.IsSuccessStatusCode)
                    {
                        throw new InvalidPluginExecutionException($"Azure Function tra ve loi ({response.StatusCode}): {responseContent}");
                    }

                    // 5. Trả kết quả về cho JS
                    context.OutputParameters["ResponseResult"] = responseContent;
                }
            }
            catch (Exception ex)
            {
                tracing.Trace("Loi Plugin: {0}", ex.ToString());
                // Quăng lỗi ra UI để biết chính xác nguyên nhân
                throw new InvalidPluginExecutionException("Loi ket noi Azure Function: " + ex.Message);
            }
        }
    }
}
