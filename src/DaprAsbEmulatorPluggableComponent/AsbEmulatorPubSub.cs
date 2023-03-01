using Dapr.PluggableComponents.Components;
using Dapr.PluggableComponents.Components.PubSub;

namespace DaprAsbEmulatorPluggableComponent;

public class AsbEmulatorPubSub : IPubSub
{
    string consumerId;

    public Task InitAsync(MetadataRequest request, CancellationToken cancellationToken = new CancellationToken())
    {
        _ = request.Properties.TryGetValue("consumerID", out consumerId);
        return Task.CompletedTask;
    }

    public Task PublishAsync(PubSubPublishRequest request, CancellationToken cancellationToken = new CancellationToken())
    {
        
    }

    public Task PullMessagesAsync(PubSubPullMessagesTopic topic, MessageDeliveryHandler<string?, PubSubPullMessagesResponse> deliveryHandler,
        CancellationToken cancellationToken = new CancellationToken())
    {
        throw new NotImplementedException();
    }
}