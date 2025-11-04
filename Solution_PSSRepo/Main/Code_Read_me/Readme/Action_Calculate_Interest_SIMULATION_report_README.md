# Phân tích mã nguồn: Action_Calculate_Interest_SIMULATION_report.cs

## Tổng quan

Tệp mã nguồn `Action_Calculate_Interest_SIMULATION_report.cs` chứa một Plugin (hoặc Custom Action) của Microsoft Dynamics 365/Power Platform, được thiết kế để tính toán tiền lãi chậm thanh toán (late interest) cho một đợt thanh toán cụ thể (`bsd_paymentschemedetail`). Chức năng chính của nó là mô phỏng hoặc báo cáo số tiền lãi ước tính dựa trên ngày nhận tiền thực tế (`receiptdate`) và các quy tắc lãi suất được định nghĩa trong hệ thống (Interest Rate Master).

Plugin này xử lý các logic nghiệp vụ phức tạp liên quan đến việc xác định ngày bắt đầu tính lãi, số ngày trễ hạn, và áp dụng mức trần (CAP) cho tổng tiền lãi phát sinh, có tính đến các điều kiện ký hợp đồng (Sign Contract/Sign DA).

## Chi tiết các Hàm (Functions/Methods)

### Execute(IServiceProvider serviceProvider)

**Chức năng tổng quát:** Đây là điểm vào tiêu chuẩn của Plugin Dynamics 365. Hàm này khởi tạo các dịch vụ cần thiết (Service, Factory, Tracing) và xử lý việc nhận các tham số đầu vào từ ngữ cảnh thực thi (Context Input Parameters).

**Logic nghiệp vụ chi tiết:**

1.  **Khởi tạo dịch vụ:** Lấy các dịch vụ `IPluginExecutionContext`, `IOrganizationServiceFactory`, và `ITracingService`. Khởi tạo `service` để tương tác với CRM.
2.  **Lấy tham số đầu vào:** Lấy ba tham số bắt buộc từ `context.InputParameters`:
    *   `installmentid` (ID của đợt thanh toán).
    *   `stramountpay` (Số tiền thanh toán, dưới dạng chuỗi).
    *   `receiptdate` (Ngày nhận tiền, dưới dạng chuỗi).
3.  **Thực thi logic chính:** Gọi hàm `Main` với các tham số đã lấy được. Kết quả tính toán sẽ được lưu vào biến `serializedResult`.
4.  **Thiết lập đầu ra:** Gán `serializedResult` vào tham số đầu ra `context.OutputParameters["result"]`.
5.  **Ghi log và Xử lý lỗi:** Ghi lại thông tin theo dõi (`TracingSe`) và xử lý các ngoại lệ (`InvalidPluginExecutionException`), đảm bảo thông báo lỗi được trả về trong tham số đầu ra nếu có lỗi xảy ra.

### Main(string installmentid, string stramountpay, string receiptdateimport, ref string serializedResult)

**Chức năng tổng quát:** Hàm này chứa luồng nghiệp vụ chính, thực hiện việc truy xuất dữ liệu, tính toán các thông số cơ bản, áp dụng logic ngày ký hợp đồng, và gọi hàm tính lãi cuối cùng.

**Logic nghiệp vụ chi tiết:**

1.  **Chuyển đổi dữ liệu:** Chuyển đổi `stramountpay` sang `decimal` và `receiptdateimport` sang `DateTime`.
2.  **Chuyển đổi múi giờ:** Chuyển đổi `receiptdate` từ UTC sang giờ địa phương bằng cách gọi `RetrieveLocalTimeFromUTCTime`.
3.  **Truy xuất thực thể:** Truy xuất thực thể `bsd_paymentschemedetail` (Installment) hiện tại. Nếu đây là lần gọi đầu tiên (`!isMap`), lưu trữ nó làm `enInstallmentroot`.
4.  **Truy xuất thực thể liên quan:** Truy xuất các thực thể liên quan: `bsd_paymentscheme`, `bsd_interestratemaster`, và `bsd_optionentry`.
5.  **Tính toán ngày bắt đầu tính lãi:**
    *   Lấy số ngày ân hạn (`bsd_gracedays`) từ Interest Rate Master.
    *   Tính `interestStarDate` bằng cách cộng số ngày ân hạn vào ngày đến hạn (`bsd_duedate`).
6.  **Tính toán số ngày trễ ban đầu:** Tính `lateDays` dựa trên sự khác biệt giữa `receiptdate` và `interestStarDate`.
7.  **Logic kiểm tra ngày ký hợp đồng (Case Sign):**
    *   Kiểm tra sự tồn tại của `bsd_signedcontractdate` và `bsd_signeddadate` trên Option Entry để xác định `caseSign` (0, 1, 2, 3, 4).
    *   Gọi hàm `checkCaseSignAndCalLateDays` để điều chỉnh `lateDays` dựa trên logic `caseSign` và ngày ký hợp đồng.
