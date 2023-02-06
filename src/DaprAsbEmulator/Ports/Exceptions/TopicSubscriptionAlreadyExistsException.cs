namespace DaprAsbEmulator.Ports.Exceptions;

public sealed class TopicSubscriptionAlreadyExistsException : TopicSubscriptionException
{
    public TopicSubscriptionAlreadyExistsException(string topicName, string subscriptionName) : base($"Subscription '{subscriptionName}' for topic '{topicName}' already exists", topicName, subscriptionName)
    {
    }
}