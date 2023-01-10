using DomainTopicSubscription = DaprAsbEmulator.Model.TopicSubscription;
namespace DaprAsbEmulator.Adapter.Rest.Model;

public record TopicSubscription(string TopicName, string SubscriptionName)
{
    public static TopicSubscription FromDomainTopicSubscription(DomainTopicSubscription topicSubscription) => 
        new(topicSubscription.TopicName, topicSubscription.Name);
}