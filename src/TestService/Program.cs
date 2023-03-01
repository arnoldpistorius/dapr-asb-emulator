using Dapr.Client;
using Man.Dapr.Sidekick;
using Microsoft.AspNetCore.Mvc;

var builder = WebApplication.CreateBuilder(args);
var services = builder.Services;
services.AddDaprSidekick(c =>
{
    c.Sidecar = new DaprSidecarOptions
    {
        AppId = "testapp",
        ComponentsDirectory = "dapr-components"
    };
});
services.AddDaprClient();
var app = builder.Build();
app.UseCloudEvents();

app.MapPost("handler", async context =>
{
    Console.WriteLine(context.Request.ReadFromJsonAsync<Time>());
    await context.Response.WriteAsync("OK");
}).WithTopic("asb", "a-topic-name");

app.MapGet("send", async ([FromServices] DaprClient dapr, HttpContext context) =>
{
    await dapr.PublishEventAsync("asb", "a-topic-name", new Time(DateTimeOffset.Now.ToString()));
    await context.Response.WriteAsync("OK");
});
app.MapSubscribeHandler();

await app.RunAsync();

public record Time(string Timestamp);