using Dapper;
using DocuDesk.Application.Abstractions.Persistence;
using System.Reflection;

namespace DocuDesk.Persistence.Sqlite;

public sealed class EmbeddedSqliteMigrator : IDatabaseMigrator
{
    private readonly ISqliteConnectionFactory _connectionFactory;
    private readonly Assembly _assembly;

    public EmbeddedSqliteMigrator(ISqliteConnectionFactory connectionFactory, Assembly assembly)
    {
        _connectionFactory = connectionFactory;
        _assembly = assembly;
    }

    public async Task MigrateAsync(CancellationToken ct)
    {
        using var con = await _connectionFactory.OpenConnectionAsync(ct);

        const string ensureMigrations = """
            CREATE TABLE IF NOT EXISTS __schema_migrations (
                version TEXT PRIMARY KEY,
                applied_utc TEXT NOT NULL
            );
            """;

        await con.ExecuteAsync(new CommandDefinition(ensureMigrations, cancellationToken: ct));

        var applied = (await con.QueryAsync<string>(
            new CommandDefinition("SELECT version FROM __schema_migrations;", cancellationToken: ct)))
            .ToHashSet(StringComparer.OrdinalIgnoreCase);

        var resources = _assembly.GetManifestResourceNames()
            .Where(x => x.Contains(".Migrations.", StringComparison.OrdinalIgnoreCase) && x.EndsWith(".sql", StringComparison.OrdinalIgnoreCase))
            .OrderBy(x => x, StringComparer.OrdinalIgnoreCase)
            .ToList();

        foreach (var resource in resources)
        {
            var version = resource[(resource.LastIndexOf(".Migrations.", StringComparison.OrdinalIgnoreCase) + ".Migrations.".Length)..];
            version = version[..^4];

            if (applied.Contains(version))
            {
                continue;
            }

            await using var stream = _assembly.GetManifestResourceStream(resource)
                ?? throw new InvalidOperationException($"Migration resource '{resource}' not found.");
            using var reader = new StreamReader(stream);
            var sql = await reader.ReadToEndAsync(ct);

            using var tx = con.BeginTransaction();
            await con.ExecuteAsync(sql, transaction: tx);
            await con.ExecuteAsync(
                "INSERT INTO __schema_migrations(version, applied_utc) VALUES (@version, @appliedUtc);",
                new { version, appliedUtc = DateTimeOffset.UtcNow.UtcDateTime },
                tx);
            tx.Commit();
        }
    }
}
