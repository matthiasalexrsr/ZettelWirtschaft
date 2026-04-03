namespace DocuDesk.Desktop.ViewModels;

public sealed class JobRecordViewModel
{
    public string JobType { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public DateTimeOffset CreatedAt { get; set; }
    public string? ErrorText { get; set; }
}
