using DaprAsbEmulator.Model;

namespace DaprAsbEmulator.Ports;

public interface ITopicSubscriptionEvents
{
    Task OnDeadLetterMessage(Message deadLetterMessage);
}