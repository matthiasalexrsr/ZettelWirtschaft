namespace DocuDesk.Contracts.Mail;

public sealed class MailDraftRequest
{
    public IReadOnlyList<string> Recipients { get; set; } = Array.Empty<string>();
    public string Subject { get; set; } = string.Empty;
    public string BodyPlainText { get; set; } = string.Empty;
    public IReadOnlyList<string> Attachments { get; set; } = Array.Empty<string>();
}
