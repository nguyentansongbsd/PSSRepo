# Phân tích mã nguồn: Plugin_AutoShareRecord_V2.cs

## Tổng quan

Tệp mã nguồn `Plugin_AutoShareRecord_V2.cs` là một Plugin C# được thiết kế để chạy trong môi trường Microsoft Dynamics 365/CRM. Chức năng chính của plugin này là tự động chia sẻ (Grant Access) các bản ghi mới được tạo hoặc cập nhật cho các Team cụ thể dựa trên mã dự án (`ProjectCode`) liên quan và loại thực thể (entity) đang được xử lý.

Plugin này hoạt động trên các thông điệp `Create` và `Update` và sử dụng các hằng số để xác định các loại Team nghiệp vụ (SALE, CCR, FINANCE, SALE-MGT) cần được cấp quyền truy cập. Đáng chú ý, phần lớn logic chia sẻ trực tiếp trong phương thức `Execute` đã được comment out và thay thế bằng các lệnh gọi đến các lớp helper bên ngoài (`Plugin_AutoShareRecord_V2_1` và `Plugin_AutoShareRecord_V2_2`), cho thấy đây là một phiên bản đã được tái cấu trúc.

## Chi tiết các Hàm (Functions/Methods)

### IPlugin.Execute(IServiceProvider serviceProvider)

#### Chức năng tổng quát:
Đây là điểm vào chính của plugin, chịu trách nhiệm khởi tạo các dịch vụ CRM, theo dõi quá trình thực thi, và điều phối logic chia sẻ bản ghi dựa trên thông điệp (Create hoặc Update) và loại thực thể mục tiêu.

#### Logic nghiệp vụ chi tiết:
1.  **Khởi tạo Dịch vụ:**
    *   Lấy `IPluginExecutionContext`, `IOrganizationServiceFactory`, và `ITracingService` từ `serviceProvider`.
    *   Tạo `IOrganizationService` (`this.service`) bằng cách sử dụng ID người dùng của ngữ cảnh thực thi (`service1.UserId`).
    *   Ghi dấu vết ("start") vào `traceService`.
2.  **Kiểm tra Thông điệp:**
    *   Kiểm tra xem `MessageName` có phải là "Create" hoặc "Update" hay không. Nếu không phải, hàm sẽ thoát mà không thực hiện hành động nào.
3.  **Lấy Target Entity:**
    *   Lấy thực thể mục tiêu (`Target`) từ `InputParameters`.
    *   Thực hiện lệnh `service.Retrieve` đầy đủ trên thực thể mục tiêu để đảm bảo tất cả các thuộc tính được tải (`new ColumnSet(true)`).
4.  **Xử lý sự kiện "Create":**
    *   Phần lớn logic chia sẻ cho các thực thể như `bsd_payment`, `opportunity`, `quote`, `salesorder`, `bsd_termination` đã bị comment out.
    *   **Logic đang hoạt động (cho thực thể `bsd_coowner`):**
        *   Truy xuất bản ghi `bsd_coowner` để lấy các trường liên quan: `bsd_reservation`, `bsd_optionentry`, hoặc `bsd_subsale`.
        *   Xác định ID Dự án (`Guid id`) bằng cách kiểm tra lần lượt các trường liên quan. Nếu không tìm thấy bất kỳ trường nào, ném ra lỗi `InvalidPluginExecutionException` ("Cannot create Co-owner!").
        *   Truy xuất thực thể `bsd_project` dựa trên ID dự án để lấy `bsd_projectcode` (`ProjectCode`).
        *   Thiết lập cờ truy cập: `FinTeam`, `CcrTeam`, `SaleTeam`, `SaleMgtTeam` đều được đặt là `true` (giá trị 1).
        *   Gọi `this.Get_TeamAccess` để tìm các Team liên quan đến `ProjectCode` và các phòng ban đã chọn.
        *   Nếu tìm thấy Team, lặp qua từng Team và gọi `this.Role_SharePrivileges` để cấp quyền Đọc, Ghi, Thêm (Read, Write, Append) cho bản ghi `bsd_coowner` này.
    *   **Chuyển giao cho Helper Class:** Khởi tạo và gọi `tmp.Run_Create(service1)` từ lớp `Plugin_AutoShareRecord_V2_1`.
5.  **Xử lý sự kiện "Update":**
    *   Logic chia sẻ cho `bsd_discount` đã bị comment out.
    *   **Chuyển giao cho Helper Class:** Khởi tạo và gọi `tmp.Run_Update(service1)` từ lớp `Plugin_AutoShareRecord_V2_1`.
6.  **Xử lý Chung:**
    *   Sau khi xử lý Create/Update, plugin tiếp tục gọi `Plugin_AutoShareRecord_V2_2.Run_ProcessShareTeam(service1)` để thực hiện các quy trình chia sẻ bổ sung, độc lập với thông điệp Create/Update.
7.  **Xử lý Lỗi:** Bắt mọi ngoại lệ (`Exception ex`) và ném ra `InvalidPluginExecutionException` với thông báo lỗi.

### Role_SharePrivileges(string targetEntityName, Guid targetRecordID, Guid teamID, bool read_Access, bool write_Access, bool append_Access, IOrganizationService orgService, bool FinTeam)

