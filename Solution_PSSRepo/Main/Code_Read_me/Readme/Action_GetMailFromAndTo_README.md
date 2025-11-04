# Phân tích mã nguồn: Action_GetMailFromAndTo.cs

## Tổng quan

Tệp mã nguồn `Action_GetMailFromAndTo.cs` chứa một Plugin (Custom Action) được phát triển cho nền tảng Microsoft Dynamics 365/CRM. Mục đích chính của Plugin này là xác định và xác thực cấu hình email của người gửi (thường là cấu hình hệ thống liên quan đến Dự án) và email của người nhận (Khách hàng hoặc Liên hệ) trước khi thực hiện một hành động gửi email nào đó.

Plugin này nhận các tham số về ID thực thể và ID người nhận, sau đó truy vấn CRM để kiểm tra sự tồn tại của các trường email cần thiết trên thực thể Dự án và thực thể Khách hàng/Liên hệ. Kết quả kiểm tra được trả về thông qua tham số đầu ra `res`.

## Chi tiết các Hàm (Functions/Methods)

### Execute(IServiceProvider serviceProvider)

#### Chức năng tổng quát:
Là điểm vào chính của Plugin/Custom Action. Hàm này khởi tạo các dịch vụ CRM, lấy các tham số đầu vào cần thiết, xác định loại thực thể người nhận, và gọi hàm kiểm tra logic nghiệp vụ chính (`CheckMailIsContain`).

#### Logic nghiệp vụ chi tiết:
1.  **Khởi tạo Dịch vụ:**
    *   Khởi tạo các đối tượng CRM tiêu chuẩn: `IPluginExecutionContext` (`context`), `IOrganizationServiceFactory` (`factory`), `IOrganizationService` (`service`), và `ITracingService` (`tracingService`).
2.  **Lấy Tham số Đầu vào:**
    *   Lấy các tham số từ `context.InputParameters`:
        *   `idTo`: ID của người nhận (Contact hoặc Account).
        *   `entityName`: Tên logic của thực thể hiện tại đang kích hoạt hành động (ví dụ: `bsd_payment`).
        *   `entityId`: ID của thực thể hiện tại.
3.  **Truy vấn Thực thể Hiện tại:**
    *   Thực hiện `service.Retrieve` để lấy toàn bộ dữ liệu của thực thể hiện tại (`en`) dựa trên `entityName` và `entityId`.
4.  **Xác định Loại Người nhận (`cusType`):**
    *   Truy vấn thực thể người nhận (`cus`) bằng `idTo` (ban đầu giả định là `contact`).
    *   Kiểm tra xem thực thể `cus` có chứa trường `bsd_fullname` hay không.
        *   Nếu có, đặt `cusType` là `"contact"`.
        *   Nếu không, đặt `cusType` là `"account"`.
5.  **Xác định Trường Dự án:**
    *   Sử dụng cấu trúc `switch` để xác định tên trường tham chiếu đến Dự án (`fieldProject`) trên thực thể hiện tại.
    *   Hiện tại, nếu `entityName` là `"bsd_payment"`, `fieldProject` là `"bsd_project"`.
    *   Trong mọi trường hợp khác (`default`), `fieldProject` cũng được đặt là `"bsd_project"`.
6.  **Thực hiện Kiểm tra Email:**
    *   Lấy ID Dự án từ trường `fieldProject` của thực thể hiện tại (`en`).
    *   Gọi hàm `CheckMailIsContain`, truyền vào `idTo`, ID Dự án, và hai biến `ref` (`idFrom`, `message`).
7.  **Đặt Tham số Đầu ra:**
    *   Dựa trên giá trị trả về của `CheckMailIsContain`:
        *   Nếu `true`, đặt `context.OutputParameters["res"] = 1`.
        *   Nếu `false`, đặt `context.OutputParameters["res"] = 0`.

### CheckMailIsContain(string idTo, string idProject, ref string idFrom, ref string mess)

#### Chức năng tổng quát:
Kiểm tra sự tồn tại của cấu hình email người gửi (trên thực thể Dự án) và các trường email cần thiết trên thực thể người nhận (Contact hoặc Account).

#### Logic nghiệp vụ chi tiết:
1.  **Kiểm tra Email Dự án (Người gửi):**
    *   Tạo một truy vấn (`QueryExpression`) để lấy thực thể `bsd_project` dựa trên `idProject`.
    *   Thực hiện truy vấn (`service.RetrieveMultiple`).
    *   Kiểm tra xem bản ghi Dự án có chứa trường `bsd_senderconfigsystem` (cấu hình người gửi) hay không.
        *   Nếu không chứa (`false`), đặt thông báo lỗi `mess = "not found mail project"` và trả về `false`.
        *   Nếu chứa, lấy ID của cấu hình người gửi và đặt nó vào `context.OutputParameters["idQueueFrom"]`.
2.  **Kiểm tra Email Khách hàng (Người nhận):**
    *   Tạo một truy vấn mới dựa trên `cusType` (`account` hoặc `contact`) và `idTo`.
    *   Thực hiện truy vấn.
3.  **Xử lý Account:**
    *   Nếu `cusType` là `"account"`:
        *   Kiểm tra xem bản ghi Account có chứa trường `emailaddress1` hay không.
        *   *Lưu ý logic:* Nếu trường `emailaddress1` tồn tại (có thể là một kiểm tra ngược để đảm bảo trường đó không bị thiếu hoặc không có giá trị), đặt `mess = "not found mail account"` và trả về `false`.
4.  **Xử lý Contact:**
    *   Nếu `cusType` là `"contact"`:
        *   **Kiểm tra Email Chính:** Kiểm tra xem bản ghi Contact có chứa trường `emailaddress1` hay không. Nếu có, đặt `mess = "not found mail contact"`. (Lưu ý: Hàm không trả về `false` ở đây, mà tiếp tục kiểm tra).
        *   **Kiểm tra Email Phụ:** Kiểm tra xem bản ghi Contact có chứa trường `emailaddress11` hay không. Nếu có, đặt `mess = "not found mail contact"` và trả về `false`.
5.  **Kết quả:**
    *   Nếu quá trình kiểm tra không gặp bất kỳ điều kiện trả về `false` nào, hàm sẽ trả về `true`.

### CreateRecordError(string message)

#### Chức năng tổng quát:
Tạo một bản ghi lỗi mới trong thực thể tùy chỉnh `bsd_errorcreatemail` để ghi lại lý do thất bại của quá trình tạo email.

#### Logic nghiệp vụ chi tiết:
1.  **Khởi tạo Thực thể Lỗi:**
    *   Tạo một đối tượng thực thể mới (`enCreate`) với tên logic là `"bsd_errorcreatemail"`.
2.  **Thiết lập Trường:**
    *   Thiết lập trường `bsd_entityid` là một tham chiếu (`EntityReference`) đến thực thể hiện tại (`en`) mà Plugin đang xử lý.
    *   Thiết lập trường `bsd_reason` bằng thông báo lỗi được truyền vào (`message`).
3.  **Lưu Bản ghi:**
    *   Thực hiện lệnh `service.Create(enCreate)` để lưu bản ghi lỗi vào CRM.
4.  **Giá trị Trả về:**
    *   Hàm luôn trả về `false`. (Mặc dù hàm này được định nghĩa, nó không được gọi trong hàm `Execute` hiện tại).