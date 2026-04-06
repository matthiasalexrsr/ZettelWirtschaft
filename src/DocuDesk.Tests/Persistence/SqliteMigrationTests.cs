using DocuDesk.Application.Abstractions.Persistence;
using DocuDesk.Persistence.DependencyInjection;
using Microsoft.Data.Sqlite;
using Microsoft.Extensions.DependencyInjection;

namespace DocuDesk.Tests.Persistence;

public sealed class SqliteMigrationTests
{
    [Fact]
    public async Task MigrateAsync_CreatesMigrationTrackingTable_AndAppliesScripts()
    {
        using var temp = new TempDirectory();
        var dbPath = Path.Combine(temp.Path, "db", "app.db");

        var services = new ServiceCollection();
        services.AddDocuDeskPersistence(dbPath);
        using var provider = services.BuildServiceProvider();

        var migrator = provider.GetRequiredService<IDatabaseMigrator>();
        await migrator.MigrateAsync(CancellationToken.None);

        await using var con = new SqliteConnection($"Data Source={dbPath}");
        await con.OpenAsync();

        await using var countCmd = con.CreateCommand();
        countCmd.CommandText = "SELECT COUNT(*) FROM __schema_migrations;";
        var migrationCount = Convert.ToInt32(await countCmd.ExecuteScalarAsync());

        await using var tableCmd = con.CreateCommand();
        tableCmd.CommandText = "SELECT COUNT(*) FROM sqlite_master WHERE type='table' AND name='documents';";
        var documentsTableCount = Convert.ToInt32(await tableCmd.ExecuteScalarAsync());

        Assert.True(migrationCount >= 1);
        Assert.Equal(1, documentsTableCount);
    }

    [Fact]
    public async Task MigrateAsync_IsIdempotent_WhenRunTwice()
    {
        using var temp = new TempDirectory();
        var dbPath = Path.Combine(temp.Path, "db", "app.db");

        var services = new ServiceCollection();
        services.AddDocuDeskPersistence(dbPath);
        using var provider = services.BuildServiceProvider();
        var migrator = provider.GetRequiredService<IDatabaseMigrator>();

        await migrator.MigrateAsync(CancellationToken.None);
        await migrator.MigrateAsync(CancellationToken.None);

        await using var con = new SqliteConnection($"Data Source={dbPath}");
        await con.OpenAsync();

        await using var cmd = con.CreateCommand();
        cmd.CommandText = "SELECT COUNT(*) FROM __schema_migrations;";
        var migrationCount = Convert.ToInt32(await cmd.ExecuteScalarAsync());

        Assert.True(migrationCount >= 1);
    }

    private sealed class TempDirectory : IDisposable
    {
        public TempDirectory()
        {
            Path = System.IO.Path.Combine(System.IO.Path.GetTempPath(), $"docudesk-tests-{Guid.NewGuid():N}");
            Directory.CreateDirectory(Path);
        }

        public string Path { get; }

        public void Dispose()
        {
            try
            {
                Directory.Delete(Path, recursive: true);
            }
            catch
            {
                // Best-effort cleanup.
            }
        }
    }
}
