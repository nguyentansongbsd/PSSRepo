# Phân tích mã nguồn: Action_HandoverNotices_Generate.cs

## Tổng quan

Tệp mã nguồn `Action_HandoverNotices_Generate.cs` là một Plugin (Custom Action) được triển khai trong môi trường Microsoft Dynamics 365/CRM. Chức năng chính của Plugin này là tự động tạo các Thông báo Bàn giao (Handover Notices - `bsd_handovernotice`) dựa trên các bản ghi Cập nhật Ngày Bàn giao Ước tính Chi tiết (`bsd_updateestimatehandoverdatedetail`) đã được phê duyệt.

Quá trình tạo thông báo bao gồm việc truy xuất các thông tin liên quan từ Hợp đồng (Option Entry), Đơn vị (Unit), và các đợt thanh toán (Installments), sau đó thực hiện các phép tính phức tạp để xác định tổng số tiền phải thanh toán, bao gồm phí quản lý, phí bảo trì, số dư nợ chưa thanh toán, lãi thực tế, lãi ước tính, và các khoản khác.

Plugin này hoạt động trong giai đoạn Post-Operation hoặc Pre-Operation của một thông điệp tùy chỉnh (Custom Action) và trả về số lượng thông báo đã được tạo.

---

## Chi tiết các Hàm (Functions/Methods)

### IPlugin.Execute(IServiceProvider serviceProvider)

#### Chức năng tổng quát:
Đây là điểm vào chính của Plugin. Hàm này chịu trách nhiệm thiết lập môi trường CRM, truy vấn danh sách các bản ghi cập nhật ngày bàn giao hợp lệ, lặp qua chúng, và tạo các Thông báo Bàn giao mới nếu chúng chưa tồn tại.

#### Logic nghiệp vụ chi tiết:
1.  **Khởi tạo Dịch vụ:** Lấy `IPluginExecutionContext`, `IOrganizationServiceFactory`, `IOrganizationService`, và `ITracingService` để tương tác với CRM và ghi log.
2.  **Xử lý Tham số Đầu vào:**
    *   Đọc các tham số đầu vào từ `context.InputParameters`: `billdate`, `project` (`pro`), và `enstimatehandover` (`estimatehandover`).
    *   Xác định `billdate` (Ngày lập hóa đơn), mặc định là `DateTime.Now` nếu không được cung cấp.
3.  **Truy vấn Danh sách Chi tiết Cập nhật Ngày Bàn giao (UEHD Detail):**
    *   Tạo một `QueryExpression` nhắm vào thực thể `bsd_updateestimatehandoverdatedetail`.
    *   **Điều kiện Lọc (Criteria):**
        *   `statuscode` = 100000000 (APPROVED).
        *   `statecode` = 0 (Active).
        *   `bsd_optionentry` và `bsd_installment` phải `NotNull`.
    *   **Liên kết Thực thể (Link Entities):**
        *   **Link 1 (Đến `bsd_updateestimatehandoverdate`):**
            *   Lọc theo `bsd_types` (100000002: Update Only for Installment HO hoặc 100000001: Update All).
            *   `bsd_usegeneratehandovernotice` = "1" (Yes).
            *   `bsd_generated` = "0" (No).
            *   Áp dụng lọc theo `bsd_project` và `bsd_updateestimatehandoverdateid` nếu các tham số đầu vào khác "-1".
        *   **Link 2 (Đến `salesorder` - Option Entry):**
            *   Lọc theo `bsd_signedcontractdate` phải `NotNull`.
        *   **Link 3 (Đến `product` - Unit):**
            *   Lọc theo `statuscode` = 100000002 (Sold).
    *   Thực hiện truy vấn và nhận `EntityCollection` (`list`).
