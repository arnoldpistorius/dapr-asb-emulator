using System.Collections.Immutable;
using DaprAsbEmulator.Model;

namespace DaprAsbEmulator.Ports;

public interface ITopicService
{
    Task<Topic> CreateTopic(string name);
    Task RemoveTopic(string name);
    Task<ImmutableArray<Topic>> GetAllTopics();
}