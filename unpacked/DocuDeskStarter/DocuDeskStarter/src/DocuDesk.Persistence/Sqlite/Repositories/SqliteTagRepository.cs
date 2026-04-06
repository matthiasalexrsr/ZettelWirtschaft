using Dapper;
using DocuDesk.Application.Abstractions.Persistence;
using DocuDesk.Domain.Entities;
using DocuDesk.Persistence.Sqlite.Mapping;

namespace DocuDesk.Persistence.Sqlite.Repositories;

public sealed class SqliteTagRepository : ITagRepository
{
    private readonly ISqliteConnectionFactory _connectionFactory;

    public SqliteTagRepository(ISqliteConnectionFactory connectionFactory)
    {
        _connectionFactory = connectionFactory;
    }

    public async Task<IReadOnlyList<Tag>> ListAsync(CancellationToken ct)
    {
        using var con = await _connectionFactory.OpenConnectionAsync(ct);
        var rows = await con.QueryAsync<TagRow>(new CommandDefinition("""
            SELECT id, name, color, sort_order AS SortOrder, created_utc AS CreatedUtc
            FROM tags
            ORDER BY sort_order, name;
            """, cancellationToken: ct));
        return rows.Select(x => x.ToDomain()).ToList();
    }

    public async Task<IReadOnlyList<Tag>> GetByDocumentAsync(string documentId, CancellationToken ct)
    {
        using var con = await _connectionFactory.OpenConnectionAsync(ct);
        var rows = await con.QueryAsync<TagRow>(new CommandDefinition("""
            SELECT t.id, t.name, t.color, t.sort_order AS SortOrder, t.created_utc AS CreatedUtc
            FROM tags t
            INNER JOIN document_tags dt ON dt.tag_id = t.id
            WHERE dt.document_id = @documentId
            ORDER BY t.sort_order, t.name;
            """, new { documentId }, cancellationToken: ct));
        return rows.Select(x => x.ToDomain()).ToList();
    }

    public async Task AddToDocumentAsync(string documentId, string tagId, DateTimeOffset createdUtc, CancellationToken ct)
    {
        using var con = await _connectionFactory.OpenConnectionAsync(ct);
        await con.ExecuteAsync(new CommandDefinition("""
            INSERT OR IGNORE INTO document_tags (document_id, tag_id, created_utc)
            VALUES (@documentId, @tagId, @createdUtc);
            """, new { documentId, tagId, createdUtc = createdUtc.UtcDateTime }, cancellationToken: ct));
    }

    public async Task RemoveFromDocumentAsync(string documentId, string tagId, CancellationToken ct)
    {
        using var con = await _connectionFactory.OpenConnectionAsync(ct);
        await con.ExecuteAsync(new CommandDefinition("DELETE FROM document_tags WHERE document_id = @documentId AND tag_id = @tagId;", new { documentId, tagId }, cancellationToken: ct));
    }
}
