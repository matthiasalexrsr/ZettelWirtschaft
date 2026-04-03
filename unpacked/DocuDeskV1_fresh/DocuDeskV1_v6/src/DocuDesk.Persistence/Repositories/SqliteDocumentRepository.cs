using System.Text.Json;
using DocuDesk.Application.Interfaces;
using DocuDesk.Contracts.Documents;
using DocuDesk.Contracts.Viewer;
using DocuDesk.Domain.Entities;
using DocuDesk.Domain.Enums;
using DocuDesk.Persistence.Sqlite;
using Microsoft.Data.Sqlite;

namespace DocuDesk.Persistence.Repositories;

public sealed class SqliteDocumentRepository : IDocumentRepository, ISearchIndex
{
    private readonly SqliteConnectionFactory _connectionFactory;

    public SqliteDocumentRepository(SqliteConnectionFactory connectionFactory)
    {
        _connectionFactory = connectionFactory;
    }

    public async Task SaveAsync(Document document, CancellationToken cancellationToken = default)
    {
        await using var connection = _connectionFactory.CreateOpenConnection();
        await using var command = connection.CreateCommand();
        command.CommandText = @"
INSERT INTO documents (
    id, title, original_file_name, display_name, mime_type, extension, sha256, file_size_bytes,
    page_count, repository_path, preview_path, thumbnail_path, source_type, source_reference,
    language, document_date, sender, recipient, subject, notes, document_type, category_id,
    ocr_status, index_status, classification_status, confidence, created_at, imported_at, modified_at, is_deleted)
VALUES (
    $id, $title, $original_file_name, $display_name, $mime_type, $extension, $sha256, $file_size_bytes,
    $page_count, $repository_path, $preview_path, $thumbnail_path, $source_type, $source_reference,
    $language, $document_date, $sender, $recipient, $subject, $notes, $document_type, $category_id,
    $ocr_status, $index_status, $classification_status, $confidence, $created_at, $imported_at, $modified_at, $is_deleted)
ON CONFLICT(id) DO UPDATE SET
    title = excluded.title,
    original_file_name = excluded.original_file_name,
    display_name = excluded.display_name,
    mime_type = excluded.mime_type,
    extension = excluded.extension,
    sha256 = excluded.sha256,
    file_size_bytes = excluded.file_size_bytes,
    page_count = excluded.page_count,
    repository_path = excluded.repository_path,
    preview_path = excluded.preview_path,
    thumbnail_path = excluded.thumbnail_path,
    source_type = excluded.source_type,
    source_reference = excluded.source_reference,
    language = excluded.language,
    document_date = excluded.document_date,
    sender = excluded.sender,
    recipient = excluded.recipient,
    subject = excluded.subject,
    notes = excluded.notes,
    document_type = excluded.document_type,
    category_id = excluded.category_id,
    ocr_status = excluded.ocr_status,
    index_status = excluded.index_status,
    classification_status = excluded.classification_status,
    confidence = excluded.confidence,
    modified_at = excluded.modified_at,
    is_deleted = excluded.is_deleted;";

        command.Parameters.AddWithValue("$id", document.Id.ToString());
        command.Parameters.AddWithValue("$title", document.Title);
        command.Parameters.AddWithValue("$original_file_name", document.OriginalFileName);
        command.Parameters.AddWithValue("$display_name", document.DisplayName);
        command.Parameters.AddWithValue("$mime_type", document.MimeType);
        command.Parameters.AddWithValue("$extension", document.Extension);
        command.Parameters.AddWithValue("$sha256", document.Sha256);
        command.Parameters.AddWithValue("$file_size_bytes", document.FileSizeBytes);
        command.Parameters.AddWithValue("$page_count", (object?)document.PageCount ?? DBNull.Value);
        command.Parameters.AddWithValue("$repository_path", document.RepositoryPath);
        command.Parameters.AddWithValue("$preview_path", (object?)document.PreviewPath ?? DBNull.Value);
        command.Parameters.AddWithValue("$thumbnail_path", (object?)document.ThumbnailPath ?? DBNull.Value);
        command.Parameters.AddWithValue("$source_type", document.SourceType.ToString());
        command.Parameters.AddWithValue("$source_reference", (object?)document.SourceReference ?? DBNull.Value);
        command.Parameters.AddWithValue("$language", (object?)document.Language ?? DBNull.Value);
        command.Parameters.AddWithValue("$document_date", document.DocumentDate?.UtcDateTime.ToString("O") ?? (object)DBNull.Value);
        command.Parameters.AddWithValue("$sender", (object?)document.Sender ?? DBNull.Value);
        command.Parameters.AddWithValue("$recipient", (object?)document.Recipient ?? DBNull.Value);
        command.Parameters.AddWithValue("$subject", (object?)document.Subject ?? DBNull.Value);
        command.Parameters.AddWithValue("$notes", (object?)document.Notes ?? DBNull.Value);
        command.Parameters.AddWithValue("$document_type", (object?)document.DocumentType ?? DBNull.Value);
        command.Parameters.AddWithValue("$category_id", document.CategoryId?.ToString() ?? (object)DBNull.Value);
        command.Parameters.AddWithValue("$ocr_status", document.OcrStatus.ToString());
        command.Parameters.AddWithValue("$index_status", document.IndexStatus);
        command.Parameters.AddWithValue("$classification_status", document.ClassificationStatus);
        command.Parameters.AddWithValue("$confidence", (object?)document.Confidence ?? DBNull.Value);
        command.Parameters.AddWithValue("$created_at", document.CreatedAt.UtcDateTime.ToString("O"));
        command.Parameters.AddWithValue("$imported_at", document.ImportedAt.UtcDateTime.ToString("O"));
        command.Parameters.AddWithValue("$modified_at", document.ModifiedAt.UtcDateTime.ToString("O"));
        command.Parameters.AddWithValue("$is_deleted", document.IsDeleted ? 1 : 0);
        await command.ExecuteNonQueryAsync(cancellationToken);

        await using var fileCommand = connection.CreateCommand();
        fileCommand.CommandText = @"
INSERT INTO document_files (document_id, original_path, searchable_pdf_path, thumbnail_folder, ocr_artifact_path)
VALUES ($document_id, $original_path, $searchable_pdf_path, $thumbnail_folder, $ocr_artifact_path)
ON CONFLICT(document_id) DO UPDATE SET
    original_path = excluded.original_path,
    searchable_pdf_path = excluded.searchable_pdf_path,
    thumbnail_folder = excluded.thumbnail_folder,
    ocr_artifact_path = excluded.ocr_artifact_path;";
        fileCommand.Parameters.AddWithValue("$document_id", document.Id.ToString());
        fileCommand.Parameters.AddWithValue("$original_path", document.RepositoryPath);
        fileCommand.Parameters.AddWithValue("$searchable_pdf_path", (object?)document.PreviewPath ?? DBNull.Value);
        fileCommand.Parameters.AddWithValue("$thumbnail_folder", document.ThumbnailPath is null ? (object)DBNull.Value : Path.GetDirectoryName(document.ThumbnailPath) ?? document.ThumbnailPath);
        fileCommand.Parameters.AddWithValue("$ocr_artifact_path", document.PreviewPath is null ? (object)DBNull.Value : Path.GetDirectoryName(document.PreviewPath) ?? document.PreviewPath);
        await fileCommand.ExecuteNonQueryAsync(cancellationToken);
    }

