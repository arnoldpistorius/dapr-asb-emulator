namespace DaprAsbEmulator.Model;

public class TopicSubscription
{
    public string TopicName { get; }
    public string SubscriptionName { get; }

    public TopicSubscription(string topicName, string subscriptionName)
    {
        TopicName = topicName;
        SubscriptionName = subscriptionName;
    }
    
    // ToString() is used in the logs
    public override string ToString() => $"TopicSubscription {{ TopicName: {TopicName}, SubscriptionName: {SubscriptionName} }}";
}