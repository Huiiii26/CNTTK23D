using Microsoft.Data.Sqlite;

namespace Ticketing;

public record TicketEvent(int Id, string Name, string Venue, int Remaining);
public record Booking(Guid Id, int EventId, string UserId, int Quantity, string Status);
public record BookingRequest(int EventId, string? UserId, int Quantity);

public sealed class Store(string path)
{
    private SqliteConnection Open()
    {
        var db = new SqliteConnection(new SqliteConnectionStringBuilder
        {
            DataSource = path, ForeignKeys = true, DefaultTimeout = 10
        }.ToString());
        db.Open();
        return db;
    }

    private static SqliteCommand Command(SqliteConnection db, string sql, params (string, object)[] parameters)
    {
        var command = db.CreateCommand();
        command.CommandText = sql;
        foreach (var (name, value) in parameters)
            command.Parameters.AddWithValue(name, value);
        return command;
    }

    public void Initialize()
    {
        Directory.CreateDirectory(Path.GetDirectoryName(Path.GetFullPath(path))!);
        using var db = Open();
        using var command = Command(db, """
            PRAGMA journal_mode = WAL;
            CREATE TABLE IF NOT EXISTS Events (
                Id INTEGER PRIMARY KEY, Name TEXT NOT NULL, Venue TEXT NOT NULL,
                Remaining INTEGER NOT NULL CHECK (Remaining >= 0)
            );
            CREATE TABLE IF NOT EXISTS Bookings (
                Id TEXT PRIMARY KEY, EventId INTEGER NOT NULL REFERENCES Events(Id),
                UserId TEXT NOT NULL, Quantity INTEGER NOT NULL CHECK (Quantity > 0),
                Status TEXT NOT NULL CHECK (Status IN ('Pending','Confirmed','Rejected')),
                Published INTEGER NOT NULL DEFAULT 0
            );
            CREATE INDEX IF NOT EXISTS IX_Bookings_User ON Bookings(UserId);
            INSERT OR IGNORE INTO Events VALUES (1, 'Đêm nhạc Indie', 'Hà Nội', 30),
                (2, 'Workshop Nhiếp ảnh', 'Đà Nẵng', 12),
                (3, 'Chung kết Robotics', 'TP. Hồ Chí Minh', 5);
            """);
        command.ExecuteNonQuery();
    }

    public List<TicketEvent> Events(int? id = null)
    {
        using var db = Open();
        using var command = Command(db, """
            SELECT Id, Name, Venue, Remaining FROM Events WHERE $id IS NULL OR Id = $id ORDER BY Id
            """, ("$id", (object?)id ?? DBNull.Value));
        using var reader = command.ExecuteReader();
        var events = new List<TicketEvent>();
        while (reader.Read())
            events.Add(new(reader.GetInt32(0), reader.GetString(1), reader.GetString(2), reader.GetInt32(3)));
        return events;
    }

    public Booking? Create(int eventId, string userId, int quantity)
    {
        var booking = new Booking(Guid.NewGuid(), eventId, userId, quantity, "Pending");
        using var db = Open();
        using var command = Command(db, """
            INSERT INTO Bookings (Id, EventId, UserId, Quantity, Status)
            SELECT $id, Id, $user, $qty, 'Pending' FROM Events WHERE Id = $event
            """, ("$id", booking.Id.ToString()), ("$user", userId), ("$qty", quantity), ("$event", eventId));
        return command.ExecuteNonQuery() == 1 ? booking : null;
    }

    public List<Booking> Bookings(Guid? id = null, string? user = null)
    {
        using var db = Open();
        using var command = Command(db, """
            SELECT Id, EventId, UserId, Quantity, Status FROM Bookings
            WHERE ($id IS NULL OR Id = $id) AND ($user IS NULL OR UserId = $user)
            ORDER BY rowid DESC
            """, ("$id", (object?)id?.ToString() ?? DBNull.Value), ("$user", (object?)user ?? DBNull.Value));
        using var reader = command.ExecuteReader();
        var bookings = new List<Booking>();
        while (reader.Read())
            bookings.Add(new(Guid.Parse(reader.GetString(0)), reader.GetInt32(1), reader.GetString(2),
                reader.GetInt32(3), reader.GetString(4)));
        return bookings;
    }
}
