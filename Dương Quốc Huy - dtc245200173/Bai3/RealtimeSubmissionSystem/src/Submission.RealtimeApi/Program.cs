var builder = WebApplication.CreateBuilder(args);
var app = builder.Build();
app.MapGet("/", () => "EduSubmit");
app.Run();
