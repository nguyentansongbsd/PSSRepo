# Phân tích mã nguồn: Plugin_MailNoti.cs

## Tổng quan

Tệp mã nguồn `Plugin_MailNoti.cs` chứa một Plugin Microsoft Dynamics 365/Power Platform được triển khai trên giao diện `IPlugin`. Plugin này được thiết kế để tự động hóa quy trình thông báo qua email khi có sự thay đổi trạng thái quan trọng trên các bản ghi **Quote** (Báo giá) hoặc **Sales Order** (Đơn hàng/Kế hoạch vận hành - OP).

Plugin hoạt động ở giai đoạn Post-Operation (hoặc Pre-Operation, tùy thuộc vào cấu hình, nhưng logic cho thấy nó xử lý dữ liệu sau khi cập nhật trạng thái) của sự kiện `Update`. Nó xác định trạng thái mới, tìm kiếm Email Template và Team nhận thông báo tương ứng (dựa trên mã dự án), thu thập dữ liệu liên quan (Dự án, Sản phẩm, Khách hàng), và tạo một bản ghi Email mới để hệ thống gửi đi.

## Chi tiết các Hàm (Functions/Methods)

### Execute(IServiceProvider serviceProvider)

**Chức năng tổng quát:**
Đây là điểm vào chính của plugin. Hàm này chịu trách nhiệm thiết lập ngữ cảnh, kiểm tra tính hợp lệ của dữ liệu đầu vào, xác định logic nghiệp vụ dựa trên trạng thái của bản ghi, thu thập dữ liệu, và cuối cùng là tạo và lưu bản ghi email thông báo.

**Logic nghiệp vụ chi tiết:**
1.  **Khởi tạo Dịch vụ:** Khởi tạo các dịch vụ cần thiết của Dynamics 365: `ITracingService` (để ghi log), `IPluginExecutionContext` (ngữ cảnh thực thi), `IOrganizationServiceFactory`, và `IOrganizationService` (để tương tác với dữ liệu).
2.  **Kiểm tra Ngữ cảnh:** Kiểm tra xem `InputParameters` có chứa "Target" và "Target" có phải là một `Entity` hay không. Nếu không, plugin kết thúc.
3.  **Lấy Dữ liệu Bản ghi:** Lấy ID của bản ghi mục tiêu (`entity.Id`). Sau đó, truy vấn bản ghi đầy đủ (`en`) bằng `service.Retrieve` và lấy giá trị `statuscode` hiện tại.
4.  **Xử lý theo Entity và Trạng thái (Switch Case):**
    *   **Case "quote":**
        *   Gọi hàm `GetValue` để lấy thông tin Khách hàng, Dự án, và Sản phẩm, sử dụng các trường lookup cụ thể của Quote (`bsd_projectid`, `bsd_unitno`, `customerid`).
        *   Kiểm tra `status`:
            *   Nếu `status` là `100000000` hoặc `100000006` (Collected status): Thiết lập `tileTemplate` là "Collected status (Step 2)" và `teamname` là `{projectCode}-FINANCE-TEAM`.
            *   Nếu `status` là `3` (Deposited status): Thiết lập `tileTemplate` là "Deposited status (Step 3)" và `teamname` là `{projectCode}-CCR-TEAM`.
            *   Nếu `status` là `4` (Option status): Thiết lập `tileTemplate` là "Option status (Step 4)" và `teamname` là `{projectCode}-FINANCE-TEAM`.
    *   **Case "salesorder":**
        *   Gọi hàm `GetValue` với các trường lookup của Sales Order (`bsd_project`, `bsd_unitnumber`, `customerid`).
        *   Kiểm tra `status`:
            *   Nếu `status` là `100000001` (1st installment status): Thiết lập `tileTemplate` là "1st installment status (Step 5)" và `teamname` là `{projectCode}-CCR-TEAM`.
    *   **Default:** Nếu entity không phải Quote hoặc Sales Order, plugin kết thúc.
5.  **Tạo Email (Nếu Template tồn tại):**
    *   Nếu `tileTemplate` đã được xác định:
        *   Gọi `GetTemplateByName(tileTemplate)` để lấy nội dung template. Nếu không tìm thấy, ném `InvalidPluginExecutionException`.
        *   Gọi `GetConfigValue("linkRegardingmail")` để lấy URL cơ sở từ cấu hình.
        *   Thay thế các placeholder `{entityname}` và `{entityid}` trong URL.
        *   **Xử lý Chuyển đổi Quote sang OP:** Nếu entity là Quote và trạng thái là 4, plugin thực hiện truy vấn để tìm bản ghi Sales Order (OP) liên quan đến Quote này. Nếu tìm thấy OP, URL thông báo sẽ được cập nhật để trỏ đến bản ghi Sales Order thay vì Quote.
        *   **Chuẩn bị Nội dung:** Thay thế các placeholder dữ liệu (`{bsd_tenda}`, `{bsd_tensp}`, `{bsd_tenkh}`, `{link}`) trong Subject và Content của template.
        *   **Tạo Entity Email:** Tạo một bản ghi entity `email` mới.
        *   Thiết lập các thuộc tính: `description` (nội dung), `regardingobjectid` (liên quan đến bản ghi hiện tại), `subject`, và `bsd_typemail` (thiết lập giá trị OptionSet là 100000001).
        *   Gọi `AddTeamMembersToEmail(teamname, email)` để thêm người nhận.
        *   Gọi `SetEmailSender(email)` để thiết lập người gửi.
        *   Lưu bản ghi email mới bằng `service.Create(emailMessage)`.