4.  **Lặp và Tạo Thông báo Bàn giao:**
    *   Duyệt qua từng bản ghi `detail` trong `list`.
    *   Chuyển đổi thời gian hiện tại sang giờ địa phương (`today`).
    *   **Kiểm tra Tồn tại:** Gọi `CheckExistHandoverNotices` để đảm bảo Thông báo Bàn giao chưa được tạo cho Option Entry và Installment này.
    *   **Lấy Thông tin Cập nhật Ngày Bàn giao (UpEHDEn):** Truy xuất bản ghi cha `bsd_updateestimatehandoverdate` để lấy `bsd_simulationdate` (`UpEHD_SimuDate`) và cờ `bsd_isincludelastinstallment`.
    *   **Khởi tạo Tính toán:** Khởi tạo các biến tính toán (phí, lãi, số dư, v.v.) về 0.
    *   **Truy xuất Option Entry (OE):** Lấy các trường cần thiết từ Option Entry (tên, sơ đồ thanh toán, tổng phần trăm, khách hàng, phí, ngày hợp đồng, v.v.).
    *   **Tính toán các Khoản Mục:**
        *   Tính Khoản Thanh toán Ứng trước (`advancePaymentAmount`) bằng cách gọi `CalculateAdvancePayment`.
        *   Tính Phí Bảo trì và Phí Quản lý còn lại (`maintenanceF`, `managementF`) bằng cách gọi `CalSum_FeeRemaining`.
        *   Tính Số dư Nợ chưa thanh toán (`outstandingUnPaid`) bằng cách gọi `CalculateOutstanding` (có tính đến cờ `bsd_isincludelastinstallment`).
        *   Tính Tổng Biên lai Hệ thống (`TotalSysRe`) bằng cách gọi `CalSum_SystemReceipt` và cộng vào `outstandingUnPaid`.
        *   Tính Lãi Thực tế (`actualInterest`) bằng cách gọi `CalculateActualInterest` (Lãi tính - Lãi đã trả - Lãi miễn trừ).
        *   Tính Khoản Khác (`orther`) bằng cách gọi `CalculateOther`.
        *   Tính Lãi Ước tính (`estimateInterest`) bằng cách gọi `Interest`, sử dụng `UpEHD_SimuDate` (Ngày mô phỏng).
    *   **Tạo Bản ghi Handover Notice (HN):**
        *   Gán các giá trị đã tính toán và các trường tham chiếu (Option Entry, Installment, Unit, Dates) vào bản ghi `bsd_handovernotice` mới.
        *   Tính `bsd_totalamount` (Tổng = Phí QL + Phí BT + Số dư Nợ + Khoản Khác + Lãi Thực tế + Lãi Ước tính + Số tiền Đợt thanh toán - Ứng trước).
        *   Đặt `bsd_generatedbysystem = true`.
        *   Thực hiện `service.Create(hn)`.
    *   **Cập nhật UEHD:** Cập nhật bản ghi `bsd_updateestimatehandoverdate` cha, đặt `bsd_generated = true`.
5.  **Kết thúc:** Trả về số lượng thông báo đã tạo (`count`) qua `context.OutputParameters["ReturnId"]`.

### Interest(IOrganizationService crmservices, Entity oe, DateTime dateCalculate)

#### Chức năng tổng quát:
Hàm này tính toán tổng lãi ước tính cho các đợt thanh toán chưa được thanh toán (Outstanding Installments) tính đến một ngày tính toán cụ thể (`dateCalculate`).

#### Logic nghiệp vụ chi tiết:
1.  **Truy vấn Đợt Thanh toán:**
    *   Truy vấn `bsd_paymentschemedetail` (chi tiết sơ đồ thanh toán) liên quan đến Option Entry (`oe.Id`).
    *   Lọc các đợt có `statuscode` = 100000000 (NOT PAID).
    *   Lọc các đợt có `bsd_duedate` (Ngày đến hạn) `OnOrBefore` (trước hoặc bằng) `dateCalculate`.
2.  **Kiểm tra Cấu hình Lãi suất:**
    *   Kiểm tra xem Option Entry có `bsd_paymentscheme` không.
    *   Kiểm tra xem Payment Scheme có `bsd_interestratemaster` không.
    *   Kiểm tra các trường cần thiết (`bsd_intereststartdatetype`, `bsd_gracedays`, `bsd_termsinterestpercentage`) trong Interest Master.
