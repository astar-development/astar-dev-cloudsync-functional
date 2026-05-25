using System;
using System.Windows.Input;
using ReactiveUI;

namespace AStar.Dev.CloudSyncFunctional;

public class MainWindowViewModel : ReactiveObject
{
    private string _statusText = "Ready to explore OneDrive functional workflows.";

    public string StatusText
    {
        get => _statusText;
        set => this.RaiseAndSetIfChanged(ref _statusText, value);
    }

    public ICommand UpdateStatusCommand { get; }

    public MainWindowViewModel()
    {
        UpdateStatusCommand = new DelegateCommand(() =>
        {
            StatusText = "ReactiveUI + Avalonia is connected.";
        });
    }

    private sealed class DelegateCommand(Action execute) : ICommand
    {
        public bool CanExecute(object? parameter) => true;

        public void Execute(object? parameter) => execute();

        public event EventHandler? CanExecuteChanged
        {
            add { }
            remove { }
        }
    }
}
