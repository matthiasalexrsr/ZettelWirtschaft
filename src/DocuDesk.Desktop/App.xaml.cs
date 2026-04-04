using System.Windows;
using DocuDesk.Application;
using DocuDesk.Application.Abstractions.Persistence;
using DocuDesk.Application.Interfaces;
using DocuDesk.Application.Services;
using DocuDesk.Desktop.ViewModels;
using DocuDesk.Infrastructure.DependencyInjection;
using DocuDesk.Infrastructure.Settings;
using DocuDesk.Persistence.DependencyInjection;
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
        services.AddSingleton<ISettingsStore>(settingsStore);

        services.AddDocuDeskInfrastructure(settings);
        services.AddDocuDeskPersistence(settings.DatabasePath);

        services.AddSingleton<IImportService, DocumentImportService>();
        services.AddSingleton<SearchService>();
        services.AddSingleton<DocumentProcessingWorker>();
        services.AddSingleton<JobScheduler>();
        services.AddSingleton<MainViewModel>();
        services.AddSingleton<MainWindow>();

        Services = services.BuildServiceProvider();

        var migrator = Services.GetRequiredService<IDatabaseMigrator>();
        await migrator.MigrateAsync(CancellationToken.None);

        var mainWindow = Services.GetRequiredService<MainWindow>();
        mainWindow.DataContext = Services.GetRequiredService<MainViewModel>();
        mainWindow.Show();
    }
}
