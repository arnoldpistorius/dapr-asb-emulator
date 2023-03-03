using DaprAsbEmulator.Adapter.Grpc;
using DaprAsbEmulator.Adapter.Memory;
using DaprAsbEmulator.Application;
using DaprAsbEmulator.Ports;
using Microsoft.AspNetCore.Server.Kestrel.Core;

var builder = WebApplication.CreateBuilder(args);

builder.WebHost.ConfigureKestrel(options => options.ListenLocalhost(5000, o => o.Protocols = HttpProtocols.Http2));

var services = builder.Services;

services.AddSwaggerDocument()
    .Configure<TopicRepositorySettings>(builder.Configuration.GetSection("TopicRepositorySettings"))
    .AddTransient<ITopicService, TopicService>()
    .AddSingleton<InMemoryTopicRepository>()
    .AddSingleton<ITopicRepository>(svc => svc.GetRequiredService<InMemoryTopicRepository>())
    .AddSingleton<ISubscriptionRepository>(svc => svc.GetRequiredService<InMemoryTopicRepository>())
    .AddSingleton<ITopicSubscriptionEvents>(svc => svc.GetRequiredService<InMemoryTopicRepository>())
    .AddTransient<IValidatorService, ValidatorService>()
    // .AddControllers()
    // .Services
    .AddGrpc();

var app = builder.Build();

app.MapGrpcService<TopicsController>();
// app.MapControllers();
// app.UseOpenApi();
// app.UseSwaggerUi3();

await app.RunAsync();

// Required for tests
public partial class Program { }