8.  **Thu thập dữ liệu báo cáo:** Thu thập các giá trị hiện tại của đợt thanh toán (Amount This Phase, Waiver, Balance, Interest Charge Actual) vào đối tượng `ResultReport`.
9.  **Tính toán lãi suất ước tính:**
    *   Gọi `getInterestStartDate()` để tải các tham số tính lãi vào `objIns`.
    *   Gọi `getLateDays(receiptdate)` để tính lại số ngày trễ (đã được điều chỉnh bởi logic Case Sign).
    *   Gọi `calc_InterestCharge(receiptdate, amountpay)` để tính toán tiền lãi cuối cùng.
10. **Serialization:** Chuyển đổi đối tượng `ResultReport` thành chuỗi JSON và gán cho `serializedResult`.

### getInterestStartDate()

**Chức năng tổng quát:** Truy xuất các thông số cấu hình lãi suất cần thiết từ các thực thể liên quan và lưu trữ chúng vào đối tượng `objIns` (Installment object).

**Logic nghiệp vụ chi tiết:**

1.  **Truy xuất thực thể:** Truy xuất Option Entry, Payment Scheme, và Interest Rate Master.
2.  **Lấy thông số:** Lấy các thông số sau từ Interest Rate Master và Installment:
    *   `Gracedays` (Số ngày ân hạn).
    *   `Intereststartdatetype` (Loại ngày bắt đầu tính lãi: Due date hoặc Grace period).
    *   `MaxPercent` (Tỷ lệ phần trăm lãi suất trần).
    *   `MaxAmount` (Số tiền lãi trần tối đa).
    *   `InterestPercent` (Tỷ lệ lãi suất áp dụng, được lấy từ trường `bsd_interestchargeper` của Installment, thay vì Master).
3.  **Tính toán ngày bắt đầu tính lãi:** Tính `InterestStarDate` bằng cách cộng `Gracedays + 1` vào `Duedate` (sau khi đã chuyển đổi múi giờ).

### getLateDays(DateTime dateCalculate)

**Chức năng tổng quát:** Tính toán số ngày trễ hạn dựa trên ngày tính toán (ngày nhận tiền) và các quy tắc ân hạn/ngày bắt đầu tính lãi.

**Logic nghiệp vụ chi tiết:**

1.  **Tính toán chênh lệch ngày:** Tính số ngày chênh lệch giữa `dateCalculate` và `Duedate` của đợt thanh toán.
2.  **Áp dụng quy tắc loại ngày bắt đầu tính lãi:**
    *   Nếu `Intereststartdatetype` là Grace Period (100000001): Trừ đi số ngày ân hạn (`Gracedays`) khỏi tổng số ngày trễ.
    *   Nếu là Due Date (hoặc loại khác): Kiểm tra nếu `InterestStarDate` lớn hơn `dateCalculate`, số ngày trễ là 0 (nghĩa là chưa đến ngày bắt đầu tính lãi).
3.  **Đảm bảo không âm:** Nếu số ngày trễ tính ra nhỏ hơn 0, đặt lại thành 0.
4.  Cập nhật `objIns.LateDays` và trả về giá trị.

### RetrieveCurrentUsersSettings(IOrganizationService service)

**Chức năng tổng quát:** Truy vấn cài đặt người dùng hiện tại để lấy mã múi giờ (`timezonecode`).

**Logic nghiệp vụ chi tiết:**

1.  Tạo một `QueryExpression` nhắm vào thực thể `usersettings`.
2.  Lọc theo `systemuserid` bằng `ConditionOperator.EqualUserId` (người dùng đang thực thi plugin).
3.  Lấy mã múi giờ (`timezonecode`) từ kết quả truy vấn.

### RetrieveLocalTimeFromUTCTime(DateTime utcTime)

**Chức năng tổng quát:** Chuyển đổi thời gian UTC (thời gian lưu trữ trong CRM) sang thời gian địa phương của người dùng.

**Logic nghiệp vụ chi tiết:**

1.  Gọi `RetrieveCurrentUsersSettings` để lấy mã múi giờ.
2.  Sử dụng `LocalTimeFromUtcTimeRequest` (một thông điệp SDK) để yêu cầu CRM thực hiện chuyển đổi múi giờ.
3.  Trả về `LocalTime` từ phản hồi.

### check_Data_Setup()

**Chức năng tổng quát:** Kiểm tra xem các trường cấu hình lãi suất trần (`bsd_termsinterestpercentage` hoặc `bsd_toleranceinterestamount`) đã được thiết lập trên Interest Rate Master hay chưa.

**Logic nghiệp vụ chi tiết:**

