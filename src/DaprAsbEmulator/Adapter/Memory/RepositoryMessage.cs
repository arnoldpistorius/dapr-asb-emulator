namespace DaprAsbEmulator.Adapter.Memory;

public class RepositoryMessage
{
    int attemptCount = 1;

    
    public Guid Id { get; }
    public string Value { get; }
    public int AttemptCount => attemptCount;

    public RepositoryMessage(Guid id, string value)
    {
        Id = id;
        Value = value;
    }

    public void IncrementAttemptCount() =>
        Interlocked.Increment(ref attemptCount);
}