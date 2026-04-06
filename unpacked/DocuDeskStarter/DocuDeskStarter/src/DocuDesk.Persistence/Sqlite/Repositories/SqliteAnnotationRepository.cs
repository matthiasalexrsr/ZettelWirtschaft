using Dapper;
using DocuDesk.Application.Abstractions.Persistence;
using DocuDesk.Domain.Entities;
using DocuDesk.Persistence.Sqlite.Mapping;

namespace DocuDesk.Persistence.Sqlite.Repositories;

public sealed class SqliteAnnotationRepository : IAnnotationRepository
{
    private readonly ISqliteConnectionFactory _connectionFactory;

    public SqliteAnnotationRepository(ISqliteConnectionFactory connectionFactory)
    {
        _connectionFactory = connectionFactory;
    }

    public async Task<IReadOnlyList<Annotation>> GetByDocumentAsync(string documentId, CancellationToken ct)
    {
        using var con = await _connectionFactory.OpenConnectionAsync(ct);
        var rows = await con.QueryAsync<AnnotationRow>(new CommandDefinition("""
            SELECT
                id,
                document_id AS DocumentId,
                page_id AS PageId,
                type,
                payload_json AS PayloadJson,
                x_norm AS XNorm,
                y_norm AS YNorm,
                w_norm AS WNorm,
                h_norm AS HNorm,
                z_index AS ZIndex,
                author_name AS AuthorName,
                created_utc AS CreatedUtc,
                updated_utc AS UpdatedUtc
            FROM annotations
            WHERE document_id = @documentId
            ORDER BY page_id, z_index, created_utc;
            """, new { documentId }, cancellationToken: ct));
        return rows.Select(x => x.ToDomain()).ToList();
    }

    public async Task<IReadOnlyList<Annotation>> GetByPageAsync(string pageId, CancellationToken ct)
    {
        using var con = await _connectionFactory.OpenConnectionAsync(ct);
        var rows = await con.QueryAsync<AnnotationRow>(new CommandDefinition("""
            SELECT
                id,
                document_id AS DocumentId,
                page_id AS PageId,
                type,
                payload_json AS PayloadJson,
                x_norm AS XNorm,
                y_norm AS YNorm,
                w_norm AS WNorm,
                h_norm AS HNorm,
                z_index AS ZIndex,
                author_name AS AuthorName,
                created_utc AS CreatedUtc,
                updated_utc AS UpdatedUtc
            FROM annotations
            WHERE page_id = @pageId
            ORDER BY z_index, created_utc;
            """, new { pageId }, cancellationToken: ct));
        return rows.Select(x => x.ToDomain()).ToList();
    }

    public async Task UpsertAsync(Annotation annotation, CancellationToken ct)
    {
        annotation.Bounds.EnsureValid();

        using var con = await _connectionFactory.OpenConnectionAsync(ct);
        await con.ExecuteAsync(new CommandDefinition("""
            INSERT INTO annotations (
                id, document_id, page_id, type, payload_json,
                x_norm, y_norm, w_norm, h_norm,
                z_index, author_name, created_utc, updated_utc
            ) VALUES (
                @Id, @DocumentId, @PageId, @Type, @PayloadJson,
                @XNorm, @YNorm, @WNorm, @HNorm,
                @ZIndex, @AuthorName, @CreatedUtc, @UpdatedUtc
            )
            ON CONFLICT(id) DO UPDATE SET
                type = excluded.type,
                payload_json = excluded.payload_json,
                x_norm = excluded.x_norm,
                y_norm = excluded.y_norm,
                w_norm = excluded.w_norm,
                h_norm = excluded.h_norm,
                z_index = excluded.z_index,
                author_name = excluded.author_name,
                updated_utc = excluded.updated_utc;
            """, new
        {
            annotation.Id,
            annotation.DocumentId,
            annotation.PageId,
            Type = annotation.Type.ToDb(),
            annotation.PayloadJson,
            XNorm = annotation.Bounds.X,
            YNorm = annotation.Bounds.Y,
            WNorm = annotation.Bounds.Width,
            HNorm = annotation.Bounds.Height,
            annotation.ZIndex,
            annotation.AuthorName,
            CreatedUtc = annotation.CreatedUtc.UtcDateTime,
            UpdatedUtc = annotation.UpdatedUtc.UtcDateTime
        }, cancellationToken: ct));
    }

    public async Task DeleteAsync(string annotationId, CancellationToken ct)
    {
        using var con = await _connectionFactory.OpenConnectionAsync(ct);
        await con.ExecuteAsync(new CommandDefinition("DELETE FROM annotations WHERE id = @annotationId;", new { annotationId }, cancellationToken: ct));
    }
}