    public async Task<Document?> GetAsync(Guid id, CancellationToken cancellationToken = default)
    {
        await using var connection = _connectionFactory.CreateOpenConnection();
        await using var command = connection.CreateCommand();
        command.CommandText = "SELECT * FROM documents WHERE id = $id AND is_deleted = 0 LIMIT 1;";
        command.Parameters.AddWithValue("$id", id.ToString());
        await using var reader = await command.ExecuteReaderAsync(cancellationToken);
        return await reader.ReadAsync(cancellationToken) ? MapDocument(reader) : null;
    }

    public async Task<IReadOnlyList<DocumentListItemDto>> SearchAsync(DocumentSearchRequest request, CancellationToken cancellationToken = default)
    {
        var results = new List<DocumentListItemDto>();
        await using var connection = _connectionFactory.CreateOpenConnection();
        await using var command = connection.CreateCommand();

        if (string.IsNullOrWhiteSpace(request.SearchText))
        {
            command.CommandText = @"SELECT id, title, display_name, sender, document_type, imported_at, ocr_status, repository_path, NULL AS match_snippet, NULL AS match_score
                                    FROM documents
                                    WHERE is_deleted = 0
                                    ORDER BY imported_at DESC
                                    LIMIT $take;";
            command.Parameters.AddWithValue("$take", request.Take);
        }
        else
        {
            command.CommandText = @"
SELECT d.id,
       d.title,
       d.display_name,
       d.sender,
       d.document_type,
       d.imported_at,
       d.ocr_status,
       d.repository_path,
       snippet(document_fts, 6, '[', ']', ' … ', 18) AS match_snippet,
       bm25(document_fts, 10.0, 4.0, 3.0, 3.0, 2.0, 1.0, 1.5) AS match_score
FROM document_fts f
JOIN documents d ON d.id = f.document_id
WHERE document_fts MATCH $query AND d.is_deleted = 0
ORDER BY match_score ASC
LIMIT $take;";
            command.Parameters.AddWithValue("$query", request.SearchText.Trim());
            command.Parameters.AddWithValue("$take", request.Take);
        }

        await using var reader = await command.ExecuteReaderAsync(cancellationToken);
        while (await reader.ReadAsync(cancellationToken))
        {
            results.Add(new DocumentListItemDto
            {
                Id = Guid.Parse(reader.GetString(0)),
                Title = reader.GetString(1),
                DisplayName = reader.GetString(2),
                Sender = reader.IsDBNull(3) ? null : reader.GetString(3),
                DocumentType = reader.IsDBNull(4) ? null : reader.GetString(4),
                ImportedAt = DateTimeOffset.Parse(reader.GetString(5)),
                OcrStatus = reader.GetString(6),
                RepositoryPath = reader.GetString(7),
                MatchSnippet = reader.IsDBNull(8) ? null : reader.GetString(8),
                MatchScore = reader.IsDBNull(9) ? null : reader.GetDouble(9)
            });
        }

        return results;
    }

