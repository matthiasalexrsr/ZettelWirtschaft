using DocuDesk.Infrastructure.Files;

namespace DocuDesk.Infrastructure.Tests.Files;

public sealed class SimpleDocumentImportInspectorTests
{
    [Theory]
    [InlineData("scan.png", "image/png")]
    [InlineData("scan.jpg", "image/jpeg")]
    [InlineData("scan.jpeg", "image/jpeg")]
    [InlineData("scan.tiff", "image/tiff")]
    [InlineData("scan.bmp", "image/bmp")]
    [InlineData("scan.txt", "application/octet-stream")]
    public async Task InspectAsync_MapsKnownMimeTypesAndBasicMetadata(string fileName, string expectedMimeType)
    {
        using var scope = new TemporaryDirectoryScope();
        var sut = new SimpleDocumentImportInspector();
        var filePath = scope.CreateFile(fileName, "sample-content");

        var result = await sut.InspectAsync(filePath, CancellationToken.None);

        result.OriginalFileName.Should().Be(fileName);
        result.MimeType.Should().Be(expectedMimeType);
        result.FileSizeBytes.Should().Be(new FileInfo(filePath).Length);
        result.PageCount.Should().Be(1);
        result.SuggestedTitle.Should().Be(Path.GetFileNameWithoutExtension(fileName));
    }

    [Fact]
    public async Task InspectAsync_EstimatesPdfPageCountWithoutCountingPagesNode()
    {
        using var scope = new TemporaryDirectoryScope();
        var sut = new SimpleDocumentImportInspector();
        var filePath = scope.CreateBinaryFile("statement.pdf", FakePdfBuilder.CreateWithPageCount(3));

        var result = await sut.InspectAsync(filePath, CancellationToken.None);

        result.MimeType.Should().Be("application/pdf");
        result.PageCount.Should().Be(3);
        result.SuggestedTitle.Should().Be("statement");
    }

    [Fact]
    public async Task InspectAsync_FallsBackToSinglePageWhenPdfHasNoPageMarkers()
    {
        using var scope = new TemporaryDirectoryScope();
        var sut = new SimpleDocumentImportInspector();
        var filePath = scope.CreateBinaryFile("broken.pdf", new byte[] { 0x00, 0xFF, 0x01, 0x02 });

        var result = await sut.InspectAsync(filePath, CancellationToken.None);

        result.MimeType.Should().Be("application/pdf");
        result.PageCount.Should().Be(1);
    }
}
