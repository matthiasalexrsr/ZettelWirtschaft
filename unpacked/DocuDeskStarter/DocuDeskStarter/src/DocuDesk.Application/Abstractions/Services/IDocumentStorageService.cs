namespace DocuDesk.Application.Abstractions.Services;

public interface IDocumentStorageService
{
    Task<StoredDocumentFile> StoreImportedFileAsync(
        string documentId,
        string sha256,
        string sourceFilePath,
        CancellationToken ct);
}

public sealed record StoredDocumentFile(
    string StoredPath,
    long FileSizeBytes,
    string OriginalFileName);
