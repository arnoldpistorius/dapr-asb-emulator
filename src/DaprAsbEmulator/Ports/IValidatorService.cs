namespace DaprAsbEmulator.Ports;

public interface IValidatorService
{
    Task<bool> IsValidTopicName(string topicName);
}