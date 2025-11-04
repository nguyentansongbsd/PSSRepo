# Phân tích mã nguồn: Plugin_Create_Invoice_Payment.cs

## Tổng quan

Tệp mã nguồn `Plugin_Create_Invoice_Payment.cs` là một Plugin tùy chỉnh được phát triển cho nền tảng Microsoft Dynamics 365/CRM. Plugin này được thiết kế để tự động tạo các bản ghi hóa đơn (`bsd_invoice`) khi một giao dịch thanh toán (`bsd_payment` hoặc `bsd_applydocument`) được xác nhận.

Logic nghiệp vụ của plugin rất phức tạp, tập trung vào việc phân loại các khoản thanh toán theo từng đợt (`bsd_paymentschemedetail`), xử lý tiền đặt cọc, tính toán thuế VAT (sử dụng quy tắc 1/11), và tạo các loại hóa đơn khác nhau cho tiền đợt, giá trị quyền sử dụng đất (Land Value), phí bảo trì, và phí quản lý, đặc biệt chú trọng đến các trường hợp thanh toán liên quan đến đợt bàn giao và đợt cuối.

## Chi tiết các Hàm (Functions/Methods)

### Execute(IServiceProvider serviceProvider)

**Chức năng tổng quát:**
Hàm này là điểm vào chính của plugin CRM, chịu trách nhiệm khởi tạo các dịch vụ cần thiết, kiểm tra điều kiện thực thi (như độ sâu và trạng thái), và kích hoạt logic tạo hóa đơn chính nếu điều kiện được đáp ứng.

**Logic nghiệp vụ chi tiết:**
1.  **Khởi tạo Context và Service:** Hàm lấy `IPluginExecutionContext` (context), `IOrganizationServiceFactory` (factory), và tạo `IOrganizationService` (service) bằng cách sử dụng ID người dùng hiện tại.
2.  **Lấy Target Entity:** Lấy bản ghi đang được xử lý (`Target`) từ `context.InputParameters`.
3.  **Kiểm tra Độ sâu (Depth Check):** Kiểm tra `context.Depth`. Nếu độ sâu lớn hơn 2, plugin sẽ thoát ngay lập tức (`return;`) để ngăn chặn các vòng lặp vô hạn hoặc thực thi đệ quy không mong muốn.
4.  **Kiểm tra Trạng thái Kích hoạt:** Plugin chỉ tiếp tục nếu bản ghi `Target` chứa trường `statuscode` và giá trị của nó là `100000000` (thường đại diện cho trạng thái "Confirmed" hoặc "Completed" của giao dịch thanh toán).
5.  **Kiểm tra Trùng lặp Invoice:**
    *   Thực hiện một truy vấn FetchXML trên Entity `bsd_invoice` để tìm kiếm các hóa đơn đã được tạo liên quan đến ID của bản ghi `Target` hiện tại (`bsd_payment` operator='eq' value='{target.Id}').
    *   Nếu truy vấn trả về kết quả rỗng (`checkInvoice.Entities.Count == 0`), điều đó có nghĩa là hóa đơn chưa được tạo.
6.  **Thực thi Logic Chính:** Nếu hóa đơn chưa tồn tại, plugin truy xuất toàn bộ bản ghi `EnPayment` (Payment/ApplyDocument) và gọi hàm `processApplyDocument(EnPayment)` để bắt đầu quá trình tạo hóa đơn.

### processApplyDocument(Entity EnPayment)

**Chức năng tổng quát:**
Hàm này chứa logic nghiệp vụ cốt lõi, chịu trách nhiệm phân tích dữ liệu thanh toán và tạo ra một hoặc nhiều bản ghi hóa đơn (`bsd_invoice`) tương ứng.

**Logic nghiệp vụ chi tiết:**
1.  **Kiểm tra Installment (Đợt thanh toán):**
    *   Truy xuất chi tiết đợt thanh toán (`bsd_paymentschemedetail`) liên quan.
    *   Kiểm tra nếu là đợt 1 (`bsd_ordernumber` == "1") và trạng thái là `100000000` (chưa hoàn tất thanh toán đợt 1), hoặc nếu là đợt cuối (`bsd_lastinstallment` == true), hàm sẽ thoát sớm (`return;`).
