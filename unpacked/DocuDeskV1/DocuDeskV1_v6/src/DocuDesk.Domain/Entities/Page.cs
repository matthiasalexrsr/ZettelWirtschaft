namespace DocuDesk.Domain.Entities;

public sealed class Page
{
    public Guid Id { get; set; }
    public Guid DocumentId { get; set; }
    public int PageNumber { get; set; }
    public int? WidthPx { get; set; }
    public int? HeightPx { get; set; }
    public int RotationDeg { get; set; }
}
