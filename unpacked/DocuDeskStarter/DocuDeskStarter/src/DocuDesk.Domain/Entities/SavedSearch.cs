namespace DocuDesk.Domain.Entities;

public sealed record SavedSearch
{
    public required string Id { get; init; }
    public required string Name { get; init; }
    public required string QueryJson { get; init; }
    public required string SortMode { get; init; }
    public bool IsPinned { get; init; }
    public required DateTimeOffset CreatedUtc { get; init; }
    public required DateTimeOffset UpdatedUtc { get; init; }
}