    public async Task<Document?> FindBySha256Async(string sha256, CancellationToken cancellationToken = default)
    {
        await using var connection = _connectionFactory.CreateOpenConnection();
        await using var command = connection.CreateCommand();
        command.CommandText = "SELECT * FROM documents WHERE sha256 = $sha256 AND is_deleted = 0 LIMIT 1;";
        command.Parameters.AddWithValue("$sha256", sha256);
        await using var reader = await command.ExecuteReaderAsync(cancellationToken);
        return await reader.ReadAsync(cancellationToken) ? MapDocument(reader) : null;
    }

    public async Task UpsertDocumentTextAsync(DocumentText documentText, CancellationToken cancellationToken = default)
    {
        await using var connection = _connectionFactory.CreateOpenConnection();
        await using var command = connection.CreateCommand();
        command.CommandText = @"
INSERT INTO document_text (document_id, plain_text, normalized_text, language, ocr_confidence_avg, last_ocr_utc)
VALUES ($document_id, $plain_text, $normalized_text, $language, $ocr_confidence_avg, $last_ocr_utc)
ON CONFLICT(document_id) DO UPDATE SET
    plain_text = excluded.plain_text,
    normalized_text = excluded.normalized_text,
    language = excluded.language,
    ocr_confidence_avg = excluded.ocr_confidence_avg,
    last_ocr_utc = excluded.last_ocr_utc;";
        command.Parameters.AddWithValue("$document_id", documentText.DocumentId.ToString());
        command.Parameters.AddWithValue("$plain_text", documentText.PlainText);
        command.Parameters.AddWithValue("$normalized_text", (object?)documentText.NormalizedText ?? DBNull.Value);
        command.Parameters.AddWithValue("$language", (object?)documentText.Language ?? DBNull.Value);
        command.Parameters.AddWithValue("$ocr_confidence_avg", (object?)documentText.OcrConfidenceAverage ?? DBNull.Value);
        command.Parameters.AddWithValue("$last_ocr_utc", documentText.LastOcrUtc?.UtcDateTime.ToString("O") ?? (object)DBNull.Value);
        await command.ExecuteNonQueryAsync(cancellationToken);
    }

