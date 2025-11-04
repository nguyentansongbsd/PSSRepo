# Phân tích mã nguồn: Action_Termination_Refund.cs

## Tổng quan

Tệp mã nguồn `Action_Termination_Refund.cs` chứa một Plugin Dynamics 365/CRM được thiết kế để xử lý quy trình chấm dứt (termination) một giao dịch (thường là Reservation hoặc Option Entry) và tự động tạo bản ghi Hoàn tiền (`bsd_refund`) tương ứng.

Plugin này thực thi trên thực thể `bsd_termination`. Logic chính bao gồm việc xác thực các trường bắt buộc, cập nhật trạng thái của các bản ghi liên quan (Unit, Reservation/Option Entry, Quote), tạo bản ghi hoàn tiền, và trong trường hợp đặc biệt là Resell, nó sẽ cập nhật lại giá và trạng thái của Unit để chuẩn bị cho việc bán lại.

## Chi tiết các Hàm (Functions/Methods)

### IPlugin.Execute(IServiceProvider serviceProvider)

#### Chức năng tổng quát:
Đây là hàm thực thi chính của Plugin, chịu trách nhiệm xử lý toàn bộ logic nghiệp vụ khi một bản ghi `bsd_termination` được tạo hoặc cập nhật.

#### Logic nghiệp vụ chi tiết:
1.  **Khởi tạo và Xác thực Context:**
    *   Lấy `IPluginExecutionContext` và xác định tham số đầu vào (`Target`), phải là một `EntityReference`.
    *   Kiểm tra xem `LogicalName` của Target có phải là `"bsd_termination"` hay không. Nếu không, hàm kết thúc ngay lập tức.
    *   Khởi tạo `IOrganizationServiceFactory` và `IOrganizationService` để tương tác với dữ liệu CRM.

2.  **Truy xuất Dữ liệu Termination:**
    *   Truy xuất bản ghi `bsd_termination` hiện tại, lấy 13 trường quan trọng (bao gồm trạng thái, nguồn, đơn vị, số tiền, và các tham chiếu đến Reservation/Option Entry).

3.  **Kiểm tra Ban đầu:**
    *   Xác định biến `flag` (kiểm tra xem đây có phải là giao dịch bán lại - `bsd_resell` hay không).
    *   Lấy tham chiếu đến Phase Launch (`bsd_phaselaunch`) nếu có.
    *   **Xác thực Nguồn:** Nếu trường `bsd_source` không tồn tại, ném ra lỗi `InvalidPluginExecutionException` yêu cầu người dùng chọn Nguồn.

