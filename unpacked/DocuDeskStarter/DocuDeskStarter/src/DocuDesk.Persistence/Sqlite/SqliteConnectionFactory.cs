using Microsoft.Data.Sqlite;
using System.Data;

namespace DocuDesk.Persistence.Sqlite;

public sealed class SqliteConnectionFactory : ISqliteConnectionFactory
{
    private readonly string _connectionString;

    public SqliteConnectionFactory(string connectionString)
    {
        _connectionString = connectionString;
    }

    public async Task<IDbConnection> OpenConnectionAsync(CancellationToken ct)
    {
        var connection = new SqliteConnection(_connectionString);
        await connection.OpenAsync(ct);
        await EnsurePragmasAsync(connection, ct);
        return connection;
    }

    private static async Task EnsurePragmasAsync(SqliteConnection connection, CancellationToken ct)
    {
        var cmd = connection.CreateCommand();
        cmd.CommandText = "PRAGMA foreign_keys = ON; PRAGMA journal_mode = WAL; PRAGMA synchronous = NORMAL;";
        await cmd.ExecuteNonQueryAsync(ct);
    }
}
