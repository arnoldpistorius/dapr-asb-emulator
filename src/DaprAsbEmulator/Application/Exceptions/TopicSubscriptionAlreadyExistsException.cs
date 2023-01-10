namespace DaprAsbEmulator.Application.Exceptions;

public class TopicSubscriptionAlreadyExistsException : Exception
{
    public string SubscriptionName { get; }
    public string TopicName { get; }

    public TopicSubscriptionAlreadyExistsException(string subscriptionName, string topicName) : base($"The subscription '{subscriptionName}' for topic '{topicName}' already exists.")
    {
        SubscriptionName = subscriptionName;
        TopicName = topicName;
    }
}