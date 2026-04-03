using DocuDesk.Contracts.Mail;

namespace DocuDesk.Application.Interfaces;

public interface IMailClientAdapter
{
    Task<MailDraftResult> CreateDraftAsync(MailDraftRequest request, CancellationToken cancellationToken = default);
}
