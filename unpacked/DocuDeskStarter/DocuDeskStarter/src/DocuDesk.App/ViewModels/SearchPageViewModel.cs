using DocuDesk.Application.Abstractions.Persistence;
using System.Collections.ObjectModel;

namespace DocuDesk.App.ViewModels;

public sealed class SearchPageViewModel : ObservableObject
{
    private readonly ISearchRepository _searchRepository;

    private string _queryText = string.Empty;
    private bool _isBusy;
    private string _statusMessage = "Bitte Suchbegriff eingeben.";
    private string? _errorMessage;
    private int _resultCount;

    public SearchPageViewModel(ISearchRepository searchRepository)
    {
        _searchRepository = searchRepository;
    }

    public ObservableCollection<SearchResultItemViewModel> Results { get; } = new();

    public string QueryText
    {
        get => _queryText;
        set => SetProperty(ref _queryText, value);
    }

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

    public int ResultCount
    {
        get => _resultCount;
        set
        {
            if (SetProperty(ref _resultCount, value))
            {
                OnPropertyChanged(nameof(ResultCountText));
            }
        }
    }

    public string ResultCountText => $"Treffer: {ResultCount}";

    public async Task SearchAsync(CancellationToken ct = default)
    {
        ErrorMessage = null;

        if (string.IsNullOrWhiteSpace(QueryText))
        {
            Results.Clear();
            ResultCount = 0;
            StatusMessage = "Bitte Suchbegriff eingeben.";
            return;
        }

        IsBusy = true;
        StatusMessage = "Suche läuft ...";

        try
        {
            var hits = await _searchRepository.SearchAsync(QueryText.Trim(), 0, 100, ct);
            Results.Clear();
            foreach (var hit in hits.Select(SearchResultItemViewModel.FromReadModel))
            {
                Results.Add(hit);
            }

            ResultCount = Results.Count;
            StatusMessage = ResultCount == 0
                ? "Keine Treffer gefunden."
                : $"{ResultCount} Treffer gefunden.";
        }
        catch (Exception ex)
        {
            ErrorMessage = ex.Message;
            Results.Clear();
            ResultCount = 0;
            StatusMessage = "Suche fehlgeschlagen.";
        }
        finally
        {
            IsBusy = false;
        }
    }
}
