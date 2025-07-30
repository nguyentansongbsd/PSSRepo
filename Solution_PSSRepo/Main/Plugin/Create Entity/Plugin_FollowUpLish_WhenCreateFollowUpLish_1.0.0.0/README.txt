Plugin_FollowUpLish_WhenCreateFollowUpLish
Mục đích
Plugin này tự động cập nhật trường bsd_followuplist trên các bản ghi liên quan khi tạo mới bản ghi bsd_followuplist trong Dynamics CRM. Plugin xử lý các trường hợp liên quan đến Reservation và OptionEntry.
Cách hoạt động
•	Khi một bản ghi bsd_followuplist được tạo và có liên kết tới Reservation hoặc OptionEntry:
•	Nếu liên kết tới Reservation:
•	Kiểm tra trạng thái của Reservation. Nếu đã "won" (statecode = 2), plugin sẽ báo lỗi và không cho phép tạo.
•	Nếu statuscode = 3, plugin sẽ chuyển trạng thái Reservation sang Active (statecode = 0, statuscode = 100000000), cập nhật trường bsd_followuplist = true, sau đó chuyển trạng thái về lại trạng thái ban đầu.
•	Nếu statuscode khác, chỉ cập nhật trường bsd_followuplist = true.
•	Nếu liên kết tới OptionEntry:
•	Cập nhật trường bsd_followuplist = true cho OptionEntry.
Cách sử dụng
1.	Triển khai plugin này lên môi trường Dynamics CRM.
2.	Đảm bảo plugin được đăng ký trên sự kiện "Create" của entity bsd_followuplist.
3.	Khi tạo mới bản ghi bsd_followuplist có liên kết tới Reservation hoặc OptionEntry, plugin sẽ tự động thực thi và cập nhật dữ liệu liên quan.
Lưu ý
•	Plugin chỉ thực thi ở depth = 1 để tránh lặp vô hạn.
•	Nếu Reservation đã "won", plugin sẽ ngăn không cho tạo mới bản ghi followuplist.
•	Plugin sử dụng .NET Framework 4.8 và C# 7.3.