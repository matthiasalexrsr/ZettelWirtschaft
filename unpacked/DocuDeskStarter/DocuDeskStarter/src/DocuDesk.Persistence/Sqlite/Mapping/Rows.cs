using DocuDesk.Application.Abstractions.Persistence.ReadModels;
using DocuDesk.Domain.Entities;
using DocuDesk.Domain.ValueObjects;

namespace DocuDesk.Persistence.Sqlite.Mapping;

internal sealed record DocumentRow
{
    public required string Id { get; init; }
    public string? Title { get; init; }
    public required string OriginalFileName { get; init; }
    public required string MimeType { get; init; }
    public required string Sha256 { get; init; }
    public int PageCount { get; init; }
    public required DateTime ImportDateUtc { get; init; }
    public string? DocumentDate { get; init; }
    public required string Status { get; init; }
    public string? Sender { get; init; }
    public string? Recipient { get; init; }
    public string? Notes { get; init; }
    public string? CategoryId { get; init; }
    public bool HasOcr { get; init; }
    public bool IsDeleted { get; init; }
    public required DateTime LastModifiedUtc { get; init; }

    public Document ToDomain() => new()
    {
        Id = Id,
        Title = Title,
        OriginalFileName = OriginalFileName,
        MimeType = MimeType,
        Sha256 = Sha256,
        PageCount = PageCount,
        ImportDateUtc = DateTime.SpecifyKind(ImportDateUtc, DateTimeKind.Utc),
        DocumentDate = string.IsNullOrWhiteSpace(DocumentDate) ? null : DateOnly.Parse(DocumentDate),
        Status = EnumMap.ToDocumentStatus(Status),
        Sender = Sender,
        Recipient = Recipient,
        Notes = Notes,
        CategoryId = CategoryId,
        HasOcr = HasOcr,
        IsDeleted = IsDeleted,
        LastModifiedUtc = DateTime.SpecifyKind(LastModifiedUtc, DateTimeKind.Utc)
    };
}

internal sealed record DocumentFileRow
{
    public long Id { get; init; }
    public required string DocumentId { get; init; }
    public required string OriginalPath { get; init; }
    public string? SearchablePdfPath { get; init; }
    public string? ThumbnailFolder { get; init; }
    public string? OcrArtifactPath { get; init; }
    public long FileSizeBytes { get; init; }
    public required string StorageState { get; init; }
    public required DateTime CreatedUtc { get; init; }

    public DocumentFile ToDomain() => new()
    {
        Id = Id,
        DocumentId = DocumentId,
        OriginalPath = OriginalPath,
        SearchablePdfPath = SearchablePdfPath,
        ThumbnailFolder = ThumbnailFolder,
        OcrArtifactPath = OcrArtifactPath,
        FileSizeBytes = FileSizeBytes,
        StorageState = EnumMap.ToStorageState(StorageState),
        CreatedUtc = DateTime.SpecifyKind(CreatedUtc, DateTimeKind.Utc)
    };
}

internal sealed record DocumentPageRow
{
    public required string Id { get; init; }
    public required string DocumentId { get; init; }
    public int PageNumber { get; init; }
    public int? WidthPx { get; init; }
    public int? HeightPx { get; init; }
    public int RotationDeg { get; init; }
    public string? PreviewImagePath { get; init; }

    public DocumentPage ToDomain() => new()
    {
        Id = Id,
        DocumentId = DocumentId,
        PageNumber = PageNumber,
        WidthPx = WidthPx,
        HeightPx = HeightPx,
        RotationDeg = RotationDeg,
        PreviewImagePath = PreviewImagePath
    };
}

internal sealed record DocumentTextRow
{
    public long Id { get; init; }
    public required string DocumentId { get; init; }
    public required string PlainText { get; init; }
    public required string NormalizedText { get; init; }
    public string? Language { get; init; }
    public double? OcrConfidenceAverage { get; init; }
    public DateTime? LastOcrUtc { get; init; }
    public int RowVersion { get; init; }

