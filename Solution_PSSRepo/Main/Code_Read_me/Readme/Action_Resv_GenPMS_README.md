# Phân tích mã nguồn: Action_Resv_GenPMS.cs

Tệp mã nguồn `Action_Resv_GenPMS.cs` là một Plugin (hoặc Custom Action) được thiết kế để chạy trong môi trường Microsoft Dynamics 365/XRM. Chức năng chính của nó là tự động tạo ra các chi tiết đợt thanh toán (Payment Scheme Details - PMS DTL) dựa trên Kế hoạch thanh toán (Payment Scheme) đã chọn trên một thực thể Đặt chỗ/Báo giá (Quote/Reservation).

Logic nghiệp vụ trong tệp này rất phức tạp, liên quan đến việc tính toán các khoản tiền dựa trên giá bán ròng, thuế VAT, giá trị đất được khấu trừ, và các loại phí quản lý/bảo trì, đồng thời xác định ngày đến hạn (Due Date) theo các quy tắc cố định hoặc tự động.

## Tổng quan

Plugin `Action_Resv_GenPMS` chịu trách nhiệm khởi tạo và cập nhật các đợt thanh toán chi tiết (`bsd_paymentschemedetail`) cho một giao dịch Đặt chỗ (`quote`). Quá trình này bao gồm việc xác thực dữ liệu đầu vào, tính toán số tiền chính xác cho từng đợt (có tính đến các yếu tố thuế và khấu trừ giá trị đất), xác định ngày đến hạn, và đảm bảo tổng số tiền của các đợt thanh toán khớp với tổng giá trị của giao dịch.

## Chi tiết các Hàm (Functions/Methods)

### IPlugin.Execute(IServiceProvider serviceProvider)

**Chức năng tổng quát:** Điểm vào chính của Plugin. Hàm này khởi tạo các dịch vụ cần thiết, xác thực dữ liệu đầu vào trên thực thể Đặt chỗ (`quote`) và điều phối quá trình tạo các đợt thanh toán.

**Logic nghiệp vụ chi tiết:**
1.  **Khởi tạo:** Lấy `IPluginExecutionContext`, `ITracingService`, `IOrganizationServiceFactory`, và `IOrganizationService`.
2.  **Kiểm tra Target:** Đảm bảo thực thể mục tiêu (`Target`) là `quote`.
3.  **Truy xuất dữ liệu Quote:** Lấy các trường quan trọng của Quote như `statuscode`, `totalamount`, `bsd_paymentscheme`, `bsd_totalamountlessfreight`, `bsd_landvaluededuction`, `bsd_freightamount`, `bsd_managementfee`, và các trường ngày tháng liên quan.
4.  **Xác thực dữ liệu:** Thực hiện nhiều kiểm tra bắt buộc:
    *   `statuscode` không được rỗng.
    *   Nếu Quote không phải là loại đặc biệt (`bsd_quotecodesams`), phải có `customerid`, `createdon`, `bsd_paymentscheme`, `bsd_totalamountlessfreight`, `totalamount`, `totaltax`, và `bsd_projectid`.
5.  **Xác định Ngày Bắt đầu:** Xác định ngày cơ sở (`date`) để tính toán Due Date. Ưu tiên `bsd_ngaydatcoc` (ngày đặt cọc), sau đó là `bsd_reservationtime` nếu trạng thái là Reservation (100000000).
6.  **Tạo Kế hoạch Thanh toán:** Gọi hàm `GenPaymentScheme(ref QO, ref date, traceService)` để tạo các chi tiết đợt thanh toán.
7.  **Kiểm tra Due Date (TDTT):** Sau khi tạo, truy vấn tất cả các đợt thanh toán và kiểm tra xem Due Date có bị đảo ngược không (Due Date của đợt sau phải lớn hơn hoặc bằng đợt trước). Nếu không, ném ra lỗi.
8.  **Cập nhật Số tiền Còn lại:** Gọi `updateRemainMoney(QO)` để điều chỉnh số tiền thừa/thiếu vào đợt thanh toán áp chót (thường là đợt Bàn giao).
9.  **Kiểm tra Tiền Đặt cọc:** Truy vấn đợt 1 và kiểm tra xem `bsd_depositfee` trên Quote có lớn hơn `bsd_amountofthisphase` của đợt 1 hay không. Nếu có, ném ra lỗi.
10. **Cập nhật Quote:** Đặt trường `bsd_existinstallment` thành `true` trên Quote.

### private void updateRemainMoney(Entity quote)

