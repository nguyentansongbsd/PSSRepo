# Phân tích mã nguồn: Action_CollectionMeeting_Complete_Detail.cs

## Tổng quan

Tệp mã nguồn `Action_CollectionMeeting_Complete_Detail.cs` chứa một Plugin CRM (Dynamics 365) được thiết kế để xử lý hành động hoàn thành (Complete) cho các bản ghi chi tiết của Collection Meeting, được gọi là `bsd_followuplist`.

Plugin này hoạt động như một hành động tùy chỉnh (Custom Action) hoặc được kích hoạt thông qua một thông điệp (Message) cụ thể, nhận ID của bản ghi `bsd_followuplist` làm tham số đầu vào. Nhiệm vụ chính của nó là đánh dấu bản ghi chi tiết là hoàn thành và sau đó thực hiện các logic nghiệp vụ phức tạp liên quan đến các thực thể khác như Reservation, Option Entry, Installment, Termination, và Terminate Letter, tùy thuộc vào loại hình (`bsd_type`) của Follow Up List. Plugin cũng bao gồm cơ chế xử lý lỗi, tạo bản sao của Follow Up List khi có lỗi hoặc khi cần thiết cho quy trình nghiệp vụ.

## Chi tiết các Hàm (Functions/Methods)

### Execute(IServiceProvider serviceProvider)

#### Chức năng tổng quát:

Đây là điểm vào chính của Plugin. Hàm này khởi tạo các dịch vụ CRM cần thiết, xác định người dùng thực thi, truy xuất bản ghi `bsd_followuplist` mục tiêu, và thực hiện logic cập nhật trạng thái cũng như các hành động nghiệp vụ liên quan (cập nhật ngày hết hạn, tạo bản ghi Termination/Terminate Letter) dựa trên loại hình của bản ghi.

#### Logic nghiệp vụ chi tiết:

