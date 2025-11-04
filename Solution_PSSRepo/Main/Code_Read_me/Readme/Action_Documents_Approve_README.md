# Phân tích mã nguồn: Action_Documents_Approve.cs

## Tổng quan

Tệp `Action_Documents_Approve.cs` chứa một Plugin C# được triển khai trong môi trường Microsoft Dynamics 365/Power Platform, thực thi giao diện `IPlugin`. Plugin này được thiết kế để xử lý một hành động tùy chỉnh (Custom Action) hoặc sự kiện (Event) liên quan đến việc phê duyệt một bản ghi tài liệu (Document) tùy chỉnh.

Chức năng cốt lõi của Plugin là:
1.  Cập nhật trạng thái của bản ghi tài liệu mục tiêu thành "Đã phê duyệt" (Approved).
2.  Tải xuống nội dung tệp Word đính kèm trên bản ghi tài liệu.
3.  Sử dụng nội dung tệp đã tải xuống để tạo một bản ghi Mẫu Tài liệu (Document Template) mới trong hệ thống, liên kết nó với một thực thể nghiệp vụ cụ thể (ví dụ: Sales Order hoặc Customer Notices).

Plugin sử dụng các tác vụ bất đồng bộ (`async/await`) để thực hiện các hoạt động cập nhật và tạo bản ghi song song, tối ưu hóa hiệu suất.

## Chi tiết các Hàm (Functions/Methods)

### Execute(IServiceProvider serviceProvider)

**Chức năng tổng quát:**
Đây là điểm vào chính của Plugin Dynamics 365. Hàm này khởi tạo các dịch vụ cần thiết (Context, Service, Tracing) và bắt đầu quá trình xử lý nghiệp vụ chính bằng cách gọi hàm `Init()` bất đồng bộ.

**Logic nghiệp vụ chi tiết:**
1.  **Khởi tạo Dịch vụ:** Hàm truy xuất và gán các đối tượng dịch vụ tiêu chuẩn của Dynamics 365: `IPluginExecutionContext` (context), `IOrganizationServiceFactory` (serviceFactory), `IOrganizationService` (service, được tạo bằng `context.UserId`), và `ITracingService` (tracingService).
2.  **Thực thi Logic:** Gọi hàm `Init()` (là một `async Task`) và chờ kết quả bằng `.Wait()`.
3.  **Xử lý Ngoại lệ:** Sử dụng khối `try-catch` để bắt `AggregateException`, loại ngoại lệ thường xảy ra khi chờ một tác vụ bất đồng bộ.
4.  Nếu bắt được `AggregateException`, nó sẽ cố gắng trích xuất ngoại lệ bên trong đầu tiên (`ex.InnerExceptions.FirstOrDefault()`) và ném lại ngoại lệ đó, giúp cung cấp thông báo lỗi rõ ràng hơn cho người dùng hoặc hệ thống.

### Init()

**Chức năng tổng quát:**
Hàm bất đồng bộ này chịu trách nhiệm trích xuất các tham số đầu vào, truy vấn bản ghi tài liệu mục tiêu, kiểm tra trạng thái hiện tại, và điều phối việc cập nhật trạng thái cùng với việc tạo mẫu tài liệu.

**Logic nghiệp vụ chi tiết:**
1.  **Truy xuất Tham số:**
    *   Lấy `EntityReference` của bản ghi mục tiêu từ `context.InputParameters["Target"]`.
    *   Lấy nội dung tệp Base64 từ `context.InputParameters["File"]`. Nếu chuỗi này rỗng hoặc chỉ chứa khoảng trắng, nó được gán là `null`.
2.  **Truy vấn Bản ghi:** Thực hiện `service.Retrieve()` để lấy bản ghi mục tiêu (`this.en`) với các cột cần thiết: `"bsd_name"`, `"bsd_project"`, `"bsd_filewordtemplatedocx"`, `"statuscode"`, và `"bsd_type"`.
3.  **Kiểm tra Trạng thái:** Kiểm tra giá trị của trường `statuscode`. Nếu giá trị là `100000000` (được giả định là trạng thái "approved"), hàm sẽ thoát ngay lập tức (`return`) để tránh xử lý trùng lặp.
4.  **Thực thi Song song:** Sử dụng `Task.WhenAll()` để đảm bảo cả hai tác vụ `updateDocument()` và `createDocumentTemplate()` được thực thi đồng thời và chờ cả hai hoàn thành trước khi kết thúc `Init()`.
5.  **Xử lý Ngoại lệ:** Bắt và ném lại `InvalidPluginExecutionException`.

### updateDocument()

**Chức năng tổng quát:**
Cập nhật trạng thái của bản ghi tài liệu mục tiêu thành trạng thái "Đã phê duyệt" (Approved).

**Logic nghiệp vụ chi tiết:**
1.  **Ghi dấu vết:** Ghi log bắt đầu quá trình cập nhật trạng thái.
2.  **Tạo Entity Cập nhật:** Tạo một đối tượng `Entity` mới chỉ chứa tên logic và ID của bản ghi mục tiêu (cập nhật một phần).
3.  **Thiết lập Trạng thái:** Gán giá trị `100000000` cho trường `statuscode` bằng cách sử dụng `OptionSetValue`.
4.  **Thực hiện Cập nhật:** Gọi `this.service.Update(enDocument)`.
5.  **Ghi dấu vết:** Ghi log kết thúc quá trình cập nhật.
6.  Bao bọc trong `try-catch` để xử lý ngoại lệ.

