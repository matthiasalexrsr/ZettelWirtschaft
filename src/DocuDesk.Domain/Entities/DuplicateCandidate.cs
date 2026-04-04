namespace DocuDesk.Domain.Entities;

public sealed class DuplicateCandidate
{
    public Guid Id { get; set; }
    public Guid DocumentIdA { get; set; }
    public Guid DocumentIdB { get; set; }
    public string MatchType { get; set; } = string.Empty;
    public double Score { get; set; }
    public string Status { get; set; } = "Open";
    public DateTimeOffset CreatedAt { get; set; }
}
