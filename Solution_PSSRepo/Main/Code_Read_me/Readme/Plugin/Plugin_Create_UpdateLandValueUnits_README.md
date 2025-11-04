# Phân tích mã nguồn: Plugin_Create_UpdateLandValueUnits.cs

## Tổng quan

Tệp mã nguồn `Plugin_Create_UpdateLandValueUnits.cs` chứa một plugin Dynamics 365/CRM được thiết kế để thực thi logic nghiệp vụ khi một bản ghi (entity) được tạo hoặc cập nhật. Plugin này chịu trách nhiệm tự động liên kết bản ghi hiện tại với một Option Entry (thường là thực thể `salesorder`) đang hoạt động dựa trên đơn vị (Unit) được liên kết. Sau khi tìm thấy Option Entry, plugin sẽ sao chép các giá trị tài chính (tiền tệ) từ Option Entry sang bản ghi hiện tại và thực hiện các phép tính phức tạp liên quan đến giá trị đất đai, VAT, và chênh lệch giá trị.

Plugin này chỉ thực thi logic nếu bản ghi đích đáp ứng một điều kiện loại (type) cụ thể (`bsd_type` = 100000000).

## Chi tiết các Hàm (Functions/Methods)

### Execute(IServiceProvider serviceProvider)

#### Chức năng tổng quát

Hàm này là điểm vào chính của plugin, chịu trách nhiệm thiết lập môi trường CRM (dịch vụ, ngữ cảnh) và thực thi toàn bộ logic nghiệp vụ: tìm kiếm Option Entry liên quan đến đơn vị, kiểm tra trạng thái, ánh xạ các trường tài chính, tính toán lại các giá trị mới (New Values) và cập nhật bản ghi đích.

#### Logic nghiệp vụ chi tiết

1.  **Khởi tạo Dịch vụ CRM:**
    *   Truy xuất `IPluginExecutionContext` (ngữ cảnh thực thi), `IOrganizationServiceFactory`, `IOrganizationService` (dịch vụ tổ chức), và `ITracingService` (dịch vụ theo dõi/ghi log) từ `serviceProvider`.

2.  **Truy xuất Bản ghi Đích:**
    *   Lấy bản ghi đích (`entity`) từ `context.InputParameters["Target"]`.
    *   Thực hiện `service.Retrieve` để lấy bản ghi đầy đủ (`enCreated`) với tất cả các cột (`ColumnSet(true)`), đảm bảo có đủ dữ liệu cho các bước kiểm tra tiếp theo.

3.  **Kiểm tra Điều kiện Loại:**
    *   Kiểm tra xem giá trị của trường `bsd_type` trên bản ghi (`enCreated`) có phải là `100000000` không. Nếu không, plugin sẽ kết thúc mà không thực hiện thêm hành động nào.

4.  **Kiểm tra Đơn vị Liên kết:**
    *   Kiểm tra xem bản ghi có chứa trường tham chiếu đơn vị (`bsd_units`) không. Nếu có, lấy `EntityReference` của đơn vị (`enUnitRef`).

5.  **Tìm kiếm Option Entry (salesorder):**
    *   Định nghĩa 6 mã trạng thái (`statuscode`) hợp lệ cho Option Entry (từ `100000000` đến `100000005`).
    *   Tạo `QueryExpression` nhắm vào thực thể `salesorder`.
    *   Thiết lập điều kiện truy vấn:
        *   `bsd_unitnumber` phải bằng ID của đơn vị liên kết (`enUnitRef.Id.ToString()`).
        *   `statuscode` phải nằm trong 6 mã trạng thái đã định nghĩa (sử dụng `ConditionOperator.In`).
    *   Thực thi truy vấn (`service.RetrieveMultiple`).

