namespace DocuDesk.App.Services;

public sealed class DocumentWorkspaceState
{
    private string? _selectedDocumentId;

    public event EventHandler<string?>? SelectedDocumentChanged;

    public string? SelectedDocumentId => _selectedDocumentId;

    public void SetSelectedDocument(string? documentId)
    {
        if (string.Equals(_selectedDocumentId, documentId, StringComparison.Ordinal))
        {
            return;
        }

        _selectedDocumentId = documentId;
        SelectedDocumentChanged?.Invoke(this, documentId);
    }
}
