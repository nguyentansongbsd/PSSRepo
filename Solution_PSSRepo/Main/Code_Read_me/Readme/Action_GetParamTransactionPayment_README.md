# Phân tích mã nguồn: Action_GetParamTransactionPayment.cs

## Tổng quan

Tệp `Action_GetParamTransactionPayment.cs` chứa một Plugin Dynamics 365/Power Platform được thiết kế để chạy như một phần của Custom Action hoặc quy trình nghiệp vụ. Mục đích chính của plugin này là truy xuất, tính toán, và tổng hợp các thông tin chi tiết về một giao dịch thanh toán (`bsd_payment`) cụ thể, bao gồm các khoản thanh toán chi tiết (transaction payments), thông tin dự án, thông tin nhà phát triển (developer), và giá trị căn hộ.

Kết quả tổng hợp (các chuỗi chứa tên khoản mục và giá trị tương ứng) sau đó được trả về thông qua các tham số đầu ra (`OutputParameters`) để sử dụng trong các bước tiếp theo của quy trình, thường là để tạo báo cáo hoặc tài liệu.

## Chi tiết các Hàm (Functions/Methods)

### Execute(IServiceProvider serviceProvider)

#### Chức năng tổng quát:

Hàm này là điểm vào chính của Plugin, chịu trách nhiệm khởi tạo các dịch vụ Dynamics 365, truy xuất bản ghi thanh toán chính, tính toán giá trị đợt thanh toán, tổng hợp các giao dịch liên quan, và thiết lập các tham số đầu ra chứa thông tin chi tiết về giao dịch và nhà phát triển.

#### Logic nghiệp vụ chi tiết:

1.  **Khởi tạo Dịch vụ:**
    *   Lấy các đối tượng dịch vụ cần thiết: `IPluginExecutionContext` (context), `IOrganizationServiceFactory` (serviceFactory), `IOrganizationService` (service), và `ITracingService` (tracingService).
    *   `service` được tạo bằng ID người dùng hiện tại của context.

2.  **Truy xuất Bản ghi Chính:**
    *   Truy xuất bản ghi `bsd_payment` dựa trên ID được truyền qua `context.InputParameters["id"]`.
    *   Truy xuất bản ghi `bsd_project` liên quan thông qua trường `bsd_project` của Payment.
    *   Truy xuất bản ghi `account` (Nhà phát triển/Developer) liên quan thông qua trường `bsd_investor` của Project.

3.  **Xử lý Giá trị Đợt Thanh toán (`ValueInstallment` và `nameF`):**
    *   Lấy giá trị `bsd_differentamount` (kiểu Money) từ bản ghi Payment, mặc định là 0 nếu không tồn tại.
    *   Sử dụng lệnh `switch` dựa trên giá trị OptionSet của trường `bsd_paymenttype`:

    *   **Case 100000002 (Installment/New Installment):**
        *   **Điều kiện 1:** Nếu `bsd_differentamount <= 0`, `ValueInstallment` được đặt bằng giá trị của `bsd_amountpay` (đã định dạng tiền tệ "N0").
        *   **Điều kiện 2:** Nếu `bsd_differentamount > 0`, `ValueInstallment` được đặt bằng giá trị của `bsd_balance` (đã định dạng tiền tệ "N0").
        *   Truy xuất bản ghi `bsd_paymentschemedetail` liên quan để lấy `bsd_ordernumber`.
        *   Chuyển đổi `bsd_ordernumber` (số thứ tự) thành định dạng chuỗi thứ tự (ví dụ: 1 -> "1st", 2 -> "2nd", 3 -> "3rd", N -> "Nth").
        *   Thiết lập `nameF` với tên đợt thanh toán đã định dạng (ví dụ: "2nd Installment / Thanh toán lần thứ 2").

    *   **Case 100000001 (Deposit fee/ Đặt cọc):**
        *   `ValueInstallment` được đặt bằng giá trị của `bsd_amountpay`.
        *   `nameF` được đặt là "Deposit fee/ Đặt cọc".

    *   **Case 100000000 (Mặc định/Khác):**
        *   `ValueInstallment` được đặt bằng giá trị của `bsd_amountpay`.

