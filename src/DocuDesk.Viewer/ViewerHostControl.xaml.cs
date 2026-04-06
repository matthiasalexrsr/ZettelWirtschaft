using System.IO;
using System.Text.Json;
using DocuDesk.Contracts.Viewer;
using System.Windows;
using System.Windows.Controls;
using Microsoft.Web.WebView2.Core;

namespace DocuDesk.Viewer;

public partial class ViewerHostControl : UserControl
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        PropertyNameCaseInsensitive = true
    };

    public event EventHandler<ViewerAnnotationCreatedEventArgs>? AnnotationCreated;
    private bool _isInitialized;

    public static readonly DependencyProperty SourcePathProperty = DependencyProperty.Register(
        nameof(SourcePath),
        typeof(string),
        typeof(ViewerHostControl),
        new PropertyMetadata(null, OnViewerPayloadChanged));

    public static readonly DependencyProperty OverlayJsonProperty = DependencyProperty.Register(
        nameof(OverlayJson),
        typeof(string),
        typeof(ViewerHostControl),
        new PropertyMetadata(null, OnViewerPayloadChanged));

    public static readonly DependencyProperty HighlightQueryProperty = DependencyProperty.Register(
        nameof(HighlightQuery),
        typeof(string),
        typeof(ViewerHostControl),
        new PropertyMetadata(null, OnViewerPayloadChanged));

    public ViewerHostControl()
    {
        InitializeComponent();
        Loaded += OnLoaded;
    }

    public string? SourcePath
    {
        get => (string?)GetValue(SourcePathProperty);
        set => SetValue(SourcePathProperty, value);
    }

    public string? OverlayJson
    {
        get => (string?)GetValue(OverlayJsonProperty);
        set => SetValue(OverlayJsonProperty, value);
    }

    public string? HighlightQuery
    {
        get => (string?)GetValue(HighlightQueryProperty);
        set => SetValue(HighlightQueryProperty, value);
    }

    private async void OnLoaded(object sender, RoutedEventArgs e)
    {
        if (_isInitialized)
        {
            await PostOpenMessageAsync();
            return;
        }

        try
        {
            await PART_WebView.EnsureCoreWebView2Async();
            PART_WebView.CoreWebView2.WebMessageReceived += OnWebMessageReceived;
            var assetsPath = Path.Combine(AppContext.BaseDirectory, "viewer-assets");
            PART_WebView.CoreWebView2.SetVirtualHostNameToFolderMapping(
                "app.docudesk.viewer",
                assetsPath,
                CoreWebView2HostResourceAccessKind.DenyCors);
            PART_WebView.Source = new Uri("https://app.docudesk.viewer/index.html");
            _isInitialized = true;
            await PostOpenMessageAsync();
        }
        catch (Exception ex)
        {
            LogViewerInitializationFailure(ex);
            PART_WebView.Visibility = Visibility.Collapsed;
            PART_ErrorText.Text = $"Viewer konnte nicht initialisiert werden: {ex.Message}";
            PART_ErrorOverlay.Visibility = Visibility.Visible;
        }
    }

    private static void LogViewerInitializationFailure(Exception ex)
    {
        try
        {
            var logsDir = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                "DocuDesk",
                "logs");
            Directory.CreateDirectory(logsDir);
            var logFile = Path.Combine(logsDir, "viewer-init.log");
            var entry = $"[{DateTime.UtcNow:O}] [ViewerHostControl.OnLoaded] {ex}\n\n";
            File.AppendAllText(logFile, entry);
        }
        catch
        {
            // Logging must never crash UI startup.
        }
    }

    private void OnWebMessageReceived(object? sender, CoreWebView2WebMessageReceivedEventArgs e)
    {
        try
        {
            using var document = JsonDocument.Parse(e.WebMessageAsJson);
            var root = document.RootElement;
            if (!root.TryGetProperty("type", out var typeEl))
            {
                return;
            }

            switch (typeEl.GetString())
            {
                case "copyText":
                {
                    var text = root.TryGetProperty("text", out var textEl) ? textEl.GetString() : null;
                    if (!string.IsNullOrWhiteSpace(text))
                    {
                        Clipboard.SetText(text);
                    }
                    break;
                }
                case "annotationCreated":
                {
                    if (!root.TryGetProperty("annotation", out var annotationEl))
                    {
                        return;
                    }

                    var request = annotationEl.Deserialize<ViewerAnnotationCreateRequest>(JsonOptions);
                    if (request is not null)
                    {
                        AnnotationCreated?.Invoke(this, new ViewerAnnotationCreatedEventArgs(request));
                    }
                    break;
                }
            }
        }
        catch
        {
            // Swallow viewer-bridge parsing errors in V1.
        }
    }

    private static async void OnViewerPayloadChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is ViewerHostControl control && control._isInitialized)
        {
            await control.PostOpenMessageAsync();
        }
    }

    private async Task PostOpenMessageAsync()
    {
        if (!_isInitialized || PART_WebView.CoreWebView2 is null)
        {
            return;
        }

        object? overlay = null;
        if (!string.IsNullOrWhiteSpace(OverlayJson))
        {
            try
            {
                overlay = JsonSerializer.Deserialize<object>(OverlayJson!, JsonOptions);
            }
            catch
            {
                overlay = null;
            }
        }

        var payload = JsonSerializer.Serialize(new
        {
            type = "openDocument",
            path = SourcePath ?? string.Empty,
            overlay,
            highlightQuery = HighlightQuery ?? string.Empty
        }, JsonOptions);
        PART_WebView.CoreWebView2.PostWebMessageAsJson(payload);
        await Task.CompletedTask;
    }
}
