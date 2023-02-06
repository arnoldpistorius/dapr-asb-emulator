namespace DaprAsbEmulator.Ports.Exceptions;

public sealed class TopicAlreadyExistsException : TopicException
{
    public TopicAlreadyExistsException(string topicName) : base($"Topic '{topicName}' already exists", topicName)
    {
    }
}