4.  **Xử lý theo Nguồn (Switch Case):**

    *   **Case 1: Nguồn là Reservation (Giá trị OptionSet: 100000000)**
        *   **Xác thực Trạng thái:** Chỉ xử lý nếu trạng thái hiện tại của Termination là `1` (thường là Active/Draft).
        *   **Xác thực Trường bắt buộc:** Kiểm tra sự tồn tại của `bsd_units` và `bsd_quotationreservation`. Nếu thiếu, ném lỗi.
        *   **Truy xuất Dữ liệu Liên quan:** Lấy thông tin chi tiết của Unit (`bsd_units`) và Reservation (`bsd_quotationreservation`).
        *   **Cập nhật Termination:** Cập nhật trạng thái của bản ghi Termination thành `100000001` (thường là Hoàn thành).
        *   **Xác định Tên Khách hàng và Tạo Tên Refund:**
            *   Truy xuất thông tin khách hàng (`customerid`) từ Reservation.
            *   Dựa vào loại khách hàng (Contact/Account), truy xuất tên đầy đủ và tạo tên cho bản ghi Refund theo định dạng: `"Refund-" + [Tên Reservation] + "-" + [Số Unit] + "-" + [Tên Khách hàng]`.
        *   **Tạo Bản ghi Refund (`bsd_refund`):**
            *   Gán các trường thông tin như Unit, Reservation, Số tiền đã thanh toán, Dự án, Loại Refund (`100000001`), Khách hàng, và Số tiền hoàn lại.
        *   **Xác thực Phase Launch:** Gọi hàm `find_phase`. Nếu Phase Launch tồn tại và trạng thái của nó *không phải* là `100000000` (Launched), ném lỗi.
        *   **Lưu Refund:** Thực hiện `service.Create(entity4)`.
        *   **Xác thực Trạng thái Reservation:** Kiểm tra xem trạng thái của Reservation có phải là `3` (thường là Won/Active) hay không. Nếu không, ném lỗi.
        *   **Đóng Reservation:** Thực hiện `CloseQuoteRequest` để đóng Reservation/Quote, đặt trạng thái đóng là `100000001` (thường là Canceled/Lost).
        *   **Cập nhật Unit:** Cập nhật trạng thái của Unit về `1` (thường là Available/Mở bán).

    *   **Case 2: Nguồn là Option Entry (Giá trị OptionSet: 100000001)**
        *   **Xác thực Trạng thái:** Kiểm tra nếu Termination đã hoàn thành (`100000001`), ném lỗi.
        *   **Xác thực Trường bắt buộc:** Kiểm tra sự tồn tại của `bsd_optionentry`, `bsd_units`, `bsd_followuplist`, `bsd_totalamountpaid`, và `bsd_totalforfeitureamount`. Nếu thiếu, ném lỗi.
        *   **Xác thực Trạng thái:** Chỉ xử lý nếu trạng thái hiện tại của Termination là `1`.
        *   **Cập nhật Termination:** Cập nhật trạng thái của bản ghi Termination thành `100000001`.
        *   **Truy xuất Dữ liệu Liên quan:** Lấy thông tin Option Entry, Quote (từ Option Entry), và Unit.
        *   **Xác thực Khách hàng:** Kiểm tra Option Entry có `customerid` hay không. Nếu không, ném lỗi.
        *   **Cập nhật Option Entry:** Cập nhật trạng thái của Option Entry thành `100000006` (thường là Terminated).
        *   **Cập nhật Quote:** Cập nhật trạng thái của Quote liên quan thành `statecode=3` (Closed) và `statuscode=100000001` (Canceled).
        *   **Cập nhật Unit:**
            *   Cập nhật trạng thái Unit về `1`.
            *   Xóa liên kết `bsd_phaseslaunchid`.
            *   Đặt `bsd_terminated = true`.
            *   Tăng giá trị trường `bsd_terminatecount` lên 1.
        *   **Tạo Bản ghi Refund (`bsd_refund`):**
            *   Tương tự như Case Reservation, xác định tên khách hàng và tạo tên Refund.
            *   Gán các trường thông tin, bao gồm Unit, Option Entry, Số tiền, Khách hàng, và Termination.
        *   **Xác thực Phase Launch:** Gọi hàm `find_phase`. Nếu Phase Launch tồn tại và trạng thái của nó *không phải* là `100000000`, ném lỗi.
        *   **Lưu Refund:** Thực hiện `service.Create(entity13)`.

5.  **Xử lý Resell và Cập nhật Giá Unit (Logic Hậu kỳ):**
    *   Logic này chỉ chạy nếu Termination là Resell (`flag` là true) VÀ có liên kết Phase Launch (`entityReference` khác null).
    *   Lấy tham chiếu Unit.
    *   Gọi `fetch_phase` để lấy thông tin chi tiết Phase Launch (bao gồm `bsd_pricelistid`).
    *   **Vòng lặp qua các Phase Launch:**
        *   Lấy `PriceListId` (`pha`) từ Phase Launch.
        *   Gọi `checkunit_phase` để kiểm tra xem Unit đã có giá (Product Price Level) trong Price List này chưa.
        *   **Nếu Unit Đã Có Giá:**
            *   Truy xuất lại thông tin Unit.
            *   Lặp qua các bản ghi giá đã tìm thấy.
            *   Cập nhật Unit: Đặt `statuscode` là `100000000` (thường là Available for Resell), gán lại `bsd_phaseslaunchid`, và cập nhật trường `price` bằng giá trị tìm thấy.
        *   **Nếu Unit Chưa Có Giá:**
            *   Gọi `price_list_new` để lấy bản ghi `productpricelevel` mới nhất của Unit (giá gốc/gần nhất).
            *   Nếu tìm thấy giá mới nhất:
                *   **Tạo Product Price Level Mới:** Tạo một bản ghi `productpricelevel` mới, liên kết Unit với Price List của Phase Launch (`pha`), sử dụng các thông tin giá, đơn vị, tiền tệ từ bản ghi giá mới nhất tìm được.
                *   **Cập nhật Unit:** Cập nhật Unit: Đặt `statuscode` là `100000000`, gán lại `bsd_phaseslaunchid`, và cập nhật trường `price` bằng giá trị vừa được sử dụng.

