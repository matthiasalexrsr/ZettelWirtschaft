using DocuDesk.Application.Interfaces;
using DocuDesk.Contracts.Documents;

namespace DocuDesk.Application.Services;

public sealed class SearchService
{
    private readonly IDocumentRepository _documentRepository;

    public SearchService(IDocumentRepository documentRepository)
    {
        _documentRepository = documentRepository;
    }

    public Task<IReadOnlyList<DocumentListItemDto>> SearchAsync(DocumentSearchRequest request, CancellationToken cancellationToken = default)
        => _documentRepository.SearchAsync(request, cancellationToken);
}
