namespace DaprAsbEmulator.Application.Exceptions;

public class TopicNotFoundException : Exception
{
    string TopicName { get; }
    public TopicNotFoundException(string topicName) : base($"Topic with name '{topicName}' doesn't exist.")
    {
        TopicName = topicName;
    }
}