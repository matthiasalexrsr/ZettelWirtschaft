namespace DocuDesk.Domain.Entities;

public sealed class DocumentText
{
    public Guid DocumentId { get; set; }
    public string PlainText { get; set; } = string.Empty;
    public string? NormalizedText { get; set; }
    public string? Language { get; set; }
    public double? OcrConfidenceAverage { get; set; }
    public DateTimeOffset? LastOcrUtc { get; set; }
}
