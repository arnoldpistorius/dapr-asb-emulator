using DaprAsbEmulator.Adapter.Memory;
using DaprAsbEmulator.Application;
using DaprAsbEmulator.Ports;

var builder = WebApplication.CreateBuilder(args);

var services = builder.Services;

services.AddTransient<ITopicService, TopicService>()
    .AddSingleton<ITopicRepository, TopicRepository>()
    .AddSwaggerDocument()
    .AddControllers();

var app = builder.Build();
app.MapControllers();
app.UseOpenApi();
app.UseSwaggerUi3();

await app.RunAsync();

// Required for tests
public partial class Program { }