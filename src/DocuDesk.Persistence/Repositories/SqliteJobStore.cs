using DocuDesk.Application.Interfaces;
using DocuDesk.Domain.Entities;
using DocuDesk.Domain.Enums;
using DocuDesk.Persistence.Sqlite;

namespace DocuDesk.Persistence.Repositories;

public sealed class SqliteJobStore : IJobStore
{
    private readonly SqliteConnectionFactory _connectionFactory;

    public SqliteJobStore(SqliteConnectionFactory connectionFactory)
    {
        _connectionFactory = connectionFactory;
    }

    public async Task EnqueueAsync(JobRecord job, CancellationToken cancellationToken = default)
    {
        await using var connection = _connectionFactory.CreateOpenConnection();
        await using var command = connection.CreateCommand();
        command.CommandText = @"
INSERT INTO job_records (id, job_type, target_document_id, payload_json, status, error_text, retry_count, created_at, started_at, completed_at, last_updated_at)
VALUES ($id, $job_type, $target_document_id, $payload_json, $status, $error_text, $retry_count, $created_at, $started_at, $completed_at, $last_updated_at);";
        command.Parameters.AddWithValue("$id", job.Id.ToString());
        command.Parameters.AddWithValue("$job_type", job.JobType);
        command.Parameters.AddWithValue("$target_document_id", job.TargetDocumentId?.ToString() ?? (object)DBNull.Value);
        command.Parameters.AddWithValue("$payload_json", (object?)job.PayloadJson ?? DBNull.Value);
        command.Parameters.AddWithValue("$status", job.Status.ToString());
        command.Parameters.AddWithValue("$error_text", (object?)job.ErrorText ?? DBNull.Value);
        command.Parameters.AddWithValue("$retry_count", job.RetryCount);
        command.Parameters.AddWithValue("$created_at", job.CreatedAt.UtcDateTime.ToString("O"));
        command.Parameters.AddWithValue("$started_at", job.StartedAt?.UtcDateTime.ToString("O") ?? (object)DBNull.Value);
        command.Parameters.AddWithValue("$completed_at", job.CompletedAt?.UtcDateTime.ToString("O") ?? (object)DBNull.Value);
        command.Parameters.AddWithValue("$last_updated_at", job.LastUpdatedAt.UtcDateTime.ToString("O"));
        await command.ExecuteNonQueryAsync(cancellationToken);
    }

    public Task<IReadOnlyList<JobRecord>> GetLatestAsync(int take = 50, CancellationToken cancellationToken = default)
        => ReadAsync(@"SELECT id, job_type, target_document_id, payload_json, status, error_text, retry_count, created_at, started_at, completed_at, last_updated_at
                       FROM job_records ORDER BY created_at DESC LIMIT $take;", take, cancellationToken);

    public Task<IReadOnlyList<JobRecord>> GetPendingAsync(int take = 20, CancellationToken cancellationToken = default)
        => ReadAsync(@"SELECT id, job_type, target_document_id, payload_json, status, error_text, retry_count, created_at, started_at, completed_at, last_updated_at
                       FROM job_records WHERE status IN ('Pending', 'RetryScheduled') ORDER BY created_at ASC LIMIT $take;", take, cancellationToken);

    public async Task UpdateStatusAsync(Guid jobId, JobStatus status, string? errorText = null, CancellationToken cancellationToken = default)
    {
        await using var connection = _connectionFactory.CreateOpenConnection();
        await using var command = connection.CreateCommand();
        command.CommandText = @"UPDATE job_records SET status = $status, error_text = $error_text, last_updated_at = $last_updated_at,
                                started_at = CASE WHEN $set_started = 1 THEN $started_at ELSE started_at END,
                                completed_at = CASE WHEN $is_terminal = 1 THEN $completed_at ELSE completed_at END
                                WHERE id = $id;";
        command.Parameters.AddWithValue("$status", status.ToString());
        command.Parameters.AddWithValue("$error_text", (object?)errorText ?? DBNull.Value);
        command.Parameters.AddWithValue("$last_updated_at", DateTimeOffset.UtcNow.UtcDateTime.ToString("O"));
        command.Parameters.AddWithValue("$started_at", DateTimeOffset.UtcNow.UtcDateTime.ToString("O"));
        command.Parameters.AddWithValue("$completed_at", DateTimeOffset.UtcNow.UtcDateTime.ToString("O"));
        command.Parameters.AddWithValue("$set_started", status == JobStatus.Running ? 1 : 0);
        command.Parameters.AddWithValue("$is_terminal", status is JobStatus.Succeeded or JobStatus.Failed or JobStatus.Cancelled ? 1 : 0);
        command.Parameters.AddWithValue("$id", jobId.ToString());
        await command.ExecuteNonQueryAsync(cancellationToken);
    }

    private async Task<IReadOnlyList<JobRecord>> ReadAsync(string sql, int take, CancellationToken cancellationToken)
    {
        var items = new List<JobRecord>();
        await using var connection = _connectionFactory.CreateOpenConnection();
        await using var command = connection.CreateCommand();
        command.CommandText = sql;
        command.Parameters.AddWithValue("$take", take);
        await using var reader = await command.ExecuteReaderAsync(cancellationToken);
        while (await reader.ReadAsync(cancellationToken))
        {
            items.Add(new JobRecord
            {
                Id = Guid.Parse(reader.GetString(0)),
                JobType = reader.GetString(1),
                TargetDocumentId = reader.IsDBNull(2) ? null : Guid.Parse(reader.GetString(2)),
                PayloadJson = reader.IsDBNull(3) ? null : reader.GetString(3),
                Status = Enum.TryParse<JobStatus>(reader.GetString(4), out var status) ? status : JobStatus.Pending,
                ErrorText = reader.IsDBNull(5) ? null : reader.GetString(5),
                RetryCount = reader.GetInt32(6),
                CreatedAt = DateTimeOffset.Parse(reader.GetString(7)),
                StartedAt = reader.IsDBNull(8) ? null : DateTimeOffset.Parse(reader.GetString(8)),
                CompletedAt = reader.IsDBNull(9) ? null : DateTimeOffset.Parse(reader.GetString(9)),
                LastUpdatedAt = DateTimeOffset.Parse(reader.GetString(10))
            });
        }

        return items;
    }
}
