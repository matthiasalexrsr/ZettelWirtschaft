using Dapper;
using DocuDesk.Application.Abstractions.Persistence;
using DocuDesk.Application.Abstractions.Persistence.ReadModels;
using DocuDesk.Domain.Entities;
using DocuDesk.Domain.Enums;
using DocuDesk.Persistence.Sqlite.Mapping;

namespace DocuDesk.Persistence.Sqlite.Repositories;

public sealed class SqliteDocumentRepository : IDocumentRepository
{
    private readonly ISqliteConnectionFactory _connectionFactory;

    public SqliteDocumentRepository(ISqliteConnectionFactory connectionFactory)
    {
        _connectionFactory = connectionFactory;
    }

    public async Task<bool> ExistsBySha256Async(string sha256, CancellationToken ct)
    {
        const string sql = """
            SELECT EXISTS(
                SELECT 1
                FROM documents
                WHERE sha256 = @sha256
                  AND is_deleted = 0
            );
            """;

        using var con = await _connectionFactory.OpenConnectionAsync(ct);
        var result = await con.ExecuteScalarAsync<long>(new CommandDefinition(sql, new { sha256 }, cancellationToken: ct));
        return result == 1;
    }

    public async Task<Document?> GetAsync(string documentId, CancellationToken ct)
    {
        const string sql = """
            SELECT
                id,
                title,
                original_file_name AS OriginalFileName,
                mime_type AS MimeType,
                sha256,
                page_count AS PageCount,
                import_date_utc AS ImportDateUtc,
                document_date AS DocumentDate,
                status,
                sender,
                recipient,
                notes,
                category_id AS CategoryId,
                has_ocr AS HasOcr,
                is_deleted AS IsDeleted,
                last_modified_utc AS LastModifiedUtc
            FROM documents
            WHERE id = @documentId;
            """;

        using var con = await _connectionFactory.OpenConnectionAsync(ct);
        var row = await con.QuerySingleOrDefaultAsync<DocumentRow>(new CommandDefinition(sql, new { documentId }, cancellationToken: ct));
        return row?.ToDomain();
    }

