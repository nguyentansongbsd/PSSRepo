# Phân tích mã nguồn: Action_MappingParamToWord.cs

## Tổng quan

Tệp mã nguồn `Action_MappingParamToWord.cs` định nghĩa một Plugin (Custom Action) được thiết kế để chạy trong môi trường Microsoft Dynamics 365/Power Platform. Mục đích chính của plugin này là nhận một tài liệu Word dưới dạng chuỗi Base64 và một tập hợp các tham số dưới dạng JSON, sau đó thực hiện việc ánh xạ và thay thế các placeholder (bao gồm Content Controls và văn bản thuần) trong tài liệu Word bằng các giá trị tham số được cung cấp.

Plugin sử dụng thư viện OpenXML SDK (`DocumentFormat.OpenXml`) để thao tác trực tiếp với cấu trúc bên trong của tài liệu Word (.docx), cho phép thay thế văn bản, xử lý ngắt dòng, và chèn hình ảnh (QR code) vào các vị trí được chỉ định.

## Chi tiết các Hàm (Functions/Methods)

### Execute(IServiceProvider serviceProvider)

**Chức năng tổng quát:**
Đây là điểm vào chính của Plugin (thực thi giao diện `IPlugin`). Hàm này khởi tạo các dịch vụ CRM cần thiết, đọc các tham số đầu vào, gọi các hàm xử lý tài liệu Word cốt lõi, và trả về tài liệu đã được sửa đổi dưới dạng Base64.

**Logic nghiệp vụ chi tiết:**
1.  **Khởi tạo Dịch vụ:** Lấy các đối tượng `IPluginExecutionContext`, `IOrganizationServiceFactory`, `IOrganizationService`, và `ITracingService` từ `serviceProvider`.
2.  **Đọc Tham số Đầu vào:**
    *   Đọc chuỗi Base64 của tài liệu Word từ `context.InputParameters["base64"]`.
    *   Đọc chuỗi JSON chứa các cặp tham số/giá trị từ `context.InputParameters["lstParamValue"]`.
3.  **Xử lý Tài liệu:**
    *   Gọi `PrintContentControlsFromBase64(base64Input, jsonInput)`: Hàm này được sử dụng chủ yếu để ghi log (tracing) thông tin chi tiết về các Content Controls hiện có trong tài liệu, phục vụ mục đích gỡ lỗi.
    *   Gọi `ReplaceByRemovingControls(base64Input, jsonInput)`: Thực hiện thay thế các Content Controls (SDT elements) bằng giá trị tương ứng, loại bỏ cấu trúc SDT. Kết quả được lưu vào biến `rs`.
    *   Gọi `ReplaceTextInDocument(rs, jsonInput)`: Tiếp tục xử lý tài liệu đã được thay thế (từ bước trước) để thay thế các placeholder văn bản thuần (không nằm trong Content Controls).
4.  **Xử lý QR Code (Tùy chọn):**
    *   Kiểm tra xem `context.InputParameters` có chứa khóa `"qrcodebase64"` và giá trị của nó có rỗng không.
    *   Nếu có, đọc giá trị QR code Base64 và gọi hàm `MapQRcode(qrcodebase64, rs, qrText)` để chèn hình ảnh QR code vào vị trí placeholder đã định.
5.  **Trả về Kết quả:** Gán chuỗi Base64 của tài liệu Word đã được sửa đổi cuối cùng vào `context.OutputParameters["result"]`.

### MapQRcode(string qrCodeBase64, string base64Word, string textToAdd)

**Chức năng tổng quát:**
Hàm này chịu trách nhiệm tìm kiếm một placeholder cụ thể (`bsd_qr_bank`) trong tài liệu Word và thay thế nó bằng hình ảnh QR code được cung cấp.

**Logic nghiệp vụ chi tiết:**
1.  **Thiết lập Kích thước:** Định nghĩa kích thước cố định cho hình ảnh (1x1 inch, tương đương 914400L EMU - English Metric Units).
2.  **Chuyển đổi Dữ liệu:** Chuyển đổi chuỗi Base64 của tài liệu Word và QR code thành mảng byte.
3.  **Mở Tài liệu:** Mở tài liệu Word bằng `WordprocessingDocument.Open` trong `MemoryStream` ở chế độ chỉnh sửa (`true`).
4.  **Tìm Placeholder:** Tìm tất cả các phần tử `Text` trong phần thân tài liệu (`body.Descendants<Text>`) có chứa chuỗi `"bsd_qr_bank"`.
5.  **Chèn Hình ảnh Part:** Nếu tìm thấy placeholder:
    *   Thêm một `ImagePart` mới vào `MainDocumentPart` (với loại `ImagePartType.Png`).
    *   Đổ dữ liệu byte của QR code vào `ImagePart` và lấy `relationshipId`.
