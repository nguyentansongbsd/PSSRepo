# Phân tích mã nguồn: Plugin_Payment_Import.cs

## Tổng quan

Tệp mã nguồn `Plugin_Payment_Import.cs` định nghĩa một Plugin Dynamics 365/Power Platform được thực thi trên thực thể thanh toán (Payment record, có thể là một thực thể tùy chỉnh). Plugin này chủ yếu xử lý logic nghiệp vụ phức tạp khi tạo (`Create`) hoặc cập nhật (`Update`) bản ghi thanh toán, đặc biệt là việc liên kết khoản thanh toán với các đợt thanh toán cụ thể (Payment Scheme Details) trong hợp đồng/đơn hàng (Sales Order/Quote) và tính toán các khoản phí, lãi suất chậm trả.

Plugin này sử dụng các truy vấn FetchXML để tìm kiếm các bản ghi liên quan (Quote, Sales Order, Payment Scheme Detail) dựa trên đơn vị (Unit) và loại thanh toán, đảm bảo rằng giao dịch thanh toán tuân thủ các quy tắc nghiệp vụ và trạng thái của hợp đồng.

## Chi tiết các Hàm (Functions/Methods)

### Execute(IServiceProvider serviceProvider)

#### Chức năng tổng quát:
Là điểm vào chính của Plugin, chịu trách nhiệm thiết lập môi trường dịch vụ (Service, Factory, Context, Trace) và thực thi logic nghiệp vụ dựa trên loại thông điệp (Create hoặc Update) được kích hoạt trên thực thể Target.

#### Logic nghiệp vụ chi tiết:

1.  **Khởi tạo và Thiết lập:**
    *   Lấy `IPluginExecutionContext`, `IOrganizationServiceFactory`, và `IOrganizationService`.
    *   Lấy `ITracingService` để ghi lại nhật ký theo dõi.
    *   Lấy thực thể `Target` từ tham số đầu vào.

2.  **Kiểm tra `bsd_assignamount` (Áp dụng cho cả Create và Update):**
    *   Kiểm tra nếu trường `bsd_assignamount` (số tiền gán) có giá trị âm. Nếu có, ném ra `InvalidPluginExecutionException` với thông báo lỗi.

