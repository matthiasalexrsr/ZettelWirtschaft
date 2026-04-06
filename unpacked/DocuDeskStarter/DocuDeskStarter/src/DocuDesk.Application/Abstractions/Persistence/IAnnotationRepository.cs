using DocuDesk.Domain.Entities;

namespace DocuDesk.Application.Abstractions.Persistence;

public interface IAnnotationRepository
{
    Task<IReadOnlyList<Annotation>> GetByDocumentAsync(string documentId, CancellationToken ct);
    Task<IReadOnlyList<Annotation>> GetByPageAsync(string pageId, CancellationToken ct);
    Task UpsertAsync(Annotation annotation, CancellationToken ct);
    Task DeleteAsync(string annotationId, CancellationToken ct);
}
