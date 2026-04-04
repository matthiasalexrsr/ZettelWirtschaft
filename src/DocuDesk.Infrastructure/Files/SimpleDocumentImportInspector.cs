using DocuDesk.Application.Abstractions.Services;
using System.Text;
using System.Text.RegularExpressions;

namespace DocuDesk.Infrastructure.Files;

public sealed partial class SimpleDocumentImportInspector : IDocumentImportInspector
{
    public async Task<DocumentImportInspection> InspectAsync(string sourceFilePath, CancellationToken ct)
    {
        var fileInfo = new FileInfo(sourceFilePath);
        var originalFileName = fileInfo.Name;
        var extension = fileInfo.Extension.ToLowerInvariant();

        var mimeType = extension switch
        {
            ".pdf" => "application/pdf",
            ".tif" or ".tiff" => "image/tiff",
            ".png" => "image/png",
            ".jpg" or ".jpeg" => "image/jpeg",
            ".bmp" => "image/bmp",
            _ => "application/octet-stream"
        };

        var pageCount = extension switch
        {
            ".pdf" => await EstimatePdfPageCountAsync(sourceFilePath, ct),
            _ => 1
        };

        return new DocumentImportInspection(
            OriginalFileName: originalFileName,
            MimeType: mimeType,
            FileSizeBytes: fileInfo.Length,
            PageCount: Math.Max(pageCount, 1),
            SuggestedTitle: Path.GetFileNameWithoutExtension(originalFileName));
    }

    private static async Task<int> EstimatePdfPageCountAsync(string sourceFilePath, CancellationToken ct)
    {
        try
        {
            var content = await File.ReadAllTextAsync(sourceFilePath, Encoding.ASCII, ct);
            var matches = PdfPageRegex().Matches(content);
            return matches.Count > 0 ? matches.Count : 1;
        }
        catch
        {
            return 1;
        }
    }

    [GeneratedRegex(@"/Type\s*/Page(?!s)", RegexOptions.Compiled)]
    private static partial Regex PdfPageRegex();
}