4.  **Lấy Giá Bán Căn hộ (`giacanho`):**
    *   Kiểm tra lại `bsd_paymenttype`:
        *   Nếu là Đặt cọc (100000001), truy xuất bản ghi `bsd_reservation` (Quote) và lấy `totalamount`.
        *   Nếu là loại thanh toán khác, truy xuất bản ghi `bsd_optionentry` và lấy `totalamount`.
    *   Giá trị này được định dạng tiền tệ "N0" và lưu vào biến `giacanho`.

5.  **Truy vấn và Tổng hợp Giao dịch Chi tiết:**
    *   **Truy vấn 1:** Tạo `QueryExpression` để lấy tất cả bản ghi `bsd_transactionpayment` có liên kết đến Payment ID hiện tại.
    *   **Truy vấn 2:** Tạo `QueryExpression` để lấy tất cả bản ghi `bsd_advancepayment` có liên kết đến Payment ID hiện tại.
    *   Khởi tạo hai chuỗi `resultName` và `resultValue` để lưu trữ danh sách các khoản mục và giá trị tương ứng, phân tách bằng ký tự xuống dòng (`\n`).

6.  **Xử lý Kết quả `bsd_transactionpayment`:**
    *   Lặp qua các bản ghi `bsd_transactionpayment` (`rs.Entities`).
    *   Sử dụng `switch` trên `bsd_transactiontype` để xác định loại giao dịch và xây dựng chuỗi `resultName` và `resultValue`:
        *   **Case 100000000 (Installment):** Truy xuất `bsd_installment`, định dạng số thứ tự (1st, 2nd, etc.), và thêm tên đợt thanh toán cùng với số tiền (`bsd_amount`).
        *   **Case 100000001 (Installment Interest Charge):** Tương tự như trên, nhưng thêm nhãn "Installment Interest Charge/ Trả lãi suất".
        *   **Case 100000002 (Fee):** Kiểm tra `bsd_feetype` (100000000 là Maintenance Fee, còn lại là Management Fee) và thêm tên phí cùng số tiền.
        *   **Case 100000003 (Miscellaneous/Other):** Nếu có trường `bsd_miscellaneous`, truy xuất và sử dụng `bsd_description`. Nếu không, sử dụng nhãn "Other/ Khoản khác". Thêm số tiền.

7.  **Xử lý Kết quả `bsd_advancepayment`:**
    *   Lặp qua các bản ghi `bsd_advancepayment` (`rs2.Entities`).
    *   Thêm nhãn "Advance Payment/Thanh toán trước hạn" và số tiền (`bsd_amount`) vào `resultName` và `resultValue`.

8.  **Thiết lập Tham số Đầu ra (Output Parameters):**
    *   Gán các giá trị đã tính toán và tổng hợp vào `context.OutputParameters`:
        *   `giacanho`, `nameF`, `resultName`, `resultValue`, `valueInstallment`.
    *   Gán các thông tin chi tiết của Nhà phát triển (`enDev`) vào các tham số đầu ra, sử dụng kiểm tra `Contains` để đảm bảo trường tồn tại. Nếu trường không tồn tại, giá trị mặc định là `_`. Các trường được xuất bao gồm:
        *   `bsd_accountnameother`
        *   `bsd_accountname` (tên)
        *   `bsd_acountdiachi` (địa chỉ thường trú)
        *   `bsd_registrationcode1` (mã đăng ký)
        *   `bsd_developphone` (điện thoại)
        *   `bsd_developfax` (fax)
        *   `bsd_companycode` (mã công ty)
        *   `bsd_developerthuongtruVN` (địa chỉ thường trú VN)
        *   `bsd_developthuongtruEg` (địa chỉ thường trú Eg/Anh)