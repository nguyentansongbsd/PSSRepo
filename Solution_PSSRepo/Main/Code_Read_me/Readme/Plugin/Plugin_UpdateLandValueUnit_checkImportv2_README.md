# Phân tích mã nguồn: Plugin_UpdateLandValueUnit_checkImportv2.cs

## Tổng quan

Tệp mã nguồn `Plugin_UpdateLandValueUnit_checkImportv2.cs` chứa một Plugin Microsoft Dynamics 365/CRM được viết bằng C#. Plugin này được thiết kế để thực thi logic nghiệp vụ và các kiểm tra xác thực (validation) khi một bản ghi mới được tạo (`Create`) hoặc một bản ghi hiện có được cập nhật (`Update`) trên một thực thể (entity) liên quan đến việc cập nhật giá trị đất (Land Value Unit).

Chức năng chính của Plugin là thiết lập các giá trị mặc định, sao chép dữ liệu tài chính từ các thực thể liên quan (như Unit và Option Entry), và ngăn chặn các thao tác nếu các điều kiện trạng thái (status code) của các bản ghi liên quan không hợp lệ.

## Chi tiết các Hàm (Functions/Methods)

### Execute(IServiceProvider serviceProvider)

#### Chức năng tổng quát

Đây là điểm vào bắt buộc của Plugin CRM, chịu trách nhiệm khởi tạo các dịch vụ cần thiết (Service, Context, Tracing) và thực thi logic nghiệp vụ chính dựa trên loại thông điệp (Message Name) đang kích hoạt Plugin (Create hoặc Update).

#### Logic nghiệp vụ chi tiết

1.  **Khởi tạo Dịch vụ:**
    *   Hàm nhận `serviceProvider` và sử dụng nó để lấy các đối tượng cốt lõi của CRM: `IPluginExecutionContext` (context), `IOrganizationServiceFactory` (factory), `ITracingService` (traceService).
    *   Sử dụng factory để tạo `IOrganizationService` (service) với quyền của người dùng hiện tại (`context.UserId`).
    *   Lấy thực thể đầu vào (`Target`) từ `context.InputParameters`.
    *   Ghi dấu vết ("start") để theo dõi quá trình thực thi.

2.  **Xử lý Sự kiện "Create" (Tạo mới bản ghi):**

    *   **Kiểm tra 1: Thiết lập Giá trị Đất Cũ (`bsd_landvalueold`):**
        *   Kiểm tra xem thực thể đầu vào có chứa trường tham chiếu `bsd_units` hay không.
        *   Nếu có, truy xuất bản ghi Unit được tham chiếu.
        *   Lấy giá trị đất hiện tại của Unit (`bsd_landvalueofunit`). Nếu trường này không tồn tại, mặc định là 0.
        *   Gán giá trị này cho trường `bsd_landvalueold` trên thực thể Target.

    *   **Kiểm tra 2: Xác thực Trạng thái Cập nhật Giá trị Đất (`bsd_updatelandvalue`):**
        *   Kiểm tra xem thực thể đầu vào có chứa trường tham chiếu `bsd_updatelandvalue` hay không.
        *   Nếu có, truy xuất bản ghi Update Land Value được tham chiếu.
        *   Kiểm tra `statuscode` của bản ghi được tham chiếu.
        *   Nếu `statuscode` bằng `100000001`, Plugin sẽ ném ra lỗi `InvalidPluginExecutionException` với thông báo: "Status of Update Land Value is invalid. Please check again." (Ngăn chặn việc tạo bản ghi nếu bản ghi liên quan có trạng thái không hợp lệ).

    *   **Kiểm tra 3: Chuẩn hóa Giá trị Đất Mới (`bsd_landvaluenew`):**
        *   Kiểm tra xem thực thể đầu vào có chứa `bsd_landvaluenew` hay không.
        *   Lấy giá trị này (hoặc 0 nếu không tồn tại) và gán lại cho chính trường `bsd_landvaluenew` dưới dạng đối tượng `Money` (đảm bảo kiểu dữ liệu chính xác).

    *   **Kiểm tra 4: Xử lý Chi tiết Option Entry (Nếu `bsd_type` là 100000000):**
        *   Thực hiện khối logic này chỉ khi `bsd_type` là `100000000` VÀ trường tham chiếu `bsd_optionentry` tồn tại.
        *   Truy xuất bản ghi Option Entry được tham chiếu.
        *   **Xác thực Trạng thái Option Entry:** Kiểm tra `statuscode` của Option Entry. Nếu `statuscode` bằng `100000006` (Terminated), ném ra lỗi `InvalidPluginExecutionException`: "Option Entry is Terminated. Please check again."
        *   **Sao chép Dữ liệu Tài chính (Current Values):**
            *   Truy xuất 8 trường tiền tệ (Money) từ Option Entry (`bsd_detailamount`, `bsd_discount`, `bsd_packagesellingamount`, `bsd_totalamountlessfreight`, `bsd_landvaluededuction`, `totaltax`, `bsd_freightamount`, `totalamount`). Mặc định là 0 nếu thiếu.
            *   Gán 8 giá trị này vào các trường "Current" tương ứng trên thực thể Target (`bsd_listedpricecurrent`, `bsd_discountcurrent`, v.v.).
        *   **Sao chép Dữ liệu Tài chính (New Values):**
            *   Gán 4 giá trị đầu tiên (Listed Price, Discount, Handover Condition Amount, Net Selling Price) vào các trường "New" tương ứng trên thực thể Target.
        *   **Tìm kiếm Chi tiết Kế hoạch Thanh toán (`bsd_paymentschemedetail`):**
            *   Thực hiện truy vấn FetchXML để tìm các bản ghi `bsd_paymentschemedetail` liên quan đến `bsd_optionentry` hiện tại, với điều kiện `bsd_duedatecalculatingmethod` bằng `100000002`.
            *   Nếu tìm thấy ít nhất một bản ghi:
                *   Thiết lập trường `bsd_installment` trên Target bằng tham chiếu đến bản ghi chi tiết đầu tiên tìm thấy.
                *   Lấy giá trị `bsd_amountofthisphase` từ bản ghi chi tiết đó và gán cho `bsd_amountofthisphasecurrent` trên Target.

3.  **Xử lý Sự kiện "Update" (Cập nhật bản ghi):**

    *   Plugin chỉ thực thi logic này nếu `context.MessageName` là "Update".
    *   Truy xuất ảnh trước khi cập nhật (`PreEntityImages["preimage"]`).
    *   Lấy `statuscode` và `statecode` cũ từ `preimage` (`num1`, `num2`).
    *   Lấy `statuscode` và `statecode` mới từ `inputParameter`.
    *   **Xác thực Trạng thái:**
        *   Kiểm tra nếu `statecode` mới là `1` (thường là trạng thái Active/Hoạt động) VÀ `statuscode` cũ (`num1`) là `100000002`.
        *   Nếu điều kiện này đúng, ném ra lỗi `InvalidPluginExecutionException`: "Status of Update Land Value is invalid. Please check again." (Ngăn chặn việc kích hoạt bản ghi nếu trạng thái trước đó là một trạng thái không cho phép chuyển đổi).