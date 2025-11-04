# Phân tích mã nguồn: Action_WarningNotices_GenerateWarningNotices.cs

## Tổng quan

Tệp mã nguồn `Action_WarningNotices_GenerateWarningNotices.cs` chứa một Plugin Dynamics 365 (hoặc Custom Action) được thiết kế để tự động tạo các Thông báo Cảnh báo (Warning Notices - WN) cho các chi tiết lịch thanh toán (`bsd_paymentschemedetail`) đã quá hạn.

Plugin này hoạt động bằng cách kiểm tra từng đợt thanh toán (installment) liên quan đến một Đơn hàng/Hợp đồng (Option Entry - `salesorder`). Nếu đợt thanh toán đã quá hạn và đáp ứng các điều kiện về số ngày trễ theo quy định trong Lịch thanh toán (`bsd_paymentscheme`), hệ thống sẽ tạo ra Thông báo Cảnh báo tiếp theo (WN1, WN2, WN3, hoặc WN4) và cập nhật trạng thái của đợt thanh toán đó.

Mã nguồn sử dụng các truy vấn FetchXML và QueryExpression để tương tác với cơ sở dữ liệu Dynamics 365.

## Chi tiết các Hàm (Functions/Methods)

### IPlugin.Execute(IServiceProvider serviceProvider)

#### Chức năng tổng quát:

Đây là điểm vào chính của Plugin. Hàm này chịu trách nhiệm khởi tạo dịch vụ CRM, lấy các tham số đầu vào, xác định các đợt thanh toán quá hạn, và thực hiện logic nghiệp vụ để tạo các Thông báo Cảnh báo mới.

#### Logic nghiệp vụ chi tiết:

1.  **Khởi tạo Context:** Thiết lập các đối tượng `IPluginExecutionContext`, `IOrganizationServiceFactory`, `IOrganizationService`, và `ITracingService` để tương tác với môi trường CRM và ghi lại dấu vết.
2.  **Lấy Tham số Đầu vào:**
    *   Lấy `optionEntryId` (ID của `salesorder`).
    *   Lấy `Date` (Ngày tạo WN, nếu được cung cấp).
    *   Lấy `Owner` (Chủ sở hữu của WN, được làm sạch khỏi dấu ngoặc nhọn `{}`).
    *   Lấy `GenWNId` (ID của bản ghi Generate Warning Notices, được làm sạch khỏi dấu ngoặc nhọn `{}`).
3.  **Truy xuất Dữ liệu Chính:**
    *   Truy xuất bản ghi Option Entry (`salesorder` - OE) dựa trên `optionEntryId`, lấy các trường quan trọng như `bsd_paymentscheme`, `customerid`, `bsd_project`, và `bsd_unitnumber`.
    *   Truy xuất bản ghi Payment Scheme (`bsd_paymentscheme` - PS) liên quan đến OE bằng hàm `findPaymentScheme()`.
