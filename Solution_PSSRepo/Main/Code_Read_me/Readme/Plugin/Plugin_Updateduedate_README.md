# Phân tích mã nguồn: Plugin_Updateduedate.cs

## Tổng quan

Tệp mã nguồn `Plugin_Updateduedate.cs` định nghĩa một Plugin Dynamics 365 (C#) được triển khai trên nền tảng Microsoft Power Platform/Dynamics 365 Customer Engagement. Plugin này, có tên là `Plugin_Updateduedate`, thực hiện logic nghiệp vụ phức tạp liên quan đến việc phê duyệt và cập nhật ngày đến hạn (Due Date) cho các đợt thanh toán (Installments) thông qua một quy trình yêu cầu thay đổi ngày đến hạn (Update Due Date Request - Master/Detail).

Chức năng chính của Plugin là kiểm tra các điều kiện ràng buộc nghiêm ngặt (như trạng thái hợp đồng, thứ tự ngày đến hạn, và tình trạng thanh toán) trước khi kích hoạt một Custom Action để hoàn tất việc cập nhật ngày đến hạn. Plugin này được thiết kế để chạy trên bản ghi Master của yêu cầu cập nhật ngày đến hạn.

## Chi tiết các Hàm (Functions/Methods)

### Execute(IServiceProvider serviceProvider)

#### Chức năng tổng quát
Đây là điểm vào chính của Plugin. Hàm này thiết lập môi trường Dynamics 365, truy xuất bản ghi mục tiêu, kiểm tra trạng thái kích hoạt, và nếu điều kiện thỏa mãn, nó sẽ cập nhật bản ghi Master và kích hoạt một Custom Action để xử lý các bản ghi chi tiết.

#### Logic nghiệp vụ chi tiết
1.  **Khởi tạo Dịch vụ:** Hàm lấy các dịch vụ cần thiết từ `serviceProvider`, bao gồm `IPluginExecutionContext` (context), `IOrganizationServiceFactory` (factory), `IOrganizationService` (service), và `ITracingService` (tracingService).
2.  **Truy xuất Bản ghi Mục tiêu:** Lấy Entity mục tiêu từ `context.InputParameters["Target"]`.
3.  **Lấy Dữ liệu Đầy đủ:** Sử dụng `service.Retrieve` để lấy toàn bộ dữ liệu của bản ghi Master hiện tại (`en`) dựa trên ID và tên logic của Entity.
4.  **Kiểm tra Trạng thái:** Lấy giá trị `statuscode` của bản ghi Master.
5.  **Điều kiện Kích hoạt:** Nếu `status` bằng `100000000` (thường đại diện cho một trạng thái cụ thể, ví dụ: "Pending Approval" hoặc trạng thái kích hoạt quy trình), Plugin sẽ tiếp tục xử lý.
6.  **Kiểm tra Chi tiết:** Gọi hàm `ExistDetail(ref result)` để xác minh rằng có ít nhất một bản ghi chi tiết hợp lệ liên quan đến bản ghi Master này. Kết quả trả về là một `EntityCollection` (`rs`).
7.  **Cập nhật Bản ghi Master:**
    *   Tạo một Entity mới (`enDetailUpdate`) chỉ chứa các trường cần cập nhật cho bản ghi Master.
    *   Thiết lập các trường:
        *   `bsd_processing_pa = true`
        *   `bsd_error = false`
        *   `bsd_errordetail = ""`
        *   `statuscode = new OptionSetValue(1)` (Chuyển trạng thái sang Active/Approved).
        *   `bsd_approvedrejecteddate = DateTime.UtcNow` (Ngày phê duyệt).
        *   `bsd_approvedrejectedperson = new EntityReference("systemuser", context.UserId)` (Người phê duyệt).
    *   Thực hiện `service.Update(enDetailUpdate)`.
8.  **Thực thi Custom Action:**
    *   Tạo một `OrganizationRequest` với tên `bsd_Action_Active_Approved_Updateduedate_Detail`.
    *   Tạo chuỗi `listid` bằng cách nối các ID của các bản ghi chi tiết hợp lệ (từ `rs.Entities`) bằng dấu phẩy.
    *   Gán tham số `listid` và `idmaster` (ID của bản ghi Master) vào request.
    *   Thực hiện `service.Execute(request)` để kích hoạt logic xử lý chi tiết (có thể là một Workflow hoặc Action khác).

### ExistDetail(ref bool result)

#### Chức năng tổng quát
Hàm này chịu trách nhiệm truy vấn và xác minh sự tồn tại của các bản ghi chi tiết (`bsd_updateduedatedetail`) liên quan đến bản ghi Master hiện tại.

#### Logic nghiệp vụ chi tiết
1.  **Tạo Query:** Khởi tạo `QueryExpression` nhắm vào entity `bsd_updateduedatedetail`.
2.  **Thiết lập Cột:** Yêu cầu lấy tất cả các cột (`ColumnSet.AllColumns = true`).
3.  **Thiết lập Điều kiện Lọc (Criteria):**
    *   Điều kiện 1: Liên kết với bản ghi Master: `bsd_updateduedate` bằng ID của bản ghi Master (`en.Id.ToString()`).
    *   Điều kiện 2: Trạng thái của bản ghi chi tiết: `statuscode` bằng `1` (thường là trạng thái "Active" hoặc "Pending").
4.  **Thực thi Query:** Gọi `service.RetrieveMultiple(query)` để lấy danh sách các bản ghi chi tiết.
5.  **Kiểm tra Kết quả:** Nếu `rs.Entities.Count == 0`, hàm sẽ ném ra `InvalidPluginExecutionException` với thông báo lỗi, ngăn chặn Plugin tiếp tục thực thi.
6.  **Trả về:** Trả về `EntityCollection` chứa các bản ghi chi tiết hợp lệ.

### CheckExistParentInDetail(ref bool result, Entity item)

#### Chức năng tổng quát
Hàm này kiểm tra tính nhất quán của dữ liệu bằng cách đảm bảo rằng trường Dự án (`bsd_project`) trên bản ghi chi tiết khớp với trường Dự án trên bản ghi Master.

#### Logic nghiệp vụ chi tiết
1.  **So sánh ID Dự án:** Truy cập trường `bsd_project` (kiểu `EntityReference`) trên cả bản ghi Master (`en`) và bản ghi chi tiết (`item`).
2.  **Điều kiện Lỗi:** Nếu ID của EntityReference Dự án trên Master khác với ID của EntityReference Dự án trên Detail, hàm sẽ ném ra `InvalidPluginExecutionException` (The project in the Detail entity is invalid...).

### CheckIsLast(ref bool result, Entity item, Entity enInstallment)

#### Chức năng tổng quát
Hàm này kiểm tra xem đợt thanh toán (Installment) liên quan có phải là đợt cuối cùng hay không, hoặc nếu phương thức tính ngày đến hạn của đợt đó được đặt là "Estimate handover date".

#### Logic nghiệp vụ chi tiết
1.  **Kiểm tra Điều kiện (OR Logic):** Hàm kiểm tra hai điều kiện sau trên bản ghi Installment (`enInstallment`):
    *   Điều kiện 1: Trường `bsd_lastinstallment` (kiểu boolean) là `true`.
    *   Điều kiện 2: Trường `bsd_duedatecalculatingmethod` tồn tại VÀ giá trị của OptionSetValue đó là `100000002`.
2.  **Xử lý Lỗi:** Nếu bất kỳ điều kiện nào trong hai điều kiện trên là đúng, hàm sẽ ném ra `InvalidPluginExecutionException` (The record contains an invalid batch...).

### CheckHD(ref bool result, Entity item, Entity enHD)

#### Chức năng tổng quát
Hàm này kiểm tra trạng thái của Hợp đồng (Contract) liên quan (`enHD`) để đảm bảo rằng hợp đồng đó không bị chấm dứt (Terminated).

#### Logic nghiệp vụ chi tiết
1.  **Kiểm tra Trạng thái Hợp đồng:** Lấy giá trị `statuscode` (kiểu `OptionSetValue`) của bản ghi Hợp đồng (`enHD`).
2.  **Điều kiện Lỗi:** Nếu giá trị `statuscode` bằng `100000006` (được chú thích là trạng thái Terminated/Liquidated), hàm sẽ ném ra `InvalidPluginExecutionException` (The record contains a contract that has already been liquidated...).

### CheckPaidDetail(ref bool result, Entity item, Entity enInstallment)

#### Chức năng tổng quát
Hàm này kiểm tra xem đợt thanh toán (Installment) đã được thanh toán một phần hoặc toàn bộ hay chưa, dựa trên số tiền đặt cọc và số tiền đã thanh toán.

#### Logic nghiệp vụ chi tiết
1.  **Kiểm tra Số tiền:** Truy cập giá trị của hai trường kiểu Money trên bản ghi Installment (`enInstallment`): `bsd_depositamount` và `bsd_amountwaspaid`.
2.  **Điều kiện Lỗi (AND Logic):** Nếu CẢ hai trường này đều có giá trị khác 0 (`!= 0`), điều đó được coi là đợt thanh toán đã được thực hiện. Trong trường hợp này, hàm sẽ ném ra `InvalidPluginExecutionException` (There is a batch that has already been paid...).

### CheckDueDate(ref bool result, Entity item, Entity enInstallment, Entity enHD)

#### Chức năng tổng quát
Hàm này đảm bảo rằng Ngày đến hạn mới (`bsd_duedatenew`) được đề xuất tuân thủ thứ tự thời gian hợp lý so với các đợt thanh toán khác trong cùng một Hợp đồng.

#### Logic nghiệp vụ chi tiết
1.  **Lấy Ngày mới:** Lấy giá trị `newDate` (kiểu `DateTime`) từ bản ghi chi tiết (`item`).
2.  **Truy vấn các Đợt khác:**
    *   Tạo `QueryExpression` để truy vấn các đợt thanh toán khác (cùng loại logic với `enInstallment`).
    *   Lọc theo Hợp đồng: `bsd_optionentry` bằng ID của Hợp đồng (`enHD.Id.ToString()`).
    *   Loại trừ đợt hiện tại: `bsd_paymentschemedetailid` khác ID của đợt hiện tại (`enInstallment.Id.ToString()`).
3.  **Lặp và So sánh Thứ tự:** Duyệt qua từng đợt thanh toán khác (`JItem`) trong kết quả truy vấn:
    *   **So sánh với Đợt TRƯỚC:** Nếu số thứ tự (`bsd_ordernumber`) của `JItem` nhỏ hơn số thứ tự của đợt hiện tại (`enInstallment`):
        *   `newDate` PHẢI lớn hơn ngày đến hạn cũ của đợt trước (`JItem["bsd_duedate"]`). Nếu `newDate <= JItem["bsd_duedate"]`, ném lỗi.
    *   **So sánh với Đợt SAU:** Nếu số thứ tự của `JItem` lớn hơn số thứ tự của đợt hiện tại (`enInstallment`):
        *   `newDate` PHẢI nhỏ hơn ngày đến hạn cũ của đợt sau (`JItem["bsd_duedate"]`). Nếu `newDate >= JItem["bsd_duedate"]`, ném lỗi.

### CheckNewDate(ref bool result, Entity item, Entity enInstallment)

#### Chức năng tổng quát
Hàm này kiểm tra xem Ngày đến hạn mới được đề xuất có thực sự khác với Ngày đến hạn cũ của đợt thanh toán hay không.

#### Logic nghiệp vụ chi tiết
1.  **Lấy Ngày:** Lấy `newDate` từ bản ghi chi tiết (`item`) và ngày đến hạn cũ (`enInstallment["bsd_duedate"]`) từ bản ghi Installment.
2.  **So sánh Khoảng cách:** Tính toán sự khác biệt giữa hai ngày bằng cách sử dụng `TotalDays`.
3.  **Điều kiện Lỗi:** Nếu sự khác biệt về ngày là 0 (`TotalDays == 0`), điều đó có nghĩa là ngày mới trùng với ngày cũ. Hàm sẽ ném ra `InvalidPluginExecutionException` (The new due date is the same as the old due date...).

### Approve(ref bool result, Entity item, Entity enInstallment)

#### Chức năng tổng quát
Hàm này kích hoạt một Custom Action khác để hoàn tất quá trình phê duyệt, chuyển ngày đến hạn mới từ bản ghi chi tiết sang bản ghi Installment thực tế.

#### Logic nghiệp vụ chi tiết
1.  **Tạo Custom Action:** Khởi tạo `OrganizationRequest` có tên `bsd_Action_Approved_Updateduedate_Master`.
2.  **Chuẩn bị Tham số:**
    *   Lấy `newDate` (kiểu `DateTime`) từ bản ghi chi tiết (`item`).
    *   Gán `detail_id` (ID của bản ghi chi tiết).
    *   Gán `duedatenew`: Ngày đến hạn mới được chuyển đổi thành chuỗi. **Lưu ý quan trọng:** Ngày này được thêm 7 giờ (`AddHours(7)`) trước khi chuyển thành chuỗi, điều này thường được thực hiện để bù múi giờ khi lưu trữ ngày giờ UTC trong Dynamics 365 (ví dụ: chuyển từ UTC sang GMT+7).
    *   Gán `statuscode` là `100000000`.
3.  **Thực thi:** Gọi `service.Execute(request)` để kích hoạt logic phê duyệt cuối cùng.