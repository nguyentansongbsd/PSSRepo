# Phân tích mã nguồn: Plugin_AutoShareRecord_V2_1.cs

## Tổng quan

Tệp mã nguồn `Plugin_AutoShareRecord_V2_1.cs` chứa logic của một Plugin Microsoft Dynamics 365/CRM được thiết kế để tự động chia sẻ (Auto-Share) các bản ghi nghiệp vụ (Entity Records) với các Nhóm (Teams) cụ thể dựa trên loại bản ghi (Entity Logical Name) và trạng thái của nó.

Plugin này hoạt động chủ yếu trên các sự kiện `Create` và `Update` của nhiều thực thể tùy chỉnh (custom entities) liên quan đến quy trình bán hàng, tài chính, và quản lý dự án (ví dụ: `bsd_phaseslaunch`, `salesorder`, `bsd_paymentscheme`). Logic chia sẻ được xác định dựa trên Mã Dự án (`Project Code`) liên quan đến bản ghi, đảm bảo rằng bản ghi chỉ được chia sẻ với các nhóm thuộc dự án đó (ví dụ: `PJT-CCR-TEAM`, `PJT-FINANCE-TEAM`).

## Chi tiết các Hàm (Functions/Methods)

### Plugin_AutoShareRecord_V2_1(IOrganizationService _service, ITracingService _traceService, Entity _target)

**Chức năng tổng quát:**
Đây là hàm khởi tạo (constructor) của lớp Plugin, chịu trách nhiệm thiết lập các đối tượng dịch vụ và ngữ cảnh cần thiết để tương tác với môi trường CRM.

**Logic nghiệp vụ chi tiết:**
1.  Hàm nhận ba tham số: `_service` (dịch vụ tổ chức CRM), `_traceService` (dịch vụ theo dõi/ghi log), và `_target` (bản ghi đang được xử lý).
2.  Gán các tham số này vào các biến thành viên của lớp (`service`, `traceService`, `target`) để các phương thức khác có thể sử dụng.

### Run_Update(IPluginExecutionContext _context)

**Chức năng tổng quát:**
Thực thi logic chia sẻ bản ghi khi một bản ghi được cập nhật (Update event) trong CRM.

**Logic nghiệp vụ chi tiết:**
1.  Ghi dấu vết (trace) để xác định Plugin đang chạy trên thực thể nào.
2.  Sử dụng câu lệnh `switch` để kiểm tra tên logic của thực thể mục tiêu (`target.LogicalName`):
    *   **`bsd_phaseslaunch`**: Gọi hàm `Run_PhasesLaunch()` để xử lý logic chia sẻ phức tạp cho việc ra mắt giai đoạn.
    *   **`bsd_paymentscheme`**: Gọi hàm `Run_PaymentScheme()` để xử lý logic chia sẻ cho sơ đồ thanh toán.
    *   **`bsd_event`**: Gọi `ShareTeams_OneEntity` để chia sẻ bản ghi với các nhóm CCR, FINANCE, SALE, SALE-MGT (quyền 2 - Write/Share), và SALE-ADMIN. Việc chia sẻ chỉ xảy ra nếu trạng thái bản ghi là `100000000`.
    *   **`bsd_updatepricelist`**: Gọi `ShareTeams_OneEntity` để chia sẻ với SALE-ADMIN và SALE-MGT (quyền 1 - Write). Việc chia sẻ chỉ xảy ra nếu trạng thái bản ghi là `100000000`.
    *   **`bsd_discount`**:
        *   Kiểm tra điều kiện: Nếu thuộc tính boolean `bsd_applyafterpl` là `false`, Plugin dừng lại (`break`).
        *   Nếu điều kiện thỏa mãn, gọi `ShareTeams_OneEntity` với cấu hình quyền tương tự như `bsd_event`.

### Run_Create(IPluginExecutionContext _context)

**Chức năng tổng quát:**
Thực thi logic chia sẻ bản ghi khi một bản ghi được tạo mới (Create event) trong CRM.

