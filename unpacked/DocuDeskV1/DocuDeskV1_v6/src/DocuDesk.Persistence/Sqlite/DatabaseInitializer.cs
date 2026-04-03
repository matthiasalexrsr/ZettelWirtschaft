using Microsoft.Data.Sqlite;

namespace DocuDesk.Persistence.Sqlite;

public sealed class DatabaseInitializer
{
    private readonly SqliteConnectionFactory _connectionFactory;
    private readonly string _ddlPath;

    public DatabaseInitializer(SqliteConnectionFactory connectionFactory, string ddlPath)
    {
        _connectionFactory = connectionFactory;
        _ddlPath = ddlPath;
    }

    public async Task InitializeAsync(CancellationToken cancellationToken = default)
    {
        var ddl = await File.ReadAllTextAsync(_ddlPath, cancellationToken);
        await using var connection = _connectionFactory.CreateOpenConnection();
        await using var command = connection.CreateCommand();
        command.CommandText = ddl;
        await command.ExecuteNonQueryAsync(cancellationToken);
    }
}