    public async Task ReplacePagesAsync(Guid documentId, IReadOnlyList<Page> pages, CancellationToken cancellationToken = default)
    {
        await using var connection = _connectionFactory.CreateOpenConnection();
        await using var transaction = await connection.BeginTransactionAsync(cancellationToken);

        await using (var deleteCommand = connection.CreateCommand())
        {
            deleteCommand.Transaction = transaction;
            deleteCommand.CommandText = "DELETE FROM pages WHERE document_id = $document_id;";
            deleteCommand.Parameters.AddWithValue("$document_id", documentId.ToString());
            await deleteCommand.ExecuteNonQueryAsync(cancellationToken);
        }

        foreach (var page in pages.OrderBy(p => p.PageNumber))
        {
            await using var insertCommand = connection.CreateCommand();
            insertCommand.Transaction = transaction;
            insertCommand.CommandText = @"
INSERT INTO pages (id, document_id, page_number, width_px, height_px, rotation_deg)
VALUES ($id, $document_id, $page_number, $width_px, $height_px, $rotation_deg);";
            insertCommand.Parameters.AddWithValue("$id", page.Id.ToString());
            insertCommand.Parameters.AddWithValue("$document_id", documentId.ToString());
            insertCommand.Parameters.AddWithValue("$page_number", page.PageNumber);
            insertCommand.Parameters.AddWithValue("$width_px", (object?)page.WidthPx ?? DBNull.Value);
            insertCommand.Parameters.AddWithValue("$height_px", (object?)page.HeightPx ?? DBNull.Value);
            insertCommand.Parameters.AddWithValue("$rotation_deg", page.RotationDeg);
            await insertCommand.ExecuteNonQueryAsync(cancellationToken);
        }

        await transaction.CommitAsync(cancellationToken);
    }

    public async Task ReplaceOcrBlocksAsync(Guid documentId, IReadOnlyList<OcrBlock> blocks, CancellationToken cancellationToken = default)
    {
        await using var connection = _connectionFactory.CreateOpenConnection();
        await using var transaction = await connection.BeginTransactionAsync(cancellationToken);

        await using (var deleteCommand = connection.CreateCommand())
        {
            deleteCommand.Transaction = transaction;
            deleteCommand.CommandText = "DELETE FROM ocr_blocks WHERE document_id = $document_id;";
            deleteCommand.Parameters.AddWithValue("$document_id", documentId.ToString());
            await deleteCommand.ExecuteNonQueryAsync(cancellationToken);
        }

        foreach (var block in blocks)
        {
            await using var insertCommand = connection.CreateCommand();
            insertCommand.Transaction = transaction;
            insertCommand.CommandText = @"
INSERT INTO ocr_blocks (id, document_id, page_id, block_type, text, x, y, width, height, confidence)
VALUES ($id, $document_id, $page_id, $block_type, $text, $x, $y, $width, $height, $confidence);";
            insertCommand.Parameters.AddWithValue("$id", block.Id.ToString());
            insertCommand.Parameters.AddWithValue("$document_id", documentId.ToString());
            insertCommand.Parameters.AddWithValue("$page_id", (object?)block.PageId?.ToString() ?? DBNull.Value);
            insertCommand.Parameters.AddWithValue("$block_type", block.BlockType);
            insertCommand.Parameters.AddWithValue("$text", block.Text);
            insertCommand.Parameters.AddWithValue("$x", block.X);
            insertCommand.Parameters.AddWithValue("$y", block.Y);
            insertCommand.Parameters.AddWithValue("$width", block.Width);
            insertCommand.Parameters.AddWithValue("$height", block.Height);
            insertCommand.Parameters.AddWithValue("$confidence", (object?)block.Confidence ?? DBNull.Value);
            await insertCommand.ExecuteNonQueryAsync(cancellationToken);
        }

        await transaction.CommitAsync(cancellationToken);
    }