**Logic nghiệp vụ chi tiết:**
1.  Ghi dấu vết để xác định Plugin đang chạy.
2.  Sử dụng câu lệnh `switch` để kiểm tra tên logic của thực thể mục tiêu (`target.LogicalName`):
    *   **`salesorder`**: Gọi `ShareTeams_OE` (Share Teams Option Entry/Sales Order) với quyền Write (1) cho CCR, FINANCE, SALE-MGT, và SALE-ADMIN.
    *   **`quote`, `bsd_followuplist`, `opportunity`, `bsd_quotation`**: Gọi `ShareTeams_OneEntity` với quyền Write (1) cho CCR, FINANCE, SALE-MGT, và SALE-ADMIN.
    *   **`bsd_advancepayment`**: Chia sẻ với FINANCE-TEAM (quyền 1 - Write).
    *   **`bsd_updateactualarea`, `bsd_updateactualareaapprove`**: Chia sẻ với SALE-MGT (quyền 1 - Write).
    *   **`bsd_appendixcontract`, `bsd_transfermoney`**: Chia sẻ với CCR-TEAM và FINANCE-TEAM (quyền 1 - Write).
    *   **`bsd_assign`**: Chia sẻ với CCR-TEAM (quyền 1 - Write) và FINANCE-TEAM (quyền 0 - Read).
    *   **`bsd_paymentschemedetail`**: Gọi `ShareTeams_Ins` (Share Teams Installment) với quyền Share (2) cho tất cả các nhóm (CCR, FINANCE, SALE-MGT, SALE-ADMIN).
    *   **Các thực thể liên quan đến thông báo/chấm dứt hợp đồng (`bsd_terminateletter`, `bsd_customernotices`, v.v.)**: Chia sẻ với FINANCE-TEAM (quyền 2 - Share).
    *   **Các thực thể liên quan đến thanh toán/hóa đơn (`bsd_applydocument`, `bsd_refund`, v.v.)**: Chia sẻ với FINANCE-TEAM (quyền 2 - Share) và SALE-MGT (quyền 0 - Read).
    *   **`bsd_termination`**: Chia sẻ với tất cả các nhóm (CCR, FINANCE, SALE-MGT, SALE-ADMIN) với quyền Write (1).
    *   **Các thực thể liên quan đến cập nhật ngày đáo hạn (`bsd_updateduedate`, v.v.)**: Chia sẻ với FINANCE-TEAM, SALE-MGT, và SALE-ADMIN (quyền 1 - Write).
    *   **Các thực thể liên quan đến cập nhật ngày đáo hạn kỳ cuối**: Chia sẻ với FINANCE-TEAM (quyền 1 - Write).
    *   **`bsd_confirmpayment`, `bsd_confirmapplydocument`**:
        *   Truy xuất bản ghi mục tiêu để kiểm tra xem nó có chứa trường `bsd_project` hay không.
        *   Nếu có `bsd_project`, gọi `ShareTeams_OneEntity` (FINANCE-TEAM quyền 2, SALE-MGT quyền 0).
        *   Nếu không có `bsd_project`, gọi hàm đặc biệt `ShareTeams_ConfirmPayment(_context)` để xử lý logic chia sẻ dựa trên vai trò người dùng.

### GetAccessRights(int accessType)

**Chức năng tổng quát:**
Chuyển đổi một giá trị số nguyên thành tập hợp các quyền truy cập CRM (`AccessRights`) cần thiết cho việc chia sẻ.

**Logic nghiệp vụ chi tiết:**
1.  Khởi tạo quyền truy cập cơ bản: `ReadAccess`, `AppendAccess`, và `AppendToAccess`.
2.  Sử dụng `switch` để bổ sung quyền dựa trên `accessType`:
    *   `case 1`: Thêm `WriteAccess`.
    *   `case 2`: Thêm `WriteAccess` và `ShareAccess`.
3.  Trả về tập hợp các quyền đã xác định.

### ShareTeams(EntityReference sharedRecord, EntityReference shareTeams, int accessType)

