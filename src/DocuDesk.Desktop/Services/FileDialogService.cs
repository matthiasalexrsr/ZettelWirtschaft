using Microsoft.Win32;

namespace DocuDesk.Desktop.Services;

public sealed class FileDialogService : IFileDialogService
{
    public string[] PickImportFiles()
    {
        var dialog = new OpenFileDialog
        {
            Multiselect = true,
            Filter = "Dokumente|*.pdf;*.png;*.jpg;*.jpeg;*.tif;*.tiff;*.txt|Alle Dateien|*.*"
        };

        return dialog.ShowDialog() == true ? dialog.FileNames : Array.Empty<string>();
    }
}
