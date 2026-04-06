namespace DocuDesk.Domain.Entities;

public sealed record DocumentText
{
    public long Id { get; init; }
    public required string DocumentId { get; init; }
    public required string PlainText { get; init; }
    public required string NormalizedText { get; init; }
    public string? Language { get; init; }
    public double? OcrConfidenceAverage { get; init; }
    public DateTimeOffset? LastOcrUtc { get; init; }
    public int RowVersion { get; init; }
}
