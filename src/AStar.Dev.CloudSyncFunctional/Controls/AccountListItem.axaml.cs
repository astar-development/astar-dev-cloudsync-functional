using AStar.Dev.CloudSyncFunctional.Accounts;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Media;

namespace AStar.Dev.CloudSyncFunctional.Controls;

/// <summary>A list item card that displays a cloud account's provider, status, storage, and folder count.</summary>
public partial class AccountListItem : UserControl
{
    /// <summary>Identifies the <see cref="AccountName"/> styled property.</summary>
    public static readonly StyledProperty<string> AccountNameProperty = AvaloniaProperty.Register<AccountListItem, string>(nameof(AccountName), string.Empty);

    /// <summary>Identifies the <see cref="Email"/> styled property.</summary>
    public static readonly StyledProperty<string> EmailProperty = AvaloniaProperty.Register<AccountListItem, string>(nameof(Email), string.Empty);

    /// <summary>Identifies the <see cref="Kind"/> styled property.</summary>
    public static readonly StyledProperty<ProviderKind> KindProperty = AvaloniaProperty.Register<AccountListItem, ProviderKind>(nameof(Kind));

    /// <summary>Identifies the <see cref="UsedBytes"/> styled property.</summary>
    public static readonly StyledProperty<long> UsedBytesProperty = AvaloniaProperty.Register<AccountListItem, long>(nameof(UsedBytes));

    /// <summary>Identifies the <see cref="TotalBytes"/> styled property.</summary>
    public static readonly StyledProperty<long> TotalBytesProperty = AvaloniaProperty.Register<AccountListItem, long>(nameof(TotalBytes));

    /// <summary>Identifies the <see cref="FolderCount"/> styled property.</summary>
    public static readonly StyledProperty<int> FolderCountProperty = AvaloniaProperty.Register<AccountListItem, int>(nameof(FolderCount));

    /// <summary>Identifies the <see cref="Status"/> styled property.</summary>
    public static readonly StyledProperty<SyncStatus> StatusProperty = AvaloniaProperty.Register<AccountListItem, SyncStatus>(nameof(Status));

    /// <summary>Identifies the <see cref="IsSelected"/> styled property.</summary>
    public static readonly StyledProperty<bool> IsSelectedProperty = AvaloniaProperty.Register<AccountListItem, bool>(nameof(IsSelected));

    /// <summary>Gets or sets the display name of the account.</summary>
    public string AccountName
    {
        get => GetValue(AccountNameProperty);
        set => SetValue(AccountNameProperty, value);
    }

    /// <summary>Gets or sets the email address of the account.</summary>
    public string Email
    {
        get => GetValue(EmailProperty);
        set => SetValue(EmailProperty, value);
    }

    /// <summary>Gets or sets the cloud provider kind, which drives the provider mark and bar color.</summary>
    public ProviderKind Kind
    {
        get => GetValue(KindProperty);
        set => SetValue(KindProperty, value);
    }

    /// <summary>Gets or sets the number of bytes currently used.</summary>
    public long UsedBytes
    {
        get => GetValue(UsedBytesProperty);
        set => SetValue(UsedBytesProperty, value);
    }

    /// <summary>Gets or sets the total storage capacity in bytes.</summary>
    public long TotalBytes
    {
        get => GetValue(TotalBytesProperty);
        set => SetValue(TotalBytesProperty, value);
    }

    /// <summary>Gets or sets the number of synced folders.</summary>
    public int FolderCount
    {
        get => GetValue(FolderCountProperty);
        set => SetValue(FolderCountProperty, value);
    }

    /// <summary>Gets or sets the current sync status, which drives the status dot.</summary>
    public SyncStatus Status
    {
        get => GetValue(StatusProperty);
        set => SetValue(StatusProperty, value);
    }

    /// <summary>Gets or sets whether this item is the currently selected account.</summary>
    public bool IsSelected
    {
        get => GetValue(IsSelectedProperty);
        set => SetValue(IsSelectedProperty, value);
    }

    /// <summary>Initializes a new instance of <see cref="AccountListItem"/>.</summary>
    public AccountListItem() => InitializeComponent();

    /// <summary>Converts a byte count to a GB string with one decimal place.</summary>
    /// <param name="bytes">The number of bytes to format.</param>
    /// <returns>A string such as "4.2" representing the value in GB.</returns>
    public static string FormatGb(long bytes) =>
        (bytes / 1_073_741_824.0).ToString("F1");

