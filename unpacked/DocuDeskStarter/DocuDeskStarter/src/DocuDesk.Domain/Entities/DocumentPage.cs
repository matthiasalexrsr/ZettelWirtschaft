namespace DocuDesk.Domain.Entities;

public sealed record DocumentPage
{
    public required string Id { get; init; }
    public required string DocumentId { get; init; }
    public int PageNumber { get; init; }
    public int? WidthPx { get; init; }
    public int? HeightPx { get; init; }
    public int RotationDeg { get; init; }
    public string? PreviewImagePath { get; init; }
}
