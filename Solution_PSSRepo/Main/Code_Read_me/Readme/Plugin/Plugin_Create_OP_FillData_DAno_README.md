# Phân tích mã nguồn: Plugin_Create_OP_FillData_DAno.cs

## Tổng quan

Tệp mã nguồn `Plugin_Create_OP_FillData_DAno.cs` chứa một Plugin tùy chỉnh được phát triển cho Microsoft Dynamics 365/CRM. Plugin này thực thi logic nghiệp vụ sau khi một bản ghi (Entity) được tạo hoặc cập nhật (dựa trên việc sử dụng `context.InputParameters["Target"]`).

Mục đích chính của Plugin là tự động tính toán và điền giá trị cho trường tùy chỉnh `bsd_dano` (có thể là một mã số hoặc số đăng ký) dựa trên tên Dự án (`bsd_project`) và Số Option (`bsd_optionno`) của bản ghi đó. Logic tính toán sử dụng các quy tắc định dạng chuỗi khác nhau tùy thuộc vào tên dự án.

Plugin này triển khai giao diện `IPlugin` và sử dụng các dịch vụ tiêu chuẩn của CRM như `IOrganizationService` để thao tác dữ liệu và `ITracingService` để ghi log.

## Chi tiết các Hàm (Functions/Methods)

### Execute(IServiceProvider serviceProvider)

#### Chức năng tổng quát:

Là điểm vào chính của Plugin, chịu trách nhiệm khởi tạo các dịch vụ CRM cần thiết (Context, Factory, Service, Tracing) và chuẩn bị dữ liệu Entity để gọi hàm xử lý logic nghiệp vụ chính.

#### Logic nghiệp vụ chi tiết:

1.  **Khởi tạo Dịch vụ:** Hàm bắt đầu bằng việc lấy và gán các đối tượng dịch vụ tiêu chuẩn của CRM từ `serviceProvider`:
    *   `IPluginExecutionContext` (context): Chứa thông tin về sự kiện đang được thực thi.
    *   `IOrganizationServiceFactory` (factory): Dùng để tạo `IOrganizationService`.
    *   `IOrganizationService` (service): Dùng để thao tác với dữ liệu CRM (Retrieve, Update).
    *   `ITracingService` (tracingService): Dùng để ghi log gỡ lỗi.
2.  **Lấy Target Entity:** Lấy đối tượng Entity đang được xử lý từ `context.InputParameters["Target"]`.
3.  **Retrieve Entity Đầy Đủ:** Sử dụng `service.Retrieve` để lấy lại toàn bộ dữ liệu của Entity vừa được tạo hoặc cập nhật (`enCreated`). Việc này đảm bảo rằng plugin có thể truy cập tất cả các trường, kể cả những trường không có trong Target Entity ban đầu (sử dụng `new Microsoft.Xrm.Sdk.Query.ColumnSet(true)`).
4.  **Gọi Logic Chính:** Cuối cùng, hàm gọi phương thức `FillBsdDaNoField`, truyền vào Entity đầy đủ (`enCreated`), `service`, và `tracingService` để thực hiện việc điền dữ liệu.

### FillBsdDaNoField(Entity entity, IOrganizationService service, ITracingService tracingService)

#### Chức năng tổng quát:

Thực hiện logic nghiệp vụ chính: tính toán giá trị chuỗi cho trường `bsd_dano` dựa trên tên dự án và số option, sau đó cập nhật bản ghi Entity trong CRM.

#### Logic nghiệp vụ chi tiết:

1.  **Ghi Log Bắt đầu:** Ghi log "Start FillBsdDaNoField" bằng `tracingService`.
2.  **Thu thập Dữ liệu Đầu vào:**
    *   Lấy tên dự án (`projectName`) từ trường tham chiếu `bsd_project`. Nếu trường này tồn tại, lấy thuộc tính `Name` của EntityReference; nếu không, gán là `null`.
    *   Lấy số option (`optionNo`) từ trường chuỗi `bsd_optionno`. Nếu trường này tồn tại, lấy giá trị chuỗi; nếu không, gán là `null`.
