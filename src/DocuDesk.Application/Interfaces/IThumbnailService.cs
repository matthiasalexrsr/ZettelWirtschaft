namespace DocuDesk.Application.Interfaces;

public interface IThumbnailService
{
    Task<string?> TryGenerateAsync(Guid documentId, string sourceFilePath, CancellationToken cancellationToken = default);
}
