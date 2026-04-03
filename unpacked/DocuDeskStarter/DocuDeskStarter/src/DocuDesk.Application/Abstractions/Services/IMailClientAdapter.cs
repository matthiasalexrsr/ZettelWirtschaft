namespace DocuDesk.Application.Abstractions.Services;

public interface IMailClientAdapter
{
    Task<MailDispatchResult> CreateDraftAsync(MailDispatchRequest request, CancellationToken ct);
}

public sealed record MailDispatchRequest(
    string MailJobId,
    string Subject,
    string Body,
    IReadOnlyList<string> To,
    IReadOnlyList<string> Cc,
    IReadOnlyList<string> Bcc,
    IReadOnlyList<MailAttachment> Attachments);

public sealed record MailAttachment(string FilePath, string DisplayName, string Role);
public sealed record MailDispatchResult(bool Success, string? ExternalReference, string? ErrorText);
