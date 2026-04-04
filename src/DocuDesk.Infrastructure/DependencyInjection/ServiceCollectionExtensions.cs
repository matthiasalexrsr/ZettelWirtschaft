using DocuDesk.Application.Abstractions.Services;
using DocuDesk.Application.Interfaces;
using DocuDesk.Infrastructure.Backup;
using DocuDesk.Infrastructure.Files;
using DocuDesk.Infrastructure.Ids;
using DocuDesk.Infrastructure.Imaging;
using DocuDesk.Infrastructure.Ocr;
using DocuDesk.Infrastructure.Security;
using DocuDesk.Infrastructure.Settings;
using DocuDesk.Infrastructure.Storage;
using DocuDesk.Infrastructure.Time;
using DocuDesk.Integrations.Mail;
using Microsoft.Extensions.DependencyInjection;

namespace DocuDesk.Infrastructure.DependencyInjection;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddDocuDeskInfrastructure(
        this IServiceCollection services,
        Application.AppSettings settings)
    {
        services.AddSingleton(settings);

        services.Configure<RepositoryStorageOptions>(options =>
        {
            options.RepositoryRoot = settings.RepositoryRoot;
        });
        services.Configure<TesseractOcrOptions>(options =>
        {
            options.TesseractExecutablePath = settings.TesseractExecutablePath;
            options.DefaultLanguage = settings.OcrLanguage;
        });

        services.AddSingleton<IClock, SystemClock>();
        services.AddSingleton<IIdGenerator, GuidIdGenerator>();
        services.AddSingleton<IHashService, Sha256HashService>();
        services.AddSingleton<IDocumentStorageService, LocalDocumentStorageService>();
        services.AddSingleton<IDocumentImportInspector, SimpleDocumentImportInspector>();
        services.AddSingleton<IOcrEngine, TesseractCliOcrEngine>();
        services.AddSingleton<IThumbnailService, ImageSharpThumbnailService>();
        services.AddSingleton<IDocumentBinaryStore>(new FileSystemDocumentBinaryStore(settings.RepositoryRoot));
        services.AddSingleton<IBackupService>(new FileSystemBackupService(settings.DatabasePath, settings.BackupsPath));
        services.AddSingleton<IMailClientAdapter, ThunderbirdShellAdapter>();

        return services;
    }
}
