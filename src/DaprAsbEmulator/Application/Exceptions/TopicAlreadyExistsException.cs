using DaprAsbEmulator.Model;

namespace DaprAsbEmulator.Application.Exceptions;

public class TopicAlreadyExistsException : Exception
{
    public Topic Topic { get; }
    
    public TopicAlreadyExistsException(Topic topic) : base($"The topic {topic.Name} already exists")
    {
        Topic = topic;
    }
}