    public async Task SaveAnnotationAsync(Guid documentId, ViewerAnnotationCreateRequest request, CancellationToken cancellationToken = default)
    {
        await using var connection = _connectionFactory.CreateOpenConnection();

        string? pageId = null;
        await using (var pageCommand = connection.CreateCommand())
        {
            pageCommand.CommandText = "SELECT id FROM pages WHERE document_id = $document_id AND page_number = $page_number LIMIT 1;";
            pageCommand.Parameters.AddWithValue("$document_id", documentId.ToString());
            pageCommand.Parameters.AddWithValue("$page_number", request.PageNumber);
            var result = await pageCommand.ExecuteScalarAsync(cancellationToken);
            pageId = result as string;
        }

        var payload = JsonSerializer.Serialize(new
        {
            request.PageNumber,
            request.Text,
            request.Color
        });

        await using var command = connection.CreateCommand();
        command.CommandText = @"
INSERT INTO annotations (
    id, document_id, page_id, annotation_type, payload_json, x, y, width, height, created_at, updated_at)
VALUES (
    $id, $document_id, $page_id, $annotation_type, $payload_json, $x, $y, $width, $height, $created_at, $updated_at);";
        command.Parameters.AddWithValue("$id", Guid.NewGuid().ToString());
        command.Parameters.AddWithValue("$document_id", documentId.ToString());
        command.Parameters.AddWithValue("$page_id", (object?)pageId ?? DBNull.Value);
        command.Parameters.AddWithValue("$annotation_type", request.AnnotationType);
        command.Parameters.AddWithValue("$payload_json", payload);
        command.Parameters.AddWithValue("$x", request.X);
        command.Parameters.AddWithValue("$y", request.Y);
        command.Parameters.AddWithValue("$width", request.Width);
        command.Parameters.AddWithValue("$height", request.Height);
        command.Parameters.AddWithValue("$created_at", DateTimeOffset.UtcNow.UtcDateTime.ToString("O"));
        command.Parameters.AddWithValue("$updated_at", DBNull.Value);
        await command.ExecuteNonQueryAsync(cancellationToken);
    }

    public async Task<DocumentOcrOverlayDto?> GetOcrOverlayAsync(Guid documentId, CancellationToken cancellationToken = default)
    {
        await using var connection = _connectionFactory.CreateOpenConnection();
        var overlay = new DocumentOcrOverlayDto { DocumentId = documentId };

        await using (var textCommand = connection.CreateCommand())
        {
            textCommand.CommandText = "SELECT language, ocr_confidence_avg FROM document_text WHERE document_id = $document_id LIMIT 1;";
            textCommand.Parameters.AddWithValue("$document_id", documentId.ToString());
            await using var textReader = await textCommand.ExecuteReaderAsync(cancellationToken);
            if (await textReader.ReadAsync(cancellationToken))
            {
                overlay.Language = textReader.IsDBNull(0) ? null : textReader.GetString(0);
                overlay.AverageConfidence = textReader.IsDBNull(1) ? null : textReader.GetDouble(1);
            }
        }

        await using var command = connection.CreateCommand();
        command.CommandText = @"
SELECT p.page_number, p.width_px, p.height_px, b.text, b.x, b.y, b.width, b.height, b.confidence
FROM pages p
LEFT JOIN ocr_blocks b ON b.page_id = p.id
WHERE p.document_id = $document_id
ORDER BY p.page_number ASC, b.y ASC, b.x ASC;";
        command.Parameters.AddWithValue("$document_id", documentId.ToString());

        var pageMap = new Dictionary<int, OcrOverlayPageDto>();
        var pageTextMap = new Dictionary<int, List<string>>();
        await using var reader = await command.ExecuteReaderAsync(cancellationToken);
        while (await reader.ReadAsync(cancellationToken))
        {
            var pageNumber = reader.GetInt32(0);
            if (!pageMap.TryGetValue(pageNumber, out var page))
            {
                page = new OcrOverlayPageDto
                {
                    PageNumber = pageNumber,
                    WidthPx = reader.IsDBNull(1) ? 1000 : reader.GetInt32(1),
                    HeightPx = reader.IsDBNull(2) ? 1400 : reader.GetInt32(2)
                };
                pageMap[pageNumber] = page;
                pageTextMap[pageNumber] = new List<string>();
            }

            if (!reader.IsDBNull(3))
            {
                var blockText = reader.GetString(3);
                page.Blocks.Add(new OcrOverlayBlockDto
                {
                    Text = blockText,
                    X = reader.GetDouble(4),
                    Y = reader.GetDouble(5),
                    Width = reader.GetDouble(6),
                    Height = reader.GetDouble(7),
                    Confidence = reader.IsDBNull(8) ? null : reader.GetDouble(8)
                });
                pageTextMap[pageNumber].Add(blockText);
            }
        }

        foreach (var kvp in pageMap)
        {
            kvp.Value.PageText = string.Join(" ", pageTextMap[kvp.Key]);
        }

        await using (var annotationCommand = connection.CreateCommand())
        {
            annotationCommand.CommandText = @"
SELECT a.id, a.annotation_type, a.x, a.y, a.width, a.height, a.payload_json, p.page_number
FROM annotations a
LEFT JOIN pages p ON p.id = a.page_id
WHERE a.document_id = $document_id
ORDER BY p.page_number ASC, a.created_at ASC;";
            annotationCommand.Parameters.AddWithValue("$document_id", documentId.ToString());
            await using var annotationReader = await annotationCommand.ExecuteReaderAsync(cancellationToken);
            while (await annotationReader.ReadAsync(cancellationToken))
            {
                var pageNumber = annotationReader.IsDBNull(7) ? 1 : annotationReader.GetInt32(7);
                if (!pageMap.TryGetValue(pageNumber, out var page))
                {
                    page = new OcrOverlayPageDto
                    {
                        PageNumber = pageNumber,
                        WidthPx = 1000,
                        HeightPx = 1400
                    };
                    pageMap[pageNumber] = page;
                    pageTextMap[pageNumber] = new List<string>();
                }

                string? textValue = null;
                string? colorValue = null;
                if (!annotationReader.IsDBNull(6))
                {
                    try
                    {
                        using var payloadDoc = JsonDocument.Parse(annotationReader.GetString(6));
                        var root = payloadDoc.RootElement;
                        textValue = root.TryGetProperty("Text", out var textEl) ? textEl.GetString() : null;
                        colorValue = root.TryGetProperty("Color", out var colorEl) ? colorEl.GetString() : null;
                    }
                    catch
                    {
                    }
                }

                page.Annotations.Add(new AnnotationOverlayItemDto
                {
                    Id = Guid.Parse(annotationReader.GetString(0)),
                    AnnotationType = annotationReader.GetString(1),
                    X = annotationReader.IsDBNull(2) ? 0 : annotationReader.GetDouble(2),
                    Y = annotationReader.IsDBNull(3) ? 0 : annotationReader.GetDouble(3),
                    Width = annotationReader.IsDBNull(4) ? 0 : annotationReader.GetDouble(4),
                    Height = annotationReader.IsDBNull(5) ? 0 : annotationReader.GetDouble(5),
                    Text = textValue,
                    Color = colorValue
                });
            }
        }

        overlay.Pages = pageMap.Values.OrderBy(p => p.PageNumber).ToList();
        return overlay.Pages.Count == 0 && overlay.Language is null && overlay.AverageConfidence is null ? null : overlay;
    }

