using DocuDesk.Application.Abstractions.Persistence;
using DocuDesk.Application.Abstractions.Services;

namespace DocuDesk.Application.Documents;

public sealed record UpdateDocumentMetadataRequest(
    string DocumentId,
    string? Title,
    DateOnly? DocumentDate,
    string? Sender,
    string? Recipient,
    string? Notes,
    string? CategoryId);

public sealed class UpdateDocumentMetadataHandler
{
    private readonly IDocumentRepository _documentRepository;
    private readonly IClock _clock;

    public UpdateDocumentMetadataHandler(
        IDocumentRepository documentRepository,
        IClock clock)
    {
        _documentRepository = documentRepository;
        _clock = clock;
    }

    public async Task HandleAsync(UpdateDocumentMetadataRequest request, CancellationToken ct)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(request.DocumentId);

        var current = await _documentRepository.GetAsync(request.DocumentId, ct)
            ?? throw new InvalidOperationException("Dokument wurde nicht gefunden.");

        var updated = current with
        {
            Title = Normalize(request.Title),
            DocumentDate = request.DocumentDate,
            Sender = Normalize(request.Sender),
            Recipient = Normalize(request.Recipient),
            Notes = Normalize(request.Notes),
            CategoryId = Normalize(request.CategoryId),
            LastModifiedUtc = _clock.UtcNow
        };

        await _documentRepository.UpdateMetadataAsync(updated, ct);
    }

    private static string? Normalize(string? value) => string.IsNullOrWhiteSpace(value) ? null : value.Trim();
}
