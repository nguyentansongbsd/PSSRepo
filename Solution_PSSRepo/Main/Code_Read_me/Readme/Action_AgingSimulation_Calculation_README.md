# Phân tích mã nguồn: Action_AgingSimulation_Calculation.cs

## Tổng quan

Tệp mã nguồn `Action_AgingSimulation_Calculation.cs` triển khai một Plugin (Custom Action) trong môi trường Microsoft Dynamics 365/Power Platform, chịu trách nhiệm thực hiện các bước tính toán phức tạp liên quan đến mô phỏng lãi suất quá hạn (Aging Simulation) và phí lãi suất (Interest Charge) cho các đợt thanh toán (`bsd_paymentschemedetail`) thuộc một Option Entry (`salesorder`).

Plugin này hoạt động theo các bước tuần tự (Bước 01, Bước 03, Bước 05, Bước 06) được điều khiển bởi tham số đầu vào `input01`, thường được sử dụng để tích hợp với Power Automate hoặc các quy trình tự động hóa khác. Logic cốt lõi xoay quanh việc xác định số ngày trễ, tính toán lãi suất mới dựa trên các quy tắc kinh doanh phức tạp (bao gồm cả giới hạn CAP), và tạo các bản ghi chi tiết mô phỏng.

## Chi tiết các Hàm (Functions/Methods)

### Action_AgingSimulation_Calculation.Execute(IServiceProvider serviceProvider)

#### Chức năng tổng quát
Đây là điểm vào chính của Plugin, chịu trách nhiệm khởi tạo các dịch vụ CRM cần thiết và điều phối luồng nghiệp vụ dựa trên tham số đầu vào `input01` để thực hiện các bước tính toán mô phỏng lãi suất.

#### Logic nghiệp vụ chi tiết
1.  **Khởi tạo Dịch vụ:** Khởi tạo `IPluginExecutionContext`, `ITracingService`, `IOrganizationServiceFactory`, và `IOrganizationService`.
2.  **Lấy Tham số Đầu vào:** Lấy 4 tham số đầu vào dạng chuỗi: `input01`, `input02`, `input03`, và `input04`.
3.  **Điều phối Bước (Switching Logic):** Dựa vào giá trị của `input01`:

    *   **Nếu `input01 == "Bước 01"`:**
        *   Tạo một thực thể `bsd_interestsimulation` với ID là `input02`.
        *   Cập nhật trường `bsd_powerautomate` thành `true`.
        *   Thiết lập `output01` là User ID của người dùng thực thi.
        *   Truy vấn thực thể `bsd_configgolive` để tìm URL chạy Power Automate (với điều kiện `bsd_name` là "Aging Simulation Calculation").
        *   Nếu không tìm thấy URL, ném ra ngoại lệ.
        *   Thiết lập `output02` là URL tìm được.

    *   **Nếu `input01 == "Bước 03"`:**
        *   Tạo lại `IOrganizationService` bằng User ID được truyền qua `input04`.
        *   Truy vấn bản ghi `bsd_interestsimulation` (ID là `input02`) để lấy loại mô phỏng (`bsd_type`).
        *   Gọi hàm `getOptionEntrys` để lấy danh sách các Option Entry (`salesorder`) liên quan đến đơn vị (`input03`) và loại mô phỏng (`bsd_type`).
        *   Lặp qua danh sách Option Entry và tạo các bản ghi `bsd_aginginterestsimulationoption` mới, liên kết chúng với Option Entry và bản ghi mô phỏng chính.

    *   **Nếu `input01 == "Bước 05"`:**
        *   Tạo lại `IOrganizationService` bằng User ID được truyền qua `input04`.
        *   Sử dụng FetchXML để truy vấn bản ghi `bsd_aginginterestsimulationoption` có ID là `input03`.
        *   Lặp qua các tùy chọn mô phỏng tìm được và gọi hàm `CreateAgingDetail` cho mỗi tùy chọn để bắt đầu tính toán chi tiết.

    *   **Nếu `input01 == "Bước 06"`:**
        *   Tạo lại `IOrganizationService` bằng User ID được truyền qua `input04`.
        *   Cập nhật bản ghi `bsd_interestsimulation` (ID là `input02`) để đặt `bsd_powerautomate` thành `false` và xóa trường lỗi (`bsd_errorincalculation`).

