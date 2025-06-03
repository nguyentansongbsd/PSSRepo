using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using DocumentFormat.OpenXml;
using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Wordprocessing;
using Microsoft.Xrm.Sdk;
using Newtonsoft.Json.Linq;

namespace Action_MappingParamToWord
{
    public class RootObject
    {
        public List<Dictionary<string, string>> lstParamValue { get; set; }
    }
    public class Action_MappingParamToWord : IPlugin
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
            string base64Input = context.InputParameters["base64"].ToString();
            string jsonInput = context.InputParameters["lstParamValue"].ToString();
            PrintContentControlsFromBase64(base64Input, jsonInput);
            var rs = ReplaceByRemovingControls(base64Input, jsonInput);
            rs = ReplaceTextInDocument(rs, jsonInput);

            //AnalyzeTemplate(base64Input);
            context.OutputParameters["result"] = rs;

        }
        private string ReplaceTextInDocument(string base64Word, string jsonData)
        {
            try
            {
                byte[] wordBytes = Convert.FromBase64String(base64Word);
                var jsonObject = JObject.Parse(jsonData);

                if (jsonObject["lstParamValue"] == null)
                    throw new ArgumentException("JSON does not contain 'lstParamValue'.");

                var replacements = jsonObject["lstParamValue"]
                    .SelectMany(j => ((JObject)j).Properties())
                    .ToDictionary(p => p.Name, p => p.Value?.ToString() ?? "");

                using (var stream = new MemoryStream())
                {
                    stream.Write(wordBytes, 0, wordBytes.Length);

                    using (var wordDoc = WordprocessingDocument.Open(stream, true))
                    {
                        var body = wordDoc.MainDocumentPart.Document.Body;

                        // Duyệt qua tất cả các đoạn văn bản trong tài liệu
                        foreach (var textElement in body.Descendants<Text>().ToList())
                        {
                            var textValue = textElement.Text;
                            tracingService.Trace($"Processing text: '{textValue}'");
                            // Kiểm tra xem đoạn văn bản có khớp với key trong dictionary không
                            if (replacements.TryGetValue(textValue, out var replacementValue))
                            {
                                tracingService.Trace($"Replacing text: '{textValue}' with value: '{replacementValue}'");

                                // Thay thế đoạn văn bản
                                textElement.Text = replacementValue;
                                if (replacementValue.Contains("\n"))
                                {
                                    // Xử lý chuỗi có xuống dòng
                                    InsertTextWithLineBreaks(textElement, replacementValue, wordDoc);
                                }
                                else
                                {
                                    // Nếu không có xuống dòng, thực hiện thay thế thông thường
                                    textElement.Text = replacementValue;
                                }
                            }
                        }

                        // Duyệt qua tất cả các bảng trong tài liệu
                        foreach (var table in body.Descendants<Table>())
                        {
                            foreach (var row in table.Descendants<TableRow>())
                            {
                                foreach (var cell in row.Descendants<TableCell>())
                                {
                                    // Duyệt qua tất cả các đoạn văn bản trong ô
                                    foreach (var textElement in cell.Descendants<Text>().ToList())
                                    {
                                        var textValue = textElement.Text;

                                        // Kiểm tra xem đoạn văn bản có khớp với key trong dictionary không
                                        if (replacements.TryGetValue(textValue, out var replacementValue))
                                        {
                                            tracingService.Trace($"Replacing text in cell: '{textValue}' with value: '{replacementValue}'");

                                            // Thay thế đoạn văn bản
                                            textElement.Text = replacementValue;
                                            if (replacementValue.Contains("\n"))
                                            {
                                                // Xử lý chuỗi có xuống dòng
                                                InsertTextWithLineBreaks(textElement, replacementValue, wordDoc);
                                            }
                                            else
                                            {
                                                // Nếu không có xuống dòng, thực hiện thay thế thông thường
                                                textElement.Text = replacementValue;
                                            }
                                        }
                                    }
                                }
                            }
                        }

                        wordDoc.Save();
                       
                    }
                    return Convert.ToBase64String(stream.ToArray());
                }
            }
            catch (Exception ex)
            {
                tracingService.Trace($"Error: {ex.Message}");
                throw;
            }
        }
        private void InsertTextWithLineBreaks(Text textElement, string replacementValue, WordprocessingDocument wordDoc)
        {
            // Tách chuỗi thành các dòng
            string[] lines = replacementValue.Split(new[] { "\r\n", "\r", "\n" }, StringSplitOptions.None);

            if (lines.Length == 1)
            {
                // Nếu chỉ có một dòng, thực hiện thay thế thông thường
                textElement.Text = replacementValue;
                return;
            }

            // Lấy phần tử cha (Run) của Text
            var run = textElement.Parent as Run;
            if (run == null)
            {
                // Nếu không có phần tử cha, chỉ thay thế nội dung
                textElement.Text = replacementValue.Replace("\n", " ").Replace("\r", "");
                return;
            }

            // Xóa Text hiện tại
            textElement.Remove();

            // Thêm dòng đầu tiên
            run.AppendChild(new Text(lines[0]));

            // Thêm các dòng còn lại với break line
            for (int i = 1; i < lines.Length; i++)
            {
                // Thêm break line
                run.AppendChild(new Break());
                // Thêm text mới
                run.AppendChild(new Text(lines[i]));
            }
        }
        public void PrintContentControlsFromBase64(string base64Word,string jsonInput)
        {
            try
            {
                var jsonObject = JObject.Parse(jsonInput);
                // Chuyển base64 thành byte[] và mở tài liệu Word
                byte[] wordBytes = Convert.FromBase64String(base64Word);

                using (var stream = new MemoryStream(wordBytes))
                using (var wordDoc = WordprocessingDocument.Open(stream, false)) // Mở ở chế độ read-only
                {
                    var body = wordDoc.MainDocumentPart?.Document?.Body;

                    if (body == null)
                    {
                        tracingService.Trace("Không tìm thấy nội dung trong tài liệu Word.");
                        return;
                    }

                    // Lấy tất cả Content Controls
                    var allControls = body.Descendants<SdtElement>();
                    tracingService.Trace($"Tìm thấy {allControls.Count()} Content Controls trong tài liệu:");
                    var replacements = jsonObject["lstParamValue"]
                    .SelectMany(j => ((JObject)j).Properties())
                    .ToDictionary(p => p.Name, p => p.Value?.ToString() ?? "");
                    // Duyệt qua từng control và in thông tin
                    foreach (var control in allControls)
                    {
                        var alias = control.SdtProperties?.GetFirstChild<SdtAlias>()?.Val?.Value ?? "Không có Alias";
                        var tag = control.SdtProperties?.GetFirstChild<Tag>()?.Val?.Value ?? "Không có Tag";
                        var controlType = control.GetType().Name; // Loại control (SdtRun, SdtBlock, v.v.)
                        if (replacements.TryGetValue(control.InnerText,out var value))
                        {
                            tracingService.Trace($"- LocalName: {control.LocalName}");
                            tracingService.Trace($"- XmlQualifiedName: {control.XmlQualifiedName}");
                            tracingService.Trace($"- InnerXml: {control.InnerXml}");
                            tracingService.Trace($"- InnerText: {control.InnerText}");
                            tracingService.Trace($"- Alias: {alias}");
                            tracingService.Trace($"  Tag: {tag}");
                            tracingService.Trace($"  Loại Control: {controlType}");
                            tracingService.Trace($"  Vị trí: {control.Parent?.GetType().Name}"); // Paragraph, TableCell, v.v.
                            tracingService.Trace("----------------------------------");
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                tracingService.Trace($"Lỗi khi đọc tài liệu Word: {ex.Message}");
            }
        }
        public string ReplaceByRemovingControls(string base64Word, string jsonData)
        {
            try
            {
                byte[] wordBytes = Convert.FromBase64String(base64Word);
                var jsonObject = JObject.Parse(jsonData);

                if (jsonObject["lstParamValue"] == null)
                    throw new ArgumentException("JSON does not contain 'lstParamValue'.");

                var replacements = jsonObject["lstParamValue"]
                    .SelectMany(j => ((JObject)j).Properties())
                    .ToDictionary(p => p.Name, p => p.Value?.ToString() ?? "");

                using (var stream = new MemoryStream())
                {
                    stream.Write(wordBytes, 0, wordBytes.Length);

                    using (var wordDoc = WordprocessingDocument.Open(stream, true))
                    {
                        var body = wordDoc.MainDocumentPart.Document.Body;

                        foreach (var control in body.Descendants<SdtElement>().ToList())
                        {
                            var innerText = control.InnerText;
                            if (innerText != null && replacements.TryGetValue(innerText, out var value))
                            {
                                tracingService.Trace($"Replacing control: {innerText} with value: {value}");

                                if (control is SdtCell sdtCell)
                                {
                                    ReplaceSdtCell(sdtCell, value);
                                }
                                else if (control is SdtRun sdtRun)
                                {
                                    ReplaceSdtRun(sdtRun, value);
                                }
                                else
                                {
                                    ReplaceGenericControl(control, value);
                                }
                            }
                        }
                        wordDoc.Save();
                    }
                    return Convert.ToBase64String(stream.ToArray());
                }
            }
            catch (Exception ex)
            {
                tracingService.Trace($"Error: {ex.Message}");
                throw;
            }
        }

        private void ReplaceSdtCell(SdtCell sdtCell, string value)
        {
            var parentRow = sdtCell.Parent as TableRow;
            if (parentRow == null) return;

            // Lấy các thuộc tính định dạng từ SdtCell
            var cellProperties = sdtCell.Elements<TableCellProperties>().FirstOrDefault();

            // Tạo cell mới
            var newCell = new TableCell();

            // Thêm properties nếu tồn tại
            if (cellProperties != null)
            {
                newCell.Append(cellProperties.CloneNode(true));
            }

            // Thêm nội dung
            newCell.Append(new Paragraph(new Run(new Text(value))));

            // Thay thế
            parentRow.InsertAfter(newCell, sdtCell);
            sdtCell.Remove();
        }

        private void ReplaceSdtRun(SdtRun sdtRun, string value)
        {
            var parent = sdtRun.Parent;
            if (parent == null) return;

            // Lấy RunProperties từ SdtRun (nếu có)
            var runProperties = sdtRun.Descendants<RunProperties>().FirstOrDefault();

            // Tạo run mới
            var newRun = new Run();

            // Thêm properties nếu tồn tại
            if (runProperties != null)
            {
                newRun.RunProperties = (RunProperties)runProperties.CloneNode(true);
            }

            // Thêm text
            newRun.Append(new Text(value));

            // Thay thế
            if (parent is Paragraph paragraph)
            {
                paragraph.InsertAfter(newRun, sdtRun);
            }
            else
            {
                parent.InsertAfter(newRun, sdtRun);
            }

            sdtRun.Remove();
        }

        private void ReplaceGenericControl(SdtElement control, string value)
        {
            var parent = control.Parent;
            if (parent == null) return;

            OpenXmlElement newElement;

            if (parent is TableRow tableRow)
            {
                newElement = new TableCell(new Paragraph(new Run(new Text(value))));
                tableRow.InsertAfter(newElement, control);
            }
            else if (parent is Paragraph paragraph)
            {
                var newRun = new Run(new Text(value));
                paragraph.InsertAfter(newRun, control);
            }
            else
            {
                newElement = new Paragraph(new Run(new Text(value)));
                parent.InsertAfter(newElement, control);
            }

            control.Remove();
        }

     
        public void AnalyzeTemplate(string base64Word)
        {
            byte[] wordBytes = Convert.FromBase64String(base64Word);

            using (var stream = new MemoryStream(wordBytes))
            using (var wordDoc = WordprocessingDocument.Open(stream, false)) // Read-only mode
            {
                var body = wordDoc.MainDocumentPart.Document.Body;

                // Tìm tất cả các đoạn text chứa pattern placeholder
                var allTexts = body.Descendants<Text>()
                                  .Where(t => t.Text.Contains("[") && t.Text.Contains("]"))
                                  .ToList();

                tracingService.Trace($"Found {allTexts.Count} potential placeholders:");

                foreach (var text in allTexts)
                {
                    tracingService.Trace($"- Text: '{text.Text.Trim()}'");
                    tracingService.Trace($"  Parent element: {text.Parent.GetType().Name}");

                    // Kiểm tra xem có phải là Content Control không
                    var sdtElement = text.Ancestors<SdtElement>().FirstOrDefault();
                    if (sdtElement != null)
                    {
                        var alias = sdtElement.SdtProperties?.GetFirstChild<SdtAlias>()?.Val?.Value;
                        tracingService.Trace($"  Content Control with Alias: '{alias}'");
                    }
                }
            }
        }

        private void ReplaceContentWithStyle(SdtElement control, string newValue)
        {
            tracingService.Trace("newValue:" + newValue);
            tracingService.Trace("control:" + control.InnerText);
            // Tìm phần tử chứa nội dung của Control
            var content = control.Elements().FirstOrDefault(e =>
                e is SdtContentRun || e is SdtContentBlock || e is SdtContentCell
            );
            
            if (content == null) return;

            // Lấy Run hoặc Paragraph gốc (sửa lỗi CS0019)
            OpenXmlElement oldContent = content.Descendants<Run>().FirstOrDefault();
            if (oldContent == null)
            {
                oldContent = content.Descendants<Paragraph>().FirstOrDefault();
            }

            // Xóa nội dung cũ
            content.RemoveAllChildren();

            // Thêm nội dung mới với định dạng gốc
            if (oldContent is Run oldRun)
            {
                tracingService.Trace("3");

                var newRun = new Run(new Text("demo thôi ádasdasdasdsadasdasd"));

                content.AppendChild(newRun);
            }
            else if (oldContent is Paragraph oldPara)
            {
                tracingService.Trace("2");

                var newPara = new Paragraph(
                    new ParagraphProperties(),
                    new Run(new Text(newValue))
                );
                content.AppendChild(newPara);
            }
            else
            {
                tracingService.Trace("1");
                // Trường hợp không có định dạng
                content.AppendChild(new Run(new Text(newValue)));
            }
        }
        private void UpdateTextInsideControl(SdtElement control, string newValue)
        {

            tracingService.Trace("newValue:" + newValue);
            tracingService.Trace("control:" + control.InnerText);
            OpenXmlElement newControl;

            if (control is SdtBlock)
                newControl = CreateNewSdtBlock(control, newValue);
            else if (control is SdtRun)
                newControl = CreateNewSdtRun(control, newValue);
            else if (control is SdtRow)
                newControl = CreateNewSdtRow(control, newValue);
            else
                newControl = CreateNewSdtBlock(control, newValue); // Mặc định

            // Thay thế control cũ bằng control mới
            control.Parent.InsertAfter(newControl, control);
            control.Remove();

        }
        private SdtBlock CreateNewSdtBlock(SdtElement control, string value)
        {
            return new SdtBlock(
               control.SdtProperties.CloneNode(true),
                new SdtContentBlock(
                    new Paragraph(
                        new Run(
                            new Text(value)
                        )
                    )
                )
            );
        }

        private SdtRun CreateNewSdtRun(SdtElement control, string value)
        {
            return new SdtRun(
               control.SdtProperties.CloneNode(true),
                new SdtContentRun(
                    new Run(
                        new Text(value)
                    )
                )
            );
        }

        private SdtRow CreateNewSdtRow(SdtElement control, string value)
        {
            return new SdtRow(
               control.SdtProperties.CloneNode(true),
                new SdtContentRow(
                    new TableRow(
                        new TableCell(
                            new Paragraph(
                                new Run(
                                    new Text(value)
                                )
                            )
                        )
                    )
                )
            );
        }
    }
}


