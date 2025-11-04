# Phân tích mã nguồn: Plugin_Create_FUL.cs

## Tổng quan

Tệp mã nguồn `Plugin_Create_FUL.cs` chứa một plugin Dynamics 365/Power Platform được thiết kế để thực hiện logic xác thực nghiệp vụ (validation) trước khi một bản ghi được lưu (thường là trong giai đoạn `PreOperation` hoặc `PreValidation` của sự kiện `Create` hoặc `Update`).

Plugin này tập trung vào việc kiểm tra các trường liên quan đến việc chấm dứt (termination) của một hồ sơ. Cụ thể, nó đảm bảo rằng nếu bản ghi thuộc một loại (type) và trạng thái (status) nhất định, ít nhất một trong hai cờ chấm dứt bắt buộc phải được đánh dấu là `true` trước khi cho phép lưu.

## Chi tiết các Hàm (Functions/Methods)

### Execute(IServiceProvider serviceProvider)

#### Chức năng tổng quát

Hàm này là điểm vào chính của plugin, chịu trách nhiệm khởi tạo các dịch vụ Dynamics 365 cần thiết, truy vấn bản ghi đầy đủ, và thực hiện logic kiểm tra các trường liên quan đến việc chấm dứt hồ sơ dựa trên loại và trạng thái của bản ghi.

#### Logic nghiệp vụ chi tiết

Hàm `Execute` thực hiện các bước sau để xác thực dữ liệu:

1.  **Khởi tạo Dịch vụ (Service Initialization):**
    *   Lấy `IPluginExecutionContext` (context) để truy cập thông tin về sự kiện đang chạy.
    *   Lấy `IOrganizationServiceFactory` (factory) và sử dụng nó để tạo `IOrganizationService` (service) với quyền của người dùng đang thực thi (`context.UserId`).
    *   Lấy `ITracingService` (tracingService) để ghi nhật ký gỡ lỗi (debugging).

2.  **Lấy Entity Mục tiêu và Truy vấn Đầy đủ:**
    *   Lấy đối tượng `Entity` đang được xử lý từ `context.InputParameters["Target"]`.
    *   Sử dụng `service.Retrieve` để truy vấn bản ghi đầy đủ từ cơ sở dữ liệu, bao gồm tất cả các cột (`new Microsoft.Xrm.Sdk.Query.ColumnSet(true)`). Bản ghi đầy đủ này được lưu vào biến cấp lớp `en`.

3.  **Kiểm tra Điều kiện Loại (Type Validation):**
    *   Plugin kiểm tra giá trị của trường `bsd_type` (là một `OptionSetValue`).
    *   **Điều kiện thoát 1:** Nếu giá trị số của `bsd_type` KHÔNG phải là `100000006` VÀ KHÔNG phải là `100000005`, plugin sẽ thoát (`return`) ngay lập tức mà không thực hiện bất kỳ logic xác thực nào tiếp theo.

4.  **Kiểm tra Điều kiện Trạng thái (Status Validation):**
    *   Plugin kiểm tra giá trị của trường `statuscode` (là một `OptionSetValue`).
    *   **Điều kiện thoát 2:** Nếu giá trị số của `statuscode` BẰNG `100000002`, plugin sẽ thoát (`return`) ngay lập tức.

5.  **Thiết lập Cờ Kiểm tra (Flag Initialization):**
    *   Hai biến boolean, `bol1` và `bol2`, được khởi tạo là `true`. Các biến này sẽ được sử dụng để theo dõi trạng thái của hai trường chấm dứt bắt buộc.

6.  **Kiểm tra Trường `bsd_terminateletter` (Cờ 1):**
    *   Kiểm tra xem bản ghi `en` có chứa trường `bsd_terminateletter` hay không, HOẶC nếu nó có chứa nhưng giá trị của nó là `false`.
    *   Nếu một trong hai điều kiện trên đúng (nghĩa là trường này không được đánh dấu là true), `bol1` được đặt thành `false`.

7.  **Kiểm tra Trường `bsd_termination` (Cờ 2):**
    *   Kiểm tra xem bản ghi `en` có chứa trường `bsd_termination` hay không, HOẶC nếu nó có chứa nhưng giá trị của nó là `false`.
    *   Nếu một trong hai điều kiện trên đúng, `bol2` được đặt thành `false`.

8.  **Thực hiện Xác thực Bắt buộc (Mandatory Validation):**
    *   Plugin kiểm tra điều kiện: `if (bol1 == false && bol2 == false)`.
    *   Điều này có nghĩa là nếu CẢ hai cờ chấm dứt (`bsd_terminateletter` VÀ `bsd_termination`) đều không được đánh dấu là `true`, logic xác thực sẽ thất bại.

9.  **Ném Ngoại lệ (Throw Exception):**
    *   Nếu điều kiện xác thực thất bại (cả hai cờ đều là `false`), plugin sẽ ném ra một `InvalidPluginExecutionException` với thông báo lỗi: `"Please check the Termination information before saving."`. Điều này ngăn chặn giao dịch cơ sở dữ liệu và hiển thị thông báo lỗi cho người dùng.