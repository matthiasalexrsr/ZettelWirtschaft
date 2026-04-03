using Dapper;
using DocuDesk.Application.Abstractions.Persistence;
using DocuDesk.Application.Abstractions.Persistence.ReadModels;
using DocuDesk.Domain.Entities;
using DocuDesk.Persistence.Sqlite.Mapping;

namespace DocuDesk.Persistence.Sqlite.Repositories;

public sealed class SqliteSearchRepository : ISearchRepository
{
    private readonly ISqliteConnectionFactory _connectionFactory;

    public SqliteSearchRepository(ISqliteConnectionFactory connectionFactory)
    {
        _connectionFactory = connectionFactory;
    }

    public async Task<IReadOnlyList<SearchHit>> SearchAsync(string query, int skip, int take, CancellationToken ct)
    {
        using var con = await _connectionFactory.OpenConnectionAsync(ct);
        var rows = await con.QueryAsync<SearchHitRow>(new CommandDefinition("""
            SELECT
                d.id AS DocumentId,
                d.title,
                d.sender,
                d.recipient,
                d.document_date AS DocumentDate,
                d.import_date_utc AS ImportDateUtc,
                bm25(fts_search_documents) AS Rank,
                snippet(fts_search_documents, 5, '[', ']', ' … ', 12) AS Snippet
            FROM fts_search_documents
            JOIN search_documents sd ON sd.rowid = fts_search_documents.rowid
            JOIN documents d ON d.id = sd.document_id
            WHERE fts_search_documents MATCH @query
              AND d.is_deleted = 0
            ORDER BY Rank, d.import_date_utc DESC
            LIMIT @take OFFSET @skip;
            """, new { query, take, skip }, cancellationToken: ct));
        return rows.Select(x => x.ToReadModel()).ToList();
    }

    public async Task RebuildProjectionAsync(CancellationToken ct)
    {
        using var con = await _connectionFactory.OpenConnectionAsync(ct);
        using var tx = con.BeginTransaction();

        await con.ExecuteAsync("DELETE FROM search_documents;", transaction: tx);
        await con.ExecuteAsync("""
            INSERT INTO search_documents(document_id, title, sender, recipient, notes, plain_text, updated_utc)
            SELECT
                d.id,
                COALESCE(d.title, ''),
                COALESCE(d.sender, ''),
                COALESCE(d.recipient, ''),
                COALESCE(d.notes, ''),
                COALESCE(NULLIF(dt.normalized_text, ''), dt.plain_text, ''),
                COALESCE(dt.last_ocr_utc, d.last_modified_utc, d.import_date_utc, CURRENT_TIMESTAMP)
            FROM documents d
            LEFT JOIN document_text dt ON dt.document_id = d.id;
            """, transaction: tx);
        tx.Commit();
    }

    public async Task<IReadOnlyList<SavedSearch>> ListSavedSearchesAsync(CancellationToken ct)
    {
        using var con = await _connectionFactory.OpenConnectionAsync(ct);
        var rows = await con.QueryAsync<SavedSearch>(new CommandDefinition("""
            SELECT
                id,
                name,
                query_json AS QueryJson,
                sort_mode AS SortMode,
                is_pinned AS IsPinned,
                created_utc AS CreatedUtc,
                updated_utc AS UpdatedUtc
            FROM saved_searches
            ORDER BY is_pinned DESC, updated_utc DESC;
            """, cancellationToken: ct));
        return rows.ToList();
    }

    public async Task UpsertSavedSearchAsync(SavedSearch search, CancellationToken ct)
    {
        using var con = await _connectionFactory.OpenConnectionAsync(ct);
        await con.ExecuteAsync(new CommandDefinition("""
            INSERT INTO saved_searches (id, name, query_json, sort_mode, is_pinned, created_utc, updated_utc)
            VALUES (@Id, @Name, @QueryJson, @SortMode, @IsPinned, @CreatedUtc, @UpdatedUtc)
            ON CONFLICT(id) DO UPDATE SET
                name = excluded.name,
                query_json = excluded.query_json,
                sort_mode = excluded.sort_mode,
                is_pinned = excluded.is_pinned,
                updated_utc = excluded.updated_utc;
            """, new
        {
            search.Id,
            search.Name,
            search.QueryJson,
            search.SortMode,
            IsPinned = search.IsPinned ? 1 : 0,
            CreatedUtc = search.CreatedUtc.UtcDateTime,
            UpdatedUtc = search.UpdatedUtc.UtcDateTime
        }, cancellationToken: ct));
    }

    public async Task DeleteSavedSearchAsync(string searchId, CancellationToken ct)
    {
        using var con = await _connectionFactory.OpenConnectionAsync(ct);
        await con.ExecuteAsync(new CommandDefinition("DELETE FROM saved_searches WHERE id = @searchId;", new { searchId }, cancellationToken: ct));
    }
}
