namespace DaprAsbEmulator.Model;

public class Message
{
    public Guid Id { get; }
    public string Value { get; }

    public Message(Guid id, string value)
    {
        Id = id;
        Value = value;
    }
    
    // ToString() is used in the logs
    public override string ToString() => $"Message {{ Id: {Id}, Value: {Value} }}";
}