namespace DocuDesk.Contracts.Documents;

public sealed class DocumentImportRequest
{
    public string FilePath { get; set; } = string.Empty;
    public string? TitleOverride { get; set; }
}
