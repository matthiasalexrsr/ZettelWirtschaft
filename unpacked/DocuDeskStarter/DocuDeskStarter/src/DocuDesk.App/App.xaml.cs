using DocuDesk.Application.Abstractions.Persistence;
using DocuDesk.Application.Documents;
using DocuDesk.App.Services;
using DocuDesk.App.ViewModels;
using DocuDesk.Infrastructure.DependencyInjection;
using DocuDesk.Persistence.DependencyInjection;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.UI.Xaml;

namespace DocuDesk.App;

public partial class App : Application
{
    public static IHost Host { get; private set; } = default!;
    public static Window? MainWindowInstance { get; private set; }

    public App()
    {
        InitializeComponent();
        Host = BuildHost();
    }

    protected override async void OnLaunched(LaunchActivatedEventArgs args)
    {
        await Host.StartAsync();

        using (var scope = Host.Services.CreateScope())
        {
            var migrator = scope.ServiceProvider.GetRequiredService<IDatabaseMigrator>();
            await migrator.MigrateAsync(CancellationToken.None);
        }

        MainWindowInstance = Host.Services.GetRequiredService<MainWindow>();
        MainWindowInstance.Activate();
    }

    private static IHost BuildHost()
    {
        var repositoryRoot = ResolveRepositoryRoot();
        Directory.CreateDirectory(Path.Combine(repositoryRoot, "db"));
        Directory.CreateDirectory(Path.Combine(repositoryRoot, "files"));
        Directory.CreateDirectory(Path.Combine(repositoryRoot, "derivatives"));
        Directory.CreateDirectory(Path.Combine(repositoryRoot, "logs"));

        var config = new ConfigurationBuilder()
            .AddJsonFile("appsettings.json", optional: true)
            .Build();

        return Microsoft.Extensions.Hosting.Host.CreateDefaultBuilder()
            .ConfigureServices((_, services) =>
            {
                services.AddSingleton<IConfiguration>(config);

                services.AddDocuDeskInfrastructure(
                    configureStorage: options =>
                    {
                        options.RepositoryRoot = repositoryRoot;
                    },
                    configureOcr: options =>
                    {
                        options.TesseractExecutablePath = config["Ocr:TesseractExecutablePath"] ?? options.TesseractExecutablePath;
                        options.PdfToPpmExecutablePath = config["Ocr:PdfToPpmExecutablePath"] ?? options.PdfToPpmExecutablePath;
                        if (int.TryParse(config["Ocr:PdfRasterDpi"], out var dpi) && dpi > 0)
                        {
                            options.PdfRasterDpi = dpi;
                        }
                        options.DefaultLanguage = config["Ocr:DefaultLanguage"] ?? options.DefaultLanguage;
                    });

                services.AddDocuDeskPersistence(options =>
                {
                    options.ConnectionString = $"Data Source={Path.Combine(repositoryRoot, "db", "app.db")};Cache=Shared;Mode=ReadWriteCreate;";
                });

                services.AddTransient<ImportDocumentFromFileHandler>();
                services.AddTransient<UpdateDocumentMetadataHandler>();
                services.AddTransient<SyncDocumentTagsHandler>();
                services.AddTransient<CompleteOcrHandler>();
                services.AddTransient<RunDocumentOcrHandler>();
                services.AddSingleton<AppNavigationService>();
                services.AddSingleton<DocumentWorkspaceState>();

                services.AddSingleton<MainWindow>();
                services.AddSingleton<MainWindowViewModel>();
                services.AddTransient<InboxPageViewModel>();
                services.AddTransient<SearchPageViewModel>();
                services.AddTransient<ViewerPageViewModel>();

                services.AddTransient<Views.InboxPage>();
                services.AddTransient<Views.SearchPage>();
                services.AddTransient<Views.ViewerPage>();
            })
            .Build();
    }

    private static string ResolveRepositoryRoot()
    {
        return Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData),
            "DocuDesk");
    }
}
