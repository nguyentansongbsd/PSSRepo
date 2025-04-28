using Microsoft.Xrm.Sdk;
using PdfSharp.Pdf;
using PdfSharp.Pdf.IO;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
namespace Action_MergeFilePDF
{
    public class Action_MergeFilePDF : IPlugin
    {

        IPluginExecutionContext context = null;
        IOrganizationServiceFactory factory = null;
        IOrganizationService service = null;
        ITracingService tracingService = null;
        void IPlugin.Execute(IServiceProvider serviceProvider)
        {
            context = serviceProvider.GetService(typeof(IPluginExecutionContext)) as IPluginExecutionContext;
            factory = (IOrganizationServiceFactory)serviceProvider.GetService(typeof(IOrganizationServiceFactory));
            service = (IOrganizationService)factory.CreateOrganizationService(context.UserId);
            tracingService = (ITracingService)serviceProvider.GetService(typeof(ITracingService));
            var files = context.InputParameters["files"].ToString().Split(',');
            tracingService.Trace("count: " + files.Count());
            var filers =  MergePdfFiles(files);
            context.OutputParameters["fileres"] = filers;
        }
        public static string MergePdfFiles(string[] base64Files)
        {
            using (MemoryStream outputStream = new MemoryStream())
            {
                using (PdfDocument outputDocument = new PdfDocument())
                {
                    foreach (var base64File in base64Files)
                    {
                        byte[] pdfBytes = Convert.FromBase64String(base64File);

                        // Mở file PDF từ byte array
                        using (PdfDocument inputDocument = PdfReader.Open(new MemoryStream(pdfBytes), PdfDocumentOpenMode.Import))
                        {
                            // Thêm tất cả các trang từ file PDF vào file PDF đầu ra
                            for (int i = 0; i < inputDocument.PageCount; i++)
                            {
                                outputDocument.AddPage(inputDocument.Pages[i]);
                            }
                        }
                    }

                    // Lưu file PDF đã hợp nhất vào MemoryStream
                    outputDocument.Save(outputStream);
                }

                // Chuyển đổi MemoryStream thành base64
                return Convert.ToBase64String(outputStream.ToArray());
            }
        }
    }
}