    public async Task<DocumentDetails?> GetDetailsAsync(string documentId, CancellationToken ct)
    {
        using var con = await _connectionFactory.OpenConnectionAsync(ct);

        var docRow = await con.QuerySingleOrDefaultAsync<DocumentRow>(new CommandDefinition("""
            SELECT
                id,
                title,
                original_file_name AS OriginalFileName,
                mime_type AS MimeType,
                sha256,
                page_count AS PageCount,
                import_date_utc AS ImportDateUtc,
                document_date AS DocumentDate,
                status,
                sender,
                recipient,
                notes,
                category_id AS CategoryId,
                has_ocr AS HasOcr,
                is_deleted AS IsDeleted,
                last_modified_utc AS LastModifiedUtc
            FROM documents
            WHERE id = @documentId;
            """, new { documentId }, cancellationToken: ct));

        if (docRow is null)
        {
            return null;
        }

        var fileRow = await con.QuerySingleAsync<DocumentFileRow>(new CommandDefinition("""
            SELECT
                id,
                document_id AS DocumentId,
                original_path AS OriginalPath,
                searchable_pdf_path AS SearchablePdfPath,
                thumbnail_folder AS ThumbnailFolder,
                ocr_artifact_path AS OcrArtifactPath,
                file_size_bytes AS FileSizeBytes,
                storage_state AS StorageState,
                created_utc AS CreatedUtc
            FROM document_files
            WHERE document_id = @documentId;
            """, new { documentId }, cancellationToken: ct));

        var pages = (await con.QueryAsync<DocumentPageRow>(new CommandDefinition("""
            SELECT
                id,
                document_id AS DocumentId,
                page_number AS PageNumber,
                width_px AS WidthPx,
                height_px AS HeightPx,
                rotation_deg AS RotationDeg,
                preview_image_path AS PreviewImagePath
            FROM document_pages
            WHERE document_id = @documentId
            ORDER BY page_number;
            """, new { documentId }, cancellationToken: ct))).Select(x => x.ToDomain()).ToList();

        var textRow = await con.QuerySingleOrDefaultAsync<DocumentTextRow>(new CommandDefinition("""
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

        var tags = (await con.QueryAsync<TagRow>(new CommandDefinition("""
            SELECT
                t.id,
                t.name,
                t.color,
                t.sort_order AS SortOrder,
                t.created_utc AS CreatedUtc
            FROM tags t
            INNER JOIN document_tags dt ON dt.tag_id = t.id
            WHERE dt.document_id = @documentId
            ORDER BY t.sort_order, t.name;
            """, new { documentId }, cancellationToken: ct))).Select(x => x.ToDomain()).ToList();

        var categoryRow = await con.QuerySingleOrDefaultAsync<CategoryRow>(new CommandDefinition("""
            SELECT
                c.id,
                c.name,
                c.parent_category_id AS ParentCategoryId,
                c.sort_order AS SortOrder,
                c.is_system AS IsSystem,
                c.created_utc AS CreatedUtc
            FROM categories c
            INNER JOIN documents d ON d.category_id = c.id
            WHERE d.id = @documentId;
            """, new { documentId }, cancellationToken: ct));

        return new DocumentDetails
        {
            Document = docRow.ToDomain(),
            File = fileRow.ToDomain(),
            Pages = pages,
            Text = textRow?.ToDomain(),
            Tags = tags,
            Category = categoryRow?.ToDomain()
        };
    }

    public async Task<IReadOnlyList<DocumentListItem>> ListAsync(DocumentListFilter filter, CancellationToken ct)
    {
        var orderBy = filter.SortBy switch
        {
            "import_date_asc" => "d.import_date_utc ASC",
            "document_date_desc" => "d.document_date DESC, d.import_date_utc DESC",
            "document_date_asc" => "d.document_date ASC, d.import_date_utc DESC",
            "title_asc" => "COALESCE(d.title, d.original_file_name) COLLATE NOCASE ASC, d.import_date_utc DESC",
            "title_desc" => "COALESCE(d.title, d.original_file_name) COLLATE NOCASE DESC, d.import_date_utc DESC",
            _ => "d.import_date_utc DESC"
        };

        var sql = $"""
            SELECT
                d.id,
                d.title,
                d.original_file_name AS OriginalFileName,
                d.status,
                d.document_date AS DocumentDate,
                d.import_date_utc AS ImportDateUtc,
                c.name AS CategoryName,
                d.sender,
                d.recipient,
                d.page_count AS PageCount,
                d.has_ocr AS HasOcr
            FROM documents d
            LEFT JOIN categories c ON c.id = d.category_id
            WHERE (@excludeDeleted = 0 OR d.is_deleted = 0)
              AND (@categoryId IS NULL OR d.category_id = @categoryId)
              AND (@status IS NULL OR d.status = @status)
              AND (
                    @queryText IS NULL
                 OR d.title LIKE '%' || @queryText || '%'
                 OR d.original_file_name LIKE '%' || @queryText || '%'
                 OR d.sender LIKE '%' || @queryText || '%'
                 OR d.recipient LIKE '%' || @queryText || '%'
              )
              AND (
                    @tagId IS NULL
                 OR EXISTS (
                        SELECT 1
                        FROM document_tags dt
                        WHERE dt.document_id = d.id AND dt.tag_id = @tagId
                    )
              )
            ORDER BY {orderBy}
            LIMIT @take OFFSET @skip;
            """;

        using var con = await _connectionFactory.OpenConnectionAsync(ct);
        var rows = await con.QueryAsync<DocumentListItemRow>(new CommandDefinition(sql, new
        {
            excludeDeleted = filter.ExcludeDeleted ? 1 : 0,
            categoryId = filter.CategoryId,
            status = filter.Status?.ToDb(),
            queryText = string.IsNullOrWhiteSpace(filter.QueryText) ? null : filter.QueryText.Trim(),
            tagId = filter.TagId,
            take = filter.Take,
            skip = filter.Skip
        }, cancellationToken: ct));

        return rows.Select(x => x.ToReadModel()).ToList();
    }

    public async Task InsertAsync(Document document, DocumentFile file, IReadOnlyList<DocumentPage> pages, CancellationToken ct)
    {
        using var con = await _connectionFactory.OpenConnectionAsync(ct);
        using var tx = con.BeginTransaction();

        await con.ExecuteAsync("""
            INSERT INTO documents (
                id, title, original_file_name, mime_type, sha256, page_count,
                import_date_utc, document_date, status, sender, recipient, notes,
                category_id, has_ocr, is_deleted, last_modified_utc
            ) VALUES (
                @Id, @Title, @OriginalFileName, @MimeType, @Sha256, @PageCount,
                @ImportDateUtc, @DocumentDate, @Status, @Sender, @Recipient, @Notes,
                @CategoryId, @HasOcr, @IsDeleted, @LastModifiedUtc
            );
            """, new
        {
            document.Id,
            document.Title,
            document.OriginalFileName,
            document.MimeType,
            document.Sha256,
            document.PageCount,
            ImportDateUtc = document.ImportDateUtc.UtcDateTime,
            DocumentDate = document.DocumentDate?.ToString("yyyy-MM-dd"),
            Status = document.Status.ToDb(),
            document.Sender,
            document.Recipient,
            document.Notes,
            document.CategoryId,
            HasOcr = document.HasOcr ? 1 : 0,
            IsDeleted = document.IsDeleted ? 1 : 0,
            LastModifiedUtc = document.LastModifiedUtc.UtcDateTime
        }, tx);

        await con.ExecuteAsync("""
            INSERT INTO document_files (
                document_id, original_path, searchable_pdf_path, thumbnail_folder,
                ocr_artifact_path, file_size_bytes, storage_state, created_utc
            ) VALUES (
                @DocumentId, @OriginalPath, @SearchablePdfPath, @ThumbnailFolder,
                @OcrArtifactPath, @FileSizeBytes, @StorageState, @CreatedUtc
            );
            """, new
        {
            file.DocumentId,
            file.OriginalPath,
            file.SearchablePdfPath,
            file.ThumbnailFolder,
            file.OcrArtifactPath,
            file.FileSizeBytes,
            StorageState = file.StorageState.ToDb(),
            CreatedUtc = file.CreatedUtc.UtcDateTime
        }, tx);

        foreach (var page in pages)
        {
            await con.ExecuteAsync("""
                INSERT INTO document_pages (
                    id, document_id, page_number, width_px, height_px, rotation_deg, preview_image_path
                ) VALUES (
                    @Id, @DocumentId, @PageNumber, @WidthPx, @HeightPx, @RotationDeg, @PreviewImagePath
                );
                """, new
            {
                page.Id,
                page.DocumentId,
                page.PageNumber,
                page.WidthPx,
                page.HeightPx,
                page.RotationDeg,
                page.PreviewImagePath
            }, tx);
        }

        tx.Commit();
    }

    public async Task UpdateMetadataAsync(Document document, CancellationToken ct)
    {
        const string sql = """
            UPDATE documents
               SET title = @Title,
                   document_date = @DocumentDate,
                   sender = @Sender,
                   recipient = @Recipient,
                   notes = @Notes,
                   category_id = @CategoryId,
                   last_modified_utc = @LastModifiedUtc
             WHERE id = @Id;
            """;

        using var con = await _connectionFactory.OpenConnectionAsync(ct);
        await con.ExecuteAsync(new CommandDefinition(sql, new
        {
            document.Id,
            document.Title,
            DocumentDate = document.DocumentDate?.ToString("yyyy-MM-dd"),
            document.Sender,
            document.Recipient,
            document.Notes,
            document.CategoryId,
            LastModifiedUtc = document.LastModifiedUtc.UtcDateTime
        }, cancellationToken: ct));
    }

    public async Task SetStatusAsync(string documentId, DocumentStatus status, DateTimeOffset modifiedUtc, CancellationToken ct)
    {
        using var con = await _connectionFactory.OpenConnectionAsync(ct);
        await con.ExecuteAsync(new CommandDefinition("""
            UPDATE documents
               SET status = @status,
                   last_modified_utc = @modifiedUtc
             WHERE id = @documentId;
            """, new { documentId, status = status.ToDb(), modifiedUtc = modifiedUtc.UtcDateTime }, cancellationToken: ct));
    }

    public async Task SetOcrCompletedAsync(string documentId, int pageCount, DateTimeOffset modifiedUtc, CancellationToken ct)
    {
        using var con = await _connectionFactory.OpenConnectionAsync(ct);
        await con.ExecuteAsync(new CommandDefinition("""
            UPDATE documents
               SET has_ocr = 1,
                   page_count = @pageCount,
                   status = 'ready',
                   last_modified_utc = @modifiedUtc
             WHERE id = @documentId;
            """, new { documentId, pageCount, modifiedUtc = modifiedUtc.UtcDateTime }, cancellationToken: ct));
    }

    public async Task SoftDeleteAsync(string documentId, DateTimeOffset modifiedUtc, CancellationToken ct)
    {
        using var con = await _connectionFactory.OpenConnectionAsync(ct);
        await con.ExecuteAsync(new CommandDefinition("""
            UPDATE documents
               SET is_deleted = 1,
                   last_modified_utc = @modifiedUtc
             WHERE id = @documentId;
            """, new { documentId, modifiedUtc = modifiedUtc.UtcDateTime }, cancellationToken: ct));
    }
}
