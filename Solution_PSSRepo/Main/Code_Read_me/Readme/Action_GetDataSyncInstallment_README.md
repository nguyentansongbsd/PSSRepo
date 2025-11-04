# Phân tích mã nguồn: Action_GetDataSyncInstallment.cs

## Tổng quan

Tệp `Action_GetDataSyncInstallment.cs` chứa một Plugin Dynamics 365 (C#) được thiết kế để thực thi một hành động nghiệp vụ phức tạp. Chức năng chính của plugin này là truy vấn và thu thập hàng loạt các bản ghi từ hai thực thể tùy chỉnh khác nhau (`bsd_warningnotices` và `bsd_customernotices`) dựa trên các điều kiện trạng thái và liên kết cụ thể.

Sau khi thu thập các bản ghi, plugin sẽ chia nhỏ danh sách ID thành các lô (batch) có kích thước xác định và gọi một Custom Action khác (`bsd_Action_Active_SynsImtallment`) để xử lý đồng bộ hóa hoặc cập nhật dữ liệu trả góp (installment) cho các bản ghi đó. Plugin này hoạt động như một cơ chế thu thập và phân phối dữ liệu hàng loạt.

## Chi tiết các Hàm (Functions/Methods)

### Execute(IServiceProvider serviceProvider)

#### Chức năng tổng quát

Hàm này là điểm vào chính của Plugin Dynamics 365. Nó chịu trách nhiệm khởi tạo các dịch vụ CRM, đọc các tham số đầu vào, thực hiện hai truy vấn riêng biệt để thu thập ID của các bản ghi cần xử lý, và sau đó gọi một Custom Action bên ngoài theo lô (batch) để xử lý các ID đã thu thập.

#### Logic nghiệp vụ chi tiết

Hàm `Execute` được chia thành ba phần chính: Khởi tạo, Xử lý `bsd_warningnotices` (Giai đoạn 1), và Xử lý `bsd_customernotices` (Giai đoạn 2).

**1. Khởi tạo Dịch vụ và Tham số Đầu vào (Lines 15-30):**

*   **Khởi tạo Dịch vụ:** Thiết lập các đối tượng tiêu chuẩn của CRM: `IPluginExecutionContext` (context), `IOrganizationServiceFactory` (serviceFactory), `IOrganizationService` (service), và `ITracingService` (tracingService) để ghi nhật ký.
*   **Đọc Tham số:**
    *   Định nghĩa một đối tượng ẩn danh `fetchData` với `statuscode = "2"`.
    *   Đọc tham số đầu vào từ `context.InputParameters`:
        *   `pageNumber` (ban đầu đọc giá trị của `size`).
        *   `query_statuscode` được đặt cứng là `2`.
        *   `query_bsd_numberofwarning` được đọc từ tham số `wnumber`.
    *   Khởi tạo danh sách `allEntities` để lưu trữ kết quả truy vấn.

**2. Giai đoạn 1: Xử lý `bsd_warningnotices` (Lines 31-59):**

*   **Thiết lập Truy vấn (QueryExpression):**
    *   Tạo một `QueryExpression` nhắm vào thực thể `bsd_warningnotices`.
    *   **Điều kiện Lọc:**
        *   Lọc các bản ghi có `statuscode` **Không Bằng** `2`.
        *   Lọc các bản ghi có `bsd_numberofwarning` **Bằng** giá trị của tham số đầu vào `wnumber`.
    *   **Sắp xếp:** Sắp xếp theo ngày tạo (`createdon`) tăng dần.
    *   **Liên kết Thực thể (Link Entity):** Liên kết với thực thể `bsd_paymentschemedetail`.
    *   **Điều kiện Liên kết:** Thêm điều kiện lọc trên thực thể liên kết: trường có tên động `bsd_w_noticesnumberX` (trong đó X là giá trị của `wnumber`) phải là `Null`.
*   **Thực thi Truy vấn:** Thực hiện `service.RetrieveMultiple(query)` để lấy tất cả các bản ghi phù hợp và thêm chúng vào `allEntities`.
*   **Xác định Kích thước Lô (Batch Size):** Kích thước mặc định là 500. Nếu tham số `size` được cung cấp trong đầu vào, nó sẽ ghi đè giá trị này.
*   **Vòng lặp Xử lý Lô:**
    *   Lặp qua danh sách `allEntities` theo các bước nhảy bằng `size`.
    *   Trong mỗi lần lặp:
        *   Sử dụng LINQ (`Skip().Take()`) để chọn một lô các bản ghi.
        *   Trích xuất ID của các bản ghi này và định dạng chúng thành một chuỗi XML/chuỗi giá trị (ví dụ: `<value>{ID}</value>`) sau khi loại bỏ dấu ngoặc nhọn.
        *   Tạo một `OrganizationRequest` mới để gọi Custom Action có tên `bsd_Action_Active_SynsImtallment`.
        *   Thiết lập tham số: `listid` (chứa chuỗi ID) và `type` là `"warningNo"`.
        *   Thực thi Custom Action bằng `service.Execute(request)`.

**3. Giai đoạn 2: Xử lý `bsd_customernotices` (Lines 60-94):**

*   **Kiểm tra Điều kiện Thoát:** Kiểm tra giá trị của tham số đầu vào `payno_start`. Nếu giá trị này bằng 0, hàm sẽ thoát ngay lập tức (`return`), bỏ qua Giai đoạn 2.
*   **Thiết lập lại:** Danh sách `allEntities` được làm mới, `pagingCookie` được đặt lại là `null`, và `pageNumber` được đặt là 1.
*   **Thiết lập Truy vấn (FetchXML):**
    *   Sử dụng FetchXML để truy vấn thực thể `bsd_customernotices`.
    *   **Điều kiện Lọc:** `statuscode` **Không Bằng** `2`.
    *   **Liên kết Thực thể:** Liên kết với `bsd_paymentschemedetail`.
    *   **Điều kiện Liên kết:** Trường `bsd_paymentnoticesnumber` trên thực thể liên kết phải là `null`.
    *   Truy vấn được thiết lập để lấy tối đa 5000 bản ghi mỗi trang và hỗ trợ phân trang (paging).
*   **Thực thi Truy vấn:** Định dạng chuỗi FetchXML với các giá trị `pageNumber`, `pagingCookie`, và `statuscode`, sau đó thực thi `service.RetrieveMultiple(new FetchExpression(fetchXml))`.
*   **Kiểm tra Phân trang:** Kiểm tra thuộc tính `MoreRecords` của kết quả. Nếu có nhiều bản ghi hơn, nó sẽ tăng `pageNumber` và lưu `PagingCookie` (mặc dù logic tiếp theo chỉ xử lý các bản ghi đã được thu thập trong lần gọi đầu tiên này).
*   **Vòng lặp Xử lý Lô:**
    *   Lặp qua danh sách `allEntities` đã thu thập (từ `bsd_customernotices`) theo kích thước lô (`size`).
    *   Thực hiện logic tương tự như Giai đoạn 1: Trích xuất ID, định dạng chuỗi ID (`listid`).
    *   Tái sử dụng `request` cho Custom Action `bsd_Action_Active_SynsImtallment`.
    *   Thiết lập tham số: `listid` (chuỗi ID mới) và `type` là `"paymentno"`.
    *   Thực thi Custom Action bằng `service.Execute(request)`.