### RetrieveMultiRecord(IOrganizationService crmservices, string entity, ColumnSet column, string condition, object value)

#### Chức năng tổng quát
Truy vấn nhiều bản ghi từ CRM dựa trên tên thực thể, bộ cột, và một điều kiện lọc đơn giản (Equal) trên một trường cụ thể, đồng thời chỉ lấy các bản ghi đang hoạt động (`statecode = 0`).

#### Logic nghiệp vụ chi tiết
1.  Khởi tạo `QueryExpression` cho thực thể được chỉ định.
2.  Thiết lập `ColumnSet`.
3.  Tạo `FilterExpression` và thêm hai điều kiện:
    *   Điều kiện lọc chính (`condition`, `ConditionOperator.Equal`, `value`).
    *   Điều kiện trạng thái cố định (`statecode`, `ConditionOperator.Equal`, 0).
4.  Thực hiện `service.RetrieveMultiple(q)` và trả về `EntityCollection`.

### CreateAgingDetail(Entity InterestOption)

#### Chức năng tổng quát
Xử lý logic chính để xác định các đợt thanh toán quá hạn và tạo các bản ghi chi tiết mô phỏng lãi suất (`bsd_interestsimulationdetail`) cho một tùy chọn mô phỏng cụ thể.

#### Logic nghiệp vụ chi tiết
1.  **Lấy Thông tin:** Lấy ID của Option Entry (`bsd_optionentry`) và ID của tùy chọn mô phỏng hiện tại (`bsd_aginginterestsimulationoption`).
2.  **Xác định Ngày Tính:** Truy vấn bản ghi `bsd_interestsimulation` liên quan để lấy `bsd_simulationdate` và `bsd_dateofinterestcalculation`. Chuyển đổi các ngày này sang giờ địa phương bằng `RetrieveLocalTimeFromUTCTime`.
3.  **Truy vấn Đợt Thanh toán (Installments):** Xây dựng `QueryExpression` (`q1`) để lấy các đợt thanh toán (`bsd_paymentschemedetail`) dựa trên loại mô phỏng (`bsd_type1`):
    *   **Case 100000000 (Aging Report):** Lọc các đợt có ngày đáo hạn (`bsd_duedate`) trước hoặc bằng ngày tính toán, bao gồm:
        *   Các đợt chưa thanh toán (`statuscode = 100000000`).
        *   Các đợt đã thanh toán (`statuscode = 100000001`) nhưng có phí lãi suất chưa thanh toán (`bsd_interestchargestatus = 100000000`) và số tiền phí lãi suất lớn hơn 0.
    *   **Case 100000001 (Interest Simulation) hoặc Default:** Chỉ lọc các đợt liên quan đến Option Entry hiện tại.
4.  **Tạo Chi tiết Mô phỏng:** Nếu tìm thấy các đợt thanh toán:
    *   Lặp qua từng đợt (`ins`) và gọi hàm `createAgingInterestSimulationDetail` để tính toán và tạo bản ghi chi tiết.
5.  **Cập nhật Sau Tính toán:** Gọi `updateNewInterestAmount` để áp dụng giới hạn CAP cho tổng lãi suất mới.
6.  Gọi `updateAdvantPayment` để cập nhật thông tin thanh toán trước.

### RetrieveLocalTimeFromUTCTime(DateTime utcTime)

#### Chức năng tổng quát
Chuyển đổi một giá trị thời gian từ múi giờ UTC sang múi giờ địa phương của người dùng đang thực thi Plugin.

#### Logic nghiệp vụ chi tiết
1.  Gọi `RetrieveCurrentUsersSettings` để lấy mã múi giờ (`timeZoneCode`) của người dùng hiện tại.
2.  Nếu không tìm thấy mã múi giờ, ném ra ngoại lệ.
3.  Tạo `LocalTimeFromUtcTimeRequest` với mã múi giờ và thời gian UTC đầu vào.
4.  Thực thi yêu cầu bằng `service.Execute(request)` và trả về thời gian địa phương từ phản hồi.

### RetrieveCurrentUsersSettings(IOrganizationService service)

#### Chức năng tổng quát
Truy vấn cài đặt người dùng hiện tại để lấy mã múi giờ (`timezonecode`).

