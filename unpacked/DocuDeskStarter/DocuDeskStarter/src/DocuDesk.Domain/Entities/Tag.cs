namespace DocuDesk.Domain.Entities;

public sealed record Tag
{
    public required string Id { get; init; }
    public required string Name { get; init; }
    public string? Color { get; init; }
    public int SortOrder { get; init; }
    public required DateTimeOffset CreatedUtc { get; init; }
}
