namespace DocuDesk.Domain.Enums;

public enum BackgroundJobState
{
    Queued,
    Running,
    Completed,
    Failed,
    Cancelled
}