**Chức năng tổng quát:**
Thực hiện lệnh gọi API CRM để chia sẻ một bản ghi cụ thể với một Nhóm (Team) cụ thể với mức quyền truy cập đã cho.

**Logic nghiệp vụ chi tiết:**
1.  Ghi dấu vết ID của Nhóm được chia sẻ.
2.  Tạo một đối tượng `GrantAccessRequest`.
3.  Thiết lập `PrincipalAccess`:
    *   `AccessMask`: Lấy quyền truy cập bằng cách gọi `GetAccessRights(accessType)`.
    *   `Principal`: Là tham chiếu đến Nhóm (`shareTeams`).
4.  Thiết lập `Target`: Là tham chiếu đến bản ghi cần chia sẻ (`sharedRecord`).
5.  Thực thi `grantAccessRequest` thông qua `service.Execute()`.
6.  Bắt và ghi lại bất kỳ ngoại lệ nào xảy ra trong quá trình chia sẻ.

### GetProjectCode()

**Chức năng tổng quát:**
Truy xuất Mã Dự án (`bsd_projectcode`) liên quan đến bản ghi mục tiêu hiện tại. Mã Dự án này được sử dụng để xác định các Nhóm (Teams) cụ thể của dự án.

**Logic nghiệp vụ chi tiết:**
1.  Khởi tạo `projectCode` là chuỗi rỗng.
2.  Sử dụng `switch` để xác định cách truy xuất tham chiếu Dự án (`refProject`) dựa trên thực thể mục tiêu (`target.LogicalName`):
    *   Đối với các thực thể như `bsd_phaseslaunch`, `quote`: Truy xuất trực tiếp trường `bsd_projectid`.
    *   Đối với `bsd_updateactualarea`: Phải truy xuất `bsd_units` trước, sau đó truy xuất `bsd_projectcode` từ bản ghi Unit.
    *   Đối với `bsd_termination`: Phải truy xuất `bsd_followuplist` trước, sau đó truy xuất `bsd_project` từ bản ghi Follow Up List.
    *   Đối với `bsd_paymentschemedetail`: Phải truy xuất `bsd_paymentscheme` trước, sau đó truy xuất `bsd_project` từ bản ghi Payment Scheme.
    *   **Mặc định (Default)**: Truy xuất trực tiếp trường `bsd_project`.
3.  Nếu không tìm thấy tham chiếu dự án, trả về chuỗi rỗng.
4.  Nếu tìm thấy `refProject`, sử dụng FetchXML để truy vấn thực thể `bsd_project` dựa trên ID dự án và lấy giá trị của thuộc tính `bsd_projectcode`.
5.  Trả về `projectCode`.

### GetTeams(string projectCode)

**Chức năng tổng quát:**
Truy vấn CRM để lấy danh sách các Nhóm (Team) cụ thể thuộc về một Mã Dự án đã cho.

**Logic nghiệp vụ chi tiết:**
1.  Kiểm tra nếu `projectCode` rỗng, trả về `null`.
2.  Sử dụng FetchXML để truy vấn thực thể `team`.
3.  Lọc các Nhóm có tên khớp với mẫu `{projectCode}-TEAM_SUFFIX` (ví dụ: CCR-TEAM, FINANCE-TEAM, SALE-MGT, v.v.).
4.  Trả về tập hợp các thực thể Nhóm tìm được.

### Run_PhasesLaunch()

**Chức năng tổng quát:**
Thực hiện logic chia sẻ phức tạp cho thực thể `bsd_phaseslaunch` khi nó đạt trạng thái "Launched" (100000000).

