using DocuDesk.Application.Abstractions.Persistence.ReadModels;
using DocuDesk.Domain.Enums;

namespace DocuDesk.App.ViewModels;

public sealed record InboxDocumentItemViewModel
{
    public required string Id { get; init; }
    public required string DisplayTitle { get; init; }
    public required string SecondaryText { get; init; }
    public required string ImportDateText { get; init; }
    public required string StatusText { get; init; }
    public required string PageCountText { get; init; }
    public int PageCount { get; init; }
    public bool HasOcr { get; init; }

    public static InboxDocumentItemViewModel FromReadModel(DocumentListItem item)
    {
        var displayTitle = string.IsNullOrWhiteSpace(item.Title)
            ? item.OriginalFileName
            : item.Title!;

        var secondaryParts = new List<string>();

        if (!string.IsNullOrWhiteSpace(item.Sender))
        {
            secondaryParts.Add($"Absender: {item.Sender}");
        }

        if (!string.IsNullOrWhiteSpace(item.CategoryName))
        {
            secondaryParts.Add($"Kategorie: {item.CategoryName}");
        }

        secondaryParts.Add(item.HasOcr ? "OCR vorhanden" : "Noch keine OCR");

        return new InboxDocumentItemViewModel
        {
            Id = item.Id,
            DisplayTitle = displayTitle,
            SecondaryText = string.Join(" • ", secondaryParts),
            ImportDateText = $"Importiert: {item.ImportDateUtc.LocalDateTime:dd.MM.yyyy HH:mm}",
            StatusText = ToGermanStatus(item.Status),
            PageCountText = $"Seiten: {item.PageCount}",
            PageCount = item.PageCount,
            HasOcr = item.HasOcr
        };
    }

    private static string ToGermanStatus(DocumentStatus status) => status switch
    {
        DocumentStatus.New => "Neu",
        DocumentStatus.Processing => "In Verarbeitung",
        DocumentStatus.Ready => "Bereit",
        DocumentStatus.Review => "Prüfen",
        DocumentStatus.Error => "Fehler",
        DocumentStatus.Archived => "Archiviert",
        _ => status.ToString()
    };
}
