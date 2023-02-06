using DaprAsbEmulator.Model;

namespace DaprAsbEmulator.Ports;

public interface ISubscriptionRepository
{
    Task<Message> PeekMessage(string topicName, string subscriptionName, CancellationToken cancellationToken);
    Task<TopicSubscription?> GetSubscription(string topicName, string subscriptionName);
    Task<bool> CreateSubscription(TopicSubscription topicSubscription);
    FailMessageResult FailMessage(string topicName, string subscriptionName, Message peekedMessage);
    void SucceedMessage(string topicName, string subscriptionName, Message peekedMessage);
}