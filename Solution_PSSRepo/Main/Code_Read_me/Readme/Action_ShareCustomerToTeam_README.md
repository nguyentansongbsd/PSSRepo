# Phân tích mã nguồn: Action_ShareCustomerToTeam.cs

## Tổng quan

Tệp mã nguồn `Action_ShareCustomerToTeam.cs` chứa một Plugin Dynamics 365/Power Platform được kích hoạt bởi một Custom Action có tên là `bsd_Action_ShareCustomerToTeam`.

Plugin này đóng vai trò là bộ điều phối trung tâm cho việc quản lý quyền truy cập (sharing) các bản ghi Khách hàng (Contact hoặc Account) cho các Team nghiệp vụ chuyên biệt (như CCR, Finance, Sale Management, Sale Admin) thuộc các dự án cụ thể.

Plugin hỗ trợ ba luồng nghiệp vụ chính:
1.  **Yêu cầu Chia sẻ:** Xử lý việc người dùng chọn dự án/team và tạo các bản ghi yêu cầu chia sẻ (`bsd_sharecustomers`).
2.  **Chia sẻ Tự động:** Tự động chia sẻ khách hàng cho các team nghiệp vụ liên quan nếu người dùng chỉ thuộc một dự án duy nhất.
3.  **Phê duyệt Chia sẻ:** Thực hiện việc cấp quyền truy cập thực tế khi một yêu cầu chia sẻ được phê duyệt.

## Chi tiết các Hàm (Functions/Methods)

### EndsWithAny(string mainString, List<string> endings)

*   **Chức năng tổng quát:** Hàm tiện ích này kiểm tra xem một chuỗi ký tự có kết thúc bằng bất kỳ chuỗi con nào được cung cấp trong danh sách hay không, không phân biệt chữ hoa/thường.
*   **Logic nghiệp vụ chi tiết:**
    1.  Hàm thực hiện kiểm tra tính hợp lệ của đầu vào: nếu `mainString` rỗng hoặc `endings` là null/rỗng, hàm trả về `false`.
    2.  Hàm lặp qua từng chuỗi trong danh sách `endings`.
    3.  Trong mỗi lần lặp, nó sử dụng phương thức `mainString.EndsWith(ending, StringComparison.OrdinalIgnoreCase)` để so sánh.
    4.  Nếu tìm thấy bất kỳ chuỗi kết thúc nào khớp, hàm sẽ trả về `true` ngay lập tức.
    5.  Nếu vòng lặp hoàn thành mà không tìm thấy kết quả khớp, hàm trả về `false`.

### Execute(IServiceProvider serviceProvider)