2.  **Lấy Dữ liệu Mảng và Phí:** Lấy các chuỗi chứa ID đợt thanh toán (`bsd_arraypsdid`), số tiền thanh toán (`bsd_arrayamountpay`), ID phí (`IV_arrayfees`), và số tiền phí (`IV_arrayfeesamount`).
3.  **Truy vấn Tax Code:** Truy vấn Entity `bsd_taxcode` để lấy thông tin thuế suất 10% và -1 (không chịu thuế).
4.  **Xác định Thời gian và Bên mua:** Xác định ngày thanh toán thực tế (`actualtime_iv`) và bên mua (`Pay_Perchaser`) dựa trên loại Entity đầu vào (`bsd_applydocument` hoặc Entity khác).
5.  **Tạo Invoice cho Đợt thanh toán Đơn (Single Installment):** (Nếu không có mảng ID đợt thanh toán gộp)
    *   Tính toán số tiền thanh toán thực tế (`d_amp`), bao gồm cả tiền đặt cọc nếu là đợt 1.
    *   Tạo Entity `bsd_invoice` và điền các thông tin cơ bản (Dự án, Đơn vị, Bên mua, Ngày phát hành, v.v.).
    *   **Xử lý Đợt bàn giao (Handover - `bsd_duedatecalculatingmethod` == `100000002`):**
        *   Truy vấn tổng VAT đã thanh toán và thông tin đợt cuối.
        *   Tính toán VAT cho đợt bàn giao, VAT cho giá trị quyền sử dụng đất (`land_value`).
        *   Phân chia logic dựa trên điều kiện tổng VAT (`totaltax`) so với tổng VAT đã thanh toán.
        *   Nếu thanh toán đủ/dư (`bolThanhToanDu` là true), tạo 3 hóa đơn riêng biệt: Installment (100000000), Land Value (100000006), và Last Installment (100000004).
        *   Nếu thanh toán thiếu, chỉ tạo một hóa đơn Installment (100000000) hoặc Installment Land Value (100000005) tùy thuộc vào điều kiện VAT.
    *   **Xử lý Đợt thanh toán Thường:** Tính VAT 1/11 và tạo một hóa đơn Installment (100000000).
6.  **Tạo Invoice cho Nhiều Đợt thanh toán (Multiple Installments):** (Nếu `s_bsd_arraypsdid != ""`)
    *   Gộp ID đợt thanh toán hiện tại (nếu có) vào mảng.
    *   Truy vấn chi tiết các đợt thanh toán bằng `get_ecINS`.
    *   Lặp qua các đợt để xác định tổng số tiền, đợt 1, và đợt bàn giao.
    *   **Tạo Invoice Gộp:** Tạo một hóa đơn gộp cho các đợt thanh toán thường (loại 100000000).
    *   **Tạo Invoice Đợt 1:** Nếu có đợt 1, tạo hóa đơn riêng (loại 100000003), bao gồm tiền đặt cọc.
    *   **Tạo Invoice Đợt Bàn giao:** Nếu có đợt bàn giao, thực hiện lại logic phức tạp (tương tự bước 5) để tạo 1, 2, hoặc 3 hóa đơn riêng biệt cho đợt bàn giao, Land Value, và Last Installment.
7.  **Tạo Invoice cho Phí Bảo trì (Maintenance Fees):** (Nếu `IV_arrayfees` chứa phí bảo trì)
    *   Lặp qua các ID phí, kiểm tra trạng thái phí bảo trì (`bsd_maintenancefeesstatus`).
    *   Tính tổng phí bảo trì (`fee`).
    *   Tạo hóa đơn loại Phí Bảo trì (100000001). VAT được đặt là 0.
8.  **Tạo Invoice cho Phí Quản lý (Management Fees):** (Nếu `IV_arrayfees` chứa phí quản lý)
    *   Lặp qua các ID phí, kiểm tra trạng thái phí quản lý (`bsd_managementfeesstatus`).
    *   Tính tổng phí quản lý (`fee`).
    *   Tạo hóa đơn loại Phí Quản lý (100000002). Tính VAT 1/11.

