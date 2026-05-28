using Avalonia;
using Avalonia.Controls;
using Avalonia.Media;

namespace AStar.Dev.CloudSyncFunctional.Controls;

/// <summary>A fixed-height status strip at the bottom of the application showing health, throughput, queue, and version.</summary>
public partial class StatusBar : UserControl
{
    /// <summary>Identifies the <see cref="HealthText"/> styled property.</summary>
    public static readonly StyledProperty<string> HealthTextProperty = AvaloniaProperty.Register<StatusBar, string>(nameof(HealthText), "All accounts healthy");

    /// <summary>Identifies the <see cref="UploadRate"/> styled property.</summary>
    public static readonly StyledProperty<string> UploadRateProperty = AvaloniaProperty.Register<StatusBar, string>(nameof(UploadRate), "↑ 0 B/s");

    /// <summary>Identifies the <see cref="DownloadRate"/> styled property.</summary>
    public static readonly StyledProperty<string> DownloadRateProperty = AvaloniaProperty.Register<StatusBar, string>(nameof(DownloadRate), "↓ 0 B/s");

    /// <summary>Identifies the <see cref="QueueSummary"/> styled property.</summary>
    public static readonly StyledProperty<string> QueueSummaryProperty = AvaloniaProperty.Register<StatusBar, string>(nameof(QueueSummary), string.Empty);

    /// <summary>Identifies the <see cref="Version"/> styled property.</summary>
    public static readonly StyledProperty<string> VersionProperty = AvaloniaProperty.Register<StatusBar, string>(nameof(Version), string.Empty);

    /// <summary>Identifies the <see cref="IsHealthy"/> styled property.</summary>
    public static readonly StyledProperty<bool> IsHealthyProperty = AvaloniaProperty.Register<StatusBar, bool>(nameof(IsHealthy), true);

    /// <summary>Gets or sets the health status text.</summary>
    public string HealthText
    {
        get => GetValue(HealthTextProperty);
        set => SetValue(HealthTextProperty, value);
    }

    /// <summary>Gets or sets the upload rate display string.</summary>
    public string UploadRate
    {
        get => GetValue(UploadRateProperty);
        set => SetValue(UploadRateProperty, value);
    }

    /// <summary>Gets or sets the download rate display string.</summary>
    public string DownloadRate
    {
        get => GetValue(DownloadRateProperty);
        set => SetValue(DownloadRateProperty, value);
    }

    /// <summary>Gets or sets the queue summary display string.</summary>
    public string QueueSummary
    {
        get => GetValue(QueueSummaryProperty);
        set => SetValue(QueueSummaryProperty, value);
    }

    /// <summary>Gets or sets the application version display string.</summary>
    public string Version
    {
        get => GetValue(VersionProperty);
        set => SetValue(VersionProperty, value);
    }

    /// <summary>Gets or sets whether all accounts are healthy, which drives the health dot fill colour.</summary>
    public bool IsHealthy
    {
        get => GetValue(IsHealthyProperty);
        set => SetValue(IsHealthyProperty, value);
    }

    /// <summary>Initializes a new instance of <see cref="StatusBar"/>.</summary>
    public StatusBar() => InitializeComponent();

    /// <inheritdoc/>
    protected override void OnInitialized()
    {
        base.OnInitialized();
        UpdateBackground();
        UpdateBorder();
        UpdateHealthDot();
        UpdateHealthText();
        UpdateUploadText();
        UpdateDownloadText();
        UpdateQueueText();
        UpdateVersionText();
    }

    /// <inheritdoc/>
    protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
    {
        base.OnPropertyChanged(change);
        if (change.Property == IsHealthyProperty) UpdateHealthDot();
        if (change.Property == HealthTextProperty) UpdateHealthText();
        if (change.Property == UploadRateProperty) UpdateUploadText();
        if (change.Property == DownloadRateProperty) UpdateDownloadText();
        if (change.Property == QueueSummaryProperty) UpdateQueueText();
        if (change.Property == VersionProperty) UpdateVersionText();
    }

    private void UpdateBackground()
    {
        if (ContainerBorder is null) return;

        if (this.TryFindResource("SurfaceAlt", out var res) && res is IBrush brush)
            ContainerBorder.Background = brush;
    }

    private void UpdateBorder()
    {
        if (ContainerBorder is null) return;

        if (this.TryFindResource("Border", out var res) && res is IBrush borderBrush)
            ContainerBorder.BorderBrush = borderBrush;
    }

    private void UpdateHealthDot()
    {
        if (HealthDot is null) return;

        var colorKey = IsHealthy ? "Good" : "Warn";

        HealthDot.Fill = this.TryFindResource(colorKey, out var res) && res is IBrush brush
            ? brush
            : null;
    }

    private void UpdateHealthText()
    {
        if (HealthTextBlock is null) return;

        HealthTextBlock.Text = HealthText;

        if (this.TryFindResource("Ink3", out var res) && res is IBrush brush)
            HealthTextBlock.Foreground = brush;
    }

    private void UpdateUploadText()
    {
        if (UploadText is null) return;

        UploadText.Text = UploadRate;

        if (this.TryFindResource("Ink3", out var res) && res is IBrush brush)
            UploadText.Foreground = brush;
    }

    private void UpdateDownloadText()
    {
        if (DownloadText is null) return;

        DownloadText.Text = DownloadRate;

        if (this.TryFindResource("Primary", out var res) && res is IBrush brush)
            DownloadText.Foreground = brush;
    }

    private void UpdateQueueText()
    {
        if (QueueText is null) return;

        QueueText.Text = QueueSummary;

        if (this.TryFindResource("Ink3", out var res) && res is IBrush brush)
            QueueText.Foreground = brush;
    }

    private void UpdateVersionText()
    {
        if (VersionText is null) return;

        VersionText.Text = Version;

        if (this.TryFindResource("Ink3", out var res) && res is IBrush brush)
            VersionText.Foreground = brush;
    }
}
