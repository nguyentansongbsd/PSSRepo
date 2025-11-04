# Phân tích mã nguồn: Action_Approved_Updateduedate_Detail.cs

## Tổng quan

Tệp mã nguồn `Action_Approved_Updateduedate_Detail.cs` chứa một Dynamics 365 Plugin được thiết kế để xử lý quá trình phê duyệt một yêu cầu cập nhật ngày đến hạn chi tiết (`bsd_updateduedatedetail`).

Plugin này được kích hoạt thông qua một Custom Action và đóng vai trò là cổng kiểm soát nghiệp vụ nghiêm ngặt. Chức năng chính của nó là thực hiện một chuỗi các kiểm tra xác thực (ví dụ: kiểm tra tính hợp lệ của ngày mới so với các đợt thanh toán trước/sau, kiểm tra trạng thái hợp đồng, kiểm tra đợt cuối) trước khi cho phép cập nhật ngày đến hạn của đợt thanh toán (Installment) liên quan. Nếu bất kỳ kiểm tra nào thất bại, bản ghi Master và Detail sẽ được cập nhật trạng thái "Error" cùng với thông báo lỗi chi tiết.

## Chi tiết các Hàm (Functions/Methods)

### Execute(IServiceProvider serviceProvider)

#### Chức năng tổng quát:
Là điểm vào chính của plugin, chịu trách nhiệm khởi tạo các dịch vụ Dynamics 365, lấy dữ liệu đầu vào, thực hiện chuỗi kiểm tra nghiệp vụ nghiêm ngặt, và cuối cùng là gọi hành động phê duyệt hoặc xử lý lỗi.

#### Logic nghiệp vụ chi tiết:
1.  **Khởi tạo Dịch vụ:** Lấy các đối tượng cần thiết từ `serviceProvider`, bao gồm `IPluginExecutionContext` (context), `IOrganizationServiceFactory` (factory), `IOrganizationService` (service), và `ITracingService` (tracingService) để ghi log.
2.  **Lấy Dữ liệu:** Lấy ID của bản ghi chi tiết (`bsd_updateduedatedetail`) từ `context.InputParameters["id"]` và truy vấn toàn bộ dữ liệu của bản ghi này (`en`).
3.  **Kiểm tra Điều kiện Chạy:** Gọi hàm `CheckConditionRun(en)`. Hiện tại, hàm này luôn trả về `true`, cho phép plugin tiếp tục thực thi.
4.  **Xác định Loại Giao dịch:** Dựa trên sự tồn tại của các trường tham chiếu (`bsd_optionentry`, `bsd_quote`, `bsd_quotation`) trên bản ghi chi tiết, plugin xác định loại hợp đồng liên quan (SalesOrder, Quote, hoặc bsd_quotation) và thiết lập các biến toàn cục (`enIntalments_fieldNameHD`, `enHD_name`). Nếu không tìm thấy loại hợp đồng nào, nó gọi `HandleError`.
5.  **Truy vấn Hợp đồng:** Lấy bản ghi Hợp đồng (`enHD`) liên quan đến đợt thanh toán.
6.  **Chuỗi Kiểm tra Nghiệp vụ:** Thực hiện tuần tự 6 bước kiểm tra. Nếu bất kỳ bước nào đặt biến `result` thành `false`, quá trình sẽ dừng lại ngay lập tức (`return`):
    *   `CheckExistParentInDetail`: Kiểm tra sự khớp nhau của Project giữa Master và Detail.
    *   `CheckIsLast`: Kiểm tra đợt thanh toán có phải là đợt cuối hoặc liên quan đến bàn giao không.
    *   `CheckHD`: Kiểm tra trạng thái hợp đồng không bị thanh lý.
    *   `CheckPaidDetail`: Kiểm tra đợt thanh toán đã được trả tiền chưa (hiện đang bị vô hiệu hóa).
    *   `CheckDueDate`: Kiểm tra tính hợp lệ của ngày đến hạn mới so với các đợt trước và sau.
    *   `CheckNewDate`: Kiểm tra ngày mới có khác ngày cũ không (hiện đang bị vô hiệu hóa).
7.  **Phê duyệt:** Nếu tất cả kiểm tra thành công, gọi hàm `Approve`.
8.  **Xử lý Lỗi:** Sử dụng khối `try-catch` để bắt các lỗi ngoại lệ không mong muốn và gọi `HandleError` để cập nhật trạng thái lỗi cho bản ghi.

### CheckExistParentInDetail(ref bool result, Entity item)

#### Chức năng tổng quát:
Đảm bảo rằng trường Project trên bản ghi chi tiết (`bsd_updateduedatedetail`) phải khớp với trường Project trên bản ghi Master (`bsd_updateduedate`) mà nó tham chiếu.

