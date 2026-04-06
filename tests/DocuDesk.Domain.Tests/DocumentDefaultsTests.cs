using DocuDesk.Domain.Entities;
using DocuDesk.Domain.Enums;
using FluentAssertions;

namespace DocuDesk.Domain.Tests;

public sealed class DocumentDefaultsTests
{
    [Fact]
    public void New_document_should_have_expected_safe_defaults()
    {
        var document = new Document();

        document.Title.Should().BeEmpty();
        document.OriginalFileName.Should().BeEmpty();
        document.DisplayName.Should().BeEmpty();
        document.MimeType.Should().BeEmpty();
        document.Extension.Should().BeEmpty();
        document.Sha256.Should().BeEmpty();
        document.RepositoryPath.Should().BeEmpty();
        document.SourceType.Should().Be(DocumentSourceType.FileSystem);
        document.OcrStatus.Should().Be(OcrStatus.NotStarted);
        document.IndexStatus.Should().Be("Pending");
        document.ClassificationStatus.Should().Be("Pending");
        document.IsDeleted.Should().BeFalse();
        document.PageCount.Should().BeNull();
    }

    [Fact]
    public void Document_should_allow_assigning_business_metadata()
    {
        var now = DateTimeOffset.UtcNow;
        var documentDate = now.AddDays(-2);
        var categoryId = Guid.NewGuid();

        var document = new Document
        {
            Id = Guid.NewGuid(),
            Title = "Arztbrief",
            OriginalFileName = "arztbrief.pdf",
            DisplayName = "Arztbrief 2026-04-01",
            MimeType = "application/pdf",
            Extension = ".pdf",
            Sha256 = "abc123",
            FileSizeBytes = 2048,
            PageCount = 3,
            RepositoryPath = "files/ab/cd/arztbrief.pdf",
            SourceReference = "mail:123",
            Language = "de",
            DocumentDate = documentDate,
            Sender = "Klinik Nord",
            Recipient = "Praxis Süd",
            Subject = "Befund",
            Notes = "Wiedervorlage erforderlich",
            DocumentType = "Arztbrief",
            CategoryId = categoryId,
            OcrStatus = OcrStatus.Completed,
            IndexStatus = "Indexed",
            ClassificationStatus = "Reviewed",
            Confidence = 97.5,
            CreatedAt = now,
            ImportedAt = now,
            ModifiedAt = now
        };

        document.Title.Should().Be("Arztbrief");
        document.PageCount.Should().Be(3);
        document.Sender.Should().Be("Klinik Nord");
        document.Recipient.Should().Be("Praxis Süd");
        document.DocumentType.Should().Be("Arztbrief");
        document.CategoryId.Should().Be(categoryId);
        document.OcrStatus.Should().Be(OcrStatus.Completed);
        document.IndexStatus.Should().Be("Indexed");
        document.ClassificationStatus.Should().Be("Reviewed");
        document.Confidence.Should().Be(97.5);
    }
}
