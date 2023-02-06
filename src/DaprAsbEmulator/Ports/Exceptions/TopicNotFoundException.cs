namespace DaprAsbEmulator.Ports.Exceptions;

public sealed class TopicNotFoundException : TopicException
{
    public TopicNotFoundException(string topicName) : base($"Topic '{topicName}' not found", topicName)
    {
    }
}