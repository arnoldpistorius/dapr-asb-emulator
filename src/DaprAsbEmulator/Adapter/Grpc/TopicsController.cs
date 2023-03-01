using DaprAsbEmulator.Ports;
using Grpc.Core;

namespace DaprAsbEmulator.Adapter.Grpc;

public class TopicsController : Topics.TopicsBase
{
    readonly ITopicService topicService;

    public TopicsController(ITopicService topicService)
    {
        this.topicService = topicService;
    }

    public override async Task<CreateTopicResponse> CreateTopic(CreateTopicRequest request, ServerCallContext context)
    {
        var topic = await topicService.CreateTopic(request.TopicName);
        return new CreateTopicResponse
        {
            TopicName = topic.Name
        };
    }

    public override async Task<SubscribeTopicResponse> SubscribeTopic(SubscribeTopicRequest request, ServerCallContext context)
    {
        var subscription = await topicService.Subscribe(request.TopicName, request.SubscriptionName);
        return new SubscribeTopicResponse
        {
            TopicName = subscription.TopicName,
            SubscriptionName = subscription.SubscriptionName
        };
    }

    public override async Task<PublishMessageResponse> PublishMessage(PublishMessageRequest request, ServerCallContext context)
    {
        await topicService.PublishMessage(request.TopicName, request.Message);
        return new PublishMessageResponse();
    }

    public override async Task PeekMessage(PeekMessageRequest request, IServerStreamWriter<PeekMessageResponse> responseStream, ServerCallContext context)
    {
        try
        {
            while (!context.CancellationToken.IsCancellationRequested)
            {
                var peek = await topicService.Peek(request.TopicName, request.SubscriptionName,
                    context.CancellationToken);
                await responseStream.WriteAsync(new PeekMessageResponse
                {
                    Message = peek.Value
                }, context.CancellationToken);
            }
        }
        catch (OperationCanceledException)
        {
            // do nothing
        }
    }
}