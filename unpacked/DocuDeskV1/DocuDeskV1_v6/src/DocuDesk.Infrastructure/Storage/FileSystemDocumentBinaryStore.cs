using DocuDesk.Application.Interfaces;

namespace DocuDesk.Infrastructure.Storage;

public sealed class FileSystemDocumentBinaryStore : IDocumentBinaryStore
{
    private readonly string _repositoryRoot;

    public FileSystemDocumentBinaryStore(string repositoryRoot)
    {
        _repositoryRoot = repositoryRoot;
        Directory.CreateDirectory(_repositoryRoot);
    }

    public async Task<string> StoreOriginalAsync(string sourceFilePath, Guid documentId, CancellationToken cancellationToken = default)
    {
        var now = DateTime.UtcNow;
        var safeName = SanitizeFileName(Path.GetFileName(sourceFilePath));
        var relativePath = Path.Combine("originals", now.Year.ToString("0000"), now.Month.ToString("00"), $"{documentId:N}_{safeName}");
        var fullPath = Path.Combine(_repositoryRoot, relativePath);
        Directory.CreateDirectory(Path.GetDirectoryName(fullPath)!);

        await using var source = File.OpenRead(sourceFilePath);
        await using var destination = File.Create(fullPath);
        await source.CopyToAsync(destination, cancellationToken);

        return fullPath;
    }

    public Task<Stream> OpenReadAsync(string repositoryPath, CancellationToken cancellationToken = default)
    {
        Stream stream = File.OpenRead(repositoryPath);
        return Task.FromResult(stream);
    }

    private static string SanitizeFileName(string fileName)
    {
        var invalidChars = Path.GetInvalidFileNameChars();
        return string.Concat(fileName.Select(ch => invalidChars.Contains(ch) ? '_' : ch));
    }
}
