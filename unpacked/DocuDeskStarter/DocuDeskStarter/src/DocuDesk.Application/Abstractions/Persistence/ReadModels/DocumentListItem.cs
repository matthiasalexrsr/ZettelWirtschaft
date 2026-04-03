using DocuDesk.Domain.Enums;

namespace DocuDesk.Application.Abstractions.Persistence.ReadModels;

public sealed record DocumentListItem
{
    public required string Id { get; init; }
    public string? Title { get; init; }
    public required string OriginalFileName { get; init; }
    public DocumentStatus Status { get; init; }
    public DateOnly? DocumentDate { get; init; }
    public required DateTimeOffset ImportDateUtc { get; init; }
    public string? CategoryName { get; init; }
    public string? Sender { get; init; }
    public string? Recipient { get; init; }
    public int PageCount { get; init; }
    public bool HasOcr { get; init; }
}
