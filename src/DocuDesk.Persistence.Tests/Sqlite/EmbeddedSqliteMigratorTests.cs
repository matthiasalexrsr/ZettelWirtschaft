using DocuDesk.Persistence.DependencyInjection;
using DocuDesk.Persistence.Sqlite;

namespace DocuDesk.Persistence.Tests.Sqlite;

public sealed class EmbeddedSqliteMigratorTests
{
    [Fact]
    public async Task MigrateAsync_CreatesSchemaTablesAndTracksAppliedMigration()
    {
        using var scope = new TemporaryDirectoryScope();
        var databasePath = Path.Combine(scope.Path, "db", "docudesk.db");
        var factory = new SqliteConnectionFactory(databasePath);
        var sut = new EmbeddedSqliteMigrator(factory, typeof(ServiceCollectionExtensions).Assembly);

        await sut.MigrateAsync(CancellationToken.None);

        await using var connection = factory.CreateOpenConnection();

        var hasSchemaMigrationsTable = await TableExistsAsync(connection, "__schema_migrations");
        var hasDocumentsTable = await TableExistsAsync(connection, "documents");
        var hasFullTextTable = await TableExistsAsync(connection, "document_fts");
        var appliedMigrationCount = await ScalarAsync<long>(connection, "SELECT COUNT(*) FROM __schema_migrations;");
        var appliedVersion = await ScalarAsync<string>(connection, "SELECT version FROM __schema_migrations LIMIT 1;");

        hasSchemaMigrationsTable.Should().BeTrue();
        hasDocumentsTable.Should().BeTrue();
        hasFullTextTable.Should().BeTrue();
        appliedMigrationCount.Should().Be(1);
        appliedVersion.Should().Be("001_initial_schema");
    }

    [Fact]
    public async Task MigrateAsync_IsIdempotentAcrossMultipleRuns()
    {
        using var scope = new TemporaryDirectoryScope();
        var databasePath = Path.Combine(scope.Path, "db", "docudesk.db");
        var factory = new SqliteConnectionFactory(databasePath);
        var sut = new EmbeddedSqliteMigrator(factory, typeof(ServiceCollectionExtensions).Assembly);

        await sut.MigrateAsync(CancellationToken.None);
        await sut.MigrateAsync(CancellationToken.None);

        await using var connection = factory.CreateOpenConnection();
        var appliedMigrationCount = await ScalarAsync<long>(connection, "SELECT COUNT(*) FROM __schema_migrations;");

        appliedMigrationCount.Should().Be(1);
    }

    private static async Task<bool> TableExistsAsync(Microsoft.Data.Sqlite.SqliteConnection connection, string tableName)
    {
        await using var command = connection.CreateCommand();
        command.CommandText = "SELECT COUNT(*) FROM sqlite_master WHERE name = $name;";
        command.Parameters.AddWithValue("$name", tableName);

        return await command.ExecuteScalarAsync() is long count && count == 1;
    }

    private static async Task<T> ScalarAsync<T>(Microsoft.Data.Sqlite.SqliteConnection connection, string sql)
    {
        await using var command = connection.CreateCommand();
        command.CommandText = sql;
        var result = await command.ExecuteScalarAsync();

        return (T)result!;
    }
}
