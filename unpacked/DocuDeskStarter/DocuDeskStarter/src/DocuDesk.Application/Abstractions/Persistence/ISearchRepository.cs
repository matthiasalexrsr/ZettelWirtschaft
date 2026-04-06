using DocuDesk.Application.Abstractions.Persistence.ReadModels;
using DocuDesk.Domain.Entities;

namespace DocuDesk.Application.Abstractions.Persistence;

public interface ISearchRepository
{
    Task<IReadOnlyList<SearchHit>> SearchAsync(string query, int skip, int take, CancellationToken ct);
    Task RebuildProjectionAsync(CancellationToken ct);
    Task<IReadOnlyList<SavedSearch>> ListSavedSearchesAsync(CancellationToken ct);
    Task UpsertSavedSearchAsync(SavedSearch search, CancellationToken ct);
    Task DeleteSavedSearchAsync(string searchId, CancellationToken ct);
}
