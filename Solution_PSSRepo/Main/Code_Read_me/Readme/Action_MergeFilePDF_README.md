# Phân tích mã nguồn: Action_MergeFilePDF.cs

## Tổng quan

Tệp mã nguồn `Action_MergeFilePDF.cs` định nghĩa một Plugin (hoặc Custom Action) được thiết kế để chạy trong môi trường Microsoft Dynamics 365/Power Platform. Mục đích chính của lớp này là nhận một danh sách các tệp PDF được mã hóa dưới dạng chuỗi Base64, hợp nhất chúng thành một tệp PDF duy nhất, và trả về tệp đã hợp nhất cũng dưới dạng chuỗi Base64.

Plugin này sử dụng thư viện `PdfSharp` để xử lý và thao tác với các tài liệu PDF. Nó tuân thủ giao diện `IPlugin` tiêu chuẩn của Dynamics 365.

## Chi tiết các Hàm (Functions/Methods)

### IPlugin.Execute(IServiceProvider serviceProvider)

#### Chức năng tổng quát
Đây là điểm vào chính (entry point) của Plugin. Hàm này chịu trách nhiệm thiết lập ngữ cảnh thực thi Dynamics 365, lấy dữ liệu đầu vào từ Custom Action, gọi hàm hợp nhất PDF cốt lõi, và đặt kết quả vào tham số đầu ra.

#### Logic nghiệp vụ chi tiết
1.  **Khởi tạo Dịch vụ:** Hàm bắt đầu bằng việc lấy và khởi tạo các dịch vụ Dynamics 365 tiêu chuẩn:
    *   `IPluginExecutionContext context`: Ngữ cảnh thực thi hiện tại.
    *   `IOrganizationServiceFactory factory`: Factory để tạo dịch vụ tổ chức.
    *   `IOrganizationService service`: Dịch vụ để tương tác với dữ liệu Dynamics 365.
    *   `ITracingService tracingService`: Dịch vụ ghi nhật ký (tracing) để gỡ lỗi.
2.  **Lấy Tham số Đầu vào:** Hàm truy cập tham số đầu vào của ngữ cảnh thực thi (`context.InputParameters`).
    *   Nó lấy giá trị của tham số có tên `"files"`. Giá trị này được mong đợi là một chuỗi chứa các chuỗi Base64 của các tệp PDF, được phân tách bằng dấu phẩy (`,`).
    *   Chuỗi này sau đó được tách (`Split(',')`) thành một mảng các chuỗi (`files`).
3.  **Ghi nhật ký (Tracing):** Ghi lại số lượng tệp đầu vào được tìm thấy.
4.  **Gọi Logic Hợp nhất:** Hàm gọi phương thức `MergePdfFiles(files)` để thực hiện quá trình hợp nhất. Kết quả được lưu vào biến `filers`.
5.  **Đặt Tham số Đầu ra:** Kết quả Base64 của tệp PDF đã hợp nhất (`filers`) được đặt vào tham số đầu ra của ngữ cảnh thực thi với tên `"fileres"`.

### MergePdfFiles(string[] base64Files)

#### Chức năng tổng quát
Hàm này thực hiện logic cốt lõi: nhận một mảng các chuỗi Base64 đại diện cho các tệp PDF, xử lý chúng bằng thư viện PdfSharp để tạo một tài liệu PDF duy nhất, và trả về tài liệu kết quả dưới dạng chuỗi Base64.

#### Logic nghiệp vụ chi tiết
1.  **Ghi nhật ký:** Ghi lại chuỗi Base64 đầu tiên trong mảng để kiểm tra dữ liệu đầu vào.
2.  **Lọc Dữ liệu Hợp lệ:** Hàm tạo một danh sách mới (`validBase64Files`) bằng cách lọc mảng đầu vào (`base64Files`). Chỉ những chuỗi không rỗng hoặc không chỉ chứa khoảng trắng (`!string.IsNullOrWhiteSpace(f)`) mới được giữ lại.
3.  **Kiểm tra Điều kiện Thoát:**
    *   Nếu danh sách các tệp hợp lệ rỗng (`validBase64Files.Count == 0`), hàm ghi nhật ký thông báo lỗi và trả về một chuỗi rỗng (`string.Empty`), kết thúc quá trình.
4.  **Khởi tạo Tài liệu Đầu ra:** Một đối tượng `PdfDocument` mới (`outputDocument`) được tạo ra trong khối `using` để đảm bảo việc quản lý bộ nhớ. Đây là tài liệu sẽ chứa tất cả các trang đã hợp nhất.
5.  **Lặp qua các Tệp Hợp lệ:** Hàm lặp qua từng chuỗi Base64 trong danh sách `validBase64Files`.
6.  **Xử lý Từng Tệp (Trong khối Try-Catch):**
    *   **Giải mã:** Chuỗi Base64 (`base64File`) được chuyển đổi thành mảng byte (`pdfBytes`) bằng `Convert.FromBase64String`.
    *   **Tạo MemoryStream:** Mảng byte được bọc trong một `MemoryStream` (`pdfStream`).
    *   **Mở Tài liệu Đầu vào:** Sử dụng `PdfReader.Open(pdfStream, PdfDocumentOpenMode.Import)` để mở tệp PDF. Chế độ `Import` đảm bảo rằng tài liệu được mở một cách an toàn để sao chép trang.
    *   **Hợp nhất Trang:** Hàm lặp qua tất cả các trang (`PdfPage page`) trong tài liệu đầu vào (`inputDocument.Pages`). Mỗi trang được thêm vào tài liệu đầu ra (`outputDocument.AddPage(page)`).
    *   **Xử lý Ngoại lệ:** Nếu bất kỳ bước nào trong quá trình giải mã hoặc mở/import PDF gặp lỗi (ví dụ: chuỗi Base64 không hợp lệ hoặc tệp PDF bị hỏng), khối `catch` sẽ được kích hoạt:
        *   Ghi nhật ký chi tiết lỗi (`tracingService.Trace`).
        *   Ném ra một `InvalidPluginExecutionException` để dừng quá trình thực thi của Plugin và thông báo lỗi cho người dùng Dynamics 365.
7.  **Lưu và Mã hóa Kết quả:** Sau khi tất cả các tệp đã được hợp nhất:
    *   Một `MemoryStream` mới (`resultStream`) được tạo ra.
    *   Tài liệu đầu ra (`outputDocument`) được lưu vào `resultStream`. Tham số `false` (hoặc không có tham số thứ hai) chỉ định việc đóng luồng sau khi lưu.
    *   `resultStream` được chuyển đổi thành mảng byte, và sau đó được mã hóa lại thành chuỗi Base64 bằng `Convert.ToBase64String`.
    *   Chuỗi Base64 kết quả được trả về.