#### Logic nghiệp vụ chi tiết:
1.  Lấy tham chiếu đến bản ghi Master (`bsd_updateduedate`) từ bản ghi Detail (`item`).
2.  Truy vấn bản ghi Master.
3.  So sánh ID của trường `bsd_project` trên bản ghi Master với ID của trường `bsd_project` trên bản ghi Detail.
4.  Nếu hai ID không khớp, hàm gọi `HandleError` với thông báo lỗi về việc không khớp Project và đặt biến `result = false`.

### CheckIsLast(ref bool result, Entity item, Entity enInstallment)

#### Chức năng tổng quát:
Ngăn chặn việc thay đổi ngày đến hạn nếu đợt thanh toán là đợt cuối cùng hoặc có phương thức tính ngày đến hạn liên quan đến bàn giao.

#### Logic nghiệp vụ chi tiết:
1.  Kiểm tra hai điều kiện trên bản ghi đợt thanh toán (`enInstallment`):
    *   Điều kiện 1: Trường boolean `bsd_lastinstallment` là `true`.
    *   Điều kiện 2: Trường `bsd_duedatecalculatingmethod` tồn tại và có giá trị là `100000002` (giá trị đại diện cho "Estimate handover date").
2.  Nếu một trong hai điều kiện trên đúng, hàm gọi `HandleError` với thông báo lỗi liên quan đến "handover batch" và đặt biến `result = false`.

### CheckHD(ref bool result, Entity item, Entity enHD)

#### Chức năng tổng quát:
Kiểm tra trạng thái của Hợp đồng (Quotation, Option Entry, Reservation) liên quan để đảm bảo nó không ở trạng thái đã bị thanh lý (Terminated).

#### Logic nghiệp vụ chi tiết:
1.  Sử dụng biến toàn cục `enHD_name` (đã được xác định trong `Execute`) để xác định loại entity hợp đồng đang được kiểm tra.
2.  Sử dụng cấu trúc `switch` để kiểm tra `statuscode` của bản ghi hợp đồng (`enHD`) dựa trên loại entity:
    *   Nếu là `bsd_quotation` hoặc `quote`: Kiểm tra nếu `statuscode` là `100000001` (Terminated).
    *   Nếu là `salesorder` (Option Entry): Kiểm tra nếu `statuscode` là `100000006` (Terminated).
3.  Nếu trạng thái hợp đồng là Terminated, hàm gọi `HandleError` và đặt biến `result = false`.

### CheckPaidDetail(ref bool result, Entity item, Entity enInstallment)

#### Chức năng tổng quát:
Kiểm tra xem đợt thanh toán đã được trả tiền hay chưa (dựa trên số tiền đã thanh toán hoặc tiền đặt cọc).

#### Logic nghiệp vụ chi tiết:
1.  **LƯU Ý:** Toàn bộ logic bên trong hàm này hiện đang bị vô hiệu hóa (comment out) theo ghi chú của lập trình viên.
2.  Logic gốc (nếu được kích hoạt) sẽ kiểm tra nếu giá trị của `bsd_depositamount` hoặc `bsd_amountwaspaid` (cả hai đều là kiểu Money) khác 0.
3.  Nếu số tiền đã trả khác 0, nó sẽ gọi `HandleError` và đặt `result = false`.

### CheckDueDate(ref bool result, Entity item, Entity enInstallment, Entity enHD)

#### Chức năng tổng quát:
Đảm bảo rằng ngày đến hạn mới (`bsd_duedatenew`) phải lớn hơn ngày đến hạn của đợt thanh toán trước đó và nhỏ hơn ngày đến hạn của đợt thanh toán sau đó.

#### Logic nghiệp vụ chi tiết:
1.  Lấy ngày đến hạn mới (`newDate`) từ bản ghi Detail.
2.  Truy vấn tất cả các đợt thanh toán (`Installment`) khác thuộc cùng một hợp đồng (`enHD`).
3.  Gọi `GetListDetail()` để lấy danh sách các bản ghi Detail khác đang được xử lý trong cùng một Master. Danh sách này được sử dụng để ưu tiên kiểm tra với ngày mới đề xuất của các đợt khác (nếu có).
4.  Lặp qua từng đợt thanh toán khác (`JItem`):
    *   **Kiểm tra đợt trước (Order Number nhỏ hơn):**
        *   Nếu `bsd_ordernumber` của `JItem` nhỏ hơn đợt hiện tại, kiểm tra xem `newDate` có sớm hơn ngày đến hạn của đợt trước đó không.
        *   Ưu tiên sử dụng `bsd_duedatenew` từ bản ghi Detail khác (nếu tìm thấy trong `lstDetail`).
        *   Nếu `newDate` sớm hơn hoặc bằng ngày của đợt trước, gọi `HandleError` và đặt `result = false`.
    *   **Kiểm tra đợt sau (Order Number lớn hơn):**
        *   Nếu `bsd_ordernumber` của `JItem` lớn hơn đợt hiện tại, kiểm tra xem `newDate` có muộn hơn ngày đến hạn của đợt sau đó không.
        *   Ưu tiên sử dụng `bsd_duedatenew` từ bản ghi Detail khác (nếu tìm thấy trong `lstDetail`).
        *   Nếu `newDate` muộn hơn hoặc bằng ngày của đợt sau, gọi `HandleError` và đặt `result = false`.

