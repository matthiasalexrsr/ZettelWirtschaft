using DocuDesk.Application;
using DocuDesk.Application.Interfaces;
using DocuDesk.Application.Services;
using DocuDesk.Infrastructure.DependencyInjection;
using DocuDesk.Persistence.DependencyInjection;
using Microsoft.Extensions.DependencyInjection;

namespace DocuDesk.IntegrationTests.Infrastructure;

public sealed class IntegrationTestFixture : IDisposable
{
    private readonly string _rootPath;

    public IntegrationTestFixture()
    {
        _rootPath = Path.Combine(Path.GetTempPath(), $"docudesk-int-{Guid.NewGuid():N}");
        Directory.CreateDirectory(_rootPath);

        Settings = new AppSettings
        {
            AppDataRoot = Path.Combine(_rootPath, "app"),
            RepositoryRoot = Path.Combine(_rootPath, "repo"),
            DatabasePath = Path.Combine(_rootPath, "db", "app.db"),
            BackupsPath = Path.Combine(_rootPath, "backups"),
            LogsPath = Path.Combine(_rootPath, "logs"),
            CacheRoot = Path.Combine(_rootPath, "cache"),
            OcrArtifactsRoot = Path.Combine(_rootPath, "ocr"),
            TesseractExecutablePath = "tesseract",
            OcrLanguage = "deu+eng"
        };

        Directory.CreateDirectory(Settings.RepositoryRoot);
        Directory.CreateDirectory(Path.GetDirectoryName(Settings.DatabasePath)!);
        Directory.CreateDirectory(Settings.BackupsPath);
        Directory.CreateDirectory(Settings.LogsPath);
        Directory.CreateDirectory(Settings.CacheRoot);
        Directory.CreateDirectory(Settings.OcrArtifactsRoot);

        var services = new ServiceCollection();
        services.AddDocuDeskInfrastructure(Settings);
        services.AddDocuDeskPersistence(Settings.DatabasePath);
        services.AddSingleton<IImportService, DocumentImportService>();
        services.AddSingleton<SearchService>();

        Services = services.BuildServiceProvider();
    }

    public AppSettings Settings { get; }

    public ServiceProvider Services { get; }

    public void Dispose()
    {
        Services.Dispose();
        try
        {
            Directory.Delete(_rootPath, recursive: true);
        }
        catch
        {
            // Best effort cleanup.
        }
    }
}
