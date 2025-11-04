# Phân tích mã nguồn: Plugin_Create_PaymentNotices.cs

## Tổng quan

Tệp mã nguồn `Plugin_Create_PaymentNotices.cs` chứa một Plugin Dynamics 365/Power Platform được thiết kế để tự động tính toán và cập nhật các trường tài chính quan trọng trên một bản ghi (có thể là bản ghi "Payment Notice" hoặc tương đương) ngay sau khi bản ghi đó được tạo hoặc cập nhật (dựa trên ngữ cảnh thực thi).

Plugin này thực hiện các phép tính phức tạp liên quan đến số tiền thanh toán trước, số tiền chưa thanh toán của các đợt trước, và số tiền cần chuyển, bằng cách truy xuất dữ liệu từ các bản ghi liên quan như Chi tiết Kế hoạch Thanh toán (`bsd_paymentschemedetail`) và Mục nhập Tùy chọn (`bsd_optionentry`).

## Chi tiết các Hàm (Functions/Methods)

### Execute(IServiceProvider serviceProvider)

#### Chức năng tổng quát

Hàm này là điểm vào chính của Plugin, chịu trách nhiệm khởi tạo các dịch vụ cần thiết, truy xuất dữ liệu từ các bản ghi liên quan (Instalment Detail và Option Entry), thực hiện các phép tính nghiệp vụ tài chính, và cập nhật các trường tính toán trở lại bản ghi đích.

#### Logic nghiệp vụ chi tiết

1.  **Khởi tạo Dịch vụ và Ngữ cảnh:**
    *   Hàm khởi tạo các đối tượng tiêu chuẩn của Plugin Dynamics 365: `IPluginExecutionContext` (ngữ cảnh), `IOrganizationServiceFactory` (factory dịch vụ), `IOrganizationService` (dịch vụ tổ chức), và `ITracingService` (dịch vụ theo dõi lỗi/debug).
    *   Lấy bản ghi đích (`entity`) từ tham số đầu vào (`context.InputParameters["Target"]`).
    *   Truy xuất toàn bộ dữ liệu của bản ghi đích vừa được tạo/cập nhật (`enCreated`) và khởi tạo một đối tượng `enUpdate` để chứa các giá trị cần cập nhật.

2.  **Truy xuất Dữ liệu Liên quan:**
    *   **Lấy Chi tiết Đợt Thanh toán (Instalment Detail):** Truy xuất bản ghi `bsd_paymentschemedetail` thông qua trường tham chiếu `bsd_paymentschemedetail` trên bản ghi đích.
    *   **Lấy Mục nhập Tùy chọn (Option Entry):** Truy xuất bản ghi `bsd_optionentry` thông qua trường tham chiếu `bsd_optionentry` trên bản ghi đích.

3.  **Tính toán và Ánh xạ Trường (Mapping and Calculation):**

    *   **Ánh xạ `bsd_amountofthisphase` (Số tiền trong EDA):**
        *   Giá trị này được ánh xạ trực tiếp từ trường `bsd_amountofthisphase` của bản ghi Chi tiết Đợt Thanh toán (`enInsDetail`).
        *   Sử dụng toán tử ba ngôi để kiểm tra sự tồn tại của trường, nếu không tồn tại thì gán giá trị là 0.

    *   **Ánh xạ `bsd_totaladvancepayment` (Tổng số tiền thanh toán trước):**
        *   Giá trị này được ánh xạ trực tiếp từ trường `bsd_totaladvancepayment` của bản ghi Mục nhập Tùy chọn (`EnOP`).
        *   Sử dụng toán tử ba ngôi để kiểm tra sự tồn tại của trường, nếu không tồn tại thì gán giá trị là 0.

    *   **Tính toán `bsd_totalprepaymentamount` (Tổng số tiền thanh toán trước):**
        *   Đây là một trường tính toán.
        *   Công thức: `bsd_totaladvancepayment` (vừa được gán vào `enUpdate`) + `bsd_amountwaspaid` (từ `enInsDetail`).
        *   Kết quả được lưu dưới dạng kiểu `Money`.

    *   **Tính toán `bsd_shortfallinpreviousinstallment` (Số tiền chưa thanh toán các đợt trước):**
        *   Đây là logic phức tạp nhất, yêu cầu truy vấn dữ liệu.
        *   Khởi tạo biến `sum_bsd_balance = 0`.
        *   Lấy số thứ tự (`bsd_ordernumber`) của đợt thanh toán hiện tại (`enInsDetail`).
        *   **Truy vấn (QueryExpression):**
            *   Tạo một truy vấn để tìm tất cả các bản ghi `bsd_paymentschemedetail`.
            *   **Điều kiện 1:** Liên kết với cùng một Mục nhập Tùy chọn (`bsd_optionentry` bằng `EnOP.Id`).
            *   **Điều kiện 2:** Số thứ tự (`bsd_ordernumber`) phải **nhỏ hơn** số thứ tự của đợt hiện tại (`orderNumberInsDetail`). Điều này đảm bảo chỉ lấy các đợt thanh toán trước đó.
        *   **Tổng hợp:** Nếu truy vấn trả về kết quả, hàm lặp qua từng bản ghi Chi tiết Đợt Thanh toán trước đó và cộng dồn giá trị của trường `bsd_balance` (số tiền chưa thanh toán) vào `sum_bsd_balance`.
        *   Cập nhật `enUpdate["bsd_shortfallinpreviousinstallment"]` với tổng số tiền này.

    *   **Tính toán `bsd_amounttotransfer` (Số tiền phải chuyển):**
        *   Đây là trường tính toán cuối cùng.
        *   Công thức: `bsd_amountofthisphase` - `bsd_totalprepaymentamount` + `bsd_shortfallinpreviousinstallment`.
        *   Tất cả các giá trị tham gia vào phép tính đều được lấy từ các trường đã được tính toán và gán vào `enUpdate` ở các bước trước.

4.  **Cập nhật Bản ghi:**
    *   Cuối cùng, hàm gọi `service.Update(enUpdate)` để lưu tất cả các giá trị tài chính đã được tính toán trở lại bản ghi đích trong hệ thống Dynamics 365.