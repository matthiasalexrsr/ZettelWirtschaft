namespace DocuDesk.Application.Abstractions.Persistence;

public interface IDatabaseMigrator
{
    Task MigrateAsync(CancellationToken ct);
}