3.  **Xử lý thông điệp "Create":**
    *   Nếu trường `bsd_checkimport` không tồn tại (ngụ ý đây không phải là một giao dịch nhập dữ liệu hàng loạt), plugin tiếp tục xử lý.
    *   Kiểm tra cờ `bsd_createbyrevert`. Nếu là `false`, logic chính được thực thi.
    *   Xác định loại thanh toán (`Payment_Type` - trường `bsd_paymenttype`).

    *   **A. Loại thanh toán: Deposit fee (100000001)**
        *   Kiểm tra xem bản ghi có chứa tham chiếu đến đơn vị (`bsd_units`) không.
        *   Truy vấn thực thể `quote` (Reservation) liên quan đến đơn vị đó.
        *   Lọc các Quote có trạng thái (`statuscode`) là `100000000` (Active/Draft) hoặc `100000006` (Pending).
        *   Nếu tìm thấy Quote:
            *   Gán người mua (`bsd_purchaser`) và tham chiếu đặt chỗ (`bsd_reservation`) từ Quote.
            *   Lấy số tiền đặt cọc (`bsd_depositfee`) từ Quote và gán cho `bsd_totalamountpayablephase`.
            *   Tính toán `bsd_differentamount` = `bsd_amountpay` - `bsd_depositfee`.
            *   Cập nhật bản ghi Target.
        *   Nếu không tìm thấy Quote thỏa mãn điều kiện: Ném lỗi.

    *   **B. Loại thanh toán: Instalment (100000002)**
        *   Yêu cầu phải có `bsd_units` và `bsd_odernumber` (số thứ tự đợt).
        *   Truy vấn Sales Order (`salesorder`) liên quan đến đơn vị, với các trạng thái hoạt động (100000000 đến 100000005).
        *   Nếu tìm thấy Sales Order:
            *   Truy vấn `bsd_paymentschemedetail` (chi tiết đợt thanh toán) dựa trên Sales Order ID, trạng thái `100000000` và `bsd_ordernumber` khớp.
            *   Nếu không tìm thấy đợt thanh toán: Ném lỗi.
            *   Nếu tìm thấy:
                *   Gán tham chiếu Sales Order (`bsd_optionentry`) và người mua (`bsd_purchaser`).
                *   Gán tham chiếu chi tiết đợt thanh toán (`bsd_paymentschemedetail`).
                *   Chuyển đổi ngày đến hạn (`bsd_duedate`) sang giờ địa phương và gán cho `bsd_duedateinstallment`.
                *   Lấy các giá trị thanh toán (Amount Payable, Paid, Waiver) và gán cho Target.
                *   Tính toán `bsd_balance` (Số tiền phải trả - Đã trả - Đặt cọc - Miễn trừ).
                *   Tính toán `bsd_differentamount` = `bsd_amountpay` - `bsd_balance`.
                *   **Tính toán Lãi chậm trả:**
                    *   Chuyển đổi ngày thanh toán thực tế (`bsd_paymentactualtime`) và ngày đến hạn (`bsd_duedate`) sang giờ địa phương bằng hàm `RetrieveLocalTimeFromUTCTime`.
                    *   Tính số ngày trễ (`bsd_latedays`) = (Ngày nhận tiền - Ngày đến hạn) - `bsd_gracedays`. Nếu kết quả âm, đặt bằng 0.
                    *   Gọi hàm `getViTriDotSightContract` để tìm số thứ tự đợt ký hợp đồng.
                    *   Áp dụng logic phức tạp để điều chỉnh `bsd_latedays` dựa trên vị trí đợt ký hợp đồng và ngày ký hợp đồng/DA (nếu có).
                    *   Tính toán tiền lãi (`bsd_interestcharge`) dựa trên lãi suất (`Termsinterest`), số ngày trễ (`bsd_latedays`), và số tiền gốc (sử dụng `balane` nếu `amount_pay > balane`, hoặc `amount_pay` nếu ngược lại).
                *   Cập nhật bản ghi Target.

    *   **C. Loại thanh toán: Interest chart (100000003)**
        *   Yêu cầu có `bsd_units` và `bsd_odernumber`.
        *   Truy vấn Sales Order liên quan.
        *   Truy vấn `bsd_paymentschemedetail` có `bsd_interestchargeremaining` (lãi suất còn lại) khác 0 và không rỗng.
        *   Nếu tìm thấy: So sánh `bsd_amountpay` với `bsd_interestchargeremaining`.
            *   Nếu `amount_pay <= remaining`: Gán tham chiếu Sales Order, ID chi tiết đợt thanh toán, số tiền áp dụng, và người mua. Cập nhật Target.
            *   Nếu `amount_pay > remaining`: Ném lỗi.

    *   **D. Loại thanh toán: Fees (100000004)**
        *   Yêu cầu có `bsd_units`.
        *   Truy vấn Sales Order liên quan.
        *   Truy vấn `bsd_paymentschemedetail` có `statuscode` 100000000 và `bsd_duedatecalculatingmethod` 100000002 (phương thức tính phí).
        *   Xác định loại phí (`bsd_typefee`: 100000000 là Maintenance Fee, khác là Management Fee).
        *   So sánh `amount_pay` với số tiền phí còn lại tương ứng (`bsd_maintenancefeeremaining` hoặc `bsd_managementfeeremaining`).
        *   Nếu `amount_pay` lớn hơn số tiền còn lại: Ném lỗi.
        *   Nếu không: Cập nhật các trường áp dụng phí (`bsd_arrayfees`, `bsd_arrayfeesamount`, v.v.) và cập nhật Target.

    *   **E. Loại thanh toán: Other (100000005)**
        *   Yêu cầu có `bsd_units`, `bsd_odernumber`, và `bsd_numbermisc` (số thứ tự khoản khác).
        *   Truy vấn Sales Order và Payment Scheme Detail (Instalment) tương ứng.
        *   Truy vấn `bsd_miscellaneous` (khoản khác) liên quan đến đợt thanh toán đó và `bsd_numbermisc`.
        *   Nếu tìm thấy Miscellaneous: So sánh `bsd_amountpay` với `bsd_balance` của khoản Miscellaneous.
            *   Nếu `amount_pay <= balance`: Cập nhật các trường áp dụng khoản khác (`bsd_arraymicellaneousid`, `bsd_arraymicellaneousamount`, v.v.) và cập nhật Target.
            *   Nếu `amount_pay > balance`: Ném lỗi.

