using DocuDesk.Application.Abstractions.Persistence;
using DocuDesk.Application.Documents;
using System.Collections.ObjectModel;

namespace DocuDesk.App.ViewModels;

public sealed class InboxPageViewModel : ObservableObject
{
    private readonly IDocumentRepository _documentRepository;
    private readonly ImportDocumentFromFileHandler _importDocumentFromFileHandler;

    private bool _isBusy;
    private string _statusMessage = "Bereit";
    private string? _errorMessage;
    private int _documentCount;

    public InboxPageViewModel(
        IDocumentRepository documentRepository,
        ImportDocumentFromFileHandler importDocumentFromFileHandler)
    {
        _documentRepository = documentRepository;
        _importDocumentFromFileHandler = importDocumentFromFileHandler;
    }

    public ObservableCollection<InboxDocumentItemViewModel> Documents { get; } = new();

    public bool IsBusy
    {
        get => _isBusy;
        set => SetProperty(ref _isBusy, value);
    }

    public string StatusMessage
    {
        get => _statusMessage;
        set => SetProperty(ref _statusMessage, value);
    }

    public string? ErrorMessage
    {
        get => _errorMessage;
        set
        {
            if (SetProperty(ref _errorMessage, value))
            {
                OnPropertyChanged(nameof(HasError));
            }
        }
    }

    public bool HasError => !string.IsNullOrWhiteSpace(ErrorMessage);

    public int DocumentCount
    {
        get => _documentCount;
        set
        {
            if (SetProperty(ref _documentCount, value))
            {
                OnPropertyChanged(nameof(DocumentCountText));
            }
        }
    }

    public string DocumentCountText => $"Dokumente in der Inbox: {DocumentCount}";

    public async Task LoadAsync(CancellationToken ct = default)
    {
        IsBusy = true;
        ErrorMessage = null;
        StatusMessage = "Inbox wird geladen ...";

        try
        {
            var items = await _documentRepository.ListAsync(new DocumentListFilter
            {
                CategoryId = "cat-inbox",
                ExcludeDeleted = true,
                Take = 200,
                SortBy = "import_date_desc"
            }, ct);

            Documents.Clear();
            foreach (var item in items.Select(InboxDocumentItemViewModel.FromReadModel))
            {
                Documents.Add(item);
            }

            DocumentCount = Documents.Count;
            StatusMessage = DocumentCount == 0
                ? "Noch keine Dokumente in der Inbox."
                : $"{DocumentCount} Dokument(e) in der Inbox.";
        }
        catch (Exception ex)
        {
            ErrorMessage = ex.Message;
            StatusMessage = "Inbox konnte nicht geladen werden.";
        }
        finally
        {
            IsBusy = false;
        }
    }

    public async Task ImportFileAsync(string filePath, CancellationToken ct = default)
    {
        IsBusy = true;
        ErrorMessage = null;
        StatusMessage = "Dokument wird importiert ...";

        try
        {
            var documentId = await _importDocumentFromFileHandler.HandleAsync(filePath, ct);
            StatusMessage = $"Dokument importiert: {documentId}";
            await LoadAsync(ct);
        }
        catch (Exception ex)
        {
            ErrorMessage = ex.Message;
            StatusMessage = "Import fehlgeschlagen.";
            IsBusy = false;
        }
    }
}
