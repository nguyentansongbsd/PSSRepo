# Phân tích mã nguồn: Plugin\_AutoShareRecord\_V2\_2.cs

## Tổng quan

Tệp mã nguồn `Plugin_AutoShareRecord_V2_2.cs` là một thành phần Plugin được thiết kế cho môi trường Microsoft Dynamics 365/CRM. Chức năng chính của plugin này là tự động chia sẻ (Auto-Share) các bản ghi (records) mới được tạo hoặc cập nhật thuộc các thực thể nghiệp vụ cụ thể (custom entities) cho các Nhóm (Teams) liên quan đến Dự án (Project) mà bản ghi đó thuộc về.

Plugin này hoạt động như một bộ điều phối, sử dụng tên logic của thực thể đang được xử lý (`target.LogicalName`) để xác định nhóm nào cần được cấp quyền truy cập (đọc, ghi, đính kèm, v.v.) và sau đó thực hiện lệnh gọi chia sẻ thông qua API của Dynamics 365.

## Chi tiết các Hàm (Functions/Methods)

### Plugin\_AutoShareRecord\_V2\_2(IOrganizationService \_service, ITracingService \_traceService, Entity \_target)

**Chức năng tổng quát:**
Đây là hàm khởi tạo (Constructor) của lớp Plugin, chịu trách nhiệm thiết lập các dịch vụ và đối tượng cần thiết để thực thi logic nghiệp vụ trong môi trường Dynamics 365.

**Logic nghiệp vụ chi tiết:**
1.  Hàm nhận ba tham số: `_service` (dịch vụ tổ chức để tương tác với dữ liệu CRM), `_traceService` (dịch vụ theo dõi để ghi nhật ký quá trình thực thi), và `_target` (thực thể đang được xử lý bởi plugin).
2.  Các tham số này được gán cho các biến thành viên tương ứng (`service`, `traceService`, `target`) để có thể sử dụng trong suốt vòng đời của đối tượng plugin.

### Run\_ProcessShareTeam(IPluginExecutionContext \_context)

**Chức năng tổng quát:**
Đây là phương thức chính điều phối logic chia sẻ. Nó xác định loại thực thể đang được xử lý và gọi hàm chia sẻ dự án (`Run_ShareTemProject`) với các tham số quyền và danh sách nhóm cụ thể.

**Logic nghiệp vụ chi tiết:**
1.  Bắt đầu bằng việc ghi dấu vết (trace) tên plugin.
2.  Sử dụng câu lệnh `switch` dựa trên tên logic của thực thể mục tiêu (`target.LogicalName`) để xác định hành động chia sẻ:
    *   **Các thực thể chung (ví dụ: `bsd_documents`, `bsd_genpaymentnotices`, `bsd_bankingloan`, v.v.):** Gọi `Run_ShareTemProject(true, List<string> teamShares)` để chia sẻ bản ghi với các nhóm được chỉ định (ví dụ: FINANCE-TEAM, SALE-MGT) với quyền Ghi (Write access).
    *   **`bsd_bulksendmailmanager`:** Chia sẻ bản ghi chính với `FINANCE-TEAM`. Sau đó, gọi `GetDetailBulkMailManager()` để lấy tất cả các bản ghi email chi tiết liên quan và lặp qua chúng, gọi `Run_ShareTemProject(false, List<string> teamShares, item)` để chia sẻ từng email chi tiết với `FINANCE-TEAM` (chỉ quyền Đọc).
    *   **`email`:** Chỉ thực hiện chia sẻ nếu bản ghi email chứa trường lookup `bsd_bulksendmailmanager`. Nếu có, chia sẻ với `FINANCE-TEAM` (có quyền Ghi).
    *   **`bsd_applybankaccountunits`:** Chia sẻ với `FINANCE-TEAM` (có quyền Ghi) và sau đó chia sẻ lại với `SALE-MGT` (chỉ quyền Đọc).
    *   **`bsd_updateestimatehandoverdate` / `bsd_updateestimatehandoverdatedetail`:** Thực hiện kiểm tra vai trò người dùng.
        *   Nếu người dùng hiện tại (`_context.UserId`) có vai trò "CLVN\_S&M\_Senior Sale Staff", chia sẻ với `SALE-TEAM` (có quyền Ghi).
        *   Ngược lại, chia sẻ với `FINANCE-TEAM` (có quyền Ghi).
    *   **`bsd_sharecustomerproject`:** Chia sẻ bản ghi chi tiết với 4 nhóm (FINANCE, SALE-MGT, CCR, SALE-ADMIN). Sau đó, truy xuất bản ghi Master (`bsd_sharecustomers`) thông qua lookup `bsd_sharecustomer`. Lặp qua các nhóm dự án đã tìm thấy và chia sẻ bản ghi Master này với các nhóm tương ứng (FINANCE, SALE-MGT, CCR, SALE-ADMIN) (có quyền Ghi).

