using DocuDesk.Application.Abstractions.Services;

namespace DocuDesk.Infrastructure.Mail;

public sealed class ThunderbirdNativeMailClientAdapter : IMailClientAdapter
{
    public Task<MailDispatchResult> CreateDraftAsync(MailDispatchRequest request, CancellationToken ct)
    {
        // V1-Skelett: echte Thunderbird-/Native-Messaging-Übergabe folgt im nächsten Ausbauschritt.
        return Task.FromResult(new MailDispatchResult(
            Success: false,
            ExternalReference: null,
            ErrorText: "Thunderbird-Integration ist im Starterprojekt nur als Schnittstelle vorbereitet."));
    }
}
