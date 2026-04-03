using System.Diagnostics;
using DocuDesk.Application.Interfaces;
using DocuDesk.Contracts.Mail;

namespace DocuDesk.Integrations.Mail;

public sealed class ThunderbirdShellAdapter : IMailClientAdapter
{
    public Task<MailDraftResult> CreateDraftAsync(MailDraftRequest request, CancellationToken cancellationToken = default)
    {
        try
        {
            var subject = Uri.EscapeDataString(request.Subject);
            var body = Uri.EscapeDataString(request.BodyPlainText);
            var recipient = request.Recipients.FirstOrDefault() ?? string.Empty;
            var mailto = $"mailto:{recipient}?subject={subject}&body={body}";

            Process.Start(new ProcessStartInfo
            {
                FileName = mailto,
                UseShellExecute = true
            });

            return Task.FromResult(new MailDraftResult
            {
                Success = true,
                ClientName = "DefaultMailClient",
                ExternalReference = mailto
            });
        }
        catch (Exception ex)
        {
            return Task.FromResult(new MailDraftResult
            {
                Success = false,
                ClientName = "DefaultMailClient",
                ErrorText = ex.Message
            });
        }
    }
}
