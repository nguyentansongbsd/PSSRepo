# Phân tích mã nguồn: Plugin_Create_UpdateDueDateDetail.cs

## Tổng quan

Tệp mã nguồn `Plugin_Create_UpdateDueDateDetail.cs` định nghĩa một Plugin Dynamics 365/Power Platform được kích hoạt khi tạo hoặc cập nhật bản ghi chi tiết thay đổi ngày đến hạn (`bsd_updateduedatedetail`).

Plugin này chịu trách nhiệm thực hiện một loạt các kiểm tra nghiệp vụ phức tạp và xác thực dữ liệu trước khi cho phép thay đổi ngày đến hạn của một đợt thanh toán (`bsd_paymentschemedetail`). Các kiểm tra bao gồm xác định loại hợp đồng liên quan (Đơn hàng, Báo giá, hoặc Báo giá nội bộ), đảm bảo tính toàn vẹn của dữ liệu dự án, kiểm tra trạng thái thanh toán và trạng thái thanh lý của hợp đồng, và xác minh rằng ngày đến hạn mới không trùng lặp hoặc vi phạm thứ tự thời gian với các đợt thanh toán khác.

## Chi tiết các Hàm (Functions/Methods)

### Execute(IServiceProvider serviceProvider)

#### Chức năng tổng quát:
Đây là điểm vào chính của Plugin. Hàm này khởi tạo các dịch vụ CRM, truy xuất bản ghi mục tiêu (`bsd_updateduedatedetail`), thực hiện kiểm tra trạng thái ban đầu, xác định loại giao dịch liên quan (Hợp đồng/Báo giá), liên kết bản ghi đợt thanh toán nếu cần, và chạy chuỗi các hàm xác thực nghiệp vụ.

#### Logic nghiệp vụ chi tiết:
1.  **Khởi tạo Dịch vụ:** Lấy các dịch vụ tiêu chuẩn của Plugin (Context, Factory, Service, Tracing Service).
2.  **Truy xuất Target:** Lấy bản ghi mục tiêu (`entity`) từ `context.InputParameters["Target"]`. Sau đó, truy xuất lại bản ghi đầy đủ (`en`) của `bsd_updateduedatedetail` bằng ID.
3.  **Kiểm tra Trạng thái (Status Gate):**
    *   Lấy giá trị `statuscode` của bản ghi chi tiết.
    *   Nếu `status` KHÔNG phải là `1` (Active/Pending) và KHÔNG phải là `100000003` (một trạng thái tùy chỉnh, có thể là Draft/New), Plugin sẽ thoát (`return`) mà không thực hiện thêm hành động nào.
4.  **Xác định Loại Giao dịch (Header Entity):**
    *   Sử dụng một khối `#region check giao dịch` để xác định bản ghi Header (Hợp đồng) liên quan dựa trên các trường lookup tồn tại trên bản ghi chi tiết:
        *   Nếu chứa `bsd_optionentry`: Header là `salesorder`, trường liên kết là `bsd_optionentry`.
        *   Nếu chứa `bsd_quote`: Header là `quote`, trường liên kết là `bsd_reservation`.
        *   Nếu chứa `bsd_quotation`: Header là `bsd_quotation`, trường liên kết là `bsd_quotation`.
    *   Nếu không tìm thấy trường liên kết nào, gọi `HandleError` ("not found entity!").
5.  **Liên kết Đợt Thanh toán (Installment):**
    *   Kiểm tra nếu trường `bsd_installment` (lookup đến `bsd_paymentschemedetail`) chưa được điền.
    *   Nếu thiếu, thực hiện truy vấn (`QueryExpression`) đến entity `bsd_paymentschemedetail` để tìm đợt thanh toán phù hợp:
        *   Điều kiện 1: `bsd_ordernumber` bằng `bsd_installmentnumber` trên bản ghi chi tiết.
        *   Điều kiện 2: Trường liên kết Header (`enIntalments_fieldNameHD`) bằng ID của Header (lấy từ `bsd_quote` hoặc `bsd_optionentry` trên bản ghi chi tiết).
    *   Nếu tìm thấy:
        *   Cập nhật trường `bsd_installment` trên bản ghi chi tiết.
        *   Truy xuất bản ghi Header (`endHD`) để lấy thông tin Đơn vị (`bsd_unitnumber` hoặc `bsd_unitno`).
        *   Tạo một Entity tạm thời (`enTempUpdate`) để cập nhật bản ghi chi tiết, bao gồm: `bsd_units`, `bsd_installment`, và `bsd_duedateold` (Ngày đến hạn cũ, được lấy từ đợt thanh toán và **cộng thêm 7 giờ**).
    *   Nếu không tìm thấy đợt thanh toán, ném lỗi (`InvalidPluginExecutionException`).
