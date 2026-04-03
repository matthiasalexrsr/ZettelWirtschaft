using Dapper;
using DocuDesk.Application.Abstractions.Persistence;
using DocuDesk.Domain.Entities;
using DocuDesk.Persistence.Sqlite.Mapping;

namespace DocuDesk.Persistence.Sqlite.Repositories;

public sealed class SqliteCategoryRepository : ICategoryRepository
{
    private readonly ISqliteConnectionFactory _connectionFactory;

    public SqliteCategoryRepository(ISqliteConnectionFactory connectionFactory)
    {
        _connectionFactory = connectionFactory;
    }

    public async Task<IReadOnlyList<Category>> ListAsync(CancellationToken ct)
    {
        using var con = await _connectionFactory.OpenConnectionAsync(ct);
        var rows = await con.QueryAsync<CategoryRow>(new CommandDefinition("""
            SELECT
                id,
                name,
                parent_category_id AS ParentCategoryId,
                sort_order AS SortOrder,
                is_system AS IsSystem,
                created_utc AS CreatedUtc
            FROM categories
            ORDER BY sort_order, name;
            """, cancellationToken: ct));
        return rows.Select(x => x.ToDomain()).ToList();
    }

    public async Task<Category?> GetAsync(string categoryId, CancellationToken ct)
    {
        using var con = await _connectionFactory.OpenConnectionAsync(ct);
        var row = await con.QuerySingleOrDefaultAsync<CategoryRow>(new CommandDefinition("""
            SELECT
                id,
                name,
                parent_category_id AS ParentCategoryId,
                sort_order AS SortOrder,
                is_system AS IsSystem,
                created_utc AS CreatedUtc
            FROM categories
            WHERE id = @categoryId;
            """, new { categoryId }, cancellationToken: ct));
        return row?.ToDomain();
    }
}
