using DocuDesk.Application.Abstractions.Persistence;
using DocuDesk.Application.Interfaces;
using DocuDesk.Persistence.DependencyInjection;
using DocuDesk.Persistence.Sqlite;

namespace DocuDesk.Persistence.Tests.DependencyInjection;

public sealed class ServiceCollectionExtensionsTests
{
    [Fact]
    public async Task AddDocuDeskPersistence_RegistersPersistenceServicesAndResolvesMigrator()
    {
        using var scope = new TemporaryDirectoryScope();
        var databasePath = Path.Combine(scope.Path, "db", "docudesk.db");
        IServiceCollection services = new ServiceCollection();

        var returnedServices = services.AddDocuDeskPersistence(databasePath);

        returnedServices.Should().BeSameAs(services);
        services.Should().ContainSingle(descriptor =>
            descriptor.ServiceType == typeof(SqliteConnectionFactory) &&
            descriptor.ImplementationInstance is SqliteConnectionFactory &&
            descriptor.Lifetime == ServiceLifetime.Singleton);
        services.Should().ContainSingle(descriptor =>
            descriptor.ServiceType == typeof(ISqliteConnectionFactory) &&
            descriptor.ImplementationInstance is SqliteConnectionFactory &&
            descriptor.Lifetime == ServiceLifetime.Singleton);
        services.Should().ContainSingle(descriptor =>
            descriptor.ServiceType == typeof(IDatabaseMigrator) &&
            descriptor.ImplementationFactory is not null &&
            descriptor.Lifetime == ServiceLifetime.Singleton);
        services.Should().ContainSingle(descriptor =>
            descriptor.ServiceType == typeof(IDocumentRepository) &&
            descriptor.Lifetime == ServiceLifetime.Singleton);
        services.Should().ContainSingle(descriptor =>
            descriptor.ServiceType == typeof(ISearchIndex) &&
            descriptor.Lifetime == ServiceLifetime.Singleton);
        services.Should().ContainSingle(descriptor =>
            descriptor.ServiceType == typeof(IJobStore) &&
            descriptor.Lifetime == ServiceLifetime.Singleton);
        services.Should().ContainSingle(descriptor =>
            descriptor.ServiceType == typeof(IAuditLog) &&
            descriptor.Lifetime == ServiceLifetime.Singleton);

        using var provider = services.BuildServiceProvider();
        var migrator = provider.GetRequiredService<IDatabaseMigrator>();

        await migrator.MigrateAsync(CancellationToken.None);

        File.Exists(databasePath).Should().BeTrue();
    }
}
