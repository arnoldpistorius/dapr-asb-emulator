using DaprAsbEmulator.Application;
using DaprAsbEmulator.Application.Exceptions;
using DaprAsbEmulator.Extensions;
using DomainTopicSubscription = DaprAsbEmulator.Model.TopicSubscription;
using DomainTopic = DaprAsbEmulator.Model.Topic;
using DaprAsbEmulator.Adapter.Memory.Model;

namespace DaprAsbEmulator.Adapter.Memory;

public class TopicRepository : ITopicRepository
{
    readonly HashSet<Topic> topics = new(Topic.TopicNameEqualityComparer.Instance);
    readonly ReaderWriterLockSlim rwLock = new();

    public Task<DomainTopic> GetTopic(string name)
    {
        using var readLock = rwLock.ReadLock();
        return Task.FromResult(GetTopicInternal(name).ToDomainTopic());
    }

    Topic GetTopicInternal(string name)
    {
        return topics.FirstOrDefault(x => DomainTopic.NameEqualityComparer.Instance.Equals(x.Name, name)) ??
               throw new TopicNotFoundException(name);
    }

    public Task AddTopic(DomainTopic topic)
    {
        using var writeLock = rwLock.WriteLock();
        if (!topics.Add(Topic.FromDomainTopic(topic)))
        {
            throw new TopicAlreadyExistsException(topic);
        }

        return Task.CompletedTask;
    }

    public Task RemoveTopic(DomainTopic topic)
    {
        using var writeLock = rwLock.WriteLock();
        if (topics.RemoveWhere(x => DomainTopic.NameEqualityComparer.Instance.Equals(topic.Name, x.Name)) == 0)
        {
            throw new TopicNotFoundException(topic.Name);
        }

        return Task.CompletedTask;
    }

    public Task<IReadOnlyCollection<DomainTopic>> GetAllTopics()
    {
        using var readLock = rwLock.ReadLock();
        return Task.FromResult<IReadOnlyCollection<DomainTopic>>(topics.Select(x => x.ToDomainTopic()).ToList().AsReadOnly());
    }

    public Task<DomainTopicSubscription> AddSubscription(DomainTopicSubscription topicSubscription)
    {
        using var readLock = rwLock.ReadLock();
        var topic = GetTopicInternal(topicSubscription.TopicName);
        
        var subscription = topic.CreateSubscription(topicSubscription.Name);
        return Task.FromResult(subscription.ToDomainTopicSubscription());
    }
}