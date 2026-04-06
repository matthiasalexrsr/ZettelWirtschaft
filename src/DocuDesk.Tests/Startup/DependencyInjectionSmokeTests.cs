using DocuDesk.Application;
using DocuDesk.Application.Abstractions.Persistence;
using DocuDesk.Application.Interfaces;
using DocuDesk.Infrastructure.DependencyInjection;
using DocuDesk.Persistence.DependencyInjection;
using Microsoft.Extensions.DependencyInjection;

namespace DocuDesk.Tests.Startup;

public sealed class DependencyInjectionSmokeTests
{
    [Fact]
    public async Task ServiceComposition_ResolvesCoreServices_AndCanMigrateDatabase()
    {
        using var temp = new TempDirectory();

        var settings = new AppSettings
        {
            AppDataRoot = Path.Combine(temp.Path, "app"),
            RepositoryRoot = Path.Combine(temp.Path, "repo"),
            DatabasePath = Path.Combine(temp.Path, "db", "app.db"),
            BackupsPath = Path.Combine(temp.Path, "backups"),
            LogsPath = Path.Combine(temp.Path, "logs"),
            CacheRoot = Path.Combine(temp.Path, "cache"),
            OcrArtifactsRoot = Path.Combine(temp.Path, "ocr"),
            TesseractExecutablePath = "tesseract",
            OcrLanguage = "deu+eng"
        };

        Directory.CreateDirectory(settings.RepositoryRoot);
        Directory.CreateDirectory(Path.GetDirectoryName(settings.DatabasePath)!);
        Directory.CreateDirectory(settings.BackupsPath);
        Directory.CreateDirectory(settings.LogsPath);

        var services = new ServiceCollection();
        services.AddDocuDeskInfrastructure(settings);
        services.AddDocuDeskPersistence(settings.DatabasePath);

        using var provider = services.BuildServiceProvider();

        _ = provider.GetRequiredService<IDocumentRepository>();
        _ = provider.GetRequiredService<IBackupService>();
        _ = provider.GetRequiredService<IMailClientAdapter>();
        _ = provider.GetRequiredService<IDatabaseMigrator>();

        var migrator = provider.GetRequiredService<IDatabaseMigrator>();
        await migrator.MigrateAsync(CancellationToken.None);
    }

    private sealed class TempDirectory : IDisposable
    {
        public TempDirectory()
        {
            Path = System.IO.Path.Combine(System.IO.Path.GetTempPath(), $"docudesk-tests-{Guid.NewGuid():N}");
            Directory.CreateDirectory(Path);
        }

        public string Path { get; }

        public void Dispose()
        {
            try
            {
                Directory.Delete(Path, recursive: true);
            }
            catch
            {
                // Best-effort cleanup.
            }
        }
    }
}
