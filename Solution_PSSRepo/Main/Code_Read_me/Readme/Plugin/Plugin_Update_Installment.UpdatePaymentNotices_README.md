# Phân tích mã nguồn: Plugin_Update_Installment.UpdatePaymentNotices.cs

## Tổng quan

Tệp mã nguồn này định nghĩa một Plugin (Custom Workflow Activity) trong môi trường Microsoft Dynamics 365/Power Platform, được thiết kế để tự động cập nhật các trường tài chính trên thực thể "Thông báo Thanh toán" (`bsd_customernotices`) khi có sự thay đổi liên quan đến các đợt thanh toán (Installment Details).

Plugin này thực hiện việc tính toán lại các trường quan trọng như số tiền thanh toán trước, số tiền thiếu hụt từ các đợt trước, và tổng số tiền cần chuyển, đảm bảo rằng thông tin tài chính trên Thông báo Thanh toán luôn đồng bộ và chính xác dựa trên dữ liệu của Option Entry và các đợt thanh toán trước đó.

---

## Chi tiết các Hàm (Functions/Methods)

### GetPaymentNoticesByInstalment(string instalmentId, string OPId)

#### Chức năng tổng quát
Hàm này chịu trách nhiệm truy vấn và trả về bản ghi Thông báo Thanh toán (`bsd_customernotices`) duy nhất đang ở trạng thái "Generate" (1), dựa trên ID của Đợt thanh toán (Installment Detail ID) và ID của Option Entry.

#### Logic nghiệp vụ chi tiết
1.  **Khởi tạo Truy vấn:** Tạo một đối tượng `QueryExpression` nhắm vào thực thể `bsd_customernotices`.
2.  **Thiết lập Cột:** Yêu cầu truy xuất tất cả các cột (`ColumnSet.AllColumns = true`).
3.  **Thiết lập Điều kiện (Criteria):**
    *   Thêm điều kiện: Trường `bsd_optionentry` phải bằng giá trị `OPId` được cung cấp.
    *   Thêm điều kiện: Trường `bsd_paymentschemedetail` (liên kết với Installment) phải bằng giá trị `instalmentId` được cung cấp.
    *   Thêm điều kiện: Trường `statuscode` (Trạng thái) phải bằng 1 (thường đại diện cho trạng thái "Generate" hoặc "Active" trong ngữ cảnh này).
4.  **Thực thi Truy vấn:** Thực hiện truy vấn bằng `service.RetrieveMultiple(query)`.
5.  **Xử lý Kết quả:**
    *   Nếu tập hợp kết quả (`rs.Entities`) có nhiều hơn 0 bản ghi, hàm sẽ trả về bản ghi đầu tiên (`rs[0]`).
    *   Nếu không có bản ghi nào khớp, hàm trả về `null`.

### Execute(IServiceProvider serviceProvider)

#### Chức năng tổng quát
Đây là điểm vào chính của Plugin, thực hiện việc khởi tạo các dịch vụ CRM, truy xuất dữ liệu liên quan, tính toán lại các trường tài chính trên Thông báo Thanh toán hiện tại, và sau đó gọi hàm cập nhật các Thông báo Thanh toán khác.

#### Logic nghiệp vụ chi tiết
1.  **Khởi tạo Dịch vụ:** Khởi tạo các đối tượng cần thiết cho môi trường Plugin: `context`, `factory`, `service`, và `tracingService`.
2.  **Truy xuất Target:** Lấy thực thể `Target` từ `context.InputParameters`. Sau đó, truy xuất bản ghi đầy đủ (`enTarget`) của thực thể này từ CRM.
3.  **Xác định Option Entry:**
    *   Kiểm tra nếu `enTarget` không chứa trường `bsd_optionentry`, Plugin sẽ thoát (return).
    *   Lấy `EntityReference` của Option Entry (`enOPRef`) và truy xuất bản ghi Option Entry đầy đủ (`EnOP`).
4.  **Tìm Payment Notice Hiện tại:**
    *   Gọi hàm `GetPaymentNoticesByInstalment` để tìm bản ghi Thông báo Thanh toán (`enCreated`) liên quan đến Installment ID (`enTarget.Id`) và Option Entry ID (`EnOP.Id`).
    *   Nếu không tìm thấy `enCreated` (bản ghi Payment Notice), Plugin sẽ thoát (return).
5.  **Chuẩn bị Cập nhật:** Tạo một thực thể `enUpdate` mới để chứa các giá trị cập nhật cho `enCreated`.
6.  **Truy xuất Installment Detail:** Lấy `EntityReference` của Installment Detail (`enInsDetailRef`) từ `enCreated` và truy xuất bản ghi Installment Detail đầy đủ (`enInsDetail`).
7.  **Tính toán Trường 1: `bsd_amountofthisphase` (Số tiền trong EDA):**
    *   Gán giá trị của `bsd_amountofthisphase` từ `enInsDetail` sang `enUpdate`. Nếu trường không tồn tại, gán giá trị `Money(0)`.
8.  **Tính toán Trường 2: `bsd_totaladvancepayment` (Số tiền thanh toán trước):**
    *   Gán giá trị của `bsd_totaladvancepayment` từ Option Entry (`EnOP`) sang `enUpdate`. Nếu trường không tồn tại, gán giá trị `Money(0)`.
