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

namespace DocuDesk.Tests.Desktop;

public sealed class MainViewModelCommandTests
{
    [Fact]
    public async Task SearchCommand_RefreshesDocumentCollection()
    {
        var harness = new MainViewModelHarness();
        harness.Repository.SearchResults = new[]
        {
            new DocumentListItemDto { Id = Guid.NewGuid(), Title = "Invoice 001", RepositoryPath = "repo/a.pdf" }
        };
        var vm = harness.CreateViewModel();

        vm.SearchCommand.Execute(null);
        await WaitForAsync(() => vm.Documents.Count == 1);

        Assert.Single(vm.Documents);
        Assert.Equal("Invoice 001", vm.Documents[0].Title);
    }

    [Fact]
    public async Task ImportCommand_ImportsSelectedFiles()
    {
        var harness = new MainViewModelHarness();
        harness.FileDialog.FilesToReturn = new[] { "a.pdf", "b.pdf" };
        var vm = harness.CreateViewModel();

        vm.ImportCommand.Execute(null);
        await WaitForAsync(() => harness.ImportService.ImportedFiles.Count == 2);

        Assert.Equal(2, harness.ImportService.ImportedFiles.Count);
        Assert.Contains("a.pdf", harness.ImportService.ImportedFiles);
        Assert.Contains("b.pdf", harness.ImportService.ImportedFiles);
    }

    [Fact]
    public async Task ImportCommand_DoesNothing_WhenNoFilesAreSelected()
    {
        var harness = new MainViewModelHarness();
        harness.FileDialog.FilesToReturn = Array.Empty<string>();
        var vm = harness.CreateViewModel();

        vm.ImportCommand.Execute(null);
        await Task.Delay(100);

        Assert.Empty(harness.ImportService.ImportedFiles);
    }

    [Fact]
    public async Task BackupCommand_CallsBackupService_AndShowsInfoMessage()
    {
        var harness = new MainViewModelHarness();
        var vm = harness.CreateViewModel();

        vm.BackupCommand.Execute(null);
        await WaitForAsync(() => harness.BackupService.CreateBackupCallCount == 1);

        Assert.Equal(1, harness.BackupService.CreateBackupCallCount);
        Assert.Contains("Backup erstellt", harness.Notifications.InfoMessages.Single());
    }

    [Fact]
    public async Task DraftMailCommand_CreatesDraft_ForSelectedDocument()
    {
        var harness = new MainViewModelHarness();
        var vm = harness.CreateViewModel();
        vm.SelectedDocument = new DocumentListItemDto
        {
            Id = Guid.NewGuid(),
            Title = "Contract",
            RepositoryPath = "repo/contract.pdf"
        };

        vm.DraftMailCommand.Execute(null);
        await WaitForAsync(() => harness.MailAdapter.LastRequest is not null);

        Assert.NotNull(harness.MailAdapter.LastRequest);
        Assert.Equal("Dokument: Contract", harness.MailAdapter.LastRequest!.Subject);
        Assert.Contains("repo/contract.pdf", harness.MailAdapter.LastRequest.Attachments);
    }

    [Fact]
    public async Task DraftMailCommand_ShowsWarning_WhenMailAdapterFails()
    {
        var harness = new MainViewModelHarness();
        harness.MailAdapter.Result = new MailDraftResult { Success = false, ErrorText = "SMTP offline" };
        var vm = harness.CreateViewModel();
        vm.SelectedDocument = new DocumentListItemDto
        {
            Id = Guid.NewGuid(),
            Title = "Contract",
            RepositoryPath = "repo/contract.pdf"
        };

        vm.DraftMailCommand.Execute(null);
        await WaitForAsync(() => harness.Notifications.WarningMessages.Count == 1);

        Assert.Contains("SMTP offline", harness.Notifications.WarningMessages.Single());
    }

    [Fact]
    public void DraftMailCommand_CanExecute_TracksSelectedDocument()
    {
        var harness = new MainViewModelHarness();
        var vm = harness.CreateViewModel();

        Assert.False(vm.DraftMailCommand.CanExecute(null));

        vm.SelectedDocument = new DocumentListItemDto { Id = Guid.NewGuid(), Title = "X", RepositoryPath = "repo/x.pdf" };

        Assert.True(vm.DraftMailCommand.CanExecute(null));
    }

    private static async Task WaitForAsync(Func<bool> condition, int timeoutMs = 3000)
    {
        var started = Environment.TickCount64;
        while (!condition())
        {
            if (Environment.TickCount64 - started > timeoutMs)
            {
                throw new TimeoutException("Condition was not reached in time.");
            }

            await Task.Delay(20);
        }
    }

    private sealed class MainViewModelHarness
    {
        public readonly FakeImportService ImportService = new();
        public readonly FakeDocumentRepository Repository = new();
        public readonly FakeBackupService BackupService = new();
        public readonly FakeMailClientAdapter MailAdapter = new();
        public readonly FakeFileDialogService FileDialog = new();
        public readonly FakeNotificationService Notifications = new();
        public readonly FakeJobStore JobStore = new();

        public MainViewModel CreateViewModel()
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
                Notifications);
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
        public MailDraftResult Result { get; set; } = new() { Success = true };

        public Task<MailDraftResult> CreateDraftAsync(MailDraftRequest request, CancellationToken cancellationToken = default)
        {
            LastRequest = request;
            return Task.FromResult(Result);
        }
    }

    private sealed class FakeFileDialogService : IFileDialogService
    {
        public string[] FilesToReturn { get; set; } = Array.Empty<string>();

        public string[] PickImportFiles() => FilesToReturn;
    }

    private sealed class FakeNotificationService : IUserNotificationService
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

        public Task<Document?> GetAsync(Guid id, CancellationToken cancellationToken = default)
            => Task.FromResult<Document?>(null);

        public Task<IReadOnlyList<DocumentListItemDto>> SearchAsync(DocumentSearchRequest request, CancellationToken cancellationToken = default)
            => Task.FromResult(SearchResults);

        public Task<Document?> FindBySha256Async(string sha256, CancellationToken cancellationToken = default)
            => Task.FromResult<Document?>(null);

        public Task UpsertDocumentTextAsync(DocumentText documentText, CancellationToken cancellationToken = default) => Task.CompletedTask;

        public Task ReplacePagesAsync(Guid documentId, IReadOnlyList<Page> pages, CancellationToken cancellationToken = default) => Task.CompletedTask;

        public Task ReplaceOcrBlocksAsync(Guid documentId, IReadOnlyList<OcrBlock> blocks, CancellationToken cancellationToken = default) => Task.CompletedTask;

        public Task SaveAnnotationAsync(Guid documentId, ViewerAnnotationCreateRequest request, CancellationToken cancellationToken = default)
            => Task.CompletedTask;

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
            => Task.FromResult(new OcrExtractionResult
            {
                DocumentText = new DocumentText { DocumentId = documentId, PlainText = string.Empty, Language = "deu" },
                Pages = new List<Page>(),
                Blocks = new List<OcrBlock>()
            });
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
