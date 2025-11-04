# Phân tích mã nguồn: Action_WarningNotices_GenerateWarningNotices_BackUp.cs

## Tổng quan

Tệp mã nguồn `Action_WarningNotices_GenerateWarningNotices_BackUp.cs` là một Custom Action hoặc Plugin được triển khai trong môi trường Microsoft Dynamics 365/Power Platform (sử dụng giao diện `IPlugin`).

Chức năng chính của mã này là tự động kiểm tra các chi tiết thanh toán (installments) đã quá hạn trong các Hợp đồng/Đơn đặt hàng (Option Entry - `salesorder`) và tạo ra các Thông báo Cảnh báo (Warning Notices - `bsd_warningnotices`) tương ứng. Logic nghiệp vụ xác định mức độ cảnh báo (WN1, WN2, WN3, WN4) dựa trên số ngày quá hạn so với các quy tắc được định nghĩa trong Bảng Kế hoạch Thanh toán (`bsd_paymentscheme`).

Action này nhận các tham số lọc đầu vào như Dự án, Block, Tầng và Đơn vị để giới hạn phạm vi xử lý.

---

## Chi tiết các Hàm (Functions/Methods)

### IPlugin.Execute(IServiceProvider serviceProvider)

#### Chức năng tổng quát:
Đây là điểm vào chính của Plugin/Action. Hàm này chịu trách nhiệm khởi tạo các dịch vụ CRM, thu thập các tham số đầu vào, tìm kiếm các khoản thanh toán quá hạn, và tạo hoặc nâng cấp các Thông báo Cảnh báo (Warning Notices) dựa trên logic quá hạn.

#### Logic nghiệp vụ chi tiết:
1.  **Khởi tạo Dịch vụ:** Khởi tạo `IPluginExecutionContext`, `IOrganizationServiceFactory`, `IOrganizationService` (sử dụng ID người dùng của Context), và `ITracingService`.
2.  **Thu thập Tham số Đầu vào:** Lấy các tham số lọc từ `context.InputParameters`: `Project` (`pro`), `Block` (`blo`), `Floor` (`flo`), `Units` (`units`), và `Date` (`date`).
3.  **Tìm kiếm Option Entry:** Gọi hàm `findOptionEntry` để lấy danh sách các Option Entry (`salesorder`) phù hợp với các tiêu chí lọc đã nhập.
4.  **Vòng lặp Chính (Duyệt qua Option Entry):**
    *   Khởi tạo biến đếm `dem = 0` (đếm số WN được tạo).
    *   Duyệt qua từng Option Entry (`OE`) trong danh sách tìm được.
    *   **Tìm Kế hoạch Thanh toán:** Gọi `findPaymentScheme` để lấy Kế hoạch Thanh toán (`PS`) liên quan đến `OE`.
    *   **Kiểm tra Điều kiện WN:** Kiểm tra xem `PS` có chứa bất kỳ trường ngày cảnh báo nào (từ `bsd_warningnotices1date` đến `bsd_warningnotices4date`) không.
    *   **Duyệt qua Chi tiết Thanh toán (Installments):**
        *   Gọi `findPaymentSchemeDetail` để lấy danh sách các khoản thanh toán chi tiết (`PSD`) chưa hoàn thành của `OE`.
        *   Duyệt qua từng `PSD`:
            *   **Tính số ngày quá hạn:** Tính toán `nday` là số ngày chênh lệch giữa ngày hiện tại (cộng thêm 7 giờ, có thể là điều chỉnh múi giờ) và ngày đến hạn (`bsd_duedate`) (cộng thêm 7 giờ).
            *   **Kiểm tra Quá hạn:** Nếu `nday > 0` (khoản thanh toán đã quá hạn):
                *   **Tìm WN Hiện có:** Gọi `findWarningNotices` để tìm Thông báo Cảnh báo gần nhất đã được tạo cho `PSD` này.

                *   **Logic Nâng cấp WN (Đã Generate WN):**
                    *   Nếu `L_warning.Entities.Count > 0` (đã có WN):
                        *   Lấy `numberofWarning` (mức cảnh báo hiện tại).
                        *   Nếu `numberofWarning` lớn hơn 0 và nhỏ hơn 4 (tức là WN1, WN2, hoặc WN3):
                            *   Xác định tên trường ngày cảnh báo tiếp theo (`warningdate`, ví dụ: nếu hiện tại là 1, tìm `bsd_warningnotices2date`).
                            *   Kiểm tra xem `PS` có chứa `warningdate` đó không VÀ số ngày quá hạn (`nday`) có lớn hơn hoặc bằng giá trị ngày quy định trong `PS[warningdate]` không.
                            *   Nếu đủ điều kiện, gọi `findWarningNoticesByNumberOfWarning` để đảm bảo WN cấp độ tiếp theo chưa được tạo.
                            *   Nếu chưa tạo, tiến hành tạo Entity `bsd_warningnotices` mới với `bsd_numberofwarning = numberofWarning + 1`.
                            *   Thiết lập các trường dữ liệu (tên, chủ đề, khách hàng, dự án, số tiền, ngày tạo, ngày đến hạn, ngày đến hạn ước tính có tính Grace Day).
                            *   Gọi `findGraceDays` để tính ngày ân hạn và cập nhật `bsd_estimateduedate`.
                            *   Tạo bản ghi WN mới và tăng biến đếm `dem`.
                            *   Cập nhật bản ghi `PSD` gốc để đánh dấu WN mới đã được tạo (ví dụ: `bsd_warningnotices2 = true`) và lưu ngày tạo WN cùng số thông báo.

                *   **Logic Tạo WN1 (Chưa Generate WN):**
                    *   Nếu `L_warning.Entities.Count == 0` (chưa có WN nào):
                        *   Lấy số ngày cần thiết cho WN1 (`PN_Date`) từ `bsd_warningnotices1date` trong `PS`.
                        *   Nếu `PN_Date >= 0` và `nday >= PN_Date`:
                            *   Tiến hành tạo Entity `bsd_warningnotices` mới với `bsd_numberofwarning = 1`.
                            *   Thiết lập các trường dữ liệu tương tự như trên.
                            *   Tạo bản ghi WN mới và tăng biến đếm `dem`.
                            *   Cập nhật bản ghi `PSD` gốc để đánh dấu WN1 đã được tạo.

