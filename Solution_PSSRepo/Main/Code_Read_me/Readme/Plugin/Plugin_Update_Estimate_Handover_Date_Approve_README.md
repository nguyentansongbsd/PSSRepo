# Phân tích mã nguồn: Plugin_Update_Estimate_Handover_Date_Approve.cs

## Tổng quan

Tệp mã nguồn `Plugin_Update_Estimate_Handover_Date_Approve.cs` chứa một plugin C# được thiết kế để chạy trong môi trường Microsoft Dynamics 365/Power Platform. Plugin này thực hiện logic nghiệp vụ phức tạp sau khi một bản ghi yêu cầu cập nhật ngày bàn giao ước tính (`bsd_updateestimatehandoverdate`) được phê duyệt.

Chức năng chính của plugin là cập nhật các trường ngày tháng quan trọng (như ngày bàn giao ước tính và ngày OP Date) trên các bản ghi liên quan, bao gồm Đơn vị (Units - entity `product`), Chi tiết lịch thanh toán (`bsd_paymentschemedetail`), và Hợp đồng/Option Entry (entity `salesorder`), dựa trên loại yêu cầu cập nhật được chỉ định.

Plugin được kích hoạt trong giai đoạn xử lý bản ghi `bsd_updateestimatehandoverdate` và chỉ chạy nếu bản ghi đang ở trạng thái chờ xử lý và chưa được phê duyệt.

---

## Chi tiết các Hàm (Functions/Methods)

### Execute(IServiceProvider serviceProvider)

#### Chức năng tổng quát:

Đây là phương thức chính của plugin, chịu trách nhiệm khởi tạo các dịch vụ CRM, xác thực điều kiện kích hoạt, và thực hiện logic cập nhật hàng loạt các bản ghi Đơn vị, Lịch thanh toán và Hợp đồng dựa trên loại yêu cầu phê duyệt.

#### Logic nghiệp vụ chi tiết:

1.  **Khởi tạo Dịch vụ:**
    *   Lấy `IPluginExecutionContext` (service1), `IOrganizationServiceFactory` (factory), và `ITracingService` (service2).
    *   Tạo `IOrganizationService` (service) bằng cách sử dụng `factory.CreateOrganizationService` với ID người dùng hiện tại, đảm bảo plugin chạy với quyền của người dùng đó.

2.  **Kiểm tra Target và Entity:**
    *   Lấy đối tượng `Target` từ `InputParameters`.
    *   Kiểm tra xem `LogicalName` của Target có phải là `"bsd_updateestimatehandoverdate"` hay không. Nếu không phải, plugin dừng lại.

3.  **Truy xuất và Kiểm tra Điều kiện Phê duyệt:**
    *   Truy xuất toàn bộ bản ghi `handoverdate` hiện tại.
    *   Kiểm tra hai điều kiện để đảm bảo logic phê duyệt được thực thi:
        *   `statuscode` của bản ghi phải là `100000001` (Đây là một giá trị Option Set tùy chỉnh, thường đại diện cho trạng thái "Pending Approval" hoặc tương tự).
        *   Bản ghi *không* chứa trường `bsd_approvedrejectedperson` (đảm bảo logic phê duyệt chỉ chạy một lần).

4.  **Lấy Chi tiết Liên quan:**
    *   Lấy giá trị loại yêu cầu (`num = ((OptionSetValue) handoverdate["bsd_types"]).Value`).
    *   Gọi hàm `fetch_es` để lấy tất cả các bản ghi chi tiết liên quan (`bsd_updateestimatehandoverdatedetail`) vào `entityCollection1`.

