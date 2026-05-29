using System;
using AStar.Dev.CloudSyncFunctional.FolderTree;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Media;
using Lucide.Avalonia;

namespace AStar.Dev.CloudSyncFunctional.Controls;

/// <summary>A single row in the folder tree showing name, selection state, size, and last sync time.</summary>
public partial class FolderTreeRow : UserControl
{
    private const long GigaByte = 1_073_741_824L;
    private const long MegaByte = 1_048_576L;

    /// <summary>Identifies the <see cref="NodeName"/> styled property.</summary>
    public static readonly StyledProperty<string> NodeNameProperty = AvaloniaProperty.Register<FolderTreeRow, string>(nameof(NodeName), string.Empty);

    /// <summary>Identifies the <see cref="Depth"/> styled property.</summary>
    public static readonly StyledProperty<int> DepthProperty = AvaloniaProperty.Register<FolderTreeRow, int>(nameof(Depth));

    /// <summary>Identifies the <see cref="ChildCount"/> styled property.</summary>
    public static readonly StyledProperty<int> ChildCountProperty = AvaloniaProperty.Register<FolderTreeRow, int>(nameof(ChildCount));

    /// <summary>Identifies the <see cref="SizeBytes"/> styled property.</summary>
    public static readonly StyledProperty<long> SizeBytesProperty = AvaloniaProperty.Register<FolderTreeRow, long>(nameof(SizeBytes));

    /// <summary>Identifies the <see cref="LastSync"/> styled property.</summary>
    public static readonly StyledProperty<DateTimeOffset> LastSyncProperty = AvaloniaProperty.Register<FolderTreeRow, DateTimeOffset>(nameof(LastSync));

    /// <summary>Identifies the <see cref="SelectionState"/> styled property.</summary>
    public static readonly StyledProperty<CheckState> SelectionStateProperty = AvaloniaProperty.Register<FolderTreeRow, CheckState>(nameof(SelectionState));

    /// <summary>Identifies the <see cref="IsExpanded"/> styled property.</summary>
    public static readonly StyledProperty<bool> IsExpandedProperty = AvaloniaProperty.Register<FolderTreeRow, bool>(nameof(IsExpanded));

    /// <summary>Identifies the <see cref="IsSyncing"/> styled property.</summary>
    public static readonly StyledProperty<bool> IsSyncingProperty = AvaloniaProperty.Register<FolderTreeRow, bool>(nameof(IsSyncing));

    /// <summary>Identifies the <see cref="HasChildren"/> styled property.</summary>
    public static readonly StyledProperty<bool> HasChildrenProperty = AvaloniaProperty.Register<FolderTreeRow, bool>(nameof(HasChildren));

    /// <summary>Gets or sets the display name of the folder.</summary>
    public string NodeName
    {
        get => GetValue(NodeNameProperty);
        set => SetValue(NodeNameProperty, value);
    }

    /// <summary>Gets or sets the nesting depth; 0 is a top-level folder.</summary>
    public int Depth
    {
        get => GetValue(DepthProperty);
        set => SetValue(DepthProperty, value);
    }

    /// <summary>Gets or sets the number of child items.</summary>
    public int ChildCount
    {
        get => GetValue(ChildCountProperty);
        set => SetValue(ChildCountProperty, value);
    }

    /// <summary>Gets or sets the folder size in bytes.</summary>
    public long SizeBytes
    {
        get => GetValue(SizeBytesProperty);
        set => SetValue(SizeBytesProperty, value);
    }

    /// <summary>Gets or sets the date and time of the last sync.</summary>
    public DateTimeOffset LastSync
    {
        get => GetValue(LastSyncProperty);
        set => SetValue(LastSyncProperty, value);
    }

    /// <summary>Gets or sets the tri-state selection state of the folder.</summary>
    public CheckState SelectionState
    {
        get => GetValue(SelectionStateProperty);
        set => SetValue(SelectionStateProperty, value);
    }

    /// <summary>Gets or sets whether the folder node is expanded to show children.</summary>
    public bool IsExpanded
    {
        get => GetValue(IsExpandedProperty);
        set => SetValue(IsExpandedProperty, value);
    }

    /// <summary>Gets or sets whether this folder is currently being synced.</summary>
    public bool IsSyncing
    {
        get => GetValue(IsSyncingProperty);
        set => SetValue(IsSyncingProperty, value);
    }

    /// <summary>Gets or sets whether this folder has child nodes, which drives chevron visibility.</summary>
    public bool HasChildren
    {
        get => GetValue(HasChildrenProperty);
        set => SetValue(HasChildrenProperty, value);
    }

    /// <summary>Initializes a new instance of <see cref="FolderTreeRow"/>.</summary>
    public FolderTreeRow() => InitializeComponent();

    /// <summary>Formats a byte count as a human-readable size string.</summary>
    /// <param name="bytes">The number of bytes to format.</param>
    /// <returns>"X.X GB", "X MB", or "—" depending on magnitude.</returns>
    public static string FormatSize(long bytes) =>
        bytes switch
        {
            >= GigaByte => $"{(bytes / (double)GigaByte):F1} GB",
            >= MegaByte => $"{bytes / MegaByte} MB",
            _ => "—"
        };

