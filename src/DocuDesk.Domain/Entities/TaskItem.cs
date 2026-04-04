namespace DocuDesk.Domain.Entities;

public sealed class TaskItem
{
    public Guid Id { get; set; }
    public Guid DocumentId { get; set; }
    public string Title { get; set; } = string.Empty;
    public DateTimeOffset? DueDate { get; set; }
    public string Priority { get; set; } = "Normal";
    public string Status { get; set; } = "Open";
    public string? Notes { get; set; }
    public DateTimeOffset CreatedAt { get; set; }
}
