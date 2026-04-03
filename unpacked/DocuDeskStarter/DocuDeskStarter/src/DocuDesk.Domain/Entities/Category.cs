namespace DocuDesk.Domain.Entities;

public sealed record Category
{
    public required string Id { get; init; }
    public required string Name { get; init; }
    public string? ParentCategoryId { get; init; }
    public int SortOrder { get; init; }
    public bool IsSystem { get; init; }
    public required DateTimeOffset CreatedUtc { get; init; }
}
