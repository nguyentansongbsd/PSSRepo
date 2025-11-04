# Phân tích mã nguồn: Action_SearchCustomer.cs

## Tổng quan

Tệp mã nguồn `Action_SearchCustomer.cs` định nghĩa một plugin tùy chỉnh (Custom Action) cho Microsoft Dynamics 365/Power Platform, được đặt tên là `Action_SearchCustomer`. Plugin này được thiết kế để thực hiện chức năng tìm kiếm khách hàng trong hệ thống. Dựa trên tham số đầu vào, nó có thể tìm kiếm Khách hàng Cá nhân (thực thể `contact`) hoặc Khách hàng Doanh nghiệp (thực thể `account`) và trả về kết quả tìm kiếm dưới dạng một chuỗi HTML được định dạng sẵn.

Plugin sử dụng `QueryExpression` để xây dựng các truy vấn tìm kiếm dựa trên các trường định danh chính (như CMND/Passport cho cá nhân hoặc Mã đăng ký cho doanh nghiệp).

## Chi tiết các Hàm (Functions/Methods)

### Execute(IServiceProvider serviceProvider)

#### Chức năng tổng quát:

Đây là điểm vào chính của plugin Dynamics 365. Hàm này chịu trách nhiệm khởi tạo các dịch vụ cần thiết, đọc các tham số đầu vào để xác định loại tìm kiếm (Cá nhân hay Doanh nghiệp), thực hiện truy vấn dữ liệu tương ứng, và trả về kết quả dưới dạng chuỗi HTML.

#### Logic nghiệp vụ chi tiết:

1.  **Khởi tạo Dịch vụ:**
    *   Hàm khởi tạo các đối tượng dịch vụ tiêu chuẩn của Dynamics 365: `IPluginExecutionContext` (ngữ cảnh thực thi), `IOrganizationServiceFactory`, `IOrganizationService` (dịch vụ tổ chức để tương tác với dữ liệu), và `ITracingService` (dịch vụ theo dõi/ghi log).
    *   Ghi log bắt đầu quá trình (`traceService.Trace("start")`).

2.  **Xác định Loại Tìm kiếm:**
    *   Hàm lấy tham số đầu vào `type` (kiểu `int`) từ ngữ cảnh.
    *   Biến `html` được khởi tạo là chuỗi rỗng để lưu trữ kết quả HTML.

3.  **Trường hợp 1: Khách hàng Cá nhân (KHCN) - `if (type == 0)`:**
    *   **Lấy Tham số:** Lấy các tham số tìm kiếm liên quan đến cá nhân: `cmnd`, `cccd`, `passport`, `otherCode`, `fullName`, `telephone`, `email`.
    *   **Xây dựng Truy vấn:** Tạo một `QueryExpression` nhắm vào thực thể `contact`.
    *   Thiết lập `ColumnSet.AllColumns = true` để lấy tất cả các cột của thực thể.
    *   **Thiết lập Điều kiện (Criteria):**
        *   Nếu `cmnd` không rỗng, thêm điều kiện tìm kiếm chính xác bằng `bsd_identitycardnumber`.
        *   **Kiểm tra Bắt buộc:** Nếu `cmnd` rỗng sau khi kiểm tra, hàm sẽ thoát ngay lập tức (`return;`), ngụ ý rằng CMND là trường bắt buộc để thực hiện tìm kiếm KHCN.
        *   Nếu `passport` không rỗng, thêm điều kiện tìm kiếm chính xác bằng `bsd_passport`.
        *   *Lưu ý:* Các điều kiện tìm kiếm dựa trên `cccd`, `otherCode`, `fullName`, `telephone`, và `email` đã bị chú thích trong mã nguồn và không được áp dụng vào truy vấn.
        *   Thêm điều kiện trạng thái bắt buộc: `statecode` bằng 0 (Active/Hoạt động).
    *   **Thực thi Truy vấn:** Gọi `service.RetrieveMultiple(queryKHCN)` để lấy dữ liệu.
    *   **Xử lý Kết quả:**
        *   Nếu có kết quả hợp lệ, hàm lặp qua từng thực thể (`Entity item`) trong `result.Entities`.
        *   Đối với mỗi thực thể, nó xây dựng một hàng HTML (`<tr>`) chứa checkbox (với `data-id` là ID của thực thể), số thứ tự, tên đầy đủ, giới tính (lấy từ `FormattedValues` nếu tồn tại), địa chỉ, điện thoại di động và email. Việc lấy giá trị thuộc tính được thực hiện thông qua hàm trợ giúp `getValueXML`.

4.  **Trường hợp 2: Khách hàng Doanh nghiệp (KHDN) - `else`:**
    *   **Lấy Tham số:** Lấy các tham số tìm kiếm liên quan đến doanh nghiệp: `nameCompany`, `registrationCode`, `telephone`, `email`.
    *   **Xây dựng Truy vấn:** Tạo một `QueryExpression` nhắm vào thực thể `account`.
    *   Thiết lập `ColumnSet.AllColumns = true`.
    *   **Thiết lập Điều kiện (Criteria):**
        *   Nếu `nameCompany` không rỗng, thêm điều kiện tìm kiếm bằng `bsd_name`.
        *   Nếu `registrationCode` không rỗng, thêm điều kiện tìm kiếm bằng `bsd_registrationcode`.
        *   **Kiểm tra Bắt buộc:** Nếu `registrationCode` rỗng, hàm sẽ thoát ngay lập tức (`return;`), ngụ ý rằng Mã đăng ký là trường bắt buộc cho tìm kiếm KHDN.
        *   Nếu `telephone` không rỗng, thêm điều kiện tìm kiếm bằng `telephone1`.
        *   Nếu `email` không rỗng, thêm điều kiện tìm kiếm bằng `emailaddress1`.
        *   Thêm điều kiện trạng thái bắt buộc: `statecode` bằng 0 (Active/Hoạt động).
    *   **Thực thi Truy vấn:** Gọi `service.RetrieveMultiple(queryKHDN)` để lấy dữ liệu.
    *   **Xử lý Kết quả:**
        *   Nếu có kết quả hợp lệ, hàm lặp qua từng thực thể.
        *   Xây dựng một hàng HTML (`<tr>`) chứa checkbox, số thứ tự, tên công ty, thông tin chủ đầu tư, địa chỉ, điện thoại và email.

5.  **Trả về Kết quả:**
    *   Cuối cùng, chuỗi HTML đã được xây dựng (`html`) được gán vào tham số đầu ra có tên là `result` của ngữ cảnh plugin.

### getValueXML(Entity item, string logicalName)

#### Chức năng tổng quát:

Đây là một hàm tiện ích (helper function) được sử dụng để truy xuất giá trị chuỗi của một thuộc tính từ một thực thể Dynamics 365 một cách an toàn, tránh lỗi nếu thuộc tính đó không được tải hoặc không tồn tại trong thực thể.

#### Logic nghiệp vụ chi tiết:

1.  Hàm nhận hai tham số: `Entity item` (thực thể cần kiểm tra) và `string logicalName` (tên logic của thuộc tính).
2.  Hàm sử dụng toán tử điều kiện ba ngôi để kiểm tra sự tồn tại của thuộc tính: `item.Contains(logicalName)`.
3.  **Nếu thuộc tính tồn tại:** Hàm trả về giá trị của thuộc tính đó sau khi ép kiểu thành `string`.
4.  **Nếu thuộc tính không tồn tại:** Hàm trả về một chuỗi rỗng (`""`).