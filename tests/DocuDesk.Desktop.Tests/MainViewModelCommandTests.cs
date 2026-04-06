using DocuDesk.Application.Interfaces;
using DocuDesk.Application.Services;
using DocuDesk.Contracts.Documents;
using DocuDesk.Contracts.Mail;
using DocuDesk.Contracts.Viewer;
using DocuDesk.Desktop.Services;
using DocuDesk.Desktop.ViewModels;
using DocuDesk.Domain.Entities;
using DocuDesk.Domain.Enums;
using DocuDesk.Worker;

namespace DocuDesk.Desktop.Tests;

public sealed class MainViewModelCommandTests
{
    [Fact]
    public async Task SearchCommand_UsesSearchServiceAndUpdatesCollection()
    {
        var harness = new MainViewModelHarness();
        harness.Repository.SearchResults = new[]
        {
            new DocumentListItemDto { Id = Guid.NewGuid(), Title = "Result A", RepositoryPath = "repo/a.pdf" }
        };
        var vm = harness.Create(runInitialLoad: false);

        vm.SearchCommand.Execute(null);
        Assert.NotNull(vm.SearchCommand.ExecutionTask);
        await vm.SearchCommand.ExecutionTask!;

        Assert.Single(vm.Documents);
        Assert.Equal("Result A", vm.Documents[0].Title);
    }

    [Fact]
    public async Task ImportCommand_UsesDialogSelectionAndImportsEachFile()
    {
        var harness = new MainViewModelHarness();
        harness.FileDialog.FilesToReturn = new[] { "a.pdf", "b.pdf" };
        var vm = harness.Create(runInitialLoad: false);

        vm.ImportCommand.Execute(null);
        Assert.NotNull(vm.ImportCommand.ExecutionTask);
        await vm.ImportCommand.ExecutionTask!;

        Assert.Equal(new[] { "a.pdf", "b.pdf" }, harness.ImportService.ImportedFiles);
    }

    [Fact]
    public async Task BackupCommand_CallsBackupServiceAndNotifiesUser()
    {
        var harness = new MainViewModelHarness();
        var vm = harness.Create(runInitialLoad: false);

        vm.BackupCommand.Execute(null);
        Assert.NotNull(vm.BackupCommand.ExecutionTask);
        await vm.BackupCommand.ExecutionTask!;

        Assert.Equal(1, harness.BackupService.CreateBackupCallCount);
        Assert.Contains("Backup erstellt", harness.Notifier.InfoMessages.Single());
    }

    [Fact]
    public async Task DraftMailCommand_CreatesDraftForSelectedDocument()
    {
        var harness = new MainViewModelHarness();
        var vm = harness.Create(runInitialLoad: false);
        vm.SelectedDocument = new DocumentListItemDto
        {
            Id = Guid.NewGuid(),
            Title = "Contract",
            RepositoryPath = "repo/contract.pdf"
        };

        vm.DraftMailCommand.Execute(null);
        Assert.NotNull(vm.DraftMailCommand.ExecutionTask);
        await vm.DraftMailCommand.ExecutionTask!;

        Assert.NotNull(harness.MailAdapter.LastRequest);
        Assert.Equal("Dokument: Contract", harness.MailAdapter.LastRequest!.Subject);
        Assert.Contains("repo/contract.pdf", harness.MailAdapter.LastRequest.Attachments);
    }

    private sealed class MainViewModelHarness
    {
        public readonly FakeImportService ImportService = new();
        public readonly FakeDocumentRepository Repository = new();
        public readonly FakeBackupService BackupService = new();
        public readonly FakeMailClientAdapter MailAdapter = new();
        public readonly FakeFileDialogService FileDialog = new();
        public readonly FakeNotifier Notifier = new();
        public readonly FakeJobStore JobStore = new();

        public MainViewModel Create(bool runInitialLoad)
        {
            var worker = new DocumentProcessingWorker(
                JobStore,
                Repository,
                new FakeThumbnailService(),
                new FakeOcrEngine(),
                new FakeSearchIndex(),
                new FakeAuditLog());
            var scheduler = new JobScheduler(JobStore, worker);
            var search = new SearchService(Repository);

            return new MainViewModel(
                ImportService,
                search,
                BackupService,
                MailAdapter,
                scheduler,
                Repository,
                FileDialog,
                Notifier,
                runInitialLoad: runInitialLoad);
        }
    }

    private sealed class FakeImportService : IImportService
    {
        public List<string> ImportedFiles { get; } = new();

        public Task<DocumentImportResult> ImportAsync(DocumentImportRequest request, CancellationToken cancellationToken = default)
        {
            ImportedFiles.Add(request.FilePath);
            return Task.FromResult(new DocumentImportResult { DocumentId = Guid.NewGuid(), StoredPath = request.FilePath });
        }
    }

