using System;
using System.Linq;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Layout;
using Avalonia.Media;

namespace AStar.Dev.CloudSyncFunctional.Controls;

/// <summary>A card showing today's sync throughput with a 24-bar hourly histogram.</summary>
public partial class ThroughputCard : UserControl
{
    private const int BucketCount = 24;
    private const double HistogramHeight = 28.0;
    private const double MinBarHeight = 2.0;
    private const int HistoryBucketCount = 20;

    /// <summary>Identifies the <see cref="Buckets"/> styled property.</summary>
    public static readonly StyledProperty<int[]> BucketsProperty = AvaloniaProperty.Register<ThroughputCard, int[]>(nameof(Buckets), new int[BucketCount]);

    /// <summary>Identifies the <see cref="TodayGb"/> styled property.</summary>
    public static readonly StyledProperty<double> TodayGbProperty = AvaloniaProperty.Register<ThroughputCard, double>(nameof(TodayGb));

    /// <summary>Identifies the <see cref="FileCount"/> styled property.</summary>
    public static readonly StyledProperty<int> FileCountProperty = AvaloniaProperty.Register<ThroughputCard, int>(nameof(FileCount));

    /// <summary>Identifies the <see cref="CurrentRate"/> styled property.</summary>
    public static readonly StyledProperty<string> CurrentRateProperty = AvaloniaProperty.Register<ThroughputCard, string>(nameof(CurrentRate), string.Empty);

    private IBrush? _ink3Brush;
    private IBrush? _primaryBrush;
    private IBrush? _ink3BrushSemiTransparent;

    /// <summary>Gets or sets the 24 hourly bucket values for the histogram. Array length must be 24.</summary>
    public int[] Buckets
    {
        get => GetValue(BucketsProperty);
        set => SetValue(BucketsProperty, value);
    }

    /// <summary>Gets or sets the total gigabytes transferred today.</summary>
    public double TodayGb
    {
        get => GetValue(TodayGbProperty);
        set => SetValue(TodayGbProperty, value);
    }

    /// <summary>Gets or sets the total number of files transferred today.</summary>
    public int FileCount
    {
        get => GetValue(FileCountProperty);
        set => SetValue(FileCountProperty, value);
    }

    /// <summary>Gets or sets the current transfer rate string, e.g. "142 KB/s".</summary>
    public string CurrentRate
    {
        get => GetValue(CurrentRateProperty);
        set => SetValue(CurrentRateProperty, value);
    }

    /// <summary>Initializes a new instance of <see cref="ThroughputCard"/>.</summary>
    public ThroughputCard() => InitializeComponent();

    /// <summary>Computes the pixel height of a histogram bar clamped to the given range.</summary>
    /// <param name="bucketValue">The value for this bar's bucket.</param>
    /// <param name="maxValue">The maximum value across all buckets.</param>
    /// <param name="histogramHeight">The total available pixel height.</param>
    /// <param name="minBarHeight">The minimum pixel height for any bar.</param>
    /// <returns>The pixel height for the bar, at least <paramref name="minBarHeight"/>.</returns>
    public static double ComputeBarHeight(int bucketValue, int maxValue, double histogramHeight, double minBarHeight) =>
        maxValue > 0
            ? Math.Max(minBarHeight, histogramHeight * bucketValue / maxValue)
            : minBarHeight;

    /// <summary>Returns whether a bar at the given index is a history bar (not the current period).</summary>
    /// <param name="barIndex">The zero-based index of the bar.</param>
    /// <returns><see langword="true"/> when the bar is a history bar (index &lt; 20).</returns>
    public static bool IsHistoryBar(int barIndex) => barIndex < HistoryBucketCount;

    /// <inheritdoc/>
    protected override void OnInitialized()
    {
        base.OnInitialized();
        UpdateColors();
        UpdateRate();
        UpdateTotals();
        BuildHistogram();
    }

    /// <inheritdoc/>
    protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
    {
        base.OnPropertyChanged(change);
        if (change.Property == BucketsProperty) BuildHistogram();
        if (change.Property == CurrentRateProperty) UpdateRate();
        if (change.Property == TodayGbProperty || change.Property == FileCountProperty) UpdateTotals();
    }

    private void UpdateRate()
    {
        if (RateText is null) return;

        RateText.Text = $"↓ {CurrentRate}";

        if (this.TryFindResource("Primary", out var res) && res is IBrush primaryBrush)
            RateText.Foreground = primaryBrush;
    }

    private void UpdateTotals()
    {
        if (GbText is null || FilesText is null) return;

        GbText.Text = TodayGb.ToString("F2");
        FilesText.Text = $"GB · {FileCount:N0} files";
    }

    private void UpdateColors()
    {
        if (CardBorder is null) return;

        if (this.TryFindResource("Border", out var borderRes) && borderRes is IBrush borderBrush)
            CardBorder.BorderBrush = borderBrush;

        if (this.TryFindResource("Surface", out var surfaceRes) && surfaceRes is IBrush surfaceBrush)
            CardBorder.Background = surfaceBrush;

        if (this.TryFindResource("Ink3", out var ink3Res) && ink3Res is IBrush ink3Brush)
        {
            _ink3Brush = ink3Brush;
            _ink3BrushSemiTransparent = new SolidColorBrush(((SolidColorBrush)ink3Brush).Color, 0.15);

            TodayLabel?.Foreground = ink3Brush;
            AxisStartLabel?.Foreground = ink3Brush;
            AxisMidLabel?.Foreground = ink3Brush;
            AxisEndLabel?.Foreground = ink3Brush;
        }

        if (this.TryFindResource("Primary", out var primaryRes) && primaryRes is IBrush primaryBrush)
        {
            _primaryBrush = primaryBrush;

            GbText?.Foreground = primaryBrush;
        }

        if (this.TryFindResource("Ink2", out var ink2Res) && ink2Res is IBrush ink2Brush)
            FilesText?.Foreground = ink2Brush;
    }

    private void BuildHistogram()
    {
        if (HistogramGrid is null) return;

        HistogramGrid.Children.Clear();

        var buckets = Buckets;
        var maxValue = buckets.Length > 0 ? buckets.Max() : 0;

        for (var i = 0; i < BucketCount; i++)
        {
            var rect = new Avalonia.Controls.Shapes.Rectangle
            {
                RadiusX = 1,
                RadiusY = 1,
                Height = ComputeBarHeight(buckets[i], maxValue, HistogramHeight, MinBarHeight),
                Margin = new Thickness(1, 0),
                VerticalAlignment = VerticalAlignment.Bottom,
                Fill = IsHistoryBar(i) ? _ink3BrushSemiTransparent : _primaryBrush
            };

            HistogramGrid.Children.Add(rect);
        }
    }
}
