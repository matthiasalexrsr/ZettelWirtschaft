using DocuDesk.Domain.Entities;

namespace DocuDesk.Application.Abstractions.Persistence;

public interface IDocumentContentRepository
{
    Task<DocumentText?> GetTextAsync(string documentId, CancellationToken ct);
    Task ReplaceDocumentTextAsync(DocumentText text, CancellationToken ct);
    Task<IReadOnlyList<OcrBlock>> GetOcrBlocksByPageAsync(string pageId, CancellationToken ct);
    Task ReplaceOcrBlocksForPageAsync(string documentId, string pageId, IReadOnlyList<OcrBlock> blocks, CancellationToken ct);
}