3.  **Tính toán Lãi suất cho từng Đợt:**
    *   Lặp qua từng đợt thanh toán (`ins`) chưa thanh toán.
    *   Lấy `interestPercent` và `Graceday` từ đợt thanh toán.
    *   Chuyển đổi `bsd_duedate` sang giờ địa phương.
    *   **Tính Ngày trễ cơ bản:** Tính số ngày chênh lệch giữa `dateCalculate` và `duedate`. Số ngày trễ (`bsd_latedays`) là số ngày chênh lệch trừ đi `Graceday`, và không được nhỏ hơn 0.
    *   **Xác định Vị trí Đợt Ký Hợp đồng:** Gọi `getViTriDotSightContract` để xác định số thứ tự của đợt thanh toán được đánh dấu là "Sign Contract Installment".
    *   **Điều chỉnh Ngày trễ (Logic Nghiệp vụ phức tạp):**
        *   Nếu tìm thấy đợt ký hợp đồng (`orderNumberSightContract != -1`):
            *   **Trường hợp 1 (Đợt hiện tại >= Đợt Ký Hợp đồng):** Nếu `orderNumberSightContract <= bsd_ordernumber` và OE có `bsd_signedcontractdate`, biến `numberOfDays2` được đặt thành giá trị đặc biệt `-100599` (ngụ ý không tính lãi theo logic này).
            *   **Trường hợp 2 (Đợt hiện tại < Đợt Ký Hợp đồng):** Nếu OE có `bsd_signeddadate` (Ngày ký DA), `numberOfDays2` được tính là số ngày chênh lệch giữa `dateCalculate` và `bsd_signeddadate`.
            *   **Áp dụng Ngày trễ nhỏ hơn:** Nếu `numberOfDays2` hợp lệ (khác `-100599`) và nhỏ hơn `bsd_latedays` đã tính, thì `bsd_latedays` sẽ được cập nhật bằng `numberOfDays2`. (Điều này có nghĩa là ngày bắt đầu tính lãi có thể bị giới hạn bởi ngày ký DA/Hợp đồng nếu đợt thanh toán đó xảy ra trước sự kiện ký kết).
    *   Lấy số dư (`balance`) của đợt thanh toán.
    *   Tính lãi mới (`Newinterest`) bằng cách gọi `CalculateNewInterest`.
    *   Cộng dồn lãi suất vào tổng lãi (`interest`).
4.  **Trả về:** Trả về tổng lãi suất đã tính.

### getViTriDotSightContract(Guid idOE)

#### Chức năng tổng quát:
Hàm này truy vấn để tìm số thứ tự (`bsd_ordernumber`) của đợt thanh toán được đánh dấu là đợt ký hợp đồng (`bsd_signcontractinstallment = 1`) cho một Option Entry cụ thể.

#### Logic nghiệp vụ chi tiết:
1.  **Truy vấn FetchXML:** Sử dụng FetchXML để truy vấn thực thể `bsd_paymentschemedetail`.
2.  **Điều kiện Lọc:**
    *   `bsd_optionentry` bằng `idOE`.
    *   `bsd_signcontractinstallment` bằng 1.
    *   `statecode` bằng 0 (Active).
3.  **Xử lý Kết quả:** Lặp qua kết quả. Vì chỉ có một đợt ký hợp đồng, hàm sẽ lấy `bsd_ordernumber` của bản ghi đầu tiên tìm thấy và gán cho biến `location`.
4.  **Trả về:** Trả về số thứ tự (`location`) hoặc -1 nếu không tìm thấy.

### CalculateNewInterest(decimal balance, int lateDays, decimal interestPercent)

#### Chức năng tổng quát:
Hàm tính toán số tiền lãi phát sinh dựa trên số dư, số ngày trễ và tỷ lệ lãi suất.

#### Logic nghiệp vụ chi tiết:
1.  **Công thức Tính toán:** Áp dụng công thức tính lãi đơn giản:
    $$Lãi = Số\ dư \times Số\ ngày\ trễ \times \left(\frac{Tỷ\ lệ\ lãi\ suất}{100}\right)$$
