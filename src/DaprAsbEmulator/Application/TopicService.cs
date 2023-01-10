using System.Collections.Immutable;
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

    public async Task<ImmutableArray<Topic>> GetAllTopics()
    {
        var topics = await topicRepository.GetAllTopics();
        return topics.ToImmutableArray();
    }

    public async Task<Topic> GetTopic(string topicName)
    {
        var topic = await topicRepository.GetTopic(topicName);
        return topic;
    }

    public Task PublishMessage(string topicName, string message)
    {
        return topicRepository.GetTopic(topicName);
    }

    public Task<TopicSubscription> SubscribeTopic(string topicName, string subscriptionName) => 
        topicRepository.AddSubscription(new TopicSubscription(subscriptionName, topicName));
}

public interface ITopicRepository
{
    Task<Topic> GetTopic(string name);
    Task AddTopic(Topic topic);
    Task RemoveTopic(Topic topic);
    Task<IReadOnlyCollection<Topic>> GetAllTopics();
    Task<TopicSubscription> AddSubscription(TopicSubscription topicSubscription);
}