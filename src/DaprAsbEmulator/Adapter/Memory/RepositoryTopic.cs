using System.Collections.Concurrent;

namespace DaprAsbEmulator.Adapter.Memory;

public class RepositoryTopic
{
    public string Name { get; set; }

    public ConcurrentDictionary<string, RepositoryTopicSubscription> Subscriptions { get; } =
        new(StringComparer.OrdinalIgnoreCase);

    public RepositoryTopic(string name)
    {
        Name = name;
    }
}