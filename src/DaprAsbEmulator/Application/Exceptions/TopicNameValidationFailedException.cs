namespace DaprAsbEmulator.Application.Exceptions;

public class TopicNameValidationFailedException : Exception
{
    public string TopicName { get; }

    public TopicNameValidationFailedException(string topicName) : base($"Topic name '{topicName}' is invalid.")
    {
        TopicName = topicName;
    }
}