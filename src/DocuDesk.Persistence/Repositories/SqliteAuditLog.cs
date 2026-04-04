using DocuDesk.Application.Interfaces;
using DocuDesk.Persistence.Sqlite;

namespace DocuDesk.Persistence.Repositories;

public sealed class SqliteAuditLog : IAuditLog
{
    private readonly SqliteConnectionFactory _connectionFactory;

    public SqliteAuditLog(SqliteConnectionFactory connectionFactory)
    {
        _connectionFactory = connectionFactory;
    }

    public async Task WriteAsync(string eventType, string entityType, string entityId, string? payloadJson = null, CancellationToken cancellationToken = default)
    {
        await using var connection = _connectionFactory.CreateOpenConnection();
        await using var command = connection.CreateCommand();
        command.CommandText = @"INSERT INTO audit_events (id, event_type, entity_type, entity_id, created_at, payload_json)
                                VALUES ($id, $event_type, $entity_type, $entity_id, $created_at, $payload_json);";
        command.Parameters.AddWithValue("$id", Guid.NewGuid().ToString());
        command.Parameters.AddWithValue("$event_type", eventType);
        command.Parameters.AddWithValue("$entity_type", entityType);
        command.Parameters.AddWithValue("$entity_id", entityId);
        command.Parameters.AddWithValue("$created_at", DateTimeOffset.UtcNow.UtcDateTime.ToString("O"));
        command.Parameters.AddWithValue("$payload_json", (object?)payloadJson ?? DBNull.Value);
        await command.ExecuteNonQueryAsync(cancellationToken);
    }
}
