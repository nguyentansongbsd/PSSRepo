# Phân tích mã nguồn: Action_ConvertWordToPDF.cs

## Tổng quan

Tệp mã nguồn `Action_ConvertWordToPDF.cs` định nghĩa một Plugin (Custom Action) cho nền tảng Microsoft Dynamics 365/Power Platform. Mục đích chính của Plugin này là nhận một tài liệu Microsoft Word (dưới dạng mảng byte Base64) và chuyển đổi nó thành tài liệu PDF, sau đó trả về tài liệu PDF đó cũng dưới dạng mảng byte Base64.

Plugin sử dụng thư viện `DocumentFormat.OpenXml` để phân tích cấu trúc tài liệu Word (.docx) và thư viện `PdfSharp` để tạo và vẽ nội dung lên tài liệu PDF. Nó bao gồm các logic phức tạp để xử lý đoạn văn bản, bảng, hình ảnh, và quản lý bố cục trang (ngắt trang, căn chỉnh, font chữ).

## Chi tiết các Hàm (Functions/Methods)

### Execute(IServiceProvider serviceProvider)

**Chức năng tổng quát:**
Đây là điểm vào chính (entry point) của Plugin Dynamics 365, chịu trách nhiệm khởi tạo các dịch vụ cần thiết, nhận dữ liệu đầu vào, thực hiện chuyển đổi, và trả về kết quả.

**Logic nghiệp vụ chi tiết:**
1.  **Khởi tạo Dịch vụ:** Hàm lấy các dịch vụ cốt lõi của Dynamics 365: `IPluginExecutionContext` (context), `IOrganizationService` (service), và `ITracingService` (tracingService) để ghi log.
2.  **Xử lý Lỗi (Try-Catch):** Toàn bộ logic nghiệp vụ được đặt trong khối `try-catch` để đảm bảo mọi lỗi đều được ghi lại bằng `tracingService`.
3.  **Đọc Đầu vào:** Lấy giá trị của tham số đầu vào có tên là `"input"` từ `context.InputParameters`. Giá trị này được kỳ vọng là một chuỗi Base64 đại diện cho tệp DOCX.
4.  **Giải mã DOCX:** Chuỗi Base64 đầu vào được chuyển đổi thành mảng byte (`wordBytes`) bằng `Convert.FromBase64String`.
5.  **Thực hiện Chuyển đổi:** Gọi hàm nội bộ `ConvertDocxToPdf(wordBytes)` để thực hiện quá trình chuyển đổi chính, nhận lại mảng byte của tệp PDF (`pdfBytes`).
6.  **Gán Đầu ra:** Mảng byte PDF (`pdfBytes`) được chuyển đổi ngược lại thành chuỗi Base64 và gán vào tham số đầu ra có tên là `"output"` trong `context.OutputParameters`.
7.  **Xử lý Ngoại lệ:** Nếu có bất kỳ ngoại lệ nào xảy ra, lỗi được ghi vào `tracingService` và một `InvalidPluginExecutionException` được ném ra, khiến giao dịch Dynamics 365 bị hủy bỏ.

### ConvertDocxToPdf(byte[] docxBytes)

**Chức năng tổng quát:**
Đây là hàm cốt lõi thực hiện việc đọc cấu trúc tài liệu Word và vẽ nội dung tương ứng lên tài liệu PDF mới.

**Logic nghiệp vụ chi tiết:**
1.  **Khởi tạo Stream và Tài liệu:** Tạo `MemoryStream` từ `docxBytes` và khởi tạo một đối tượng `PdfDocument` mới.
2.  **Thiết lập Font:** Thiết lập `GlobalFontSettings.FontResolver` sử dụng `EnhancedFontResolver` tùy chỉnh. Điều này rất quan trọng để đảm bảo rằng các font được sử dụng trong PDFSharp có thể được nhúng hoặc tìm thấy, đặc biệt trong môi trường hạn chế như Plugin.
3.  **Mở Tài liệu Word:** Mở tài liệu Word bằng `WordprocessingDocument.Open(docxStream, false)`.
4.  **Lấy Cài đặt Trang:** Gọi `GetPageSettings` để xác định kích thước và lề trang PDF dựa trên cài đặt trong tài liệu Word.
5.  **Khởi tạo Layout Manager:** Tạo và khởi tạo đối tượng `PdfLayoutManager` (sử dụng `pageSettings`) để quản lý vị trí vẽ, lề, và logic ngắt trang.
6.  **Duyệt qua Nội dung:** Lặp qua tất cả các phần tử cấp cao nhất (`element`) trong phần thân tài liệu Word (`body.Elements()`):
    *   Gọi `layout.CheckPageBreak(pdfDocument, 40)` để kiểm tra xem có cần ngắt trang trước khi vẽ phần tử mới không (sử dụng chiều cao tối thiểu là 40).
    *   **Xử lý Đoạn văn:** Nếu phần tử là `Paragraph`, gọi `ProcessParagraph`.
    *   **Xử lý Bảng:** Nếu phần tử là `Table`, gọi `ProcessTable`.
