using DocuDesk.Application.Abstractions.Persistence;
using DocuDesk.Domain.Entities;

namespace DocuDesk.Application.Documents;

public sealed class CompleteOcrHandler
{
    private readonly IDocumentContentRepository _contentRepository;
    private readonly IDocumentRepository _documentRepository;

    public CompleteOcrHandler(
        IDocumentContentRepository contentRepository,
        IDocumentRepository documentRepository)
    {
        _contentRepository = contentRepository;
        _documentRepository = documentRepository;
    }

    public async Task HandleAsync(
        string documentId,
        int pageCount,
        string plainText,
        string normalizedText,
        string? language,
        double? confidence,
        DateTimeOffset completedUtc,
        IReadOnlyDictionary<string, IReadOnlyList<OcrBlock>> blocksByPageId,
        string? searchablePdfPath,
        string? ocrArtifactPath,
        CancellationToken ct)
    {
        var text = new DocumentText
        {
            DocumentId = documentId,
            PlainText = plainText,
            NormalizedText = normalizedText,
            Language = language,
            OcrConfidenceAverage = confidence,
            LastOcrUtc = completedUtc,
            RowVersion = 1
        };

        await _contentRepository.ReplaceDocumentTextAsync(text, ct);

        foreach (var pair in blocksByPageId)
        {
            await _contentRepository.ReplaceOcrBlocksForPageAsync(documentId, pair.Key, pair.Value, ct);
        }

        await _documentRepository.UpdateFileArtifactsAsync(documentId, searchablePdfPath, ocrArtifactPath, ct);
        await _documentRepository.SetOcrCompletedAsync(documentId, pageCount, completedUtc, ct);
    }
}
