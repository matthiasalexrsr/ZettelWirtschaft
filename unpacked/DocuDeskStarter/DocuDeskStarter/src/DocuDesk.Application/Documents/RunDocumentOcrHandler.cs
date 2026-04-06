using DocuDesk.Application.Abstractions.Persistence;
using DocuDesk.Application.Abstractions.Services;
using DocuDesk.Domain.Enums;

namespace DocuDesk.Application.Documents;

public sealed class RunDocumentOcrHandler
{
    private readonly IDocumentRepository _documentRepository;
    private readonly IOcrService _ocrService;
    private readonly CompleteOcrHandler _completeOcrHandler;
    private readonly IClock _clock;

    public RunDocumentOcrHandler(
        IDocumentRepository documentRepository,
        IOcrService ocrService,
        CompleteOcrHandler completeOcrHandler,
        IClock clock)
    {
        _documentRepository = documentRepository;
        _ocrService = ocrService;
        _completeOcrHandler = completeOcrHandler;
        _clock = clock;
    }

    public async Task HandleAsync(string documentId, string language, CancellationToken ct)
    {
        var details = await _documentRepository.GetDetailsAsync(documentId, ct)
            ?? throw new InvalidOperationException("Dokument wurde nicht gefunden.");

        if (string.IsNullOrWhiteSpace(details.File.OriginalPath) || !File.Exists(details.File.OriginalPath))
        {
            throw new InvalidOperationException("Originaldatei ist im Repository nicht vorhanden.");
        }

        var normalizedLanguage = string.IsNullOrWhiteSpace(language) ? "deu+eng" : language.Trim();

        await _documentRepository.SetStatusAsync(documentId, DocumentStatus.Processing, _clock.UtcNow, ct);

        try
        {
            var result = await _ocrService.RunAsync(new OcrRequest(
                details.Document.Id,
                details.File.OriginalPath,
                normalizedLanguage,
                details.Pages.Select(static x => x.Id).ToArray()), ct);

            await _completeOcrHandler.HandleAsync(
                documentId,
                details.Pages.Count,
                result.PlainText,
                result.NormalizedText,
                result.Language,
                result.OcrConfidenceAverage,
                result.CompletedUtc,
                result.BlocksByPageId,
                result.SearchablePdfPath,
                result.OcrArtifactPath,
                ct);
        }
        catch
        {
            await _documentRepository.SetStatusAsync(documentId, DocumentStatus.Error, _clock.UtcNow, ct);
            throw;
        }
    }
}
