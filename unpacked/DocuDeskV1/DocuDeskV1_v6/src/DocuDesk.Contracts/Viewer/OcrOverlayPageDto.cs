namespace DocuDesk.Contracts.Viewer;

public sealed class OcrOverlayPageDto
{
    public int PageNumber { get; set; }
    public int WidthPx { get; set; }
    public int HeightPx { get; set; }
    public string? PageText { get; set; }
    public List<OcrOverlayBlockDto> Blocks { get; set; } = new();
    public List<AnnotationOverlayItemDto> Annotations { get; set; } = new();
}
