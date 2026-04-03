namespace DocuDesk.Contracts.Viewer;

public sealed class AnnotationOverlayItemDto
{
    public Guid Id { get; set; }
    public string AnnotationType { get; set; } = string.Empty;
    public double X { get; set; }
    public double Y { get; set; }
    public double Width { get; set; }
    public double Height { get; set; }
    public string? Text { get; set; }
    public string? Color { get; set; }
}