#### Logic nghiệp vụ chi tiết
1.  Tạo `QueryExpression` trên thực thể `usersettings`.
2.  Lọc theo điều kiện `systemuserid` bằng `ConditionOperator.EqualUserId` (người dùng hiện tại).
3.  Lấy các cột `localeid` và `timezonecode`.
4.  Thực hiện truy vấn và trả về giá trị `timezonecode` từ bản ghi đầu tiên.

### createAgingInterestSimulationDetail(...)

#### Chức năng tổng quát
Tính toán chi tiết lãi suất quá hạn (số ngày trễ, lãi suất mới, tổng phí lãi suất) cho một đợt thanh toán (`ins`) và tạo bản ghi `bsd_interestsimulationdetail`.

#### Logic nghiệp vụ chi tiết
1.  **Kiểm tra Dữ liệu:** Đảm bảo Option Entry (`oe`) có `bsd_paymentscheme` và Payment Scheme có `bsd_interestratemaster`.
2.  **Tính Lãi suất Hiện tại:**
    *   Gọi `CalculateInterestNotPaid` để tính số tiền trễ chưa thanh toán.
    *   Gọi `CalculateInterestAmount` để tính số tiền lãi còn lại của đợt.
3.  **Tính Ngày Trễ và Phí Lãi suất:**
    *   Gọi `Calculate_Interest` để xác định số ngày trễ (`lateDays`) và phí lãi suất cơ bản (`decInterestCharge`) dựa trên ngày tính toán (`dateCalculate`).
4.  **Lấy Cấu hình Lãi suất:** Truy vấn `bsd_interestratemaster` để lấy các thông số như `bsd_intereststartdatetype`, `bsd_gracedays`, và `bsd_termsinterestpercentage`.
5.  **Khởi tạo Chi tiết:** Tạo bản ghi `bsd_interestsimulationdetail` và điền các thông tin cơ bản (Tên, Liên kết, Ngày mô phỏng, Số tiền trả góp, Số tiền đã trả, Số tiền còn lại).
6.  **Logic Tính toán Lãi suất Mới:**
    *   Chỉ tính toán nếu có lãi suất chưa thanh toán (`interest_NotPaid > 0`) hoặc đợt thanh toán chưa hoàn thành (`statuscode = 100000000`) hoặc đợt đã trả nhưng lãi suất chưa tính (`bolCheckPaid`).
    *   Lấy ngày đáo hạn (`duedate`) và ngày bắt đầu tính lãi (`InterestStarDate1`).
    *   Tính số ngày quá hạn (`bsd_numberofdaysdue`).
    *   Nếu ngày bắt đầu tính lãi nhỏ hơn ngày tính toán, gọi `CalculateNewInterest` để tính lãi suất mới (`interest_New`) dựa trên số dư còn lại.
    *   Áp dụng giới hạn: `decnewinterestamount` được đặt là giá trị nhỏ hơn giữa `interest_New` và `decInterestCharge` (trừ khi `bolCheckPaid` là true, khi đó lãi suất mới là 0).
    *   Cập nhật các trường liên quan đến lãi suất (số ngày trễ, phần trăm lãi suất, nhóm aging, số tiền lãi mới, tổng phí lãi suất).
7.  **Tạo Bản ghi:** Thực hiện `service.Create(ISDetail)`.

### getOptionEntrys(IOrganizationService crmservices, string idUnit, int bsd_type)

#### Chức năng tổng quát
Truy vấn danh sách các Option Entry (`salesorder`) liên quan đến một đơn vị cụ thể, loại trừ các trạng thái không mong muốn.

#### Logic nghiệp vụ chi tiết
1.  Xây dựng FetchXML để truy vấn thực thể `salesorder`.
2.  Lọc theo ID đơn vị (`bsd_unitnumber` = `idUnit`).
3.  Loại trừ các trạng thái (`statuscode`) 100000006 và 100000007.
4.  Nếu loại mô phỏng (`bsd_type`) là 100000002, thêm điều kiện loại trừ các Option Entry sắp bị chấm dứt (`bsd_tobeterminated != 1`).
5.  Thực hiện `service.RetrieveMultiple` và trả về kết quả.

### getInterestCap(Entity enOptionEntry)

#### Chức năng tổng quát
Tính toán giới hạn tối đa (CAP) của tổng lãi suất có thể tính cho một Option Entry dựa trên cấu hình trong Interest Rate Master.

