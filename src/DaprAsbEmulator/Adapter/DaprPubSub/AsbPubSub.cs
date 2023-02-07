using Dapr.PluggableComponents.Components;
using Dapr.PluggableComponents.Components.PubSub;

namespace DaprAsbEmulator.Adapter.DaprPubSub;

public class AsbPubSub : IPubSub
{
    public Task InitAsync(MetadataRequest request, CancellationToken cancellationToken = new CancellationToken())
    {
        throw new NotImplementedException();
    }

    public Task PublishAsync(PubSubPublishRequest request, CancellationToken cancellationToken = new CancellationToken())
    {
        throw new NotImplementedException();
    }

    public Task PullMessagesAsync(PubSubPullMessagesTopic topic, MessageDeliveryHandler<string?, PubSubPullMessagesResponse> deliveryHandler,
        CancellationToken cancellationToken = new CancellationToken())
    {
        throw new NotImplementedException();
    }
}