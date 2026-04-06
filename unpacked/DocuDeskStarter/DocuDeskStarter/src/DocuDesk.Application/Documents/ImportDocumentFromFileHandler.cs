using DocuDesk.Application.Abstractions.Persistence;
using DocuDesk.Application.Abstractions.Services;
using DocuDesk.Domain.Entities;
using DocuDesk.Domain.Enums;

namespace DocuDesk.Application.Documents;

public sealed class ImportDocumentFromFileHandler
{
    private const string InboxCategoryId = "cat-inbox";

    private readonly IIdGenerator _idGenerator;
    private readonly IClock _clock;
    private readonly IHashService _hashService;
    private readonly IDocumentStorageService _storageService;
    private readonly IDocumentImportInspector _inspectionService;
    private readonly IDocumentRepository _documentRepository;

    public ImportDocumentFromFileHandler(
        IIdGenerator idGenerator,
        IClock clock,
        IHashService hashService,
        IDocumentStorageService storageService,
        IDocumentImportInspector inspectionService,
        IDocumentRepository documentRepository)
    {
        _idGenerator = idGenerator;
        _clock = clock;
        _hashService = hashService;
        _storageService = storageService;
        _inspectionService = inspectionService;
        _documentRepository = documentRepository;
    }

    public async Task<string> HandleAsync(string sourceFilePath, CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(sourceFilePath))
        {
            throw new ArgumentException("Der Quelldateipfad darf nicht leer sein.", nameof(sourceFilePath));
        }

        if (!File.Exists(sourceFilePath))
        {
            throw new FileNotFoundException("Die ausgewählte Datei wurde nicht gefunden.", sourceFilePath);
        }

        var sha256 = await _hashService.ComputeSha256Async(sourceFilePath, ct);

        if (await _documentRepository.ExistsBySha256Async(sha256, ct))
        {
            throw new InvalidOperationException("Dieses Dokument wurde bereits importiert.");
        }

        var documentId = _idGenerator.NewId();
        var now = _clock.UtcNow;

        var inspection = await _inspectionService.InspectAsync(sourceFilePath, ct);
        var storedFile = await _storageService.StoreImportedFileAsync(documentId, sha256, sourceFilePath, ct);

        var document = new Document
        {
            Id = documentId,
            Title = string.IsNullOrWhiteSpace(inspection.SuggestedTitle)
                ? Path.GetFileNameWithoutExtension(inspection.OriginalFileName)
                : inspection.SuggestedTitle,
            OriginalFileName = inspection.OriginalFileName,
            MimeType = inspection.MimeType,
            Sha256 = sha256,
            PageCount = inspection.PageCount,
            ImportDateUtc = now,
            Status = DocumentStatus.New,
            CategoryId = InboxCategoryId,
            HasOcr = false,
            IsDeleted = false,
            LastModifiedUtc = now
        };

        var file = new DocumentFile
        {
            DocumentId = documentId,
            OriginalPath = storedFile.StoredPath,
            FileSizeBytes = inspection.FileSizeBytes,
            StorageState = StorageState.Present,
            CreatedUtc = now
        };

        var pages = Enumerable.Range(1, Math.Max(inspection.PageCount, 1))
            .Select(pageNumber => new DocumentPage
            {
                Id = _idGenerator.NewId(),
                DocumentId = documentId,
                PageNumber = pageNumber,
                RotationDeg = 0
            })
            .ToList();

        await _documentRepository.InsertAsync(document, file, pages, ct);
        return documentId;
    }
}
