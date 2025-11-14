using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Query;
using iTextSharp.text;
using iTextSharp.text.pdf;
using System;
using System.IO;

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
            service = ((IOrganizationServiceFactory)serviceProvider.GetService(typeof(IOrganizationServiceFactory)))
                .CreateOrganizationService(context.UserId);
            tracingService = (ITracingService)serviceProvider.GetService(typeof(ITracingService));

            try
            {
                tracingService.Trace("Bắt đầu Action_InsertQrcodeInPDF.");

                // 1. Lấy PDF base64 từ InputParameters
                if (!context.InputParameters.Contains("pdfBase64") || !(context.InputParameters["pdfBase64"] is string))
                    throw new InvalidPluginExecutionException("Tham số 'pdfBase64' không hợp lệ.");

                string pdfBase64 = context.InputParameters["pdfBase64"].ToString();

                //// 2. Lấy QR code từ Note (annotation) của entity bằng FetchXML
                //if (!context.InputParameters.Contains("targetEntityId") || !(context.InputParameters["targetEntityId"] is string))
                //    throw new InvalidPluginExecutionException("Tham số 'targetEntityId' không hợp lệ.");

                Guid objectId = new Guid("DC514F48-0EAB-F011-BBD2-6045BD1CFFFF");

                string fetchXml = $@"
                <fetch top='1'>
                  <entity name='annotation'>
                    <attribute name='subject' />
                    <attribute name='filename' />
                    <attribute name='documentbody' />
                    <attribute name='createdon' />
                    <attribute name='annotationid' />
                    <filter type='and'>
                      <condition attribute='objectid' operator='eq' value='{objectId}' />
                      <condition attribute='isdocument' operator='eq' value='1' />
                      <condition attribute='subject' operator='eq' value='QR Code' />
                    </filter>
                    <order attribute='createdon' descending='true' />
                  </entity>
                </fetch>";

                EntityCollection notes = service.RetrieveMultiple(new FetchExpression(fetchXml));

                if (notes.Entities.Count == 0)
                {
                    tracingService.Trace("Không tìm thấy note QR Code nào.");
                    context.OutputParameters["modifiedPdfBase64"] = pdfBase64;
                    return;
                }
                Entity note = notes.Entities[0];
                string qrBase64 = note.GetAttributeValue<string>("documentbody");

                if (string.IsNullOrEmpty(qrBase64))
                {
                    tracingService.Trace("Note QR Code không có nội dung documentbody.");
                    context.OutputParameters["modifiedPdfBase64"] = pdfBase64;
                    return;
                }

                tracingService.Trace("Giải mã chuỗi QR code base64.");
                byte[] pdfBytes = Convert.FromBase64String(pdfBase64);
                byte[] qrBytes = Convert.FromBase64String(qrBase64);

                // 3. Chèn QR code vào PDF bằng iTextSharp
                tracingService.Trace("Bắt đầu xử lý PDF và chèn QR code trên trang mới.");

                using (MemoryStream outputStream = new MemoryStream())
                {
                    PdfReader reader = new PdfReader(pdfBytes);
                    PdfStamper stamper = new PdfStamper(reader, outputStream);

                    // Tạo đối tượng hình ảnh từ QR code
                    Image qrImage = Image.GetInstance(qrBytes);

                    // Thiết lập kích thước cho QR code (ví dụ: 70x70 points)
                    qrImage.ScaleAbsolute(700f, 700f);

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

                    tracingService.Trace("Hoàn thành việc thêm QR code trên trang mới.");

                    stamper.Close();
                    reader.Close();

                    // 4. Chuyển đổi PDF đã sửa đổi thành chuỗi Base64
                    tracingService.Trace("Chuyển đổi PDF đã sửa đổi sang base64.");
                    string modifiedPdfBase64 = Convert.ToBase64String(outputStream.ToArray());

                    // 5. Gán kết quả vào OutputParameters
                    context.OutputParameters["modifiedPdfBase64"] = modifiedPdfBase64;
                    tracingService.Trace("Hoàn thành Action_InsertQrcodeInPDF.");
                }
            }
            catch (Exception ex)
            {
                tracingService.Trace("Lỗi trong Action_InsertQrcodeInPDF: {0}", ex.ToString());
                throw new InvalidPluginExecutionException("Đã xảy ra lỗi trong quá trình chèn QR code vào PDF.", ex);
            }
        }
    }
}
