using DocuDesk.Application.Abstractions.Services;
using DocuDesk.Infrastructure.Files;
using DocuDesk.Infrastructure.Ids;
using DocuDesk.Infrastructure.Mail;
using DocuDesk.Infrastructure.Ocr;
using DocuDesk.Infrastructure.Security;
using DocuDesk.Infrastructure.Time;
using Microsoft.Extensions.DependencyInjection;

namespace DocuDesk.Infrastructure.DependencyInjection;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddDocuDeskInfrastructure(
        this IServiceCollection services,
        Action<RepositoryStorageOptions>? configureStorage = null,
        Action<TesseractOcrOptions>? configureOcr = null)
    {
        if (configureStorage is not null)
        {
            services.Configure(configureStorage);
        }
        else
        {
            services.Configure<RepositoryStorageOptions>(_ => { });
        }

        if (configureOcr is not null)
        {
            services.Configure(configureOcr);
        }
        else
        {
            services.Configure<TesseractOcrOptions>(_ => { });
        }

        services.AddSingleton<IClock, SystemClock>();
        services.AddSingleton<IIdGenerator, GuidIdGenerator>();
        services.AddSingleton<IHashService, Sha256HashService>();
        services.AddSingleton<IDocumentStorageService, LocalDocumentStorageService>();
        services.AddSingleton<IDocumentImportInspector, SimpleDocumentImportInspector>();
        services.AddSingleton<IOcrService, TesseractOcrService>();
        services.AddSingleton<IMailClientAdapter, ThunderbirdNativeMailClientAdapter>();
        return services;
    }
}
