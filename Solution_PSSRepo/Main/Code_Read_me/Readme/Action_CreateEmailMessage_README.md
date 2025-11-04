# Phân tích mã nguồn: Action_CreateEmailMessage.cs

## Tổng quan

Tệp mã nguồn `Action_CreateEmailMessage.cs` là một Plugin (Custom Action) được phát triển cho nền tảng Microsoft Dynamics 365/Power Platform (sử dụng Microsoft.Xrm.Sdk). Plugin này có nhiệm vụ tự động hóa quá trình tạo một thực thể Email Message (`email`) trong hệ thống.

Chức năng chính của Plugin là lấy dữ liệu từ một thực thể gốc (thường là `bsd_payment` hoặc `bsd_customernotices`), truy xuất các thông tin liên quan (Khách hàng, Dự án, Đơn vị), ánh xạ dữ liệu này vào một template email đã định sẵn, thiết lập người gửi/người nhận (To, From, CC, BCC), và tạo bản ghi email hoàn chỉnh trong Dynamics 365. Nó cũng tích hợp logic để quản lý việc gửi email hàng loạt thông qua thực thể `bsd_bulksendmailmanager`.

## Chi tiết các Hàm (Functions/Methods)

### Execute(IServiceProvider serviceProvider)

#### Chức năng tổng quát
Đây là điểm vào chính của Plugin, chịu trách nhiệm khởi tạo các dịch vụ, xử lý logic nghiệp vụ cốt lõi, truy xuất dữ liệu, tạo thực thể email, và cập nhật trạng thái của thực thể gốc.

#### Logic nghiệp vụ chi tiết
1.  **Khởi tạo Dịch vụ:** Lấy các đối tượng cần thiết từ `serviceProvider`, bao gồm `IPluginExecutionContext` (context), `IOrganizationServiceFactory`, `IOrganizationService` (service), và `ITracingService` (tracingService) để ghi nhật ký.
2.  **Lấy Tham số Đầu vào:** Truy xuất các tham số được truyền từ Custom Action, bao gồm `entityIdBulkSendMail`, `entityName`, `entityMainId`, `base64FileAttach`, và `userAction`.
3.  **Truy xuất Thực thể Chính:** Lấy thực thể chính (`entityMain`) dựa trên `entityName` và `entityMainId`.
4.  **Truy xuất Thực thể Liên quan:** Lấy thông tin Dự án (`enProject`) và Người dùng hành động (`enUserAction`).
5.  **Gọi Hàm Trợ giúp:** Gọi `GetEntityCustomer()`, `GetEntityProject()`, và `GetEntityunit()` để điền dữ liệu vào các biến toàn cục `enCus`, `enProject`, và `enUnit`.
6.  **Tạo Thực thể Email:** Khởi tạo một thực thể `email` mới (`emailMessage`).
    *   Thiết lập nội dung email bằng cách gọi `GetEmailTemplate()`.
    *   Thiết lập các trường tùy chỉnh như `bsd_entityname`, `bsd_entityid`, `bsd_emailcreator`.
    *   Thiết lập trường `regardingobjectid` (liên quan đến thực thể chính).
    *   Thiết lập trạng thái mặc định: `statecode = 0` (Open), `statuscode = 1` (Draft).
7.  **Cập nhật Thực thể Gốc:**
    *   Truy xuất lại thực thể gốc (`enUpdate`).
    *   Sử dụng `switch` dựa trên `entityName`:
        *   Nếu là `bsd_customernotices`: Cập nhật người tạo email, ngày tạo, cờ `bsd_iscreateemail = true`, và `bsd_emailstatus = 1`.
        *   Nếu là `bsd_payment`: Chỉ cập nhật `bsd_emailstatus = 1`.
    *   Thực hiện `service.Update(enUpdate)`.
8.  **Quản lý Bulk Send Mail Manager:**
    *   Kiểm tra nếu `entityIdBulkSendMail` rỗng (lần gửi đầu tiên).
    *   Nếu rỗng, tạo một thực thể `bsd_bulksendmailmanager` mới.
    *   Thiết lập `bsd_project`, `bsd_types` (100000000 cho Payment, 100000001 cho Notices), tên động, và `ownerid`.
    *   Tạo bản ghi và lưu ID mới vào `context.OutputParameters["idBulkSendMail"]`.
    *   Gán ID này vào trường `bsd_bulksendmailmanager` của email.