7.  **Xử lý Hình ảnh:** Sau khi duyệt qua nội dung chính, gọi `ProcessImages` để xử lý các hình ảnh được nhúng trong tài liệu Word.
8.  **Lưu và Trả về:** Lưu `pdfDocument` vào một `MemoryStream` mới (`pdfStream`) và trả về mảng byte của stream đó.

### ProcessParagraph(Paragraph paragraph, PdfDocument document, PdfLayoutManager layout)

**Chức năng tổng quát:**
Vẽ nội dung văn bản của một đoạn văn bản Word lên trang PDF hiện tại, xử lý định dạng và căn chỉnh.

**Logic nghiệp vụ chi tiết:**
1.  **Duyệt qua Runs:** Lặp qua tất cả các `Run` (một chuỗi văn bản có cùng định dạng) trong đoạn văn bản.
2.  **Lấy Dữ liệu:** Lấy nội dung văn bản (`text`) bằng `GetRunText` và thông tin font (`font`) bằng `GetRunFont`.
3.  **Đo Kích thước:** Đo kích thước (`size`) mà văn bản sẽ chiếm trên PDF bằng `layout.MeasureText`.
4.  **Kiểm tra Ngắt trang:** Gọi `layout.CheckPageBreak(document, size.Height)` để đảm bảo có đủ không gian cho văn bản này.
5.  **Vẽ Văn bản:** Sử dụng `layout.Gfx.DrawString` để vẽ văn bản:
    *   Sử dụng màu đen (`XBrushes.Black`).
    *   Vị trí vẽ được xác định bởi lề trái (`layout.MarginLeft`) và vị trí dọc hiện tại (`layout.CurrentY`).
    *   Căn chỉnh văn bản được xác định bằng cách gọi `GetParagraphAlignment`.
6.  **Cập nhật Vị trí:** Thêm khoảng trống dọc bằng chiều cao của văn bản (`layout.AddVerticalSpace(size.Height)`).
7.  **Khoảng cách Đoạn:** Sau khi xử lý tất cả các `Run`, thêm một khoảng trống dọc cố định (10 đơn vị) để tạo khoảng cách giữa các đoạn văn.

### ProcessTable(Table table, PdfDocument document, PdfLayoutManager layout)

**Chức năng tổng quát:**
Vẽ một bảng Word lên PDF, bao gồm việc tính toán chiều rộng cột và vẽ các ô cùng nội dung.

**Logic nghiệp vụ chi tiết:**
1.  **Tính toán Chiều rộng Cột:** Gọi `CalculateColumnWidths` để xác định chiều rộng thực tế của từng cột trong bảng, dựa trên chiều rộng khả dụng (`layout.AvailableWidth`).
2.  **Chiều cao Hàng:** Định nghĩa chiều cao hàng cố định (`rowHeight = 20`).
3.  **Duyệt qua Hàng:** Lặp qua từng `TableRow` trong bảng.
4.  **Kiểm tra Ngắt trang:** Gọi `layout.CheckPageBreak(document, rowHeight)` để đảm bảo hàng này vừa với trang hiện tại.
5.  **Duyệt qua Ô:** Lặp qua từng `TableCell` trong hàng:
    *   Lấy nội dung văn bản của ô (`cellContent`) bằng `GetCellText`.
    *   **Vẽ Đường viền:** Vẽ hình chữ nhật (`layout.Gfx.DrawRectangle`) bằng bút đen (`XPens.Black`) để tạo đường viền ô, sử dụng vị trí `currentX`, `layout.CurrentY` và chiều rộng/chiều cao đã tính.
    *   **Vẽ Nội dung:** Vẽ nội dung văn bản (`layout.Gfx.DrawString`) bên trong ô, với một khoảng đệm nhỏ (2 đơn vị) và căn chỉnh `TopLeft`.
    *   Cập nhật vị trí ngang (`currentX`) để chuẩn bị vẽ ô tiếp theo.
