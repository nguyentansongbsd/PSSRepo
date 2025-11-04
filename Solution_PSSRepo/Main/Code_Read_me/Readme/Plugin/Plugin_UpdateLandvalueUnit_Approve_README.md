# Phân tích mã nguồn: Plugin_UpdateLandvalueUnit_Approve.cs

## Tổng quan

Tệp mã nguồn `Plugin_UpdateLandvalueUnit_Approve.cs` định nghĩa một Plugin Dynamics 365/Power Platform được thiết kế để xử lý logic phê duyệt cho các yêu cầu cập nhật giá trị đất (`bsd_updatelandvalue`).

Plugin này được kích hoạt trên sự kiện **Update** của thực thể `bsd_updatelandvalue`. Nhiệm vụ chính của nó là kiểm tra trạng thái phê duyệt của bản ghi chính. Nếu bản ghi đã được phê duyệt, nó sẽ thu thập tất cả các bản ghi giá trị đất chi tiết liên quan (`bsd_landvalue`) và sau đó kích hoạt một Custom Action (`bsd_Action_Action_UpdateLandValue`) để xử lý logic cập nhật giá trị đất phức tạp một cách bất đồng bộ (asynchronously), đồng thời ghi lại thông tin người phê duyệt và thời gian phê duyệt.

## Chi tiết các Hàm (Functions/Methods)

### Execute(IServiceProvider serviceProvider)

#### Chức năng tổng quát
Đây là điểm vào chính của Plugin. Hàm này chịu trách nhiệm khởi tạo các dịch vụ CRM, kiểm tra điều kiện kích hoạt, và điều phối việc xử lý phê duyệt bằng cách gọi một Custom Action.

#### Logic nghiệp vụ chi tiết
1.  **Khởi tạo Dịch vụ:** Hàm khởi tạo các đối tượng cần thiết cho môi trường Dynamics 365, bao gồm `IPluginExecutionContext` (context), `IOrganizationServiceFactory` (factory), `IOrganizationService` (service), và `ITracingService` (tracingService).
2.  **Lấy Target:** Lấy các tham số đầu vào, đặc biệt là thực thể Target đang được cập nhật.
3.  **Kiểm tra Điều kiện Kích hoạt:** Plugin chỉ tiếp tục thực thi nếu:
    *   Thông điệp (MessageName) là `"Update"`.
    *   Tên logic của thực thể Target là `"bsd_updatelandvalue"`.
4.  **Truy vấn Bản ghi Chính:** Lấy toàn bộ thuộc tính của bản ghi `bsd_updatelandvalue` hiện tại (`entity2`) bằng cách sử dụng `service.Retrieve()`.
5.  **Kiểm tra Trạng thái Phê duyệt:** Kiểm tra xem giá trị của trường `statuscode` trên bản ghi chính có bằng `100000001` hay không. Đây là điều kiện để xác định bản ghi đã đạt đến trạng thái cần xử lý phê duyệt.
6.  **Truy vấn Bản ghi Chi tiết:** Nếu trạng thái đúng, Plugin thực hiện một truy vấn FetchXML để lấy tất cả các bản ghi `bsd_landvalue` (các đơn vị giá đất) liên quan đến bản ghi `bsd_updatelandvalue` hiện tại.
    *   Điều kiện truy vấn: `statecode` bằng `0` (Active) và `bsd_updatelandvalue` bằng ID của bản ghi chính.
