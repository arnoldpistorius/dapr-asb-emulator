// using System.Collections.Concurrent;
// using Dapr.Client.Autogen.Grpc.v1;
// using Dapr.Proto.Components.V1;
// using DaprAsbEmulator.Application.Exceptions;
// using DaprAsbEmulator.Ports;
// using Google.Protobuf;
// using Grpc.Core;
//
// namespace DaprAsbEmulator.Adapter.DaprPubSub;
//
// public class PubSubService : Dapr.Proto.Components.V1.PubSub.PubSubBase
// {
//     readonly ITopicService topicService;
//     readonly ConcurrentDictionary<string, string> consumers = new();
//     public PubSubService(ITopicService topicService)
//     {
//         this.topicService = topicService;
//     }
//
//     public override Task<PubSubInitResponse> Init(PubSubInitRequest request, ServerCallContext context)
//     {
//         return Task.FromResult(new PubSubInitResponse());
//     }
//
//     public override Task<FeaturesResponse> Features(FeaturesRequest request, ServerCallContext context)
//     {
//         return Task.FromResult(new FeaturesResponse());
//     }
//
//     public override async Task<PublishResponse> Publish(PublishRequest request, ServerCallContext context)
//     {
//         try
//         {
//             await topicService.CreateTopic(request.Topic);
//         }
//         catch (TopicAlreadyExistsException)
//         {
//         }
//
//         return new PublishResponse();
//     }
//
//     public override async Task PullMessages(IAsyncStreamReader<PullMessagesRequest> requestStream, IServerStreamWriter<PullMessagesResponse> responseStream, ServerCallContext context)
//     {
//         try
//         {
//             List<Task> tasks = new();
//             Topic topic;
//             if (!await requestStream.MoveNext(context.CancellationToken))
//             {
//                 // Close connection?
//                 return;
//             }
//
//             topic = requestStream.Current.Topic;
//
//             while (true)
//             {
//                 var payload = GetPayload();
//                 var id = Guid.NewGuid().ToString();
//                 await responseStream.WriteAsync(new()
//                 {
//                     ContentType = "application/json",
//                     Data = payload,
//                     Id = id,
//                     TopicName = topic.Name
//                 }, context.CancellationToken);
//
//                 if (!await requestStream.MoveNext(context.CancellationToken))
//                 {
//                     // What?
//                     return;
//                 }
//
//                 if (requestStream.Current.AckMessageId != id)
//                 {
//                     // What?
//                 }
//
//                 if (requestStream.Current.AckError?.Message != null)
//                 {
//                     // What?
//                 }
//
//                 await Task.Delay(1000, context.CancellationToken);
//             }
//         }
//         catch (Exception exception)
//         {
//
//         }
//
//         ByteString GetPayload() => ByteString.CopyFromUtf8($"{{\"timestamp\": \"{DateTimeOffset.UtcNow}\"}}");
//     }
//
//     public override Task<PingResponse> Ping(PingRequest request, ServerCallContext context)
//     {
//         return Task.FromResult(new PingResponse());
//     }
//
//
// }