### GetListDetail()

#### Chức năng tổng quát:
Truy vấn và trả về danh sách các bản ghi chi tiết (`bsd_updateduedatedetail`) khác thuộc cùng một bản ghi Master và chưa bị xử lý lỗi.

#### Logic nghiệp vụ chi tiết:
1.  Tạo một `QueryExpression` nhắm vào entity `bsd_updateduedatedetail`.
2.  Thiết lập hai điều kiện lọc:
    *   `statuscode` không bằng `100000003` (Trạng thái "Error").
    *   `bsd_updateduedate` (trường tham chiếu Master) bằng ID của bản ghi Master hiện tại (`enMaster.Id`).
3.  Thực thi truy vấn và trả về `EntityCollection` kết quả.

### CheckNewDate(ref bool result, Entity item, Entity enInstallment)

#### Chức năng tổng quát:
Kiểm tra xem ngày đến hạn mới (`bsd_duedatenew`) có trùng với ngày đến hạn cũ (`bsd_duedate`) hay không.

#### Logic nghiệp vụ chi tiết:
1.  **LƯU Ý:** Toàn bộ logic kiểm tra so sánh ngày hiện đang bị vô hiệu hóa (comment out).
2.  Logic gốc (nếu được kích hoạt) sẽ so sánh `newDate` với `bsd_duedate` của Installment, có tính đến việc điều chỉnh múi giờ (cộng 7 giờ).
3.  Nếu hai ngày trùng nhau, nó sẽ gọi `HandleError` và đặt `result = false`.

### Approve(ref bool result, Entity item, Entity enInstallment)

#### Chức năng tổng quát:
Thực hiện hành động phê duyệt cuối cùng bằng cách gọi một Custom Action (`bsd_Action_Approved_Updateduedate_Master`) để cập nhật trạng thái và ngày đến hạn trên các bản ghi liên quan.

#### Logic nghiệp vụ chi tiết:
1.  Tạo một `OrganizationRequest` để gọi Custom Action có tên `bsd_Action_Approved_Updateduedate_Master`.
2.  Lấy ngày đến hạn mới (`newDate`).
3.  Thiết lập các tham số đầu vào cho Custom Action:
    *   `detail_id`: ID của bản ghi Detail hiện tại.
    *   `duedatenew`: Ngày mới được chuyển đổi sang chuỗi, đồng thời được điều chỉnh bằng cách thêm 7 giờ (để xử lý múi giờ UTC+7).
    *   `statuscode`: Đặt là `100000000` (giá trị cho trạng thái "Approved").
4.  Thực thi Custom Action bằng `service.Execute(request)`.

### HandleError(Entity item, string error)

#### Chức năng tổng quát:
Xử lý khi có lỗi xảy ra trong quá trình kiểm tra nghiệp vụ hoặc lỗi ngoại lệ, bằng cách cập nhật trạng thái của bản ghi Master và Detail thành "Error" và ghi lại thông báo lỗi.

#### Logic nghiệp vụ chi tiết:
1.  **Cập nhật Master:**
    *   Truy vấn tham chiếu Master từ Detail.
    *   Tạo Entity Master mới và cập nhật các trường: `bsd_error = true`, `bsd_errordetail = error`.
    *   Xóa thông tin phê duyệt cũ (`bsd_approvedrejecteddate`, `bsd_approvedrejectedperson`) bằng cách đặt chúng thành `null`.
    *   Thực hiện `service.Update(enMaster)`.
2.  **Cập nhật Detail:**
    *   Tạo Entity Detail mới.
    *   Cập nhật `statuscode` thành `100000003` (Trạng thái "Error").
    *   Cập nhật `bsd_errordetail = error`.
    *   Thực hiện `service.Update(enupdate)`.

### CheckConditionRun(Entity item)

#### Chức năng tổng quát:
Kiểm tra các điều kiện ban đầu để quyết định có thực thi plugin hay không.

#### Logic nghiệp vụ chi tiết:
1.  Hàm này hiện tại chỉ trả về giá trị `true` và không thực hiện bất kỳ kiểm tra điều kiện nào.
2.  Logic gốc bị vô hiệu hóa (comment out) dường như nhằm mục đích kiểm tra nếu bản ghi Master đang ở trạng thái lỗi (`bsd_error == true`) và không đang được xử lý, thì sẽ đặt lại trạng thái của Detail về trạng thái ban đầu và dừng plugin.