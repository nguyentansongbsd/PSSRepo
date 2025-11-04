# Phân tích mã nguồn: Action_TerminateLetter_GenerateTerminateLetter_Detail.cs

Tệp mã nguồn `Action_TerminateLetter_GenerateTerminateLetter_Detail.cs` là một Plugin (IPlugin) được viết bằng C# cho nền tảng Microsoft Dynamics 365/XRM. Plugin này được thiết kế để thực thi một hành động nghiệp vụ (Action) nhằm tạo ra một bản ghi Thư Chấm dứt Hợp đồng (`bsd_terminateletter`) chi tiết, dựa trên một bản ghi Sales Order (thường được gọi là Option Entry trong ngữ cảnh này) khi các điều kiện quá hạn thanh toán được đáp ứng.

## Tổng quan

Plugin này chịu trách nhiệm kiểm tra tình trạng thanh toán của một Option Entry (Sales Order). Nếu phát hiện có đợt thanh toán quá hạn nghiêm trọng (thường là hơn 60 ngày), nó sẽ tiến hành tính toán các khoản phạt (Penalty) và lãi quá hạn (Overdue Interest) theo các quy tắc nghiệp vụ phức tạp liên quan đến ngày ký hợp đồng/DA, sau đó tạo ra bản ghi Thư Chấm dứt Hợp đồng mới.

## Chi tiết các Hàm (Functions/Methods)

### IPlugin.Execute(IServiceProvider serviceProvider)

**Chức năng tổng quát:**
Đây là điểm vào chính của plugin. Hàm này khởi tạo môi trường CRM, truy xuất dữ liệu Option Entry, kiểm tra các điều kiện quá hạn, và thực hiện toàn bộ quy trình tính toán và tạo bản ghi Thư Chấm dứt Hợp đồng.

**Logic nghiệp vụ chi tiết:**

1.  **Khởi tạo Dịch vụ:** Lấy các dịch vụ cần thiết: `IPluginExecutionContext`, `IOrganizationServiceFactory`, `IOrganizationService`, và `ITracingService`.
2.  **Impersonation (Ủy quyền):** Kiểm tra nếu tham số đầu vào chứa `userid`. Nếu có, nó tạo một `IOrganizationService` mới để thực thi logic dưới quyền của người dùng được chỉ định.
3.  **Truy xuất Option Entry:** Lấy bản ghi `salesorder` (`entity1`) dựa trên `id` được truyền qua `InputParameters`.
4.  **Xác định Ngày tính toán:** Lấy ngày tính toán (`_date`) từ `InputParameters`. Nếu không có, sử dụng ngày hiện tại.
5.  **Kiểm tra Tồn tại Thư Chấm dứt:** Gọi hàm `CheckExist_optionEntry_on_Terminateletter` để xác định xem Thư Chấm dứt đã được tạo cho Option Entry này chưa. Nếu đã có, plugin kết thúc.
6.  **Tính Tổng Ngày Quá Hạn:**
    *   Gọi `get_pmSchDtl_fromOpentryID` để lấy tất cả các chi tiết lịch thanh toán (`bsd_paymentschemedetail`) chưa thanh toán (statuscode = 100000000).
    *   Lặp qua các chi tiết này để tính tổng số ngày quá hạn (`num2`). Đối với mỗi đợt chưa thanh toán, nó tính số ngày từ ngày đến hạn (`bsd_duedate`) đến ngày tính toán (`_date`).
7.  **Kiểm tra Điều kiện Kích hoạt:** Kiểm tra nếu có đợt thanh toán nào quá hạn hơn 60 ngày (sử dụng `check_overDuedate_pmShcDtl`) HOẶC nếu tổng số ngày quá hạn tích lũy (`num2`) lớn hơn 60. Nếu điều kiện này đúng, tiến hành tạo Thư Chấm dứt.
8.  **Khởi tạo Thư Chấm dứt (`bsd_terminateletter`):**
    *   Tạo entity `bsd_genterminationletterdetail` (`enGenDetail`) để ghi lại quá trình tạo.
    *   Tạo entity `bsd_terminateletter` (`entity3`).
    *   Ánh xạ các trường cơ bản: đợt thanh toán quá hạn (`bsd_insallment`), khách hàng (`bsd_customer`), tên, ngày, Option Entry, và dự án.
9.  **Ánh xạ Cảnh báo (Warning Notices):** Truy vấn 2 bản ghi `bsd_warningnotices` gần nhất liên quan đến Option Entry và đợt thanh toán đang xét. Ánh xạ chúng vào các trường `bsd_warning_notices_1` và `bsd_warning_notices_2` (đảm bảo thứ tự cảnh báo đúng).
10. **Tính Phạt (Penalty):**
    *   Truy xuất Payment Scheme liên quan.
    *   Logic hiện tại tính toán khoản phạt (`bsd_penaty`) bằng 20% của tổng giá trị hợp đồng chưa bao gồm phí vận chuyển (`bsd_totalamountlessfreight`).
