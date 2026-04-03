using DocuDesk.Application.Abstractions.Services;
using DocuDesk.Domain.Entities;
using DocuDesk.Domain.Enums;
using DocuDesk.Domain.ValueObjects;
using DocuDesk.Infrastructure.Files;
using Microsoft.Extensions.Options;
using System.Diagnostics;
using System.Globalization;
using System.Text;
using System.Text.RegularExpressions;

namespace DocuDesk.Infrastructure.Ocr;

public sealed class TesseractOcrService : IOcrService
{
    private static readonly HashSet<string> ImageExtensions = new(StringComparer.OrdinalIgnoreCase)
    {
        ".png", ".jpg", ".jpeg", ".bmp", ".tif", ".tiff"
    };

    private readonly RepositoryStorageOptions _storageOptions;
    private readonly TesseractOcrOptions _options;

    public TesseractOcrService(
        IOptions<RepositoryStorageOptions> storageOptions,
        IOptions<TesseractOcrOptions> options)
    {
        _storageOptions = storageOptions.Value;
        _options = options.Value;
    }

    public async Task<OcrResult> RunAsync(OcrRequest request, CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(request.SourceFilePath) || !File.Exists(request.SourceFilePath))
        {
            throw new InvalidOperationException("Quelldatei für OCR wurde nicht gefunden.");
        }

        var language = string.IsNullOrWhiteSpace(request.Language)
            ? _options.DefaultLanguage
            : request.Language.Trim();

        var artifactRoot = Path.Combine(_storageOptions.RepositoryRoot, "derivatives", request.DocumentId, "ocr");
        if (Directory.Exists(artifactRoot))
        {
            Directory.Delete(artifactRoot, recursive: true);
        }

        Directory.CreateDirectory(artifactRoot);

        var pageInputs = await ResolvePageInputsAsync(request.SourceFilePath, artifactRoot, ct);

        var allText = new StringBuilder();
        var allBlocks = new Dictionary<string, IReadOnlyList<OcrBlock>>(StringComparer.Ordinal);
        var confidences = new List<double>();
        string? searchablePdfPath = null;

        var pagesToProcess = request.PageIds.Count == 0
            ? pageInputs.Count
            : Math.Min(pageInputs.Count, request.PageIds.Count);

        for (var index = 0; index < pagesToProcess; index++)
        {
            ct.ThrowIfCancellationRequested();

            var pageId = index < request.PageIds.Count
                ? request.PageIds[index]
                : $"page-{index + 1}";

            var pageInput = pageInputs[index];
            var outputBase = Path.Combine(artifactRoot, $"page-{index + 1:0000}");

            await RunTesseractAsync(pageInput, outputBase, language, ct);

            var txtPath = outputBase + ".txt";
            var tsvPath = outputBase + ".tsv";
            var pdfPath = outputBase + ".pdf";

            var pageText = File.Exists(txtPath)
                ? await File.ReadAllTextAsync(txtPath, Encoding.UTF8, ct)
                : string.Empty;

            if (!string.IsNullOrWhiteSpace(pageText))
            {
                if (allText.Length > 0)
                {
                    allText.AppendLine().AppendLine();
                }

                allText.Append(pageText.Trim());
            }

            var parseResult = await ParseTsvAsync(tsvPath, request.DocumentId, pageId, ct);
            allBlocks[pageId] = parseResult.Blocks;
            confidences.AddRange(parseResult.WordConfidences);

            if (pageInputs.Count == 1 && File.Exists(pdfPath))
            {
                searchablePdfPath = pdfPath;
            }
        }

        var plainText = allText.ToString().Trim();
        var normalizedText = NormalizeText(plainText);
        var averageConfidence = confidences.Count == 0 ? null : confidences.Average();

