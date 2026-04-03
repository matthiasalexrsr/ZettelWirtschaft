using System.Windows;
using DocuDesk.Application;
using DocuDesk.Application.Interfaces;
using DocuDesk.Application.Services;
using DocuDesk.Desktop.ViewModels;
using DocuDesk.Infrastructure.Backup;
using DocuDesk.Infrastructure.Imaging;
using DocuDesk.Infrastructure.Ocr;
using DocuDesk.Infrastructure.Settings;
using DocuDesk.Infrastructure.Storage;
using DocuDesk.Integrations.Mail;
using DocuDesk.Persistence.Repositories;
using DocuDesk.Persistence.Sqlite;
using DocuDesk.Worker;
using Microsoft.Extensions.DependencyInjection;

namespace DocuDesk.Desktop;

public partial class App : Application
{
    public static ServiceProvider Services { get; private set; } = default!;

    protected override async void OnStartup(StartupEventArgs e)
    {
        base.OnStartup(e);

        var appRoot = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "DocuDesk");
        var settingsFile = Path.Combine(appRoot, "settings.json");
        var settingsStore = new JsonSettingsStore(settingsFile);
        var settings = await settingsStore.LoadAsync();

        Directory.CreateDirectory(settings.AppDataRoot);
        Directory.CreateDirectory(settings.RepositoryRoot);
        Directory.CreateDirectory(Path.GetDirectoryName(settings.DatabasePath)!);
        Directory.CreateDirectory(settings.BackupsPath);
        Directory.CreateDirectory(settings.LogsPath);
        Directory.CreateDirectory(settings.CacheRoot);
        Directory.CreateDirectory(settings.OcrArtifactsRoot);

        var services = new ServiceCollection();
        services.AddSingleton(settings);
        services.AddSingleton<ISettingsStore>(settingsStore);
        services.AddSingleton(new SqliteConnectionFactory(settings.DatabasePath));
        services.AddSingleton(provider => new DatabaseInitializer(provider.GetRequiredService<SqliteConnectionFactory>(), Path.Combine(AppContext.BaseDirectory, "database", "ddl_v1.sql")));
        services.AddSingleton<SqliteDocumentRepository>();
        services.AddSingleton<IDocumentRepository>(provider => provider.GetRequiredService<SqliteDocumentRepository>());
        services.AddSingleton<ISearchIndex>(provider => provider.GetRequiredService<SqliteDocumentRepository>());
        services.AddSingleton<IJobStore, SqliteJobStore>();
        services.AddSingleton<IAuditLog, SqliteAuditLog>();
        services.AddSingleton<IDocumentBinaryStore>(new FileSystemDocumentBinaryStore(settings.RepositoryRoot));
        services.AddSingleton<IOcrEngine, TesseractCliOcrEngine>();
        services.AddSingleton<IThumbnailService, ImageSharpThumbnailService>();
        services.AddSingleton<IBackupService>(new FileSystemBackupService(settings.DatabasePath, settings.BackupsPath));
        services.AddSingleton<IMailClientAdapter, ThunderbirdShellAdapter>();
        services.AddSingleton<IImportService, DocumentImportService>();
        services.AddSingleton<SearchService>();
        services.AddSingleton<DocumentProcessingWorker>();
        services.AddSingleton<JobScheduler>();
        services.AddSingleton<MainViewModel>();
        services.AddSingleton<MainWindow>();

        Services = services.BuildServiceProvider();

        var initializer = Services.GetRequiredService<DatabaseInitializer>();
        await initializer.InitializeAsync();

        var mainWindow = Services.GetRequiredService<MainWindow>();
        mainWindow.DataContext = Services.GetRequiredService<MainViewModel>();
        mainWindow.Show();
    }
}
