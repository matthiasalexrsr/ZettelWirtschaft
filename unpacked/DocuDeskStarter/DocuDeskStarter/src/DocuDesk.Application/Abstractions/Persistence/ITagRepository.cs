using DocuDesk.Domain.Entities;

namespace DocuDesk.Application.Abstractions.Persistence;

public interface ITagRepository
{
    Task<IReadOnlyList<Tag>> ListAsync(CancellationToken ct);
    Task<IReadOnlyList<Tag>> GetByDocumentAsync(string documentId, CancellationToken ct);
    Task AddToDocumentAsync(string documentId, string tagId, DateTimeOffset createdUtc, CancellationToken ct);
    Task RemoveFromDocumentAsync(string documentId, string tagId, CancellationToken ct);
}