**Chức năng tổng quát:** Điều chỉnh số tiền còn lại (thừa hoặc thiếu) sau khi tính toán các đợt thanh toán theo tỷ lệ phần trăm, đảm bảo tổng số tiền của các đợt khớp với tổng giá trị giao dịch.

**Logic nghiệp vụ chi tiết:**
1.  **Truy vấn Master:** Lấy các chi tiết đợt thanh toán Master (từ Payment Scheme) để xác định các thuộc tính như `bsd_duedatecalculatingmethod` và `bsd_lastinstallment`.
2.  **Truy vấn Installment đã tạo:** Lấy tất cả các chi tiết đợt thanh toán đã tạo cho Quote này.
3.  **Tính Tổng:** Tính tổng số tiền (`sum`) của tất cả các đợt thanh toán đã tạo.
4.  **Tính Tổng Phí Thực tế (`genfee`):** Xác định tổng giá trị cần phải thu, thường là `totalamount` trừ `bsd_freightamount`, với các kiểm tra đặc biệt dựa trên `bsd_projectid` (một số dự án có thể không trừ `bsd_freightamount`).
5.  **Tính Số tiền Còn lại (`remain`):** `remain = genfee - sum`.
6.  **Tìm Đợt Bàn giao (Handover):** Tìm đợt thanh toán có phương thức tính Due Date là Estimate Handover (100000002).
7.  **Điều chỉnh:**
    *   Nếu tìm thấy đợt Bàn giao (`flag = true`): Lấy số tiền hiện tại của đợt Bàn giao, cộng thêm `remain`. Cập nhật số tiền mới (`fee`) vào đợt Bàn giao, bao gồm cả các trường chuyển đổi số tiền sang chữ (VN/EN).
    *   Nếu không tìm thấy đợt Bàn giao (`flag = false`): Truy vấn đợt thanh toán *áp chót* (đợt không phải cuối cùng, có `bsd_lastinstallment = 0`) và cộng số tiền `remain` vào đợt đó.
8.  **Cập nhật Thuộc tính Master:** Nếu chỉ có 1 hoặc 2 đợt thanh toán, cập nhật lại các thuộc tính như `bsd_duedatecalculatingmethod` và `bsd_lastinstallment` từ Master sang các đợt đã tạo.

### private void GenPaymentScheme(ref Entity QO, ref DateTime date, ITracingService trac)

**Chức năng tổng quát:** Thiết lập các tham số tài chính và lặp qua các chi tiết Kế hoạch thanh toán Master để tạo ra các đợt thanh toán chi tiết cho Reservation.

**Logic nghiệp vụ chi tiết:**
1.  **Thiết lập Tham số Tài chính:**
    *   Lấy `priceperunit` và `productId` từ `quotedetail`.
    *   Lấy `landValue` từ trường `bsd_landvaluededuction` trên Quote.
    *   Lấy `tax` (tỷ lệ thuế) từ `bsd_taxcode`.
    *   Tính `TaxAmount` (giá trước VAT - giá trị đất) * 10%.
    *   Xác định `totalAmount` (tổng giá trị giao dịch cần tính toán).
2.  **Lấy thông tin Khách hàng:** Lấy `bsd_localization` (Nội địa/Nước ngoài) của khách hàng.
3.  **Truy vấn Master Installment:** Lấy tất cả các chi tiết đợt thanh toán Master (`bsd_paymentschemedetail`) liên quan đến `bsd_paymentscheme` của Quote.
4.  **Kiểm tra Master:** Nếu không có chi tiết Master nào, ném ra lỗi.
5.  **Lấy thông tin Đơn vị (Unit) và Dự án (Project):** Lấy diện tích thực tế (`bsd_actualarea` hoặc `bsd_netsaleablearea`) từ Unit và `bsd_managementamount` từ Project (để tính phí quản lý).
6.  **Lấy thông tin Lãi suất:** Truy vấn `bsd_interestratemaster` để lấy `graceDays`, `eda` (lãi suất EDA), và `spa` (lãi suất SPA).
7.  **Lấy Word Template:** Truy vấn các định nghĩa Word Template (VN và EN) để điền vào các trường văn bản của đợt thanh toán.
8.  **Lặp và Tạo Đợt Thanh toán:** Lặp qua từng chi tiết Master:
    *   Xác định các cờ như `f_ESmaintenancefees`, `f_ESmanagementfee`, `f_signcontractinstallment`, `f_installmentForEDA`.
    *   Lấy `percent` (tỷ lệ phần trăm thanh toán).
    *   Kiểm tra `i_dueCalMethod`:
        *   Nếu là Fixed Date (100000000), Estimate Handover (100000002), hoặc Last Installment: Gọi `CreatePaymentPhase_fixDate`.
        *   Nếu là Auto Date (100000001):
            *   Xác định loại thanh toán (`payment_type`: default/month/times).
            *   Nếu `payment_type` là default/month: Gọi `CreatePaymentPhase` một lần.
            *   Nếu `payment_type` là times: Lặp lại việc gọi `CreatePaymentPhase` theo số lần (`number`) quy định, có tính đến `bsd_nextdaysofendphase` cho đợt cuối cùng trong chuỗi lặp.

