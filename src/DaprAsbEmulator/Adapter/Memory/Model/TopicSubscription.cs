using DomainTopicSubscription = DaprAsbEmulator.Model.TopicSubscription;

namespace DaprAsbEmulator.Adapter.Memory.Model;

public record TopicSubscription(string SubscriptionName, string TopicName)
{
    public DomainTopicSubscription ToDomainTopicSubscription()
    {
        return new(SubscriptionName, TopicName);
    }
}