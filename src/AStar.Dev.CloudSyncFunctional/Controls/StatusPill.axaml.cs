using Avalonia;
using Avalonia.Controls;
using Avalonia.Media;

namespace AStar.Dev.CloudSyncFunctional.Controls;

/// <summary>A small rounded-rectangle badge with a semantic tone and label text.</summary>
public partial class StatusPill : UserControl
{
    /// <summary>Identifies the <see cref="Label"/> styled property.</summary>
    public static readonly StyledProperty<string> LabelProperty = AvaloniaProperty.Register<StatusPill, string>(nameof(Label), string.Empty);

    /// <summary>Identifies the <see cref="Tone"/> styled property.</summary>
    public static readonly StyledProperty<Tone> ToneProperty = AvaloniaProperty.Register<StatusPill, Tone>(nameof(Tone), Tone.Neutral);

    /// <summary>Gets or sets the display text for the pill.</summary>
    public string Label
    {
        get => GetValue(LabelProperty);
        set => SetValue(LabelProperty, value);
    }

    /// <summary>Gets or sets the semantic tone that drives the pill colour.</summary>
    public Tone Tone
    {
        get => GetValue(ToneProperty);
        set => SetValue(ToneProperty, value);
    }

    /// <summary>Initializes a new instance of <see cref="StatusPill"/>.</summary>
    public StatusPill() => InitializeComponent();

    /// <inheritdoc/>
    protected override void OnInitialized()
    {
        base.OnInitialized();
        UpdateLabel();
        UpdateColors();
    }

    /// <inheritdoc/>
    protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
    {
        base.OnPropertyChanged(change);
        if (change.Property == LabelProperty) UpdateLabel();
        if (change.Property == ToneProperty) UpdateColors();
    }

    private void UpdateLabel()
    {
        if (LabelText is null) return;

        LabelText.Text = Label;
    }

    private void UpdateColors()
    {
        if (Container is null || LabelText is null) return;

        var (bgKey, usesWhiteFg) = Tone switch
        {
            Tone.Good => ("Good", true),
            Tone.Warn => ("Warn", true),
            Tone.Primary => ("Primary", true),
            Tone.Accent => ("Accent", true),
            Tone.Danger => ("Danger", true),
            _ => ("SurfaceAlt", false)
        };

        if (this.TryFindResource(bgKey, out var bg) && bg is IBrush bgBrush)
            Container.Background = bgBrush;

        if (usesWhiteFg)
        {
            LabelText.Foreground = Brushes.White;
        }
        else if (this.TryFindResource("Ink3", out var fg) && fg is IBrush fgBrush)
        {
            LabelText.Foreground = fgBrush;
        }
    }
}
