# Action: Hoàn thành Chi tiết Cuộc họp Thu nợ (Action_CollectionMeeting_Complete_Detail)

## Mô tả tổng quan

Plugin này được kích hoạt khi một hành động (Action) được gọi từ hệ thống Dynamics 365. Nhiệm vụ chính của nó là xử lý việc hoàn thành một bản ghi "Follow Up List" (`bsd_followuplist`) liên quan đến một cuộc họp thu nợ (Collection Meeting).

Khi một "Follow Up List" được đánh dấu là hoàn thành, plugin sẽ thực hiện các logic nghiệp vụ khác nhau tùy thuộc vào loại (`bsd_type`) của bản ghi đó. Các logic này bao gồm cập nhật các bản ghi liên quan như Reservation, Option Entry, Installment, hoặc tạo ra các bản ghi mới như Termination và Terminate Letter.

Plugin cũng bao gồm cơ chế xử lý lỗi (error handling) để đảm bảo rằng nếu có bất kỳ lỗi nào xảy ra trong quá trình xử lý, thông tin lỗi sẽ được ghi lại và hệ thống có thể phục hồi một cách an toàn.

## Logic chi tiết của từng Function

### `Execute(IServiceProvider serviceProvider)`

Đây là hàm chính của plugin, được gọi khi Action được kích hoạt.

1.  **Khởi tạo:**
    *   Lấy các đối tượng cần thiết từ `serviceProvider` như `IPluginExecutionContext`, `IOrganizationServiceFactory`, `IOrganizationService`, và `ITracingService`.
    *   Cho phép thực thi dưới danh nghĩa của một người dùng cụ thể nếu `userid` được cung cấp trong `InputParameters`.

2.  **Lấy bản ghi mục tiêu:**
    *   Kiểm tra xem `InputParameters` có chứa `id` của bản ghi `bsd_followuplist` không.
    *   Nếu có, truy xuất toàn bộ thông tin của bản ghi `bsd_followuplist` đó.

3.  **Xử lý chính (trong khối `try...catch`):**
    *   Lấy ngày giờ hiện tại theo múi giờ của người dùng.
    *   Kiểm tra xem bản ghi `bsd_followuplist` đã ở trạng thái "Complete" chưa. Nếu chưa, nó sẽ:
        *   Cập nhật `statuscode` của bản ghi `bsd_followuplist` thành "Complete" (100000000).
        *   Sau đó, thực hiện các hành động cụ thể dựa trên giá trị của trường `bsd_type`:

            *   **Nếu `bsd_type` là "Reservation - Sign off RF" (100000000):**
                *   Cập nhật ngày hết hạn ký (`bsd_signingexpired`) trên bản ghi Reservation liên quan.
                *   Chuyển trạng thái của Reservation sang "Deposited".

            *   **Nếu `bsd_type` là "Option entry - 1St installment" (100000002) hoặc "Option entry - Installment" (100000004):**
                *   Cập nhật ngày đến hạn (`bsd_duedate`) trên bản ghi Installment liên quan.

            *   **Nếu `bsd_type` là "Reservation - Terminate" (100000005):**
                *   **Tạo Termination:** Nếu trường `bsd_termination` được đánh dấu là `true`, một bản ghi `bsd_termination` mới sẽ được tạo ra. Các giá trị như tiền phạt (`forfeiture`), tiền hoàn lại (`refund`) được tính toán dựa trên các trường trong Follow Up List.
                *   **Tạo Terminate Letter:** Nếu `bsd_terminateletter` là `true` (và không phải là tạo Termination), một bản ghi `bsd_terminateletter` sẽ được tạo. Đồng thời, một bản sao (copy) của Follow Up List hiện tại cũng được tạo ra để lưu trữ.

            *   **Nếu `bsd_type` là "Option entry - Termination" (100000006):**
                *   Tương tự như "Reservation - Terminate", nhưng áp dụng cho bản ghi `Option Entry`.
                *   Nó cũng sẽ tạo bản ghi `bsd_termination` hoặc `bsd_terminateletter` dựa trên các điều kiện tương ứng.

4.  **Xử lý lỗi:**
    *   Nếu có bất kỳ ngoại lệ (exception) nào xảy ra trong khối `try`, hàm `HandleError` sẽ được gọi.

### `HandleError(Entity item, string error)`

Hàm này được gọi khi có lỗi xảy ra trong quá trình xử lý của hàm `Execute`.

1.  **Ghi log lỗi:** Ghi lại thông báo lỗi chi tiết bằng `tracingService`.
2.  **Cập nhật bản ghi cha:**
    *   Cập nhật bản ghi `appointment` (Collection Meeting) liên quan, đánh dấu là có lỗi (`bsd_error = true`) và ghi lại chi tiết lỗi (`bsd_errordetail`).
3.  **Cập nhật Follow Up List:**
    *   Cập nhật `statuscode` của bản ghi `bsd_followuplist` hiện tại thành "Error" (100000001).
    *   Ghi lại chi tiết lỗi vào trường `bsd_errordetail`.
4.  **Tạo bản sao:**
    *   Tạo một bản sao của bản ghi `bsd_followuplist` bị lỗi. Bản sao này được đánh dấu là `bsd_copy = true` và `bsd_system = true` để dễ dàng theo dõi và xử lý lại nếu cần.

### `find_phase(IOrganizationService service, EntityReference phase)`

Một hàm tiện ích để tìm kiếm bản ghi `bsd_phaseslaunch`.

*   **Mục đích:** Kiểm tra xem một `Phase Launch` có đang ở trạng thái khác "Launched" hay không.
*   **Đầu vào:** `IOrganizationService` và `EntityReference` của `Phase Launch`.
*   **Logic:** Sử dụng FetchXML để truy vấn các bản ghi `bsd_phaseslaunch` có ID trùng khớp và có `statuscode` khác "Launched" (100000000).
*   **Đầu ra:** Trả về một `EntityCollection` chứa các bản ghi tìm được.

### `CloneEntity(Entity input)`

Một hàm tiện ích để tạo một bản sao (clone) của một Entity.

*   **Mục đích:** Sao chép tất cả các thuộc tính từ một entity này sang một entity mới.
*   **Logic:** Tạo một `Entity` mới với cùng `LogicalName` và lặp qua tất cả các thuộc tính (`Attributes`) của entity đầu vào để sao chép chúng sang entity mới.
*   **Lưu ý:** Đây là một bản sao nông (shallow copy).

### `RetrieveLocalTimeFromUTCTime(DateTime utcTime, IOrganizationService service)`

Một hàm tiện ích để chuyển đổi thời gian từ múi giờ UTC sang múi giờ địa phương của người dùng đang thực thi.

*   **Mục đích:** Đảm bảo rằng các giá trị ngày tháng được xử lý và hiển thị một cách chính xác theo múi giờ của người dùng.
*   **Logic:**
    1.  Truy vấn `usersettings` của người dùng hiện tại để lấy `timezonecode`.
    2.  Sử dụng `LocalTimeFromUtcTimeRequest` để yêu cầu Dynamics 365 chuyển đổi `utcTime` đã cho sang giờ địa phương.
    3.  Trả về giá trị `DateTime` đã được chuyển đổi.
