using DocuDesk.Domain.Entities;

namespace DocuDesk.Application.Abstractions.Persistence;

public interface ICategoryRepository
{
    Task<IReadOnlyList<Category>> ListAsync(CancellationToken ct);
    Task<Category?> GetAsync(string categoryId, CancellationToken ct);
}
