using System.Data;

namespace DocuDesk.Persistence.Sqlite;

public interface ISqliteConnectionFactory
{
    Task<IDbConnection> OpenConnectionAsync(CancellationToken ct);
}