6.  **Xử lý Kết quả Truy vấn:**

    *   **Nếu tìm thấy Option Entry (rs.Entities.Count > 0):**
        *   Khởi tạo một thực thể mới (`enUpdate`) để chứa các giá trị cập nhật cho bản ghi đích.
        *   Thiết lập trường `bsd_optionentry` trên `enUpdate` trỏ đến Option Entry tìm thấy đầu tiên (`rs.Entities[0]`).

    *   **Logic Ánh xạ và Tính toán Giá trị (Map Value):**
        *   **Kiểm tra Điều kiện Ánh xạ:** Đảm bảo `bsd_type` vẫn là `100000000` và `bsd_optionentry` đã được thiết lập.
        *   **Truy xuất Option Entry Chi tiết:** Lấy Option Entry đầy đủ (`entity1`) để truy cập các trường tài chính.
        *   **Kiểm tra Trạng thái Chấm dứt:** Nếu `statuscode` của Option Entry (`entity1`) là `100000006` (Terminated), ném ra `InvalidPluginExecutionException` để ngăn chặn việc cập nhật.
        *   **Ánh xạ Giá trị Tiền tệ (Current Values):**
            *   Truy xuất 8 giá trị tiền tệ (`Money`) từ Option Entry (`entity1`) (ví dụ: `bsd_detailamount`, `bsd_discount`, `totaltax`, v.v.). Nếu trường không tồn tại, gán giá trị mặc định là 0.
            *   Gán các giá trị này vào các trường "Current" và một số trường "New" tương ứng trên `enUpdate` (ví dụ: `bsd_listedpricecurrent`, `bsd_discountcurrent`, `bsd_listedpricenew`, v.v.).

        *   **Tìm kiếm Chi tiết Kế hoạch Thanh toán (Payment Scheme Detail):**
            *   Sử dụng `FetchExpression` để tìm bản ghi `bsd_paymentschemedetail` liên quan đến Option Entry.
            *   Điều kiện tìm kiếm: `bsd_optionentry` bằng ID của Option Entry và `bsd_duedatecalculatingmethod` bằng `100000002`.
            *   Nếu tìm thấy, liên kết bản ghi chi tiết này vào trường `bsd_installment` và sao chép giá trị `bsd_amountofthisphase` vào `bsd_amountofthisphasecurrent` trên `enUpdate`.

        *   **Thực hiện Tính toán Tài chính Phức tạp (New Values Calculation):**
            *   Truy xuất bản ghi Unit (`entity22`) và Option Entry (`entity3`).
            *   Lấy các giá trị đầu vào cần thiết (dưới dạng Decimal):
                *   `num2`: `bsd_freightamount` (Phí vận chuyển) từ Option Entry.
                *   `num3`: `bsd_landvaluenew` (Giá trị đất mới) từ bản ghi hiện tại (`enCreated`).
                *   `num4`: `bsd_netsellingpricenew` (Giá bán ròng mới) từ `enUpdate`.
                *   `num5`: `bsd_totalamount` (Tổng số tiền) từ `enUpdate`.
                *   `num8`: `bsd_netsaleablearea` (Diện tích bán ròng) từ Unit (`entity22`).
            *   **Thực hiện các phép tính:**
                *   `num9` (Giá trị khấu trừ đất mới - `bsd_landvaluedeductionnew`): `num3` (Giá trị đất mới) nhân với `num8` (Diện tích bán ròng).
                *   `num10` (Tổng thuế VAT mới - `bsd_totalvattaxnew`): (`num4` (Giá bán ròng mới) trừ `num9`) nhân với 0.1M (10%).
                *   `num11` (Tổng số tiền tính toán): `num4` + `num10` + `num2` (Phí vận chuyển).
                *   `num12` (Chênh lệch giá trị - `bsd_valuedifference`): Làm tròn (`num5` (Tổng số tiền ban đầu) trừ `num11`).
            *   **Cập nhật các trường "New" và "Difference":** Gán các giá trị `num2`, `num9`, `num10`, `num12` (dưới dạng `Money`) vào các trường tương ứng trên `enUpdate`.
            *   **Tính toán Lượng Thanh toán Giai đoạn này:** Cập nhật trường `bsd_amountofthisphase` bằng cách làm tròn (`num7` (`bsd_amountofthisphasecurrent`) trừ `num12`).

    *   **Thực thi Cập nhật:** Gọi `service.Update(enUpdate)` để lưu các thay đổi đã tính toán vào bản ghi đích.

    *   **Nếu không tìm thấy Option Entry (rs.Entities.Count == 0):**
        *   Ném ra `Exception` với thông báo lỗi: "Unit don't have option entry. Please check again."