    /// <summary>Formats a sync timestamp as a relative time string.</summary>
    /// <param name="lastSync">The time of last sync.</param>
    /// <returns>"—", "now", "X m", "Xh", or "X d" depending on elapsed time.</returns>
    public static string FormatLastSync(DateTimeOffset lastSync)
    {
        if (lastSync == DateTimeOffset.MinValue)
            return "—";

        var elapsed = DateTimeOffset.UtcNow - lastSync;

        return elapsed.TotalSeconds switch
        {
            < 60 => "now",
            < 3600 => $"{(int)elapsed.TotalMinutes} m",
            < 86400 => $"{(int)elapsed.TotalHours}h",
            _ => $"{(int)elapsed.TotalDays} d"
        };
    }

    /// <inheritdoc/>
    protected override void OnInitialized()
    {
        base.OnInitialized();
        UpdateBackground();
        UpdateChevron();
        UpdateCheckbox();
        UpdateName();
        UpdateItemCount();
        UpdateSize();
        UpdateLastSync();
        UpdateColors();
        UpdateIndent();
    }

    /// <inheritdoc/>
    protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
    {
        base.OnPropertyChanged(change);
        if (change.Property == IsSyncingProperty) { UpdateBackground(); UpdateSyncingPill(); }
        if (change.Property == IsExpandedProperty || change.Property == HasChildrenProperty) UpdateChevron();
        if (change.Property == SelectionStateProperty) { UpdateCheckbox(); UpdateColors(); }
        if (change.Property == NodeNameProperty || change.Property == DepthProperty) UpdateName();
        if (change.Property == ChildCountProperty) UpdateItemCount();
        if (change.Property == SizeBytesProperty) UpdateSize();
        if (change.Property == LastSyncProperty) UpdateLastSync();
        if (change.Property == DepthProperty) UpdateIndent();
    }

    private void UpdateBackground()
    {
        if (RowBackground is null) return;

        RowBackground.Background = IsSyncing && this.TryFindResource("PrimaryWeak", out var res) && res is IBrush brush
            ? brush
            : Brushes.Transparent;
    }

    private void UpdateChevron()
    {
        if (ChevronIcon is null) return;

        ChevronIcon.IsVisible = HasChildren;
        ChevronIcon.Kind = IsExpanded ? LucideIconKind.ChevronDown : LucideIconKind.ChevronRight;

        if (this.TryFindResource("Ink3", out var res) && res is IBrush ink3Brush)
            ChevronIcon.Foreground = ink3Brush;
    }

    private void UpdateCheckbox()
    {
        if (CheckboxControl is null) return;

        CheckboxControl.State = SelectionState;
    }

    private void UpdateName()
    {
        if (NameText is null) return;

        NameText.Text = NodeName;
        NameText.FontSize = 12.5;
        NameText.FontWeight = Depth == 0 ? FontWeight.SemiBold : FontWeight.Medium;

        var isPath = NodeName.EndsWith('/');
        NameText.FontFamily = isPath
            ? this.TryFindResource("JetBrainsMono", out var mono) && mono is FontFamily monoFamily ? monoFamily : FontFamily.Default
            : this.TryFindResource("PlusJakartaSans", out var sans) && sans is FontFamily sansFamily ? sansFamily : FontFamily.Default;

        if (this.TryFindResource("Ink", out var inkRes) && inkRes is IBrush inkBrush)
            NameText.Foreground = inkBrush;
    }

    private void UpdateSyncingPill()
    {
        if (SyncingPill is null) return;

        SyncingPill.IsVisible = IsSyncing;
    }

    private void UpdateItemCount()
    {
        if (ItemCountText is null) return;

        ItemCountText.Text = ChildCount.ToString();
    }

    private void UpdateSize()
    {
        if (SizeText is null) return;

        SizeText.Text = FormatSize(SizeBytes);
    }

    private void UpdateLastSync()
    {
        if (UpdatedText is null) return;

        UpdatedText.Text = FormatLastSync(LastSync);
    }

    private void UpdateColors()
    {
        var isActive = SelectionState != CheckState.Off;

        if (FolderIcon is not null)
        {
            var iconKey = isActive ? "Primary" : "Ink3";
            if (this.TryFindResource(iconKey, out var iconRes) && iconRes is IBrush iconBrush)
                FolderIcon.Foreground = iconBrush;
        }

        IBrush? ink3Brush = null;
        if (this.TryFindResource("Ink3", out var res) && res is IBrush b)
            ink3Brush = b;

        if (ink3Brush is null) return;

        ItemCountText?.Foreground = ink3Brush;
        SizeText?.Foreground = ink3Brush;
        UpdatedText?.Foreground = ink3Brush;
    }

    private void UpdateIndent()
    {
        if (ChevronIcon is null) return;

        var indentWidth = 14 + Depth * 16;
        ChevronIcon.Margin = new Thickness(indentWidth, 0, 0, 0);
    }
}
