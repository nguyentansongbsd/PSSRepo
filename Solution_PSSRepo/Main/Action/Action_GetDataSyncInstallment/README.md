# Action: Lấy Dữ liệu để Đồng bộ Installment (Action_GetDataSyncInstallment)

## Mô tả tổng quan

Plugin này hoạt động như một tiến trình xử lý hàng loạt (batch process) được kích hoạt bởi một Action trong Dynamics 365. Chức năng chính của nó là thu thập hai loại bản ghi khác nhau (`Warning Notices` và `Customer Notices`) chưa được xử lý, sau đó gọi một Action khác (`bsd_Action_Active_SynsImtallment`) để thực hiện việc đồng bộ hoặc cập nhật dữ liệu liên quan đến các kỳ thanh toán (Installment).

Việc chia nhỏ quá trình (lấy dữ liệu và xử lý dữ liệu) và xử lý theo từng lô (batch) giúp hệ thống xử lý một số lượng lớn bản ghi mà không vượt quá giới hạn thời gian thực thi của plugin, đảm bảo hiệu suất và sự ổn định.

## Logic chi tiết của từng Function

### `Execute(IServiceProvider serviceProvider)`

Đây là hàm chính của plugin, được thực thi khi Action được gọi. Quá trình xử lý được chia thành hai phần chính.

#### Phần 1: Xử lý `Warning Notices` (`bsd_warningnotices`)

1.  **Mục tiêu:** Tìm kiếm các bản ghi `Warning Notices` cần được xử lý dựa trên một số cảnh báo cụ thể.

2.  **Tham số đầu vào:**
    *   `wnumber`: Số thứ tự của cảnh báo (ví dụ: cảnh báo lần 1, lần 2,...). Dùng để xác định cột cần kiểm tra trong `paymentschemedetail`.
    *   `size`: Kích thước của mỗi lô xử lý (ví dụ: 500 bản ghi mỗi lần).

3.  **Logic truy vấn:**
    *   Sử dụng `QueryExpression` để xây dựng một truy vấn phức tạp.
    *   **Điều kiện lọc `Warning Notices`:**
        *   `statuscode` không phải là "Inactive" (giá trị 2).
        *   `bsd_numberofwarning` bằng với `wnumber` được truyền vào.
    *   **Liên kết (Link-Entity) với `Payment Scheme Detail` (`bsd_paymentschemedetail`):**
        *   Liên kết đến bản ghi chi tiết kỳ thanh toán liên quan.
        *   **Điều kiện lọc trên `Payment Scheme Detail`:** Kiểm tra xem trường `bsd_w_noticesnumber{wnumber}` (ví dụ: `bsd_w_noticesnumber1`) có bị rỗng (null) hay không. Điều này giúp xác định các kỳ thanh toán chưa được cập nhật cho lần cảnh báo này.

4.  **Xử lý hàng loạt:**
    *   Tất cả các bản ghi thỏa mãn điều kiện sẽ được lấy về.
    *   Plugin chia danh sách kết quả thành các lô nhỏ hơn dựa trên tham số `size`.
    *   Với mỗi lô:
        *   Tạo một chuỗi XML chứa ID của các bản ghi trong lô đó.
        *   Gọi Action `bsd_Action_Active_SynsImtallment`.
        *   Truyền vào chuỗi ID và một tham số `type` với giá trị là `"warningNo"` để Action xử lý biết cần phải thực hiện logic cho Warning Notices.

#### Phần 2: Xử lý `Customer Notices` (`bsd_customernotices`)

1.  **Mục tiêu:** Tìm kiếm các bản ghi `Customer Notices` (thông báo thanh toán) chưa được xử lý.

2.  **Điều kiện thực thi:**
    *   Phần này chỉ chạy nếu tham số đầu vào `payno_start` khác 0. Điều này cho phép kiểm soát việc có thực thi phần xử lý này hay không.

3.  **Logic truy vấn:**
    *   Sử dụng `FetchXML` để truy vấn dữ liệu.
    *   **Điều kiện lọc `Customer Notices`:**
        *   `statuscode` không phải là "Inactive" (giá trị 2).
    *   **Liên kết với `Payment Scheme Detail`:**
        *   Kiểm tra xem trường `bsd_paymentnoticesnumber` trên bản ghi `Payment Scheme Detail` liên quan có bị rỗng (null) hay không. Điều này giúp xác định các kỳ thanh toán chưa được cập nhật thông tin từ thông báo thanh toán.

4.  **Xử lý hàng loạt:**
    *   Tương tự như Phần 1, plugin lấy về các bản ghi thỏa mãn điều kiện.
    *   Chia kết quả thành các lô nhỏ.
    *   Với mỗi lô, gọi Action `bsd_Action_Active_SynsImtallment` với tham số `type` được đặt thành `"paymentno"`.

---

### Tóm tắt luồng hoạt động:

1.  **Nhận yêu cầu:** Action được gọi với các tham số để xác định loại và phạm vi công việc.
2.  **Thu thập Warning Notices:** Plugin tìm và thu thập ID của các `Warning Notices` cần xử lý.
3.  **Xử lý Warning Notices:** Gửi các ID này theo từng lô đến Action `bsd_Action_Active_SynsImtallment` để xử lý.
4.  **Thu thập Customer Notices:** Nếu được yêu cầu, plugin tiếp tục tìm và thu thập ID của các `Customer Notices` cần xử lý.
5.  **Xử lý Customer Notices:** Gửi các ID này theo từng lô đến cùng Action `bsd_Action_Active_SynsImtallment` để xử lý.
