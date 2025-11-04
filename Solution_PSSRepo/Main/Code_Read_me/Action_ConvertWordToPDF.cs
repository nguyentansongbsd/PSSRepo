using Microsoft.Xrm.Sdk;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Wordprocessing;
using PdfSharp.Pdf;
using PdfSharp.Drawing;
using PdfSharp.Fonts;
using System.Reflection;
using System.Diagnostics;
using A = DocumentFormat.OpenXml.Drawing;
using DW = DocumentFormat.OpenXml.Drawing.Wordprocessing;

namespace Action_ConvertWordToPDF
{
    public class Action_ConvertWordToPDF : IPlugin
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
                string input = context.InputParameters["input"].ToString();
                byte[] wordBytes = Convert.FromBase64String(input);
                byte[] pdfBytes = ConvertDocxToPdf(wordBytes);
                context.OutputParameters["output"] = Convert.ToBase64String(pdfBytes);
            }
            catch (Exception ex)
            {
                tracingService.Trace($"Critical error: {ex.ToString()}");
                throw new InvalidPluginExecutionException(ex.Message);
            }
        }

        private byte[] ConvertDocxToPdf(byte[] docxBytes)
        {
            using (var docxStream = new MemoryStream(docxBytes))
            using (var pdfDocument = new PdfDocument())
            {
                GlobalFontSettings.FontResolver = new EnhancedFontResolver(tracingService);

                using (var wordDocument = WordprocessingDocument.Open(docxStream, false))
                {
                    var body = wordDocument.MainDocumentPart.Document.Body;
                    var pageSettings = GetPageSettings(wordDocument);
                    var layout = new PdfLayoutManager(pageSettings, tracingService);

                    layout.Initialize(pdfDocument);

                    foreach (var element in body.Elements())
                    {
                        layout.CheckPageBreak(pdfDocument, 40);

                        if (element is Paragraph paragraph)
                        {
                            ProcessParagraph(paragraph, pdfDocument, layout);
                        }
                        else if (element is Table table)
                        {
                            ProcessTable(table, pdfDocument, layout);
                        }
                    }

                    ProcessImages(wordDocument, pdfDocument, layout);
                }

                using (var pdfStream = new MemoryStream())
                {
                    pdfDocument.Save(pdfStream);
                    return pdfStream.ToArray();
                }
            }
        }

        #region Paragraph Processing
        private void ProcessParagraph(Paragraph paragraph, PdfDocument document, PdfLayoutManager layout)
        {
            foreach (var run in paragraph.Elements<Run>())
            {
                var text = GetRunText(run);
                if (string.IsNullOrEmpty(text)) continue;

                var font = GetRunFont(run);
                var size = layout.MeasureText(text, font);

                layout.CheckPageBreak(document, size.Height);

                // Sửa tại đây - dòng 102
                layout.Gfx.DrawString(
                    text,
                    font,
                    XBrushes.Black,
                    new XPoint(layout.MarginLeft, layout.CurrentY), // Thay XRect bằng XPoint
                    GetParagraphAlignment(paragraph)
                );

                layout.AddVerticalSpace(size.Height);
            }
            layout.AddVerticalSpace(10);
        }
        #endregion

        #region Table Processing
        private void ProcessTable(Table table, PdfDocument document, PdfLayoutManager layout)
        {
            var columnWidths = CalculateColumnWidths(table, layout.AvailableWidth);
            const double rowHeight = 20;

            foreach (var row in table.Elements<TableRow>())
            {
                layout.CheckPageBreak(document, rowHeight);

                double currentX = layout.MarginLeft;
                int cellIndex = 0;

                foreach (var cell in row.Elements<TableCell>())
                {
                    var cellContent = GetCellText(cell);
                    var cellSize = layout.MeasureText(cellContent, layout.DefaultFont);

                    // Draw cell border and content
                    layout.Gfx.DrawRectangle(XPens.Black,
                        new XRect(currentX, layout.CurrentY, columnWidths[cellIndex], rowHeight));

                    layout.Gfx.DrawString(cellContent,
                        layout.DefaultFont,
                        XBrushes.Black,
                        new XRect(currentX + 2, layout.CurrentY + 2, columnWidths[cellIndex] - 4, rowHeight - 4),
                        XStringFormats.TopLeft);

                    currentX += columnWidths[cellIndex];
                    cellIndex++;
                }

                layout.AddVerticalSpace(rowHeight);
            }
        }
        #endregion

        #region Image Processing
        private void ProcessImages(WordprocessingDocument wordDocument, PdfDocument document, PdfLayoutManager layout)
        {
            foreach (var imagePart in wordDocument.MainDocumentPart.ImageParts)
            {
                using (var stream = imagePart.GetStream())
                using (var ms = new MemoryStream())
                {
                    stream.CopyTo(ms);
                    ProcessImage(ms.ToArray(), document, layout);
                }
            }
        }

        private void ProcessImage(byte[] imageData, PdfDocument document, PdfLayoutManager layout)
        {
            using (var imgStream = new MemoryStream(imageData))
            using (var xImage = XImage.FromStream(imgStream))
            {
                double scale = Math.Min(
                    layout.AvailableWidth / xImage.PixelWidth,
                    (layout.PageSettings.UsableHeight - layout.CurrentY) / xImage.PixelHeight
                );

                double width = xImage.PixelWidth * scale;
                double height = xImage.PixelHeight * scale;

                layout.CheckPageBreak(document, height);

                layout.Gfx.DrawImage(xImage, layout.MarginLeft, layout.CurrentY, width, height);
                layout.AddVerticalSpace(height + 10);
            }
        }
        #endregion

        #region Helper Methods
        private PageSettings GetPageSettings(WordprocessingDocument doc)
        {
            var sectionProps = doc.MainDocumentPart.Document.Body
                .Descendants<SectionProperties>().FirstOrDefault();

            // Sửa tại đây - dòng 192
            var pageSize = sectionProps?.Descendants<DocumentFormat.OpenXml.Wordprocessing.PageSize>().FirstOrDefault();

            return new PageSettings(
                width: (pageSize?.Width?.Value ?? 8420) / 20.0,  // Giá trị mặc định A4 (842 pt)
                height: (pageSize?.Height?.Value ?? 5950) / 20.0, // Giá trị mặc định A4 (595 pt)
                margin: 40
            );
        }

        private XStringFormat GetParagraphAlignment(Paragraph paragraph)
        {
            var alignment = paragraph.ParagraphProperties?.Justification?.Val;
            if (alignment == JustificationValues.Center)
                return XStringFormats.TopCenter;
            if (alignment == JustificationValues.Right)
                return XStringFormats.TopRight;
            return XStringFormats.TopLeft;
        }

        private string GetRunText(Run run)
        {
            return string.Join("", run.Elements<Text>().Select(t => t.Text));
        }

        private string GetCellText(TableCell cell)
        {
            return string.Join(" ", cell.Elements<Paragraph>().SelectMany(p => p.Elements<Run>())
                .SelectMany(r => r.Elements<Text>().Select(t => t.Text)));
        }

        private double[] CalculateColumnWidths(Table table, double maxWidth)
        {
            var gridCols = table.Descendants<GridColumn>().ToList();
            if (gridCols.Count == 0) return new[] { maxWidth };

            // Tính tổng chiều rộng sau khi chuyển đổi từ string sang double
            double totalWidth = gridCols.Sum(c =>
            {
                if (c.Width?.Value == null) return 0;
                return double.TryParse(c.Width.Value, out double width) ? width : 0;
            });

            // Tính toán tỷ lệ chiều rộng cho từng cột
            return gridCols.Select(c =>
            {
                if (c.Width?.Value == null) return 0;
                return double.TryParse(c.Width.Value, out double width)
                    ? (width / totalWidth * maxWidth)
                    : 0;
            }).ToArray();
        }

        private XFont GetRunFont(Run run)
        {
            var props = run.RunProperties ?? new RunProperties();
            string fontName = props.RunFonts?.Ascii?.Value ?? "Arial";
            double fontSize = 11;

            if (props.FontSize?.Val != null)
                fontSize = double.Parse(props.FontSize.Val) / 2.0;

            var style = XFontStyle.Regular;
            if (props.Bold != null) style |= XFontStyle.Bold;
            if (props.Italic != null) style |= XFontStyle.Italic;

            return new XFont(fontName, fontSize, style);
        }
        #endregion

        #region Support Classes
        private class PageSettings
        {
            public double Width { get; }
            public double Height { get; }
            public double Margin { get; }
            public double UsableWidth => Width - Margin * 2;
            public double UsableHeight => Height - Margin * 2;
            public double MarginLeft => Margin;

            public PageSettings(double width, double height, double margin)
            {
                Width = width;
                Height = height;
                Margin = margin;
            }
        }

        private class PdfLayoutManager
        {
            private readonly PageSettings _settings;
            private readonly ITracingService _tracing;

            public XGraphics Gfx { get; private set; }
            public PdfPage CurrentPage { get; private set; }
            public double CurrentY { get; private set; }
            public XFont DefaultFont => new XFont("Arial", 11);
            public double MarginLeft => _settings.MarginLeft;
            public double AvailableWidth => _settings.UsableWidth;
            public PageSettings PageSettings => _settings;

            public PdfLayoutManager(PageSettings settings, ITracingService tracing)
            {
                _settings = settings;
                _tracing = tracing;
            }

            public void Initialize(PdfDocument document)
            {
                CurrentPage = document.AddPage();
                CurrentPage.Width = _settings.Width;
                CurrentPage.Height = _settings.Height;
                Gfx = XGraphics.FromPdfPage(CurrentPage);
                Reset();
            }

            public void CheckPageBreak(PdfDocument document, double requiredHeight)
            {
                if (CurrentY + requiredHeight > _settings.UsableHeight)
                {
                    CurrentPage = document.AddPage();
                    CurrentPage.Width = _settings.Width;
                    CurrentPage.Height = _settings.Height;
                    Gfx = XGraphics.FromPdfPage(CurrentPage);
                    Reset();
                }
            }

            public void AddVerticalSpace(double height)
            {
                CurrentY += height;
                _tracing.Trace($"New Y position: {CurrentY}");
            }

            public XSize MeasureText(string text, XFont font)
            {
                return Gfx.MeasureString(text, font);
            }

            private void Reset()
            {
                CurrentY = _settings.Margin;
            }
        }

        private class EnhancedFontResolver : IFontResolver
        {
            private readonly Dictionary<string, byte[]> _fontData = new Dictionary<string, byte[]>(StringComparer.OrdinalIgnoreCase);
            private readonly ITracingService _tracing;

            public EnhancedFontResolver(ITracingService tracing)
            {
                _tracing = tracing;
                LoadFontResource("Arial", "ARIAL.TTF");
                LoadFontResource("Arial-Bold", "ARIALBD.TTF");
                LoadFontResource("Arial-Italic", "ARIALI.TTF");
            }

            private void LoadFontResource(string name, string resource)
            {
                try
                {
                    var assembly = Assembly.GetExecutingAssembly();
                    using (var stream = assembly.GetManifestResourceStream($"Action_ConvertWordToPDF.Fonts.{resource}"))
                    {
                        if (stream != null)
                        {
                            using (var ms = new MemoryStream())
                            {
                                stream.CopyTo(ms);
                                _fontData[name] = ms.ToArray();
                                _tracing.Trace($"Loaded font: {name}");
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    _tracing.Trace($"Font load error: {ex.ToString()}");
                }
            }

            public byte[] GetFont(string faceName)
            {
                if (_fontData.TryGetValue(faceName, out var data)) return data;
                return _fontData.TryGetValue("Arial", out data) ? data : throw new KeyNotFoundException(faceName);
            }

            public FontResolverInfo ResolveTypeface(string familyName, bool isBold, bool isItalic)
            {
                var key = $"{familyName}{(isBold ? "-Bold" : "")}{(isItalic ? "-Italic" : "")}";
                if (_fontData.ContainsKey(key)) return new FontResolverInfo(key);

                _tracing.Trace($"Using fallback font for: {key}");
                return new FontResolverInfo("Arial");
            }
        }
        #endregion
    }
}