11. **Tính Lãi Quá Hạn (Overdue Interest):**
    *   Truy xuất `bsd_interestratemaster` để lấy lãi suất và số ngày ân hạn (`bsd_gracedays`).
    *   **Xác định Kịch bản Tính Lãi (`caseSign`):** Dựa trên sự tồn tại của `bsd_signedcontractdate` và `bsd_signeddadate` trên Option Entry, biến `caseSign` được gán giá trị (1, 2, 3, hoặc 4) để xác định công thức tính ngày trễ hạn.
    *   **Tính Ngày Trễ Hạn:** Gọi `checkCaseSignAndCalLateDays` để tính số ngày trễ hạn (`lateDays`) dựa trên kịch bản `caseSign` và ngày ân hạn.
    *   **Tính Lãi Cơ bản:** Nếu hàm kiểm tra kịch bản trả về `true`, tính lãi cơ bản: `Lãi = (Lãi suất / 100) * lateDays * bsd_balance`.
    *   **Cộng dồn Lãi chưa thanh toán:** Truy vấn tất cả các đợt thanh toán trước đợt đang xét (`bsd_ordernumber` nhỏ hơn) và cộng thêm số tiền lãi còn lại chưa thanh toán (`bsd_interestchargeremaining`) vào tổng lãi quá hạn. Sau đó, cộng thêm lãi còn lại của chính đợt đang xét.
12. **Tạo Bản ghi:** Thực hiện lệnh `this.service.Create(entity3)` để tạo Thư Chấm dứt Hợp đồng.
13. **Xử lý Ngoại lệ:** Nếu có lỗi xảy ra trong quá trình tạo hoặc tính toán, nó sẽ bắt lỗi, ghi chi tiết lỗi vào trường `bsd_errordetail` và tạo bản ghi `bsd_genterminationletterdetail` với trạng thái Lỗi (100000001).

### checkCaseSignAndCalLateDays(Entity enInstallment, Entity enOptionEntry, DateTime bsd_signedcontractdate, DateTime bsd_signeddadate, DateTime receiptdate, ref int lateDays)

**Chức năng tổng quát:**
Hàm này xác định số ngày trễ hạn thực tế (`lateDays`) dựa trên kịch bản tính lãi (`caseSign`) đã được xác định trước đó, có tính đến ngày ký hợp đồng/DA và ngày đến hạn của đợt tích "Sign Contract Installment".

**Logic nghiệp vụ chi tiết:**

1.  **Tìm Đợt Ký Hợp đồng:** Truy vấn `bsd_paymentschemedetail` có trường `bsd_signcontractinstallment` bằng `true` để tìm ngày đến hạn mốc (`bsd_duedateFlag`).
2.  **Phân tích Kịch bản (`caseSign`):**
    *   **Case 0:** Trả về `false` (Không tính lãi).
    *   **Case 1 (Chỉ tính lãi cho các đợt trước đợt tích Sign Contract Installment):**
        *   Nếu đợt hiện tại là đợt ký hợp đồng, trả về `false`.
        *   Nếu ngày đến hạn của đợt hiện tại (`bsd_duedate`) nhỏ hơn `bsd_duedateFlag`, tính `lateDays` từ `receiptdate` trừ đi `bsd_signeddadate`.
        *   Nếu `bsd_duedate` lớn hơn hoặc bằng `bsd_duedateFlag`, tính `lateDays` từ `receiptdate` trừ đi `bsd_duedate`.
    *   **Case 2 (Chỉ tính lãi cho các đợt sau đợt tích Sign Contract Installment):**
        *   Nếu `bsd_duedate` lớn hơn hoặc bằng `bsd_duedateFlag`, tính `lateDays` từ `receiptdate` trừ đi `bsd_signedcontractdate`.
    *   **Case 3 (Tính lãi cho các đợt theo mức lãi suất bình thường):**
        *   Nếu `bsd_duedate` nhỏ hơn `bsd_duedateFlag`, tính `lateDays` từ `receiptdate` trừ đi `bsd_signeddadate`.
        *   Nếu không, tính `lateDays` từ `receiptdate` trừ đi `bsd_duedate`.
    *   **Case 4 (Không có ngày ký HĐ/DA):** Nếu `bsd_duedate` lớn hơn hoặc bằng `bsd_duedateFlag`, tính `lateDays` từ `receiptdate` trừ đi `bsd_duedate`.
3.  Hàm trả về `true` nếu việc tính lãi được áp dụng theo kịch bản, và cập nhật số ngày trễ hạn qua tham số `lateDays` (ref).

### check_overDuedate_pmShcDtl(EntityCollection entCll, ref Entity installment)

**Chức năng tổng quát:**
Kiểm tra xem trong danh sách các chi tiết thanh toán được cung cấp, có đợt nào đã quá hạn hơn 60 ngày so với ngày tính toán (`_date`) hay không.

**Logic nghiệp vụ chi tiết:**

1.  Lặp qua tất cả các entity trong `entCll` (danh sách chi tiết thanh toán).
2.  Đối với mỗi entity, kiểm tra xem nó có trường `bsd_duedate` không.
3.  Tính toán số ngày chênh lệch giữa ngày tính toán hiện tại (`_date`) và ngày đến hạn (`bsd_duedate`).
4.  Nếu số ngày chênh lệch lớn hơn 60, hàm sẽ gán entity chi tiết thanh toán đó vào tham số `installment` (ref) và trả về `true`.
5.  Nếu không tìm thấy đợt nào quá hạn hơn 60 ngày, hàm trả về `false`.

