# Phân tích mã nguồn: Action_UpdateLandValue_Detail.cs

## Tổng quan

Tệp mã nguồn `Action_UpdateLandValue_Detail.cs` định nghĩa một Plugin (hoặc Custom Action) trong môi trường Microsoft Dynamics 365/Power Platform, được thiết kế để thực hiện các cập nhật phức tạp liên quan đến giá trị đất đai (Land Value) sau khi một bản ghi chi tiết (`bsd_landvalue`) được xử lý hoặc phê duyệt.

Chức năng chính của mã nguồn này là lấy các giá trị mới về giá đất, chiết khấu, thuế, và tổng tiền từ bản ghi chi tiết, sau đó áp dụng các giá trị này để cập nhật các bản ghi liên quan như Hợp đồng/Đơn hàng (Sales Order), Chi tiết Đơn hàng (Sales Order Detail), Kế hoạch trả góp (Installment), và Đơn vị (Unit) liên quan. Plugin này sử dụng cơ chế Impersonation (chạy dưới quyền người dùng khác) và bao gồm logic xử lý lỗi chi tiết.

## Chi tiết các Hàm (Functions/Methods)

### Execute(IServiceProvider serviceProvider)

#### Chức năng tổng quát:
Đây là điểm vào chính của Plugin. Hàm này chịu trách nhiệm khởi tạo các dịch vụ CRM, lấy dữ liệu đầu vào, kiểm tra điều kiện chạy, và thực hiện logic nghiệp vụ phức tạp để cập nhật các bản ghi liên quan dựa trên loại hình cập nhật giá trị đất đai.

#### Logic nghiệp vụ chi tiết:

1.  **Khởi tạo Dịch vụ:**
    *   Lấy các đối tượng ngữ cảnh (context), factory dịch vụ (factory), dịch vụ tổ chức (service), và dịch vụ theo dõi (tracingService) từ `serviceProvider`.
2.  **Lấy Tham số và Impersonation:**
    *   Lấy ID của bản ghi chi tiết (`enDetailid`) và ID người dùng (`userid`) từ `context.InputParameters`.
    *   Tạo `EntityReference` cho người dùng.
    *   **Quan trọng:** Tạo lại đối tượng `service` bằng cách sử dụng ID của người dùng được cung cấp (`user.Id`). Điều này cho phép Plugin chạy dưới quyền của người dùng đó (Impersonation).
    *   Khởi tạo `serviceHelper` để hỗ trợ các thao tác CRUD.
3.  **Truy xuất Dữ liệu Chính:**
    *   Truy xuất bản ghi chi tiết hiện tại (`bsd_landvalue`) bằng `enDetailid` với tất cả các cột (`ColumnSet(true)`).
4.  **Kiểm tra Điều kiện Chạy:**
    *   Gọi hàm `CheckConditionRun(en)`. Nếu hàm này trả về `false`, Plugin ghi log "stop" và thoát. (Lưu ý: Trong mã hiện tại, hàm này luôn trả về `true`).
5.  **Kiểm tra Trạng thái và Loại hình (`bsd_type`):**
    *   Lấy giá trị `statuscode` hiện tại.
    *   Truy xuất lại bản ghi chi tiết vào biến `entity4` để đảm bảo dữ liệu mới nhất.
