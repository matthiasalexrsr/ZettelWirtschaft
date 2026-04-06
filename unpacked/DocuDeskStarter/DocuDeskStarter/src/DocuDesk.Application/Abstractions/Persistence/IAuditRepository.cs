using DocuDesk.Domain.Entities;

namespace DocuDesk.Application.Abstractions.Persistence;

public interface IAuditRepository
{
    Task AppendAsync(AuditEvent auditEvent, CancellationToken ct);
}