    public DocumentText ToDomain() => new()
    {
        Id = Id,
        DocumentId = DocumentId,
        PlainText = PlainText,
        NormalizedText = NormalizedText,
        Language = Language,
        OcrConfidenceAverage = OcrConfidenceAverage,
        LastOcrUtc = LastOcrUtc is null ? null : DateTime.SpecifyKind(LastOcrUtc.Value, DateTimeKind.Utc),
        RowVersion = RowVersion
    };
}

internal sealed record CategoryRow
{
    public required string Id { get; init; }
    public required string Name { get; init; }
    public string? ParentCategoryId { get; init; }
    public int SortOrder { get; init; }
    public bool IsSystem { get; init; }
    public required DateTime CreatedUtc { get; init; }

    public Category ToDomain() => new()
    {
        Id = Id,
        Name = Name,
        ParentCategoryId = ParentCategoryId,
        SortOrder = SortOrder,
        IsSystem = IsSystem,
        CreatedUtc = DateTime.SpecifyKind(CreatedUtc, DateTimeKind.Utc)
    };
}

internal sealed record TagRow
{
    public required string Id { get; init; }
    public required string Name { get; init; }
    public string? Color { get; init; }
    public int SortOrder { get; init; }
    public required DateTime CreatedUtc { get; init; }

    public Tag ToDomain() => new()
    {
        Id = Id,
        Name = Name,
        Color = Color,
        SortOrder = SortOrder,
        CreatedUtc = DateTime.SpecifyKind(CreatedUtc, DateTimeKind.Utc)
    };
}

internal sealed record OcrBlockRow
{
    public required string Id { get; init; }
    public required string DocumentId { get; init; }
    public required string PageId { get; init; }
    public string? ParentBlockId { get; init; }
    public required string BlockType { get; init; }
    public required string Text { get; init; }
    public double XNorm { get; init; }
    public double YNorm { get; init; }
    public double WNorm { get; init; }
    public double HNorm { get; init; }
    public double? Confidence { get; init; }
    public int ReadingOrder { get; init; }

    public OcrBlock ToDomain() => new()
    {
        Id = Id,
        DocumentId = DocumentId,
        PageId = PageId,
        ParentBlockId = ParentBlockId,
        BlockType = EnumMap.ToOcrBlockType(BlockType),
        Text = Text,
        Bounds = new NormalizedRect(XNorm, YNorm, WNorm, HNorm),
        Confidence = Confidence,
        ReadingOrder = ReadingOrder
    };
}

internal sealed record AnnotationRow
{
    public required string Id { get; init; }
    public required string DocumentId { get; init; }
    public required string PageId { get; init; }
    public required string Type { get; init; }
    public required string PayloadJson { get; init; }
    public double XNorm { get; init; }
    public double YNorm { get; init; }
    public double WNorm { get; init; }
    public double HNorm { get; init; }
    public int ZIndex { get; init; }
    public string? AuthorName { get; init; }
    public required DateTime CreatedUtc { get; init; }
    public required DateTime UpdatedUtc { get; init; }

    public Annotation ToDomain() => new()
    {
        Id = Id,
        DocumentId = DocumentId,
        PageId = PageId,
        Type = EnumMap.ToAnnotationType(Type),
        PayloadJson = PayloadJson,
        Bounds = new NormalizedRect(XNorm, YNorm, WNorm, HNorm),
        ZIndex = ZIndex,
        AuthorName = AuthorName,
        CreatedUtc = DateTime.SpecifyKind(CreatedUtc, DateTimeKind.Utc),
        UpdatedUtc = DateTime.SpecifyKind(UpdatedUtc, DateTimeKind.Utc)
    };
}

internal sealed record DocumentListItemRow
{
    public required string Id { get; init; }
    public string? Title { get; init; }
    public required string OriginalFileName { get; init; }
    public required string Status { get; init; }
    public string? DocumentDate { get; init; }
    public required DateTime ImportDateUtc { get; init; }
    public string? CategoryName { get; init; }
    public string? Sender { get; init; }
    public string? Recipient { get; init; }
    public int PageCount { get; init; }
    public bool HasOcr { get; init; }

