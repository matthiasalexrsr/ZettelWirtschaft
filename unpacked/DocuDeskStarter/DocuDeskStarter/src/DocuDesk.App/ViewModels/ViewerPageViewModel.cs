using DocuDesk.Application.Abstractions.Persistence;
using DocuDesk.Application.Documents;
using DocuDesk.Domain.Entities;
using DocuDesk.Domain.Enums;
using System.Collections.ObjectModel;
using System.Globalization;

namespace DocuDesk.App.ViewModels;

public sealed class ViewerPageViewModel : ObservableObject
{
    private static readonly string[] AcceptedDateFormats = ["dd.MM.yyyy", "d.M.yyyy", "yyyy-MM-dd"];

    private readonly IDocumentRepository _documentRepository;
    private readonly ICategoryRepository _categoryRepository;
    private readonly ITagRepository _tagRepository;
    private readonly UpdateDocumentMetadataHandler _updateDocumentMetadataHandler;
    private readonly SyncDocumentTagsHandler _syncDocumentTagsHandler;
    private readonly RunDocumentOcrHandler _runDocumentOcrHandler;

    private bool _isBusy;
    private string _statusMessage = "Noch kein Dokument ausgewählt.";
    private string? _errorMessage;
    private string _title = "Kein Dokument ausgewählt";
    private string _originalFileName = string.Empty;
    private string _categoryText = "–";
    private string _statusText = "–";
    private string _senderText = "–";
    private string _recipientText = "–";
    private string _importDateText = "–";
    private string _documentDateText = "–";
    private string _pageCountText = "0";
    private string _storagePath = "–";
    private string _tagSummary = "Keine Tags";
    private string _ocrInfoText = "Noch keine OCR verfügbar.";
    private string _ocrPreviewText = string.Empty;
    private string _ocrArtifactText = "Noch keine OCR-Artefakte gespeichert.";
    private string _viewerStatusText = "Keine Vorschau verfügbar.";
    private string _pageSelectionText = "Seite – / –";
    private string _previewHintText = "Importiere ein Dokument und öffne es aus der Inbox oder Suche.";
    private string _editableTitle = string.Empty;
    private string _editableSender = string.Empty;
    private string _editableRecipient = string.Empty;
    private string _editableDocumentDateText = string.Empty;
    private string _editableNotes = string.Empty;
    private string _ocrLanguage = "deu+eng";
    private CategoryOptionViewModel? _selectedCategory;
    private string? _documentId;
    private string? _currentFilePath;
    private string? _currentMimeType;
    private int _currentPageNumber;
    private Document? _loadedDocument;

    public ViewerPageViewModel(
        IDocumentRepository documentRepository,
        ICategoryRepository categoryRepository,
        ITagRepository tagRepository,
        UpdateDocumentMetadataHandler updateDocumentMetadataHandler,
        SyncDocumentTagsHandler syncDocumentTagsHandler,
        RunDocumentOcrHandler runDocumentOcrHandler)
    {
        _documentRepository = documentRepository;
        _categoryRepository = categoryRepository;
        _tagRepository = tagRepository;
        _updateDocumentMetadataHandler = updateDocumentMetadataHandler;
        _syncDocumentTagsHandler = syncDocumentTagsHandler;
        _runDocumentOcrHandler = runDocumentOcrHandler;
    }

    public ObservableCollection<DocumentPageItemViewModel> Pages { get; } = new();
    public ObservableCollection<CategoryOptionViewModel> Categories { get; } = new();
    public ObservableCollection<TagSelectionItemViewModel> TagOptions { get; } = new();

