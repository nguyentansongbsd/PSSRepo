# Phân tích mã nguồn: Plugin_Reservation_ReventDelete.cs

## Tổng quan

Tệp mã nguồn `Plugin_Reservation_ReventDelete.cs` chứa một plugin Microsoft Dynamics 365/Power Platform được thiết kế để thực thi logic nghiệp vụ trong quá trình xử lý dữ liệu. Plugin này được triển khai trên thực thể Báo giá (`quote`) và được kích hoạt khi có lệnh xóa (`Delete`).

Mục đích chính của plugin là ngăn chặn người dùng xóa một bản ghi Báo giá nếu bản ghi đó đã đạt đến một trạng thái cụ thể, được xác định bằng mã trạng thái (`statuscode`) là `100000006` (thường đại diện cho trạng thái "Đã thu thập" hoặc "Đã đặt trước"). Đây là một cơ chế bảo vệ dữ liệu quan trọng, đảm bảo các giao dịch đã hoàn tất hoặc đã được xác nhận không bị xóa nhầm.

## Chi tiết các Hàm (Functions/Methods)

### IPlugin.Execute(IServiceProvider serviceProvider)

Đây là phương thức bắt buộc của giao diện `IPlugin`, đóng vai trò là điểm vào (entry point) cho logic của plugin trong môi trường Dynamics 365.

#### Chức năng tổng quát

Phương thức này chịu trách nhiệm khởi tạo các dịch vụ cần thiết (Service, Factory, Context) và thực thi logic kiểm tra trạng thái của thực thể Báo giá đang bị xóa. Nếu Báo giá có trạng thái đã được xác nhận/thu thập, quá trình xóa sẽ bị hủy bỏ và một ngoại lệ sẽ được ném ra.

#### Logic nghiệp vụ chi tiết

1.  **Khởi tạo Dịch vụ (Service Initialization):**
    *   Hàm nhận `serviceProvider` và sử dụng nó để lấy các dịch vụ cốt lõi của Dynamics 365:
        *   Lấy `IPluginExecutionContext` (`service1`) để truy cập thông tin về ngữ cảnh thực thi (người dùng, thông điệp, tham số).
        *   Lấy `IOrganizationServiceFactory` (`this.factory`).
        *   Tạo `IOrganizationService` (`this.service`) bằng cách sử dụng Factory và ID người dùng hiện tại (`((IExecutionContext)service1).UserId`). Dịch vụ này được sử dụng để tương tác với cơ sở dữ liệu.
        *   Lấy `ITracingService` (`service2`) (dùng cho mục đích ghi nhật ký/debug, mặc dù không được sử dụng trong phần logic tiếp theo).

2.  **Truy cập Tham số Đầu vào (Input Parameter Access):**
    *   Truy cập tham số đầu vào `Target` từ `InputParameters` của ngữ cảnh thực thi. Tham số này được ép kiểu thành `EntityReference` (`inputParameter`), đại diện cho thực thể đang được thao tác.

3.  **Kiểm tra Điều kiện Kích hoạt (Guard Clauses):**
    *   Thực hiện hai kiểm tra điều kiện để đảm bảo plugin chỉ chạy trong trường hợp mong muốn:
        *   **Kiểm tra Thực thể:** Kiểm tra xem tên logic của thực thể mục tiêu (`inputParameter.LogicalName`) có phải là `"quote"` (Báo giá) hay không. Nếu không phải, plugin thoát (`return`).
        *   **Kiểm tra Thông điệp:** Kiểm tra xem thông điệp đang được thực thi (`((IExecutionContext)service1).MessageName`) có phải là `"Delete"` hay không. Nếu không phải, plugin thoát (`return`).

4.  **Thực thi Logic Ngăn chặn Xóa:**
    *   Nếu các điều kiện kích hoạt được thỏa mãn (là thao tác Xóa trên thực thể Báo giá), plugin tiến hành kiểm tra trạng thái của bản ghi:
        *   **Truy vấn Trạng thái:** Sử dụng `this.service.Retrieve()` để truy vấn bản ghi Báo giá đang bị xóa.
            *   Chỉ yêu cầu cột `"statuscode"` (mã trạng thái) thông qua `new ColumnSet(new string[1] { "statuscode" })`.
        *   **So sánh Trạng thái:** Giá trị của cột `"statuscode"` được truy xuất, ép kiểu thành `OptionSetValue`, và giá trị số nguyên (`Value`) của nó được so sánh với hằng số `100000006`.
        *   **Ném Ngoại lệ:** Nếu `statuscode` bằng `100000006`, điều này cho thấy Báo giá đã ở trạng thái "Đã thu thập" (hoặc tương đương). Plugin sẽ ném ra một `InvalidPluginExecutionException`, ngăn chặn quá trình xóa và hiển thị thông báo lỗi cho người dùng:
            ```
            "This Quotaion Reservation was Collected! You can't delete it."
            ```