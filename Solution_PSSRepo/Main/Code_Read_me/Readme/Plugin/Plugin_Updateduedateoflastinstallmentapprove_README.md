# Phân tích mã nguồn: Plugin_Updateduedateoflastinstallmentapprove.cs

## Tổng quan

Tệp mã nguồn `Plugin_Updateduedateoflastinstallmentapprove.cs` định nghĩa một Dynamics 365 Plugin được thiết kế để xử lý logic phê duyệt hoặc từ chối đối với các yêu cầu cập nhật ngày đến hạn của đợt thanh toán cuối cùng. Plugin này được kích hoạt trên thực thể chính (Master) và thực hiện các kiểm tra nghiệp vụ phức tạp trên các bản ghi chi tiết liên quan trước khi thực hiện hành động cuối cùng (thông qua việc gọi một Custom Action).

Plugin này chủ yếu tập trung vào việc xác định trạng thái của bản ghi cha và sau đó thực hiện các kiểm tra xác thực chi tiết (như kiểm tra dự án, kiểm tra đợt cuối, kiểm tra hợp đồng, kiểm tra thanh toán, và kiểm tra ngày đến hạn mới) để đảm bảo tính toàn vẹn của dữ liệu trước khi cho phép cập nhật ngày đến hạn.

## Chi tiết các Hàm (Functions/Methods)

### Execute(IServiceProvider serviceProvider)

#### Chức năng tổng quát:
Đây là điểm vào chính của plugin, chịu trách nhiệm khởi tạo các dịch vụ Dynamics 365 cần thiết, truy xuất bản ghi mục tiêu, và điều phối logic xử lý phê duyệt hoặc từ chối dựa trên trạng thái của bản ghi.

#### Logic nghiệp vụ chi tiết:
1.  **Khởi tạo dịch vụ:** Thiết lập các đối tượng Dynamics 365 tiêu chuẩn: `IPluginExecutionContext` (context), `IOrganizationServiceFactory` (factory), `IOrganizationService` (service), và `ITracingService` (tracingService).
2.  **Truy xuất bản ghi:** Lấy bản ghi mục tiêu (`Target`) từ `context.InputParameters`. Sau đó, truy xuất toàn bộ bản ghi hiện tại (`en`) bằng `service.Retrieve` để lấy các trường cần thiết, bao gồm cả `statuscode`.
3.  **Kiểm tra trạng thái (`statuscode`):**
    *   **Trường hợp 1: Trạng thái là 100000002 (Có thể là Rejected/Từ chối):**
        *   Tạo một đối tượng `Entity` mới (`enDetailUpdate`) chỉ chứa ID của bản ghi hiện tại.
        *   Cập nhật các trường:
            *   `bsd_approvedrejectedperson`: Đặt là người dùng hiện tại (thông qua `context.UserId`).
            *   `bsd_approvedrejecteddate`: Đặt là thời điểm hiện tại (`DateTime.Now`).
        *   Thực hiện `service.Update(enDetailUpdate)`.
    *   **Trường hợp 2: Trạng thái là 100000001 (Có thể là Approved/Duyệt):**
        *   Gọi hàm `ExistDetail()` để kiểm tra xem bản ghi cha có bản ghi chi tiết hợp lệ nào không. Hàm này sẽ ném ra ngoại lệ nếu không tìm thấy chi tiết.
        *   Lấy kết quả truy vấn chi tiết (`rs`).
        *   Cập nhật bản ghi chính (`enDetailUpdate`):
            *   `bsd_approvedrejectedperson`: Người dùng hiện tại.
            *   `bsd_approvedrejecteddate`: Thời điểm hiện tại.
            *   `bsd_processing_pa`: Đặt thành `true` (có thể là cờ báo đang xử lý).
            *   `bsd_error`: Đặt thành `false`.
            *   `bsd_errordetail`: Đặt thành chuỗi rỗng `""`.
        *   Thực hiện `service.Update(enDetailUpdate)`.
        *   **Gọi Custom Action:** Chuẩn bị và thực thi một `OrganizationRequest` có tên là `"bsd_Action_Active_Approved_Updateduedateoflastinstallmentapprove_Detal"`.
        *   Các tham số được truyền vào Action:
            *   `listid`: Chuỗi chứa các ID của các bản ghi chi tiết được phân tách bằng dấu phẩy (lấy từ kết quả `rs`).
            *   `idmaster`: ID của bản ghi cha hiện tại.