### UserHasRole(Guid userId, string roleName)

**Chức năng tổng quát:**
Kiểm tra xem một người dùng cụ thể có được gán một vai trò bảo mật (security role) cụ thể hay không.

**Logic nghiệp vụ chi tiết:**
1.  Ghi dấu vết quá trình kiểm tra vai trò.
2.  Tạo một truy vấn `QueryExpression` trên thực thể `role`.
3.  Thiết lập các liên kết (Link Entities) để kết nối:
    *   `role` -> `systemuserroles` (qua `roleid`).
    *   `systemuserroles` -> `systemuser` (qua `systemuserid`).
4.  Thêm điều kiện lọc:
    *   Lọc trên `systemuser` bằng `userId` được cung cấp.
    *   Lọc trên `role` bằng `roleName` được cung cấp.
5.  Thực hiện truy vấn (`service.RetrieveMultiple`).
6.  Trả về `true` nếu tập hợp kết quả (`result.Entities`) chứa bất kỳ thực thể nào (tức là người dùng có vai trò đó), ngược lại trả về `false`.

### Run\_ShareTemProject(bool hasWrite = false, List<string> teamShares = null, Entity enShare = null)

**Chức năng tổng quát:**
Thực hiện logic cốt lõi để xác định dự án, tìm các nhóm dự án liên quan và gọi hàm chia sẻ thực tế.

**Logic nghiệp vụ chi tiết:**
1.  Ghi dấu vết tên thực thể đang được chia sẻ.
2.  Truy xuất toàn bộ dữ liệu của thực thể mục tiêu (`target`) và lưu vào biến `en`.
3.  Gọi `GetProject()` để lấy tham chiếu đến Dự án liên quan (`refProject`).
4.  Nếu không tìm thấy dự án, ghi dấu vết và thoát khỏi hàm.
5.  Gọi `GetProjectCode(refProject)` để lấy mã dự án (ví dụ: "P001").
6.  Gọi `GetTeams(projectCode)` để truy xuất tất cả các nhóm tiêu chuẩn (FINANCE, SALE, v.v.) được gắn với mã dự án đó.
7.  Xác định thực thể cần chia sẻ (`enShare`). Nếu `enShare` là null, sử dụng `target` làm thực thể chia sẻ.
8.  Lặp qua tập hợp các nhóm (`rs.Entities`) đã tìm thấy:
    *   Đối với mỗi nhóm, trích xuất tên nhóm và loại bỏ tiền tố mã dự án (ví dụ: chuyển "P001-FINANCE-TEAM" thành "FINANCE-TEAM").
    *   Kiểm tra xem tên nhóm đã được làm sạch có nằm trong danh sách `teamShares` được yêu cầu hay không.
    *   Nếu có, gọi `ShareTeams(enShare.ToEntityReference(), team.ToEntityReference(), hasWrite)` để cấp quyền truy cập.

### ShareTeams(EntityReference sharedRecord, EntityReference shareTeams, bool hasWriteShare)

**Chức năng tổng quát:**
Thực hiện lệnh gọi API Dynamics 365 để cấp quyền truy cập (sharing) cho một bản ghi cụ thể tới một nhóm.

**Logic nghiệp vụ chi tiết:**
1.  Ghi dấu vết bắt đầu hàm.
2.  Khởi tạo quyền truy cập cơ bản (`Access_Rights`) bao gồm: Đọc (`ReadAccess`), Đính kèm (`AppendAccess`), và Đính kèm vào (`AppendToAccess`).
3.  Kiểm tra tham số `hasWriteShare`. Nếu là `true`, thêm quyền Ghi (`WriteAccess`) vào `Access_Rights`.
4.  Tạo đối tượng `GrantAccessRequest`:
    *   Thiết lập `PrincipalAccess` với mặt nạ quyền (`AccessMask`) đã xác định và Nhóm (`Principal`) sẽ nhận quyền.
    *   Thiết lập `Target` là bản ghi cần chia sẻ.
