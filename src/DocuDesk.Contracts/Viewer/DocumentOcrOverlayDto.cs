namespace DocuDesk.Contracts.Viewer;

public sealed class DocumentOcrOverlayDto
{
    public Guid DocumentId { get; set; }
    public string? Language { get; set; }
    public double? AverageConfidence { get; set; }
    public List<OcrOverlayPageDto> Pages { get; set; } = new();
}
