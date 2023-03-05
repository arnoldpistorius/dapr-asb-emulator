using System.Collections.Concurrent;

namespace DaprAsbEmulator.Adapter.Memory;

public class ActiveMessageQueue
{
    int MaxDeliveryAttempts { get; }
    
    public ActiveMessageQueue(int maxDeliveryAttempts)
    {
        MaxDeliveryAttempts = maxDeliveryAttempts;
    }

    ConcurrentQueue<RepositoryMessage> Messages { get; } = new();
    ConcurrentDictionary<Guid, RepositoryMessage> PeekedMessages { get; } = new();
    event EventHandler? MessageEnqueued;

    public async Task<RepositoryMessage> Peek(CancellationToken cancellationToken)
    {
        while (!cancellationToken.IsCancellationRequested)
        {
            var taskCompletionSource = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
            MessageEnqueued += OnMessagePublished;
            await using var cancellationRegistration =
                cancellationToken.Register(() => taskCompletionSource.TrySetCanceled(cancellationToken));
            try
            {
                if (Messages.TryDequeue(out var message))
                {
                    PeekedMessages[message.Id] = message;
                    return message;
                }

                await taskCompletionSource.Task;
            }
            finally
            {
                MessageEnqueued -= OnMessagePublished;
            }

            void OnMessagePublished(object? sender, EventArgs e) =>
                taskCompletionSource.TrySetResult();
        }
        cancellationToken.ThrowIfCancellationRequested();
        throw new InvalidOperationException("Unreachable code");
    }

    public void Enqueue(string message)
    {
        Messages.Enqueue(new(Guid.NewGuid(), message));
        MessageEnqueued?.Invoke(this, EventArgs.Empty);
    }

    public RepositoryFailMessageResult Fail(Guid peekedMessageId)
    {
        if (!PeekedMessages.TryRemove(peekedMessageId, out var peekedMessage))
        {
            throw new InvalidOperationException($"Message with id '{peekedMessageId}' doesn't exist");
        }

        if (peekedMessage.AttemptCount >= MaxDeliveryAttempts)
        {
            return RepositoryFailMessageResult.DeadLetter;
        }
        
        peekedMessage.IncrementAttemptCount();
        Messages.Enqueue(peekedMessage);
        MessageEnqueued?.Invoke(this, EventArgs.Empty);
        return RepositoryFailMessageResult.RetryScheduled;
    }

    public void Succeed(Guid peekedMessageId)
    {
        if (!PeekedMessages.TryRemove(peekedMessageId, out _))
        {
            throw new InvalidOperationException($"Message with id '{peekedMessageId}' doesn't exist");
        }
    }
}