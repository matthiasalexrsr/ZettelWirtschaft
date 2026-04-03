using DocuDesk.Application.Abstractions.Persistence.ReadModels;

namespace DocuDesk.App.ViewModels;

public sealed record SearchResultItemViewModel
{
    public required string DocumentId { get; init; }
    public required string Title { get; init; }
    public required string SecondaryText { get; init; }
    public required string Snippet { get; init; }
    public required string RankText { get; init; }

    public static SearchResultItemViewModel FromReadModel(SearchHit hit)
    {
        var title = string.IsNullOrWhiteSpace(hit.Title)
            ? "Unbenanntes Dokument"
            : hit.Title!;

        var metaParts = new List<string>();

        if (!string.IsNullOrWhiteSpace(hit.Sender))
        {
            metaParts.Add($"Absender: {hit.Sender}");
        }

        if (hit.DocumentDate is not null)
        {
            metaParts.Add($"Dokumentdatum: {hit.DocumentDate:dd.MM.yyyy}");
        }

        metaParts.Add($"Import: {hit.ImportDateUtc.LocalDateTime:dd.MM.yyyy HH:mm}");

        return new SearchResultItemViewModel
        {
            DocumentId = hit.DocumentId,
            Title = title,
            SecondaryText = string.Join(" • ", metaParts),
            Snippet = string.IsNullOrWhiteSpace(hit.Snippet) ? "Kein Snippet verfügbar." : hit.Snippet!,
            RankText = $"Relevanz: {Math.Abs(hit.Rank):0.000}"
        };
    }
}
