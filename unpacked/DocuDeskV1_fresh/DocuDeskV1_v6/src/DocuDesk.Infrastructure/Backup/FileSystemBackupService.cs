using DocuDesk.Application.Interfaces;

namespace DocuDesk.Infrastructure.Backup;

public sealed class FileSystemBackupService : IBackupService
{
    private readonly string _databasePath;
    private readonly string _backupPath;

    public FileSystemBackupService(string databasePath, string backupPath)
    {
        _databasePath = databasePath;
        _backupPath = backupPath;
        Directory.CreateDirectory(_backupPath);
    }

    public Task<string> CreateBackupAsync(CancellationToken cancellationToken = default)
    {
        var timestamp = DateTime.UtcNow.ToString("yyyyMMdd-HHmmss");
        var destination = Path.Combine(_backupPath, $"app-{timestamp}.db");
        File.Copy(_databasePath, destination, overwrite: true);
        return Task.FromResult(destination);
    }
}