    public bool IsBusy
    {
        get => _isBusy;
        set
        {
            if (SetProperty(ref _isBusy, value))
            {
                OnPropertyChanged(nameof(CanSaveMetadata));
                OnPropertyChanged(nameof(CanRunOcr));
            }
        }
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

    public string Title { get => _title; set => SetProperty(ref _title, value); }
    public string OriginalFileName { get => _originalFileName; set => SetProperty(ref _originalFileName, value); }
    public string CategoryText { get => _categoryText; set => SetProperty(ref _categoryText, value); }
    public string StatusText { get => _statusText; set => SetProperty(ref _statusText, value); }
    public string SenderText { get => _senderText; set => SetProperty(ref _senderText, value); }
    public string RecipientText { get => _recipientText; set => SetProperty(ref _recipientText, value); }
    public string ImportDateText { get => _importDateText; set => SetProperty(ref _importDateText, value); }
    public string DocumentDateText { get => _documentDateText; set => SetProperty(ref _documentDateText, value); }
    public string PageCountText { get => _pageCountText; set => SetProperty(ref _pageCountText, value); }
    public string StoragePath { get => _storagePath; set => SetProperty(ref _storagePath, value); }
    public string TagSummary { get => _tagSummary; set => SetProperty(ref _tagSummary, value); }
    public string OcrInfoText { get => _ocrInfoText; set => SetProperty(ref _ocrInfoText, value); }
    public string OcrPreviewText { get => _ocrPreviewText; set => SetProperty(ref _ocrPreviewText, value); }
    public string OcrArtifactText { get => _ocrArtifactText; set => SetProperty(ref _ocrArtifactText, value); }
    public string ViewerStatusText { get => _viewerStatusText; set => SetProperty(ref _viewerStatusText, value); }
    public string PageSelectionText { get => _pageSelectionText; set => SetProperty(ref _pageSelectionText, value); }
    public string PreviewHintText { get => _previewHintText; set => SetProperty(ref _previewHintText, value); }
    public string EditableTitle { get => _editableTitle; set => SetProperty(ref _editableTitle, value); }
    public string EditableSender { get => _editableSender; set => SetProperty(ref _editableSender, value); }
    public string EditableRecipient { get => _editableRecipient; set => SetProperty(ref _editableRecipient, value); }
    public string EditableDocumentDateText { get => _editableDocumentDateText; set => SetProperty(ref _editableDocumentDateText, value); }
    public string EditableNotes { get => _editableNotes; set => SetProperty(ref _editableNotes, value); }
    public string OcrLanguage { get => _ocrLanguage; set => SetProperty(ref _ocrLanguage, value); }
    public string? DocumentId { get => _documentId; private set => SetProperty(ref _documentId, value); }
    public string? CurrentFilePath { get => _currentFilePath; private set => SetProperty(ref _currentFilePath, value); }
    public string? CurrentMimeType { get => _currentMimeType; private set => SetProperty(ref _currentMimeType, value); }
    public int CurrentPageNumber { get => _currentPageNumber; private set => SetProperty(ref _currentPageNumber, value); }

    public CategoryOptionViewModel? SelectedCategory
    {
        get => _selectedCategory;
        set => SetProperty(ref _selectedCategory, value);
    }

    public bool CanSaveMetadata => !IsBusy && !string.IsNullOrWhiteSpace(DocumentId);
    public bool CanRunOcr => !IsBusy && !string.IsNullOrWhiteSpace(DocumentId) && !string.IsNullOrWhiteSpace(CurrentFilePath);

    public bool CanPreviewAsPdf => string.Equals(CurrentMimeType, "application/pdf", StringComparison.OrdinalIgnoreCase)
                                   && !string.IsNullOrWhiteSpace(CurrentFilePath)
                                   && File.Exists(CurrentFilePath);

    public bool CanPreviewAsImage => CurrentMimeType is "image/png" or "image/jpeg" or "image/bmp" or "image/tiff"
                                     && !string.IsNullOrWhiteSpace(CurrentFilePath)
                                     && File.Exists(CurrentFilePath);

    public bool CanPreviewInline => CanPreviewAsPdf || CanPreviewAsImage;
    public bool CanGoToPreviousPage => CurrentPageNumber > 1;
    public bool CanGoToNextPage => Pages.Count > 0 && CurrentPageNumber < Pages.Count;

    public Uri? GetPdfPageUri()
    {
        if (!CanPreviewAsPdf || string.IsNullOrWhiteSpace(CurrentFilePath))
        {
            return null;
        }

        var baseUri = new Uri(CurrentFilePath, UriKind.Absolute);
        return new Uri($"{baseUri.AbsoluteUri}#page={Math.Max(CurrentPageNumber, 1)}");
    }

    public Uri? GetImageUri()
    {
        if (!CanPreviewAsImage || string.IsNullOrWhiteSpace(CurrentFilePath))
        {
            return null;
        }

        return new Uri(CurrentFilePath, UriKind.Absolute);
    }

    public async Task LoadDocumentAsync(string? documentId, CancellationToken ct = default)
    {
        DocumentId = documentId;
        ErrorMessage = null;

        if (string.IsNullOrWhiteSpace(documentId))
        {
            ResetToEmptyState();
            return;
        }

        IsBusy = true;
        StatusMessage = "Dokument wird geladen ...";

        try
        {
            await EnsureReferenceDataLoadedAsync(ct);

            var details = await _documentRepository.GetDetailsAsync(documentId, ct);
            if (details is null)
            {
                ResetToEmptyState();
                ErrorMessage = "Dokument wurde nicht gefunden.";
                return;
            }

            _loadedDocument = details.Document;

            Title = string.IsNullOrWhiteSpace(details.Document.Title)
                ? details.Document.OriginalFileName
                : details.Document.Title!;
            OriginalFileName = details.Document.OriginalFileName;
            CategoryText = details.Category?.Name ?? "Keine Kategorie";
            StatusText = ToGermanStatus(details.Document.Status);
            SenderText = string.IsNullOrWhiteSpace(details.Document.Sender) ? "–" : details.Document.Sender!;
            RecipientText = string.IsNullOrWhiteSpace(details.Document.Recipient) ? "–" : details.Document.Recipient!;
            ImportDateText = details.Document.ImportDateUtc.LocalDateTime.ToString("dd.MM.yyyy HH:mm");
            DocumentDateText = details.Document.DocumentDate?.ToString("dd.MM.yyyy") ?? "–";
            PageCountText = details.Pages.Count.ToString();
            StoragePath = details.File.OriginalPath;
            CurrentFilePath = details.File.OriginalPath;
            CurrentMimeType = details.Document.MimeType;
            OcrInfoText = details.Text is null
                ? "Noch keine OCR verfügbar."
                : $"OCR-Sprache: {details.Text.Language ?? "n/a"} • Ø Konfidenz: {details.Text.OcrConfidenceAverage?.ToString("0.0") ?? "n/a"}";
            OcrPreviewText = string.IsNullOrWhiteSpace(details.Text?.PlainText)
                ? "Noch kein OCR-Text vorhanden."
                : Limit(details.Text!.PlainText, 4000);
            OcrArtifactText = string.IsNullOrWhiteSpace(details.File.OcrArtifactPath)
                ? "Noch keine OCR-Artefakte gespeichert."
                : details.File.OcrArtifactPath!;

            EditableTitle = details.Document.Title ?? Path.GetFileNameWithoutExtension(details.Document.OriginalFileName);
            EditableSender = details.Document.Sender ?? string.Empty;
            EditableRecipient = details.Document.Recipient ?? string.Empty;
            EditableDocumentDateText = details.Document.DocumentDate?.ToString("dd.MM.yyyy") ?? string.Empty;
            EditableNotes = details.Document.Notes ?? string.Empty;
            SelectedCategory = Categories.FirstOrDefault(x => string.Equals(x.Id, details.Document.CategoryId ?? string.Empty, StringComparison.Ordinal))
                               ?? Categories.FirstOrDefault();

            Pages.Clear();
            foreach (var page in details.Pages)
            {
                Pages.Add(new DocumentPageItemViewModel
                {
                    Id = page.Id,
                    PageNumber = page.PageNumber,
                    PageText = $"Seite {page.PageNumber}",
                    RotationText = $"Rotation: {page.RotationDeg}°",
                    PreviewText = string.IsNullOrWhiteSpace(page.PreviewImagePath) ? "Noch keine Miniatur" : page.PreviewImagePath!
                });
            }
            OnPropertyChanged(nameof(Pages));

            var selectedTagIds = new HashSet<string>(details.Tags.Select(static x => x.Id), StringComparer.Ordinal);
            foreach (var tagOption in TagOptions)
            {
                tagOption.IsSelected = selectedTagIds.Contains(tagOption.Id);
            }
            UpdateTagSummary();

            CurrentPageNumber = Pages.Count > 0 ? 1 : 0;
            UpdateViewerState();
            StatusMessage = "Dokument geladen.";
        }
        catch (Exception ex)
        {
            ResetToEmptyState();
            ErrorMessage = ex.Message;
            StatusMessage = "Dokument konnte nicht geladen werden.";
        }
        finally
        {
            IsBusy = false;
        }
    }

    public async Task SaveMetadataAsync(CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(DocumentId) || _loadedDocument is null)
        {
            return;
        }

        ErrorMessage = null;
        IsBusy = true;
        StatusMessage = "Änderungen werden gespeichert ...";

        try
        {
            var parsedDate = ParseDocumentDateOrThrow(EditableDocumentDateText);

            await _updateDocumentMetadataHandler.HandleAsync(new UpdateDocumentMetadataRequest(
                DocumentId,
                EditableTitle,
                parsedDate,
                EditableSender,
                EditableRecipient,
                EditableNotes,
                SelectedCategory is null || string.IsNullOrWhiteSpace(SelectedCategory.Id) ? null : SelectedCategory.Id), ct);

            await _syncDocumentTagsHandler.HandleAsync(
                DocumentId,
                TagOptions.Where(static x => x.IsSelected).Select(static x => x.Id).ToArray(),
                ct);

            await LoadDocumentAsync(DocumentId, ct);
            StatusMessage = "Änderungen gespeichert.";
        }
        catch (Exception ex)
        {
            ErrorMessage = ex.Message;
            StatusMessage = "Änderungen konnten nicht gespeichert werden.";
        }
        finally
        {
            IsBusy = false;
        }
    }

