namespace IATec.Shared.EF.Repository;

public class OutboxLog(DateTimeOffset occurredOnUtc, string source, string owner, string action, string content)
{
    public long Id { get; private set; }

    public string Source { get; private set; } = source;

    public string Owner { get; private set; } = owner;
    
    public string Action { get; private set; } = action;
    
    public string Content { get; private set; } = content;

    public DateTimeOffset OccurredOnUtc { get; private set; } = occurredOnUtc;

    public DateTimeOffset? ProcessedOnUtc { get; private set; }

    public string? Error { get; private set; }

    public OutboxLogStatus Status { get; private set; } = OutboxLogStatus.Created;
    
    public void Process(DateTimeOffset processedOnUtc)
    {
        ProcessedOnUtc = processedOnUtc;
        Status = OutboxLogStatus.Sent;
    }
    
    public void Failure(string? error)
    {
        Error = error;
        Status = OutboxLogStatus.Failed;
    }
}