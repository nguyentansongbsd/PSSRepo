using Microsoft.Xrm.Sdk;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Remoting.Contexts;
using System.Runtime.Remoting.Services;
using System.Text;
using System.Threading.Tasks;
using static System.Net.Mime.MediaTypeNames;
using System.Xml.Linq;

using System.Runtime.InteropServices;
using Microsoft.Office.Interop.Word;
using Application = Microsoft.Office.Interop.Word.Application;

namespace Action_ConvertWordToPDF
{
    public class Action_ConvertWordToPDF : IPlugin
    {

        IPluginExecutionContext context = null;
        IOrganizationServiceFactory factory = null;
        IOrganizationService service = null;
        ITracingService tracingService = null;
        Entity en = new Entity();
        public void Execute(IServiceProvider serviceProvider)
        {

            context = serviceProvider.GetService(typeof(IPluginExecutionContext)) as IPluginExecutionContext;
            factory = (IOrganizationServiceFactory)serviceProvider.GetService(typeof(IOrganizationServiceFactory));
            service = (IOrganizationService)factory.CreateOrganizationService(context.UserId);
            tracingService = (ITracingService)serviceProvider.GetService(typeof(ITracingService));
            string input = context.InputParameters["input"].ToString();
            string  output=ConvertWordToPdfBase64(input);
            context.OutputParameters["output"] = output;


        }
        static string ConvertWordToPdfBase64(string base64Word)
        {
            // Chuyển đổi Base64 thành byte array
            byte[] wordBytes = Convert.FromBase64String(base64Word);
            string tempWordFilePath = Path.Combine(Path.GetTempPath(), "temp.docx");
            string tempPdfFilePath = Path.Combine(Path.GetTempPath(), "temp.pdf");

            // Lưu file Word tạm thời
            File.WriteAllBytes(tempWordFilePath, wordBytes);

            Microsoft.Office.Interop.Word.Application wordApp = new Application();
            Document wordDoc = null;

            try
            {
                // Mở file Word
                wordDoc = wordApp.Documents.Open(tempWordFilePath);
                // Chuyển đổi sang PDF
                wordDoc.SaveAs2(tempPdfFilePath, WdSaveFormat.wdFormatPDF);
            }
            catch (Exception ex)
            {
                Console.WriteLine("Lỗi: " + ex.Message);
                return null;
            }
            finally
            {
                // Đóng tài liệu và ứng dụng Word
                if (wordDoc != null)
                {
                    wordDoc.Close();
                }
                wordApp.Quit();
            }

            // Đọc file PDF và chuyển đổi thành Base64
            byte[] pdfBytes = File.ReadAllBytes(tempPdfFilePath);
            string pdfBase64 = Convert.ToBase64String(pdfBytes);

            // Xóa file tạm thời
            File.Delete(tempWordFilePath);
            File.Delete(tempPdfFilePath);

            return pdfBase64;
        }
    }
}
