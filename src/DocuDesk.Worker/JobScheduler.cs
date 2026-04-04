using DocuDesk.Application.Interfaces;
using DocuDesk.Domain.Entities;

namespace DocuDesk.Worker;

public sealed class JobScheduler
{
    private readonly IJobStore _jobStore;
    private readonly DocumentProcessingWorker _processingWorker;

    public JobScheduler(IJobStore jobStore, DocumentProcessingWorker processingWorker)
    {
        _jobStore = jobStore;
        _processingWorker = processingWorker;
    }

    public Task<IReadOnlyList<JobRecord>> GetLatestJobsAsync(CancellationToken cancellationToken = default)
        => _jobStore.GetLatestAsync(100, cancellationToken);

    public Task<int> ProcessPendingJobsAsync(CancellationToken cancellationToken = default)
        => _processingWorker.ProcessPendingAsync(cancellationToken);
}
