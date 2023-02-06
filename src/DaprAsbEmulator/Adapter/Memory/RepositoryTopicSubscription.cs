namespace DaprAsbEmulator.Adapter.Memory;

public class RepositoryTopicSubscription
{
    public string Name { get; }
    public RepositoryTopic Topic { get; }
    public ActiveMessageQueue Messages { get; }

    public RepositoryTopicSubscription(string name, RepositoryTopic topic, int maxDeliveryAttempts)
    {
        Name = name;
        Topic = topic;
        Messages = new(maxDeliveryAttempts);
    }
}