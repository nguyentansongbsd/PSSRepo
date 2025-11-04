# Phân tích mã nguồn: Plugin_Updateestimatehandoverdate.cs

## Tổng quan

Tệp mã nguồn `Plugin_Updateestimatehandoverdate.cs` chứa một Plugin Dynamics 365/Power Platform được viết bằng C#. Plugin này được thiết kế để chạy trên một sự kiện của một thực thể master (có thể là `bsd_updateestimatehandoverdate`).

Chức năng chính của Plugin là xử lý logic phê duyệt: khi trạng thái của bản ghi master được đặt thành một giá trị cụ thể (100000001), nó sẽ kiểm tra sự tồn tại của các bản ghi chi tiết liên quan, cập nhật các trường trạng thái xử lý trên bản ghi master, và sau đó kích hoạt một hành động tùy chỉnh (Custom Action) để xử lý hàng loạt các bản ghi chi tiết đó. Plugin cũng bao gồm các hàm tiện ích để chuyển đổi thời gian UTC sang thời gian địa phương của người dùng đang thực thi.

## Chi tiết các Hàm (Functions/Methods)

### Execute(IServiceProvider serviceProvider)

#### Chức năng tổng quát:
Đây là điểm vào chính của Plugin. Hàm này khởi tạo các dịch vụ Dynamics 365, kiểm tra trạng thái của bản ghi mục tiêu, xác thực sự tồn tại của các bản ghi chi tiết, cập nhật thông tin phê duyệt trên bản ghi master, và kích hoạt một hành động tùy chỉnh để xử lý các bản ghi chi tiết.

#### Logic nghiệp vụ chi tiết:
1.  **Khởi tạo Dịch vụ:** Hàm lấy các dịch vụ cần thiết từ `serviceProvider`, bao gồm `IPluginExecutionContext` (context), `IOrganizationServiceFactory` (factory), `IOrganizationService` (service), và `ITracingService` (tracingService).
2.  **Lấy Bản ghi Mục tiêu:** Lấy thực thể mục tiêu (`Target`) từ `context.InputParameters`.
3.  **Truy xuất Bản ghi Đầy đủ:** Sử dụng `service.Retrieve` để lấy toàn bộ bản ghi hiện tại (`en`) dựa trên ID và tên logic của thực thể mục tiêu. Điều này đảm bảo Plugin có quyền truy cập vào tất cả các thuộc tính, bao gồm cả `statuscode`.
4.  **Kiểm tra Trạng thái:** Lấy giá trị của `statuscode`.
5.  **Điều kiện Xử lý:** Kiểm tra xem `status` có bằng `100000001` hay không. Đây là trạng thái kích hoạt logic xử lý (ví dụ: "Đã phê duyệt").
6.  **Xác thực Chi tiết:** Nếu điều kiện trạng thái được đáp ứng, hàm gọi `ExistDetail(ref result)`.
    *   Hàm này truy vấn các bản ghi chi tiết liên quan.
    *   Nếu không tìm thấy bản ghi chi tiết nào, nó sẽ ném ra `InvalidPluginExecutionException`, ngăn chặn quá trình thực thi.
    *   Kết quả trả về là `EntityCollection` (`rs`) chứa các bản ghi chi tiết.
7.  **Cập nhật Bản ghi Master:**
    *   Tạo một đối tượng `Entity` mới (`enDetailUpdate`) chỉ chứa các trường cần cập nhật cho bản ghi master.
    *   Thiết lập các trường xử lý: `bsd_processing_pa = true`, `bsd_error = false`, `bsd_errordetail = ""`.
    *   Cập nhật thông tin phê duyệt:
        *   `bsd_approvedrejectedperson`: Được đặt là `EntityReference` đến người dùng hiện tại (`context.UserId`).
        *   `bsd_approvedrejecteddate`: Được đặt là thời gian địa phương hiện tại, sử dụng hàm `RetrieveLocalTimeFromUTCTime(DateTime.Now)`.
    *   Thực hiện `service.Update(enDetailUpdate)`.
8.  **Thực thi Custom Action:**
    *   Tạo một `OrganizationRequest` mới với tên hành động tùy chỉnh là `"bsd_Action_Active_Approved_Updateestimatehandoverdate_Detail"`.
    *   Tạo chuỗi `listid` bằng cách nối tất cả các ID của các bản ghi chi tiết tìm thấy trong bước 6, phân tách bằng dấu phẩy.
    *   Thiết lập tham số cho request: `listid` và `idmaster` (ID của bản ghi master).
    *   Thực hiện request bằng `service.Execute(request)`.

