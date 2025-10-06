using Microsoft.Xrm.Sdk;
using iTextSharp.text;
using iTextSharp.text.pdf;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Action_InsertQrcodeInPDF
{
    public class Action_InsertQrcodeInPDF : IPlugin
    {
        private IPluginExecutionContext context;
        private IOrganizationService service;
        private ITracingService tracingService;

        public void Execute(IServiceProvider serviceProvider)
        {
            context = (IPluginExecutionContext)serviceProvider.GetService(typeof(IPluginExecutionContext));
            service = ((IOrganizationServiceFactory)serviceProvider.GetService(typeof(IOrganizationServiceFactory))).CreateOrganizationService(context.UserId);
            tracingService = (ITracingService)serviceProvider.GetService(typeof(ITracingService));

            try
            {
                tracingService.Trace("Bắt đầu Action_InsertQrcodeInPDF.");

                // 1. Lấy các tham số đầu vào từ context
                if (!context.InputParameters.Contains("pdfBase64") || !(context.InputParameters["pdfBase64"] is string))
                    throw new InvalidPluginExecutionException("Tham số 'pdfBase64' không hợp lệ.");

                if (!context.InputParameters.Contains("qrBase64") || !(context.InputParameters["qrBase64"] is string))
                    context.OutputParameters["modifiedPdfBase64"] = context.InputParameters["pdfBase64"].ToString();

                string pdfBase64 = context.InputParameters["pdfBase64"].ToString();
                string qrBase64 = context.InputParameters["qrBase64"].ToString();

                // 2. Chuyển đổi chuỗi Base64 thành mảng byte
                tracingService.Trace("Giải mã chuỗi base64.");
                byte[] pdfBytes = Convert.FromBase64String(pdfBase64);
                byte[] qrBytes = Convert.FromBase64String(qrBase64);

                // 3. Sử dụng iTextSharp để chèn QR code vào PDF
                tracingService.Trace("Bắt đầu xử lý PDF.");
                using (MemoryStream outputStream = new MemoryStream())
                {
                    PdfReader reader = new PdfReader(pdfBytes);
                    PdfStamper stamper = new PdfStamper(reader, outputStream);

                    // Tạo đối tượng hình ảnh từ QR code
                    Image qrImage = Image.GetInstance(qrBytes);

                    // Thiết lập kích thước cho QR code (ví dụ: 70x70 points)
                    qrImage.ScaleAbsolute(70f, 70f);

                    // Lặp qua tất cả các trang trong file PDF
                    for (int i = 1; i <= reader.NumberOfPages; i++)
                    {
                        tracingService.Trace($"Đang xử lý trang {i}.");
                        // Lấy kích thước trang
                        Rectangle pageSize = reader.GetPageSizeWithRotation(i);

                        // Tính toán vị trí góc dưới bên phải (với lề 20 points)
                        float x = pageSize.Right - qrImage.ScaledWidth - 20;
                        float y = pageSize.Bottom + 20;
                        qrImage.SetAbsolutePosition(x, y);

                        // Lấy nội dung trang để thêm hình ảnh
                        PdfContentByte content = stamper.GetOverContent(i);
                        content.AddImage(qrImage);
                    }

                    // Đóng stamper để lưu thay đổi
                    stamper.Close();
                    reader.Close();

                    // 4. Chuyển đổi PDF đã sửa đổi thành chuỗi Base64
                    tracingService.Trace("Chuyển đổi PDF đã sửa đổi sang base64.");
                    string modifiedPdfBase64 = Convert.ToBase64String(outputStream.ToArray());

                    // 5. Gán kết quả vào OutputParameters
                    context.OutputParameters["modifiedPdfBase64"] = modifiedPdfBase64;
                }
                tracingService.Trace("Hoàn thành Action_InsertQrcodeInPDF.");
            }
            catch (Exception ex)
            {
                tracingService.Trace("Lỗi trong Action_InsertQrcodeInPDF: {0}", ex.ToString());
                throw new InvalidPluginExecutionException("Đã xảy ra lỗi trong quá trình chèn QR code vào PDF.", ex);
            }
        }
    }
}