4.  **Kiểm tra Điều kiện Lịch thanh toán:** Kiểm tra xem bản ghi PS có chứa ít nhất một trong các trường ngày cảnh báo (`bsd_warningnotices1date` đến `bsd_warningnotices4date`) hay không. Nếu không có, logic tạo WN sẽ bị bỏ qua.
5.  **Lấy Chi tiết Thanh toán:** Gọi hàm `findPaymentSchemeDetail()` để lấy tất cả các đợt thanh toán (`bsd_paymentschemedetail` - PSD) liên quan đến OE có trạng thái hoạt động (statuscode = 100000000) và có ngày đến hạn.
6.  **Vòng lặp Xử lý Từng Đợt Thanh toán (PSD):**
    *   Tính toán số ngày quá hạn (`nday`): `nday` là số ngày chênh lệch giữa Ngày hiện tại (cộng 7 giờ để đồng bộ múi giờ) và Ngày đến hạn của PSD (cộng 7 giờ).
    *   **Điều kiện Quá hạn:** Chỉ xử lý nếu `nday > 0` (đã quá hạn).
    *   **Tìm WN Hiện có:** Gọi `findWarningNotices()` để tìm các WN đang hoạt động đã được tạo cho PSD này.
    *   **Logic Tạo WN (Hai trường hợp):**

        *   **Trường hợp 1: Đã Generate WN trước đó (`L_warning.Entities.Count > 0`):**
            *   Lấy số lần cảnh báo cao nhất đã tạo (`numberofWarning`).
            *   Nếu `1 <= numberofWarning < 4` (tức là có thể tạo WN tiếp theo):
                *   Xác định tên trường ngày cảnh báo tiếp theo (ví dụ: nếu `numberofWarning` là 1, kiểm tra `bsd_warningnotices2date`).
                *   Kiểm tra xem PS có chứa trường ngày cảnh báo tiếp theo đó không VÀ số ngày quá hạn (`nday`) có lớn hơn hoặc bằng số ngày trễ quy định trong PS không.
                *   Nếu đủ điều kiện, kiểm tra xem WN tiếp theo đó đã được tạo chưa bằng `findWarningNoticesByNumberOfWarning()`.
                *   Nếu WN tiếp theo chưa tồn tại, tiến hành tạo bản ghi `bsd_warningnotices` mới:
                    *   Thiết lập các trường liên kết (Option Entry, Khách hàng, Dự án, Đơn vị, Chi tiết Lịch thanh toán).
                    *   Thiết lập `bsd_numberofwarning` là `numberofWarning + 1`.
                    *   Thiết lập `bsd_amount` (số dư của PSD).
                    *   Thiết lập `bsd_date` (Ngày tạo WN).
                    *   Thiết lập `ownerid` và `bsd_generatewarningnotices` nếu các tham số đầu vào được cung cấp.
                    *   **Tính toán Deadline:** Tính `bsd_deadlinewn1` (Ngày đến hạn + Grace Days) và `bsd_deadlinewn2` (Ngày đến hạn + 60 ngày). Grace Days được lấy từ PSD hoặc từ `bsd_interestratemaster` liên quan đến PS.
                    *   **Xử lý Order Number:** Thiết lập trường `bsd_odernumber_e` (ví dụ: "1st", "2nd", "3rd", "Nth").
                    *   Tạo bản ghi WN mới.
                    *   Cập nhật bản ghi PSD gốc: Đặt cờ `bsd_warningnoticesN` tương ứng là `true`, và cập nhật `bsd_warningdateN` và `bsd_w_noticesnumberN`.

        *   **Trường hợp 2: Chưa Generate WN nào (`else` - Chua Generate WN):**
            *   Lấy số ngày trễ cần thiết cho WN1 (`bsd_warningnotices1date`).
            *   Nếu số ngày trễ hợp lệ (`>= 0`) VÀ số ngày quá hạn (`nday`) đáp ứng điều kiện:
                *   Tiến hành tạo WN1 (tương tự như Trường hợp 1, nhưng `bsd_numberofwarning` là 1).
                *   Cập nhật bản ghi PSD gốc, đặt cờ `bsd_warningnotices1` là `true` và cập nhật các trường liên quan.

7.  **Kết quả:** Tăng biến đếm `dem` mỗi khi một WN được tạo thành công.
8.  **Trả về:** Đặt số lượng WN đã tạo (`dem`) vào tham số đầu ra `returnN`.

### RetrieveMultiRecord(IOrganizationService crmservices, string entity, ColumnSet column, string condition, object value)

#### Chức năng tổng quát:

Hàm tiện ích chung để truy xuất nhiều bản ghi từ CRM bằng cách sử dụng QueryExpression, lọc theo một điều kiện duy nhất.

#### Logic nghiệp vụ chi tiết:

1.  Tạo một đối tượng `QueryExpression` cho thực thể được chỉ định (`entity`).
2.  Thiết lập các cột cần truy xuất (`column`).
3.  Tạo một `FilterExpression` và thêm một điều kiện lọc duy nhất (`condition`, `ConditionOperator.Equal`, `value`).
4.  Thực hiện truy vấn và trả về `EntityCollection`.

### findPaymentSchemeDetail(IOrganizationService crmservices, Entity oe)

