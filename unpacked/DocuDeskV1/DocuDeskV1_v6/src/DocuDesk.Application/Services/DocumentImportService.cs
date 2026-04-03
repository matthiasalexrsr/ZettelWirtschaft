using System.Security.Cryptography;
using DocuDesk.Application.Interfaces;
using DocuDesk.Contracts.Documents;
using DocuDesk.Domain.Entities;
using DocuDesk.Domain.Enums;

namespace DocuDesk.Application.Services;

public sealed class DocumentImportService : IImportService
{
    private readonly IDocumentRepository _documentRepository;
    private readonly IDocumentBinaryStore _binaryStore;
    private readonly IAuditLog _auditLog;
    private readonly IJobStore _jobStore;
    private readonly ISearchIndex _searchIndex;

    public DocumentImportService(
        IDocumentRepository documentRepository,
        IDocumentBinaryStore binaryStore,
        IAuditLog auditLog,
        IJobStore jobStore,
        ISearchIndex searchIndex)
    {
        _documentRepository = documentRepository;
        _binaryStore = binaryStore;
        _auditLog = auditLog;
        _jobStore = jobStore;
        _searchIndex = searchIndex;
    }

    public async Task<DocumentImportResult> ImportAsync(DocumentImportRequest request, CancellationToken cancellationToken = default)
    {
        if (!File.Exists(request.FilePath))
        {
            throw new FileNotFoundException("Datei nicht gefunden.", request.FilePath);
        }

        var hash = await ComputeSha256Async(request.FilePath, cancellationToken);
        var duplicate = await _documentRepository.FindBySha256Async(hash, cancellationToken);
        if (duplicate is not null)
        {
            await _auditLog.WriteAsync("DuplicateDetected", "Document", duplicate.Id.ToString(), request.FilePath, cancellationToken);
            return new DocumentImportResult
            {
                DocumentId = duplicate.Id,
                StoredPath = duplicate.RepositoryPath,
                IsDuplicate = true,
                DuplicateOfDocumentId = duplicate.Id.ToString()
            };
        }

        var documentId = Guid.NewGuid();
        var storedPath = await _binaryStore.StoreOriginalAsync(request.FilePath, documentId, cancellationToken);
        var fileInfo = new FileInfo(request.FilePath);
        var title = string.IsNullOrWhiteSpace(request.TitleOverride)
            ? Path.GetFileNameWithoutExtension(request.FilePath)
            : request.TitleOverride.Trim();

        var now = DateTimeOffset.UtcNow;
        var document = new Document
        {
            Id = documentId,
            Title = title,
            OriginalFileName = fileInfo.Name,
            DisplayName = fileInfo.Name,
            MimeType = GuessMimeType(fileInfo.Extension),
            Extension = fileInfo.Extension,
            Sha256 = hash,
            FileSizeBytes = fileInfo.Length,
            RepositoryPath = storedPath,
            SourceType = DocumentSourceType.FileSystem,
            CreatedAt = now,
            ImportedAt = now,
            ModifiedAt = now,
            OcrStatus = OcrStatus.NotStarted,
            IndexStatus = "IndexedMetadataOnly",
            ClassificationStatus = "Pending"
        };

        await _documentRepository.SaveAsync(document, cancellationToken);
        await _searchIndex.IndexDocumentAsync(document, null, cancellationToken);
        await _auditLog.WriteAsync("Imported", "Document", document.Id.ToString(), document.OriginalFileName, cancellationToken);

        await _jobStore.EnqueueAsync(new JobRecord
        {
            Id = Guid.NewGuid(),
            JobType = "GenerateThumbnailJob",
            TargetDocumentId = document.Id,
            Status = JobStatus.Pending,
            CreatedAt = now,
            LastUpdatedAt = now
        }, cancellationToken);

        await _jobStore.EnqueueAsync(new JobRecord
        {
            Id = Guid.NewGuid(),
            JobType = "RunOcrJob",
            TargetDocumentId = document.Id,
            Status = JobStatus.Pending,
            CreatedAt = now,
            LastUpdatedAt = now
        }, cancellationToken);

        return new DocumentImportResult
        {
            DocumentId = document.Id,
            StoredPath = storedPath,
            IsDuplicate = false
        };
    }

    private static async Task<string> ComputeSha256Async(string filePath, CancellationToken cancellationToken)
    {
        await using var stream = File.OpenRead(filePath);
        using var sha = SHA256.Create();
        var hash = await sha.ComputeHashAsync(stream, cancellationToken);
        return Convert.ToHexString(hash);
    }

    private static string GuessMimeType(string extension) => extension.ToLowerInvariant() switch
    {
        ".pdf" => "application/pdf",
        ".png" => "image/png",
        ".jpg" or ".jpeg" => "image/jpeg",
        ".tif" or ".tiff" => "image/tiff",
        ".txt" => "text/plain",
        _ => "application/octet-stream"
    };
}
