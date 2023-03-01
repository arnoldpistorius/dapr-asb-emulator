using Dapr.PluggableComponents;

var app = DaprPluggableComponentsApplication.Create();
var socketName = app.Configuration.GetRequiredSection("AsbEmulator").GetValue<string>("socketName", "asb-emulator.sock");

app.RegisterService(socketName, serviceBuilder =>
{
    serviceBuilder.RegisterPubSub<AsbEmulatorPubSub>();
});

await app.RunAsync();