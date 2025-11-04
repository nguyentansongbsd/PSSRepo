# Phân tích mã nguồn: Action_SignedContract.cs

Tệp mã nguồn `Action_SignedContract.cs` chứa một Plugin Dynamics 365/CRM được thiết kế để tự động hóa các quy trình nghiệp vụ khi một Hợp đồng (được đại diện bởi thực thể `Option Entry`) được ký kết. Plugin này thực thi logic phức tạp liên quan đến việc cập nhật trạng thái các thực thể liên quan (Hợp đồng, Đơn vị, Khách hàng) và tính toán lại phí quản lý (Management Fee) dựa trên các thông số của dự án và đơn vị.

## Tổng quan

Plugin `Action_SignedContract` là một plugin đồng bộ (synchronous) được triển khai trên nền tảng Microsoft Dynamics 365/CRM, thực thi giao diện `IPlugin`. Chức năng chính của nó là xử lý các hành động cần thiết sau khi một bản ghi Hợp đồng (Option Entry) được xác nhận là đã ký. Quá trình này bao gồm xác thực dữ liệu, cập nhật trạng thái của Hợp đồng và Đơn vị, tăng số lượng giao dịch của Khách hàng, và tính toán lại phí quản lý cho đợt thanh toán liên quan.

## Chi tiết các Hàm (Functions/Methods)

### Execute(IServiceProvider serviceProvider)

#### Chức năng tổng quát:
Hàm này là điểm vào chính của Plugin, chịu trách nhiệm thực thi toàn bộ logic nghiệp vụ khi sự kiện CRM được kích hoạt (thường là khi trạng thái của thực thể Option Entry thay đổi hoặc được cập nhật).

#### Logic nghiệp vụ chi tiết:

1.  **Khởi tạo và Thiết lập Môi trường:**
    *   Thiết lập các đối tượng dịch vụ tiêu chuẩn của CRM: `IPluginExecutionContext` (context), `IOrganizationServiceFactory` (factory), `IOrganizationService` (service), và `ITracingService` (traceService).
    *   Lấy tham chiếu đến thực thể mục tiêu (`Target`) từ `InputParameters` của context.
    *   Khởi tạo đối tượng `Common` (từ `BSDLibrary`) để sử dụng các hàm tiện ích.

2.  **Truy xuất và Kiểm tra Điều kiện Kích hoạt:**
    *   Truy xuất toàn bộ dữ liệu của thực thể `enOptionEntry` (Hợp đồng/Option Entry) dựa trên tham chiếu mục tiêu.
    *   Lấy giá trị `statuscode` hiện tại (`num1`).
    *   Gọi hàm `checkShortFallAmount` (từ lớp `OptionEntry` trong `BSDLibrary`) để kiểm tra xem có thiếu hụt số tiền thanh toán nào không (`flag`).
    *   Lấy ID của `bsd_paymentscheme` (Kế hoạch thanh toán) liên quan.

3.  **Thực thi Logic Chính (Điều kiện Ký Hợp đồng):**
    *   Logic chính chỉ được thực thi nếu `statuscode` là `100000001` (thường là trạng thái "Chờ ký" hoặc tương đương) **HOẶC** nếu kiểm tra thiếu hụt (`flag`) là `true`.

4.  **Xác thực Dữ liệu Bắt buộc:**
    *   Kiểm tra sự tồn tại của các trường quan trọng trên Hợp đồng: `customerid` (Người mua), `ordernumber` (Số Option), `bsd_project` (Dự án), và `bsd_contractprinteddate` (Ngày in hợp đồng).
    *   Nếu bất kỳ trường nào thiếu, Plugin sẽ ném ra `InvalidPluginExecutionException`.

5.  **Cập nhật Trạng thái Hợp đồng:**
    *   Lấy thời gian địa phương hiện tại (`dateTime1`).
    *   Cập nhật trạng thái (`statuscode`) của `enOptionEntry` lên `100000002` (thường là trạng thái "Đã ký").

6.  **Truy xuất Dữ liệu Liên quan để Tính toán Phí Quản lý:**
    *   Lấy số tháng đã trả phí quản lý (`num2`) từ trường `bsd_numberofmonthspaidmf` trên Hợp đồng (mặc định là 0 nếu không tồn tại).
    *   Truy xuất thực thể Đơn vị (`bsd_unitnumber`) để lấy `bsd_numberofmonthspaidmf` và `bsd_managementamountmonth`.
    *   Truy xuất thực thể Dự án (`bsd_project`) để lấy `bsd_managementamount`.

