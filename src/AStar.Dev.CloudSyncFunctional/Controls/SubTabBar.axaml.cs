using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Media;

namespace AStar.Dev.CloudSyncFunctional.Controls;

/// <summary>A horizontal tab bar for switching between sub-sections of an account pane.</summary>
public partial class SubTabBar : UserControl
{
    /// <summary>Identifies the <see cref="SelectedTab"/> styled property.</summary>
    public static readonly StyledProperty<SubTab> SelectedTabProperty =
        AvaloniaProperty.Register<SubTabBar, SubTab>(nameof(SelectedTab), SubTab.SyncFolders);

    /// <summary>Identifies the <see cref="ConflictCount"/> styled property.</summary>
    public static readonly StyledProperty<int> ConflictCountProperty =
        AvaloniaProperty.Register<SubTabBar, int>(nameof(ConflictCount), 0);

    /// <summary>Gets or sets the currently active tab.</summary>
    public SubTab SelectedTab
    {
        get => GetValue(SelectedTabProperty);
        set => SetValue(SelectedTabProperty, value);
    }

    /// <summary>Gets or sets the number of sync conflicts, shown as a badge on the Conflicts tab.</summary>
    public int ConflictCount
    {
        get => GetValue(ConflictCountProperty);
        set => SetValue(ConflictCountProperty, value);
    }

    /// <summary>Initializes a new instance of <see cref="SubTabBar"/>.</summary>
    public SubTabBar()
    {
        InitializeComponent();
        SyncFoldersTab.PointerPressed += (_, _) => SelectedTab = SubTab.SyncFolders;
        ActivityTab.PointerPressed    += (_, _) => SelectedTab = SubTab.Activity;
        ConflictsTab.PointerPressed   += (_, _) => SelectedTab = SubTab.Conflicts;
        SettingsTab.PointerPressed    += (_, _) => SelectedTab = SubTab.Settings;
    }

    /// <summary>Returns the display label for the given tab.</summary>
    /// <param name="tab">The tab to look up.</param>
    /// <returns>The human-readable tab label string.</returns>
    public static string GetTabLabel(SubTab tab) =>
        tab switch
        {
            SubTab.SyncFolders => "Sync folders",
            SubTab.Activity    => "Activity",
            SubTab.Conflicts   => "Conflicts",
            SubTab.Settings    => "Settings",
            _                  => string.Empty
        };

    /// <inheritdoc/>
    protected override void OnInitialized()
    {
        base.OnInitialized();
        updateBorder();
        updateTabLabels();
        updateTabAppearances();
        updateConflictsPill();
    }

    /// <inheritdoc/>
    protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
    {
        base.OnPropertyChanged(change);
        if (change.Property == SelectedTabProperty) updateTabAppearances();
        if (change.Property == ConflictCountProperty) updateConflictsPill();
    }

    private void updateBorder()
    {
        if (ContainerBorder is null) return;

        if (this.TryFindResource("Border", out var res) && res is IBrush borderBrush)
            ContainerBorder.BorderBrush = borderBrush;
    }

    private void updateTabLabels()
    {
        if (SyncFoldersLabel is null || ActivityLabel is null || ConflictsLabel is null || SettingsLabel is null) return;

        SyncFoldersLabel.Text = GetTabLabel(SubTab.SyncFolders);
        ActivityLabel.Text    = GetTabLabel(SubTab.Activity);
        ConflictsLabel.Text   = GetTabLabel(SubTab.Conflicts);
        SettingsLabel.Text    = GetTabLabel(SubTab.Settings);
    }

    private void updateTabAppearances()
    {
        if (SyncFoldersLabel is null || ActivityLabel is null || ConflictsLabel is null || SettingsLabel is null) return;
        if (SyncFoldersUnderline is null || ActivityUnderline is null || ConflictsUnderline is null || SettingsUnderline is null) return;

        applyTabStyle(SyncFoldersLabel, SyncFoldersUnderline, SelectedTab == SubTab.SyncFolders);
        applyTabStyle(ActivityLabel,    ActivityUnderline,    SelectedTab == SubTab.Activity);
        applyTabStyle(ConflictsLabel,   ConflictsUnderline,   SelectedTab == SubTab.Conflicts);
        applyTabStyle(SettingsLabel,    SettingsUnderline,    SelectedTab == SubTab.Settings);
    }

    private void applyTabStyle(TextBlock label, Border underline, bool isActive)
    {
        if (isActive)
        {
            label.FontWeight = FontWeight.Bold;

            if (this.TryFindResource("Ink", out var inkRes) && inkRes is IBrush inkBrush)
                label.Foreground = inkBrush;

            if (this.TryFindResource("Primary", out var primaryRes) && primaryRes is IBrush primaryBrush)
                underline.Background = primaryBrush;
        }
        else
        {
            label.FontWeight = FontWeight.Medium;

            if (this.TryFindResource("Ink3", out var ink3Res) && ink3Res is IBrush ink3Brush)
                label.Foreground = ink3Brush;

            underline.Background = Brushes.Transparent;
        }
    }

    private void updateConflictsPill()
    {
        if (ConflictsPill is null) return;

        ConflictsPill.IsVisible = ConflictCount > 0;
        ConflictsPill.Label     = ConflictCount.ToString();
    }
}