### createDocumentTemplate()

**Chức năng tổng quát:**
Tạo một bản ghi Mẫu Tài liệu (Document Template) mới trong Dynamics 365, sử dụng thông tin và nội dung tệp từ tài liệu đã phê duyệt.

**Logic nghiệp vụ chi tiết:**
1.  **Ghi dấu vết:** Ghi log bắt đầu quá trình tạo mẫu.
2.  **Khởi tạo Entity:** Tạo một đối tượng `Entity` mới với tên logic là `"documenttemplate"`.
3.  **Thiết lập Thuộc tính:**
    *   `name`: Lấy từ trường `"bsd_name"` của bản ghi tài liệu gốc.
    *   `description`: Sử dụng ID của bản ghi tài liệu gốc.
    *   `documenttype`: Gán `OptionSetValue(2)`, đại diện cho loại Word Template.
    *   `associatedentitytypecode`: Gọi hàm `entityAssocia()` để xác định thực thể liên kết (ví dụ: `salesorder`).
4.  **Xử lý Nội dung Tệp:** Kiểm tra xem `this.base64File` có giá trị không. Nếu có, nó gọi hàm `DownloadFile(this.en.Id)` để tải xuống nội dung tệp từ bản ghi tài liệu và gán chuỗi Base64 kết quả cho trường `content` của mẫu tài liệu.
5.  **Tạo Bản ghi:** Thực hiện `this.service.Create(documentTemplate)`.
6.  **Ghi dấu vết:** Ghi log kết thúc quá trình tạo mẫu.

### entityAssocia()

**Chức năng tổng quát:**
Xác định tên logic của thực thể (entity) mà Mẫu Tài liệu sẽ được liên kết, dựa trên giá trị của trường loại tài liệu (`bsd_type`).

**Logic nghiệp vụ chi tiết:**
1.  Khởi tạo biến `entity` là chuỗi rỗng.
2.  Sử dụng cấu trúc `switch` để kiểm tra giá trị số nguyên của `OptionSetValue` trong trường `"bsd_type"` của bản ghi tài liệu gốc (`this.en`).
3.  **Ánh xạ Giá trị:**
    *   Nếu giá trị là `100000000`, thực thể liên kết được đặt là `"salesorder"`.
    *   Nếu giá trị là `100000001`, thực thể liên kết được đặt là `"bsd_customernotices"`.
    *   Trong các trường hợp khác (`default`), `entity` vẫn là chuỗi rỗng.
4.  Trả về tên logic của thực thể đã xác định.

### DownloadFile(Guid documentId)

**Chức năng tổng quát:**
Thực hiện quá trình tải xuống nội dung tệp lớn (Large File Download) từ một trường tệp cụ thể (`bsd_filewordtemplatedocx`) trên bản ghi tài liệu, sử dụng cơ chế tải xuống theo khối (chunking) của Dynamics 365.

**Logic nghiệp vụ chi tiết:**
1.  **Khởi tạo Tải xuống:** Tạo và thực thi `InitializeFileBlocksDownloadRequest`, chỉ định bản ghi mục tiêu (`bsd_documents`, `documentId`) và tên thuộc tính tệp (`bsd_filewordtemplatedocx`).
2.  **Nhận Thông tin Tệp:** Trích xuất `fileContinuationToken` (mã thông báo duy nhất cho phiên tải xuống) và `fileSizeInBytes` từ phản hồi.
3.  **Thiết lập Khối:**
    *   Khởi tạo danh sách byte (`fileBytes`) với kích thước dự kiến.
    *   Thiết lập `offset` ban đầu là 0.
    *   Xác định `blockSizeDownload`. Mặc định là 4MB (4 * 1024 * 1024), nhưng sẽ được điều chỉnh nếu tính năng chunking không được hỗ trợ hoặc nếu kích thước tệp nhỏ hơn 4MB.
4.  **Vòng lặp Tải xuống theo Khối:** Sử dụng vòng lặp `while (fileSizeInBytes > 0)`:
    *   Tạo `DownloadBlockRequest`, chỉ định kích thước khối (`BlockLength`), mã thông báo (`FileContinuationToken`), và vị trí bắt đầu (`Offset`).
    *   Thực thi yêu cầu và nhận `downloadBlockResponse`.
    *   Thêm dữ liệu byte (`downloadBlockResponse.Data`) vào danh sách `fileBytes`.
    *   Giảm `fileSizeInBytes` bằng kích thước khối đã tải xuống.
    *   Tăng `offset` để chuyển sang khối tiếp theo.
5.  **Chuyển đổi Base64:** Sau khi tải xuống hoàn tất, chuyển đổi mảng byte thành chuỗi Base64 (`Convert.ToBase64String`).
6.  Ghi dấu vết chuỗi Base64 và trả về chuỗi này.