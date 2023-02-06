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
}