### RetrieveMultiRecord(IOrganizationService crmservices, string entity, ColumnSet column, string condition, object value)

**Chức năng tổng quát:**
Hàm tiện ích chung để truy vấn nhiều bản ghi từ CRM bằng cách sử dụng `QueryExpression` đơn giản với một điều kiện lọc duy nhất (Equal).

**Logic nghiệp vụ chi tiết:**

1.  Tạo một `QueryExpression` cho entity được chỉ định.
2.  Thiết lập `ColumnSet` (các cột cần lấy).
3.  Tạo một `FilterExpression` và thêm một điều kiện: `condition` (tên trường) `ConditionOperator.Equal` với `value`.
4.  Thực hiện truy vấn và trả về `EntityCollection`.

### get_pmSchDtl_fromOpentryID(Guid opID)

**Chức năng tổng quát:**
Truy vấn các chi tiết lịch thanh toán (`bsd_paymentschemedetail`) chưa thanh toán (statuscode = 100000000) cho một Option Entry cụ thể.

**Logic nghiệp vụ chi tiết:**

1.  Tạo `QueryExpression` cho entity `bsd_paymentschemedetail`.
2.  Chỉ định một tập hợp các cột cần thiết cho việc tính toán lãi và ngày quá hạn (bao gồm `bsd_duedate`, `statuscode`, `bsd_balance`, `bsd_interestchargeper`, `bsd_ordernumber`, v.v.).
3.  Thiết lập điều kiện lọc:
    *   `bsd_optionentry` bằng `opID`.
    *   `statuscode` bằng `100000000` (Chưa thanh toán).
4.  Thực hiện truy vấn và trả về kết quả.

### get_all_pmSchDtl_fromOpentryID(Guid opID)

**Chức năng tổng quát:**
Truy vấn TẤT CẢ các chi tiết lịch thanh toán (`bsd_paymentschemedetail`) cho một Option Entry cụ thể, không phân biệt trạng thái.

**Logic nghiệp vụ chi tiết:**

1.  Tạo `QueryExpression` cho entity `bsd_paymentschemedetail`.
2.  Chỉ định một tập hợp các cột cần thiết (bao gồm `bsd_amountwaspaid`).
3.  Thiết lập điều kiện lọc: `bsd_optionentry` bằng `opID`.
4.  Hàm này được sử dụng trong phần tính toán Penalty để tính tổng số tiền đã thanh toán.

### CheckExist_optionEntry_on_Terminateletter(Guid opRef)

**Chức năng tổng quát:**
Kiểm tra xem đã có bản ghi Thư Chấm dứt Hợp đồng (`bsd_terminateletter`) nào được tạo cho Option Entry được tham chiếu hay chưa.

**Logic nghiệp vụ chi tiết:**

1.  Tạo `QueryExpression` cho entity `bsd_terminateletter`.
2.  Thiết lập điều kiện lọc: `bsd_optionentry` bằng `opRef`.
3.  Giới hạn kết quả trả về là 1 (`TopCount = 1`).
4.  Trả về `true` nếu tìm thấy bất kỳ bản ghi nào, ngược lại trả về `false`.

### findUnits(IOrganizationService crmservices, Entity OptionEntry)

**Chức năng tổng quát:**
Truy vấn các chi tiết đơn hàng (`salesorderdetail`) liên quan đến Option Entry để xác định các đơn vị (Units/Sản phẩm) được bán.

**Logic nghiệp vụ chi tiết:**

1.  Sử dụng `FetchExpression` (FetchXML) để truy vấn entity `salesorderdetail`.
2.  Lọc theo `salesorderid` (Option Entry ID).
3.  Trả về `EntityCollection` chứa các chi tiết đơn hàng, bao gồm `productid`.

### Các hàm tiện ích truy vấn khác

Các hàm `getPmShDtl`, `findPaymentScheme`, và `get_pmschDtl_outofDuedate` đều là các hàm tiện ích sử dụng FetchXML hoặc QueryExpression để truy vấn dữ liệu. Mặc dù chúng có thể không được sử dụng trực tiếp trong luồng logic chính của `Execute` (hoặc có thể là mã cũ/dự phòng), chúng phục vụ mục đích truy xuất dữ liệu cụ thể:

*   `getPmShDtl`: Truy vấn chi tiết lịch thanh toán dựa trên Option Entry ID (sử dụng FetchXML).
*   `findPaymentScheme`: Truy vấn các Sales Order (Option Entry) chung (sử dụng FetchXML, truy vấn này có vẻ rất rộng và có thể không hiệu quả).
*   `get_pmschDtl_outofDuedate`: Truy vấn Sales Order có liên kết với chi tiết lịch thanh toán quá hạn hơn 59 ngày và có trạng thái chưa thanh toán (statuscode = 100000000) (sử dụng FetchXML với `link-entity`).