9.  **Thiết lập Tiêu đề và Người nhận:**
    *   Thiết lập tiêu đề bằng cách gọi `GetSubject()`.
    *   Thiết lập người gửi bằng `MapFromMail()`.
    *   Thiết lập người nhận chính bằng `MapToMail()`.
    *   Thiết lập CC/BCC bằng `MapCC_BCC()`.
10. **Tạo Email:** Thực hiện `service.Create(emailMessage)` để lưu email vào hệ thống và lấy `emailId`.
11. **Tạo Tên File Đính kèm:** Gọi `GenFileNameAttach()` để tạo tên file (mặc dù logic tạo `activitymimeattachment` đã bị comment out).
12. **Trả về Kết quả:** Gán `emailId` và `filename` vào `context.OutputParameters`.
13. **Xử lý Lỗi:** Bắt và ném lại ngoại lệ `InvalidPluginExecutionException` nếu có lỗi xảy ra.

### GetEmailTemplate()

#### Chức năng tổng quát
Hàm này truy vấn template email phù hợp từ hệ thống dựa trên loại thực thể chính và trả về nội dung template đã được ánh xạ tham số.

#### Logic nghiệp vụ chi tiết
1.  **Xác định Tiêu đề Template:** Dựa trên `entityMain.LogicalName`:
    *   Nếu là `bsd_payment`, `query_title` là "Comfirm Payment".
    *   Nếu là loại khác, `query_title` là "Payment Notice".
2.  **Truy vấn Template:** Tạo `QueryExpression` để tìm kiếm thực thể `template` (Template Email) có trường `title` khớp với `query_title`.
3.  **Xử lý Kết quả:**
    *   Nếu tìm thấy template (`rs.Entities.Count > 0`), lưu thực thể template vào `enEmailTemplate`.
    *   Lấy nội dung HTML an toàn (`safehtml`) của template.
    *   Gọi `MapParamMailTemplate()` để thay thế các placeholder trong nội dung.
    *   Trả về nội dung đã được ánh xạ.
4.  Nếu không tìm thấy, trả về chuỗi rỗng.

### MapParamMailTemplate(string mailTemplate)

#### Chức năng tổng quát
Hàm này thực hiện việc thay thế các biến placeholder (ví dụ: `{fullname}`) trong nội dung template email bằng dữ liệu thực tế từ các thực thể liên quan.

#### Logic nghiệp vụ chi tiết
1.  Sử dụng `switch` dựa trên `entityMain.LogicalName` để xác định logic ánh xạ:
2.  **Trường hợp `bsd_payment`:**
    *   Thay thế `{fullname}` bằng kết quả từ `GetFullNameCustomer()`.
    *   Thay thế `{sign_mail}` bằng kết quả từ `GetSignMail()`.
3.  **Trường hợp Mặc định (Notices):**
    *   Truy xuất thông tin Nhà đầu tư (`bsd_investorname`) từ thực thể Dự án (`enProject`).
    *   Thực hiện thay thế hàng loạt các placeholder: `{fullname}`, `{sign_mail}`, `{bsd_customerservice}`, `{bsd_Acountant}`, `{bsd_extfin}` (lấy từ `enProject`), và `bsd_investorname`.
4.  Trả về chuỗi template đã được thay thế.

### GetEntityCustomer()

#### Chức năng tổng quát
Truy xuất thực thể Khách hàng (Customer) liên quan đến thực thể chính (`entityMain`).

#### Logic nghiệp vụ chi tiết
1.  Sử dụng `switch` dựa trên `entityMain.LogicalName`.
2.  **Trường hợp `bsd_payment`:** Lấy tham chiếu thực thể từ trường `bsd_purchaser`.
3.  **Trường hợp Mặc định:** Lấy tham chiếu thực thể từ trường `bsd_customer`.
4.  Thực hiện `service.Retrieve()` để lấy toàn bộ dữ liệu của thực thể Khách hàng và trả về.

### GetEntityunit()