1.  Truy xuất Option Entry, Payment Scheme, và Interest Rate Master.
2.  Kiểm tra nếu Interest Rate Master không chứa cả hai trường `bsd_termsinterestpercentage` và `bsd_toleranceinterestamount`.
3.  Nếu thiếu, ghi log thông báo "Chưa setup {tỷ lệ / số tiền} tính lãi cho CAP" và trả về `true` (ngụ ý rằng việc setup chưa hoàn chỉnh).

### format_Money(decimal money)

**Chức năng tổng quát:** Định dạng giá trị tiền tệ thành chuỗi có dấu phân cách hàng nghìn và hai chữ số thập phân (ví dụ: 1,234.00).

**Logic nghiệp vụ chi tiết:** Sử dụng `string.Format("{0:#,##0.00}", money)` để thực hiện định dạng.

### get_ec_bsd_dailyinterestrate(Guid projID)

**Chức năng tổng quát:** Truy vấn tỷ lệ lãi suất hàng ngày (`bsd_dailyinterestrate`) cho một dự án cụ thể.

**Logic nghiệp vụ chi tiết:**

1.  Sử dụng FetchXML để truy vấn thực thể `bsd_dailyinterestrate`.
2.  Lọc theo ID dự án (`bsd_project`) và trạng thái hoạt động (`statuscode` = 1).
3.  Sắp xếp theo ngày tạo (`createdon`) giảm dần để lấy bản ghi mới nhất.
4.  Trả về `EntityCollection` chứa các tỷ lệ lãi suất hàng ngày.

### sumWaiverInterest(Entity enOptionEntry)

**Chức năng tổng quát:** Tính tổng số tiền lãi được miễn giảm (`bsd_waiverinterest`) trên tất cả các đợt thanh toán thuộc cùng một Option Entry.

**Logic nghiệp vụ chi tiết:**

1.  Tạo `QueryExpression` cho thực thể `bsd_paymentschemedetail`.
2.  Lọc theo ID của Option Entry và trạng thái hoạt động (`statecode` = 0).
3.  Lặp qua các đợt thanh toán tìm được và cộng dồn giá trị của trường `bsd_waiverinterest`.
4.  Trả về tổng số tiền miễn giảm.

### getInterestSimulation(Entity enIns, DateTime dateCalculate, decimal amountpay)

**Chức năng tổng quát:** Tính toán một giá trị lãi suất mô phỏng đơn giản cho một đợt thanh toán.

**Logic nghiệp vụ chi tiết:**

1.  Tính tỷ lệ lãi suất theo ngày: `interestcharge_percent = objIns.InterestPercent / 100 * objIns.LateDays`.
2.  Tính lãi suất mô phỏng: `interestSimulation = amountpay * interestcharge_percent`.
3.  Trả về `interestSimulation`.

### SumInterestAM_OE_New(Guid OEID, DateTime dateCalculate, decimal amountpay)

**Chức năng tổng quát:** Tính tổng tiền lãi phát sinh (đã tính toán) và tiền lãi ước tính (mô phỏng) của TẤT CẢ các đợt thanh toán trước đợt hiện tại, thuộc cùng một Option Entry.

**Logic nghiệp vụ chi tiết:**

1.  **Xác định thứ tự:** Lấy `bsd_ordernumber` của đợt thanh toán hiện tại.
2.  **Truy vấn các đợt trước:** Sử dụng FetchXML để truy vấn các đợt thanh toán:
    *   Thuộc cùng Option Entry (`bsd_optionentry` = OEID).
    *   Có lãi suất phát sinh (`bsd_interestchargeamount` NOT NULL).
    *   Có thứ tự nhỏ hơn hoặc bằng đợt hiện tại (`bsd_ordernumber` <= bsd_ordernumber).
    *   Trạng thái hoạt động (`statecode` = 0).
3.  **Tính tổng:** Lặp qua các đợt thanh toán:
    *   Cộng dồn `bsd_interestchargeamount` (Lãi phát sinh thực tế) vào `sumAmount`.
    *   Đối với các đợt TRƯỚC đợt hiện tại (kiểm tra `count != entc.Entities.Count`): Nếu đợt đó còn số dư (`bsd_balance` > 0), tính lãi ước tính bằng cách gọi `getInterestSimulation` và cộng dồn vào `sumSimulation`.
4.  **Kết quả:** Trả về tổng của `sumSimulation` và `sumAmount`.

### checkCaseSignAndCalLateDays(DateTime bsd_signedcontractdate, DateTime bsd_signeddadate, DateTime receiptdate, ref int lateDays)

**Chức năng tổng quát:** Áp dụng logic nghiệp vụ phức tạp để xác định ngày bắt đầu tính lãi hiệu quả và điều chỉnh số ngày trễ (`lateDays`) dựa trên các mốc ký hợp đồng (DA/Contract) và cờ "Sign Contract Installment".