### GetInstallment(string id)

#### Chức năng tổng quát:
Truy xuất một bản ghi chi tiết đợt thanh toán (`bsd_paymentschemedetail`) dựa trên ID được cung cấp.

#### Logic nghiệp vụ chi tiết:
1.  Tạo một chuỗi FetchXML để truy vấn thực thể `bsd_paymentschemedetail`.
2.  Bộ lọc (filter) được đặt để tìm bản ghi có `bsd_paymentschemedetailid` bằng với ID đầu vào.
3.  Thực hiện truy vấn bằng `service.RetrieveMultiple(new FetchExpression(fetchXml))`.
4.  Trả về đối tượng `Entity` đầu tiên trong kết quả truy vấn (`res.Entities[0]`).

### ExistDetail(ref bool result)

#### Chức năng tổng quát:
Kiểm tra xem bản ghi cha có bất kỳ bản ghi chi tiết nào liên quan (`bsd_updateduedateoflastinstallment`) đang ở trạng thái hoạt động hay không.

#### Logic nghiệp vụ chi tiết:
1.  Tạo một `QueryExpression` cho thực thể chi tiết (`bsd_updateduedateoflastinstallment`).
2.  Thiết lập `ColumnSet.AllColumns = true`.
3.  Thêm điều kiện 1: Trường `bsd_updateduedateoflastinstapprove` (trường tham chiếu đến bản ghi cha) phải bằng ID của bản ghi cha (`en.Id`).
4.  Thêm điều kiện 2: `statuscode` phải bằng 1 (Trạng thái hoạt động/Active).
5.  Thực hiện truy vấn `service.RetrieveMultiple(query)`.
6.  **Kiểm tra lỗi:** Nếu số lượng bản ghi tìm thấy bằng 0, ném ra `InvalidPluginExecutionException` với thông báo lỗi: "The record does not have any details. Please check again."
7.  Trả về `EntityCollection` chứa các bản ghi chi tiết hợp lệ.

### CheckExistParentInDetail(ref bool result, Entity item)

#### Chức năng tổng quát:
Kiểm tra xem trường Dự án (`bsd_project`) trong bản ghi chi tiết có khớp với trường Dự án trong bản ghi cha hay không.

#### Logic nghiệp vụ chi tiết:
1.  So sánh ID của `EntityReference` trong trường `bsd_project` của bản ghi cha (`en`) với ID của `EntityReference` trong trường `bsd_project` của bản ghi chi tiết (`item`).
2.  **Kiểm tra lỗi:** Nếu hai ID không khớp, ném ra `InvalidPluginExecutionException` với thông báo lỗi: "The project in the Detail entity is invalid. Please check again."

### CheckIsLast(ref bool result, Entity item, Entity enInstallment)

#### Chức năng tổng quát:
Kiểm tra xem đợt thanh toán được tham chiếu có phải là đợt cuối cùng (`bsd_lastinstallment = true`) hay không.

#### Logic nghiệp vụ chi tiết:
1.  Truy cập giá trị boolean của trường `bsd_lastinstallment` trên thực thể đợt thanh toán (`enInstallment`).
2.  **Kiểm tra lỗi:** Nếu giá trị này là `false` (tức là không phải đợt cuối), ném ra `InvalidPluginExecutionException` với thông báo lỗi: "The record contains an invalid batch. Please check again."

### CheckHD(ref bool result, Entity item, Entity enHD)

#### Chức năng tổng quát:
Kiểm tra trạng thái của Hợp đồng (HD) liên quan để đảm bảo rằng hợp đồng chưa bị thanh lý (Terminated).

#### Logic nghiệp vụ chi tiết:
1.  Truy cập giá trị của `statuscode` (dạng `OptionSetValue`) trên thực thể Hợp đồng (`enHD`).
2.  **Kiểm tra lỗi:** Nếu giá trị `statuscode` bằng 100000006 (được chú thích là Terminated/Thanh lý), ném ra `InvalidPluginExecutionException` với thông báo lỗi: "The record contains a contract that has already been liquidated. Please check again."

### CheckPaidDetail(ref bool result, Entity item, Entity enInstallment)

