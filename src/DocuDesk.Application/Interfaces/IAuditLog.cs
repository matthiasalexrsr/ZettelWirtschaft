namespace DocuDesk.Application.Interfaces;

public interface IAuditLog
{
    Task WriteAsync(string eventType, string entityType, string entityId, string? payloadJson = null, CancellationToken cancellationToken = default);
}
