namespace DocuDesk.Application.Interfaces;

public interface IBackupService
{
    Task<string> CreateBackupAsync(CancellationToken cancellationToken = default);
}