6.  **Cập nhật Vị trí:** Sau khi hoàn thành một hàng, thêm khoảng trống dọc bằng chiều cao hàng (`layout.AddVerticalSpace(rowHeight)`).

### ProcessImages(WordprocessingDocument wordDocument, PdfDocument document, PdfLayoutManager layout)

**Chức năng tổng quát:**
Duyệt qua tất cả các phần hình ảnh được nhúng trong tài liệu Word và xử lý chúng để vẽ lên PDF.

**Logic nghiệp vụ chi tiết:**
1.  **Duyệt Image Parts:** Lặp qua tất cả các `ImagePart` trong `MainDocumentPart` của tài liệu Word.
2.  **Đọc Dữ liệu:** Đối với mỗi `ImagePart`, mở stream, sao chép nội dung vào một `MemoryStream` và chuyển đổi thành mảng byte.
3.  **Xử lý Từng Hình ảnh:** Gọi hàm `ProcessImage` với mảng byte hình ảnh để vẽ nó lên PDF.

### ProcessImage(byte[] imageData, PdfDocument document, PdfLayoutManager layout)

**Chức năng tổng quát:**
Tải và vẽ một hình ảnh lên PDF, đảm bảo hình ảnh được điều chỉnh tỷ lệ để vừa với trang và xử lý ngắt trang nếu cần.

**Logic nghiệp vụ chi tiết:**
1.  **Tải Hình ảnh:** Tải hình ảnh từ `imageData` thành đối tượng `XImage` của PdfSharp.
2.  **Tính toán Tỷ lệ:** Tính toán tỷ lệ thu phóng (`scale`) cần thiết. Tỷ lệ này được xác định bởi giá trị nhỏ nhất giữa:
    *   Tỷ lệ để vừa với chiều rộng khả dụng (`layout.AvailableWidth`).
    *   Tỷ lệ để vừa với chiều cao còn lại trên trang (`layout.PageSettings.UsableHeight - layout.CurrentY`).
3.  **Tính toán Kích thước:** Tính toán chiều rộng và chiều cao cuối cùng (`width`, `height`) sau khi áp dụng tỷ lệ.
4.  **Kiểm tra Ngắt trang:** Gọi `layout.CheckPageBreak(document, height)` để đảm bảo hình ảnh vừa với trang.
5.  **Vẽ Hình ảnh:** Sử dụng `layout.Gfx.DrawImage` để vẽ hình ảnh tại vị trí lề trái (`layout.MarginLeft`) và vị trí dọc hiện tại (`layout.CurrentY`) với kích thước đã điều chỉnh.
6.  **Cập nhật Vị trí:** Thêm khoảng trống dọc bằng chiều cao hình ảnh cộng thêm 10 đơn vị đệm (`layout.AddVerticalSpace(height + 10)`).

### GetPageSettings(WordprocessingDocument doc)

**Chức năng tổng quát:**
Trích xuất cài đặt kích thước trang và lề từ tài liệu Word.

**Logic nghiệp vụ chi tiết:**
1.  **Tìm Section Properties:** Tìm kiếm đối tượng `SectionProperties` đầu tiên trong phần thân tài liệu.
2.  **Tìm Page Size:** Tìm kiếm đối tượng `PageSize` bên trong `SectionProperties`.
3.  **Tính toán Kích thước:**
    *   Nếu tìm thấy `PageSize`, sử dụng giá trị `Width` và `Height` (được lưu trữ dưới dạng TWIPs - 1/20 điểm) và chia cho 20.0 để chuyển đổi sang đơn vị Point (pt).
    *   Nếu không tìm thấy, sử dụng giá trị mặc định cho khổ giấy A4 (8420 TWIPs cho chiều rộng và 5950 TWIPs cho chiều cao).
4.  **Trả về:** Trả về đối tượng `PageSettings` mới với kích thước trang đã tính và lề cố định là 40 pt.

### GetParagraphAlignment(Paragraph paragraph)