5.  **Kết quả:** Đặt giá trị `dem` (tổng số WN đã tạo) vào tham số đầu ra `returnN`.

### RetrieveMultiRecord(IOrganizationService crmservices, string entity, ColumnSet column, string condition, object value)

#### Chức năng tổng quát:
Hàm tiện ích chung để truy vấn nhiều bản ghi từ Dynamics 365 bằng cách sử dụng `QueryExpression` với một điều kiện lọc duy nhất (toán tử Equal).

#### Logic nghiệp vụ chi tiết:
1.  Tạo một đối tượng `QueryExpression` cho thực thể (`entity`) được chỉ định.
2.  Thiết lập `ColumnSet` để xác định các trường cần truy xuất.
3.  Tạo một `FilterExpression` và thêm một `ConditionExpression` duy nhất, sử dụng toán tử `ConditionOperator.Equal` với trường (`condition`) và giá trị (`value`) được cung cấp.
4.  Thực thi truy vấn bằng `service.RetrieveMultiple(q)` và trả về `EntityCollection`.

### findPaymentSchemeDetail(IOrganizationService crmservices, Entity oe)

#### Chức năng tổng quát:
Truy vấn các chi tiết thanh toán (installments) đang hoạt động và có ngày đến hạn, liên quan đến một Option Entry cụ thể.

#### Logic nghiệp vụ chi tiết:
1.  Sử dụng FetchXML để truy vấn thực thể `bsd_paymentschemedetail`.
2.  Lọc các bản ghi theo các điều kiện:
    *   `bsd_optionentry` bằng ID của Option Entry (`oe.Id`).
    *   `statuscode` bằng `100000000` (Active/Open).
    *   `bsd_duedate` không rỗng.
3.  Sắp xếp kết quả theo `bsd_duedate` tăng dần.
4.  Thực thi truy vấn và trả về `EntityCollection`.

### findunits(IOrganizationService crmservices, Entity oe)

#### Chức năng tổng quát:
Truy vấn các đơn vị (Units - `product`) được liên kết với một Option Entry (`salesorder`) cụ thể.

#### Logic nghiệp vụ chi tiết:
1.  Sử dụng FetchXML để truy vấn thực thể `product`.
2.  Liên kết (`link-entity`) với `salesorderdetail` để lọc các đơn vị được liên kết với ID Option Entry (`oe.Id`).
3.  Lọc thêm điều kiện `bsd_estimatehandoverdate` không rỗng.
4.  (Lưu ý: Hàm này được định nghĩa nhưng không được sử dụng trong hàm `Execute` chính.)

### findPaymentScheme(IOrganizationService crmservices, EntityReference ps)

#### Chức năng tổng quát:
Truy vấn bản ghi Kế hoạch Thanh toán (`bsd_paymentscheme`) dựa trên tham chiếu, tập trung vào việc lấy các trường ngày quy định cho Thông báo Cảnh báo.

