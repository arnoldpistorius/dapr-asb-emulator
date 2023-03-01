using DaprAsbEmulator.Adapter.Grpc;
using DaprAsbEmulator.Adapter.Memory;
using DaprAsbEmulator.Application;
using DaprAsbEmulator.Ports;

var builder = WebApplication.CreateBuilder(args);

var services = builder.Services;

services.AddSwaggerDocument()
    .Configure<TopicRepositorySettings>(builder.Configuration.GetSection("TopicRepositorySettings"))
    .AddTransient<ITopicService, TopicService>()
    .AddSingleton<InMemoryTopicRepository>()
    .AddSingleton<ITopicRepository>(svc => svc.GetRequiredService<InMemoryTopicRepository>())
    .AddSingleton<ISubscriptionRepository>(svc => svc.GetRequiredService<InMemoryTopicRepository>())
    .AddSingleton<ITopicSubscriptionEvents>(svc => svc.GetRequiredService<InMemoryTopicRepository>())
    .AddTransient<IValidatorService, ValidatorService>()
    .AddControllers()
    .Services
    .AddGrpc();

var app = builder.Build();

app.MapGrpcService<TopicsController>();
app.MapControllers();
app.UseOpenApi();
app.UseSwaggerUi3();

await app.RunAsync();

// Required for tests
public partial class Program { }