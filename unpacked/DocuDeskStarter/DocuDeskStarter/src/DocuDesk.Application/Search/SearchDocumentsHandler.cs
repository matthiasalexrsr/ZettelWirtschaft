using DocuDesk.Application.Abstractions.Persistence;
using DocuDesk.Application.Abstractions.Persistence.ReadModels;

namespace DocuDesk.Application.Search;

public sealed class SearchDocumentsHandler
{
    private readonly ISearchRepository _searchRepository;

    public SearchDocumentsHandler(ISearchRepository searchRepository)
    {
        _searchRepository = searchRepository;
    }

    public Task<IReadOnlyList<SearchHit>> HandleAsync(string query, int skip, int take, CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(query))
        {
            return Task.FromResult<IReadOnlyList<SearchHit>>(Array.Empty<SearchHit>());
        }

        return _searchRepository.SearchAsync(query.Trim(), skip, take, ct);
    }
}