9.  **Tính toán Trường 3: `bsd_totalprepaymentamount` (Tổng số tiền thanh toán trước):**
    *   **Logic:** `bsd_totalprepaymentamount` = `bsd_totaladvancepayment` + `bsd_amountwaspaid` (của đợt Installment hiện tại).
    *   Nếu `enInsDetail` chứa `bsd_amountwaspaid`, thực hiện phép cộng giá trị tiền tệ.
    *   Nếu không, gán `bsd_totalprepaymentamount` bằng `bsd_totaladvancepayment`.
10. **Tính toán Trường 4: `bsd_shortfallinpreviousinstallment` (Số tiền chưa thanh toán các đợt trước):**
    *   Khởi tạo biến `sum_bsd_balance = 0`.
    *   Lấy số thứ tự (`bsd_ordernumber`) của Installment Detail hiện tại (`enInsDetail`).
    *   **Truy vấn các đợt trước:** Thực hiện truy vấn trên `bsd_paymentschemedetail` với các điều kiện:
        *   Cùng `bsd_optionentry` với `EnOP`.
        *   `bsd_ordernumber` **nhỏ hơn** số thứ tự của đợt hiện tại.
    *   Lặp qua danh sách các đợt trước (`insDetailList.Entities`) và cộng dồn giá trị của trường `bsd_balance` vào `sum_bsd_balance`.
    *   Cập nhật `enUpdate["bsd_shortfallinpreviousinstallment"]` bằng tổng này.
11. **Tính toán Trường 5: `bsd_amounttotransfer` (Số tiền phải chuyển):**
    *   **Logic:** `bsd_amounttotransfer` = `bsd_amountofthisphase` - `bsd_totalprepaymentamount` + `bsd_shortfallinpreviousinstallment`.
    *   Thực hiện phép tính và cập nhật `enUpdate`.
12. **Cập nhật và Xử lý Tiếp theo:**
    *   Thực hiện `service.Update(enUpdate)` để lưu các thay đổi cho Payment Notice hiện tại.
    *   Gọi hàm `UpdatePaymentNoticeOther(EnOP, orderNumberInsDetail)` để cập nhật các Payment Notice của các đợt sau.

### UpdatePaymentNoticeOther(Entity EnOP, int orderNumberInsDetail)

#### Chức năng tổng quát
Hàm này được gọi sau khi Payment Notice hiện tại được cập nhật. Nó có nhiệm vụ tìm kiếm và cập nhật lại các tính toán tài chính cho tất cả các Thông báo Thanh toán khác thuộc cùng Option Entry, đặc biệt là những đợt có số thứ tự lớn hơn đợt vừa được xử lý.

#### Logic nghiệp vụ chi tiết
1.  **Truy vấn tất cả Payment Notices:** Truy vấn tất cả các bản ghi `bsd_customernotices` liên quan đến `EnOP` (Option Entry).
2.  **Lặp qua các Payment Notices:** Duyệt qua từng Payment Notice (`item`) trong danh sách kết quả.
3.  **Kiểm tra Đợt Thanh toán (Installment Order):**
    *   Đối với mỗi Payment Notice, truy vấn bản ghi Installment Detail liên quan (`bsd_paymentschemedetail`).
    *   Thêm điều kiện kiểm tra: `bsd_ordernumber` của Installment Detail phải **lớn hơn** `orderNumberInsDetail` (số thứ tự của đợt đã được xử lý trong hàm `Execute`).
    *   Nếu truy vấn trả về kết quả (`rs.Entities.Count > 0`), điều đó có nghĩa là Payment Notice này thuộc về một đợt thanh toán sau và cần được cập nhật.
4.  **Thực hiện Cập nhật (Tái sử dụng Logic):**
    *   Lấy bản ghi Installment Detail (`enTarget`) của đợt sau.
    *   Tìm Payment Notice hiện hữu (`enCreated`) cho đợt này bằng cách gọi `GetPaymentNoticesByInstalment`. Nếu không tìm thấy, hàm sẽ thoát (return).
    *   Tạo thực thể `enUpdate` và truy xuất Installment Detail đầy đủ (`enInsDetail`) cho đợt này.
    *   **Tái tính toán các trường tài chính:** Lặp lại chính xác các bước tính toán từ 7 đến 11 trong hàm `Execute` (bao gồm `bsd_amountofthisphase`, `bsd_totaladvancepayment`, `bsd_totalprepaymentamount`).
    *   **Tính toán `bsd_shortfallinpreviousinstallment` mới:**
        *   Lấy số thứ tự hiện tại (`orderNumberInsDetailCurent`) của đợt đang được xử lý.
        *   Thực hiện truy vấn mới để tính tổng `bsd_balance` của tất cả các đợt có `bsd_ordernumber` **nhỏ hơn** `orderNumberInsDetailCurent`.
        *   Cập nhật `bsd_shortfallinpreviousinstallment`.
    *   **Tính toán `bsd_amounttotransfer` mới:** Tính toán lại dựa trên các giá trị mới.
    *   Thực hiện `service.Update(enUpdate)`.