*   **Chức năng tổng quát:** Điểm vào chính của Plugin, chịu trách nhiệm phân luồng xử lý dựa trên các tham số đầu vào của Custom Action.
*   **Logic nghiệp vụ chi tiết:**
    1.  **Khởi tạo Môi trường:** Khởi tạo các đối tượng CRM tiêu chuẩn (`context`, `factory`, `traceService`). Đặc biệt, `IOrganizationService` được tạo bằng một `AdminID` cố định (`d90ce220-655a-e811-812e-3863bb36dc00`) để đảm bảo quyền thực thi đầy đủ.
    2.  **Xử lý Phê duyệt Chia sẻ (Luồng 1):**
        *   Kiểm tra nếu tham số `sharecusid` tồn tại.
        *   Nếu có, gọi hàm `ShareCustomerRecordToProjectTeams()` để thực hiện việc chia sẻ quyền truy cập thực tế và kết thúc thực thi.
    3.  **Xử lý Tham số Khách hàng:** Lấy `type` (xác định loại thực thể: 0/2 cho Contact, 1/3 cho Account, 10 cho Lead) và `id` (ID của khách hàng, có thể là danh sách ID).
    4.  **Tạo Yêu cầu Chia sẻ (Luồng 2):**
        *   Kiểm tra nếu tham số `CreateShareCustomer` và `idTeam` (danh sách ID Team được chọn) tồn tại.
        *   Nếu có, gọi hàm `CreateShareCustomer()` để tạo các bản ghi yêu cầu chia sẻ và kết thúc thực thi.
    5.  **Xử lý Zalo/Lead Conversion (Luồng 3):**
        *   Nếu tham số `idform` (ID của Lead) tồn tại, thực hiện logic liên quan đến Zalo:
            *   Cập nhật trạng thái của Lead sang `statuscode = 2`.
            *   Kiểm tra xem Lead có liên kết với dự án (`bsd_projectcode`) không. Nếu không, ném lỗi.
            *   Kiểm tra xem đã có bản ghi `bsd_zaloinfor` nào cho Zalo ID này chưa.
            *   Nếu chưa có, tạo bản ghi `bsd_zaloinfor` mới, liên kết Contact (ID lấy từ `id`) với Project (tìm kiếm qua `bsd_projectcode`) và các thông tin Zalo khác.
    6.  **Xử lý Chia sẻ Tự động/Chọn Team (Luồng 4):**
        *   Xác định tên logic của thực thể (`fieldName`). Nếu `type` là 2 hoặc 3, gọi `ShareCustomerToTeam()` (luồng chia sẻ trực tiếp).
        *   Lặp qua từng ID khách hàng trong `arrID`.
        *   **Tìm Team Người dùng:** Gọi `GetListTeamOfCurrentUser()` để lấy danh sách các team dự án mà người dùng hiện tại là thành viên. Nếu không có team nào, ném lỗi.
        *   **Trường hợp 1: Một Team Duy nhất (Chia sẻ Tự động):**
            *   Nếu người dùng chỉ thuộc một team dự án duy nhất VÀ không phải luồng tạo yêu cầu chia sẻ.
            *   Hàm tìm kiếm các team nghiệp vụ (CCR, FINANCE, SALE-MGT, SALE-ADMIN) liên quan đến mã dự án của team đó.
            *   Gọi `ShareTeams()` để chia sẻ bản ghi khách hàng và các bản ghi liên quan (Contact/Account, các Contact phụ) cho tất cả các team nghiệp vụ này.
        *   **Trường hợp 2: Nhiều Team (Yêu cầu Chọn):**
            *   Tạo danh sách các mã dự án duy nhất (`TeamReturn`) mà người dùng thuộc về.
            *   Nếu danh sách chỉ có 1 dự án, thực hiện logic chia sẻ tự động như trên (bằng cách gọi `CreateShareCustomer` với team đó).
            *   Nếu có nhiều hơn 1 dự án, serialize danh sách các dự án/team này và đặt vào `context.OutputParameters["entityColl"]` để trả về cho giao diện người dùng lựa chọn.

### ShareCustomerToTeam(string[] arrCus, string fieldName)

*   **Chức năng tổng quát:** Thực hiện chia sẻ quyền truy cập khách hàng và các bản ghi liên quan cho một danh sách Team đã được chọn.
*   **Logic nghiệp vụ chi tiết:**
    1.  Hàm nhận mảng ID khách hàng (`arrCus`) và mảng ID Team đích (`arrTeam`).
    2.  Lặp qua từng khách hàng và từng team đích.
    3.  Đối với mỗi Team ID, hàm truy vấn tên Team để trích xuất `projectCode`.
    4.  Sử dụng `projectCode` để truy vấn 4 Team nghiệp vụ tiêu chuẩn (CCR, FINANCE, SALE-MGT, SALE-ADMIN) của dự án đó.
    5.  Đối với mỗi Team nghiệp vụ tìm được:
        *   Gọi `ShareTeams()` để chia sẻ bản ghi khách hàng chính (`enRef`).
        *   Nếu khách hàng là Contact, gọi `ShareDoiTuong_CoOwner()` (hiện tại bị comment).
        *   Nếu khách hàng là Account, truy vấn Account đó và gọi `ShareTeams()` để chia sẻ các bản ghi liên quan: `primarycontactid`, `bsd_maincompany`, và các Contact/Representative trong thực thể `bsd_mandatorysecondary`.

### ShareDoiTuong_CoOwner(EntityReference enContact, EntityReference enTeam)

*   **Chức năng tổng quát:** (Hiện tại bị vô hiệu hóa/comment) Mục đích là chia sẻ các bản ghi đồng sở hữu (`bsd_coowner`) liên quan đến Contact chính cho Team được chỉ định.
*   **Logic nghiệp vụ chi tiết:** Toàn bộ logic bên trong hàm này đã bị comment. Nếu được kích hoạt, nó sẽ truy vấn các bản ghi `bsd_coowner` liên quan đến `enContact` và gọi `ShareTeams()` cho các bản ghi `bsd_relatives` (người thân) được tìm thấy.

### ShareTeams(EntityReference sharedRecord, EntityReference shareTeams)

