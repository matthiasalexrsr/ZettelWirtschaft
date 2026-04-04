namespace DocuDesk.Domain.Entities;

public sealed class AuditEvent
{
    public Guid Id { get; set; }
    public string EventType { get; set; } = string.Empty;
    public string EntityType { get; set; } = string.Empty;
    public string EntityId { get; set; } = string.Empty;
    public DateTimeOffset CreatedAt { get; set; }
    public string? PayloadJson { get; set; }
}
