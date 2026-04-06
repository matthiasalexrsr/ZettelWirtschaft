namespace DocuDesk.Domain.Entities;

public sealed record AuditEvent
{
    public required string Id { get; init; }
    public required string EventType { get; init; }
    public required string EntityType { get; init; }
    public required string EntityId { get; init; }
    public string? DocumentId { get; init; }
    public required string PayloadJson { get; init; }
    public required DateTimeOffset CreatedUtc { get; init; }
}
