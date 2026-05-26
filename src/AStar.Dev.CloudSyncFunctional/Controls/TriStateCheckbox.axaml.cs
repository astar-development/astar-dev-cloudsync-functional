using AStar.Dev.CloudSyncFunctional.FolderTree;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Media;

namespace AStar.Dev.CloudSyncFunctional.Controls;

/// <summary>A custom-drawn tri-state checkbox with Off, On, and Mixed states.</summary>
public partial class TriStateCheckbox : UserControl
{
    /// <summary>Identifies the <see cref="State"/> styled property.</summary>
    public static readonly StyledProperty<CheckState> StateProperty =
        AvaloniaProperty.Register<TriStateCheckbox, CheckState>(nameof(State), CheckState.Off);

    /// <summary>Gets or sets the current check state.</summary>
    public CheckState State
    {
        get => GetValue(StateProperty);
        set => SetValue(StateProperty, value);
    }

    /// <summary>Initializes a new instance of <see cref="TriStateCheckbox"/>.</summary>
    public TriStateCheckbox() => InitializeComponent();

    /// <inheritdoc/>
    protected override void OnInitialized()
    {
        base.OnInitialized();
        UpdateState();
    }

    /// <inheritdoc/>
    protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
    {
        base.OnPropertyChanged(change);
        if (change.Property == StateProperty) UpdateState();
    }

    private void UpdateState()
    {
        if (Box is null || Check is null || Bar is null) return;

        var isPrimary = State is CheckState.On or CheckState.Mixed;

        if (isPrimary && this.TryFindResource("Primary", out var primary) && primary is IBrush primaryBrush)
        {
            Box.Background = primaryBrush;
            Box.BorderBrush = primaryBrush;
        }
        else if (this.TryFindResource("BorderStrong", out var border) && border is IBrush borderBrush)
        {
            Box.Background = Brushes.Transparent;
            Box.BorderBrush = borderBrush;
        }

        Check.IsVisible = State == CheckState.On;
        Bar.IsVisible = State == CheckState.Mixed;
    }
}