### private EntityCollection fetch_phase(IOrganizationService crmservices, EntityReference pha)

#### Chức năng tổng quát:
Hàm này truy vấn thông tin chi tiết của một bản ghi Phase Launch (`bsd_phaseslaunch`) dựa trên ID của nó.

#### Logic nghiệp vụ chi tiết:
1.  Hàm nhận `IOrganizationService` và `EntityReference` của Phase Launch (`pha`) làm tham số.
2.  Xây dựng câu lệnh FetchXML để truy vấn thực thể `bsd_phaseslaunch`.
3.  Lọc theo điều kiện `bsd_phaseslaunchid` bằng với ID của tham số `pha`.
4.  Truy xuất các thuộc tính như `bsd_name`, `createdon`, `bsd_pricelistid`, v.v.
5.  Thực hiện truy vấn và trả về `EntityCollection`.

### private EntityCollection checkunit_phase(IOrganizationService crmservices, EntityReference pha, EntityReference unit)

#### Chức năng tổng quát:
Hàm này kiểm tra xem một Unit cụ thể đã có mức giá được định nghĩa trong một Bảng giá (Price List) cụ thể liên kết với Phase Launch hay chưa.

#### Logic nghiệp vụ chi tiết:
1.  Hàm nhận `IOrganizationService`, `EntityReference` của Price List (`pha`), và `EntityReference` của Unit (`unit`).
2.  Xây dựng câu lệnh FetchXML để truy vấn thực thể `productpricelevel`.
3.  Lọc theo hai điều kiện chính:
    *   `productid` (Unit) bằng với ID của tham số `unit`.
    *   Sử dụng `link-entity` để liên kết với thực thể `pricelevel` và lọc theo `pricelevelid` bằng với ID của tham số `pha`.
4.  Truy xuất các thuộc tính `amount` và `pricingmethodcode`.
5.  Thực hiện truy vấn và trả về `EntityCollection` chứa các mức giá tìm thấy.

### private EntityCollection price_list_new(IOrganizationService crmservices, EntityReference unit)

#### Chức năng tổng quát:
Hàm này tìm và trả về bản ghi mức giá (Product Price Level) mới nhất được tạo cho một Unit cụ thể.

#### Logic nghiệp vụ chi tiết:
1.  Hàm nhận `IOrganizationService` và `EntityReference` của Unit (`unit`).
2.  Xây dựng câu lệnh FetchXML để truy vấn thực thể `productpricelevel`.
3.  Lọc theo điều kiện `productid` bằng với ID của tham số `unit`.
4.  Sử dụng thuộc tính `top='1'` và sắp xếp theo `createdon` giảm dần (`descending='true'`) để đảm bảo chỉ lấy bản ghi mới nhất.
5.  Truy xuất các thuộc tính liên quan đến giá (amount, uomid, pricingmethodcode, v.v.).
6.  Thực hiện truy vấn và trả về `EntityCollection` (chỉ chứa tối đa 1 bản ghi).

### private EntityCollection find_phase(IOrganizationService service, EntityReference phase)

#### Chức năng tổng quát:
Hàm này kiểm tra xem một Phase Launch có tồn tại và trạng thái của nó có khác với trạng thái "Launched" hay không.

#### Logic nghiệp vụ chi tiết:
1.  Hàm nhận `IOrganizationService` và `EntityReference` của Phase Launch (`phase`).
2.  Xây dựng câu lệnh FetchXML để truy vấn thực thể `bsd_phaseslaunch`.
3.  Lọc theo hai điều kiện:
    *   `bsd_phaseslaunchid` bằng với ID của tham số `phase`.
    *   `statuscode` **khác** (`ne`) với giá trị `100000000` (thường là trạng thái Launched).
4.  Nếu truy vấn trả về kết quả, điều đó có nghĩa là Phase Launch tồn tại nhưng chưa ở trạng thái Launched, dẫn đến việc Plugin chính ném lỗi.
5.  Thực hiện truy vấn và trả về `EntityCollection`.