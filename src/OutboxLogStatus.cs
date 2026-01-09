namespace IATec.Shared.EF.Repository;

public enum OutboxLogStatus
{
    Created = 0,
    Sent = 1,
    Failed = 2
}