using DocuDesk.Application;
using DocuDesk.Application.Abstractions.Services;
using DocuDesk.Application.Interfaces;
using DocuDesk.Infrastructure.Backup;
using DocuDesk.Infrastructure.DependencyInjection;
using DocuDesk.Infrastructure.Files;
using DocuDesk.Infrastructure.Ids;
using DocuDesk.Infrastructure.Ocr;
using DocuDesk.Infrastructure.Security;
using DocuDesk.Infrastructure.Storage;
using DocuDesk.Infrastructure.Time;
using Microsoft.Extensions.Options;

namespace DocuDesk.Infrastructure.Tests.DependencyInjection;

public sealed class ServiceCollectionExtensionsTests
{
    [Fact]
    public void AddDocuDeskInfrastructure_RegistersCoreServicesAsSingletonsAndConfiguresOptions()
    {
        using var scope = new TemporaryDirectoryScope();
        var settings = new AppSettings
        {
            RepositoryRoot = Path.Combine(scope.Path, "repository"),
            DatabasePath = Path.Combine(scope.Path, "db", "app.db"),
            BackupsPath = Path.Combine(scope.Path, "backups"),
            TesseractExecutablePath = "custom-tesseract",
            OcrLanguage = "eng"
        };

        IServiceCollection services = new ServiceCollection();

        var returnedServices = services.AddDocuDeskInfrastructure(settings);

        returnedServices.Should().BeSameAs(services);
        services.Should().ContainSingle(descriptor =>
            descriptor.ServiceType == typeof(AppSettings) &&
            descriptor.ImplementationInstance == settings &&
            descriptor.Lifetime == ServiceLifetime.Singleton);
        services.Should().ContainSingle(descriptor =>
            descriptor.ServiceType == typeof(IClock) &&
            descriptor.ImplementationType == typeof(SystemClock) &&
            descriptor.Lifetime == ServiceLifetime.Singleton);
        services.Should().ContainSingle(descriptor =>
            descriptor.ServiceType == typeof(IIdGenerator) &&
            descriptor.ImplementationType == typeof(GuidIdGenerator) &&
            descriptor.Lifetime == ServiceLifetime.Singleton);
        services.Should().ContainSingle(descriptor =>
            descriptor.ServiceType == typeof(IHashService) &&
            descriptor.ImplementationType == typeof(Sha256HashService) &&
            descriptor.Lifetime == ServiceLifetime.Singleton);
        services.Should().ContainSingle(descriptor =>
            descriptor.ServiceType == typeof(IDocumentStorageService) &&
            descriptor.ImplementationType == typeof(LocalDocumentStorageService) &&
            descriptor.Lifetime == ServiceLifetime.Singleton);
        services.Should().ContainSingle(descriptor =>
            descriptor.ServiceType == typeof(IDocumentImportInspector) &&
            descriptor.ImplementationType == typeof(SimpleDocumentImportInspector) &&
            descriptor.Lifetime == ServiceLifetime.Singleton);
        services.Should().ContainSingle(descriptor =>
            descriptor.ServiceType == typeof(IDocumentBinaryStore) &&
            descriptor.ImplementationInstance is FileSystemDocumentBinaryStore &&
            descriptor.Lifetime == ServiceLifetime.Singleton);
        services.Should().ContainSingle(descriptor =>
            descriptor.ServiceType == typeof(IBackupService) &&
            descriptor.ImplementationInstance is FileSystemBackupService &&
            descriptor.Lifetime == ServiceLifetime.Singleton);

        using var provider = services.BuildServiceProvider();

        var storageOptions = provider.GetRequiredService<IOptions<RepositoryStorageOptions>>().Value;
        storageOptions.RepositoryRoot.Should().Be(settings.RepositoryRoot);

        var ocrOptions = provider.GetRequiredService<IOptions<TesseractOcrOptions>>().Value;
        ocrOptions.TesseractExecutablePath.Should().Be("custom-tesseract");
        ocrOptions.DefaultLanguage.Should().Be("eng");
    }
}