7.  **Xử lý Logic Cũ (Bị chú thích):** Mã nguồn chứa một khối logic lớn bị chú thích (`#region chuyển qua action để chạy PA từng item`). Khối này cho thấy trước đây, Plugin đã cố gắng thực hiện logic cập nhật giá trị đất, cập nhật các bản ghi `salesorder`, `salesorderdetail`, và `unit` trực tiếp trong Plugin. Việc chú thích này cho thấy logic phức tạp và tốn thời gian đã được chuyển sang một cơ chế xử lý bất đồng bộ (như Custom Action hoặc Power Automate) để tránh lỗi timeout của Plugin.
8.  **Cập nhật Bản ghi Chính (Master Record):**
    *   Tạo một thực thể cập nhật (`enUpdate`) cho bản ghi `bsd_updatelandvalue`.
    *   Đặt cờ `bsd_processing_pa` thành `true` (có thể là cờ báo hiệu rằng quá trình xử lý bất đồng bộ đã được bắt đầu).
    *   Đặt `bsd_error` thành `false` và xóa `bsd_errordetail`.
    *   Ghi lại người phê duyệt (`bsd_approvedrejectedperson`) là người dùng hiện tại (`this.context.UserId`).
    *   Ghi lại thời gian phê duyệt (`bsd_approvedrejecteddate`) bằng cách gọi hàm tiện ích `RetrieveLocalTimeFromUTCTime(DateTime.Now)` để đảm bảo thời gian được lưu trữ theo múi giờ địa phương của người dùng.
    *   Thực hiện `service.Update(enUpdate)`.
9.  **Kích hoạt Custom Action:**
    *   Tạo một `OrganizationRequest` mới với tên Action là `"bsd_Action_Action_UpdateLandValue"`.
    *   Tạo chuỗi `listid` chứa tất cả các ID của các bản ghi `bsd_landvalue` chi tiết (nối bằng dấu phẩy).
    *   Thiết lập các tham số đầu vào cho Action: `listid`, `idmaster` (ID của bản ghi chính), và `userid` (ID người dùng hiện tại).
    *   Thực thi Action bằng `this.service.Execute(request)`.

### RetrieveLocalTimeFromUTCTime(DateTime utcTime)

#### Chức năng tổng quát
Hàm này chuyển đổi một giá trị thời gian UTC thành thời gian địa phương (Local Time) dựa trên cài đặt múi giờ của người dùng đang chạy Plugin.

#### Logic nghiệp vụ chi tiết
1.  **Lấy TimeZoneCode:** Hàm gọi `RetrieveCurrentUsersSettings(this.service)` để lấy mã múi giờ (`TimeZoneCode`) của người dùng hiện tại.
2.  **Kiểm tra Lỗi:** Nếu không tìm thấy mã múi giờ (trả về `null`), hàm sẽ ném ra một `InvalidPluginExecutionException` với thông báo lỗi.
3.  **Tạo Request Chuyển đổi:** Tạo một `LocalTimeFromUtcTimeRequest` (một thông điệp SDK tiêu chuẩn của Dynamics 365).
4.  **Thiết lập Tham số:**
    *   Gán `TimeZoneCode` đã lấy được vào Request.
    *   Chuyển đổi thời gian đầu vào (`utcTime`) sang định dạng UTC chuẩn (`utcTime.ToUniversalTime()`) và gán vào trường `UtcTime`.
5.  **Thực thi và Trả về:** Thực thi Request bằng `service.Execute()` và trả về giá trị `LocalTime` từ phản hồi.

### RetrieveCurrentUsersSettings(IOrganizationService service)

#### Chức năng tổng quát
Hàm tiện ích này truy vấn thực thể `usersettings` để lấy mã múi giờ (`timezonecode`) của người dùng đang thực thi Plugin.

#### Logic nghiệp vụ chi tiết
1.  **Khởi tạo Truy vấn:** Tạo một `QueryExpression` nhắm vào thực thể `"usersettings"`.
2.  **Chọn Cột:** Chỉ định lấy các cột `"localeid"` và `"timezonecode"`.
3.  **Thiết lập Bộ lọc:** Tạo một `FilterExpression` để đảm bảo chỉ lấy cài đặt của người dùng hiện tại.
    *   Sử dụng `ConditionExpression` với điều kiện `attribute='systemuserid'` và `ConditionOperator.EqualUserId`.
4.  **Thực thi Truy vấn:** Thực hiện truy vấn bằng `organizationService.RetrieveMultiple()`.
5.  **Trích xuất Giá trị:** Lấy bản ghi đầu tiên trong kết quả truy vấn, chuyển đổi nó thành Entity, và trích xuất giá trị của thuộc tính `"timezonecode"`.
6.  **Trả về:** Trả về giá trị `timezonecode` dưới dạng kiểu `int?`.