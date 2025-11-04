# Action: Lấy Thông tin Người gửi và Người nhận Email (Action_GetMailFromAndTo)

## Mô tả tổng quan

Plugin này là một dịch vụ kiểm tra và truy xuất dữ liệu, được kích hoạt bởi một Action trong Dynamics 365. Mục đích chính của nó là xác thực xem một email có thể được gửi từ một người gửi đã cấu hình (liên kết với một dự án) đến một người nhận cụ thể (là một Contact hoặc Account) hay không. Plugin cũng truy xuất ID của hàng đợi/người dùng của người gửi nếu có.

Kết quả của plugin là một giá trị boolean cho biết liệu email có thể được gửi hay không, cùng với một thông báo lỗi nếu có vấn đề. Plugin này thường được sử dụng như một bước tiền kiểm tra trước khi thực hiện gửi email thực tế.

## Logic chi tiết của từng Function

### `Execute(IServiceProvider serviceProvider)`

Đây là hàm chính của plugin, được thực thi khi Action được gọi.

1.  **Khởi tạo:** Thiết lập các đối tượng dịch vụ cần thiết để tương tác với Dynamics 365.
2.  **Nhận Tham số đầu vào:** Plugin mong đợi bốn tham số đầu vào:
    *   `idTo`: GUID của người nhận (Contact hoặc Account).
    *   `entityName`: Tên logic của thực thể nguồn mà email đang được gửi từ đó (ví dụ: `bsd_payment`).
    *   `entityId`: GUID của thực thể nguồn.
3.  **Truy xuất Thực thể:**
    *   Truy xuất thực thể nguồn (`en`) dựa trên `entityName` và `entityId`.
    *   Truy xuất thực thể người nhận (`cus`) dựa trên `idTo`.
4.  **Xác định loại người nhận:** Kiểm tra trường `bsd_fullname` của thực thể người nhận để xác định xem đó là `contact` hay `account`. (Lưu ý: Việc kiểm tra `LogicalName` trực tiếp sẽ mạnh mẽ hơn).
5.  **Xác định trường Dự án:** Đặt `fieldProject` là `bsd_project`. Hiện tại, giá trị này được mã hóa cứng, nhưng cấu trúc `switch` cho thấy có thể mở rộng để xử lý các `entityName` khác trong tương lai.
6.  **Gọi `CheckMailIsContain`:** Đây là hàm chứa logic cốt lõi, được gọi để thực hiện xác thực và truy xuất ID người gửi.
7.  **Thiết lập Tham số đầu ra:** Dựa trên kết quả của `CheckMailIsContain`, plugin đặt `context.OutputParameters["res"]` thành `1` (thành công) hoặc `0` (thất bại).

### `CheckMailIsContain(string idTo, string idProject, ref string idFrom, ref string mess)`

Hàm này thực hiện việc xác thực và truy xuất dữ liệu chính.

1.  **Kiểm tra Mail Dự án (Người gửi):**
    *   Truy xuất thực thể `bsd_project` dựa trên `idProject`.
    *   Kiểm tra xem thực thể `bsd_project` có chứa trường `bsd_senderconfigsystem` hay không. Nếu không, nó sẽ đặt một thông báo lỗi và trả về `false`.
    *   Nếu `bsd_senderconfigsystem` tồn tại, nó sẽ đặt `context.OutputParameters["idQueueFrom"]` thành ID của hàng đợi/người dùng của người gửi.
2.  **Kiểm tra Mail Khách hàng (Người nhận):**
    *   Truy xuất thực thể người nhận (`contact` hoặc `account`) dựa trên `idTo` và `cusType`.
    *   Kiểm tra xem thực thể người nhận có chứa trường `emailaddress1` hay không.
        *   Nếu `cusType` là `account` và `emailaddress1` không tồn tại, nó sẽ đặt một thông báo lỗi và trả về `false`.
        *   Nếu `cusType` là `contact` và `emailaddress1` không tồn tại, nó sẽ đặt một thông báo lỗi. (Lưu ý: Có một kiểm tra thêm cho `emailaddress11` có thể là lỗi đánh máy hoặc một trường email thay thế. Logic này có thể gây nhầm lẫn và có khả năng chứa lỗi).
3.  **Giá trị trả về:** Nếu tất cả các kiểm tra đều thành công, hàm trả về `true`.

### `CreateRecordError(string message)`

Hàm trợ giúp này tạo một bản ghi lỗi mới trong Dynamics 365.

1.  **Tạo bản ghi lỗi:** Tạo một bản ghi thực thể `bsd_errorcreatemail` mới.
2.  **Điền thông tin lỗi:** Điền `bsd_entityid` với tham chiếu đến thực thể nguồn (`en`) và `bsd_reason` với thông báo lỗi được cung cấp.
3.  **Lưu bản ghi:** Tạo bản ghi lỗi này trong CRM.
4.  **Giá trị trả về:** Luôn trả về `false`.

*Lưu ý: Hàm `CreateRecordError` hiện không được gọi trong phương thức `Execute`, điều này có nghĩa là các lỗi hiện chỉ được trả về dưới dạng tham số đầu ra chứ không được ghi lại dưới dạng bản ghi `bsd_errorcreatemail`.*