3.  **Kiểm tra Điều kiện Bỏ qua:** Kiểm tra nếu `projectName` hoặc `optionNo` là null hoặc rỗng. Nếu thiếu một trong hai, plugin ghi log và thoát khỏi hàm (`return`).
4.  **Xử lý Logic theo Tên Dự Án (Switch Case):**
    *   Tên dự án được chuẩn hóa bằng cách loại bỏ khoảng trắng thừa (`Trim()`) và chuyển sang chữ hoa (`ToUpper()`) để đảm bảo so sánh chính xác.
    *   **Case "HERITAGE WEST LAKE":** `bsdDaNo` được tạo bằng cách lấy 3 ký tự bên trái của `optionNo`, nối với "/", và 4 ký tự bên phải của `optionNo`.
    *   **Case "LUMI SIGNATURE":** `bsdDaNo` được tạo bằng chuỗi cố định "LUMI/" nối với 4 ký tự bên phải của `optionNo`.
    *   **Case "LUMI PRESTIGE":** `bsdDaNo` được tạo bằng chuỗi cố định "LUMI PRESTIGE/" nối với 4 ký tự bên phải của `optionNo`.
    *   **Case "SENIQUE I&II":** `bsdDaNo` được tạo bằng chuỗi cố định "SENIQUE/" nối với 4 ký tự bên phải của `optionNo`.
    *   **Case "SENIQUE PREMIER":** `bsdDaNo` được tạo bằng chuỗi cố định "SENIQUE PREMIER/" nối với 4 ký tự bên phải của `optionNo`.
    *   **Default (Mặc định):** Nếu tên dự án không khớp với bất kỳ trường hợp nào trên, áp dụng quy tắc chung: 8 ký tự bên trái của `optionNo`, nối với "/", và 4 ký tự bên phải của `optionNo`.
5.  **Gán và Cập nhật:**
    *   Gán giá trị `bsdDaNo` đã tính toán vào trường `entity["bsd_dano"]`.
    *   Thực hiện lệnh `service.Update(entity)` để lưu thay đổi của trường `bsd_dano` lên cơ sở dữ liệu CRM.
6.  **Xử lý Lỗi:** Bao bọc toàn bộ logic trong khối `try-catch` để ghi lại bất kỳ ngoại lệ nào xảy ra và ném lại lỗi để CRM xử lý.

### Left(string input, int length)

#### Chức năng tổng quát:

Hàm tiện ích dùng để trích xuất một số lượng ký tự nhất định từ phía bên trái (bắt đầu) của một chuỗi.

#### Logic nghiệp vụ chi tiết:

1.  **Kiểm tra Rỗng:** Kiểm tra nếu chuỗi đầu vào (`input`) là null hoặc rỗng. Nếu có, trả về chuỗi rỗng (`string.Empty`).
2.  **Kiểm tra Độ dài:** Kiểm tra nếu độ dài của chuỗi đầu vào nhỏ hơn hoặc bằng số ký tự cần lấy (`length`). Nếu đúng, trả về toàn bộ chuỗi đầu vào.
3.  **Trích xuất:** Nếu chuỗi đủ dài, sử dụng phương thức `input.Substring(0, length)` để trích xuất `length` ký tự từ vị trí bắt đầu (index 0).

### Right(string input, int length)

#### Chức năng tổng quát:

Hàm tiện ích dùng để trích xuất một số lượng ký tự nhất định từ phía bên phải (cuối) của một chuỗi.

#### Logic nghiệp vụ chi tiết:

1.  **Kiểm tra Rỗng:** Kiểm tra nếu chuỗi đầu vào (`input`) là null hoặc rỗng. Nếu có, trả về chuỗi rỗng (`string.Empty`).
2.  **Kiểm tra Độ dài:** Kiểm tra nếu độ dài của chuỗi đầu vào nhỏ hơn hoặc bằng số ký tự cần lấy (`length`). Nếu đúng, trả về toàn bộ chuỗi đầu vào.
3.  **Trích xuất:** Nếu chuỗi đủ dài, tính toán vị trí bắt đầu trích xuất bằng cách lấy tổng độ dài trừ đi số ký tự cần lấy (`input.Length - length`). Sau đó, sử dụng `input.Substring(start_index, length)` để trích xuất `length` ký tự từ cuối chuỗi.