#### Logic nghiệp vụ chi tiết:
1.  Sử dụng FetchXML để truy vấn thực thể `bsd_paymentscheme`.
2.  Lọc theo `bsd_paymentschemeid` bằng ID của tham chiếu Kế hoạch Thanh toán (`ps.Id`).
3.  Truy xuất các trường `bsd_warningnotices1date` đến `bsd_warningnotices4date`.
4.  Giới hạn kết quả trả về là 1 bản ghi.

### findOptionEntry(IOrganizationService crmservices, string project, string block, string floor, string units)

#### Chức năng tổng quát:
Truy vấn các Option Entry (`salesorder`) đủ điều kiện để kiểm tra việc tạo Thông báo Cảnh báo, áp dụng các bộ lọc địa lý/đơn vị động.

#### Logic nghiệp vụ chi tiết:
1.  Xây dựng chuỗi điều kiện lọc động (`condition`) dựa trên các tham số đầu vào (`project`, `block`, `floor`, `units`) nếu chúng không rỗng. Các điều kiện này được áp dụng cho các trường liên quan đến đơn vị (Unit).
2.  Sử dụng FetchXML để truy vấn thực thể `salesorder`.
3.  Áp dụng các điều kiện lọc cố định cho `salesorder`:
    *   `statuscode` nằm trong một tập hợp các giá trị đang hoạt động/đang xử lý (100000001, 100000003, 100000000, 100000002, 100000005).
    *   `bsd_paymentscheme` không rỗng.
    *   `customerid` không rỗng.
    *   `totalamount` lớn hơn 0.
    *   `bsd_terminationletter` không bằng 1 (chưa bị chấm dứt).
4.  Liên kết với thực thể `product` (thông qua `bsd_unitnumber`) và áp dụng chuỗi điều kiện lọc động đã xây dựng ở bước 1.

### findWarningNotices(IOrganizationService crmservices, Entity paymentDet)

#### Chức năng tổng quát:
Truy vấn tất cả các Thông báo Cảnh báo đang hoạt động liên quan đến một chi tiết thanh toán (installment) cụ thể.

#### Logic nghiệp vụ chi tiết:
1.  Sử dụng FetchXML để truy vấn thực thể `bsd_warningnotices`.
2.  Lọc theo `bsd_paymentschemedeitail` bằng ID của chi tiết thanh toán.
3.  Lọc theo `statecode = 0` (Active).
4.  Sắp xếp kết quả theo `bsd_numberofwarning` giảm dần để dễ dàng xác định mức cảnh báo cao nhất đã được tạo.

### findWarningNoticesByNumberOfWarning(IOrganizationService crmservices, Entity paymentDet, int num)

#### Chức năng tổng quát:
Kiểm tra xem một Thông báo Cảnh báo ở cấp độ cụ thể (`num`) đã được tạo cho một chi tiết thanh toán cụ thể hay chưa.

#### Logic nghiệp vụ chi tiết:
1.  Sử dụng FetchXML để truy vấn thực thể `bsd_warningnotices`.
2.  Lọc theo `bsd_paymentschemedeitail` ID.
3.  Lọc theo `bsd_numberofwarning` bằng giá trị `num` được cung cấp.
4.  Lọc theo `statecode = 0` (Active).

### findGraceDays(IOrganizationService crmservices, EntityReference ps)

#### Chức năng tổng quát:
Truy vấn và xác định số ngày ân hạn (`bsd_gracedays`) áp dụng cho một Kế hoạch Thanh toán cụ thể.

#### Logic nghiệp vụ chi tiết:
1.  Sử dụng FetchXML để truy vấn thực thể `bsd_interestratemaster` (Bảng Lãi suất/Quy tắc).
2.  Liên kết với `bsd_paymentscheme` để lọc theo ID Kế hoạch Thanh toán (`ps.Id`).
3.  Nếu tìm thấy bản ghi:
    *   Kiểm tra giá trị của trường `bsd_intereststartdatetype`.
    *   Nếu giá trị này bằng `100000001` (giả định là loại quy tắc áp dụng ngày ân hạn), trả về giá trị của `bsd_gracedays`.
    *   Nếu giá trị khác, trả về 0.
4.  Nếu không tìm thấy bản ghi, trả về -1.

*(Các hàm `findWarningNotices_Units`, `findWarningNotices_Units_ByNumberOfWarning`, và `findWarningNotices_TODAY` được định nghĩa nhưng không được gọi trong logic chính của `Execute`. Chúng phục vụ các mục đích truy vấn WN dựa trên Đơn vị hoặc Ngày tạo.)*