**Logic nghiệp vụ chi tiết:**
1.  Kiểm tra nếu trạng thái của bản ghi là `100000000` (Launched).
2.  Lấy `projectCode` và các `Teams` liên quan đến dự án đó.
3.  Nếu tìm thấy Teams:
    *   Lấy tham chiếu đến bản ghi `PhasesLaunch` hiện tại.
    *   Truy vấn các bản ghi liên quan: `Promotions`, `HandoverCondition`, và `DiscountList` (nếu có). Nếu có `DiscountList`, truy vấn thêm các bản ghi `Discounts` liên quan.
    *   Lặp qua từng Team trong danh sách Teams của dự án:
        *   Xác định `accessType`: Nếu tên Team là `{projectCode}-SALE-MGT`, `accessType` là 2 (Write/Share); nếu không, là 0 (Read).
        *   Chia sẻ bản ghi `PhasesLaunch` chính với Team.
        *   Nếu có `DiscountList`, chia sẻ `DiscountList` và tất cả các bản ghi `Discount` liên quan với Team.
        *   Chia sẻ tất cả các bản ghi `Promotion` liên quan với Team.
        *   Chia sẻ tất cả các bản ghi `HandoverCondition` liên quan với Team.

### GetDiscounts(EntityReference refDiscountList)

**Chức năng tổng quát:**
Truy vấn tất cả các bản ghi `bsd_discount` được liên kết với một `DiscountList` cụ thể thông qua mối quan hệ N:N.

**Logic nghiệp vụ chi tiết:**
1.  Sử dụng FetchXML để truy vấn thực thể `bsd_discount`.
2.  Sử dụng `link-entity` (intersect entity `bsd_bsd_discounttype_bsd_discount`) để lọc các bản ghi `bsd_discount` có liên kết với `refDiscountList.Id`.
3.  Trả về tập hợp các bản ghi `bsd_discount`.

### GetPromotions(EntityReference refPhasesLaunch)

**Chức năng tổng quát:**
Truy vấn tất cả các bản ghi `bsd_promotion` liên quan trực tiếp đến một `PhasesLaunch` cụ thể.

**Logic nghiệp vụ chi tiết:**
1.  Sử dụng FetchXML để truy vấn thực thể `bsd_promotion`.
2.  Lọc các bản ghi có trường `bsd_phaselaunch` bằng với ID của `refPhasesLaunch`.
3.  Trả về tập hợp các bản ghi `bsd_promotion`.

### GetHandoverCondition(EntityReference refPhasesLaunch)

**Chức năng tổng quát:**
Truy vấn tất cả các bản ghi `bsd_packageselling` (được sử dụng cho điều kiện bàn giao) liên quan đến một `PhasesLaunch` cụ thể thông qua mối quan hệ N:N.

**Logic nghiệp vụ chi tiết:**
1.  Sử dụng FetchXML để truy vấn thực thể `bsd_packageselling`.
2.  Sử dụng `link-entity` (intersect entity `bsd_bsd_phaseslaunch_bsd_packageselling`) để lọc các bản ghi `bsd_packageselling` có liên kết với `refPhasesLaunch.Id`.
3.  Trả về tập hợp các bản ghi `bsd_packageselling`.

### Run_PaymentScheme()

**Chức năng tổng quát:**
Thực hiện logic chia sẻ cho thực thể `bsd_paymentscheme` khi nó đạt trạng thái "Confirm" (100000000).

**Logic nghiệp vụ chi tiết:**
1.  Kiểm tra nếu trạng thái của bản ghi là `100000000` (Confirm).
2.  Lấy `projectCode` và các `Teams` liên quan.
3.  Nếu tìm thấy Teams:
    *   Lấy tham chiếu đến bản ghi `PaymentScheme` hiện tại.
    *   Truy vấn các bản ghi `PaymentSchemeDetails` liên quan bằng cách gọi `GetPaymentSchemeDetails`.
    *   Lặp qua từng Team:
        *   Xác định `accessType`: Nếu tên Team là `{projectCode}-CCR-TEAM`, `accessType` là 2 (Write/Share); nếu không, là 0 (Read).
        *   Chia sẻ bản ghi `PaymentScheme` chính với Team.
        *   Chia sẻ tất cả các bản ghi `PaymentSchemeDetails` liên quan với Team.

### GetPaymentSchemeDetails(EntityReference refPS)

**Chức năng tổng quát:**
Truy vấn tất cả các chi tiết sơ đồ thanh toán (`bsd_paymentschemedetail`) liên quan đến một sơ đồ thanh toán chính (`bsd_paymentscheme`).