#### Chức năng tổng quát:

Truy xuất tất cả các chi tiết lịch thanh toán (`bsd_paymentschemedetail`) liên quan đến một Option Entry (salesorder) cụ thể.

#### Logic nghiệp vụ chi tiết:

1.  Sử dụng FetchXML để truy vấn thực thể `bsd_paymentschemedetail`.
2.  Lọc các bản ghi theo:
    *   Liên kết với ID của Option Entry (`bsd_optionentry` = `{0}`).
    *   Trạng thái hoạt động (`statuscode` = 100000000).
    *   Ngày đến hạn không rỗng (`bsd_duedate` operator='not-null').
3.  Sắp xếp kết quả theo `bsd_duedate` tăng dần.
4.  Trả về `EntityCollection` chứa các chi tiết thanh toán.

### findunits(IOrganizationService crmservices, Entity oe)

#### Chức năng tổng quát:

Truy xuất các đơn vị (`product`) liên quan đến một Option Entry (salesorder) cụ thể.

#### Logic nghiệp vụ chi tiết:

1.  Sử dụng FetchXML để truy vấn thực thể `product`.
2.  Lọc các đơn vị có `bsd_estimatehandoverdate` không rỗng.
3.  Liên kết với `salesorderdetail` để đảm bảo đơn vị đó thuộc về Option Entry có ID là `{0}`.
4.  Trả về `EntityCollection` (Hàm này không được sử dụng trong logic `Execute` chính).

### findPaymentScheme(IOrganizationService crmservices, EntityReference ps)

#### Chức năng tổng quát:

Truy xuất bản ghi Lịch thanh toán (`bsd_paymentscheme`) dựa trên EntityReference, tập trung vào các trường ngày cảnh báo.

#### Logic nghiệp vụ chi tiết:

1.  Thực hiện lệnh `service.Retrieve` trên thực thể `bsd_paymentscheme` bằng ID được cung cấp.
2.  Chỉ truy xuất các cột cần thiết: `bsd_paymentschemeid` và 4 trường ngày cảnh báo (`bsd_warningnotices1date` đến `bsd_warningnotices4date`).
3.  Trả về bản ghi `Entity` của Payment Scheme.

### findOptionEntry(IOrganizationService crmservices, string project, string block, string floor, string units)

#### Chức năng tổng quát:

Truy xuất các Option Entry (`salesorder`) dựa trên các tiêu chí lọc về Dự án, Block, Tầng và Đơn vị.

#### Logic nghiệp vụ chi tiết:

1.  Xây dựng chuỗi điều kiện lọc (`condition`) dựa trên các tham số đầu vào (project, block, floor, units).
2.  Sử dụng FetchXML để truy vấn thực thể `salesorder`.
3.  Lọc các Option Entry theo các điều kiện cố định:
    *   `statuscode` nằm trong danh sách các giá trị hoạt động/đang xử lý.
    *   `bsd_paymentscheme` không rỗng.
    *   `customerid` không rỗng.
    *   `totalamount` lớn hơn 0.
    *   `bsd_terminationletter` khác 1 (chưa bị hủy).
4.  Liên kết với thực thể `product` (đơn vị) để áp dụng các điều kiện lọc về vị trí (project, block, floor, units).
5.  Trả về `EntityCollection` (Hàm này không được sử dụng trong logic `Execute` chính, vì `optionEntryId` được truyền trực tiếp).

### findWarningNotices(IOrganizationService crmservices, Entity paymentDet)

#### Chức năng tổng quát:

Tìm kiếm tất cả các Thông báo Cảnh báo (`bsd_warningnotices`) đang hoạt động liên quan đến một chi tiết thanh toán (`bsd_paymentschemedetail`) cụ thể.

#### Logic nghiệp vụ chi tiết:

1.  Sử dụng FetchXML để truy vấn thực thể `bsd_warningnotices`.
2.  Lọc theo:
    *   Liên kết với ID của chi tiết thanh toán (`bsd_paymentschemedeitail` = `{0}`).
    *   Trạng thái hoạt động (`statecode` = 0).
