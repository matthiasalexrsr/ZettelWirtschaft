using DocuDesk.Domain.Entities;

namespace DocuDesk.Application.Abstractions.Persistence;

public interface IMailJobRepository
{
    Task InsertAsync(MailJob job, IReadOnlyList<MailJobDocument> documents, CancellationToken ct);
    Task<MailJob?> GetAsync(string mailJobId, CancellationToken ct);
    Task MarkDispatchedAsync(string mailJobId, string externalReference, CancellationToken ct);
    Task MarkCompletedAsync(string mailJobId, DateTimeOffset completedUtc, CancellationToken ct);
    Task MarkFailedAsync(string mailJobId, string errorText, CancellationToken ct);
}
