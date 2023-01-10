using System.Collections.Immutable;
using DaprAsbEmulator.Model;

namespace DaprAsbEmulator.Ports;

public interface ITopicService
{
    Task<Topic> CreateTopic(string name);
    Task RemoveTopic(string name);
    Task<ImmutableArray<Topic>> GetAllTopics();
    Task<Topic> GetTopic(string topicName);
    Task PublishMessage(string topicName, string message);
    Task<TopicSubscription> SubscribeTopic(string topicName, string subscriptionName);
}