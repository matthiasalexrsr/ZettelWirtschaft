using DocuDesk.App.Services;
using DocuDesk.App.ViewModels;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;

namespace DocuDesk.App.Views;

public sealed partial class SearchPage : Page
{
    private readonly SearchPageViewModel _viewModel;
    private readonly AppNavigationService _navigationService;

    public SearchPage(SearchPageViewModel viewModel, AppNavigationService navigationService)
    {
        _viewModel = viewModel;
        _navigationService = navigationService;
        InitializeComponent();
        DataContext = _viewModel;
        Loaded += OnLoaded;
    }

    private void OnLoaded(object sender, RoutedEventArgs e)
    {
        Loaded -= OnLoaded;
        QueryTextBox.Focus(FocusState.Programmatic);
    }

    private async void OnSearchClicked(object sender, RoutedEventArgs e)
    {
        await _viewModel.SearchAsync();
    }

    private void OnResultInvoked(object sender, ItemClickEventArgs e)
    {
        if (e.ClickedItem is SearchResultItemViewModel item)
        {
            _navigationService.NavigateToViewer(item.DocumentId);
        }
    }
}
