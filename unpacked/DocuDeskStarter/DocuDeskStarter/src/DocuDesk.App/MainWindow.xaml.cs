using Microsoft.Extensions.DependencyInjection;
using DocuDesk.App.Services;
using DocuDesk.App.ViewModels;
using DocuDesk.App.Views;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;

namespace DocuDesk.App;

public sealed partial class MainWindow : Window
{
    private readonly MainWindowViewModel _viewModel;
    private readonly IServiceProvider _serviceProvider;
    private readonly AppNavigationService _navigationService;
    private readonly DocumentWorkspaceState _workspaceState;

    public MainWindow(
        MainWindowViewModel viewModel,
        IServiceProvider serviceProvider,
        AppNavigationService navigationService,
        DocumentWorkspaceState workspaceState)
    {
        _viewModel = viewModel;
        _serviceProvider = serviceProvider;
        _navigationService = navigationService;
        _workspaceState = workspaceState;

        InitializeComponent();
        ExtendsContentIntoTitleBar = true;
        SetTitleBar(RootNavigationView);

        Loaded += OnLoaded;
        _navigationService.NavigationRequested += OnNavigationRequested;
        Closed += OnClosed;
    }

    private void OnLoaded(object sender, RoutedEventArgs e)
    {
        if (ContentFrame.Content is null)
        {
            NavigateTo("inbox");
        }
    }

    private void OnClosed(object sender, WindowEventArgs args)
    {
        _navigationService.NavigationRequested -= OnNavigationRequested;
    }

    private void OnNavigationRequested(object? sender, NavigationRequest request)
    {
        if (request.Target == "viewer")
        {
            _workspaceState.SetSelectedDocument(request.Payload);
        }

        NavigateTo(request.Target);
    }

    private void OnNavigationSelectionChanged(NavigationView sender, NavigationViewSelectionChangedEventArgs args)
    {
        if (args.SelectedItemContainer?.Tag is string tag)
        {
            NavigateTo(tag);
        }
    }

    private void NavigateTo(string tag)
    {
        _viewModel.StatusText = tag switch
        {
            "inbox" => "Inbox geöffnet",
            "search" => "Suche geöffnet",
            "viewer" => string.IsNullOrWhiteSpace(_workspaceState.SelectedDocumentId)
                ? "Viewer geöffnet"
                : $"Dokument geöffnet: {_workspaceState.SelectedDocumentId}",
            _ => "Bereit"
        };

        RootNavigationView.SelectedItem = RootNavigationView.MenuItems
            .OfType<NavigationViewItem>()
            .FirstOrDefault(x => string.Equals(x.Tag as string, tag, StringComparison.Ordinal));

        var page = tag switch
        {
            "search" => _serviceProvider.GetRequiredService<SearchPage>(),
            "viewer" => _serviceProvider.GetRequiredService<ViewerPage>(),
            _ => _serviceProvider.GetRequiredService<InboxPage>()
        };

        ContentFrame.Content = page;
    }
}