    /// <summary>Returns the resource key and pulsing flag for a given sync status.</summary>
    /// <param name="status">The sync status to map.</param>
    /// <returns>A tuple of (colorResourceKey, isPulsing).</returns>
    public static (string ColorKey, bool IsPulsing) GetStatusDotConfig(SyncStatus status) =>
        status switch
        {
            SyncStatus.Syncing => ("Primary", true),
            SyncStatus.Ok      => ("Good", false),
            SyncStatus.Warn    => ("Warn", false),
            SyncStatus.Paused  => ("Ink3", false),
            _                  => ("Ink3", false)
        };

    /// <summary>Returns the color resource key for a provider's storage bar.</summary>
    /// <param name="kind">The provider kind to look up.</param>
    /// <returns>The resource key string for the bar fill color.</returns>
    public static string GetBarColorKey(ProviderKind kind) =>
        kind switch
        {
            ProviderKind.GoogleDrive => "GoogleDriveAccent",
            ProviderKind.Dropbox     => "DropboxAccent",
            _                        => "OneDriveAccent"
        };

    /// <inheritdoc/>
    protected override void OnInitialized()
    {
        base.OnInitialized();
        UpdateSelection();
        UpdateProviderMark();
        UpdateStatusDot();
        UpdateStorageBar();
        UpdateStorageText();
        UpdateFolderCount();
        UpdateTexts();
    }

    /// <inheritdoc/>
    protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
    {
        base.OnPropertyChanged(change);
        if (change.Property == IsSelectedProperty) UpdateSelection();
        if (change.Property == KindProperty) { UpdateProviderMark(); UpdateStorageBar(); }
        if (change.Property == StatusProperty) UpdateStatusDot();
        if (change.Property == UsedBytesProperty || change.Property == TotalBytesProperty) { UpdateStorageBar(); UpdateStorageText(); }
        if (change.Property == FolderCountProperty) UpdateFolderCount();
        if (change.Property == AccountNameProperty || change.Property == EmailProperty) UpdateTexts();
    }

    private void UpdateSelection()
    {
        if (AccentBorder is null || Container is null) return;

        if (IsSelected)
        {
            Container.Background = this.TryFindResource("PrimaryWeak", out var bg) && bg is IBrush bgBrush
                ? bgBrush
                : Brushes.Transparent;

            AccentBorder.BorderBrush = this.TryFindResource("Primary", out var accent) && accent is IBrush accentBrush
                ? accentBrush
                : Brushes.Transparent;
        }
        else
        {
            Container.Background = Brushes.Transparent;
            AccentBorder.BorderBrush = Brushes.Transparent;
        }
    }

    private void UpdateProviderMark()
    {
        if (ProviderMarkControl is null) return;

        ProviderMarkControl.Kind = Kind;
    }

    private void UpdateStatusDot()
    {
        if (StatusDotControl is null) return;

        var (colorKey, isPulsing) = GetStatusDotConfig(Status);

        StatusDotControl.Fill = this.TryFindResource(colorKey, out var res) && res is IBrush brush
            ? brush
            : null;
        StatusDotControl.IsPulsing = isPulsing;
    }

    private void UpdateStorageBar()
    {
        if (StorageBarControl is null) return;

        var barColorKey = GetBarColorKey(Kind);

        StorageBarControl.BarColor = this.TryFindResource(barColorKey, out var res) && res is IBrush brush
            ? brush
            : null;
        StorageBarControl.Used = UsedBytes;
        StorageBarControl.Total = TotalBytes;
    }

    private void UpdateStorageText()
    {
        if (StorageText is null) return;

        StorageText.Text = $"{FormatGb(UsedBytes)} / {FormatGb(TotalBytes)} GB";
    }

    private void UpdateFolderCount()
    {
        if (FolderCountText is null) return;

        FolderCountText.Text = $"{FolderCount} folders";
    }

    private void UpdateTexts()
    {
        if (NameText is null || EmailText is null) return;

        NameText.Text = AccountName;
        EmailText.Text = Email;

        if (this.TryFindResource("Ink", out var ink) && ink is IBrush inkBrush)
            NameText.Foreground = inkBrush;

        if (this.TryFindResource("Ink3", out var ink3) && ink3 is IBrush ink3Brush)
            EmailText.Foreground = ink3Brush;
    }
}
