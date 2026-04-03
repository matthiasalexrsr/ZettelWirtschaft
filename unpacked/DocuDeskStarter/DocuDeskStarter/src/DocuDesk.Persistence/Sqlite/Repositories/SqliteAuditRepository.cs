using Dapper;
using DocuDesk.Application.Abstractions.Persistence;
using DocuDesk.Domain.Entities;

namespace DocuDesk.Persistence.Sqlite.Repositories;

public sealed class SqliteAuditRepository : IAuditRepository
{
    private readonly ISqliteConnectionFactory _connectionFactory;

    public SqliteAuditRepository(ISqliteConnectionFactory connectionFactory)
    {
        _connectionFactory = connectionFactory;
    }

    public async Task AppendAsync(AuditEvent auditEvent, CancellationToken ct)
    {
        const string sql = """
            INSERT INTO audit_events (
                id, event_type, entity_type, entity_id,
                document_id, payload_json, created_utc
            ) VALUES (
                @Id, @EventType, @EntityType, @EntityId,
                @DocumentId, @PayloadJson, @CreatedUtc
            );
            """;

        using var con = await _connectionFactory.OpenConnectionAsync(ct);
        await con.ExecuteAsync(new CommandDefinition(sql, new
        {
            auditEvent.Id,
            auditEvent.EventType,
            auditEvent.EntityType,
            auditEvent.EntityId,
            auditEvent.DocumentId,
            auditEvent.PayloadJson,
            CreatedUtc = auditEvent.CreatedUtc.UtcDateTime
        }, cancellationToken: ct));
    }
}
