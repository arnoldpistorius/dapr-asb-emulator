namespace DaprAsbEmulator.Ports.Exceptions;

public sealed class TopicSubscriptionNotFoundException : TopicSubscriptionException
{
    public TopicSubscriptionNotFoundException(string topicName, string subscriptionName) : base($"Subscription '{subscriptionName}' for topic '{topicName}' not found", topicName, subscriptionName)
    {
    }
}