    public async Task IndexDocumentAsync(Document document, string? bodyText, CancellationToken cancellationToken = default)
    {
        await using var connection = _connectionFactory.CreateOpenConnection();
        await using var deleteCommand = connection.CreateCommand();
        deleteCommand.CommandText = "DELETE FROM document_fts WHERE document_id = $document_id;";
        deleteCommand.Parameters.AddWithValue("$document_id", document.Id.ToString());
        await deleteCommand.ExecuteNonQueryAsync(cancellationToken);

        await using var command = connection.CreateCommand();
        command.CommandText = @"
INSERT INTO document_fts (document_id, title, sender, recipient, subject, notes, body)
VALUES ($document_id, $title, $sender, $recipient, $subject, $notes, $body);";
        command.Parameters.AddWithValue("$document_id", document.Id.ToString());
        command.Parameters.AddWithValue("$title", document.Title);
        command.Parameters.AddWithValue("$sender", (object?)document.Sender ?? DBNull.Value);
        command.Parameters.AddWithValue("$recipient", (object?)document.Recipient ?? DBNull.Value);
        command.Parameters.AddWithValue("$subject", (object?)document.Subject ?? DBNull.Value);
        command.Parameters.AddWithValue("$notes", (object?)document.Notes ?? DBNull.Value);
        command.Parameters.AddWithValue("$body", (object?)bodyText ?? DBNull.Value);
        await command.ExecuteNonQueryAsync(cancellationToken);

        await using var statusCommand = connection.CreateCommand();
        statusCommand.CommandText = "UPDATE documents SET index_status = 'Indexed', modified_at = $modified_at WHERE id = $id;";
        statusCommand.Parameters.AddWithValue("$modified_at", DateTimeOffset.UtcNow.UtcDateTime.ToString("O"));
        statusCommand.Parameters.AddWithValue("$id", document.Id.ToString());
        await statusCommand.ExecuteNonQueryAsync(cancellationToken);
    }