    public async Task RunOcrAsync(CancellationToken ct = default)
    {
        if (!CanRunOcr || string.IsNullOrWhiteSpace(DocumentId))
        {
            return;
        }

        ErrorMessage = null;
        IsBusy = true;
        StatusMessage = "OCR wird ausgeführt ...";

        try
        {
            await _runDocumentOcrHandler.HandleAsync(DocumentId, OcrLanguage, ct);
            await LoadDocumentAsync(DocumentId, ct);
            StatusMessage = "OCR erfolgreich abgeschlossen.";
        }
        catch (Exception ex)
        {
            ErrorMessage = ex.Message;
            StatusMessage = "OCR konnte nicht ausgeführt werden.";
        }
        finally
        {
            IsBusy = false;
        }
    }

    public void SelectPage(int pageNumber)
    {
        if (Pages.Count == 0)
        {
            return;
        }

        var normalized = Math.Clamp(pageNumber, 1, Pages.Count);
        if (normalized == CurrentPageNumber)
        {
            return;
        }

        CurrentPageNumber = normalized;
        UpdateViewerState();
    }

    public void GoToPreviousPage()
    {
        if (CanGoToPreviousPage)
        {
            SelectPage(CurrentPageNumber - 1);
        }
    }

    public void GoToNextPage()
    {
        if (CanGoToNextPage)
        {
            SelectPage(CurrentPageNumber + 1);
        }
    }