    public DocumentListItem ToReadModel() => new()
    {
        Id = Id,
        Title = Title,
        OriginalFileName = OriginalFileName,
        Status = EnumMap.ToDocumentStatus(Status),
        DocumentDate = string.IsNullOrWhiteSpace(DocumentDate) ? null : DateOnly.Parse(DocumentDate),
        ImportDateUtc = DateTime.SpecifyKind(ImportDateUtc, DateTimeKind.Utc),
        CategoryName = CategoryName,
        Sender = Sender,
        Recipient = Recipient,
        PageCount = PageCount,
        HasOcr = HasOcr
    };
}

internal sealed record SearchHitRow
{
    public required string DocumentId { get; init; }
    public string? Title { get; init; }
    public string? Sender { get; init; }
    public string? Recipient { get; init; }
    public string? DocumentDate { get; init; }
    public required DateTime ImportDateUtc { get; init; }
    public double Rank { get; init; }
    public string? Snippet { get; init; }

    public SearchHit ToReadModel() => new()
    {
        DocumentId = DocumentId,
        Title = Title,
        Sender = Sender,
        Recipient = Recipient,
        DocumentDate = string.IsNullOrWhiteSpace(DocumentDate) ? null : DateOnly.Parse(DocumentDate),
        ImportDateUtc = DateTime.SpecifyKind(ImportDateUtc, DateTimeKind.Utc),
        Rank = Rank,
        Snippet = Snippet
    };
}

internal sealed record BackgroundJobRow
{
    public required string Id { get; init; }
    public required string JobType { get; init; }
    public string? RelatedDocumentId { get; init; }
    public required string PayloadJson { get; init; }
    public required string State { get; init; }
    public int Priority { get; init; }
    public int Attempts { get; init; }
    public int MaxAttempts { get; init; }
    public required DateTime ScheduledUtc { get; init; }
    public DateTime? StartedUtc { get; init; }
    public DateTime? CompletedUtc { get; init; }
    public string? ErrorText { get; init; }
    public required DateTime CreatedUtc { get; init; }

    public BackgroundJob ToDomain() => new()
    {
        Id = Id,
        JobType = EnumMap.ToBackgroundJobType(JobType),
        RelatedDocumentId = RelatedDocumentId,
        PayloadJson = PayloadJson,
        State = EnumMap.ToBackgroundJobState(State),
        Priority = Priority,
        Attempts = Attempts,
        MaxAttempts = MaxAttempts,
        ScheduledUtc = DateTime.SpecifyKind(ScheduledUtc, DateTimeKind.Utc),
        StartedUtc = StartedUtc is null ? null : DateTime.SpecifyKind(StartedUtc.Value, DateTimeKind.Utc),
        CompletedUtc = CompletedUtc is null ? null : DateTime.SpecifyKind(CompletedUtc.Value, DateTimeKind.Utc),
        ErrorText = ErrorText,
        CreatedUtc = DateTime.SpecifyKind(CreatedUtc, DateTimeKind.Utc)
    };
}

internal sealed record MailJobRow
{
    public required string Id { get; init; }
    public required string TargetClient { get; init; }
    public required string Direction { get; init; }
    public required string RequestJson { get; init; }
    public required string Status { get; init; }
    public string? ExternalReference { get; init; }
    public string? ErrorText { get; init; }
    public required DateTime CreatedUtc { get; init; }
    public DateTime? CompletedUtc { get; init; }

    public MailJob ToDomain() => new()
    {
        Id = Id,
        TargetClient = TargetClient,
        Direction = EnumMap.ToMailDirection(Direction),
        RequestJson = RequestJson,
        Status = EnumMap.ToMailJobStatus(Status),
        ExternalReference = ExternalReference,
        ErrorText = ErrorText,
        CreatedUtc = DateTime.SpecifyKind(CreatedUtc, DateTimeKind.Utc),
        CompletedUtc = CompletedUtc is null ? null : DateTime.SpecifyKind(CompletedUtc.Value, DateTimeKind.Utc)
    };
}