#### Logic nghiệp vụ chi tiết
1.  Lấy tổng số tiền của Option Entry (`totalamount`).
2.  Truy vấn Payment Scheme và Interest Rate Master liên quan.
3.  Lấy `bsd_toleranceinterestamount` (số tiền giới hạn cố định) và `bsd_toleranceinterestpercentage` (phần trăm giới hạn).
4.  Tính số tiền giới hạn dựa trên phần trăm: `amountcalbypercent = totalamount * bsd_toleranceinterestpercentage / 100`.
5.  Giới hạn CAP (`lim`) được xác định là giá trị nhỏ nhất giữa `bsd_toleranceinterestamount` và `amountcalbypercent` (nếu cả hai đều tồn tại và lớn hơn 0). Nếu chỉ có một giá trị tồn tại, CAP là giá trị đó.
6.  Trả về giá trị CAP (hoặc -100599 nếu không có giới hạn nào được thiết lập).

### updateNewInterestAmount(Entity enOptionEntry, Entity enInterestSimulateOption, int reportype)

#### Chức năng tổng quát
Điều chỉnh số tiền lãi mới (`bsd_newinterestamount`) của các bản ghi chi tiết mô phỏng để đảm bảo tổng lãi suất không vượt quá giới hạn CAP đã thiết lập.

#### Logic nghiệp vụ chi tiết
1.  Lấy giới hạn CAP bằng cách gọi `getInterestCap`. Nếu CAP không được thiết lập, bỏ qua bước điều chỉnh CAP.
2.  Truy vấn tất cả các bản ghi `bsd_interestsimulationdetail` liên quan đến Option Entry và tùy chọn mô phỏng hiện tại, sắp xếp theo thứ tự đợt thanh toán (`bsd_ordernumber`).
3.  **Áp dụng CAP:**
    *   Khởi tạo `sumInterestAmount` (tổng lãi suất trả góp đã tính).
    *   Lặp qua các bản ghi chi tiết:
        *   Nếu tổng lãi suất hiện tại (`sumInterestAmount`) vẫn nhỏ hơn CAP:
            *   Tính tổng tiềm năng (`total = sumInterestAmount + bsd_newinterestamount`).
            *   Nếu `total` vượt quá CAP, điều chỉnh `bsd_newinterestamount` của đợt hiện tại để bằng `CAP - sumInterestAmount`. Cập nhật bản ghi.
            *   Nếu không vượt quá, cộng `bsd_newinterestamount` vào `sumInterestAmount`.
        *   Nếu `sumInterestAmount` đã đạt hoặc vượt CAP: Đặt `bsd_newinterestamount` của đợt hiện tại bằng 0 và cập nhật bản ghi.
4.  **Xử lý Aging Report (reportype = 100000000):** Sau khi điều chỉnh CAP, truy vấn lại danh sách chi tiết. Nếu một bản ghi chi tiết không có cả lãi suất mới (`bsd_newinterestamount`) và lãi suất trả góp (`bsd_interestamountinstallment`), bản ghi đó sẽ bị xóa.

### CalculateNewInterest(decimal balance, int lateDays, decimal interestPercent)

#### Chức năng tổng quát
Tính toán lãi suất mới dựa trên số dư, số ngày trễ và phần trăm lãi suất.

#### Logic nghiệp vụ chi tiết
Thực hiện công thức tính lãi suất đơn giản: `interest = balance * lateDays * interestPercent / 100`.

### CalculateInterestNotPaid(Entity ins)

#### Chức năng tổng quát
Tính số tiền còn lại của đợt thanh toán (có thể là tiền gốc hoặc tiền lãi) chưa được thanh toán, sau khi trừ đi số tiền đã trả và số tiền được miễn.

#### Logic nghiệp vụ chi tiết
Tính toán: `interestamount - interestamountpaid - waiverinterest`.

### CalculateInterestAmount(Entity ins)

#### Chức năng tổng quát
Tính số tiền phí lãi suất còn lại chưa được thanh toán cho đợt hiện tại.

#### Logic nghiệp vụ chi tiết
Tính toán: `interestamount - interestamountpaid - waiverinterest` (sử dụng các trường liên quan đến lãi suất: `bsd_interestchargeamount`, `bsd_interestwaspaid`, `bsd_waiverinterest`).

### CheckGroupAging(decimal i)

