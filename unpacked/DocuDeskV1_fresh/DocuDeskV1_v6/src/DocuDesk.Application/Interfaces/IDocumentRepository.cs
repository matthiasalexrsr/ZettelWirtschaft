using DocuDesk.Contracts.Documents;
using DocuDesk.Contracts.Viewer;
using DocuDesk.Domain.Entities;

namespace DocuDesk.Application.Interfaces;

public interface IDocumentRepository
{
    Task SaveAsync(Document document, CancellationToken cancellationToken = default);
    Task<Document?> GetAsync(Guid id, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<DocumentListItemDto>> SearchAsync(DocumentSearchRequest request, CancellationToken cancellationToken = default);
    Task<Document?> FindBySha256Async(string sha256, CancellationToken cancellationToken = default);
    Task UpsertDocumentTextAsync(DocumentText documentText, CancellationToken cancellationToken = default);
    Task ReplacePagesAsync(Guid documentId, IReadOnlyList<Page> pages, CancellationToken cancellationToken = default);
    Task ReplaceOcrBlocksAsync(Guid documentId, IReadOnlyList<OcrBlock> blocks, CancellationToken cancellationToken = default);
    Task SaveAnnotationAsync(Guid documentId, ViewerAnnotationCreateRequest request, CancellationToken cancellationToken = default);
    Task<DocumentOcrOverlayDto?> GetOcrOverlayAsync(Guid documentId, CancellationToken cancellationToken = default);
}
