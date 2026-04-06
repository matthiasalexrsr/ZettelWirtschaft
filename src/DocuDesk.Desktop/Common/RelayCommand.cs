using System.Windows.Input;

namespace DocuDesk.Desktop.Common;

public sealed class RelayCommand : ICommand
{
    private readonly Action? _execute;
    private readonly Func<Task>? _executeAsync;
    private readonly Func<bool>? _canExecute;

    public RelayCommand(Action execute, Func<bool>? canExecute = null)
    {
        _execute = execute;
        _canExecute = canExecute;
    }

    public RelayCommand(Func<Task> executeAsync, Func<bool>? canExecute = null)
    {
        _executeAsync = executeAsync;
        _canExecute = canExecute;
    }

    public event EventHandler? CanExecuteChanged;

    public Task? ExecutionTask { get; private set; }

    public bool CanExecute(object? parameter)
        => !IsExecuting && (_canExecute?.Invoke() ?? true);

    public bool IsExecuting => ExecutionTask is { IsCompleted: false };

    public void Execute(object? parameter)
    {
        if (_executeAsync is not null)
        {
            ExecutionTask = ExecuteAsync();
            return;
        }

        _execute?.Invoke();
    }

    private async Task ExecuteAsync()
    {
        RaiseCanExecuteChanged();
        try
        {
            await _executeAsync!();
        }
        finally
        {
            RaiseCanExecuteChanged();
        }
    }

    public void RaiseCanExecuteChanged() => CanExecuteChanged?.Invoke(this, EventArgs.Empty);
}
