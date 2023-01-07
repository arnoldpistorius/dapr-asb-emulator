using DaprAsbEmulator.Application;
using DaprAsbEmulator.Application.Exceptions;
using DaprAsbEmulator.Extensions;
using DaprAsbEmulator.Model;

namespace DaprAsbEmulator.Adapter.Memory;

public class TopicRepository : ITopicRepository
{
    readonly HashSet<Topic> topics = new();
    readonly ReaderWriterLockSlim rwLock = new();

    public Task<Topic> GetTopic(string name)
    {
        using var readLock = rwLock.ReadLock();
        return Task.FromResult(topics.FirstOrDefault(x => Topic.NameEqualityComparer.Instance.Equals(x.Name, name)) ??
                               throw new TopicNotFoundException(name));
    }

    public Task AddTopic(Topic topic)
    {
        using var writeLock = rwLock.WriteLock();
        if (!topics.Add(topic))
        {
            throw new TopicAlreadyExistsException(topic);
        }

        return Task.CompletedTask;
    }

    public Task RemoveTopic(Topic topic)
    {
        using var writeLock = rwLock.WriteLock();
        if (!topics.Remove(topic))
        {
            throw new TopicNotFoundException(topic.Name);
        }

        return Task.CompletedTask;
    }

    public Task<IReadOnlyCollection<Topic>> GetAllTopics()
    {
        using var readLock = rwLock.ReadLock();
        return Task.FromResult<IReadOnlyCollection<Topic>>(topics.ToList().AsReadOnly());
    }
}