using DocuDesk.Infrastructure.Storage;

namespace DocuDesk.Infrastructure.Tests.Storage;

public sealed class FileSystemDocumentBinaryStoreTests
{
    [Fact]
    public async Task StoreOriginalAsync_CopiesFileIntoRepositoryAndOpenReadAsync_ReturnsStoredContent()
    {
        using var scope = new TemporaryDirectoryScope();
        var repositoryRoot = Path.Combine(scope.Path, "repository");
        var sourceFilePath = scope.CreateFile("incoming/invoice.pdf", "binary-placeholder");
        var documentId = Guid.Parse("11111111-2222-3333-4444-555555555555");
        var sut = new FileSystemDocumentBinaryStore(repositoryRoot);

        var storedPath = await sut.StoreOriginalAsync(sourceFilePath, documentId, CancellationToken.None);

        Directory.Exists(repositoryRoot).Should().BeTrue();
        storedPath.Should().Contain(Path.Combine(repositoryRoot, "originals"));
        Path.GetFileName(storedPath).Should().StartWith("11111111222233334444555555555555_");
        File.ReadAllText(storedPath).Should().Be("binary-placeholder");

        await using var stream = await sut.OpenReadAsync(storedPath, CancellationToken.None);
        using var reader = new StreamReader(stream);
        var content = await reader.ReadToEndAsync();

        content.Should().Be("binary-placeholder");
    }
}
