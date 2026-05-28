using System.Windows.Input;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Media;

namespace AStar.Dev.CloudSyncFunctional.Controls;

/// <summary>A toolbar row showing a breadcrumb path, a selection summary pill, and folder action buttons.</summary>
public partial class FolderToolbar : UserControl
{
    /// <summary>Identifies the <see cref="BreadcrumbPath"/> styled property.</summary>
    public static readonly StyledProperty<string> BreadcrumbPathProperty = AvaloniaProperty.Register<FolderToolbar, string>(nameof(BreadcrumbPath), "~/AStar /");

    /// <summary>Identifies the <see cref="SelectionSummary"/> styled property.</summary>
    public static readonly StyledProperty<string> SelectionSummaryProperty = AvaloniaProperty.Register<FolderToolbar, string>(nameof(SelectionSummary), string.Empty);

    /// <summary>Identifies the <see cref="CanApplyChanges"/> styled property.</summary>
    public static readonly StyledProperty<bool> CanApplyChangesProperty = AvaloniaProperty.Register<FolderToolbar, bool>(nameof(CanApplyChanges), false);

    /// <summary>Identifies the <see cref="FilterCommand"/> styled property.</summary>
    public static readonly StyledProperty<ICommand?> FilterCommandProperty = AvaloniaProperty.Register<FolderToolbar, ICommand?>(nameof(FilterCommand));

    /// <summary>Identifies the <see cref="ApplyChangesCommand"/> styled property.</summary>
    public static readonly StyledProperty<ICommand?> ApplyChangesCommandProperty = AvaloniaProperty.Register<FolderToolbar, ICommand?>(nameof(ApplyChangesCommand));

    /// <summary>Gets or sets the breadcrumb path text displayed on the left.</summary>
    public string BreadcrumbPath
    {
        get => GetValue(BreadcrumbPathProperty);
        set => SetValue(BreadcrumbPathProperty, value);
    }

    /// <summary>Gets or sets the selection summary shown in the status pill.</summary>
    public string SelectionSummary
    {
        get => GetValue(SelectionSummaryProperty);
        set => SetValue(SelectionSummaryProperty, value);
    }

    /// <summary>Gets or sets whether the Apply Changes button is enabled.</summary>
    public bool CanApplyChanges
    {
        get => GetValue(CanApplyChangesProperty);
        set => SetValue(CanApplyChangesProperty, value);
    }

    /// <summary>Gets or sets the command invoked when the Filter button is clicked.</summary>
    public ICommand? FilterCommand
    {
        get => GetValue(FilterCommandProperty);
        set => SetValue(FilterCommandProperty, value);
    }

    /// <summary>Gets or sets the command invoked when the Apply Changes button is clicked.</summary>
    public ICommand? ApplyChangesCommand
    {
        get => GetValue(ApplyChangesCommandProperty);
        set => SetValue(ApplyChangesCommandProperty, value);
    }

    /// <summary>Initializes a new instance of <see cref="FolderToolbar"/>.</summary>
    public FolderToolbar() => InitializeComponent();

    /// <inheritdoc/>
    protected override void OnInitialized()
    {
        base.OnInitialized();
        UpdateBorder();
        UpdateBreadcrumb();
        UpdateSelectionPill();
        UpdateApplyButton();
        UpdateCommands();
    }

    /// <inheritdoc/>
    protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
    {
        base.OnPropertyChanged(change);
        if (change.Property == BreadcrumbPathProperty) UpdateBreadcrumb();
        if (change.Property == SelectionSummaryProperty) UpdateSelectionPill();
        if (change.Property == CanApplyChangesProperty) UpdateApplyButton();
        if (change.Property == FilterCommandProperty || change.Property == ApplyChangesCommandProperty) UpdateCommands();
    }

    private void UpdateBorder()
    {
        if (ContainerBorder is null) return;

        if (this.TryFindResource("Border", out var res) && res is IBrush borderBrush)
            ContainerBorder.BorderBrush = borderBrush;
    }

    private void UpdateBreadcrumb()
    {
        if (BreadcrumbText is null) return;

        BreadcrumbText.Text = BreadcrumbPath;

        if (this.TryFindResource("Ink3", out var res) && res is IBrush brush)
            BreadcrumbText.Foreground = brush;
    }

    private void UpdateSelectionPill()
    {
        if (SelectionPill is null) return;

        SelectionPill.Label     = SelectionSummary;
        SelectionPill.IsVisible = !string.IsNullOrEmpty(SelectionSummary);
    }

    private void UpdateApplyButton()
    {
        if (ApplyButton is null) return;

        ApplyButton.IsEnabled = CanApplyChanges;
    }

    private void UpdateCommands()
    {
        if (FilterButton is null || ApplyButton is null) return;

        FilterButton.Command = FilterCommand;
        ApplyButton.Command  = ApplyChangesCommand;
    }
}