1.  **Khởi tạo Dịch vụ:** Lấy các đối tượng `IPluginExecutionContext`, `IOrganizationServiceFactory`, `IOrganizationService`, và `ITracingService` từ `serviceProvider`.
2.  **Kiểm tra Impersonation:** Kiểm tra xem tham số đầu vào có chứa `userid` hay không. Nếu có, tạo một `IOrganizationService` mới (Impersonation) để thực thi logic dưới quyền của người dùng được chỉ định.
3.  **Truy xuất Target:** Kiểm tra xem tham số đầu vào có chứa `id` (ID của `bsd_followuplist`) hay không. Nếu có, truy xuất toàn bộ bản ghi `bsd_followuplist` mục tiêu (`ful`).
4.  **Xác định Thời gian:** Gọi hàm `RetrieveLocalTimeFromUTCTime` để lấy ngày giờ hiện tại theo múi giờ địa phương của người dùng CRM (`today`).
5.  **Xác thực Trạng thái:** Kiểm tra nếu bản ghi `ful` có trường `bsd_type` VÀ trạng thái hiện tại (`statuscode`) KHÔNG phải là Complete (giá trị 100000000).
6.  **Cập nhật Trạng thái FUL:** Nếu điều kiện trên được thỏa mãn, cập nhật trạng thái (`statuscode`) của bản ghi `bsd_followuplist` hiện tại thành Complete (100000000).
7.  **Logic theo Loại hình (`bsd_type`):**

    *   **Loại 100000000 (Reservation - Sign off RF):**
        *   Truy xuất bản ghi Reservation liên quan.
        *   Kiểm tra các điều kiện: Reservation đã được in (`bsd_reservationprinteddate`), trạng thái là 100000005, có ngày hết hạn ký (`bsd_signingexpired`) và FUL có `bsd_expiredate`.
        *   So sánh ngày hết hạn mới (`bsd_expiredate` trên FUL) với ngày hết hạn cũ (`bsd_signingexpired` trên Reservation).
        *   Nếu ngày hết hạn mới lớn hơn ngày hết hạn cũ, thực hiện:
            *   Sử dụng `SetStateRequest` để chuyển trạng thái Reservation về Active (State=0, Status=100000000).
            *   Cập nhật trường `bsd_signingexpired` trên Reservation bằng giá trị `bsd_expiredate` từ FUL.
            *   Sử dụng `SetStateRequest` để chuyển trạng thái Reservation thành Deposited (State=1, Status=3).

    *   **Loại 100000002 hoặc 100000004 (Option entry - 1St installment OR Installment):**
        *   Nếu FUL chứa `bsd_optionentry`, `bsd_installment`, và `bsd_expiredate`, cập nhật trường `bsd_duedate` của bản ghi Installment liên quan bằng giá trị `bsd_expiredate` từ FUL.

    *   **Loại 100000005 (Reservation - Terminate):**
        *   Truy xuất bản ghi Reservation liên quan.
        *   **Tạo Termination:** Nếu trường `bsd_termination` là `true`:
            *   Tạo bản ghi `bsd_termination` mới.
            *   Thiết lập tên, ngày chấm dứt (`bsd_terminationdate` = `today`), nguồn (`bsd_source` = 100000000), liên kết với FUL, Reservation, và Units.
            *   Tính toán số tiền bị tịch thu (Forfeiture) và số tiền hoàn lại (Refund) dựa trên `bsd_takeoutmoney`:
                *   Nếu `bsd_takeoutmoney` = 100000001 (Forfeiture theo phần trăm): Tính `totalForfeiture` = `percent` * `paid` / 100. `bsd_refundamount` = `totalpaid` - `totalForfeiture`.
                *   Nếu `bsd_takeoutmoney` = 100000000 (Forfeiture theo số tiền cố định): `totalForfeiture` = `amount`. `bsd_refundamount` = `totalpaid` - `totalForfeiture`.
            *   Kiểm tra logic bán lại (`bsd_resell`): Nếu `bsd_resell` là `true`, gọi `find_phase` để đảm bảo Phase Launch liên quan đang ở trạng thái Launched (statuscode = 100000000). Nếu không, ném ra lỗi.
            *   Tạo bản ghi Termination.
        *   **Tạo Terminate Letter:** Nếu `bsd_terminateletter` là `true` VÀ KHÔNG tạo Termination:
            *   Tạo bản ghi `bsd_terminateletter` mới.
            *   Thiết lập các trường liên quan (Reservation, Customer, Project, Units, Forfeiture amount).
            *   Tạo bản ghi Terminate Letter.
            *   **Tạo bản sao FUL:** Gọi `CloneEntity` để tạo bản sao của FUL, đặt tên là "Copy", đặt `bsd_copy=true`, `bsd_system=true`, và `statuscode=1` (Active/Open), sau đó tạo bản ghi này.

    *   **Loại 100000006 (Option entry - Termination):**
        *   Truy xuất bản ghi Option Entry (OE) liên quan.
        *   **Tạo Termination:** Nếu trường `bsd_termination` là `true`:
            *   Tạo bản ghi `bsd_termination` mới, thiết lập nguồn (`bsd_source` = 100000001).
            *   Tính toán Forfeiture/Refund tương tự như trên, nhưng cộng thêm `bsd_maintenancefeepaid` và `bsd_managementfeepaid` vào số tiền hoàn lại (`bsd_refundamount` và `bsd_receivedamount`).
            *   Kiểm tra logic bán lại (`bsd_resell`) và Phase Launch tương tự như loại 100000005.
            *   Tạo bản ghi Termination.
        *   **Tạo Terminate Letter & Copy FUL:** Nếu `bsd_terminateletter` là `true` VÀ KHÔNG tạo Termination, thực hiện tạo Terminate Letter và tạo bản sao FUL tương tự như loại 100000005 (loại bỏ thêm trường `createdon` khi clone).

8.  **Xử lý Ngoại lệ (Catch):** Nếu bất kỳ bước nào trong khối `try` gặp lỗi, gọi hàm `HandleError` để ghi lại thông tin lỗi và tạo bản sao FUL với trạng thái lỗi.

### HandleError(Entity item, string error)

#### Chức năng tổng quát:

Hàm này được gọi khi có lỗi xảy ra trong quá trình thực thi chính. Nó chịu trách nhiệm ghi lại chi tiết lỗi vào cả bản ghi Collection Meeting cha và bản ghi FollowUpList chi tiết, đồng thời tạo một bản sao của FollowUpList để theo dõi lỗi.

#### Logic nghiệp vụ chi tiết:

1.  **Cập nhật Collection Meeting (Cha):** Truy xuất EntityReference của Collection Meeting cha (`bsd_collectionmeeting`) và cập nhật bản ghi đó (`appointment`) với:
    *   `bsd_error` = `true`.
    *   `bsd_errordetail` = thông tin lỗi (`error`).
2.  **Cập nhật FollowUpList (Chi tiết):** Cập nhật bản ghi `bsd_followuplist` hiện tại với:
    *   `statuscode` = 100000001 (Error).
    *   `bsd_errordetail` = thông tin lỗi (`error`).
3.  **Tạo bản sao FUL lỗi:**
    *   Lấy thời gian địa phương hiện tại (`today`).
    *   Gọi `CloneEntity` để tạo bản sao của `item` (FUL gốc).
    *   Loại bỏ các trường ID (`bsd_followuplistid`) và liên kết cha (`bsd_collectionmeeting`).
    *   Thiết lập các trường: `bsd_name` (giữ nguyên tên), `bsd_date` = `today`, `bsd_copy` = `true`, `bsd_system` = `true`, và `statuscode` = 1 (Active/Open).
    *   Tạo bản ghi FollowUpList bản sao này.

### find_phase(IOrganizationService service, EntityReference phase)

#### Chức năng tổng quát:

Hàm tiện ích này thực hiện truy vấn FetchXML để kiểm tra trạng thái của một bản ghi Phase Launch cụ thể. Nó tìm kiếm các bản ghi Phase Launch có ID được cung cấp nhưng KHÔNG ở trạng thái Launched.

#### Logic nghiệp vụ chi tiết:

1.  **Xây dựng FetchXML:** Tạo một chuỗi FetchXML để truy vấn thực thể `bsd_phaseslaunch`.
2.  **Điều kiện Lọc:** Truy vấn tìm kiếm các bản ghi thỏa mãn hai điều kiện:
    *   `bsd_phaseslaunchid` bằng ID của `phase` được truyền vào.
    *   `statuscode` KHÔNG bằng 100000000 (trạng thái Launched).
3.  **Thực thi Truy vấn:** Thực thi FetchXML bằng `service.RetrieveMultiple`.
4.  **Trả về:** Trả về `EntityCollection` chứa các bản ghi Phase Launch không ở trạng thái Launched. (Mục đích sử dụng trong `Execute` là để ném lỗi nếu truy vấn này trả về kết quả, đảm bảo rằng việc bán lại chỉ được thực hiện nếu Phase Launch đã Launched).

### CloneEntity(Entity input)

#### Chức năng tổng quát:

Hàm tiện ích này tạo một bản sao nông (shallow copy) của một Entity CRM, sao chép tất cả các thuộc tính hiện có từ Entity đầu vào sang Entity đầu ra.

#### Logic nghiệp vụ chi tiết:

1.  **Khởi tạo:** Tạo một Entity mới (`outPut`) cùng loại logic (`LogicalName`) với Entity đầu vào (`input`).
2.  **Sao chép Thuộc tính:** Lặp qua tất cả các khóa thuộc tính (`Attributes.Keys`) trong Entity đầu vào.
3.  **Gán Giá trị:** Gán giá trị của từng thuộc tính từ `input` sang `outPut`.
4.  **Trả về:** Trả về Entity mới đã được sao chép.

### RetrieveLocalTimeFromUTCTime(DateTime utcTime, IOrganizationService service)

#### Chức năng tổng quát:

Hàm tiện ích này chuyển đổi một giá trị thời gian từ múi giờ UTC sang múi giờ địa phương của người dùng CRM hiện tại đang thực thi Plugin.

#### Logic nghiệp vụ chi tiết:

1.  **Truy vấn Cài đặt Người dùng:** Thực hiện truy vấn FetchExpression trên thực thể `usersettings` để lấy `timezonecode` của người dùng hiện tại.
2.  **Xác thực Time Zone:** Kiểm tra xem `timezonecode` có tồn tại hay không. Nếu không, ném ra ngoại lệ.
3.  **Tạo Request:** Khởi tạo `LocalTimeFromUtcTimeRequest`, truyền vào `TimeZoneCode` và thời gian UTC đầu vào.
4.  **Thực thi Request:** Thực thi request thông qua `service.Execute()`.
5.  **Trả về:** Trả về `LocalTime` từ response, đây là thời gian đã được chuyển đổi sang múi giờ địa phương.