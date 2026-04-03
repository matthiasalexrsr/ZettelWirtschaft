using DocuDesk.Domain.Enums;

namespace DocuDesk.Persistence.Sqlite.Mapping;

internal static class EnumMap
{
    public static string ToDb(this DocumentStatus value) => value switch
    {
        DocumentStatus.New => "new",
        DocumentStatus.Processing => "processing",
        DocumentStatus.Ready => "ready",
        DocumentStatus.Review => "review",
        DocumentStatus.Error => "error",
        DocumentStatus.Archived => "archived",
        _ => throw new ArgumentOutOfRangeException(nameof(value))
    };

    public static DocumentStatus ToDocumentStatus(string value) => value switch
    {
        "new" => DocumentStatus.New,
        "processing" => DocumentStatus.Processing,
        "ready" => DocumentStatus.Ready,
        "review" => DocumentStatus.Review,
        "error" => DocumentStatus.Error,
        "archived" => DocumentStatus.Archived,
        _ => throw new InvalidOperationException($"Unknown document status '{value}'.")
    };

    public static string ToDb(this StorageState value) => value.ToString().ToLowerInvariant();
    public static StorageState ToStorageState(string value) => Enum.Parse<StorageState>(value, ignoreCase: true);

    public static string ToDb(this OcrBlockType value) => value switch
    {
        OcrBlockType.Page => "page",
        OcrBlockType.Block => "block",
        OcrBlockType.Paragraph => "paragraph",
        OcrBlockType.Line => "line",
        OcrBlockType.Word => "word",
        _ => throw new ArgumentOutOfRangeException(nameof(value))
    };

    public static OcrBlockType ToOcrBlockType(string value) => value switch
    {
        "page" => OcrBlockType.Page,
        "block" => OcrBlockType.Block,
        "paragraph" => OcrBlockType.Paragraph,
        "line" => OcrBlockType.Line,
        "word" => OcrBlockType.Word,
        _ => throw new InvalidOperationException($"Unknown OCR block type '{value}'.")
    };

    public static string ToDb(this AnnotationType value) => value switch
    {
        AnnotationType.Highlight => "highlight",
        AnnotationType.Rectangle => "rectangle",
        AnnotationType.Arrow => "arrow",
        AnnotationType.Note => "note",
        AnnotationType.Stamp => "stamp",
        AnnotationType.Redaction => "redaction",
        _ => throw new ArgumentOutOfRangeException(nameof(value))
    };

    public static AnnotationType ToAnnotationType(string value) => value switch
    {
        "highlight" => AnnotationType.Highlight,
        "rectangle" => AnnotationType.Rectangle,
        "arrow" => AnnotationType.Arrow,
        "note" => AnnotationType.Note,
        "stamp" => AnnotationType.Stamp,
        "redaction" => AnnotationType.Redaction,
        _ => throw new InvalidOperationException($"Unknown annotation type '{value}'.")
    };

    public static string ToDb(this BackgroundJobType value) => value switch
    {
        BackgroundJobType.Import => "import",
        BackgroundJobType.Thumbnail => "thumbnail",
        BackgroundJobType.Ocr => "ocr",
        BackgroundJobType.Reindex => "reindex",
        BackgroundJobType.Export => "export",
        BackgroundJobType.IntegrityCheck => "integrity_check",
        _ => throw new ArgumentOutOfRangeException(nameof(value))
    };

    public static BackgroundJobType ToBackgroundJobType(string value) => value switch
    {
        "import" => BackgroundJobType.Import,
        "thumbnail" => BackgroundJobType.Thumbnail,
        "ocr" => BackgroundJobType.Ocr,
        "reindex" => BackgroundJobType.Reindex,
        "export" => BackgroundJobType.Export,
        "integrity_check" => BackgroundJobType.IntegrityCheck,
        _ => throw new InvalidOperationException($"Unknown background job type '{value}'.")
    };

    public static string ToDb(this BackgroundJobState value) => value switch
    {
        BackgroundJobState.Queued => "queued",
        BackgroundJobState.Running => "running",
        BackgroundJobState.Completed => "completed",
        BackgroundJobState.Failed => "failed",
        BackgroundJobState.Cancelled => "cancelled",
        _ => throw new ArgumentOutOfRangeException(nameof(value))
    };

    public static BackgroundJobState ToBackgroundJobState(string value) => value switch
    {
        "queued" => BackgroundJobState.Queued,
        "running" => BackgroundJobState.Running,
        "completed" => BackgroundJobState.Completed,
        "failed" => BackgroundJobState.Failed,
        "cancelled" => BackgroundJobState.Cancelled,
        _ => throw new InvalidOperationException($"Unknown background job state '{value}'.")
    };

    public static string ToDb(this MailDirection value) => value switch
    {
        MailDirection.AppToMail => "app_to_mail",
        MailDirection.MailToApp => "mail_to_app",
        _ => throw new ArgumentOutOfRangeException(nameof(value))
    };

    public static MailDirection ToMailDirection(string value) => value switch
    {
        "app_to_mail" => MailDirection.AppToMail,
        "mail_to_app" => MailDirection.MailToApp,
        _ => throw new InvalidOperationException($"Unknown mail direction '{value}'.")
    };

    public static string ToDb(this MailJobStatus value) => value switch
    {
        MailJobStatus.Queued => "queued",
        MailJobStatus.Dispatched => "dispatched",
        MailJobStatus.Completed => "completed",
        MailJobStatus.Failed => "failed",
        _ => throw new ArgumentOutOfRangeException(nameof(value))
    };

    public static MailJobStatus ToMailJobStatus(string value) => value switch
    {
        "queued" => MailJobStatus.Queued,
        "dispatched" => MailJobStatus.Dispatched,
        "completed" => MailJobStatus.Completed,
        "failed" => MailJobStatus.Failed,
        _ => throw new InvalidOperationException($"Unknown mail job status '{value}'.")
    };

    public static string ToDb(this AttachmentRole value) => value switch
    {
        AttachmentRole.Original => "original",
        AttachmentRole.SearchablePdf => "searchable_pdf",
        AttachmentRole.ExtractText => "extract_text",
        AttachmentRole.ImageClip => "image_clip",
        _ => throw new ArgumentOutOfRangeException(nameof(value))
    };

    public static AttachmentRole ToAttachmentRole(string value) => value switch
    {
        "original" => AttachmentRole.Original,
        "searchable_pdf" => AttachmentRole.SearchablePdf,
        "extract_text" => AttachmentRole.ExtractText,
        "image_clip" => AttachmentRole.ImageClip,
        _ => throw new InvalidOperationException($"Unknown attachment role '{value}'.")
    };
}