6.  **Tạo Cấu trúc Drawing:** Xây dựng cấu trúc OpenXML phức tạp (`Drawing`, `Inline`, `Extent`, `DocProperties`, `Picture`, `BlipFill`, `ShapeProperties`) để nhúng hình ảnh vào tài liệu với kích thước và thuộc tính đã định.
7.  **Thay thế:**
    *   Lấy phần tử `Text` đầu tiên tìm thấy (`textToReplace`).
    *   Tìm phần tử cha là `Run` (`parentRun`).
    *   Xóa tất cả các phần tử con `Text` khỏi `parentRun`.
    *   Thêm cấu trúc `Drawing` (chứa QR code) vào `parentRun`.
8.  **Lưu và Trả về:** Lưu tài liệu (`mainPart.Document.Save()`) và trả về nội dung tài liệu đã sửa đổi dưới dạng Base64.

### ReplaceTextInDocument(string base64Word, string jsonData)

**Chức năng tổng quát:**
Hàm này thực hiện việc thay thế các placeholder văn bản thuần (không phải Content Controls) trong tài liệu Word dựa trên dữ liệu JSON đầu vào.

**Logic nghiệp vụ chi tiết:**
1.  **Chuẩn bị Dữ liệu:** Chuyển đổi Base64 Word thành byte[] và phân tích chuỗi JSON (`jsonData`) thành `JObject`.
2.  **Tạo Dictionary Thay thế:** Trích xuất tất cả các cặp key-value từ mảng `lstParamValue` trong JSON thành một `Dictionary<string, string>` (`replacements`).
3.  **Duyệt và Thay thế (Chính xác):**
    *   Duyệt qua tất cả các phần tử `Text` trong phần thân tài liệu (`body.Descendants<Text>`).
    *   Nếu `textElement.Text` khớp *chính xác* với một key trong `replacements`:
        *   Nếu giá trị thay thế (`replacementValue`) chứa ký tự xuống dòng (`\n`), gọi `InsertTextWithLineBreaks` để xử lý ngắt dòng.
        *   Nếu không, thay thế nội dung `textElement.Text` bằng `replacementValue`.
4.  **Duyệt và Thay thế trong Bảng (Chính xác):** Lặp lại logic thay thế chính xác tương tự cho tất cả các phần tử `Text` nằm bên trong các ô bảng (`TableCell`).
5.  **Duyệt và Thay thế (Chuỗi con):**
    *   Thực hiện một vòng lặp thứ ba, duyệt qua tất cả các `Paragraph` và `Run` trong tài liệu.
    *   Duyệt qua từng `Text` element trong `Run`.
    *   Duyệt qua từng cặp key-value trong `replacements`. Nếu `textValue` chứa `kvp.Key` (thay thế chuỗi con), thực hiện `textElement.Text = textValue.Replace(kvp.Key, kvp.Value)`.
6.  **Lưu và Trả về:** Lưu tài liệu và trả về chuỗi Base64 mới.

### InsertTextWithLineBreaks(Text textElement, string replacementValue, WordprocessingDocument wordDoc)

**Chức năng tổng quát:**
Hàm tiện ích này xử lý việc chèn văn bản có chứa ký tự xuống dòng (`\n`) vào tài liệu Word bằng cách sử dụng các phần tử `Break` của OpenXML.

**Logic nghiệp vụ chi tiết:**
1.  **Tách Dòng:** Tách `replacementValue` thành mảng các dòng (`lines`) dựa trên các ký tự ngắt dòng phổ biến (`\r\n`, `\r`, `\n`).
2.  **Kiểm tra Đơn dòng:** Nếu chỉ có một dòng, thực hiện thay thế thông thường và thoát.
3.  **Tìm Phần tử Cha:** Lấy phần tử cha là `Run` của `textElement`. Nếu không tìm thấy `Run`, chỉ thay thế nội dung text bằng cách loại bỏ các ký tự ngắt dòng.
4.  **Chèn Dòng:**
    *   Xóa `textElement` gốc.
    *   Thêm dòng đầu tiên (`lines[0]`) vào `run` dưới dạng `Text` mới.
    *   Lặp qua các dòng còn lại (từ chỉ mục 1 trở đi):
        *   Thêm một phần tử `Break()` (tương đương với ngắt dòng).
        *   Thêm nội dung dòng tiếp theo dưới dạng `Text` mới.

### PrintContentControlsFromBase64(string base64Word, string jsonInput)

**Chức năng tổng quát:**
Hàm này được sử dụng cho mục đích gỡ lỗi và ghi log (tracing), liệt kê tất cả các Content Controls (SDT elements) có trong tài liệu Word và thông tin liên quan của chúng.

**Logic nghiệp vụ chi tiết:**
1.  **Chuẩn bị Dữ liệu:** Chuyển đổi Base64 Word thành byte[] và phân tích JSON để tạo dictionary thay thế.
2.  **Mở Tài liệu:** Mở tài liệu Word ở chế độ chỉ đọc (`false`).
3.  **Tìm Controls:** Lấy tất cả các phần tử `SdtElement` (Content Controls) trong phần thân tài liệu.
4.  **Ghi Log:** Duyệt qua từng control:
    *   Trích xuất `Alias` và `Tag` của control.
    *   Ghi log các thuộc tính như `LocalName`, `InnerXml`, `InnerText`, `Alias`, `Tag`, Loại Control, và vị trí cha của control.
    *   Việc ghi log chỉ xảy ra nếu `control.InnerText` khớp với một key trong dictionary thay thế, giúp tập trung vào các controls cần xử lý.

