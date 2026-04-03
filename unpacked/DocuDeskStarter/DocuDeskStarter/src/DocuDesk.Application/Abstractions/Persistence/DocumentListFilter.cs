using DocuDesk.Domain.Enums;

namespace DocuDesk.Application.Abstractions.Persistence;

public sealed record DocumentListFilter
{
    public string? QueryText { get; init; }
    public string? CategoryId { get; init; }
    public string? TagId { get; init; }
    public DocumentStatus? Status { get; init; }
    public bool ExcludeDeleted { get; init; } = true;
    public int Skip { get; init; }
    public int Take { get; init; } = 100;
    public string SortBy { get; init; } = "import_date_desc";
}
