using DocuDesk.Application.Abstractions.Persistence;
using DocuDesk.Application.Interfaces;
using DocuDesk.Persistence.Repositories;
using DocuDesk.Persistence.Sqlite;
using Microsoft.Extensions.DependencyInjection;

namespace DocuDesk.Persistence.DependencyInjection;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddDocuDeskPersistence(
        this IServiceCollection services,
        string databasePath)
    {
        var factory = new SqliteConnectionFactory(databasePath);
        services.AddSingleton<SqliteConnectionFactory>(factory);
        services.AddSingleton<ISqliteConnectionFactory>(factory);

        services.AddSingleton<IDatabaseMigrator>(sp =>
            new EmbeddedSqliteMigrator(
                sp.GetRequiredService<ISqliteConnectionFactory>(),
                typeof(ServiceCollectionExtensions).Assembly));

        services.AddSingleton<SqliteDocumentRepository>();
        services.AddSingleton<IDocumentRepository>(sp => sp.GetRequiredService<SqliteDocumentRepository>());
        services.AddSingleton<ISearchIndex>(sp => sp.GetRequiredService<SqliteDocumentRepository>());
        services.AddSingleton<IJobStore, SqliteJobStore>();
        services.AddSingleton<IAuditLog, SqliteAuditLog>();

        return services;
    }
}
