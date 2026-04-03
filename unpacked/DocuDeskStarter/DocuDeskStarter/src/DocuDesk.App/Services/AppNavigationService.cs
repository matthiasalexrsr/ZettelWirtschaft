namespace DocuDesk.App.Services;

public sealed class AppNavigationService
{
    public event EventHandler<NavigationRequest>? NavigationRequested;

    public void NavigateToInbox() =>
        NavigationRequested?.Invoke(this, new NavigationRequest("inbox"));

    public void NavigateToSearch(string? initialQuery = null) =>
        NavigationRequested?.Invoke(this, new NavigationRequest("search", initialQuery));

    public void NavigateToViewer(string documentId) =>
        NavigationRequested?.Invoke(this, new NavigationRequest("viewer", documentId));
}

public sealed record NavigationRequest(string Target, string? Payload = null);
