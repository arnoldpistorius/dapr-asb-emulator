var builder = WebApplication.CreateBuilder(args);
var services = builder.Services;
var app = builder.Build();

app.MapPost("handler", async context =>
{
    Console.WriteLine(context.Request.ReadFromJsonAsync<Time>());
    await context.Response.WriteAsync("OK");
}).WithTopic("asb", "a-topic-name");
app.MapSubscribeHandler();

await app.RunAsync();

public record Time(string Timestamp);