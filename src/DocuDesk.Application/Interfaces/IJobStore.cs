using DocuDesk.Domain.Entities;
using DocuDesk.Domain.Enums;

namespace DocuDesk.Application.Interfaces;

public interface IJobStore
{
    Task EnqueueAsync(JobRecord job, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<JobRecord>> GetLatestAsync(int take = 50, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<JobRecord>> GetPendingAsync(int take = 20, CancellationToken cancellationToken = default);
    Task UpdateStatusAsync(Guid jobId, JobStatus status, string? errorText = null, CancellationToken cancellationToken = default);
}
