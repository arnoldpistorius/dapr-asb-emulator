using DaprAsbEmulator.Model;

namespace DaprAsbEmulator.Ports;

public interface ITopicRepository
{
    Task<bool> CreateTopic(Topic topic);
    Task<Topic?> GetTopic(string topicName);
    Task PublishMessage(string topicName, string message);
}