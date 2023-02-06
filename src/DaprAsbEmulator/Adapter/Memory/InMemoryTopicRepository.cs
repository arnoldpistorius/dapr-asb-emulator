using System.Collections.Concurrent;
using DaprAsbEmulator.Model;
using DaprAsbEmulator.Ports;
using Microsoft.Extensions.Options;

namespace DaprAsbEmulator.Adapter.Memory;

public sealed class InMemoryTopicRepository : ITopicRepository, ISubscriptionRepository
{
    TopicRepositorySettings Settings { get; }
    
    public InMemoryTopicRepository(IOptions<TopicRepositorySettings> settings)
    {
        Settings = settings.Value;
    }

    ConcurrentDictionary<string, RepositoryTopic> Topics { get; } = new(TopicNameEqualityComparer.Instance);
    
    public Task<bool> CreateTopic(Topic topic) => 
        Task.FromResult(Topics.TryAdd(topic.Name, new RepositoryTopic(topic.Name)));

    public Task<Topic?> GetTopic(string topicName) => 
        Task.FromResult(Topics.TryGetValue(topicName, out var topic) ? new Topic(topic.Name) : null);

    public Task PublishMessage(string topicName, string message)
    {
        if (!Topics.TryGetValue(topicName, out var topic))
        {
            throw new InvalidOperationException($"Topic '{topicName}' not found");
        }

        return Parallel.ForEachAsync(topic.Subscriptions.Values, CancellationToken.None, (subscription, _) =>
        {
            subscription.Messages.Enqueue(message);
            return ValueTask.CompletedTask;
        });
    }

    public async Task<Message> PeekMessage(string topicName, string subscriptionName, CancellationToken cancellationToken)
    {
        if (!Topics.TryGetValue(topicName, out var topic))
        {
            throw new InvalidOperationException($"Topic '{topicName}' not found");
        }

        if (!topic.Subscriptions.TryGetValue(subscriptionName, out var subscription))
        {
            throw new InvalidOperationException($"Subscription '{subscriptionName}' not found");
        }

        var repositoryMessage = await subscription.Messages.Peek(cancellationToken);
        return new Message(repositoryMessage.Id, repositoryMessage.Value);
    }

    public Task<TopicSubscription?> GetSubscription(string topicName, string subscriptionName) =>
        Task.FromResult(Topics.TryGetValue(topicName, out var topic)
            ? topic.Subscriptions.TryGetValue(subscriptionName, out var subscription)
                ? new TopicSubscription(subscription.Topic.Name, subscription.Name)
                : null
            : null);

    public Task<bool> CreateSubscription(TopicSubscription topicSubscription)
    {
        return Task.FromResult(Topics.TryGetValue(topicSubscription.TopicName, out var topic) &&
                               topic.Subscriptions.TryAdd(topicSubscription.SubscriptionName,
                                   new(topicSubscription.SubscriptionName, topic, Settings.MaxDeliveryAttempts)));
    }

    public FailMessageResult FailMessage(string topicName, string subscriptionName, Message peekedMessage)
    {
        if (!Topics.TryGetValue(topicName, out var topic))
        {
            throw new InvalidOperationException($"Topic '{topicName}' not found");
        }

        if (!topic.Subscriptions.TryGetValue(subscriptionName, out var subscription))
        {
            throw new InvalidOperationException($"Subscription '{subscriptionName}' not found");
        }

        return subscription.Messages.Fail(peekedMessage.Id) switch
        {
            RepositoryFailMessageResult.RetryScheduled => FailMessageResult.RetryScheduled,
            RepositoryFailMessageResult.DeadLetter => FailMessageResult.DeadLetter,
            _ => throw new InvalidOperationException("Invalid response received from 'Fail'")
        };
    }

    public void SucceedMessage(string topicName, string subscriptionName, Message peekedMessage)
    {
        if (!Topics.TryGetValue(topicName, out var topic))
        {
            throw new InvalidOperationException($"Topic '{topicName}' not found");
        }

        if (!topic.Subscriptions.TryGetValue(subscriptionName, out var subscription))
        {
            throw new InvalidOperationException($"Subscription '{subscriptionName}' not found");
        }

        subscription.Messages.Succeed(peekedMessage.Id);
    }
}