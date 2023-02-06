// using System.Reactive.Disposables;
// using Microsoft.AspNetCore.Server.Kestrel.Core;
//
// namespace DaprAsbEmulator.Adapter.DaprPubSub;
//
// public static class DaprSocketInitializer
// {
//     const string ComponentName = "asb-emulator";
//     const string SocketDir = "/tmp/dapr-components-sockets";
//     const string Socket = SocketDir + "/" + ComponentName + ".sock";
//
//     static void Initialize()
//     {
//         if (!Directory.Exists(SocketDir))
//         {
//             Directory.CreateDirectory(SocketDir);
//         }
//
//         if (File.Exists(Socket))
//         {
//             File.Delete(Socket);
//         }
//     }
//
//     public static WebApplicationBuilder ConfigureDaprSocket(this WebApplicationBuilder builder)
//     {
//         Initialize();
//
//         builder.WebHost.ConfigureKestrel(options => options.ListenUnixSocket(Socket, listenOptions => listenOptions.Protocols = HttpProtocols.Http2));
//         builder.Services.AddGrpc();
//         builder.Services.AddGrpcReflection();
//
//         return builder;
//     }
//
//     public static WebApplication MapDaprGrpc(this WebApplication webApplication)
//     {
//         webApplication.MapGrpcService<PubSubService>();
//         webApplication.MapGrpcReflectionService();
//         return webApplication;
//     }
// }