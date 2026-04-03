using Dapper;
using DocuDesk.Application.Abstractions.Persistence;
using DocuDesk.Domain.Entities;
using DocuDesk.Persistence.Sqlite.Mapping;

namespace DocuDesk.Persistence.Sqlite.Repositories;

public sealed class SqliteDocumentContentRepository : IDocumentContentRepository
{
    private readonly ISqliteConnectionFactory _connectionFactory;

    public SqliteDocumentContentRepository(ISqliteConnectionFactory connectionFactory)
    {
        _connectionFactory = connectionFactory;
    }

    public async Task<DocumentText?> GetTextAsync(string documentId, CancellationToken ct)
    {
        using var con = await _connectionFactory.OpenConnectionAsync(ct);
        var row = await con.QuerySingleOrDefaultAsync<DocumentTextRow>(new CommandDefinition("""
            SELECT
                id,
                document_id AS DocumentId,
                plain_text AS PlainText,
                normalized_text AS NormalizedText,
                language,
                ocr_confidence_avg AS OcrConfidenceAverage,
                last_ocr_utc AS LastOcrUtc,
                row_version AS RowVersion
            FROM document_text
            WHERE document_id = @documentId;
            """, new { documentId }, cancellationToken: ct));
        return row?.ToDomain();
    }

    public async Task ReplaceDocumentTextAsync(DocumentText text, CancellationToken ct)
    {
        using var con = await _connectionFactory.OpenConnectionAsync(ct);
        await con.ExecuteAsync(new CommandDefinition("""
            INSERT INTO document_text (
                document_id, plain_text, normalized_text, language, ocr_confidence_avg, last_ocr_utc, row_version
            ) VALUES (
                @DocumentId, @PlainText, @NormalizedText, @Language, @OcrConfidenceAverage, @LastOcrUtc, @RowVersion
            )
            ON CONFLICT(document_id) DO UPDATE SET
                plain_text = excluded.plain_text,
                normalized_text = excluded.normalized_text,
                language = excluded.language,
                ocr_confidence_avg = excluded.ocr_confidence_avg,
                last_ocr_utc = excluded.last_ocr_utc,
                row_version = document_text.row_version + 1;
            """, new
        {
            text.DocumentId,
            text.PlainText,
            text.NormalizedText,
            text.Language,
            text.OcrConfidenceAverage,
            LastOcrUtc = text.LastOcrUtc?.UtcDateTime,
            RowVersion = text.RowVersion
        }, cancellationToken: ct));
    }

    public async Task<IReadOnlyList<OcrBlock>> GetOcrBlocksByPageAsync(string pageId, CancellationToken ct)
    {
        using var con = await _connectionFactory.OpenConnectionAsync(ct);
        var rows = await con.QueryAsync<OcrBlockRow>(new CommandDefinition("""
            SELECT
                id,
                document_id AS DocumentId,
                page_id AS PageId,
                parent_block_id AS ParentBlockId,
                block_type AS BlockType,
                text,
                x_norm AS XNorm,
                y_norm AS YNorm,
                w_norm AS WNorm,
                h_norm AS HNorm,
                confidence,
                reading_order AS ReadingOrder
            FROM ocr_blocks
            WHERE page_id = @pageId
            ORDER BY reading_order, id;
            """, new { pageId }, cancellationToken: ct));
        return rows.Select(x => x.ToDomain()).ToList();
    }

    public async Task ReplaceOcrBlocksForPageAsync(string documentId, string pageId, IReadOnlyList<OcrBlock> blocks, CancellationToken ct)
    {
        using var con = await _connectionFactory.OpenConnectionAsync(ct);
        using var tx = con.BeginTransaction();

        await con.ExecuteAsync("DELETE FROM ocr_blocks WHERE page_id = @pageId;", new { pageId }, tx);

        const string insertSql = """
            INSERT INTO ocr_blocks (
                id, document_id, page_id, parent_block_id, block_type, text,
                x_norm, y_norm, w_norm, h_norm, confidence, reading_order
            ) VALUES (
                @Id, @DocumentId, @PageId, @ParentBlockId, @BlockType, @Text,
                @XNorm, @YNorm, @WNorm, @HNorm, @Confidence, @ReadingOrder
            );
            """;

        foreach (var block in blocks)
        {
            block.Bounds.EnsureValid();
            await con.ExecuteAsync(insertSql, new
            {
                block.Id,
                DocumentId = documentId,
                PageId = pageId,
                block.ParentBlockId,
                BlockType = block.BlockType.ToDb(),
                block.Text,
                XNorm = block.Bounds.X,
                YNorm = block.Bounds.Y,
                WNorm = block.Bounds.Width,
                HNorm = block.Bounds.Height,
                block.Confidence,
                block.ReadingOrder
            }, tx);
        }

        tx.Commit();
    }
}
