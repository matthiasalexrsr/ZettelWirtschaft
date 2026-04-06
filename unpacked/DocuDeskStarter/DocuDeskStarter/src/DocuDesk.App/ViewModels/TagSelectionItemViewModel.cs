namespace DocuDesk.App.ViewModels;

public sealed class TagSelectionItemViewModel : ObservableObject
{
    private bool _isSelected;

    public required string Id { get; init; }
    public required string Name { get; init; }

    public bool IsSelected
    {
        get => _isSelected;
        set => SetProperty(ref _isSelected, value);
    }
}