5.  Thực thi yêu cầu `GrantAccessRequest` thông qua `service.Execute()`.

### GetProjectCode(EntityReference refProject)

**Chức năng tổng quát:**
Truy xuất mã dự án (`bsd_projectcode`) từ tham chiếu thực thể Dự án.

**Logic nghiệp vụ chi tiết:**
1.  Sử dụng FetchXML để truy vấn thực thể `bsd_project`.
2.  Lọc theo ID của dự án được cung cấp (`refProject.Id`).
3.  Đảm bảo rằng trường `bsd_projectcode` không rỗng (`not-null`).
4.  Thực hiện truy vấn (`service.RetrieveMultiple`).
5.  Nếu tìm thấy kết quả, trả về giá trị của trường `bsd_projectcode` từ thực thể đầu tiên.
6.  Nếu không tìm thấy hoặc không có mã, trả về chuỗi rỗng.

### GetTeams(string projectCode)

**Chức năng tổng quát:**
Truy xuất tập hợp các Nhóm (Team) cụ thể trong CRM dựa trên mã dự án.

**Logic nghiệp vụ chi tiết:**
1.  Sử dụng FetchXML để truy vấn thực thể `team`.
2.  Thiết lập điều kiện lọc (`condition operator="in"`) để tìm các nhóm có tên theo định dạng: `[projectCode]-[TeamSuffix]`. Các hậu tố nhóm được tìm kiếm cố định là: `CCR-TEAM`, `FINANCE-TEAM`, `SALE-TEAM`, `SALE-MGT`, và `SALE-ADMIN`.
3.  Thực hiện truy vấn và trả về tập hợp các thực thể Nhóm (`EntityCollection`).
4.  Bao gồm cơ chế xử lý lỗi để ghi dấu vết nếu truy vấn thất bại.

### GetDetailBulkMailManager()

**Chức năng tổng quát:**
Truy xuất tất cả các bản ghi email chi tiết được liên kết với một bản ghi `bsd_bulksendmailmanager` cụ thể.

**Logic nghiệp vụ chi tiết:**
1.  Tạo một truy vấn `QueryExpression` trên thực thể `email`.
2.  Yêu cầu tất cả các cột (`AllColumns = true`).
3.  Thêm điều kiện lọc: trường lookup `bsd_bulksendmailmanager` phải bằng ID của thực thể mục tiêu hiện tại (`en.Id`).
4.  Thực hiện truy vấn và trả về tập hợp các bản ghi email.

### GetProject()

**Chức năng tổng quát:**
Xác định và trả về tham chiếu đến thực thể Dự án (`EntityReference`) liên quan đến thực thể mục tiêu hiện tại.

**Logic nghiệp vụ chi tiết:**
1.  Sử dụng một câu lệnh `switch` lớn dựa trên `target.LogicalName` để xác định cách truy xuất tham chiếu dự án, vì liên kết dự án có thể là trực tiếp hoặc gián tiếp (thông qua một bản ghi Master/Parent).
2.  **Trường hợp liên kết trực tiếp:** Đối với nhiều thực thể (ví dụ: `bsd_genaratewarningnotices`, `bsd_waiverapproval`), tham chiếu dự án được lấy trực tiếp từ trường `bsd_project` trên thực thể mục tiêu (`en`).
3.  **Trường hợp liên kết gián tiếp (Master/Detail):** Đối với các thực thể chi tiết (ví dụ: `bsd_updateduedatedetail`, `bsd_updatefuldetail`, `email`):
    *   Đầu tiên, truy xuất tham chiếu đến bản ghi Master (ví dụ: `enMasterRef = (EntityReference)en["bsd_updateduedate"]`).
    *   Sau đó, truy xuất bản ghi Master đó (`enMaster = service.Retrieve(...)`).
    *   Cuối cùng, lấy tham chiếu dự án từ bản ghi Master (`enProjectRef2 = (EntityReference)enMaster["bsd_project"]`).
4.  Trả về `EntityReference` của Dự án đã tìm thấy. Nếu không tìm thấy liên kết dự án (ví dụ: `bsd_documents` không chứa `bsd_project`), trả về `null`.