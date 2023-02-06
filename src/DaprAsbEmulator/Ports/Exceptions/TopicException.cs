namespace DaprAsbEmulator.Ports.Exceptions;

public abstract class TopicException : Exception
{
    public string TopicName { get; }

    protected TopicException(string message, string topicName) : base(message)
    {
        TopicName = topicName;
    }
}