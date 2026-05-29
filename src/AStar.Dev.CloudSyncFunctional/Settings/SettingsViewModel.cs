using ReactiveUI;
using RxUnit = System.Reactive.Unit;

namespace AStar.Dev.CloudSyncFunctional.Settings;

/// <summary>View-model for the settings overlay.</summary>
public sealed class SettingsViewModel : ReactiveObject
{
    /// <summary>Gets the command that closes the settings overlay.</summary>
    public ReactiveCommand<RxUnit, RxUnit> Close { get; }

    /// <summary>Raised when the user closes the settings overlay.</summary>
    public event EventHandler? Closed;

    /// <summary>Initialises a new <see cref="SettingsViewModel"/>.</summary>
    public SettingsViewModel() => Close = ReactiveCommand.Create(ExecuteClose);

    private void ExecuteClose() => Closed?.Invoke(this, EventArgs.Empty);
}
