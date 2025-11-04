# Phân tích mã nguồn: Action_InsertQrcodeInPDF.cs

## Tổng quan

Tệp mã nguồn `Action_InsertQrcodeInPDF.cs` định nghĩa một lớp Plugin (hoặc Custom Action) được thiết kế để chạy trong môi trường Microsoft Dynamics 365 hoặc Power Platform. Lớp này, `Action_InsertQrcodeInPDF`, thực hiện giao diện `IPlugin` và sử dụng thư viện bên ngoài `iTextSharp` để thao tác với các tệp PDF.

Chức năng chính của mã nguồn này là nhận một tệp PDF và một hình ảnh QR code (cả hai đều được mã hóa dưới dạng chuỗi Base64) làm đầu vào, sau đó chèn hình ảnh QR code vào tệp PDF và trả về tệp PDF đã được sửa đổi, cũng dưới dạng Base64.

**Lưu ý quan trọng:** Mặc dù mã nguồn chứa logic chi tiết để chèn hình ảnh QR code vào góc dưới bên phải của mỗi trang PDF, phần logic chèn này hiện đang **bị chú thích (commented out)**. Do đó, trong trạng thái hiện tại, Action này chỉ đơn thuần nhận PDF đầu vào, xử lý nó qua `PdfStamper` (mà không thực hiện thay đổi nào), và trả về PDF gốc.

## Chi tiết các Hàm (Functions/Methods)

### Execute(IServiceProvider serviceProvider)

#### Chức năng tổng quát:

Đây là phương thức bắt buộc của giao diện `IPlugin`, chịu trách nhiệm thiết lập môi trường thực thi (Context, Service, Tracing) và thực hiện logic nghiệp vụ chính: nhận PDF và QR code Base64, xử lý PDF bằng iTextSharp, và trả về PDF đã sửa đổi.

#### Logic nghiệp vụ chi tiết:

1.  **Khởi tạo Dịch vụ:**
    *   Hàm bắt đầu bằng việc lấy các dịch vụ cần thiết từ `serviceProvider`:
        *   `IPluginExecutionContext` (context): Để truy cập các tham số đầu vào và đầu ra.
        *   `IOrganizationService` (service): Để tương tác với cơ sở dữ liệu Dynamics 365 (được tạo cho người dùng hiện tại).
        *   `ITracingService` (tracingService): Để ghi lại các thông báo gỡ lỗi và theo dõi quá trình thực thi.
    *   Ghi log (trace) thông báo bắt đầu thực thi.

2.  **Xác thực Tham số Đầu vào:**
    *   **Kiểm tra `pdfBase64`:** Kiểm tra xem `context.InputParameters` có chứa khóa `"pdfBase64"` và giá trị của nó có phải là kiểu `string` không. Nếu không hợp lệ, ném ra `InvalidPluginExecutionException`.
    *   **Kiểm tra `qrBase64`:** Kiểm tra xem `context.InputParameters` có chứa khóa `"qrBase64"` và giá trị của nó có phải là kiểu `string` không.
        *   *Logic đặc biệt:* Nếu `qrBase64` không hợp lệ, mã nguồn sẽ gán ngay lập tức chuỗi `pdfBase64` gốc vào tham số đầu ra `"modifiedPdfBase64"`. Tuy nhiên, sau đó mã vẫn tiếp tục thực thi khối logic chính, điều này có thể dẫn đến việc ghi đè tham số đầu ra hoặc lỗi nếu chuỗi `qrBase64` không được xử lý đúng cách sau đó.

3.  **Giải mã Base64:**
    *   Lấy giá trị chuỗi Base64 của PDF (`pdfBase64`) và QR code (`qrBase64`).
    *   Sử dụng `Convert.FromBase64String` để chuyển đổi hai chuỗi này thành mảng byte (`pdfBytes` và `qrBytes`).

4.  **Xử lý PDF bằng iTextSharp:**
    *   Ghi log bắt đầu xử lý PDF.
    *   Sử dụng `MemoryStream` (`outputStream`) để lưu trữ tệp PDF đã được sửa đổi.
    *   Tạo đối tượng `PdfReader` từ `pdfBytes` (PDF gốc).
    *   Tạo đối tượng `PdfStamper`, liên kết `reader` với `outputStream`. `PdfStamper` được sử dụng để thêm nội dung vào PDF mà không làm thay đổi cấu trúc cơ bản.

5.  **Logic Chèn QR Code (Đã bị chú thích):**
    *   Phần logic quan trọng nhất của việc chèn hình ảnh hiện đang bị chú thích (commented out). Nếu được kích hoạt, logic này sẽ:
        *   Tạo đối tượng `Image` từ `qrBytes`.
        *   Thiết lập kích thước tuyệt đối cho hình ảnh (ví dụ: 70x70 points).
        *   Lặp qua tất cả các trang trong PDF (`i = 1` đến `reader.NumberOfPages`).
        *   Tính toán vị trí tuyệt đối (`x`, `y`) để đặt hình ảnh ở góc dưới bên phải của trang (với lề 20 points).
        *   Lấy `PdfContentByte` của trang hiện tại và thêm hình ảnh QR code vào đó.

6.  **Hoàn tất và Đóng Tài nguyên:**
    *   Gọi `stamper.Close()` và `reader.Close()` để hoàn tất quá trình ghi vào `outputStream`.

7.  **Mã hóa và Gán Kết quả:**
    *   Chuyển đổi mảng byte của `outputStream` (PDF đã sửa đổi/hoặc PDF gốc nếu logic chèn bị tắt) trở lại thành chuỗi Base64 (`modifiedPdfBase64`).
    *   Gán chuỗi Base64 kết quả vào tham số đầu ra `context.OutputParameters["modifiedPdfBase64"]`.

8.  **Xử lý Ngoại lệ (Exception Handling):**
    *   Nếu bất kỳ lỗi nào xảy ra trong khối `try`, lỗi sẽ được ghi lại bằng `tracingService`.
    *   Ném ra `InvalidPluginExecutionException` với thông báo lỗi chung, đảm bảo lỗi được trả về cho hệ thống Dynamics 365.