using Dapper;
using DocuDesk.Application.Abstractions.Persistence;
using DocuDesk.Domain.Entities;
using DocuDesk.Persistence.Sqlite.Mapping;

namespace DocuDesk.Persistence.Sqlite.Repositories;

public sealed class SqliteBackgroundJobRepository : IBackgroundJobRepository
{
    private readonly ISqliteConnectionFactory _connectionFactory;

    public SqliteBackgroundJobRepository(ISqliteConnectionFactory connectionFactory)
    {
        _connectionFactory = connectionFactory;
    }

    public async Task EnqueueAsync(BackgroundJob job, CancellationToken ct)
    {
        const string sql = """
            INSERT INTO background_jobs (
                id, job_type, related_document_id, payload_json, state,
                priority, attempts, max_attempts, scheduled_utc,
                started_utc, completed_utc, error_text, created_utc
            ) VALUES (
                @Id, @JobType, @RelatedDocumentId, @PayloadJson, @State,
                @Priority, @Attempts, @MaxAttempts, @ScheduledUtc,
                @StartedUtc, @CompletedUtc, @ErrorText, @CreatedUtc
            );
            """;

        using var con = await _connectionFactory.OpenConnectionAsync(ct);
        await con.ExecuteAsync(new CommandDefinition(sql, new
        {
            job.Id,
            JobType = job.JobType.ToDb(),
            job.RelatedDocumentId,
            job.PayloadJson,
            State = job.State.ToDb(),
            job.Priority,
            job.Attempts,
            job.MaxAttempts,
            ScheduledUtc = job.ScheduledUtc.UtcDateTime,
            StartedUtc = job.StartedUtc?.UtcDateTime,
            CompletedUtc = job.CompletedUtc?.UtcDateTime,
            job.ErrorText,
            CreatedUtc = job.CreatedUtc.UtcDateTime
        }, cancellationToken: ct));
    }

    public async Task<BackgroundJob?> TryAcquireNextAsync(CancellationToken ct)
    {
        const string sql = """
            SELECT
                id AS Id,
                job_type AS JobType,
                related_document_id AS RelatedDocumentId,
                payload_json AS PayloadJson,
                state AS State,
                priority AS Priority,
                attempts AS Attempts,
                max_attempts AS MaxAttempts,
                scheduled_utc AS ScheduledUtc,
                started_utc AS StartedUtc,
                completed_utc AS CompletedUtc,
                error_text AS ErrorText,
                created_utc AS CreatedUtc
            FROM background_jobs
            WHERE state = 'queued'
              AND scheduled_utc <= CURRENT_TIMESTAMP
            ORDER BY priority ASC, scheduled_utc ASC
            LIMIT 1;
            """;

        using var con = await _connectionFactory.OpenConnectionAsync(ct);
        var row = await con.QuerySingleOrDefaultAsync<BackgroundJobRow>(new CommandDefinition(sql, cancellationToken: ct));
        return row?.ToDomain();
    }

    public Task MarkRunningAsync(string jobId, DateTimeOffset startedUtc, CancellationToken ct)
        => UpdateStateAsync(jobId, "running", startedUtc: startedUtc, ct: ct);

    public Task MarkCompletedAsync(string jobId, DateTimeOffset completedUtc, CancellationToken ct)
        => UpdateStateAsync(jobId, "completed", completedUtc: completedUtc, ct: ct);

    public Task MarkFailedAsync(string jobId, string errorText, DateTimeOffset failedUtc, CancellationToken ct)
        => UpdateStateAsync(jobId, "failed", failedUtc, errorText, ct);

    private async Task UpdateStateAsync(
        string jobId,
        string state,
        DateTimeOffset? startedUtc = null,
        DateTimeOffset? completedUtc = null,
        string? errorText = null,
        CancellationToken ct = default)
    {
        const string sql = """
            UPDATE background_jobs
               SET state = @state,
                   started_utc = COALESCE(@startedUtc, started_utc),
                   completed_utc = COALESCE(@completedUtc, completed_utc),
                   error_text = @errorText,
                   attempts = CASE WHEN @state = 'failed' THEN attempts + 1 ELSE attempts END
             WHERE id = @jobId;
            """;

        using var con = await _connectionFactory.OpenConnectionAsync(ct);
        await con.ExecuteAsync(new CommandDefinition(sql, new
        {
            jobId,
            state,
            startedUtc = startedUtc?.UtcDateTime,
            completedUtc = completedUtc?.UtcDateTime,
            errorText
        }, cancellationToken: ct));
    }
}
