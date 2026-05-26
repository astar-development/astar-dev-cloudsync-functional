using AStar.Dev.CloudSyncFunctional.Accounts;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Media;

namespace AStar.Dev.CloudSyncFunctional.Controls;

/// <summary>A rounded-square badge showing a provider initial letter and accent dot.</summary>
public partial class ProviderMark : UserControl
{
    /// <summary>Identifies the <see cref="Kind"/> styled property.</summary>
    public static readonly StyledProperty<ProviderKind> KindProperty =
        AvaloniaProperty.Register<ProviderMark, ProviderKind>(nameof(Kind));

    /// <summary>Identifies the <see cref="Size"/> styled property.</summary>
    public static readonly StyledProperty<double> SizeProperty =
        AvaloniaProperty.Register<ProviderMark, double>(nameof(Size), 26.0);

    /// <summary>Gets or sets the cloud provider this mark represents.</summary>
    public ProviderKind Kind
    {
        get => GetValue(KindProperty);
        set => SetValue(KindProperty, value);
    }

    /// <summary>Gets or sets the width and height of the mark in pixels.</summary>
    public double Size
    {
        get => GetValue(SizeProperty);
        set => SetValue(SizeProperty, value);
    }

    /// <summary>Initializes a new instance of <see cref="ProviderMark"/>.</summary>
    public ProviderMark() => InitializeComponent();

    /// <inheritdoc/>
    protected override void OnInitialized()
    {
        base.OnInitialized();
        UpdateVisuals();
    }

    /// <inheritdoc/>
    protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
    {
        base.OnPropertyChanged(change);
        if (change.Property == KindProperty || change.Property == SizeProperty)
            UpdateVisuals();
    }

    /// <summary>Returns the initial letter and resource keys for the given provider.</summary>
    /// <param name="kind">The provider kind to look up.</param>
    /// <returns>A tuple of (letter, background resource key, foreground resource key).</returns>
    public static (string Letter, string BgKey, string FgKey) GetProviderInfo(ProviderKind kind) =>
        kind switch
        {
            ProviderKind.OneDrive => ("O", "OneDriveAccentWeak", "OneDriveAccent"),
            ProviderKind.GoogleDrive => ("G", "GoogleDriveAccentWeak", "GoogleDriveAccent"),
            ProviderKind.Dropbox => ("D", "DropboxAccentWeak", "DropboxAccent"),
            _ => ("?", "Ink3", "Ink")
        };

    private void UpdateVisuals()
    {
        if (MarkBg is null || Initial is null || AccentDot is null) return;

        Width = Size;
        Height = Size;
        MarkBg.CornerRadius = new CornerRadius(Size * 0.26);
        Initial.FontSize = Size * 0.5;

        var (letter, bgKey, fgKey) = GetProviderInfo(Kind);
        Initial.Text = letter;

        if (this.TryFindResource(bgKey, out var bg) && bg is IBrush bgBrush)
            MarkBg.Background = bgBrush;

        if (this.TryFindResource(fgKey, out var fg) && fg is IBrush fgBrush)
            Initial.Foreground = fgBrush;

        if (this.TryFindResource("Accent", out var accent) && accent is IBrush accentBrush)
            AccentDot.Fill = accentBrush;
    }
}