**Logic nghiệp vụ chi tiết:**
1.  Sử dụng FetchXML để truy vấn thực thể `bsd_paymentschemedetail`.
2.  Lọc các bản ghi có trường `bsd_paymentscheme` bằng với ID của `refPS`.
3.  Trả về tập hợp các bản ghi chi tiết.

### ShareTeams_OneEntity(Dictionary<string, int> listTeamRights, int status = -999)

**Chức năng tổng quát:**
Hàm chia sẻ chung, chia sẻ bản ghi mục tiêu với một danh sách các Nhóm dự án được xác định trước, có thể tùy chọn kiểm tra trạng thái của bản ghi.

**Logic nghiệp vụ chi tiết:**
1.  **Kiểm tra trạng thái:** Nếu tham số `status` khác `-999`, hàm kiểm tra xem bản ghi mục tiêu có chứa trường `statuscode` và giá trị của nó có khớp với `status` không. Nếu không khớp, hàm dừng lại.
2.  Lấy `projectCode` và các `Teams` liên quan.
3.  Nếu tìm thấy Teams:
    *   Chuyển đổi tập hợp Teams thành một Dictionary để tra cứu nhanh bằng tên Team.
    *   Lặp qua `listTeamRights` (Key là Team Suffix, Value là Access Type).
    *   Xây dựng tên Team đầy đủ (`{projectCode}-{TeamSuffix}`).
    *   Nếu Team tồn tại trong danh sách Teams của dự án, gọi `ShareTeams` để chia sẻ bản ghi mục tiêu với Team đó với mức quyền đã định.

### ShareTeams_OE(Dictionary<string, int> listTeamRights)

**Chức năng tổng quát:**
Chia sẻ bản ghi mục tiêu (thường là Sales Order hoặc Option Entry) với các Nhóm dự án và thêm quyền truy cập cho Chủ sở hữu (Owner) của Quote liên quan.

**Logic nghiệp vụ chi tiết:**
1.  Lấy `projectCode` và các `Teams` liên quan.
2.  **Chia sẻ với Teams:** Thực hiện logic chia sẻ tương tự như `ShareTeams_OneEntity` (chia sẻ bản ghi mục tiêu với các Nhóm dự án được chỉ định trong `listTeamRights`).
3.  **Chia sẻ với Quote Owner:**
    *   Truy xuất bản ghi mục tiêu để lấy `quoteid` (tham chiếu đến Quote).
    *   Truy xuất bản ghi Quote để lấy `ownerid` (Chủ sở hữu Quote).
    *   Nếu tìm thấy Chủ sở hữu Quote, gọi `ShareTeams` để chia sẻ bản ghi mục tiêu với Chủ sở hữu Quote với quyền truy cập 0 (Read/Append/AppendTo).

### GetProjectCodes_ConfirmPayment(Guid userId)

**Chức năng tổng quát:**
Truy vấn các Mã Dự án mà người dùng hiện tại là thành viên của các Nhóm không phải mặc định.

**Logic nghiệp vụ chi tiết:**
1.  Sử dụng FetchXML để truy vấn thực thể `team`.
2.  Sử dụng `link-entity` với `teammembership` để lọc các Nhóm mà `userId` là thành viên.
3.  Lọc các Nhóm không phải là Nhóm mặc định (`isdefault=0`).
4.  Lặp qua kết quả, trích xuất tên Nhóm, phân tích cú pháp để loại bỏ hai phần cuối (ví dụ: từ "PJT-FINANCE-TEAM" lấy "PJT"), và trả về danh sách các Mã Dự án duy nhất.

### GetTeam_ConfirmPayment(Guid userId)

**Chức năng tổng quát:**
Truy vấn các Nhóm FINANCE-TEAM và SALE-MGT cho các dự án mà người dùng hiện tại có liên quan.

