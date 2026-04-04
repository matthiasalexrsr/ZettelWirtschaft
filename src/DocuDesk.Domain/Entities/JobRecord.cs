using DocuDesk.Domain.Enums;

namespace DocuDesk.Domain.Entities;

public sealed class JobRecord
{
    public Guid Id { get; set; }
    public string JobType { get; set; } = string.Empty;
    public Guid? TargetDocumentId { get; set; }
    public string? PayloadJson { get; set; }
    public JobStatus Status { get; set; }
    public string? ErrorText { get; set; }
    public int RetryCount { get; set; }
    public DateTimeOffset CreatedAt { get; set; }
    public DateTimeOffset? StartedAt { get; set; }
    public DateTimeOffset? CompletedAt { get; set; }
    public DateTimeOffset LastUpdatedAt { get; set; }
}