6.  **Truy xuất Bản ghi Liên quan:** Lấy bản ghi Đợt Thanh toán (`enInstallment`) và bản ghi Header (`enHD`) đầy đủ.
7.  **Thực hiện Xác thực:** Gọi tuần tự các hàm kiểm tra nghiệp vụ. Nếu bất kỳ hàm nào đặt `result = false`, Plugin sẽ thoát ngay lập tức.
    *   `CheckExistParentInDetail(ref result, item)`
    *   `CheckIsLast(ref result, item, enInstallment)`
    *   `CheckHD(ref result, item, enHD)`
    *   `CheckPaidDetail(ref result, item, enInstallment)` (Lưu ý: Logic bên trong hàm này hiện đang bị comment/vô hiệu hóa.)
    *   `CheckNewDate(ref result, item, enInstallment)`
8.  **Xử lý Lỗi:** Sử dụng `try-catch` để bắt các ngoại lệ và gọi `HandleError` để ghi log và ném lỗi CRM.

### CheckExistParentInDetail(ref bool result, Entity item)

#### Chức năng tổng quát:
Kiểm tra xem Dự án được liên kết trên bản ghi Chi tiết (`item`) có khớp với Dự án được liên kết trên bản ghi Master (`bsd_updateduedate`) hay không.

#### Logic nghiệp vụ chi tiết:
1.  Truy xuất bản ghi Master (`enMaster`) thông qua lookup `bsd_updateduedate` trên bản ghi Chi tiết.
2.  So sánh ID của trường `bsd_project` trên bản ghi Master với ID của trường `bsd_project` trên bản ghi Chi tiết (`item`).
3.  Nếu hai ID không khớp, gọi `HandleError` với thông báo lỗi và đặt `result = false`.

### CheckIsLast(ref bool result, Entity item, Entity enInstallment)

#### Chức năng tổng quát:
Kiểm tra xem đợt thanh toán đang được thay đổi có phải là đợt cuối cùng hoặc đợt tính theo ngày bàn giao ước tính hay không.

#### Logic nghiệp vụ chi tiết:
1.  **Kiểm tra Đợt cuối cùng:** Kiểm tra giá trị boolean của trường `bsd_lastinstallment` trên bản ghi Đợt Thanh toán (`enInstallment`). Nếu là `true`, gọi `HandleError` và đặt `result = false`.
2.  **Kiểm tra Phương thức tính toán:** Kiểm tra xem trường `bsd_duedatecalculatingmethod` có tồn tại và giá trị của nó có phải là `100000002` (Estimate handover date) hay không. Nếu đúng, gọi `HandleError` và đặt `result = false`.

### CheckHD(ref bool result, Entity item, Entity enHD)

#### Chức năng tổng quát:
Kiểm tra trạng thái của Hợp đồng/Giao dịch (Header Entity - `enHD`) liên quan để đảm bảo nó không ở trạng thái đã bị thanh lý/chấm dứt.

#### Logic nghiệp vụ chi tiết:
1.  Sử dụng cấu trúc `switch` dựa trên tên entity Header (`enHD_name`) đã được xác định trong hàm `Execute`.
2.  **Trường hợp `bsd_quotation` hoặc `quote`:** Nếu `statuscode` là `100000001` (Terminated/Liquidated), gọi `HandleError` và đặt `result = false`.
3.  **Trường hợp `salesorder`:** Nếu `statuscode` là `100000006` (Terminated/Liquidated), gọi `HandleError` và đặt `result = false`.

### CheckPaidDetail(ref bool result, Entity item, Entity enInstallment)

