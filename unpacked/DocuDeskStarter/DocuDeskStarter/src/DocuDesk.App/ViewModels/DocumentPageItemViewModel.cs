namespace DocuDesk.App.ViewModels;

public sealed record DocumentPageItemViewModel
{
    public required string Id { get; init; }
    public int PageNumber { get; init; }
    public required string PageText { get; init; }
    public string RotationText { get; init; } = string.Empty;
    public string PreviewText { get; init; } = string.Empty;
}
