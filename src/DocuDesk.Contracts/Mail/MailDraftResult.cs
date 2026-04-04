namespace DocuDesk.Contracts.Mail;

public sealed class MailDraftResult
{
    public bool Success { get; set; }
    public string ClientName { get; set; } = string.Empty;
    public string? ExternalReference { get; set; }
    public string? ErrorText { get; set; }
}