### GetValue(string projectLookupField, string productLookupField, string customerLookupField)

**Chức năng tổng quát:**
Hàm này chịu trách nhiệm truy vấn và lấy các giá trị tên hiển thị (tên dự án, tên sản phẩm, tên khách hàng) từ các trường lookup trên bản ghi hiện tại (`en`) để sử dụng trong nội dung email.

**Logic nghiệp vụ chi tiết:**
1.  **Lấy Tên Khách hàng:**
    *   Kiểm tra xem bản ghi có chứa trường `customerLookupField` và nó là một `EntityReference` không.
    *   Xác định tên trường cần lấy: Nếu là Contact, lấy `bsd_fullname`; nếu là Account, lấy `bsd_name`.
    *   Truy vấn (Retrieve) entity Khách hàng và lưu tên vào biến `bsd_tenkh`.
2.  **Lấy Tên Dự án và Mã Dự án:**
    *   Kiểm tra trường `projectLookupField`.
    *   Truy vấn entity Dự án (`bsd_name`, `bsd_projectcode`).
    *   Lưu tên dự án vào `bsd_tenduan` và mã dự án vào `projectCode`. Mã dự án này rất quan trọng vì nó được sử dụng để xây dựng tên team đích (ví dụ: `[Mã Dự án]-FINANCE-TEAM`).
3.  **Lấy Tên Sản phẩm:**
    *   Kiểm tra trường `productLookupField`.
    *   Truy vấn entity Sản phẩm (lấy trường `name`).
    *   Lưu tên sản phẩm vào `bsd_tensp`.

### GetConfigValue(string key)

**Chức năng tổng quát:**
Hàm này truy vấn entity cấu hình tùy chỉnh (`bsd_configgolive`) để lấy giá trị URL cần thiết cho việc tạo liên kết trong email.

**Logic nghiệp vụ chi tiết:**
1.  Tạo `QueryExpression` nhắm vào entity `bsd_configgolive`.
2.  Thiết lập điều kiện lọc: Trường `bsd_name` phải bằng `key` được cung cấp (ví dụ: "linkRegardingmail").
3.  Thực hiện `RetrieveMultiple`.
4.  Nếu tìm thấy ít nhất một kết quả:
    *   Lấy entity cấu hình đầu tiên.
    *   Kiểm tra và trả về giá trị của trường `bsd_url`.
5.  Nếu không tìm thấy cấu hình nào khớp với `key`, hàm sẽ ném ra `InvalidPluginExecutionException` để dừng quá trình thực thi và thông báo lỗi cấu hình.

### GetTemplateByName(string name)

**Chức năng tổng quát:**
Hàm này tìm kiếm và trả về một bản ghi Email Template (entity `template`) dựa trên tiêu đề (title) của nó.

**Logic nghiệp vụ chi tiết:**
1.  Tạo `QueryExpression` nhắm vào entity `template`.
2.  Thiết lập điều kiện lọc: Trường `title` phải bằng `name` được cung cấp.
3.  Thực hiện `RetrieveMultiple`.
4.  Nếu tập hợp kết quả có entity, trả về entity đầu tiên.
5.  Nếu không tìm thấy template nào, trả về `null`.

### AddTeamMembersToEmail(string teamName, Entity email)

**Chức năng tổng quát:**
Hàm này tìm kiếm một Team theo tên, sau đó xác định tất cả các thành viên (System Users) thuộc Team đó và thêm họ vào danh sách người nhận (`To`) của email.

**Logic nghiệp vụ chi tiết:**
1.  **Tìm Team ID:**
    *   Tạo `QueryExpression` cho entity `team`.
    *   Lọc theo tên (`name` = `teamName`).
    *   Nếu không tìm thấy team, ném `InvalidPluginExecutionException`.
    *   Lấy `teamId` của team tìm được.
2.  **Tìm Users trong Team:**
    *   Tạo `QueryExpression` cho entity `systemuser`.
    *   Sử dụng `LinkEntity` để liên kết `systemuser` với `teammembership` (bảng liên kết giữa User và Team).
    *   Thêm điều kiện liên kết: `teamid` phải bằng `teamId` đã tìm thấy.
    *   Thực hiện `RetrieveMultiple` để lấy danh sách users.
3.  **Tạo ActivityParty:**
    *   Khởi tạo danh sách `toParties`.
    *   Lặp qua từng user trong danh sách, tạo một entity `activityparty` mới cho mỗi user.
    *   Thiết lập `partyid` của `activityparty` trỏ đến `systemuser` đó.
4.  **Gán vào Email:** Gán danh sách `toParties` (dưới dạng `EntityCollection`) vào trường `to` của entity email.

### SetEmailSender(Entity email)

**Chức năng tổng quát:**
Hàm này thiết lập người gửi (`From`) của email là người dùng hiện đang thực thi plugin.

**Logic nghiệp vụ chi tiết:**
1.  Tạo một entity `activityparty` mới đại diện cho người gửi.
2.  Thiết lập `partyid` của `activityparty` trỏ đến `systemuser` sử dụng `context.UserId` (ID của người dùng đang kích hoạt plugin).
3.  Gán `activityparty` này vào trường `from` của email dưới dạng một `EntityCollection` (mặc dù chỉ có một người gửi).