**Logic nghiệp vụ chi tiết:**

1.  **Tìm đợt tích cờ:** Truy vấn đợt thanh toán có cờ `bsd_signcontractinstallment` = `true` trong Option Entry hiện tại.
2.  **Xử lý theo `caseSign`:** Sử dụng `switch` dựa trên giá trị `caseSign` (đã được xác định trong `Main`):
    *   **Case 0 (Không tính lãi):** Trả về `false`.
    *   **Case 4 (Không có Sign DA/Contract):** Nếu tìm thấy đợt tích cờ, và ngày đến hạn của đợt hiện tại lớn hơn hoặc bằng ngày đến hạn của đợt tích cờ, tính `lateDays` từ `receiptdate` trừ đi `bsd_duedate` (ngày đến hạn). Ngược lại, không tính lãi.
    *   **Case 1 (Có Sign DA):** Nếu đợt hiện tại trước đợt tích cờ, tính `lateDays` từ `receiptdate` trừ đi `bsd_signeddadate`. Nếu đợt hiện tại sau hoặc bằng đợt tích cờ, tính `lateDays` từ `receiptdate` trừ đi `bsd_duedate`.
    *   **Case 2 (Không có Sign DA, có Sign Contract):** Nếu đợt hiện tại sau hoặc bằng đợt tích cờ, tính `lateDays` từ `receiptdate` trừ đi `bsd_duedate`.
    *   **Case 3 (Có Sign DA và Sign Contract):** Nếu đợt hiện tại trước đợt tích cờ, tính `lateDays` từ `receiptdate` trừ đi `bsd_signeddadate`. Ngược lại, tính từ `receiptdate` trừ đi `bsd_duedate`.
3.  Cập nhật tham số `lateDays` (ref) và trả về `true` nếu việc tính lãi được phép theo logic này.

### calc_InterestCharge(DateTime dateCalculate, decimal amountPay)

**Chức năng tổng quát:** Tính toán số tiền lãi phát sinh cuối cùng, bao gồm việc áp dụng lãi suất hàng ngày (nếu có) và giới hạn mức trần (CAP).

**Logic nghiệp vụ chi tiết:**

1.  **Kiểm tra điều kiện tính lãi:** Nếu `resCheckCaseSign` là `false` (logic Case Sign không cho phép tính lãi), trả về 0.
2.  **Xử lý lãi suất hàng ngày:**
    *   Truy xuất Project từ Option Entry.
    *   Kiểm tra cờ `bsd_dailyinterestchargebank`. Nếu `true`, gọi `get_ec_bsd_dailyinterestrate` để lấy tỷ lệ lãi suất hàng ngày (`d_dailyinterest`).
    *   Cộng dồn lãi suất hàng ngày vào `objIns.InterestPercent`.
3.  **Tính lãi suất thô:** Tính `interestcharge_amount` (tiền lãi thô) bằng công thức: `amountPay * (objIns.InterestPercent / 100 * objIns.LateDays)`.
4.  **Tính tổng lãi lũy kế (Sum Interest):**
    *   Tính tổng tiền lãi được miễn giảm (`sum_bsd_waiverinterest`) bằng `sumWaiverInterest`.
    *   Tính tổng lãi phát sinh/ước tính của các đợt trước (đã trừ miễn giảm) (`sum_Inr_AM`) bằng `SumInterestAM_OE_New`.
    *   Tính tổng lãi tạm tính (bao gồm đợt hiện tại): `sum_temp = sum_Inr_AM + interestcharge_amount`.
5.  **Tính CAP (Mức trần):**
    *   Lấy `bsd_totalamountlessfreight` (Net Selling Price) và `totalamount` từ Option Entry.
    *   Tính CAP dựa trên hai giới hạn: `MaxPercent` (tính trên Total Amount) và `MaxAmount`. CAP là giá trị nhỏ hơn trong hai giới hạn này (hoặc chỉ một nếu giới hạn kia là 0).
6.  **Áp dụng Logic CAP:**
    *   **Nếu `cap <= 0`:** Kiểm tra `check_Data_Setup`. Nếu setup không hoàn chỉnh, tính lãi thô (`interestcharge_amount`). Nếu setup hoàn chỉnh nhưng CAP là 0, trả về 0.
    *   **Nếu `sum_temp > cap`:** Tổng lãi đã vượt trần. Tiền lãi cho đợt hiện tại được tính là phần còn lại của CAP: `result = cap - sum_Inr_AM`. Nếu `sum_Inr_AM` đã vượt CAP, kết quả là 0.
    *   **Nếu `sum_temp <= cap`:** Tổng lãi chưa vượt trần. Tính lãi thô (`interestcharge_amount`).
7.  Trả về kết quả cuối cùng.