2.  **Trả về:** Trả về số tiền lãi đã tính.

### CheckExistHandoverNotices(IOrganizationService crmservices, EntityReference oe, EntityReference ins)

#### Chức năng tổng quát:
Hàm kiểm tra xem một Thông báo Bàn giao đã tồn tại cho một Option Entry và một Đợt thanh toán cụ thể hay chưa.

#### Logic nghiệp vụ chi tiết:
1.  **Truy vấn FetchXML:** Sử dụng FetchXML để truy vấn thực thể `bsd_handovernotice`.
2.  **Điều kiện Lọc:**
    *   `bsd_optionentry` bằng ID của Option Entry (`oe.Id`).
    *   `bsd_installment` bằng ID của Đợt thanh toán (`ins.Id`).
    *   `statuscode` bằng 1.
    *   `statecode` bằng 0 (Active).
3.  **Kiểm tra Kết quả:** Trả về `true` nếu số lượng bản ghi tìm thấy lớn hơn 0, ngược lại trả về `false`.

### CalculateOutstanding(IOrganizationService crmservices, EntityReference oe, Boolean isincludelastinstallment)

#### Chức năng tổng quát:
Hàm tính tổng số dư nợ chưa thanh toán (`bsd_balance`) của các đợt thanh toán liên quan đến một Option Entry.

#### Logic nghiệp vụ chi tiết:
1.  **Truy vấn Tổng hợp (Aggregate FetchXML):** Truy vấn thực thể `bsd_paymentschemedetail` và tính tổng (`sum`) của trường `bsd_balance`.
2.  **Điều kiện Lọc Chung:**
    *   `bsd_optionentry` bằng ID của Option Entry.
    *   `bsd_duedatecalculatingmethod` khác 100000002 (loại trừ các đợt không tính vào số dư nợ).
3.  **Điều kiện Lọc Tùy chọn:**
    *   Nếu `isincludelastinstallment` là `false`, thêm điều kiện lọc: `bsd_lastinstallment` khác 1 (loại trừ đợt thanh toán cuối cùng).
4.  **Trả về:** Trả về `EntityCollection` chứa kết quả tổng hợp.

### CalSum_SystemReceipt(IOrganizationService crmservices, EntityReference oe)

#### Chức năng tổng quát:
Hàm tính tổng số tiền đã thanh toán từ các Biên lai Hệ thống (`bsd_systemreceipt`) liên quan đến một Option Entry.

#### Logic nghiệp vụ chi tiết:
1.  **Truy vấn Tổng hợp (Aggregate FetchXML):** Truy vấn thực thể `bsd_systemreceipt` và tính tổng (`sum`) của trường `bsd_amountpay`.
2.  **Điều kiện Lọc:**
    *   `bsd_optionentry` bằng ID của Option Entry.
    *   `statuscode` bằng 100000000 (Trạng thái đã thanh toán/hợp lệ).
3.  **Trả về:** Trả về `EntityCollection` chứa kết quả tổng hợp.

### CalculateOther(IOrganizationService crmservices, EntityReference oe, Boolean isincludelastinstallment)

#### Chức năng tổng quát:
Hàm tính tổng số dư của các khoản mục linh tinh (`bsd_miscellaneous`) liên quan đến các đợt thanh toán của Option Entry.

#### Logic nghiệp vụ chi tiết:
1.  **Truy vấn Tổng hợp (Aggregate FetchXML):** Truy vấn thực thể `bsd_miscellaneous` và tính tổng (`sum`) của trường `bsd_balance`.
2.  **Liên kết Thực thể:** Liên kết với `bsd_paymentschemedetail` (thông qua trường `bsd_installment`).
3.  **Điều kiện Lọc:**
    *   `bsd_optionentry` bằng ID của Option Entry.
    *   `statuscode` của `bsd_miscellaneous` bằng 1.
4.  **Điều kiện Lọc Tùy chọn:**
    *   Nếu `isincludelastinstallment` là `false`, thêm điều kiện lọc trên `bsd_paymentschemedetail`: `bsd_lastinstallment` khác 1 (loại trừ các khoản linh tinh liên quan đến đợt cuối).
