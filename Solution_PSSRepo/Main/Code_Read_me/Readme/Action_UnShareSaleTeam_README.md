# Phân tích mã nguồn: Action\_UnShareSaleTeam.cs

## Tổng quan

Tệp mã nguồn `Action_UnShareSaleTeam.cs` chứa một Plugin Dynamics 365 (C#) được thiết kế để thực thi như một Custom Action. Mục đích chính của plugin này là thu hồi (unshare) quyền truy cập của các nhóm (Team) cụ thể đối với một bản ghi mục tiêu (Target Record) được cung cấp thông qua tham số đầu vào.

Plugin này thực hiện kiểm tra logic nghiệp vụ phức tạp liên quan đến các loại hình kinh doanh (Business Type - `bsd_businesstypesys`) của Khách hàng Cá nhân (Contact) hoặc Khách hàng Doanh nghiệp (Account). Nếu bản ghi mục tiêu thuộc về một phân khúc khách hàng được xác định là "CL" (Commercial/Large), quá trình unshare sẽ bị chặn để đảm bảo các nhóm CL vẫn giữ quyền truy cập.

## Chi tiết các Hàm (Functions/Methods)

### IPlugin.Execute(IServiceProvider serviceProvider)

#### Chức năng tổng quát:

Hàm này là điểm vào chính của Plugin, chịu trách nhiệm nhận các tham số về bản ghi mục tiêu, xác định loại đối tượng, kiểm tra các điều kiện nghiệp vụ để ngăn chặn việc thu hồi quyền truy cập không mong muốn, và cuối cùng là truy vấn bảng PrincipalObjectAccess (POA) để thu hồi quyền truy cập của các nhóm bán hàng đã được chia sẻ.

#### Logic nghiệp vụ chi tiết:

1.  **Khởi tạo Dịch vụ (Service Initialization):**
    *   Lấy các đối tượng dịch vụ cần thiết từ `serviceProvider`: `IPluginExecutionContext` (context), `IOrganizationServiceFactory` (factory), và `ITracingService` (traceService).
    *   Tạo `IOrganizationService` bằng cách sử dụng ID người dùng hiện tại (`context.UserId`) để thực hiện các thao tác CRM.
    *   Bắt đầu theo dõi (trace) quá trình thực thi.

2.  **Xử lý và Xác thực Tham số Đầu vào (Input Parameter Validation):**
    *   Kiểm tra xem `context.InputParameters` có chứa hai tham số bắt buộc là `"TargetEntityName"` và `"TargetEntityId"` hay không. Nếu thiếu, ném ra lỗi `InvalidPluginExecutionException`.
    *   Lấy giá trị `entityName` (chuỗi) và cố gắng phân tích giá trị `TargetEntityId` thành một `Guid` an toàn. Nếu việc phân tích GUID thất bại, ném ra lỗi.
    *   Kiểm tra cuối cùng đảm bảo `entityName` không rỗng và `entityId` không phải là `Guid.Empty`.
    *   Tạo một `EntityReference` (`targetRecord`) từ các tham số đã xác thực.

3.  **Lấy ObjectTypeCode (Metadata Retrieval):**
    *   Sử dụng `RetrieveEntityRequest` để lấy siêu dữ liệu (metadata) của `entityName`.
    *   Trích xuất `ObjectTypeCode` từ phản hồi. Mã này là cần thiết để truy vấn bảng POA (PrincipalObjectAccess).
    *   Nếu `ObjectTypeCode` không tìm thấy (bằng 0), ném ra lỗi.

4.  **Kiểm tra Logic Nghiệp vụ Ngăn chặn Unshare (CL Share Logic Check):**
    *   Phần này kiểm tra xem bản ghi mục tiêu có thuộc các điều kiện kinh doanh đặc biệt (liên quan đến phân khúc CL) hay không. Nếu thuộc, plugin sẽ dừng lại và không thực hiện unshare.
    *   **Trường hợp 1: `entityName == "contact"` (Khách hàng Cá nhân):**
        *   Định nghĩa hai giá trị `OptionSetValue` mục tiêu: `100000003` và `100000002` (đại diện cho các loại hình kinh doanh CL).
        *   Thực hiện một truy vấn `QueryExpression` phức tạp: Tìm kiếm các bản ghi Contact mà là `primarycontactid` của một Account, và Account đó có trường `bsd_businesstypesys` chứa một trong hai giá trị mục tiêu.
        *   Lấy danh sách các ID Contact thỏa mãn điều kiện.
        *   Nếu `entityId` hiện tại nằm trong danh sách này, ghi trace và thoát khỏi hàm (`return;`).
    *   **Trường hợp 2: `entityName == "account"` (Khách hàng Doanh nghiệp):**
        *   Truy xuất bản ghi Account mục tiêu.
        *   Kiểm tra xem trường `bsd_businesstypesys` của Account có chứa giá trị `100000002` hoặc `100000003` hay không (sử dụng `OptionSetValueCollection`).
        *   Nếu Account thỏa mãn điều kiện này, ghi trace và thoát khỏi hàm (`return;`).

5.  **Truy vấn Bảng PrincipalObjectAccess (POA Query):**
    *   Nếu bản ghi vượt qua bước kiểm tra logic nghiệp vụ (tức là nó *nên* được unshare), plugin sẽ xây dựng một FetchXML.
    *   FetchXML truy vấn bảng `principalobjectaccess` (POA).
    *   **Điều kiện truy vấn:**
        *   `objectid` bằng `entityId` của bản ghi mục tiêu.
        *   `objecttypecode` bằng `objectTypeCode` đã lấy ở Bước 2.
        *   Liên kết (link-entity) với bảng `team` (alias `t`).
        *   Lọc các Team có thuộc tính `name` bằng `'cl'`.
    *   Thực thi FetchXML để lấy danh sách các Team hiện đang được chia sẻ bản ghi này và đáp ứng tiêu chí lọc.

6.  **Thu hồi Quyền truy cập (Revoke Access):**
    *   Lặp qua từng `poaEntity` trong `sharedTeams`.
    *   Trích xuất `teamId` và `teamName` từ các giá trị liên kết (AliasedValue).
    *   Tạo một `RevokeAccessRequest` mới, chỉ định `Target` là bản ghi mục tiêu và `Revokee` là `EntityReference` của Team.
    *   Thực thi `RevokeAccessRequest` để thu hồi tất cả các quyền truy cập mà Team đó có đối với bản ghi.

7.  **Xử lý Ngoại lệ (Exception Handling):**
    *   Bất kỳ lỗi nào xảy ra trong quá trình thực thi sẽ được bắt, ghi trace chi tiết (`ex.ToString()`), và sau đó được ném lại dưới dạng `InvalidPluginExecutionException` để hiển thị thông báo lỗi thân thiện cho người dùng Dynamics 365.
    *   Ghi trace "end" khi hoàn thành.