7.  **Tính toán Phí Quản lý Cơ bản:**
    *   Tính toán giá trị phí quản lý hàng tháng cơ bản (`num3`). Logic ưu tiên lấy giá trị từ trường `bsd_managementamountmonth` của Đơn vị. Nếu trường này không có, nó sẽ lấy giá trị từ trường `bsd_managementamount` của Dự án. Nếu cả hai đều không có, giá trị là 0.

8.  **Cập nhật Trạng thái Đơn vị và Khách hàng:**
    *   Truy xuất lại thực thể Đơn vị (`entity3`) để lấy các trường cần thiết (ví dụ: `bsd_netsaleablearea`).
    *   Lấy diện tích bán ròng (`num4`) từ Đơn vị.
    *   Truy xuất thực thể Khách hàng (`entity4`) để lấy `bsd_totaltransaction`.
    *   Tăng số lượng giao dịch của Khách hàng (`num5`) lên 1.
    *   **Cập nhật Đơn vị:** Cập nhật trạng thái (`statuscode`) của Đơn vị lên `100000002`, đặt `bsd_signedcontractdate` và sao chép số Option (`bsd_optionno`).
    *   **Cập nhật Khách hàng:** Cập nhật trường `bsd_totaltransaction` của Khách hàng.

9.  **Tính toán và Cập nhật Chi tiết Phí Quản lý (Management Fee Detail):**
    *   Gọi hàm `get_Inst_Fee` để truy xuất các bản ghi chi tiết kế hoạch thanh toán (`bsd_paymentschemedetail`) liên quan đến Hợp đồng này và có chứa phí quản lý.
    *   Nếu tìm thấy bản ghi chi tiết (chỉ xử lý bản ghi đầu tiên `instFee.Entities[0]`):
        *   Tính toán tổng phí quản lý mới (`num7`) theo công thức:
            $$num7 = \text{Phí Quản lý Cơ bản } (num3) \times \text{Số tháng đã trả MF } (num2) \times \text{Diện tích bán ròng } (num4)$$
        *   Cập nhật trường `bsd_managementamount` trên bản ghi chi tiết kế hoạch thanh toán bằng giá trị `num7` đã tính toán.

10. **Dọn dẹp Dữ liệu Chi tiết Kế hoạch Thanh toán:**
    *   Truy vấn tất cả các bản ghi `bsd_paymentschemedetail` liên quan đến Hợp đồng hiện tại.
    *   Lặp qua các bản ghi này. Nếu một bản ghi có `statuscode` là `100000001` (thường là trạng thái "Đã thanh toán" hoặc "Paid"), nó sẽ đặt trường `bsd_duedatewordtemplate` thành `null`. Mục đích là để xóa ngày đáo hạn trên mẫu Word cho các đợt đã thanh toán.

11. **Kết thúc:** Đặt tham số đầu ra `output` của Plugin là `"done"`.

### get_Inst_Fee(IOrganizationService crmservices, Guid oeID, Guid pmsID)

#### Chức năng tổng quát:
Hàm tiện ích này được sử dụng để truy vấn và lấy các bản ghi chi tiết kế hoạch thanh toán (`bsd_paymentschemedetail`) có liên quan đến một Hợp đồng cụ thể và có chứa một khoản phí quản lý (Management Fee) dương.

#### Logic nghiệp vụ chi tiết:

1.  **Xây dựng Truy vấn FetchXML:**
    *   Hàm tạo một chuỗi truy vấn FetchXML để tìm kiếm thực thể `bsd_paymentschemedetail`.
    *   Truy vấn yêu cầu lấy nhiều thuộc tính liên quan đến thanh toán và phí quản lý.

2.  **Thiết lập Điều kiện Lọc:**
    *   **Điều kiện 1:** Lọc theo `bsd_optionentry` (Hợp đồng) phải bằng `oeID` (ID của Option Entry được truyền vào).
    *   **Điều kiện 2:** Lọc theo `bsd_managementamount` phải lớn hơn 0 (`gt '0'`), đảm bảo chỉ lấy các đợt thanh toán có chứa phí quản lý.
    *   **Điều kiện 3:** Lọc theo `statecode` phải bằng 0 (`eq '0'`), đảm bảo chỉ lấy các bản ghi đang hoạt động.

3.  **Thực thi và Trả về Kết quả:**
    *   Thực thi truy vấn FetchXML thông qua `crmservices.RetrieveMultiple`.
    *   Trả về `EntityCollection` chứa các bản ghi chi tiết kế hoạch thanh toán thỏa mãn điều kiện.