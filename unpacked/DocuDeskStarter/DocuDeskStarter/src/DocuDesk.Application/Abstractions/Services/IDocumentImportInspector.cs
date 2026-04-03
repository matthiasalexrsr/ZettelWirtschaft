namespace DocuDesk.Application.Abstractions.Services;

public interface IDocumentImportInspector
{
    Task<DocumentImportInspection> InspectAsync(string sourceFilePath, CancellationToken ct);
}

public sealed record DocumentImportInspection(
    string OriginalFileName,
    string MimeType,
    long FileSizeBytes,
    int PageCount,
    string? SuggestedTitle);
