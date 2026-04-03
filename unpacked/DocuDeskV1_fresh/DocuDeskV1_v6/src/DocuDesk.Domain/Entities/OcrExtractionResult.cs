namespace DocuDesk.Domain.Entities;

public sealed class OcrExtractionResult
{
    public DocumentText DocumentText { get; set; } = new();
    public List<Page> Pages { get; set; } = new();
    public List<OcrBlock> Blocks { get; set; } = new();
    public string? SearchablePdfPath { get; set; }
    public string? TsvPath { get; set; }
    public string? HocrPath { get; set; }
}
