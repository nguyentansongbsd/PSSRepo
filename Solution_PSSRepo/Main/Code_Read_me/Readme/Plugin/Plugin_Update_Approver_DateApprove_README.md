# Phân tích mã nguồn: Plugin_Update_Approver_DateApprove.cs

## Tổng quan

Tệp mã nguồn `Plugin_Update_Approver_DateApprove.cs` chứa một plugin Microsoft Dynamics 365/Power Platform được thiết kế để tự động cập nhật thông tin về Người phê duyệt (Approver) và Ngày phê duyệt (Approval Date) trên một số thực thể tùy chỉnh nhất định.

Plugin này được kích hoạt trong các sự kiện `Create` (Tạo) hoặc `Update` (Cập nhật) và chỉ thực thi logic của nó nếu thực thể đáp ứng các điều kiện cụ thể về tên logic (LogicalName), mã trạng thái (StatusCode) và độ sâu thực thi (Depth). Mục đích chính là ghi lại người dùng hiện tại và thời gian địa phương chính xác khi một bản ghi đạt đến trạng thái phê duyệt hoặc xác nhận cụ thể.

## Chi tiết các Hàm (Functions/Methods)

### IPlugin.Execute(IServiceProvider serviceProvider)

#### Chức năng tổng quát
Đây là điểm vào chính của plugin Dynamics 365. Hàm này khởi tạo các dịch vụ cần thiết, kiểm tra các điều kiện thực thi (Message, Depth, LogicalName, StatusCode) và gọi hàm cập nhật chính nếu các điều kiện được đáp ứng.

#### Logic nghiệp vụ chi tiết
1.  **Khởi tạo Dịch vụ:**
    *   Lấy các dịch vụ tiêu chuẩn của Dynamics 365: `IPluginExecutionContext` (context), `IOrganizationServiceFactory` (factory), `ITracingService` (traceService).
    *   Tạo một phiên bản `IOrganizationService` (`service`) bằng cách sử dụng ID người dùng hiện tại (`this.context.UserId`), đảm bảo các thao tác tiếp theo được thực hiện dưới quyền của người dùng đang kích hoạt sự kiện.
2.  **Lấy Dữ liệu Thực thể:**
    *   Lấy thực thể mục tiêu (`Target`) từ `InputParameters` của context.
    *   Truy vấn toàn bộ thực thể hiện tại từ cơ sở dữ liệu bằng `service.Retrieve` với `new ColumnSet(true)` để đảm bảo có tất cả các thuộc tính, bao gồm cả `statuscode`.
3.  **Kiểm tra Trạng thái:**
    *   Trích xuất giá trị của thuộc tính `statuscode`. Nếu thuộc tính này tồn tại, nó được chuyển đổi từ `OptionSetValue` sang giá trị số nguyên; nếu không, nó được gán giá trị 0.
4.  **Kiểm tra Thông điệp (Message):**
    *   Lấy tên thông điệp (`messageName`). Nếu thông điệp KHÔNG phải là "Create" hoặc "Update", hàm sẽ thoát ngay lập tức (`return`).
5.  **Kiểm tra Điều kiện Thực thi Nghiệp vụ:**
    *   Plugin kiểm tra bốn bộ điều kiện riêng biệt. Logic chỉ được thực thi nếu độ sâu thực thi (`this.context.Depth`) là 1 (ngăn chặn vòng lặp vô hạn) và các điều kiện về `statuscode` và `LogicalName` khớp:
        *   **Điều kiện 1:** `statuscode` là `100000000` VÀ `LogicalName` là `"bsd_approvechangeduedateinstallment"`.
        *   **Điều kiện 2:** `statuscode` là `100000001` VÀ `LogicalName` là `"bsd_updateduedateoflastinstallmentapprove"`.
        *   **Điều kiện 3:** `statuscode` là `100000000` VÀ `LogicalName` là `"bsd_updateduedateoflastinstallment"`.
        *   **Điều kiện 4:** `statuscode` là `100000001` VÀ `LogicalName` là `"bsd_updatelandvalue"`.
    *   Nếu bất kỳ điều kiện nào ở trên được đáp ứng, hàm `updateDateApprovaldateAndApprover` sẽ được gọi với thực thể đầu vào (`inputParameter`).

### updateDateApprovaldateAndApprover(Entity en)

#### Chức năng tổng quát
Hàm này chịu trách nhiệm cập nhật các trường tham chiếu người dùng (Approver) và ngày/giờ (Date) trên thực thể đã cho, đảm bảo rằng ngày/giờ được lưu trữ là thời gian địa phương của người dùng.

