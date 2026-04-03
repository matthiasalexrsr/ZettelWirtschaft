using DocuDesk.Application.Abstractions.Services;
using Microsoft.Extensions.Options;

namespace DocuDesk.Infrastructure.Files;

public sealed class LocalDocumentStorageService : IDocumentStorageService
{
    private readonly string _repositoryRoot;

    public LocalDocumentStorageService(IOptions<RepositoryStorageOptions> options)
    {
        _repositoryRoot = options.Value.RepositoryRoot;

        if (string.IsNullOrWhiteSpace(_repositoryRoot))
        {
            throw new InvalidOperationException("RepositoryRoot ist nicht konfiguriert.");
        }
    }

    public async Task<StoredDocumentFile> StoreImportedFileAsync(
        string documentId,
        string sha256,
        string sourceFilePath,
        CancellationToken ct)
    {
        var extension = Path.GetExtension(sourceFilePath);
        var shard = sha256.Length >= 2 ? sha256[..2].ToUpperInvariant() : "00";
        var documentFolder = Path.Combine(_repositoryRoot, "files", shard, documentId);
        Directory.CreateDirectory(documentFolder);

        var targetPath = Path.Combine(documentFolder, $"original{extension.ToLowerInvariant()}");

        await using (var sourceStream = File.Open(sourceFilePath, FileMode.Open, FileAccess.Read, FileShare.Read))
        await using (var targetStream = File.Create(targetPath))
        {
            await sourceStream.CopyToAsync(targetStream, ct);
        }

        var fileInfo = new FileInfo(targetPath);

        return new StoredDocumentFile(
            StoredPath: targetPath,
            FileSizeBytes: fileInfo.Length,
            OriginalFileName: Path.GetFileName(sourceFilePath));
    }
}
