using Ticketing;

var builder = WebApplication.CreateBuilder(args);
var store = new Store(builder.Configuration["Booking:Db"] ?? "data/tickets.db");
store.Initialize();

var app = builder.Build();

app.MapGet("/events", () => store.Events());
app.MapGet("/events/{id:int}", (int id) =>
    store.Events(id).SingleOrDefault() is { } entry ? Results.Ok(entry) : Results.NotFound());

app.MapPost("/bookings", (BookingRequest request, ILogger<Program> logger) =>
{
    var userId = request.UserId?.Trim();
    if (string.IsNullOrEmpty(userId) || userId.Length > 64 ||
        userId.Any(ch => !char.IsAsciiLetterOrDigit(ch) && ch != '-' && ch != '_') ||
        request.Quantity <= 0 || request.EventId <= 0)
        return Results.BadRequest(new { message = "Mã người đặt, sự kiện hoặc số vé không hợp lệ." });

    var booking = store.Create(request.EventId, userId, request.Quantity);
    if (booking is null)
        return Results.NotFound(new { message = "Không tìm thấy sự kiện." });

    logger.LogInformation("Booking {BookingId} accepted as Pending", booking.Id);
    return Results.Accepted($"/bookings/{booking.Id}", booking);
});

app.MapGet("/bookings/{id:guid}", (Guid id) =>
    store.Bookings(id).SingleOrDefault() is { } booking ? Results.Ok(booking) : Results.NotFound());
app.MapGet("/users/{userId}/bookings", (string userId) => store.Bookings(user: userId));

app.Run();
