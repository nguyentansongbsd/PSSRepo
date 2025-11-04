# Phân tích mã nguồn: Action_GetParamPaymentNotice.cs

## Tổng quan

Tệp mã nguồn `Action_GetParamPaymentNotice.cs` định nghĩa một Plugin Dynamics 365/Power Platform tùy chỉnh, triển khai giao diện `IPlugin`. Plugin này được thiết kế để chạy trong ngữ cảnh của một bản ghi Thông báo Thanh toán (`bsd_customernotices`).

Mục đích chính của Plugin là truy xuất dữ liệu chi tiết từ bản ghi Thông báo Thanh toán hiện tại và các bản ghi liên quan (Dự án, Chi tiết Kế hoạch Thanh toán, Nhà đầu tư/Phát triển, và Chi tiết Ngân hàng) để tổng hợp và định dạng các tham số đầu ra. Các tham số đầu ra này sau đó có thể được sử dụng bởi các bước tiếp theo trong quy trình nghiệp vụ, chẳng hạn như tạo tài liệu hoặc gửi email.

## Chi tiết các Hàm (Functions/Methods)

### Execute(IServiceProvider serviceProvider)

#### Chức năng tổng quát:

Hàm này là điểm vào chính của Plugin, chịu trách nhiệm khởi tạo các dịch vụ CRM, truy xuất bản ghi Thông báo Thanh toán và các bản ghi liên quan, xác định thông tin tài khoản ngân hàng mặc định, tính toán và định dạng các giá trị cần thiết, sau đó đặt chúng vào các tham số đầu ra của ngữ cảnh Plugin.

#### Logic nghiệp vụ chi tiết:

1.  **Khởi tạo Dịch vụ (Lines 15-19):**
    *   Lấy ngữ cảnh thực thi (`context`), nhà máy dịch vụ (`serviceFactory`), dịch vụ tổ chức (`service`), và dịch vụ theo dõi (`tracingService`) từ `serviceProvider`.
    *   Tạo đối tượng `IOrganizationService` để tương tác với dữ liệu CRM.

2.  **Truy xuất Bản ghi Chính và Liên quan (Lines 20-26):**
    *   Lấy ID của bản ghi hiện tại từ `context.InputParameters["id"]`.
    *   Truy xuất bản ghi Thông báo Thanh toán (`bsd_customernotices`) hiện tại, lưu trữ trong biến `paymentNotice`.
    *   Truy xuất các bản ghi liên quan thông qua các trường tham chiếu (EntityReference):
        *   Dự án (`bsd_project`) -> `enProject`.
        *   Mục nhập Tùy chọn (`bsd_optionentry`) -> `enOP`.
        *   Chi tiết Kế hoạch Thanh toán (`bsd_paymentschemedetail`) -> `enIns`.
    *   Truy xuất bản ghi Nhà phát triển/Nhà đầu tư (`account`) liên kết với Dự án (`enProject["bsd_investor"]`) -> `enDev`.

3.  **Tìm kiếm Tài khoản Ngân hàng Dự án (Lines 28-42):**
    *   Khởi tạo `QueryExpression` để tìm kiếm các bản ghi `bsd_projectbankaccount`.
    *   Thiết lập điều kiện truy vấn: `bsd_project` phải bằng ID của `enProject`.
    *   Thực hiện truy vấn (`service.RetrieveMultiple(query)`).
    *   **Logic chọn Ngân hàng:**
        *   Lặp qua các kết quả. Nếu một bản ghi có trường `bsd_default` là `true`, nó được chọn làm `enBank` và vòng lặp kết thúc (hoặc tiếp tục nhưng chỉ giữ lại bản ghi mặc định đầu tiên).
        *   Nếu không tìm thấy tài khoản ngân hàng mặc định nào, nhưng danh sách kết quả không rỗng, chọn bản ghi đầu tiên trong danh sách làm `enBank`.

4.  **Truy xuất Chi tiết Ngân hàng (Lines 43-46):**
    *   Nếu `enBank` được tìm thấy, truy xuất bản ghi chi tiết Ngân hàng (`bsd_bank`) được tham chiếu bởi `enBank["bsd_bank"]`, lưu trữ trong `enBankDetail`.

5.  **Xử lý Số thứ tự (Order Number) (Lines 47-58):**
    *   Lấy giá trị số thứ tự (`bsd_ordernumber`) từ `enIns` (Chi tiết Kế hoạch Thanh toán).
    *   Xác định hậu tố thứ tự (`bsd_ordernumbernd`):
        *   Nếu `bsd_ordernumber` là 2, hậu tố là "2nd".
        *   Nếu `bsd_ordernumber` là 3, hậu tố là "3nd" (Lưu ý: Đây có thể là lỗi chính tả, thường là "3rd").
        *   Trong các trường hợp khác, hậu tố là `"{số}th"`.

6.  **Định dạng Dữ liệu (Lines 59-61):**
    *   **Ngày DA (`bsd_dadate`):** Lấy từ `enOP`. Nếu tồn tại, thêm 7 giờ (điều chỉnh múi giờ) và định dạng thành "dd/MM/yyyy". Nếu không tồn tại, sử dụng "_____".
    *   **Số tiền Giai đoạn (`bsd_amountofthisphase`):** Lấy từ `enIns` (kiểu Money) và định dạng thành chuỗi số có dấu phân cách hàng nghìn ("N0").
    *   **Ngày Đến hạn (`bsd_duedate`):** Lấy từ `enIns`, thêm 7 giờ và định dạng thành "dd/MM/yyyy".

7.  **Thiết lập Tham số Đầu ra (Lines 62-99):**
    *   Thiết lập 5 tham số đầu ra đã được tính toán/định dạng ở trên (`bsd_duedate`, `bsd_ordernumber`, `bsd_ordernumbernd`, `bsd_amountofthisphase`, `bsd_dadate`).
    *   Thiết lập 15 tham số đầu ra bổ sung bằng cách kiểm tra sự tồn tại của trường trong các Entity đã truy xuất (`enDev`, `enOP`, `enBank`, `enBankDetail`, `enProject`).
    *   **Quy tắc thiết lập:** Đối với hầu hết các trường, nếu trường tồn tại trong Entity, giá trị của nó được sử dụng; nếu không, giá trị mặc định là `"_"` được sử dụng.
    *   Các tham số quan trọng được thiết lập bao gồm:
        *   Thông tin Nhà phát triển/Nhà đầu tư (`bsd_accountnameother`, `bsd_accountname`, `bsd_acountdiachi`, `bsd_registrationcode1`, `bsd_developphone`, `bsd_developfax`, `bsd_companycode`, `bsd_developerthuongtruVN`, `bsd_developthuongtruEg`).
        *   Thông tin Hợp đồng/DA (`bsd_danumber`, `bsd_contractnumber`, `bsd_contractdate`).
        *   Thông tin Ngân hàng (`bsd_bankaccount`, `bsd_othername`, `bsd_bankname`, `bsd_addressother`, `bsd_address`, `bsd_swiftcode`).
        *   Thông tin Kế toán Dự án (`bsd_acountant`, `bsd_extfin`).
    *   Sử dụng `tracingService.Trace()` để ghi lại các bước sau khi thiết lập một số tham số đầu ra, hỗ trợ gỡ lỗi.