#### Chức năng tổng quát:
Kiểm tra xem đợt thanh toán chi tiết đã được thanh toán hay chưa, dựa trên hai trường tiền tệ: `bsd_depositamount` và `bsd_amountwaspaid`.

#### Logic nghiệp vụ chi tiết:
1.  Truy cập giá trị tiền tệ (`Value`) của trường `bsd_depositamount` và `bsd_amountwaspaid` trên thực thể đợt thanh toán (`enInstallment`).
2.  **Kiểm tra lỗi:** Nếu một trong hai giá trị tiền tệ này khác 0 (tức là đã có thanh toán hoặc đặt cọc), ném ra `InvalidPluginExecutionException` với thông báo lỗi: "There is a batch that has already been paid. Please check again."

### CheckDueDate(ref bool result, Entity item, Entity enInstallment, Entity enHD)

#### Chức năng tổng quát:
Kiểm tra xem Ngày đến hạn mới (`bsd_duedate` trên bản ghi chi tiết) có lớn hơn ngày đến hạn của bất kỳ đợt thanh toán nào khác trong cùng một Hợp đồng hay không.

#### Logic nghiệp vụ chi tiết:
1.  Lấy ngày đến hạn mới (`newDate`) từ bản ghi chi tiết (`item["bsd_duedate"]`).
2.  Tạo một `QueryExpression` để truy vấn tất cả các đợt thanh toán (cùng loại với `enInstallment`) liên quan đến Hợp đồng (`enHD.Id`) thông qua trường `bsd_optionentry`.
3.  Lặp qua tất cả các đợt thanh toán (`JItem`) tìm thấy:
    *   Bỏ qua việc so sánh nếu `JItem.Id` là ID của đợt thanh toán hiện tại (`enInstallment.Id`).
    *   **Kiểm tra lỗi:** Nếu `newDate` nhỏ hơn hoặc bằng ngày đến hạn của bất kỳ đợt thanh toán nào khác (`JItem["bsd_duedate"]`), ném ra `InvalidPluginExecutionException` với thông báo lỗi: "The new due date is invalid. Please check again." (Điều này đảm bảo ngày đến hạn mới không bị lùi về trước các đợt khác).

### CheckNewDate(ref bool result, Entity item, Entity enInstallment)

#### Chức năng tổng quát:
Kiểm tra xem Ngày đến hạn mới (`bsd_duedate` trên bản ghi chi tiết) có khác với Ngày đến hạn cũ (`bsd_duedate` trên bản ghi đợt thanh toán) hay không.

#### Logic nghiệp vụ chi tiết:
1.  Kiểm tra xem thực thể đợt thanh toán (`enInstallment`) có chứa trường `bsd_duedate` hay không. Nếu không chứa, hàm thoát (return).
2.  Lấy ngày đến hạn mới (`newDate`) từ bản ghi chi tiết (`item`).
3.  Tính toán sự khác biệt về ngày giữa `newDate` và ngày đến hạn cũ (`(DateTime)enInstallment["bsd_duedate"]`) dưới dạng tổng số ngày (`TotalDays`).
4.  **Kiểm tra lỗi:** Nếu sự khác biệt là 0 (tức là ngày mới và ngày cũ giống hệt nhau), ném ra `InvalidPluginExecutionException` với thông báo lỗi: "The new due date is the same as the old due date. Please check again."

### Approve(ref bool result, Entity item, Entity enInstallment)

#### Chức năng tổng quát:
Thực thi Custom Action cuối cùng để cập nhật ngày đến hạn của đợt thanh toán cuối cùng.

#### Logic nghiệp vụ chi tiết:
1.  Tạo một `OrganizationRequest` mới với tên `"bsd_Action_Approved_UpdateDuedateOfLastInstallment_Master"`.
2.  Lấy ngày đến hạn mới (`newDate`) từ bản ghi chi tiết (`item`).
3.  Thiết lập các tham số cho Request:
    *   `detail_id`: ID của bản ghi chi tiết.
    *   `duedatenew`: Ngày đến hạn mới, được chuyển đổi thành chuỗi và thêm 7 giờ (có thể là điều chỉnh múi giờ UTC sang GMT+7).
    *   `statuscode`: Đặt là 100000000.
4.  Thực thi Request bằng `service.Execute(request)`.