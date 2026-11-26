var builder = WebApplication.CreateBuilder(args);
var app = builder.Build();

TicketEvent[] events =
[
    new(1, "Đêm nhạc Indie", "Hà Nội", 30),
    new(2, "Workshop Nhiếp ảnh", "Đà Nẵng", 12),
    new(3, "Chung kết Robotics", "TP. Hồ Chí Minh", 5)
];

app.MapGet("/events", () => events);
app.MapGet("/events/{id:int}", (int id) =>
    Array.Find(events, entry => entry.Id == id) is { } entry
        ? Results.Ok(entry)
        : Results.NotFound());

app.Run();

record TicketEvent(int Id, string Name, string Venue, int Remaining);
