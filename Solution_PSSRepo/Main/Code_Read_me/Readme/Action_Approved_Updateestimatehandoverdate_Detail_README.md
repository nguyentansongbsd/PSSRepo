# Phân tích mã nguồn: Action_Approved_Updateestimatehandoverdate_Detail.cs

## Tổng quan

Tệp mã nguồn này định nghĩa một Plugin Dynamics 365 (C#) được thiết kế để thực hiện các hành động nghiệp vụ phức tạp sau khi một bản ghi chi tiết yêu cầu cập nhật ngày bàn giao ước tính (`bsd_updateestimatehandoverdatedetail`) được phê duyệt.

Chức năng chính của Plugin là xác thực các điều kiện nghiệp vụ (như trạng thái hợp đồng, ngày đến hạn thanh toán, và tính hợp lệ của dự án) và sau đó cập nhật đồng bộ các trường ngày tháng liên quan (Ngày bàn giao ước tính, Ngày OP, Ngày đến hạn thanh toán) trên các bản ghi liên quan như Đơn vị (Unit), Hợp đồng (Option Entry), và Chi tiết đợt thanh toán (Installment). Logic cập nhật được phân nhánh dựa trên loại cập nhật được chỉ định trong bản ghi cha (Master).

## Chi tiết các Hàm (Functions/Methods)

### Execute(IServiceProvider serviceProvider)

#### Chức năng tổng quát:
Đây là điểm vào chính của Plugin. Hàm này khởi tạo các dịch vụ Dynamics 365, truy xuất dữ liệu bản ghi chi tiết và bản ghi cha, thực hiện các kiểm tra điều kiện ban đầu, và điều phối logic cập nhật dựa trên loại hình cập nhật được yêu cầu.

#### Logic nghiệp vụ chi tiết:
1.  **Khởi tạo Dịch vụ:** Khởi tạo các đối tượng `IPluginExecutionContext`, `IOrganizationServiceFactory`, `IOrganizationService`, và `ITracingService`.
2.  **Truy xuất Dữ liệu:**
    *   Lấy ID của bản ghi chi tiết (`enDetailid`) từ `context.InputParameters["id"]`.
    *   Truy xuất bản ghi chi tiết (`en`) thuộc loại `bsd_updateestimatehandoverdatedetail`.
    *   Truy xuất bản ghi cha (`enMaster`) thuộc loại `bsd_updateestimatehandoverdate` thông qua tham chiếu từ bản ghi chi tiết.
3.  **Kiểm tra Điều kiện Ban đầu:** Gọi `CheckConditionRun(en)`. Hiện tại, hàm này luôn trả về `true`.
4.  **Xác thực Dự án:** Gọi `CheckExistParentInDetail(ref result, item)` để đảm bảo Dự án trên bản ghi chi tiết khớp với Dự án trên bản ghi cha. Nếu không khớp, quá trình dừng lại.
5.  **Phân nhánh Logic (Dựa trên `bsd_types` của bản ghi cha):**
    *   **Case 100000000 (Update Only for Units):**
        *   Truy xuất bản ghi Đơn vị (Unit) liên quan.
        *   Gọi `UpdateEstimateHandoverDateFromDetailToUnit` để cập nhật Ngày bàn giao ước tính trên Unit.
        *   Gọi `UpdateOPDateFromMasterToUnit` để cập nhật Ngày OP trên Unit.
        *   Gọi `AprroveDetail(item)` để đánh dấu bản ghi chi tiết là đã phê duyệt.
    *   **Default Case (Bao gồm Hợp đồng và Đợt thanh toán):**
        *   **Kiểm tra Hợp đồng:** Kiểm tra xem bản ghi chi tiết có chứa tham chiếu đến Hợp đồng (`bsd_optionentry`) hay không. Nếu không, gọi `HandleError` và dừng lại.
        *   Truy xuất bản ghi Hợp đồng (`enHD`) và bản ghi Chi tiết đợt thanh toán (`enInstallment`).
        *   **Xác thực Trạng thái Hợp đồng:** Gọi `CheckStatusHD` để đảm bảo hợp đồng chưa được bàn giao hoặc thanh lý.
        *   **Xác thực Ngày đến hạn:** Gọi `CheckDueDate` để đảm bảo Ngày đến hạn mới (`bsd_paymentduedate`) nằm giữa ngày đến hạn của đợt thanh toán trước và sau.
        *   **Phân nhánh Cập nhật Chi tiết:**
            *   Nếu `bsd_types == 100000001` (Update all): Gọi `UpdateFromDetailToUnitToInstallmentToHD` để cập nhật Unit, Installment, và Hợp đồng. Sau đó gọi `UpdateOPDateFromMasterToUnit`.
            *   Nếu `bsd_types == 100000002`: Gọi `UpdateFromDetailToInstallment` để cập nhật Installment. Sau đó gọi `UpdateOPDateFromMasterToUnit`.
        *   Gọi `AprroveDetail(item)` để đánh dấu bản ghi chi tiết là đã phê duyệt.
6.  **Xử lý Ngoại lệ:** Nếu có bất kỳ lỗi nào xảy ra trong khối `try`, gọi `HandleError` để ghi lại lỗi vào cả bản ghi chi tiết và bản ghi cha.

### AprroveDetail(Entity item)

#### Chức năng tổng quát:
Hàm này cập nhật trạng thái của bản ghi chi tiết yêu cầu cập nhật ngày bàn giao thành trạng thái "Đã phê duyệt" (Approved).

#### Logic nghiệp vụ chi tiết:
1.  Tạo một đối tượng `Entity` mới chỉ chứa ID và tên logic của bản ghi chi tiết (`item`).
2.  Thiết lập trường `statuscode` thành `OptionSetValue(100000000)`.
3.  Thực hiện lệnh `service.Update(enDetailUpdate)` để lưu thay đổi.

### CheckExistParentInDetail(ref bool result, Entity item)

#### Chức năng tổng quát:
Hàm này xác minh rằng Dự án được tham chiếu trong bản ghi chi tiết phải khớp với Dự án được tham chiếu trong bản ghi cha (Master).

#### Logic nghiệp vụ chi tiết:
1.  Truy xuất bản ghi cha (`enMaster`) thông qua tham chiếu `bsd_updateestimatehandoverdate` từ bản ghi chi tiết.
2.  So sánh ID của trường `bsd_project` trong bản ghi cha với ID của trường `bsd_project` trong bản ghi chi tiết (`item`).
3.  Nếu hai ID không khớp, thiết lập `result = false`, gọi `HandleError` với thông báo lỗi về dự án không hợp lệ, và dừng thực thi.

### CheckConditionRun(Entity item)

#### Chức năng tổng quát:
Hàm này được thiết kế để kiểm tra các điều kiện chung trước khi chạy logic cập nhật chính.

#### Logic nghiệp vụ chi tiết:
1.  Hiện tại, hàm này luôn trả về `true`, cho phép Plugin tiếp tục thực thi bất kể trạng thái nào. (Các dòng mã bị chú thích cho thấy trước đây nó có thể đã kiểm tra các cờ lỗi (`bsd_error`, `bsd_processing_pa`) trên bản ghi cha).

### HandleError(Entity item, string error)

#### Chức năng tổng quát:
Hàm này xử lý việc ghi lại lỗi nghiệp vụ hoặc lỗi hệ thống vào cả bản ghi cha và bản ghi chi tiết, đồng thời cập nhật trạng thái của chúng.

#### Logic nghiệp vụ chi tiết:
1.  Ghi lại thông báo lỗi vào `tracingService`.
2.  **Cập nhật Bản ghi Cha (Master):**
    *   Tạo đối tượng `Entity` cho bản ghi cha.
    *   Thiết lập `bsd_error = true`.
    *   Thiết lập `bsd_errordetail` bằng thông báo lỗi đã nhận.
    *   Thực hiện `service.Update` cho bản ghi cha.
3.  **Cập nhật Bản ghi Chi tiết (Detail):**
    *   Tạo đối tượng `Entity` cho bản ghi chi tiết.
    *   Thiết lập `statuscode` thành `OptionSetValue(100000002)` (Trạng thái Lỗi).
    *   Thiết lập `bsd_errordetail` bằng thông báo lỗi đã nhận.
    *   Thực hiện `service.Update` cho bản ghi chi tiết.

### UpdateOPDateFromMasterToUnit(ref bool result, Entity master, Entity enUnit)

#### Chức năng tổng quát:
Cập nhật trường Ngày OP (`bsd_opdate`) trên bản ghi Đơn vị (Unit) bằng giá trị Ngày bàn giao ước tính mới từ bản ghi Chi tiết.

#### Logic nghiệp vụ chi tiết:
1.  Tạo đối tượng `Entity` cập nhật cho bản ghi Unit (`enUnit`).
2.  Kiểm tra xem bản ghi chi tiết (`item`) có chứa trường `bsd_estimatehandoverdatenew` hay không. Nếu không, hàm thoát.
3.  Thiết lập trường `bsd_opdate` của Unit bằng giá trị của `bsd_estimatehandoverdatenew` từ bản ghi chi tiết.
4.  Thực hiện `service.Update` cho bản ghi Unit.

### UpdateEstimateHandoverDateFromDetailToUnit(ref bool result, Entity item, Entity enUnit)

#### Chức năng tổng quát:
Cập nhật trường Ngày bàn giao ước tính (`bsd_estimatehandoverdate`) trên bản ghi Đơn vị (Unit) bằng giá trị Ngày bàn giao ước tính mới từ bản ghi Chi tiết.

#### Logic nghiệp vụ chi tiết:
1.  Tạo đối tượng `Entity` cập nhật cho bản ghi Unit (`enUnit`).
2.  Thiết lập trường `bsd_estimatehandoverdate` của Unit bằng giá trị của `bsd_estimatehandoverdatenew` từ bản ghi chi tiết (`item`).
3.  Thực hiện `service.Update` cho bản ghi Unit.

### CheckStatusHD(ref bool result, Entity item, Entity enHD)

#### Chức năng tổng quát:
Kiểm tra trạng thái của Hợp đồng (Option Entry) để đảm bảo rằng nó chưa được bàn giao hoặc thanh lý.

#### Logic nghiệp vụ chi tiết:
1.  Lấy giá trị `statuscode` của bản ghi Hợp đồng (`enHD`).
2.  **Kiểm tra Bàn giao:** Nếu `statuscode` là `100000005` (Đã bàn giao), thiết lập `result = false`, gọi `HandleError` và dừng lại.
3.  **Kiểm tra Thanh lý:** Nếu `statuscode` là `100000006` (Đã thanh lý), thiết lập `result = false`, gọi `HandleError` và dừng lại.

### CheckPaid(ref bool result, Entity item, Entity enInstallment)

#### Chức năng tổng quát:
Kiểm tra xem đợt thanh toán (Installment) có bất kỳ khoản tiền nào đã được thanh toán hay chưa (Tiền đặt cọc hoặc Số tiền đã thanh toán).

#### Logic nghiệp vụ chi tiết:
1.  Kiểm tra giá trị của `bsd_depositamount` (Số tiền đặt cọc) và `bsd_amountwaspaid` (Số tiền đã thanh toán) trên bản ghi Installment.
2.  Nếu một trong hai giá trị này khác 0 (kiểu `Money`), thiết lập `result = false`, gọi `HandleError` với thông báo lỗi về việc đợt thanh toán đã được trả, và dừng lại.
*(Lưu ý: Hàm này được định nghĩa nhưng bị chú thích (comment out) trong hàm `Execute`.)*

### CheckDueDate(ref bool result, Entity item, Entity enInstallment, Entity enHD)

#### Chức năng tổng quát:
Xác thực rằng Ngày đến hạn mới (`bsd_paymentduedate`) trên bản ghi chi tiết phải nằm giữa ngày đến hạn của đợt thanh toán trước và đợt thanh toán sau trong cùng một Hợp đồng.

#### Logic nghiệp vụ chi tiết:
1.  Lấy ngày đến hạn mới (`newDate`) từ bản ghi chi tiết (`item`).
2.  **Truy vấn các đợt thanh toán khác:**
    *   Tạo `QueryExpression` để truy vấn tất cả các bản ghi Installment khác liên quan đến cùng Hợp đồng (`enHD.Id`) nhưng không phải là bản ghi Installment hiện tại (`enInstallment.Id`).
3.  **Lặp qua các đợt thanh toán liên quan:**
    *   Đối với mỗi đợt thanh toán (`JItem`) được truy vấn:
        *   Bỏ qua nếu `JItem` không chứa trường `bsd_duedate`.
        *   **So sánh với đợt trước:** Nếu `bsd_ordernumber` của `JItem` nhỏ hơn `bsd_ordernumber` của `enInstallment` (đợt trước):
            *   Kiểm tra xem `newDate` có nhỏ hơn hoặc bằng ngày đến hạn của `JItem` hay không (sau khi điều chỉnh múi giờ 7 giờ). Nếu có, ngày mới không hợp lệ.
        *   **So sánh với đợt sau:** Nếu `bsd_ordernumber` của `JItem` lớn hơn `bsd_ordernumber` của `enInstallment` (đợt sau):
            *   Kiểm tra xem `newDate` có lớn hơn hoặc bằng ngày đến hạn của `JItem` hay không (sau khi điều chỉnh múi giờ 7 giờ). Nếu có, ngày mới không hợp lệ.
4.  Nếu bất kỳ điều kiện nào không được đáp ứng, thiết lập `result = false`, gọi `HandleError` và thoát khỏi vòng lặp.

### UpdateFromDetailToUnitToInstallmentToHD(ref bool result, Entity item, Entity enInstallment, Entity unit, Entity enHD)

#### Chức năng tổng quát:
Thực hiện cập nhật đồng bộ Ngày bàn giao ước tính và Ngày đến hạn trên ba thực thể: Unit, Installment, và Hợp đồng (áp dụng cho loại cập nhật 100000001 - Update all).

#### Logic nghiệp vụ chi tiết:
1.  **Cập nhật Unit:**
    *   Cập nhật trường `bsd_estimatehandoverdate` trên Unit bằng `bsd_estimatehandoverdatenew` từ Detail.
2.  **Cập nhật Installment:**
    *   Cập nhật trường `bsd_duedate` trên Installment bằng `bsd_paymentduedate` từ Detail.
3.  **Cập nhật Hợp đồng (Option Entry):**
    *   Cập nhật trường `bsd_estimatehandoverdatecontract` trên Hợp đồng bằng `bsd_estimatehandoverdatenew` từ Detail.
4.  Thực hiện `service.Update` cho cả ba bản ghi.

### UpdateFromDetailToInstallment(ref bool result, Entity item, Entity enInstallment)

#### Chức năng tổng quát:
Cập nhật trường Ngày đến hạn (`bsd_duedate`) trên bản ghi Chi tiết đợt thanh toán (Installment) (áp dụng cho loại cập nhật 100000002).

#### Logic nghiệp vụ chi tiết:
1.  Tạo đối tượng `Entity` cập nhật cho bản ghi Installment (`enInstallment`).
2.  Thiết lập trường `bsd_duedate` của Installment bằng giá trị của `bsd_paymentduedate` từ bản ghi chi tiết (`item`).
3.  Thực hiện `service.Update` cho bản ghi Installment.