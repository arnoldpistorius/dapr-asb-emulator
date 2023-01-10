using System.Collections.Concurrent;
using System.Collections.ObjectModel;
using DaprAsbEmulator.Application.Exceptions;
using DaprAsbEmulator.Extensions;
using DomainTopic = DaprAsbEmulator.Model.Topic;

namespace DaprAsbEmulator.Adapter.Memory.Model;

public class Topic
{
    public string Name { get; init; }

    readonly Dictionary<string, (Queue<string> MessageQueue, ReaderWriterLockSlim RwLock)> subscriptionMessages = new();
    readonly ReaderWriterLockSlim rwLock = new();

    public TopicSubscription CreateSubscription(string name)
    {
        name = name.ToLower();
        using var upgradableReadLock = rwLock.UpgradableReadLock();
        if (subscriptionMessages.ContainsKey(name))
        {
            throw new TopicSubscriptionAlreadyExistsException(name, Name);
        }

        using var writeLock = rwLock.WriteLock();
        subscriptionMessages[name] = new(new(), new());
        return new(name, Name);
    }

    public void RemoveSubscription(string name)
    {
        name = name.ToLower();
        using var upgradableReadLock = rwLock.UpgradableReadLock();
        if (!subscriptionMessages.ContainsKey(name))
        {
            throw new TopicSubscriptionNotFoundException(name, Name);
        }

        // First gain exclusive access to the subscriptionMessages before locking entire dictionary
        using var subscriptionWriteLock = subscriptionMessages[name].RwLock.WriteLock();
        using var writeLock = rwLock.WriteLock();

        subscriptionMessages.Remove(name);
    }

    public ReadOnlyCollection<string> GetSubscriptions()
    {
        using var readLock = rwLock.ReadLock();
        return subscriptionMessages.Keys.ToList().AsReadOnly();
    }

    public async Task PublishMessage(string message)
    {
        using var readLock = rwLock.ReadLock();

        Parallel.ForEach(subscriptionMessages, x =>
        {
            using var writeLock = x.Value.RwLock.WriteLock();
            x.Value.MessageQueue.Enqueue(message);
        });
    }

    public async Task<string> ReadMessage(string subscriptionName)
    {
        subscriptionName = subscriptionName.ToLower();
        using var readLock = rwLock.ReadLock();
        if (!subscriptionMessages.ContainsKey(subscriptionName))
        {
            throw new TopicSubscriptionNotFoundException(subscriptionName, Name);
        }

        var subscription = subscriptionMessages[subscriptionName];
        using var subscriptionWriteLock = subscription.RwLock.WriteLock();
        return subscription.MessageQueue.Dequeue();
    }
    
    public DomainTopic ToDomainTopic()
    {
        return new(Name);
    }

    public static Topic FromDomainTopic(DomainTopic domainTopic)
    {
        return new()
        {
            Name = domainTopic.Name
        };
    }

    public class TopicNameEqualityComparer : IEqualityComparer<Topic>
    {
        static IEqualityComparer<Topic>? instance;
        public static IEqualityComparer<Topic> Instance = instance ??= new TopicNameEqualityComparer();
        
        public bool Equals(Topic x, Topic y)
        {
            if (ReferenceEquals(x, y)) return true;
            if (ReferenceEquals(x, null)) return false;
            if (ReferenceEquals(y, null)) return false;
            if (x.GetType() != y.GetType()) return false;
            return string.Equals(x.Name, y.Name, StringComparison.OrdinalIgnoreCase);
        }

        public int GetHashCode(Topic obj)
        {
            return StringComparer.OrdinalIgnoreCase.GetHashCode(obj.Name);
        }
    }
}