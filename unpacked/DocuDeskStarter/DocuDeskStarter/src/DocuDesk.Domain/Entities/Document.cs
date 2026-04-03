using DocuDesk.Domain.Enums;

namespace DocuDesk.Domain.Entities;

public sealed record Document
{
    public required string Id { get; init; }
    public string? Title { get; init; }
    public required string OriginalFileName { get; init; }
    public required string MimeType { get; init; }
    public required string Sha256 { get; init; }
    public int PageCount { get; init; }
    public required DateTimeOffset ImportDateUtc { get; init; }
    public DateOnly? DocumentDate { get; init; }
    public DocumentStatus Status { get; init; } = DocumentStatus.New;
    public string? Sender { get; init; }
    public string? Recipient { get; init; }
    public string? Notes { get; init; }
    public string? CategoryId { get; init; }
    public bool HasOcr { get; init; }
    public bool IsDeleted { get; init; }
    public required DateTimeOffset LastModifiedUtc { get; init; }
}