    private sealed class FakeBackupService : IBackupService
    {
        public int CreateBackupCallCount { get; private set; }

        public Task<string> CreateBackupAsync(CancellationToken cancellationToken = default)
        {
            CreateBackupCallCount++;
            return Task.FromResult("backup.zip");
        }
    }

    private sealed class FakeMailClientAdapter : IMailClientAdapter
    {
        public MailDraftRequest? LastRequest { get; private set; }

        public Task<MailDraftResult> CreateDraftAsync(MailDraftRequest request, CancellationToken cancellationToken = default)
        {
            LastRequest = request;
            return Task.FromResult(new MailDraftResult { Success = true });
        }
    }

    private sealed class FakeFileDialogService : IFileDialogService
    {
        public string[] FilesToReturn { get; set; } = Array.Empty<string>();

        public string[] PickImportFiles() => FilesToReturn;
    }

    private sealed class FakeNotifier : IUserNotificationService
    {
        public List<string> InfoMessages { get; } = new();
        public List<string> WarningMessages { get; } = new();

        public void ShowInfo(string message, string title) => InfoMessages.Add($"{title}: {message}");

        public void ShowWarning(string message, string title) => WarningMessages.Add($"{title}: {message}");
    }

    private sealed class FakeJobStore : IJobStore
    {
        public Task EnqueueAsync(JobRecord job, CancellationToken cancellationToken = default) => Task.CompletedTask;
        public Task<IReadOnlyList<JobRecord>> GetLatestAsync(int take = 50, CancellationToken cancellationToken = default)
            => Task.FromResult<IReadOnlyList<JobRecord>>(Array.Empty<JobRecord>());
        public Task<IReadOnlyList<JobRecord>> GetPendingAsync(int take = 20, CancellationToken cancellationToken = default)
            => Task.FromResult<IReadOnlyList<JobRecord>>(Array.Empty<JobRecord>());
        public Task UpdateStatusAsync(Guid jobId, JobStatus status, string? errorText = null, CancellationToken cancellationToken = default)
            => Task.CompletedTask;
    }

    private sealed class FakeDocumentRepository : IDocumentRepository
    {
        public IReadOnlyList<DocumentListItemDto> SearchResults { get; set; } = Array.Empty<DocumentListItemDto>();

        public Task SaveAsync(Document document, CancellationToken cancellationToken = default) => Task.CompletedTask;
        public Task<Document?> GetAsync(Guid id, CancellationToken cancellationToken = default) => Task.FromResult<Document?>(null);
        public Task<IReadOnlyList<DocumentListItemDto>> SearchAsync(DocumentSearchRequest request, CancellationToken cancellationToken = default)
            => Task.FromResult(SearchResults);
        public Task<Document?> FindBySha256Async(string sha256, CancellationToken cancellationToken = default) => Task.FromResult<Document?>(null);
        public Task UpsertDocumentTextAsync(DocumentText documentText, CancellationToken cancellationToken = default) => Task.CompletedTask;
        public Task ReplacePagesAsync(Guid documentId, IReadOnlyList<Page> pages, CancellationToken cancellationToken = default) => Task.CompletedTask;
        public Task ReplaceOcrBlocksAsync(Guid documentId, IReadOnlyList<OcrBlock> blocks, CancellationToken cancellationToken = default) => Task.CompletedTask;
        public Task SaveAnnotationAsync(Guid documentId, ViewerAnnotationCreateRequest request, CancellationToken cancellationToken = default) => Task.CompletedTask;
        public Task<DocumentOcrOverlayDto?> GetOcrOverlayAsync(Guid documentId, CancellationToken cancellationToken = default)
            => Task.FromResult<DocumentOcrOverlayDto?>(null);
    }

    private sealed class FakeThumbnailService : IThumbnailService
    {
        public Task<string?> TryGenerateAsync(Guid documentId, string sourcePath, CancellationToken cancellationToken = default)
            => Task.FromResult<string?>(null);
    }

    private sealed class FakeOcrEngine : IOcrEngine
    {
        public Task<OcrExtractionResult> ExtractTextAsync(Guid documentId, string sourcePath, CancellationToken cancellationToken = default)
            => Task.FromResult(new OcrExtractionResult { DocumentText = new DocumentText { DocumentId = documentId } });
    }

    private sealed class FakeSearchIndex : ISearchIndex
    {
        public Task IndexDocumentAsync(Document document, string? bodyText, CancellationToken cancellationToken = default) => Task.CompletedTask;
        public Task RemoveDocumentAsync(Guid documentId, CancellationToken cancellationToken = default) => Task.CompletedTask;
    }

    private sealed class FakeAuditLog : IAuditLog
    {
        public Task WriteAsync(string eventType, string entityType, string entityId, string? payloadJson = null, CancellationToken cancellationToken = default)
            => Task.CompletedTask;
    }
}
