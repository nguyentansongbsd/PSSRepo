# Phân tích mã nguồn: Action_CustomerNotices_Generate.cs

## Tổng quan

Tệp mã nguồn `Action_CustomerNotices_Generate.cs` chứa một plugin Dynamics 365 (C#) được thiết kế để xử lý quy trình tạo Thông báo Thanh toán Khách hàng (`bsd_customernotices`) và Thông báo Cảnh báo (`bsd_warningnotices`). Plugin này hoạt động như một phần của quy trình nhiều bước (có thể được gọi từ Power Automate hoặc một quy trình tùy chỉnh khác) dựa trên các tham số đầu vào xác định bước thực thi (`Units` - được sử dụng như một chỉ báo bước).

Plugin tương tác chủ yếu với các thực thể liên quan đến đơn hàng bán hàng (Sales Order/Option Entry), chi tiết kế hoạch thanh toán (Payment Scheme Detail - Installments), và các bản ghi tạo thông báo tổng thể (`bsd_genpaymentnotices`).

## Chi tiết các Hàm (Functions/Methods)

### IPlugin.Execute(IServiceProvider serviceProvider)

**Chức năng tổng quát:**
Đây là điểm vào chính của plugin, chịu trách nhiệm khởi tạo dịch vụ CRM và điều phối logic nghiệp vụ dựa trên tham số đầu vào `Units` (được sử dụng để xác định bước xử lý: "Bước 01", "Bước 02", hoặc "Bước 03").

**Logic nghiệp vụ chi tiết:**
1.  **Khởi tạo:** Lấy các dịch vụ cần thiết (Context, Tracing, Factory) và tạo đối tượng `IOrganizationService` sử dụng ID người dùng hiện tại (`context.UserId`).
2.  **Thu thập Tham số Đầu vào:** Lấy bốn tham số đầu vào từ Context: `input01` (Units - xác định bước), `input02` (Project/GenPaymentNotices ID), `input03` (OptionEntryId), và `input04` (Owner ID).
3.  **Xử lý Bước 01 (Lấy danh sách Option Entry):**
    *   Điều kiện: `input01 == "Bước 01"` và `input02 != ""`.
    *   Cập nhật bản ghi `bsd_genpaymentnotices` (sử dụng `input02` làm ID) và đặt trường `bsd_powerautomate` thành `true`.
    *   Truy vấn thực thể `bsd_configgolive` để tìm URL của Power Automate (dựa trên `bsd_name = "GenPaymentNotices"`). Nếu không tìm thấy URL, ném ra ngoại lệ.
    *   Đặt URL tìm được vào tham số đầu ra `context.OutputParameters["Count"]`.
    *   Truy xuất bản ghi `bsd_genpaymentnotices` (`enTarget`).
    *   **Xây dựng FetchXML:** Xây dựng truy vấn FetchXML phức tạp để tìm tất cả các bản ghi `salesorder` (Option Entry) hợp lệ trong dự án liên quan.
        *   Các điều kiện lọc chung cho `salesorder`: `statuscode` không phải `100000006`, `totalamount` lớn hơn 0, có `bsd_paymentscheme`, có `customerid`, và `bsd_tobeterminated` không phải `1`.
        *   Logic liên kết (`link-entity` với `product` - Unit) được điều chỉnh dựa trên các trường có sẵn trong `enTarget`:
            *   **Mặc định:** Lọc theo `bsd_projectcode` của dự án.
            *   **Nếu có `bsd_units`:** Thêm điều kiện lọc chính xác theo ID của Unit.
            *   **Nếu có `bsd_floor`:** Thêm điều kiện lọc theo ID của Tầng.
            *   **Nếu có `bsd_block`:** Thêm điều kiện lọc theo ID của Block.
    *   Thực thi FetchXML, thu thập ID của tất cả các Option Entry hợp lệ vào danh sách `listOE`.
    *   Nếu `listOE` rỗng, ném ra ngoại lệ.
    *   Đặt danh sách ID Option Entry (được nối bằng dấu `;`) vào tham số đầu ra `context.OutputParameters["ReturnId"]`.
4.  **Xử lý Bước 02 (Tạo Payment Notices):**
    *   Điều kiện: `input01 == "Bước 02"` và tất cả `input02`, `input03`, `input04` đều có giá trị.
    *   Tạo lại `IOrganizationService` sử dụng ID người dùng từ `input04` (Owner).
    *   Truy xuất bản ghi `bsd_genpaymentnotices` (`enTarget`) và Option Entry (`OE`).
    *   Chuyển đổi thời gian UTC trong trường `bsd_date` của `enTarget` sang giờ địa phương bằng hàm `RetrieveLocalTimeFromUTCTime`.
    *   Tìm kiếm Payment Scheme liên quan để lấy `bsd_paymentnoticesdate` (`PN_Date`) - số ngày trước ngày đáo hạn cần tạo thông báo.
    *   Truy xuất các chi tiết kế hoạch thanh toán (Installments - `l_PSD`) liên quan đến Option Entry.
    *   **Vòng lặp qua các Installment (`PSD`):**
        *   **Kiểm tra đợt cuối:** Nếu là đợt cuối (`bsd_lastinstallment = true`) và trạng thái là Not Paid (`100000001`), kiểm tra xem có bản ghi `bsd_miscellaneous` hoạt động liên quan không. Nếu không có, bỏ qua Installment này.
        *   Tính toán số tiền cho khách hàng và ngân hàng.
        *   Tính toán `nday`: số ngày còn lại đến ngày đáo hạn (sử dụng giờ địa phương).
        *   **Điều kiện tạo Notice:** Nếu `nday` nằm trong khoảng từ 0 đến `PN_Date`.
        *   Kiểm tra xem `bsd_customernotices` đã tồn tại cho Installment này chưa (`findCustomerNoticesByIntallment`).
        *   Nếu chưa tồn tại:
            *   Tạo bản ghi `bsd_customernotices` mới, điền các trường thông tin (Tên, Chủ đề, Dự án, Khách hàng, Ngày, OE, PSD, Unit).
            *   Xử lý dịch số thứ tự đợt thanh toán (`bsd_ordernumber`) sang định dạng chữ (1st, 2nd, 3rd, 4th...).
            *   Tạo bản ghi Notice.
            *   Cập nhật Installment (`PSD`) đặt `bsd_paymentnotices = true`.
            *   Gọi hàm `generateWarningNoticesByPaymentNotices` để kiểm tra và tạo Warning Notices ngay lập tức.
5.  **Xử lý Bước 03 (Hoàn tất):**
    *   Điều kiện: `input01 == "Bước 03"` và `input02`, `input04` có giá trị.
    *   Tạo lại `IOrganizationService` sử dụng ID người dùng từ `input04`.
    *   Cập nhật bản ghi `bsd_genpaymentnotices` (ID từ `input02`), đặt `bsd_powerautomate = false`.
    *   Truy vấn để kiểm tra xem có bất kỳ `bsd_customernotices` nào được tạo liên kết với bản ghi `bsd_genpaymentnotices` này không.
    *   Nếu không có Notice nào được tạo, đặt trường `bsd_notices` bằng thông báo lỗi.
    *   Nếu có Notice được tạo, đặt `statuscode` thành `100000000` (Thành công) và xóa trường `bsd_notices`.
    *   Cập nhật bản ghi `bsd_genpaymentnotices`.

### RetrieveMultiRecord(IOrganizationService crmservices, string entity, ColumnSet column, string condition, object value)

**Chức năng tổng quát:**
Hàm tiện ích thực hiện truy vấn `QueryExpression` đơn giản để lấy nhiều bản ghi dựa trên một điều kiện lọc và đảm bảo bản ghi đang ở trạng thái hoạt động (`statecode = 0`).

**Logic nghiệp vụ chi tiết:**
1.  Tạo đối tượng `QueryExpression` cho thực thể được chỉ định.
2.  Đặt `ColumnSet` (các cột cần truy xuất).
3.  Tạo `FilterExpression` và thêm điều kiện: trường `condition` bằng `value`.
4.  Thêm điều kiện bắt buộc thứ hai: `statecode` bằng 0 (Active).
5.  Thực hiện truy vấn và trả về `EntityCollection`.

### RetrieveMultiRecord2(IOrganizationService crmservices, string entity, ColumnSet column, string condition, object value)

**Chức năng tổng quát:**
Tương tự như `RetrieveMultiRecord`, nhưng không áp dụng bộ lọc `statecode = 0`.

**Logic nghiệp vụ chi tiết:**
1.  Tạo đối tượng `QueryExpression` cho thực thể được chỉ định.
2.  Đặt `ColumnSet`.
3.  Tạo `FilterExpression` và thêm điều kiện: trường `condition` bằng `value`.
4.  Thực hiện truy vấn và trả về `EntityCollection`.

### findPaymentSchemeDetail(IOrganizationService crmservices, Entity oe)

**Chức năng tổng quát:**
Truy vấn các chi tiết kế hoạch thanh toán (`bsd_paymentschemedetail` - Installments) liên quan đến một Option Entry (Sales Order) cụ thể.

**Logic nghiệp vụ chi tiết:**
1.  Sử dụng FetchXML để truy vấn `bsd_paymentschemedetail`.
2.  Lọc các bản ghi theo ID của Option Entry (`oe.Id`).
3.  Áp dụng bộ lọc phức tạp (OR):
    *   Điều kiện 1 (AND): Liên kết với Option Entry, `statuscode` bằng `100000000` (có thể là Paid/Hoàn thành), và `bsd_duedate` không rỗng.
    *   Điều kiện 2 (AND): Liên kết với Option Entry, và `bsd_lastinstallment` bằng 1 (Đợt cuối).
4.  Sắp xếp kết quả theo `bsd_duedate` giảm dần.
5.  Thực thi FetchXML và trả về `EntityCollection`.

### findCustomerNoticesByIntallment(IOrganizationService crmservices, EntityReference ins)

**Chức năng tổng quát:**
Kiểm tra xem đã có bản ghi Thông báo Khách hàng (`bsd_customernotices`) nào được tạo cho một Installment (`bsd_paymentschemedetail`) cụ thể hay chưa.

**Logic nghiệp vụ chi tiết:**
1.  Sử dụng FetchXML để truy vấn `bsd_customernotices`.
2.  Lọc theo trường `bsd_paymentschemedetail` bằng ID của Installment (`ins.Id`).
3.  Giới hạn kết quả trả về là 1 (`count='1'`).
4.  Thực thi FetchXML và trả về `EntityCollection`.

### findPaymentScheme(IOrganizationService crmservices, EntityReference ps)

**Chức năng tổng quát:**
Truy vấn bản ghi Kế hoạch Thanh toán (`bsd_paymentscheme`) để lấy thông tin về số ngày cần tạo thông báo thanh toán trước ngày đáo hạn.

**Logic nghiệp vụ chi tiết:**
1.  Sử dụng FetchXML để truy vấn `bsd_paymentscheme`.
2.  Lọc theo ID của Payment Scheme (`ps.Id`).
3.  Yêu cầu trường `bsd_paymentnoticesdate` phải không rỗng.
4.  Giới hạn kết quả trả về là 1 (`count='1'`).
5.  Thực thi FetchXML và trả về `EntityCollection`.

### RetrieveLocalTimeFromUTCTime(DateTime utcTime, IOrganizationService service)

**Chức năng tổng quát:**
Chuyển đổi thời gian từ múi giờ UTC sang múi giờ địa phương của người dùng hiện tại trong Dynamics 365.

**Logic nghiệp vụ chi tiết:**
1.  Gọi `RetrieveCurrentUsersSettings` để lấy `TimeZoneCode` của người dùng đang thực thi.
2.  Nếu không tìm thấy mã múi giờ, ném ra ngoại lệ.
3.  Tạo yêu cầu `LocalTimeFromUtcTimeRequest`, truyền `TimeZoneCode` và thời gian UTC.
4.  Thực thi yêu cầu và trả về thời gian địa phương (`response.LocalTime`).

### RetrieveCurrentUsersSettings(IOrganizationService service)

**Chức năng tổng quát:**
Truy vấn cài đặt người dùng hiện tại để lấy mã múi giờ (`timezonecode`).

**Logic nghiệp vụ chi tiết:**
1.  Tạo `QueryExpression` trên thực thể `usersettings`.
2.  Lọc theo điều kiện `systemuserid` bằng ID người dùng hiện tại (`ConditionOperator.EqualUserId`).
3.  Truy xuất các cột `localeid` và `timezonecode`.
4.  Trả về giá trị của `timezonecode`.

### generateWarningNoticesByPaymentNotices(Guid paymentNoticesId, string date, EntityReference erfEntity)

**Chức năng tổng quát:**
Hàm này được gọi ngay sau khi tạo Payment Notice để kiểm tra và tạo các Thông báo Cảnh báo (Warning Notices - WN) nếu các điều kiện về ngày đáo hạn đã được đáp ứng.

**Logic nghiệp vụ chi tiết:**
1.  Truy xuất Payment Notice (`en`), Option Entry (`enOptionEntry`), và Payment Scheme (`enPaymentScheme`) liên quan.
2.  Kiểm tra xem Payment Scheme có định nghĩa bất kỳ ngày cảnh báo nào không (`bsd_warningnotices1date` đến `bsd_warningnotices4date`).
3.  Lấy tất cả các Installment đã thanh toán (`statuscode = 100000000`) cho Option Entry đó (`getAllInstallmentByOptionEntry`).
4.  **Vòng lặp qua các Installment (`PSD`):**
    *   Tính toán `nday`: số ngày đã *quá* ngày đáo hạn (`bsd_duedate`). Chỉ xử lý nếu `nday > 0`.
    *   Tìm kiếm các Warning Notices đã tồn tại cho Installment này (`findWarningNotices`).
    *   **Trường hợp đã có WN:**
        *   Lấy số lần cảnh báo hiện tại (`numberofWarning`).
        *   Nếu `numberofWarning` từ 1 đến 3, xác định trường ngày cảnh báo tiếp theo (ví dụ: nếu `numberofWarning` là 1, kiểm tra `bsd_warningnotices2date`).
        *   Nếu ngày quá hạn (`nday`) lớn hơn hoặc bằng ngưỡng ngày cảnh báo tiếp theo, và chưa có WN tiếp theo được tạo:
            *   Tạo bản ghi `bsd_warningnotices` mới (với số cảnh báo là `numberofWarning + 1`).
            *   Tính toán các ngày deadline (`bsd_deadlinewn1`, `bsd_deadlinewn2`) dựa trên ngày đáo hạn và ngày ân hạn (`findGraceDays`).
            *   Xử lý dịch số thứ tự đợt thanh toán sang định dạng chữ.
            *   Tạo bản ghi WN.
            *   Cập nhật Installment (`PSD`) để đánh dấu WN đã được tạo (ví dụ: đặt `bsd_warningnotices2 = true`).
    *   **Trường hợp chưa có WN:**
        *   Lấy ngưỡng ngày cho WN1 (`bsd_warningnotices1date`).
        *   Nếu `nday` lớn hơn hoặc bằng ngưỡng WN1:
            *   Tạo bản ghi `bsd_warningnotices` mới (với số cảnh báo là 1).
            *   Thực hiện các tính toán deadline và dịch số thứ tự tương tự như trên.
            *   Tạo bản ghi WN.
            *   Cập nhật Installment (`PSD`) đặt `bsd_warningnotices1 = true`.

### getAllInstallmentByOptionEntry(Entity enOptionEntry)

**Chức năng tổng quát:**
Truy vấn tất cả các chi tiết kế hoạch thanh toán (Installments) cho một Option Entry cụ thể, lọc theo trạng thái đã thanh toán/hoàn thành và có ngày đáo hạn.

**Logic nghiệp vụ chi tiết:**
1.  Tạo `QueryExpression` cho `bsd_paymentschemedetail`.
2.  Lọc theo ID của Option Entry.
3.  Lọc theo `statuscode = 100000000`.
4.  Lọc theo `bsd_duedate` không rỗng.
5.  Sắp xếp theo `bsd_duedate` tăng dần.
6.  Thực thi truy vấn và trả về `EntityCollection`.

### findWarningNotices(Entity enInstallment)

**Chức năng tổng quát:**
Tìm kiếm các Thông báo Cảnh báo (`bsd_warningnotices`) đã được tạo cho một Installment cụ thể.

**Logic nghiệp vụ chi tiết:**
1.  Tạo `QueryExpression` cho `bsd_warningnotices`.
2.  Lọc theo `bsd_paymentschemedeitail` bằng ID của Installment.
3.  Sắp xếp theo `bsd_numberofwarning` giảm dần (để lấy WN có số thứ tự cao nhất nếu có nhiều).
4.  Thực thi truy vấn và trả về `EntityCollection`.

### findWarningNoticesByNumberOfWarning(Entity paymentDet, EntityReference enfOptionEntry)

**Chức năng tổng quát:**
Tìm kiếm các Thông báo Cảnh báo đã được tạo cho một Installment và Option Entry cụ thể.

**Logic nghiệp vụ chi tiết:**
1.  Tạo `QueryExpression` cho `bsd_warningnotices`.
2.  Lọc theo `bsd_paymentschemedeitail` và `bsd_optionentry`.
3.  Sắp xếp theo `bsd_numberofwarning` giảm dần.
4.  Thực thi truy vấn và trả về `EntityCollection`.

### findGraceDays(EntityReference ps)

**Chức năng tổng quát:**
Tìm kiếm số ngày ân hạn (`bsd_gracedays`) liên quan đến Payment Scheme thông qua thực thể trung gian Interest Rate Master.

**Logic nghiệp vụ chi tiết:**
1.  Tạo `QueryExpression` trên thực thể `bsd_interestratemaster`.
2.  Thực hiện liên kết (`AddLink`) với `bsd_paymentscheme`.
3.  Lọc liên kết theo ID của Payment Scheme (`ps.Id`).
4.  Thực thi truy vấn.
5.  Nếu tìm thấy bản ghi:
    *   Kiểm tra `bsd_intereststartdatetype`. Nếu giá trị là `100000001` (tương ứng với loại Grace Day), trả về giá trị của trường `bsd_gracedays`.
    *   Nếu không phải loại Grace Day, trả về 0.
6.  Nếu không tìm thấy bản ghi, trả về -1.