using DocuDesk.Domain.Enums;

namespace DocuDesk.Domain.Entities;

public sealed class Document
{
    public Guid Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string OriginalFileName { get; set; } = string.Empty;
    public string DisplayName { get; set; } = string.Empty;
    public string MimeType { get; set; } = string.Empty;
    public string Extension { get; set; } = string.Empty;
    public string Sha256 { get; set; } = string.Empty;
    public long FileSizeBytes { get; set; }
    public int? PageCount { get; set; }
    public string RepositoryPath { get; set; } = string.Empty;
    public string? PreviewPath { get; set; }
    public string? ThumbnailPath { get; set; }
    public DocumentSourceType SourceType { get; set; } = DocumentSourceType.FileSystem;
    public string? SourceReference { get; set; }
    public string? Language { get; set; }
    public DateTimeOffset? DocumentDate { get; set; }
    public string? Sender { get; set; }
    public string? Recipient { get; set; }
    public string? Subject { get; set; }
    public string? Notes { get; set; }
    public string? DocumentType { get; set; }
    public Guid? CategoryId { get; set; }
    public OcrStatus OcrStatus { get; set; } = OcrStatus.NotStarted;
    public string IndexStatus { get; set; } = "Pending";
    public string ClassificationStatus { get; set; } = "Pending";
    public double? Confidence { get; set; }
    public DateTimeOffset CreatedAt { get; set; }
    public DateTimeOffset ImportedAt { get; set; }
    public DateTimeOffset ModifiedAt { get; set; }
    public bool IsDeleted { get; set; }
}
