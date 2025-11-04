# Action: Lấy Thông tin Email và Cập nhật Trạng thái (Action_GetEmailMessageAndUpdatePendingSend)

## Mô tả tổng quan

Plugin này được thiết kế để hoạt động như một dịch vụ thu thập dữ liệu, được kích hoạt bởi một Action trong Dynamics 365. Chức năng chính của nó là lấy toàn bộ thông tin chi tiết của một bản ghi Email cụ thể, bao gồm người gửi, người nhận (To, CC, BCC), chủ đề, nội dung và tệp đính kèm. 

Sau khi thu thập, tất cả các thông tin này được trả về dưới dạng các tham số đầu ra (Output Parameters) của Action. Mục đích của plugin này là chuẩn bị dữ liệu một cách đầy đủ để một tiến trình khác (có thể là một dịch vụ bên ngoài như SendGrid hoặc một Azure Function) có thể sử dụng để gửi email đi.

## Logic chi tiết của từng Function

### `Execute(IServiceProvider serviceProvider)`

Đây là hàm chính của plugin, được thực thi khi Action được gọi.

1.  **Khởi tạo:** Thiết lập các đối tượng dịch vụ cần thiết để tương tác với Dynamics 365.
2.  **Nhận Tham số đầu vào:** Lấy `id` của bản ghi `email` cần xử lý từ `InputParameters`.
3.  **Truy xuất Email:** Dùng `id` để lấy thông tin chi tiết của bản ghi `email` từ hệ thống.
4.  **Gọi các hàm trợ giúp:**
    *   `GetMail_To_CC_BCC(idEmailMessage)`: Lấy danh sách địa chỉ email của người gửi, người nhận To, CC, và BCC.
    *   `GetEmailAttachments(idEmailMessage)`: Lấy thông tin về tệp đính kèm đầu tiên của email.
    *   `GetMailForm()`: Xác định địa chỉ email của người gửi (From).
5.  **Thiết lập Tham số đầu ra:** Gán tất cả các thông tin đã thu thập được vào `OutputParameters` của Action để trả về cho tiến trình đã gọi nó. Các tham số bao gồm:
    *   `mailFrom`: Địa chỉ người gửi.
    *   `mailTo`: Địa chỉ người nhận.
    *   `mailCC`: Danh sách email CC.
    *   `mailBCC`: Danh sách email BCC.
    *   `bodymail`: Nội dung (thân) của email.
    *   `subject`: Chủ đề của email.
    *   `fileNameAttach`: Tên của tệp đính kèm.
    *   `bodyfile`: Nội dung của tệp đính kèm (dưới dạng chuỗi Base64).

*Lưu ý: Có một đoạn code đã bị vô hiệu hóa (`comment out`) cho thấy ý định ban đầu là cập nhật trạng thái của email thành "Pending Send", nhưng hiện tại chức năng này không được kích hoạt.*

### `GetMailForm()`

Hàm này có nhiệm vụ xác định địa chỉ email của người gửi.

1.  **Xác định ngữ cảnh:** Lấy thông tin bản ghi liên quan (`regardingobjectid`) của email, ví dụ như `Customer Notices` hoặc `Payment`.
2.  **Truy vấn theo ngữ cảnh:**
    *   Dựa vào loại của bản ghi liên quan, plugin sẽ tìm đến bản ghi `Project` tương ứng.
    *   Trên bản ghi `Project`, nó lấy ra người dùng được cấu hình trong trường `bsd_senderconfigsystem`.
3.  **Lấy địa chỉ email:** Trả về địa chỉ email (`internalemailaddress`) của người dùng đã được cấu hình ở trên. Logic này cho thấy người gửi email được xác định động dựa trên cấu hình của dự án.

### `GetMail_To_CC_BCC(string idEmailMessage)`

Hàm này truy vấn để lấy danh sách tất cả các bên liên quan (người gửi, người nhận) của email.

1.  **Truy vấn `ActivityParty`:** `ActivityParty` là nơi Dynamics 365 lưu trữ mối quan hệ giữa một hoạt động (như email) và các bản ghi khác (như Contact, User).
2.  **Lọc theo ID Email:** Truy vấn tất cả các `ActivityParty` có `activityid` trùng với ID của email đang xử lý.
3.  **Phân loại vai trò:** Lặp qua các kết quả và dựa vào trường `participationtypemask` để xác định vai trò của từng địa chỉ email:
    *   `1`: From (Người gửi)
    *   `2`: To (Người nhận chính)
    *   `3`: CC (Người nhận đồng gửi)
    *   `4`: BCC (Người nhận ẩn danh)
4.  **Tổng hợp kết quả:** Nối các địa chỉ email vào các biến `mailTo`, `mailCC`, `mailBCC` tương ứng, phân tách nhau bằng dấu chấm phẩy (`;`).

### `GetEmailAttachments(string emailId)`

Hàm này dùng để lấy thông tin về tệp đính kèm.

1.  **Truy vấn `ActivityMimeAttachment`:** Đây là entity lưu trữ các tệp đính kèm cho hoạt động.
2.  **Lọc theo ID Email:** Tìm các tệp đính kèm có `objectid` là ID của email.
3.  **Lấy thông tin tệp:** Lấy ra `filename` (tên tệp) và `body` (nội dung tệp dưới dạng chuỗi Base64) của **tệp đính kèm đầu tiên** tìm thấy.

*Lưu ý: Logic hiện tại chỉ xử lý một tệp đính kèm duy nhất.*