5.  **Trả về:** Trả về `EntityCollection` chứa kết quả tổng hợp.

### CalculateActualInterest(IOrganizationService crmservices, EntityReference oe)

#### Chức năng tổng quát:
Hàm tính tổng lãi suất thực tế đã phát sinh, đã trả và đã miễn trừ trên tất cả các đợt thanh toán của một Option Entry.

#### Logic nghiệp vụ chi tiết:
1.  **Truy vấn Tổng hợp (Aggregate FetchXML):** Truy vấn thực thể `bsd_paymentschemedetail`.
2.  **Tính Tổng:** Tính tổng (`sum`) của ba trường:
    *   `bsd_interestchargeamount` (Lãi đã tính - alias: `amount`).
    *   `bsd_interestwaspaid` (Lãi đã trả - alias: `paid`).
    *   `bsd_waiverinterest` (Lãi miễn trừ - alias: `waiver`).
3.  **Điều kiện Lọc:** Lọc theo `bsd_optionentry` bằng ID của Option Entry.
4.  **Trả về:** Trả về `EntityCollection` chứa ba giá trị tổng hợp. (Lãi thực tế được tính trong hàm `Execute` là `amount - paid - waiver`).

### CalculateAdvancePayment(IOrganizationService crmservices, EntityReference cus, EntityReference pro, EntityReference oe)

#### Chức năng tổng quát:
Hàm tính tổng số tiền còn lại (`bsd_remainingamount`) của các khoản thanh toán ứng trước (`bsd_advancepayment`) liên quan đến Option Entry.

#### Logic nghiệp vụ chi tiết:
1.  **Truy vấn Tổng hợp (Aggregate FetchXML):** Truy vấn thực thể `bsd_advancepayment` và tính tổng (`sum`) của trường `bsd_remainingamount`.
2.  **Điều kiện Lọc:**
    *   `statuscode` bằng 100000000 (Trạng thái hợp lệ/còn lại).
    *   `bsd_optionentry` bằng ID của Option Entry.
3.  **Trả về:** Trả về `EntityCollection` chứa kết quả tổng hợp.

### RetrieveLocalTimeFromUTCTime(DateTime utcTime, IOrganizationService service)

#### Chức năng tổng quát:
Hàm tiện ích chuẩn của CRM để chuyển đổi thời gian UTC sang múi giờ địa phương của người dùng đang thực thi Plugin.

#### Logic nghiệp vụ chi tiết:
1.  **Truy vấn Cài đặt Người dùng:** Truy vấn thực thể `usersettings` để lấy `timezonecode` của người dùng hiện tại.
2.  **Kiểm tra Múi giờ:** Nếu không tìm thấy mã múi giờ, ném ra ngoại lệ.
3.  **Thực thi Yêu cầu CRM:** Tạo và thực thi `LocalTimeFromUtcTimeRequest` bằng cách truyền mã múi giờ và thời gian UTC đầu vào.
4.  **Trả về:** Trả về thời gian địa phương đã được chuyển đổi.

### CalSum_FeeRemaining(IOrganizationService crmservices, EntityReference oe)

#### Chức năng tổng quát:
Hàm tính tổng số tiền Phí Bảo trì còn lại (`bsd_maintenancefeeremaining`) và Phí Quản lý còn lại (`bsd_managementfeeremaining`) từ các đợt thanh toán.

#### Logic nghiệp vụ chi tiết:
1.  **Truy vấn Tổng hợp (Aggregate FetchXML):** Truy vấn thực thể `bsd_paymentschemedetail`.
2.  **Tính Tổng:** Tính tổng (`sum`) của hai trường:
    *   `bsd_maintenancefeeremaining` (alias: `MainFeeReAmt`).
    *   `bsd_managementfeeremaining` (alias: `ManaFeeReAmt`).
3.  **Điều kiện Lọc:** Lọc theo `bsd_optionentry` bằng ID của Option Entry.
4.  **Trả về:** Trả về `EntityCollection` chứa hai giá trị tổng hợp.