### get_ecINS(IOrganizationService crmservices, string[] s_id)

**Chức năng tổng quát:**
Hàm tiện ích này truy vấn và trả về một tập hợp các chi tiết đợt thanh toán (`bsd_paymentschemedetail`) dựa trên danh sách các ID đợt thanh toán được cung cấp.

**Logic nghiệp vụ chi tiết:**
1.  **Xây dựng FetchXML:** Tạo một truy vấn FetchXML để lấy các bản ghi `bsd_paymentschemedetail`.
2.  **Chọn Thuộc tính:** Truy vấn lấy nhiều thuộc tính quan trọng như số tiền, trạng thái, số thứ tự đợt (`bsd_ordernumber`), và các trường liên quan đến phí.
3.  **Lọc theo ID:** Sử dụng điều kiện `operator='in'` để lọc các bản ghi có ID nằm trong mảng `s_id` đầu vào.
4.  **Sắp xếp:** Kết quả được sắp xếp theo `bsd_ordernumber`.
5.  **Thực thi và Trả về:** Thực thi truy vấn và trả về `EntityCollection` chứa các chi tiết đợt thanh toán.

### RetrieveLocalTimeFromUTCTime(DateTime utcTime)

**Chức năng tổng quát:**
Chuyển đổi một giá trị thời gian từ định dạng UTC sang thời gian địa phương (Local Time) dựa trên múi giờ của người dùng đang thực thi plugin.

**Logic nghiệp vụ chi tiết:**
1.  **Lấy Time Zone Code:** Gọi hàm `RetrieveCurrentUsersSettings` để lấy mã múi giờ (`timezonecode`) của người dùng.
2.  **Kiểm tra Lỗi:** Nếu không tìm thấy mã múi giờ, ném ra ngoại lệ.
3.  **Tạo Request:** Khởi tạo `LocalTimeFromUtcTimeRequest`, cung cấp mã múi giờ và thời gian UTC đầu vào.
4.  **Thực thi Request:** Thực thi request thông qua `service.Execute()`.
5.  **Trả về:** Trả về `LocalTime` từ phản hồi.

### RetrieveCurrentUsersSettings(IOrganizationService service)

**Chức năng tổng quát:**
Truy vấn cài đặt người dùng hiện tại để lấy mã múi giờ cần thiết cho việc chuyển đổi thời gian.

**Logic nghiệp vụ chi tiết:**
1.  **Tạo Query:** Khởi tạo `QueryExpression` nhắm vào Entity `usersettings`.
2.  **Chọn Cột:** Chỉ định lấy cột `timezonecode`.
3.  **Lọc theo Người dùng:** Sử dụng `ConditionExpression` để lọc bản ghi có `systemuserid` bằng với ID của người dùng đang thực thi plugin (`ConditionOperator.EqualUserId`).
4.  **Thực thi và Trả về:** Thực thi truy vấn và trả về giá trị `timezonecode` (kiểu `int?`).

### getLastInstallment(IOrganizationService service, Entity optionentry)

**Chức năng tổng quát:**
Truy vấn và trả về chi tiết đợt thanh toán được đánh dấu là "Đợt cuối" (`bsd_lastinstallment = 1`) cho một đơn hàng/tùy chọn nhập liệu cụ thể.

**Logic nghiệp vụ chi tiết:**
1.  **Xây dựng FetchXML:** Tạo một truy vấn FetchXML nhắm vào Entity `bsd_paymentschemedetail`.
2.  **Lọc Điều kiện:**
    *   Lọc theo `bsd_optionentry` (liên kết với đơn hàng).
    *   Lọc theo `bsd_lastinstallment` bằng `1` (true).
    *   Lọc theo `statecode` bằng `0` (Active).
3.  **Định dạng Query:** Chèn ID của `optionentry` vào chuỗi FetchXML.
4.  **Thực thi và Trả về:** Thực thi truy vấn và trả về Entity đầu tiên trong tập hợp kết quả, đại diện cho đợt thanh toán cuối cùng.