#### Chức năng tổng quát
Truy xuất thực thể Đơn vị (Unit) liên quan đến thực thể chính (`entityMain`).

#### Logic nghiệp vụ chi tiết
1.  Sử dụng `switch` dựa trên `entityMain.LogicalName`.
2.  Trong cả hai trường hợp (`bsd_payment` và mặc định), lấy tham chiếu thực thể từ trường `bsd_units`.
3.  Thực hiện `service.Retrieve()` để lấy toàn bộ dữ liệu của thực thể Đơn vị và trả về.

### GetEntityProject()

#### Chức năng tổng quát
Truy xuất thực thể Dự án (Project) liên quan đến thực thể chính (`entityMain`).

#### Logic nghiệp vụ chi tiết
1.  Sử dụng `switch` dựa trên `entityMain.LogicalName`.
2.  Trong cả hai trường hợp (`bsd_payment` và mặc định), lấy tham chiếu thực thể từ trường `bsd_project`.
3.  Thực hiện `service.Retrieve()` để lấy toàn bộ dữ liệu của thực thể Dự án và trả về.

### GetSubject()

#### Chức năng tổng quát
Tạo tiêu đề email động bằng cách thay thế các placeholder trong tiêu đề template bằng tên Dự án và tên Đơn vị.

#### Logic nghiệp vụ chi tiết
1.  Lấy tiêu đề template gốc từ `enEmailTemplate["subjectsafehtml"]`.
2.  Sử dụng `switch` dựa trên `entityMain.LogicalName`:
    *   **Trường hợp `bsd_payment`:** Thay thế `{project}` bằng tên dự án và `{unitname}` bằng tên đơn vị.
    *   **Trường hợp Mặc định:** Thay thế `{project}` bằng tên dự án, nhưng tên dự án được bao quanh bởi dấu ngoặc vuông `[]`, và `{unitname}` bằng tên đơn vị.
3.  Trả về chuỗi tiêu đề đã hoàn chỉnh.

### MapCC_BCC(Entity entity)

#### Chức năng tổng quát
Truy vấn danh sách người nhận CC và BCC được cấu hình cho Dự án và gán chúng vào thực thể email.

#### Logic nghiệp vụ chi tiết
1.  Tạo `QueryExpression` để truy vấn thực thể `bsd_listmailcc` (danh sách cấu hình CC/BCC) dựa trên ID của Dự án (`enProject.Id`).
2.  Khởi tạo hai danh sách: `lstBcc` và `lstCc`.
3.  Lặp qua các kết quả truy vấn:
    *   Truy xuất thông tin người dùng hệ thống (`systemuser`) từ tham chiếu `bsd_usersystem`.
    *   Tạo một thực thể `activityparty` mới cho người nhận.
    *   Kiểm tra giá trị OptionSet `bsd_type`:
        *   Nếu `bsd_type` là 100000001 (BCC), thêm vào `lstBcc`.
        *   Nếu loại khác (CC), thêm vào `lstCc`.
    *   Thiết lập `addressused` bằng `internalemailaddress` và `partyid` bằng tham chiếu người dùng.
4.  Nếu `lstBcc` hoặc `lstCc` có phần tử, gán mảng `activityparty` tương ứng vào các trường `bcc` và `cc` của thực thể email.

### MapFromMail(Entity entity)

#### Chức năng tổng quát
Thiết lập người gửi (From) cho email, lấy thông tin từ cấu hình người gửi của Dự án.

#### Logic nghiệp vụ chi tiết
1.  Truy xuất thông tin người dùng hệ thống được cấu hình làm người gửi (`bsd_senderconfigsystem`) từ thực thể Dự án (`enProject`).
2.  Tạo một thực thể `activityparty` mới.
3.  Thiết lập `addressused` bằng `internalemailaddress` của người dùng hệ thống.
4.  Thiết lập `partyid` bằng tham chiếu người dùng.
5.  Gán mảng `activityparty` này vào trường `from` của thực thể email.

### MapToMail(Entity entity)

#### Chức năng tổng quát
Thiết lập người nhận chính (To) cho email, lấy địa chỉ email ưu tiên từ thực thể Khách hàng.

