using DocuDesk.Domain.Enums;

namespace DocuDesk.Domain.Entities;

public sealed record BackgroundJob
{
    public required string Id { get; init; }
    public BackgroundJobType JobType { get; init; }
    public string? RelatedDocumentId { get; init; }
    public required string PayloadJson { get; init; }
    public BackgroundJobState State { get; init; }
    public int Priority { get; init; }
    public int Attempts { get; init; }
    public int MaxAttempts { get; init; }
    public required DateTimeOffset ScheduledUtc { get; init; }
    public DateTimeOffset? StartedUtc { get; init; }
    public DateTimeOffset? CompletedUtc { get; init; }
    public string? ErrorText { get; init; }
    public required DateTimeOffset CreatedUtc { get; init; }
}