### RetrieveCurrentUsersSettings(IOrganizationService service)

#### Chức năng tổng quát:
Hàm tiện ích này được sử dụng để truy vấn cài đặt của người dùng đang thực thi Plugin nhằm lấy mã múi giờ (`timezonecode`).

#### Logic nghiệp vụ chi tiết:
1.  **Tạo Query:** Khởi tạo `QueryExpression` nhắm vào thực thể `usersettings`.
2.  **Chọn Cột:** Chỉ định lấy các cột `localeid` và `timezonecode`.
3.  **Lọc:** Thêm `FilterExpression` để đảm bảo chỉ truy vấn cài đặt của người dùng hiện tại bằng cách sử dụng `ConditionOperator.EqualUserId` trên trường `systemuserid`.
4.  **Thực thi:** Thực hiện `service.RetrieveMultiple`.
5.  **Trả về:** Lấy bản ghi đầu tiên trong kết quả và trả về giá trị của thuộc tính `timezonecode` dưới dạng kiểu `int?`.

### RetrieveLocalTimeFromUTCTime(DateTime utcTime)

#### Chức năng tổng quát:
Hàm này chuyển đổi một giá trị thời gian UTC thành thời gian địa phương (Local Time) dựa trên múi giờ của người dùng đang thực thi Plugin.

#### Logic nghiệp vụ chi tiết:
1.  **Lấy Múi giờ:** Gọi `RetrieveCurrentUsersSettings(this.service)` để lấy mã múi giờ của người dùng.
2.  **Kiểm tra Lỗi Múi giờ:** Nếu mã múi giờ trả về là `null`, hàm sẽ ném ra `InvalidPluginExecutionException` với thông báo lỗi.
3.  **Tạo Request Chuyển đổi:** Tạo một `LocalTimeFromUtcTimeRequest`.
4.  **Thiết lập Tham số:**
    *   `TimeZoneCode`: Sử dụng giá trị mã múi giờ đã lấy được.
    *   `UtcTime`: Chuyển đổi thời gian đầu vào (`utcTime`) thành thời gian UTC chuẩn bằng `.ToUniversalTime()`.
5.  **Thực thi:** Thực hiện request chuyển đổi thời gian bằng `this.service.Execute()`.
6.  **Trả về:** Ép kiểu kết quả thành `LocalTimeFromUtcTimeResponse` và trả về thuộc tính `LocalTime`.

### ExistDetail(ref bool result)

#### Chức năng tổng quát:
Hàm này chịu trách nhiệm xác thực xem bản ghi master hiện tại có bất kỳ bản ghi chi tiết liên quan nào (thực thể `bsd_updateestimatehandoverdatedetail`) hay không. Nếu không có, nó sẽ ném ra lỗi để ngăn chặn quá trình xử lý tiếp theo.

#### Logic nghiệp vụ chi tiết:
1.  **Tạo Query:** Khởi tạo `QueryExpression` nhắm vào thực thể chi tiết `"bsd_updateestimatehandoverdatedetail"`.
2.  **Chọn Cột:** Yêu cầu lấy tất cả các cột (`AllColumns = true`).
3.  **Lọc Quan hệ:** Thêm điều kiện lọc để tìm các bản ghi chi tiết có trường lookup `bsd_updateestimatehandoverdate` bằng với ID của bản ghi master hiện tại (`en.Id.ToString()`).
4.  **Truy vấn:** Thực hiện `service.RetrieveMultiple(query)`.
5.  **Xác thực Số lượng:** Kiểm tra số lượng bản ghi chi tiết trả về (`rs.Entities.Count`).
6.  **Xử lý Lỗi:** Nếu số lượng bằng 0:
    *   Ghi trace thông báo lỗi.
    *   Ném ra `InvalidPluginExecutionException` với thông báo: "The record contains an invalid batch. Please check again."
7.  **Trả về:** Nếu tìm thấy ít nhất một bản ghi chi tiết, hàm trả về `EntityCollection` chứa các bản ghi đó. (Lưu ý: Tham số `ref bool result` được khai báo nhưng không được sử dụng để thay đổi giá trị trong logic hiện tại).