namespace DocuDesk.App.ViewModels;

public sealed class MainWindowViewModel : ObservableObject
{
    private string _statusText = "Bereit";

    public string AppTitle => "DocuDesk";
    public string Subtitle => "Dokumentenverwaltung für gescannte Briefe";

    public string StatusText
    {
        get => _statusText;
        set => SetProperty(ref _statusText, value);
    }
}
