using System;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Media;

namespace AStar.Dev.CloudSyncFunctional.Controls;

/// <summary>A thin two-segment progress bar for displaying storage utilisation.</summary>
public partial class StorageBar : UserControl
{
    /// <summary>Identifies the <see cref="Used"/> styled property.</summary>
    public static readonly StyledProperty<double> UsedProperty = AvaloniaProperty.Register<StorageBar, double>(nameof(Used));

    /// <summary>Identifies the <see cref="Total"/> styled property.</summary>
    public static readonly StyledProperty<double> TotalProperty = AvaloniaProperty.Register<StorageBar, double>(nameof(Total));

    /// <summary>Identifies the <see cref="BarColor"/> styled property.</summary>
    public static readonly StyledProperty<IBrush?> BarColorProperty = AvaloniaProperty.Register<StorageBar, IBrush?>(nameof(BarColor));

    /// <summary>Gets or sets the bytes currently used.</summary>
    public double Used
    {
        get => GetValue(UsedProperty);
        set => SetValue(UsedProperty, value);
    }

    /// <summary>Gets or sets the total storage capacity in bytes.</summary>
    public double Total
    {
        get => GetValue(TotalProperty);
        set => SetValue(TotalProperty, value);
    }

    /// <summary>Gets or sets the fill colour for the used portion of the bar.</summary>
    public IBrush? BarColor
    {
        get => GetValue(BarColorProperty);
        set => SetValue(BarColorProperty, value);
    }

    /// <summary>Initializes a new instance of <see cref="StorageBar"/>.</summary>
    public StorageBar()
    {
        InitializeComponent();
        SizeChanged += (_, _) => UpdateBar();
    }

    /// <inheritdoc/>
    protected override void OnInitialized()
    {
        base.OnInitialized();
        if (TrackBar is not null && this.TryFindResource("Border", out var res) && res is IBrush trackBrush)
            TrackBar.Fill = trackBrush;

        UpdateBar();
    }

    /// <inheritdoc/>
    protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
    {
        base.OnPropertyChanged(change);
        if (change.Property == UsedProperty || change.Property == TotalProperty || change.Property == BarColorProperty)
            UpdateBar();
    }

    /// <summary>Computes the fill fraction clamped to [0, 1].</summary>
    /// <param name="used">The bytes currently used.</param>
    /// <param name="total">The total capacity in bytes.</param>
    /// <returns>A value between 0.0 and 1.0 representing the used fraction.</returns>
    public static double ComputeFraction(double used, double total) =>
        total > 0 ? Math.Clamp(used / total, 0.0, 1.0) : 0.0;

    private void UpdateBar()
    {
        if (FillBar is null) return;

        FillBar.Fill = BarColor;
        FillBar.Width = Bounds.Width * ComputeFraction(Used, Total);
    }
}