    private async Task EnsureReferenceDataLoadedAsync(CancellationToken ct)
    {
        if (Categories.Count == 0)
        {
            Categories.Add(new CategoryOptionViewModel { Id = string.Empty, Name = "Keine Kategorie" });

            var categories = await _categoryRepository.ListAsync(ct);
            foreach (var category in categories)
            {
                Categories.Add(new CategoryOptionViewModel
                {
                    Id = category.Id,
                    Name = category.Name
                });
            }
        }

        if (TagOptions.Count == 0)
        {
            var tags = await _tagRepository.ListAsync(ct);
            foreach (var tag in tags)
            {
                var item = new TagSelectionItemViewModel
                {
                    Id = tag.Id,
                    Name = tag.Name,
                    IsSelected = false
                };
                item.PropertyChanged += OnTagOptionPropertyChanged;
                TagOptions.Add(item);
            }
        }
    }

    private void ResetToEmptyState()
    {
        DocumentId = null;
        _loadedDocument = null;
        Title = "Kein Dokument ausgewählt";
        OriginalFileName = string.Empty;
        CategoryText = "–";
        StatusText = "–";
        SenderText = "–";
        RecipientText = "–";
        ImportDateText = "–";
        DocumentDateText = "–";
        PageCountText = "0";
        StoragePath = "–";
        TagSummary = "Keine Tags";
        OcrInfoText = "Noch keine OCR verfügbar.";
        OcrPreviewText = string.Empty;
        OcrArtifactText = "Noch keine OCR-Artefakte gespeichert.";
        ViewerStatusText = "Keine Vorschau verfügbar.";
        PageSelectionText = "Seite – / –";
        PreviewHintText = "Importiere ein Dokument und öffne es aus der Inbox oder Suche.";
        EditableTitle = string.Empty;
        EditableSender = string.Empty;
        EditableRecipient = string.Empty;
        EditableDocumentDateText = string.Empty;
        EditableNotes = string.Empty;
        OcrLanguage = "deu+eng";
        SelectedCategory = Categories.FirstOrDefault();
        CurrentFilePath = null;
        CurrentMimeType = null;
        CurrentPageNumber = 0;
        Pages.Clear();
        foreach (var tagOption in TagOptions)
        {
            tagOption.IsSelected = false;
        }
        OnPropertyChanged(nameof(Pages));
        OnPropertyChanged(nameof(CanPreviewAsPdf));
        OnPropertyChanged(nameof(CanPreviewAsImage));
        OnPropertyChanged(nameof(CanPreviewInline));
        OnPropertyChanged(nameof(CanGoToPreviousPage));
        OnPropertyChanged(nameof(CanGoToNextPage));
        OnPropertyChanged(nameof(CanRunOcr));
        StatusMessage = "Noch kein Dokument ausgewählt.";
    }

