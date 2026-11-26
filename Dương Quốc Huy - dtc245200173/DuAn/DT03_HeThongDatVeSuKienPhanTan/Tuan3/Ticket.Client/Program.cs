using System.Net.Http.Json;
using System.Text;

Console.OutputEncoding = Encoding.UTF8;

var path = args switch
{
    ["events"] => "/events",
    ["event", var id] when int.TryParse(id, out var eventId) => $"/events/{eventId}",
    ["book", var id, _, var amount] when int.TryParse(id, out _) && int.TryParse(amount, out _) => "/bookings",
    ["booking", var id] when Guid.TryParse(id, out var bookingId) => $"/bookings/{bookingId}",
    ["history", var user] => $"/users/{Uri.EscapeDataString(user)}/bookings",
    _ => null
};

if (path is null)
{
    Console.WriteLine("""
        Lệnh: events | event <mã> | book <mã sự kiện> <người đặt> <số vé>
              booking <mã đơn> | history <người đặt>
        """);
    return args.Length == 0 ? 0 : 2;
}

var address = Environment.GetEnvironmentVariable("TICKET_API_URL") ?? "http://localhost:5083";
if (!Uri.TryCreate(address, UriKind.Absolute, out var apiUrl) ||
    (apiUrl.Scheme != Uri.UriSchemeHttp && apiUrl.Scheme != Uri.UriSchemeHttps))
{
    Console.Error.WriteLine("TICKET_API_URL phải là địa chỉ HTTP hoặc HTTPS hợp lệ.");
    return 2;
}

using var client = new HttpClient { BaseAddress = apiUrl, Timeout = TimeSpan.FromSeconds(10) };
try
{
    using var response = args[0] == "book"
        ? await client.PostAsJsonAsync(path, new { EventId = int.Parse(args[1]), UserId = args[2], Quantity = int.Parse(args[3]) })
        : await client.GetAsync(path);
    var body = await response.Content.ReadAsStringAsync();
    Console.WriteLine($"HTTP {(int)response.StatusCode} {response.ReasonPhrase}");
    if (body.Length > 0)
        Console.WriteLine(body);
    return response.IsSuccessStatusCode ? 0 : 1;
}
catch (Exception error) when (error is HttpRequestException or TaskCanceledException)
{
    Console.Error.WriteLine($"Không gọi được Ticket API tại {apiUrl}: {error.Message}");
    return 1;
}
