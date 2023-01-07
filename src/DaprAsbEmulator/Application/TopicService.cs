using DaprAsbEmulator.Application.Exceptions;
using DaprAsbEmulator.Model;
using DaprAsbEmulator.Ports;

namespace DaprAsbEmulator.Application;

public class TopicService : ITopicService
{
    readonly ITopicRepository topicRepository;

    public TopicService(ITopicRepository topicRepository)
    {
        this.topicRepository = topicRepository;
    }
    
    public async Task<Topic> CreateTopic(string name)
    {
        var topic = new Topic(name);
        if (!topic.IsValidName())
        {
            throw new TopicNameValidationFailedException(name);
        }
        
        await topicRepository.AddTopic(topic);
        return topic;
    }

    public async Task RemoveTopic(string name)
    {
        var topic = await topicRepository.GetTopic(name);
        await topicRepository.RemoveTopic(topic);
    }
}

public interface ITopicRepository
{
    Task<Topic> GetTopic(string name);
    Task AddTopic(Topic topic);
    Task RemoveTopic(Topic topic);
}