#### Chức năng tổng quát
Phân loại số ngày trễ (`i`) vào các nhóm Aging (quá hạn) tiêu chuẩn.

#### Logic nghiệp vụ chi tiết
Sử dụng logic `if/else if` để trả về mã Option Set tương ứng với các nhóm:
*   <= 15 ngày: 100000000
*   16 - 30 ngày: 100000001
*   31 - 60 ngày: 100000002
*   61 - 90 ngày: 100000003
*   > 90 ngày: 100000004

### CalSum_AdvancePayment(string OptionID)

#### Chức năng tổng quát
Tính tổng số tiền còn lại của các khoản thanh toán trước (`bsd_advancepayment`) cho một Option Entry.

#### Logic nghiệp vụ chi tiết
1.  Xây dựng FetchXML sử dụng hàm tổng hợp (`aggregate='true'`).
2.  Tính tổng (`sum`) của trường `bsd_remainingamount` trên thực thể `bsd_advancepayment`.
3.  Lọc theo Option Entry ID và trạng thái (`statuscode = 100000000`).
4.  Trả về `EntityCollection` chứa kết quả tổng hợp.

### updateAdvantPayment(Entity oe, string aginginterestsimulationoption)

#### Chức năng tổng quát
Cập nhật số tiền thanh toán trước (Advance Payment) vào bản ghi chi tiết mô phỏng lãi suất đầu tiên của Option Entry.

#### Logic nghiệp vụ chi tiết
1.  Gọi `CalSum_AdvancePayment` để lấy tổng số tiền thanh toán trước (`AdvPayAmt`).
2.  Xây dựng FetchXML để truy vấn các bản ghi `bsd_interestsimulationdetail` liên quan đến Option Entry (`oe`) và tùy chọn mô phỏng hiện tại (`aginginterestsimulationoption`).
3.  Sắp xếp kết quả theo `bsd_ordernumber` giảm dần (mặc dù logic sau đó chỉ lấy bản ghi đầu tiên).
4.  Nếu tìm thấy bản ghi chi tiết, cập nhật trường `bsd_advancepayment` bằng `AdvPayAmt` trên bản ghi đầu tiên.

### getInterestStartDate(Entity enInstallment, Installment objIns)

#### Chức năng tổng quát
Truy xuất các thông số cấu hình lãi suất (Grace days, loại ngày bắt đầu tính lãi, giới hạn CAP) từ Option Entry, Payment Scheme và Interest Rate Master, sau đó lưu chúng vào đối tượng `Installment`.

#### Logic nghiệp vụ chi tiết
1.  Truy vấn Option Entry, Payment Scheme và Interest Rate Master liên quan đến đợt thanh toán.
2.  Lấy các giá trị: `Gracedays`, `Intereststartdatetype`, `MaxPercent`, `MaxAmount`, `InterestPercent`, và `Duedate`.
3.  Chuyển đổi `Duedate` sang giờ địa phương.
4.  Tính toán `InterestStarDate` (Ngày đáo hạn + Grace days + 1).
5.  Lưu tất cả các giá trị này vào đối tượng `objIns`.

### getLateDays(DateTime dateCalculate, Installment objIns)

#### Chức năng tổng quát
Tính toán số ngày trễ thực tế của đợt thanh toán, có tính đến Grace Period và các quy tắc đặc biệt liên quan đến ngày ký hợp đồng.

#### Logic nghiệp vụ chi tiết
1.  Tính số ngày trễ cơ bản: `lateDays = dateCalculate - Duedate`.
2.  Gọi `getViTriDotSightContract` để xác định vị trí đợt ký hợp đồng.
3.  **Xử lý Logic Ký Hợp đồng:** Nếu đợt thanh toán hiện tại nằm sau hoặc bằng đợt ký hợp đồng, và có ngày ký hợp đồng (`bsd_signedcontractdate`), hoặc nếu đợt hiện tại nằm trước đợt ký hợp đồng và có ngày ký DA (`bsd_signeddadate`), tính toán số ngày trễ thay thế (`numberOfDays2`).
4.  **Áp dụng Loại Ngày Bắt đầu Tính Lãi:**
    *   Nếu `Intereststartdatetype == 100000001` (Grace Period): Trừ `Gracedays` khỏi `lateDays`. Đảm bảo `lateDays` không âm.
    *   Nếu `Intereststartdatetype == 100000000` (Due Date): Đảm bảo `lateDays` không âm.
