using DocuDesk.Domain.Enums;

namespace DocuDesk.Domain.Entities;

public sealed record DocumentFile
{
    public long Id { get; init; }
    public required string DocumentId { get; init; }
    public required string OriginalPath { get; init; }
    public string? SearchablePdfPath { get; init; }
    public string? ThumbnailFolder { get; init; }
    public string? OcrArtifactPath { get; init; }
    public long FileSizeBytes { get; init; }
    public StorageState StorageState { get; init; } = StorageState.Present;
    public required DateTimeOffset CreatedUtc { get; init; }
}
