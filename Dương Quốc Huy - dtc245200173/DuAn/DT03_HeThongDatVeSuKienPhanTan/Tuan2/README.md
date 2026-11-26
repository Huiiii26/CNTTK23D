# Tuần 2 · API xem sự kiện

**Dương Quốc Huy · dtc245200173 · CNTT K23D**

Tuần này xây dựng Ticket API bằng ASP.NET Core Minimal API trên .NET 10. Dữ liệu mẫu được lưu trong bộ nhớ.

Từ thư mục `Tuan2`, chạy:

```powershell
dotnet run --project TicketApi -- --urls http://localhost:5082
```

- `GET /events` trả danh sách sự kiện và số vé còn lại.
- `GET /events/3` trả chi tiết Chung kết Robotics.
- `GET /events/999` trả HTTP 404.

SQLite và tiếp nhận yêu cầu đặt vé được triển khai ở Tuần 3.
