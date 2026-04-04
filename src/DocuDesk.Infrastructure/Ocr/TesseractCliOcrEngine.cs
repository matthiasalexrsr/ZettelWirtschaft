using System.Diagnostics;
using System.Globalization;
using System.Text;
using DocuDesk.Application;
using DocuDesk.Application.Interfaces;
using DocuDesk.Domain.Entities;
using UglyToad.PdfPig;

namespace DocuDesk.Infrastructure.Ocr;

public sealed class TesseractCliOcrEngine : IOcrEngine
{
    private readonly AppSettings _settings;

    public TesseractCliOcrEngine(AppSettings settings)
    {
        _settings = settings;
    }

    public async Task<OcrExtractionResult> ExtractTextAsync(Guid documentId, string filePath, CancellationToken cancellationToken = default)
    {
        Directory.CreateDirectory(_settings.OcrArtifactsRoot);
        var artifactDirectory = Path.Combine(_settings.OcrArtifactsRoot, documentId.ToString("N"));
        Directory.CreateDirectory(artifactDirectory);
        var extension = Path.GetExtension(filePath).ToLowerInvariant();

        if (extension == ".pdf")
        {
            var embeddedText = TryExtractEmbeddedPdfText(filePath);
            if (!string.IsNullOrWhiteSpace(embeddedText))
            {
                var txtPath = Path.Combine(artifactDirectory, "document.txt");
                await File.WriteAllTextAsync(txtPath, embeddedText, cancellationToken);
                return new OcrExtractionResult
                {
                    DocumentText = new DocumentText
                    {
                        DocumentId = documentId,
                        PlainText = embeddedText,
                        NormalizedText = Normalize(embeddedText),
                        Language = _settings.OcrLanguage,
                        OcrConfidenceAverage = null,
                        LastOcrUtc = DateTimeOffset.UtcNow
                    }
                };
            }
        }

        if (!CanRunTesseract())
        {
            var fallbackText = extension == ".txt" ? await File.ReadAllTextAsync(filePath, cancellationToken) : string.Empty;
            if (!string.IsNullOrWhiteSpace(fallbackText))
            {
                await File.WriteAllTextAsync(Path.Combine(artifactDirectory, "document.txt"), fallbackText, cancellationToken);
            }

            return new OcrExtractionResult
            {
                DocumentText = new DocumentText
                {
                    DocumentId = documentId,
                    PlainText = fallbackText,
                    NormalizedText = Normalize(fallbackText),
                    Language = _settings.OcrLanguage,
                    OcrConfidenceAverage = null,
                    LastOcrUtc = DateTimeOffset.UtcNow
                }
            };
        }

        var outputBase = Path.Combine(artifactDirectory, "ocr");
        await RunTesseractAsync(filePath, outputBase, cancellationToken);

        var textPath = outputBase + ".txt";
        var tsvPath = outputBase + ".tsv";
        var hocrPath = outputBase + ".hocr";
        var pdfPath = outputBase + ".pdf";

        var plainText = File.Exists(textPath)
            ? await File.ReadAllTextAsync(textPath, cancellationToken)
            : string.Empty;

        var pages = new List<Page>();
        var blocks = new List<OcrBlock>();
        if (File.Exists(tsvPath))
        {
            ParseTsv(documentId, tsvPath, pages, blocks);
        }

        return new OcrExtractionResult
        {
            DocumentText = new DocumentText
            {
                DocumentId = documentId,
                PlainText = plainText,
                NormalizedText = Normalize(plainText),
                Language = _settings.OcrLanguage,
                OcrConfidenceAverage = TryReadAverageConfidence(tsvPath),
                LastOcrUtc = DateTimeOffset.UtcNow
            },
            Pages = pages,
            Blocks = blocks,
            SearchablePdfPath = File.Exists(pdfPath) ? pdfPath : null,
            TsvPath = File.Exists(tsvPath) ? tsvPath : null,
            HocrPath = File.Exists(hocrPath) ? hocrPath : null
        };
    }

    private static string Normalize(string text)
        => string.Join(' ', text.Split((char[]?)null, StringSplitOptions.RemoveEmptyEntries));

    private bool CanRunTesseract()
        => !string.IsNullOrWhiteSpace(_settings.TesseractExecutablePath);