#### Logic nghiệp vụ chi tiết
1.  **Truy vấn Thực thể:** Thực hiện truy vấn `service.Retrieve` lại thực thể bằng ID và LogicalName của nó với `ColumnSet(true)`.
2.  **Xác định và Cập nhật Trường:** Dựa trên `LogicalName` của thực thể, plugin sẽ gán các giá trị mới cho các trường phê duyệt/xác nhận:
    *   **Nếu `LogicalName` là `"bsd_approvechangeduedateinstallment"`:**
        *   Cập nhật `bsd_approver` bằng một `EntityReference` tới `systemuser` (ID người dùng hiện tại).
        *   Cập nhật `bsd_approverejectdate` bằng cách gọi `RetrieveLocalTimeFromUTCTime` để chuyển đổi thời gian hiện tại (`DateTime.Now`) sang thời gian địa phương của người dùng.
    *   **Nếu `LogicalName` là `"bsd_updateduedateoflastinstallmentapprove"` hoặc `"bsd_updatelandvalue"`:**
        *   Cập nhật `bsd_approvedrejectedperson` bằng `EntityReference` tới `systemuser`.
        *   Cập nhật `bsd_approvedrejecteddate` bằng thời gian địa phương.
    *   **Nếu `LogicalName` là `"bsd_updateduedateoflastinstallment"`:**
        *   Cập nhật `bsd_usersconfirmed` bằng `EntityReference` tới `systemuser`.
        *   Cập nhật `bsd_dateconfirmedreject` bằng thời gian địa phương.
3.  **Lưu Thay đổi:** Thực hiện lệnh `this.service.Update(entity)` để lưu các thuộc tính đã sửa đổi vào cơ sở dữ liệu Dynamics 365.

### RetrieveLocalTimeFromUTCTime(DateTime utcTime, IOrganizationService service)

#### Chức năng tổng quát
Hàm tiện ích này chuyển đổi một giá trị thời gian UTC thành thời gian địa phương (Local Time) dựa trên cài đặt múi giờ của người dùng đang thực thi plugin.

#### Logic nghiệp vụ chi tiết
1.  **Lấy Mã Múi giờ:** Gọi hàm `RetrieveCurrentUsersSettings(service)` để lấy mã múi giờ (`TimeZoneCode`) của người dùng hiện tại. Nếu không tìm thấy mã múi giờ, một ngoại lệ sẽ được ném ra.
2.  **Chuẩn bị Yêu cầu Chuyển đổi:** Tạo một đối tượng `LocalTimeFromUtcTimeRequest`.
    *   Gán `TimeZoneCode` đã lấy được.
    *   Gán `UtcTime` bằng cách đảm bảo thời gian đầu vào được chuyển đổi thành định dạng UTC (`utcTime.ToUniversalTime()`).
3.  **Thực thi Yêu cầu:** Gửi yêu cầu chuyển đổi múi giờ tới Dynamics 365 bằng `service.Execute()`.
4.  **Trả về:** Trả về giá trị `LocalTime` được trích xuất từ phản hồi của hệ thống.

### RetrieveCurrentUsersSettings(IOrganizationService service)

#### Chức năng tổng quát
Hàm tiện ích này truy vấn thực thể `usersettings` để lấy mã múi giờ (`timezonecode`) được cấu hình cho người dùng đang thực thi plugin.

#### Logic nghiệp vụ chi tiết
1.  **Thiết lập Truy vấn:** Tạo một `QueryExpression` nhắm vào thực thể `"usersettings"`.
2.  **Chọn Cột:** Chỉ định rằng truy vấn chỉ cần lấy các cột `"localeid"` và `"timezonecode"`.
3.  **Thiết lập Điều kiện Lọc:** Tạo một `FilterExpression` để lọc kết quả, đảm bảo chỉ lấy cài đặt của người dùng hiện tại bằng cách sử dụng điều kiện `ConditionExpression("systemuserid", ConditionOperator.EqualUserId)`.
4.  **Thực thi Truy vấn:** Thực hiện truy vấn bằng `organizationService.RetrieveMultiple()`.
5.  **Trích xuất Kết quả:** Lấy thực thể đầu tiên trong tập hợp kết quả (`Entities[0]`) và trích xuất giá trị của thuộc tính `"timezonecode"`.
6.  **Trả về:** Trả về mã múi giờ dưới dạng kiểu `int?`.