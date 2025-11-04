# Phân tích mã nguồn: Plugin_AppendixContract_CreateAdvancePayment.cs

## Tổng quan

Tệp mã nguồn `Plugin_AppendixContract_CreateAdvancePayment.cs` chứa một plugin Microsoft Dynamics CRM/Power Platform được thiết kế để tự động chia sẻ một bản ghi mục tiêu (có khả năng là một Phụ lục Hợp đồng hoặc bản ghi liên quan đến Thanh toán Tạm ứng, dựa trên tên lớp) cho các nhóm (Teams) cụ thể.

Plugin này hoạt động bằng cách xác định tất cả các nhóm mà người dùng đang thực thi là thành viên hoặc quản trị viên. Sau đó, nó lọc ra các nhóm "SALE" có liên quan và cấp quyền truy cập Ghi (Write) và Thêm (Append/AppendTo) cho các nhóm này đối với bản ghi mục tiêu.

Plugin triển khai giao diện `IPlugin` và sử dụng FetchXML để truy vấn dữ liệu từ hệ thống CRM.

## Chi tiết các Hàm (Functions/Methods)

### IPlugin.Execute(IServiceProvider serviceProvider)

#### Chức năng tổng quát:
Đây là điểm vào chính của plugin. Hàm này chịu trách nhiệm khởi tạo các dịch vụ CRM, truy xuất bản ghi mục tiêu, xác định các nhóm liên quan đến người dùng đang thực thi, và cuối cùng là gọi hàm chia sẻ để cấp quyền truy cập cho các nhóm "SALE" được tìm thấy.

#### Logic nghiệp vụ chi tiết:
1.  **Khởi tạo Dịch vụ:**
    *   Hàm lấy các dịch vụ cần thiết từ `serviceProvider`, bao gồm `IPluginExecutionContext` (context), `IOrganizationServiceFactory` (factory), `IOrganizationService` (service), và `ITracingService` (traceService).
    *   `IOrganizationService` được tạo ra bằng cách sử dụng ID người dùng hiện tại (`this.context.UserId`) để đảm bảo các thao tác được thực hiện dưới quyền của người dùng đó.

2.  **Truy xuất Bản ghi Mục tiêu:**
    *   Bản ghi mục tiêu (`Target`) được lấy từ `context.InputParameters`.
    *   Sử dụng `this.service.Retrieve`, plugin truy xuất toàn bộ bản ghi mục tiêu (`new ColumnSet(true)`) dựa trên tên logic và ID của nó.

3.  **Thu thập Tên Nhóm (Teams):**
    *   Một danh sách chuỗi (`source`) được khởi tạo để lưu trữ các phần tên nhóm đã được xử lý.
    *   **Tìm kiếm Team (Thành viên):** Plugin thực hiện một truy vấn FetchXML để tìm tất cả các bản ghi `team` mà người dùng hiện tại (`this.context.UserId`) là thành viên (thông qua liên kết `teammembership` và `systemuser`).
        *   Đối với mỗi nhóm tìm thấy, tên nhóm được lấy, cắt bỏ phần sau dấu gạch ngang đầu tiên (ví dụ: "HN-SALE" thành "HN"), và thêm vào danh sách `source`.
    *   **Tìm kiếm Team (Quản trị viên):** Plugin thực hiện một truy vấn FetchXML thứ hai để tìm tất cả các bản ghi `team` mà người dùng hiện tại là quản trị viên (`administratorid`).
        *   Tên của các nhóm này cũng được xử lý tương tự (cắt bỏ phần sau dấu gạch ngang) và thêm vào danh sách `source`.

4.  **Thực hiện Chia sẻ (Sharing):**
    *   Plugin duyệt qua danh sách `source` sau khi loại bỏ các tên trùng lặp (`source.Distinct().ToList()`).
    *   Đối với mỗi tên nhóm duy nhất (`str`):
        *   Plugin thực hiện truy vấn FetchXML thứ ba để tìm kiếm các nhóm có tên khớp với mẫu `"str + "-SALE%"` (ví dụ: nếu `str` là "HN", nó tìm kiếm các nhóm có tên bắt đầu bằng "HN-SALE").
        *   Đối với mỗi nhóm "SALE" được tìm thấy, hàm `Role_SharePrivileges` được gọi:
            *   `USER`: Tham chiếu đến nhóm "SALE" tìm được.
            *   `Target`: Tham chiếu đến bản ghi mục tiêu.
            *   Các quyền được cấp: `write_Access = true`, `append_Access = true`, `assign = false`, `share = false`.

### Role_SharePrivileges(EntityReference USER, EntityReference Target, IOrganizationService service, bool write_Access, bool append_Access, bool assign, bool share)

#### Chức năng tổng quát:
Hàm tiện ích này chịu trách nhiệm xây dựng và thực thi yêu cầu `GrantAccessRequest` để chia sẻ một bản ghi CRM (`Target`) cho một người dùng hoặc nhóm (`USER`) với các quyền truy cập cụ thể.

#### Logic nghiệp vụ chi tiết:
1.  **Khởi tạo Quyền Truy cập:**
    *   Biến `accessRights2` (kiểu `AccessRights`) được khởi tạo. Trong mã nguồn, nó được khởi tạo bằng 1 (thường tương ứng với quyền Read trong CRM).

2.  **Xử lý Quyền Ghi (Write):**
    *   Nếu tham số `write_Access` là `true`, quyền Ghi (giá trị 2) sẽ được thêm vào `accessRights2` bằng phép toán OR bitwise (`|`).

3.  **Xử lý Quyền Thêm (Append/AppendTo):**
    *   Nếu tham số `append_Access` là `true`, quyền Append (giá trị 16) và AppendTo (giá trị 4) sẽ được thêm vào `accessRights2`. (Tổng cộng là 20).

4.  **Tạo và Thực thi Yêu cầu Chia sẻ:**
    *   Một đối tượng `GrantAccessRequest` được tạo.
    *   `PrincipalAccess` được thiết lập, bao gồm:
        *   `AccessMask`: Tổng hợp các quyền đã tính toán trong `accessRights2`.
        *   `Principal`: Tham chiếu đến người dùng hoặc nhóm (`USER`) sẽ nhận quyền.
    *   `Target` được thiết lập là bản ghi cần chia sẻ.
    *   Yêu cầu được thực thi thông qua `service.Execute((OrganizationRequest)request)`.

5.  **Xử lý Ngoại lệ:**
    *   Hàm được bao bọc trong khối `try-catch`. Nếu có bất kỳ lỗi nào xảy ra trong quá trình áp dụng quy tắc chia sẻ, một ngoại lệ mới sẽ được ném ra, bao gồm thông báo lỗi gốc. (Lưu ý: Các tham số `assign` và `share` được truyền vào nhưng không được sử dụng trong logic tính toán `AccessMask` của hàm này.)