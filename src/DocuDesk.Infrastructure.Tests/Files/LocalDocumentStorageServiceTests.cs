using DocuDesk.Infrastructure.Files;
using Microsoft.Extensions.Options;

namespace DocuDesk.Infrastructure.Tests.Files;

public sealed class LocalDocumentStorageServiceTests
{
    [Fact]
    public void Constructor_ThrowsWhenRepositoryRootIsMissing()
    {
        var options = Options.Create(new RepositoryStorageOptions());

        var act = () => new LocalDocumentStorageService(options);

        act.Should().Throw<InvalidOperationException>()
            .WithMessage("*RepositoryRoot*");
    }

    [Fact]
    public async Task StoreImportedFileAsync_CopiesFileIntoShaShardFolderAndReturnsMetadata()
    {
        using var scope = new TemporaryDirectoryScope();
        var repositoryRoot = Path.Combine(scope.Path, "repository");
        var sourceFilePath = scope.CreateFile("incoming/letter.PDF", "invoice data");
        var options = Options.Create(new RepositoryStorageOptions
        {
            RepositoryRoot = repositoryRoot
        });
        var sut = new LocalDocumentStorageService(options);

        var storedFile = await sut.StoreImportedFileAsync(
            documentId: "doc-42",
            sha256: "ab1234567890",
            sourceFilePath: sourceFilePath,
            ct: CancellationToken.None);

        storedFile.StoredPath.Should().Be(Path.Combine(repositoryRoot, "files", "AB", "doc-42", "original.pdf"));
        storedFile.FileSizeBytes.Should().Be(new FileInfo(sourceFilePath).Length);
        storedFile.OriginalFileName.Should().Be("letter.PDF");
        File.ReadAllText(storedFile.StoredPath).Should().Be("invoice data");
    }
}
