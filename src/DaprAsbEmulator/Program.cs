var builder = WebApplication.CreateBuilder(args);
// builder.ConfigureDaprSocket();
builder.WebHost.ConfigureKestrel(kestrel =>
{
    kestrel.ListenAnyIP(5555); // todo this should be configurable
});

var services = builder.Services;

services.AddSwaggerDocument()
    .AddControllers();

var app = builder.Build();
// app.MapDaprGrpc();
app.MapControllers();
app.UseOpenApi();
app.UseSwaggerUi3();

await app.RunAsync();

// Required for tests
public partial class Program { }