#### Logic nghiệp vụ chi tiết
1.  Tạo một thực thể `activityparty` mới.
2.  Xác định địa chỉ email ưu tiên từ thực thể Khách hàng (`enCus`):
    *   Kiểm tra lần lượt các trường `bsd_email2`, `emailaddress11`, và cuối cùng là `emailaddress1`.
3.  Gán địa chỉ email tìm được vào `addressused`.
4.  Gán `partyid` là tham chiếu đến thực thể Khách hàng.
5.  Gán mảng `activityparty` này vào trường `to` của thực thể email.

### GenFileNameAttach()

#### Chức năng tổng quát
Tạo tên file đính kèm động dựa trên thông tin Đơn vị, Khách hàng và loại giao dịch.

#### Logic nghiệp vụ chi tiết
1.  Kiểm tra `entityMain.LogicalName`.
2.  **Trường hợp `bsd_payment`:** Tên file được tạo theo định dạng: `{Tên Đơn vị}_{Tên Khách hàng (không dấu)}_{Tên Loại Thanh toán}`. (Sử dụng `GetFullNameCustomer()` và `GetPaymentName()`).
3.  **Trường hợp Khác (Notices):**
    *   Truy xuất thông tin chi tiết đợt thanh toán (`bsd_paymentschemedetail`).
    *   Tên file được tạo theo định dạng: `{Tên Đơn vị}_{Tên Khách hàng (không dấu)}_Installment{Số thứ tự đợt}_PN`.
4.  Trả về chuỗi tên file.

### GetPaymentName()

#### Chức năng tổng quát
Chuyển đổi giá trị OptionSet của loại thanh toán thành chuỗi mô tả.

#### Logic nghiệp vụ chi tiết
1.  Lấy giá trị OptionSet từ trường `bsd_paymenttype` của thực thể chính.
2.  Sử dụng `switch` để ánh xạ giá trị số sang tên:
    *   100000000: "Queuing fee"
    *   100000001: "Deposit fee"
    *   100000002: "Installment" (Nếu là Installment, truy xuất thêm số thứ tự đợt từ `bsd_paymentschemedetail`).
    *   100000003: "Interest charge"
    *   100000004: "Fees"
    *   Mặc định: "Other"
3.  Trả về tên thanh toán.

### GetFullNameCustomer()

#### Chức năng tổng quát
Lấy tên đầy đủ của Khách hàng dựa trên loại thực thể (Contact hoặc Account) và loại bỏ dấu tiếng Việt.

#### Logic nghiệp vụ chi tiết
1.  Sử dụng `switch` dựa trên `enCus.LogicalName`:
    *   Nếu là `contact`, lấy tên từ trường `bsd_fullname`.
    *   Nếu là `account`, lấy tên từ trường `bsd_name`.
2.  Gọi hàm `RemoveVietnameseDiacritics()` để chuẩn hóa tên.
3.  Trả về tên đã chuẩn hóa.

### GetSignMail()

#### Chức năng tổng quát
Truy vấn và trả về chữ ký email (email signature) của người dùng đang thực hiện hành động.

#### Logic nghiệp vụ chi tiết
1.  Tạo `QueryExpression` để tìm kiếm thực thể `emailsignature`.
2.  Điều kiện truy vấn là `ownerid` phải bằng ID của người dùng hành động (`enUserAction.Id`).
3.  Nếu tìm thấy chữ ký, trả về nội dung HTML an toàn (`safehtml`). Nếu không, trả về chuỗi rỗng.

### RemoveVietnameseDiacritics(string input)

#### Chức năng tổng quát
Hàm tiện ích tĩnh này loại bỏ các dấu thanh (diacritics) khỏi một chuỗi tiếng Việt.

#### Logic nghiệp vụ chi tiết
1.  Định nghĩa mảng `vietnameseChars` chứa các nhóm ký tự có dấu và ký tự không dấu tương ứng.
2.  Lặp qua mảng các ký tự có dấu.
3.  Sử dụng phương thức `Replace` để thay thế tất cả các ký tự có dấu (ví dụ: 'á', 'à', 'ạ') bằng ký tự không dấu tương ứng (ví dụ: 'a').
4.  Trả về chuỗi đã được chuẩn hóa (không dấu).