### private void CreatePaymentPhase(Entity PM, ref int orderNumber, Entity en, Entity QO, ..., bool f_installmentForEDA)

**Chức năng tổng quát:** Tạo một chi tiết đợt thanh toán mới với logic tính toán Due Date tự động (Auto Date).

**Logic nghiệp vụ chi tiết:**
1.  **Tính Due Date Tự động:**
    *   Dựa vào `bsd_nextperiodtype` (Month hoặc Day) và các trường `bsd_numberofnextmonth` hoặc `bsd_numberofnextdays`, cập nhật ngày `date` (ngày đến hạn của đợt trước hoặc ngày bắt đầu) để tính ra Due Date mới.
    *   Nếu có `i_paymentdatemonthly` (ngày cố định trong tháng), điều chỉnh ngày trong tháng của `date`.
2.  **Khởi tạo Entity:** Tạo entity `bsd_paymentschemedetail` mới, gán `orderNumber`, tên, mã, liên kết với Quote và Payment Scheme.
3.  **Gán Số tiền:** Tính `tmpamount` = `percent` * `reservationAmount` / 100.
4.  **Xử lý Đặt cọc:** Nếu là đợt 1 (`orderNumber == 1`), gán `bsd_depositamount` từ Quote.
5.  **Áp dụng Khấu trừ Giá trị Đất (Land Value Deduction):**
    *   Nếu là đợt 1 và tổng số đợt là 1, HOẶC là đợt 2 và tổng số đợt là 2, tính `d_es_LandPercent` (`tax * landValue / 100`).
    *   Trừ `d_es_LandPercent` khỏi `bsd_amountofthisphase` (nếu số tiền đợt này lớn hơn khoản khấu trừ).
6.  **Gán Phí:** Gán `bsd_managementamount` và `bsd_maintenanceamount` nếu các cờ tương ứng là `true`.
7.  **Gán Lãi suất và Grace Days:** Đặt `bsd_interestchargeper` (EDA/SPA) và `bsd_gracedays`.
8.  **Gán Word Template:** Gọi `SetTextWordTemplate` và `SetTextWordTemplate_EN` để điền các trường văn bản.
9.  **Tạo Record:** Thực hiện `service.Create(tmp)`.

### private void CreatePaymentPhase_fixDate(Entity PM, ref int orderNumber, ..., bool f_installmentForEDA)

**Chức năng tổng quát:** Tạo một chi tiết đợt thanh toán mới với logic tính toán Due Date cố định hoặc dựa trên ngày Bàn giao ước tính.

**Logic nghiệp vụ chi tiết:**
1.  **Xác định Due Date:**
    *   Nếu không phải đợt cuối và không phải đợt Bàn giao, lấy `bsd_fixeddate` từ Master để đặt làm `bsd_duedate`.
2.  **Khởi tạo Entity:** Tương tự như `CreatePaymentPhase`.
3.  **Đợt 1 (`orderNumber == 1`):**
    *   Gán `bsd_depositamount`.
    *   Tính `tmpamount` theo phần trăm.
    *   Áp dụng Land Value Deduction nếu tổng số đợt là 1.
    *   Đặt `bsd_duedatecalculatingmethod` là Fixed (100000000).
4.  **Các Đợt Khác (Không phải Đợt 1):**
    *   **Nếu là Bàn giao (`f_es == true`):**
        *   Tính `d_es_LandPercent` (`tax * landValue / 100`).
        *   Áp dụng khấu trừ này vào `bsd_amountofthisphase`.
        *   Đặt `bsd_duedate` là `d_esDate` (Ngày Bàn giao ước tính).
        *   Đặt `bsd_duedatecalculatingmethod` là Estimate Handover (100000002).
    *   **Nếu là Fixed Date (`f_es == false`):**
        *   Đặt `bsd_duedate` là `bsd_fixeddate`.
        *   Áp dụng Land Value Deduction nếu tổng số đợt là 2 và đây là đợt 2.
    *   **Nếu là Đợt Cuối (`f_last == true`):**
        *   Đặt `bsd_lastinstallment = true`.
        *   *Lưu ý:* Sau khi tạo, nếu `f_last_ES` (cờ cho biết Land Value Deduction đã được áp dụng) là true, hàm sẽ truy vấn lại tổng số tiền đã tạo và điều chỉnh số tiền của đợt cuối cùng để bù đắp cho số tiền còn lại (dựa trên công thức `tmpamount + totalTMP - bsd_maintenancefees - d_SumTmp`).