3.  Sắp xếp kết quả theo `bsd_numberofwarning` giảm dần (để lấy WN có số lần cao nhất).
4.  Trả về `EntityCollection`.

### findWarningNoticesByNumberOfWarning(IOrganizationService crmservices, Entity paymentDet, int num)

#### Chức năng tổng quát:

Tìm kiếm một Thông báo Cảnh báo cụ thể (dựa trên số lần cảnh báo) liên quan đến một chi tiết thanh toán.

#### Logic nghiệp vụ chi tiết:

1.  Sử dụng FetchXML để truy vấn thực thể `bsd_warningnotices`.
2.  Lọc theo:
    *   Liên kết với ID của chi tiết thanh toán (`bsd_paymentschemedeitail` = `{0}`).
    *   Số lần cảnh báo cụ thể (`bsd_numberofwarning` = `{1}`).
    *   Trạng thái hoạt động (`statecode` = 0).
3.  Trả về `EntityCollection`.

### findWarningNotices_Units(IOrganizationService crmservices, Entity units)

#### Chức năng tổng quát:

Tìm kiếm tất cả các Thông báo Cảnh báo đang hoạt động liên quan đến một Đơn vị (`product`) cụ thể.

#### Logic nghiệp vụ chi tiết:

1.  Sử dụng FetchXML để truy vấn thực thể `bsd_warningnotices`.
2.  Lọc theo liên kết với ID của Đơn vị (`bsd_units` = `{0}`) và trạng thái hoạt động.
3.  Trả về `EntityCollection` (Hàm này không được sử dụng trong logic `Execute` chính).

### findWarningNotices_Units_ByNumberOfWarning(IOrganizationService crmservices, Entity units, int num)

#### Chức năng tổng quát:

Tìm kiếm một Thông báo Cảnh báo cụ thể (dựa trên số lần cảnh báo) liên quan đến một Đơn vị.

#### Logic nghiệp vụ chi tiết:

1.  Sử dụng FetchXML để truy vấn thực thể `bsd_warningnotices`.
2.  Lọc theo liên kết với ID của Đơn vị (`bsd_units` = `{0}`), số lần cảnh báo cụ thể (`bsd_numberofwarning` = `{1}`) và trạng thái hoạt động.
3.  Trả về `EntityCollection` (Hàm này không được sử dụng trong logic `Execute` chính).

### findWarningNotices_TODAY(IOrganizationService crmservices)

#### Chức năng tổng quát:

Tìm kiếm tất cả các Thông báo Cảnh báo đang hoạt động được tạo trong ngày hôm nay.

#### Logic nghiệp vụ chi tiết:

1.  Sử dụng FetchXML để truy vấn thực thể `bsd_warningnotices`.
2.  Lọc theo điều kiện `createdon` operator='today' và trạng thái hoạt động.
3.  Trả về `EntityCollection` (Hàm này không được sử dụng trong logic `Execute` chính).

### findGraceDays(IOrganizationService crmservices, EntityReference ps)

#### Chức năng tổng quát:

Tìm kiếm số ngày ân hạn (`bsd_gracedays`) liên quan đến một Lịch thanh toán (`bsd_paymentscheme`) thông qua bản ghi Lãi suất Chủ đạo (`bsd_interestratemaster`).

#### Logic nghiệp vụ chi tiết:

1.  Sử dụng FetchXML để truy vấn thực thể `bsd_interestratemaster`.
2.  Liên kết ngược với `bsd_paymentscheme` để lọc theo ID của Lịch thanh toán được cung cấp (`ps.Id`).
3.  Chỉ truy xuất 1 bản ghi (`count='1'`).
4.  **Xử lý kết quả:**
    *   Nếu tìm thấy bản ghi: Kiểm tra trường `bsd_intereststartdatetype`.
        *   Nếu loại ngày bắt đầu là Grace Day (giá trị 100000001), trả về giá trị của `bsd_gracedays`.
        *   Nếu không phải, trả về 0.
    *   Nếu không tìm thấy bản ghi, trả về -1.