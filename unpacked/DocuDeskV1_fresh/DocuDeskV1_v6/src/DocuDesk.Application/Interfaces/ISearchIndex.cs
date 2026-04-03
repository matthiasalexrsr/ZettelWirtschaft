using DocuDesk.Domain.Entities;

namespace DocuDesk.Application.Interfaces;

public interface ISearchIndex
{
    Task IndexDocumentAsync(Document document, string? bodyText, CancellationToken cancellationToken = default);
    Task RemoveDocumentAsync(Guid documentId, CancellationToken cancellationToken = default);
}
