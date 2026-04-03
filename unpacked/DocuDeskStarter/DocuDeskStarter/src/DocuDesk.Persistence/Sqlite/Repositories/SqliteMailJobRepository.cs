using Dapper;
using DocuDesk.Application.Abstractions.Persistence;
using DocuDesk.Domain.Entities;
using DocuDesk.Persistence.Sqlite.Mapping;

namespace DocuDesk.Persistence.Sqlite.Repositories;

public sealed class SqliteMailJobRepository : IMailJobRepository
{
    private readonly ISqliteConnectionFactory _connectionFactory;

    public SqliteMailJobRepository(ISqliteConnectionFactory connectionFactory)
    {
        _connectionFactory = connectionFactory;
    }

    public async Task InsertAsync(MailJob job, IReadOnlyList<MailJobDocument> documents, CancellationToken ct)
    {
        using var con = await _connectionFactory.OpenConnectionAsync(ct);
        using var tx = con.BeginTransaction();

        await con.ExecuteAsync("""
            INSERT INTO mail_jobs (
                id, target_client, direction, request_json, status,
                external_reference, error_text, created_utc, completed_utc
            ) VALUES (
                @Id, @TargetClient, @Direction, @RequestJson, @Status,
                @ExternalReference, @ErrorText, @CreatedUtc, @CompletedUtc
            );
            """, new
        {
            job.Id,
            job.TargetClient,
            Direction = job.Direction.ToDb(),
            job.RequestJson,
            Status = job.Status.ToDb(),
            job.ExternalReference,
            job.ErrorText,
            CreatedUtc = job.CreatedUtc.UtcDateTime,
            CompletedUtc = job.CompletedUtc?.UtcDateTime
        }, tx);

        foreach (var doc in documents)
        {
            await con.ExecuteAsync("""
                INSERT INTO mail_job_documents (
                    mail_job_id, document_id, export_artifact_path, attachment_role
                ) VALUES (
                    @MailJobId, @DocumentId, @ExportArtifactPath, @AttachmentRole
                );
                """, new
            {
                doc.MailJobId,
                doc.DocumentId,
                doc.ExportArtifactPath,
                AttachmentRole = doc.AttachmentRole.ToDb()
            }, tx);
        }

        tx.Commit();
    }

    public async Task<MailJob?> GetAsync(string mailJobId, CancellationToken ct)
    {
        const string sql = """
            SELECT
                id AS Id,
                target_client AS TargetClient,
                direction AS Direction,
                request_json AS RequestJson,
                status AS Status,
                external_reference AS ExternalReference,
                error_text AS ErrorText,
                created_utc AS CreatedUtc,
                completed_utc AS CompletedUtc
            FROM mail_jobs
            WHERE id = @mailJobId;
            """;

        using var con = await _connectionFactory.OpenConnectionAsync(ct);
        var row = await con.QuerySingleOrDefaultAsync<MailJobRow>(new CommandDefinition(sql, new { mailJobId }, cancellationToken: ct));
        return row?.ToDomain();
    }

    public async Task MarkDispatchedAsync(string mailJobId, string externalReference, CancellationToken ct)
    {
        const string sql = """
            UPDATE mail_jobs
               SET status = 'dispatched',
                   external_reference = @externalReference
             WHERE id = @mailJobId;
            """;

        using var con = await _connectionFactory.OpenConnectionAsync(ct);
        await con.ExecuteAsync(new CommandDefinition(sql, new { mailJobId, externalReference }, cancellationToken: ct));
    }

    public async Task MarkCompletedAsync(string mailJobId, DateTimeOffset completedUtc, CancellationToken ct)
    {
        const string sql = """
            UPDATE mail_jobs
               SET status = 'completed',
                   completed_utc = @completedUtc
             WHERE id = @mailJobId;
            """;

        using var con = await _connectionFactory.OpenConnectionAsync(ct);
        await con.ExecuteAsync(new CommandDefinition(sql, new { mailJobId, completedUtc = completedUtc.UtcDateTime }, cancellationToken: ct));
    }

    public async Task MarkFailedAsync(string mailJobId, string errorText, CancellationToken ct)
    {
        const string sql = """
            UPDATE mail_jobs
               SET status = 'failed',
                   error_text = @errorText
             WHERE id = @mailJobId;
            """;

        using var con = await _connectionFactory.OpenConnectionAsync(ct);
        await con.ExecuteAsync(new CommandDefinition(sql, new { mailJobId, errorText }, cancellationToken: ct));
    }
}