### ReplaceByRemovingControls(string base64Word, string jsonData)

**Chức năng tổng quát:**
Hàm này thực hiện việc thay thế nội dung của Content Controls (SDT) bằng giá trị tham số, đồng thời loại bỏ cấu trúc SDT bao quanh.

**Logic nghiệp vụ chi tiết:**
1.  **Chuẩn bị Dữ liệu:** Tương tự như các hàm thay thế khác, chuyển đổi Base64 và tạo dictionary `replacements`.
2.  **Duyệt Controls:** Duyệt qua tất cả các phần tử `SdtElement` trong tài liệu.
3.  **Thay thế Cụ thể:** Nếu `control.InnerText` khớp với một key trong `replacements`:
    *   Kiểm tra loại control:
        *   Nếu là `SdtCell`, gọi `ReplaceSdtCell`.
        *   Nếu là `SdtRun`, gọi `ReplaceSdtRun`.
        *   Nếu là loại khác (mặc định), gọi `ReplaceGenericControl`.
4.  **Lưu và Trả về:** Lưu tài liệu và trả về chuỗi Base64 mới.

### ReplaceSdtCell(SdtCell sdtCell, string value)

**Chức năng tổng quát:**
Thay thế một Content Control nằm trong ô bảng (`SdtCell`) bằng một ô bảng thuần (`TableCell`) chứa giá trị mới, cố gắng giữ lại định dạng ô gốc.

**Logic nghiệp vụ chi tiết:**
1.  **Tìm Phần tử Cha:** Xác định `TableRow` cha.
2.  **Lấy Thuộc tính:** Lấy các thuộc tính định dạng ô (`TableCellProperties`) từ `sdtCell`.
3.  **Tạo Ô mới:** Tạo một `TableCell` mới.
4.  **Sao chép Thuộc tính:** Nếu có thuộc tính, sao chép chúng sang `newCell`.
5.  **Thêm Nội dung:** Thêm nội dung mới dưới dạng `Paragraph` chứa `Run` và `Text(value)`.
6.  **Thay thế:** Chèn `newCell` sau `sdtCell` trong `parentRow`, sau đó xóa `sdtCell` gốc.

### ReplaceSdtRun(SdtRun sdtRun, string value)

**Chức năng tổng quát:**
Thay thế một Content Control nằm trong dòng văn bản (`SdtRun`) bằng một `Run` thuần chứa giá trị mới, cố gắng giữ lại định dạng văn bản gốc.

**Logic nghiệp vụ chi tiết:**
1.  **Tìm Phần tử Cha:** Xác định phần tử cha (`parent`).
2.  **Lấy Thuộc tính:** Lấy các thuộc tính định dạng `Run` (`RunProperties`) từ `sdtRun`.
3.  **Tạo Run mới:** Tạo một `Run` mới.
4.  **Sao chép Thuộc tính:** Nếu có thuộc tính, sao chép chúng vào `newRun.RunProperties`.
5.  **Thêm Nội dung:** Thêm `Text(value)` vào `newRun`.
6.  **Thay thế:** Chèn `newRun` sau `sdtRun` trong phần tử cha, sau đó xóa `sdtRun` gốc.

### ReplaceGenericControl(SdtElement control, string value)

**Chức năng tổng quát:**
Thay thế một Content Control chung (không phải `SdtCell` hoặc `SdtRun`) bằng một cấu trúc OpenXML thuần chứa giá trị mới.

**Logic nghiệp vụ chi tiết:**
1.  **Tìm Phần tử Cha:** Xác định phần tử cha (`parent`).
2.  **Tạo Phần tử Thay thế:**
    *   Nếu cha là `TableRow`, tạo một `TableCell` mới chứa giá trị.
    *   Nếu cha là `Paragraph`, tạo một `Run` mới chứa giá trị.
    *   Nếu là trường hợp khác, tạo một `Paragraph` mới chứa giá trị.
3.  **Thay thế:** Chèn phần tử mới sau `control` trong phần tử cha, sau đó xóa `control` gốc.

### AnalyzeTemplate(string base64Word)

**Chức năng tổng quát:**
Hàm gỡ lỗi này được thiết kế để phân tích tài liệu Word và tìm kiếm các placeholder tiềm năng được định dạng theo mẫu (ví dụ: chứa `[` và `]`), đồng thời kiểm tra xem chúng có nằm trong Content Controls hay không.

**Logic nghiệp vụ chi tiết:**
1.  **Mở Tài liệu:** Mở tài liệu Word ở chế độ chỉ đọc.
2.  **Tìm Text:** Tìm tất cả các phần tử `Text` có chứa cả ký tự `[` và `]`.
3.  **Ghi Log:** Duyệt qua các phần tử tìm thấy:
    *   Ghi log nội dung văn bản và loại phần tử cha.
    *   Tìm kiếm phần tử `SdtElement` tổ tiên gần nhất. Nếu tìm thấy, ghi log `Alias` của Content Control đó.