**Logic nghiệp vụ chi tiết:**
1.  Gọi `GetProjectCodes_ConfirmPayment(userId)` để lấy danh sách các Mã Dự án liên quan đến người dùng.
2.  Khởi tạo một `EntityCollection` rỗng.
3.  Lặp qua từng `projectCode` tìm được:
    *   Sử dụng FetchXML để truy vấn các Nhóm có tên là `{projectCode}-FINANCE-TEAM` và `{projectCode}-SALE-MGT`.
    *   Thêm các Nhóm tìm được vào `EntityCollection` kết quả.
4.  Trả về tập hợp các Nhóm này.

### IsSystemAdmin(Guid userId)

**Chức năng tổng quát:**
Kiểm tra xem người dùng hiện tại có vai trò "System Administrator" (Quản trị viên Hệ thống) hay không.

**Logic nghiệp vụ chi tiết:**
1.  Sử dụng FetchXML để truy vấn thực thể `systemuser`.
2.  Sử dụng `link-entity` qua `systemuserroles` và `role` để kiểm tra xem người dùng có vai trò có tên là "System Administrator" hay không.
3.  Trả về `true` nếu tìm thấy vai trò đó, ngược lại là `false`.

### GetTeam_ConfirmPayment_All(Guid userId)

**Chức năng tổng quát:**
Truy vấn tất cả các Nhóm FINANCE-TEAM và SALE-MGT trên toàn bộ hệ thống (được sử dụng cho Quản trị viên Hệ thống).

**Logic nghiệp vụ chi tiết:**
1.  Sử dụng FetchXML để truy vấn thực thể `team`.
2.  Lọc các Nhóm có tên kết thúc bằng `-FINANCE-TEAM` HOẶC `-SALE-MGT`.
3.  Trả về tập hợp tất cả các Nhóm này.

### ShareTeams_ConfirmPayment(IPluginExecutionContext _context)

**Chức năng tổng quát:**
Thực hiện logic chia sẻ đặc biệt cho các bản ghi xác nhận thanh toán/áp dụng tài liệu, dựa trên việc người dùng có phải là Quản trị viên Hệ thống hay không.

**Logic nghiệp vụ chi tiết:**
1.  **Xác định phạm vi Nhóm:**
    *   Nếu `IsSystemAdmin(_context.UserId)` là `true`, gọi `GetTeam_ConfirmPayment_All` để lấy tất cả các Nhóm liên quan.
    *   Nếu là người dùng thông thường, gọi `GetTeam_ConfirmPayment` để lấy các Nhóm liên quan đến dự án của người dùng.
2.  Nếu tìm thấy Teams:
    *   Lấy tham chiếu đến bản ghi mục tiêu.
    *   Lặp qua từng Team:
        *   Xác định `accessType`: Nếu tên Team kết thúc bằng `-FINANCE-TEAM`, `accessType` là 2 (Write/Share); nếu không (ví dụ: SALE-MGT), `accessType` là 0 (Read).
        *   Gọi `ShareTeams` để chia sẻ bản ghi mục tiêu với Team đó.

### ShareTeams_Ins(Dictionary<string, int> listTeamRights)

**Chức năng tổng quát:**
Chia sẻ bản ghi chi tiết kỳ thanh toán (`bsd_paymentschemedetail`) với các Nhóm dự án và thêm quyền truy cập cho Chủ sở hữu của bản ghi Đặt chỗ (Reservation) liên quan.

**Logic nghiệp vụ chi tiết:**
1.  Lấy `projectCode` và các `Teams` liên quan.
2.  **Chia sẻ với Teams:** Thực hiện logic chia sẻ tương tự như `ShareTeams_OneEntity` (chia sẻ bản ghi mục tiêu với các Nhóm dự án được chỉ định trong `listTeamRights`).
3.  **Chia sẻ với Reservation Owner:**
    *   Truy xuất bản ghi mục tiêu để lấy `bsd_reservation` (tham chiếu đến bản ghi Đặt chỗ/Quote).
    *   Truy xuất bản ghi Reservation để lấy `ownerid` (Chủ sở hữu).
    *   Nếu tìm thấy Chủ sở hữu, gọi `ShareTeams` để chia sẻ bản ghi mục tiêu với Chủ sở hữu đó với quyền truy cập 2 (Write/Share).