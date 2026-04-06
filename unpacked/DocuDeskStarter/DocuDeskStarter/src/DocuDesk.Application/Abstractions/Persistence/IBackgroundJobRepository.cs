using DocuDesk.Domain.Entities;

namespace DocuDesk.Application.Abstractions.Persistence;

public interface IBackgroundJobRepository
{
    Task EnqueueAsync(BackgroundJob job, CancellationToken ct);
    Task<BackgroundJob?> TryAcquireNextAsync(CancellationToken ct);
    Task MarkRunningAsync(string jobId, DateTimeOffset startedUtc, CancellationToken ct);
    Task MarkCompletedAsync(string jobId, DateTimeOffset completedUtc, CancellationToken ct);
    Task MarkFailedAsync(string jobId, string errorText, DateTimeOffset failedUtc, CancellationToken ct);
}
