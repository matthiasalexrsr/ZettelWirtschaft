namespace DocuDesk.Domain.Entities;

public sealed class EmailLink
{
    public Guid Id { get; set; }
    public Guid DocumentId { get; set; }
    public string MessageId { get; set; } = string.Empty;
    public string? AccountId { get; set; }
    public string? FolderPath { get; set; }
    public string? AttachmentName { get; set; }
    public int? AttachmentIndex { get; set; }
    public string? MailSubject { get; set; }
    public string? MailFrom { get; set; }
    public DateTimeOffset? MailDate { get; set; }
    public DateTimeOffset ImportedAt { get; set; }
}