    public async Task RemoveDocumentAsync(Guid documentId, CancellationToken cancellationToken = default)
    {
        await using var connection = _connectionFactory.CreateOpenConnection();
        await using var command = connection.CreateCommand();
        command.CommandText = "DELETE FROM document_fts WHERE document_id = $document_id;";
        command.Parameters.AddWithValue("$document_id", documentId.ToString());
        await command.ExecuteNonQueryAsync(cancellationToken);
    }

    private static Document MapDocument(SqliteDataReader reader)
    {
        return new Document
        {
            Id = Guid.Parse(reader.GetString(reader.GetOrdinal("id"))),
            Title = reader.GetString(reader.GetOrdinal("title")),
            OriginalFileName = reader.GetString(reader.GetOrdinal("original_file_name")),
            DisplayName = reader.GetString(reader.GetOrdinal("display_name")),
            MimeType = reader.GetString(reader.GetOrdinal("mime_type")),
            Extension = reader.GetString(reader.GetOrdinal("extension")),
            Sha256 = reader.GetString(reader.GetOrdinal("sha256")),
            FileSizeBytes = reader.GetInt64(reader.GetOrdinal("file_size_bytes")),
            PageCount = reader.IsDBNull(reader.GetOrdinal("page_count")) ? null : reader.GetInt32(reader.GetOrdinal("page_count")),
            RepositoryPath = reader.GetString(reader.GetOrdinal("repository_path")),
            PreviewPath = reader.IsDBNull(reader.GetOrdinal("preview_path")) ? null : reader.GetString(reader.GetOrdinal("preview_path")),
            ThumbnailPath = reader.IsDBNull(reader.GetOrdinal("thumbnail_path")) ? null : reader.GetString(reader.GetOrdinal("thumbnail_path")),
            SourceType = Enum.TryParse<DocumentSourceType>(reader.GetString(reader.GetOrdinal("source_type")), out var sourceType) ? sourceType : DocumentSourceType.FileSystem,
            SourceReference = reader.IsDBNull(reader.GetOrdinal("source_reference")) ? null : reader.GetString(reader.GetOrdinal("source_reference")),
            Language = reader.IsDBNull(reader.GetOrdinal("language")) ? null : reader.GetString(reader.GetOrdinal("language")),
            DocumentDate = reader.IsDBNull(reader.GetOrdinal("document_date")) ? null : DateTimeOffset.Parse(reader.GetString(reader.GetOrdinal("document_date"))),
            Sender = reader.IsDBNull(reader.GetOrdinal("sender")) ? null : reader.GetString(reader.GetOrdinal("sender")),
            Recipient = reader.IsDBNull(reader.GetOrdinal("recipient")) ? null : reader.GetString(reader.GetOrdinal("recipient")),
            Subject = reader.IsDBNull(reader.GetOrdinal("subject")) ? null : reader.GetString(reader.GetOrdinal("subject")),
            Notes = reader.IsDBNull(reader.GetOrdinal("notes")) ? null : reader.GetString(reader.GetOrdinal("notes")),
            DocumentType = reader.IsDBNull(reader.GetOrdinal("document_type")) ? null : reader.GetString(reader.GetOrdinal("document_type")),
            CategoryId = reader.IsDBNull(reader.GetOrdinal("category_id")) ? null : Guid.Parse(reader.GetString(reader.GetOrdinal("category_id"))),
            OcrStatus = Enum.TryParse<OcrStatus>(reader.GetString(reader.GetOrdinal("ocr_status")), out var ocrStatus) ? ocrStatus : OcrStatus.NotStarted,
            IndexStatus = reader.GetString(reader.GetOrdinal("index_status")),
            ClassificationStatus = reader.GetString(reader.GetOrdinal("classification_status")),
            Confidence = reader.IsDBNull(reader.GetOrdinal("confidence")) ? null : reader.GetDouble(reader.GetOrdinal("confidence")),
            CreatedAt = DateTimeOffset.Parse(reader.GetString(reader.GetOrdinal("created_at"))),
            ImportedAt = DateTimeOffset.Parse(reader.GetString(reader.GetOrdinal("imported_at"))),
            ModifiedAt = DateTimeOffset.Parse(reader.GetString(reader.GetOrdinal("modified_at"))),
            IsDeleted = reader.GetInt32(reader.GetOrdinal("is_deleted")) == 1
        };
    }
}
