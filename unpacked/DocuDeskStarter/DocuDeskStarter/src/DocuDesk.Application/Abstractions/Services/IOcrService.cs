using DocuDesk.Domain.Entities;

namespace DocuDesk.Application.Abstractions.Services;

public interface IOcrService
{
    Task<OcrResult> RunAsync(OcrRequest request, CancellationToken ct);
}

public sealed record OcrRequest(
    string DocumentId,
    string SourceFilePath,
    string Language,
    IReadOnlyList<string> PageIds);

public sealed record OcrResult(
    string DocumentId,
    string PlainText,
    string NormalizedText,
    string? Language,
    double? OcrConfidenceAverage,
    DateTimeOffset CompletedUtc,
    IReadOnlyDictionary<string, IReadOnlyList<OcrBlock>> BlocksByPageId,
    string? SearchablePdfPath,
    string? OcrArtifactPath);