        return new OcrResult(
            request.DocumentId,
            plainText,
            normalizedText,
            language,
            averageConfidence,
            DateTimeOffset.UtcNow,
            allBlocks,
            searchablePdfPath,
            artifactRoot);
    }

    private async Task<IReadOnlyList<string>> ResolvePageInputsAsync(string sourceFilePath, string artifactRoot, CancellationToken ct)
    {
        var extension = Path.GetExtension(sourceFilePath);
        if (ImageExtensions.Contains(extension))
        {
            return [sourceFilePath];
        }

        if (string.Equals(extension, ".pdf", StringComparison.OrdinalIgnoreCase))
        {
            var rasterRoot = Path.Combine(artifactRoot, "raster");
            Directory.CreateDirectory(rasterRoot);

            var prefix = Path.Combine(rasterRoot, "page");
            await RunPdfToPpmAsync(sourceFilePath, prefix, ct);

            var files = Directory.GetFiles(rasterRoot, "page-*.png")
                .OrderBy(static x => ExtractTrailingNumber(Path.GetFileNameWithoutExtension(x)))
                .ToList();

            if (files.Count == 0)
            {
                throw new InvalidOperationException("PDF konnte nicht in Bildseiten aufgelöst werden.");
            }

            return files;
        }

        throw new InvalidOperationException($"OCR wird für den Dateityp '{extension}' aktuell nicht unterstützt.");
    }

    private async Task RunPdfToPpmAsync(string sourcePdfPath, string outputPrefix, CancellationToken ct)
    {
        var startInfo = new ProcessStartInfo
        {
            FileName = _options.PdfToPpmExecutablePath,
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            UseShellExecute = false,
            CreateNoWindow = true
        };

        startInfo.ArgumentList.Add("-png");
        startInfo.ArgumentList.Add("-r");
        startInfo.ArgumentList.Add(_options.PdfRasterDpi.ToString(CultureInfo.InvariantCulture));
        startInfo.ArgumentList.Add(sourcePdfPath);
        startInfo.ArgumentList.Add(outputPrefix);

        await RunExternalProcessAsync(
            startInfo,
            "PDF-Rasterisierung",
            "pdftoppm wurde nicht gefunden. Installiere Poppler/pdftoppm oder verwende zunächst Bilddateien für OCR.",
            ct);
    }

    private async Task RunTesseractAsync(string inputPath, string outputBase, string language, CancellationToken ct)
    {
        var startInfo = new ProcessStartInfo
        {
            FileName = _options.TesseractExecutablePath,
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            UseShellExecute = false,
            CreateNoWindow = true
        };

        startInfo.ArgumentList.Add(inputPath);
        startInfo.ArgumentList.Add(outputBase);
        startInfo.ArgumentList.Add("-l");
        startInfo.ArgumentList.Add(language);
        startInfo.ArgumentList.Add("--oem");
        startInfo.ArgumentList.Add("1");
        startInfo.ArgumentList.Add("txt");
        startInfo.ArgumentList.Add("tsv");
        startInfo.ArgumentList.Add("pdf");

        await RunExternalProcessAsync(
            startInfo,
            "OCR-Ausführung",
            "Tesseract wurde nicht gefunden. Installiere Tesseract OCR und stelle sicher, dass 'tesseract' im PATH liegt oder konfiguriert ist.",
            ct);
    }

    private static async Task RunExternalProcessAsync(
        ProcessStartInfo startInfo,
        string operationName,
        string notFoundMessage,
        CancellationToken ct)
    {
        try
        {
            using var process = new Process { StartInfo = startInfo };
            process.Start();

            var stdOutTask = process.StandardOutput.ReadToEndAsync(ct);
            var stdErrTask = process.StandardError.ReadToEndAsync(ct);

            await process.WaitForExitAsync(ct);

            var stdOut = await stdOutTask;
            var stdErr = await stdErrTask;

            if (process.ExitCode != 0)
            {
                var message = string.IsNullOrWhiteSpace(stdErr) ? stdOut : stdErr;
                throw new InvalidOperationException($"{operationName} fehlgeschlagen: {message.Trim()}");
            }
        }
        catch (System.ComponentModel.Win32Exception)
        {
            throw new InvalidOperationException(notFoundMessage);
        }
    }

    private static async Task<TsvParseResult> ParseTsvAsync(string tsvPath, string documentId, string pageId, CancellationToken ct)
    {
        if (!File.Exists(tsvPath))
        {
            return new TsvParseResult([], []);
        }

        var lines = await File.ReadAllLinesAsync(tsvPath, ct);
        if (lines.Length <= 1)
        {
            return new TsvParseResult([], []);
        }

        var blocks = new List<OcrBlock>();
        var confidences = new List<double>();

        double pageWidth = 0;
        double pageHeight = 0;
        var readingOrder = 0;

        foreach (var raw in lines.Skip(1))
        {
            if (string.IsNullOrWhiteSpace(raw))
            {
                continue;
            }

            var columns = raw.Split('\t');
            if (columns.Length < 12)
            {
                continue;
            }

            if (!int.TryParse(columns[0], NumberStyles.Integer, CultureInfo.InvariantCulture, out var level))
            {
                continue;
            }

            if (!double.TryParse(columns[6], NumberStyles.Float, CultureInfo.InvariantCulture, out var left)
                || !double.TryParse(columns[7], NumberStyles.Float, CultureInfo.InvariantCulture, out var top)
                || !double.TryParse(columns[8], NumberStyles.Float, CultureInfo.InvariantCulture, out var width)
                || !double.TryParse(columns[9], NumberStyles.Float, CultureInfo.InvariantCulture, out var height))
            {
                continue;
            }

            if (level == 1)
            {
                pageWidth = width;
                pageHeight = height;
                continue;
            }

            if (pageWidth <= 0 || pageHeight <= 0)
            {
                continue;
            }

            var text = columns.Length > 11 ? columns[11] : string.Empty;
            var confValue = columns[10];

            if (level is 4 or 5 && (width > 0 && height > 0))
            {
                var bounds = new NormalizedRect(
                    Clamp01(left / pageWidth),
                    Clamp01(top / pageHeight),
                    Clamp01(width / pageWidth),
                    Clamp01(height / pageHeight));

                var confidence = double.TryParse(confValue, NumberStyles.Float, CultureInfo.InvariantCulture, out var conf) && conf >= 0
                    ? conf
                    : null;

                if (level == 5 && confidence.HasValue)
                {
                    confidences.Add(confidence.Value);
                }

                blocks.Add(new OcrBlock
                {
                    Id = Guid.NewGuid().ToString("N"),
                    DocumentId = documentId,
                    PageId = pageId,
                    ParentBlockId = null,
                    BlockType = level == 4 ? OcrBlockType.Line : OcrBlockType.Word,
                    Text = text ?? string.Empty,
                    Bounds = bounds,
                    Confidence = confidence,
                    ReadingOrder = ++readingOrder
                });
            }
        }

        return new TsvParseResult(blocks, confidences);
    }

    private static string NormalizeText(string text)
    {
        if (string.IsNullOrWhiteSpace(text))
        {
            return string.Empty;
        }

        var normalized = Regex.Replace(text, @"\s+", " ");
        return normalized.Trim();
    }

    private static int ExtractTrailingNumber(string name)
    {
        var match = Regex.Match(name, @"(\d+)$");
        return match.Success && int.TryParse(match.Groups[1].Value, out var value)
            ? value
            : int.MaxValue;
    }

    private static double Clamp01(double value) => Math.Max(0, Math.Min(1, value));

    private sealed record TsvParseResult(
        IReadOnlyList<OcrBlock> Blocks,
        IReadOnlyList<double> WordConfidences);
}
