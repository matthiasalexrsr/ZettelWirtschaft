using DocuDesk.Domain.Entities;

namespace DocuDesk.Application.Abstractions.Persistence.ReadModels;

public sealed record DocumentDetails
{
    public required Document Document { get; init; }
    public required DocumentFile File { get; init; }
    public required IReadOnlyList<DocumentPage> Pages { get; init; }
    public DocumentText? Text { get; init; }
    public required IReadOnlyList<Tag> Tags { get; init; }
    public Category? Category { get; init; }
}
