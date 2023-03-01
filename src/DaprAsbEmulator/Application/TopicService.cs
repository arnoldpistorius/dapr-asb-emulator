using System.Collections.Immutable;
using DaprAsbEmulator.Model;
using DaprAsbEmulator.Ports;
using DaprAsbEmulator.Ports.Exceptions;

namespace DaprAsbEmulator.Application;

public class TopicService : ITopicService
{
    IValidatorService ValidatorService { get; }
    ITopicRepository Topics { get; }
    ISubscriptionRepository Subscriptions { get; }
    ITopicSubscriptionEvents TopicSubscriptionEvents { get; }

    public TopicService(ITopicRepository topicRepository, ISubscriptionRepository subscriptionRepository, ITopicSubscriptionEvents topicSubscriptionEvents, IValidatorService validatorService)
    {
        ValidatorService = validatorService;
        Topics = topicRepository;
        Subscriptions = subscriptionRepository;
        TopicSubscriptionEvents = topicSubscriptionEvents;
    }

    public async Task<Topic> CreateTopic(string topicName)
    {
        if (!await ValidatorService.IsValidTopicName(topicName))
        {
            throw new ArgumentException("Topic name is invalid", nameof(topicName));
        }

        var topic = new Topic(topicName);
        if (!await Topics.CreateTopic(topic))
        {
            throw new TopicAlreadyExistsException(topicName);
        }
        return topic;
    }

    public async Task PublishMessage(string topicName, string message)
    {
        var topic = await Topics.GetTopic(topicName);
        if (topic == null)
        {
            throw new TopicNotFoundException(topicName);
        }

        await Topics.PublishMessage(topic.Name, message);
    }

    public async Task<TopicSubscription> Subscribe(string topicName, string subscriptionName)
    {
        var topic = await Topics.GetTopic(topicName);
        if (topic == null)
        {
            throw new TopicNotFoundException(topicName);
        }

        var topicSubscription = new TopicSubscription(topicName, subscriptionName);
        if (!await Subscriptions.CreateSubscription(topicSubscription))
        {
            throw new TopicSubscriptionAlreadyExistsException(topicName, subscriptionName);
        }

        return topicSubscription;
    }

    public async Task<Message> Peek(string topicName, string subscriptionName, CancellationToken cancellationToken)
    {
        var topic = await Topics.GetTopic(topicName);
        if (topic == null)
        {
            throw new TopicNotFoundException(topicName);
        }

        var subscription = await Subscriptions.GetSubscription(topicName, subscriptionName);
        if(subscription == null)
        {
            throw new TopicSubscriptionNotFoundException(topicName, subscriptionName);
        }

        return await Subscriptions.PeekMessage(topic.Name, subscription.SubscriptionName, cancellationToken);
    }

    public async Task FailMessage(string topicName, string subscriptionName, Message peekedMessage)
    {
        var topic = await Topics.GetTopic(topicName);
        if (topic == null)
        {
            throw new TopicNotFoundException(topicName);
        }

        var subscription = await Subscriptions.GetSubscription(topicName, subscriptionName);
        if(subscription == null)
        {
            throw new TopicSubscriptionNotFoundException(topicName, subscriptionName);
        }

        var result = Subscriptions.FailMessage(topic.Name, subscription.SubscriptionName, peekedMessage);
        if (result is FailMessageResult.DeadLetter)
        {
            await TopicSubscriptionEvents.OnDeadLetterMessage(peekedMessage);
        }
    }

    public async Task SucceedMessage(string topicName, string subscriptionName, Message peekedMessage)
    {
        var topic = await Topics.GetTopic(topicName);
        if (topic == null)
        {
            throw new TopicNotFoundException(topicName);
        }

        var subscription = await Subscriptions.GetSubscription(topicName, subscriptionName);
        if(subscription == null)
        {
            throw new TopicSubscriptionNotFoundException(topicName, subscriptionName);
        }

        Subscriptions.SucceedMessage(topic.Name, subscription.SubscriptionName, peekedMessage);
    }

    public async Task<ImmutableList<Topic>> GetTopics()
    {
        return await Topics.GetTopics();
    }
}