using Microsoft.Data.Sqlite;

namespace DocuDesk.Persistence.Sqlite;

public interface ISqliteConnectionFactory
{
    SqliteConnection CreateOpenConnection();
}
