# Tuần 3 · Lưu dữ liệu và tiếp nhận yêu cầu đặt vé

**Dương Quốc Huy · dtc245200173 · CNTT K23D**

Tuần này dùng SQLite để lưu sự kiện và đơn đặt vé. Khi nhận yêu cầu hợp lệ, API trả HTTP 202 cùng trạng thái `Pending` và đường dẫn tra cứu. Tồn vé được giữ nguyên trong lúc đơn chờ xử lý. Bước gửi yêu cầu qua RabbitMQ thuộc Tuần 4.

## Chạy thử

Cài .NET SDK 10. Từ thư mục `Tuan3`, chạy:

```sh
dotnet run --project TicketApi -- --urls http://localhost:5083
```

SQLite tự tạo dữ liệu mẫu ở `data/tickets.db` trong thư mục chạy của API. Có thể cấu hình vị trí bằng biến môi trường `Booking__Db`.

| Phương thức | Đường dẫn | Kết quả |
|---|---|---|
| GET | `/events` | Danh sách sự kiện và số vé hiện có |
| GET | `/events/{id}` | Chi tiết sự kiện hoặc 404 |
| POST | `/bookings` | Lưu yêu cầu và trả 202 Pending |
| GET | `/bookings/{id}` | Tra cứu trạng thái đơn |
| GET | `/users/{userId}/bookings` | Xem lịch sử theo mã người đặt |

Ví dụ trên PowerShell:

```powershell
$base = 'http://localhost:5083'
Invoke-RestMethod "$base/events"
$body = @{ eventId = 3; userId = 'demo01'; quantity = 2 } | ConvertTo-Json
$booking = Invoke-RestMethod "$base/bookings" -Method Post -ContentType 'application/json' -Body $body
Invoke-RestMethod "$base/bookings/$($booking.id)"
Invoke-RestMethod "$base/users/demo01/bookings"
```

Số lượng phải là số nguyên dương. Mã người đặt dài tối đa 64 ký tự và gồm chữ cái ASCII, chữ số, gạch dưới hoặc gạch nối. Dữ liệu sai trả 400. Mã sự kiện không tồn tại trả 404.

## Console Client

Trong một terminal khác tại Tuan3, gọi API đang chạy:

```powershell
dotnet run --project Ticket.Client -- events
dotnet run --project Ticket.Client -- event 3
dotnet run --project Ticket.Client -- book 3 demo01 2
dotnet run --project Ticket.Client -- history demo01
```

Dùng lệnh booking kèm mã đơn để xem trạng thái một đơn. Client mặc định gọi http://localhost:5083. Đặt biến môi trường TICKET_API_URL khi API chạy ở địa chỉ khác.