**Chức năng tổng quát:**
Xác định căn chỉnh (trái, giữa, phải) của một đoạn văn bản Word và ánh xạ nó sang định dạng căn chỉnh của PdfSharp (`XStringFormat`).

**Logic nghiệp vụ chi tiết:**
1.  **Trích xuất Giá trị:** Lấy giá trị căn chỉnh (`JustificationValues`) từ thuộc tính đoạn văn bản (`ParagraphProperties`).
2.  **Ánh xạ:**
    *   Nếu giá trị là `JustificationValues.Center`, trả về `XStringFormats.TopCenter`.
    *   Nếu giá trị là `JustificationValues.Right`, trả về `XStringFormats.TopRight`.
    *   Mọi trường hợp khác (bao gồm cả căn lề trái hoặc không xác định), trả về `XStringFormats.TopLeft`.

### GetRunText(Run run)

**Chức năng tổng quát:**
Trích xuất nội dung văn bản thuần túy từ một đối tượng `Run` của Word.

**Logic nghiệp vụ chi tiết:**
1.  Tìm tất cả các phần tử `Text` bên trong `Run`.
2.  Chọn giá trị văn bản (`t.Text`) của từng phần tử.
3.  Nối các chuỗi văn bản lại với nhau (sử dụng `string.Join("", ...)`).

### GetCellText(TableCell cell)

**Chức năng tổng quát:**
Trích xuất nội dung văn bản thuần túy từ một ô bảng (TableCell).

**Logic nghiệp vụ chi tiết:**
1.  Duyệt qua tất cả các `Paragraph` trong `TableCell`.
2.  Duyệt qua tất cả các `Run` trong mỗi `Paragraph`.
3.  Duyệt qua tất cả các phần tử `Text` trong mỗi `Run`.
4.  Nối tất cả các chuỗi văn bản lại với nhau, sử dụng dấu cách (" ") làm ký tự phân tách giữa các phần tử văn bản.

### CalculateColumnWidths(Table table, double maxWidth)

**Chức năng tổng quát:**
Tính toán chiều rộng thực tế (theo đơn vị Point) của từng cột trong bảng, dựa trên tổng chiều rộng khả dụng.

**Logic nghiệp vụ chi tiết:**
1.  **Lấy Grid Columns:** Lấy danh sách các đối tượng `GridColumn` (định nghĩa chiều rộng cột) từ bảng.
2.  **Xử lý Mặc định:** Nếu không có `GridColumn` nào được định nghĩa, giả định chỉ có một cột và trả về chiều rộng tối đa (`maxWidth`).
3.  **Tính Tổng Chiều rộng:** Tính tổng chiều rộng được định nghĩa (dưới dạng TWIPs) của tất cả các cột.
4.  **Tính Tỷ lệ:** Lặp qua từng `GridColumn` và tính toán tỷ lệ chiều rộng của cột đó so với tổng chiều rộng.
5.  **Áp dụng Chiều rộng Tối đa:** Nhân tỷ lệ này với `maxWidth` để có được chiều rộng cột cuối cùng theo đơn vị Point.
6.  Trả về mảng chứa chiều rộng của từng cột.

### GetRunFont(Run run)

**Chức năng tổng quát:**
Xác định tên font, kích thước và kiểu (in đậm, in nghiêng) của một `Run` và tạo đối tượng `XFont` của PdfSharp.

**Logic nghiệp vụ chi tiết:**
1.  **Lấy Thuộc tính:** Lấy đối tượng `RunProperties` (hoặc tạo mới nếu không có).
2.  **Tên Font:** Lấy tên font từ thuộc tính `RunFonts.Ascii`. Mặc định là "Arial".
3.  **Kích thước Font:**
    *   Lấy giá trị kích thước từ `FontSize.Val`.
    *   Kích thước trong Word được lưu dưới dạng nửa Point (half-point). Do đó, giá trị được chuyển đổi thành `double` và chia cho 2.0. Mặc định là 11 pt.
4.  **Kiểu Font:** Khởi tạo kiểu font là `Regular`.
    *   Nếu thuộc tính `Bold` tồn tại, thêm kiểu `XFontStyle.Bold`.
    *   Nếu thuộc tính `Italic` tồn tại, thêm kiểu `XFontStyle.Italic`.
5.  **Trả về:** Tạo và trả về đối tượng `XFont` mới với tên, kích thước và kiểu đã xác định.