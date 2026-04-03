using DocuDesk.Application;
using DocuDesk.Application.Interfaces;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Formats.Png;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;

namespace DocuDesk.Infrastructure.Imaging;

public sealed class ImageSharpThumbnailService : IThumbnailService
{
    private readonly AppSettings _settings;

    public ImageSharpThumbnailService(AppSettings settings)
    {
        _settings = settings;
    }

    public async Task<string?> TryGenerateAsync(Guid documentId, string sourceFilePath, CancellationToken cancellationToken = default)
    {
        Directory.CreateDirectory(_settings.CacheRoot);
        var thumbsRoot = Path.Combine(_settings.CacheRoot, "thumbs");
        Directory.CreateDirectory(thumbsRoot);
        var outputDirectory = Path.Combine(thumbsRoot, documentId.ToString("N"));
        Directory.CreateDirectory(outputDirectory);
        var outputPath = Path.Combine(outputDirectory, "page-1.png");

        var extension = Path.GetExtension(sourceFilePath).ToLowerInvariant();
        if (extension is ".png" or ".jpg" or ".jpeg" or ".bmp" or ".gif" or ".tif" or ".tiff")
        {
            using var image = await Image.LoadAsync(sourceFilePath, cancellationToken);
            image.Mutate(x => x.Resize(new ResizeOptions
            {
                Mode = ResizeMode.Max,
                Size = new Size(320, 320)
            }));
            await image.SaveAsync(outputPath, new PngEncoder(), cancellationToken);
            return outputPath;
        }

        using var placeholder = new Image<Rgba32>(320, 240, new Rgba32(245, 247, 250, 255));
        placeholder.Mutate(x =>
        {
            x.Fill(new Rgba32(245, 247, 250, 255));
            x.DrawLine(new Rgba32(220, 226, 232, 255), 2, new PointF(0, 0), new PointF(319, 0));
            x.DrawLine(new Rgba32(220, 226, 232, 255), 2, new PointF(0, 239), new PointF(319, 239));
            x.DrawLine(new Rgba32(220, 226, 232, 255), 2, new PointF(0, 0), new PointF(0, 239));
            x.DrawLine(new Rgba32(220, 226, 232, 255), 2, new PointF(319, 0), new PointF(319, 239));
            x.Fill(extension == ".pdf" ? new Rgba32(220, 38, 38, 255) : new Rgba32(59, 130, 246, 255), new Rectangle(24, 24, 64, 80));
            x.Fill(new Rgba32(255, 255, 255, 255), new Rectangle(36, 40, 40, 8));
            x.Fill(new Rgba32(255, 255, 255, 255), new Rectangle(36, 58, 40, 8));
            x.Fill(new Rgba32(255, 255, 255, 255), new Rectangle(36, 76, 24, 8));
            x.Fill(new Rgba32(229, 231, 235, 255), new Rectangle(110, 40, 170, 10));
            x.Fill(new Rgba32(229, 231, 235, 255), new Rectangle(110, 68, 140, 10));
            x.Fill(new Rgba32(229, 231, 235, 255), new Rectangle(110, 96, 170, 10));
            x.Fill(new Rgba32(229, 231, 235, 255), new Rectangle(24, 150, 260, 8));
            x.Fill(new Rgba32(229, 231, 235, 255), new Rectangle(24, 170, 200, 8));
            x.Fill(new Rgba32(229, 231, 235, 255), new Rectangle(24, 190, 230, 8));
        });
        await placeholder.SaveAsync(outputPath, new PngEncoder(), cancellationToken);
        return outputPath;
    }
}
