using System.Collections.ObjectModel;
using System.Text.Json;
using System.Windows;
using DocuDesk.Application.Interfaces;
using DocuDesk.Application.Services;
using DocuDesk.Contracts.Documents;
using DocuDesk.Contracts.Mail;
using DocuDesk.Contracts.Viewer;
using DocuDesk.Desktop.Common;
using DocuDesk.Desktop.Services;
using DocuDesk.Worker;

namespace DocuDesk.Desktop.ViewModels;

public sealed class MainViewModel : ObservableObject
{
    private static readonly JsonSerializerOptions JsonOptions = new() { PropertyNamingPolicy = JsonNamingPolicy.CamelCase };
    private readonly IImportService _importService;
    private readonly SearchService _searchService;
    private readonly IBackupService _backupService;
    private readonly IMailClientAdapter _mailClientAdapter;
    private readonly JobScheduler _jobScheduler;
    private readonly IDocumentRepository _documentRepository;
    private readonly IFileDialogService _fileDialogService;
    private readonly IUserNotificationService _notificationService;

    private string? _searchText;
    private DocumentListItemDto? _selectedDocument;
    private string? _selectedDocumentOverlayJson;

    public MainViewModel(
        IImportService importService,
        SearchService searchService,
        IBackupService backupService,
        IMailClientAdapter mailClientAdapter,
        JobScheduler jobScheduler,
        IDocumentRepository documentRepository,
        IFileDialogService fileDialogService,
        IUserNotificationService notificationService)
    {
        _importService = importService;
        _searchService = searchService;
        _backupService = backupService;
        _mailClientAdapter = mailClientAdapter;
        _jobScheduler = jobScheduler;
        _documentRepository = documentRepository;
        _fileDialogService = fileDialogService;
        _notificationService = notificationService;

        SearchCommand = new RelayCommand(async () => await LoadDocumentsAsync());
        ImportCommand = new RelayCommand(async () => await ImportAsync());
        BackupCommand = new RelayCommand(async () => await BackupAsync());
        DraftMailCommand = new RelayCommand(async () => await DraftMailAsync(), () => SelectedDocument is not null);
        ProcessJobsCommand = new RelayCommand(async () => await ProcessJobsAsync());

        Documents = new ObservableCollection<DocumentListItemDto>();
        Jobs = new ObservableCollection<JobRecordViewModel>();

        _ = InitializeAsync();
    }

    public ObservableCollection<DocumentListItemDto> Documents { get; }
    public ObservableCollection<JobRecordViewModel> Jobs { get; }

    public RelayCommand SearchCommand { get; }
    public RelayCommand ImportCommand { get; }
    public RelayCommand BackupCommand { get; }
    public RelayCommand DraftMailCommand { get; }
    public RelayCommand ProcessJobsCommand { get; }

    public string? SearchText
    {
        get => _searchText;
        set
        {
            if (SetProperty(ref _searchText, value))
            {
                OnPropertyChanged(nameof(SearchText));
            }
        }
    }

    public DocumentListItemDto? SelectedDocument
    {
        get => _selectedDocument;
        set
        {
            if (SetProperty(ref _selectedDocument, value))
            {
                DraftMailCommand.RaiseCanExecuteChanged();
                OnPropertyChanged(nameof(SelectedDocumentPath));
                _ = LoadSelectedDocumentOverlayAsync();
            }
        }
    }

    public string? SelectedDocumentPath => SelectedDocument?.RepositoryPath;
    public Guid? SelectedDocumentId => SelectedDocument?.Id;

    public string? SelectedDocumentOverlayJson
    {
        get => _selectedDocumentOverlayJson;
        private set => SetProperty(ref _selectedDocumentOverlayJson, value);
    }

    private async Task InitializeAsync()
    {
        try
        {
            await ProcessJobsAsync();
            await LoadDocumentsAsync();
            await LoadJobsAsync();
        }
        catch (Exception ex)
        {
            _notificationService.ShowWarning($"Initialdaten konnten nicht geladen werden:\n\n{ex.Message}", "DocuDesk");
        }
    }

    private async Task LoadDocumentsAsync()
    {
        var items = await _searchService.SearchAsync(new DocumentSearchRequest { SearchText = SearchText });
        InvokeOnUiThread(() =>
        {
            Documents.Clear();
            foreach (var item in items)
            {
                Documents.Add(item);
            }
            SelectedDocument = Documents.FirstOrDefault();
        });
    }

    private async Task LoadJobsAsync()
    {
        var jobs = await _jobScheduler.GetLatestJobsAsync();
        InvokeOnUiThread(() =>
        {
            Jobs.Clear();
            foreach (var job in jobs)
            {
                Jobs.Add(new JobRecordViewModel
                {
                    JobType = job.JobType,
                    Status = job.Status.ToString(),
                    CreatedAt = job.CreatedAt,
                    ErrorText = job.ErrorText
                });
            }
        });
    }

    private async Task ImportAsync()
    {
        var selectedFiles = _fileDialogService.PickImportFiles();
        if (selectedFiles.Length == 0)
        {
            return;
        }

        foreach (var fileName in selectedFiles)
        {
            await _importService.ImportAsync(new DocumentImportRequest { FilePath = fileName });
        }

        await ProcessJobsAsync();
        await LoadDocumentsAsync();
        await LoadJobsAsync();
    }

    private async Task BackupAsync()
    {
        var backupPath = await _backupService.CreateBackupAsync();
        _notificationService.ShowInfo($"Backup erstellt:\n{backupPath}", "DocuDesk");
    }

    private async Task DraftMailAsync()
    {
        if (SelectedDocument is null)
        {
            return;
        }

        var result = await _mailClientAdapter.CreateDraftAsync(new MailDraftRequest
        {
            Subject = $"Dokument: {SelectedDocument.Title}",
            BodyPlainText = $"Bitte prüfen: {SelectedDocument.Title}",
            Attachments = new[] { SelectedDocument.RepositoryPath }
        });

        if (!result.Success)
        {
            _notificationService.ShowWarning(result.ErrorText ?? "Mail-Entwurf konnte nicht erzeugt werden.", "DocuDesk");
        }
    }

    private async Task ProcessJobsAsync()
    {
        await _jobScheduler.ProcessPendingJobsAsync();
        await LoadJobsAsync();
        await LoadSelectedDocumentOverlayAsync();
    }

    private async Task LoadSelectedDocumentOverlayAsync()
    {
        if (SelectedDocument is null)
        {
            SelectedDocumentOverlayJson = null;
            OnPropertyChanged(nameof(SelectedDocumentId));
            return;
        }

        var overlay = await _documentRepository.GetOcrOverlayAsync(SelectedDocument.Id);
        SelectedDocumentOverlayJson = overlay is null ? null : JsonSerializer.Serialize(overlay, JsonOptions);
        OnPropertyChanged(nameof(SelectedDocumentId));
    }

    public async Task HandleViewerAnnotationCreatedAsync(ViewerAnnotationCreateRequest request)
    {
        if (SelectedDocument is null)
        {
            return;
        }

        await _documentRepository.SaveAnnotationAsync(SelectedDocument.Id, request);
        await LoadSelectedDocumentOverlayAsync();
    }

    private static void InvokeOnUiThread(Action action)
    {
        if (Application.Current?.Dispatcher is { } dispatcher)
        {
            dispatcher.Invoke(action);
            return;
        }

        action();
    }
}
