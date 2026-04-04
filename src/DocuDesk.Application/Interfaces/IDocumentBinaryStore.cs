namespace DocuDesk.Application.Interfaces;

public interface IDocumentBinaryStore
{
    Task<string> StoreOriginalAsync(string sourceFilePath, Guid documentId, CancellationToken cancellationToken = default);
    Task<Stream> OpenReadAsync(string repositoryPath, CancellationToken cancellationToken = default);
}
