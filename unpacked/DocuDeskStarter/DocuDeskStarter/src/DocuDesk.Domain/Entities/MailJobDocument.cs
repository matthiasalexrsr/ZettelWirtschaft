using DocuDesk.Domain.Enums;

namespace DocuDesk.Domain.Entities;

public sealed record MailJobDocument
{
    public long Id { get; init; }
    public required string MailJobId { get; init; }
    public required string DocumentId { get; init; }
    public string? ExportArtifactPath { get; init; }
    public AttachmentRole AttachmentRole { get; init; }
}