*   **Chức năng tổng quát:** Thực hiện lệnh gọi CRM SDK để cấp quyền truy cập (Grant Access) cho một Team đối với một bản ghi cụ thể.
*   **Logic nghiệp vụ chi tiết:**
    1.  Định nghĩa `AccessRights` bao gồm: `ReadAccess`, `AppendAccess`, `AppendToAccess`, `WriteAccess`, và `ShareAccess`.
    2.  Tạo đối tượng `GrantAccessRequest`.
    3.  Thiết lập `PrincipalAccess` với Team đích (`shareTeams`) và mặt nạ quyền đã định nghĩa.
    4.  Thiết lập `Target` là bản ghi cần chia sẻ (`sharedRecord`).
    5.  Thực thi yêu cầu bằng `service.Execute(grantAccessRequest)`.
    6.  Bao bọc trong khối `try-catch` để xử lý và ném ngoại lệ nếu việc chia sẻ thất bại.

### GetListTeamOfCurrentUser(Guid CurrentUser)

*   **Chức năng tổng quát:** Truy vấn danh sách các Team mà người dùng hiện tại là thành viên.
*   **Logic nghiệp vụ chi tiết:**
    1.  Sử dụng FetchXML để truy vấn thực thể `teammembership`.
    2.  Lọc các bản ghi mà `systemuserid` bằng `CurrentUser`.
    3.  Loại trừ một Team ID cố định (`e653d77d-6f7e-e911-a83b-000d3a07fbb4`) khỏi kết quả.
    4.  Trả về `EntityCollection` chứa các Team ID.

### CreateShareCustomer(string[] arrID, string[] arrTeamID)

*   **Chức năng tổng quát:** Tạo các bản ghi yêu cầu chia sẻ (`bsd_sharecustomers` và `bsd_sharecustomerproject`) sau khi người dùng đã chọn dự án/team.
*   **Logic nghiệp vụ chi tiết:**
    1.  **Lấy Team và Dự án:** Truy vấn tất cả các Team được chọn (`arrTeamID`), trích xuất mã dự án (từ tên Team), và sau đó truy vấn các bản ghi `bsd_project` tương ứng.
    2.  **Ánh xạ:** Tạo một từ điển để ánh xạ Team ID sang Project ID.
    3.  **Tạo Bản ghi Yêu cầu:** Lặp qua từng ID khách hàng (`arrID`):
        *   Tạo bản ghi `bsd_sharecustomers`, liên kết với Contact/Account và Owner (người dùng hiện tại). Đặt `statuscode` là 1 (Chưa chia sẻ).
        *   Lặp qua từng Team ID đã chọn:
            *   Tìm Project ID tương ứng thông qua ánh xạ.
            *   Tạo bản ghi `bsd_sharecustomerproject`, liên kết với `bsd_sharecustomers` vừa tạo và `bsd_project`.

### ShareCustomerRecordToProjectTeams(Guid shareCustomerId)

*   **Chức năng tổng quát:** Thực hiện việc cấp quyền truy cập thực tế cho các Team nghiệp vụ khi một yêu cầu chia sẻ được phê duyệt.
*   **Logic nghiệp vụ chi tiết:**
    1.  **Lấy Khách hàng:** Truy vấn bản ghi `bsd_sharecustomers` để lấy tham chiếu đến bản ghi Khách hàng (`bsd_customer`).
    2.  **Lấy Dự án:** Truy vấn tất cả các bản ghi `bsd_sharecustomerproject` liên kết để xác định các dự án cần chia sẻ.
    3.  **Chia sẻ cho Team Nghiệp vụ:**
        *   Sử dụng `HashSet` để theo dõi các mã dự án đã xử lý, tránh chia sẻ trùng lặp.
        *   Đối với mỗi dự án:
            *   Lấy `bsd_projectcode`.
            *   Xác định 4 tên Team nghiệp vụ tiêu chuẩn (CCR, FINANCE, SALE-MGT, SALE-ADMIN) dựa trên mã dự án.
            *   Truy vấn các Team này.
            *   Sử dụng `GrantAccessRequest` để chia sẻ bản ghi khách hàng (`customerRef`) cho từng Team nghiệp vụ với các quyền Read/Write/Append/AppendTo.
    4.  **Chia sẻ cho Owner:** Cấp quyền Read/Write/Append/AppendTo cho Owner của bản ghi `bsd_sharecustomers`.
    5.  **Cập nhật Trạng thái:** Cập nhật bản ghi `bsd_sharecustomers` sang trạng thái Đã chia sẻ (`statuscode = 100000000`), đồng thời ghi lại người phê duyệt (`bsd_approver`) và thời gian phê duyệt.