### private decimal GetTax(EntityReference taxcode)

**Chức năng tổng quát:** Truy xuất giá trị tỷ lệ thuế (tax value) từ thực thể Tax Code.

**Logic nghiệp vụ chi tiết:**
1.  Truy vấn thực thể Tax Code bằng `taxcode.Id`.
2.  Kiểm tra và trả về giá trị của trường `bsd_value`. Nếu không có, ném ra lỗi.

### private decimal GetProductPrice(Guid quoteId, out EntityReference productId)

**Chức năng tổng quát:** Lấy giá trên mỗi đơn vị (`priceperunit`) và tham chiếu sản phẩm (`productid`) từ chi tiết Đặt chỗ (`quotedetail`).

**Logic nghiệp vụ chi tiết:**
1.  Truy vấn `quotedetail` liên quan đến `quoteId`.
2.  Lấy bản ghi đầu tiên (giả định chỉ có một sản phẩm/unit).
3.  Kiểm tra và trả về giá trị `priceperunit` (Money). Nếu không có, ném ra lỗi.
4.  Gán `productid` cho tham số `out`.

### private DateTime get_EstimatehandoverDate(Entity OE)

**Chức năng tổng quát:** Lấy ngày bàn giao ước tính (Estimate Handover Date) từ Product (Unit) liên quan đến Quote.

**Logic nghiệp vụ chi tiết:**
1.  Sử dụng FetchXML/QueryExpression để liên kết Quote với Product thông qua Quotedetail.
2.  Truy vấn trường `bsd_estimatehandoverdate` trên Product.
3.  Nếu trường này không tồn tại trên Product, gọi `get_EstimateFromProject(OE)` để lấy ngày từ Project.
4.  Trả về ngày tìm được.

### private DateTime get_EstimateFromProject(Entity e_OE)

**Chức năng tổng quát:** Lấy ngày bàn giao ước tính từ thực thể Project nếu không tìm thấy trên Product.

**Logic nghiệp vụ chi tiết:**
1.  Truy vấn thực thể `bsd_project` bằng `bsd_projectid` trên Quote.
2.  Kiểm tra và trả về giá trị của trường `bsd_estimatehandoverdate`. Nếu không có, ném ra lỗi.

### Các Hàm Hỗ trợ khác (Helper Functions)

| Hàm | Chức năng tổng quát |
| :--- | :--- |
| `GetDepositAmount` | Lấy số tiền đặt cọc từ Payment Scheme Master. |
| `GetLandvalueOfProduct` | Lấy giá trị đất của sản phẩm (chủ yếu là logic cũ, hiện tại lấy từ Quote). |
| `DeletePaymentPhase` | Xóa tất cả các chi tiết đợt thanh toán đã tạo trước đó cho Quote này. |
| `SumAmountPhase` | Tính tổng số tiền của tất cả các đợt thanh toán đã tạo cho một Reservation (sử dụng Aggregate FetchXML). |
| `get_AMPhase_Ins` | Truy vấn danh sách các đợt thanh toán chi tiết để lấy số tiền từng đợt. |
| `getAmountLastInstallment` | Lấy số tiền của đợt thanh toán cuối cùng. |
| `get_PreviousIns` | Lấy thông tin đợt thanh toán trước đó dựa trên số thứ tự. |
| `GetTienBangChu_VN` / `TienBangChu` / `setNum` / `getNum` | Bộ hàm phức tạp chịu trách nhiệm chuyển đổi số tiền (phần nguyên) sang chữ Tiếng Việt. |
| `GetTienBangChu_ENG` / `NumberToWords` / `ConvertIntegerToWords` / `ConvertThreeDigitNumber` / `ConvertDecimalToWords` / `CapitalizeFirstLetter` | Bộ hàm phức tạp chịu trách nhiệm chuyển đổi số tiền sang chữ Tiếng Anh. |
| `GetDinhNghiaWordTemplate` / `GetDinhNghiaWordTemplate_EN` | Truy vấn các định nghĩa văn bản mẫu (Word Template) cho các đợt thanh toán (VN/EN). |
| `SetTextWordTemplate` / `SetTextWordTemplate_EN` | Gán các trường văn bản mẫu (text1 đến text10) vào chi tiết đợt thanh toán mới tạo. |