using DocuDesk.Domain.Enums;
using DocuDesk.Domain.ValueObjects;

namespace DocuDesk.Domain.Entities;

public sealed record OcrBlock
{
    public required string Id { get; init; }
    public required string DocumentId { get; init; }
    public required string PageId { get; init; }
    public string? ParentBlockId { get; init; }
    public OcrBlockType BlockType { get; init; }
    public required string Text { get; init; }
    public required NormalizedRect Bounds { get; init; }
    public double? Confidence { get; init; }
    public int ReadingOrder { get; init; }
}