    private void UpdateViewerState()
    {
        PageSelectionText = Pages.Count == 0
            ? "Seite – / –"
            : $"Seite {CurrentPageNumber} / {Pages.Count}";

        if (CanPreviewAsPdf)
        {
            ViewerStatusText = $"PDF-Vorschau • {PageSelectionText}";
            PreviewHintText = "PDF wird im eingebetteten Viewer angezeigt.";
        }
        else if (CanPreviewAsImage)
        {
            ViewerStatusText = $"Bildvorschau • {PageSelectionText}";
            PreviewHintText = "Bilddatei wird direkt eingebettet angezeigt.";
        }
        else if (!string.IsNullOrWhiteSpace(CurrentFilePath))
        {
            ViewerStatusText = "Für diesen Dateityp ist aktuell keine eingebettete Vorschau vorhanden.";
            PreviewHintText = "Öffne die Originaldatei über die Schaltfläche im Standardprogramm.";
        }
        else
        {
            ViewerStatusText = "Keine Vorschau verfügbar.";
            PreviewHintText = "Importiere ein Dokument und öffne es aus der Inbox oder Suche.";
        }

        OnPropertyChanged(nameof(CanPreviewAsPdf));
        OnPropertyChanged(nameof(CanPreviewAsImage));
        OnPropertyChanged(nameof(CanPreviewInline));
        OnPropertyChanged(nameof(CanGoToPreviousPage));
        OnPropertyChanged(nameof(CanGoToNextPage));
        OnPropertyChanged(nameof(CanRunOcr));
    }

    private void UpdateTagSummary()
    {
        var selected = TagOptions.Where(static x => x.IsSelected).Select(static x => x.Name).ToList();
        TagSummary = selected.Count == 0 ? "Keine Tags" : string.Join(", ", selected);
    }

    private void OnTagOptionPropertyChanged(object? sender, System.ComponentModel.PropertyChangedEventArgs e)
    {
        if (e.PropertyName == nameof(TagSelectionItemViewModel.IsSelected))
        {
            UpdateTagSummary();
        }
    }

    private static string Limit(string text, int maxLength)
    {
        if (string.IsNullOrWhiteSpace(text))
        {
            return string.Empty;
        }

        return text.Length <= maxLength ? text : text[..maxLength] + Environment.NewLine + "…";
    }

    private static string ToGermanStatus(DocumentStatus status) => status switch
    {
        DocumentStatus.New => "Neu",
        DocumentStatus.Processing => "In Verarbeitung",
        DocumentStatus.Ready => "Bereit",
        DocumentStatus.Review => "Zur Prüfung",
        DocumentStatus.Error => "Fehler",
        DocumentStatus.Archived => "Archiviert",
        _ => status.ToString()
    };

    private static DateOnly? ParseDocumentDateOrThrow(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return null;
        }

        if (DateOnly.TryParseExact(value.Trim(), AcceptedDateFormats, CultureInfo.GetCultureInfo("de-DE"), DateTimeStyles.None, out var parsed))
        {
            return parsed;
        }

        if (DateOnly.TryParse(value.Trim(), CultureInfo.GetCultureInfo("de-DE"), DateTimeStyles.None, out parsed))
        {
            return parsed;
        }

        throw new InvalidOperationException("Dokumentdatum konnte nicht gelesen werden. Verwende z. B. 31.12.2025 oder 2025-12-31.");
    }
}
