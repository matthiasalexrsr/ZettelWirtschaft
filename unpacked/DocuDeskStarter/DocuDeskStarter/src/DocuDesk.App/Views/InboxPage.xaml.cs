using DocuDesk.App.Services;
using DocuDesk.App.ViewModels;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Windows.Storage.Pickers;
using WinRT.Interop;

namespace DocuDesk.App.Views;

public sealed partial class InboxPage : Page
{
    private readonly InboxPageViewModel _viewModel;
    private readonly AppNavigationService _navigationService;

    public InboxPage(InboxPageViewModel viewModel, AppNavigationService navigationService)
    {
        _viewModel = viewModel;
        _navigationService = navigationService;
        InitializeComponent();
        DataContext = _viewModel;
        Loaded += OnLoaded;
    }

    private async void OnLoaded(object sender, RoutedEventArgs e)
    {
        Loaded -= OnLoaded;
        await _viewModel.LoadAsync();
    }

    private async void OnImportClicked(object sender, RoutedEventArgs e)
    {
        var picker = new FileOpenPicker();
        picker.FileTypeFilter.Add(".pdf");
        picker.FileTypeFilter.Add(".tif");
        picker.FileTypeFilter.Add(".tiff");
        picker.FileTypeFilter.Add(".png");
        picker.FileTypeFilter.Add(".jpg");
        picker.FileTypeFilter.Add(".jpeg");
        picker.FileTypeFilter.Add(".bmp");

        if (App.MainWindowInstance is not null)
        {
            var hwnd = WindowNative.GetWindowHandle(App.MainWindowInstance);
            InitializeWithWindow.Initialize(picker, hwnd);
        }

        var file = await picker.PickSingleFileAsync();
        if (file is null)
        {
            return;
        }

        await _viewModel.ImportFileAsync(file.Path);
    }

    private async void OnRefreshClicked(object sender, RoutedEventArgs e)
    {
        await _viewModel.LoadAsync();
    }

    private void OnDocumentInvoked(object sender, ItemClickEventArgs e)
    {
        if (e.ClickedItem is InboxDocumentItemViewModel item)
        {
            _navigationService.NavigateToViewer(item.Id);
        }
    }
}
