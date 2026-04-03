namespace DocuDesk.Contracts.Documents;

public sealed class DocumentImportResult
{
    public Guid DocumentId { get; set; }
    public string StoredPath { get; set; } = string.Empty;
    public bool IsDuplicate { get; set; }
    public string? DuplicateOfDocumentId { get; set; }
}
