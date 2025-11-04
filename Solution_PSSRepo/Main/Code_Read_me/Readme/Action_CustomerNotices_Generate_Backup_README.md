# Phân tích mã nguồn: Action_CustomerNotices_Generate_Backup.cs

## Tổng quan

Tệp mã nguồn `Action_CustomerNotices_Generate_Backup.cs` chứa một Plugin Dynamics 365 (C#) được thiết kế để thực thi một hành động tùy chỉnh (Custom Action). Chức năng chính của Plugin này là tự động tạo ra các Thông báo Thanh toán (`bsd_customernotices`) và sau đó là các Thông báo Cảnh báo (`bsd_warningnotices`) dựa trên các đợt thanh toán (`bsd_paymentschemedetail`) của các Đơn hàng/Hợp đồng (Option Entry - `salesorder`) đã được lọc.

Plugin này sử dụng các tham số đầu vào để lọc các Option Entry theo Dự án, Block, Tầng và Đơn vị, sau đó kiểm tra các điều kiện về ngày đến hạn thanh toán so với ngày hiện tại và các quy tắc được định nghĩa trong Bảng Kế hoạch Thanh toán (`bsd_paymentscheme`) để quyết định việc tạo thông báo.

## Chi tiết các Hàm (Functions/Methods)

### IPlugin.Execute(IServiceProvider serviceProvider)

#### Chức năng tổng quát:
Đây là điểm vào chính của Plugin. Hàm này chịu trách nhiệm khởi tạo các dịch vụ CRM, lấy các tham số đầu vào, lọc các Option Entry phù hợp, và sau đó lặp qua các đợt thanh toán để tạo các Thông báo Thanh toán (Payment Notices) và Thông báo Cảnh báo (Warning Notices) tương ứng.

#### Logic nghiệp vụ chi tiết:
1.  **Khởi tạo Dịch vụ:** Lấy các dịch vụ cần thiết: `IPluginExecutionContext` (context), `IOrganizationServiceFactory` (factory), `IOrganizationService` (service), và `ITracingService` (traceService).
2.  **Lấy Tham số Đầu vào:** Truy xuất các tham số đầu vào từ Context: `Project` (pro), `Block` (blo), `Floor` (flo), `Units` (units), và `Date` (date).
3.  **Lọc Option Entry:** Gọi hàm `findOptionEntry` để truy vấn các Option Entry (`salesorder`) dựa trên các tham số lọc đã cung cấp.
4.  **Kiểm tra Giới hạn:** Nếu số lượng Option Entry tìm thấy lớn hơn hoặc bằng 5000, một ngoại lệ (`InvalidPluginExecutionException`) sẽ được ném ra, yêu cầu người dùng thêm bộ lọc.
5.  **Tạo Payment Notices:**
    *   Khởi tạo biến đếm `dem` và `ArrayList listid` để lưu trữ ID của các Payment Notices được tạo.
    *   Lặp qua từng Option Entry (OE) trong tập hợp `l_OptionEntry`.
    *   Tìm kiếm Kế hoạch Thanh toán (Payment Scheme - PS) liên quan bằng cách gọi `findPaymentScheme`.
    *   Nếu tìm thấy PS và nó chứa giá trị `bsd_paymentnoticesdate` (số ngày cần thông báo trước), tiến hành:
        *   Lấy giá trị `PN_Date` (số ngày thông báo trước).
        *   Gọi `findPaymentSchemeDetail` để lấy tất cả các đợt thanh toán (PSD) liên quan đến OE.
        *   Lặp qua từng đợt thanh toán (PSD):
            *   Truy xuất chi tiết đầy đủ của PSD.
            *   Tính toán số tiền và phần trăm cho khách hàng (`bsd_amountforcustomer`) và ngân hàng (`bsd_amountforbank`).
            *   Tính toán số ngày còn lại (`nday`) giữa ngày đến hạn (`bsd_duedate`) và ngày hiện tại (có cộng thêm 7 giờ để điều chỉnh múi giờ).
            *   **Điều kiện tạo Notice:** Nếu `nday` nằm trong khoảng từ 0 đến `PN_Date` (tức là sắp đến hạn hoặc đúng hạn, trong phạm vi thông báo).
            *   Kiểm tra xem Payment Notice đã được tạo cho đợt thanh toán này chưa bằng cách gọi `findCustomerNoticesByIntallment`.
            *   Nếu chưa có, tạo một thực thể `bsd_customernotices` mới, điền các trường như Tên, Chủ đề, Dự án, Khách hàng, Ngày (sử dụng ngày hiện tại được chuyển đổi múi giờ hoặc ngày từ tham số `date`), Option Entry, và Chi tiết Kế hoạch Thanh toán.
            *   Lưu ID của Payment Notice mới vào `listid` và tăng biến đếm `dem`.
            *   Cập nhật thực thể đợt thanh toán (PSD) bằng cách đặt trường `bsd_paymentnotices` thành `true`.
6.  **Kết quả và Warning Notices:**
    *   Đặt tham số đầu ra `ReturnId` là số lượng Payment Notices đã tạo.
    *   Gọi hàm `generateWarningNoticesByPaymentNotices` để tiếp tục tạo các Thông báo Cảnh báo.

### CompareDate(DateTime date1, DateTime date2)

#### Chức năng tổng quát:
Hàm này tính toán sự khác biệt về số ngày giữa hai ngày, sau khi chuyển đổi chúng sang múi giờ cục bộ của máy chủ.

#### Logic nghiệp vụ chi tiết:
1.  Lấy ID múi giờ cục bộ hiện tại (`currentTimerZone`).
2.  Chuyển đổi cả `date1` và `date2` sang múi giờ cục bộ.
3.  Tính toán sự khác biệt về số ngày giữa hai ngày (chỉ tính phần ngày, bỏ qua thời gian) và trả về dưới dạng `decimal`.

### RetrieveMultiRecord(IOrganizationService crmservices, string entity, ColumnSet column, string condition, object value)

#### Chức năng tổng quát:
Hàm tiện ích chung để truy vấn nhiều bản ghi từ CRM dựa trên một điều kiện đơn giản.

#### Logic nghiệp vụ chi tiết:
1.  Tạo một `QueryExpression` cho thực thể được chỉ định.
2.  Đặt `ColumnSet` (các cột cần truy xuất).
3.  Tạo `FilterExpression` và thêm một `ConditionExpression` duy nhất: trường `condition` bằng giá trị `value`.
4.  Thực hiện truy vấn bằng `service.RetrieveMultiple(q)` và trả về `EntityCollection`.

### findPaymentSchemeDetail(IOrganizationService crmservices, Entity oe)

#### Chức năng tổng quát:
Truy vấn các chi tiết đợt thanh toán (`bsd_paymentschemedetail`) liên quan đến một Option Entry cụ thể.

#### Logic nghiệp vụ chi tiết:
1.  Sử dụng FetchXML để truy vấn thực thể `bsd_paymentschemedetail`.
2.  Lọc theo hai điều kiện:
    *   `bsd_optionentry` bằng ID của Option Entry (`oe.Id`).
    *   `statuscode` bằng `100000000` (trạng thái hoạt động/đang chờ).
    *   `bsd_duedate` phải khác rỗng (`not-null`).
3.  Sắp xếp kết quả theo `bsd_duedate` giảm dần.
4.  Thực hiện truy vấn và trả về `EntityCollection`.

### findCustomerNotices(IOrganizationService crmservices)

#### Chức năng tổng quát:
Truy vấn các Thông báo Khách hàng (`bsd_customernotices`) được tạo trong ngày hôm nay. (Hàm này bị comment-out trong logic chính, nhưng vẫn tồn tại).

#### Logic nghiệp vụ chi tiết:
1.  Sử dụng FetchXML để truy vấn thực thể `bsd_customernotices`.
2.  Giới hạn số lượng kết quả là 1 (`count='1'`).
3.  Lọc theo điều kiện `createdon` (Ngày tạo) là `today`.
4.  Thực hiện truy vấn và trả về `EntityCollection`.

### findCustomerNoticesByIntallment(IOrganizationService crmservices, EntityReference ins)

#### Chức năng tổng quát:
Kiểm tra xem đã có Thông báo Khách hàng (`bsd_customernotices`) nào được tạo cho một chi tiết đợt thanh toán (`bsd_paymentschemedetail`) cụ thể hay chưa.

#### Logic nghiệp vụ chi tiết:
1.  Sử dụng FetchXML để truy vấn thực thể `bsd_customernotices`.
2.  Giới hạn số lượng kết quả là 1 (`count='1'`).
3.  Lọc theo điều kiện `bsd_paymentschemedetail` bằng ID của đợt thanh toán (`ins.Id`).
4.  Thực hiện truy vấn và trả về `EntityCollection`.

### findPaymentScheme(IOrganizationService crmservices, EntityReference ps)

#### Chức năng tổng quát:
Truy vấn Kế hoạch Thanh toán (`bsd_paymentscheme`) cụ thể và đảm bảo rằng trường `bsd_paymentnoticesdate` (số ngày thông báo trước) đã được điền.

#### Logic nghiệp vụ chi tiết:
1.  Sử dụng FetchXML để truy vấn thực thể `bsd_paymentscheme`.
2.  Giới hạn số lượng kết quả là 1 (`count='1'`).
3.  Lọc theo hai điều kiện:
    *   `bsd_paymentschemeid` bằng ID của Kế hoạch Thanh toán (`ps.Id`).
    *   `bsd_paymentnoticesdate` phải khác rỗng (`not-null`).
4.  Thực hiện truy vấn và trả về `EntityCollection`.

### findOptionEntry(IOrganizationService crmservices, string project, string block, string floor, string units)

#### Chức năng tổng quát:
Truy vấn các Option Entry (`salesorder`) dựa trên các bộ lọc tùy chọn về Dự án, Block, Tầng và Đơn vị.

#### Logic nghiệp vụ chi tiết:
1.  Sử dụng `StringBuilder` để xây dựng FetchXML một cách linh hoạt.
2.  **Điều kiện cơ bản:** Lọc các Option Entry có:
    *   `statuscode` khác `100000006` (Đã hủy).
    *   `bsd_paymentscheme` khác rỗng.
    *   `customerid` khác rỗng.
    *   `totalamount` lớn hơn 0.
3.  **Điều kiện nâng cao (Liên kết động):** Nếu tham số `project` được cung cấp:
    *   Thêm một `link-entity` tới thực thể `product` (Đơn vị) thông qua trường `bsd_unitnumber`.
    *   Thêm điều kiện lọc `bsd_projectcode` bằng giá trị `project`.
    *   Nếu các tham số `block`, `floor`, hoặc `units` được cung cấp, chúng sẽ được thêm vào bộ lọc của `link-entity` này.
4.  Thực hiện truy vấn bằng FetchXML đã xây dựng và trả về `EntityCollection`.

### RetrieveLocalTimeFromUTCTime(DateTime utcTime, IOrganizationService service)

#### Chức năng tổng quát:
Chuyển đổi thời gian UTC (Universal Time Coordinated) sang thời gian cục bộ của người dùng hiện tại trong CRM.

#### Logic nghiệp vụ chi tiết:
1.  Gọi `RetrieveCurrentUsersSettings` để lấy mã múi giờ (`timezonecode`) của người dùng hiện tại.
2.  Nếu không tìm thấy mã múi giờ, ném ra ngoại lệ.
3.  Tạo một `LocalTimeFromUtcTimeRequest` với mã múi giờ và thời gian UTC đầu vào.
4.  Thực thi yêu cầu bằng `service.Execute(request)`.
5.  Trả về thời gian cục bộ từ phản hồi.

### RetrieveCurrentUsersSettings(IOrganizationService service)

#### Chức năng tổng quát:
Truy xuất cài đặt của người dùng hiện tại, cụ thể là mã múi giờ (`timezonecode`).

#### Logic nghiệp vụ chi tiết:
1.  Tạo một `QueryExpression` cho thực thể `usersettings`.
2.  Chỉ định các cột cần lấy (`localeid`, `timezonecode`).
3.  Sử dụng `ConditionExpression` với `ConditionOperator.EqualUserId` để đảm bảo chỉ truy vấn cài đặt của người dùng đang thực thi Plugin.
4.  Truy xuất và trả về giá trị `timezonecode` (dưới dạng `int?`).

### generateWarningNoticesByPaymentNotices(ArrayList listid, string date)

#### Chức năng tổng quát:
Tạo các Thông báo Cảnh báo (`bsd_warningnotices`) dựa trên các Payment Notices vừa được tạo, kiểm tra xem các đợt thanh toán đã quá hạn và đạt đến ngưỡng cảnh báo nào.

#### Logic nghiệp vụ chi tiết:
1.  Lặp qua danh sách ID của các Payment Notices (`listid`).
2.  Đối với mỗi Payment Notice:
    *   Truy xuất Payment Notice, Option Entry (OE) và Payment Scheme (PS) liên quan.
    *   Kiểm tra xem PS có định nghĩa bất kỳ ngày cảnh báo nào (từ `bsd_warningnotices1date` đến `bsd_warningnotices4date`) không.
    *   Lấy tất cả các đợt thanh toán (Installment/PSD) thuộc OE bằng `getAllInstallmentByOptionEntry`.
    *   Lặp qua từng đợt thanh toán (PSD):
        *   Tính toán số ngày quá hạn (`nday`) bằng cách lấy ngày hiện tại trừ đi ngày đến hạn (có điều chỉnh múi giờ +7). Chỉ xử lý nếu `nday > 0` (đã quá hạn).
        *   **Kiểm tra Warning Notices đã tồn tại:** Gọi `findWarningNotices` để xem có WN nào cho đợt này chưa.
        *   **Trường hợp 1: Đã Generate WN:**
            *   Lấy `numberofWarning` hiện tại.
            *   Nếu `numberofWarning` nằm giữa 1 và 3, xác định trường ngày cảnh báo tiếp theo (ví dụ: nếu là WN1, kiểm tra `bsd_warningnotices2date`).
            *   Nếu PS chứa ngày cảnh báo tiếp theo VÀ số ngày quá hạn (`nday`) lớn hơn hoặc bằng ngưỡng ngày cảnh báo đó, tiến hành tạo WN cấp độ tiếp theo (ví dụ: WN2).
            *   Trước khi tạo, kiểm tra lại bằng `findWarningNoticesByNumberOfWarning` để tránh trùng lặp.
            *   Tạo thực thể `bsd_warningnotices`, điền các thông tin chi tiết (Tên, Chủ đề, Khách hàng, Dự án, Số lượng cảnh báo, Số tiền còn lại (`bsd_balance`), Ngày đến hạn, Ngày tạo).
            *   Tính toán `bsd_estimateduedate` bằng cách gọi `findGraceDays` để lấy số ngày ân hạn.
            *   Cập nhật đợt thanh toán (PSD) bằng cách đặt trường `bsd_warningnoticesX` tương ứng thành `true`.
        *   **Trường hợp 2: Chưa Generate WN:**
            *   Lấy ngưỡng ngày cảnh báo 1 (`bsd_warningnotices1date`).
            *   Nếu `nday` lớn hơn hoặc bằng ngưỡng này, tạo WN cấp độ 1.
            *   Thực hiện các bước tạo WN tương tự như trên (tính ngày ân hạn, cập nhật trường `bsd_warningnotices1` trên PSD).

### getAllInstallmentByOptionEntry(Entity enOptionEntry)

#### Chức năng tổng quát:
Truy vấn tất cả các chi tiết đợt thanh toán (`bsd_paymentschemedetail`) thuộc một Option Entry cụ thể, với điều kiện là chúng đang hoạt động và có ngày đến hạn.

#### Logic nghiệp vụ chi tiết:
1.  Tạo `QueryExpression` cho thực thể `bsd_paymentschemedetail`.
2.  Lọc theo ID của Option Entry, `statuscode` bằng `100000000`, và `bsd_duedate` khác rỗng.
3.  Sắp xếp kết quả theo `bsd_duedate` tăng dần.
4.  Thực hiện truy vấn và trả về `EntityCollection`.

### findWarningNotices(Entity enInstallment)

#### Chức năng tổng quát:
Tìm kiếm các Thông báo Cảnh báo (`bsd_warningnotices`) đã được tạo cho một chi tiết đợt thanh toán cụ thể.

#### Logic nghiệp vụ chi tiết:
1.  Tạo `QueryExpression` cho thực thể `bsd_warningnotices`.
2.  Lọc theo trường `bsd_paymentschemedeitail` bằng ID của đợt thanh toán.
3.  Sắp xếp kết quả theo `bsd_numberofwarning` giảm dần (để lấy cảnh báo cấp độ cao nhất nếu có nhiều).
4.  Thực hiện truy vấn và trả về `EntityCollection`.

### findWarningNoticesByNumberOfWarning(Entity paymentDet, EntityReference enfOptionEntry)

#### Chức năng tổng quát:
Tìm kiếm các Thông báo Cảnh báo dựa trên chi tiết thanh toán và Option Entry. (Hàm này có vẻ dư thừa so với `findWarningNotices` nhưng được sử dụng để kiểm tra sự tồn tại trước khi tạo WN cấp độ tiếp theo).

#### Logic nghiệp vụ chi tiết:
1.  Tạo `QueryExpression` cho thực thể `bsd_warningnotices`.
2.  Lọc theo `bsd_paymentschemedeitail` và `bsd_optionentry`.
3.  Sắp xếp theo `bsd_numberofwarning` giảm dần.
4.  Thực hiện truy vấn và trả về `EntityCollection`.

### findGraceDays(EntityReference ps)

#### Chức năng tổng quát:
Tìm kiếm số ngày ân hạn (`bsd_gracedays`) liên quan đến một Kế hoạch Thanh toán (`bsd_paymentscheme`) thông qua thực thể `bsd_interestratemaster`.

#### Logic nghiệp vụ chi tiết:
1.  Tạo `QueryExpression` cho thực thể `bsd_interestratemaster`.
2.  Thêm một `LinkEntity` tới `bsd_paymentscheme`.
3.  Lọc liên kết theo ID của Kế hoạch Thanh toán (`ps.Id`).
4.  Thực hiện truy vấn.
5.  Nếu tìm thấy kết quả:
    *   Kiểm tra trường `bsd_intereststartdatetype`. Nếu loại này là `100000001` (tức là loại "graceday"), trả về giá trị của trường `bsd_gracedays`.
    *   Nếu loại khác, trả về 0.
6.  Nếu không tìm thấy kết quả, trả về -1.