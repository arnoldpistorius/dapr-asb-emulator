using DomainTopic = DaprAsbEmulator.Model.Topic;
namespace DaprAsbEmulator.Adapter.Rest.Model;

public record Topic(string Name)
{
    public static Topic FromDomainTopic(DomainTopic t) => new(t.Name);
}