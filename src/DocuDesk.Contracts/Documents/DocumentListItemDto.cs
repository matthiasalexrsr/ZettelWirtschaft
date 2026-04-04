namespace DocuDesk.Contracts.Documents;

public sealed class DocumentListItemDto
{
    public Guid Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string DisplayName { get; set; } = string.Empty;
    public string? Sender { get; set; }
    public string? DocumentType { get; set; }
    public DateTimeOffset ImportedAt { get; set; }
    public string OcrStatus { get; set; } = string.Empty;
    public string RepositoryPath { get; set; } = string.Empty;
    public string? MatchSnippet { get; set; }
    public double? MatchScore { get; set; }
}
