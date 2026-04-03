using DocuDesk.Application.Abstractions.Persistence;
using DocuDesk.Persistence.Sqlite;
using DocuDesk.Persistence.Sqlite.Repositories;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using System.Reflection;

namespace DocuDesk.Persistence.DependencyInjection;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddDocuDeskPersistence(
        this IServiceCollection services,
        Action<SqliteOptions> configure)
    {
        services.Configure(configure);

        services.AddSingleton<ISqliteConnectionFactory>(sp =>
        {
            var options = sp.GetRequiredService<IOptions<SqliteOptions>>().Value;
            return new SqliteConnectionFactory(options.ConnectionString);
        });

        services.AddSingleton<IDatabaseMigrator>(sp =>
            new EmbeddedSqliteMigrator(
                sp.GetRequiredService<ISqliteConnectionFactory>(),
                typeof(ServiceCollectionExtensions).Assembly));

        services.AddScoped<IDocumentRepository, SqliteDocumentRepository>();
        services.AddScoped<IDocumentContentRepository, SqliteDocumentContentRepository>();
        services.AddScoped<IAnnotationRepository, SqliteAnnotationRepository>();
        services.AddScoped<ICategoryRepository, SqliteCategoryRepository>();
        services.AddScoped<ITagRepository, SqliteTagRepository>();
        services.AddScoped<ISearchRepository, SqliteSearchRepository>();
        services.AddScoped<IBackgroundJobRepository, SqliteBackgroundJobRepository>();
        services.AddScoped<IMailJobRepository, SqliteMailJobRepository>();
        services.AddScoped<IAuditRepository, SqliteAuditRepository>();

        return services;
    }
}
