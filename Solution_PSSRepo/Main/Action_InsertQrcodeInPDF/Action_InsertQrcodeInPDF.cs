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

                // 2. Lấy QR code từ Note (annotation) của entity bằng FetchXML
                if (!context.InputParameters.Contains("targetEntityId") || !(context.InputParameters["targetEntityId"] is string))
                    throw new InvalidPluginExecutionException("Tham số 'targetEntityId' không hợp lệ.");

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

                    Image qrImage = Image.GetInstance(qrBytes);

                    // Kích thước trang A4
                    float pageWidth = PageSize.A4.Width;
                    float pageHeight = PageSize.A4.Height;

                    // Lề 20 points
                    float margin = 20f;
                    qrImage.ScaleAbsolute(pageWidth - 2 * margin, pageHeight - 2 * margin);

                    // Thêm một trang mới vào cuối PDF
                    stamper.InsertPage(reader.NumberOfPages + 1, PageSize.A4);

                    // Đặt vị trí QR code tại góc dưới bên trái (sau lề)
                    qrImage.SetAbsolutePosition(margin, margin);

                    // Lấy nội dung trang mới để chèn QR
                    PdfContentByte content = stamper.GetOverContent(reader.NumberOfPages + 1);
                    content.AddImage(qrImage);

                    stamper.Close();
                    reader.Close();

                    string modifiedPdfBase64 = Convert.ToBase64String(outputStream.ToArray());
                    context.OutputParameters["modifiedPdfBase64"] = modifiedPdfBase64;
                }

                tracingService.Trace("Hoàn thành việc thêm QR code trên trang mới.");


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