6.  **Logic Cập nhật theo Loại hình:**

    *   **Trường hợp 1: Loại hình là 100000001 (Type A):**
        *   Ghi log "step1.1".
        *   Cập nhật bản ghi chi tiết: Đặt `statecode` (Trạng thái) là `0` (Active) và `statuscode` (Lý do Trạng thái) là `100000002` (thường là "Đã phê duyệt" hoặc "Hoàn thành").

    *   **Trường hợp 2: Loại hình là 100000000 (Type B):**
        *   Ghi log "step1.2".
        *   **Cập nhật Trạng thái Chi tiết:** Cập nhật bản ghi chi tiết tương tự như Type A (`statecode=0`, `statuscode=100000002`).
        *   **Kiểm tra Điều kiện Liên kết:** Kiểm tra xem trường `bsd_optionentry` có tồn tại không. Nếu không, thoát khỏi hàm.
        *   **Trích xuất Giá trị Mới:** Trích xuất 8 giá trị tiền tệ mới (ví dụ: `bsd_listedpricenew`, `bsd_netsellingpricenew`, `bsd_totalamountnew`, v.v.) từ `entity4`. Nếu trường không tồn tại, giá trị mặc định là 0M.
        *   **Cập nhật Bản ghi Option Entry (Header):**
            *   Lấy `EntityReference` của `bsd_optionentry`.
            *   Truy xuất bản ghi Option Entry (`entity5`) với 5 cột cần thiết.
            *   Cập nhật 4 trường tiền tệ trên bản ghi Option Entry (`entity5`) bằng các giá trị mới đã trích xuất (ví dụ: `bsd_landvaluededuction`, `totaltax`, `bsd_freightamount`, `totalamount`).
        *   **Cập nhật Chi tiết Đơn hàng (Sales Order Detail):**
            *   Sử dụng `FetchExpression` để truy vấn bản ghi `salesorderdetail` liên quan đến `entity5.Id` (ID của Option Entry/Sales Order).
            *   Lấy ID của bản ghi chi tiết đầu tiên tìm thấy.
            *   Cập nhật bản ghi chi tiết đó: Đặt `tax` bằng `num6` (Total VAT Tax New) và `extendedamount` bằng tổng của `num1` (Listed Price New) cộng với `num6` (Total VAT Tax New).
        *   **Cập nhật Kế hoạch Trả góp (Installment):**
            *   Kiểm tra xem trường `bsd_installment` có tồn tại không.
            *   Lấy giá trị `bsd_amountofthisphase` (số tiền của giai đoạn này) mới (`num9`).
            *   Nếu `num9` lớn hơn 0:
                *   Truy xuất bản ghi Trả góp (`entity6`).
                *   Cập nhật trường `bsd_amountofthisphase` trên bản ghi trả góp bằng `num9`.
                *   **Tính toán Số dư (`bsd_balance`):** Tính toán số dư mới dựa trên công thức: `bsd_amountofthisphase - bsd_depositamount - bsd_amountwaspaid - bsd_waiverinstallment`.
                *   Cập nhật bản ghi trả góp với số tiền giai đoạn mới và số dư mới.
7.  **Cập nhật Giá trị Đất đai Đơn vị (Unit):**
    *   Kiểm tra xem trường `bsd_units` có tồn tại không.
    *   Truy xuất bản ghi Đơn vị (`entity7`).
    *   Cập nhật trường `bsd_landvalueofunit` trên bản ghi Đơn vị bằng giá trị đất đai mới (`bsd_landvaluenew`) từ bản ghi chi tiết.
8.  **Xử lý Lỗi:**
    *   Nếu bất kỳ lỗi nào xảy ra trong khối `try`, khối `catch` sẽ gọi hàm `HandleError` để ghi lại thông báo lỗi và cập nhật trạng thái lỗi trên cả bản ghi chi tiết và bản ghi tổng thể (Master).

### HandleError(Entity item, string error)

#### Chức năng tổng quát:
Hàm này được gọi khi có lỗi xảy ra trong quá trình thực thi nghiệp vụ. Nó chịu trách nhiệm ghi lại thông báo lỗi và đánh dấu trạng thái lỗi trên cả bản ghi chi tiết và bản ghi tổng thể liên quan.

#### Logic nghiệp vụ chi tiết:

1.  **Cập nhật Bản ghi Chi tiết (Detail):**
    *   Tạo một đối tượng Entity để cập nhật bản ghi chi tiết (`item`).
    *   Đặt trường `bsd_errordetail` bằng thông báo lỗi (`error`).
    *   Đặt `statuscode` là `100000005` (thường là trạng thái "Lỗi").
    *   Thực hiện lệnh `service.Update` cho bản ghi chi tiết.
2.  **Cập nhật Bản ghi Tổng thể (Master):**
    *   Lấy tham chiếu đến bản ghi tổng thể (`bsd_updatelandvalue`) từ bản ghi chi tiết.
    *   Tạo một đối tượng Entity cho bản ghi tổng thể.
    *   Đặt cờ lỗi: `bsd_error = true`.
    *   Đặt thông báo lỗi chung: `bsd_errordetail = "Error exist. Please check (View Error) for detail"`.
    *   Đặt cờ xử lý: `bsd_processing_pa = false`.
    *   Thực hiện lệnh `serviceHelper.Update` cho bản ghi tổng thể.

### CheckConditionRun(Entity item)

#### Chức năng tổng quát:
Hàm này được thiết kế để kiểm tra các điều kiện tiên quyết trước khi thực hiện logic cập nhật chính.

#### Logic nghiệp vụ chi tiết:

1.  **Logic Hiện tại:**
    *   Hàm hiện tại được **hardcode** để luôn trả về `true`, cho phép Plugin luôn chạy bất kể trạng thái của bản ghi.
2.  **Logic Dự kiến (Đã bị chú thích):**
    *   Phần mã bị chú thích cho thấy ý định ban đầu là truy xuất bản ghi tổng thể (`bsd_updatelandvalue`).
    *   Nó sẽ kiểm tra nếu bản ghi tổng thể đã có lỗi (`bsd_error == true`) VÀ không đang trong quá trình xử lý (`bsd_processing_pa == false`). Nếu điều kiện này đúng, hàm sẽ trả về `false` (ngăn không cho Plugin chạy). Ngược lại, nó sẽ trả về `true`.