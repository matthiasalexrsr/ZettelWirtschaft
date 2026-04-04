namespace DocuDesk.Domain.Entities;

public sealed class OcrBlock
{
    public Guid Id { get; set; }
    public Guid DocumentId { get; set; }
    public Guid? PageId { get; set; }
    public string BlockType { get; set; } = string.Empty;
    public string Text { get; set; } = string.Empty;
    public double X { get; set; }
    public double Y { get; set; }
    public double Width { get; set; }
    public double Height { get; set; }
    public double? Confidence { get; set; }
}
