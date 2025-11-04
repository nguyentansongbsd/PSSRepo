# Phân tích mã nguồn: Plugin_PaymentNotices_CreateQRCode.cs

## Tổng quan

Tệp mã nguồn `Plugin_PaymentNotices_CreateQRCode.cs` định nghĩa một Plugin Dynamics 365 (C#) được thiết kế để tự động tạo mã QR thanh toán theo tiêu chuẩn VietQR cho các bản ghi Thông báo Thanh toán (Payment Notices).

Plugin này hoạt động bằng cách:
1.  Truy xuất thông tin chi tiết của Dự án, Khách hàng và Sản phẩm liên quan.
2.  Xác định tài khoản ngân hàng mặc định của Dự án.
3.  Xây dựng chuỗi nội dung thanh toán (mục đích chuyển khoản) đã được chuẩn hóa.
4.  Sử dụng thư viện `QRCoder` và `VietQRHelper` để sinh ra mã QR dưới dạng ảnh (byte array).
5.  Cập nhật ảnh mã QR này trở lại trường tương ứng trên bản ghi Thông báo Thanh toán.

Plugin này thường được kích hoạt trong giai đoạn `Create` hoặc `Update` của thực thể Thông báo Thanh toán.

## Chi tiết các Hàm (Functions/Methods)

### IPlugin.Execute(IServiceProvider serviceProvider)

#### Chức năng tổng quát:
Đây là điểm vào chính của plugin. Hàm này chịu trách nhiệm thiết lập môi trường Dynamics 365, thu thập dữ liệu cần thiết, kiểm tra điều kiện tạo QR, tạo payload VietQR, sinh ảnh mã QR, và cập nhật bản ghi mục tiêu.

#### Logic nghiệp vụ chi tiết:
1.  **Khởi tạo Dịch vụ:** Lấy các đối tượng dịch vụ tiêu chuẩn của Dynamics 365: `IPluginExecutionContext` (context), `IOrganizationServiceFactory` (factory), `IOrganizationService` (service), và `ITracingService` (tracingService).
2.  **Lấy Thực thể Mục tiêu:** Lấy thực thể mục tiêu (`Target`) từ `InputParameters` và truy xuất bản ghi đầy đủ (`enTarget`) để đảm bảo có tất cả các trường.
3.  **Truy xuất Dự án:** Lấy tham chiếu đến Dự án (`bsd_project`) từ `enTarget` và truy xuất thực thể Dự án (`enProject`).
4.  **Lấy Nội dung Thanh toán:** Gọi hàm `GetContent(service, enTarget)` để tạo chuỗi mục đích thanh toán (`purpose`).
5.  **Kiểm tra Điều kiện Tạo QR:** Sau khi gọi `GetContent`, kiểm tra biến cờ `isGenQR`. Nếu `isGenQR` là `false` (do cấu hình Dự án không cho phép tạo QR), hàm sẽ dừng lại (`return`).
6.  **Lấy Thông tin Ngân hàng:** Gọi hàm `GetBankInfor(enProject, ref banknumber, ref bankbin)` để tìm số tài khoản và mã BIN ngân hàng mặc định của Dự án.
7.  **Xác thực Ngân hàng:** Kiểm tra biến cờ `isExistBankInfor`. Nếu `false`, ném ra một `Exception` yêu cầu người dùng kiểm tra lại Tài khoản Ngân hàng Mặc định của Dự án.
8.  **Tạo Payload VietQR:** Gọi hàm tĩnh `GenerateVietQRpayload(bankbin, banknumber, purpose)` để tạo chuỗi dữ liệu theo định dạng VietQR.
9.  **Sinh Mã QR:**
    *   Sử dụng `QRCodeGenerator` để tạo dữ liệu mã QR (`QRCodeData`) từ chuỗi payload, với mức độ sửa lỗi `ECCLevel.Q`.
    *   Sử dụng `PngByteQRCode` để chuyển đổi dữ liệu thành ảnh byte array (`qrCodeImageBytes`) với kích thước mô-đun là 20 pixel.
10. **Chuẩn hóa Ảnh:** Chuyển đổi byte array thành chuỗi Base64, sau đó chuyển ngược lại thành byte array (`imageBytes`) để đảm bảo định dạng lưu trữ phù hợp với trường ảnh trong Dynamics 365.
11. **Cập nhật Bản ghi:**
    *   Tạo một thực thể mới (`entityToUpdate`) chỉ chứa ID và tên logic của bản ghi mục tiêu.
    *   Gán byte array của ảnh QR vào trường `bsd_qrcode`.
    *   Gán tham chiếu đến Tài khoản Ngân hàng Dự án mặc định (`projectBankAccount`) vào trường `bsd_projectbankaccount`.
    *   Thực hiện `service.Update(entityToUpdate)`.

### GetBankInfor(Entity enproject, ref string banknumber, ref string bankbin)

#### Chức năng tổng quát:
Hàm này truy vấn Dynamics 365 để tìm kiếm và trích xuất số tài khoản ngân hàng và mã BIN của ngân hàng mặc định được liên kết với Dự án.

#### Logic nghiệp vụ chi tiết:
1.  **Thiết lập Truy vấn:** Định nghĩa các tên thực thể và trường cần thiết (ví dụ: `bsd_projectbankaccount`, `bsd_default`, `bsd_bank`).
2.  **Tạo QueryExpression:** Tạo một truy vấn nhắm vào thực thể Tài khoản Ngân hàng Dự án (`bsd_projectbankaccount`).
3.  **Thêm Điều kiện Lọc:**
    *   Lọc theo ID Dự án (`projectLookupFieldOnBankAccount` bằng `enproject.Id`).
    *   Lọc theo cờ mặc định (`isDefaultField` bằng `true`).
4.  **Liên kết Thực thể (LinkEntity):** Tạo `LinkEntity` để liên kết Tài khoản Ngân hàng Dự án với thực thể Ngân hàng (`bsd_bank`) để lấy mã BIN (`new_bankcode`).
5.  **Thực hiện Truy vấn:** Gọi `service.RetrieveMultiple(query)`.
6.  **Xử lý Kết quả:**
    *   Nếu tìm thấy kết quả (`results.Entities.Count > 0`):
        *   Lấy bản ghi đầu tiên (`defaultBankAccount`) và lưu vào biến toàn cục `projectBankAccount`.
        *   Lấy Số tài khoản (`banknumber`) từ trường `bsd_name`.
        *   Lấy Mã BIN Ngân hàng (`bankbin`) từ giá trị AliasedValue (do sử dụng LinkEntity).
        *   Đặt cờ `isExistBankInfor` thành `true`.

### GetContent(IOrganizationService service, Entity target)

#### Chức năng tổng quát:
Hàm này thu thập thông tin từ các thực thể liên quan (Dự án, Khách hàng, Sản phẩm, Đợt thanh toán) để xây dựng chuỗi nội dung thanh toán chuẩn hóa.

#### Logic nghiệp vụ chi tiết:
1.  **Khởi tạo Giá trị Mặc định:** Thiết lập các chuỗi mặc định cho tên khách hàng, sản phẩm, dự án và số thứ tự.
2.  **Xử lý Dự án:**
    *   Truy xuất thực thể Dự án.
    *   Kiểm tra trường `bsd_print_qr_bank`. Nếu trường này tồn tại và có giá trị `false`, đặt biến toàn cục `isGenQR = false` và trả về chuỗi rỗng. Điều này ngăn chặn việc tạo QR nếu dự án không cho phép.
3.  **Xử lý Khách hàng:**
    *   Truy xuất thực thể Khách hàng (`bsd_customer`).
    *   Xác định tên khách hàng: Nếu là thực thể `contact`, lấy `fullname`; nếu là `account`, lấy `name`.
4.  **Xử lý Sản phẩm (Unit):** Truy xuất thực thể Sản phẩm (`bsd_units`) và lấy tên sản phẩm (`name`).
5.  **Xử lý Đợt thanh toán:** Truy xuất chi tiết đợt thanh toán (`bsd_paymentschemedetail`) để lấy số thứ tự đợt (`bsd_ordernumber`).
6.  **Chuẩn hóa Tên:** Gọi hàm tĩnh `RemoveDiacritics()` để chuyển tên khách hàng thành dạng không dấu và viết hoa. Tên sản phẩm cũng được viết hoa.
7.  **Kết hợp Chuỗi:** Trả về chuỗi nội dung thanh toán theo định dạng cố định:
    `[Tên Sản phẩm (Viết hoa)]_[Tên Dự án]_[Tên Khách hàng không dấu]_Thanh toan dot [Số thứ tự đợt]`

### RemoveDiacritics(string text)

#### Chức năng tổng quát:
Là một hàm tiện ích tĩnh, chịu trách nhiệm chuyển đổi chuỗi tiếng Việt có dấu thành chuỗi không dấu, đồng thời xử lý các ký tự đặc biệt như 'Đ' và 'đ'.

#### Logic nghiệp vụ chi tiết:
1.  **Chuẩn hóa FormD:** Chuỗi đầu vào được chuẩn hóa về `NormalizationForm.FormD` để tách các ký tự cơ bản khỏi các dấu phụ (diacritics).
2.  **Lọc Dấu Phụ:** Lặp qua chuỗi đã chuẩn hóa. Sử dụng `CharUnicodeInfo.GetUnicodeCategory` để kiểm tra. Chỉ các ký tự không phải là dấu phụ (`UnicodeCategory.NonSpacingMark`) mới được thêm vào `StringBuilder`.
3.  **Chuẩn hóa FormC:** Chuỗi kết quả được chuẩn hóa lại về `NormalizationForm.FormC`.
4.  **Xử lý Ký tự Đặc biệt:** Thực hiện thay thế thủ công: thay thế 'Đ' bằng 'D' và 'đ' bằng 'd'.
5.  **Trả về:** Trả về chuỗi không dấu đã được làm sạch.

### GenerateVietQRpayload(string bankBin, string bankNumber, string purpose)

#### Chức năng tổng quát:
Là một hàm tiện ích tĩnh, chịu trách nhiệm xây dựng chuỗi dữ liệu (payload) theo tiêu chuẩn VietQR, sử dụng thư viện hỗ trợ bên ngoài (`VietQRHelper`).

#### Logic nghiệp vụ chi tiết:
1.  **Khởi tạo QRPay:** Sử dụng phương thức tĩnh `QRPay.InitVietQR` (từ thư viện `VietQRHelper`) để khởi tạo đối tượng thanh toán, truyền vào mã ngân hàng (`bankBin`), số tài khoản (`bankNumber`), và mục đích thanh toán (`purpose`).
2.  **Xây dựng Payload:** Gọi phương thức `Build()` trên đối tượng `qrPay` để tạo ra chuỗi dữ liệu VietQR hoàn chỉnh, tuân thủ cấu trúc EMV Co. QR Code.
3.  **Trả về:** Trả về chuỗi payload VietQR.