5.  **Xử lý theo Loại Yêu cầu (`num`):**

    *   **Trường hợp 1: `num == 100000000` (Loại 1):**
        *   Kiểm tra bắt buộc phải có trường `bsd_opdate` trên bản ghi chính. Nếu không, ném ra `InvalidPluginExecutionException`.
        *   Lặp qua từng bản ghi chi tiết (`entity1`) trong `entityCollection1`.
        *   Lấy `unit` (EntityReference đến Unit/Product) và `bsd_estimatehandoverdatenew`.
        *   Nếu có `unit`, gọi `fetch_units` để lấy bản ghi Unit.
        *   Nếu tìm thấy Unit, cập nhật Unit đó:
            *   `bsd_estimatehandoverdate` được cập nhật bằng `bsd_estimatehandoverdatenew`.
            *   `bsd_opdate` được cập nhật bằng `bsd_opdate` từ bản ghi phê duyệt chính.
        *   Cập nhật `statuscode` của bản ghi chi tiết (`entity1`) thành `100000000` (Thường là trạng thái "Hoàn thành" hoặc "Đã phê duyệt").

    *   **Trường hợp 2: `num == 100000002` (Loại 2):**
        *   Kiểm tra bắt buộc phải có trường `bsd_opdate` trên bản ghi chính. Nếu không, ném lỗi.
        *   Lặp qua từng bản ghi chi tiết (`entity3`).
        *   Cập nhật `statuscode` của bản ghi chi tiết thành `100000000`.
        *   Nếu có `unit`, gọi `fetch_units` và cập nhật trường `bsd_opdate` trên Unit liên quan.
        *   **Xử lý Ngày đáo hạn thanh toán:**
            *   Nếu chi tiết (`entity3`) chứa `bsd_paymentduedate`:
                *   Nếu có `ins` (Installment/Payment Scheme Detail), gọi `fetch_ins` và cập nhật trường `bsd_duedate` trên bản ghi Installment bằng `bsd_paymentduedate` từ chi tiết.
            *   Nếu chi tiết KHÔNG chứa `bsd_paymentduedate`:
                *   Gọi `fetch_op_due` để tìm các chi tiết lịch thanh toán liên quan đến Option Entry (`op`) có phương thức tính toán ngày đáo hạn đặc biệt (`100000002`).
                *   Nếu tìm thấy, lấy `bsd_duedate` từ kết quả này.
                *   Nếu có `ins`, gọi `fetch_ins` và cập nhật `bsd_duedate` trên bản ghi Installment bằng ngày đáo hạn vừa tìm được.

    *   **Trường hợp 3: `num == 100000001` (Loại 3):**
        *   Kiểm tra bắt buộc phải có trường `bsd_opdate` trên bản ghi chính. Nếu không, ném lỗi.
        *   Lặp qua từng bản ghi chi tiết (`entity8`).
        *   Nếu có `unit`, gọi `fetch_units` và cập nhật Unit: `bsd_estimatehandoverdate` và `bsd_opdate`.
        *   **Xác định Ngày đáo hạn mới (`dateTime9`):**
            *   Ưu tiên `bsd_paymentduedate` từ bản ghi chi tiết.
            *   Nếu không có, lấy `bsd_paymentduedate` từ bản ghi phê duyệt chính.
            *   Nếu vẫn không có, sử dụng `bsd_estimatehandoverdatenew`.
        *   Nếu có `ins`, gọi `fetch_ins` và cập nhật `bsd_duedate` trên Installment bằng `dateTime9`.
        *   Nếu có `op` (Option Entry/Sales Order), gọi `fetch_op` và cập nhật trường `bsd_estimatehandoverdatecontract` trên Sales Order bằng `bsd_estimatehandoverdatenew`.
        *   Cập nhật `statuscode` của bản ghi chi tiết (`entity8`) thành `100000000`.

6.  **Hoàn tất Phê duyệt Bản ghi Chính:**
    *   Cập nhật bản ghi phê duyệt chính (`inputParameter`):
        *   Đặt `bsd_approvedrejecteddate` là thời điểm hiện tại (`DateTime.Now`).
        *   Đặt `bsd_approvedrejectedperson` là người dùng đang thực thi plugin.

### fetch_es(IOrganizationService crmservices, Entity handoverdate)

#### Chức năng tổng quát:

Truy vấn tất cả các bản ghi chi tiết cập nhật ngày bàn giao ước tính (`bsd_updateestimatehandoverdatedetail`) liên quan đến bản ghi phê duyệt chính.

#### Logic nghiệp vụ chi tiết:

1.  Hàm nhận `IOrganizationService` và bản ghi `handoverdate` chính.
2.  Xây dựng một truy vấn FetchXML để lấy entity `bsd_updateestimatehandoverdatedetail`.
3.  Truy vấn lọc theo điều kiện `bsd_updateestimatehandoverdate` bằng ID của bản ghi `handoverdate` chính.
4.  Truy xuất các thuộc tính cần thiết như `bsd_optionentry`, `bsd_units`, `bsd_paymentduedate`, `bsd_installment`, và `bsd_estimatehandoverdatenew`.
5.  Thực hiện truy vấn và trả về `EntityCollection`.

### fetch_units(IOrganizationService crmservices, EntityReference unit)

#### Chức năng tổng quát:

Truy vấn bản ghi Đơn vị (Unit), được lưu trữ trong entity `product`, dựa trên EntityReference được cung cấp.

#### Logic nghiệp vụ chi tiết:

1.  Hàm nhận `IOrganizationService` và `EntityReference` của Unit (`unit`).
2.  Xây dựng một truy vấn FetchXML để lấy entity `product`.
3.  Truy vấn lọc theo điều kiện `productid` bằng ID của EntityReference `unit`.
4.  Truy xuất các thuộc tính liên quan đến ngày bàn giao và OP Date (`bsd_estimatehandoverdate`, `bsd_opdate`).
5.  Thực hiện truy vấn và trả về `EntityCollection`.

### fetch_ins(IOrganizationService crmservices, EntityReference ins)

#### Chức năng tổng quát:

Truy vấn bản ghi Chi tiết Lịch thanh toán (`bsd_paymentschemedetail`) dựa trên EntityReference được cung cấp.

#### Logic nghiệp vụ chi tiết:

1.  Hàm nhận `IOrganizationService` và `EntityReference` của Installment (`ins`).
2.  Xây dựng một truy vấn FetchXML để lấy entity `bsd_paymentschemedetail`.
3.  Truy vấn lọc theo điều kiện `bsd_paymentschemedetailid` bằng ID của EntityReference `ins`.
4.  Thực hiện truy vấn và trả về `EntityCollection`.

### fetch_op(IOrganizationService crmservices, EntityReference op)

#### Chức năng tổng quát:

Truy vấn bản ghi Option Entry (entity `salesorder`) dựa trên EntityReference được cung cấp.

#### Logic nghiệp vụ chi tiết:

1.  Hàm nhận `IOrganizationService` và `EntityReference` của Option Entry (`op`).
2.  Xây dựng một truy vấn FetchXML để lấy entity `salesorder`.
3.  Truy vấn lọc theo điều kiện `salesorderid` bằng ID của EntityReference `op`.
4.  Truy xuất các thuộc tính như `bsd_optioncodesams`, `bsd_contractnumber`, v.v.
5.  Thực hiện truy vấn và trả về `EntityCollection`.

### RetrieveMultiRecord(IOrganizationService crmservices, string entity, ColumnSet column, string condition, object value)

#### Chức năng tổng quát:

Đây là một hàm tiện ích chung để truy vấn nhiều bản ghi bằng `QueryExpression` dựa trên một điều kiện lọc bằng (Equal) duy nhất.

#### Logic nghiệp vụ chi tiết:

1.  Hàm nhận tên entity, `ColumnSet`, tên trường điều kiện (`condition`), và giá trị điều kiện (`value`).
2.  Khởi tạo một `QueryExpression` cho entity được chỉ định.
3.  Thiết lập `ColumnSet` cho truy vấn.
4.  Tạo một `FilterExpression` mới.
5.  Thêm một `ConditionExpression` vào bộ lọc, sử dụng `ConditionOperator.Equal` với trường và giá trị được cung cấp.
6.  Thực hiện truy vấn bằng `service.RetrieveMultiple` và trả về `EntityCollection`.

### fetch_op_due(IOrganizationService crmservices, EntityReference op)

#### Chức năng tổng quát:

Truy vấn các bản ghi chi tiết lịch thanh toán (`bsd_paymentschemedetail`) liên quan đến một Option Entry cụ thể, nơi phương thức tính toán ngày đáo hạn được đặt là một giá trị tùy chỉnh (`100000002`).

#### Logic nghiệp vụ chi tiết:

1.  Hàm nhận `IOrganizationService` và `EntityReference` của Option Entry (`op`).
2.  Xây dựng một truy vấn FetchXML phức tạp:
    *   Truy vấn entity chính là `bsd_paymentschemedetail`.
    *   Lọc trực tiếp trên `bsd_paymentschemedetail` với điều kiện `bsd_duedatecalculatingmethod` bằng `100000002`.
    *   Sử dụng `link-entity` để liên kết `bsd_paymentschemedetail` với `salesorder` (thông qua trường `bsd_optionentry`).
    *   Lọc trên `salesorder` bằng điều kiện `salesorderid` bằng ID của EntityReference `op`.
3.  Mục đích là tìm ngày đáo hạn (`bsd_duedate`) từ các chi tiết thanh toán được liên kết với Option Entry và có phương thức tính toán đặc biệt.
4.  Thực hiện truy vấn và trả về `EntityCollection`.