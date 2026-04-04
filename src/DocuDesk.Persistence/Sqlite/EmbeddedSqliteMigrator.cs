using DocuDesk.Application.Abstractions.Persistence;
using Microsoft.Data.Sqlite;
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
        await using var con = _connectionFactory.CreateOpenConnection();

        await using (var ensureCmd = con.CreateCommand())
        {
            ensureCmd.CommandText = """
                CREATE TABLE IF NOT EXISTS __schema_migrations (
                    version TEXT PRIMARY KEY,
                    applied_utc TEXT NOT NULL
                );
                """;
            await ensureCmd.ExecuteNonQueryAsync(ct);
        }

        var applied = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        await using (var queryCmd = con.CreateCommand())
        {
            queryCmd.CommandText = "SELECT version FROM __schema_migrations;";
            await using var reader = await queryCmd.ExecuteReaderAsync(ct);
            while (await reader.ReadAsync(ct))
            {
                applied.Add(reader.GetString(0));
            }
        }

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

            await using var tx = (SqliteTransaction)await con.BeginTransactionAsync(ct);

            await using (var execCmd = con.CreateCommand())
            {
                execCmd.Transaction = tx;
                execCmd.CommandText = sql;
                await execCmd.ExecuteNonQueryAsync(ct);
            }

            await using (var insertCmd = con.CreateCommand())
            {
                insertCmd.Transaction = tx;
                insertCmd.CommandText = "INSERT INTO __schema_migrations(version, applied_utc) VALUES ($version, $applied_utc);";
                insertCmd.Parameters.AddWithValue("$version", version);
                insertCmd.Parameters.AddWithValue("$applied_utc", DateTimeOffset.UtcNow.UtcDateTime.ToString("O"));
                await insertCmd.ExecuteNonQueryAsync(ct);
            }

            await tx.CommitAsync(ct);
        }
    }
}
