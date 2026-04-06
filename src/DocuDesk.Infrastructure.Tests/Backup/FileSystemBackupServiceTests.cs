using DocuDesk.Infrastructure.Backup;

namespace DocuDesk.Infrastructure.Tests.Backup;

public sealed class FileSystemBackupServiceTests
{
    [Fact]
    public async Task CreateBackupAsync_CreatesTimestampedCopyInsideBackupDirectory()
    {
        using var scope = new TemporaryDirectoryScope();
        var databasePath = scope.CreateFile("db/app.db", "sqlite-content");
        var backupsPath = Path.Combine(scope.Path, "backups");
        var sut = new FileSystemBackupService(databasePath, backupsPath);

        var backupPath = await sut.CreateBackupAsync(CancellationToken.None);

        Directory.Exists(backupsPath).Should().BeTrue();
        File.Exists(backupPath).Should().BeTrue();
        backupPath.Should().StartWith(backupsPath);
        Path.GetFileName(backupPath).Should().MatchRegex(@"^app-\d{8}-\d{6}\.db$");
        File.ReadAllText(backupPath).Should().Be("sqlite-content");
    }
}
