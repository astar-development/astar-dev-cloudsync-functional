using Avalonia;
using Avalonia.Controls;
using Avalonia.Media;

namespace AStar.Dev.CloudSyncFunctional.Controls;

/// <summary>An 8 px circle indicator with an optional repeating pulse animation.</summary>
public partial class StatusDot : UserControl
{
    /// <summary>Identifies the <see cref="Fill"/> styled property.</summary>
    public static readonly StyledProperty<IBrush?> FillProperty = AvaloniaProperty.Register<StatusDot, IBrush?>(nameof(Fill));

    /// <summary>Identifies the <see cref="IsPulsing"/> styled property.</summary>
    public static readonly StyledProperty<bool> IsPulsingProperty = AvaloniaProperty.Register<StatusDot, bool>(nameof(IsPulsing));

    /// <summary>Gets or sets the fill brush for the dot.</summary>
    public IBrush? Fill
    {
        get => GetValue(FillProperty);
        set => SetValue(FillProperty, value);
    }

    /// <summary>Gets or sets whether the dot pulses repeatedly.</summary>
    public bool IsPulsing
    {
        get => GetValue(IsPulsingProperty);
        set => SetValue(IsPulsingProperty, value);
    }

    /// <summary>Initializes a new instance of <see cref="StatusDot"/>.</summary>
    public StatusDot() => InitializeComponent();

    /// <inheritdoc/>
    protected override void OnInitialized()
    {
        base.OnInitialized();
        UpdateFill();
        UpdatePulse();
    }

    /// <inheritdoc/>
    protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
    {
        base.OnPropertyChanged(change);
        if (change.Property == FillProperty) UpdateFill();
        if (change.Property == IsPulsingProperty) UpdatePulse();
    }

    private void UpdateFill()
    {
        if (Dot is null) return;

        Dot.Fill = Fill;
    }

    private void UpdatePulse()
    {
        if (IsPulsing)
            Classes.Add("Pulsing");
        else
            Classes.Remove("Pulsing");
    }
}
