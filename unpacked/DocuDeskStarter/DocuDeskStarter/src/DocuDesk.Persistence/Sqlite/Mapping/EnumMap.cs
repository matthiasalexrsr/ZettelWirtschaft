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
}
