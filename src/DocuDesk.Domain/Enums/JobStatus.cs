namespace DocuDesk.Domain.Enums;

public enum JobStatus
{
    Pending = 0,
    Running = 1,
    Succeeded = 2,
    Failed = 3,
    RetryScheduled = 4,
    Cancelled = 5
}
