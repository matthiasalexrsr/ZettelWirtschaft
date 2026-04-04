using DocuDesk.Domain.Entities;

namespace DocuDesk.Application.Interfaces;

public interface IOcrEngine
{
    Task<OcrExtractionResult> ExtractTextAsync(Guid documentId, string filePath, CancellationToken cancellationToken = default);
}
