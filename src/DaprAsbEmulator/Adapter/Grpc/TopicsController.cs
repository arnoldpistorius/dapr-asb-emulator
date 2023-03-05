using DaprAsbEmulator.Model;
using DaprAsbEmulator.Ports;
using DaprAsbEmulator.Ports.Exceptions;
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
        try
        {
            var topic = await topicService.CreateTopic(request.TopicName);
            return new CreateTopicResponse
            {
                TopicName = topic.Name
            };
        }
        catch (TopicAlreadyExistsException)
        {
            throw new RpcException(new Status(StatusCode.AlreadyExists, "Topic already exists"));
        }
        catch (ArgumentException argumentException)
        {
            throw new RpcException(new Status(StatusCode.InvalidArgument, argumentException.Message));
        }
        catch (Exception exception)
        {
            throw new RpcException(new Status(StatusCode.Internal, exception.Message));
        }
    }

    public override async Task<SubscribeTopicResponse> SubscribeTopic(SubscribeTopicRequest request, ServerCallContext context)
    {
        try
        {
            var subscription = await topicService.Subscribe(request.TopicName, request.SubscriptionName);
            return new SubscribeTopicResponse
            {
                TopicName = subscription.TopicName,
                SubscriptionName = subscription.SubscriptionName
            };
        }
        catch (TopicNotFoundException)
        {
            throw new RpcException(new Status(StatusCode.NotFound, "Topic not found"));
        }
        catch (TopicSubscriptionAlreadyExistsException)
        {
            throw new RpcException(new Status(StatusCode.AlreadyExists, "Topic subscription already exists"));
        }
        catch (Exception exception)
        {
            throw new RpcException(new Status(StatusCode.Internal, exception.Message));
        }
    }

    public override async Task<PublishMessageResponse> PublishMessage(PublishMessageRequest request, ServerCallContext context)
    {
        try
        {
            await topicService.PublishMessage(request.TopicName, request.Message);
            return new PublishMessageResponse();
        }
        catch (TopicNotFoundException)
        {
            throw new RpcException(new Status(StatusCode.NotFound, "Topic not found"));
        }
        catch (Exception exception)
        {
            throw new RpcException(new Status(StatusCode.Internal, exception.Message));
        }
    }

    public override async Task PeekMessage(PeekMessageRequest request, IServerStreamWriter<PeekMessageResponse> responseStream, ServerCallContext context)
    {
        try
        {
            try
            {
                int counter = 0;
                while (!context.CancellationToken.IsCancellationRequested)
                {
                    var peek = await topicService.Peek(request.TopicName, request.SubscriptionName,
                        context.CancellationToken);
                    await responseStream.WriteAsync(new PeekMessageResponse
                    {
                        MessageId = peek.Id.ToString("N"),
                        Value = peek.Value
                    }, context.CancellationToken);
                    
                    counter++;
                    if (request.MaxMessages != 0 && request.MaxMessages <= counter)
                    {
                        break;
                    }
                }
            }
            catch (OperationCanceledException)
            {
                // do nothing
            }
        }
        catch (TopicNotFoundException)
        {
            throw new RpcException(new Status(StatusCode.NotFound, "Topic not found"));
        }
        catch (TopicSubscriptionNotFoundException)
        {
            throw new RpcException(new Status(StatusCode.NotFound, "Topic subscription not found"));
        }
        catch (Exception exception)
        {
            throw new RpcException(new Status(StatusCode.Internal, exception.Message));
        }
    }

    public override async Task<SucceedMessageResponse> SucceedMessage(SucceedMessageRequest request, ServerCallContext context)
    {
        try
        {
            var message = new Message(Guid.Parse(request.MessageId), string.Empty);
            await topicService.SucceedMessage(request.TopicName, request.SubscriptionName, message);
            return new SucceedMessageResponse();
        }
        catch (TopicNotFoundException)
        {
            throw new RpcException(new Status(StatusCode.NotFound, "Topic not found"));
        }
        catch (TopicSubscriptionNotFoundException)
        {
            throw new RpcException(new Status(StatusCode.NotFound, "Topic subscription not found"));
        }
        catch (Exception exception)
        {
            throw new RpcException(new Status(StatusCode.Internal, exception.Message));
        }
    }

    public override async Task<FailMessageResponse> FailMessage(FailMessageRequest request, ServerCallContext context)
    {
        try
        {
            var message = new Message(Guid.Parse(request.MessageId), string.Empty);
            await topicService.FailMessage(request.TopicName, request.SubscriptionName, message);
            return new FailMessageResponse();
        }
        catch (TopicNotFoundException)
        {
            throw new RpcException(new Status(StatusCode.NotFound, "Topic not found"));
        }
        catch (TopicSubscriptionNotFoundException)
        {
            throw new RpcException(new Status(StatusCode.NotFound, "Topic subscription not found"));
        }
        catch (Exception exception)
        {
            throw new RpcException(new Status(StatusCode.Internal, exception.Message));
        }
    }
}