5.  Nếu `numberOfDays2` hợp lệ, giới hạn `lateDays` bằng `numberOfDays2` (lấy giá trị nhỏ hơn).
6.  Nếu `InterestStarDate` lớn hơn `dateCalculate`, đặt `lateDays` bằng 0.
7.  Lưu kết quả vào `objIns.LateDays` và trả về.

### getViTriDotSightContract(Guid idOE)

#### Chức năng tổng quát
Tìm số thứ tự đợt thanh toán (`bsd_ordernumber`) được đánh dấu là đợt ký hợp đồng (`bsd_signcontractinstallment`).

#### Logic nghiệp vụ chi tiết
1.  Sử dụng FetchXML để truy vấn `bsd_paymentschemedetail`.
2.  Lọc theo Option Entry ID và điều kiện `bsd_signcontractinstallment = 1`.
3.  Lặp qua kết quả và trả về `bsd_ordernumber` của đợt đó. Nếu không tìm thấy, trả về -1.

### calc_InterestCharge(DateTime dateCalculate, decimal amountPay, Entity enInstallment, Installment objIns, ref decimal interestMasterPercent)

#### Chức năng tổng quát
Tính toán phí lãi suất cuối cùng cho đợt thanh toán hiện tại, bao gồm việc áp dụng lãi suất hàng ngày của ngân hàng và giới hạn CAP.

#### Logic nghiệp vụ chi tiết
1.  **Lãi suất Hàng ngày:** Truy vấn thông tin dự án để kiểm tra `bsd_dailyinterestchargebank`. Nếu `true`, gọi `get_ec_bsd_dailyinterestrate` để lấy lãi suất hàng ngày (`d_dailyinterest`) và cộng vào `objIns.InterestPercent`.
2.  Tính phí lãi suất cơ bản: `interestcharge_amount = amountPay * (objIns.InterestPercent / 100 * objIns.LateDays)`.
3.  **Tính Tổng Lãi suất Tích lũy:**
    *   Gọi `sumWaiverInterest` để lấy tổng lãi suất được miễn.
    *   Gọi `SumInterestAM_OE_New` để tính tổng lãi suất đã phát sinh (bao gồm cả mô phỏng) của các đợt trước và đợt hiện tại (trước khi áp dụng CAP).
4.  **Xác định CAP:** Tính toán giới hạn CAP (`cap`) dựa trên `bsd_totalamountlessfreight` hoặc `totalamount` của Option Entry và các thông số `MaxPercent`/`MaxAmount` từ Interest Rate Master.
5.  **Áp dụng Giới hạn CAP:**
    *   Nếu `cap <= 0`: Trả về `interestcharge_amount` (nếu `check_Data_Setup` là true, ngược lại là 0).
    *   Nếu tổng lãi suất tích lũy (`sum_temp`) vượt quá CAP: Lãi suất cho đợt hiện tại được giới hạn là `cap - sum_Inr_AM` (nếu `cap > sum_Inr_AM`), hoặc 0 (nếu đã vượt quá).
    *   Nếu `sum_temp` nhỏ hơn hoặc bằng CAP: Trả về `interestcharge_amount`.

### sumWaiverInterest(Entity enOptionEntry)

#### Chức năng tổng quát
Tính tổng số tiền lãi được miễn (`bsd_waiverinterest`) trên tất cả các đợt thanh toán của một Option Entry.

#### Logic nghiệp vụ chi tiết
1.  Tạo `QueryExpression` trên `bsd_paymentschemedetail`.
2.  Lọc theo Option Entry ID và `statecode = 0`.
3.  Lặp qua các đợt thanh toán và tính tổng giá trị của trường `bsd_waiverinterest`.

### getInterestSimulation(Entity enIns, DateTime dateCalculate, decimal amountpay, Installment objIns)

#### Chức năng tổng quát
Tính toán lãi suất mô phỏng cho một đợt thanh toán dựa trên số ngày trễ và phần trăm lãi suất.

#### Logic nghiệp vụ chi tiết
1.  Gọi `getInterestStartDate` và `getLateDays` để xác định các thông số tính toán.
2.  Tính lãi suất mô phỏng: `interestSimulation = amountpay * (objIns.InterestPercent / 100 * objIns.LateDays)`.

