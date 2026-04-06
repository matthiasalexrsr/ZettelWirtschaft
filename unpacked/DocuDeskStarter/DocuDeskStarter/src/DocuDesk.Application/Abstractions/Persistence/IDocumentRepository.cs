using DocuDesk.Application.Abstractions.Persistence.ReadModels;
using DocuDesk.Domain.Entities;
using DocuDesk.Domain.Enums;

namespace DocuDesk.Application.Abstractions.Persistence;

public interface IDocumentRepository
{
    Task<bool> ExistsBySha256Async(string sha256, CancellationToken ct);
    Task<Document?> GetAsync(string documentId, CancellationToken ct);
    Task<DocumentDetails?> GetDetailsAsync(string documentId, CancellationToken ct);
    Task<IReadOnlyList<DocumentListItem>> ListAsync(DocumentListFilter filter, CancellationToken ct);
    Task InsertAsync(Document document, DocumentFile file, IReadOnlyList<DocumentPage> pages, CancellationToken ct);
    Task UpdateMetadataAsync(Document document, CancellationToken ct);
    Task SetStatusAsync(string documentId, DocumentStatus status, DateTimeOffset modifiedUtc, CancellationToken ct);
    Task SetOcrCompletedAsync(string documentId, int pageCount, DateTimeOffset modifiedUtc, CancellationToken ct);
    Task UpdateFileArtifactsAsync(string documentId, string? searchablePdfPath, string? ocrArtifactPath, CancellationToken ct);
    Task SoftDeleteAsync(string documentId, DateTimeOffset modifiedUtc, CancellationToken ct);
}