4.  **Xử lý thông điệp "Update":**
    *   Kiểm tra `bsd_assignamount` (phải >= 0).
    *   Nếu trường `statuscode` được cập nhật:
        *   Lấy trạng thái cũ từ PreEntityImage (`preimage`).
        *   Nếu trạng thái cũ khác 1 (Active/Draft) và trạng thái mới là 2 (Deactivated/Inactive), ném lỗi, ngăn chặn việc hủy kích hoạt không hợp lệ.

### getViTriDotSightContract(Guid idOE)

#### Chức năng tổng quát:
Truy vấn các chi tiết đợt thanh toán (`bsd_paymentschemedetail`) để xác định số thứ tự đợt (`bsd_ordernumber`) được đánh dấu là đợt ký hợp đồng.

#### Logic nghiệp vụ chi tiết:
1.  Thực hiện truy vấn FetchXML trên thực thể `bsd_paymentschemedetail`.
2.  Lọc các bản ghi theo ID của Sales Order (`idOE`), điều kiện `bsd_signcontractinstallment` bằng 1 (Đợt ký hợp đồng), và `statecode` bằng 0 (Active).
3.  Lặp qua các kết quả (dù logic chỉ mong đợi một kết quả) và gán giá trị `bsd_ordernumber` cho biến `location`.
4.  Trả về `location` (số thứ tự đợt ký hợp đồng), hoặc -1 nếu không tìm thấy.

### RetrieveLocalTimeFromUTCTime(DateTime utcTime)

#### Chức năng tổng quát:
Chuyển đổi một giá trị thời gian từ múi giờ UTC sang múi giờ địa phương của người dùng đang thực thi Plugin.

#### Logic nghiệp vụ chi tiết:
1.  Gọi hàm `RetrieveCurrentUsersSettings` để lấy mã múi giờ (`timeZoneCode`) của người dùng hiện tại.
2.  Nếu không tìm thấy mã múi giờ, ném lỗi.
3.  Tạo một `LocalTimeFromUtcTimeRequest` với mã múi giờ và thời gian UTC đầu vào.
4.  Thực thi Request thông qua `IOrganizationService`.
5.  Trả về `LocalTime` từ phản hồi.

### RetrieveCurrentUsersSettings(IOrganizationService service)

#### Chức năng tổng quát:
Truy vấn hệ thống Dynamics 365 để lấy cài đặt người dùng hiện tại, cụ thể là mã múi giờ (`timezonecode`).

#### Logic nghiệp vụ chi tiết:
1.  Tạo một `QueryExpression` nhắm vào thực thể `usersettings`.
2.  Chỉ định cột cần lấy là `localeid` và `timezonecode`.
3.  Thiết lập điều kiện lọc để chỉ lấy cài đặt của người dùng hiện tại (`ConditionOperator.EqualUserId`).
4.  Thực thi truy vấn và lấy bản ghi đầu tiên.
5.  Trả về giá trị của trường `timezonecode` dưới dạng số nguyên nullable.