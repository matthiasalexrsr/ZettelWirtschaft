using DocuDesk.Application.Interfaces;
using DocuDesk.Domain.Enums;

namespace DocuDesk.Worker;

public sealed class DocumentProcessingWorker
{
    private readonly IJobStore _jobStore;
    private readonly IDocumentRepository _documentRepository;
    private readonly IThumbnailService _thumbnailService;
    private readonly IOcrEngine _ocrEngine;
    private readonly ISearchIndex _searchIndex;
    private readonly IAuditLog _auditLog;

    public DocumentProcessingWorker(
        IJobStore jobStore,
        IDocumentRepository documentRepository,
        IThumbnailService thumbnailService,
        IOcrEngine ocrEngine,
        ISearchIndex searchIndex,
        IAuditLog auditLog)
    {
        _jobStore = jobStore;
        _documentRepository = documentRepository;
        _thumbnailService = thumbnailService;
        _ocrEngine = ocrEngine;
        _searchIndex = searchIndex;
        _auditLog = auditLog;
    }

    public async Task<int> ProcessPendingAsync(CancellationToken cancellationToken = default)
    {
        var jobs = await _jobStore.GetPendingAsync(50, cancellationToken);
        var processed = 0;

        foreach (var job in jobs)
        {
            try
            {
                await _jobStore.UpdateStatusAsync(job.Id, JobStatus.Running, cancellationToken: cancellationToken);
                await ProcessJobAsync(job, cancellationToken);
                await _jobStore.UpdateStatusAsync(job.Id, JobStatus.Succeeded, cancellationToken: cancellationToken);
                processed++;
            }
            catch (Exception ex)
            {
                await _jobStore.UpdateStatusAsync(job.Id, JobStatus.Failed, ex.Message, cancellationToken);
                if (job.TargetDocumentId.HasValue)
                {
                    await _auditLog.WriteAsync("JobFailed", "Document", job.TargetDocumentId.Value.ToString(), ex.ToString(), cancellationToken);
                }
            }
        }

        return processed;
    }

    private async Task ProcessJobAsync(DocuDesk.Domain.Entities.JobRecord job, CancellationToken cancellationToken)
    {
        if (!job.TargetDocumentId.HasValue)
        {
            return;
        }

        var document = await _documentRepository.GetAsync(job.TargetDocumentId.Value, cancellationToken)
            ?? throw new InvalidOperationException($"Document {job.TargetDocumentId} was not found.");

        switch (job.JobType)
        {
            case "GenerateThumbnailJob":
            {
                var thumbnailPath = await _thumbnailService.TryGenerateAsync(document.Id, document.RepositoryPath, cancellationToken);
                document.ThumbnailPath = thumbnailPath;
                document.ModifiedAt = DateTimeOffset.UtcNow;
                await _documentRepository.SaveAsync(document, cancellationToken);
                await _auditLog.WriteAsync("ThumbnailGenerated", "Document", document.Id.ToString(), thumbnailPath, cancellationToken);
                break;
            }
            case "RunOcrJob":
            {
                var extraction = await _ocrEngine.ExtractTextAsync(document.Id, document.RepositoryPath, cancellationToken);
                await _documentRepository.UpsertDocumentTextAsync(extraction.DocumentText, cancellationToken);
                await _documentRepository.ReplacePagesAsync(document.Id, extraction.Pages, cancellationToken);
                await _documentRepository.ReplaceOcrBlocksAsync(document.Id, extraction.Blocks, cancellationToken);

                document.OcrStatus = string.IsNullOrWhiteSpace(extraction.DocumentText.PlainText) ? OcrStatus.Failed : OcrStatus.Completed;
                document.Language = extraction.DocumentText.Language;
                document.PageCount = extraction.Pages.Count > 0 ? extraction.Pages.Count : document.PageCount;
                document.PreviewPath = extraction.SearchablePdfPath ?? document.PreviewPath;
                document.ModifiedAt = DateTimeOffset.UtcNow;

                await _documentRepository.SaveAsync(document, cancellationToken);
                await _searchIndex.IndexDocumentAsync(document, extraction.DocumentText.PlainText, cancellationToken);
                await _auditLog.WriteAsync("OcrCompleted", "Document", document.Id.ToString(), extraction.TsvPath, cancellationToken);
                break;
            }
        }
    }
}
