namespace DocuDesk.Domain.Entities;

public sealed class Annotation
{
    public Guid Id { get; set; }
    public Guid DocumentId { get; set; }
    public Guid? PageId { get; set; }
    public string AnnotationType { get; set; } = string.Empty;
    public string PayloadJson { get; set; } = string.Empty;
    public double? X { get; set; }
    public double? Y { get; set; }
    public double? Width { get; set; }
    public double? Height { get; set; }
    public DateTimeOffset CreatedAt { get; set; }
    public DateTimeOffset? UpdatedAt { get; set; }
}
