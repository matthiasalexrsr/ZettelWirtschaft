using DocuDesk.Application.Abstractions.Persistence;
using DocuDesk.Application.Interfaces;
using DocuDesk.Application.Services;
using DocuDesk.Contracts.Documents;
using DocuDesk.IntegrationTests.Infrastructure;
using Microsoft.Extensions.DependencyInjection;

namespace DocuDesk.IntegrationTests.Import;

public sealed class DocumentImportIntegrationTests
{
    [Fact]
    public async Task ImportAsync_PersistsDocument_AndIsSearchable()
    {
        using var fixture = new IntegrationTestFixture();
        var migrator = fixture.Services.GetRequiredService<IDatabaseMigrator>();
        await migrator.MigrateAsync(CancellationToken.None);

        var importService = fixture.Services.GetRequiredService<IImportService>();
        var searchService = fixture.Services.GetRequiredService<SearchService>();

        var filePath = CreateSampleFile(fixture.Settings.AppDataRoot, "invoice-001.txt", "Invoice 001");
        var result = await importService.ImportAsync(new DocumentImportRequest { FilePath = filePath });

        Assert.NotEqual(Guid.Empty, result.DocumentId);
        Assert.False(string.IsNullOrWhiteSpace(result.StoredPath));
        Assert.False(result.IsDuplicate);

        var matches = await searchService.SearchAsync(new DocumentSearchRequest { SearchText = "invoice-001" });
        Assert.Contains(matches, x => x.Id == result.DocumentId);
    }

    [Fact]
    public async Task ImportAsync_DetectsDuplicate_OnSecondImport()
    {
        using var fixture = new IntegrationTestFixture();
        var migrator = fixture.Services.GetRequiredService<IDatabaseMigrator>();
        await migrator.MigrateAsync(CancellationToken.None);

        var importService = fixture.Services.GetRequiredService<IImportService>();
        var filePath = CreateSampleFile(fixture.Settings.AppDataRoot, "duplicate.txt", "Duplicate payload");

        var first = await importService.ImportAsync(new DocumentImportRequest { FilePath = filePath });
        var second = await importService.ImportAsync(new DocumentImportRequest { FilePath = filePath });

        Assert.False(first.IsDuplicate);
        Assert.True(second.IsDuplicate);
        Assert.Equal(first.DocumentId, second.DocumentId);
    }

    private static string CreateSampleFile(string root, string fileName, string content)
    {
        Directory.CreateDirectory(root);
        var path = Path.Combine(root, fileName);
        File.WriteAllText(path, content);
        return path;
    }
}
