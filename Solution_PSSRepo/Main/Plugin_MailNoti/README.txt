# Plugin Gửi Email Thông Báo Tự Động (Plugin_MailNoti)

## 1. Tổng Quan

`Plugin_MailNoti` là một plugin cho Microsoft Dynamics 365, được thiết kế để tự động tạo và gửi email thông báo khi trạng thái (Status) 
của các bản ghi (Quote) hoặc  (Sales Order) thay đổi.

Plugin sẽ dựa vào các quy tắc được định nghĩa sẵn để xác định mẫu email (Email Template) và nhóm người nhận (Team) tương ứng,
 sau đó tạo một email với nội dung được cá nhân hóa và gửi đến các thành viên trong nhóm đó.

## 2. Luồng Hoạt Động

Plugin được đăng ký để kích hoạt (trigger) trên sự kiện **Update** của các entity `Quote` và `Sales Order`.

1.  **Khởi tạo**: Plugin lấy thông tin ngữ cảnh (context) và khởi tạo các dịch vụ cần thiết.
2.  **Kiểm tra điều kiện**:
    *   Plugin kiểm tra `LogicalName` của entity được cập nhật (`quote` hoặc `salesorder`).
    *   Dựa trên `statuscode` mới của bản ghi, plugin xác định:
        *   `tileTemplate`: Tên của Email Template sẽ được sử dụng.
        *   `teamname`: Tên của Team sẽ nhận email.
3.  **Thu thập dữ liệu**:
    *   Plugin gọi hàm `GetValue` để truy xuất thông tin chi tiết từ các trường lookup liên quan trên bản ghi, bao gồm:
        *   Tên dự án (`bsd_tenduan`)
        *   Mã dự án (`projectCode`)
        *   Tên sản phẩm/căn hộ (`bsd_tensp`)
        *   Tên khách hàng (`bsd_tenkh`)
4.  **Tạo Email**:
    *   Plugin tìm và lấy nội dung từ Email Template đã xác định.
    *   Lấy URL từ bản ghi cấu hình (`bsd_configgolive`) có tên là `linkRegardingmail` để tạo liên kết đến bản ghi gốc.
    *   Thay thế các từ khóa (placeholders) trong tiêu đề và nội dung email (ví dụ: `{bsd_tenda}`, `{bsd_tensp}`)
     bằng dữ liệu đã thu thập.
    *   **Người gửi (From)**: Gán người dùng đang thực thi plugin làm người gửi.
    *   **Người nhận (To)**: Lấy danh sách tất cả người dùng trong Team đã xác định và thêm vào trường "To" của email.
5.  **Lưu Email**: Plugin tạo (Create) bản ghi email trong hệ thống. Email này sau đó sẽ được Dynamics 365 gửi đi.

## 3. Cấu Hình Yêu Cầu

Để plugin hoạt động chính xác, các cấu hình sau trong Dynamics 365 là bắt buộc:

### 3.1. Email Templates

Các Email Template với các tên (Title) sau phải tồn tại trong hệ thống:
*   `Reservation_Form`
*   `Deposit`
*   `Convert_Quotation_Reservation`
*   `1st_installment`

### 3.2. Teams

Các Team phải được tạo với quy ước đặt tên động dựa trên mã dự án (`projectCode`). Ví dụ: nếu một dự án có mã là `PROJ001`, các team sau cần tồn tại:
*   `PROJ001-FINANCE-TEAM`
*   `PROJ001-CCR-TEAM`

### 3.3. Cấu hình hệ thống (bsd_configgolive)

Một bản ghi trong entity `bsd_configgolive` phải được tạo với các giá trị sau:
*   **Tên (bsd_name)**: `linkRegardingmail`
*   **Giá trị (bsd_url)**: Một chuỗi URL chứa các placeholder `{entityname}` và `{entityid}`.
    *   *Ví dụ*: `https://your-org.crm.dynamics.com/main.aspx?etn={entityname}&id={entityid}&pagetype=entityrecord`

## 4. Đăng ký Plugin (Plugin Registration)

Plugin này cần được đăng ký bằng Plugin Registration Tool với các thông số sau:

*   **Assembly**: `Plugin_MailNoti.dll`
*   **Step 1**:
    *   **Message**: `Update`
    *   **Primary Entity**: `quote`
    *   **Filtering Attributes**: `statuscode`
    *   **Event Pipeline Stage**: `PostOperation`
    *   **Execution Mode**: `Asynchronous` (Khuyến nghị để không ảnh hưởng đến trải nghiệm người dùng)
*   **Step 2**:
    *   **Message**: `Update`
    *   **Primary Entity**: `salesorder`
    *   **Filtering Attributes**: `statuscode`
    *   **Event Pipeline Stage**: `PostOperation`
    *   **Execution Mode**: `Asynchronous`

## 5. Gỡ lỗi (Debugging)

Plugin sử dụng `ITracingService` để ghi lại chi tiết các bước thực thi. Nếu có lỗi xảy ra, bạn có thể kiểm tra các bản ghi "Plugin-in Trace Log" trong Dynamics 365 để xem luồng hoạt động và xác định nguyên nhân.

Các lỗi phổ biến thường liên quan đến việc thiếu các cấu hình bắt buộc (Email Template, Team, hoặc `bsd_configgolive`). Plugin sẽ ném ra một `InvalidPluginExecutionException` với thông báo lỗi rõ ràng trong những trường hợp này.
