using DocuDesk.Domain.Enums;

namespace DocuDesk.Domain.Entities;

public sealed record MailJob
{
    public required string Id { get; init; }
    public required string TargetClient { get; init; }
    public MailDirection Direction { get; init; }
    public required string RequestJson { get; init; }
    public MailJobStatus Status { get; init; }
    public string? ExternalReference { get; init; }
    public string? ErrorText { get; init; }
    public required DateTimeOffset CreatedUtc { get; init; }
    public DateTimeOffset? CompletedUtc { get; init; }
}
