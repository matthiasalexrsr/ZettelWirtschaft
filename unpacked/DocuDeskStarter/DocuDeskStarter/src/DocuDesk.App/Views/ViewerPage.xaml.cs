using DocuDesk.App.Services;
using DocuDesk.App.ViewModels;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media.Imaging;
using Windows.Storage;
using Windows.Storage.Streams;
using Windows.System;

namespace DocuDesk.App.Views;

public sealed partial class ViewerPage : Page
{
    private readonly ViewerPageViewModel _viewModel;
    private readonly DocumentWorkspaceState _workspaceState;

    public ViewerPage(ViewerPageViewModel viewModel, DocumentWorkspaceState workspaceState)
    {
        _viewModel = viewModel;
        _workspaceState = workspaceState;
        InitializeComponent();
        DataContext = _viewModel;
        Loaded += OnLoaded;
        Unloaded += OnUnloaded;
    }

    private async void OnLoaded(object sender, RoutedEventArgs e)
    {
        _workspaceState.SelectedDocumentChanged += OnSelectedDocumentChanged;
        _viewModel.PropertyChanged += OnViewModelPropertyChanged;
        await _viewModel.LoadDocumentAsync(_workspaceState.SelectedDocumentId);
        SyncSelectedPage();
        await RefreshPreviewAsync();
    }

    private void OnUnloaded(object sender, RoutedEventArgs e)
    {
        _workspaceState.SelectedDocumentChanged -= OnSelectedDocumentChanged;
        _viewModel.PropertyChanged -= OnViewModelPropertyChanged;
    }

    private async void OnSelectedDocumentChanged(object? sender, string? documentId)
    {
        await _viewModel.LoadDocumentAsync(documentId);
        SyncSelectedPage();
        await RefreshPreviewAsync();
    }

    private async void OnPagesSelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (PagesListView.SelectedItem is DocumentPageItemViewModel item)
        {
            _viewModel.SelectPage(item.PageNumber);
            await RefreshPreviewAsync();
        }
    }

    private async void OnPreviousPageClicked(object sender, RoutedEventArgs e)
    {
        _viewModel.GoToPreviousPage();
        SyncSelectedPage();
        await RefreshPreviewAsync();
    }

    private async void OnNextPageClicked(object sender, RoutedEventArgs e)
    {
        _viewModel.GoToNextPage();
        SyncSelectedPage();
        await RefreshPreviewAsync();
    }

    private async void OnOpenOriginalClicked(object sender, RoutedEventArgs e)
    {
        if (string.IsNullOrWhiteSpace(_viewModel.CurrentFilePath) || !File.Exists(_viewModel.CurrentFilePath))
        {
            return;
        }

        var file = await StorageFile.GetFileFromPathAsync(_viewModel.CurrentFilePath);
        await Launcher.LaunchFileAsync(file);
    }

    private async void OnSaveMetadataClicked(object sender, RoutedEventArgs e)
    {
        await _viewModel.SaveMetadataAsync();
        SyncSelectedPage();
        await RefreshPreviewAsync();
    }

    private async void OnRunOcrClicked(object sender, RoutedEventArgs e)
    {
        await _viewModel.RunOcrAsync();
        SyncSelectedPage();
        await RefreshPreviewAsync();
    }

    private async void OnViewModelPropertyChanged(object? sender, System.ComponentModel.PropertyChangedEventArgs e)
    {
        if (e.PropertyName is nameof(ViewerPageViewModel.CurrentPageNumber)
            or nameof(ViewerPageViewModel.CurrentFilePath)
            or nameof(ViewerPageViewModel.CurrentMimeType))
        {
            SyncSelectedPage();
            await RefreshPreviewAsync();
        }
    }

    private void SyncSelectedPage()
    {
        if (_viewModel.CurrentPageNumber <= 0)
        {
            PagesListView.SelectedItem = null;
            return;
        }

        PagesListView.SelectedItem = _viewModel.Pages.FirstOrDefault(x => x.PageNumber == _viewModel.CurrentPageNumber);
    }

    private async Task RefreshPreviewAsync()
    {
        PlaceholderBorder.Visibility = Visibility.Collapsed;
        PdfWebView.Visibility = Visibility.Collapsed;
        ImageScrollViewer.Visibility = Visibility.Collapsed;
        PreviewImage.Source = null;

        if (_viewModel.CanPreviewAsPdf)
        {
            var pdfUri = _viewModel.GetPdfPageUri();
            if (pdfUri is not null)
            {
                PdfWebView.Visibility = Visibility.Visible;
                PdfWebView.Source = pdfUri;
                return;
            }
        }

        if (_viewModel.CanPreviewAsImage)
        {
            var imageUri = _viewModel.GetImageUri();
            if (imageUri is not null)
            {
                var bitmap = new BitmapImage();
                IRandomAccessStream stream = await RandomAccessStreamReference.CreateFromUri(imageUri).OpenReadAsync();
                await bitmap.SetSourceAsync(stream);
                PreviewImage.Source = bitmap;
                ImageScrollViewer.Visibility = Visibility.Visible;
                return;
            }
        }

        PlaceholderBorder.Visibility = Visibility.Visible;
    }
}
