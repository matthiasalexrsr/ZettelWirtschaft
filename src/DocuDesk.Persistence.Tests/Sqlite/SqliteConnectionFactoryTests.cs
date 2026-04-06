using System.Data;
using DocuDesk.Persistence.Sqlite;

namespace DocuDesk.Persistence.Tests.Sqlite;

public sealed class SqliteConnectionFactoryTests
{
    [Fact]
    public async Task CreateOpenConnection_CreatesDatabaseDirectoryAndReturnsOpenConnection()
    {
        using var scope = new TemporaryDirectoryScope();
        var databasePath = Path.Combine(scope.Path, "data", "docudesk.db");
        var sut = new SqliteConnectionFactory(databasePath);

        Directory.Exists(Path.GetDirectoryName(databasePath)!).Should().BeTrue();

        await using var connection = sut.CreateOpenConnection();
        connection.State.Should().Be(ConnectionState.Open);

        await using var command = connection.CreateCommand();
        command.CommandText = "CREATE TABLE sample(id INTEGER PRIMARY KEY);";
        await command.ExecuteNonQueryAsync();

        File.Exists(databasePath).Should().BeTrue();
    }
}
