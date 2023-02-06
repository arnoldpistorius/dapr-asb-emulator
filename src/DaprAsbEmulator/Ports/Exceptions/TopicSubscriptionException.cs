namespace DaprAsbEmulator.Ports.Exceptions;

public abstract class TopicSubscriptionException : TopicException
{
    public string SubscriptionName { get; }

    protected TopicSubscriptionException(string message, string topicName, string subscriptionName) : base(message, topicName)
    {
        SubscriptionName = subscriptionName;
    }
}