namespace DocuDesk.Application.Abstractions.Persistence.ReadModels;

public sealed record SearchHit
{
    public required string DocumentId { get; init; }
    public string? Title { get; init; }
    public string? Sender { get; init; }
    public string? Recipient { get; init; }
    public DateOnly? DocumentDate { get; init; }
    public DateTimeOffset ImportDateUtc { get; init; }
    public double Rank { get; init; }
    public string? Snippet { get; init; }
}
