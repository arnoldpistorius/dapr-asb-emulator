using DaprAsbEmulator.Model;

namespace DaprAsbEmulator.Ports;

public interface ITopicService
{
    Task<Topic> CreateTopic(string topicName);
    Task PublishMessage(string topicName, string message);
    Task<TopicSubscription> Subscribe(string topicName, string subscriptionName);
    Task<Message> Peek(string topicName, string subscriptionName, CancellationToken cancellationToken);
    Task FailMessage(string topicName, string subscriptionName, Message peekedMessage);
    Task SucceedMessage(string topicName, string subscriptionName, Message peekedMessage);
}