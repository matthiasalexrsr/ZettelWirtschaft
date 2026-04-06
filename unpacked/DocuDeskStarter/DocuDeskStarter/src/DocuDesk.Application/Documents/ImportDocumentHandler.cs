using DocuDesk.Application.Abstractions.Persistence;
using DocuDesk.Application.Abstractions.Services;
using DocuDesk.Domain.Entities;

namespace DocuDesk.Application.Documents;

public sealed class ImportDocumentHandler
{
    private readonly IIdGenerator _idGenerator;
    private readonly IClock _clock;
    private readonly IHashService _hashService;
    private readonly IDocumentRepository _documentRepository;

    public ImportDocumentHandler(
        IIdGenerator idGenerator,
        IClock clock,
        IHashService hashService,
        IDocumentRepository documentRepository)
    {
        _idGenerator = idGenerator;
        _clock = clock;
        _hashService = hashService;
        _documentRepository = documentRepository;
    }

    public async Task<string> HandleAsync(
        string sourceFilePath,
        string originalFileName,
        string mimeType,
        string storedPath,
        long fileSizeBytes,
        int pageCount,
        CancellationToken ct)
    {
        var sha256 = await _hashService.ComputeSha256Async(sourceFilePath, ct);
        if (await _documentRepository.ExistsBySha256Async(sha256, ct))
        {
            throw new InvalidOperationException("Dokument ist bereits im Repository vorhanden.");
        }

        var now = _clock.UtcNow;
        var documentId = _idGenerator.NewId();

        var document = new Document
        {
            Id = documentId,
            OriginalFileName = originalFileName,
            MimeType = mimeType,
            Sha256 = sha256,
            PageCount = pageCount,
            ImportDateUtc = now,
            LastModifiedUtc = now
        };

        var file = new DocumentFile
        {
            DocumentId = documentId,
            OriginalPath = storedPath,
            FileSizeBytes = fileSizeBytes,
            CreatedUtc = now
        };

        var pages = Enumerable.Range(1, pageCount)
            .Select(i => new DocumentPage
            {
                Id = _idGenerator.NewId(),
                DocumentId = documentId,
                PageNumber = i,
                RotationDeg = 0
            })
            .ToList();

        await _documentRepository.InsertAsync(document, file, pages, ct);
        return documentId;
    }
}
