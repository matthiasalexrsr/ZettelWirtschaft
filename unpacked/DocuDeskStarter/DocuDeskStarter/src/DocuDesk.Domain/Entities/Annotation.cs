using DocuDesk.Domain.Enums;
using DocuDesk.Domain.ValueObjects;

namespace DocuDesk.Domain.Entities;

public sealed record Annotation
{
    public required string Id { get; init; }
    public required string DocumentId { get; init; }
    public required string PageId { get; init; }
    public AnnotationType Type { get; init; }
    public required string PayloadJson { get; init; }
    public required NormalizedRect Bounds { get; init; }
    public int ZIndex { get; init; }
    public string? AuthorName { get; init; }
    public required DateTimeOffset CreatedUtc { get; init; }
    public required DateTimeOffset UpdatedUtc { get; init; }
}
