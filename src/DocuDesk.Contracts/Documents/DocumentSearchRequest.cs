namespace DocuDesk.Contracts.Documents;

public sealed class DocumentSearchRequest
{
    public string? SearchText { get; set; }
    public int Take { get; set; } = 200;
}
