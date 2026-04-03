using System.Windows;
using DocuDesk.Desktop.ViewModels;

namespace DocuDesk.Desktop;

public partial class MainWindow : Window
{
    public MainWindow()
    {
        InitializeComponent();
        Loaded += OnLoaded;
    }

    private void OnLoaded(object sender, RoutedEventArgs e)
    {
        DocumentViewer.AnnotationCreated -= OnViewerAnnotationCreated;
        DocumentViewer.AnnotationCreated += OnViewerAnnotationCreated;
    }

    private async void OnViewerAnnotationCreated(object? sender, DocuDesk.Viewer.ViewerAnnotationCreatedEventArgs e)
    {
        if (DataContext is MainViewModel vm)
        {
            await vm.HandleViewerAnnotationCreatedAsync(e.Request);
        }
    }
}