#### Chức năng tổng quát:
(Logic nghiệp vụ bị vô hiệu hóa) Chức năng này ban đầu được thiết kế để kiểm tra xem đợt thanh toán đã được thanh toán một phần hay toàn bộ hay chưa.

#### Logic nghiệp vụ chi tiết:
1.  Mặc dù hàm được gọi trong `Execute`, toàn bộ logic kiểm tra bên trong hàm này hiện đang bị comment (vô hiệu hóa) bởi nhà phát triển.
2.  Logic bị comment ban đầu kiểm tra nếu `bsd_depositamount` hoặc `bsd_amountwaspaid` khác 0. Nếu đúng, nó sẽ ném lỗi.

### CheckDueDate(ref bool result, Entity item, Entity enInstallment, Entity enHD)

#### Chức năng tổng quát:
Kiểm tra tính hợp lệ của Ngày đến hạn mới (`bsd_duedatenew`) so với các đợt thanh toán khác trong cùng một Hợp đồng/Giao dịch, đảm bảo thứ tự thời gian không bị vi phạm.

#### Logic nghiệp vụ chi tiết:
*(Lưu ý: Hàm này hiện đang bị comment (vô hiệu hóa) trong hàm `Execute`.)*
1.  Lấy Ngày đến hạn mới (`newDate`) từ bản ghi Chi tiết.
2.  Truy vấn tất cả các đợt thanh toán (`bsd_paymentschemedetail`) khác (`ConditionOperator.NotEqual`) thuộc cùng một Hợp đồng/Giao dịch (`enIntalments_fieldNameHD`).
3.  Lặp qua từng đợt thanh toán khác (`JItem`):
    *   **So sánh với đợt trước:** Nếu số thứ tự đợt (`bsd_ordernumber`) của `JItem` nhỏ hơn số thứ tự của đợt hiện tại (`enInstallment`):
        *   Kiểm tra xem hiệu số ngày giữa `newDate` và ngày đến hạn cũ của `JItem` (đã cộng 7 giờ) có nhỏ hơn hoặc bằng 0 hay không.
        *   Nếu `newDate` sớm hơn hoặc bằng ngày đến hạn của đợt trước, ném lỗi ("The new due date is earlier than the next batch.") và đặt `result = false`.
    *   **So sánh với đợt sau:** Nếu số thứ tự đợt của `JItem` lớn hơn số thứ tự của đợt hiện tại:
        *   Kiểm tra xem hiệu số ngày giữa `newDate` và ngày đến hạn cũ của `JItem` (đã cộng 7 giờ) có lớn hơn hoặc bằng 0 hay không.
        *   Nếu `newDate` muộn hơn hoặc bằng ngày đến hạn của đợt sau, ném lỗi ("The new due date is later than the previous batch.") và đặt `result = false`.

### CheckNewDate(ref bool result, Entity item, Entity enInstallment)

#### Chức năng tổng quát:
Kiểm tra xem Ngày đến hạn mới (`bsd_duedatenew`) có trùng với Ngày đến hạn cũ (`bsd_duedate`) của đợt thanh toán hay không.

#### Logic nghiệp vụ chi tiết:
1.  Lấy Ngày đến hạn mới (`newDate`) từ bản ghi Chi tiết.
2.  Nếu trường `bsd_duedate` không tồn tại trên bản ghi Đợt Thanh toán, hàm thoát (`return`).
3.  So sánh `newDate` với `bsd_duedate` trên bản ghi Đợt Thanh toán (đã **cộng thêm 7 giờ**).
4.  Nếu hiệu số ngày giữa hai ngày bằng 0 (tức là hai ngày trùng nhau), gọi `HandleError` ("The new due date is the same as the old due date.") và đặt `result = false`.

### HandleError(Entity item, string error)

#### Chức năng tổng quát:
Hàm tiện ích dùng để ghi lại thông báo lỗi vào Tracing Service và ném ra ngoại lệ Plugin tiêu chuẩn.

#### Logic nghiệp vụ chi tiết:
1.  Ghi thông báo lỗi (`error`) vào Tracing Service để hỗ trợ gỡ lỗi.
2.  Ném ra một `InvalidPluginExecutionException` chứa thông báo lỗi, khiến giao dịch CRM bị hủy bỏ và hiển thị lỗi cho người dùng.