using DocuDesk.Application.Abstractions.Persistence;
using DocuDesk.Application.Abstractions.Services;

namespace DocuDesk.Application.Documents;

public sealed class SyncDocumentTagsHandler
{
    private readonly ITagRepository _tagRepository;
    private readonly IClock _clock;

    public SyncDocumentTagsHandler(
        ITagRepository tagRepository,
        IClock clock)
    {
        _tagRepository = tagRepository;
        _clock = clock;
    }

    public async Task HandleAsync(string documentId, IReadOnlyCollection<string> selectedTagIds, CancellationToken ct)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(documentId);

        var normalizedSelected = new HashSet<string>(
            selectedTagIds.Where(static x => !string.IsNullOrWhiteSpace(x)).Select(static x => x.Trim()),
            StringComparer.Ordinal);

        var existing = await _tagRepository.GetByDocumentAsync(documentId, ct);
        var existingIds = new HashSet<string>(existing.Select(static x => x.Id), StringComparer.Ordinal);
        var now = _clock.UtcNow;

        foreach (var tagId in normalizedSelected.Except(existingIds, StringComparer.Ordinal))
        {
            await _tagRepository.AddToDocumentAsync(documentId, tagId, now, ct);
        }

        foreach (var tagId in existingIds.Except(normalizedSelected, StringComparer.Ordinal))
        {
            await _tagRepository.RemoveFromDocumentAsync(documentId, tagId, ct);
        }
    }
}