#### Chức năng tổng quát:
Thực hiện thao tác cấp quyền truy cập (Grant Access) cho một Team cụ thể đối với một bản ghi mục tiêu trong CRM.

#### Logic nghiệp vụ chi tiết:
1.  **Khởi tạo Tham chiếu:** Tạo `EntityReference` cho bản ghi mục tiêu (`targetEntityName`, `targetRecordID`) và Team (`teamID`).
2.  **Tính toán AccessRights:**
    *   Khởi tạo biến số nguyên `accessRights = 0`.
    *   Nếu `read_Access` là true, thêm quyền Đọc (1).
    *   Nếu `write_Access` là true, thêm quyền Ghi (2).
    *   Nếu `append_Access` là true, thêm quyền Thêm (Append) và Thêm vào (AppendTo) (16 | 4 = 20).
    *   Nếu `FinTeam` là true, thêm quyền đặc biệt `524288` (thường liên quan đến quyền Tài chính hoặc quyền tùy chỉnh).
3.  **Thực thi GrantAccessRequest:**
    *   Chuyển đổi giá trị số nguyên đã tính toán thành `AccessRights` enum (`finalAccessRights`).
    *   Tạo đối tượng `GrantAccessRequest`, thiết lập `PrincipalAccess` (Team và quyền) và `Target` (bản ghi).
    *   Thực thi yêu cầu thông qua `orgService.Execute()`.
4.  **Xử lý Lỗi:** Bắt và ném ra ngoại lệ nếu quá trình chia sẻ thất bại, kèm theo thông báo lỗi chi tiết.

### Role_ModifyAccess(string targetEntityName, Guid targetRecordID, Guid teamID, IOrganizationService orgService)

#### Chức năng tổng quát:
Sửa đổi quyền truy cập hiện có của một Team đối với một bản ghi cụ thể.

#### Logic nghiệp vụ chi tiết:
1.  **Khởi tạo Tham chiếu:** Tạo `EntityReference` cho bản ghi mục tiêu và Team.
2.  **Thiết lập AccessRights:** Thiết lập `AccessRights` cố định là `65536`. Trong CRM SDK, giá trị này thường đại diện cho quyền Modify Access.
3.  **Thực thi ModifyAccessRequest:**
    *   Tạo đối tượng `ModifyAccessRequest`, thiết lập `PrincipalAccess` (Team và quyền 65536) và `Target`.
    *   Thực thi yêu cầu thông qua `orgService.Execute()`.
4.  **Xử lý Lỗi:** Bắt và ném ra ngoại lệ nếu quá trình sửa đổi quyền thất bại.

### Role_RevokeAccess(string targetEntityName, Guid targetRecordID, Guid teamID, IOrganizationService orgService)

#### Chức năng tổng quát:
Thu hồi tất cả quyền truy cập đã chia sẻ của một Team đối với một bản ghi cụ thể.

#### Logic nghiệp vụ chi tiết:
1.  **Khởi tạo Tham chiếu:** Tạo `EntityReference` cho bản ghi mục tiêu và Team.
2.  **Thực thi RevokeAccessRequest:**
    *   Tạo đối tượng `RevokeAccessRequest`, chỉ định Team cần thu hồi quyền (`Revokee`) và Bản ghi mục tiêu (`Target`).
    *   Thực thi yêu cầu thông qua `orgService.Execute()`.
3.  **Xử lý Lỗi:** Bắt và ném ra ngoại lệ nếu quá trình thu hồi quyền thất bại.

### Get_TeamAccess(string ProjectCode, bool SaleTeam, bool CcrTeam, bool FinTeam, bool SaleMgtTeam)

#### Chức năng tổng quát:
Truy vấn CRM để lấy danh sách các Team có tên chứa mã dự án (`ProjectCode`) và thuộc các phòng ban nghiệp vụ được chỉ định.

#### Logic nghiệp vụ chi tiết:
1.  **Xây dựng FetchXML cơ bản:** Khởi tạo chuỗi FetchXML để truy vấn thực thể `team`, lấy các thuộc tính `name` và `teamid`.
2.  **Lọc theo ProjectCode:** Thêm điều kiện lọc chính (`filter type='and'`) yêu cầu tên Team phải chứa `ProjectCode` theo mẫu `%{ProjectCode}-%`.
3.  **Lọc theo Phòng ban (OR Filter):**
    *   Kiểm tra nếu ít nhất một cờ Team (`SaleTeam`, `CcrTeam`, `FinTeam`, `SaleMgtTeam`) là `true`, thêm một bộ lọc OR (`<filter type='or' >`).
    *   Đối với mỗi cờ `true`, thêm một điều kiện `operator='like'` để tìm kiếm các hằng số Team (ví dụ: `strSM` cho Sale, `strFIN` cho Finance) trong tên Team.
4.  **Thực thi Truy vấn:**
    *   Sử dụng `string.Format` để chèn `ProjectCode` vào chuỗi FetchXML đã xây dựng.
    *   Thực thi truy vấn bằng `service.RetrieveMultiple` với `FetchExpression` và trả về tập hợp các Team tìm thấy (`EntityCollection`).