### SumInterestAM_OE_New(Guid OEID, DateTime dateCalculate, decimal amountpay, Entity enInstallment, Installment objIns)

#### Chức năng tổng quát
Tính tổng lãi suất phát sinh (đã tính và mô phỏng) của các đợt thanh toán trước đợt hiện tại.

#### Logic nghiệp vụ chi tiết
1.  Truy vấn các đợt thanh toán (`bsd_paymentschemedetail`) có số thứ tự nhỏ hơn hoặc bằng đợt hiện tại (`bsd_ordernumber`).
2.  Lặp qua các đợt:
    *   Cộng dồn `bsd_interestchargeamount` (lãi suất đã tính).
    *   Nếu đợt đó không phải là đợt cuối cùng trong danh sách truy vấn và có số dư (`bsd_balance > 0`), gọi `getInterestSimulation` để tính lãi suất mô phỏng cho đợt đó và cộng dồn vào tổng mô phỏng.
3.  Trả về tổng lãi suất (đã tính + mô phỏng).

### check_Data_Setup(Entity enInstallment)

#### Chức năng tổng quát
Kiểm tra xem Interest Rate Master có thiếu các trường cấu hình quan trọng hay không.

#### Logic nghiệp vụ chi tiết
1.  Truy vấn Option Entry, Payment Scheme và Interest Rate Master.
2.  Kiểm tra nếu Interest Rate Master không chứa cả hai trường `bsd_termsinterestpercentage` và `bsd_toleranceinterestamount`, hàm trả về `true`. Ngược lại, trả về `false`.

### get_ec_bsd_dailyinterestrate(Guid projID)

#### Chức năng tổng quát
Truy vấn lãi suất hàng ngày của ngân hàng (`bsd_dailyinterestrate`) cho một dự án cụ thể.

#### Logic nghiệp vụ chi tiết
1.  Sử dụng FetchXML để truy vấn `bsd_dailyinterestrate`.
2.  Lọc theo Project ID và trạng thái hoạt động (`statuscode = 1`).
3.  Sắp xếp theo ngày tạo (`createdon`) giảm dần để lấy bản ghi mới nhất.

### Calculate_Interest(string installmentid, string stramountpay, string receiptdateimport, Installment objIns, ref int lateDays, ref decimal interestMasterPercent)

#### Chức năng tổng quát
Hàm tổng hợp để tính toán số ngày trễ và phí lãi suất cho một đợt thanh toán dựa trên ngày tính toán.

#### Logic nghiệp vụ chi tiết
1.  Chuyển đổi chuỗi đầu vào thành `decimal` (`amountpay`) và `DateTime` (`receiptdate`).
2.  Chuyển đổi `receiptdate` sang giờ địa phương.
3.  Truy vấn đợt thanh toán.
4.  Gọi `getInterestStartDate` để lấy cấu hình.
5.  Gọi `getLateDays` để tính số ngày trễ thực tế và cập nhật `lateDays`.
6.  Gọi `calc_InterestCharge` để tính phí lãi suất cuối cùng và cập nhật `interestMasterPercent`.

## Lớp Hỗ trợ (Helper Class)

### Installment

#### Chức năng tổng quát
Lớp dữ liệu (Data Transfer Object - DTO) được sử dụng để lưu trữ tạm thời các thông số quan trọng liên quan đến việc tính toán lãi suất cho một đợt thanh toán cụ thể, giúp truyền dữ liệu qua lại giữa các hàm tính toán.

#### Chi tiết các thuộc tính
*   `InterestStarDate`: Ngày bắt đầu tính lãi suất.
*   `Intereststartdatetype`: Loại ngày bắt đầu tính lãi (Due date hoặc Grace period).
*   `Gracedays`: Số ngày ân hạn.
*   `LateDays`: Số ngày trễ thực tế đã tính.
*   `orderNumber`: Số thứ tự của đợt thanh toán.
*   `idOE`: ID của Option Entry liên quan.
*   `MaxPercent`: Phần trăm giới hạn tối đa (CAP).
*   `MaxAmount`: Số tiền giới hạn tối đa (CAP).
*   `InterestPercent`: Phần trăm lãi suất cơ bản.
*   `InterestCharge`: Phí lãi suất cuối cùng đã tính.
*   `Duedate`: Ngày đáo hạn của đợt thanh toán.