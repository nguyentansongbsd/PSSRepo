# Phân tích mã nguồn: Plugin\_Create\_TerminationLetter.cs

## Tổng quan

Tệp mã nguồn `Plugin_Create_TerminationLetter.cs` chứa một plugin Dynamics 365 (C#) được thiết kế để tự động hóa quá trình tạo và tính toán các khoản phí liên quan đến Thư Chấm dứt Hợp đồng (Termination Letter - `bsd_terminateletter`).

Plugin này thường được kích hoạt trên sự kiện `Create` hoặc `Update` của bản ghi Thư Chấm dứt Hợp đồng. Nhiệm vụ chính của nó là ánh xạ dữ liệu từ các bản ghi liên quan (Dự án, Chủ đầu tư, Option Entry), cập nhật trạng thái của Option Entry, và quan trọng nhất là tính toán các khoản Phạt (Penalty) và Lãi suất quá hạn (Overdue Interest) dựa trên các đợt thanh toán quá hạn của khách hàng.

## Chi tiết các Hàm (Functions/Methods)

### Execute(IServiceProvider serviceProvider)

#### Chức năng tổng quát:
Đây là phương thức chính của plugin, chịu trách nhiệm thiết lập môi trường CRM, truy xuất bản ghi Thư Chấm dứt Hợp đồng đang được xử lý, ánh xạ dữ liệu cơ bản, cập nhật Option Entry, và khởi động quá trình tính toán Phạt và Lãi suất quá hạn.

#### Logic nghiệp vụ chi tiết:
1.  **Khởi tạo Dịch vụ:** Thiết lập các đối tượng tiêu chuẩn của CRM Plugin như `IPluginExecutionContext`, `IOrganizationServiceFactory`, `IOrganizationService`, và `ITracingService`.
2.  **Truy xuất Bản ghi Mục tiêu:** Lấy bản ghi `bsd_terminateletter` hiện tại (`enCreated`) từ tham số `Target`.
3.  **Ánh xạ Dữ liệu (Mapping):**
    *   Truy xuất bản ghi Dự án (`bsd_project`) liên quan.
    *   Truy xuất bản ghi Chủ đầu tư/Nhà phát triển (`account`) liên quan từ Dự án.
    *   Cập nhật trường `bsd_accountnameother_develop` trên bản ghi Thư Chấm dứt Hợp đồng bằng tên khác của Chủ đầu tư (nếu có) hoặc tên chính thức.
4.  **Kiểm tra Option Entry:** Kiểm tra xem bản ghi Thư Chấm dứt Hợp đồng có chứa tham chiếu đến Option Entry (`bsd_optionentry`) hay không. Nếu không, plugin kết thúc.
5.  **Cập nhật Option Entry:**
    *   Truy xuất bản ghi Option Entry (`op`).
    *   Tạo bản ghi cập nhật cho Option Entry và đặt trường `bsd_terminationletter` thành `true`.
6.  **Kiểm tra Follow Up List:** Kiểm tra xem bản ghi có chứa tham chiếu đến Follow Up List (`bsd_followuplist`) hay không. Nếu không, plugin kết thúc.
7.  **Truy xuất Chi tiết Đợt Thanh toán:** Gọi hàm `get_pmSchDtl_fromOpentryID(opRef.Id)` để lấy danh sách các Chi tiết Đợt Thanh toán (`bsd_paymentschemedetail`) liên quan đến Option Entry (chỉ những đợt có `statuscode` là `100000000`).
8.  **Vòng lặp Tính toán:** Lặp qua từng Chi tiết Đợt Thanh toán đã truy xuất:
    *   Gọi hàm `TinhLai(enupdate)` để thực hiện tính toán Phạt và Lãi suất quá hạn.
9.  **Tính toán Phí Chấm dứt Cuối cùng:** Sau khi tính toán xong, tính toán phí chấm dứt cuối cùng:
    *   `bsd_terminatefee` = `bsd_totalforfeitureamount` - `bsd_terminatefeewaiver`.
10. **Cập nhật Bản ghi:** Cập nhật bản ghi Thư Chấm dứt Hợp đồng (`enupdate`) với các giá trị đã tính toán.

### TinhLai(Entity enupdate)

#### Chức năng tổng quát:
Hàm này chịu trách nhiệm xác định đợt thanh toán quá hạn đầu tiên chưa được xử lý, ánh xạ các Thông báo Cảnh báo (Warning Notices) liên quan, và tính toán các khoản Phạt (Penalty) và Lãi suất quá hạn (Overdue Interest).

#### Logic nghiệp vụ chi tiết:
1.  **Kiểm tra Quá hạn:** Gọi hàm `check_overDuedate_pmShcDtl` để tìm một đợt thanh toán quá hạn (`installment`) chưa được xử lý.
2.  **Ánh xạ Thông báo Cảnh báo (Warning Notices):**
    *   Nếu tìm thấy đợt quá hạn và biến cờ `isMapWN` là `false` (chưa ánh xạ):
        *   Truy vấn 2 bản ghi Thông báo Cảnh báo (`bsd_warningnotices`) mới nhất liên quan đến đợt thanh toán quá hạn này.
        *   Ánh xạ các bản ghi này vào các trường `bsd_warning_notices_1` và `bsd_warning_notices_2` trên Thư Chấm dứt Hợp đồng.
        *   Ánh xạ đợt thanh toán quá hạn vào trường `bsd_insallment`.
        *   Đặt `isMapWN = true`.
3.  **Tính toán Phạt (Penalty):**
    *   Tính toán giá trị phạt dựa trên 20% của tổng số tiền chưa bao gồm phí vận chuyển (`bsd_totalamountlessfreight`) của Option Entry.
    *   Cập nhật trường `bsd_penaty` trên bản ghi `enupdate`.
4.  **Tính toán Lãi suất Quá hạn (Overdue Interest):**
    *   Truy xuất bản ghi Payment Scheme (`bsd_paymentscheme`) và Interest Rate Master (`bsd_interestratemaster`) liên quan.
    *   Lấy các thông số cần thiết: tỷ lệ lãi suất (`bsd_termsinterestpercentage`), số ngày ân hạn (`bsd_gracedays`), và ngày đến hạn (`bsd_duedate`).
    *   **Xác định Case Ký kết (`caseSign`):** Xác định trường hợp ký kết (1, 2, 3, hoặc 4) dựa trên sự hiện diện của `bsd_signedcontractdate` và `bsd_signeddadate` trên Option Entry.
    *   **Tính toán Số ngày Trễ hạn:** Gọi hàm `checkCaseSignAndCalLateDays` để tính toán số ngày trễ hạn (`lateDays`) dựa trên ngày ký kết và ngày đến hạn.
    *   **Tính Lãi suất:** Nếu `checkCaseSignAndCalLateDays` trả về `true`:
        *   Tính toán lãi suất quá hạn cho đợt hiện tại: `(Tỷ lệ Lãi suất / 100) * Số ngày Trễ hạn * Số dư (bsd_balance)`.
        *   Cộng dồn giá trị này vào tổng lãi suất quá hạn (`bsd_overdue_interest`).
        *   Cộng thêm số tiền lãi còn lại của đợt hiện tại (`bsd_interestchargeremaining_current`) vào tổng lãi suất quá hạn.
        *   Cập nhật các trường `bsd_overdue_interest` (Decimal) và `bsd_overdue_interest_money` (Money) trên bản ghi `enupdate`.

### get\_pmSchDtl\_fromOpentryID(Guid opID)

#### Chức năng tổng quát:
Truy vấn và trả về danh sách các Chi tiết Đợt Thanh toán (`bsd_paymentschemedetail`) liên quan đến một Option Entry cụ thể, chỉ bao gồm các đợt có trạng thái cụ thể (statuscode = 100000000).

#### Logic nghiệp vụ chi tiết:
1.  Tạo một `QueryExpression` cho thực thể `bsd_paymentschemedetail`.
2.  Chỉ định các cột cần thiết (ngày đến hạn, trạng thái, số dư, ngày ân hạn thực tế, v.v.).
3.  Thiết lập điều kiện lọc (`FilterExpression`):
    *   `bsd_optionentry` bằng `opID`.
    *   `statuscode` bằng `100000000`.
4.  Sắp xếp kết quả theo `bsd_duedate` giảm dần.
5.  Thực hiện truy vấn và trả về `EntityCollection`.

### checkCaseSignAndCalLateDays(Entity enInstallment, Entity enOptionEntry, DateTime bsd\_signedcontractdate, DateTime bsd\_signeddadate, DateTime receiptdate, ref int lateDays)

#### Chức năng tổng quát:
Xác định cách tính số ngày trễ hạn (`lateDays`) dựa trên các ngày ký hợp đồng/DA (Deposit Agreement) và ngày đến hạn của đợt thanh toán, sử dụng biến `caseSign` đã được xác định trước.

#### Logic nghiệp vụ chi tiết:
1.  **Kiểm tra Ngày đến hạn:** Xác định xem đợt thanh toán hiện tại có trường `bsd_duedate` hay không.
2.  **Tìm Đợt Ký Hợp đồng:** Truy vấn để tìm Chi tiết Đợt Thanh toán được đánh dấu là "Sign Contract Installment" (`bsd_signcontractinstallment = true`) cho Option Entry hiện tại.
3.  **Xử lý theo Case Ký kết (`caseSign`):**
    *   **Case 0 (Mặc định/Không xác định):** Trả về `false`.
    *   **Case 1 (Chỉ có DA được ký):**
        *   Nếu không có ngày đến hạn, trả về `false`.
        *   So sánh ngày đến hạn của đợt hiện tại (`bsd_duedate`) với ngày đến hạn của đợt Ký Hợp đồng (`bsd_duedateFlag`).
        *   Nếu `bsd_duedate < bsd_duedateFlag`: `lateDays` được tính từ `receiptdate` trừ đi `bsd_signeddadate`.
        *   Nếu `bsd_duedate >= bsd_duedateFlag`: `lateDays` được tính từ `receiptdate` trừ đi `bsd_duedate`.
    *   **Case 2 (Chỉ có Hợp đồng được ký):**
        *   Nếu `bsd_duedate >= bsd_duedateFlag`: `lateDays` được tính từ `receiptdate` trừ đi `bsd_signedcontractdate`.
    *   **Case 3 (Cả Hợp đồng và DA được ký):**
        *   Nếu `bsd_duedate < bsd_duedateFlag`: `lateDays` được tính từ `receiptdate` trừ đi `bsd_signeddadate`.
        *   Nếu `bsd_duedate >= bsd_duedateFlag`: `lateDays` được tính từ `receiptdate` trừ đi `bsd_duedate`.
    *   **Case 4 (Không có ngày ký nào):**
        *   Nếu `bsd_duedate >= bsd_duedateFlag`: `lateDays` được tính từ `receiptdate` trừ đi `bsd_duedate`.
4.  **Trả về Kết quả:** Trả về `true` nếu việc tính toán trễ hạn được thực hiện, ngược lại là `false`.

### check\_overDuedate\_pmShcDtl(EntityCollection entCll, ref Entity installment)

#### Chức năng tổng quát:
Kiểm tra danh sách các Chi tiết Đợt Thanh toán để tìm đợt thanh toán quá hạn đầu tiên (dựa trên ngày Follow Up List) chưa được xử lý.

#### Logic nghiệp vụ chi tiết:
1.  Lặp qua `EntityCollection` các Chi tiết Đợt Thanh toán (`entCll`).
2.  **Kiểm tra Ngày đến hạn:** Bỏ qua nếu đợt thanh toán không có `bsd_duedate`.
3.  **Tính Ngày ân hạn:** Lấy ngày ân hạn (`graceDays`) từ đợt thanh toán (nếu có).
4.  **Tính Ngày Quá hạn Thực tế:** Tính ngày đến hạn thực tế bằng cách cộng `graceDays` vào `bsd_duedate`.
5.  **So sánh Ngày:** So sánh ngày của Follow Up List (`FUL["bsd_date"]`) với ngày đến hạn thực tế đã điều chỉnh.
6.  **Xác định Quá hạn:** Nếu số ngày chênh lệch lớn hơn 0 (tức là đã quá hạn) VÀ đợt thanh toán chưa có trong danh sách đã xử lý (`lstInstallments`):
    *   Gán đợt thanh toán này vào biến `installment` (tham chiếu `ref`).
    *   Thêm ID của đợt thanh toán vào `lstInstallments`.
    *   Trả về `true`.
7.  Nếu kết thúc vòng lặp mà không tìm thấy đợt quá hạn nào, trả về `false`.

### get\_all\_pmSchDtl\_fromOpentryID(Guid opID)

#### Chức năng tổng quát:
Truy vấn và trả về TẤT CẢ các Chi tiết Đợt Thanh toán (`bsd_paymentschemedetail`) liên quan đến một Option Entry cụ thể, không giới hạn bởi trạng thái.

#### Logic nghiệp vụ chi tiết:
1.  Tạo một `QueryExpression` cho thực thể `bsd_paymentschemedetail`.
2.  Chỉ định các cột cần thiết (ngày đến hạn, trạng thái, số tiền đã trả, v.v.).
3.  Thiết lập điều kiện lọc: `bsd_optionentry` bằng `opID`.
4.  Thực hiện truy vấn và trả về `EntityCollection`. (Hàm này dường như chỉ được sử dụng trong phần logic tính toán Penalty đã bị comment out, nhưng vẫn tồn tại trong mã nguồn).