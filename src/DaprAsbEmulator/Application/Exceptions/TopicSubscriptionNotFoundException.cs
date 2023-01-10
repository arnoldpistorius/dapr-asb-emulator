namespace DaprAsbEmulator.Application.Exceptions;

public class TopicSubscriptionNotFoundException : Exception
{
    public string Name { get; }
    public string TopicName { get; }

    public TopicSubscriptionNotFoundException(string name, string topicName) : base($"Subscription '{name}' not found for topic '{topicName}'")
    {
        Name = name;
        TopicName = topicName;
    }
}