    private async Task RunTesseractAsync(string filePath, string outputBase, CancellationToken cancellationToken)
    {
        var startInfo = new ProcessStartInfo
        {
            FileName = _settings.TesseractExecutablePath,
            RedirectStandardError = true,
            RedirectStandardOutput = true,
            UseShellExecute = false,
            CreateNoWindow = true,
            StandardOutputEncoding = Encoding.UTF8,
            StandardErrorEncoding = Encoding.UTF8,
            ArgumentList =
            {
                filePath,
                outputBase,
                "-l",
                _settings.OcrLanguage,
                "txt",
                "tsv",
                "hocr",
                "pdf"
            }
        };

        if (!string.IsNullOrWhiteSpace(_settings.TesseractDataPath))
        {
            startInfo.Environment["TESSDATA_PREFIX"] = _settings.TesseractDataPath;
        }

        using var process = new Process { StartInfo = startInfo };
        process.Start();
        await process.WaitForExitAsync(cancellationToken);

        if (process.ExitCode != 0)
        {
            var error = await process.StandardError.ReadToEndAsync(cancellationToken);
            throw new InvalidOperationException($"Tesseract execution failed: {error}");
        }
    }

    private static string TryExtractEmbeddedPdfText(string filePath)
    {
        try
        {
            using var document = PdfDocument.Open(filePath);
            var builder = new StringBuilder();
            foreach (var page in document.GetPages())
            {
                var pageText = page.Text;
                if (!string.IsNullOrWhiteSpace(pageText))
                {
                    builder.AppendLine(pageText);
                }
            }
            return builder.ToString().Trim();
        }
        catch
        {
            return string.Empty;
        }
    }

    private static void ParseTsv(Guid documentId, string tsvPath, List<Page> pages, List<OcrBlock> blocks)
    {
        var pageMap = new Dictionary<int, Page>();
        foreach (var line in File.ReadLines(tsvPath).Skip(1))
        {
            if (string.IsNullOrWhiteSpace(line))
            {
                continue;
            }

            var columns = line.Split('	');
            if (columns.Length < 12)
            {
                continue;
            }

            if (!int.TryParse(columns[0], out var level) || level != 5)
            {
                continue;
            }

            if (!int.TryParse(columns[1], out var pageNumber))
            {
                continue;
            }

            var text = columns[11]?.Trim();
            if (string.IsNullOrWhiteSpace(text))
            {
                continue;
            }

            var left = ParseInt(columns[6]);
            var top = ParseInt(columns[7]);
            var width = ParseInt(columns[8]);
            var height = ParseInt(columns[9]);
            var confidence = ParseNullableDouble(columns[10]);

            if (!pageMap.TryGetValue(pageNumber, out var page))
            {
                page = new Page
                {
                    Id = Guid.NewGuid(),
                    DocumentId = documentId,
                    PageNumber = pageNumber,
                    WidthPx = Math.Max(left + width, 1),
                    HeightPx = Math.Max(top + height, 1),
                    RotationDeg = 0
                };
                pageMap[pageNumber] = page;
            }
            else
            {
                page.WidthPx = Math.Max(page.WidthPx ?? 1, left + width);
                page.HeightPx = Math.Max(page.HeightPx ?? 1, top + height);
            }

            blocks.Add(new OcrBlock
            {
                Id = Guid.NewGuid(),
                DocumentId = documentId,
                PageId = page.Id,
                BlockType = "word",
                Text = text,
                X = left,
                Y = top,
                Width = width,
                Height = height,
                Confidence = confidence
            });
        }

        pages.AddRange(pageMap.OrderBy(kvp => kvp.Key).Select(kvp => kvp.Value));
    }

    private static int ParseInt(string value)
        => int.TryParse(value, NumberStyles.Integer, CultureInfo.InvariantCulture, out var parsed) ? parsed : 0;

    private static double? ParseNullableDouble(string value)
        => double.TryParse(value, NumberStyles.Float, CultureInfo.InvariantCulture, out var parsed) && parsed >= 0 ? parsed : null;

    private static double? TryReadAverageConfidence(string tsvPath)
    {
        try
        {
            if (!File.Exists(tsvPath))
            {
                return null;
            }

            var confidences = new List<double>();
            foreach (var line in File.ReadLines(tsvPath).Skip(1))
            {
                var columns = line.Split('	');
                if (columns.Length < 11)
                {
                    continue;
                }

                if (double.TryParse(columns[10], NumberStyles.Float, CultureInfo.InvariantCulture, out var confidence) && confidence >= 0)
                {
                    confidences.Add(confidence);
                }
            }

            return confidences.Count == 0 ? null : confidences.Average();
        }
        catch
        {
            return null;
        }
    }
}
